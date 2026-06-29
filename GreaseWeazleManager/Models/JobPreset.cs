using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace GwCopyPro.Models
{
    /// <summary>
    /// Serialisable snapshot of all settings required to recreate a <see cref="GwJob"/>.
    /// Persisted as JSON under <c>%APPDATA%\GreaseWeazleManager\Presets\</c>.
    /// </summary>
    public class JobPreset
    {
        /// <summary>User-facing name displayed when browsing saved presets.</summary>
        public string  PresetName    { get; set; } = "New Preset";

        /// <summary>Whether the preset represents a read or write job.</summary>
        public JobType JobType       { get; set; } = JobType.Read;

        /// <summary>See <see cref="GwParameters.Device"/>.</summary>
        public string? Device        { get; set; }

        /// <summary>See <see cref="GwParameters.Drive"/>.</summary>
        public string? Drive         { get; set; }

        /// <summary>See <see cref="GwParameters.DiskFormat"/>.</summary>
        public string? DiskFormat    { get; set; }

        /// <summary>See <see cref="GwParameters.StartCylinder"/>.</summary>
        public int?    StartCylinder { get; set; }

        /// <summary>See <see cref="GwParameters.EndCylinder"/>.</summary>
        public int?    EndCylinder   { get; set; }

        /// <summary>See <see cref="GwParameters.Head"/>.</summary>
        public int?    Head          { get; set; }

        /// <summary>See <see cref="GwParameters.Step"/>.</summary>
        public int?    Step          { get; set; }

        /// <summary>See <see cref="GwParameters.HSwap"/>.</summary>
        public bool    HSwap         { get; set; }

        /// <summary>See <see cref="GwParameters.Head0Offset"/>.</summary>
        public int?    Head0Offset   { get; set; }

        /// <summary>See <see cref="GwParameters.Head1Offset"/>.</summary>
        public int?    Head1Offset   { get; set; }

        /// <summary>See <see cref="GwParameters.Revolutions"/>.</summary>
        public int?    Revolutions   { get; set; }

        /// <summary>See <see cref="GwParameters.Densel"/>.</summary>
        public string? Densel        { get; set; }

        /// <summary>See <see cref="GwParameters.Bitrate"/>.</summary>
        public int?    Bitrate       { get; set; }

        /// <summary>See <see cref="GwParameters.Retries"/>.</summary>
        public int?    Retries       { get; set; }

        /// <summary>See <see cref="GwParameters.NoClobber"/>.</summary>
        public bool    NoClobber     { get; set; }

        /// <summary>See <see cref="GwParameters.RawRead"/>.</summary>
        public bool    RawRead       { get; set; }

        /// <summary>See <see cref="GwParameters.Reverse"/>.</summary>
        public bool    Reverse       { get; set; }

        /// <summary>See <see cref="GwParameters.HardSectors"/>.</summary>
        public bool    HardSectors   { get; set; }

        /// <summary>See <see cref="GwParameters.Erase"/>.</summary>
        public bool    Erase         { get; set; }

        /// <summary>See <see cref="GwParameters.Verify"/>.</summary>
        public bool    Verify        { get; set; }

        /// <summary>See <see cref="GwParameters.Precomp"/>.</summary>
        public string? Precomp       { get; set; }

        /// <summary>See <see cref="GwParameters.GenTg43"/>.</summary>
        public bool    GenTg43       { get; set; }

        /// <summary>See <see cref="GwParameters.ExtraArgs"/>.</summary>
        public string? ExtraArgs     { get; set; }

        /// <summary>File name pattern for repetitive mode (e.g. <c>Disk_{n:D3}_{dt}</c>).</summary>
        public string? FilePattern        { get; set; }

        /// <summary>Whether repetitive (multi-disk) mode is enabled.</summary>
        public bool    RepetitiveMode     { get; set; }

        /// <summary>1-based counter value for the first disk in a repetitive run.</summary>
        public int     StartIndex         { get; set; } = 1;

        /// <summary>C# <see cref="DateTime"/> format string used for the <c>{dt}</c> token.</summary>
        public string  DateTimeFormat     { get; set; } = "yyyyMMdd_HHmmss";

        /// <summary>
        /// Root folder where repetitive disk images are written.
        /// An empty string means the directory of <see cref="GwParameters.ImageFile"/> is used.
        /// </summary>
        public string  OutputFolder       { get; set; } = "";

        /// <summary>Serialisable post-action definitions attached to this preset.</summary>
        public List<PostActionPreset> PostActions { get; set; } = new();

        /// <summary>
        /// Constructs a <see cref="GwParameters"/> instance from the flattened preset fields.
        /// <see cref="GwParameters.ImageFile"/> is left <see langword="null"/> because it is resolved at runtime.
        /// </summary>
        /// <returns>A new <see cref="GwParameters"/> populated from this preset.</returns>
        public GwParameters ToParameters() => new()
        {
            Device        = Device,
            Drive         = Drive,
            DiskFormat    = DiskFormat,
            StartCylinder = StartCylinder,
            EndCylinder   = EndCylinder,
            Head          = Head,
            Step          = Step,
            HSwap         = HSwap,
            Head0Offset   = Head0Offset,
            Head1Offset   = Head1Offset,
            Revolutions   = Revolutions,
            Densel        = Densel,
            Bitrate       = Bitrate,
            Retries       = Retries,
            NoClobber     = NoClobber,
            RawRead       = RawRead,
            Reverse       = Reverse,
            HardSectors   = HardSectors,
            Erase         = Erase,
            Verify        = Verify,
            Precomp       = Precomp,
            GenTg43       = GenTg43,
            ExtraArgs     = ExtraArgs,
            ImageFile     = null
        };

        /// <summary>
        /// Constructs a <see cref="JobPreset"/> from an existing <see cref="GwParameters"/> instance,
        /// a job type, and a post-action list.
        /// </summary>
        /// <param name="p">The gw.exe parameters to snapshot.</param>
        /// <param name="jt">Read or write.</param>
        /// <param name="actions">Post-actions attached to the job.</param>
        /// <param name="presetName">Display name for the resulting preset.</param>
        /// <returns>A new <see cref="JobPreset"/> mirroring the provided parameters.</returns>
        public static JobPreset FromParameters(GwParameters p, JobType jt,
            List<PostAction> actions, string presetName = "Preset") => new()
        {
            PresetName    = presetName,
            JobType       = jt,
            Device        = p.Device,
            Drive         = p.Drive,
            DiskFormat    = p.DiskFormat,
            StartCylinder = p.StartCylinder,
            EndCylinder   = p.EndCylinder,
            Head          = p.Head,
            Step          = p.Step,
            HSwap         = p.HSwap,
            Head0Offset   = p.Head0Offset,
            Head1Offset   = p.Head1Offset,
            Revolutions   = p.Revolutions,
            Densel        = p.Densel,
            Bitrate       = p.Bitrate,
            Retries       = p.Retries,
            NoClobber     = p.NoClobber,
            RawRead       = p.RawRead,
            Reverse       = p.Reverse,
            HardSectors   = p.HardSectors,
            Erase         = p.Erase,
            Verify        = p.Verify,
            Precomp       = p.Precomp,
            GenTg43       = p.GenTg43,
            ExtraArgs     = p.ExtraArgs,
            OutputFolder  = "",
            PostActions   = actions.ConvertAll(a => new PostActionPreset
            {
                Name           = a.Name,
                ExecutablePath = a.ExecutablePath,
                Arguments      = a.Arguments,
                IsEnabled      = a.IsEnabled,
                ActionType     = a.ActionType,
                Order          = a.Order
            })
        };

        private static readonly JsonSerializerOptions _opts =
            new() { WriteIndented = true };

        /// <summary>
        /// Serialises this preset to a JSON file, creating parent directories as needed.
        /// </summary>
        /// <param name="path">Destination file path.</param>
        public void SaveToFile(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(this, _opts));
        }

        /// <summary>
        /// Deserialises a <see cref="JobPreset"/> from a JSON file.
        /// </summary>
        /// <param name="path">Source file path.</param>
        /// <returns>The deserialised preset.</returns>
        /// <exception cref="InvalidDataException">Thrown when the file is empty or cannot be deserialised.</exception>
        public static JobPreset LoadFromFile(string path)
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<JobPreset>(json, _opts)
                   ?? throw new InvalidDataException("Empty preset file.");
        }

        /// <summary>
        /// Default directory for preset files: <c>%APPDATA%\GreaseWeazleManager\Presets\</c>.
        /// </summary>
        public static string PresetsDirectory =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GreaseWeazleManager", "Presets");
    }

    /// <summary>
    /// Serialisable representation of a <see cref="PostAction"/>, used inside <see cref="JobPreset"/>
    /// to avoid GUID regeneration when the preset is deserialised.
    /// </summary>
    public class PostActionPreset
    {
        /// <summary>Display name of the post-action.</summary>
        public string         Name           { get; set; } = "";

        /// <summary>Path to the executable or script file.</summary>
        public string         ExecutablePath { get; set; } = "";

        /// <summary>Argument string (may contain expansion tokens).</summary>
        public string         Arguments      { get; set; } = "";

        /// <summary>Whether the action is enabled.</summary>
        public bool           IsEnabled      { get; set; } = true;

        /// <summary>How the action is launched.</summary>
        public PostActionType ActionType     { get; set; } = PostActionType.Executable;

        /// <summary>Execution order within the post-action sequence.</summary>
        public int            Order          { get; set; }

        /// <summary>
        /// Converts this serialisable snapshot back to a live <see cref="PostAction"/> with a freshly generated ID.
        /// </summary>
        /// <returns>A new <see cref="PostAction"/> instance.</returns>
        public PostAction ToPostAction() => new()
        {
            Name           = Name,
            ExecutablePath = ExecutablePath,
            Arguments      = Arguments,
            IsEnabled      = IsEnabled,
            ActionType     = ActionType,
            Order          = Order
        };
    }

    /// <summary>
    /// Expands file name patterns that contain counter and date/time tokens.
    /// </summary>
    /// <remarks>
    /// Supported tokens:
    /// <list type="bullet">
    ///   <item><c>{n}</c> — disk counter as a plain integer.</item>
    ///   <item><c>{n:FORMAT}</c> — disk counter with a .NET format specifier (e.g. <c>{n:D3}</c> → <c>001</c>).</item>
    ///   <item><c>{dt}</c> — current date/time using <see cref="JobPreset.DateTimeFormat"/>.</item>
    /// </list>
    /// </remarks>
    public static class FilePattern
    {
        /// <summary>
        /// Expands all recognised tokens in <paramref name="pattern"/>.
        /// </summary>
        /// <param name="pattern">Raw pattern string, e.g. <c>Disk_{n:D3}_{dt}.scp</c>.</param>
        /// <param name="index">Current disk counter (1-based).</param>
        /// <param name="dateTimeFormat">C# format string applied to <see cref="DateTime.Now"/> for the <c>{dt}</c> token.</param>
        /// <returns>The fully expanded file name string.</returns>
        public static string Expand(string pattern, int index, string dateTimeFormat)
        {
            if (string.IsNullOrWhiteSpace(pattern)) return pattern;

            string dt = DateTime.Now.ToString(dateTimeFormat);

            string result = Regex.Replace(pattern, @"\{n:([^}]+)\}", m =>
            {
                string fmt = m.Groups[1].Value;
                try { return index.ToString(fmt); }
                catch { return index.ToString(); }
            });

            result = result.Replace("{n}", index.ToString());
            result = result.Replace("{dt}", dt);

            return result;
        }

        /// <summary>
        /// Returns a preview of how <paramref name="pattern"/> will expand at the given counter index.
        /// </summary>
        /// <param name="pattern">Raw pattern string.</param>
        /// <param name="index">Disk counter to use for the preview.</param>
        /// <param name="dtFormat">Date/time format string.</param>
        /// <returns>The expanded preview string.</returns>
        public static string Preview(string pattern, int index, string dtFormat = "yyyyMMdd_HHmmss")
            => Expand(pattern, index, dtFormat);

        /// <summary>
        /// Returns <see langword="true"/> when <paramref name="pattern"/> contains at least one
        /// recognised expansion token (<c>{n}</c>, <c>{n:…}</c>, or <c>{dt}</c>).
        /// </summary>
        /// <param name="pattern">Pattern string to inspect.</param>
        public static bool HasTokens(string pattern) =>
            pattern.Contains("{n}") || Regex.IsMatch(pattern, @"\{n:[^}]+\}") || pattern.Contains("{dt}");
    }
}
