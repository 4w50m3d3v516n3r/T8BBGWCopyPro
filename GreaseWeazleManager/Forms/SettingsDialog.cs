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
    public class SettingsDialog : Form
    {
        private TextBox  txtGwPath   = null!;
        private ComboBox cmbLanguage = null!;
        private Label    lblSaved    = null!;

        /// <summary>
        /// Initialises the dialog and populates controls with the current <see cref="AppSettings"/> values.
        /// </summary>
        public SettingsDialog()
        {
            InitializeComponent();
            LoadValues();
        }

        /// <summary>Builds all child controls and lays them out within the fixed-size dialog.</summary>
        private void InitializeComponent()
        {
            Text            = L10n.T("settings.title");
            Size            = new Size(560, 300);
            MinimumSize     = new Size(560, 300);
            MaximumSize     = new Size(560, 300);
            BackColor       = Color.FromArgb(18, 22, 32);
            ForeColor       = Color.FromArgb(180, 210, 255);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition   = FormStartPosition.CenterParent;
            MaximizeBox     = false;

            var titleBar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 4,
                BackColor = Color.FromArgb(60, 120, 200)
            };
            Controls.Add(titleBar);

            int y = 22;

            Controls.Add(MkLbl(L10n.T("settings.gw_exe"), 18, y + 3));

            txtGwPath = new TextBox
            {
                Location    = new Point(18, y + 22),
                Size        = new Size(418, 22),
                BackColor   = Color.FromArgb(28, 34, 48),
                ForeColor   = Color.FromArgb(200, 230, 255),
                BorderStyle = BorderStyle.FixedSingle,
                Font        = new Font("Consolas", 8.5f)
            };
            Controls.Add(txtGwPath);

            var btnBrowse = MakeBtn(L10n.T("settings.browse"), 444, y + 22, 90, 22,
                Color.FromArgb(25, 45, 80), Color.FromArgb(120, 175, 255), Color.FromArgb(50, 90, 160));
            btnBrowse.Click += (s, e) =>
            {
                using var ofd = new OpenFileDialog
                {
                    Title  = L10n.T("settings.gw_exe"),
                    Filter = "gw.exe|gw.exe|All executables (*.exe)|*.exe"
                };
                if (!string.IsNullOrWhiteSpace(txtGwPath.Text) &&
                    File.Exists(Path.GetDirectoryName(txtGwPath.Text)))
                    ofd.InitialDirectory = Path.GetDirectoryName(txtGwPath.Text);

                if (ofd.ShowDialog(this) == DialogResult.OK)
                    txtGwPath.Text = ofd.FileName;
            };
            Controls.Add(btnBrowse);

            y += 68;

            Controls.Add(MkLbl(L10n.T("settings.language"), 18, y + 3));

            cmbLanguage = new ComboBox
            {
                Location      = new Point(18, y + 22),
                Size          = new Size(200, 22),
                BackColor     = Color.FromArgb(28, 34, 48),
                ForeColor     = Color.FromArgb(200, 230, 255),
                FlatStyle     = FlatStyle.Flat,
                Font          = new Font("Consolas", 8.5f),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbLanguage.Items.Add(L10n.T("settings.lang_english"));
            cmbLanguage.Items.Add(L10n.T("settings.lang_german"));
            Controls.Add(cmbLanguage);

            y += 68;

            var lblNote = new Label
            {
                Text      = L10n.T("settings.restart_note"),
                Location  = new Point(18, y),
                Size      = new Size(520, 18),
                Font      = new Font("Consolas", 7.5f),
                ForeColor = Color.FromArgb(90, 120, 160),
                BackColor = Color.Transparent
            };
            Controls.Add(lblNote);

            y += 34;

            lblSaved = new Label
            {
                Text      = "",
                Location  = new Point(18, y),
                Size      = new Size(300, 18),
                Font      = new Font("Consolas", 8f),
                ForeColor = Color.FromArgb(80, 210, 110),
                BackColor = Color.Transparent
            };
            Controls.Add(lblSaved);

            var btnSave = MakeBtn(L10n.T("settings.save"), 344, y - 2, 100, 28,
                Color.FromArgb(20, 65, 38), Color.FromArgb(90, 225, 130), Color.FromArgb(45, 130, 75));
            btnSave.Font   = new Font("Consolas", 8.5f, FontStyle.Bold);
            btnSave.Click += BtnSave_Click;
            Controls.Add(btnSave);

            var btnCancel = MakeBtn(L10n.T("settings.cancel"), 452, y - 2, 86, 28,
                Color.FromArgb(50, 25, 25), Color.FromArgb(200, 100, 100), Color.FromArgb(100, 50, 50));
            btnCancel.DialogResult = DialogResult.Cancel;
            Controls.Add(btnCancel);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        /// <summary>Populates controls from the current <see cref="AppSettings.Instance"/>.</summary>
        private void LoadValues()
        {
            var s = AppSettings.Instance;
            txtGwPath.Text         = s.GwExePath;
            cmbLanguage.SelectedIndex = s.Language == AppLanguage.German ? 1 : 0;
        }

        /// <summary>
        /// Persists the edited values to <see cref="AppSettings.Instance"/>, applies the
        /// new language via <see cref="L10n.SetLanguage"/>, and briefly shows a confirmation label.
        /// </summary>
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            var s      = AppSettings.Instance;
            s.GwExePath = txtGwPath.Text.Trim();
            s.Language  = cmbLanguage.SelectedIndex == 1 ? AppLanguage.German : AppLanguage.English;
            s.Save();

            L10n.SetLanguage(s.Language);

            lblSaved.Text = L10n.T("settings.saved");

            var t = new System.Windows.Forms.Timer { Interval = 2000 };
            t.Tick += (ts, te) => { lblSaved.Text = ""; t.Stop(); t.Dispose(); };
            t.Start();
        }

        /// <summary>Creates a styled label for use as a field caption.</summary>
        /// <param name="text">Label text.</param>
        /// <param name="x">Left position.</param>
        /// <param name="y">Top position.</param>
        /// <returns>A configured <see cref="Label"/>.</returns>
        private static Label MkLbl(string text, int x, int y) => new()
        {
            Text      = text,
            Location  = new Point(x, y),
            AutoSize  = true,
            Font      = new Font("Consolas", 8f),
            ForeColor = Color.FromArgb(130, 160, 200),
            BackColor = Color.Transparent
        };

        /// <summary>Creates a flat-styled button with the given position, size, and colours.</summary>
        /// <param name="text">Button label.</param>
        /// <param name="x">Left position.</param>
        /// <param name="y">Top position.</param>
        /// <param name="w">Width.</param>
        /// <param name="h">Height.</param>
        /// <param name="bg">Background colour.</param>
        /// <param name="fg">Foreground colour.</param>
        /// <param name="border">Border colour.</param>
        /// <returns>The configured <see cref="Button"/>.</returns>
        private static Button MakeBtn(string text, int x, int y, int w, int h,
            Color bg, Color fg, Color border)
        {
            var b = new Button
            {
                Text      = text,
                Location  = new Point(x, y),
                Size      = new Size(w, h),
                FlatStyle = FlatStyle.Flat,
                BackColor = bg,
                ForeColor = fg,
                Font      = new Font("Consolas", 8f)
            };
            b.FlatAppearance.BorderColor = border;
            return b;
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
