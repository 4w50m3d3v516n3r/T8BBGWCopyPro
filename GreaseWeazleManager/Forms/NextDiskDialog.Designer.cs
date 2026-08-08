#nullable enable
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using GwCopyPro.Services;

namespace GwCopyPro.Forms
{
    partial class NextDiskDialog
    {
        /// <summary>Required designer variable.</summary>
        private IContainer? components = null;

        private Panel accent = null!;
        private Label lblDone = null!;
        private Label lblDevice = null!;
        private Label lblDoneFile = null!;
        private Label lblDuration = null!;
        private Label sepTop = null!;
        private Label lblNextLabel = null!;
        private Label lblNextFile = null!;
        private Label lblDtPreview = null!;
        private Label _lblWaiting = null!;
        private Label sepBottom = null!;
        private Button btnGo = null!;
        private Button btnStop = null!;

        /// <summary>Required method for Designer support - do not modify the contents of this method with the code editor.</summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.accent = new Panel();
            this.lblDone = new Label();
            this.lblDevice = new Label();
            this.lblDoneFile = new Label();
            this.lblDuration = new Label();
            this.sepTop = new Label();
            this.lblNextLabel = new Label();
            this.lblNextFile = new Label();
            this.lblDtPreview = new Label();
            this._lblWaiting = new Label();
            this.sepBottom = new Label();
            this.btnGo = new Button();
            this.btnStop = new Button();
            this.SuspendLayout();
            //
            // accent
            //
            this.accent.Dock = DockStyle.Top;
            this.accent.Height = 4;
            this.accent.BackColor = Color.FromArgb(40, 160, 80);
            //
            // lblDone
            //
            this.lblDone.Location = new Point(14, 18);
            this.lblDone.Size = new Size(520, 20);
            this.lblDone.Font = new Font("Consolas", 10f, FontStyle.Bold);
            this.lblDone.ForeColor = Color.FromArgb(80, 215, 110);
            this.lblDone.BackColor = Color.Transparent;
            this.lblDone.AutoSize = false;
            //
            // lblDevice
            //
            this.lblDevice.Location = new Point(14, 46);
            this.lblDevice.Size = new Size(520, 16);
            this.lblDevice.Font = new Font("Consolas", 8.5f, FontStyle.Bold);
            this.lblDevice.ForeColor = Color.FromArgb(120, 190, 255);
            this.lblDevice.BackColor = Color.Transparent;
            this.lblDevice.AutoSize = false;
            //
            // lblDoneFile
            //
            this.lblDoneFile.Location = new Point(14, 68);
            this.lblDoneFile.Size = new Size(520, 16);
            this.lblDoneFile.Font = new Font("Consolas", 8f);
            this.lblDoneFile.ForeColor = Color.FromArgb(100, 140, 180);
            this.lblDoneFile.BackColor = Color.Transparent;
            this.lblDoneFile.AutoSize = false;
            //
            // lblDuration
            //
            this.lblDuration.Location = new Point(14, 90);
            this.lblDuration.Size = new Size(520, 16);
            this.lblDuration.Font = new Font("Consolas", 8f);
            this.lblDuration.ForeColor = Color.FromArgb(100, 140, 180);
            this.lblDuration.BackColor = Color.Transparent;
            this.lblDuration.AutoSize = false;
            //
            // sepTop
            //
            this.sepTop.Location = new Point(14, 120);
            this.sepTop.Size = new Size(520, 1);
            this.sepTop.BackColor = Color.FromArgb(40, 60, 90);
            //
            // lblNextLabel
            //
            this.lblNextLabel.Location = new Point(14, 134);
            this.lblNextLabel.Size = new Size(520, 16);
            this.lblNextLabel.Font = new Font("Consolas", 8.5f, FontStyle.Bold);
            this.lblNextLabel.ForeColor = Color.FromArgb(160, 200, 255);
            this.lblNextLabel.BackColor = Color.Transparent;
            this.lblNextLabel.AutoSize = false;
            this.lblNextLabel.Text = L10n.T("nextdisk.next_label");
            //
            // lblNextFile
            //
            this.lblNextFile.Location = new Point(14, 156);
            this.lblNextFile.Size = new Size(520, 20);
            this.lblNextFile.Font = new Font("Consolas", 9f);
            this.lblNextFile.ForeColor = Color.FromArgb(220, 200, 100);
            this.lblNextFile.BackColor = Color.Transparent;
            this.lblNextFile.AutoSize = false;
            //
            // lblDtPreview
            //
            this.lblDtPreview.Location = new Point(14, 184);
            this.lblDtPreview.Size = new Size(520, 16);
            this.lblDtPreview.Font = new Font("Consolas", 7.5f);
            this.lblDtPreview.ForeColor = Color.FromArgb(90, 120, 160);
            this.lblDtPreview.BackColor = Color.Transparent;
            this.lblDtPreview.AutoSize = false;
            //
            // _lblWaiting
            //
            this._lblWaiting.Text = L10n.T("nextdisk.waiting");
            this._lblWaiting.Location = new Point(14, 214);
            this._lblWaiting.Size = new Size(520, 22);
            this._lblWaiting.Font = new Font("Consolas", 9f, FontStyle.Italic);
            this._lblWaiting.ForeColor = Color.FromArgb(80, 160, 220);
            this._lblWaiting.BackColor = Color.Transparent;
            this._lblWaiting.AutoSize = false;
            //
            // sepBottom
            //
            this.sepBottom.Location = new Point(14, 250);
            this.sepBottom.Size = new Size(520, 1);
            this.sepBottom.BackColor = Color.FromArgb(40, 60, 90);
            //
            // btnGo
            //
            this.btnGo.Text = L10n.T("nextdisk.btn_go");
            this.btnGo.Location = new Point(14, 264);
            this.btnGo.Size = new Size(300, 40);
            this.btnGo.FlatStyle = FlatStyle.Flat;
            this.btnGo.BackColor = Color.FromArgb(18, 65, 32);
            this.btnGo.ForeColor = Color.FromArgb(80, 230, 120);
            this.btnGo.Font = new Font("Consolas", 11f, FontStyle.Bold);
            this.btnGo.FlatAppearance.BorderColor = Color.FromArgb(45, 140, 75);
            this.btnGo.Click += new System.EventHandler(this.BtnGo_Click);
            //
            // btnStop
            //
            this.btnStop.Text = L10n.T("nextdisk.btn_stop");
            this.btnStop.Location = new Point(326, 264);
            this.btnStop.Size = new Size(208, 40);
            this.btnStop.FlatStyle = FlatStyle.Flat;
            this.btnStop.BackColor = Color.FromArgb(55, 20, 20);
            this.btnStop.ForeColor = Color.FromArgb(220, 90, 90);
            this.btnStop.Font = new Font("Consolas", 9.5f);
            this.btnStop.FlatAppearance.BorderColor = Color.FromArgb(120, 45, 45);
            this.btnStop.Click += new System.EventHandler(this.BtnStop_Click);
            //
            // NextDiskDialog
            //
            this.Text = L10n.T("nextdisk.title");
            this.Size = new Size(560, 402);
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
            this.BackColor = Color.FromArgb(18, 22, 32);
            this.ForeColor = Color.FromArgb(180, 210, 255);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.AcceptButton = this.btnGo;
            this.CancelButton = this.btnStop;
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnGo);
            this.Controls.Add(this.sepBottom);
            this.Controls.Add(this._lblWaiting);
            this.Controls.Add(this.lblDtPreview);
            this.Controls.Add(this.lblNextFile);
            this.Controls.Add(this.lblNextLabel);
            this.Controls.Add(this.sepTop);
            this.Controls.Add(this.lblDuration);
            this.Controls.Add(this.lblDoneFile);
            this.Controls.Add(this.lblDevice);
            this.Controls.Add(this.lblDone);
            this.Controls.Add(this.accent);
            this.ResumeLayout(false);
        }
    }
}
