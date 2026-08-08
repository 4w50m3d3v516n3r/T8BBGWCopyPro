#nullable enable
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using GwCopyPro.Services;

namespace GwCopyPro.Forms
{
    partial class MainForm
    {
        /// <summary>Required designer variable.</summary>
        private IContainer? components = null;

        /// <summary>Clean up any resources being used.</summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _flashBorderTimer?.Stop();
                _flashBorderTimer?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

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
        /// Required method for Designer support - do not modify the contents of this method with the code editor.
        /// Builds the top toolbar, device strip, jobs scroll area, and status bar and
        /// adds them to the form in reverse dock order.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._topBar = new Panel();
            var lblTitle = new Label();
            this._btnNewJob = new Button();
            this._btnDevices = new Button();
            this._btnSettings = new Button();
            this._btnClear = new Button();
            var deviceHeaderBar = new Panel();
            this._lblDevices = new Label();
            this._deviceBar = new FlowLayoutPanel();
            var jobsHeaderBar = new Panel();
            this._lblJobs = new Label();
            this._lblJobCount = new Label();
            var jobsScroll = new Panel();
            this._jobsFlow = new FlowLayoutPanel();
            var statusBar = new Panel();
            this._lblGwPath = new Label();
            this._statusMsg = new Label();
            this._statusTimer = new System.Windows.Forms.Timer(this.components) { Interval = 4000 };
            this.SuspendLayout();
            //
            // _topBar
            //
            this._topBar.Dock = DockStyle.Top;
            this._topBar.Height = 52;
            this._topBar.BackColor = Color.FromArgb(16, 20, 32);
            this._topBar.Paint += new PaintEventHandler(this.TopBar_Paint);
            //
            // lblTitle
            //
            lblTitle.Text = "The8BitBox™ - Ilija Injac\nPresents - GW COPY PRO";
            lblTitle.Font = new Font("Consolas", 10f, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(100, 180, 255);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(14, 14);
            lblTitle.BackColor = Color.Transparent;
            //
            // _btnNewJob
            //
            this._btnNewJob.Text = L10n.T("btn.new_job");
            this._btnNewJob.Location = new Point(320, 12);
            this._btnNewJob.Size = new Size(148, 30);
            this._btnNewJob.FlatStyle = FlatStyle.Flat;
            this._btnNewJob.BackColor = Color.FromArgb(20, 70, 40);
            this._btnNewJob.ForeColor = Color.FromArgb(80, 230, 120);
            this._btnNewJob.Font = new Font("Consolas", 8.5f, FontStyle.Bold);
            this._btnNewJob.FlatAppearance.BorderColor = Color.FromArgb(50, 140, 80);
            this._btnNewJob.Click += new System.EventHandler(this.BtnNewJob_Click);
            //
            // _btnDevices
            //
            this._btnDevices.Text = L10n.T("btn.devices");
            this._btnDevices.Location = new Point(478, 12);
            this._btnDevices.Size = new Size(148, 30);
            this._btnDevices.FlatStyle = FlatStyle.Flat;
            this._btnDevices.BackColor = Color.FromArgb(20, 40, 80);
            this._btnDevices.ForeColor = Color.FromArgb(100, 160, 255);
            this._btnDevices.Font = new Font("Consolas", 8.5f, FontStyle.Bold);
            this._btnDevices.FlatAppearance.BorderColor = Color.FromArgb(50, 90, 180);
            this._btnDevices.Click += new System.EventHandler(this.BtnDevices_Click);
            //
            // _btnSettings
            //
            this._btnSettings.Text = L10n.T("btn.settings");
            this._btnSettings.Location = new Point(636, 12);
            this._btnSettings.Size = new Size(148, 30);
            this._btnSettings.FlatStyle = FlatStyle.Flat;
            this._btnSettings.BackColor = Color.FromArgb(40, 35, 20);
            this._btnSettings.ForeColor = Color.FromArgb(220, 180, 80);
            this._btnSettings.Font = new Font("Consolas", 8.5f, FontStyle.Bold);
            this._btnSettings.FlatAppearance.BorderColor = Color.FromArgb(120, 100, 40);
            this._btnSettings.Click += new System.EventHandler(this.BtnSettings_Click);
            //
            // _btnClear
            //
            this._btnClear.Text = L10n.T("btn.clear_done");
            this._btnClear.Location = new Point(794, 12);
            this._btnClear.Size = new Size(148, 30);
            this._btnClear.FlatStyle = FlatStyle.Flat;
            this._btnClear.BackColor = Color.FromArgb(50, 25, 25);
            this._btnClear.ForeColor = Color.FromArgb(220, 100, 100);
            this._btnClear.Font = new Font("Consolas", 8.5f, FontStyle.Bold);
            this._btnClear.FlatAppearance.BorderColor = Color.FromArgb(120, 50, 50);
            this._btnClear.Click += new System.EventHandler(this.BtnClearDone_Click);
            //
            // _topBar contents
            //
            this._topBar.Controls.Add(lblTitle);
            this._topBar.Controls.Add(this._btnNewJob);
            this._topBar.Controls.Add(this._btnDevices);
            this._topBar.Controls.Add(this._btnSettings);
            this._topBar.Controls.Add(this._btnClear);
            //
            // deviceHeaderBar
            //
            deviceHeaderBar.Dock = DockStyle.Top;
            deviceHeaderBar.Height = 20;
            deviceHeaderBar.BackColor = Color.FromArgb(14, 18, 28);
            //
            // _lblDevices
            //
            this._lblDevices.Text = L10n.T("app.devices");
            this._lblDevices.Font = new Font("Consolas", 7f, FontStyle.Bold);
            this._lblDevices.ForeColor = Color.FromArgb(70, 100, 150);
            this._lblDevices.AutoSize = true;
            this._lblDevices.Location = new Point(10, 4);
            this._lblDevices.BackColor = Color.Transparent;
            deviceHeaderBar.Controls.Add(this._lblDevices);
            //
            // _deviceBar
            //
            this._deviceBar.Dock = DockStyle.Top;
            this._deviceBar.Height = 148;
            this._deviceBar.BackColor = Color.FromArgb(16, 18, 28);
            this._deviceBar.FlowDirection = FlowDirection.LeftToRight;
            this._deviceBar.WrapContents = false;
            this._deviceBar.AutoScroll = true;
            this._deviceBar.Padding = new Padding(6);
            //
            // jobsHeaderBar
            //
            jobsHeaderBar.Dock = DockStyle.Top;
            jobsHeaderBar.Height = 24;
            jobsHeaderBar.BackColor = Color.FromArgb(16, 20, 32);
            //
            // _lblJobs
            //
            this._lblJobs.Text = L10n.T("app.active_jobs");
            this._lblJobs.Font = new Font("Consolas", 7.5f, FontStyle.Bold);
            this._lblJobs.ForeColor = Color.FromArgb(70, 100, 150);
            this._lblJobs.AutoSize = true;
            this._lblJobs.Location = new Point(12, 5);
            this._lblJobs.BackColor = Color.Transparent;
            //
            // _lblJobCount
            //
            this._lblJobCount.Font = new Font("Consolas", 7.5f);
            this._lblJobCount.ForeColor = Color.FromArgb(55, 85, 125);
            this._lblJobCount.AutoSize = true;
            this._lblJobCount.Location = new Point(180, 5);
            this._lblJobCount.BackColor = Color.Transparent;
            jobsHeaderBar.Controls.Add(this._lblJobs);
            jobsHeaderBar.Controls.Add(this._lblJobCount);
            //
            // jobsScroll
            //
            jobsScroll.Dock = DockStyle.Fill;
            jobsScroll.BackColor = Color.FromArgb(14, 16, 24);
            jobsScroll.AutoScroll = true;
            //
            // _jobsFlow
            //
            this._jobsFlow.FlowDirection = FlowDirection.TopDown;
            this._jobsFlow.WrapContents = false;
            this._jobsFlow.AutoSize = true;
            this._jobsFlow.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this._jobsFlow.Padding = new Padding(8);
            this._jobsFlow.BackColor = Color.FromArgb(14, 16, 24);
            jobsScroll.Controls.Add(this._jobsFlow);
            //
            // statusBar
            //
            statusBar.Dock = DockStyle.Bottom;
            statusBar.Height = 24;
            statusBar.BackColor = Color.FromArgb(12, 16, 26);
            //
            // _lblGwPath
            //
            this._lblGwPath.Font = new Font("Consolas", 7.5f);
            this._lblGwPath.ForeColor = Color.FromArgb(75, 105, 145);
            this._lblGwPath.AutoSize = true;
            this._lblGwPath.Location = new Point(8, 5);
            this._lblGwPath.BackColor = Color.Transparent;
            //
            // _statusMsg
            //
            this._statusMsg.Text = L10n.T("app.ready");
            this._statusMsg.Font = new Font("Consolas", 7.5f);
            this._statusMsg.ForeColor = Color.FromArgb(90, 175, 90);
            this._statusMsg.AutoSize = true;
            this._statusMsg.Location = new Point(420, 5);
            this._statusMsg.BackColor = Color.Transparent;
            statusBar.Controls.Add(this._lblGwPath);
            statusBar.Controls.Add(this._statusMsg);
            //
            // _statusTimer
            //
            this._statusTimer.Tick += new System.EventHandler(this.StatusTimer_Tick);
            //
            // MainForm
            //
            this.Text = L10n.T("app.title");
            this.Size = new Size(1120, 900);
            this.MinimumSize = new Size(1060, 600);
            this.BackColor = Color.FromArgb(14, 16, 24);
            this.ForeColor = Color.FromArgb(180, 210, 255);
            this.Controls.Add(jobsScroll);
            this.Controls.Add(jobsHeaderBar);
            this.Controls.Add(this._deviceBar);
            this.Controls.Add(deviceHeaderBar);
            this.Controls.Add(this._topBar);
            this.Controls.Add(statusBar);
            this.ResumeLayout(false);
        }
    }
}
