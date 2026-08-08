using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GwCopyPro.Models;
using GwCopyPro.Services;

namespace GwCopyPro.Controls
{
    /// <summary>
    /// A dark-themed card panel representing a single <see cref="GreaseWeazleDevice"/>.
    /// Displays the device name, COM port, firmware version, and connection status,
    /// and features a pulsing LED that animates while the device is connected.
    /// Provides buttons to start a new job or remove the device.
    /// </summary>
    public partial class DevicePanel : UserControl
    {
        private readonly GreaseWeazleDevice _device = new();
        private Action<GreaseWeazleDevice>? _removeCallback;
        private Action<GreaseWeazleDevice>? _newJobCallback;
        private Action<GreaseWeazleDevice>? _blinkCallback;
        private float _pulse    = 0f;
        private bool  _pulseDir = true;

        /// <summary>The <see cref="GreaseWeazleDevice"/> this panel represents.</summary>
        public GreaseWeazleDevice Device => _device;

        /// <summary>Design-time-only constructor. Do not use at runtime.</summary>
        public DevicePanel()
        {
            InitializeComponent();
            SetDoubleBuffered();
        }

        /// <summary>
        /// Initialises the device panel, building all child controls and starting
        /// the LED pulse timer when the device is connected.
        /// </summary>
        /// <param name="device">The device whose information is displayed.</param>
        /// <param name="removeCallback">Invoked when the user clicks the remove button.</param>
        /// <param name="newJobCallback">Invoked when the user clicks the New Job button.</param>
        /// <param name="blinkCallback">Invoked when the user clicks the Blink button to identify the drive.</param>
        public DevicePanel(
            GreaseWeazleDevice       device,
            Action<GreaseWeazleDevice> removeCallback,
            Action<GreaseWeazleDevice> newJobCallback,
            Action<GreaseWeazleDevice> blinkCallback)
        {
            _device         = device;
            _removeCallback = removeCallback;
            _newJobCallback = newJobCallback;
            _blinkCallback  = blinkCallback;

            InitializeComponent();
            SetDoubleBuffered();
            PopulateContent(device);

            if (device.IsConnected) _pulseTimer.Start();
        }

        /// <summary>
        /// Enables flicker-free custom drawing. Set here rather than in InitializeComponent
        /// because the WinForms Designer's CodeDom reader cannot represent a bare method call.
        /// </summary>
        private void SetDoubleBuffered() => SetStyle(ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

        /// <summary>Fills in the per-instance text that InitializeComponent cannot know statically.</summary>
        private void PopulateContent(GreaseWeazleDevice device)
        {
            _lblName.Text = device.Name;
            _lblPort.Text = $"Port: {device.SerialPort}";
            _lblFw.Text   = $"FW: {device.FirmwareVersion}";

            _lblConn.ForeColor = device.IsConnected
                ? Color.FromArgb(80, 200, 100)
                : Color.FromArgb(200, 80, 80);
            _lblConn.Text = device.IsConnected
                ? L10n.T("dev.connected")
                : L10n.T("dev.disconnected");

            _btnBlink.Enabled = device.IsConnected;
        }

        /// <summary>Disables the Blink button while an identify sequence runs on this device.</summary>
        public void SetBlinkBusy(bool busy) => _btnBlink.Enabled = !busy && _device.IsConnected;

        private void BtnNewJob_Click(object? sender, EventArgs e) => _newJobCallback?.Invoke(_device);

        private void BtnRemove_Click(object? sender, EventArgs e) => _removeCallback?.Invoke(_device);

        private void BtnBlink_Click(object? sender, EventArgs e) => _blinkCallback?.Invoke(_device);

        /// <summary>Advances the LED pulse animation by one step and repaints the status light.</summary>
        private void PulseTimer_Tick(object? sender, EventArgs e)
        {
            if (_pulseDir) _pulse += 0.06f; else _pulse -= 0.06f;
            if (_pulse >= 1f) _pulseDir = false;
            if (_pulse <= 0f) _pulseDir = true;
            _statusLight.Invalidate();
            _statusLight.Refresh();
        }

        /// <summary>
        /// Paints the status LED as a filled ellipse whose green intensity pulses smoothly
        /// when connected, or a static dim red when disconnected.
        /// </summary>
        private void StatusLight_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color c = _device.IsConnected
                ? Color.FromArgb(Math.Clamp((int)(80 + _pulse * 170), 0, 255), 200, 100)
                : Color.FromArgb(180, 60, 60);
            using var b = new SolidBrush(c);
            g.FillEllipse(b, 1, 1, 13, 13);
        }

        /// <summary>
        /// Paints the panel border and a 3-pixel horizontal gradient accent bar at the top.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            using var pen = new Pen(Color.FromArgb(40, 60, 90), 1f);
            g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            using var accentBrush = new LinearGradientBrush(
                new Point(0, 0), new Point(Width, 0),
                Color.FromArgb(60, 120, 200), Color.FromArgb(20, 50, 100));
            g.FillRectangle(accentBrush, 0, 0, Width, 3);
        }
    }
}
