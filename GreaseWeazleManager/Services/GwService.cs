using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GwCopyPro.Models;

namespace GwCopyPro.Services
{
    /// <summary>Event arguments carrying the <see cref="GwJob"/> that triggered a job-level event.</summary>
    public class GwJobEventArgs : EventArgs
    {
        /// <summary>The job that raised this event.</summary>
        public GwJob Job { get; }

        /// <summary>Initialises a new instance with the given job.</summary>
        /// <param name="job">The job associated with this event.</param>
        public GwJobEventArgs(GwJob job) => Job = job;
    }

    /// <summary>Event arguments for a single-track status update within a running job.</summary>
    public class TrackUpdateEventArgs : EventArgs
    {
        /// <summary>The job that owns the updated track.</summary>
        public GwJob Job { get; }

        /// <summary>Zero-based cylinder index of the updated track.</summary>
        public int Cylinder { get; }

        /// <summary>Head index (0 or 1) of the updated track.</summary>
        public int Head { get; }

        /// <summary>New status for the track cell.</summary>
        public TrackStatus Status { get; }

        /// <summary>The gw.exe output line that triggered this update, or <see langword="null"/>.</summary>
        public string? Message { get; }

        /// <summary>
        /// Initialises a new <see cref="TrackUpdateEventArgs"/> with all track-update fields.
        /// </summary>
        public TrackUpdateEventArgs(GwJob job, int cyl, int head, TrackStatus status, string? msg = null)
        { Job = job; Cylinder = cyl; Head = head; Status = status; Message = msg; }
    }

    /// <summary>
    /// Raised after each individual disk completes in repetitive mode.
    /// The UI handler must call <see cref="Signal"/> to indicate whether to continue or stop.
    /// </summary>
    public class DiskCompletedEventArgs : EventArgs
    {
        /// <summary>The repetitive job that raised this event.</summary>
        public GwJob     Job           { get; }

        /// <summary>1-based index of the disk that just finished.</summary>
        public int       DiskNumber    { get; }

        /// <summary>Full path to the image file that was just written.</summary>
        public string    CompletedFile { get; }

        /// <summary>Full path to the image file that will be used for the next disk.</summary>
        public string    NextFile      { get; }

        /// <summary>Wall-clock duration of the disk that just completed.</summary>
        public TimeSpan  Duration      { get; }

        private readonly TaskCompletionSource<bool> _tcs;

        /// <summary>
        /// Initialises a new <see cref="DiskCompletedEventArgs"/>.
        /// </summary>
        /// <param name="job">Owning job.</param>
        /// <param name="diskNo">1-based disk number that completed.</param>
        /// <param name="completed">Path to the completed image file.</param>
        /// <param name="next">Path to the next image file.</param>
        /// <param name="duration">Time taken for the completed disk.</param>
        /// <param name="tcs">Completion source the UI resolves via <see cref="Signal"/>.</param>
        public DiskCompletedEventArgs(GwJob job, int diskNo, string completed,
            string next, TimeSpan duration, TaskCompletionSource<bool> tcs)
        { Job = job; DiskNumber = diskNo; CompletedFile = completed;
          NextFile = next; Duration = duration; _tcs = tcs; }

        /// <summary>
        /// Called from the UI thread to resume the repetitive loop.
        /// Pass <see langword="true"/> to start the next disk, or <see langword="false"/> to stop.
        /// </summary>
        /// <param name="go"><see langword="true"/> to continue; <see langword="false"/> to stop.</param>
        public void Signal(bool go) => _tcs.TrySetResult(go);
    }

    /// <summary>
    /// Orchestrates gw.exe process execution, output parsing, track-grid updates,
    /// post-action execution, and repetitive-mode looping for <see cref="GwJob"/> instances.
    /// </summary>
    public class GwService
    {
        /// <summary>Full path to the <c>gw.exe</c> binary used for all operations.</summary>
        public string GwExePath { get; set; } = "gw.exe";

        /// <summary>Raised once when gw.exe is launched for a single-disk job.</summary>
        public event EventHandler<GwJobEventArgs>?      JobStarted;

