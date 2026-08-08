#nullable enable
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using GwCopyPro.Services;

namespace GwCopyPro.Forms
{
    partial class DeviceManagerDialog
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

        private Label lblHeading = null!;
        private Button btnAutoDetect = null!;
        private ListView lvDevices = null!;
        private Label lblPort = null!;
        private ComboBox cmbPort = null!;
        private Label lblName = null!;
        private TextBox txtName = null!;
        private Button btnAdd = null!;
        private Label lblFwProbe = null!;
        private Button btnRemove = null!;
        private Button btnRefresh = null!;
        private Button btnClose = null!;

        /// <summary>Required method for Designer support - do not modify the contents of this method with the code editor.</summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblHeading = new Label();
            this.btnAutoDetect = new Button();
            this.lvDevices = new ListView();
            this.lblPort = new Label();
            this.cmbPort = new ComboBox();
            this.lblName = new Label();
            this.txtName = new TextBox();
            this.btnAdd = new Button();
            this.lblFwProbe = new Label();
            this.btnRemove = new Button();
            this.btnRefresh = new Button();
            this.btnClose = new Button();
            this.SuspendLayout();
            //
            // lblHeading
            //
            this.lblHeading.Text = L10n.T("devmgr.heading");
            this.lblHeading.Location = new Point(10, 12);
            this.lblHeading.AutoSize = true;
            this.lblHeading.Font = new Font("Consolas", 10f, FontStyle.Bold);
            this.lblHeading.ForeColor = Color.FromArgb(160, 200, 255);
            //
            // btnAutoDetect
            //
            this.btnAutoDetect.Text = L10n.T("devmgr.auto_detect");
            this.btnAutoDetect.Location = new Point(430, 10);
            this.btnAutoDetect.Size = new Size(210, 28);
            this.btnAutoDetect.FlatStyle = FlatStyle.Flat;
            this.btnAutoDetect.BackColor = Color.FromArgb(20, 50, 90);
            this.btnAutoDetect.ForeColor = Color.FromArgb(100, 180, 255);
            this.btnAutoDetect.Font = new Font("Consolas", 8f);
            this.btnAutoDetect.FlatAppearance.BorderColor = Color.FromArgb(50, 100, 200);
            this.btnAutoDetect.Click += new System.EventHandler(this.BtnAutoDetect_Click);
            //
            // lvDevices
            //
            this.lvDevices.Location = new Point(10, 46);
            this.lvDevices.Size = new Size(624, 280);
            this.lvDevices.View = View.Details;
            this.lvDevices.FullRowSelect = true;
            this.lvDevices.BackColor = Color.FromArgb(18, 22, 32);
            this.lvDevices.ForeColor = Color.FromArgb(180, 210, 255);
            this.lvDevices.Font = new Font("Consolas", 8.5f);
            this.lvDevices.BorderStyle = BorderStyle.FixedSingle;
            this.lvDevices.Columns.Add(L10n.T("devmgr.col_name"), 150);
            this.lvDevices.Columns.Add(L10n.T("devmgr.col_port"), 70);
            this.lvDevices.Columns.Add(L10n.T("devmgr.col_fw"), 110);
            this.lvDevices.Columns.Add(L10n.T("devmgr.col_hwid"), 200);
            this.lvDevices.Columns.Add(L10n.T("devmgr.col_status"), 80);
            //
            // lblPort
            //
            this.lblPort.Text = L10n.T("devmgr.port");
            this.lblPort.Location = new Point(10, 344);
            this.lblPort.AutoSize = true;
            this.lblPort.Font = new Font("Consolas", 8f);
            this.lblPort.ForeColor = Color.FromArgb(130, 160, 200);
            //
            // cmbPort
            //
            this.cmbPort.Location = new Point(60, 340);
            this.cmbPort.Size = new Size(100, 22);
            this.cmbPort.BackColor = Color.FromArgb(28, 34, 48);
            this.cmbPort.ForeColor = Color.FromArgb(200, 230, 255);
            this.cmbPort.FlatStyle = FlatStyle.Flat;
            this.cmbPort.Font = new Font("Consolas", 8.5f);
            this.cmbPort.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbPort.SelectedIndexChanged += new System.EventHandler(this.CmbPort_SelectedIndexChanged);
            //
            // lblName
            //
            this.lblName.Text = L10n.T("devmgr.name");
            this.lblName.Location = new Point(175, 344);
            this.lblName.AutoSize = true;
            this.lblName.Font = new Font("Consolas", 8f);
            this.lblName.ForeColor = Color.FromArgb(130, 160, 200);
            //
            // txtName
            //
            this.txtName.Location = new Point(225, 340);
            this.txtName.Size = new Size(180, 22);
            this.txtName.BackColor = Color.FromArgb(28, 34, 48);
            this.txtName.ForeColor = Color.FromArgb(200, 230, 255);
            this.txtName.BorderStyle = BorderStyle.FixedSingle;
            this.txtName.Font = new Font("Consolas", 8.5f);
            this.txtName.Text = "GreaseWeazle";
            //
            // btnAdd
            //
            this.btnAdd.Text = L10n.T("devmgr.add");
            this.btnAdd.Location = new Point(415, 340);
            this.btnAdd.Size = new Size(90, 22);
            this.btnAdd.FlatStyle = FlatStyle.Flat;
            this.btnAdd.BackColor = Color.FromArgb(25, 60, 35);
            this.btnAdd.ForeColor = Color.FromArgb(100, 220, 130);
            this.btnAdd.Font = new Font("Consolas", 8f);
            this.btnAdd.FlatAppearance.BorderColor = Color.FromArgb(50, 120, 70);
            this.btnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
            //
            // lblFwProbe
            //
            this.lblFwProbe.Text = L10n.T("devmgr.probe_hint");
            this.lblFwProbe.Location = new Point(10, 370);
            this.lblFwProbe.Size = new Size(624, 18);
            this.lblFwProbe.Font = new Font("Consolas", 8f);
            this.lblFwProbe.ForeColor = Color.FromArgb(100, 160, 220);
            this.lblFwProbe.BackColor = Color.Transparent;
            //
            // btnRemove
            //
            this.btnRemove.Text = L10n.T("devmgr.remove");
            this.btnRemove.Location = new Point(10, 402);
            this.btnRemove.Size = new Size(150, 26);
            this.btnRemove.FlatStyle = FlatStyle.Flat;
            this.btnRemove.BackColor = Color.FromArgb(60, 20, 20);
            this.btnRemove.ForeColor = Color.FromArgb(220, 80, 80);
            this.btnRemove.Font = new Font("Consolas", 8f);
            this.btnRemove.FlatAppearance.BorderColor = Color.FromArgb(100, 40, 40);
            this.btnRemove.Click += new System.EventHandler(this.BtnRemove_Click);
            //
            // btnRefresh
            //
            this.btnRefresh.Text = L10n.T("devmgr.refresh");
            this.btnRefresh.Location = new Point(170, 402);
            this.btnRefresh.Size = new Size(150, 26);
            this.btnRefresh.FlatStyle = FlatStyle.Flat;
            this.btnRefresh.BackColor = Color.FromArgb(25, 40, 70);
            this.btnRefresh.ForeColor = Color.FromArgb(100, 160, 240);
            this.btnRefresh.Font = new Font("Consolas", 8f);
            this.btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(50, 80, 140);
            this.btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            //
            // btnClose
            //
            this.btnClose.Text = L10n.T("devmgr.close");
            this.btnClose.Location = new Point(520, 402);
            this.btnClose.Size = new Size(114, 26);
            this.btnClose.FlatStyle = FlatStyle.Flat;
            this.btnClose.BackColor = Color.FromArgb(30, 40, 60);
            this.btnClose.ForeColor = Color.White;
            this.btnClose.Font = new Font("Consolas", 8f);
            this.btnClose.FlatAppearance.BorderColor = Color.FromArgb(60, 80, 120);
            this.btnClose.DialogResult = DialogResult.OK;
            //
            // DeviceManagerDialog
            //
            this.Text = L10n.T("devmgr.title");
            this.Size = new Size(660, 520);
            this.BackColor = Color.FromArgb(18, 22, 32);
            this.ForeColor = Color.FromArgb(180, 210, 255);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.lblFwProbe);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.cmbPort);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.lvDevices);
            this.Controls.Add(this.btnAutoDetect);
            this.Controls.Add(this.lblHeading);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
