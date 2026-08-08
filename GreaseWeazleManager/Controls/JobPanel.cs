using System;
using System.Drawing;
using System.Windows.Forms;
using GwCopyPro.Models;
using GwCopyPro.Services;

namespace GwCopyPro.Controls
{
    /// <summary>
    /// A fixed-size panel that displays the real-time status of a single <see cref="GwJob"/>.
    /// Contains a title bar, progress bar, two <see cref="FloppyDiskControl"/> visualisers
    /// (one per head), a live log pane, and Cancel / View Log / Restart buttons.
    /// The border colour and a flash animation reflect the current job status.
    /// </summary>
    public partial class JobPanel : UserControl
    {
        private const int LEFT_PAD  = 8;
        private const int RIGHT_COL = 230;
        private const int LOG_X     = 808;
        private const int PANEL_W   = 1038;
        private const int TITLE_Y   = 7;
        private const int STATUS_Y  = 26;
        private const int PROG_Y    = 44;
        private const int PROG_H    = 8;
        private const int SIDE0_Y   = 58;
        private const int SIDE1_Y   = 115;
        private const int PANEL_H   = 178;

        private readonly GwJob _job = new();
        private bool _flashState;
        private readonly Action<GwJob>? _cancelCallback;
        private readonly Action<GwJob>? _logCallback;
        private readonly Action<GwJob>? _restartCallback;

        /// <summary>The <see cref="GwJob"/> this panel represents.</summary>
        public GwJob Job => _job;

        /// <summary>Design-time-only constructor. Do not use at runtime.</summary>
        public JobPanel()
        {
            InitializeComponent();
            SetDoubleBuffered();
        }

        /// <summary>
        /// Initialises the job panel, builds all child controls, and performs an initial
        /// display update to reflect the job's current state.
        /// </summary>
        /// <param name="job">The job to visualise.</param>
        /// <param name="cancelCallback">Invoked when the user clicks Cancel.</param>
        /// <param name="logCallback">Invoked when the user clicks View Log.</param>
        /// <param name="restartCallback">Invoked when the user clicks Restart.</param>
        public JobPanel(GwJob job,
            Action<GwJob> cancelCallback,
            Action<GwJob> logCallback,
            Action<GwJob> restartCallback)
        {
            _job             = job;
            _cancelCallback  = cancelCallback;
            _logCallback     = logCallback;
            _restartCallback = restartCallback;

            InitializeComponent();
            SetDoubleBuffered();

            UpdateTrackVisualisers();
            UpdateDisplay();
        }