        /// <summary>Raised when gw.exe exits with code 0 and all post-actions have run.</summary>
        public event EventHandler<GwJobEventArgs>?      JobCompleted;

        /// <summary>Raised when gw.exe exits with a non-zero exit code or throws an exception.</summary>
        public event EventHandler<GwJobEventArgs>?      JobError;

        /// <summary>Raised whenever a track cell changes status.</summary>
        public event EventHandler<TrackUpdateEventArgs>? TrackUpdated;

        /// <summary>Raised on every line of gw.exe output to allow the UI to refresh progress indicators.</summary>
        public event EventHandler<GwJobEventArgs>?      JobProgress;

        /// <summary>Raised after each disk completes in repetitive mode; the handler must call <see cref="DiskCompletedEventArgs.Signal"/>.</summary>
        public event EventHandler<DiskCompletedEventArgs>? DiskCompleted;

        private static readonly Regex TrackRegex = new(
            @"(?:T(?:rack)?[\s:]*)(\d+)\.(\d+)[\s:]*(?:(ok|good|error|bad|retry|reading|writing))?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex CylHeadRegex = new(
            @"(?:Cyl|C)[\s:]*(\d+)[,\s]+(?:Head|H|Side)[\s:]*(\d+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ErrorRegex = new(
            @"(error|fail|bad|corrupt|exception|cannot|no disk|no media)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ProgressRegex = new(@"(\d+)/(\d+)", RegexOptions.Compiled);

        /// <summary>
        /// Entry point for running a job. Dispatches to the repetitive loop when
        /// <see cref="GwJob.RepetitiveMode"/> is enabled and the file pattern contains tokens;
        /// otherwise runs a single disk.
        /// </summary>
        /// <param name="job">The job to execute.</param>
        /// <param name="ct">Cancellation token; cancels the process and stops the loop.</param>
        public async Task RunJobAsync(GwJob job, CancellationToken ct = default)
        {
            job.StartedAt = DateTime.Now;

            if (job.RepetitiveMode && FilePattern.HasTokens(job.FilePattern))
                await RunRepetitiveAsync(job, ct);
            else
                await RunSingleDiskAsync(job, ct);
        }

        /// <summary>
        /// Runs the repetitive-mode loop, expanding the file pattern for each disk,
        /// resolving the output folder, running a single-disk operation, then raising
        /// <see cref="DiskCompleted"/> to ask the UI whether to continue.
        /// </summary>
        /// <remarks>
        /// Output folder resolution order:
        /// <list type="number">
        ///   <item>If <see cref="GwJob.OutputFolder"/> is an absolute path, it is used directly.</item>
        ///   <item>If it is relative, it is resolved against the application base directory.</item>
        ///   <item>If empty and the image file has an absolute path, its directory is used.</item>
        ///   <item>Otherwise the user's Desktop is used as a last resort.</item>
        /// </list>
        /// </remarks>
        private async Task RunRepetitiveAsync(GwJob job, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                string file = FilePattern.Expand(job.FilePattern, job.DiskIndex,
                    job.DateTimeFormat);

                string folder;
                if (!string.IsNullOrWhiteSpace(job.OutputFolder))
                {
                    folder = Path.IsPathRooted(job.OutputFolder)
                        ? job.OutputFolder
                        : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, job.OutputFolder);
                }
                else if (!string.IsNullOrWhiteSpace(job.Parameters.ImageFile) &&
                         Path.IsPathRooted(job.Parameters.ImageFile))
                {
                    folder = Path.GetDirectoryName(job.Parameters.ImageFile)!;
                }
                else
                {
                    folder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                }

                if (!Path.IsPathRooted(file))
                    file = Path.Combine(folder, file);

                Directory.CreateDirectory(Path.GetDirectoryName(file)!);

                job.Parameters.ImageFile = file;

                ResetTracks(job);

                DateTime diskStart = DateTime.Now;
                bool ok = await RunSingleDiskAsync(job, ct);

                if (!ok || ct.IsCancellationRequested) break;

                job.DisksCompleted++;
                TimeSpan duration = DateTime.Now - diskStart;

                string nextFile = FilePattern.Expand(job.FilePattern, job.DiskIndex + 1,
                    job.DateTimeFormat);

                var tcs = new TaskCompletionSource<bool>();
                var args = new DiskCompletedEventArgs(job, job.DiskIndex, file,
                    nextFile, duration, tcs);
                DiskCompleted?.Invoke(this, args);

                bool continueLoop = await tcs.Task;
                if (!continueLoop) break;

                job.DiskIndex++;
            }

