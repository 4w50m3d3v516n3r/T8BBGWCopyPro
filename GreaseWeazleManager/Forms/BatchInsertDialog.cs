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
    /// blinks the current drive's LED via <see cref="IDriveProber"/>, verifies disk
    /// insertion on confirmation, and lets the user include/exclude drives per batch.
    /// </summary>
    public class BatchInsertDialog : Form
    {
        /// <summary><see langword="true"/> when the user chose Start batch; <see langword="false"/> for Finish job.</summary>
        public bool StartBatchChosen { get; private set; }

        private readonly GroupRepetitiveJob      _group;
        private readonly IDriveProber            _prober;
        private readonly BatchInsertStateMachine _sm;
        private readonly CancellationTokenSource _cts = new();
        private readonly System.Windows.Forms.Timer _blinkTimer;

        private readonly List<Label>    _stateLabels = new();
        private readonly List<CheckBox> _includeChecks = new();
        private Label  _lblHint     = null!;
        private Button _btnInserted = null!;
        private Button _btnStart    = null!;
        private bool   _busy;      // any gw call is in flight (blink or probe)
        private bool   _probing;   // a probe specifically — disables the confirm button

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
            _sm.StateChanged += () => { RenderRows(); UpdateButtons(); };

            InitializeComponent();
            RenderRows();
            UpdateButtons();

            _blinkTimer = new System.Windows.Forms.Timer { Interval = 1500 };
            _blinkTimer.Tick += async (s, e) => await BlinkTickAsync();
            _blinkTimer.Start();
        }

        /// <summary>Builds and lays out all child controls.</summary>
        private void InitializeComponent()
        {
            int rows = _group.Members.Count;
            Text            = string.Format(L10n.T("batch.title"), _group.BatchNumber + 1);
            Size            = new Size(700, 240 + rows * 56);
            MinimumSize     = Size;
            MaximumSize     = Size;
            BackColor       = Color.FromArgb(18, 22, 32);
            ForeColor       = Color.FromArgb(180, 210, 255);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition   = FormStartPosition.CenterParent;
            MaximizeBox     = false;
            ControlBox      = false;

            var accent = new Panel
            {
                Dock = DockStyle.Top, Height = 4,
                BackColor = Color.FromArgb(40, 160, 80)
            };
            Controls.Add(accent);

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
                int idx = i;
                chk.CheckedChanged += (s, e) => _sm.SetIncluded(idx, chk.Checked);
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

                y += 56;
            }

            _lblHint = new Label
            {
                Text      = "",
                Location  = new Point(14, y), Size = new Size(660, 18),
                Font      = new Font("Consolas", 8f, FontStyle.Italic),
                ForeColor = Color.FromArgb(220, 180, 80), BackColor = Color.Transparent
            };
            Controls.Add(_lblHint);
            y += 26;

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
            _btnInserted.Click += async (s, e) => await ConfirmInsertedAsync();

            _btnStart = MakeBtn(L10n.T("batch.btn_start"), 238, y, 230, 40,
                Color.FromArgb(18, 65, 32), Color.FromArgb(80, 230, 120), Color.FromArgb(45, 140, 75));
            _btnStart.Font = new Font("Consolas", 10f, FontStyle.Bold);
            _btnStart.Click += (s, e) => CloseWithChoice(startBatch: true);

            var btnFinish = MakeBtn(L10n.T("batch.btn_finish"), 482, y, 198, 40,
                Color.FromArgb(55, 20, 20), Color.FromArgb(220, 90, 90), Color.FromArgb(120, 45, 45));
            btnFinish.Click += (s, e) => CloseWithChoice(startBatch: false);

            Controls.AddRange(new Control[] { _btnInserted, _btnStart, btnFinish });
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
            _btnInserted.Enabled = _sm.CurrentBlink != null && !_probing;
            _btnStart.Enabled    = _sm.CanStart;
        }

        /// <summary>Fires one LED blink pulse on the currently blinking drive.</summary>
        private async Task BlinkTickAsync()
        {
            if (_busy || _sm.CurrentBlink is not int i) return;
            var m = _group.Members[i];
            _busy = true;
            try   { await _prober.BlinkOnceAsync(m.Device.SerialPort, m.Drive, _cts.Token); }
            catch { }
            finally { _busy = false; UpdateButtons(); }
        }

        /// <summary>
        /// Probes the currently blinking drive. On success advances the queue; otherwise
        /// keeps the drive blinking and shows the matching hint.
        /// </summary>
        private async Task ConfirmInsertedAsync()
        {
            if (_sm.CurrentBlink is not int i || _busy) return;
            var m = _group.Members[i];
            _busy    = true;
            _probing = true;
            _btnInserted.Enabled = false;
            _lblHint.Text = "";
            try
            {
                var r = await _prober.ProbeDiskAsync(m.Device.SerialPort, m.Drive, _cts.Token);
                switch (r)
                {
                    case DiskProbeResult.DiskPresent:
                        _sm.MarkDetected(i);
                        break;
                    case DiskProbeResult.NoDisk:
                        _lblHint.Text = L10n.T("batch.no_disk_hint");
                        break;
                    default:
                        _lblHint.Text = L10n.T("batch.dev_error_hint");
                        break;
                }
            }
            finally { _busy = false; _probing = false; UpdateButtons(); }
        }

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

        /// <summary>Stops the blink timer and cancels in-flight gw calls.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _blinkTimer?.Stop();
                _blinkTimer?.Dispose();
                _cts.Cancel();
                _cts.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
