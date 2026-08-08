using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GwCopyPro.Models;
using GwCopyPro.Services;

namespace GwCopyPro.Forms
{
    /// <summary>
    /// Insert-phase dialog for group repetitive jobs. Shows one row per group member,
    /// blinks the current drive's LED via <see cref="IDriveProber"/>, and advances to the
    /// next drive as soon as the user confirms a disk is inserted — there is no automated
    /// disk-presence check (gw.exe has no reliable one; see <see cref="BtnInserted_Click"/>).
    /// Lets the user include/exclude drives per batch.
    /// </summary>
    public partial class BatchInsertDialog : Form
    {
        /// <summary><see langword="true"/> when the user chose Start batch; <see langword="false"/> for Finish job.</summary>
        public bool StartBatchChosen { get; private set; }

        private readonly GroupRepetitiveJob      _group;
        private readonly IDriveProber            _prober = null!;
        private readonly BatchInsertStateMachine _sm;
        private readonly CancellationTokenSource _cts = new();
        private readonly System.Windows.Forms.Timer _blinkTimer = new() { Interval = 1500 };

        private readonly List<Label>    _stateLabels = new();
        private readonly List<CheckBox> _includeChecks = new();
        private Button _btnInserted = null!;
        private Button _btnStart    = null!;
        private bool   _busy;      // a blink gw call is in flight

        /// <summary>Design-time-only constructor. Do not use at runtime.</summary>
        public BatchInsertDialog()
        {
            _group = new GroupRepetitiveJob();
            _sm    = new BatchInsertStateMachine(new List<bool>());
            InitializeComponent();
            BuildDynamicContent();
        }

        /// <summary>Initialises the dialog for the group's next batch.</summary>
        /// <param name="group">The group job (member state is read and written).</param>
        /// <param name="prober">Prober used for blinking and disk detection.</param>
        public BatchInsertDialog(GroupRepetitiveJob group, IDriveProber prober)
        {
            _group  = group;
            _prober = prober;

            var included = new List<bool>();
            foreach (var m in group.Members) included.Add(m.IncludedThisBatch);
            _sm = new BatchInsertStateMachine(included);
            _sm.StateChanged += Sm_StateChanged;

            InitializeComponent();
            BuildDynamicContent();
            RenderRows();
            UpdateButtons();

            _blinkTimer.Tick += BlinkTimer_Tick;
            _blinkTimer.Start();
        }

        /// <summary>
        /// Builds the title, per-member rows, files label, and action buttons. Their positions
        /// and the form's own size depend on the group's member count, so this cannot be
        /// represented statically in the Designer surface.
        /// </summary>
        private void BuildDynamicContent()
        {
            int rows = _group.Members.Count;
            Text            = string.Format(L10n.T("batch.title"), _group.BatchNumber + 1);
            Size            = new Size(700, 214 + rows * 56);
            MinimumSize     = Size;
            MaximumSize     = Size;

            int y = 16;
            Controls.Add(new Label
            {
                Text      = string.Format(L10n.T("batch.title"), _group.BatchNumber + 1),
                Location  = new Point(14, y), Size = new Size(660, 20),
                Font      = new Font("Consolas", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 200, 255), BackColor = Color.Transparent
            });
            y += 30;

            for (int i = 0; i < rows; i++)
                y = AddMemberRow(i, y);

            var lblFiles = new Label
            {
                Text      = L10n.T("batch.files_label") + " " +
                            Models.FilePattern.Expand(_group.FilePattern,
                                _group.NextDiskNumber, _group.DateTimeFormat) + " …",
                Location  = new Point(14, y), Size = new Size(660, 16),
                Font      = new Font("Consolas", 7.5f),
                ForeColor = Color.FromArgb(90, 130, 170), BackColor = Color.Transparent
            };
            Controls.Add(lblFiles);
            y += 30;

            _btnInserted = MakeBtn(L10n.T("batch.btn_inserted"), 14, y, 210, 40,
                Color.FromArgb(25, 45, 80), Color.FromArgb(120, 175, 255), Color.FromArgb(50, 90, 160));
            _btnInserted.Click += BtnInserted_Click;

            _btnStart = MakeBtn(L10n.T("batch.btn_start"), 238, y, 230, 40,
                Color.FromArgb(18, 65, 32), Color.FromArgb(80, 230, 120), Color.FromArgb(45, 140, 75));
            _btnStart.Font = new Font("Consolas", 10f, FontStyle.Bold);
            _btnStart.Click += BtnStart_Click;

            var btnFinish = MakeBtn(L10n.T("batch.btn_finish"), 482, y, 198, 40,
                Color.FromArgb(55, 20, 20), Color.FromArgb(220, 90, 90), Color.FromArgb(120, 45, 45));
            btnFinish.Click += BtnFinish_Click;

            Controls.AddRange(new Control[] { _btnInserted, _btnStart, btnFinish });
        }

