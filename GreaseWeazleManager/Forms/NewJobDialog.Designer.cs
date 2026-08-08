#nullable enable
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using GwCopyPro.Services;

namespace GwCopyPro.Forms
{
    partial class NewJobDialog
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

        private TabControl    tabs             = null!;
        private TabPage       tabMain          = null!;
        private TabPage       tabTracks        = null!;
        private TabPage       tabAdvanced      = null!;
        private TabPage       tabPostActions   = null!;
        private TabPage       tabRepeat        = null!;

        // Main tab
        private Label         lblDevice         = null!;
        private ComboBox      cmbDevice         = null!;
        private Label         lblJobType        = null!;
        private ComboBox      cmbJobType        = null!;
        private Label         lblImageFile      = null!;
        private TextBox       txtImageFile      = null!;
        private Button        btnBrowseImage    = null!;
        private Label         lblDiskFormat     = null!;
        private TextBox       txtFormat         = null!;
        private ComboBox      cmbFmtQuick       = null!;
        private Label         sepCommonOpts        = null!;
        private Label         lblCommonOptsHeader  = null!;
        private Label         lblRevs           = null!;
        private NumericUpDown nudRevs           = null!;
        private Label         lblRevsHint       = null!;
        private Label         lblDensel         = null!;
        private ComboBox      cmbDensel         = null!;
        private Label         lblBitrate        = null!;
        private NumericUpDown nudBitrate        = null!;
        private Label         lblBitrateHint    = null!;
        private Label         sepReadOpts          = null!;
        private Label         lblReadOptsHeader    = null!;
        private CheckBox      chkRetries        = null!;
        private NumericUpDown nudRetries        = null!;
        private CheckBox      chkNoClobber      = null!;
        private CheckBox      chkRaw            = null!;
        private CheckBox      chkReverse        = null!;
        private CheckBox      chkHardSectors    = null!;
        private Label         sepWriteOpts         = null!;
        private Label         lblWriteOptsHeader   = null!;
        private CheckBox      chkErase          = null!;
        private CheckBox      chkVerify         = null!;
        private CheckBox      chkGenTg43        = null!;
        private Label         lblPrecomp        = null!;
        private TextBox       txtPrecomp        = null!;
        private CheckBox      chkReverseW       = null!;
        private CheckBox      chkHardSectorsW   = null!;

        // Tracks tab
        private Label         lblTrackSelHeader = null!;
        private Label         lblTrackInfo      = null!;
        private Label         lblCylinders      = null!;
        private Label         lblCylStart       = null!;
        private NumericUpDown nudStartCyl       = null!;
        private Label         lblCylEnd         = null!;
        private NumericUpDown nudEndCyl         = null!;
        private Label         lblCylHint        = null!;
        private Label         lblHeads          = null!;
        private ComboBox      cmbHead           = null!;
        private Label         lblStep           = null!;
        private NumericUpDown nudStep           = null!;
        private Label         lblStepHint       = null!;
        private CheckBox      chkHSwap          = null!;
        private Label         sepFlippy            = null!;
        private Label         lblFlippyHeader      = null!;
        private CheckBox      chkHead0Off       = null!;
        private NumericUpDown nudHead0Off       = null!;
        private Label         lblH0OffHint      = null!;
        private CheckBox      chkHead1Off       = null!;
        private NumericUpDown nudHead1Off       = null!;
        private Label         lblH1OffHint      = null!;
        private Label         lblTrackSpec      = null!;

        // Advanced tab
        private Label         lblAdvHeader      = null!;
        private Label         lblDrive          = null!;
        private ComboBox      cmbDrive          = null!;
        private Label         lblDriveHint      = null!;
        private Label         lblExtraArgs      = null!;
        private TextBox       txtExtraArgs      = null!;
        private Label         lblTokenNote      = null!;

        // Post-Actions tab
        private Label         lblPaHint         = null!;
        private ListView      lvPostActions     = null!;
        private ColumnHeader  columnHeaderOrd   = null!;
        private ColumnHeader  columnHeaderName  = null!;
        private ColumnHeader  columnHeaderType  = null!;
        private ColumnHeader  columnHeaderExe   = null!;
        private ColumnHeader  columnHeaderArgs  = null!;
        private ColumnHeader  columnHeaderEn    = null!;
        private Button        btnAddAction      = null!;
        private Button        btnEditAction     = null!;
        private Button        btnRemoveActionBtn = null!;
        private Button        btnMoveActionUpBtn = null!;
        private Button        btnMoveActionDownBtn = null!;

        // Repeat tab
        private CheckBox      chkRepetitive     = null!;
        private Label         sepRepeat1           = null!;
        private Label         lblOutputFolder   = null!;
        private TextBox       txtOutputFolder   = null!;
        private Button        btnBrowseFolder   = null!;
        private TextBox       txtFilePattern    = null!;
        private Label         lblPatternHint    = null!;
        private Label         lblStartIndex     = null!;
        private NumericUpDown nudStartIndex     = null!;
        private Label         lblDtFormat       = null!;
        private TextBox       txtDtFormat       = null!;
        private Label         lblDtFormatHint   = null!;
        private Label         sepRepeat2           = null!;
        private Label         lblPatternPreviewCaption = null!;
        private Label         lblPatternPreview = null!;
        private Label         lblRepeatNote     = null!;
        private Label         sepRepeat3           = null!;
        private Label         lblPresetNameCaption = null!;
        private TextBox       txtPresetName     = null!;
        private Label         sepRepeat4           = null!;
        private CheckBox      chkUseGroup       = null!;
        private ComboBox      cmbGroupDevice    = null!;
        private ComboBox      cmbGroupDrive     = null!;
        private Button        btnGroupAdd       = null!;
        private Button        btnGroupRemove    = null!;
        private ListView      lvGroupMembers    = null!;
        private ColumnHeader  columnHeaderDevice = null!;
        private ColumnHeader  columnHeaderDrive  = null!;

        // Bottom bar
        private Label         lblPreview        = null!;
        private Label         sepLine           = null!;
        private Button        btnSavePreset     = null!;
        private Button        btnLoadPreset     = null!;
        private Button        btnOk             = null!;
        private Button        btnCancel         = null!;

