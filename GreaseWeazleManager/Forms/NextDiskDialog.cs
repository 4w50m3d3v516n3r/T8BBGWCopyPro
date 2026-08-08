using System;
using System.Drawing;
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
    public partial class NextDiskDialog : Form
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

        private readonly System.Windows.Forms.Timer _pulseTimer = new() { Interval = 600 };
        private int _dots = 0;

        /// <summary>Design-time-only constructor. Do not use at runtime.</summary>
        public NextDiskDialog()
        {
            InitializeComponent();
        }

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
        /// <param name="deviceText">Display name of the GreaseWeazle device in use (name + COM port).</param>
        /// <param name="driveText">Drive address in use, or a localized "(auto)" placeholder.</param>
        public NextDiskDialog(
            int      completedDiskNumber,
            string   completedFile,
            string   nextFile,
            TimeSpan lastDuration,
            string   dateTimeFormat,
            string   deviceText,
            string   driveText)
        {
            InitializeComponent();
            PopulateContent(completedDiskNumber, completedFile, nextFile,
                lastDuration, dateTimeFormat, deviceText, driveText);

            _pulseTimer.Tick += PulseTimer_Tick;
            _pulseTimer.Start();
        }

        /// <summary>Fills in the per-instance text that InitializeComponent cannot know statically.</summary>
        private void PopulateContent(
            int completedDisk, string completedFile, string nextFile,
            TimeSpan lastDuration, string dtFmt, string deviceText, string driveText)
        {
            lblDone.Text = $"✓  {string.Format(L10n.T("nextdisk.done_disk"), completedDisk)}";
            lblDevice.Text = string.Format(L10n.T("nextdisk.device"), deviceText, driveText);
            lblDoneFile.Text = string.Format(L10n.T("nextdisk.done_file"), completedFile);
            lblDuration.Text = string.Format(L10n.T("nextdisk.duration"), lastDuration.TotalSeconds);
            lblNextFile.Text = nextFile;
            lblDtPreview.Text = string.Format(L10n.T("nextdisk.dt_preview"), DateTime.Now.ToString(dtFmt));
        }

        /// <summary>Advances the "waiting" ellipsis animation by one dot.</summary>
        private void PulseTimer_Tick(object? sender, EventArgs e)
        {
            _dots = (_dots + 1) % 4;
            _lblWaiting.Text = L10n.T("nextdisk.waiting") + new string('.', _dots);
        }

        /// <summary>Records the Go choice, stops the pulse animation, and closes the dialog.</summary>
        private void BtnGo_Click(object? sender, EventArgs e)
        {
            Choice = NextDiskResult.Go;
            _pulseTimer.Stop();
            DialogResult = DialogResult.OK;
        }

        /// <summary>Records the Stop choice, stops the pulse animation, and closes the dialog.</summary>
        private void BtnStop_Click(object? sender, EventArgs e)
        {
            Choice = NextDiskResult.Stop;
            _pulseTimer.Stop();
            DialogResult = DialogResult.Cancel;
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
            if (disposing)
            {
                _pulseTimer.Stop();
                _pulseTimer.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
