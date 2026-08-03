using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GwCopyPro.Services
{
    /// <summary>Outcome of a disk-presence probe on one drive.</summary>
    public enum DiskProbeResult
    {
        /// <summary>gw.exe reported an RPM value — a spinning disk is present.</summary>
        DiskPresent,
        /// <summary>gw.exe ran but saw no index pulses — no disk (or lever open).</summary>
        NoDisk,
        /// <summary>gw.exe could not talk to the device (unplugged, port busy).</summary>
        DeviceError
    }

    /// <summary>
    /// Blink and disk-presence operations on a single drive, used by the group-job
    /// insert phase and the device-tile identify button.
    /// </summary>
    public interface IDriveProber
    {
        /// <summary>
        /// Selects the drive briefly (<c>gw seek 0</c>) so its LED lights once.
        /// Call repeatedly to produce a visible blink.
        /// </summary>
        Task BlinkOnceAsync(string comPort, string drive, CancellationToken ct);

        /// <summary>Runs <c>gw rpm</c> to check whether a disk is inserted.</summary>
        Task<DiskProbeResult> ProbeDiskAsync(string comPort, string drive, CancellationToken ct);
    }

    /// <summary>
    /// <see cref="IDriveProber"/> implementation that shells out to gw.exe.
    /// All calls are short-lived; a watchdog kills hung processes.
    /// </summary>
    public class DriveProber : IDriveProber
    {
        private static readonly Regex RpmRegex =
            new(@"\d+(\.\d+)?\s*rpm", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex NoIndexRegex =
            new(@"no\s+index", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly string _gwExePath;

        /// <summary>Initialises the prober with the path to gw.exe.</summary>
        public DriveProber(string gwExePath) => _gwExePath = gwExePath;

        /// <inheritdoc/>
        public async Task BlinkOnceAsync(string comPort, string drive, CancellationToken ct)
            => await RunGwAsync($"seek --device {comPort} --drive {drive} 0",
                   timeoutMs: 4000, ct);

        /// <inheritdoc/>
        public async Task<DiskProbeResult> ProbeDiskAsync(string comPort, string drive,
            CancellationToken ct)
        {
            var (exitCode, output) = await RunGwAsync(
                $"rpm --device {comPort} --drive {drive}", timeoutMs: 8000, ct);
            return InterpretProbeOutput(exitCode, output);
        }

        /// <summary>
        /// Maps a <c>gw rpm</c> exit code and combined output to a <see cref="DiskProbeResult"/>.
        /// An RPM figure means a disk is present; "no index" means the drive answered but is
        /// empty; anything else failing is a device error.
        /// </summary>
        internal static DiskProbeResult InterpretProbeOutput(int exitCode, string output)
        {
            if (RpmRegex.IsMatch(output)) return DiskProbeResult.DiskPresent;
            if (NoIndexRegex.IsMatch(output)) return DiskProbeResult.NoDisk;
            return exitCode == 0 ? DiskProbeResult.NoDisk : DiskProbeResult.DeviceError;
        }

        /// <summary>
        /// Runs gw.exe with the given arguments, returning exit code and combined output.
        /// Kills the process on timeout or cancellation and reports exit code -1.
        /// </summary>
        private async Task<(int ExitCode, string Output)> RunGwAsync(
            string args, int timeoutMs, CancellationToken ct)
        {
            var psi = new ProcessStartInfo
            {
                FileName               = _gwExePath,
                Arguments              = args,
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                CreateNoWindow         = true
            };

            var sb = new StringBuilder();
            try
            {
                using var p = new Process { StartInfo = psi };
                p.OutputDataReceived += (s, e) => { if (e.Data != null) lock (sb) sb.AppendLine(e.Data); };
                p.ErrorDataReceived  += (s, e) => { if (e.Data != null) lock (sb) sb.AppendLine(e.Data); };
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeoutCts.CancelAfter(timeoutMs);
                try
                {
                    await p.WaitForExitAsync(timeoutCts.Token);
                }
                catch (OperationCanceledException)
                {
                    try { p.Kill(entireProcessTree: true); } catch { }
                    return (-1, sb.ToString());
                }
                return (p.ExitCode, sb.ToString());
            }
            catch (Exception ex)
            {
                lock (sb) sb.AppendLine(ex.Message);
                return (-1, sb.ToString());
            }
        }
    }
}
