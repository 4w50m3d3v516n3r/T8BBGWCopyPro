using System;
using System.Collections.Generic;
using System.Text;

namespace GwCopyPro.Models
{
    /// <summary>Distinguishes between disk-read and disk-write operations.</summary>
    public enum JobType { Read, Write }

    /// <summary>Lifecycle states a <see cref="GwJob"/> passes through.</summary>
    public enum JobStatus { Idle, Running, Completed, Error, Cancelled }

    /// <summary>Per-cell state shown in the disk visualiser.</summary>
    public enum TrackStatus { Unknown, Pending, Reading, Writing, Good, Error, Empty }

    /// <summary>
    /// All parameters that can be passed to <c>gw.exe read</c> or <c>gw.exe write</c>.
    /// Track selection uses the <c>--tracks=</c> compound syntax introduced in gw.exe v0.24.
    /// The legacy flags <c>--scyl</c>, <c>--ecyl</c>, <c>--shead</c>, <c>--ehead</c>, and
    /// <c>--single-sided</c> are not generated; they were removed in v0.24.
    /// </summary>
    public class GwParameters
    {
        /// <summary>COM port of the GreaseWeazle device passed as <c>--device COMx</c>.</summary>
        public string? Device { get; set; }

        /// <summary>Drive identifier passed as <c>--drive a|b|0|1|2|3</c>.</summary>
        public string? Drive { get; set; }

        /// <summary>Disk format string passed as <c>--format ibm.1440</c> etc.</summary>
        public string? DiskFormat { get; set; }

        /// <summary>Start cylinder for the <c>c=</c> component of <c>--tracks=</c> (inclusive).</summary>
        public int? StartCylinder { get; set; }

        /// <summary>End cylinder for the <c>c=</c> component of <c>--tracks=</c> (inclusive).</summary>
        public int? EndCylinder { get; set; }

        /// <summary>
        /// Head selection for the <c>h=</c> component of <c>--tracks=</c>.
        /// <see langword="null"/> means both sides (<c>h=0-1</c>).
        /// </summary>
        public int? Head { get; set; }

        /// <summary>Physical steps per cylinder passed as <c>step=N</c> inside <c>--tracks=</c>. Default is 1.</summary>
        public int? Step { get; set; }

        /// <summary>When <see langword="true"/>, emits <c>hswap</c> inside <c>--tracks=</c> to swap physical heads.</summary>
        public bool HSwap { get; set; }

        /// <summary>Head-0 cylinder offset for flippy drives, emitted as <c>h0.off=N</c>.</summary>
        public int? Head0Offset { get; set; }

        /// <summary>Head-1 cylinder offset for flippy drives, emitted as <c>h1.off=N</c>.</summary>
        public int? Head1Offset { get; set; }

        /// <summary>Flux revolutions to capture per track, passed as <c>--revs N</c>.</summary>
        public int? Revolutions { get; set; }

        /// <summary>Density-select signal override passed as <c>--densel hd|dd|ed</c>.</summary>
        public string? Densel { get; set; }

        /// <summary>Raw bitrate override passed as <c>--bitrate N</c>. Zero means auto-detect.</summary>
        public int? Bitrate { get; set; }

        /// <summary>Retry count on bad sectors passed as <c>--retries N</c> (read only).</summary>
        public int? Retries { get; set; }

        /// <summary>When <see langword="true"/>, emits <c>--no-clobber</c> to skip already-imaged tracks.</summary>
        public bool NoClobber { get; set; }

        /// <summary>When <see langword="true"/>, emits <c>--raw</c> to write raw flux, bypassing the format codec.</summary>
        public bool RawRead { get; set; }

        /// <summary>When <see langword="true"/>, emits <c>--reverse</c> to reverse track data (e.g. flippy side B).</summary>
        public bool Reverse { get; set; }

        /// <summary>When <see langword="true"/>, emits <c>--hard-sectors</c> for hard-sectored disk support.</summary>
        public bool HardSectors { get; set; }

        /// <summary>When <see langword="true"/>, emits <c>--erase</c> to blank the disk before writing.</summary>
        public bool Erase { get; set; }

        /// <summary>When <see langword="true"/>, emits <c>--verify</c> to verify written data (write only).</summary>
        public bool Verify { get; set; }

        /// <summary>Write precompensation in microseconds, passed as <c>--precomp N</c>.</summary>
        public string? Precomp { get; set; }

        /// <summary>When <see langword="true"/>, emits <c>--gen-tg43</c> to drive the /TG43 signal on 8″ drives.</summary>
        public bool GenTg43 { get; set; }