        /// <summary>
        /// Required method for Designer support - do not modify the contents of this method with the code editor.
        /// Builds the tab control and all five tab pages' contents directly (the WinForms
        /// Designer's CodeDom reader cannot represent control creation delegated to another
        /// method, so everything static lives here rather than in helper methods). All
        /// Location/Size values are literal integers rather than expressions built from local
        /// constants — the Designer's round-trip serializer cannot correctly re-emit arithmetic
        /// expressions and will silently corrupt them (verified: it drops every term after the
        /// first in a subtraction chain) the next time it resaves this file.
        /// </summary>
        private void InitializeComponent()
        {
            tabs = new TabControl();
            tabMain = new TabPage();
            lblDevice = new Label();
            cmbDevice = new ComboBox();
            lblJobType = new Label();
            cmbJobType = new ComboBox();
            lblImageFile = new Label();
            txtImageFile = new TextBox();
            btnBrowseImage = new Button();
            lblDiskFormat = new Label();
            txtFormat = new TextBox();
            cmbFmtQuick = new ComboBox();
            sepCommonOpts = new Label();
            lblCommonOptsHeader = new Label();
            lblRevs = new Label();
            nudRevs = new NumericUpDown();
            lblRevsHint = new Label();
            lblDensel = new Label();
            cmbDensel = new ComboBox();
            lblBitrate = new Label();
            nudBitrate = new NumericUpDown();
            lblBitrateHint = new Label();
            sepReadOpts = new Label();
            lblReadOptsHeader = new Label();
            chkRetries = new CheckBox();
            nudRetries = new NumericUpDown();
            chkNoClobber = new CheckBox();
            chkRaw = new CheckBox();
            chkReverse = new CheckBox();
            chkHardSectors = new CheckBox();
            sepWriteOpts = new Label();
            lblWriteOptsHeader = new Label();
            chkErase = new CheckBox();
            chkVerify = new CheckBox();
            chkGenTg43 = new CheckBox();
            lblPrecomp = new Label();
            txtPrecomp = new TextBox();
            chkReverseW = new CheckBox();
            chkHardSectorsW = new CheckBox();
            tabTracks = new TabPage();
            lblTrackSelHeader = new Label();
            lblTrackInfo = new Label();
            lblCylinders = new Label();
            lblCylStart = new Label();
            nudStartCyl = new NumericUpDown();
            lblCylEnd = new Label();
            nudEndCyl = new NumericUpDown();
            lblCylHint = new Label();
            lblHeads = new Label();
            cmbHead = new ComboBox();
            lblStep = new Label();
            nudStep = new NumericUpDown();
            lblStepHint = new Label();
            chkHSwap = new CheckBox();
            sepFlippy = new Label();
            lblFlippyHeader = new Label();
            chkHead0Off = new CheckBox();
            nudHead0Off = new NumericUpDown();
            lblH0OffHint = new Label();
            chkHead1Off = new CheckBox();
            nudHead1Off = new NumericUpDown();
            lblH1OffHint = new Label();
            lblTrackSpec = new Label();
            tabAdvanced = new TabPage();
            lblAdvHeader = new Label();
            lblDrive = new Label();
            cmbDrive = new ComboBox();
            lblDriveHint = new Label();
            lblExtraArgs = new Label();
            txtExtraArgs = new TextBox();
            lblTokenNote = new Label();
            tabPostActions = new TabPage();
            lblPaHint = new Label();
            lvPostActions = new ListView();
            columnHeaderOrd = new ColumnHeader();
            columnHeaderName = new ColumnHeader();
            columnHeaderType = new ColumnHeader();
            columnHeaderExe = new ColumnHeader();
            columnHeaderArgs = new ColumnHeader();
            columnHeaderEn = new ColumnHeader();
            btnAddAction = new Button();
            btnEditAction = new Button();
            btnRemoveActionBtn = new Button();
            btnMoveActionUpBtn = new Button();
            btnMoveActionDownBtn = new Button();
            tabRepeat = new TabPage();
            chkRepetitive = new CheckBox();
            sepRepeat1 = new Label();
            lblOutputFolder = new Label();
            txtOutputFolder = new TextBox();
            btnBrowseFolder = new Button();
            txtFilePattern = new TextBox();
            lblPatternHint = new Label();
            lblStartIndex = new Label();
            nudStartIndex = new NumericUpDown();
            lblDtFormat = new Label();
            txtDtFormat = new TextBox();
            lblDtFormatHint = new Label();
            sepRepeat2 = new Label();
            lblPatternPreviewCaption = new Label();
            lblPatternPreview = new Label();
            lblRepeatNote = new Label();
            sepRepeat3 = new Label();
            lblPresetNameCaption = new Label();
            txtPresetName = new TextBox();
            sepRepeat4 = new Label();
            chkUseGroup = new CheckBox();
            cmbGroupDevice = new ComboBox();
            cmbGroupDrive = new ComboBox();
            btnGroupAdd = new Button();
            btnGroupRemove = new Button();
            lvGroupMembers = new ListView();
            columnHeaderDevice = new ColumnHeader();
            columnHeaderDrive = new ColumnHeader();
            lblPreview = new Label();
            sepLine = new Label();
            btnSavePreset = new Button();
            btnLoadPreset = new Button();
            btnOk = new Button();
            btnCancel = new Button();
            tabs.SuspendLayout();
            tabMain.SuspendLayout();
            ((ISupportInitialize)nudRevs).BeginInit();
            ((ISupportInitialize)nudBitrate).BeginInit();
            ((ISupportInitialize)nudRetries).BeginInit();
            tabTracks.SuspendLayout();
            ((ISupportInitialize)nudStartCyl).BeginInit();
            ((ISupportInitialize)nudEndCyl).BeginInit();
            ((ISupportInitialize)nudStep).BeginInit();
            ((ISupportInitialize)nudHead0Off).BeginInit();
            ((ISupportInitialize)nudHead1Off).BeginInit();
            tabAdvanced.SuspendLayout();
            tabPostActions.SuspendLayout();
            tabRepeat.SuspendLayout();
            ((ISupportInitialize)nudStartIndex).BeginInit();
            SuspendLayout();
            // 
            // tabs
            // 
            tabs.Controls.Add(tabMain);
            tabs.Controls.Add(tabTracks);
            tabs.Controls.Add(tabAdvanced);
            tabs.Controls.Add(tabPostActions);
            tabs.Controls.Add(tabRepeat);
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.ItemSize = new Size(190, 32);
            tabs.Location = new Point(10, 10);
            tabs.Name = "tabs";
            tabs.SelectedIndex = 0;
            tabs.Size = new Size(880, 630);
            tabs.SizeMode = TabSizeMode.Fixed;
            tabs.TabIndex = 0;
            tabs.DrawItem += Tabs_DrawItem;
            // 
            // tabMain
            // 
            tabMain.BackColor = Color.FromArgb(22, 26, 36);
            tabMain.Controls.Add(lblDevice);
            tabMain.Controls.Add(cmbDevice);
            tabMain.Controls.Add(lblJobType);
            tabMain.Controls.Add(cmbJobType);
            tabMain.Controls.Add(lblImageFile);
            tabMain.Controls.Add(txtImageFile);
            tabMain.Controls.Add(btnBrowseImage);
            tabMain.Controls.Add(lblDiskFormat);
            tabMain.Controls.Add(txtFormat);
            tabMain.Controls.Add(cmbFmtQuick);
            tabMain.Controls.Add(sepCommonOpts);
            tabMain.Controls.Add(lblCommonOptsHeader);
            tabMain.Controls.Add(lblRevs);
            tabMain.Controls.Add(nudRevs);
            tabMain.Controls.Add(lblRevsHint);
            tabMain.Controls.Add(lblDensel);
            tabMain.Controls.Add(cmbDensel);
            tabMain.Controls.Add(lblBitrate);
            tabMain.Controls.Add(nudBitrate);
            tabMain.Controls.Add(lblBitrateHint);
            tabMain.Controls.Add(sepReadOpts);
            tabMain.Controls.Add(lblReadOptsHeader);
            tabMain.Controls.Add(chkRetries);
            tabMain.Controls.Add(nudRetries);
            tabMain.Controls.Add(chkNoClobber);
            tabMain.Controls.Add(chkRaw);
            tabMain.Controls.Add(chkReverse);
            tabMain.Controls.Add(chkHardSectors);
            tabMain.Controls.Add(sepWriteOpts);
            tabMain.Controls.Add(lblWriteOptsHeader);
            tabMain.Controls.Add(chkErase);
            tabMain.Controls.Add(chkVerify);
            tabMain.Controls.Add(chkGenTg43);
            tabMain.Controls.Add(lblPrecomp);
            tabMain.Controls.Add(txtPrecomp);
            tabMain.Controls.Add(chkReverseW);
            tabMain.Controls.Add(chkHardSectorsW);
            tabMain.Location = new Point(4, 36);
            tabMain.Name = "tabMain";
            tabMain.Size = new Size(872, 590);
            tabMain.TabIndex = 0;
            tabMain.Text = "Main Settings";
            // 
            // lblDevice
            // 
            lblDevice.AutoSize = true;
            lblDevice.BackColor = Color.Transparent;
            lblDevice.Font = new Font("Consolas", 8F);
            lblDevice.ForeColor = Color.FromArgb(130, 160, 200);
            lblDevice.Location = new Point(10, 17);
            lblDevice.Name = "lblDevice";
            lblDevice.Size = new Size(49, 13);
            lblDevice.TabIndex = 0;
            lblDevice.Text = "Device:";
            // 
            // cmbDevice
            // 
            cmbDevice.BackColor = Color.FromArgb(28, 34, 48);
            cmbDevice.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDevice.FlatStyle = FlatStyle.Flat;
            cmbDevice.Font = new Font("Consolas", 8.5F);
            cmbDevice.ForeColor = Color.FromArgb(200, 230, 255);
            cmbDevice.Location = new Point(150, 14);
            cmbDevice.Name = "cmbDevice";
            cmbDevice.Size = new Size(360, 21);
            cmbDevice.TabIndex = 1;
            // 
            // lblJobType
            // 
            lblJobType.AutoSize = true;
            lblJobType.BackColor = Color.Transparent;
            lblJobType.Font = new Font("Consolas", 8F);
            lblJobType.ForeColor = Color.FromArgb(130, 160, 200);
            lblJobType.Location = new Point(10, 51);
            lblJobType.Name = "lblJobType";
            lblJobType.Size = new Size(61, 13);
            lblJobType.TabIndex = 2;
            lblJobType.Text = "Job Type:";
            // 
            // cmbJobType
            // 
            cmbJobType.BackColor = Color.FromArgb(28, 34, 48);
            cmbJobType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbJobType.FlatStyle = FlatStyle.Flat;
            cmbJobType.Font = new Font("Consolas", 8.5F);
            cmbJobType.ForeColor = Color.FromArgb(200, 230, 255);
            cmbJobType.Items.AddRange(new object[] { "Read (disk → image)", "Write (image → disk)" });
            cmbJobType.Location = new Point(150, 48);
            cmbJobType.Name = "cmbJobType";
            cmbJobType.Size = new Size(220, 21);
            cmbJobType.TabIndex = 3;
            cmbJobType.SelectedIndexChanged += OnParamChanged;
            // 
            // lblImageFile
            // 
            lblImageFile.AutoSize = true;
            lblImageFile.BackColor = Color.Transparent;
            lblImageFile.Font = new Font("Consolas", 8F);
            lblImageFile.ForeColor = Color.FromArgb(130, 160, 200);
            lblImageFile.Location = new Point(10, 85);
            lblImageFile.Name = "lblImageFile";
            lblImageFile.Size = new Size(73, 13);
            lblImageFile.TabIndex = 4;
            lblImageFile.Text = "Image File:";
            // 
            // txtImageFile
            // 
            txtImageFile.BackColor = Color.FromArgb(28, 34, 48);
            txtImageFile.BorderStyle = BorderStyle.FixedSingle;
            txtImageFile.Font = new Font("Consolas", 8.5F);
            txtImageFile.ForeColor = Color.FromArgb(200, 230, 255);
            txtImageFile.Location = new Point(150, 82);
            txtImageFile.Name = "txtImageFile";
            txtImageFile.Size = new Size(538, 21);
            txtImageFile.TabIndex = 5;
            txtImageFile.TextChanged += OnParamChanged;
            // 
            // btnBrowseImage
            // 
            btnBrowseImage.BackColor = Color.FromArgb(30, 50, 80);
            btnBrowseImage.FlatAppearance.BorderColor = Color.FromArgb(60, 100, 160);
            btnBrowseImage.FlatStyle = FlatStyle.Flat;
            btnBrowseImage.Font = new Font("Consolas", 8F);
            btnBrowseImage.ForeColor = Color.White;
            btnBrowseImage.Location = new Point(696, 82);
            btnBrowseImage.Name = "btnBrowseImage";
            btnBrowseImage.Size = new Size(30, 22);
            btnBrowseImage.TabIndex = 6;
            btnBrowseImage.Text = "...";
            btnBrowseImage.UseVisualStyleBackColor = false;
            btnBrowseImage.Click += BtnBrowse_Click;
            // 
            // lblDiskFormat
            // 
            lblDiskFormat.AutoSize = true;
            lblDiskFormat.BackColor = Color.Transparent;
            lblDiskFormat.Font = new Font("Consolas", 8F);
            lblDiskFormat.ForeColor = Color.FromArgb(130, 160, 200);
            lblDiskFormat.Location = new Point(10, 119);
            lblDiskFormat.Name = "lblDiskFormat";
            lblDiskFormat.Size = new Size(79, 13);
            lblDiskFormat.TabIndex = 7;
            lblDiskFormat.Text = "Disk Format:";
            // 
            // txtFormat
            // 
            txtFormat.BackColor = Color.FromArgb(28, 34, 48);
            txtFormat.BorderStyle = BorderStyle.FixedSingle;
            txtFormat.Font = new Font("Consolas", 8.5F);
            txtFormat.ForeColor = Color.FromArgb(200, 230, 255);
            txtFormat.Location = new Point(150, 116);
            txtFormat.Name = "txtFormat";
            txtFormat.Size = new Size(180, 21);
            txtFormat.TabIndex = 8;
            txtFormat.TextChanged += OnParamChanged;
            // 
            // cmbFmtQuick
            // 
            cmbFmtQuick.BackColor = Color.FromArgb(28, 34, 48);
            cmbFmtQuick.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFmtQuick.FlatStyle = FlatStyle.Flat;
            cmbFmtQuick.Font = new Font("Consolas", 8.5F);
            cmbFmtQuick.ForeColor = Color.FromArgb(200, 230, 255);
            cmbFmtQuick.Items.AddRange(new object[] { "ibm.1440", "ibm.720", "ibm.1200", "ibm.360", "ibm.180", "ibm.320", "ibm.800", "ibm.2880", "amiga.amigados", "amiga.amigados-hd", "atarist.360", "atarist.400", "atarist.720", "atarist.800", "atari.90", "atari.130", "atari.180", "atari.360", "commodore.1541", "commodore.1571", "commodore.1581", "apple2.525.ss.sd.35", "apple2.525.ss.sd.40", "mac.400", "mac.800", "msx.1", "msx.2", "pc98.2hd", "pc98.2dd", "pc98.2d", "acorn.adfs.s", "acorn.adfs.m", "acorn.adfs.l", "acorn.adfs.d", "acorn.adfs.e", "acorn.adfs.f", "dec.rx50", "dec.rx33", "ensoniq.mirage", "ensoniq.esq1", "gem.1", "dragon.40", "coco.35", "zx.trdos.ds80" });
            cmbFmtQuick.Location = new Point(340, 116);
            cmbFmtQuick.Name = "cmbFmtQuick";
            cmbFmtQuick.Size = new Size(390, 21);
            cmbFmtQuick.TabIndex = 9;
            cmbFmtQuick.SelectedIndexChanged += CmbFmtQuick_SelectedIndexChanged;
            // 
            // sepCommonOpts
            // 
            sepCommonOpts.BackColor = Color.FromArgb(40, 60, 90);
            sepCommonOpts.Location = new Point(10, 160);
            sepCommonOpts.Name = "sepCommonOpts";
            sepCommonOpts.Size = new Size(750, 1);
            sepCommonOpts.TabIndex = 10;
            // 
            // lblCommonOptsHeader
            // 
            lblCommonOptsHeader.AutoSize = true;
            lblCommonOptsHeader.BackColor = Color.Transparent;
            lblCommonOptsHeader.Font = new Font("Consolas", 8.5F, FontStyle.Bold);
            lblCommonOptsHeader.ForeColor = Color.FromArgb(100, 160, 220);
            lblCommonOptsHeader.Location = new Point(10, 168);
            lblCommonOptsHeader.Name = "lblCommonOptsHeader";
            lblCommonOptsHeader.Size = new Size(133, 14);
            lblCommonOptsHeader.TabIndex = 11;
            lblCommonOptsHeader.Text = "─ Common Options ─";
            // 
            // lblRevs
            // 
            lblRevs.AutoSize = true;
            lblRevs.BackColor = Color.Transparent;
            lblRevs.Font = new Font("Consolas", 8F);
            lblRevs.ForeColor = Color.FromArgb(130, 160, 200);
            lblRevs.Location = new Point(10, 193);
            lblRevs.Name = "lblRevs";
            lblRevs.Size = new Size(133, 13);
            lblRevs.TabIndex = 12;
            lblRevs.Text = "Revolutions (--revs):";
            // 
            // nudRevs
            // 
            nudRevs.BackColor = Color.FromArgb(28, 34, 48);
            nudRevs.BorderStyle = BorderStyle.FixedSingle;
            nudRevs.Font = new Font("Consolas", 8.5F);
            nudRevs.ForeColor = Color.FromArgb(200, 230, 255);
            nudRevs.Location = new Point(220, 190);
            nudRevs.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            nudRevs.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudRevs.Name = "nudRevs";
            nudRevs.Size = new Size(70, 21);
            nudRevs.TabIndex = 13;
            nudRevs.Value = new decimal(new int[] { 1, 0, 0, 0 });
            nudRevs.ValueChanged += OnParamChanged;
            // 
            // lblRevsHint
            // 
            lblRevsHint.AutoSize = true;
            lblRevsHint.BackColor = Color.Transparent;
            lblRevsHint.Font = new Font("Consolas", 8F);
            lblRevsHint.ForeColor = Color.FromArgb(130, 160, 200);
            lblRevsHint.Location = new Point(300, 193);
            lblRevsHint.Name = "lblRevsHint";
            lblRevsHint.Size = new Size(199, 13);
            lblRevsHint.TabIndex = 14;
            lblRevsHint.Text = "(flux revs per track, default 1)";
            // 
            // lblDensel
            // 
            lblDensel.AutoSize = true;
            lblDensel.BackColor = Color.Transparent;
            lblDensel.Font = new Font("Consolas", 8F);
            lblDensel.ForeColor = Color.FromArgb(130, 160, 200);
            lblDensel.Location = new Point(10, 223);
            lblDensel.Name = "lblDensel";
            lblDensel.Size = new Size(121, 13);
            lblDensel.TabIndex = 15;
            lblDensel.Text = "Density (--densel):";
            // 
            // cmbDensel
            // 
            cmbDensel.BackColor = Color.FromArgb(28, 34, 48);
            cmbDensel.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDensel.FlatStyle = FlatStyle.Flat;
            cmbDensel.Font = new Font("Consolas", 8.5F);
            cmbDensel.ForeColor = Color.FromArgb(200, 230, 255);
            cmbDensel.Items.AddRange(new object[] { "(auto)", "hd", "dd", "ed" });
            cmbDensel.Location = new Point(220, 220);
            cmbDensel.Name = "cmbDensel";
            cmbDensel.Size = new Size(110, 21);
            cmbDensel.TabIndex = 16;
            cmbDensel.SelectedIndexChanged += OnParamChanged;
            // 
            // lblBitrate
            // 
            lblBitrate.AutoSize = true;
            lblBitrate.BackColor = Color.Transparent;
            lblBitrate.Font = new Font("Consolas", 8F);
            lblBitrate.ForeColor = Color.FromArgb(130, 160, 200);
            lblBitrate.Location = new Point(340, 223);
            lblBitrate.Name = "lblBitrate";
            lblBitrate.Size = new Size(139, 13);
            lblBitrate.TabIndex = 17;
            lblBitrate.Text = "  Bitrate (--bitrate):";
            // 
            // nudBitrate
            // 
            nudBitrate.BackColor = Color.FromArgb(28, 34, 48);
            nudBitrate.BorderStyle = BorderStyle.FixedSingle;
            nudBitrate.Font = new Font("Consolas", 8.5F);
            nudBitrate.ForeColor = Color.FromArgb(200, 230, 255);
            nudBitrate.Location = new Point(500, 220);
            nudBitrate.Maximum = new decimal(new int[] { 2000000, 0, 0, 0 });
            nudBitrate.Name = "nudBitrate";
            nudBitrate.Size = new Size(100, 21);
            nudBitrate.TabIndex = 18;
            nudBitrate.ValueChanged += OnParamChanged;
            // 
            // lblBitrateHint
            // 
            lblBitrateHint.AutoSize = true;
            lblBitrateHint.BackColor = Color.Transparent;
            lblBitrateHint.Font = new Font("Consolas", 8F);
            lblBitrateHint.ForeColor = Color.FromArgb(130, 160, 200);
            lblBitrateHint.Location = new Point(610, 223);
            lblBitrateHint.Name = "lblBitrateHint";
            lblBitrateHint.Size = new Size(55, 13);
            lblBitrateHint.TabIndex = 19;
            lblBitrateHint.Text = "(0=auto)";
            // 
            // sepReadOpts
            // 
            sepReadOpts.BackColor = Color.FromArgb(40, 60, 90);
            sepReadOpts.Location = new Point(10, 260);
            sepReadOpts.Name = "sepReadOpts";
            sepReadOpts.Size = new Size(750, 1);
            sepReadOpts.TabIndex = 20;
            // 
            // lblReadOptsHeader
            // 
            lblReadOptsHeader.AutoSize = true;
            lblReadOptsHeader.BackColor = Color.Transparent;
            lblReadOptsHeader.Font = new Font("Consolas", 8.5F, FontStyle.Bold);
            lblReadOptsHeader.ForeColor = Color.FromArgb(60, 160, 240);
            lblReadOptsHeader.Location = new Point(10, 268);
            lblReadOptsHeader.Name = "lblReadOptsHeader";
            lblReadOptsHeader.Size = new Size(119, 14);
            lblReadOptsHeader.TabIndex = 21;
            lblReadOptsHeader.Text = "─ Read Options ─";
            // 
            // chkRetries
            // 
            chkRetries.AutoSize = true;
            chkRetries.BackColor = Color.Transparent;
            chkRetries.Font = new Font("Consolas", 8.5F);
            chkRetries.ForeColor = Color.FromArgb(160, 200, 255);
            chkRetries.Location = new Point(10, 292);
            chkRetries.Name = "chkRetries";
            chkRetries.Size = new Size(110, 18);
            chkRetries.TabIndex = 22;
            chkRetries.Text = "--retries N:";
            chkRetries.UseVisualStyleBackColor = false;
            chkRetries.CheckedChanged += ChkRetries_CheckedChanged;
            // 
            // nudRetries
            // 
            nudRetries.BackColor = Color.FromArgb(28, 34, 48);
            nudRetries.BorderStyle = BorderStyle.FixedSingle;
            nudRetries.Enabled = false;
            nudRetries.Font = new Font("Consolas", 8.5F);
            nudRetries.ForeColor = Color.FromArgb(200, 230, 255);
            nudRetries.Location = new Point(145, 290);
            nudRetries.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            nudRetries.Name = "nudRetries";
            nudRetries.Size = new Size(70, 21);
            nudRetries.TabIndex = 23;
            nudRetries.Value = new decimal(new int[] { 3, 0, 0, 0 });
            nudRetries.ValueChanged += OnParamChanged;
            // 
            // chkNoClobber
            // 
            chkNoClobber.AutoSize = true;
            chkNoClobber.BackColor = Color.Transparent;
            chkNoClobber.Font = new Font("Consolas", 8.5F);
            chkNoClobber.ForeColor = Color.FromArgb(160, 200, 255);
            chkNoClobber.Location = new Point(240, 292);
            chkNoClobber.Name = "chkNoClobber";
            chkNoClobber.Size = new Size(110, 18);
            chkNoClobber.TabIndex = 24;
            chkNoClobber.Text = "--no-clobber";
            chkNoClobber.UseVisualStyleBackColor = false;
            chkNoClobber.CheckedChanged += OnParamChanged;
            // 
            // chkRaw
            // 
            chkRaw.AutoSize = true;
            chkRaw.BackColor = Color.Transparent;
            chkRaw.Font = new Font("Consolas", 8.5F);
            chkRaw.ForeColor = Color.FromArgb(160, 200, 255);
            chkRaw.Location = new Point(400, 292);
            chkRaw.Name = "chkRaw";
            chkRaw.Size = new Size(61, 18);
            chkRaw.TabIndex = 25;
            chkRaw.Text = "--raw";
            chkRaw.UseVisualStyleBackColor = false;
            chkRaw.CheckedChanged += OnParamChanged;
            // 
            // chkReverse
            // 
            chkReverse.AutoSize = true;
            chkReverse.BackColor = Color.Transparent;
            chkReverse.Font = new Font("Consolas", 8.5F);
            chkReverse.ForeColor = Color.FromArgb(160, 200, 255);
            chkReverse.Location = new Point(10, 320);
            chkReverse.Name = "chkReverse";
            chkReverse.Size = new Size(201, 18);
            chkReverse.TabIndex = 26;
            chkReverse.Text = "--reverse (flippy side B)";
            chkReverse.UseVisualStyleBackColor = false;
            chkReverse.CheckedChanged += OnParamChanged;
            // 
            // chkHardSectors
            // 
            chkHardSectors.AutoSize = true;
            chkHardSectors.BackColor = Color.Transparent;
            chkHardSectors.Font = new Font("Consolas", 8.5F);
            chkHardSectors.ForeColor = Color.FromArgb(160, 200, 255);
            chkHardSectors.Location = new Point(280, 320);
            chkHardSectors.Name = "chkHardSectors";
            chkHardSectors.Size = new Size(124, 18);
            chkHardSectors.TabIndex = 27;
            chkHardSectors.Text = "--hard-sectors";
            chkHardSectors.UseVisualStyleBackColor = false;
            chkHardSectors.CheckedChanged += OnParamChanged;
            // 
            // sepWriteOpts
            // 
            sepWriteOpts.BackColor = Color.FromArgb(40, 60, 90);
            sepWriteOpts.Location = new Point(10, 354);
            sepWriteOpts.Name = "sepWriteOpts";
            sepWriteOpts.Size = new Size(750, 1);
            sepWriteOpts.TabIndex = 28;
            // 
            // lblWriteOptsHeader
            // 
            lblWriteOptsHeader.AutoSize = true;
            lblWriteOptsHeader.BackColor = Color.Transparent;
            lblWriteOptsHeader.Font = new Font("Consolas", 8.5F, FontStyle.Bold);
            lblWriteOptsHeader.ForeColor = Color.FromArgb(220, 140, 40);
            lblWriteOptsHeader.Location = new Point(10, 362);
            lblWriteOptsHeader.Name = "lblWriteOptsHeader";
            lblWriteOptsHeader.Size = new Size(126, 14);
            lblWriteOptsHeader.TabIndex = 29;
            lblWriteOptsHeader.Text = "─ Write Options ─";
            // 
            // chkErase
            // 
            chkErase.AutoSize = true;
            chkErase.BackColor = Color.Transparent;
            chkErase.Font = new Font("Consolas", 8.5F);
            chkErase.ForeColor = Color.FromArgb(160, 200, 255);
            chkErase.Location = new Point(10, 386);
            chkErase.Name = "chkErase";
            chkErase.Size = new Size(75, 18);
            chkErase.TabIndex = 30;
            chkErase.Text = "--erase";
            chkErase.UseVisualStyleBackColor = false;
            chkErase.CheckedChanged += OnParamChanged;
            // 
            // chkVerify
            // 
            chkVerify.AutoSize = true;
            chkVerify.BackColor = Color.Transparent;
            chkVerify.Font = new Font("Consolas", 8.5F);
            chkVerify.ForeColor = Color.FromArgb(160, 200, 255);
            chkVerify.Location = new Point(120, 386);
            chkVerify.Name = "chkVerify";
            chkVerify.Size = new Size(82, 18);
            chkVerify.TabIndex = 31;
            chkVerify.Text = "--verify";
            chkVerify.UseVisualStyleBackColor = false;
            chkVerify.CheckedChanged += OnParamChanged;
            // 
            // chkGenTg43
            // 
            chkGenTg43.AutoSize = true;
            chkGenTg43.BackColor = Color.Transparent;
            chkGenTg43.Font = new Font("Consolas", 8.5F);
            chkGenTg43.ForeColor = Color.FromArgb(160, 200, 255);
            chkGenTg43.Location = new Point(240, 386);
            chkGenTg43.Name = "chkGenTg43";
            chkGenTg43.Size = new Size(180, 18);
            chkGenTg43.TabIndex = 32;
            chkGenTg43.Text = "--gen-tg43 (8\" drives)";
            chkGenTg43.UseVisualStyleBackColor = false;
            chkGenTg43.CheckedChanged += OnParamChanged;
            // 
            // lblPrecomp
            // 
            lblPrecomp.AutoSize = true;
            lblPrecomp.BackColor = Color.Transparent;
            lblPrecomp.Font = new Font("Consolas", 8F);
            lblPrecomp.ForeColor = Color.FromArgb(130, 160, 200);
            lblPrecomp.Location = new Point(10, 415);
            lblPrecomp.Name = "lblPrecomp";
            lblPrecomp.Size = new Size(67, 13);
            lblPrecomp.TabIndex = 33;
            lblPrecomp.Text = "--precomp:";
            // 
            // txtPrecomp
            // 
            txtPrecomp.BackColor = Color.FromArgb(28, 34, 48);
            txtPrecomp.BorderStyle = BorderStyle.FixedSingle;
            txtPrecomp.Font = new Font("Consolas", 8.5F);
            txtPrecomp.ForeColor = Color.FromArgb(200, 230, 255);
            txtPrecomp.Location = new Point(100, 412);
            txtPrecomp.Name = "txtPrecomp";
            txtPrecomp.Size = new Size(100, 21);
            txtPrecomp.TabIndex = 34;
            txtPrecomp.TextChanged += OnParamChanged;
            // 
            // chkReverseW
            // 
            chkReverseW.AutoSize = true;
            chkReverseW.BackColor = Color.Transparent;
            chkReverseW.Font = new Font("Consolas", 8.5F);
            chkReverseW.ForeColor = Color.FromArgb(160, 200, 255);
            chkReverseW.Location = new Point(220, 414);
            chkReverseW.Name = "chkReverseW";
            chkReverseW.Size = new Size(89, 18);
            chkReverseW.TabIndex = 35;
            chkReverseW.Text = "--reverse";
            chkReverseW.UseVisualStyleBackColor = false;
            chkReverseW.CheckedChanged += OnParamChanged;
            // 
            // chkHardSectorsW
            // 
            chkHardSectorsW.AutoSize = true;
            chkHardSectorsW.BackColor = Color.Transparent;
            chkHardSectorsW.Font = new Font("Consolas", 8.5F);
            chkHardSectorsW.ForeColor = Color.FromArgb(160, 200, 255);
            chkHardSectorsW.Location = new Point(350, 414);
            chkHardSectorsW.Name = "chkHardSectorsW";
            chkHardSectorsW.Size = new Size(124, 18);
            chkHardSectorsW.TabIndex = 36;
            chkHardSectorsW.Text = "--hard-sectors";
            chkHardSectorsW.UseVisualStyleBackColor = false;
            chkHardSectorsW.CheckedChanged += OnParamChanged;
            // 
            // tabTracks
            // 
            tabTracks.BackColor = Color.FromArgb(22, 26, 36);
            tabTracks.Controls.Add(lblTrackSelHeader);
            tabTracks.Controls.Add(lblTrackInfo);
            tabTracks.Controls.Add(lblCylinders);
            tabTracks.Controls.Add(lblCylStart);
            tabTracks.Controls.Add(nudStartCyl);
            tabTracks.Controls.Add(lblCylEnd);
            tabTracks.Controls.Add(nudEndCyl);
            tabTracks.Controls.Add(lblCylHint);
            tabTracks.Controls.Add(lblHeads);
            tabTracks.Controls.Add(cmbHead);
            tabTracks.Controls.Add(lblStep);
            tabTracks.Controls.Add(nudStep);
            tabTracks.Controls.Add(lblStepHint);
            tabTracks.Controls.Add(chkHSwap);
            tabTracks.Controls.Add(sepFlippy);
            tabTracks.Controls.Add(lblFlippyHeader);
            tabTracks.Controls.Add(chkHead0Off);
            tabTracks.Controls.Add(nudHead0Off);
            tabTracks.Controls.Add(lblH0OffHint);
            tabTracks.Controls.Add(chkHead1Off);
            tabTracks.Controls.Add(nudHead1Off);
            tabTracks.Controls.Add(lblH1OffHint);
            tabTracks.Controls.Add(lblTrackSpec);
            tabTracks.Location = new Point(4, 36);
            tabTracks.Name = "tabTracks";
            tabTracks.Size = new Size(872, 590);
            tabTracks.TabIndex = 1;
            tabTracks.Text = "Track Selection";
            // 
            // lblTrackSelHeader
            // 
            lblTrackSelHeader.AutoSize = true;
            lblTrackSelHeader.BackColor = Color.Transparent;
            lblTrackSelHeader.Font = new Font("Consolas", 8.5F, FontStyle.Bold);
            lblTrackSelHeader.ForeColor = Color.FromArgb(160, 200, 255);
            lblTrackSelHeader.Location = new Point(10, 14);
            lblTrackSelHeader.Name = "lblTrackSelHeader";
            lblTrackSelHeader.Size = new Size(273, 14);
            lblTrackSelHeader.TabIndex = 0;
            lblTrackSelHeader.Text = "Track Selection  (builds --tracks=...)";
            // 
            // lblTrackInfo
            // 
            lblTrackInfo.BackColor = Color.Transparent;
            lblTrackInfo.Font = new Font("Consolas", 7.5F);
            lblTrackInfo.ForeColor = Color.FromArgb(90, 130, 180);
            lblTrackInfo.Location = new Point(10, 40);
            lblTrackInfo.Name = "lblTrackInfo";
            lblTrackInfo.Size = new Size(760, 38);
            lblTrackInfo.TabIndex = 1;
            lblTrackInfo.Text = "gw.exe v0.24+ uses  --tracks=c=START-END:h=HEAD[:step=N][:hswap][:h0.off=N][:h1.off=N]\nThe old --scyl/--ecyl/--shead/--ehead flags are no longer valid.";
            // 
            // lblCylinders
            // 
            lblCylinders.AutoSize = true;
            lblCylinders.BackColor = Color.Transparent;
            lblCylinders.Font = new Font("Consolas", 8F);
            lblCylinders.ForeColor = Color.FromArgb(130, 160, 200);
            lblCylinders.Location = new Point(10, 89);
            lblCylinders.Name = "lblCylinders";
            lblCylinders.Size = new Size(97, 13);
            lblCylinders.TabIndex = 2;
            lblCylinders.Text = "Cylinders (c=):";
            // 
            // lblCylStart
            // 
            lblCylStart.AutoSize = true;
            lblCylStart.BackColor = Color.Transparent;
            lblCylStart.Font = new Font("Consolas", 8F);
            lblCylStart.ForeColor = Color.FromArgb(130, 160, 200);
            lblCylStart.Location = new Point(180, 89);
            lblCylStart.Name = "lblCylStart";
            lblCylStart.Size = new Size(43, 13);
            lblCylStart.TabIndex = 3;
            lblCylStart.Text = "Start:";
            // 
            // nudStartCyl
            // 
            nudStartCyl.BackColor = Color.FromArgb(28, 34, 48);
            nudStartCyl.BorderStyle = BorderStyle.FixedSingle;
            nudStartCyl.Font = new Font("Consolas", 8.5F);
            nudStartCyl.ForeColor = Color.FromArgb(200, 230, 255);
            nudStartCyl.Location = new Point(230, 86);
            nudStartCyl.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            nudStartCyl.Name = "nudStartCyl";
            nudStartCyl.Size = new Size(75, 21);
            nudStartCyl.TabIndex = 4;
            nudStartCyl.ValueChanged += OnParamChanged;
            // 
            // lblCylEnd
            // 
            lblCylEnd.AutoSize = true;
            lblCylEnd.BackColor = Color.Transparent;
            lblCylEnd.Font = new Font("Consolas", 8F);
            lblCylEnd.ForeColor = Color.FromArgb(130, 160, 200);
            lblCylEnd.Location = new Point(320, 89);
            lblCylEnd.Name = "lblCylEnd";
            lblCylEnd.Size = new Size(31, 13);
            lblCylEnd.TabIndex = 5;
            lblCylEnd.Text = "End:";
            // 
            // nudEndCyl
            // 
            nudEndCyl.BackColor = Color.FromArgb(28, 34, 48);
            nudEndCyl.BorderStyle = BorderStyle.FixedSingle;
            nudEndCyl.Font = new Font("Consolas", 8.5F);
            nudEndCyl.ForeColor = Color.FromArgb(200, 230, 255);
            nudEndCyl.Location = new Point(360, 86);
            nudEndCyl.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            nudEndCyl.Name = "nudEndCyl";
            nudEndCyl.Size = new Size(75, 21);
            nudEndCyl.TabIndex = 6;
            nudEndCyl.Value = new decimal(new int[] { 79, 0, 0, 0 });
            nudEndCyl.ValueChanged += OnParamChanged;
            // 
            // lblCylHint
            // 
            lblCylHint.AutoSize = true;
            lblCylHint.BackColor = Color.Transparent;
            lblCylHint.Font = new Font("Consolas", 8F);
            lblCylHint.ForeColor = Color.FromArgb(130, 160, 200);
            lblCylHint.Location = new Point(450, 89);
            lblCylHint.Name = "lblCylHint";
            lblCylHint.Size = new Size(163, 13);
            lblCylHint.TabIndex = 7;
            lblCylHint.Text = "(0–79 = standard 80-track)";
            // 
            // lblHeads
            // 
            lblHeads.AutoSize = true;
            lblHeads.BackColor = Color.Transparent;
            lblHeads.Font = new Font("Consolas", 8F);
            lblHeads.ForeColor = Color.FromArgb(130, 160, 200);
            lblHeads.Location = new Point(10, 123);
            lblHeads.Name = "lblHeads";
            lblHeads.Size = new Size(73, 13);
            lblHeads.TabIndex = 8;
            lblHeads.Text = "Heads (h=):";
            // 
            // cmbHead
            // 
            cmbHead.BackColor = Color.FromArgb(28, 34, 48);
            cmbHead.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbHead.FlatStyle = FlatStyle.Flat;
            cmbHead.Font = new Font("Consolas", 8.5F);
            cmbHead.ForeColor = Color.FromArgb(200, 230, 255);
            cmbHead.Items.AddRange(new object[] { "Both sides  (h=0-1)", "Head 0 only  (h=0)", "Head 1 only  (h=1)" });
            cmbHead.Location = new Point(180, 120);
            cmbHead.Name = "cmbHead";
            cmbHead.Size = new Size(220, 21);
            cmbHead.TabIndex = 9;
            cmbHead.SelectedIndexChanged += OnParamChanged;
            // 
            // lblStep
            // 
            lblStep.AutoSize = true;
            lblStep.BackColor = Color.Transparent;
            lblStep.Font = new Font("Consolas", 8F);
            lblStep.ForeColor = Color.FromArgb(130, 160, 200);
            lblStep.Location = new Point(10, 157);
            lblStep.Name = "lblStep";
            lblStep.Size = new Size(85, 13);
            lblStep.TabIndex = 10;
            lblStep.Text = "Step (step=):";
            // 
            // nudStep
            // 
            nudStep.BackColor = Color.FromArgb(28, 34, 48);
            nudStep.BorderStyle = BorderStyle.FixedSingle;
            nudStep.Font = new Font("Consolas", 8.5F);
            nudStep.ForeColor = Color.FromArgb(200, 230, 255);
            nudStep.Location = new Point(180, 154);
            nudStep.Maximum = new decimal(new int[] { 9, 0, 0, 0 });
            nudStep.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudStep.Name = "nudStep";
            nudStep.Size = new Size(75, 21);
            nudStep.TabIndex = 11;
            nudStep.Value = new decimal(new int[] { 1, 0, 0, 0 });
            nudStep.ValueChanged += OnParamChanged;
            // 
            // lblStepHint
            // 
            lblStepHint.AutoSize = true;
            lblStepHint.BackColor = Color.Transparent;
            lblStepHint.Font = new Font("Consolas", 8F);
            lblStepHint.ForeColor = Color.FromArgb(130, 160, 200);
            lblStepHint.Location = new Point(270, 157);
            lblStepHint.Name = "lblStepHint";
            lblStepHint.Size = new Size(259, 13);
            lblStepHint.TabIndex = 12;
            lblStepHint.Text = "(2 = read 40-track disk in 80-track drive)";
            // 
            // chkHSwap
            // 
            chkHSwap.AutoSize = true;
            chkHSwap.BackColor = Color.Transparent;
            chkHSwap.Font = new Font("Consolas", 8.5F);
            chkHSwap.ForeColor = Color.FromArgb(160, 200, 255);
            chkHSwap.Location = new Point(10, 190);
            chkHSwap.Name = "chkHSwap";
            chkHSwap.Size = new Size(271, 18);
            chkHSwap.TabIndex = 13;
            chkHSwap.Text = "hswap  (physical heads are swapped)";
            chkHSwap.UseVisualStyleBackColor = false;
            chkHSwap.CheckedChanged += OnParamChanged;
            // 
            // sepFlippy
            // 
            sepFlippy.BackColor = Color.FromArgb(40, 60, 90);
            sepFlippy.Location = new Point(10, 224);
            sepFlippy.Name = "sepFlippy";
            sepFlippy.Size = new Size(760, 1);
            sepFlippy.TabIndex = 14;
            // 
            // lblFlippyHeader
            // 
            lblFlippyHeader.AutoSize = true;
            lblFlippyHeader.BackColor = Color.Transparent;
            lblFlippyHeader.Font = new Font("Consolas", 8.5F, FontStyle.Bold);
            lblFlippyHeader.ForeColor = Color.FromArgb(180, 140, 60);
            lblFlippyHeader.Location = new Point(10, 234);
            lblFlippyHeader.Name = "lblFlippyHeader";
            lblFlippyHeader.Size = new Size(182, 14);
            lblFlippyHeader.TabIndex = 15;
            lblFlippyHeader.Text = "Flippy Drive Head Offsets";
            // 
            // chkHead0Off
            // 
            chkHead0Off.AutoSize = true;
            chkHead0Off.BackColor = Color.Transparent;
            chkHead0Off.Font = new Font("Consolas", 8.5F);
            chkHead0Off.ForeColor = Color.FromArgb(160, 200, 255);
            chkHead0Off.Location = new Point(10, 260);
            chkHead0Off.Name = "chkHead0Off";
            chkHead0Off.Size = new Size(75, 18);
            chkHead0Off.TabIndex = 16;
            chkHead0Off.Text = "h0.off=";
            chkHead0Off.UseVisualStyleBackColor = false;
            chkHead0Off.CheckedChanged += ChkHead0Off_CheckedChanged;
            // 
            // nudHead0Off
            // 
            nudHead0Off.BackColor = Color.FromArgb(28, 34, 48);
            nudHead0Off.BorderStyle = BorderStyle.FixedSingle;
            nudHead0Off.Enabled = false;
            nudHead0Off.Font = new Font("Consolas", 8.5F);
            nudHead0Off.ForeColor = Color.FromArgb(200, 230, 255);
            nudHead0Off.Location = new Point(100, 258);
            nudHead0Off.Maximum = new decimal(new int[] { 9, 0, 0, 0 });
            nudHead0Off.Minimum = new decimal(new int[] { 9, 0, 0, int.MinValue });
            nudHead0Off.Name = "nudHead0Off";
            nudHead0Off.Size = new Size(75, 21);
            nudHead0Off.TabIndex = 17;
            nudHead0Off.ValueChanged += OnParamChanged;
            // 
            // lblH0OffHint
            // 
            lblH0OffHint.AutoSize = true;
            lblH0OffHint.BackColor = Color.Transparent;
            lblH0OffHint.Font = new Font("Consolas", 8F);
            lblH0OffHint.ForeColor = Color.FromArgb(130, 160, 200);
            lblH0OffHint.Location = new Point(190, 261);
            lblH0OffHint.Name = "lblH0OffHint";
            lblH0OffHint.Size = new Size(343, 13);
            lblH0OffHint.TabIndex = 18;
            lblH0OffHint.Text = "(head 0 cylinder offset, for flippy-modded 5.25\" drives)";
            // 
            // chkHead1Off
            // 
            chkHead1Off.AutoSize = true;
            chkHead1Off.BackColor = Color.Transparent;
            chkHead1Off.Font = new Font("Consolas", 8.5F);
            chkHead1Off.ForeColor = Color.FromArgb(160, 200, 255);
            chkHead1Off.Location = new Point(10, 290);
            chkHead1Off.Name = "chkHead1Off";
            chkHead1Off.Size = new Size(75, 18);
            chkHead1Off.TabIndex = 19;
            chkHead1Off.Text = "h1.off=";
            chkHead1Off.UseVisualStyleBackColor = false;
            chkHead1Off.CheckedChanged += ChkHead1Off_CheckedChanged;
            // 
            // nudHead1Off
            // 
            nudHead1Off.BackColor = Color.FromArgb(28, 34, 48);
            nudHead1Off.BorderStyle = BorderStyle.FixedSingle;
            nudHead1Off.Enabled = false;
            nudHead1Off.Font = new Font("Consolas", 8.5F);
            nudHead1Off.ForeColor = Color.FromArgb(200, 230, 255);
            nudHead1Off.Location = new Point(100, 288);
            nudHead1Off.Maximum = new decimal(new int[] { 9, 0, 0, 0 });
            nudHead1Off.Minimum = new decimal(new int[] { 9, 0, 0, int.MinValue });
            nudHead1Off.Name = "nudHead1Off";
            nudHead1Off.Size = new Size(75, 21);
            nudHead1Off.TabIndex = 20;
            nudHead1Off.ValueChanged += OnParamChanged;
            // 
            // lblH1OffHint
            // 
            lblH1OffHint.AutoSize = true;
            lblH1OffHint.BackColor = Color.Transparent;
            lblH1OffHint.Font = new Font("Consolas", 8F);
            lblH1OffHint.ForeColor = Color.FromArgb(130, 160, 200);
            lblH1OffHint.Location = new Point(190, 291);
            lblH1OffHint.Name = "lblH1OffHint";
            lblH1OffHint.Size = new Size(151, 13);
            lblH1OffHint.TabIndex = 21;
            lblH1OffHint.Text = "(head 1 cylinder offset)";
            // 
            // lblTrackSpec
            // 
            lblTrackSpec.BackColor = Color.FromArgb(14, 18, 28);
            lblTrackSpec.Font = new Font("Consolas", 8.5F);
            lblTrackSpec.ForeColor = Color.FromArgb(80, 200, 80);
            lblTrackSpec.Location = new Point(10, 332);
            lblTrackSpec.Name = "lblTrackSpec";
            lblTrackSpec.Size = new Size(760, 22);
            lblTrackSpec.TabIndex = 22;
            lblTrackSpec.Text = "→  (default)";
            // 
            // tabAdvanced
            // 
            tabAdvanced.BackColor = Color.FromArgb(22, 26, 36);
            tabAdvanced.Controls.Add(lblAdvHeader);
            tabAdvanced.Controls.Add(lblDrive);
            tabAdvanced.Controls.Add(cmbDrive);
            tabAdvanced.Controls.Add(lblDriveHint);
            tabAdvanced.Controls.Add(lblExtraArgs);
            tabAdvanced.Controls.Add(txtExtraArgs);
            tabAdvanced.Controls.Add(lblTokenNote);
            tabAdvanced.Location = new Point(4, 36);
            tabAdvanced.Name = "tabAdvanced";
            tabAdvanced.Size = new Size(872, 590);
            tabAdvanced.TabIndex = 2;
            tabAdvanced.Text = "Advanced";
            // 
            // lblAdvHeader
            // 
            lblAdvHeader.AutoSize = true;
            lblAdvHeader.BackColor = Color.Transparent;
            lblAdvHeader.Font = new Font("Consolas", 8.5F, FontStyle.Bold);
            lblAdvHeader.ForeColor = Color.FromArgb(160, 200, 255);
            lblAdvHeader.Location = new Point(10, 14);
            lblAdvHeader.Name = "lblAdvHeader";
            lblAdvHeader.Size = new Size(231, 14);
            lblAdvHeader.TabIndex = 0;
            lblAdvHeader.Text = "Drive Selection & Extra Arguments";
            // 
            // lblDrive
            // 
            lblDrive.AutoSize = true;
            lblDrive.BackColor = Color.Transparent;
            lblDrive.Font = new Font("Consolas", 8F);
            lblDrive.ForeColor = Color.FromArgb(130, 160, 200);
            lblDrive.Location = new Point(10, 45);
            lblDrive.Name = "lblDrive";
            lblDrive.Size = new Size(55, 13);
            lblDrive.TabIndex = 1;
            lblDrive.Text = "--drive:";
            // 
            // cmbDrive
            // 
            cmbDrive.BackColor = Color.FromArgb(28, 34, 48);
            cmbDrive.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDrive.FlatStyle = FlatStyle.Flat;
            cmbDrive.Font = new Font("Consolas", 8.5F);
            cmbDrive.ForeColor = Color.FromArgb(200, 230, 255);
            cmbDrive.Items.AddRange(new object[] { "(auto)", "a", "b", "0", "1", "2", "3" });
            cmbDrive.Location = new Point(120, 42);
            cmbDrive.Name = "cmbDrive";
            cmbDrive.Size = new Size(120, 21);
            cmbDrive.TabIndex = 2;
            cmbDrive.SelectedIndexChanged += OnParamChanged;
            // 
            // lblDriveHint
            // 
            lblDriveHint.AutoSize = true;
            lblDriveHint.BackColor = Color.Transparent;
            lblDriveHint.Font = new Font("Consolas", 8F);
            lblDriveHint.ForeColor = Color.FromArgb(130, 160, 200);
            lblDriveHint.Location = new Point(255, 45);
            lblDriveHint.Name = "lblDriveHint";
            lblDriveHint.Size = new Size(235, 13);
            lblDriveHint.TabIndex = 3;
            lblDriveHint.Text = "(drive letter or number on the GW bus)";
            // 
            // lblExtraArgs
            // 
            lblExtraArgs.AutoSize = true;
            lblExtraArgs.BackColor = Color.Transparent;
            lblExtraArgs.Font = new Font("Consolas", 8F);
            lblExtraArgs.ForeColor = Color.FromArgb(130, 160, 200);
            lblExtraArgs.Location = new Point(10, 79);
            lblExtraArgs.Name = "lblExtraArgs";
            lblExtraArgs.Size = new Size(73, 13);
            lblExtraArgs.TabIndex = 4;
            lblExtraArgs.Text = "Extra Args:";
            // 
            // txtExtraArgs
            // 
            txtExtraArgs.BackColor = Color.FromArgb(28, 34, 48);
            txtExtraArgs.BorderStyle = BorderStyle.FixedSingle;
            txtExtraArgs.Font = new Font("Consolas", 8.5F);
            txtExtraArgs.ForeColor = Color.FromArgb(200, 230, 255);
            txtExtraArgs.Location = new Point(120, 76);
            txtExtraArgs.Name = "txtExtraArgs";
            txtExtraArgs.Size = new Size(620, 21);
            txtExtraArgs.TabIndex = 5;
            txtExtraArgs.TextChanged += OnParamChanged;
            // 
            // lblTokenNote
            // 
            lblTokenNote.BackColor = Color.FromArgb(16, 20, 30);
            lblTokenNote.Font = new Font("Consolas", 8F);
            lblTokenNote.ForeColor = Color.FromArgb(90, 130, 170);
            lblTokenNote.Location = new Point(10, 126);
            lblTokenNote.Name = "lblTokenNote";
            lblTokenNote.Size = new Size(750, 80);
            lblTokenNote.TabIndex = 6;
            lblTokenNote.Text = "Tokens available in Post-Action arguments:\r\n  {ImageFile}  — full path to the image file\r\n  {LogFolder}  — path to this job's log folder\r\n  {JobId}      — unique job ID string";
            // 
            // tabPostActions
            // 
            tabPostActions.BackColor = Color.FromArgb(22, 26, 36);
            tabPostActions.Controls.Add(lblPaHint);
            tabPostActions.Controls.Add(lvPostActions);
            tabPostActions.Controls.Add(btnAddAction);
            tabPostActions.Controls.Add(btnEditAction);
            tabPostActions.Controls.Add(btnRemoveActionBtn);
            tabPostActions.Controls.Add(btnMoveActionUpBtn);
            tabPostActions.Controls.Add(btnMoveActionDownBtn);
            tabPostActions.Location = new Point(4, 36);
            tabPostActions.Name = "tabPostActions";
            tabPostActions.Size = new Size(872, 590);
            tabPostActions.TabIndex = 3;
            tabPostActions.Text = "Post-Actions";
            // 
            // lblPaHint
            // 
            lblPaHint.BackColor = Color.Transparent;
            lblPaHint.Font = new Font("Consolas", 7.5F);
            lblPaHint.ForeColor = Color.FromArgb(90, 130, 170);
            lblPaHint.Location = new Point(10, 10);
            lblPaHint.Name = "lblPaHint";
            lblPaHint.Size = new Size(760, 18);
            lblPaHint.TabIndex = 0;
            lblPaHint.Text = "Actions run sequentially after a successful job.  Tokens: {ImageFile}, {LogFolder}, {JobId}";
            // 
            // lvPostActions
            // 
            lvPostActions.BackColor = Color.FromArgb(18, 22, 32);
            lvPostActions.BorderStyle = BorderStyle.FixedSingle;
            lvPostActions.Columns.AddRange(new ColumnHeader[] { columnHeaderOrd, columnHeaderName, columnHeaderType, columnHeaderExe, columnHeaderArgs, columnHeaderEn });
            lvPostActions.Font = new Font("Consolas", 8F);
            lvPostActions.ForeColor = Color.FromArgb(180, 210, 255);
            lvPostActions.FullRowSelect = true;
            lvPostActions.Location = new Point(10, 34);
            lvPostActions.Name = "lvPostActions";
            lvPostActions.Size = new Size(760, 380);
            lvPostActions.TabIndex = 1;
            lvPostActions.UseCompatibleStateImageBehavior = false;
            lvPostActions.View = View.Details;
            // 
            // columnHeaderOrd
            // 
            columnHeaderOrd.Text = "#";
            columnHeaderOrd.Width = 30;
            // 
            // columnHeaderName
            // 
            columnHeaderName.Text = "Name";
            columnHeaderName.Width = 150;
            // 
            // columnHeaderType
            // 
            columnHeaderType.Text = "Type";
            columnHeaderType.Width = 90;
            // 
            // columnHeaderExe
            // 
            columnHeaderExe.Text = "Executable / Script";
            columnHeaderExe.Width = 280;
            // 
            // columnHeaderArgs
            // 
            columnHeaderArgs.Text = "Arguments";
            columnHeaderArgs.Width = 170;
            // 
            // columnHeaderEn
            // 
            columnHeaderEn.Text = "En";
            columnHeaderEn.Width = 30;
            // 
            // btnAddAction
            // 
            btnAddAction.BackColor = Color.FromArgb(20, 55, 30);
            btnAddAction.FlatAppearance.BorderColor = Color.FromArgb(50, 120, 70);
            btnAddAction.FlatStyle = FlatStyle.Flat;
            btnAddAction.Font = new Font("Consolas", 8F);
            btnAddAction.ForeColor = Color.FromArgb(100, 220, 130);
            btnAddAction.Location = new Point(10, 422);
            btnAddAction.Name = "btnAddAction";
            btnAddAction.Size = new Size(90, 26);
            btnAddAction.TabIndex = 2;
            btnAddAction.Text = "+ Add";
            btnAddAction.UseVisualStyleBackColor = false;
            btnAddAction.Click += BtnAddAction_Click;
            // 
            // btnEditAction
            // 
            btnEditAction.BackColor = Color.FromArgb(20, 35, 65);
            btnEditAction.FlatAppearance.BorderColor = Color.FromArgb(50, 80, 140);
            btnEditAction.FlatStyle = FlatStyle.Flat;
            btnEditAction.Font = new Font("Consolas", 8F);
            btnEditAction.ForeColor = Color.FromArgb(100, 160, 240);
            btnEditAction.Location = new Point(110, 422);
            btnEditAction.Name = "btnEditAction";
            btnEditAction.Size = new Size(80, 26);
            btnEditAction.TabIndex = 3;
            btnEditAction.Text = "Edit";
            btnEditAction.UseVisualStyleBackColor = false;
            btnEditAction.Click += BtnEditAction_Click;
            // 
            // btnRemoveActionBtn
            // 
            btnRemoveActionBtn.BackColor = Color.FromArgb(55, 20, 20);
            btnRemoveActionBtn.FlatAppearance.BorderColor = Color.FromArgb(100, 40, 40);
            btnRemoveActionBtn.FlatStyle = FlatStyle.Flat;
            btnRemoveActionBtn.Font = new Font("Consolas", 8F);
            btnRemoveActionBtn.ForeColor = Color.FromArgb(220, 80, 80);
            btnRemoveActionBtn.Location = new Point(200, 422);
            btnRemoveActionBtn.Name = "btnRemoveActionBtn";
            btnRemoveActionBtn.Size = new Size(90, 26);
            btnRemoveActionBtn.TabIndex = 4;
            btnRemoveActionBtn.Text = "Remove";
            btnRemoveActionBtn.UseVisualStyleBackColor = false;
            btnRemoveActionBtn.Click += BtnRemoveAction_Click;
            // 
            // btnMoveActionUpBtn
            // 
            btnMoveActionUpBtn.BackColor = Color.FromArgb(20, 35, 55);
            btnMoveActionUpBtn.FlatAppearance.BorderColor = Color.FromArgb(50, 80, 120);
            btnMoveActionUpBtn.FlatStyle = FlatStyle.Flat;
            btnMoveActionUpBtn.Font = new Font("Consolas", 8F);
            btnMoveActionUpBtn.ForeColor = Color.White;
            btnMoveActionUpBtn.Location = new Point(300, 422);
            btnMoveActionUpBtn.Name = "btnMoveActionUpBtn";
            btnMoveActionUpBtn.Size = new Size(40, 26);
            btnMoveActionUpBtn.TabIndex = 5;
            btnMoveActionUpBtn.Text = "▲";
            btnMoveActionUpBtn.UseVisualStyleBackColor = false;
            btnMoveActionUpBtn.Click += BtnMoveActionUp_Click;
            // 
            // btnMoveActionDownBtn
            // 
            btnMoveActionDownBtn.BackColor = Color.FromArgb(20, 35, 55);
            btnMoveActionDownBtn.FlatAppearance.BorderColor = Color.FromArgb(50, 80, 120);
            btnMoveActionDownBtn.FlatStyle = FlatStyle.Flat;
            btnMoveActionDownBtn.Font = new Font("Consolas", 8F);
            btnMoveActionDownBtn.ForeColor = Color.White;
            btnMoveActionDownBtn.Location = new Point(348, 422);
            btnMoveActionDownBtn.Name = "btnMoveActionDownBtn";
            btnMoveActionDownBtn.Size = new Size(40, 26);
            btnMoveActionDownBtn.TabIndex = 6;
            btnMoveActionDownBtn.Text = "▼";
            btnMoveActionDownBtn.UseVisualStyleBackColor = false;
            btnMoveActionDownBtn.Click += BtnMoveActionDown_Click;
            // 
            // tabRepeat
            // 
            tabRepeat.BackColor = Color.FromArgb(22, 26, 36);
            tabRepeat.Controls.Add(chkRepetitive);
            tabRepeat.Controls.Add(sepRepeat1);
            tabRepeat.Controls.Add(lblOutputFolder);
            tabRepeat.Controls.Add(txtOutputFolder);
            tabRepeat.Controls.Add(btnBrowseFolder);
            tabRepeat.Controls.Add(txtFilePattern);
            tabRepeat.Controls.Add(lblPatternHint);
            tabRepeat.Controls.Add(lblStartIndex);
            tabRepeat.Controls.Add(nudStartIndex);
            tabRepeat.Controls.Add(lblDtFormat);
            tabRepeat.Controls.Add(txtDtFormat);
            tabRepeat.Controls.Add(lblDtFormatHint);
            tabRepeat.Controls.Add(sepRepeat2);
            tabRepeat.Controls.Add(lblPatternPreviewCaption);
            tabRepeat.Controls.Add(lblPatternPreview);
            tabRepeat.Controls.Add(lblRepeatNote);
            tabRepeat.Controls.Add(sepRepeat3);
            tabRepeat.Controls.Add(lblPresetNameCaption);
            tabRepeat.Controls.Add(txtPresetName);
            tabRepeat.Controls.Add(sepRepeat4);
            tabRepeat.Controls.Add(chkUseGroup);
            tabRepeat.Controls.Add(cmbGroupDevice);
            tabRepeat.Controls.Add(cmbGroupDrive);
            tabRepeat.Controls.Add(btnGroupAdd);
            tabRepeat.Controls.Add(btnGroupRemove);
            tabRepeat.Controls.Add(lvGroupMembers);
            tabRepeat.Location = new Point(4, 36);
            tabRepeat.Name = "tabRepeat";
            tabRepeat.Size = new Size(872, 590);
            tabRepeat.TabIndex = 4;
            tabRepeat.Text = "Repetitive";
            // 
            // chkRepetitive
            // 
            chkRepetitive.AutoSize = true;
            chkRepetitive.BackColor = Color.Transparent;
            chkRepetitive.Font = new Font("Consolas", 9F, FontStyle.Bold);
            chkRepetitive.ForeColor = Color.FromArgb(100, 220, 160);
            chkRepetitive.Location = new Point(10, 14);
            chkRepetitive.Name = "chkRepetitive";
            chkRepetitive.Size = new Size(250, 18);
            chkRepetitive.TabIndex = 0;
            chkRepetitive.Text = "Repetitive mode (image sequence)";
            chkRepetitive.UseVisualStyleBackColor = false;
            chkRepetitive.CheckedChanged += OnParamChanged;
            // 
            // sepRepeat1
            // 
            sepRepeat1.BackColor = Color.FromArgb(40, 60, 90);
            sepRepeat1.Location = new Point(10, 48);
            sepRepeat1.Name = "sepRepeat1";
            sepRepeat1.Size = new Size(760, 1);
            sepRepeat1.TabIndex = 1;
            // 
            // lblOutputFolder
            // 
            lblOutputFolder.AutoSize = true;
            lblOutputFolder.BackColor = Color.Transparent;
            lblOutputFolder.Font = new Font("Consolas", 8F);
            lblOutputFolder.ForeColor = Color.FromArgb(130, 160, 200);
            lblOutputFolder.Location = new Point(10, 63);
            lblOutputFolder.Name = "lblOutputFolder";
            lblOutputFolder.Size = new Size(91, 13);
            lblOutputFolder.TabIndex = 2;
            lblOutputFolder.Text = "Output Folder:";
            // 
            // txtOutputFolder
            // 
            txtOutputFolder.BackColor = Color.FromArgb(28, 34, 48);
            txtOutputFolder.BorderStyle = BorderStyle.FixedSingle;
            txtOutputFolder.Font = new Font("Consolas", 8.5F);
            txtOutputFolder.ForeColor = Color.FromArgb(200, 230, 255);
            txtOutputFolder.Location = new Point(175, 60);
            txtOutputFolder.Name = "txtOutputFolder";
            txtOutputFolder.PlaceholderText = "(leave empty to use directory of Image File)";
            txtOutputFolder.Size = new Size(500, 21);
            txtOutputFolder.TabIndex = 3;
            txtOutputFolder.TextChanged += OnPatternChanged;
            // 
            // btnBrowseFolder
            // 
            btnBrowseFolder.BackColor = Color.FromArgb(30, 50, 80);
            btnBrowseFolder.FlatAppearance.BorderColor = Color.FromArgb(60, 100, 160);
            btnBrowseFolder.FlatStyle = FlatStyle.Flat;
            btnBrowseFolder.Font = new Font("Consolas", 8F);
            btnBrowseFolder.ForeColor = Color.White;
            btnBrowseFolder.Location = new Point(683, 60);
            btnBrowseFolder.Name = "btnBrowseFolder";
            btnBrowseFolder.Size = new Size(30, 22);
            btnBrowseFolder.TabIndex = 4;
            btnBrowseFolder.Text = "…";
            btnBrowseFolder.UseVisualStyleBackColor = false;
            btnBrowseFolder.Click += BtnBrowseFolder_Click;
            // 
            // txtFilePattern
            // 
            txtFilePattern.BackColor = Color.FromArgb(28, 34, 48);
            txtFilePattern.BorderStyle = BorderStyle.FixedSingle;
            txtFilePattern.Font = new Font("Consolas", 8.5F);
            txtFilePattern.ForeColor = Color.FromArgb(200, 230, 255);
            txtFilePattern.Location = new Point(175, 92);
            txtFilePattern.Name = "txtFilePattern";
            txtFilePattern.Size = new Size(570, 21);
            txtFilePattern.TabIndex = 5;
            txtFilePattern.TextChanged += OnPatternChanged;
            // 
            // lblPatternHint
            // 
            lblPatternHint.BackColor = Color.Transparent;
            lblPatternHint.Font = new Font("Consolas", 7.5F);
            lblPatternHint.ForeColor = Color.FromArgb(90, 130, 170);
            lblPatternHint.Location = new Point(175, 120);
            lblPatternHint.Name = "lblPatternHint";
            lblPatternHint.Size = new Size(570, 16);
            lblPatternHint.TabIndex = 6;
            lblPatternHint.Text = "Tokens:  {n}  {n:D3}  {dt}   Example: Disk_{n:D3}_{dt}.scp";
            // 
            // lblStartIndex
            // 
            lblStartIndex.AutoSize = true;
            lblStartIndex.BackColor = Color.Transparent;
            lblStartIndex.Font = new Font("Consolas", 8F);
            lblStartIndex.ForeColor = Color.FromArgb(130, 160, 200);
            lblStartIndex.Location = new Point(10, 151);
            lblStartIndex.Name = "lblStartIndex";
            lblStartIndex.Size = new Size(79, 13);
            lblStartIndex.TabIndex = 7;
            lblStartIndex.Text = "Start Index:";
            // 
            // nudStartIndex
            // 
            nudStartIndex.BackColor = Color.FromArgb(28, 34, 48);
            nudStartIndex.BorderStyle = BorderStyle.FixedSingle;
            nudStartIndex.Font = new Font("Consolas", 8.5F);
            nudStartIndex.ForeColor = Color.FromArgb(200, 230, 255);
            nudStartIndex.Location = new Point(175, 148);
            nudStartIndex.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            nudStartIndex.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudStartIndex.Name = "nudStartIndex";
            nudStartIndex.Size = new Size(100, 21);
            nudStartIndex.TabIndex = 8;
            nudStartIndex.Value = new decimal(new int[] { 1, 0, 0, 0 });
            nudStartIndex.ValueChanged += OnPatternChanged;
            // 
            // lblDtFormat
            // 
            lblDtFormat.AutoSize = true;
            lblDtFormat.BackColor = Color.Transparent;
            lblDtFormat.Font = new Font("Consolas", 8F);
            lblDtFormat.ForeColor = Color.FromArgb(130, 160, 200);
            lblDtFormat.Location = new Point(10, 183);
            lblDtFormat.Name = "lblDtFormat";
            lblDtFormat.Size = new Size(103, 13);
            lblDtFormat.TabIndex = 9;
            lblDtFormat.Text = "DateTime format:";
            // 
            // txtDtFormat
            // 
            txtDtFormat.BackColor = Color.FromArgb(28, 34, 48);
            txtDtFormat.BorderStyle = BorderStyle.FixedSingle;
            txtDtFormat.Font = new Font("Consolas", 8.5F);
            txtDtFormat.ForeColor = Color.FromArgb(200, 230, 255);
            txtDtFormat.Location = new Point(175, 180);
            txtDtFormat.Name = "txtDtFormat";
            txtDtFormat.Size = new Size(260, 21);
            txtDtFormat.TabIndex = 10;
            txtDtFormat.Text = "yyyyMMdd_HHmmss";
            txtDtFormat.TextChanged += OnPatternChanged;
            // 
            // lblDtFormatHint
            // 
            lblDtFormatHint.AutoSize = true;
            lblDtFormatHint.BackColor = Color.Transparent;
            lblDtFormatHint.Font = new Font("Consolas", 8F);
            lblDtFormatHint.ForeColor = Color.FromArgb(130, 160, 200);
            lblDtFormatHint.Location = new Point(444, 183);
            lblDtFormatHint.Name = "lblDtFormatHint";
            lblDtFormatHint.Size = new Size(259, 13);
            lblDtFormatHint.TabIndex = 11;
            lblDtFormatHint.Text = "(C# DateTime format, e.g. yyyyMMdd_HHmmss)";
            // 
            // sepRepeat2
            // 
            sepRepeat2.BackColor = Color.FromArgb(40, 60, 90);
            sepRepeat2.Location = new Point(10, 216);
            sepRepeat2.Name = "sepRepeat2";
            sepRepeat2.Size = new Size(760, 1);
            sepRepeat2.TabIndex = 12;
            // 
            // lblPatternPreviewCaption
            // 
            lblPatternPreviewCaption.AutoSize = true;
            lblPatternPreviewCaption.BackColor = Color.Transparent;
            lblPatternPreviewCaption.Font = new Font("Consolas", 8F);
            lblPatternPreviewCaption.ForeColor = Color.FromArgb(130, 160, 200);
            lblPatternPreviewCaption.Location = new Point(10, 231);
            lblPatternPreviewCaption.Name = "lblPatternPreviewCaption";
            lblPatternPreviewCaption.Size = new Size(55, 13);
            lblPatternPreviewCaption.TabIndex = 13;
            lblPatternPreviewCaption.Text = "Preview:";
            // 
            // lblPatternPreview
            // 
            lblPatternPreview.BackColor = Color.FromArgb(14, 18, 28);
            lblPatternPreview.Font = new Font("Consolas", 9F);
            lblPatternPreview.ForeColor = Color.FromArgb(220, 200, 80);
            lblPatternPreview.Location = new Point(175, 228);
            lblPatternPreview.Name = "lblPatternPreview";
            lblPatternPreview.Padding = new Padding(4, 2, 0, 0);
            lblPatternPreview.Size = new Size(570, 22);
            lblPatternPreview.TabIndex = 14;
            // 
            // lblRepeatNote
            // 
            lblRepeatNote.BackColor = Color.FromArgb(16, 20, 30);
            lblRepeatNote.Font = new Font("Consolas", 8F);
            lblRepeatNote.ForeColor = Color.FromArgb(90, 130, 160);
            lblRepeatNote.Location = new Point(10, 268);
            lblRepeatNote.Name = "lblRepeatNote";
            lblRepeatNote.Size = new Size(760, 42);
            lblRepeatNote.TabIndex = 15;
            lblRepeatNote.Text = "The file pattern overrides the image file for each disk.\nAfter each disk a dialog will ask to insert the next one.";
            // 
            // sepRepeat3
            // 
            sepRepeat3.BackColor = Color.FromArgb(40, 60, 90);
            sepRepeat3.Location = new Point(10, 332);
            sepRepeat3.Name = "sepRepeat3";
            sepRepeat3.Size = new Size(760, 1);
            sepRepeat3.TabIndex = 16;
            // 
            // lblPresetNameCaption
            // 
            lblPresetNameCaption.AutoSize = true;
            lblPresetNameCaption.BackColor = Color.Transparent;
            lblPresetNameCaption.Font = new Font("Consolas", 8F);
            lblPresetNameCaption.ForeColor = Color.FromArgb(130, 160, 200);
            lblPresetNameCaption.Location = new Point(10, 347);
            lblPresetNameCaption.Name = "lblPresetNameCaption";
            lblPresetNameCaption.Size = new Size(79, 13);
            lblPresetNameCaption.TabIndex = 17;
            lblPresetNameCaption.Text = "Preset Name:";
            // 
            // txtPresetName
            // 
            txtPresetName.BackColor = Color.FromArgb(28, 34, 48);
            txtPresetName.BorderStyle = BorderStyle.FixedSingle;
            txtPresetName.Font = new Font("Consolas", 8.5F);
            txtPresetName.ForeColor = Color.FromArgb(200, 230, 255);
            txtPresetName.Location = new Point(175, 344);
            txtPresetName.Name = "txtPresetName";
            txtPresetName.Size = new Size(400, 21);
            txtPresetName.TabIndex = 18;
            txtPresetName.Text = "My Preset";
            // 
            // sepRepeat4
            // 
            sepRepeat4.BackColor = Color.FromArgb(40, 60, 90);
            sepRepeat4.Location = new Point(10, 376);
            sepRepeat4.Name = "sepRepeat4";
            sepRepeat4.Size = new Size(760, 1);
            sepRepeat4.TabIndex = 19;
            // 
            // chkUseGroup
            // 
            chkUseGroup.AutoSize = true;
            chkUseGroup.BackColor = Color.Transparent;
            chkUseGroup.Font = new Font("Consolas", 9F, FontStyle.Bold);
            chkUseGroup.ForeColor = Color.FromArgb(120, 190, 255);
            chkUseGroup.Location = new Point(10, 386);
            chkUseGroup.Name = "chkUseGroup";
            chkUseGroup.Size = new Size(313, 18);
            chkUseGroup.TabIndex = 20;
            chkUseGroup.Text = "Use device group (parallel batch imaging)";
            chkUseGroup.UseVisualStyleBackColor = false;
            chkUseGroup.CheckedChanged += ChkUseGroup_CheckedChanged;
            // 
            // cmbGroupDevice
            // 
            cmbGroupDevice.BackColor = Color.FromArgb(28, 34, 48);
            cmbGroupDevice.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGroupDevice.Enabled = false;
            cmbGroupDevice.FlatStyle = FlatStyle.Flat;
            cmbGroupDevice.Font = new Font("Consolas", 8.5F);
            cmbGroupDevice.ForeColor = Color.FromArgb(200, 230, 255);
            cmbGroupDevice.Location = new Point(10, 412);
            cmbGroupDevice.Name = "cmbGroupDevice";
            cmbGroupDevice.Size = new Size(320, 21);
            cmbGroupDevice.TabIndex = 21;
            // 
            // cmbGroupDrive
            // 
            cmbGroupDrive.BackColor = Color.FromArgb(28, 34, 48);
            cmbGroupDrive.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGroupDrive.Enabled = false;
            cmbGroupDrive.FlatStyle = FlatStyle.Flat;
            cmbGroupDrive.Font = new Font("Consolas", 8.5F);
            cmbGroupDrive.ForeColor = Color.FromArgb(200, 230, 255);
            cmbGroupDrive.Items.AddRange(new object[] { "0", "1", "a", "b" });
            cmbGroupDrive.Location = new Point(338, 412);
            cmbGroupDrive.Name = "cmbGroupDrive";
            cmbGroupDrive.Size = new Size(70, 21);
            cmbGroupDrive.TabIndex = 22;
            // 
            // btnGroupAdd
            // 
            btnGroupAdd.BackColor = Color.FromArgb(18, 60, 32);
            btnGroupAdd.FlatAppearance.BorderColor = Color.FromArgb(40, 120, 65);
            btnGroupAdd.FlatStyle = FlatStyle.Flat;
            btnGroupAdd.Font = new Font("Consolas", 8.5F);
            btnGroupAdd.ForeColor = Color.FromArgb(90, 220, 120);
            btnGroupAdd.Location = new Point(416, 412);
            btnGroupAdd.Name = "btnGroupAdd";
            btnGroupAdd.Size = new Size(110, 22);
            btnGroupAdd.TabIndex = 23;
            btnGroupAdd.Text = "+ Add";
            btnGroupAdd.UseVisualStyleBackColor = false;
            btnGroupAdd.Click += BtnGroupAdd_Click;
            // 
            // btnGroupRemove
            // 
            btnGroupRemove.BackColor = Color.FromArgb(60, 20, 20);
            btnGroupRemove.FlatAppearance.BorderColor = Color.FromArgb(100, 40, 40);
            btnGroupRemove.FlatStyle = FlatStyle.Flat;
            btnGroupRemove.Font = new Font("Consolas", 8.5F);
            btnGroupRemove.ForeColor = Color.FromArgb(200, 80, 80);
            btnGroupRemove.Location = new Point(534, 412);
            btnGroupRemove.Name = "btnGroupRemove";
            btnGroupRemove.Size = new Size(110, 22);
            btnGroupRemove.TabIndex = 24;
            btnGroupRemove.Text = "− Remove";
            btnGroupRemove.UseVisualStyleBackColor = false;
            btnGroupRemove.Click += BtnGroupRemove_Click;
            // 
            // lvGroupMembers
            // 
            lvGroupMembers.BackColor = Color.FromArgb(28, 34, 48);
            lvGroupMembers.Columns.AddRange(new ColumnHeader[] { columnHeaderDevice, columnHeaderDrive });
            lvGroupMembers.Enabled = false;
            lvGroupMembers.Font = new Font("Consolas", 8.5F);
            lvGroupMembers.ForeColor = Color.FromArgb(200, 230, 255);
            lvGroupMembers.FullRowSelect = true;
            lvGroupMembers.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lvGroupMembers.Location = new Point(10, 440);
            lvGroupMembers.Name = "lvGroupMembers";
            lvGroupMembers.Size = new Size(760, 86);
            lvGroupMembers.TabIndex = 25;
            lvGroupMembers.UseCompatibleStateImageBehavior = false;
            lvGroupMembers.View = View.Details;
            // 
            // columnHeaderDevice
            // 
            columnHeaderDevice.Text = "Device";
            columnHeaderDevice.Width = 480;
            // 
            // columnHeaderDrive
            // 
            columnHeaderDrive.Text = "Drive";
            columnHeaderDrive.Width = 120;
            // 
            // lblPreview
            // 
            lblPreview.BackColor = Color.FromArgb(12, 16, 22);
            lblPreview.Font = new Font("Consolas", 7.5F);
            lblPreview.ForeColor = Color.FromArgb(80, 180, 80);
            lblPreview.Location = new Point(10, 648);
            lblPreview.Name = "lblPreview";
            lblPreview.Padding = new Padding(4, 3, 0, 0);
            lblPreview.Size = new Size(880, 22);
            lblPreview.TabIndex = 1;
            // 
            // sepLine
            // 
            sepLine.BackColor = Color.FromArgb(40, 60, 90);
            sepLine.Location = new Point(10, 676);
            sepLine.Name = "sepLine";
            sepLine.Size = new Size(880, 1);
            sepLine.TabIndex = 2;
            // 
            // btnSavePreset
            // 
            btnSavePreset.BackColor = Color.FromArgb(20, 35, 65);
            btnSavePreset.FlatAppearance.BorderColor = Color.FromArgb(50, 85, 155);
            btnSavePreset.FlatStyle = FlatStyle.Flat;
            btnSavePreset.Font = new Font("Consolas", 8F);
            btnSavePreset.ForeColor = Color.FromArgb(100, 160, 240);
            btnSavePreset.Location = new Point(10, 684);
            btnSavePreset.Name = "btnSavePreset";
            btnSavePreset.Size = new Size(160, 36);
            btnSavePreset.TabIndex = 3;
            btnSavePreset.Text = "💾  Save Preset";
            btnSavePreset.UseVisualStyleBackColor = false;
            btnSavePreset.Click += BtnSavePreset_Click;
            // 
            // btnLoadPreset
            // 
            btnLoadPreset.BackColor = Color.FromArgb(20, 35, 65);
            btnLoadPreset.FlatAppearance.BorderColor = Color.FromArgb(50, 85, 155);
            btnLoadPreset.FlatStyle = FlatStyle.Flat;
            btnLoadPreset.Font = new Font("Consolas", 8F);
            btnLoadPreset.ForeColor = Color.FromArgb(100, 160, 240);
            btnLoadPreset.Location = new Point(178, 684);
            btnLoadPreset.Name = "btnLoadPreset";
            btnLoadPreset.Size = new Size(160, 36);
            btnLoadPreset.TabIndex = 4;
            btnLoadPreset.Text = "📂  Load Preset";
            btnLoadPreset.UseVisualStyleBackColor = false;
            btnLoadPreset.Click += BtnLoadPreset_Click;
            // 
            // btnOk
            // 
            btnOk.BackColor = Color.FromArgb(20, 70, 40);
            btnOk.DialogResult = DialogResult.OK;
            btnOk.FlatAppearance.BorderColor = Color.FromArgb(50, 140, 80);
            btnOk.FlatStyle = FlatStyle.Flat;
            btnOk.Font = new Font("Consolas", 9.5F, FontStyle.Bold);
            btnOk.ForeColor = Color.FromArgb(80, 230, 120);
            btnOk.Location = new Point(580, 684);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(160, 36);
            btnOk.TabIndex = 5;
            btnOk.Text = "▶  Start Job";
            btnOk.UseVisualStyleBackColor = false;
            btnOk.Click += BtnOk_Click;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.FromArgb(50, 25, 25);
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(100, 50, 50);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Consolas", 9F);
            btnCancel.ForeColor = Color.FromArgb(200, 100, 100);
            btnCancel.Location = new Point(763, 685);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(100, 36);
            btnCancel.TabIndex = 6;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            // 
            // NewJobDialog
            // 
            AcceptButton = btnOk;
            BackColor = Color.FromArgb(18, 22, 32);
            CancelButton = btnCancel;
            ClientSize = new Size(884, 781);
            Controls.Add(tabs);
            Controls.Add(lblPreview);
            Controls.Add(sepLine);
            Controls.Add(btnSavePreset);
            Controls.Add(btnLoadPreset);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            ForeColor = Color.FromArgb(180, 210, 255);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MaximumSize = new Size(900, 820);
            MinimumSize = new Size(900, 820);
            Name = "NewJobDialog";
            StartPosition = FormStartPosition.CenterParent;
            Text = "New GreaseWeazle Job";
            tabs.ResumeLayout(false);
            tabMain.ResumeLayout(false);
            tabMain.PerformLayout();
            ((ISupportInitialize)nudRevs).EndInit();
            ((ISupportInitialize)nudBitrate).EndInit();
            ((ISupportInitialize)nudRetries).EndInit();
            tabTracks.ResumeLayout(false);
            tabTracks.PerformLayout();
            ((ISupportInitialize)nudStartCyl).EndInit();
            ((ISupportInitialize)nudEndCyl).EndInit();
            ((ISupportInitialize)nudStep).EndInit();
            ((ISupportInitialize)nudHead0Off).EndInit();
            ((ISupportInitialize)nudHead1Off).EndInit();
            tabAdvanced.ResumeLayout(false);
            tabAdvanced.PerformLayout();
            tabPostActions.ResumeLayout(false);
            tabRepeat.ResumeLayout(false);
            tabRepeat.PerformLayout();
            ((ISupportInitialize)nudStartIndex).EndInit();
            ResumeLayout(false);
        }
    }
}