            if (job.Status != JobStatus.Error && job.Status != JobStatus.Cancelled)
            {
                job.Status      = JobStatus.Completed;
                job.CompletedAt = DateTime.Now;
                JobCompleted?.Invoke(this, new GwJobEventArgs(job));
            }
        }

        /// <summary>
        /// Launches <c>gw.exe</c> for a single disk, streams its output through the log and
        /// the track parser, runs post-actions on success, and fires the appropriate events.
        /// </summary>
        /// <param name="job">The job to execute for one disk.</param>
        /// <param name="ct">Cancellation token; kills the process and marks the job cancelled.</param>
        /// <returns><see langword="true"/> on success; <see langword="false"/> on error or cancellation.</returns>
        private async Task<bool> RunSingleDiskAsync(GwJob job, CancellationToken ct)
        {
            job.Status = JobStatus.Running;

            string logSuffix = job.RepetitiveMode ? $"_disk{job.DiskIndex}" : "";
            string baseLog = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "Logs",
                $"Job_{job.JobType}_{job.Id}_{job.CreatedAt:yyyyMMdd_HHmmss}{logSuffix}");
            Directory.CreateDirectory(baseLog);
            job.LogFolder = baseLog;
            job.LogFile   = Path.Combine(baseLog, "gw_output.log");

            using var logWriter = new StreamWriter(job.LogFile, append: false) { AutoFlush = true };

            string args = job.Parameters.BuildArgs(job.JobType);
            logWriter.WriteLine("=== GreaseWeazle Job ===");
            logWriter.WriteLine($"Type    : {job.JobType}");
            logWriter.WriteLine($"Device  : {job.Device?.SerialPort ?? "default"}");
            logWriter.WriteLine($"DiskNo  : {(job.RepetitiveMode ? job.DiskIndex.ToString() : "—")}");
            logWriter.WriteLine($"Args    : {GwExePath} {args}");
            logWriter.WriteLine($"Started : {DateTime.Now}");
            logWriter.WriteLine(new string('=', 60));

            if (!job.RepetitiveMode)
                JobStarted?.Invoke(this, new GwJobEventArgs(job));

            int startCyl  = job.Parameters.StartCylinder ?? 0;
            int endCyl    = job.Parameters.EndCylinder   ?? 79;
            int headStart = job.Parameters.Head          ?? 0;
            int headEnd   = job.Parameters.Head          ?? 1;
            job.TotalTracks     = job.Parameters.TotalTrackCount;
            job.CompletedTracks = 0;
            job.ErrorTracks     = 0;