        /// <summary>Full path to the disk image file, appended verbatim as the final argument.</summary>
        public string? ImageFile { get; set; }

        /// <summary>Additional arguments appended verbatim after all structured flags.</summary>
        public string? ExtraArgs { get; set; }

        /// <summary>
        /// Builds the complete argument string to pass to <c>gw.exe</c>.
        /// </summary>
        /// <param name="jobType">Determines whether <c>read</c> or <c>write</c> is the first token.</param>
        /// <returns>The argument string ready to be passed to <see cref="System.Diagnostics.ProcessStartInfo.Arguments"/>.</returns>
        public string BuildArgs(JobType jobType)
        {
            var sb = new StringBuilder();
            sb.Append(jobType == JobType.Read ? "read" : "write");
            sb.Append(' ');

            if (!string.IsNullOrWhiteSpace(Device))
                sb.Append($"--device {Device} ");

            if (!string.IsNullOrWhiteSpace(Drive))
                sb.Append($"--drive {Drive} ");

            if (!string.IsNullOrWhiteSpace(DiskFormat))
                sb.Append($"--format {DiskFormat} ");

            string? tracks = BuildTracksSpec();
            if (tracks != null)
                sb.Append($"--tracks={tracks} ");

            if (Revolutions.HasValue && Revolutions.Value > 0)
                sb.Append($"--revs {Revolutions} ");

            if (!string.IsNullOrWhiteSpace(Densel))
                sb.Append($"--densel {Densel} ");

            if (Bitrate.HasValue && Bitrate.Value > 0)
                sb.Append($"--bitrate {Bitrate} ");

            if (jobType == JobType.Read)
            {
                if (Retries.HasValue && Retries.Value > 0)
                    sb.Append($"--retries {Retries} ");
                if (NoClobber)   sb.Append("--no-clobber ");
                if (RawRead)     sb.Append("--raw ");
                if (Reverse)     sb.Append("--reverse ");
                if (HardSectors) sb.Append("--hard-sectors ");
            }
            else
            {
                if (Erase)       sb.Append("--erase ");
                if (Verify)      sb.Append("--verify ");
                if (!string.IsNullOrWhiteSpace(Precomp))
                    sb.Append($"--precomp {Precomp} ");
                if (GenTg43)     sb.Append("--gen-tg43 ");
                if (Reverse)     sb.Append("--reverse ");
                if (HardSectors) sb.Append("--hard-sectors ");
            }

            if (!string.IsNullOrWhiteSpace(ExtraArgs))
                sb.Append(ExtraArgs.Trim() + " ");

            if (!string.IsNullOrWhiteSpace(ImageFile))
                sb.Append($"\"{ImageFile}\"");

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Builds the value of <c>--tracks=</c> from the individual track-selection fields.
        /// Returns <see langword="null"/> when all fields are at their defaults, so the flag is omitted entirely.
        /// </summary>
        /// <returns>
        /// A colon-separated specifier such as <c>c=0-79:h=0-1</c>,
        /// or <see langword="null"/> when no non-default track selection is set.
        /// </returns>
        private string? BuildTracksSpec()
        {
            bool hasNonDefault =
                StartCylinder.HasValue ||
                EndCylinder.HasValue ||
                Head.HasValue ||
                (Step.HasValue && Step.Value != 1) ||
                HSwap ||
                Head0Offset.HasValue ||
                Head1Offset.HasValue;

            if (!hasNonDefault) return null;

            var parts = new List<string>();

            int cStart = StartCylinder ?? 0;
            int cEnd   = EndCylinder   ?? 79;
            parts.Add(cStart == cEnd ? $"c={cStart}" : $"c={cStart}-{cEnd}");

            if (Head.HasValue)
                parts.Add($"h={Head.Value}");
            else
                parts.Add("h=0-1");

            if (Step.HasValue && Step.Value != 1)
                parts.Add($"step={Step.Value}");

            if (HSwap)
                parts.Add("hswap");

            if (Head0Offset.HasValue)
                parts.Add($"h0.off={Head0Offset.Value:+0;-0;0}");
            if (Head1Offset.HasValue)
                parts.Add($"h1.off={Head1Offset.Value:+0;-0;0}");

            return string.Join(":", parts);
        }

        /// <summary>Returns a shallow copy of this instance.</summary>
        public GwParameters Clone() => (GwParameters)MemberwiseClone();

        /// <summary>Number of cylinders implied by the current track selection.</summary>
        public int CylinderCount => (EndCylinder ?? 79) - (StartCylinder ?? 0) + 1;

        /// <summary>Number of heads implied by the current track selection (1 when <see cref="Head"/> is set, otherwise 2).</summary>
        public int HeadCount => Head.HasValue ? 1 : 2;

        /// <summary>Total track count equal to <see cref="CylinderCount"/> × <see cref="HeadCount"/>.</summary>
        public int TotalTrackCount => CylinderCount * HeadCount;
    }

