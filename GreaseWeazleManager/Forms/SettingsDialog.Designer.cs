#nullable enable
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using GwCopyPro.Services;

namespace GwCopyPro.Forms
{
    partial class SettingsDialog
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

        private Panel titleBar = null!;
        private Label lblGwPath = null!;
        private TextBox txtGwPath = null!;
        private Button btnBrowse = null!;
        private Label lblLanguage = null!;
        private ComboBox cmbLanguage = null!;
        private Label lblNote = null!;
        private Label lblSaved = null!;
        private Button btnSave = null!;
        private Button btnCancel = null!;

        /// <summary>Required method for Designer support - do not modify the contents of this method with the code editor.</summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.titleBar = new Panel();
            this.lblGwPath = new Label();
            this.txtGwPath = new TextBox();
            this.btnBrowse = new Button();
            this.lblLanguage = new Label();
            this.cmbLanguage = new ComboBox();
            this.lblNote = new Label();
            this.lblSaved = new Label();
            this.btnSave = new Button();
            this.btnCancel = new Button();
            this.SuspendLayout();
            //
            // titleBar
            //
            this.titleBar.Dock = DockStyle.Top;
            this.titleBar.Height = 4;
            this.titleBar.BackColor = Color.FromArgb(60, 120, 200);
            //
            // lblGwPath
            //
            this.lblGwPath.AutoSize = true;
            this.lblGwPath.Location = new Point(18, 25);
            this.lblGwPath.Font = new Font("Consolas", 8f);
            this.lblGwPath.ForeColor = Color.FromArgb(130, 160, 200);
            this.lblGwPath.BackColor = Color.Transparent;
            this.lblGwPath.Text = L10n.T("settings.gw_exe");
            //
            // txtGwPath
            //
            this.txtGwPath.Location = new Point(18, 44);
            this.txtGwPath.Size = new Size(418, 22);
            this.txtGwPath.BackColor = Color.FromArgb(28, 34, 48);
            this.txtGwPath.ForeColor = Color.FromArgb(200, 230, 255);
            this.txtGwPath.BorderStyle = BorderStyle.FixedSingle;
            this.txtGwPath.Font = new Font("Consolas", 8.5f);
            //
            // btnBrowse
            //
            this.btnBrowse.Text = L10n.T("settings.browse");
            this.btnBrowse.Location = new Point(444, 44);
            this.btnBrowse.Size = new Size(90, 22);
            this.btnBrowse.FlatStyle = FlatStyle.Flat;
            this.btnBrowse.BackColor = Color.FromArgb(25, 45, 80);
            this.btnBrowse.ForeColor = Color.FromArgb(120, 175, 255);
            this.btnBrowse.Font = new Font("Consolas", 8f);
            this.btnBrowse.FlatAppearance.BorderColor = Color.FromArgb(50, 90, 160);
            this.btnBrowse.Click += new System.EventHandler(this.BtnBrowse_Click);
            //
            // lblLanguage
            //
            this.lblLanguage.AutoSize = true;
            this.lblLanguage.Location = new Point(18, 93);
            this.lblLanguage.Font = new Font("Consolas", 8f);
            this.lblLanguage.ForeColor = Color.FromArgb(130, 160, 200);
            this.lblLanguage.BackColor = Color.Transparent;
            this.lblLanguage.Text = L10n.T("settings.language");
            //
            // cmbLanguage
            //
            this.cmbLanguage.Location = new Point(18, 112);
            this.cmbLanguage.Size = new Size(200, 22);
            this.cmbLanguage.BackColor = Color.FromArgb(28, 34, 48);
            this.cmbLanguage.ForeColor = Color.FromArgb(200, 230, 255);
            this.cmbLanguage.FlatStyle = FlatStyle.Flat;
            this.cmbLanguage.Font = new Font("Consolas", 8.5f);
            this.cmbLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbLanguage.Items.Add(L10n.T("settings.lang_english"));
            this.cmbLanguage.Items.Add(L10n.T("settings.lang_german"));
            //
            // lblNote
            //
            this.lblNote.Text = L10n.T("settings.restart_note");
            this.lblNote.Location = new Point(18, 158);
            this.lblNote.Size = new Size(520, 18);
            this.lblNote.Font = new Font("Consolas", 7.5f);
            this.lblNote.ForeColor = Color.FromArgb(90, 120, 160);
            this.lblNote.BackColor = Color.Transparent;
            //
            // lblSaved
            //
            this.lblSaved.Text = "";
            this.lblSaved.Location = new Point(18, 192);
            this.lblSaved.Size = new Size(300, 18);
            this.lblSaved.Font = new Font("Consolas", 8f);
            this.lblSaved.ForeColor = Color.FromArgb(80, 210, 110);
            this.lblSaved.BackColor = Color.Transparent;
            //
            // btnSave
            //
            this.btnSave.Text = L10n.T("settings.save");
            this.btnSave.Location = new Point(344, 190);
            this.btnSave.Size = new Size(100, 28);
            this.btnSave.FlatStyle = FlatStyle.Flat;
            this.btnSave.BackColor = Color.FromArgb(20, 65, 38);
            this.btnSave.ForeColor = Color.FromArgb(90, 225, 130);
            this.btnSave.Font = new Font("Consolas", 8.5f, FontStyle.Bold);
            this.btnSave.FlatAppearance.BorderColor = Color.FromArgb(45, 130, 75);
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            //
            // btnCancel
            //
            this.btnCancel.Text = L10n.T("settings.cancel");
            this.btnCancel.Location = new Point(452, 190);
            this.btnCancel.Size = new Size(86, 28);
            this.btnCancel.FlatStyle = FlatStyle.Flat;
            this.btnCancel.BackColor = Color.FromArgb(50, 25, 25);
            this.btnCancel.ForeColor = Color.FromArgb(200, 100, 100);
            this.btnCancel.Font = new Font("Consolas", 8f);
            this.btnCancel.FlatAppearance.BorderColor = Color.FromArgb(100, 50, 50);
            this.btnCancel.DialogResult = DialogResult.Cancel;
            //
            // SettingsDialog
            //
            this.Text = L10n.T("settings.title");
            this.Size = new Size(560, 300);
            this.MinimumSize = new Size(560, 300);
            this.MaximumSize = new Size(560, 300);
            this.BackColor = Color.FromArgb(18, 22, 32);
            this.ForeColor = Color.FromArgb(180, 210, 255);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.AcceptButton = this.btnSave;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.lblSaved);
            this.Controls.Add(this.lblNote);
            this.Controls.Add(this.cmbLanguage);
            this.Controls.Add(this.lblLanguage);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtGwPath);
            this.Controls.Add(this.lblGwPath);
            this.Controls.Add(this.titleBar);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
