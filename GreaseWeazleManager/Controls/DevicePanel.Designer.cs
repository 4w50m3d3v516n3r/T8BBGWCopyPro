#nullable enable
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using GwCopyPro.Services;

namespace GwCopyPro.Controls
{
    partial class DevicePanel
    {
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

        private Label _lblName = null!;
        private Label _lblPort = null!;
        private Label _lblFw = null!;
        private Label _lblConn = null!;
        private PictureBox _statusLight = null!;
        private Button _btnNewJob = null!;
        private Button _btnRemove = null!;
        private Button _btnBlink = null!;
        private System.Windows.Forms.Timer _pulseTimer = null!;

        /// <summary>Required method for Designer support - do not modify the contents of this method with the code editor.</summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._statusLight = new PictureBox();
            this._lblName = new Label();
            this._lblPort = new Label();
            this._lblFw = new Label();
            this._lblConn = new Label();
            this._btnNewJob = new Button();
            this._btnRemove = new Button();
            this._btnBlink = new Button();
            this._pulseTimer = new System.Windows.Forms.Timer(this.components) { Interval = 50 };
            this.SuspendLayout();
            //
            // _statusLight
            //
            this._statusLight.Location = new Point(182, 10);
            this._statusLight.Size = new Size(16, 16);
            this._statusLight.BackColor = Color.Transparent;
            this._statusLight.Paint += new PaintEventHandler(this.StatusLight_Paint);
            //
            // _lblName
            //
            this._lblName.Font = new Font("Consolas", 8.5f, FontStyle.Bold);
            this._lblName.ForeColor = Color.FromArgb(160, 200, 255);
            this._lblName.AutoSize = false;
            this._lblName.Size = new Size(172, 18);
            this._lblName.Location = new Point(10, 12);
            this._lblName.BackColor = Color.Transparent;
            //
            // _lblPort
            //
            this._lblPort.Font = new Font("Consolas", 8f);
            this._lblPort.ForeColor = Color.FromArgb(120, 150, 190);
            this._lblPort.AutoSize = false;
            this._lblPort.Size = new Size(190, 15);
            this._lblPort.Location = new Point(10, 32);
            this._lblPort.BackColor = Color.Transparent;
            //
            // _lblFw
            //
            this._lblFw.Font = new Font("Consolas", 7.5f);
            this._lblFw.ForeColor = Color.FromArgb(90, 120, 150);
            this._lblFw.AutoSize = false;
            this._lblFw.Size = new Size(190, 15);
            this._lblFw.Location = new Point(10, 49);
            this._lblFw.BackColor = Color.Transparent;
            //
            // _lblConn
            //
            this._lblConn.Font = new Font("Consolas", 7.5f);
            this._lblConn.AutoSize = false;
            this._lblConn.Size = new Size(190, 14);
            this._lblConn.Location = new Point(10, 66);
            this._lblConn.BackColor = Color.Transparent;
            //
            // _btnNewJob
            //
            this._btnNewJob.Text = L10n.T("dev.new_job");
            this._btnNewJob.Location = new Point(10, 84);
            this._btnNewJob.Size = new Size(190, 22);
            this._btnNewJob.FlatStyle = FlatStyle.Flat;
            this._btnNewJob.BackColor = Color.FromArgb(18, 60, 32);
            this._btnNewJob.ForeColor = Color.FromArgb(90, 220, 120);
            this._btnNewJob.Font = new Font("Consolas", 8f, FontStyle.Bold);
            this._btnNewJob.FlatAppearance.BorderColor = Color.FromArgb(40, 120, 65);
            this._btnNewJob.Click += new System.EventHandler(this.BtnNewJob_Click);
            //
            // _btnRemove
            //
            this._btnRemove.Text = L10n.T("dev.remove");
            this._btnRemove.Location = new Point(10, 110);
            this._btnRemove.Size = new Size(60, 18);
            this._btnRemove.FlatStyle = FlatStyle.Flat;
            this._btnRemove.BackColor = Color.FromArgb(60, 20, 20);
            this._btnRemove.ForeColor = Color.FromArgb(200, 80, 80);
            this._btnRemove.Font = new Font("Consolas", 8f);
            this._btnRemove.FlatAppearance.BorderColor = Color.FromArgb(100, 40, 40);
            this._btnRemove.Click += new System.EventHandler(this.BtnRemove_Click);
            //
            // _btnBlink
            //
            this._btnBlink.Text = L10n.T("dev.blink");
            this._btnBlink.Location = new Point(76, 110);
            this._btnBlink.Size = new Size(124, 18);
            this._btnBlink.FlatStyle = FlatStyle.Flat;
            this._btnBlink.BackColor = Color.FromArgb(40, 35, 20);
            this._btnBlink.ForeColor = Color.FromArgb(220, 180, 80);
            this._btnBlink.Font = new Font("Consolas", 7.5f);
            this._btnBlink.FlatAppearance.BorderColor = Color.FromArgb(120, 100, 40);
            this._btnBlink.Click += new System.EventHandler(this.BtnBlink_Click);
            //
            // _pulseTimer
            //
            this._pulseTimer.Tick += new System.EventHandler(this.PulseTimer_Tick);
            //
            // DevicePanel
            //
            this.AutoScaleMode = AutoScaleMode.None;
            this.BackColor = Color.FromArgb(20, 24, 34);
            this.Size = new Size(210, 136);
            this.Margin = new Padding(6);
            this.TabStop = false;
            this.Controls.Add(this._statusLight);
            this.Controls.Add(this._lblName);
            this.Controls.Add(this._lblPort);
            this.Controls.Add(this._lblFw);
            this.Controls.Add(this._lblConn);
            this.Controls.Add(this._btnNewJob);
            this.Controls.Add(this._btnRemove);
            this.Controls.Add(this._btnBlink);
            this.ResumeLayout(false);
        }
    }
}
