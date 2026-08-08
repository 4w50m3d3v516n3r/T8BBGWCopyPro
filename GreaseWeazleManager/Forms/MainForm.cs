using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GwCopyPro.Controls;
using GwCopyPro.Models;
using GwCopyPro.Services;

namespace GwCopyPro.Forms
{
    /// <summary>
    /// Application main window. Hosts the device strip, the scrollable jobs panel, the top
    /// toolbar, and the status bar. Manages the shared device list, starts <see cref="GwJob"/>
    /// instances via <see cref="GwService"/>, and handles all service events (track updates,
    /// job completion, disk-prompt for repetitive mode).
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly List<GreaseWeazleDevice>      _devices   = new();
        private readonly List<GwJob>                   _jobs      = new();
        private readonly List<CancellationTokenSource> _cts       = new();
        private readonly GwService                     _gwService = new();
        private readonly Dictionary<string, JobPanel>  _jobPanels = new();
        private bool _blinkInProgress;
        private bool _errorFlash;
        private System.Windows.Forms.Timer? _flashBorderTimer;

        /// <summary>
        /// Initialises the form, loads <see cref="AppSettings"/>, applies the saved language,
        /// and triggers async device auto-detection once the window is loaded.
        /// </summary>
        public MainForm()
        {
            var settings = AppSettings.Instance;
            _gwService.GwExePath = settings.GwExePath;
            L10n.SetLanguage(settings.Language);

            InitializeComponent();
            Icon = CreateAppIcon();
            RefreshDeviceBar();
            UpdateJobCount();
            WireEvents();
            this.WindowState = FormWindowState.Maximized;
            Load += async (s, e) => await AutoDetectDevicesAsync();
        }

        /// <summary>Paints the thin separator line at the bottom of the top toolbar.</summary>
        private void TopBar_Paint(object? sender, PaintEventArgs e)
        {
            using var pen = new Pen(Color.FromArgb(40, 80, 140), 1f);
            e.Graphics.DrawLine(pen, 0, _topBar.Height - 1, _topBar.Width, _topBar.Height - 1);
        }

        /// <summary>Reverts the status label to "Ready" once the auto-clear timer elapses.</summary>
        private void StatusTimer_Tick(object? sender, EventArgs e)
        {
            _statusMsg.Text      = L10n.T("app.ready");
            _statusMsg.ForeColor = Color.FromArgb(90, 175, 90);
            _statusTimer.Stop();
        }

        /// <summary>
        /// Subscribes to all <see cref="GwService"/> events. Each handler marshals to the
        /// UI thread via <see cref="SafeInvoke"/>.
        /// </summary>
        private void WireEvents()
        {
            _gwService.JobStarted   += GwService_JobStarted;
            _gwService.TrackUpdated += GwService_TrackUpdated;
            _gwService.JobProgress  += GwService_JobProgress;
            _gwService.JobCompleted += GwService_JobCompleted;
            _gwService.JobError     += GwService_JobError;
            _gwService.DiskCompleted += GwService_DiskCompleted;
        }

        private void GwService_JobStarted(object? sender, GwJobEventArgs e) => SafeInvoke(() =>
        {
            SetStatus(string.Format(L10n.T("status.job_started"),
                e.Job.JobType, Path.GetFileName(e.Job.Parameters.ImageFile ?? "")),
                Color.FromArgb(100, 200, 255));
            SoundService.PlayStart();
            UpdateJobCount();
        });

        private void GwService_TrackUpdated(object? sender, TrackUpdateEventArgs e) => SafeInvoke(() =>
        {
            if (_jobPanels.TryGetValue(e.Job.Id, out var p)) p.UpdateFromJob();
        });

        private void GwService_JobProgress(object? sender, GwJobEventArgs e) => SafeInvoke(() =>
        {
            if (_jobPanels.TryGetValue(e.Job.Id, out var p)) p.UpdateFromJob();
        });

        private void GwService_JobCompleted(object? sender, GwJobEventArgs e) => SafeInvoke(() =>
        {
            if (_jobPanels.TryGetValue(e.Job.Id, out var p)) p.UpdateFromJob();
            SetStatus(string.Format(L10n.T("status.job_done"),
                Path.GetFileName(e.Job.Parameters.ImageFile ?? "")),
                Color.FromArgb(80, 220, 100));
            SoundService.PlaySuccess();
            UpdateJobCount();
        });