    /// <summary>Represents a single GreaseWeazle device connected via a serial port.</summary>
    public class GreaseWeazleDevice
    {
        /// <summary>Stable identifier for this device instance (8-character hex fragment of a GUID).</summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

        /// <summary>Human-readable display name, editable by the user in Device Manager.</summary>
        public string Name { get; set; } = "GreaseWeazle";

        /// <summary>COM port name (e.g. <c>COM3</c>) through which the device is reached.</summary>
        public string SerialPort { get; set; } = "COM3";

        /// <summary>Whether the device is currently reachable on its COM port.</summary>
        public bool IsConnected { get; set; }

        /// <summary>Firmware version string returned by <c>gw.exe info</c>, or <c>"Unknown"</c>.</summary>
        public string FirmwareVersion { get; set; } = "Unknown";

        /// <summary>Hardware device-ID string extracted from the WMI PnP entity.</summary>
        public string HardwareId { get; set; } = "";

        /// <summary>Lime-green when connected, orange-red when not, used by the status LED.</summary>
        public System.Drawing.Color StatusColor =>
            IsConnected ? System.Drawing.Color.LimeGreen : System.Drawing.Color.OrangeRed;

        /// <inheritdoc/>
        public override string ToString() => $"{Name} ({SerialPort})";
    }

    /// <summary>An action executed automatically after a successful job completes.</summary>
    public class PostAction
    {
        /// <summary>Unique identifier for this action instance.</summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

        /// <summary>Display name shown in the post-actions list.</summary>
        public string Name { get; set; } = "Action";

        /// <summary>Path to the executable or script file to run.</summary>
        public string ExecutablePath { get; set; } = "";

        /// <summary>
        /// Argument string. Supports <c>{ImageFile}</c>, <c>{LogFolder}</c>,
        /// <c>{JobId}</c>, and <c>{DiskIndex}</c> tokens expanded at runtime.
        /// </summary>
        public string Arguments { get; set; } = "";

        /// <summary>When <see langword="false"/>, the action is skipped without being removed.</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>Determines how the action is launched (direct exe, cmd.exe, or powershell.exe).</summary>
        public PostActionType ActionType { get; set; } = PostActionType.Executable;

        /// <summary>Zero-based execution order within the job's post-action sequence.</summary>
        public int Order { get; set; }

        /// <inheritdoc/>
        public override string ToString() => $"[{Order}] {Name}";
    }

    /// <summary>Specifies how a <see cref="PostAction"/> is launched.</summary>
    public enum PostActionType { Executable, BatchScript, PowerShellScript }

    /// <summary>Status and timing data for a single cylinder/head cell in the disk visualiser.</summary>
    public class TrackCell
    {
        /// <summary>Cylinder index (0-based).</summary>
        public int Cylinder { get; set; }

        /// <summary>Head index (0 or 1).</summary>
        public int Head { get; set; }

        /// <summary>Current visual state of this cell.</summary>
        public TrackStatus Status { get; set; } = TrackStatus.Unknown;

        /// <summary>Error message from the gw.exe output line that caused an <see cref="TrackStatus.Error"/> state.</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>Wall-clock time taken to read or write this track.</summary>
        public TimeSpan? ReadWriteTime { get; set; }

        /// <summary>Number of retries gw.exe performed before reporting a final status.</summary>
        public int RetryCount { get; set; }
    }

    /// <summary>
    /// Represents a single gw.exe read or write operation, including its parameters,
    /// runtime state, per-track visualiser grid, and post-action list.
    /// </summary>
    public class GwJob
    {
        /// <summary>Unique job identifier used as a dictionary key and embedded in log folder names.</summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

        /// <summary>Whether this is a disk-read or disk-write operation.</summary>
        public JobType JobType { get; set; }

        /// <summary>Current lifecycle state of this job.</summary>
        public JobStatus Status { get; set; } = JobStatus.Idle;

        /// <summary>The GreaseWeazle device this job targets.</summary>
        public GreaseWeazleDevice? Device { get; set; }

