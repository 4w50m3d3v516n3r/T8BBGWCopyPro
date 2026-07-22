using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GwCopyPro.Models;
using GwCopyPro.Services;

namespace GwCopyPro.Forms
{
    /// <summary>
    /// Modal dialog for creating a new <see cref="GwJob"/>. Presents five tabs:
    /// Main (device, job type, image file, read/write options), Tracks (cylinder and
    /// head selection), Advanced (drive, extra args), Post-Actions, and Repeat
    /// (repetitive-mode file-pattern settings). Provides a live command-line preview
    /// and supports saving and loading <see cref="JobPreset"/> files.
    /// </summary>
    public class NewJobDialog : Form
    {
        private readonly List<GreaseWeazleDevice> _devices;
        private readonly GreaseWeazleDevice?      _preselectedDevice;

        /// <summary>Gets the <see cref="GwJob"/> created when the user clicks Start Job, or <see langword="null"/> if cancelled.</summary>
        public GwJob? Result { get; private set; }

        private ComboBox      cmbDevice        = null!;
        private ComboBox      cmbJobType       = null!;
        private TextBox       txtImageFile      = null!;
        private TextBox       txtFormat         = null!;
        private NumericUpDown nudStartCyl       = null!;
        private NumericUpDown nudEndCyl         = null!;
        private ComboBox      cmbHead           = null!;
        private NumericUpDown nudStep           = null!;
        private CheckBox      chkHSwap          = null!;
        private CheckBox      chkHead0Off       = null!;
        private NumericUpDown nudHead0Off       = null!;
        private CheckBox      chkHead1Off       = null!;
        private NumericUpDown nudHead1Off       = null!;
        private NumericUpDown nudRevs           = null!;
        private ComboBox      cmbDensel         = null!;
        private NumericUpDown nudBitrate        = null!;
        private CheckBox      chkRetries        = null!;
        private NumericUpDown nudRetries        = null!;
        private CheckBox      chkNoClobber      = null!;
        private CheckBox      chkRaw            = null!;
        private CheckBox      chkReverse        = null!;
        private CheckBox      chkHardSectors    = null!;
        private CheckBox      chkErase          = null!;
        private CheckBox      chkVerify         = null!;
        private TextBox       txtPrecomp        = null!;
        private CheckBox      chkGenTg43        = null!;
        private CheckBox      chkReverseW       = null!;
        private CheckBox      chkHardSectorsW   = null!;
        private ComboBox      cmbDrive          = null!;
        private TextBox       txtExtraArgs      = null!;
        private Label         lblPreview        = null!;
        private Label         lblTrackSpec      = null!;
        private ListView      lvPostActions     = null!;
        private CheckBox      chkRepetitive     = null!;
        private TextBox       txtFilePattern    = null!;
        private TextBox       txtOutputFolder   = null!;
        private NumericUpDown nudStartIndex     = null!;
        private TextBox       txtDtFormat       = null!;
        private Label         lblPatternPreview = null!;
        private TextBox       txtPresetName     = null!;

        private bool _initialized;

        /// <summary>
        /// Initialises the dialog, builds all controls, and optionally pre-selects a device.
        /// </summary>
        /// <param name="devices">List of registered <see cref="GreaseWeazleDevice"/> instances to populate the device combo.</param>
        /// <param name="preselectedDevice">Device to select by default, or <see langword="null"/> to auto-select.</param>
        public NewJobDialog(List<GreaseWeazleDevice> devices, GreaseWeazleDevice? preselectedDevice = null)
        {
            _devices           = devices;
            _preselectedDevice = preselectedDevice;
            InitializeComponent();
            PopulateDevices();
            _initialized = true;
            UpdatePreview();
            UpdateTrackSpecLabel();
        }