        private void GwService_JobError(object? sender, GwJobEventArgs e) => SafeInvoke(() =>
        {
            if (_jobPanels.TryGetValue(e.Job.Id, out var p)) p.UpdateFromJob();
            SetStatus(string.Format(L10n.T("status.job_error"), e.Job.LastError),
                Color.FromArgb(240, 80, 80));
            SoundService.PlayError();
            FlashErrorBorder();
            UpdateJobCount();
        });

        private void GwService_DiskCompleted(object? sender, DiskCompletedEventArgs e) =>
            SafeInvoke(() => ShowNextDiskDialogAndSignal(e));

        /// <summary>Updates the job panel, shows <see cref="NextDiskDialog"/>, and signals whether to continue.</summary>
        private void ShowNextDiskDialogAndSignal(DiskCompletedEventArgs e)
        {
            SoundService.PlaySuccess();
            if (_jobPanels.TryGetValue(e.Job.Id, out var p)) p.UpdateFromJob();

            using var dlg = new NextDiskDialog(
                e.DiskNumber,
                e.CompletedFile,
                e.NextFile,
                e.Duration,
                e.Job.DateTimeFormat,
                e.Job.Device != null
                    ? $"{e.Job.Device.Name} ({e.Job.Device.SerialPort})"
                    : L10n.T("job_dlg.auto_device"),
                string.IsNullOrWhiteSpace(e.Job.Parameters.Drive)
                    ? L10n.T("nextdisk.drive_auto")
                    : e.Job.Parameters.Drive);

            dlg.ShowDialog(this);
            e.Signal(dlg.Choice == NextDiskDialog.NextDiskResult.Go);
        }

        /// <summary>Opens <see cref="NewJobDialog"/> without a pre-selected device.</summary>
        private void BtnNewJob_Click(object? sender, EventArgs e) =>
            OpenNewJobDialog(preselectedDevice: null);

        /// <summary>Opens <see cref="DeviceManagerDialog"/> and refreshes the device strip on close.</summary>
        private void BtnDevices_Click(object? sender, EventArgs e)
        {
            using var dlg = new DeviceManagerDialog(_devices, _gwService.GwExePath);
            dlg.ShowDialog(this);
            RefreshDeviceBar();
        }

        /// <summary>
        /// Opens <see cref="SettingsDialog"/> and, on close, applies the saved gw.exe path,
        /// re-localises all toolbar labels, and refreshes the device strip.
        /// </summary>
        private void BtnSettings_Click(object? sender, EventArgs e)
        {
            using var dlg = new SettingsDialog();
            dlg.ShowDialog(this);

            var s = AppSettings.Instance;
            _gwService.GwExePath  = s.GwExePath;
            _lblGwPath.Text       = string.Format(L10n.T("app.gw_path"), s.GwExePath);
            L10n.SetLanguage(s.Language);

            _btnNewJob.Text   = L10n.T("btn.new_job");
            _btnDevices.Text  = L10n.T("btn.devices");
            _btnSettings.Text = L10n.T("btn.settings");
            _btnClear.Text    = L10n.T("btn.clear_done");
            _lblDevices.Text  = L10n.T("app.devices");
            _lblJobs.Text     = L10n.T("app.active_jobs");
            _statusMsg.Text   = L10n.T("app.ready");
            UpdateJobCount();
            RefreshDeviceBar();
        }

        /// <summary>Removes all completed, errored, and cancelled jobs and their panels from the jobs flow.</summary>
        private void BtnClearDone_Click(object? sender, EventArgs e)
        {
            var done = _jobs
                .Where(j => j.Status is JobStatus.Completed or JobStatus.Error or JobStatus.Cancelled)
                .ToList();
            foreach (var job in done)
            {
                if (_jobPanels.TryGetValue(job.Id, out var panel))
                {
                    _jobsFlow.Controls.Remove(panel);
                    panel.Dispose();
                    _jobPanels.Remove(job.Id);
                }
                _jobs.Remove(job);
            }
            UpdateJobCount();
        }

