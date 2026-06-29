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
    public class DevicePanel : Panel
    {
        private readonly GreaseWeazleDevice   _device;
        private readonly Label                _lblName;
        private readonly Label                _lblPort;
        private readonly Label                _lblFw;
        private readonly Label                _lblConn;
        private readonly PictureBox           _statusLight;
        private readonly Button               _btnRemove;
        private readonly Button               _btnNewJob;
        private readonly System.Windows.Forms.Timer _pulseTimer;
        private Action<GreaseWeazleDevice>?   _removeCallback;
        private Action<GreaseWeazleDevice>?   _newJobCallback;
        private float _pulse    = 0f;
        private bool  _pulseDir = true;

        /// <summary>The <see cref="GreaseWeazleDevice"/> this panel represents.</summary>
        public GreaseWeazleDevice Device => _device;

        /// <summary>
        /// Initialises the device panel, building all child controls and starting
        /// the LED pulse timer when the device is connected.
        /// </summary>
        /// <param name="device">The device whose information is displayed.</param>
        /// <param name="removeCallback">Invoked when the user clicks the remove button.</param>
        /// <param name="newJobCallback">Invoked when the user clicks the New Job button.</param>
        public DevicePanel(
            GreaseWeazleDevice       device,
            Action<GreaseWeazleDevice> removeCallback,
            Action<GreaseWeazleDevice> newJobCallback)
        {
            _device         = device;
            _removeCallback = removeCallback;
            _newJobCallback = newJobCallback;

            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);

            BackColor = Color.FromArgb(20, 24, 34);
            Size      = new Size(210, 136);
            Margin    = new Padding(6);

            _statusLight = new PictureBox
            {
                Location  = new Point(182, 10),
                Size      = new Size(16, 16),
                BackColor = Color.Transparent
            };
            _statusLight.Paint += StatusLight_Paint;

            _lblName = new Label
            {
                Font      = new Font("Consolas", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 200, 255),
                AutoSize  = false,
                Size      = new Size(172, 18),
                Location  = new Point(10, 12),
                BackColor = Color.Transparent,
                Text      = device.Name
            };

            _lblPort = new Label
            {
                Font      = new Font("Consolas", 8f),
                ForeColor = Color.FromArgb(120, 150, 190),
                AutoSize  = false,
                Size      = new Size(190, 15),
                Location  = new Point(10, 32),
                BackColor = Color.Transparent,
                Text      = $"Port: {device.SerialPort}"
            };

            _lblFw = new Label
            {
                Font      = new Font("Consolas", 7.5f),
                ForeColor = Color.FromArgb(90, 120, 150),
                AutoSize  = false,
                Size      = new Size(190, 15),
                Location  = new Point(10, 49),
                BackColor = Color.Transparent,
                Text      = $"FW: {device.FirmwareVersion}"
            };

            _lblConn = new Label
            {
                Font      = new Font("Consolas", 7.5f),
                ForeColor = device.IsConnected
                                ? Color.FromArgb(80, 200, 100)
                                : Color.FromArgb(200, 80, 80),
                AutoSize  = false,
                Size      = new Size(190, 14),
                Location  = new Point(10, 66),
                BackColor = Color.Transparent,
                Text      = device.IsConnected
                                ? L10n.T("dev.connected")
                                : L10n.T("dev.disconnected")
            };

            _btnNewJob = new Button
            {
                Text      = L10n.T("dev.new_job"),
                Location  = new Point(10, 84),
                Size      = new Size(190, 22),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(18, 60, 32),
                ForeColor = Color.FromArgb(90, 220, 120),
                Font      = new Font("Consolas", 8f, FontStyle.Bold)
            };
            _btnNewJob.FlatAppearance.BorderColor = Color.FromArgb(40, 120, 65);
            _btnNewJob.Click += (s, e) => _newJobCallback?.Invoke(_device);

            _btnRemove = new Button
            {
                Text      = L10n.T("dev.remove"),
                Location  = new Point(10, 110),
                Size      = new Size(60, 18),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(60, 20, 20),
                ForeColor = Color.FromArgb(200, 80, 80),
                Font      = new Font("Consolas", 8f)
            };
            _btnRemove.FlatAppearance.BorderColor = Color.FromArgb(100, 40, 40);
            _btnRemove.Click += (s, e) => _removeCallback?.Invoke(_device);

            Controls.AddRange(new Control[]
            {
                _lblName, _lblPort, _lblFw, _lblConn,
                _btnNewJob, _btnRemove, _statusLight
            });

            _pulseTimer = new System.Windows.Forms.Timer { Interval = 50 };
            _pulseTimer.Tick += (s, e) =>
            {
                if (_pulseDir) _pulse += 0.06f; else _pulse -= 0.06f;
                if (_pulse >= 1f) _pulseDir = false;
                if (_pulse <= 0f) _pulseDir = true;
                _statusLight.Invalidate();
                _statusLight.Refresh();
            };

            if (device.IsConnected) _pulseTimer.Start();
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

        /// <summary>
        /// Stops and disposes the pulse timer before releasing other resources.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _pulseTimer?.Stop();
                _pulseTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
