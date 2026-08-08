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
            components = new Container();
            _statusLight = new PictureBox();
            _lblName = new Label();
            _lblPort = new Label();
            _lblFw = new Label();
            _lblConn = new Label();
            _btnNewJob = new Button();
            _btnRemove = new Button();
            _btnBlink = new Button();
            _pulseTimer = new System.Windows.Forms.Timer(components);
            ((ISupportInitialize)_statusLight).BeginInit();
            SuspendLayout();
            // 
            // _statusLight
            // 
            _statusLight.BackColor = Color.Transparent;
            _statusLight.Location = new Point(182, 10);
            _statusLight.Name = "_statusLight";
            _statusLight.Size = new Size(16, 16);
            _statusLight.TabIndex = 0;
            _statusLight.TabStop = false;
            _statusLight.Paint += StatusLight_Paint;
            // 
            // _lblName
            // 
            _lblName.BackColor = Color.Transparent;
            _lblName.Font = new Font("Consolas", 8.5F, FontStyle.Bold);
            _lblName.ForeColor = Color.FromArgb(160, 200, 255);
            _lblName.Location = new Point(10, 12);
            _lblName.Name = "_lblName";
            _lblName.Size = new Size(172, 18);
            _lblName.TabIndex = 1;
            // 
            // _lblPort
            // 
            _lblPort.BackColor = Color.Transparent;
            _lblPort.Font = new Font("Consolas", 8F);
            _lblPort.ForeColor = Color.FromArgb(120, 150, 190);
            _lblPort.Location = new Point(10, 32);
            _lblPort.Name = "_lblPort";
            _lblPort.Size = new Size(190, 15);
            _lblPort.TabIndex = 2;
            // 
            // _lblFw
            // 
            _lblFw.BackColor = Color.Transparent;
            _lblFw.Font = new Font("Consolas", 7.5F);
            _lblFw.ForeColor = Color.FromArgb(90, 120, 150);
            _lblFw.Location = new Point(10, 49);
            _lblFw.Name = "_lblFw";
            _lblFw.Size = new Size(190, 15);
            _lblFw.TabIndex = 3;
            // 
            // _lblConn
            // 
            _lblConn.BackColor = Color.Transparent;
            _lblConn.Font = new Font("Consolas", 7.5F);
            _lblConn.Location = new Point(10, 66);
            _lblConn.Name = "_lblConn";
            _lblConn.Size = new Size(190, 14);
            _lblConn.TabIndex = 4;
            // 
            // _btnNewJob
            // 
            _btnNewJob.BackColor = Color.FromArgb(18, 60, 32);
            _btnNewJob.FlatAppearance.BorderColor = Color.FromArgb(40, 120, 65);
            _btnNewJob.FlatStyle = FlatStyle.Flat;
            _btnNewJob.Font = new Font("Consolas", 8F, FontStyle.Bold);
            _btnNewJob.ForeColor = Color.FromArgb(90, 220, 120);
            _btnNewJob.Location = new Point(10, 84);
            _btnNewJob.Name = "_btnNewJob";
            _btnNewJob.Size = new Size(190, 22);
            _btnNewJob.TabIndex = 5;
            _btnNewJob.UseVisualStyleBackColor = false;
            _btnNewJob.Click += BtnNewJob_Click;
            // 
            // _btnRemove
            // 
            _btnRemove.BackColor = Color.FromArgb(60, 20, 20);
            _btnRemove.FlatAppearance.BorderColor = Color.FromArgb(100, 40, 40);
            _btnRemove.FlatStyle = FlatStyle.Flat;
            _btnRemove.Font = new Font("Consolas", 8F);
            _btnRemove.ForeColor = Color.FromArgb(200, 80, 80);
            _btnRemove.Location = new Point(10, 110);
            _btnRemove.Name = "_btnRemove";
            _btnRemove.Size = new Size(60, 18);
            _btnRemove.TabIndex = 6;
            _btnRemove.UseVisualStyleBackColor = false;
            _btnRemove.Click += BtnRemove_Click;
            // 
            // _btnBlink
            // 
            _btnBlink.BackColor = Color.FromArgb(40, 35, 20);
            _btnBlink.FlatAppearance.BorderColor = Color.FromArgb(120, 100, 40);
            _btnBlink.FlatStyle = FlatStyle.Flat;
            _btnBlink.Font = new Font("Consolas", 6.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            _btnBlink.ForeColor = Color.FromArgb(220, 180, 80);
            _btnBlink.Location = new Point(76, 110);
            _btnBlink.Name = "_btnBlink";
            _btnBlink.Size = new Size(124, 18);
            _btnBlink.TabIndex = 7;
            _btnBlink.UseVisualStyleBackColor = false;
            _btnBlink.Click += BtnBlink_Click;
            // 
            // _pulseTimer
            // 
            _pulseTimer.Tick += PulseTimer_Tick;
            // 
            // DevicePanel
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.FromArgb(20, 24, 34);
            Controls.Add(_statusLight);
            Controls.Add(_lblName);
            Controls.Add(_lblPort);
            Controls.Add(_lblFw);
            Controls.Add(_lblConn);
            Controls.Add(_btnNewJob);
            Controls.Add(_btnRemove);
            Controls.Add(_btnBlink);
            Margin = new Padding(6);
            Name = "DevicePanel";
            Size = new Size(210, 136);
            ((ISupportInitialize)_statusLight).EndInit();
            ResumeLayout(false);
        }
    }
}