        /// <summary>Builds the tab control, all five tab pages, the preview bar, and the action buttons.</summary>
        private void InitializeComponent()
        {
            const int FORM_W     = 900;
            const int FORM_H     = 820;
            const int PAD        = 10;
            const int TAB_H      = 630;
            const int TAB_ITEM_H = 32;
            const int TAB_ITEM_W = 190;
            const int PREV_Y     = PAD + TAB_H + 8;
            const int BTN_Y      = PREV_Y + 24 + 12;
            const int BTN_H      = 36;
            const int BTN_W_OK   = 160;
            const int BTN_W_CAN  = 100;

            Text            = L10n.T("job_dlg.title");
            Size            = new Size(FORM_W, FORM_H);
            MinimumSize     = new Size(FORM_W, FORM_H);
            MaximumSize     = new Size(FORM_W, FORM_H);
            BackColor       = Color.FromArgb(18, 22, 32);
            ForeColor       = Color.FromArgb(180, 210, 255);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;

            var tabs = new TabControl
            {
                Location = new Point(PAD, PAD),
                Size     = new Size(FORM_W - PAD * 2, TAB_H),
                DrawMode = TabDrawMode.OwnerDrawFixed,
                ItemSize = new Size(TAB_ITEM_W, TAB_ITEM_H),
                SizeMode = TabSizeMode.Fixed
            };
            tabs.DrawItem += Tabs_DrawItem;

            var tabMain        = new TabPage(L10n.T("job_dlg.tab_main"))        { BackColor = Color.FromArgb(22, 26, 36) };
            var tabTracks      = new TabPage(L10n.T("job_dlg.tab_tracks"))      { BackColor = Color.FromArgb(22, 26, 36) };
            var tabAdvanced    = new TabPage(L10n.T("job_dlg.tab_advanced"))    { BackColor = Color.FromArgb(22, 26, 36) };
            var tabPostActions = new TabPage(L10n.T("job_dlg.tab_postactions")) { BackColor = Color.FromArgb(22, 26, 36) };
            var tabRepeat      = new TabPage(L10n.T("job_dlg.tab_repeat"))      { BackColor = Color.FromArgb(22, 26, 36) };

            BuildMainTab(tabMain);
            BuildTracksTab(tabTracks);
            BuildAdvancedTab(tabAdvanced);
            BuildPostActionsTab(tabPostActions);
            BuildRepeatTab(tabRepeat);

            tabs.TabPages.AddRange(new[] { tabMain, tabTracks, tabAdvanced, tabPostActions, tabRepeat });

            lblPreview = new Label
            {
                Location  = new Point(PAD, PREV_Y),
                Size      = new Size(FORM_W - PAD * 2, 22),
                Font      = new Font("Consolas", 7.5f),
                ForeColor = Color.FromArgb(80, 180, 80),
                BackColor = Color.FromArgb(12, 16, 22),
                AutoSize  = false,
                Padding   = new Padding(4, 3, 0, 0)
            };

            var sepLine = new Label
            {
                Location  = new Point(PAD, BTN_Y - 8),
                Size      = new Size(FORM_W - PAD * 2, 1),
                BackColor = Color.FromArgb(40, 60, 90)
            };

            var btnSavePreset = MakeBtn(L10n.T("preset.save"), PAD, BTN_Y, 160, BTN_H,
                Color.FromArgb(20, 35, 65), Color.FromArgb(100, 160, 240), Color.FromArgb(50, 85, 155));
            btnSavePreset.Click += BtnSavePreset_Click;

            var btnLoadPreset = MakeBtn(L10n.T("preset.load"), PAD + 168, BTN_Y, 160, BTN_H,
                Color.FromArgb(20, 35, 65), Color.FromArgb(100, 160, 240), Color.FromArgb(50, 85, 155));
            btnLoadPreset.Click += BtnLoadPreset_Click;

            var btnOk = MakeBtn(L10n.T("job_dlg.start_job"),
                FORM_W - PAD - BTN_W_CAN - 10 - BTN_W_OK, BTN_Y, BTN_W_OK, BTN_H,
                Color.FromArgb(20, 70, 40), Color.FromArgb(80, 230, 120), Color.FromArgb(50, 140, 80));
            btnOk.Font         = new Font("Consolas", 9.5f, FontStyle.Bold);
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Click       += BtnOk_Click;

            var btnCancel = MakeBtn(L10n.T("job_dlg.cancel"),
                FORM_W - PAD - BTN_W_CAN, BTN_Y, BTN_W_CAN, BTN_H,
                Color.FromArgb(50, 25, 25), Color.FromArgb(200, 100, 100), Color.FromArgb(100, 50, 50));
            btnCancel.Font         = new Font("Consolas", 9f);
            btnCancel.DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[] { tabs, lblPreview, sepLine,
                btnSavePreset, btnLoadPreset, btnOk, btnCancel });
            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        /// <summary>Populates the Main tab with device, job type, image file, format, and read/write option controls.</summary>
        private void BuildMainTab(TabPage tab)
        {
            int y = 14;

            tab.Controls.Add(MkLbl(L10n.T("job_dlg.device"), 10, y + 3));
            cmbDevice = MkCombo(150, y, 360);
            tab.Controls.Add(cmbDevice);

            y += 34;
            tab.Controls.Add(MkLbl(L10n.T("job_dlg.job_type"), 10, y + 3));
            cmbJobType = MkCombo(150, y, 220);
            cmbJobType.Items.AddRange(new object[] { L10n.T("job_dlg.read"), L10n.T("job_dlg.write") });
            cmbJobType.SelectedIndex = 0;
            cmbJobType.SelectedIndexChanged += (s, e) => SafeUpdatePreviews();
            tab.Controls.Add(cmbJobType);

            y += 34;
            tab.Controls.Add(MkLbl(L10n.T("job_dlg.image_file"), 10, y + 3));
            txtImageFile = MkTxt(150, y, 538);
            txtImageFile.TextChanged += (s, e) => SafeUpdatePreviews();
            var btnBrowse = MakeBtn("...", 696, y, 30, 22,
                Color.FromArgb(30, 50, 80), Color.White, Color.FromArgb(60, 100, 160));
            btnBrowse.Click += BtnBrowse_Click;
            tab.Controls.AddRange(new Control[] { txtImageFile, btnBrowse });

            y += 34;
            tab.Controls.Add(MkLbl(L10n.T("job_dlg.disk_format"), 10, y + 3));
            txtFormat = MkTxt(150, y, 180);
            txtFormat.TextChanged += (s, e) => SafeUpdatePreviews();
            var cmbFmtQuick = MkCombo(340, y, 390);
            cmbFmtQuick.Items.AddRange(new object[] {
                "ibm.1440","ibm.720","ibm.1200","ibm.360","ibm.180","ibm.320","ibm.800","ibm.2880",
                "amiga.amigados","amiga.amigados-hd",
                "atarist.360","atarist.400","atarist.720","atarist.800",
                "atari.90","atari.130","atari.180","atari.360",
                "commodore.1541","commodore.1571","commodore.1581",
                "apple2.525.ss.sd.35","apple2.525.ss.sd.40","mac.400","mac.800",
                "msx.1","msx.2",
                "pc98.2hd","pc98.2dd","pc98.2d",
                "acorn.adfs.s","acorn.adfs.m","acorn.adfs.l",
                "acorn.adfs.d","acorn.adfs.e","acorn.adfs.f",
                "dec.rx50","dec.rx33",
                "ensoniq.mirage","ensoniq.esq1",
                "gem.1","dragon.40","coco.35","zx.trdos.ds80",
            });
            cmbFmtQuick.SelectedIndexChanged += (s, e) =>
            { if (cmbFmtQuick.SelectedItem != null) txtFormat.Text = cmbFmtQuick.SelectedItem.ToString(); };
            tab.Controls.AddRange(new Control[] { txtFormat, cmbFmtQuick });

            y += 44;
            tab.Controls.Add(Sep(10, y, 750)); y += 8;
            AddSectionHeader(tab, L10n.T("job_dlg.common_opts"), 10, y, Color.FromArgb(100, 160, 220)); y += 22;

            tab.Controls.Add(MkLbl(L10n.T("job_dlg.revs"), 10, y + 3));
            nudRevs = MkNum(220, y, 70, 1, 10, 1);
            nudRevs.ValueChanged += (s, e) => SafeUpdatePreviews();
            tab.Controls.Add(MkLbl(L10n.T("job_dlg.revs_hint"), 300, y + 3));
            tab.Controls.Add(nudRevs);

            y += 30;
            tab.Controls.Add(MkLbl(L10n.T("job_dlg.densel"), 10, y + 3));
            cmbDensel = MkCombo(220, y, 110);
            cmbDensel.Items.AddRange(new object[] { "(auto)", "hd", "dd", "ed" });
            cmbDensel.SelectedIndex = 0;
            cmbDensel.SelectedIndexChanged += (s, e) => SafeUpdatePreviews();
            tab.Controls.Add(MkLbl(L10n.T("job_dlg.bitrate"), 340, y + 3));
            nudBitrate = MkNum(500, y, 100, 0, 2000000, 0);
            nudBitrate.ValueChanged += (s, e) => SafeUpdatePreviews();
            tab.Controls.Add(MkLbl(L10n.T("job_dlg.bitrate_hint"), 610, y + 3));
            tab.Controls.AddRange(new Control[] { cmbDensel, nudBitrate });

            y += 40;
            tab.Controls.Add(Sep(10, y, 750)); y += 8;
            AddSectionHeader(tab, L10n.T("job_dlg.read_opts"), 10, y, Color.FromArgb(60, 160, 240)); y += 22;

            chkRetries = MkChk(L10n.T("job_dlg.retries"), 10, y + 2);
            chkRetries.CheckedChanged += (s, e) => { nudRetries.Enabled = chkRetries.Checked; SafeUpdatePreviews(); };
            nudRetries = MkNum(145, y, 70, 0, 99, 3);
            nudRetries.Enabled = false;
            nudRetries.ValueChanged += (s, e) => SafeUpdatePreviews();
            chkNoClobber = MkChk(L10n.T("job_dlg.no_clobber"), 240, y + 2);
            chkNoClobber.CheckedChanged += (s, e) => SafeUpdatePreviews();
            chkRaw = MkChk(L10n.T("job_dlg.raw"), 400, y + 2);
            chkRaw.CheckedChanged += (s, e) => SafeUpdatePreviews();
            tab.Controls.AddRange(new Control[] { chkRetries, nudRetries, chkNoClobber, chkRaw });

            y += 28;
            chkReverse = MkChk(L10n.T("job_dlg.reverse_read"), 10, y + 2);
            chkReverse.CheckedChanged += (s, e) => SafeUpdatePreviews();
            chkHardSectors = MkChk(L10n.T("job_dlg.hard_sectors"), 280, y + 2);
            chkHardSectors.CheckedChanged += (s, e) => SafeUpdatePreviews();
            tab.Controls.AddRange(new Control[] { chkReverse, chkHardSectors });

            y += 36;
            tab.Controls.Add(Sep(10, y, 750)); y += 8;
            AddSectionHeader(tab, L10n.T("job_dlg.write_opts"), 10, y, Color.FromArgb(220, 140, 40)); y += 22;

            chkErase = MkChk(L10n.T("job_dlg.erase"), 10, y + 2);
            chkErase.CheckedChanged += (s, e) => SafeUpdatePreviews();
            chkVerify = MkChk(L10n.T("job_dlg.verify"), 120, y + 2);
            chkVerify.CheckedChanged += (s, e) => SafeUpdatePreviews();
            chkGenTg43 = MkChk(L10n.T("job_dlg.gen_tg43"), 240, y + 2);
            chkGenTg43.CheckedChanged += (s, e) => SafeUpdatePreviews();
            tab.Controls.AddRange(new Control[] { chkErase, chkVerify, chkGenTg43 });

            y += 28;
            tab.Controls.Add(MkLbl(L10n.T("job_dlg.precomp"), 10, y + 3));
            txtPrecomp = MkTxt(100, y, 100);
            txtPrecomp.TextChanged += (s, e) => SafeUpdatePreviews();
            chkReverseW = MkChk(L10n.T("job_dlg.reverse_write"), 220, y + 2);
            chkReverseW.CheckedChanged += (s, e) => SafeUpdatePreviews();
            chkHardSectorsW = MkChk(L10n.T("job_dlg.hard_sectors"), 350, y + 2);
            chkHardSectorsW.CheckedChanged += (s, e) => SafeUpdatePreviews();
            tab.Controls.AddRange(new Control[] { txtPrecomp, chkReverseW, chkHardSectorsW });
        }

