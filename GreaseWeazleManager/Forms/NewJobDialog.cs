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
    public partial class NewJobDialog : Form
    {
        private readonly List<GreaseWeazleDevice> _devices = new();
        private readonly GreaseWeazleDevice?      _preselectedDevice;

        /// <summary>Gets the <see cref="GwJob"/> created when the user clicks Start Job, or <see langword="null"/> if cancelled.</summary>
        public GwJob? Result { get; private set; }

        /// <summary>Gets the group job created when the user starts a device-group job, or <see langword="null"/>.</summary>
        public GroupRepetitiveJob? GroupResult { get; private set; }

        private bool _initialized;

        /// <summary>Design-time-only constructor. Do not use at runtime.</summary>
        public NewJobDialog() : this(new List<GreaseWeazleDevice>())
        {
        }

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
            ApplyLocalizedText();
            PopulateGroupDeviceCombo();
            PopulateDevices();
            _initialized = true;
            UpdatePreview();
            UpdateTrackSpecLabel();
        }

        /// <summary>
        /// Re-applies every localised string over the (possibly English-literal) defaults baked
        /// into InitializeComponent. This runs every time the dialog is constructed, so it stays
        /// correct even if the WinForms Designer resaves NewJobDialog.Designer.cs and flattens its
        /// L10n.T(...) calls back to literals the next time that file is opened and saved there —
        /// a real, repeatedly-observed limitation of the Designer's CodeDom round-trip.
        /// </summary>
        private void ApplyLocalizedText()
        {
            Text = L10n.T("job_dlg.title");

            tabMain.Text        = L10n.T("job_dlg.tab_main");
            tabTracks.Text      = L10n.T("job_dlg.tab_tracks");
            tabAdvanced.Text    = L10n.T("job_dlg.tab_advanced");
            tabPostActions.Text = L10n.T("job_dlg.tab_postactions");
            tabRepeat.Text      = L10n.T("job_dlg.tab_repeat");

            // Main tab
            lblDevice.Text     = L10n.T("job_dlg.device");
            lblJobType.Text    = L10n.T("job_dlg.job_type");
            ReplaceComboItems(cmbJobType, L10n.T("job_dlg.read"), L10n.T("job_dlg.write"));
            lblImageFile.Text  = L10n.T("job_dlg.image_file");
            lblDiskFormat.Text = L10n.T("job_dlg.disk_format");
            lblCommonOptsHeader.Text = L10n.T("job_dlg.common_opts");
            lblRevs.Text        = L10n.T("job_dlg.revs");
            lblRevsHint.Text    = L10n.T("job_dlg.revs_hint");
            lblDensel.Text      = L10n.T("job_dlg.densel");
            lblBitrate.Text     = L10n.T("job_dlg.bitrate");
            lblBitrateHint.Text = L10n.T("job_dlg.bitrate_hint");
            lblReadOptsHeader.Text = L10n.T("job_dlg.read_opts");
            chkRetries.Text     = L10n.T("job_dlg.retries");
            chkNoClobber.Text   = L10n.T("job_dlg.no_clobber");
            chkRaw.Text         = L10n.T("job_dlg.raw");
            chkReverse.Text     = L10n.T("job_dlg.reverse_read");
            chkHardSectors.Text = L10n.T("job_dlg.hard_sectors");
            lblWriteOptsHeader.Text = L10n.T("job_dlg.write_opts");
            chkErase.Text        = L10n.T("job_dlg.erase");
            chkVerify.Text       = L10n.T("job_dlg.verify");
            chkGenTg43.Text      = L10n.T("job_dlg.gen_tg43");
            lblPrecomp.Text      = L10n.T("job_dlg.precomp");
            chkReverseW.Text     = L10n.T("job_dlg.reverse_write");
            chkHardSectorsW.Text = L10n.T("job_dlg.hard_sectors");

            // Tracks tab
            lblTrackSelHeader.Text = L10n.T("job_dlg.track_sel_head");
            lblTrackInfo.Text      = L10n.T("job_dlg.track_info");
            lblCylinders.Text      = L10n.T("job_dlg.cylinders");
            lblCylStart.Text       = L10n.T("job_dlg.cyl_start");
            lblCylEnd.Text         = L10n.T("job_dlg.cyl_end");
            lblCylHint.Text        = L10n.T("job_dlg.cyl_hint");
            lblHeads.Text          = L10n.T("job_dlg.heads");
            ReplaceComboItems(cmbHead, L10n.T("job_dlg.heads_both"), L10n.T("job_dlg.heads_0"), L10n.T("job_dlg.heads_1"));
            lblStep.Text           = L10n.T("job_dlg.step");
            lblStepHint.Text       = L10n.T("job_dlg.step_hint");
            chkHSwap.Text          = L10n.T("job_dlg.hswap");
            lblFlippyHeader.Text   = L10n.T("job_dlg.flippy_head");
            chkHead0Off.Text       = L10n.T("job_dlg.h0off");
            lblH0OffHint.Text      = L10n.T("job_dlg.h0off_hint");
            chkHead1Off.Text       = L10n.T("job_dlg.h1off");
            lblH1OffHint.Text      = L10n.T("job_dlg.h1off_hint");

            // Advanced tab
            lblAdvHeader.Text = L10n.T("job_dlg.adv_head");
            lblDrive.Text     = L10n.T("job_dlg.drive");
            lblDriveHint.Text = L10n.T("job_dlg.drive_hint");
            lblExtraArgs.Text = L10n.T("job_dlg.extra_args");
            lblTokenNote.Text = L10n.T("job_dlg.token_note");

            // Post-Actions tab
            lblPaHint.Text        = L10n.T("job_dlg.pa_hint");
            columnHeaderOrd.Text  = L10n.T("job_dlg.pa_col_ord");
            columnHeaderName.Text = L10n.T("job_dlg.pa_col_name");
            columnHeaderType.Text = L10n.T("job_dlg.pa_col_type");
            columnHeaderExe.Text  = L10n.T("job_dlg.pa_col_exe");
            columnHeaderArgs.Text = L10n.T("job_dlg.pa_col_args");
            columnHeaderEn.Text   = L10n.T("job_dlg.pa_col_en");
            btnAddAction.Text     = L10n.T("job_dlg.pa_add");
            btnEditAction.Text    = L10n.T("job_dlg.pa_edit");
            btnRemoveActionBtn.Text = L10n.T("job_dlg.pa_remove");

            // Repeat tab
            chkRepetitive.Text     = L10n.T("job_dlg.repeat_enabled");
            lblOutputFolder.Text   = L10n.T("job_dlg.output_folder");
            txtOutputFolder.PlaceholderText = L10n.T("job_dlg.output_folder_hint");
            lblPatternHint.Text    = L10n.T("job_dlg.pattern_hint");
            lblStartIndex.Text     = L10n.T("job_dlg.start_index");
            lblDtFormat.Text       = L10n.T("job_dlg.dt_format");
            lblDtFormatHint.Text   = L10n.T("job_dlg.dt_format_hint");
            lblPatternPreviewCaption.Text = L10n.T("job_dlg.pattern_preview");
            lblRepeatNote.Text     = L10n.T("job_dlg.repeat_note");
            chkUseGroup.Text       = L10n.T("job_dlg.use_group");
            btnGroupAdd.Text       = L10n.T("job_dlg.group_add");
            btnGroupRemove.Text    = L10n.T("job_dlg.group_remove");
            columnHeaderDevice.Text = L10n.T("job_dlg.group_col_device");
            columnHeaderDrive.Text  = L10n.T("job_dlg.group_col_drive");

            // Bottom bar
            btnSavePreset.Text = L10n.T("preset.save");
            btnLoadPreset.Text = L10n.T("preset.load");
            btnOk.Text         = L10n.T("job_dlg.start_job");
            btnCancel.Text     = L10n.T("job_dlg.cancel");
        }

        /// <summary>Replaces a ComboBox's items with localised text, preserving its current selection.</summary>
        private static void ReplaceComboItems(ComboBox combo, params string[] items)
        {
            int selected = combo.SelectedIndex;
            combo.Items.Clear();
            combo.Items.AddRange(items);
            combo.SelectedIndex = selected;
        }

        /// <summary>Populates the device-group combo box from <see cref="_devices"/> (dynamic, so it can't live in InitializeComponent).</summary>
        private void PopulateGroupDeviceCombo()
        {
            foreach (var d in _devices) cmbGroupDevice.Items.Add(d);
            if (cmbGroupDevice.Items.Count > 0) cmbGroupDevice.SelectedIndex = 0;
        }

        /// <summary>Shared handler for controls whose only effect is refreshing the command-line preview.</summary>
        private void OnParamChanged(object? sender, EventArgs e) => SafeUpdatePreviews();

        /// <summary>Shared handler for controls whose only effect is refreshing the file-pattern preview.</summary>
        private void OnPatternChanged(object? sender, EventArgs e) => UpdatePatternPreview();

        /// <summary>Sets the disk format text box from the quick-select combo.</summary>
        private void CmbFmtQuick_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbFmtQuick.SelectedItem != null) txtFormat.Text = cmbFmtQuick.SelectedItem.ToString();
        }

        /// <summary>Enables/disables the retry-count spinner alongside the retries checkbox.</summary>
        private void ChkRetries_CheckedChanged(object? sender, EventArgs e)
        {
            nudRetries.Enabled = chkRetries.Checked;
            SafeUpdatePreviews();
        }

        /// <summary>Enables/disables the head-0 offset spinner alongside its checkbox.</summary>
        private void ChkHead0Off_CheckedChanged(object? sender, EventArgs e)
        {
            nudHead0Off.Enabled = chkHead0Off.Checked;
            SafeUpdatePreviews();
        }

        /// <summary>Enables/disables the head-1 offset spinner alongside its checkbox.</summary>
        private void ChkHead1Off_CheckedChanged(object? sender, EventArgs e)
        {
            nudHead1Off.Enabled = chkHead1Off.Checked;
            SafeUpdatePreviews();
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
        /// Validates inputs and builds either the group result or the single-job result,
        /// cancelling the dialog result (without closing) if validation fails.
        /// </summary>
        private void BtnOk_Click(object? sender, EventArgs e)
        {
            bool ok = (chkUseGroup?.Checked ?? false)
                ? TryBuildGroupResult()
                : TryBuildSingleJobResult();
            if (!ok) DialogResult = DialogResult.None;
        }

        /// <summary>
        /// Validates the device-group inputs and, on success, assigns <see cref="GroupResult"/>.
        /// Shows a warning message box and returns <see langword="false"/> on any validation failure.
        /// </summary>
        private bool TryBuildGroupResult()
        {
            if (!(chkRepetitive?.Checked ?? false) ||
                !Models.FilePattern.HasTokens(txtFilePattern?.Text ?? ""))
            {
                MessageBox.Show(L10n.T("job_dlg.group_needs_repeat"),
                    L10n.T("job_dlg.group_cap"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            var members = ReadGroupMembers();
            string? err = GroupRepetitiveJob.Validate(members);
            if (err != null)
            {
                MessageBox.Show(L10n.T(err), L10n.T("job_dlg.group_cap"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            var missing = members.Find(m => !m.Device.IsConnected);
            if (missing != null)
            {
                MessageBox.Show(
                    string.Format(L10n.T("job_dlg.group_missing"),
                        missing.Device.ToString()),
                    L10n.T("job_dlg.group_cap"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            var template = BuildParameters();
            template.Device    = null;
            template.Drive     = null;
            template.ImageFile = null;

            var groupActions = new List<PostAction>();
            foreach (ListViewItem item in lvPostActions.Items)
                groupActions.Add((PostAction)item.Tag!);

            GroupResult = new GroupRepetitiveJob
            {
                JobType           = cmbJobType.SelectedIndex == 0 ? JobType.Read : JobType.Write,
                ParameterTemplate = template,
                PostActions       = groupActions,
                FilePattern       = txtFilePattern!.Text,
                OutputFolder      = txtOutputFolder?.Text ?? "",
                DateTimeFormat    = txtDtFormat?.Text ?? "yyyyMMdd_HHmmss",
                NextDiskNumber    = (int)(nudStartIndex?.Value ?? 1),
                Members           = members
            };
            return true;   // DialogResult stays OK; Result stays null
        }

        /// <summary>
        /// Validates the single-job inputs and, on success, builds the <see cref="GwJob"/> and assigns <see cref="Result"/>.
        /// Shows a warning message box and returns <see langword="false"/> if the image file is missing (non-repetitive mode).
        /// </summary>
        private bool TryBuildSingleJobResult()
        {
            if (string.IsNullOrWhiteSpace(txtImageFile.Text) &&
                !(chkRepetitive?.Checked ?? false))
            {
                MessageBox.Show(
                    L10n.T("job_dlg.missing_image"),
                    L10n.T("job_dlg.missing_image_cap"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
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
            return true;
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

        /// <summary>Removes the selected post-action row and re-numbers the remaining rows.</summary>
        private void BtnRemoveAction_Click(object? sender, EventArgs e)
        {
            if (lvPostActions.SelectedItems.Count > 0) lvPostActions.Items.Remove(lvPostActions.SelectedItems[0]);
            ReorderActions();
        }

        private void BtnMoveActionUp_Click(object? sender, EventArgs e) => MoveAction(-1);

        private void BtnMoveActionDown_Click(object? sender, EventArgs e) => MoveAction(1);

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

        /// <summary>Opens a folder picker for the repetitive-mode output folder.</summary>
        private void BtnBrowseFolder_Click(object? sender, EventArgs e)
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
        }

        /// <summary>
        /// Toggles the group-member controls and force-enables repetitive mode when a group is
        /// turned on. Device groups are read-only (imaging several source disks in parallel), so
        /// Job Type is locked to Read for as long as a group is in use.
        /// </summary>
        private void ChkUseGroup_CheckedChanged(object? sender, EventArgs e)
        {
            bool on = chkUseGroup.Checked;
            cmbGroupDevice.Enabled = on;
            cmbGroupDrive.Enabled  = on;
            lvGroupMembers.Enabled = on;
            if (on && !chkRepetitive.Checked) chkRepetitive.Checked = true;

            if (on) cmbJobType.SelectedIndex = 0;
            cmbJobType.Enabled = !on;
        }

        /// <summary>Adds the selected device/drive pair as a new group-member row.</summary>
        private void BtnGroupAdd_Click(object? sender, EventArgs e)
        {
            if (cmbGroupDevice.SelectedItem is not GreaseWeazleDevice dev) return;
            var item = new ListViewItem(dev.ToString());
            item.SubItems.Add(cmbGroupDrive.SelectedItem?.ToString() ?? "0");
            item.Tag = dev;
            lvGroupMembers.Items.Add(item);
        }

        /// <summary>Removes all selected group-member rows.</summary>
        private void BtnGroupRemove_Click(object? sender, EventArgs e)
        {
            foreach (ListViewItem it in lvGroupMembers.SelectedItems)
                lvGroupMembers.Items.Remove(it);
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

            preset.UseDeviceGroup = chkUseGroup?.Checked ?? false;
            preset.GroupMembers.Clear();
            if (lvGroupMembers != null)
                foreach (ListViewItem item in lvGroupMembers.Items)
                    if (item.Tag is GreaseWeazleDevice dev)
                        preset.GroupMembers.Add(new GroupMemberPreset
                        {
                            DeviceId   = dev.Id,
                            DeviceName = dev.ToString(),
                            Drive      = item.SubItems[1].Text
                        });
            return preset;
        }

        /// <summary>Reads the group member rows from the list view.</summary>
        /// <returns>Members in row order. Rows whose device is absent (null tag) are skipped.</returns>
        private List<DeviceGroupMember> ReadGroupMembers()
        {
            var members = new List<DeviceGroupMember>();
            foreach (ListViewItem item in lvGroupMembers.Items)
                if (item.Tag is GreaseWeazleDevice dev)
                    members.Add(new DeviceGroupMember
                    {
                        Device = dev,
                        Drive  = item.SubItems[1].Text
                    });
            return members;
        }

        /// <summary>
        /// Populates all dialog controls from a loaded <see cref="Models.JobPreset"/>.
        /// Preview updates are suppressed during loading and re-enabled at the end.
        /// </summary>
        /// <param name="preset">The preset to apply.</param>
        public void LoadFromPreset(Models.JobPreset preset)
        {
            _initialized = false;

            ApplyPresetMainFields(preset);
            ApplyPresetTrackFields(preset);
            ApplyPresetAdvancedFields(preset);
            ApplyPresetRepeatFields(preset);
            ApplyPresetGroupFields(preset);
            ApplyPresetPostActions(preset);

            _initialized = true;
            SafeUpdatePreviews();
            UpdatePatternPreview();
        }

        /// <summary>
        /// Applies device and job type fields from a loaded preset. Disk format is saved in the
        /// preset file but deliberately not restored here — the physical disk in the drive at
        /// load time may not match what was saved, so the user must re-pick a format from the
        /// quick-select combo (or type one) each time a preset is loaded.
        /// </summary>
        private void ApplyPresetMainFields(Models.JobPreset preset)
        {
            cmbJobType.SelectedIndex = preset.JobType == JobType.Read ? 0 : 1;
            txtFormat.Text = "";
            cmbFmtQuick.SelectedIndex = -1;

            if (!string.IsNullOrWhiteSpace(preset.Device))
                for (int i = 0; i < cmbDevice.Items.Count; i++)
                    if (cmbDevice.Items[i] is GreaseWeazleDevice d &&
                        d.SerialPort == preset.Device)
                    { cmbDevice.SelectedIndex = i; break; }
        }

        /// <summary>Applies cylinder range, head, step, hswap, and flippy head-offset fields from a loaded preset.</summary>
        private void ApplyPresetTrackFields(Models.JobPreset preset)
        {
            nudStartCyl.Value     = preset.StartCylinder ?? 0;
            nudEndCyl.Value       = preset.EndCylinder   ?? 79;
            cmbHead.SelectedIndex = preset.Head switch { 0 => 1, 1 => 2, _ => 0 };
            nudStep.Value         = preset.Step           ?? 1;
            chkHSwap.Checked      = preset.HSwap;
            chkHead0Off.Checked   = preset.Head0Offset.HasValue;
            nudHead0Off.Value     = preset.Head0Offset   ?? 0;
            chkHead1Off.Checked   = preset.Head1Offset.HasValue;
            nudHead1Off.Value     = preset.Head1Offset   ?? 0;
        }

        /// <summary>Applies revolutions, density, bitrate, retries, drive, and other read/write/advanced fields from a loaded preset.</summary>
        private void ApplyPresetAdvancedFields(Models.JobPreset preset)
        {
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
        }

        /// <summary>Applies repetitive-mode, file pattern, output folder, start index, date-time format, and preset name fields from a loaded preset.</summary>
        private void ApplyPresetRepeatFields(Models.JobPreset preset)
        {
            chkRepetitive.Checked = preset.RepetitiveMode;
            txtFilePattern.Text   = preset.FilePattern    ?? "";
            txtOutputFolder.Text  = preset.OutputFolder   ?? "";
            nudStartIndex.Value   = Math.Max(1, preset.StartIndex);
            txtDtFormat.Text      = preset.DateTimeFormat ?? "yyyyMMdd_HHmmss";
            if (txtPresetName != null) txtPresetName.Text = preset.PresetName;
        }

        /// <summary>Applies device-group member rows and the group-enabled flag from a loaded preset.</summary>
        private void ApplyPresetGroupFields(Models.JobPreset preset)
        {
            lvGroupMembers.Items.Clear();
            foreach (var gm in preset.GroupMembers)
            {
                var dev = _devices.Find(d => d.Id == gm.DeviceId)
                       ?? _devices.Find(d => d.ToString() == gm.DeviceName);
                var item = new ListViewItem(dev?.ToString() ?? gm.DeviceName + " ⚠");
                item.SubItems.Add(gm.Drive);
                item.Tag = dev;               // null when the device is absent
                if (dev == null) item.ForeColor = Color.FromArgb(220, 120, 80);
                lvGroupMembers.Items.Add(item);
            }
            chkUseGroup.Checked = preset.UseDeviceGroup;
        }

        /// <summary>Applies the post-action list from a loaded preset.</summary>
        private void ApplyPresetPostActions(Models.JobPreset preset)
        {
            if (lvPostActions == null) return;
            lvPostActions.Items.Clear();
            foreach (var ap in preset.PostActions)
                lvPostActions.Items.Add(ActionToItem(ap.ToPostAction()));
        }
    }
}
