#nullable enable
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using GwCopyPro.Services;

namespace GwCopyPro.Forms
{
    partial class PostActionDialog
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

        private Label lblName = null!;
        private TextBox txtName = null!;
        private Label lblType = null!;
        private ComboBox cmbType = null!;
        private Label lblFile = null!;
        private TextBox txtExe = null!;
        private Button btnBrowse = null!;
        private Label lblArgs = null!;
        private TextBox txtArgs = null!;
        private CheckBox chkEnabled = null!;
        private Button btnOk = null!;
        private Button btnCancel = null!;

        /// <summary>Required method for Designer support - do not modify the contents of this method with the code editor.</summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblName = new Label();
            this.txtName = new TextBox();
            this.lblType = new Label();
            this.cmbType = new ComboBox();
            this.lblFile = new Label();
            this.txtExe = new TextBox();
            this.btnBrowse = new Button();
            this.lblArgs = new Label();
            this.txtArgs = new TextBox();
            this.chkEnabled = new CheckBox();
            this.btnOk = new Button();
            this.btnCancel = new Button();
            this.SuspendLayout();
            //
            // lblName
            //
            this.lblName.Text = L10n.T("pa_dlg.name");
            this.lblName.Location = new Point(10, 19);
            this.lblName.AutoSize = true;
            this.lblName.Font = new Font("Consolas", 8f);
            this.lblName.ForeColor = Color.FromArgb(130, 160, 200);
            //
            // txtName
            //
            this.txtName.Location = new Point(130, 16);
            this.txtName.Size = new Size(400, 22);
            this.txtName.BackColor = Color.FromArgb(28, 34, 48);
            this.txtName.ForeColor = Color.FromArgb(200, 230, 255);
            this.txtName.BorderStyle = BorderStyle.FixedSingle;
            this.txtName.Font = new Font("Consolas", 8.5f);
            //
            // lblType
            //
            this.lblType.Text = L10n.T("pa_dlg.type");
            this.lblType.Location = new Point(10, 53);
            this.lblType.AutoSize = true;
            this.lblType.Font = new Font("Consolas", 8f);
            this.lblType.ForeColor = Color.FromArgb(130, 160, 200);
            //
            // cmbType
            //
            this.cmbType.Location = new Point(130, 50);
            this.cmbType.Size = new Size(200, 22);
            this.cmbType.BackColor = Color.FromArgb(28, 34, 48);
            this.cmbType.ForeColor = Color.FromArgb(200, 230, 255);
            this.cmbType.FlatStyle = FlatStyle.Flat;
            this.cmbType.Font = new Font("Consolas", 8.5f);
            this.cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbType.Items.Add(L10n.T("pa_dlg.type_exe"));
            this.cmbType.Items.Add(L10n.T("pa_dlg.type_bat"));
            this.cmbType.Items.Add(L10n.T("pa_dlg.type_ps1"));
            //
            // lblFile
            //
            this.lblFile.Text = L10n.T("pa_dlg.file");
            this.lblFile.Location = new Point(10, 87);
            this.lblFile.AutoSize = true;
            this.lblFile.Font = new Font("Consolas", 8f);
            this.lblFile.ForeColor = Color.FromArgb(130, 160, 200);
            //
            // txtExe
            //
            this.txtExe.Location = new Point(130, 84);
            this.txtExe.Size = new Size(358, 22);
            this.txtExe.BackColor = Color.FromArgb(28, 34, 48);
            this.txtExe.ForeColor = Color.FromArgb(200, 230, 255);
            this.txtExe.BorderStyle = BorderStyle.FixedSingle;
            this.txtExe.Font = new Font("Consolas", 8.5f);
            //
            // btnBrowse
            //
            this.btnBrowse.Text = "...";
            this.btnBrowse.Location = new Point(496, 84);
            this.btnBrowse.Size = new Size(32, 22);
            this.btnBrowse.FlatStyle = FlatStyle.Flat;
            this.btnBrowse.BackColor = Color.FromArgb(30, 50, 80);
            this.btnBrowse.ForeColor = Color.White;
            this.btnBrowse.FlatAppearance.BorderColor = Color.FromArgb(60, 100, 160);
            this.btnBrowse.Click += new System.EventHandler(this.BtnBrowse_Click);
            //
            // lblArgs
            //
            this.lblArgs.Text = L10n.T("pa_dlg.args");
            this.lblArgs.Location = new Point(10, 121);
            this.lblArgs.AutoSize = true;
            this.lblArgs.Font = new Font("Consolas", 8f);
            this.lblArgs.ForeColor = Color.FromArgb(130, 160, 200);
            //
            // txtArgs
            //
            this.txtArgs.Location = new Point(130, 118);
            this.txtArgs.Size = new Size(400, 22);
            this.txtArgs.BackColor = Color.FromArgb(28, 34, 48);
            this.txtArgs.ForeColor = Color.FromArgb(200, 230, 255);
            this.txtArgs.BorderStyle = BorderStyle.FixedSingle;
            this.txtArgs.Font = new Font("Consolas", 8.5f);
            //
            // chkEnabled
            //
            this.chkEnabled.Text = L10n.T("pa_dlg.enabled");
            this.chkEnabled.Location = new Point(130, 152);
            this.chkEnabled.Font = new Font("Consolas", 8.5f);
            this.chkEnabled.ForeColor = Color.FromArgb(160, 200, 255);
            this.chkEnabled.AutoSize = true;
            //
            // btnOk
            //
            this.btnOk.Text = L10n.T("pa_dlg.ok");
            this.btnOk.Location = new Point(360, 192);
            this.btnOk.Size = new Size(80, 28);
            this.btnOk.FlatStyle = FlatStyle.Flat;
            this.btnOk.BackColor = Color.FromArgb(20, 60, 30);
            this.btnOk.ForeColor = Color.FromArgb(100, 220, 130);
            this.btnOk.Font = new Font("Consolas", 8.5f);
            this.btnOk.DialogResult = DialogResult.OK;
            this.btnOk.FlatAppearance.BorderColor = Color.FromArgb(50, 120, 70);
            this.btnOk.Click += new System.EventHandler(this.BtnOk_Click);
            //
            // btnCancel
            //
            this.btnCancel.Text = L10n.T("pa_dlg.cancel");
            this.btnCancel.Location = new Point(450, 192);
            this.btnCancel.Size = new Size(80, 28);
            this.btnCancel.FlatStyle = FlatStyle.Flat;
            this.btnCancel.BackColor = Color.FromArgb(50, 25, 25);
            this.btnCancel.ForeColor = Color.FromArgb(200, 100, 100);
            this.btnCancel.Font = new Font("Consolas", 8.5f);
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.FlatAppearance.BorderColor = Color.FromArgb(100, 50, 50);
            //
            // PostActionDialog
            //
            this.Text = L10n.T("pa_dlg.title");
            this.Size = new Size(560, 270);
            this.BackColor = Color.FromArgb(18, 22, 32);
            this.ForeColor = Color.FromArgb(180, 210, 255);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.AcceptButton = this.btnOk;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.cmbType);
            this.Controls.Add(this.lblFile);
            this.Controls.Add(this.txtExe);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.lblArgs);
            this.Controls.Add(this.txtArgs);
            this.Controls.Add(this.chkEnabled);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