        /// <summary>Populates the Tracks tab with cylinder range, head selection, step, hswap, and flippy head-offset controls.</summary>
        private void BuildTracksTab(TabPage tab)
        {
            int y = 14;
            AddSectionHeader(tab, L10n.T("job_dlg.track_sel_head"), 10, y, Color.FromArgb(160, 200, 255));
            y += 26;

            tab.Controls.Add(new Label
            {
                Text      = L10n.T("job_dlg.track_info"),
                Location  = new Point(10, y),
                Size      = new Size(760, 38),
                Font      = new Font("Consolas", 7.5f),
                ForeColor = Color.FromArgb(90, 130, 180),
                BackColor = Color.Transparent
            });
            y += 46;

            tab.Controls.Add(MkLbl(L10n.T("job_dlg.cylinders"), 10, y + 3));
            tab.Controls.Add(MkLbl(L10n.T("job_dlg.cyl_start"), 180, y + 3));
            nudStartCyl = MkNum(230, y, 75, 0, 255, 0);
            nudStartCyl.ValueChanged += (s, e) => SafeUpdatePreviews();
            tab.Controls.Add(MkLbl(L10n.T("job_dlg.cyl_end"), 320, y + 3));
            nudEndCyl = MkNum(360, y, 75, 0, 255, 79);
            nudEndCyl.ValueChanged += (s, e) => SafeUpdatePreviews();
            tab.Controls.Add(MkLbl(L10n.T("job_dlg.cyl_hint"), 450, y + 3));
            tab.Controls.AddRange(new Control[] { nudStartCyl, nudEndCyl });

            y += 34;
            tab.Controls.Add(MkLbl(L10n.T("job_dlg.heads"), 10, y + 3));
            cmbHead = MkCombo(180, y, 220);
            cmbHead.Items.AddRange(new object[]
            {
                L10n.T("job_dlg.heads_both"),
                L10n.T("job_dlg.heads_0"),
                L10n.T("job_dlg.heads_1")
            });
            cmbHead.SelectedIndex = 0;
            cmbHead.SelectedIndexChanged += (s, e) => SafeUpdatePreviews();
            tab.Controls.Add(cmbHead);

            y += 34;
            tab.Controls.Add(MkLbl(L10n.T("job_dlg.step"), 10, y + 3));
            nudStep = MkNum(180, y, 75, 1, 9, 1);
            nudStep.ValueChanged += (s, e) => SafeUpdatePreviews();
            tab.Controls.Add(MkLbl(L10n.T("job_dlg.step_hint"), 270, y + 3));
            tab.Controls.Add(nudStep);

            y += 34;
            chkHSwap = MkChk(L10n.T("job_dlg.hswap"), 10, y + 2);
            chkHSwap.CheckedChanged += (s, e) => SafeUpdatePreviews();
            tab.Controls.Add(chkHSwap);

            y += 36;
            tab.Controls.Add(Sep(10, y, 760)); y += 10;
            AddSectionHeader(tab, L10n.T("job_dlg.flippy_head"), 10, y, Color.FromArgb(180, 140, 60));
            y += 24;

            chkHead0Off = MkChk(L10n.T("job_dlg.h0off"), 10, y + 2);
            nudHead0Off = MkNum(100, y, 75, -9, 9, 0);
            nudHead0Off.Enabled = false;
            chkHead0Off.CheckedChanged += (s, e) => { nudHead0Off.Enabled = chkHead0Off.Checked; SafeUpdatePreviews(); };
            nudHead0Off.ValueChanged += (s, e) => SafeUpdatePreviews();
            tab.Controls.Add(MkLbl(L10n.T("job_dlg.h0off_hint"), 190, y + 3));
            tab.Controls.AddRange(new Control[] { chkHead0Off, nudHead0Off });

            y += 30;
            chkHead1Off = MkChk(L10n.T("job_dlg.h1off"), 10, y + 2);
            nudHead1Off = MkNum(100, y, 75, -9, 9, 0);
            nudHead1Off.Enabled = false;
            chkHead1Off.CheckedChanged += (s, e) => { nudHead1Off.Enabled = chkHead1Off.Checked; SafeUpdatePreviews(); };
            nudHead1Off.ValueChanged += (s, e) => SafeUpdatePreviews();
            tab.Controls.Add(MkLbl(L10n.T("job_dlg.h1off_hint"), 190, y + 3));
            tab.Controls.AddRange(new Control[] { chkHead1Off, nudHead1Off });

            y += 44;
            lblTrackSpec = new Label
            {
                Location  = new Point(10, y),
                Size      = new Size(760, 22),
                Font      = new Font("Consolas", 8.5f),
                ForeColor = Color.FromArgb(80, 200, 80),
                BackColor = Color.FromArgb(14, 18, 28),
                AutoSize  = false,
                Text      = "→  (default)"
            };
            tab.Controls.Add(lblTrackSpec);
        }