        /// <summary>Adds the checkbox, device label, last-batch-result label, and state label for one member row.</summary>
        /// <param name="i">Zero-based member index.</param>
        /// <param name="y">Top position for this row.</param>
        /// <returns>The top position for the next row.</returns>
        private int AddMemberRow(int i, int y)
        {
            var m = _group.Members[i];

            var chk = new CheckBox
            {
                Text      = L10n.T("batch.chk_include"),
                Checked   = m.IncludedThisBatch,
                Location  = new Point(14, y + 6),
                Size      = new Size(120, 20),
                Font      = new Font("Consolas", 8f),
                ForeColor = Color.FromArgb(130, 160, 200),
                BackColor = Color.Transparent
            };
            chk.CheckedChanged += Chk_CheckedChanged;
            _includeChecks.Add(chk);
            Controls.Add(chk);

            Controls.Add(new Label
            {
                Text      = $"{m.Device.Name} ({m.Device.SerialPort}) — drive {m.Drive}",
                Location  = new Point(140, y),
                Size      = new Size(320, 18),
                Font      = new Font("Consolas", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 190, 255),
                BackColor = Color.Transparent
            });

            string last = m.LastBatchFailed
                ? string.Format(L10n.T("batch.last_fail"), m.LastBatchError ?? "?")
                : m.LastBatchFile != null
                    ? string.Format(L10n.T("batch.last_ok"), Path.GetFileName(m.LastBatchFile))
                    : "";
            Controls.Add(new Label
            {
                Text      = last,
                Location  = new Point(140, y + 18),
                Size      = new Size(320, 15),
                Font      = new Font("Consolas", 7.5f),
                ForeColor = m.LastBatchFailed
                    ? Color.FromArgb(220, 90, 90) : Color.FromArgb(90, 120, 160),
                BackColor = Color.Transparent
            });

            var lblState = new Label
            {
                Location  = new Point(468, y + 4),
                Size      = new Size(212, 32),
                Font      = new Font("Consolas", 8.5f, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            _stateLabels.Add(lblState);
            Controls.Add(lblState);

            return y + 56;
        }

        /// <summary>Creates a flat-styled button.</summary>
        private static Button MakeBtn(string text, int x, int y, int w, int h,
            Color bg, Color fg, Color border)
        {
            var b = new Button
            {
                Text = text, Location = new Point(x, y), Size = new Size(w, h),
                FlatStyle = FlatStyle.Flat, BackColor = bg, ForeColor = fg,
                Font = new Font("Consolas", 8.5f)
            };
            b.FlatAppearance.BorderColor = border;
            return b;
        }

        /// <summary>Toggles a member's inclusion in the current batch.</summary>
        private void Chk_CheckedChanged(object? sender, EventArgs e)
        {
            int idx = _includeChecks.IndexOf((CheckBox)sender!);
            if (idx >= 0) _sm.SetIncluded(idx, _includeChecks[idx].Checked);
        }

        /// <summary>Refreshes all per-row state labels from the state machine.</summary>
        private void RenderRows()
        {
            for (int i = 0; i < _stateLabels.Count; i++)
            {
                var (text, color) = _sm.State(i) switch
                {
                    MemberInsertState.Blinking     =>
                        (L10n.T("batch.state_blinking"), Color.FromArgb(240, 200, 60)),
                    MemberInsertState.DiskDetected =>
                        (L10n.T("batch.state_detected"), Color.FromArgb(80, 215, 110)),
                    MemberInsertState.Excluded     =>
                        (L10n.T("batch.state_excluded"), Color.FromArgb(110, 120, 140)),
                    _                              =>
                        (L10n.T("batch.state_waiting"), Color.FromArgb(90, 120, 160))
                };
                _stateLabels[i].Text      = text;
                _stateLabels[i].ForeColor = color;
            }
        }

        /// <summary>Enables/disables the confirm and start buttons from the current state.</summary>
        private void UpdateButtons()
        {
            _btnInserted.Enabled = _sm.CurrentBlink != null;
            _btnStart.Enabled    = _sm.CanStart;
        }

        /// <summary>Re-renders rows and button state whenever the state machine changes.</summary>
        private void Sm_StateChanged()
        {
            RenderRows();
            UpdateButtons();
        }

        private async void BlinkTimer_Tick(object? sender, EventArgs e) => await BlinkTickAsync();

        /// <summary>Fires one LED blink pulse on the currently blinking drive.</summary>
        private async Task BlinkTickAsync()
        {
            if (_busy || _sm.CurrentBlink is not int i) return;
            var m = _group.Members[i];
            _busy = true;
            try   { await _prober.BlinkOnceAsync(m.Device.SerialPort, m.Drive, _cts.Token); }
            catch { }
            finally { _busy = false; }
        }

        /// <summary>
        /// Confirms the currently blinking drive and advances to the next one. There is no
        /// automated disk-presence check here: gw.exe has no reliable, dedicated way to detect
        /// whether a disk is inserted (its closest tool, <c>gw rpm</c>, is a spindle-RPM
        /// measurement utility that fails unpredictably — including via an unhandled exception
        /// upstream — when no disk is present, rather than reporting presence/absence cleanly).
        /// The user's own confirmation is authoritative; if a drive turns out to be empty or
        /// otherwise fails, imaging simply reports an error for that member without blocking the
        /// rest of the batch (see <see cref="GroupJobService.RunAsync"/>).
        /// </summary>
        private void BtnInserted_Click(object? sender, EventArgs e)
        {
            if (_sm.CurrentBlink is int i) _sm.MarkDetected(i);
        }

        private void BtnStart_Click(object? sender, EventArgs e) => CloseWithChoice(startBatch: true);

        private void BtnFinish_Click(object? sender, EventArgs e) => CloseWithChoice(startBatch: false);

        /// <summary>Writes dialog state back to the members and closes.</summary>
        private void CloseWithChoice(bool startBatch)
        {
            StartBatchChosen = startBatch;
            for (int i = 0; i < _group.Members.Count; i++)
            {
                _group.Members[i].IncludedThisBatch = _sm.IsIncluded(i);
                _group.Members[i].Verified =
                    _sm.State(i) == MemberInsertState.DiskDetected;
            }
            DialogResult = startBatch ? DialogResult.OK : DialogResult.Cancel;
        }
    }
}
