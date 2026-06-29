using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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
    public class JobPanel : Panel
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

        private readonly GwJob            _job;
        private readonly FloppyDiskControl _side0;
        private readonly FloppyDiskControl _side1;
        private readonly Label            _lblTitle;
        private readonly Label            _lblStatus;
        private readonly ProgressBar      _progress;
        private readonly Button           _btnCancel;
        private readonly Button           _btnLog;
        private readonly Button           _btnRestart;
        private readonly RichTextBox      _logBox;
        private readonly System.Windows.Forms.Timer _flashTimer;
        private bool _flashState;
        private readonly Action<GwJob>? _cancelCallback;
        private readonly Action<GwJob>? _logCallback;
        private readonly Action<GwJob>? _restartCallback;

        /// <summary>The <see cref="GwJob"/> this panel represents.</summary>
        public GwJob Job => _job;

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

            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);

            BackColor = Color.FromArgb(22, 26, 36);
            Size      = new Size(PANEL_W, PANEL_H);
            Margin    = new Padding(6, 6, 6, 0);

            _lblTitle = new Label
            {
                Font      = new Font("Consolas", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(155, 195, 255),
                AutoSize  = false,
                Size      = new Size(LOG_X - LEFT_PAD - 4, 18),
                Location  = new Point(LEFT_PAD, TITLE_Y),
                BackColor = Color.Transparent
            };

            _lblStatus = new Label
            {
                Font      = new Font("Consolas", 8f),
                ForeColor = Color.FromArgb(110, 165, 110),
                AutoSize  = false,
                Size      = new Size(LOG_X - LEFT_PAD - 4, 16),
                Location  = new Point(LEFT_PAD, STATUS_Y),
                BackColor = Color.Transparent
            };

            _progress = new ProgressBar
            {
                Location = new Point(LEFT_PAD, PROG_Y),
                Size     = new Size(FloppyDiskControl.ControlWidth, PROG_H),
                Minimum  = 0,
                Maximum  = 100,
                Style    = ProgressBarStyle.Continuous
            };

            _side0 = new FloppyDiskControl
            {
                Location  = new Point(LEFT_PAD, SIDE0_Y),
                SideLabel = "Side 0  (Head 0 — Upper)"
            };

            _side1 = new FloppyDiskControl
            {
                Location  = new Point(LEFT_PAD, SIDE1_Y),
                SideLabel = "Side 1  (Head 1 — Lower)",
                Head      = 1
            };

            _side0.SetTracks(job.Tracks);
            _side1.SetTracks(job.Tracks);

            int btnW = RIGHT_COL - 12;

            _btnCancel = MakeBtn(L10n.T("job.cancel"), LOG_X + 4, TITLE_Y,
                btnW, 22,
                Color.FromArgb(90, 25, 25), Color.FromArgb(240, 120, 120),
                Color.FromArgb(160, 50, 50));
            _btnCancel.Click += (s, e) => _cancelCallback?.Invoke(_job);

            _btnLog = MakeBtn(L10n.T("job.view_log"), LOG_X + 4, TITLE_Y + 28,
                btnW, 22,
                Color.FromArgb(22, 45, 75), Color.FromArgb(130, 185, 255),
                Color.FromArgb(50, 90, 160));
            _btnLog.Click += (s, e) => _logCallback?.Invoke(_job);

            _btnRestart = MakeBtn(L10n.T("job.restart"), LOG_X + 4, TITLE_Y + 56,
                btnW, 22,
                Color.FromArgb(40, 35, 12), Color.FromArgb(220, 185, 60),
                Color.FromArgb(110, 95, 30));
            _btnRestart.Click   += (s, e) => _restartCallback?.Invoke(_job);
            _btnRestart.Enabled  = false;

            _logBox = new RichTextBox
            {
                Location    = new Point(LOG_X + 4, TITLE_Y + 84),
                Size        = new Size(btnW, PANEL_H - TITLE_Y - 84 - 6),
                BackColor   = Color.FromArgb(12, 14, 20),
                ForeColor   = Color.FromArgb(90, 195, 90),
                Font        = new Font("Consolas", 6.5f),
                ScrollBars  = RichTextBoxScrollBars.Vertical,
                ReadOnly    = true,
                BorderStyle = BorderStyle.None,
                WordWrap    = false
            };

            Controls.AddRange(new Control[]
            {
                _lblTitle, _lblStatus, _progress,
                _side0, _side1,
                _btnCancel, _btnLog, _btnRestart, _logBox
            });

            _flashTimer = new System.Windows.Forms.Timer { Interval = 500 };
            _flashTimer.Tick += (s, e) => { _flashState = !_flashState; Invalidate(); };

            UpdateDisplay();
        }

        /// <summary>
        /// Refreshes all displayed values from the underlying <see cref="GwJob"/>.
        /// Safe to call from any thread; marshals to the UI thread automatically.
        /// Appends newly captured log lines to the inline log pane with syntax colouring.
        /// </summary>
        public void UpdateFromJob()
        {
            if (InvokeRequired) { Invoke(UpdateFromJob); return; }

            _side0.SetTracks(_job.Tracks);
            _side1.SetTracks(_job.Tracks);

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

            UpdateDisplay();
            Invalidate();
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

        /// <summary>
        /// Creates a flat-styled button with the given text, position, size, and colours.
        /// </summary>
        /// <param name="text">Button label.</param>
        /// <param name="x">Left position.</param>
        /// <param name="y">Top position.</param>
        /// <param name="w">Width in pixels.</param>
        /// <param name="h">Height in pixels.</param>
        /// <param name="bg">Background colour.</param>
        /// <param name="fg">Foreground (text) colour.</param>
        /// <param name="border">Border colour.</param>
        /// <returns>The configured <see cref="Button"/>.</returns>
        private static Button MakeBtn(string text, int x, int y, int w, int h,
            Color bg, Color fg, Color border)
        {
            var b = new Button
            {
                Text      = text,
                Location  = new Point(x, y),
                Size      = new Size(w, h),
                FlatStyle = FlatStyle.Flat,
                BackColor = bg,
                ForeColor = fg,
                Font      = new Font("Consolas", 7.5f)
            };
            b.FlatAppearance.BorderColor = border;
            return b;
        }

        /// <summary>
        /// Stops and disposes the flash timer before releasing other resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing) { _flashTimer?.Stop(); _flashTimer?.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
