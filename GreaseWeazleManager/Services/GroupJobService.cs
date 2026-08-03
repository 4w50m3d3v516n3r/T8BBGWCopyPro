using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GwCopyPro.Models;

namespace GwCopyPro.Services
{
    /// <summary>Event arguments carrying a <see cref="GroupRepetitiveJob"/>.</summary>
    public class GroupJobEventArgs : EventArgs
    {
        /// <summary>The group job that raised this event.</summary>
        public GroupRepetitiveJob Group { get; }

        /// <summary>Initialises a new instance with the given group job.</summary>
        public GroupJobEventArgs(GroupRepetitiveJob group) => Group = group;
    }

    /// <summary>
    /// Raised before each batch. The UI handler shows the insert dialog, updates member
    /// inclusion/verification, then calls <see cref="Signal"/> to start the batch or finish.
    /// </summary>
    public class BatchPromptEventArgs : EventArgs
    {
        /// <summary>The group job awaiting its next batch.</summary>
        public GroupRepetitiveJob Group { get; }

        private readonly TaskCompletionSource<bool> _tcs;

        /// <summary>Initialises a new instance.</summary>
        /// <param name="group">The owning group job.</param>
        /// <param name="tcs">Completion source resolved via <see cref="Signal"/>.</param>
        public BatchPromptEventArgs(GroupRepetitiveJob group, TaskCompletionSource<bool> tcs)
        { Group = group; _tcs = tcs; }

        /// <summary>
        /// Resumes the group loop. Pass <see langword="true"/> to run the batch with the
        /// currently included, verified members; <see langword="false"/> to finish the job.
        /// </summary>
        public void Signal(bool startBatch) => _tcs.TrySetResult(startBatch);
    }

    /// <summary>
    /// Orchestrates a group repetitive job: creates one <see cref="GwJob"/> per member,
    /// prompts the UI for each insert phase, then runs all included members' disks in
    /// parallel via <see cref="GwService.RunSingleDiskAsync"/>.
    /// </summary>
    public class GroupJobService
    {
        private readonly GwService _gw;

        /// <summary>Raised once after member jobs are created, so the UI can add job panels.</summary>
        public event EventHandler<GroupJobEventArgs>? MemberJobsCreated;

        /// <summary>Raised before every batch; the handler must call <see cref="BatchPromptEventArgs.Signal"/>.</summary>
        public event EventHandler<BatchPromptEventArgs>? BatchPromptRequested;

        /// <summary>Raised once when the group job finishes (user choice, cancellation, or no members left).</summary>
        public event EventHandler<GroupJobEventArgs>? GroupCompleted;

        /// <summary>Initialises the service over the shared <see cref="GwService"/>.</summary>
        public GroupJobService(GwService gw) => _gw = gw;

        /// <summary>
        /// Runs the group job until the user finishes it, it is cancelled, or every
        /// member is excluded.
        /// </summary>
        /// <param name="group">The group job to run.</param>
        /// <param name="ct">Group-level cancellation token.</param>
        public async Task RunAsync(GroupRepetitiveJob group, CancellationToken ct)
        {
            foreach (var m in group.Members)
            {
                m.Job = new GwJob
                {
                    JobType        = group.JobType,
                    RepetitiveMode = true,
                    Device         = m.Device,
                    FilePattern    = group.FilePattern,
                    OutputFolder   = group.OutputFolder,
                    DateTimeFormat = group.DateTimeFormat,
                    Parameters     = group.ParameterTemplate.Clone()
                };
                m.Job.Parameters.Device = m.Device.SerialPort;
                m.Job.Parameters.Drive  = m.Drive;
                m.Job.PostActions.AddRange(group.PostActions);
            }
            MemberJobsCreated?.Invoke(this, new GroupJobEventArgs(group));

            while (!ct.IsCancellationRequested)
            {
                foreach (var m in group.Members) m.Verified = false;

                var tcs = new TaskCompletionSource<bool>();
                BatchPromptRequested?.Invoke(this, new BatchPromptEventArgs(group, tcs));
                bool start = await tcs.Task;
                if (!start || ct.IsCancellationRequested) break;

                var batch = group.PrepareBatch();
                if (batch.Count == 0) break;

                var runs = new List<Task>();
                foreach (var a in batch)
                {
                    var member = a.Member;
                    var job    = member.Job!;

                    string file = GwService.ResolveOutputFile(
                        group.OutputFolder, job.Parameters.ImageFile, a.FileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(file)!);

                    job.DiskIndex            = a.DiskNumber;
                    job.Parameters.ImageFile = file;
                    GwService.ResetTracks(job);

                    member.BatchCts?.Dispose();
                    member.BatchCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    member.LastBatchFailed = false;
                    member.LastBatchError  = null;

                    runs.Add(Task.Run(async () =>
                    {
                        bool ok = false;
                        try
                        {
                            ok = await _gw.RunSingleDiskAsync(job, member.BatchCts.Token);
                        }
                        catch (Exception ex)
                        {
                            job.Status    = JobStatus.Error;
                            job.LastError = ex.Message;
                        }
                        if (ok)
                        {
                            job.DisksCompleted++;
                            member.LastBatchFile = file;
                        }
                        else
                        {
                            member.LastBatchFailed = true;
                            member.LastBatchError  = job.LastError ?? job.Status.ToString();
                        }
                    }, CancellationToken.None));
                }

                await Task.WhenAll(runs);

                if (!group.Members.Any(m => m.IncludedThisBatch)) break;
            }

            foreach (var m in group.Members)
            {
                var job = m.Job;
                if (job != null && job.Status is not (JobStatus.Error or JobStatus.Cancelled))
                {
                    job.Status      = JobStatus.Completed;
                    job.CompletedAt = DateTime.Now;
                }
            }
            GroupCompleted?.Invoke(this, new GroupJobEventArgs(group));
        }
    }
}
