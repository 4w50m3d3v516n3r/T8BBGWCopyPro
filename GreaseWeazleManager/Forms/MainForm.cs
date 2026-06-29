using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
    public class MainForm : Form
    {
        private readonly List<GreaseWeazleDevice>      _devices   = new();
        private readonly List<GwJob>                   _jobs      = new();
        private readonly List<CancellationTokenSource> _cts       = new();
        private readonly GwService                     _gwService = new();
        private readonly Dictionary<string, JobPanel>  _jobPanels = new();

        private Panel           _topBar      = null!;
        private FlowLayoutPanel _deviceBar   = null!;
        private FlowLayoutPanel _jobsFlow    = null!;
        private Label           _lblGwPath   = null!;
        private Label           _lblJobCount = null!;
        private Label           _statusMsg   = null!;
        private Label           _lblDevices  = null!;
        private Label           _lblJobs     = null!;
        private Button          _btnNewJob   = null!;
        private Button          _btnDevices  = null!;
        private Button          _btnSettings = null!;
        private Button          _btnClear    = null!;
        private System.Windows.Forms.Timer _statusTimer = null!;

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
            WireEvents();
            Load += async (s, e) => await AutoDetectDevicesAsync();
        }

        /// <summary>
        /// Builds the top toolbar, device strip, jobs scroll area, and status bar and
        /// adds them to the form in reverse dock order.
        /// </summary>
        private void InitializeComponent()
        {
            Text        = L10n.T("app.title");
            Size        = new Size(1120, 900);
            MinimumSize = new Size(1060, 600);
            BackColor   = Color.FromArgb(14, 16, 24);
            ForeColor   = Color.FromArgb(180, 210, 255);
            Icon        = CreateAppIcon();

            _topBar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 52,
                BackColor = Color.FromArgb(16, 20, 32)
            };
            _topBar.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(40, 80, 140), 1f);
                e.Graphics.DrawLine(pen, 0, _topBar.Height - 1, _topBar.Width, _topBar.Height - 1);
            };

            var lblTitle = new Label
            {
                Text      = "The8BitBox™ - Ilija Injac\nPresents - GW COPY PRO",
                Font      = new Font("Consolas", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 180, 255),
                AutoSize  = true,
                Location  = new Point(14, 14),
                BackColor = Color.Transparent
            };

            Button MakeTopBtn(string text, int x, Color bg, Color fg, Color border)
            {
                var b = new Button
                {
                    Text      = text,
                    Location  = new Point(x, 12),
                    Size      = new Size(148, 30),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = bg,
                    ForeColor = fg,
                    Font      = new Font("Consolas", 8.5f, FontStyle.Bold)
                };
                b.FlatAppearance.BorderColor = border;
                return b;
            }

            _btnNewJob = MakeTopBtn(L10n.T("btn.new_job"), 320,
                Color.FromArgb(20, 70, 40), Color.FromArgb(80, 230, 120), Color.FromArgb(50, 140, 80));
            _btnNewJob.Click += BtnNewJob_Click;

            _btnDevices = MakeTopBtn(L10n.T("btn.devices"), 478,
                Color.FromArgb(20, 40, 80), Color.FromArgb(100, 160, 255), Color.FromArgb(50, 90, 180));
            _btnDevices.Click += BtnDevices_Click;

            _btnSettings = MakeTopBtn(L10n.T("btn.settings"), 636,
                Color.FromArgb(40, 35, 20), Color.FromArgb(220, 180, 80), Color.FromArgb(120, 100, 40));
            _btnSettings.Click += BtnSettings_Click;

            _btnClear = MakeTopBtn(L10n.T("btn.clear_done"), 794,
                Color.FromArgb(50, 25, 25), Color.FromArgb(220, 100, 100), Color.FromArgb(120, 50, 50));
            _btnClear.Click += BtnClearDone_Click;

            _topBar.Controls.AddRange(new Control[]
                { lblTitle, _btnNewJob, _btnDevices, _btnSettings, _btnClear });

            var deviceHeaderBar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 20,
                BackColor = Color.FromArgb(14, 18, 28)
            };
            _lblDevices = new Label
            {
                Text      = L10n.T("app.devices"),
                Font      = new Font("Consolas", 7f, FontStyle.Bold),
                ForeColor = Color.FromArgb(70, 100, 150),
                AutoSize  = true,
                Location  = new Point(10, 4),
                BackColor = Color.Transparent
            };
            deviceHeaderBar.Controls.Add(_lblDevices);

            _deviceBar = new FlowLayoutPanel
            {
                Dock          = DockStyle.Top,
                Height        = 148,
                BackColor     = Color.FromArgb(16, 18, 28),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                AutoScroll    = true,
                Padding       = new Padding(6)
            };
            _deviceBar.Controls.Add(MakeNoDevLabel());

            var jobsHeaderBar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 24,
                BackColor = Color.FromArgb(16, 20, 32)
            };
            _lblJobs = new Label
            {
                Text      = L10n.T("app.active_jobs"),
                Font      = new Font("Consolas", 7.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(70, 100, 150),
                AutoSize  = true,
                Location  = new Point(12, 5),
                BackColor = Color.Transparent
            };
            _lblJobCount = new Label
            {
                Font      = new Font("Consolas", 7.5f),
                ForeColor = Color.FromArgb(55, 85, 125),
                AutoSize  = true,
                Location  = new Point(180, 5),
                BackColor = Color.Transparent
            };
            jobsHeaderBar.Controls.AddRange(new Control[] { _lblJobs, _lblJobCount });

            var jobsScroll = new Panel
            {
                Dock       = DockStyle.Fill,
                BackColor  = Color.FromArgb(14, 16, 24),
                AutoScroll = true
            };
            _jobsFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents  = false,
                AutoSize      = true,
                AutoSizeMode  = AutoSizeMode.GrowAndShrink,
                Padding       = new Padding(8),
                BackColor     = Color.FromArgb(14, 16, 24)
            };
            jobsScroll.Controls.Add(_jobsFlow);

            var statusBar = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 24,
                BackColor = Color.FromArgb(12, 16, 26)
            };
            _lblGwPath = new Label
            {
                Text      = string.Format(L10n.T("app.gw_path"), _gwService.GwExePath),
                Font      = new Font("Consolas", 7.5f),
                ForeColor = Color.FromArgb(75, 105, 145),
                AutoSize  = true,
                Location  = new Point(8, 5),
                BackColor = Color.Transparent
            };
            _statusMsg = new Label
            {
                Text      = L10n.T("app.ready"),
                Font      = new Font("Consolas", 7.5f),
                ForeColor = Color.FromArgb(90, 175, 90),
                AutoSize  = true,
                Location  = new Point(420, 5),
                BackColor = Color.Transparent
            };
            statusBar.Controls.AddRange(new Control[] { _lblGwPath, _statusMsg });

            Controls.AddRange(new Control[]
            {
                jobsScroll,
                jobsHeaderBar,
                _deviceBar,
                deviceHeaderBar,
                _topBar,
                statusBar
            });

            _statusTimer = new System.Windows.Forms.Timer { Interval = 4000 };
            _statusTimer.Tick += (s, e) =>
            {
                _statusMsg.Text      = L10n.T("app.ready");
                _statusMsg.ForeColor = Color.FromArgb(90, 175, 90);
                _statusTimer.Stop();
            };

            UpdateJobCount();
        }

        /// <summary>
        /// Subscribes to all <see cref="GwService"/> events. Marshals each callback to the UI
        /// thread and, for the <c>DiskCompleted</c> event, shows a <see cref="NextDiskDialog"/>
        /// and signals the service to continue or stop.
        /// </summary>
        private void WireEvents()
        {
            _gwService.JobStarted += (s, e) => SafeInvoke(() =>
            {
                SetStatus(string.Format(L10n.T("status.job_started"),
                    e.Job.JobType, Path.GetFileName(e.Job.Parameters.ImageFile ?? "")),
                    Color.FromArgb(100, 200, 255));
                SoundService.PlayStart();
                UpdateJobCount();
            });

            _gwService.TrackUpdated += (s, e) => SafeInvoke(() =>
            {
                if (_jobPanels.TryGetValue(e.Job.Id, out var p)) p.UpdateFromJob();
            });

            _gwService.JobProgress += (s, e) => SafeInvoke(() =>
            {
                if (_jobPanels.TryGetValue(e.Job.Id, out var p)) p.UpdateFromJob();
            });

            _gwService.JobCompleted += (s, e) => SafeInvoke(() =>
            {
                if (_jobPanels.TryGetValue(e.Job.Id, out var p)) p.UpdateFromJob();
                SetStatus(string.Format(L10n.T("status.job_done"),
                    Path.GetFileName(e.Job.Parameters.ImageFile ?? "")),
                    Color.FromArgb(80, 220, 100));
                SoundService.PlaySuccess();
                UpdateJobCount();
            });

            _gwService.JobError += (s, e) => SafeInvoke(() =>
            {
                if (_jobPanels.TryGetValue(e.Job.Id, out var p)) p.UpdateFromJob();
                SetStatus(string.Format(L10n.T("status.job_error"), e.Job.LastError),
                    Color.FromArgb(240, 80, 80));
                SoundService.PlayError();
                FlashErrorBorder();
                UpdateJobCount();
            });

            _gwService.DiskCompleted += (s, e) =>
            {
                SafeInvoke(() =>
                {
                    SoundService.PlaySuccess();
                    if (_jobPanels.TryGetValue(e.Job.Id, out var p)) p.UpdateFromJob();

                    using var dlg = new NextDiskDialog(
                        e.DiskNumber,
                        e.CompletedFile,
                        e.NextFile,
                        e.Duration,
                        e.Job.DateTimeFormat);

                    dlg.ShowDialog(this);
                    e.Signal(dlg.Choice == NextDiskDialog.NextDiskResult.Go);
                });
            };
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
            if (dlg.ShowDialog(this) == DialogResult.OK && dlg.Result != null)
                StartJob(dlg.Result);
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
                logJob =>
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
                },
                restartJob =>
                {
                    if (restartJob.SourcePreset != null)
                        OpenNewJobDialog(restartJob.Device, restartJob.SourcePreset);
                    else
                        OpenNewJobDialog(restartJob.Device);
                });

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
                    d => OpenNewJobDialog(preselectedDevice: d));
                _deviceBar.Controls.Add(dp);
            }
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

        private bool _errorFlash;
        private System.Windows.Forms.Timer? _flashBorderTimer;

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
            Icon icon = Icon.ExtractAssociatedIcon(@"icon\favicon.ico");
            return icon;
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
