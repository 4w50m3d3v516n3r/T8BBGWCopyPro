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
    public partial class DeviceManagerDialog : Form
    {
        private readonly List<GreaseWeazleDevice> _devices = new();
        private readonly string _gwExePath = string.Empty;

        /// <summary>Design-time-only constructor. Do not use at runtime.</summary>
        public DeviceManagerDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialises the dialog with the application's device list and the path to gw.exe.
        /// </summary>
        /// <param name="devices">Shared list of registered devices; modified in-place by this dialog.</param>
        /// <param name="gwExePath">Path to <c>gw.exe</c> used for firmware probing.</param>
        public DeviceManagerDialog(List<GreaseWeazleDevice> devices, string gwExePath)
        {
            _devices = devices;
            _gwExePath = gwExePath;
            InitializeComponent();
            PopulateComPorts();
            RefreshList();
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

            try
            {
                string fw = await GwDetector.QueryFirmwareAsync(_gwExePath, port);

                lblFwProbe.ForeColor = fw.StartsWith("Error") || fw == "Unknown" || fw == "Timeout"
                    ? Color.FromArgb(220, 120, 60)
                    : Color.FromArgb(80, 220, 120);
                lblFwProbe.Text = string.Format(L10n.T("devmgr.fw_result"), port, fw);
            }
            catch (Exception ex)
            {
                lblFwProbe.ForeColor = Color.FromArgb(230, 80, 80);
                lblFwProbe.Text      = string.Format(L10n.T("devmgr.detect_error"), ex.Message);
            }
            finally
            {
                btnAdd.Enabled = true;
            }
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

            try
            {
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
            }
            catch (Exception ex)
            {
                lblFwProbe.ForeColor = Color.FromArgb(230, 80, 80);
                lblFwProbe.Text      = string.Format(L10n.T("devmgr.detect_error"), ex.Message);
            }
            finally
            {
                btnAdd.Enabled = true;
            }
        }

        /// <summary>Removes the currently selected device from the list, if any, and refreshes the view.</summary>
        private void BtnRemove_Click(object? sender, EventArgs e)
        {
            if (lvDevices.SelectedItems.Count > 0)
            {
                _devices.Remove((GreaseWeazleDevice)lvDevices.SelectedItems[0].Tag!);
                RefreshList();
            }
        }

        /// <summary>Re-scans the system's COM ports into the port combo box.</summary>
        private void BtnRefresh_Click(object? sender, EventArgs e) => PopulateComPorts();

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
    }
}
