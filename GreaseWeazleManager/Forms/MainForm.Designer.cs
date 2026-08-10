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
            components = new Container();
            _topBar = new Panel();
            lblTitle = new Label();
            _btnNewJob = new Button();
            _btnDevices = new Button();
            _btnSettings = new Button();
            _btnClear = new Button();
            deviceHeaderBar = new Panel();
            _lblDevices = new Label();
            _deviceBar = new FlowLayoutPanel();
            jobsHeaderBar = new Panel();
            _lblJobs = new Label();
            _lblJobCount = new Label();
            jobsScroll = new Panel();
            _jobsFlow = new FlowLayoutPanel();
            statusBar = new Panel();
            _lblGwPath = new Label();
            _statusMsg = new Label();
            _statusTimer = new System.Windows.Forms.Timer(components);
            _topBar.SuspendLayout();
            deviceHeaderBar.SuspendLayout();
            jobsHeaderBar.SuspendLayout();
            jobsScroll.SuspendLayout();
            statusBar.SuspendLayout();
            SuspendLayout();
            // 
            // _topBar
            // 
            _topBar.BackColor = Color.FromArgb(16, 20, 32);
            _topBar.Controls.Add(lblTitle);
            _topBar.Controls.Add(_btnNewJob);
            _topBar.Controls.Add(_btnDevices);
            _topBar.Controls.Add(_btnSettings);
            _topBar.Controls.Add(_btnClear);
            _topBar.Dock = DockStyle.Top;
            _topBar.Location = new Point(0, 0);
            _topBar.Name = "_topBar";
            _topBar.Size = new Size(1104, 52);
            _topBar.TabIndex = 4;
            _topBar.Paint += TopBar_Paint;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.BackColor = Color.Transparent;
            lblTitle.Font = new Font("Consolas", 10F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(100, 180, 255);
            lblTitle.Location = new Point(14, 14);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(184, 34);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Ilija Injac\r\nPresents - GW COPY PRO";
            lblTitle.Click += lblTitle_Click;
            // 
            // _btnNewJob
            // 
            _btnNewJob.BackColor = Color.FromArgb(20, 70, 40);
            _btnNewJob.FlatAppearance.BorderColor = Color.FromArgb(50, 140, 80);
            _btnNewJob.FlatStyle = FlatStyle.Flat;
            _btnNewJob.Font = new Font("Consolas", 8.5F, FontStyle.Bold);
            _btnNewJob.ForeColor = Color.FromArgb(80, 230, 120);
            _btnNewJob.Location = new Point(320, 12);
            _btnNewJob.Name = "_btnNewJob";
            _btnNewJob.Size = new Size(148, 30);
            _btnNewJob.TabIndex = 1;
            _btnNewJob.Text = "▶  New Job";
            _btnNewJob.UseVisualStyleBackColor = false;
            _btnNewJob.Click += BtnNewJob_Click;
            // 
            // _btnDevices
            // 
            _btnDevices.BackColor = Color.FromArgb(20, 40, 80);
            _btnDevices.FlatAppearance.BorderColor = Color.FromArgb(50, 90, 180);
            _btnDevices.FlatStyle = FlatStyle.Flat;
            _btnDevices.Font = new Font("Consolas", 8.5F, FontStyle.Bold);
            _btnDevices.ForeColor = Color.FromArgb(100, 160, 255);
            _btnDevices.Location = new Point(478, 12);
            _btnDevices.Name = "_btnDevices";
            _btnDevices.Size = new Size(148, 30);
            _btnDevices.TabIndex = 2;
            _btnDevices.Text = "⬡  Devices";
            _btnDevices.UseVisualStyleBackColor = false;
            _btnDevices.Click += BtnDevices_Click;
            // 
            // _btnSettings
            // 
            _btnSettings.BackColor = Color.FromArgb(40, 35, 20);
            _btnSettings.FlatAppearance.BorderColor = Color.FromArgb(120, 100, 40);
            _btnSettings.FlatStyle = FlatStyle.Flat;
            _btnSettings.Font = new Font("Consolas", 8.5F, FontStyle.Bold);
            _btnSettings.ForeColor = Color.FromArgb(220, 180, 80);
            _btnSettings.Location = new Point(636, 12);
            _btnSettings.Name = "_btnSettings";
            _btnSettings.Size = new Size(148, 30);
            _btnSettings.TabIndex = 3;
            _btnSettings.Text = "⚙  Settings";
            _btnSettings.UseVisualStyleBackColor = false;
            _btnSettings.Click += BtnSettings_Click;
            // 
            // _btnClear
            // 
            _btnClear.BackColor = Color.FromArgb(50, 25, 25);
            _btnClear.FlatAppearance.BorderColor = Color.FromArgb(120, 50, 50);
            _btnClear.FlatStyle = FlatStyle.Flat;
            _btnClear.Font = new Font("Consolas", 8.5F, FontStyle.Bold);
            _btnClear.ForeColor = Color.FromArgb(220, 100, 100);
            _btnClear.Location = new Point(794, 12);
            _btnClear.Name = "_btnClear";
            _btnClear.Size = new Size(148, 30);
            _btnClear.TabIndex = 4;
            _btnClear.Text = "✕  Clear Done";
            _btnClear.UseVisualStyleBackColor = false;
            _btnClear.Click += BtnClearDone_Click;
            // 
            // deviceHeaderBar
            // 
            deviceHeaderBar.BackColor = Color.FromArgb(14, 18, 28);
            deviceHeaderBar.Controls.Add(_lblDevices);
            deviceHeaderBar.Dock = DockStyle.Top;
            deviceHeaderBar.Location = new Point(0, 52);
            deviceHeaderBar.Name = "deviceHeaderBar";
            deviceHeaderBar.Size = new Size(1104, 20);
            deviceHeaderBar.TabIndex = 3;
            // 
            // _lblDevices
            // 
            _lblDevices.AutoSize = true;
            _lblDevices.BackColor = Color.Transparent;
            _lblDevices.Font = new Font("Consolas", 7F, FontStyle.Bold);
            _lblDevices.ForeColor = Color.FromArgb(70, 100, 150);
            _lblDevices.Location = new Point(10, 4);
            _lblDevices.Name = "_lblDevices";
            _lblDevices.Size = new Size(40, 12);
            _lblDevices.TabIndex = 0;
            _lblDevices.Text = "DEVICES";
            // 
            // _deviceBar
            // 
            _deviceBar.AutoScroll = true;
            _deviceBar.BackColor = Color.FromArgb(16, 18, 28);
            _deviceBar.Dock = DockStyle.Top;
            _deviceBar.Location = new Point(0, 72);
            _deviceBar.Name = "_deviceBar";
            _deviceBar.Padding = new Padding(6);
            _deviceBar.Size = new Size(1104, 148);
            _deviceBar.TabIndex = 2;
            _deviceBar.WrapContents = false;
            // 
            // jobsHeaderBar
            // 
            jobsHeaderBar.BackColor = Color.FromArgb(16, 20, 32);
            jobsHeaderBar.Controls.Add(_lblJobs);
            jobsHeaderBar.Controls.Add(_lblJobCount);
            jobsHeaderBar.Dock = DockStyle.Top;
            jobsHeaderBar.Location = new Point(0, 220);
            jobsHeaderBar.Name = "jobsHeaderBar";
            jobsHeaderBar.Size = new Size(1104, 24);
            jobsHeaderBar.TabIndex = 1;
            // 
            // _lblJobs
            // 
            _lblJobs.AutoSize = true;
            _lblJobs.BackColor = Color.Transparent;
            _lblJobs.Font = new Font("Consolas", 7.5F, FontStyle.Bold);
            _lblJobs.ForeColor = Color.FromArgb(70, 100, 150);
            _lblJobs.Location = new Point(12, 5);
            _lblJobs.Name = "_lblJobs";
            _lblJobs.Size = new Size(60, 12);
            _lblJobs.TabIndex = 0;
            _lblJobs.Text = "ACTIVE JOBS";
            // 
            // _lblJobCount
            // 
            _lblJobCount.AutoSize = true;
            _lblJobCount.BackColor = Color.Transparent;
            _lblJobCount.Font = new Font("Consolas", 7.5F);
            _lblJobCount.ForeColor = Color.FromArgb(55, 85, 125);
            _lblJobCount.Location = new Point(180, 5);
            _lblJobCount.Name = "_lblJobCount";
            _lblJobCount.Size = new Size(0, 12);
            _lblJobCount.TabIndex = 1;
            // 
            // jobsScroll
            // 
            jobsScroll.AutoScroll = true;
            jobsScroll.BackColor = Color.FromArgb(14, 16, 24);
            jobsScroll.Controls.Add(_jobsFlow);
            jobsScroll.Dock = DockStyle.Fill;
            jobsScroll.Location = new Point(0, 244);
            jobsScroll.Name = "jobsScroll";
            jobsScroll.Size = new Size(1104, 593);
            jobsScroll.TabIndex = 0;
            // 
            // _jobsFlow
            // 
            _jobsFlow.AutoSize = true;
            _jobsFlow.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            _jobsFlow.BackColor = Color.FromArgb(14, 16, 24);
            _jobsFlow.FlowDirection = FlowDirection.TopDown;
            _jobsFlow.Location = new Point(0, 0);
            _jobsFlow.Name = "_jobsFlow";
            _jobsFlow.Padding = new Padding(8);
            _jobsFlow.Size = new Size(16, 16);
            _jobsFlow.TabIndex = 0;
            _jobsFlow.WrapContents = false;
            // 
            // statusBar
            // 
            statusBar.BackColor = Color.FromArgb(12, 16, 26);
            statusBar.Controls.Add(_lblGwPath);
            statusBar.Controls.Add(_statusMsg);
            statusBar.Dock = DockStyle.Bottom;
            statusBar.Location = new Point(0, 837);
            statusBar.Name = "statusBar";
            statusBar.Size = new Size(1104, 24);
            statusBar.TabIndex = 5;
            // 
            // _lblGwPath
            // 
            _lblGwPath.AutoSize = true;
            _lblGwPath.BackColor = Color.Transparent;
            _lblGwPath.Font = new Font("Consolas", 7.5F);
            _lblGwPath.ForeColor = Color.FromArgb(75, 105, 145);
            _lblGwPath.Location = new Point(8, 5);
            _lblGwPath.Name = "_lblGwPath";
            _lblGwPath.Size = new Size(0, 12);
            _lblGwPath.TabIndex = 0;
            // 
            // _statusMsg
            // 
            _statusMsg.AutoSize = true;
            _statusMsg.BackColor = Color.Transparent;
            _statusMsg.Font = new Font("Consolas", 7.5F);
            _statusMsg.ForeColor = Color.FromArgb(90, 175, 90);
            _statusMsg.Location = new Point(420, 5);
            _statusMsg.Name = "_statusMsg";
            _statusMsg.Size = new Size(30, 12);
            _statusMsg.TabIndex = 1;
            _statusMsg.Text = "Ready";
            // 
            // _statusTimer
            // 
            _statusTimer.Tick += StatusTimer_Tick;
            // 
            // MainForm
            // 
            BackColor = Color.FromArgb(14, 16, 24);
            ClientSize = new Size(1104, 861);
            Controls.Add(jobsScroll);
            Controls.Add(jobsHeaderBar);
            Controls.Add(_deviceBar);
            Controls.Add(deviceHeaderBar);
            Controls.Add(_topBar);
            Controls.Add(statusBar);
            ForeColor = Color.FromArgb(180, 210, 255);
            MinimumSize = new Size(1060, 600);
            Name = "MainForm";
            Text = "GW COPY PRO - by The8BitBox - Ilija Injac";
            _topBar.ResumeLayout(false);
            _topBar.PerformLayout();
            deviceHeaderBar.ResumeLayout(false);
            deviceHeaderBar.PerformLayout();
            jobsHeaderBar.ResumeLayout(false);
            jobsHeaderBar.PerformLayout();
            jobsScroll.ResumeLayout(false);
            jobsScroll.PerformLayout();
            statusBar.ResumeLayout(false);
            statusBar.PerformLayout();
            ResumeLayout(false);
        }
        private Label lblTitle;
        private Panel deviceHeaderBar;
        private Panel jobsHeaderBar;
        private Panel jobsScroll;
        private Panel statusBar;
    }
}
