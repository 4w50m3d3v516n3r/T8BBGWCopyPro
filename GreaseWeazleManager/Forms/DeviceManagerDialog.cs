using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows.Forms;
using GwCopyPro.Models;
using GwCopyPro.Services;

namespace GwCopyPro.Forms
{
    /// <summary>
    /// Modal dialog that lets the user view, add, remove, and auto-detect
    /// <see cref="GreaseWeazleDevice"/> instances. Probes firmware via <c>gw.exe info</c>
    /// whenever a COM port is selected or a device is added manually.
    /// </summary>
    public class DeviceManagerDialog : Form
    {
        private readonly List<GreaseWeazleDevice> _devices;
        private readonly string _gwExePath;
        private ListView lvDevices    = null!;
        private ComboBox cmbPort      = null!;
        private TextBox  txtName      = null!;
        private Label    lblFwProbe   = null!;
        private Button   btnAdd       = null!;
        private Button   btnAutoDetect = null!;

        /// <summary>
        /// Initialises the dialog with the application's device list and the path to gw.exe.
        /// </summary>
        /// <param name="devices">Shared list of registered devices; modified in-place by this dialog.</param>
        /// <param name="gwExePath">Path to <c>gw.exe</c> used for firmware probing.</param>
        public DeviceManagerDialog(List<GreaseWeazleDevice> devices, string gwExePath)
        {
            _devices   = devices;
            _gwExePath = gwExePath;
            InitializeComponent();
            RefreshList();
        }