        /// <summary>Populates the Advanced tab with drive selection, extra CLI arguments, and token reference notes.</summary>
        private void BuildAdvancedTab(TabPage tab)
        {
            int y = 14;
            AddSectionHeader(tab, L10n.T("job_dlg.adv_head"), 10, y, Color.FromArgb(160, 200, 255));
            y += 28;

            tab.Controls.Add(MkLbl(L10n.T("job_dlg.drive"), 10, y + 3));
            cmbDrive = MkCombo(120, y, 120);
            cmbDrive.Items.AddRange(new object[] { "(auto)", "a", "b", "0", "1", "2", "3" });
            cmbDrive.SelectedIndex = 0;
            cmbDrive.SelectedIndexChanged += (s, e) => SafeUpdatePreviews();
            tab.Controls.Add(MkLbl(L10n.T("job_dlg.drive_hint"), 255, y + 3));
            tab.Controls.Add(cmbDrive);

            y += 34;
            tab.Controls.Add(MkLbl(L10n.T("job_dlg.extra_args"), 10, y + 3));
            txtExtraArgs = MkTxt(120, y, 620);
            txtExtraArgs.TextChanged += (s, e) => SafeUpdatePreviews();
            tab.Controls.Add(txtExtraArgs);

            y += 50;
            tab.Controls.Add(new Label
            {
                Text      = L10n.T("job_dlg.token_note"),
                Location  = new Point(10, y),
                Size      = new Size(750, 80),
                Font      = new Font("Consolas", 8f),
                ForeColor = Color.FromArgb(90, 130, 170),
                BackColor = Color.FromArgb(16, 20, 30)
            });
        }

