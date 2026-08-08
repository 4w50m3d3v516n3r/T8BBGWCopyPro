using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GwCopyPro.Services
{
    /// <summary>
    /// Blink operations on a single drive, used by the group-job insert phase and the
    /// device-tile identify button. gw.exe has no reliable way to detect whether a disk is
    /// physically present in a drive, so disk presence is confirmed by the user, not probed.
    /// </summary>
    public interface IDriveProber
    {
        /// <summary>
        /// Selects the drive briefly (<c>gw seek 0</c>) so its LED lights once.
        /// Call repeatedly to produce a visible blink.
        /// </summary>
        Task BlinkOnceAsync(string comPort, string drive, CancellationToken ct);
    }

    /// <summary>
    /// <see cref="IDriveProber"/> implementation that shells out to gw.exe.
    /// All calls are short-lived; a watchdog kills hung processes.
    /// </summary>
    public class DriveProber : IDriveProber
    {
        private readonly string _gwExePath;

        /// <summary>Initialises the prober with the path to gw.exe.</summary>
        public DriveProber(string gwExePath) => _gwExePath = gwExePath;

        /// <inheritdoc/>
        public async Task BlinkOnceAsync(string comPort, string drive, CancellationToken ct)
            => await RunGwAsync($"seek --device {comPort} --drive {drive} 0",
                   timeoutMs: 4000, ct);

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
