using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GwCopyPro.Services;

namespace GwCopyPro.Forms
{
    /// <summary>
    /// Shown between disks in repetitive-mode jobs. Displays a summary of the disk
    /// that just completed and the filename that will be used for the next disk.
    /// The user chooses to continue (<c>Go</c>) or stop (<c>No More</c>).
    /// A pulsing animation runs until the user makes a choice.
    /// </summary>
    public class NextDiskDialog : Form
    {
        /// <summary>Represents the user's response to the prompt.</summary>
        public enum NextDiskResult
        {
            /// <summary>Insert the next disk and continue the job.</summary>
            Go,
            /// <summary>Stop the repetitive job after this disk.</summary>
            Stop
        }

        /// <summary>Gets the choice the user made when dismissing the dialog.</summary>
        public NextDiskResult Choice { get; private set; } = NextDiskResult.Stop;

        private readonly System.Windows.Forms.Timer _pulseTimer;
        private Label _lblWaiting = null!;
        private int   _dots       = 0;

        /// <summary>
        /// Initialises the dialog with information about the completed and next disk.
        /// </summary>
        /// <param name="completedDiskNumber">One-based index of the disk that just finished.</param>
        /// <param name="completedFile">Filename that was written for the completed disk.</param>
        /// <param name="nextFile">Filename that will be written for the next disk.</param>
        /// <param name="lastDuration">Wall-clock time taken for the completed disk.</param>
        /// <param name="dateTimeFormat">
        /// Format string used to preview how the <c>{dt}</c> token will expand for the next disk.
        /// </param>
        public NextDiskDialog(
            int      completedDiskNumber,
            string   completedFile,
            string   nextFile,
            TimeSpan lastDuration,
            string   dateTimeFormat)
        {
            InitializeComponent(completedDiskNumber, completedFile, nextFile,
                                lastDuration, dateTimeFormat);

            _pulseTimer = new System.Windows.Forms.Timer { Interval = 600 };
            _pulseTimer.Tick += (s, e) =>
            {
                _dots = (_dots + 1) % 4;
                _lblWaiting.Text = L10n.T("nextdisk.waiting") + new string('.', _dots);
            };
            _pulseTimer.Start();
        }

        /// <summary>Builds and lays out all child controls.</summary>
        private void InitializeComponent(
            int completedDisk, string completedFile, string nextFile,
            TimeSpan lastDuration, string dtFmt)
        {
            Text            = L10n.T("nextdisk.title");
            Size            = new Size(560, 380);
            MaximumSize     = Size;
            MinimumSize     = Size;
            BackColor       = Color.FromArgb(18, 22, 32);
            ForeColor       = Color.FromArgb(180, 210, 255);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition   = FormStartPosition.CenterParent;
            MaximizeBox     = false;

            var accent = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 4,
                BackColor = Color.FromArgb(40, 160, 80)
            };
            Controls.Add(accent);

            int y = 18;

            AddLabel($"✓  {string.Format(L10n.T("nextdisk.done_disk"), completedDisk)}",
                14, y, 520, 20,
                new Font("Consolas", 10f, FontStyle.Bold),
                Color.FromArgb(80, 215, 110));
            y += 28;

            AddLabel(string.Format(L10n.T("nextdisk.done_file"), completedFile),
                14, y, 520, 16,
                new Font("Consolas", 8f),
                Color.FromArgb(100, 140, 180));
            y += 22;

            AddLabel(string.Format(L10n.T("nextdisk.duration"), lastDuration.TotalSeconds),
                14, y, 520, 16,
                new Font("Consolas", 8f),
                Color.FromArgb(100, 140, 180));
            y += 30;

            AddSep(14, y, 520); y += 14;

            AddLabel(L10n.T("nextdisk.next_label"),
                14, y, 520, 16,
                new Font("Consolas", 8.5f, FontStyle.Bold),
                Color.FromArgb(160, 200, 255));
            y += 22;