        /// <summary>Builds all child controls and the device list view.</summary>
        private void InitializeComponent()
        {
            Text            = L10n.T("devmgr.title");
            Size            = new Size(660, 520);
            BackColor       = Color.FromArgb(18, 22, 32);
            ForeColor       = Color.FromArgb(180, 210, 255);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition   = FormStartPosition.CenterParent;
            MaximizeBox     = false;

            Controls.Add(new Label
            {
                Text      = L10n.T("devmgr.heading"),
                Location  = new Point(10, 12),
                AutoSize  = true,
                Font      = new Font("Consolas", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 200, 255)
            });

            btnAutoDetect = MakeBtn(L10n.T("devmgr.auto_detect"), 430, 10, 210, 28,
                Color.FromArgb(20, 50, 90), Color.FromArgb(100, 180, 255), Color.FromArgb(50, 100, 200));
            btnAutoDetect.Click += BtnAutoDetect_Click;
            Controls.Add(btnAutoDetect);

            lvDevices = new ListView
            {
                Location      = new Point(10, 46),
                Size          = new Size(624, 280),
                View          = View.Details,
                FullRowSelect = true,
                BackColor     = Color.FromArgb(18, 22, 32),
                ForeColor     = Color.FromArgb(180, 210, 255),
                Font          = new Font("Consolas", 8.5f),
                BorderStyle   = BorderStyle.FixedSingle
            };
            lvDevices.Columns.Add(L10n.T("devmgr.col_name"),   150);
            lvDevices.Columns.Add(L10n.T("devmgr.col_port"),    70);
            lvDevices.Columns.Add(L10n.T("devmgr.col_fw"),     110);
            lvDevices.Columns.Add(L10n.T("devmgr.col_hwid"),   200);
            lvDevices.Columns.Add(L10n.T("devmgr.col_status"),  80);
            Controls.Add(lvDevices);

            int y = 340;
            Controls.Add(MakeLbl(L10n.T("devmgr.port"), 10, y + 4));
            cmbPort = new ComboBox
            {
                Location      = new Point(60, y),
                Size          = new Size(100, 22),
                BackColor     = Color.FromArgb(28, 34, 48),
                ForeColor     = Color.FromArgb(200, 230, 255),
                FlatStyle     = FlatStyle.Flat,
                Font          = new Font("Consolas", 8.5f),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            PopulateComPorts();
            cmbPort.SelectedIndexChanged += CmbPort_SelectedIndexChanged;
            Controls.Add(cmbPort);

            Controls.Add(MakeLbl(L10n.T("devmgr.name"), 175, y + 4));
            txtName = new TextBox
            {
                Location    = new Point(225, y),
                Size        = new Size(180, 22),
                BackColor   = Color.FromArgb(28, 34, 48),
                ForeColor   = Color.FromArgb(200, 230, 255),
                BorderStyle = BorderStyle.FixedSingle,
                Font        = new Font("Consolas", 8.5f),
                Text        = "GreaseWeazle"
            };
            Controls.Add(txtName);

            btnAdd = MakeBtn(L10n.T("devmgr.add"), 415, y, 90, 22,
                Color.FromArgb(25, 60, 35), Color.FromArgb(100, 220, 130), Color.FromArgb(50, 120, 70));
            btnAdd.Click += BtnAdd_Click;
            Controls.Add(btnAdd);

            y += 30;
            lblFwProbe = new Label
            {
                Location  = new Point(10, y),
                Size      = new Size(624, 18),
                Font      = new Font("Consolas", 8f),
                ForeColor = Color.FromArgb(100, 160, 220),
                BackColor = Color.Transparent,
                Text      = L10n.T("devmgr.probe_hint")
            };
            Controls.Add(lblFwProbe);

            y += 32;
            var btnRemove = MakeBtn(L10n.T("devmgr.remove"), 10, y, 150, 26,
                Color.FromArgb(60, 20, 20), Color.FromArgb(220, 80, 80), Color.FromArgb(100, 40, 40));
            btnRemove.Click += (s, e) =>
            {
                if (lvDevices.SelectedItems.Count > 0)
                {
                    _devices.Remove((GreaseWeazleDevice)lvDevices.SelectedItems[0].Tag!);
                    RefreshList();
                }
            };

            var btnRefresh = MakeBtn(L10n.T("devmgr.refresh"), 170, y, 150, 26,
                Color.FromArgb(25, 40, 70), Color.FromArgb(100, 160, 240), Color.FromArgb(50, 80, 140));
            btnRefresh.Click += (s, e) => PopulateComPorts();

            var btnClose = MakeBtn(L10n.T("devmgr.close"), 520, y, 114, 26,
                Color.FromArgb(30, 40, 60), Color.White, Color.FromArgb(60, 80, 120));
            btnClose.DialogResult = DialogResult.OK;

            Controls.AddRange(new Control[] { btnRemove, btnRefresh, btnClose });
        }

        /// <summary>
        /// Runs WMI auto-detection on a background thread, queries firmware for each new device,
        /// adds them to <see cref="_devices"/>, and refreshes the list view.
        /// </summary>
        private async void BtnAutoDetect_Click(object? sender, EventArgs e)
        {
            btnAutoDetect.Enabled = false;
            btnAutoDetect.Text    = L10n.T("devmgr.scanning");
            lblFwProbe.ForeColor  = Color.FromArgb(200, 180, 60);
            lblFwProbe.Text       = L10n.T("status.scanning");

            try
            {
                var detected = await Task.Run(() => GwDetector.GetAllGwDevicesConnected());

                if (detected.Count == 0)
                {
                    lblFwProbe.ForeColor = Color.FromArgb(200, 120, 60);
                    lblFwProbe.Text      = L10n.T("devmgr.none_found");
                    return;
                }

                int added = 0;
                foreach (var props in detected)
                {
                    if (_devices.Exists(d => d.SerialPort.Equals(
                            props.DeviceComport, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    lblFwProbe.Text = string.Format(L10n.T("status.querying_fw"), props.DeviceComport);
                    var dev = await GwDetector.BuildDeviceAsync(props, _gwExePath);
                    _devices.Add(dev);
                    added++;
                }

                RefreshList();
                lblFwProbe.ForeColor = Color.FromArgb(80, 220, 120);
                lblFwProbe.Text = added > 0
                    ? string.Format(L10n.T("devmgr.added_n"), added)
                    : L10n.T("devmgr.all_registered");
            }
            catch (Exception ex)
            {
                lblFwProbe.ForeColor = Color.FromArgb(230, 80, 80);
                lblFwProbe.Text      = string.Format(L10n.T("devmgr.detect_error"), ex.Message);
            }
            finally
            {
                btnAutoDetect.Enabled = true;
                btnAutoDetect.Text    = L10n.T("devmgr.auto_detect");
            }
        }

        /// <summary>
        /// Probes the firmware version of the newly selected COM port and updates
        /// the status label. Disables the Add button during the probe.
        /// </summary>
        private async void CmbPort_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbPort.SelectedItem is not string port) return;

            lblFwProbe.ForeColor = Color.FromArgb(200, 180, 60);
            lblFwProbe.Text      = string.Format(L10n.T("devmgr.probing"), port);
            btnAdd.Enabled       = false;

            string fw = await GwDetector.QueryFirmwareAsync(_gwExePath, port);

            lblFwProbe.ForeColor = fw.StartsWith("Error") || fw == "Unknown" || fw == "Timeout"
                ? Color.FromArgb(220, 120, 60)
                : Color.FromArgb(80, 220, 120);
            lblFwProbe.Text = string.Format(L10n.T("devmgr.fw_result"), port, fw);
            btnAdd.Enabled  = true;
        }

        /// <summary>
        /// Adds a new device for the selected COM port, probing firmware first,
        /// then refreshes the list view.
        /// </summary>
        private async void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (cmbPort.SelectedItem is not string port)
            {
                MessageBox.Show(L10n.T("devmgr.missing_port"), "",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnAdd.Enabled       = false;
            lblFwProbe.ForeColor = Color.FromArgb(200, 180, 60);
            lblFwProbe.Text      = string.Format(L10n.T("devmgr.probing"), port);

            string fw = await GwDetector.QueryFirmwareAsync(_gwExePath, port);

            _devices.Add(new GreaseWeazleDevice
            {
                Name            = string.IsNullOrWhiteSpace(txtName.Text) ? "GreaseWeazle" : txtName.Text,
                SerialPort      = port,
                IsConnected     = true,
                FirmwareVersion = fw
            });
            RefreshList();

            lblFwProbe.ForeColor = Color.FromArgb(80, 220, 120);
            lblFwProbe.Text      = string.Format(L10n.T("devmgr.added_manual"), port, fw);
            btnAdd.Enabled       = true;
        }

        /// <summary>Repopulates the COM port combo box with all ports currently available on the system.</summary>
        private void PopulateComPorts()
        {
            cmbPort.Items.Clear();
            foreach (var p in SerialPort.GetPortNames())
                cmbPort.Items.Add(p);
            if (cmbPort.Items.Count > 0) cmbPort.SelectedIndex = 0;
        }

        /// <summary>Rebuilds the list view rows from the current contents of <see cref="_devices"/>.</summary>
        private void RefreshList()
        {
            lvDevices.Items.Clear();
            foreach (var dev in _devices)
            {
                var item = new ListViewItem(dev.Name);
                item.SubItems.Add(dev.SerialPort);
                item.SubItems.Add(dev.FirmwareVersion);
                item.SubItems.Add(dev.HardwareId);
                item.SubItems.Add(dev.IsConnected ? L10n.T("devmgr.status_ok") : L10n.T("devmgr.status_no"));
                item.Tag       = dev;
                item.ForeColor = dev.IsConnected
                    ? Color.FromArgb(100, 220, 130)
                    : Color.FromArgb(180, 100, 100);
                lvDevices.Items.Add(item);
            }
        }

        /// <summary>Creates a styled field-caption label.</summary>
        private static Label MakeLbl(string text, int x, int y) => new()
        {
            Text = text, Location = new Point(x, y), AutoSize = true,
            Font = new Font("Consolas", 8f), ForeColor = Color.FromArgb(130, 160, 200)
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
    }
}