        /// <summary>
        /// Opens <see cref="NewJobDialog"/>, optionally pre-selecting a device and/or
        /// loading a preset, then starts the job if the user clicks Start.
        /// </summary>
        /// <param name="preselectedDevice">Device to pre-select in the dialog, or <see langword="null"/>.</param>
        /// <param name="preset">Preset to load into the dialog, or <see langword="null"/>.</param>
        private void OpenNewJobDialog(GreaseWeazleDevice? preselectedDevice,
            Models.JobPreset? preset = null)
        {
            using var dlg = new NewJobDialog(_devices, preselectedDevice);
            if (preset != null) dlg.LoadFromPreset(preset);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                if (dlg.GroupResult != null) StartGroupJob(dlg.GroupResult);
                else if (dlg.Result != null) StartJob(dlg.Result);
            }
        }

        /// <summary>
        /// Adds the job to the tracking collections, creates a <see cref="JobPanel"/>,
        /// and launches <see cref="GwService.RunJobAsync"/> on a background thread.
        /// </summary>
        /// <param name="job">The job to start.</param>
        private void StartJob(GwJob job)
        {
            _jobs.Add(job);
            var cts = new CancellationTokenSource();
            _cts.Add(cts);

            var panel = new JobPanel(job,
                cancelJob => cts.Cancel(),
                LogJobCallback,
                restartJob => RestartJobCallback(restartJob));

            _jobPanels[job.Id] = panel;
            _jobsFlow.Controls.Add(panel);
            UpdateJobCount();

            Task.Run(async () =>
            {
                try { await _gwService.RunJobAsync(job, cts.Token); }
                catch (Exception ex)
                {
                    SafeInvoke(() => SetStatus(
                        string.Format(L10n.T("status.exception"), ex.Message),
                        Color.FromArgb(240, 80, 80)));
                }
            });
        }

        /// <summary>Opens the job's log folder/file in Explorer or Notepad, if available.</summary>
        private void LogJobCallback(GwJob logJob)
        {
            if (Directory.Exists(logJob.LogFolder))
                System.Diagnostics.Process.Start("explorer.exe", logJob.LogFolder);
            else if (File.Exists(logJob.LogFile))
                System.Diagnostics.Process.Start("notepad.exe", logJob.LogFile);
            else
                MessageBox.Show(
                    L10n.T("job.log_unavailable"),
                    L10n.T("job.log_caption"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>Reopens the New Job dialog pre-loaded from the job's source preset (or device only, if none).</summary>
        private void RestartJobCallback(GwJob restartJob)
        {
            if (restartJob.SourcePreset != null)
                OpenNewJobDialog(restartJob.Device, restartJob.SourcePreset);
            else
                OpenNewJobDialog(restartJob.Device);
        }

        /// <summary>
        /// Starts a group repetitive job: wires a <see cref="GroupJobService"/>, creates a
        /// job panel per member, shows the <see cref="BatchInsertDialog"/> before each
        /// batch, and reports completion in the status bar.
        /// </summary>
        /// <param name="group">The group job to run.</param>
        private void StartGroupJob(GroupRepetitiveJob group)
        {
            var cts = new CancellationTokenSource();
            _cts.Add(cts);

            var service = new GroupJobService(_gwService);
            var prober  = new DriveProber(_gwService.GwExePath);

            service.MemberJobsCreated += (s, e) => SafeInvoke(() => CreateGroupMemberPanels(e.Group));

            service.BatchPromptRequested += (s, e) => SafeInvoke(() => PromptForBatchInsert(e));

            service.GroupCompleted += (s, e) => SafeInvoke(() => ReportGroupCompleted(e.Group));

            Task.Run(async () =>
            {
                try { await service.RunAsync(group, cts.Token); }
                catch (Exception ex)
                {
                    SafeInvoke(() => SetStatus(
                        string.Format(L10n.T("status.exception"), ex.Message),
                        Color.FromArgb(240, 80, 80)));
                }
            });
        }

        /// <summary>Creates and registers a <see cref="JobPanel"/> for every member of a freshly created group batch.</summary>
        private void CreateGroupMemberPanels(GroupRepetitiveJob group)
        {
            foreach (var m in group.Members)
            {
                var job = m.Job!;
                _jobs.Add(job);
                var member = m;
                var panel = new JobPanel(job,
                    cancelJob => member.BatchCts?.Cancel(),
                    LogJobCallback,
                    restartJob => { });
                _jobPanels[job.Id] = panel;
                _jobsFlow.Controls.Add(panel);
            }
            UpdateJobCount();
        }

        /// <summary>Shows the <see cref="BatchInsertDialog"/> for the next batch and signals whether it was started.</summary>
        private void PromptForBatchInsert(BatchPromptEventArgs e)
        {
            var prober = new DriveProber(_gwService.GwExePath);
            using var dlg = new BatchInsertDialog(e.Group, prober);
            dlg.ShowDialog(this);
            if (dlg.StartBatchChosen)
                SetStatus(string.Format(L10n.T("status.batch_running"),
                        e.Group.BatchNumber + 1,
                        e.Group.Members.Count(m => m.IncludedThisBatch && m.Verified)),
                    Color.FromArgb(100, 200, 255));
            e.Signal(dlg.StartBatchChosen);
        }

        /// <summary>Refreshes all member job panels and reports the total disks completed in the status bar.</summary>
        private void ReportGroupCompleted(GroupRepetitiveJob group)
        {
            foreach (var m in group.Members)
                if (m.Job != null && _jobPanels.TryGetValue(m.Job.Id, out var p))
                    p.UpdateFromJob();
            SetStatus(string.Format(L10n.T("status.group_done"),
                    group.Members.Sum(m => m.Job?.DisksCompleted ?? 0)),
                Color.FromArgb(80, 220, 100));
            SoundService.PlaySuccess();
            UpdateJobCount();
        }

        /// <summary>
        /// Runs WMI device detection on startup, queries firmware for each new device,
        /// and populates the device strip. Updates the status bar throughout.
        /// </summary>
        private async Task AutoDetectDevicesAsync()
        {
            SetStatus(L10n.T("status.scanning"), Color.FromArgb(200, 180, 60));
            try
            {
                var detected = await Task.Run(() => GwDetector.GetAllGwDevicesConnected());
                if (detected.Count == 0)
                {
                    SetStatus(L10n.T("status.no_devices"), Color.FromArgb(160, 120, 60));
                    return;
                }

                int added = 0;
                foreach (var props in detected)
                {
                    if (_devices.Exists(d => d.SerialPort.Equals(
                            props.DeviceComport, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    SetStatus(string.Format(L10n.T("status.querying_fw"), props.DeviceComport),
                        Color.FromArgb(200, 180, 60));
                    var dev = await GwDetector.BuildDeviceAsync(props, _gwService.GwExePath);
                    _devices.Add(dev);
                    added++;
                }

                RefreshDeviceBar();
                SetStatus(
                    added > 0
                        ? string.Format(L10n.T("status.detected"), added)
                        : L10n.T("status.no_new"),
                    Color.FromArgb(80, 220, 120));
            }
            catch (Exception ex)
            {
                SetStatus(string.Format(L10n.T("status.detect_error"), ex.Message),
                    Color.FromArgb(230, 80, 80));
            }
        }

        /// <summary>Clears and rebuilds the device strip from the current <see cref="_devices"/> list.</summary>
        private void RefreshDeviceBar()
        {
            _deviceBar.Controls.Clear();

            if (_devices.Count == 0)
            {
                _deviceBar.Controls.Add(MakeNoDevLabel());
                return;
            }

            foreach (var dev in _devices)
            {
                var dp = new DevicePanel(
                    dev,
                    d => { _devices.Remove(d); RefreshDeviceBar(); },
                    d => OpenNewJobDialog(preselectedDevice: d),
                    d => BlinkIdentify(d));
                _deviceBar.Controls.Add(dp);
            }
        }

        /// <summary>
        /// Runs a short identify sequence on the device: one blink pulse each on drive 0 and
        /// drive 1, covering both unit-select lines so any attached drive lights regardless of
        /// its 0/1/a/b addressing. Only one sequence runs at a time.
        /// </summary>
        /// <param name="dev">The device whose drive should blink.</param>
        private void BlinkIdentify(GreaseWeazleDevice dev)
        {
            if (_blinkInProgress) return;
            _blinkInProgress = true;
            SetPanelsBlinkBusy(true);
            SetStatus(string.Format(L10n.T("status.blinking_dev"), dev.Name, dev.SerialPort),
                Color.FromArgb(220, 180, 80));

            var prober = new DriveProber(_gwService.GwExePath);
            Task.Run(async () =>
            {
                bool ok = true;
                try
                {
                    await prober.BlinkOnceAsync(dev.SerialPort, "0", CancellationToken.None);
                    await Task.Delay(350);
                    await prober.BlinkOnceAsync(dev.SerialPort, "1", CancellationToken.None);
                }
                catch { ok = false; }

                SafeInvoke(() =>
                {
                    _blinkInProgress = false;
                    SetPanelsBlinkBusy(false);
                    SetStatus(
                        ok ? string.Format(L10n.T("status.blink_done"), dev.Name)
                           : string.Format(L10n.T("status.blink_error"), dev.Name),
                        ok ? Color.FromArgb(80, 220, 120) : Color.FromArgb(240, 80, 80));
                });
            });
        }

        /// <summary>Toggles the Blink button on every device tile.</summary>
        private void SetPanelsBlinkBusy(bool busy)
        {
            foreach (Control c in _deviceBar.Controls)
                if (c is DevicePanel dp) dp.SetBlinkBusy(busy);
        }

        /// <summary>Creates the placeholder label shown in the device strip when no devices are registered.</summary>
        private Label MakeNoDevLabel() => new()
        {
            Text      = L10n.T("nodev.label"),
            Font      = new Font("Consolas", 8.5f),
            ForeColor = Color.FromArgb(65, 85, 115),
            AutoSize  = true,
            Padding   = new Padding(10, 50, 0, 0),
            BackColor = Color.Transparent
        };

        /// <summary>Refreshes the job-count label in the jobs header bar.</summary>
        private void UpdateJobCount()
        {
            int running = _jobs.Count(j => j.Status == JobStatus.Running);
            int total   = _jobs.Count;
            _lblJobCount.Text = string.Format(L10n.T("status.jobs_count"), total, running);
        }

        /// <summary>
        /// Updates the status bar message and colour, then starts an auto-clear timer that
        /// reverts the status label to "Ready" after 4 seconds.
        /// </summary>
        /// <param name="text">Status message to display.</param>
        /// <param name="color">Foreground colour for the message.</param>
        private void SetStatus(string text, Color color)
        {
            _statusMsg.Text      = text;
            _statusMsg.ForeColor = color;
            _statusTimer.Stop();
            _statusTimer.Start();
        }

        /// <summary>
        /// Briefly flashes the form background red/dark to signal a job error.
        /// The flash runs for 4 cycles (8 timer ticks at 200 ms each) then resets.
        /// Does nothing if a flash is already in progress.
        /// </summary>
        private void FlashErrorBorder()
        {
            if (_flashBorderTimer != null) return;
            int count = 0;
            _flashBorderTimer = new System.Windows.Forms.Timer { Interval = 200 };
            _flashBorderTimer.Tick += (s, e) =>
            {
                _errorFlash = !_errorFlash;
                BackColor   = _errorFlash ? Color.FromArgb(40, 14, 14) : Color.FromArgb(14, 16, 24);
                if (++count >= 8)
                {
                    _flashBorderTimer.Stop();
                    _flashBorderTimer.Dispose();
                    _flashBorderTimer = null;
                    BackColor = Color.FromArgb(14, 16, 24);
                }
            };
            _flashBorderTimer.Start();
        }

        /// <summary>
        /// Marshals <paramref name="action"/> to the UI thread, swallowing
        /// <see cref="ObjectDisposedException"/> that can occur during form teardown.
        /// </summary>
        private void SafeInvoke(Action action)
        {
            if (IsHandleCreated && !IsDisposed)
                try { Invoke(action); } catch (ObjectDisposedException) { }
        }

        /// <summary>Loads the application icon from the bundled <c>icon\favicon.ico</c> file.</summary>
        /// <returns>The loaded <see cref="Icon"/>.</returns>
        private static Icon CreateAppIcon()
        {
            Icon? icon = Icon.ExtractAssociatedIcon(@"icon\favicon.ico");
            return icon!;
        }

        /// <summary>Cancels all running jobs before the form closes.</summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            foreach (var cts in _cts)
                try { cts.Cancel(); } catch { }
            base.OnFormClosing(e);
        }
    }
}