            var lblNextFile = new Label
            {
                Text      = nextFile,
                Location  = new Point(14, y),
                Size      = new Size(520, 20),
                Font      = new Font("Consolas", 9f),
                ForeColor = Color.FromArgb(220, 200, 100),
                BackColor = Color.Transparent,
                AutoSize  = false
            };
            Controls.Add(lblNextFile);
            y += 28;

            AddLabel(string.Format(L10n.T("nextdisk.dt_preview"),
                        DateTime.Now.ToString(dtFmt)),
                14, y, 520, 16,
                new Font("Consolas", 7.5f),
                Color.FromArgb(90, 120, 160));
            y += 30;

            _lblWaiting = new Label
            {
                Text      = L10n.T("nextdisk.waiting"),
                Location  = new Point(14, y),
                Size      = new Size(520, 22),
                Font      = new Font("Consolas", 9f, FontStyle.Italic),
                ForeColor = Color.FromArgb(80, 160, 220),
                BackColor = Color.Transparent,
                AutoSize  = false
            };
            Controls.Add(_lblWaiting);
            y += 36;

            AddSep(14, y, 520); y += 14;

            var btnGo = new Button
            {
                Text      = L10n.T("nextdisk.btn_go"),
                Location  = new Point(14, y),
                Size      = new Size(300, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(18, 65, 32),
                ForeColor = Color.FromArgb(80, 230, 120),
                Font      = new Font("Consolas", 11f, FontStyle.Bold)
            };
            btnGo.FlatAppearance.BorderColor = Color.FromArgb(45, 140, 75);
            btnGo.Click += (s, e) =>
            {
                Choice       = NextDiskResult.Go;
                _pulseTimer.Stop();
                DialogResult = DialogResult.OK;
            };

            var btnStop = new Button
            {
                Text      = L10n.T("nextdisk.btn_stop"),
                Location  = new Point(326, y),
                Size      = new Size(208, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(55, 20, 20),
                ForeColor = Color.FromArgb(220, 90, 90),
                Font      = new Font("Consolas", 9.5f)
            };
            btnStop.FlatAppearance.BorderColor = Color.FromArgb(120, 45, 45);
            btnStop.Click += (s, e) =>
            {
                Choice       = NextDiskResult.Stop;
                _pulseTimer.Stop();
                DialogResult = DialogResult.Cancel;
            };

            Controls.AddRange(new Control[] { btnGo, btnStop });
            AcceptButton = btnGo;
            CancelButton = btnStop;
        }

        /// <summary>Adds a styled label at the given position.</summary>
        /// <param name="text">Label text.</param>
        /// <param name="x">Left position.</param>
        /// <param name="y">Top position.</param>
        /// <param name="w">Width.</param>
        /// <param name="h">Height.</param>
        /// <param name="font">Font to apply.</param>
        /// <param name="color">Foreground colour.</param>
        private void AddLabel(string text, int x, int y, int w, int h,
            Font font, Color color)
        {
            Controls.Add(new Label
            {
                Text      = text,
                Location  = new Point(x, y),
                Size      = new Size(w, h),
                Font      = font,
                ForeColor = color,
                BackColor = Color.Transparent,
                AutoSize  = false
            });
        }

        /// <summary>Adds a 1-pixel horizontal rule as a visual section separator.</summary>
        /// <param name="x">Left position.</param>
        /// <param name="y">Top position.</param>
        /// <param name="w">Width.</param>
        private void AddSep(int x, int y, int w)
        {
            Controls.Add(new Label
            {
                Location  = new Point(x, y),
                Size      = new Size(w, 1),
                BackColor = Color.FromArgb(40, 60, 90)
            });
        }

        /// <summary>Paints a thin border around the dialog.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using var pen = new Pen(Color.FromArgb(35, 55, 85), 1f);
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }

        /// <summary>Stops and disposes the pulse timer before releasing other resources.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing) { _pulseTimer?.Stop(); _pulseTimer?.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
