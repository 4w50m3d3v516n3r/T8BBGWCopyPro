using System;
using System.Collections.Generic;
using System.Threading;

namespace GwCopyPro.Models
{
    /// <summary>
    /// One drive participating in a group repetitive job: a GreaseWeazle device plus
    /// the drive address on that device, with per-batch runtime state.
    /// </summary>
    public class DeviceGroupMember
    {
        /// <summary>The GreaseWeazle device this member uses.</summary>
        public GreaseWeazleDevice Device { get; set; } = new();

        /// <summary>Drive address on the device: <c>0</c>, <c>1</c>, <c>a</c>, or <c>b</c>.</summary>
        public string Drive { get; set; } = "0";

        /// <summary>Whether this member takes part in the current batch. Togglable every batch.</summary>
        public bool IncludedThisBatch { get; set; } = true;

        /// <summary>Whether a disk has been detected in this drive for the current batch.</summary>
        public bool Verified { get; set; }

        /// <summary>Whether this member's disk failed in the previous batch.</summary>
        public bool LastBatchFailed { get; set; }

        /// <summary>Error message from the previous batch, when <see cref="LastBatchFailed"/> is set.</summary>
        public string? LastBatchError { get; set; }

        /// <summary>Image file successfully written in the previous batch, if any.</summary>
        public string? LastBatchFile { get; set; }

        /// <summary>The member's job instance, created once at group-job start and reused per batch.</summary>
        public GwJob? Job { get; set; }

        /// <summary>Cancellation source for this member's current batch run (linked to the group token).</summary>
        public CancellationTokenSource? BatchCts { get; set; }
    }

    /// <summary>Result of <see cref="GroupRepetitiveJob.PrepareBatch"/> for one member.</summary>
    /// <param name="Member">The member imaging this disk.</param>
    /// <param name="DiskNumber">1-based disk counter assigned to this disk.</param>
    /// <param name="FileName">Expanded file name (not yet combined with the output folder).</param>
    public record BatchAssignment(DeviceGroupMember Member, int DiskNumber, string FileName);

    /// <summary>
    /// A repetitive job that images batches of disks on several GreaseWeazle devices in parallel.
    /// Holds the shared parameter template, the ordered member list, and the disk counter.
    /// </summary>
    public class GroupRepetitiveJob
    {
        /// <summary>Unique group-job identifier.</summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

        /// <summary>Read or write. Group jobs are typically reads.</summary>
        public JobType JobType { get; set; } = JobType.Read;

        /// <summary>
        /// Shared gw.exe parameters for all members. <see cref="GwParameters.Device"/>,
        /// <see cref="GwParameters.Drive"/>, and <see cref="GwParameters.ImageFile"/> are
        /// overwritten per member at batch start.
        /// </summary>
        public GwParameters ParameterTemplate { get; set; } = new();

        /// <summary>Post-actions copied into each member job.</summary>
        public List<PostAction> PostActions { get; set; } = new();

        /// <summary>File name pattern with <c>{n}</c>/<c>{dt}</c> tokens.</summary>
        public string FilePattern { get; set; } = "";

        /// <summary>Root folder where images are written.</summary>
        public string OutputFolder { get; set; } = "";

        /// <summary>Format string for the <c>{dt}</c> token.</summary>
        public string DateTimeFormat { get; set; } = "yyyyMMdd_HHmmss";

        /// <summary>Next disk counter value. Monotonically increasing; numbers are never reused.</summary>
        public int NextDiskNumber { get; set; } = 1;

        /// <summary>1-based number of the batch most recently prepared. 0 before the first batch.</summary>
        public int BatchNumber { get; set; }

        /// <summary>Ordered member list. List order is the blink order.</summary>
        public List<DeviceGroupMember> Members { get; set; } = new();

        /// <summary>
        /// Validates a member list for use as a group.
        /// </summary>
        /// <param name="members">Members to validate.</param>
        /// <returns>
        /// A localization key describing the problem (<c>job_dlg.group_min</c> or
        /// <c>job_dlg.group_dup_device</c>), or <see langword="null"/> when valid.
        /// </returns>
        public static string? Validate(IReadOnlyList<DeviceGroupMember> members)
        {
            if (members.Count < 2) return "job_dlg.group_min";

            var seen = new HashSet<string>();
            foreach (var m in members)
                if (!seen.Add(m.Device.Id)) return "job_dlg.group_dup_device";

            return null;
        }

        /// <summary>
        /// Assigns disk numbers and file names to all included, verified members in group
        /// order, advancing <see cref="NextDiskNumber"/> and <see cref="BatchNumber"/>.
        /// Numbers consumed here are never handed out again, even if the disk later fails.
        /// </summary>
        /// <returns>One assignment per participating member.</returns>
        public List<BatchAssignment> PrepareBatch()
        {
            var result = new List<BatchAssignment>();
            foreach (var m in Members)
            {
                if (!m.IncludedThisBatch || !m.Verified) continue;
                int n = NextDiskNumber++;
                result.Add(new BatchAssignment(m, n,
                    Models.FilePattern.Expand(FilePattern, n, DateTimeFormat)));
            }
            BatchNumber++;
            return result;
        }
    }
}