        /// <summary>Populates the Post-Actions tab with a list view and Add/Edit/Remove/Move buttons.</summary>
        private void BuildPostActionsTab(TabPage tab)
        {
            tab.Controls.Add(new Label
            {
                Text      = L10n.T("job_dlg.pa_hint"),
                Location  = new Point(10, 10),
                Size      = new Size(760, 18),
                Font      = new Font("Consolas", 7.5f),
                ForeColor = Color.FromArgb(90, 130, 170),
                BackColor = Color.Transparent
            });

            lvPostActions = new ListView
            {
                Location      = new Point(10, 34),
                Size          = new Size(760, 380),
                View          = View.Details,
                FullRowSelect = true,
                BackColor     = Color.FromArgb(18, 22, 32),
                ForeColor     = Color.FromArgb(180, 210, 255),
                Font          = new Font("Consolas", 8f),
                BorderStyle   = BorderStyle.FixedSingle
            };
            lvPostActions.Columns.Add(L10n.T("job_dlg.pa_col_ord"),   30);
            lvPostActions.Columns.Add(L10n.T("job_dlg.pa_col_name"), 150);
            lvPostActions.Columns.Add(L10n.T("job_dlg.pa_col_type"),  90);
            lvPostActions.Columns.Add(L10n.T("job_dlg.pa_col_exe"),  280);
            lvPostActions.Columns.Add(L10n.T("job_dlg.pa_col_args"), 170);
            lvPostActions.Columns.Add(L10n.T("job_dlg.pa_col_en"),    30);
            tab.Controls.Add(lvPostActions);

            int y = 422;
            var btnAdd  = MakeBtn(L10n.T("job_dlg.pa_add"),    10, y,  90, 26, Color.FromArgb(20, 55, 30), Color.FromArgb(100, 220, 130), Color.FromArgb(50, 120, 70));
            var btnEdit = MakeBtn(L10n.T("job_dlg.pa_edit"),  110, y,  80, 26, Color.FromArgb(20, 35, 65), Color.FromArgb(100, 160, 240), Color.FromArgb(50, 80, 140));
            var btnRem  = MakeBtn(L10n.T("job_dlg.pa_remove"),200, y,  90, 26, Color.FromArgb(55, 20, 20), Color.FromArgb(220, 80, 80),   Color.FromArgb(100, 40, 40));
            var btnUp   = MakeBtn("▲",                         300, y,  40, 26, Color.FromArgb(20, 35, 55), Color.White,                   Color.FromArgb(50, 80, 120));
            var btnDown = MakeBtn("▼",                         348, y,  40, 26, Color.FromArgb(20, 35, 55), Color.White,                   Color.FromArgb(50, 80, 120));

            btnAdd.Click  += BtnAddAction_Click;
            btnEdit.Click += BtnEditAction_Click;
            btnRem.Click  += (s, e) => { if (lvPostActions.SelectedItems.Count > 0) lvPostActions.Items.Remove(lvPostActions.SelectedItems[0]); ReorderActions(); };
            btnUp.Click   += (s, e) => MoveAction(-1);
            btnDown.Click += (s, e) => MoveAction(1);

            tab.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnRem, btnUp, btnDown });
        }

        /// <summary>
        /// Calls <see cref="UpdatePreview"/> and <see cref="UpdateTrackSpecLabel"/> only after
        /// the dialog is fully initialised to avoid null-reference exceptions during construction.
        /// </summary>
        private void SafeUpdatePreviews()
        {
            if (!_initialized) return;
            UpdatePreview();
            UpdateTrackSpecLabel();
        }

        /// <summary>Rebuilds the command-line preview label from the current control values.</summary>
        private void UpdatePreview()
        {
            if (lblPreview == null) return;
            try
            {
                var p  = BuildParameters();
                var jt = cmbJobType.SelectedIndex == 0 ? JobType.Read : JobType.Write;
                string full = $"gw.exe {p.BuildArgs(jt)}";
                lblPreview.Text = full.Length > 130 ? full[..127] + "…" : full;
            }
            catch { lblPreview.Text = "(preview unavailable)"; }
        }

        /// <summary>Rebuilds the <c>--tracks=…</c> spec label shown on the Tracks tab.</summary>
        private void UpdateTrackSpecLabel()
        {
            if (lblTrackSpec == null) return;
            try
            {
                var p   = BuildParameters();
                string  full = p.BuildArgs(JobType.Read);
                int     idx  = full.IndexOf("--tracks=");
                lblTrackSpec.Text = idx >= 0
                    ? $"→  {full.Substring(idx).Split(' ')[0]}"
                    : "→  (default — full disk, both sides)";
            }
            catch { }
        }

        /// <summary>Reads all dialog controls and constructs a <see cref="GwParameters"/> instance.</summary>
        /// <returns>A fully populated <see cref="GwParameters"/> reflecting the current UI state.</returns>
        private GwParameters BuildParameters()
        {
            var p = new GwParameters
            {
                ImageFile     = txtImageFile?.Text ?? "",
                DiskFormat    = string.IsNullOrWhiteSpace(txtFormat?.Text) ? null : txtFormat.Text,
                StartCylinder = (int)(nudStartCyl?.Value ?? 0),
                EndCylinder   = (int)(nudEndCyl?.Value   ?? 79),
                Head          = (cmbHead?.SelectedIndex) switch { 1 => 0, 2 => 1, _ => (int?)null },
                Step          = (int)(nudStep?.Value ?? 1) == 1 ? (int?)null : (int)nudStep!.Value,
                HSwap         = chkHSwap?.Checked        ?? false,
                Head0Offset   = (chkHead0Off?.Checked  ?? false) ? (int)nudHead0Off!.Value  : (int?)null,
                Head1Offset   = (chkHead1Off?.Checked  ?? false) ? (int)nudHead1Off!.Value  : (int?)null,
                Revolutions   = (int)(nudRevs?.Value ?? 1) > 1   ? (int)nudRevs!.Value       : (int?)null,
                Densel        = (cmbDensel?.SelectedIndex ?? 0) > 0 ? cmbDensel!.Text        : null,
                Bitrate       = (int)(nudBitrate?.Value ?? 0) > 0   ? (int)nudBitrate!.Value : (int?)null,
                Retries       = (chkRetries?.Checked    ?? false) ? (int)nudRetries!.Value   : (int?)null,
                NoClobber     = chkNoClobber?.Checked    ?? false,
                RawRead       = chkRaw?.Checked          ?? false,
                Reverse       = (cmbJobType?.SelectedIndex ?? 0) == 0
                                    ? (chkReverse?.Checked      ?? false)
                                    : (chkReverseW?.Checked     ?? false),
                HardSectors   = (cmbJobType?.SelectedIndex ?? 0) == 0
                                    ? (chkHardSectors?.Checked  ?? false)
                                    : (chkHardSectorsW?.Checked ?? false),
                Erase         = chkErase?.Checked    ?? false,
                Verify        = chkVerify?.Checked   ?? false,
                Precomp       = string.IsNullOrWhiteSpace(txtPrecomp?.Text) ? null : txtPrecomp!.Text,
                GenTg43       = chkGenTg43?.Checked  ?? false,
                Drive         = (cmbDrive?.SelectedIndex ?? 0) > 0 ? cmbDrive!.SelectedItem?.ToString() : null,
                ExtraArgs     = string.IsNullOrWhiteSpace(txtExtraArgs?.Text) ? null : txtExtraArgs!.Text
            };

            if (cmbDevice?.SelectedItem is GreaseWeazleDevice dev)
                p.Device = dev.SerialPort;

            return p;
        }

        /// <summary>
        /// Opens a Save or Open file dialog for the image file depending on whether the
        /// selected job type is Read or Write, then sets <see cref="txtImageFile"/>.
        /// </summary>
        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            bool isRead = cmbJobType.SelectedIndex == 0;
            if (isRead)
            {
                using var sfd = new SaveFileDialog
                {
                    Title      = L10n.T("job_dlg.save_image"),
                    Filter     = "SCP (*.scp)|*.scp|HFE (*.hfe)|*.hfe|IMG (*.img)|*.img|ADF (*.adf)|*.adf|All (*.*)|*.*",
                    DefaultExt = "scp"
                };
                if (sfd.ShowDialog(this) == DialogResult.OK) txtImageFile.Text = sfd.FileName;
            }
            else
            {
                using var ofd = new OpenFileDialog
                {
                    Title  = L10n.T("job_dlg.open_image"),
                    Filter = "Disk Images (*.scp;*.hfe;*.img;*.adf;*.ipf)|*.scp;*.hfe;*.img;*.adf;*.ipf|All (*.*)|*.*"
                };
                if (ofd.ShowDialog(this) == DialogResult.OK) txtImageFile.Text = ofd.FileName;
            }
        }

        /// <summary>
        /// Validates inputs, builds the <see cref="GwJob"/>, and assigns it to <see cref="Result"/>.
        /// Shows a warning and cancels the dialog result if the image file is missing (non-repetitive mode).
        /// </summary>
        private void BtnOk_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtImageFile.Text) &&
                !(chkRepetitive?.Checked ?? false))
            {
                MessageBox.Show(
                    L10n.T("job_dlg.missing_image"),
                    L10n.T("job_dlg.missing_image_cap"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            var jt  = cmbJobType.SelectedIndex == 0 ? JobType.Read : JobType.Write;
            var job = new GwJob { JobType = jt, Parameters = BuildParameters() };

            job.RepetitiveMode = chkRepetitive?.Checked ?? false;
            job.FilePattern    = txtFilePattern?.Text   ?? "";
            job.OutputFolder   = txtOutputFolder?.Text  ?? "";
            job.DiskIndex      = (int)(nudStartIndex?.Value ?? 1);
            job.DateTimeFormat = txtDtFormat?.Text      ?? "yyyyMMdd_HHmmss";

            if (cmbDevice.SelectedItem is GreaseWeazleDevice selectedDev)
                job.Device = selectedDev;

            foreach (ListViewItem item in lvPostActions.Items)
                job.PostActions.Add((PostAction)item.Tag!);

            job.SourcePreset = BuildPreset();

            Result = job;
        }

        /// <summary>Populates the device combo box and selects the pre-selected device if provided.</summary>
        private void PopulateDevices()
        {
            cmbDevice.Items.Add(L10n.T("job_dlg.auto_device"));
            foreach (var d in _devices) cmbDevice.Items.Add(d);

            if (_preselectedDevice != null)
            {
                for (int i = 0; i < cmbDevice.Items.Count; i++)
                    if (cmbDevice.Items[i] is GreaseWeazleDevice d && d.Id == _preselectedDevice.Id)
                    { cmbDevice.SelectedIndex = i; break; }
            }
            else
                cmbDevice.SelectedIndex = _devices.Count > 0 ? 1 : 0;
        }

        /// <summary>Opens a <see cref="PostActionDialog"/> for a new action and appends it to the list view.</summary>
        private void BtnAddAction_Click(object? sender, EventArgs e)
        {
            var action = new PostAction { Order = lvPostActions.Items.Count + 1 };
            using var dlg = new PostActionDialog(action);
            if (dlg.ShowDialog(this) == DialogResult.OK)
                lvPostActions.Items.Add(ActionToItem(action));
        }

        /// <summary>Opens a <see cref="PostActionDialog"/> to edit the selected action and refreshes its list view row.</summary>
        private void BtnEditAction_Click(object? sender, EventArgs e)
        {
            if (lvPostActions.SelectedItems.Count == 0) return;
            var item   = lvPostActions.SelectedItems[0];
            var action = (PostAction)item.Tag!;
            using var dlg = new PostActionDialog(action);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                item.Text             = action.Order.ToString();
                item.SubItems[1].Text = action.Name;
                item.SubItems[2].Text = action.ActionType.ToString();
                item.SubItems[3].Text = action.ExecutablePath;
                item.SubItems[4].Text = action.Arguments;
                item.SubItems[5].Text = action.IsEnabled ? "✓" : "—";
            }
        }

        /// <summary>Creates a <see cref="ListViewItem"/> representing the given <see cref="PostAction"/>.</summary>
        /// <param name="a">The post-action to represent.</param>
        /// <returns>A list view item with sub-items for name, type, executable, arguments, and enabled state.</returns>
        private ListViewItem ActionToItem(PostAction a)
        {
            var item = new ListViewItem(a.Order.ToString());
            item.SubItems.Add(a.Name);
            item.SubItems.Add(a.ActionType.ToString());
            item.SubItems.Add(a.ExecutablePath);
            item.SubItems.Add(a.Arguments);
            item.SubItems.Add(a.IsEnabled ? "✓" : "—");
            item.Tag = a;
            return item;
        }

        /// <summary>Moves the selected post-action row up (<paramref name="dir"/> = -1) or down (+1) and re-numbers all rows.</summary>
        private void MoveAction(int dir)
        {
            if (lvPostActions.SelectedItems.Count == 0) return;
            var item = lvPostActions.SelectedItems[0];
            int idx = item.Index, nIdx = idx + dir;
            if (nIdx < 0 || nIdx >= lvPostActions.Items.Count) return;
            lvPostActions.Items.RemoveAt(idx);
            lvPostActions.Items.Insert(nIdx, item);
            item.Selected = true;
            ReorderActions();
        }

        /// <summary>Re-numbers all post-action list view rows and their underlying <see cref="PostAction.Order"/> values.</summary>
        private void ReorderActions()
        {
            for (int i = 0; i < lvPostActions.Items.Count; i++)
            {
                ((PostAction)lvPostActions.Items[i].Tag!).Order = i + 1;
                lvPostActions.Items[i].Text = (i + 1).ToString();
            }
        }

        /// <summary>Owner-draws each tab header with a dark background, blue accent on selection, and styled text.</summary>
        private void Tabs_DrawItem(object? sender, DrawItemEventArgs e)
        {
            var tab    = (TabControl)sender!;
            var bounds = tab.GetTabRect(e.Index);
            bool sel   = e.Index == tab.SelectedIndex;

            using var bg = new SolidBrush(sel
                ? Color.FromArgb(28, 48, 82)
                : Color.FromArgb(18, 22, 34));
            e.Graphics.FillRectangle(bg, bounds);

            if (sel)
            {
                using var accent = new SolidBrush(Color.FromArgb(60, 130, 220));
                e.Graphics.FillRectangle(accent, bounds.X, bounds.Bottom - 3, bounds.Width, 3);
            }

            using var border = new Pen(Color.FromArgb(40, 65, 100), 1f);
            e.Graphics.DrawRectangle(border, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);

            using var fg   = new SolidBrush(sel
                ? Color.FromArgb(180, 220, 255)
                : Color.FromArgb(110, 145, 185));
            using var font = new Font("Consolas", 8.5f, sel ? FontStyle.Bold : FontStyle.Regular);
            var sf = new System.Drawing.StringFormat
                { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString(tab.TabPages[e.Index].Text, font, fg, bounds, sf);
        }

        /// <summary>Populates the Repeat tab with output folder, file pattern, start index, date-time format, and live preview controls.</summary>
        private void BuildRepeatTab(TabPage tab)
        {
            int y = 14;

            chkRepetitive = MkChk(L10n.T("job_dlg.repeat_enabled"), 10, y);
            chkRepetitive.Font = new Font("Consolas", 9f, FontStyle.Bold);
            chkRepetitive.ForeColor = Color.FromArgb(100, 220, 160);
            chkRepetitive.CheckedChanged += (s, e) => SafeUpdatePreviews();
            tab.Controls.Add(chkRepetitive);

            y += 34;
            tab.Controls.Add(Sep(10, y, 760)); y += 12;

            tab.Controls.Add(MkLbl(L10n.T("job_dlg.output_folder"), 10, y + 3));
            txtOutputFolder = MkTxt(175, y, 500);
            txtOutputFolder.PlaceholderText = L10n.T("job_dlg.output_folder_hint");
            txtOutputFolder.TextChanged += (s, e) => UpdatePatternPreview();
            var btnBrowseFolder = MakeBtn("…", 683, y, 30, 22,
                Color.FromArgb(30, 50, 80), Color.White, Color.FromArgb(60, 100, 160));
            btnBrowseFolder.Click += (s, e) =>
            {
                using var fbd = new FolderBrowserDialog
                {
                    Description            = L10n.T("job_dlg.output_folder"),
                    UseDescriptionForTitle = true,
                    ShowNewFolderButton    = true
                };
                if (!string.IsNullOrWhiteSpace(txtOutputFolder.Text) &&
                    Directory.Exists(txtOutputFolder.Text))
                    fbd.InitialDirectory = txtOutputFolder.Text;
                if (fbd.ShowDialog(this) == DialogResult.OK)
                    txtOutputFolder.Text = fbd.SelectedPath;
            };
            tab.Controls.AddRange(new Control[] { txtOutputFolder, btnBrowseFolder });

            y += 32;
            txtFilePattern = MkTxt(175, y, 570);
            txtFilePattern.TextChanged += (s, e) => UpdatePatternPreview();
            tab.Controls.Add(txtFilePattern);

            y += 28;
            tab.Controls.Add(new Label
            {
                Text      = L10n.T("job_dlg.pattern_hint"),
                Location  = new Point(175, y),
                Size      = new Size(570, 16),
                Font      = new Font("Consolas", 7.5f),
                ForeColor = Color.FromArgb(90, 130, 170),
                BackColor = Color.Transparent
            });

            y += 28;
            tab.Controls.Add(MkLbl(L10n.T("job_dlg.start_index"), 10, y + 3));
            nudStartIndex = MkNum(175, y, 100, 1, 9999, 1);
            nudStartIndex.ValueChanged += (s, e) => UpdatePatternPreview();
            tab.Controls.Add(nudStartIndex);

            y += 32;
            tab.Controls.Add(MkLbl(L10n.T("job_dlg.dt_format"), 10, y + 3));
            txtDtFormat = MkTxt(175, y, 260);
            txtDtFormat.Text = "yyyyMMdd_HHmmss";
            txtDtFormat.TextChanged += (s, e) => UpdatePatternPreview();
            tab.Controls.Add(MkLbl(L10n.T("job_dlg.dt_format_hint"), 444, y + 3));
            tab.Controls.Add(txtDtFormat);

            y += 36;
            tab.Controls.Add(Sep(10, y, 760)); y += 12;

            tab.Controls.Add(MkLbl(L10n.T("job_dlg.pattern_preview"), 10, y + 3));
            lblPatternPreview = new Label
            {
                Location  = new Point(175, y),
                Size      = new Size(570, 22),
                Font      = new Font("Consolas", 9f),
                ForeColor = Color.FromArgb(220, 200, 80),
                BackColor = Color.FromArgb(14, 18, 28),
                AutoSize  = false,
                Padding   = new Padding(4, 2, 0, 0)
            };
            tab.Controls.Add(lblPatternPreview);

            y += 40;
            tab.Controls.Add(new Label
            {
                Text      = L10n.T("job_dlg.repeat_note"),
                Location  = new Point(10, y),
                Size      = new Size(760, 42),
                Font      = new Font("Consolas", 8f),
                ForeColor = Color.FromArgb(90, 130, 160),
                BackColor = Color.FromArgb(16, 20, 30)
            });

            y += 64;
            tab.Controls.Add(Sep(10, y, 760)); y += 12;

            tab.Controls.Add(MkLbl("Preset Name:", 10, y + 3));
            txtPresetName = MkTxt(175, y, 400);
            txtPresetName.Text = "My Preset";
            tab.Controls.Add(txtPresetName);
        }

        /// <summary>Refreshes the file-pattern live preview label from the current pattern, index, date-time format, and output folder.</summary>
        private void UpdatePatternPreview()
        {
            if (lblPatternPreview == null || txtFilePattern == null) return;
            try
            {
                string pat = txtFilePattern.Text;
                string dtf = string.IsNullOrWhiteSpace(txtDtFormat?.Text)
                    ? "yyyyMMdd_HHmmss" : txtDtFormat.Text;
                int idx = (int)(nudStartIndex?.Value ?? 1);

                if (string.IsNullOrWhiteSpace(pat))
                {
                    lblPatternPreview.Text = "—";
                    return;
                }

                string expanded = Models.FilePattern.Preview(pat, idx, dtf);

                string folder = txtOutputFolder?.Text ?? "";
                if (!string.IsNullOrWhiteSpace(folder))
                    expanded = System.IO.Path.Combine(folder, expanded);

                lblPatternPreview.Text = expanded;
            }
            catch { }
        }

        /// <summary>
        /// Opens a Save dialog and serialises the current dialog state to a <c>.gwpreset</c> file.
        /// </summary>
        private void BtnSavePreset_Click(object? sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog
            {
                Title            = L10n.T("preset.save_title"),
                Filter           = L10n.T("preset.filter"),
                DefaultExt       = "gwpreset",
                InitialDirectory = Models.JobPreset.PresetsDirectory,
                FileName         = (txtPresetName?.Text ?? "preset")
                                       .Replace(" ", "_")
                                       .Replace("/", "-") + ".gwpreset"
            };
            if (sfd.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                var preset = BuildPreset();
                preset.SaveToFile(sfd.FileName);
                MessageBox.Show(L10n.T("preset.saved"), L10n.T("preset.save_title"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(L10n.T("preset.error_save"), ex.Message),
                    L10n.T("preset.save_title"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Opens an Open dialog, deserialises a <c>.gwpreset</c> file, and applies it to all controls via <see cref="LoadFromPreset"/>.
        /// </summary>
        private void BtnLoadPreset_Click(object? sender, EventArgs e)
        {
            string dir = Models.JobPreset.PresetsDirectory;
            System.IO.Directory.CreateDirectory(dir);

            using var ofd = new OpenFileDialog
            {
                Title            = L10n.T("preset.load_title"),
                Filter           = L10n.T("preset.filter"),
                InitialDirectory = dir
            };
            if (ofd.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                var preset = Models.JobPreset.LoadFromFile(ofd.FileName);
                LoadFromPreset(preset);
                MessageBox.Show(L10n.T("preset.loaded"), L10n.T("preset.load_title"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(L10n.T("preset.error_load"), ex.Message),
                    L10n.T("preset.load_title"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>Snapshots all dialog controls into a <see cref="Models.JobPreset"/>.</summary>
        /// <returns>A new preset reflecting the current UI state.</returns>
        private Models.JobPreset BuildPreset()
        {
            var jt      = cmbJobType?.SelectedIndex == 0 ? JobType.Read : JobType.Write;
            var p       = BuildParameters();
            var actions = new List<PostAction>();
            if (lvPostActions != null)
                foreach (ListViewItem item in lvPostActions.Items)
                    if (item.Tag is PostAction a) actions.Add(a);

            var preset = Models.JobPreset.FromParameters(p, jt, actions,
                txtPresetName?.Text ?? "Preset");

            preset.FilePattern    = txtFilePattern?.Text ?? "";
            preset.RepetitiveMode = chkRepetitive?.Checked ?? false;
            preset.StartIndex     = (int)(nudStartIndex?.Value ?? 1);
            preset.DateTimeFormat = txtDtFormat?.Text ?? "yyyyMMdd_HHmmss";
            preset.OutputFolder   = txtOutputFolder?.Text ?? "";
            return preset;
        }

        /// <summary>
        /// Populates all dialog controls from a loaded <see cref="Models.JobPreset"/>.
        /// Preview updates are suppressed during loading and re-enabled at the end.
        /// </summary>
        /// <param name="preset">The preset to apply.</param>
        public void LoadFromPreset(Models.JobPreset preset)
        {
            _initialized = false;

            cmbJobType.SelectedIndex = preset.JobType == JobType.Read ? 0 : 1;

            txtFormat.Text = preset.DiskFormat ?? "";

            if (!string.IsNullOrWhiteSpace(preset.Device))
                for (int i = 0; i < cmbDevice.Items.Count; i++)
                    if (cmbDevice.Items[i] is GreaseWeazleDevice d &&
                        d.SerialPort == preset.Device)
                    { cmbDevice.SelectedIndex = i; break; }

            nudStartCyl.Value     = preset.StartCylinder ?? 0;
            nudEndCyl.Value       = preset.EndCylinder   ?? 79;
            cmbHead.SelectedIndex = preset.Head switch { 0 => 1, 1 => 2, _ => 0 };
            nudStep.Value         = preset.Step           ?? 1;
            chkHSwap.Checked      = preset.HSwap;
            chkHead0Off.Checked   = preset.Head0Offset.HasValue;
            nudHead0Off.Value     = preset.Head0Offset   ?? 0;
            chkHead1Off.Checked   = preset.Head1Offset.HasValue;
            nudHead1Off.Value     = preset.Head1Offset   ?? 0;

            nudRevs.Value = preset.Revolutions ?? 1;
            if (!string.IsNullOrWhiteSpace(preset.Densel))
                for (int i = 0; i < cmbDensel.Items.Count; i++)
                    if (cmbDensel.Items[i]?.ToString() == preset.Densel)
                    { cmbDensel.SelectedIndex = i; break; }
            nudBitrate.Value = preset.Bitrate ?? 0;

            chkRetries.Checked     = preset.Retries.HasValue;
            nudRetries.Value       = preset.Retries    ?? 3;
            chkNoClobber.Checked   = preset.NoClobber;
            chkRaw.Checked         = preset.RawRead;
            chkReverse.Checked     = preset.Reverse;
            chkHardSectors.Checked = preset.HardSectors;

            chkErase.Checked         = preset.Erase;
            chkVerify.Checked        = preset.Verify;
            txtPrecomp.Text          = preset.Precomp    ?? "";
            chkGenTg43.Checked       = preset.GenTg43;
            chkReverseW.Checked      = preset.Reverse;
            chkHardSectorsW.Checked  = preset.HardSectors;

            txtExtraArgs.Text = preset.ExtraArgs ?? "";
            if (!string.IsNullOrWhiteSpace(preset.Drive))
                for (int i = 0; i < cmbDrive.Items.Count; i++)
                    if (cmbDrive.Items[i]?.ToString() == preset.Drive)
                    { cmbDrive.SelectedIndex = i; break; }

            chkRepetitive.Checked = preset.RepetitiveMode;
            txtFilePattern.Text   = preset.FilePattern    ?? "";
            txtOutputFolder.Text  = preset.OutputFolder   ?? "";
            nudStartIndex.Value   = Math.Max(1, preset.StartIndex);
            txtDtFormat.Text      = preset.DateTimeFormat ?? "yyyyMMdd_HHmmss";
            if (txtPresetName != null) txtPresetName.Text = preset.PresetName;

            if (lvPostActions != null)
            {
                lvPostActions.Items.Clear();
                foreach (var ap in preset.PostActions)
                    lvPostActions.Items.Add(ActionToItem(ap.ToPostAction()));
            }

            _initialized = true;
            SafeUpdatePreviews();
            UpdatePatternPreview();
        }

        /// <summary>Creates a styled field-caption label.</summary>
        private static Label MkLbl(string text, int x, int y) => new()
        {
            Text = text, Location = new Point(x, y), AutoSize = true,
            Font = new Font("Consolas", 8f), ForeColor = Color.FromArgb(130, 160, 200),
            BackColor = Color.Transparent
        };

        /// <summary>Creates a styled single-line text box.</summary>
        private static TextBox MkTxt(int x, int y, int w) => new()
        {
            Location = new Point(x, y), Size = new Size(w, 22),
            BackColor = Color.FromArgb(28, 34, 48), ForeColor = Color.FromArgb(200, 230, 255),
            BorderStyle = BorderStyle.FixedSingle, Font = new Font("Consolas", 8.5f)
        };

        /// <summary>Creates a styled drop-down combo box.</summary>
        private static ComboBox MkCombo(int x, int y, int w) => new()
        {
            Location = new Point(x, y), Size = new Size(w, 22),
            BackColor = Color.FromArgb(28, 34, 48), ForeColor = Color.FromArgb(200, 230, 255),
            FlatStyle = FlatStyle.Flat, Font = new Font("Consolas", 8.5f),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        /// <summary>Creates a styled numeric spinner with the given bounds and value range.</summary>
        private static NumericUpDown MkNum(int x, int y, int w, int min, int max, int val) => new()
        {
            Location = new Point(x, y), Size = new Size(w, 22),
            Minimum = min, Maximum = max, Value = val,
            BackColor = Color.FromArgb(28, 34, 48), ForeColor = Color.FromArgb(200, 230, 255),
            Font = new Font("Consolas", 8.5f), BorderStyle = BorderStyle.FixedSingle
        };

        /// <summary>Creates a styled check box.</summary>
        private static CheckBox MkChk(string text, int x, int y) => new()
        {
            Text = text, Location = new Point(x, y), AutoSize = true,
            ForeColor = Color.FromArgb(160, 200, 255), BackColor = Color.Transparent,
            Font = new Font("Consolas", 8.5f)
        };

        /// <summary>Creates a flat-styled button with the given position, size, and colours.</summary>
        private static Button MakeBtn(string text, int x, int y, int w, int h,
            Color bg, Color fg, Color border)
        {
            var b = new Button
            {
                Text = text, Location = new Point(x, y), Size = new Size(w, h),
                FlatStyle = FlatStyle.Flat, BackColor = bg, ForeColor = fg,
                Font = new Font("Consolas", 8f)
            };
            b.FlatAppearance.BorderColor = border;
            return b;
        }

        /// <summary>Creates a 1-pixel horizontal rule for use as a visual section separator.</summary>
        private static Label Sep(int x, int y, int w) => new()
        {
            Location = new Point(x, y), Size = new Size(w, 1),
            BackColor = Color.FromArgb(40, 60, 90)
        };

        /// <summary>Adds a bold, coloured section heading label to a tab page.</summary>
        private static void AddSectionHeader(TabPage tab, string text, int x, int y, Color color) =>
            tab.Controls.Add(new Label
            {
                Text = text, Location = new Point(x, y), AutoSize = true,
                Font = new Font("Consolas", 8.5f, FontStyle.Bold),
                ForeColor = color, BackColor = Color.Transparent
            });
    }

    /// <summary>
    /// Compact modal dialog for creating or editing a single <see cref="PostAction"/>.
    /// Presents fields for name, action type, executable path, arguments, and enabled state.
    /// Changes are written back to the supplied <see cref="PostAction"/> on OK.
    /// </summary>
    public class PostActionDialog : Form
    {
        private readonly PostAction _action;
        private TextBox  txtName    = null!;
        private ComboBox cmbType    = null!;
        private TextBox  txtExe     = null!;
        private TextBox  txtArgs    = null!;
        private CheckBox chkEnabled = null!;

        /// <summary>
        /// Initialises the dialog, pre-populating all fields from <paramref name="action"/>.
        /// </summary>
        /// <param name="action">The post-action to edit; modified in-place on OK.</param>
        public PostActionDialog(PostAction action)
        {
            _action = action;
            InitializeComponent();
        }

        /// <summary>Builds and lays out all child controls.</summary>
        private void InitializeComponent()
        {
            Text            = L10n.T("pa_dlg.title");
            Size            = new Size(560, 270);
            BackColor       = Color.FromArgb(18, 22, 32);
            ForeColor       = Color.FromArgb(180, 210, 255);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition   = FormStartPosition.CenterParent;
            MaximizeBox     = false;

            Label L(string key, int x, int y) => new()
            {
                Text = L10n.T(key), Location = new Point(x, y), AutoSize = true,
                Font = new Font("Consolas", 8f), ForeColor = Color.FromArgb(130, 160, 200)
            };
            TextBox T(int x, int y, int w, string val) => new()
            {
                Location = new Point(x, y), Size = new Size(w, 22), Text = val,
                BackColor = Color.FromArgb(28, 34, 48), ForeColor = Color.FromArgb(200, 230, 255),
                BorderStyle = BorderStyle.FixedSingle, Font = new Font("Consolas", 8.5f)
            };

            int y = 16;
            Controls.Add(L("pa_dlg.name", 10, y + 3));
            txtName = T(130, y, 400, _action.Name);
            Controls.Add(txtName);

            y += 34;
            Controls.Add(L("pa_dlg.type", 10, y + 3));
            cmbType = new ComboBox
            {
                Location = new Point(130, y), Size = new Size(200, 22),
                BackColor = Color.FromArgb(28, 34, 48), ForeColor = Color.FromArgb(200, 230, 255),
                FlatStyle = FlatStyle.Flat, Font = new Font("Consolas", 8.5f),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbType.Items.AddRange(new object[]
            {
                L10n.T("pa_dlg.type_exe"),
                L10n.T("pa_dlg.type_bat"),
                L10n.T("pa_dlg.type_ps1")
            });
            cmbType.SelectedIndex = (int)_action.ActionType;
            Controls.Add(cmbType);

            y += 34;
            Controls.Add(L("pa_dlg.file", 10, y + 3));
            txtExe = T(130, y, 358, _action.ExecutablePath);
            var btnBrowse = new Button
            {
                Text = "...", Location = new Point(496, y), Size = new Size(32, 22),
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(30, 50, 80), ForeColor = Color.White
            };
            btnBrowse.FlatAppearance.BorderColor = Color.FromArgb(60, 100, 160);
            btnBrowse.Click += (s, e) =>
            {
                using var ofd = new OpenFileDialog
                    { Filter = "Executables (*.exe;*.bat;*.ps1)|*.exe;*.bat;*.ps1|All (*.*)|*.*" };
                if (ofd.ShowDialog(this) == DialogResult.OK) txtExe.Text = ofd.FileName;
            };
            Controls.AddRange(new Control[] { txtExe, btnBrowse });

            y += 34;
            Controls.Add(L("pa_dlg.args", 10, y + 3));
            txtArgs = T(130, y, 400, _action.Arguments);
            Controls.Add(txtArgs);

            y += 34;
            chkEnabled = new CheckBox
            {
                Text = L10n.T("pa_dlg.enabled"), Location = new Point(130, y),
                Checked = _action.IsEnabled, Font = new Font("Consolas", 8.5f),
                ForeColor = Color.FromArgb(160, 200, 255), AutoSize = true
            };
            Controls.Add(chkEnabled);

            y += 40;
            var btnOk = new Button
            {
                Text = L10n.T("pa_dlg.ok"), Location = new Point(360, y), Size = new Size(80, 28),
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(20, 60, 30),
                ForeColor = Color.FromArgb(100, 220, 130), Font = new Font("Consolas", 8.5f),
                DialogResult = DialogResult.OK
            };
            btnOk.FlatAppearance.BorderColor = Color.FromArgb(50, 120, 70);
            btnOk.Click += (s, e) =>
            {
                _action.Name           = txtName.Text;
                _action.ActionType     = (PostActionType)cmbType.SelectedIndex;
                _action.ExecutablePath = txtExe.Text;
                _action.Arguments      = txtArgs.Text;
                _action.IsEnabled      = chkEnabled.Checked;
            };

            var btnCancel = new Button
            {
                Text = L10n.T("pa_dlg.cancel"), Location = new Point(450, y), Size = new Size(80, 28),
                FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(50, 25, 25),
                ForeColor = Color.FromArgb(200, 100, 100), Font = new Font("Consolas", 8.5f),
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(100, 50, 50);

            Controls.AddRange(new Control[] { btnOk, btnCancel });
            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }
    }
}
