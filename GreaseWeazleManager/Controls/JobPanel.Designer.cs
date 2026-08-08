#nullable enable
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using GwCopyPro.Services;

namespace GwCopyPro.Controls
{
    partial class JobPanel
    {
        private const int BTN_W = RIGHT_COL - 12;

        /// <summary>Required designer variable.</summary>
        private IContainer? components = null;

        /// <summary>Clean up any resources being used.</summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                components?.Dispose();
            base.Dispose(disposing);
        }

        private Label _lblTitle = null!;
        private Label _lblStatus = null!;
        private ProgressBar _progress = null!;
        private FloppyDiskControl _side0 = null!;
        private FloppyDiskControl _side1 = null!;
        private Button _btnCancel = null!;
        private Button _btnLog = null!;
        private Button _btnRestart = null!;
        private RichTextBox _logBox = null!;
        private System.Windows.Forms.Timer _flashTimer = null!;

        /// <summary>Required method for Designer support - do not modify the contents of this method with the code editor.</summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._lblTitle = new Label();
            this._lblStatus = new Label();
            this._progress = new ProgressBar();
            this._side0 = new FloppyDiskControl();
            this._side1 = new FloppyDiskControl();
            this._btnCancel = new Button();
            this._btnLog = new Button();
            this._btnRestart = new Button();
            this._logBox = new RichTextBox();
            this._flashTimer = new System.Windows.Forms.Timer(this.components) { Interval = 500 };
            this.SuspendLayout();
            //
            // _lblTitle
            //
            this._lblTitle.Font = new Font("Consolas", 9f, FontStyle.Bold);
            this._lblTitle.ForeColor = Color.FromArgb(155, 195, 255);
            this._lblTitle.AutoSize = false;
            this._lblTitle.Size = new Size(LOG_X - LEFT_PAD - 4, 18);
            this._lblTitle.Location = new Point(LEFT_PAD, TITLE_Y);
            this._lblTitle.BackColor = Color.Transparent;
            //
            // _lblStatus
            //
            this._lblStatus.Font = new Font("Consolas", 8f);
            this._lblStatus.ForeColor = Color.FromArgb(110, 165, 110);
            this._lblStatus.AutoSize = false;
            this._lblStatus.Size = new Size(LOG_X - LEFT_PAD - 4, 16);
            this._lblStatus.Location = new Point(LEFT_PAD, STATUS_Y);
            this._lblStatus.BackColor = Color.Transparent;
            //
            // _progress
            //
            this._progress.Location = new Point(LEFT_PAD, PROG_Y);
            this._progress.Size = new Size(FloppyDiskControl.ControlWidth, PROG_H);
            this._progress.Minimum = 0;
            this._progress.Maximum = 100;
            this._progress.Style = ProgressBarStyle.Continuous;
            //
            // _side0
            //
            this._side0.Location = new Point(LEFT_PAD, SIDE0_Y);
            this._side0.SideLabel = "Side 0  (Head 0 — Upper)";
            //
            // _side1
            //
            this._side1.Location = new Point(LEFT_PAD, SIDE1_Y);
            this._side1.SideLabel = "Side 1  (Head 1 — Lower)";
            this._side1.Head = 1;
            //
            // _btnCancel
            //
            this._btnCancel.Text = L10n.T("job.cancel");
            this._btnCancel.Location = new Point(LOG_X + 4, TITLE_Y);
            this._btnCancel.Size = new Size(BTN_W, 22);
            this._btnCancel.FlatStyle = FlatStyle.Flat;
            this._btnCancel.BackColor = Color.FromArgb(90, 25, 25);
            this._btnCancel.ForeColor = Color.FromArgb(240, 120, 120);
            this._btnCancel.Font = new Font("Consolas", 7.5f);
            this._btnCancel.FlatAppearance.BorderColor = Color.FromArgb(160, 50, 50);
            this._btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            //
            // _btnLog
            //
            this._btnLog.Text = L10n.T("job.view_log");
            this._btnLog.Location = new Point(LOG_X + 4, TITLE_Y + 28);
            this._btnLog.Size = new Size(BTN_W, 22);
            this._btnLog.FlatStyle = FlatStyle.Flat;
            this._btnLog.BackColor = Color.FromArgb(22, 45, 75);
            this._btnLog.ForeColor = Color.FromArgb(130, 185, 255);
            this._btnLog.Font = new Font("Consolas", 7.5f);
            this._btnLog.FlatAppearance.BorderColor = Color.FromArgb(50, 90, 160);
            this._btnLog.Click += new System.EventHandler(this.BtnLog_Click);
            //
            // _btnRestart
            //
            this._btnRestart.Text = L10n.T("job.restart");
            this._btnRestart.Location = new Point(LOG_X + 4, TITLE_Y + 56);
            this._btnRestart.Size = new Size(BTN_W, 22);
            this._btnRestart.FlatStyle = FlatStyle.Flat;
            this._btnRestart.BackColor = Color.FromArgb(40, 35, 12);
            this._btnRestart.ForeColor = Color.FromArgb(220, 185, 60);
            this._btnRestart.Font = new Font("Consolas", 7.5f);
            this._btnRestart.FlatAppearance.BorderColor = Color.FromArgb(110, 95, 30);
            this._btnRestart.Enabled = false;
            this._btnRestart.Click += new System.EventHandler(this.BtnRestart_Click);
            //
            // _logBox
            //
            this._logBox.Location = new Point(LOG_X + 4, TITLE_Y + 84);
            this._logBox.Size = new Size(BTN_W, PANEL_H - TITLE_Y - 84 - 6);
            this._logBox.BackColor = Color.FromArgb(12, 14, 20);
            this._logBox.ForeColor = Color.FromArgb(90, 195, 90);
            this._logBox.Font = new Font("Consolas", 6.5f);
            this._logBox.ScrollBars = RichTextBoxScrollBars.Vertical;
            this._logBox.ReadOnly = true;
            this._logBox.BorderStyle = BorderStyle.None;
            this._logBox.WordWrap = false;
            //
            // _flashTimer
            //
            this._flashTimer.Tick += new System.EventHandler(this.FlashTimer_Tick);
            //
            // JobPanel
            //
            this.AutoScaleMode = AutoScaleMode.None;
            this.BackColor = Color.FromArgb(22, 26, 36);
            this.Size = new Size(PANEL_W, PANEL_H);
            this.Margin = new Padding(6, 6, 6, 0);
            this.TabStop = false;
            this.Controls.Add(this._lblTitle);
            this.Controls.Add(this._lblStatus);
            this.Controls.Add(this._progress);
            this.Controls.Add(this._side0);
            this.Controls.Add(this._side1);
            this.Controls.Add(this._btnCancel);
            this.Controls.Add(this._btnLog);
            this.Controls.Add(this._btnRestart);
            this.Controls.Add(this._logBox);
            this.ResumeLayout(false);
        }
    }
}