        /// <summary>
        /// Enables flicker-free custom drawing. Set here rather than in InitializeComponent
        /// because the WinForms Designer's CodeDom reader cannot represent a bare method call.
        /// </summary>
        private void SetDoubleBuffered() => SetStyle(ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

        /// <summary>
        /// Refreshes all displayed values from the underlying <see cref="GwJob"/>.
        /// Safe to call from any thread; marshals to the UI thread automatically.
        /// Appends newly captured log lines to the inline log pane with syntax colouring.
        /// </summary>
        public void UpdateFromJob()
        {
            if (InvokeRequired) { Invoke(UpdateFromJob); return; }

            UpdateTrackVisualisers();
            UpdateProgressAndStatusColor();
            AppendNewLogLines();
            UpdateDisplay();
            Invalidate();
        }

        /// <summary>Pushes the current per-track status grid into both head visualisers.</summary>
        private void UpdateTrackVisualisers()
        {
            _side0.SetTracks(_job.Tracks);
            _side1.SetTracks(_job.Tracks);
        }

        /// <summary>Updates the progress bar, status text/colour, and the flash-timer run state for the current job status.</summary>
        private void UpdateProgressAndStatusColor()
        {
            _progress.Value = Math.Max(0, Math.Min(100, (int)_job.ProgressPercent));
            _lblStatus.Text = _job.StatusText;

            switch (_job.Status)
            {
                case JobStatus.Running:
                    _lblStatus.ForeColor = Color.FromArgb(90, 200, 255);
                    if (!_flashTimer.Enabled) _flashTimer.Start();
                    break;
                case JobStatus.Completed:
                    _lblStatus.ForeColor = Color.FromArgb(75, 215, 100);
                    _flashTimer.Stop();
                    _progress.Value  = 100;
                    _btnRestart.Enabled = _job.SourcePreset != null;
                    break;
                case JobStatus.Error:
                    _lblStatus.ForeColor = Color.FromArgb(235, 75, 75);
                    _flashTimer.Stop();
                    _btnRestart.Enabled = _job.SourcePreset != null;
                    break;
                case JobStatus.Cancelled:
                    _flashTimer.Stop();
                    _btnRestart.Enabled = _job.SourcePreset != null;
                    break;
                default:
                    _flashTimer.Stop();
                    break;
            }
        }

        /// <summary>Appends any log lines captured since the last update, colour-coded by content.</summary>
        private void AppendNewLogLines()
        {
            int existing = _logBox.Lines.Length;
            for (int i = existing; i < _job.LogLines.Count; i++)
            {
                string line  = _job.LogLines[i];
                Color  color = line.StartsWith("[ERR")    ? Color.FromArgb(235, 80, 80)
                             : line.Contains("ok") || line.Contains("good")
                                                          ? Color.FromArgb(75, 215, 100)
                             : Color.FromArgb(90, 195, 90);
                _logBox.SelectionStart  = _logBox.TextLength;
                _logBox.SelectionLength = 0;
                _logBox.SelectionColor  = color;
                _logBox.AppendText(line + "\n");
            }
            if (_job.LogLines.Count > existing) _logBox.ScrollToCaret();
        }

        /// <summary>
        /// Rebuilds the title label text from the current job state and updates
        /// whether the Cancel button is enabled.
        /// </summary>
        private void UpdateDisplay()
        {
            string icon   = _job.JobType == JobType.Read ? "▼ READ" : "▲ WRITE";
            string device = _job.Device?.Name ?? "No Device";
            string file   = System.IO.Path.GetFileName(_job.Parameters.ImageFile ?? "?");
            string fmt    = _job.Parameters.DiskFormat ?? "auto";
            string diskInfo = _job.RepetitiveMode
                ? $"  │  {string.Format(L10n.T("job.disk_n"), _job.DiskIndex)}"
                : "";
            _lblTitle.Text     = $"{icon}  [{device}]  {file}  │  {fmt}{diskInfo}";
            _btnCancel.Enabled = _job.Status == JobStatus.Running;
        }

        private void BtnCancel_Click(object? sender, EventArgs e) => _cancelCallback?.Invoke(_job);

        private void BtnLog_Click(object? sender, EventArgs e) => _logCallback?.Invoke(_job);

        private void BtnRestart_Click(object? sender, EventArgs e) => _restartCallback?.Invoke(_job);

        /// <summary>Toggles the flashing state of the running-job border and repaints.</summary>
        private void FlashTimer_Tick(object? sender, EventArgs e)
        {
            _flashState = !_flashState;
            Invalidate();
        }

        /// <summary>
        /// Paints the panel border (colour reflects job status), a 4-pixel left accent bar,
        /// and a vertical separator between the visualiser column and the log column.
        /// The border flashes while the job is running.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;

            Color border = _job.Status switch
            {
                JobStatus.Running   => _flashState ? Color.FromArgb(55, 135, 225) : Color.FromArgb(35, 85, 155),
                JobStatus.Completed => Color.FromArgb(35, 155, 75),
                JobStatus.Error     => Color.FromArgb(195, 45, 45),
                JobStatus.Cancelled => Color.FromArgb(115, 95, 35),
                _                   => Color.FromArgb(38, 48, 68)
            };

            using var pen = new Pen(border, 1.5f);
            g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            using var accent = new SolidBrush(border);
            g.FillRectangle(accent, 0, 0, 4, Height);
            using var sep = new Pen(Color.FromArgb(38, 48, 68), 1f);
            g.DrawLine(sep, LOG_X, 4, LOG_X, Height - 4);
        }
    }
}
