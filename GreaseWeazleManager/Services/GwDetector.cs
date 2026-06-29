using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GwCopyPro.Models;

namespace GwCopyPro.Services
{
    /// <summary>Raw WMI properties extracted for a detected GreaseWeazle USB device.</summary>
    public class GwDeviceProps
    {
        /// <summary>Hardware device-ID fragment starting from the "GW" prefix in the PnP device ID.</summary>
        public string DeviceId { get; set; } = "";

        /// <summary>COM port name extracted from the WMI device name string (e.g. <c>COM3</c>).</summary>
        public string DeviceComport { get; set; } = "";
    }

    /// <summary>
    /// Detects GreaseWeazle USB devices via WMI and queries their firmware version via <c>gw.exe info</c>.
    /// </summary>
    public static class GwDetector
    {
        /// <summary>
        /// Queries WMI for all USB serial devices whose PnP device ID contains "GW",
        /// returning one <see cref="GwDeviceProps"/> per match.
        /// Returns an empty list if WMI is unavailable.
        /// </summary>
        /// <returns>List of detected GreaseWeazle device properties.</returns>
        public static List<GwDeviceProps> GetAllGwDevicesConnected()
        {
            var result = new List<GwDeviceProps>();
            try
            {
                string query = "SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%(COM%)' AND Name LIKE '%USB%'";
                using var searcher = new ManagementObjectSearcher(query);
                foreach (ManagementObject obj in searcher.Get())
                {
#pragma warning disable CS8602
                    string deviceId = obj["DeviceID"]?.ToString() ?? "";
                    string name     = obj["Name"]?.ToString()     ?? "";
                    if (deviceId.Contains("GW", StringComparison.OrdinalIgnoreCase))
                    {
                        int gwIdx  = deviceId.IndexOf("GW", StringComparison.OrdinalIgnoreCase);
                        int comIdx = name.IndexOf("COM", StringComparison.OrdinalIgnoreCase);

                        if (comIdx >= 0)
                        {
                            string comRaw = name.Substring(comIdx);
                            var comMatch  = Regex.Match(comRaw, @"COM\d+");
                            result.Add(new GwDeviceProps
                            {
                                DeviceId      = deviceId.Substring(gwIdx),
                                DeviceComport = comMatch.Success ? comMatch.Value : comRaw[..Math.Min(5, comRaw.Length)]
                            });
                        }
                    }
#pragma warning restore CS8602
                }
            }
            catch { }
            return result;
        }

        /// <summary>
        /// Runs <c>gw.exe info --device &lt;comPort&gt;</c> and returns the firmware version string.
        /// </summary>
        /// <param name="gwExePath">Path to the <c>gw.exe</c> binary.</param>
        /// <param name="comPort">COM port to query (e.g. <c>COM3</c>).</param>
        /// <returns>
        /// A version string such as <c>"v1.6"</c>, or <c>"Unknown"</c>, <c>"Timeout"</c>,
        /// or <c>"Error: …"</c> on failure.
        /// </returns>
        public static async Task<string> QueryFirmwareAsync(string gwExePath, string comPort)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName               = gwExePath,
                    Arguments              = $"info --device {comPort}",
                    UseShellExecute        = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    CreateNoWindow         = true
                };

                using var proc = Process.Start(psi);
                if (proc == null) return "Unknown";

                var outputTask = proc.StandardOutput.ReadToEndAsync();
                var errorTask  = proc.StandardError.ReadToEndAsync();

                bool exited = await Task.WhenAny(
                    proc.WaitForExitAsync(),
                    Task.Delay(5000)
                ) == proc.WaitForExitAsync();

                if (!exited)
                {
                    try { proc.Kill(); } catch { }
                    return "Timeout";
                }

                string output = await outputTask + await errorTask;
                return ParseFirmwareVersion(output);
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Builds a <see cref="GreaseWeazleDevice"/> from the supplied WMI properties by
        /// querying the firmware version and populating all fields.
        /// </summary>
        /// <param name="props">WMI properties identifying the device.</param>
        /// <param name="gwExePath">Path to <c>gw.exe</c> used for the firmware query.</param>
        /// <returns>A fully populated <see cref="GreaseWeazleDevice"/> marked as connected.</returns>
        public static async Task<GreaseWeazleDevice> BuildDeviceAsync(
            GwDeviceProps props, string gwExePath)
        {
            string fw = await QueryFirmwareAsync(gwExePath, props.DeviceComport);
            return new GreaseWeazleDevice
            {
                Name            = $"GreaseWeazle ({props.DeviceComport})",
                SerialPort      = props.DeviceComport,
                IsConnected     = true,
                FirmwareVersion = fw,
                HardwareId      = props.DeviceId
            };
        }

        /// <summary>
        /// Parses the output of <c>gw.exe info</c> to extract a firmware version string.
        /// Tries several common patterns before falling back to the first non-empty output line.
        /// </summary>
        /// <param name="output">Combined stdout + stderr from <c>gw.exe info</c>.</param>
        /// <returns>A version string prefixed with <c>"v"</c>, or <c>"Unknown"</c>.</returns>
        private static string ParseFirmwareVersion(string output)
        {
            if (string.IsNullOrWhiteSpace(output)) return "Unknown";

            var patterns = new[]
            {
                @"[Ff]irmware[:\s]+v?([\d.]+)",
                @"[Vv]ersion[:\s]+v?([\d.]+)",
                @"fw[:\s]+v?([\d.]+)",
                @"v([\d]+\.[\d]+)"
            };

            foreach (var pattern in patterns)
            {
                var m = Regex.Match(output, pattern);
                if (m.Success) return "v" + m.Groups[1].Value;
            }

            foreach (var line in output.Split('\n'))
            {
                string t = line.Trim();
                if (!string.IsNullOrEmpty(t)) return t[..Math.Min(40, t.Length)];
            }

            return "Unknown";
        }
    }
}