            for (int c = startCyl; c <= endCyl; c++)
                for (int h = headStart; h <= headEnd; h++)
                    job.Tracks[c, h].Status = TrackStatus.Pending;

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName               = GwExePath,
                    Arguments              = args,
                    UseShellExecute        = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    CreateNoWindow         = true
                };

                using var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
                job.Process = process;
                var outputLock = new object();

                void HandleLine(string? line, bool isError)
                {
                    if (line == null) return;
                    lock (outputLock)
                    {
                        job.LogLines.Add(line);
                        logWriter.WriteLine(isError ? $"[ERR] {line}" : line);
                        ParseOutputLine(job, line);
                        JobProgress?.Invoke(this, new GwJobEventArgs(job));
                    }
                }

                process.OutputDataReceived += (s, e) => HandleLine(e.Data, false);
                process.ErrorDataReceived  += (s, e) => HandleLine(e.Data, true);
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                while (!process.HasExited)
                {
                    if (ct.IsCancellationRequested)
                    {
                        try { process.Kill(entireProcessTree: true); } catch { }
                        job.Status = JobStatus.Cancelled;
                        logWriter.WriteLine($"\n[CANCELLED] {DateTime.Now}");
                        return false;
                    }
                    await Task.Delay(100, CancellationToken.None);
                }

                await process.WaitForExitAsync(CancellationToken.None);
                job.CompletedAt = DateTime.Now;

                if (process.ExitCode != 0)
                {
                    job.Status    = JobStatus.Error;
                    job.LastError = $"gw.exe exited with code {process.ExitCode}";
                    logWriter.WriteLine($"\n[ERROR] Exit code: {process.ExitCode}");
                    JobError?.Invoke(this, new GwJobEventArgs(job));
                    return false;
                }

                for (int c = startCyl; c <= endCyl; c++)
                    for (int h = headStart; h <= headEnd; h++)
                        if (job.Tracks[c, h].Status is TrackStatus.Pending
                            or TrackStatus.Reading or TrackStatus.Writing)
                            job.Tracks[c, h].Status = TrackStatus.Good;

                logWriter.WriteLine($"\n[COMPLETED] {job.CompletedAt}");

                if (!job.RepetitiveMode)
                {
                    job.Status = JobStatus.Completed;
                    JobCompleted?.Invoke(this, new GwJobEventArgs(job));
                    await RunPostActionsAsync(job, logWriter, ct);
                }
                else
                {
                    await RunPostActionsAsync(job, logWriter, ct);
                }

                return true;
            }
            catch (Exception ex)
            {
                job.Status    = JobStatus.Error;
                job.LastError = ex.Message;
                job.CompletedAt = DateTime.Now;
                logWriter.WriteLine($"\n[EXCEPTION] {ex}");
                JobError?.Invoke(this, new GwJobEventArgs(job));
                return false;
            }
        }

        /// <summary>
        /// Resets every cell in the job's track grid to <see cref="TrackStatus.Unknown"/>
        /// before starting a new disk in repetitive mode.
        /// </summary>
        /// <param name="job">The job whose track grid should be cleared.</param>
        private static void ResetTracks(GwJob job)
        {
            for (int c = 0; c < 84; c++)
                for (int h = 0; h < 2; h++)
                    job.Tracks[c, h].Status = TrackStatus.Unknown;
        }

        /// <summary>
        /// Parses a single line of gw.exe output and updates the job's track grid and
        /// progress counters. Fires <see cref="TrackUpdated"/> for each cell state change.
        /// Tries three patterns in order: T##.## status, Cyl/Head notation, and n/m fraction.
        /// </summary>
        /// <param name="job">The job whose state should be updated.</param>
        /// <param name="line">A single stdout or stderr line from gw.exe.</param>
        private void ParseOutputLine(GwJob job, string line)
        {
            var m = TrackRegex.Match(line);
            if (m.Success && int.TryParse(m.Groups[1].Value, out int cyl) &&
                int.TryParse(m.Groups[2].Value, out int head) && cyl < 84 && head < 2)
            {
                string statusStr = m.Groups[3].Value.ToLower();
                TrackStatus ts = statusStr switch
                {
                    "ok" or "good"    => TrackStatus.Good,
                    "error" or "bad"  => TrackStatus.Error,
                    "reading"         => TrackStatus.Reading,
                    "writing"         => TrackStatus.Writing,
                    _                 => TrackStatus.Good
                };
                bool wasIncomplete = job.Tracks[cyl, head].Status is
                    TrackStatus.Pending or TrackStatus.Reading or
                    TrackStatus.Writing or TrackStatus.Unknown;
                job.Tracks[cyl, head].Status = ts;
                if (ts == TrackStatus.Error) { job.Tracks[cyl, head].ErrorMessage = line; job.ErrorTracks++; }
                if (wasIncomplete && ts is TrackStatus.Good or TrackStatus.Error) job.CompletedTracks++;
                TrackUpdated?.Invoke(this, new TrackUpdateEventArgs(job, cyl, head, ts, line));
                return;
            }

            var m2 = CylHeadRegex.Match(line);
            if (m2.Success && int.TryParse(m2.Groups[1].Value, out int c2) &&
                int.TryParse(m2.Groups[2].Value, out int h2) && c2 < 84 && h2 < 2)
            {
                bool isErr = ErrorRegex.IsMatch(line);
                var ts = job.JobType == JobType.Read
                    ? (isErr ? TrackStatus.Error : TrackStatus.Reading)
                    : (isErr ? TrackStatus.Error : TrackStatus.Writing);
                job.Tracks[c2, h2].Status = ts;
                if (isErr) job.ErrorTracks++;
                TrackUpdated?.Invoke(this, new TrackUpdateEventArgs(job, c2, h2, ts, line));
            }

            var pm = ProgressRegex.Match(line);
            if (pm.Success && int.TryParse(pm.Groups[1].Value, out int done) &&
                int.TryParse(pm.Groups[2].Value, out int total) && total > 0)
            {
                job.CompletedTracks = Math.Min(done, job.TotalTracks);
                if (job.TotalTracks == 0) job.TotalTracks = total;
            }
        }

        /// <summary>
        /// Executes all enabled <see cref="PostAction"/> entries in <see cref="GwJob.PostActions"/>
        /// in their defined order, appending output to <paramref name="log"/>.
        /// Skips actions whose <see cref="PostAction.IsEnabled"/> is <see langword="false"/> or
        /// when <paramref name="ct"/> is already cancelled.
        /// </summary>
        /// <param name="job">The completed job whose post-actions should run.</param>
        /// <param name="log">Stream writer for the job's log file.</param>
        /// <param name="ct">Cancellation token.</param>
        private async Task RunPostActionsAsync(GwJob job, StreamWriter log, CancellationToken ct)
        {
            if (job.PostActions.Count == 0) return;
            log.WriteLine("\n=== Post-Actions ===");
            var sorted = new List<PostAction>(job.PostActions);
            sorted.Sort((a, b) => a.Order.CompareTo(b.Order));

            foreach (var action in sorted)
            {
                if (!action.IsEnabled || ct.IsCancellationRequested) continue;
                log.WriteLine($"[ACTION] {action.Name}: {action.ExecutablePath} {action.Arguments}");
                try
                {
                    string exe = action.ActionType switch
                    {
                        PostActionType.PowerShellScript => "powershell.exe",
                        PostActionType.BatchScript      => "cmd.exe",
                        _                               => action.ExecutablePath
                    };
                    string actionArgs = action.ActionType switch
                    {
                        PostActionType.PowerShellScript =>
                            $"-NoProfile -ExecutionPolicy Bypass -File \"{action.ExecutablePath}\" {action.Arguments}",
                        PostActionType.BatchScript =>
                            $"/c \"{action.ExecutablePath}\" {action.Arguments}",
                        _ => action.Arguments
                    };
                    actionArgs = actionArgs
                        .Replace("{ImageFile}", job.Parameters.ImageFile ?? "")
                        .Replace("{LogFolder}", job.LogFolder)
                        .Replace("{JobId}",     job.Id)
                        .Replace("{DiskIndex}", job.DiskIndex.ToString());

                    var psi = new ProcessStartInfo
                    {
                        FileName               = exe,
                        Arguments              = actionArgs,
                        UseShellExecute        = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError  = true,
                        CreateNoWindow         = true
                    };
                    using var p = Process.Start(psi)!;
                    p.OutputDataReceived += (s, e) => { if (e.Data != null) log.WriteLine(e.Data); };
                    p.ErrorDataReceived  += (s, e) => { if (e.Data != null) log.WriteLine($"[ERR] {e.Data}"); };
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    await p.WaitForExitAsync(ct);
                    log.WriteLine($"[ACTION] Exit: {p.ExitCode}");
                }
                catch (Exception ex) { log.WriteLine($"[ACTION ERROR] {ex.Message}"); }
            }
        }
    }
}
