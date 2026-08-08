using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GwCopyPro.Services;

namespace GwCopyPro.Forms
{
    /// <summary>
    /// Modal dialog for editing application settings: the path to <c>gw.exe</c>
    /// and the active UI language. Changes are applied to <see cref="AppSettings.Instance"/>
    /// and persisted immediately on Save.
    /// </summary>
    public partial class SettingsDialog : Form
    {
        /// <summary>
        /// Initialises the dialog and populates controls with the current <see cref="AppSettings"/> values.
        /// </summary>
        public SettingsDialog()
        {
            InitializeComponent();
            LoadValues();
        }

        /// <summary>Populates controls from the current <see cref="AppSettings.Instance"/>.</summary>
        private void LoadValues()
        {
            var s = AppSettings.Instance;
            txtGwPath.Text = s.GwExePath;
            cmbLanguage.SelectedIndex = s.Language == AppLanguage.German ? 1 : 0;
        }

        /// <summary>Opens a file picker for <c>gw.exe</c>, seeded with the current path's directory.</summary>
        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = L10n.T("settings.gw_exe"),
                Filter = "gw.exe|gw.exe|All executables (*.exe)|*.exe"
            };
            if (!string.IsNullOrWhiteSpace(txtGwPath.Text) &&
                File.Exists(Path.GetDirectoryName(txtGwPath.Text)))
                ofd.InitialDirectory = Path.GetDirectoryName(txtGwPath.Text);

            if (ofd.ShowDialog(this) == DialogResult.OK)
                txtGwPath.Text = ofd.FileName;
        }

        /// <summary>
        /// Persists the edited values to <see cref="AppSettings.Instance"/>, applies the
        /// new language via <see cref="L10n.SetLanguage"/>, and briefly shows a confirmation label.
        /// </summary>
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            var s = AppSettings.Instance;
            s.GwExePath = txtGwPath.Text.Trim();
            s.Language = cmbLanguage.SelectedIndex == 1 ? AppLanguage.German : AppLanguage.English;
            s.Save();

            L10n.SetLanguage(s.Language);

            lblSaved.Text = L10n.T("settings.saved");
            btnCancel.Text = L10n.T("settings.ok");

            var t = new System.Windows.Forms.Timer { Interval = 2000 };
            t.Tick += (ts, te) => { lblSaved.Text = ""; t.Stop(); t.Dispose(); };
            t.Start();
        }

        /// <summary>Paints a thin border around the dialog.</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using var pen = new Pen(Color.FromArgb(35, 55, 85), 1f);
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }
    }
}