        /// <summary>All gw.exe parameters for this job.</summary>
        public GwParameters Parameters { get; set; } = new();

        /// <summary>Ordered list of actions to run after the job succeeds.</summary>
        public List<PostAction> PostActions { get; set; } = new();

        /// <summary>When the job object was created.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>When gw.exe was launched. <see langword="null"/> until the job starts.</summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>When gw.exe exited. <see langword="null"/> until the job finishes.</summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>Full path to the per-job log folder.</summary>
        public string LogFolder { get; set; } = "";

        /// <summary>Full path to <c>gw_output.log</c> inside <see cref="LogFolder"/>.</summary>
        public string LogFile { get; set; } = "";

        /// <summary>Accumulated stdout/stderr lines captured from gw.exe and post-actions.</summary>
        public List<string> LogLines { get; set; } = new();

        /// <summary>84 × 2 grid of per-track status cells (cylinders × heads).</summary>
        public TrackCell[,] Tracks { get; set; } = new TrackCell[84, 2];

        /// <summary>Total number of tracks to process, used to compute <see cref="ProgressPercent"/>.</summary>
        public int TotalTracks { get; set; }

        /// <summary>Number of tracks that have reached a terminal state (good or error).</summary>
        public int CompletedTracks { get; set; }

        /// <summary>Number of tracks that ended in an error state.</summary>
        public int ErrorTracks { get; set; }

        /// <summary>Last error message, set when the job enters <see cref="JobStatus.Error"/>.</summary>
        public string? LastError { get; set; }

        /// <summary>Reference to the running gw.exe process, used for cancellation.</summary>
        public System.Diagnostics.Process? Process { get; set; }

        /// <summary>
        /// When <see langword="true"/>, the job loops over multiple disks and increments
        /// <see cref="DiskIndex"/> after each one completes.
        /// </summary>
        public bool RepetitiveMode { get; set; }

        /// <summary>
        /// File name pattern for repetitive mode.
        /// Supports <c>{n}</c>, <c>{n:D3}</c>, and <c>{dt}</c> tokens.
        /// </summary>
        public string FilePattern { get; set; } = "";

        /// <summary>1-based index of the disk currently being imaged in repetitive mode.</summary>
        public int DiskIndex { get; set; } = 1;

        /// <summary>Root folder where repetitive disk images are written.</summary>
        public string OutputFolder { get; set; } = "";

        /// <summary>C# <see cref="DateTime"/> format string used to expand the <c>{dt}</c> token.</summary>
        public string DateTimeFormat { get; set; } = "yyyyMMdd_HHmmss";

        /// <summary>Total number of disks successfully completed during a repetitive run.</summary>
        public int DisksCompleted { get; set; }

        /// <summary>
        /// Preset snapshot captured when the job was created.
        /// Allows the Restart button to re-open <c>NewJobDialog</c> pre-filled with the original settings.
        /// Not serialised.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public JobPreset? SourcePreset { get; set; }

        /// <summary>
        /// Initialises the <see cref="Tracks"/> grid so every cell has its cylinder and head indices populated.
        /// </summary>
        public GwJob()
        {
            for (int c = 0; c < 84; c++)
                for (int h = 0; h < 2; h++)
                    Tracks[c, h] = new TrackCell { Cylinder = c, Head = h };
        }

        /// <summary>Job completion as a percentage in [0, 100].</summary>
        public double ProgressPercent =>
            TotalTracks > 0 ? (CompletedTracks / (double)TotalTracks) * 100.0 : 0;

        /// <summary>
        /// Human-readable status string shown in the job panel, including progress fractions,
        /// timing, and disk-index information for repetitive runs.
        /// </summary>
        public string StatusText => Status switch
        {
            JobStatus.Idle      => "Idle",
            JobStatus.Running   => RepetitiveMode
                ? $"Disk #{DiskIndex}  {ProgressPercent:0}%  ({CompletedTracks}/{TotalTracks})"
                : $"{ProgressPercent:0}%  ({CompletedTracks}/{TotalTracks})",
            JobStatus.Completed => RepetitiveMode
                ? $"Done — {DisksCompleted} disk(s) in {(CompletedAt - StartedAt)?.TotalSeconds:0.0}s"
                : $"Done in {(CompletedAt - StartedAt)?.TotalSeconds:0.0}s",
            JobStatus.Error     => $"Error: {LastError}",
            JobStatus.Cancelled => "Cancelled",
            _                   => "Unknown"
        };
    }
}
