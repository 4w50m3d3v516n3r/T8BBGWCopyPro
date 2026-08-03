# Group Repetitive Jobs Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add multi-device "group" repetitive imaging jobs with LED-guided disk insertion, show device info in the next-disk dialog, turn the Settings Cancel button into "OK" after saving, and add a Blink button to device tiles.

**Architecture:** A new `GroupJobService` orchestrates batches above the existing single-disk machinery (`GwService.RunSingleDiskAsync` is reused unchanged, made `internal`). A pure `BatchInsertStateMachine` drives the blink/verify insert phase and is unit-tested; a thin `DriveProber` wraps short `gw.exe seek`/`rpm` calls. New `BatchInsertDialog` replaces `NextDiskDialog` for group jobs only.

**Tech Stack:** .NET 8 WinForms (`net8.0-windows`), xUnit for the new test project, `gw.exe` CLI (v0.24+ syntax).

**Spec:** `docs/superpowers/specs/2026-08-03-group-repetitive-jobs-design.md`

## Global Constraints

- All user-facing strings go through `L10n.T("key")` with entries in BOTH `_en` and `_de` dictionaries in `GreaseWeazleManager/Services/Localizer.cs`.
- Namespaces: models in `GwCopyPro.Models`, services in `GwCopyPro.Services`, forms in `GwCopyPro.Forms`, controls in `GwCopyPro.Controls` (note: csproj RootNamespace is `GWCopyPro` but all existing code uses `GwCopyPro.*` — follow the existing code).
- UI style: dark theme, `Consolas` fonts, `FlatStyle.Flat` buttons, colors copied from neighboring code. Match the existing hand-layout style (absolute `Location`/`Size`).
- XML doc comments on all public types/members (existing codebase convention).
- Build: `dotnet build T8BBGWCopyPro.sln`. Tests: `dotnet test GreaseWeazleManager.Tests/GreaseWeazleManager.Tests.csproj`.
- Commit after every task with the trailer `Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>`.
- A group needs ≥ 2 members and may not contain the same device (same `GreaseWeazleDevice.Id`) twice.
- Disk numbers (`{n}`) are assigned at batch start in group order and are never reused, including after failures.

---

### Task 1: Settings dialog — Cancel becomes "OK" after save

**Files:**
- Modify: `GreaseWeazleManager/Services/Localizer.cs` (both dictionaries)
- Modify: `GreaseWeazleManager/Forms/SettingsDialog.cs`

**Interfaces:**
- Consumes: `L10n.T(string)`.
- Produces: new localization key `settings.ok`.

No test infra exists yet for UI; this is a UI-only change verified by build + manual smoke at the end.

- [ ] **Step 1: Add localization keys**

In `Localizer.cs`, in the `_en` dictionary directly after `["settings.saved"]`:

```csharp
            ["settings.ok"]            = "OK",
```

In `_de` directly after the German `["settings.saved"]`:

```csharp
            ["settings.ok"]            = "OK",
```

- [ ] **Step 2: Keep a field reference to the Cancel button and retitle it on save**

In `SettingsDialog.cs`, add a field next to `lblSaved`:

```csharp
        private Button   btnCancel   = null!;
```

In `InitializeComponent()`, change the local `var btnCancel = MakeBtn(...)` to assign the field instead:

```csharp
            btnCancel = MakeBtn(L10n.T("settings.cancel"), 452, y - 2, 86, 28,
                Color.FromArgb(50, 25, 25), Color.FromArgb(200, 100, 100), Color.FromArgb(100, 50, 50));
```

At the end of `BtnSave_Click`, after `lblSaved.Text = L10n.T("settings.saved");` (order matters: `L10n.SetLanguage` has already run, so the new language is used):

```csharp
            btnCancel.Text = L10n.T("settings.ok");
```

- [ ] **Step 3: Build**

Run: `dotnet build T8BBGWCopyPro.sln`
Expected: Build succeeded.

- [ ] **Step 4: Commit**

```bash
git add GreaseWeazleManager/Services/Localizer.cs GreaseWeazleManager/Forms/SettingsDialog.cs
git commit -m "feat: settings Cancel button reads OK after saving"
```

---

### Task 2: NextDiskDialog shows device and drive

**Files:**
- Modify: `GreaseWeazleManager/Services/Localizer.cs`
- Modify: `GreaseWeazleManager/Forms/NextDiskDialog.cs`
- Modify: `GreaseWeazleManager/Forms/MainForm.cs` (`WireEvents`, `DiskCompleted` handler around line 300)

**Interfaces:**
- Consumes: `GwJob.Device` (`GreaseWeazleDevice?` with `Name`, `SerialPort`), `GwJob.Parameters.Drive` (`string?`).
- Produces: `NextDiskDialog` constructor gains two parameters: `string deviceText, string driveText` (see Step 2).

- [ ] **Step 1: Add localization keys**

`_en`, after `["nextdisk.title"]`:

```csharp
            ["nextdisk.device"]        = "Device: {0} — drive {1}",
            ["nextdisk.drive_auto"]    = "(auto)",
```

`_de`, after the German `["nextdisk.title"]`:

```csharp
            ["nextdisk.device"]        = "Gerät: {0} — Laufwerk {1}",
            ["nextdisk.drive_auto"]    = "(auto)",
```

- [ ] **Step 2: Extend the dialog**

In `NextDiskDialog.cs`, change the constructor signature (and the XML doc accordingly):

```csharp
        public NextDiskDialog(
            int      completedDiskNumber,
            string   completedFile,
            string   nextFile,
            TimeSpan lastDuration,
            string   dateTimeFormat,
            string   deviceText,
            string   driveText)
        {
            InitializeComponent(completedDiskNumber, completedFile, nextFile,
                                lastDuration, dateTimeFormat, deviceText, driveText);
            ...
```

Change `InitializeComponent` to accept `string deviceText, string driveText` and insert a device line directly after the "✓ done_disk" label block (after the first `y += 28;`):

```csharp
            AddLabel(string.Format(L10n.T("nextdisk.device"), deviceText, driveText),
                14, y, 520, 16,
                new Font("Consolas", 8.5f, FontStyle.Bold),
                Color.FromArgb(120, 190, 255));
            y += 22;
```

Grow the form so nothing clips: change `Size = new Size(560, 380);` to `Size = new Size(560, 402);` (MaximumSize/MinimumSize follow `Size`, they are assigned from it).

- [ ] **Step 3: Pass device info from MainForm**

In `MainForm.WireEvents()`, `DiskCompleted` handler, replace the `new NextDiskDialog(...)` call with:

```csharp
                    using var dlg = new NextDiskDialog(
                        e.DiskNumber,
                        e.CompletedFile,
                        e.NextFile,
                        e.Duration,
                        e.Job.DateTimeFormat,
                        e.Job.Device != null
                            ? $"{e.Job.Device.Name} ({e.Job.Device.SerialPort})"
                            : L10n.T("job_dlg.auto_device"),
                        string.IsNullOrWhiteSpace(e.Job.Parameters.Drive)
                            ? L10n.T("nextdisk.drive_auto")
                            : e.Job.Parameters.Drive);
```

- [ ] **Step 4: Build**

Run: `dotnet build T8BBGWCopyPro.sln`
Expected: Build succeeded.

- [ ] **Step 5: Commit**

```bash
git add GreaseWeazleManager/Services/Localizer.cs GreaseWeazleManager/Forms/NextDiskDialog.cs GreaseWeazleManager/Forms/MainForm.cs
git commit -m "feat: show device and drive in next-disk dialog"
```

---

### Task 3: Test project scaffold

**Files:**
- Create: `GreaseWeazleManager.Tests/GreaseWeazleManager.Tests.csproj`
- Create: `GreaseWeazleManager.Tests/SmokeTests.cs`
- Modify: `T8BBGWCopyPro.sln` (via `dotnet sln add`)
- Modify: `GreaseWeazleManager/GWCopyPro.csproj` (InternalsVisibleTo)

**Interfaces:**
- Produces: test project `GreaseWeazleManager.Tests` referencing `GWCopyPro`; internals of `GWCopyPro` visible to it.

- [ ] **Step 1: Create the project**

```bash
cd C:/src/T8BBGWCopyPro
dotnet new xunit -n GreaseWeazleManager.Tests -o GreaseWeazleManager.Tests
dotnet sln T8BBGWCopyPro.sln add GreaseWeazleManager.Tests/GreaseWeazleManager.Tests.csproj
```

- [ ] **Step 2: Fix the csproj for WinForms referencing**

Overwrite `GreaseWeazleManager.Tests/GreaseWeazleManager.Tests.csproj` (keep the xunit package versions the template generated if newer):

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.6.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.4" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\GreaseWeazleManager\GWCopyPro.csproj" />
  </ItemGroup>
</Project>
```

Delete the template's `UnitTest1.cs`.

- [ ] **Step 3: Expose internals to the test assembly**

In `GreaseWeazleManager/GWCopyPro.csproj`, add inside the root `<Project>`:

```xml
  <ItemGroup>
    <InternalsVisibleTo Include="GreaseWeazleManager.Tests" />
  </ItemGroup>
```

- [ ] **Step 4: Write a smoke test against existing code**

`GreaseWeazleManager.Tests/SmokeTests.cs`:

```csharp
using GwCopyPro.Models;
using Xunit;

namespace GreaseWeazleManager.Tests
{
    public class SmokeTests
    {
        [Fact]
        public void FilePattern_Expand_ReplacesCounterToken()
        {
            Assert.Equal("Disk_007.scp",
                FilePattern.Expand("Disk_{n:D3}.scp", 7, "yyyyMMdd"));
        }
    }
}
```

- [ ] **Step 5: Run tests**

Run: `dotnet test GreaseWeazleManager.Tests/GreaseWeazleManager.Tests.csproj`
Expected: 1 passed.

- [ ] **Step 6: Commit**

```bash
git add T8BBGWCopyPro.sln GreaseWeazleManager.Tests GreaseWeazleManager/GWCopyPro.csproj
git commit -m "test: add GreaseWeazleManager.Tests xunit project"
```

---

### Task 4: Group models — DeviceGroupMember, GroupRepetitiveJob, validation, batch numbering

**Files:**
- Create: `GreaseWeazleManager/Models/GroupModels.cs`
- Test: `GreaseWeazleManager.Tests/GroupModelsTests.cs`

**Interfaces:**
- Consumes: `GreaseWeazleDevice`, `GwParameters`, `GwJob`, `JobType`, `PostAction`, `FilePattern.Expand`.
- Produces (used by Tasks 7, 9, 10, 11, 12):
  - `class DeviceGroupMember { GreaseWeazleDevice Device; string Drive; bool IncludedThisBatch; bool Verified; bool LastBatchFailed; string? LastBatchError; string? LastBatchFile; GwJob? Job; CancellationTokenSource? BatchCts; }`
  - `class GroupRepetitiveJob { string Id; JobType JobType; GwParameters ParameterTemplate; List<PostAction> PostActions; string FilePattern; string OutputFolder; string DateTimeFormat; int NextDiskNumber; int BatchNumber; List<DeviceGroupMember> Members; static string? Validate(IReadOnlyList<DeviceGroupMember>); List<BatchAssignment> PrepareBatch(); }`
  - `record BatchAssignment(DeviceGroupMember Member, int DiskNumber, string FileName)`
  - `Validate` returns a **localization key** (`"job_dlg.group_min"` or `"job_dlg.group_dup_device"`) or `null` when valid.

- [ ] **Step 1: Write failing tests**

`GreaseWeazleManager.Tests/GroupModelsTests.cs`:

```csharp
using System.Collections.Generic;
using System.Linq;
using GwCopyPro.Models;
using Xunit;

namespace GreaseWeazleManager.Tests
{
    public class GroupModelsTests
    {
        private static DeviceGroupMember Member(string id, string port, string drive) => new()
        {
            Device = new GreaseWeazleDevice { Id = id, Name = "GW " + id, SerialPort = port },
            Drive  = drive
        };

        private static GroupRepetitiveJob Group(params DeviceGroupMember[] members) => new()
        {
            FilePattern    = "Disk_{n:D3}.scp",
            DateTimeFormat = "yyyyMMdd",
            Members        = new List<DeviceGroupMember>(members)
        };

        [Fact]
        public void Validate_RejectsFewerThanTwoMembers()
        {
            var err = GroupRepetitiveJob.Validate(new[] { Member("a1", "COM3", "0") });
            Assert.Equal("job_dlg.group_min", err);
        }

        [Fact]
        public void Validate_RejectsDuplicateDevice()
        {
            var err = GroupRepetitiveJob.Validate(new[]
                { Member("a1", "COM3", "0"), Member("a1", "COM3", "1") });
            Assert.Equal("job_dlg.group_dup_device", err);
        }

        [Fact]
        public void Validate_AcceptsTwoDistinctDevices()
        {
            var err = GroupRepetitiveJob.Validate(new[]
                { Member("a1", "COM3", "0"), Member("b2", "COM4", "0") });
            Assert.Null(err);
        }

        [Fact]
        public void PrepareBatch_AssignsSequentialNumbersInGroupOrder()
        {
            var g = Group(Member("a1", "COM3", "0"), Member("b2", "COM4", "1"));
            foreach (var m in g.Members) { m.IncludedThisBatch = true; m.Verified = true; }

            var batch = g.PrepareBatch();

            Assert.Equal(2, batch.Count);
            Assert.Equal(1, batch[0].DiskNumber);
            Assert.Equal("Disk_001.scp", batch[0].FileName);
            Assert.Equal(2, batch[1].DiskNumber);
            Assert.Same(g.Members[0], batch[0].Member);
            Assert.Equal(3, g.NextDiskNumber);
            Assert.Equal(1, g.BatchNumber);
        }

        [Fact]
        public void PrepareBatch_SkipsExcludedAndUnverifiedMembers()
        {
            var g = Group(Member("a1", "COM3", "0"), Member("b2", "COM4", "0"),
                          Member("c3", "COM5", "0"));
            g.Members[0].IncludedThisBatch = true; g.Members[0].Verified = true;
            g.Members[1].IncludedThisBatch = false; g.Members[1].Verified = true;
            g.Members[2].IncludedThisBatch = true; g.Members[2].Verified = false;

            var batch = g.PrepareBatch();

            Assert.Single(batch);
            Assert.Same(g.Members[0], batch[0].Member);
            Assert.Equal(2, g.NextDiskNumber);
        }

        [Fact]
        public void PrepareBatch_NeverReusesNumbersAcrossBatches()
        {
            var g = Group(Member("a1", "COM3", "0"), Member("b2", "COM4", "0"));
            foreach (var m in g.Members) { m.IncludedThisBatch = true; m.Verified = true; }

            var b1 = g.PrepareBatch();
            g.Members[1].LastBatchFailed = true;   // failure must not free number 2
            foreach (var m in g.Members) m.Verified = true;
            var b2 = g.PrepareBatch();

            Assert.Equal(new[] { 1, 2 }, b1.Select(a => a.DiskNumber));
            Assert.Equal(new[] { 3, 4 }, b2.Select(a => a.DiskNumber));
            Assert.Equal(2, g.BatchNumber);
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test GreaseWeazleManager.Tests/GreaseWeazleManager.Tests.csproj`
Expected: FAIL — `DeviceGroupMember` / `GroupRepetitiveJob` do not exist (compile error).

- [ ] **Step 3: Implement the models**

`GreaseWeazleManager/Models/GroupModels.cs`:

```csharp
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
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test GreaseWeazleManager.Tests/GreaseWeazleManager.Tests.csproj`
Expected: all pass.

- [ ] **Step 5: Commit**

```bash
git add GreaseWeazleManager/Models/GroupModels.cs GreaseWeazleManager.Tests/GroupModelsTests.cs
git commit -m "feat: add group repetitive job models with validation and batch numbering"
```

---

### Task 5: JobPreset group fields with round-trip test

**Files:**
- Modify: `GreaseWeazleManager/Models/JobPreset.cs`
- Test: `GreaseWeazleManager.Tests/JobPresetGroupTests.cs`

**Interfaces:**
- Produces (used by Tasks 10, 11):
  - `class GroupMemberPreset { string DeviceId; string DeviceName; string Drive; }`
  - `JobPreset.UseDeviceGroup : bool`
  - `JobPreset.GroupMembers : List<GroupMemberPreset>`

- [ ] **Step 1: Write failing test**

`GreaseWeazleManager.Tests/JobPresetGroupTests.cs`:

```csharp
using System.IO;
using GwCopyPro.Models;
using Xunit;

namespace GreaseWeazleManager.Tests
{
    public class JobPresetGroupTests
    {
        [Fact]
        public void GroupFields_SurviveSaveAndLoad()
        {
            var preset = new JobPreset
            {
                PresetName     = "Group",
                UseDeviceGroup = true
            };
            preset.GroupMembers.Add(new GroupMemberPreset
                { DeviceId = "a1b2c3d4", DeviceName = "GW Left", Drive = "0" });
            preset.GroupMembers.Add(new GroupMemberPreset
                { DeviceId = "e5f6a7b8", DeviceName = "GW Right", Drive = "b" });

            string path = Path.Combine(Path.GetTempPath(),
                Path.GetRandomFileName() + ".gwpreset");
            try
            {
                preset.SaveToFile(path);
                var loaded = JobPreset.LoadFromFile(path);

                Assert.True(loaded.UseDeviceGroup);
                Assert.Equal(2, loaded.GroupMembers.Count);
                Assert.Equal("a1b2c3d4", loaded.GroupMembers[0].DeviceId);
                Assert.Equal("GW Left",  loaded.GroupMembers[0].DeviceName);
                Assert.Equal("b",        loaded.GroupMembers[1].Drive);
            }
            finally { File.Delete(path); }
        }

        [Fact]
        public void OldPresetWithoutGroupFields_LoadsWithDefaults()
        {
            string path = Path.Combine(Path.GetTempPath(),
                Path.GetRandomFileName() + ".gwpreset");
            try
            {
                File.WriteAllText(path, "{\"PresetName\":\"Legacy\"}");
                var loaded = JobPreset.LoadFromFile(path);

                Assert.False(loaded.UseDeviceGroup);
                Assert.Empty(loaded.GroupMembers);
            }
            finally { File.Delete(path); }
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test GreaseWeazleManager.Tests/GreaseWeazleManager.Tests.csproj`
Expected: FAIL — `UseDeviceGroup` / `GroupMemberPreset` do not exist.

- [ ] **Step 3: Implement**

In `JobPreset.cs`, after the `OutputFolder` property (line ~107):

```csharp
        /// <summary>Whether this preset uses a device group for repetitive mode.</summary>
        public bool UseDeviceGroup { get; set; }

        /// <summary>Serialisable device-group member list (empty when <see cref="UseDeviceGroup"/> is off).</summary>
        public List<GroupMemberPreset> GroupMembers { get; set; } = new();
```

After the `PostActionPreset` class in the same file:

```csharp
    /// <summary>
    /// Serialisable snapshot of one device-group member: the device identity plus drive address.
    /// </summary>
    public class GroupMemberPreset
    {
        /// <summary>The <see cref="GreaseWeazleDevice.Id"/> of the member device.</summary>
        public string DeviceId { get; set; } = "";

        /// <summary>Device display name at save time, shown when the device is currently absent.</summary>
        public string DeviceName { get; set; } = "";

        /// <summary>Drive address: <c>0</c>, <c>1</c>, <c>a</c>, or <c>b</c>.</summary>
        public string Drive { get; set; } = "0";
    }
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test GreaseWeazleManager.Tests/GreaseWeazleManager.Tests.csproj`
Expected: all pass.

- [ ] **Step 5: Commit**

```bash
git add GreaseWeazleManager/Models/JobPreset.cs GreaseWeazleManager.Tests/JobPresetGroupTests.cs
git commit -m "feat: persist device groups in job presets"
```

---

### Task 6: DriveProber service (blink + disk probe)

**Files:**
- Create: `GreaseWeazleManager/Services/DriveProber.cs`
- Test: `GreaseWeazleManager.Tests/DriveProberTests.cs`

**Interfaces:**
- Consumes: gw.exe path (constructor parameter).
- Produces (used by Tasks 10 in dialog, 12, 13):
  - `enum DiskProbeResult { DiskPresent, NoDisk, DeviceError }`
  - `interface IDriveProber { Task BlinkOnceAsync(string comPort, string drive, CancellationToken ct); Task<DiskProbeResult> ProbeDiskAsync(string comPort, string drive, CancellationToken ct); }`
  - `class DriveProber : IDriveProber { DriveProber(string gwExePath); }`
  - `internal static DiskProbeResult DriveProber.InterpretProbeOutput(int exitCode, string output)` (unit-tested)

- [ ] **Step 1: Write failing tests for output interpretation**

`GreaseWeazleManager.Tests/DriveProberTests.cs`:

```csharp
using GwCopyPro.Services;
using Xunit;

namespace GreaseWeazleManager.Tests
{
    public class DriveProberTests
    {
        [Fact]
        public void InterpretProbeOutput_RpmLine_MeansDiskPresent()
        {
            var r = DriveProber.InterpretProbeOutput(0,
                "Opened /dev/COM3\nDrive 0: Motor spun up\nDrive 0: 300.12 RPM");
            Assert.Equal(DiskProbeResult.DiskPresent, r);
        }

        [Fact]
        public void InterpretProbeOutput_NoIndex_MeansNoDisk()
        {
            var r = DriveProber.InterpretProbeOutput(1,
                "Drive 0: No index pulses detected");
            Assert.Equal(DiskProbeResult.NoDisk, r);
        }

        [Fact]
        public void InterpretProbeOutput_NonZeroExitWithoutOutput_MeansDeviceError()
        {
            var r = DriveProber.InterpretProbeOutput(1, "Cannot open serial port COM3");
            Assert.Equal(DiskProbeResult.DeviceError, r);
        }

        [Fact]
        public void InterpretProbeOutput_ZeroExitWithoutRpm_MeansNoDisk()
        {
            var r = DriveProber.InterpretProbeOutput(0, "Drive 0: Motor spun up");
            Assert.Equal(DiskProbeResult.NoDisk, r);
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test GreaseWeazleManager.Tests/GreaseWeazleManager.Tests.csproj`
Expected: FAIL — `DriveProber` does not exist.

- [ ] **Step 3: Implement**

`GreaseWeazleManager/Services/DriveProber.cs`:

```csharp
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
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test GreaseWeazleManager.Tests/GreaseWeazleManager.Tests.csproj`
Expected: all pass.

- [ ] **Step 5: Commit**

```bash
git add GreaseWeazleManager/Services/DriveProber.cs GreaseWeazleManager.Tests/DriveProberTests.cs
git commit -m "feat: add DriveProber for LED blink and disk-presence probing"
```

---

### Task 7: BatchInsertStateMachine

**Files:**
- Create: `GreaseWeazleManager/Services/BatchInsertStateMachine.cs`
- Test: `GreaseWeazleManager.Tests/BatchInsertStateMachineTests.cs`

**Interfaces:**
- Produces (used by Task 10):
  - `enum MemberInsertState { Waiting, Blinking, DiskDetected, Excluded }`
  - `class BatchInsertStateMachine { BatchInsertStateMachine(IReadOnlyList<bool> initiallyIncluded); MemberInsertState State(int i); int? CurrentBlink { get; } bool CanStart { get; } bool IsIncluded(int i); void SetIncluded(int i, bool included); void MarkDetected(int i); event Action? StateChanged; }`
  - Semantics: explicit blink queue in constructor order; `MarkDetected` removes from queue; excluding removes from queue; re-including appends to queue end and clears detection; `CurrentBlink` = head of queue; `CanStart` = queue empty AND ≥ 1 included member. "Device error"/"no disk" outcomes do NOT change state — the row simply stays `Blinking` (the dialog shows the hint text separately).

- [ ] **Step 1: Write failing tests**

`GreaseWeazleManager.Tests/BatchInsertStateMachineTests.cs`:

```csharp
using GwCopyPro.Services;
using Xunit;

namespace GreaseWeazleManager.Tests
{
    public class BatchInsertStateMachineTests
    {
        private static BatchInsertStateMachine Sm(params bool[] included)
            => new(included);

        [Fact]
        public void FirstIncludedMemberBlinksInitially()
        {
            var sm = Sm(true, true, true);
            Assert.Equal(0, sm.CurrentBlink);
            Assert.Equal(MemberInsertState.Blinking, sm.State(0));
            Assert.Equal(MemberInsertState.Waiting,  sm.State(1));
            Assert.False(sm.CanStart);
        }

        [Fact]
        public void InitiallyExcludedMemberIsSkipped()
        {
            var sm = Sm(false, true);
            Assert.Equal(1, sm.CurrentBlink);
            Assert.Equal(MemberInsertState.Excluded, sm.State(0));
        }

        [Fact]
        public void MarkDetected_AdvancesBlinkToNextMember()
        {
            var sm = Sm(true, true);
            sm.MarkDetected(0);
            Assert.Equal(MemberInsertState.DiskDetected, sm.State(0));
            Assert.Equal(1, sm.CurrentBlink);
        }

        [Fact]
        public void AllDetected_EnablesStart()
        {
            var sm = Sm(true, true);
            sm.MarkDetected(0);
            sm.MarkDetected(1);
            Assert.Null(sm.CurrentBlink);
            Assert.True(sm.CanStart);
        }

        [Fact]
        public void ExcludingBlinkingMember_AdvancesImmediately()
        {
            var sm = Sm(true, true, true);
            sm.SetIncluded(0, false);
            Assert.Equal(1, sm.CurrentBlink);
            Assert.Equal(MemberInsertState.Excluded, sm.State(0));
        }

        [Fact]
        public void ExcludingAllMembers_DisablesStart()
        {
            var sm = Sm(true, true);
            sm.SetIncluded(0, false);
            sm.SetIncluded(1, false);
            Assert.Null(sm.CurrentBlink);
            Assert.False(sm.CanStart);
        }

        [Fact]
        public void ReIncludedMember_AppendsToQueueEnd()
        {
            var sm = Sm(true, true, true);
            sm.SetIncluded(0, false);       // queue: 1, 2
            sm.MarkDetected(1);             // queue: 2
            sm.SetIncluded(0, true);        // queue: 2, 0
            Assert.Equal(2, sm.CurrentBlink);
            sm.MarkDetected(2);
            Assert.Equal(0, sm.CurrentBlink);
            Assert.Equal(MemberInsertState.Blinking, sm.State(0));
        }

        [Fact]
        public void ReIncludingDetectedThenExcludedMember_RequiresNewDetection()
        {
            var sm = Sm(true, true);
            sm.MarkDetected(0);
            sm.SetIncluded(0, false);
            sm.SetIncluded(0, true);
            Assert.NotEqual(MemberInsertState.DiskDetected, sm.State(0));
            Assert.False(sm.CanStart);
        }

        [Fact]
        public void StateChanged_FiresOnTransitions()
        {
            var sm = Sm(true, true);
            int fired = 0;
            sm.StateChanged += () => fired++;
            sm.MarkDetected(0);
            sm.SetIncluded(1, false);
            Assert.Equal(2, fired);
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test GreaseWeazleManager.Tests/GreaseWeazleManager.Tests.csproj`
Expected: FAIL — type does not exist.

- [ ] **Step 3: Implement**

`GreaseWeazleManager/Services/BatchInsertStateMachine.cs`:

```csharp
using System;
using System.Collections.Generic;

namespace GwCopyPro.Services
{
    /// <summary>Visual state of one group member during the insert phase.</summary>
    public enum MemberInsertState
    {
        /// <summary>Included, queued behind the currently blinking drive.</summary>
        Waiting,
        /// <summary>This drive's LED is blinking — insert a disk here.</summary>
        Blinking,
        /// <summary>A disk was detected in this drive.</summary>
        DiskDetected,
        /// <summary>Excluded from the current batch.</summary>
        Excluded
    }

    /// <summary>
    /// Pure state machine for the batch insert phase: which drive blinks, which are
    /// verified, and when the batch may start. Owns no timers or processes — the
    /// dialog feeds it events and renders its state.
    /// </summary>
    public class BatchInsertStateMachine
    {
        private readonly bool[] _included;
        private readonly bool[] _detected;
        private readonly List<int> _queue = new();

        /// <summary>Raised after every state transition.</summary>
        public event Action? StateChanged;

        /// <summary>
        /// Initialises the machine. Queue order is index order; initially excluded
        /// members are not queued.
        /// </summary>
        /// <param name="initiallyIncluded">Per-member inclusion flags, in group order.</param>
        public BatchInsertStateMachine(IReadOnlyList<bool> initiallyIncluded)
        {
            _included = new bool[initiallyIncluded.Count];
            _detected = new bool[initiallyIncluded.Count];
            for (int i = 0; i < initiallyIncluded.Count; i++)
            {
                _included[i] = initiallyIncluded[i];
                if (_included[i]) _queue.Add(i);
            }
        }

        /// <summary>Index of the member whose drive should blink now, or <see langword="null"/>.</summary>
        public int? CurrentBlink => _queue.Count > 0 ? _queue[0] : null;

        /// <summary>Whether the batch may start: every included member verified, at least one included.</summary>
        public bool CanStart
        {
            get
            {
                if (_queue.Count > 0) return false;
                for (int i = 0; i < _included.Length; i++)
                    if (_included[i]) return true;
                return false;
            }
        }

        /// <summary>Whether the member takes part in this batch.</summary>
        public bool IsIncluded(int i) => _included[i];

        /// <summary>Current visual state of the member.</summary>
        public MemberInsertState State(int i)
        {
            if (!_included[i]) return MemberInsertState.Excluded;
            if (_detected[i])  return MemberInsertState.DiskDetected;
            return CurrentBlink == i ? MemberInsertState.Blinking : MemberInsertState.Waiting;
        }

        /// <summary>
        /// Includes or excludes a member. Excluding removes it from the blink queue;
        /// re-including clears any previous detection and appends it to the queue end.
        /// </summary>
        public void SetIncluded(int i, bool included)
        {
            if (_included[i] == included) return;
            _included[i] = included;
            if (included)
            {
                _detected[i] = false;
                _queue.Add(i);
            }
            else
            {
                _queue.Remove(i);
            }
            StateChanged?.Invoke();
        }

        /// <summary>Records that a disk was detected in the member's drive.</summary>
        public void MarkDetected(int i)
        {
            _detected[i] = true;
            _queue.Remove(i);
            StateChanged?.Invoke();
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test GreaseWeazleManager.Tests/GreaseWeazleManager.Tests.csproj`
Expected: all pass.

- [ ] **Step 5: Commit**

```bash
git add GreaseWeazleManager/Services/BatchInsertStateMachine.cs GreaseWeazleManager.Tests/BatchInsertStateMachineTests.cs
git commit -m "feat: add batch insert-phase state machine"
```

---

### Task 8: GwService refactor — expose single-disk run, extract output-path resolution

**Files:**
- Modify: `GreaseWeazleManager/Services/GwService.cs`
- Test: `GreaseWeazleManager.Tests/GwServicePathTests.cs`

**Interfaces:**
- Produces (used by Task 9):
  - `internal Task<bool> GwService.RunSingleDiskAsync(GwJob job, CancellationToken ct)` (was `private`; body unchanged)
  - `internal static void GwService.ResetTracks(GwJob job)` (was `private`; body unchanged)
  - `internal static string GwService.ResolveOutputFile(string outputFolder, string? currentImageFile, string fileName)` — combines the expanded file name with the resolved folder using the existing precedence rules.

- [ ] **Step 1: Write failing tests for path resolution**

`GreaseWeazleManager.Tests/GwServicePathTests.cs`:

```csharp
using System;
using System.IO;
using GwCopyPro.Services;
using Xunit;

namespace GreaseWeazleManager.Tests
{
    public class GwServicePathTests
    {
        [Fact]
        public void AbsoluteOutputFolder_IsUsedDirectly()
        {
            string f = GwService.ResolveOutputFile(@"C:\images", null, "d1.scp");
            Assert.Equal(@"C:\images\d1.scp", f);
        }

        [Fact]
        public void RelativeOutputFolder_ResolvesAgainstBaseDirectory()
        {
            string f = GwService.ResolveOutputFile("out", null, "d1.scp");
            Assert.Equal(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "out", "d1.scp"), f);
        }

        [Fact]
        public void EmptyFolderWithRootedImageFile_UsesImageFileDirectory()
        {
            string f = GwService.ResolveOutputFile("", @"C:\old\prev.scp", "d2.scp");
            Assert.Equal(@"C:\old\d2.scp", f);
        }

        [Fact]
        public void EmptyFolderWithoutImageFile_FallsBackToDesktop()
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string f = GwService.ResolveOutputFile("", null, "d3.scp");
            Assert.Equal(Path.Combine(desktop, "d3.scp"), f);
        }

        [Fact]
        public void RootedFileName_IsReturnedUnchanged()
        {
            string f = GwService.ResolveOutputFile(@"C:\images", null, @"D:\direct\d4.scp");
            Assert.Equal(@"D:\direct\d4.scp", f);
        }
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test GreaseWeazleManager.Tests/GreaseWeazleManager.Tests.csproj`
Expected: FAIL — `ResolveOutputFile` does not exist.

- [ ] **Step 3: Implement the refactor**

In `GwService.cs`:

1. Change `private async Task<bool> RunSingleDiskAsync(` to `internal async Task<bool> RunSingleDiskAsync(`.
2. Change `private static void ResetTracks(` to `internal static void ResetTracks(`.
3. Add the new method after `RunRepetitiveAsync`:

```csharp
        /// <summary>
        /// Combines an expanded repetitive file name with the resolved output folder.
        /// Folder precedence: absolute <paramref name="outputFolder"/>; relative folder
        /// under the application base directory; the directory of
        /// <paramref name="currentImageFile"/> when rooted; the user's Desktop.
        /// A rooted <paramref name="fileName"/> is returned unchanged.
        /// </summary>
        internal static string ResolveOutputFile(string outputFolder,
            string? currentImageFile, string fileName)
        {
            if (Path.IsPathRooted(fileName)) return fileName;

            string folder;
            if (!string.IsNullOrWhiteSpace(outputFolder))
            {
                folder = Path.IsPathRooted(outputFolder)
                    ? outputFolder
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, outputFolder);
            }
            else if (!string.IsNullOrWhiteSpace(currentImageFile) &&
                     Path.IsPathRooted(currentImageFile))
            {
                folder = Path.GetDirectoryName(currentImageFile)!;
            }
            else
            {
                folder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            return Path.Combine(folder, fileName);
        }
```

4. Replace the folder-resolution block inside `RunRepetitiveAsync` (the `string folder; if (...) ... if (!Path.IsPathRooted(file)) file = Path.Combine(folder, file);` block, currently lines ~172-190) with:

```csharp
                file = ResolveOutputFile(job.OutputFolder, job.Parameters.ImageFile, file);
```

(Keep the subsequent `Directory.CreateDirectory(Path.GetDirectoryName(file)!);` line.)

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test GreaseWeazleManager.Tests/GreaseWeazleManager.Tests.csproj`
Expected: all pass. Also run `dotnet build T8BBGWCopyPro.sln` — build succeeds.

- [ ] **Step 5: Commit**

```bash
git add GreaseWeazleManager/Services/GwService.cs GreaseWeazleManager.Tests/GwServicePathTests.cs
git commit -m "refactor: expose single-disk run and extract output path resolution"
```

---

### Task 9: GroupJobService

**Files:**
- Create: `GreaseWeazleManager/Services/GroupJobService.cs`

**Interfaces:**
- Consumes: `GwService.RunSingleDiskAsync` (internal, Task 8), `GwService.ResetTracks`, `GwService.ResolveOutputFile`, `GroupRepetitiveJob.PrepareBatch` (Task 4).
- Produces (used by Task 12):
  - `class BatchPromptEventArgs : EventArgs { GroupRepetitiveJob Group; void Signal(bool startBatch); }`
  - `class GroupJobEventArgs : EventArgs { GroupRepetitiveJob Group; }`
  - `class GroupJobService { GroupJobService(GwService gw); event EventHandler<GroupJobEventArgs>? MemberJobsCreated; event EventHandler<BatchPromptEventArgs>? BatchPromptRequested; event EventHandler<GroupJobEventArgs>? GroupCompleted; Task RunAsync(GroupRepetitiveJob group, CancellationToken ct); }`
  - Event contract: `MemberJobsCreated` fires once after member `GwJob`s exist (UI creates panels). `BatchPromptRequested` fires before every batch; the UI shows `BatchInsertDialog`, updates `IncludedThisBatch`/`Verified` on members, and calls `Signal(true)` to run the batch or `Signal(false)` to finish the job. `GroupCompleted` fires once at the end.

This class is orchestration glue over already-tested parts (state machine, numbering, path resolution, single-disk runs); it is exercised by the manual smoke test, not unit tests.

- [ ] **Step 1: Implement**

`GreaseWeazleManager/Services/GroupJobService.cs`:

```csharp
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
```

- [ ] **Step 2: Build and run existing tests**

Run: `dotnet build T8BBGWCopyPro.sln` then `dotnet test GreaseWeazleManager.Tests/GreaseWeazleManager.Tests.csproj`
Expected: build succeeds, all tests pass.

- [ ] **Step 3: Commit**

```bash
git add GreaseWeazleManager/Services/GroupJobService.cs
git commit -m "feat: add GroupJobService orchestrating parallel batch imaging"
```

---

### Task 10: BatchInsertDialog

**Files:**
- Modify: `GreaseWeazleManager/Services/Localizer.cs`
- Create: `GreaseWeazleManager/Forms/BatchInsertDialog.cs`

**Interfaces:**
- Consumes: `GroupRepetitiveJob`, `DeviceGroupMember` (Task 4), `BatchInsertStateMachine`, `MemberInsertState` (Task 7), `IDriveProber`, `DiskProbeResult` (Task 6), `FilePattern.Expand`.
- Produces (used by Task 12):
  - `class BatchInsertDialog : Form { BatchInsertDialog(GroupRepetitiveJob group, IDriveProber prober); bool StartBatchChosen { get; } }`
  - On close with `StartBatchChosen == true`, every member's `IncludedThisBatch` and `Verified` reflect the dialog state. `StartBatchChosen == false` means finish the job.

- [ ] **Step 1: Add localization keys**

`_en`, after the `nextdisk.*` block:

```csharp
            ["batch.title"]            = "Batch {0} — insert disks",
            ["batch.files_label"]      = "This batch will write:",
            ["batch.state_waiting"]    = "waiting",
            ["batch.state_blinking"]   = "● INSERT DISK — LED blinking",
            ["batch.state_detected"]   = "✓ disk detected",
            ["batch.state_excluded"]   = "— excluded",
            ["batch.no_disk_hint"]     = "No disk detected — insert a disk or exclude the drive.",
            ["batch.dev_error_hint"]   = "Device not responding — check the connection or exclude the drive.",
            ["batch.last_ok"]          = "last: ✓ {0}",
            ["batch.last_fail"]        = "last: ✗ {0}",
            ["batch.chk_include"]      = "include",
            ["batch.btn_inserted"]     = "✔  Disk inserted",
            ["batch.btn_start"]        = "▶  Start batch",
            ["batch.btn_finish"]       = "✕  Finish job",
```

`_de`, after the German `nextdisk.*` block:

```csharp
            ["batch.title"]            = "Stapel {0} — Disketten einlegen",
            ["batch.files_label"]      = "Dieser Stapel schreibt:",
            ["batch.state_waiting"]    = "wartet",
            ["batch.state_blinking"]   = "● DISKETTE EINLEGEN — LED blinkt",
            ["batch.state_detected"]   = "✓ Diskette erkannt",
            ["batch.state_excluded"]   = "— ausgeschlossen",
            ["batch.no_disk_hint"]     = "Keine Diskette erkannt — Diskette einlegen oder Laufwerk ausschließen.",
            ["batch.dev_error_hint"]   = "Gerät antwortet nicht — Verbindung prüfen oder Laufwerk ausschließen.",
            ["batch.last_ok"]          = "zuletzt: ✓ {0}",
            ["batch.last_fail"]        = "zuletzt: ✗ {0}",
            ["batch.chk_include"]      = "einbeziehen",
            ["batch.btn_inserted"]     = "✔  Diskette eingelegt",
            ["batch.btn_start"]        = "▶  Stapel starten",
            ["batch.btn_finish"]       = "✕  Auftrag beenden",
```

- [ ] **Step 2: Implement the dialog**

`GreaseWeazleManager/Forms/BatchInsertDialog.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GwCopyPro.Models;
using GwCopyPro.Services;

namespace GwCopyPro.Forms
{
    /// <summary>
    /// Insert-phase dialog for group repetitive jobs. Shows one row per group member,
    /// blinks the current drive's LED via <see cref="IDriveProber"/>, verifies disk
    /// insertion on confirmation, and lets the user include/exclude drives per batch.
    /// </summary>
    public class BatchInsertDialog : Form
    {
        /// <summary><see langword="true"/> when the user chose Start batch; <see langword="false"/> for Finish job.</summary>
        public bool StartBatchChosen { get; private set; }

        private readonly GroupRepetitiveJob      _group;
        private readonly IDriveProber            _prober;
        private readonly BatchInsertStateMachine _sm;
        private readonly CancellationTokenSource _cts = new();
        private readonly System.Windows.Forms.Timer _blinkTimer;

        private readonly List<Label>    _stateLabels = new();
        private readonly List<CheckBox> _includeChecks = new();
        private Label  _lblHint     = null!;
        private Button _btnInserted = null!;
        private Button _btnStart    = null!;
        private bool   _busy;      // any gw call is in flight (blink or probe)
        private bool   _probing;   // a probe specifically — disables the confirm button

        /// <summary>Initialises the dialog for the group's next batch.</summary>
        /// <param name="group">The group job (member state is read and written).</param>
        /// <param name="prober">Prober used for blinking and disk detection.</param>
        public BatchInsertDialog(GroupRepetitiveJob group, IDriveProber prober)
        {
            _group  = group;
            _prober = prober;

            var included = new List<bool>();
            foreach (var m in group.Members) included.Add(m.IncludedThisBatch);
            _sm = new BatchInsertStateMachine(included);
            _sm.StateChanged += () => { RenderRows(); UpdateButtons(); };

            InitializeComponent();
            RenderRows();
            UpdateButtons();

            _blinkTimer = new System.Windows.Forms.Timer { Interval = 1500 };
            _blinkTimer.Tick += async (s, e) => await BlinkTickAsync();
            _blinkTimer.Start();
        }

        /// <summary>Builds and lays out all child controls.</summary>
        private void InitializeComponent()
        {
            int rows = _group.Members.Count;
            Text            = string.Format(L10n.T("batch.title"), _group.BatchNumber + 1);
            Size            = new Size(700, 240 + rows * 56);
            MinimumSize     = Size;
            MaximumSize     = Size;
            BackColor       = Color.FromArgb(18, 22, 32);
            ForeColor       = Color.FromArgb(180, 210, 255);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition   = FormStartPosition.CenterParent;
            MaximizeBox     = false;
            ControlBox      = false;

            var accent = new Panel
            {
                Dock = DockStyle.Top, Height = 4,
                BackColor = Color.FromArgb(40, 160, 80)
            };
            Controls.Add(accent);

            int y = 16;
            Controls.Add(new Label
            {
                Text      = string.Format(L10n.T("batch.title"), _group.BatchNumber + 1),
                Location  = new Point(14, y), Size = new Size(660, 20),
                Font      = new Font("Consolas", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 200, 255), BackColor = Color.Transparent
            });
            y += 30;

            for (int i = 0; i < rows; i++)
            {
                var m = _group.Members[i];

                var chk = new CheckBox
                {
                    Text      = L10n.T("batch.chk_include"),
                    Checked   = m.IncludedThisBatch,
                    Location  = new Point(14, y + 6),
                    Size      = new Size(120, 20),
                    Font      = new Font("Consolas", 8f),
                    ForeColor = Color.FromArgb(130, 160, 200),
                    BackColor = Color.Transparent
                };
                int idx = i;
                chk.CheckedChanged += (s, e) => _sm.SetIncluded(idx, chk.Checked);
                _includeChecks.Add(chk);
                Controls.Add(chk);

                Controls.Add(new Label
                {
                    Text      = $"{m.Device.Name} ({m.Device.SerialPort}) — drive {m.Drive}",
                    Location  = new Point(140, y),
                    Size      = new Size(320, 18),
                    Font      = new Font("Consolas", 8.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(120, 190, 255),
                    BackColor = Color.Transparent
                });

                string last = m.LastBatchFailed
                    ? string.Format(L10n.T("batch.last_fail"), m.LastBatchError ?? "?")
                    : m.LastBatchFile != null
                        ? string.Format(L10n.T("batch.last_ok"), Path.GetFileName(m.LastBatchFile))
                        : "";
                Controls.Add(new Label
                {
                    Text      = last,
                    Location  = new Point(140, y + 18),
                    Size      = new Size(320, 15),
                    Font      = new Font("Consolas", 7.5f),
                    ForeColor = m.LastBatchFailed
                        ? Color.FromArgb(220, 90, 90) : Color.FromArgb(90, 120, 160),
                    BackColor = Color.Transparent
                });

                var lblState = new Label
                {
                    Location  = new Point(468, y + 4),
                    Size      = new Size(212, 32),
                    Font      = new Font("Consolas", 8.5f, FontStyle.Bold),
                    BackColor = Color.Transparent
                };
                _stateLabels.Add(lblState);
                Controls.Add(lblState);

                y += 56;
            }

            _lblHint = new Label
            {
                Text      = "",
                Location  = new Point(14, y), Size = new Size(660, 18),
                Font      = new Font("Consolas", 8f, FontStyle.Italic),
                ForeColor = Color.FromArgb(220, 180, 80), BackColor = Color.Transparent
            };
            Controls.Add(_lblHint);
            y += 26;

            var lblFiles = new Label
            {
                Text      = L10n.T("batch.files_label") + " " +
                            Models.FilePattern.Expand(_group.FilePattern,
                                _group.NextDiskNumber, _group.DateTimeFormat) + " …",
                Location  = new Point(14, y), Size = new Size(660, 16),
                Font      = new Font("Consolas", 7.5f),
                ForeColor = Color.FromArgb(90, 130, 170), BackColor = Color.Transparent
            };
            Controls.Add(lblFiles);
            y += 30;

            _btnInserted = MakeBtn(L10n.T("batch.btn_inserted"), 14, y, 210, 40,
                Color.FromArgb(25, 45, 80), Color.FromArgb(120, 175, 255), Color.FromArgb(50, 90, 160));
            _btnInserted.Click += async (s, e) => await ConfirmInsertedAsync();

            _btnStart = MakeBtn(L10n.T("batch.btn_start"), 238, y, 230, 40,
                Color.FromArgb(18, 65, 32), Color.FromArgb(80, 230, 120), Color.FromArgb(45, 140, 75));
            _btnStart.Font = new Font("Consolas", 10f, FontStyle.Bold);
            _btnStart.Click += (s, e) => CloseWithChoice(startBatch: true);

            var btnFinish = MakeBtn(L10n.T("batch.btn_finish"), 482, y, 198, 40,
                Color.FromArgb(55, 20, 20), Color.FromArgb(220, 90, 90), Color.FromArgb(120, 45, 45));
            btnFinish.Click += (s, e) => CloseWithChoice(startBatch: false);

            Controls.AddRange(new Control[] { _btnInserted, _btnStart, btnFinish });
        }

        /// <summary>Creates a flat-styled button.</summary>
        private static Button MakeBtn(string text, int x, int y, int w, int h,
            Color bg, Color fg, Color border)
        {
            var b = new Button
            {
                Text = text, Location = new Point(x, y), Size = new Size(w, h),
                FlatStyle = FlatStyle.Flat, BackColor = bg, ForeColor = fg,
                Font = new Font("Consolas", 8.5f)
            };
            b.FlatAppearance.BorderColor = border;
            return b;
        }

        /// <summary>Refreshes all per-row state labels from the state machine.</summary>
        private void RenderRows()
        {
            for (int i = 0; i < _stateLabels.Count; i++)
            {
                var (text, color) = _sm.State(i) switch
                {
                    MemberInsertState.Blinking     =>
                        (L10n.T("batch.state_blinking"), Color.FromArgb(240, 200, 60)),
                    MemberInsertState.DiskDetected =>
                        (L10n.T("batch.state_detected"), Color.FromArgb(80, 215, 110)),
                    MemberInsertState.Excluded     =>
                        (L10n.T("batch.state_excluded"), Color.FromArgb(110, 120, 140)),
                    _                              =>
                        (L10n.T("batch.state_waiting"), Color.FromArgb(90, 120, 160))
                };
                _stateLabels[i].Text      = text;
                _stateLabels[i].ForeColor = color;
            }
        }

        /// <summary>Enables/disables the confirm and start buttons from the current state.</summary>
        private void UpdateButtons()
        {
            _btnInserted.Enabled = _sm.CurrentBlink != null && !_probing;
            _btnStart.Enabled    = _sm.CanStart;
        }

        /// <summary>Fires one LED blink pulse on the currently blinking drive.</summary>
        private async Task BlinkTickAsync()
        {
            if (_busy || _sm.CurrentBlink is not int i) return;
            var m = _group.Members[i];
            _busy = true;
            try   { await _prober.BlinkOnceAsync(m.Device.SerialPort, m.Drive, _cts.Token); }
            catch { }
            finally { _busy = false; UpdateButtons(); }
        }

        /// <summary>
        /// Probes the currently blinking drive. On success advances the queue; otherwise
        /// keeps the drive blinking and shows the matching hint.
        /// </summary>
        private async Task ConfirmInsertedAsync()
        {
            if (_sm.CurrentBlink is not int i || _busy) return;
            var m = _group.Members[i];
            _busy    = true;
            _probing = true;
            _btnInserted.Enabled = false;
            _lblHint.Text = "";
            try
            {
                var r = await _prober.ProbeDiskAsync(m.Device.SerialPort, m.Drive, _cts.Token);
                switch (r)
                {
                    case DiskProbeResult.DiskPresent:
                        _sm.MarkDetected(i);
                        break;
                    case DiskProbeResult.NoDisk:
                        _lblHint.Text = L10n.T("batch.no_disk_hint");
                        break;
                    default:
                        _lblHint.Text = L10n.T("batch.dev_error_hint");
                        break;
                }
            }
            finally { _busy = false; _probing = false; UpdateButtons(); }
        }

        /// <summary>Writes dialog state back to the members and closes.</summary>
        private void CloseWithChoice(bool startBatch)
        {
            StartBatchChosen = startBatch;
            for (int i = 0; i < _group.Members.Count; i++)
            {
                _group.Members[i].IncludedThisBatch = _sm.IsIncluded(i);
                _group.Members[i].Verified =
                    _sm.State(i) == MemberInsertState.DiskDetected;
            }
            DialogResult = startBatch ? DialogResult.OK : DialogResult.Cancel;
        }

        /// <summary>Stops the blink timer and cancels in-flight gw calls.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _blinkTimer?.Stop();
                _blinkTimer?.Dispose();
                _cts.Cancel();
                _cts.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
```

- [ ] **Step 3: Build and test**

Run: `dotnet build T8BBGWCopyPro.sln` then `dotnet test GreaseWeazleManager.Tests/GreaseWeazleManager.Tests.csproj`
Expected: build succeeds, tests pass.

- [ ] **Step 4: Commit**

```bash
git add GreaseWeazleManager/Services/Localizer.cs GreaseWeazleManager/Forms/BatchInsertDialog.cs
git commit -m "feat: add batch insert dialog with LED-guided disk insertion"
```

---

### Task 11: NewJobDialog — device group UI, validation, GroupResult, presets

**Files:**
- Modify: `GreaseWeazleManager/Services/Localizer.cs`
- Modify: `GreaseWeazleManager/Forms/NewJobDialog.cs`

**Interfaces:**
- Consumes: `GroupRepetitiveJob`, `DeviceGroupMember`, `GroupRepetitiveJob.Validate` (Task 4), `GroupMemberPreset`, `JobPreset.UseDeviceGroup`, `JobPreset.GroupMembers` (Task 5).
- Produces (used by Task 12):
  - `NewJobDialog.GroupResult : GroupRepetitiveJob?` — non-null (and `Result` null) when the user started a group job.

- [ ] **Step 1: Add localization keys**

`_en`, after the last existing `job_dlg.*` key:

```csharp
            ["job_dlg.use_group"]        = "Use device group (parallel batch imaging)",
            ["job_dlg.group_add"]        = "+ Add",
            ["job_dlg.group_remove"]     = "− Remove",
            ["job_dlg.group_col_device"] = "Device",
            ["job_dlg.group_col_drive"]  = "Drive",
            ["job_dlg.group_min"]        = "A device group needs at least 2 drives.",
            ["job_dlg.group_dup_device"] = "Each GreaseWeazle device may appear only once in a group.",
            ["job_dlg.group_missing"]    = "A group device is not connected: {0}",
            ["job_dlg.group_cap"]        = "Device Group",
            ["job_dlg.group_needs_repeat"] = "Device groups require repetitive mode with a file pattern.",
```

`_de`, after the last German `job_dlg.*` key:

```csharp
            ["job_dlg.use_group"]        = "Gerätegruppe verwenden (paralleles Stapel-Imaging)",
            ["job_dlg.group_add"]        = "+ Hinzufügen",
            ["job_dlg.group_remove"]     = "− Entfernen",
            ["job_dlg.group_col_device"] = "Gerät",
            ["job_dlg.group_col_drive"]  = "Laufwerk",
            ["job_dlg.group_min"]        = "Eine Gerätegruppe benötigt mindestens 2 Laufwerke.",
            ["job_dlg.group_dup_device"] = "Jedes GreaseWeazle-Gerät darf nur einmal in einer Gruppe vorkommen.",
            ["job_dlg.group_missing"]    = "Ein Gruppengerät ist nicht verbunden: {0}",
            ["job_dlg.group_cap"]        = "Gerätegruppe",
            ["job_dlg.group_needs_repeat"] = "Gerätegruppen erfordern den Wiederholmodus mit Dateimuster.",
```

- [ ] **Step 2: Add fields and group UI to the Repeat tab**

In `NewJobDialog.cs`, add fields after `txtPresetName`:

```csharp
        private CheckBox chkUseGroup     = null!;
        private ComboBox cmbGroupDevice  = null!;
        private ComboBox cmbGroupDrive   = null!;
        private ListView lvGroupMembers  = null!;
```

Add a public result property after `Result`:

```csharp
        /// <summary>Gets the group job created when the user starts a device-group job, or <see langword="null"/>.</summary>
        public GroupRepetitiveJob? GroupResult { get; private set; }
```

At the end of `BuildRepeatTab(TabPage tab)` (after the preset-name block, continuing with the running `y`):

```csharp
            y += 32;
            tab.Controls.Add(Sep(10, y, 760)); y += 10;

            chkUseGroup = MkChk(L10n.T("job_dlg.use_group"), 10, y);
            chkUseGroup.Font = new Font("Consolas", 9f, FontStyle.Bold);
            chkUseGroup.ForeColor = Color.FromArgb(120, 190, 255);
            chkUseGroup.CheckedChanged += (s, e) =>
            {
                bool on = chkUseGroup.Checked;
                cmbGroupDevice.Enabled = on;
                cmbGroupDrive.Enabled  = on;
                lvGroupMembers.Enabled = on;
                if (on && !chkRepetitive.Checked) chkRepetitive.Checked = true;
            };
            tab.Controls.Add(chkUseGroup);

            y += 26;
            cmbGroupDevice = MkCombo(10, y, 320);
            foreach (var d in _devices) cmbGroupDevice.Items.Add(d);
            if (cmbGroupDevice.Items.Count > 0) cmbGroupDevice.SelectedIndex = 0;

            cmbGroupDrive = MkCombo(338, y, 70);
            cmbGroupDrive.Items.AddRange(new object[] { "0", "1", "a", "b" });
            cmbGroupDrive.SelectedIndex = 0;

            var btnGroupAdd = MakeBtn(L10n.T("job_dlg.group_add"), 416, y, 110, 22,
                Color.FromArgb(18, 60, 32), Color.FromArgb(90, 220, 120), Color.FromArgb(40, 120, 65));
            btnGroupAdd.Click += (s, e) =>
            {
                if (cmbGroupDevice.SelectedItem is not GreaseWeazleDevice dev) return;
                var item = new ListViewItem(dev.ToString());
                item.SubItems.Add(cmbGroupDrive.SelectedItem?.ToString() ?? "0");
                item.Tag = dev;
                lvGroupMembers.Items.Add(item);
            };

            var btnGroupRemove = MakeBtn(L10n.T("job_dlg.group_remove"), 534, y, 110, 22,
                Color.FromArgb(60, 20, 20), Color.FromArgb(200, 80, 80), Color.FromArgb(100, 40, 40));
            btnGroupRemove.Click += (s, e) =>
            {
                foreach (ListViewItem it in lvGroupMembers.SelectedItems)
                    lvGroupMembers.Items.Remove(it);
            };

            tab.Controls.AddRange(new Control[]
                { cmbGroupDevice, cmbGroupDrive, btnGroupAdd, btnGroupRemove });

            y += 28;
            lvGroupMembers = new ListView
            {
                Location      = new Point(10, y),
                Size          = new Size(760, 86),
                View          = View.Details,
                FullRowSelect = true,
                BackColor     = Color.FromArgb(28, 34, 48),
                ForeColor     = Color.FromArgb(200, 230, 255),
                Font          = new Font("Consolas", 8.5f),
                HeaderStyle   = ColumnHeaderStyle.Nonclickable
            };
            lvGroupMembers.Columns.Add(L10n.T("job_dlg.group_col_device"), 480);
            lvGroupMembers.Columns.Add(L10n.T("job_dlg.group_col_drive"), 120);
            tab.Controls.Add(lvGroupMembers);

            cmbGroupDevice.Enabled = false;
            cmbGroupDrive.Enabled  = false;
            lvGroupMembers.Enabled = false;
```

If the added controls exceed the tab height, reduce `lvGroupMembers` height to fit — the tab area is 630 px tall; keep everything above y = 600.

**Note:** `PopulateDevices()` runs after `InitializeComponent()` in the constructor and populates `cmbDevice` only; `cmbGroupDevice` is filled directly in `BuildRepeatTab` from `_devices` (already assigned by the constructor before `InitializeComponent` is called).

- [ ] **Step 3: Build the group members list and validate in BtnOk_Click**

Add a helper method after `BuildPreset()`:

```csharp
        /// <summary>Reads the group member rows from the list view.</summary>
        /// <returns>Members in row order.</returns>
        private List<DeviceGroupMember> ReadGroupMembers()
        {
            var members = new List<DeviceGroupMember>();
            foreach (ListViewItem item in lvGroupMembers.Items)
                if (item.Tag is GreaseWeazleDevice dev)
                    members.Add(new DeviceGroupMember
                    {
                        Device = dev,
                        Drive  = item.SubItems[1].Text
                    });
            return members;
        }
```

In `BtnOk_Click`, insert at the very top (before the missing-image check):

```csharp
            if (chkUseGroup?.Checked ?? false)
            {
                if (!(chkRepetitive?.Checked ?? false) ||
                    !Models.FilePattern.HasTokens(txtFilePattern?.Text ?? ""))
                {
                    MessageBox.Show(L10n.T("job_dlg.group_needs_repeat"),
                        L10n.T("job_dlg.group_cap"),
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                    return;
                }

                var members = ReadGroupMembers();
                string? err = GroupRepetitiveJob.Validate(members);
                if (err != null)
                {
                    MessageBox.Show(L10n.T(err), L10n.T("job_dlg.group_cap"),
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                    return;
                }

                var missing = members.Find(m => !m.Device.IsConnected);
                if (missing != null)
                {
                    MessageBox.Show(
                        string.Format(L10n.T("job_dlg.group_missing"),
                            missing.Device.ToString()),
                        L10n.T("job_dlg.group_cap"),
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                    return;
                }

                var template = BuildParameters();
                template.Device    = null;
                template.Drive     = null;
                template.ImageFile = null;

                var groupActions = new List<PostAction>();
                foreach (ListViewItem item in lvPostActions.Items)
                    groupActions.Add((PostAction)item.Tag!);

                GroupResult = new GroupRepetitiveJob
                {
                    JobType           = cmbJobType.SelectedIndex == 0 ? JobType.Read : JobType.Write,
                    ParameterTemplate = template,
                    PostActions       = groupActions,
                    FilePattern       = txtFilePattern!.Text,
                    OutputFolder      = txtOutputFolder?.Text ?? "",
                    DateTimeFormat    = txtDtFormat?.Text ?? "yyyyMMdd_HHmmss",
                    NextDiskNumber    = (int)(nudStartIndex?.Value ?? 1),
                    Members           = members
                };
                return;   // DialogResult stays OK; Result stays null
            }
```

- [ ] **Step 4: Preset round-trip**

In `BuildPreset()`, before `return preset;`:

```csharp
            preset.UseDeviceGroup = chkUseGroup?.Checked ?? false;
            preset.GroupMembers.Clear();
            if (lvGroupMembers != null)
                foreach (ListViewItem item in lvGroupMembers.Items)
                    if (item.Tag is GreaseWeazleDevice dev)
                        preset.GroupMembers.Add(new GroupMemberPreset
                        {
                            DeviceId   = dev.Id,
                            DeviceName = dev.ToString(),
                            Drive      = item.SubItems[1].Text
                        });
```

In `LoadFromPreset()`, after the `chkRepetitive.Checked = preset.RepetitiveMode;` block (before `_initialized = true;`):

```csharp
            lvGroupMembers.Items.Clear();
            foreach (var gm in preset.GroupMembers)
            {
                var dev = _devices.Find(d => d.Id == gm.DeviceId)
                       ?? _devices.Find(d => d.ToString() == gm.DeviceName);
                var item = new ListViewItem(dev?.ToString() ?? gm.DeviceName + " ⚠");
                item.SubItems.Add(gm.Drive);
                item.Tag = dev;               // null when the device is absent
                if (dev == null) item.ForeColor = Color.FromArgb(220, 120, 80);
                lvGroupMembers.Items.Add(item);
            }
            chkUseGroup.Checked = preset.UseDeviceGroup;
```

(Rows whose `Tag` is null are dropped by `ReadGroupMembers`, so a preset with absent devices fails the `group_min`/validation checks until the user fixes the rows — matching the spec.)

- [ ] **Step 5: Build and test**

Run: `dotnet build T8BBGWCopyPro.sln` then `dotnet test GreaseWeazleManager.Tests/GreaseWeazleManager.Tests.csproj`
Expected: build succeeds, tests pass.

- [ ] **Step 6: Commit**

```bash
git add GreaseWeazleManager/Services/Localizer.cs GreaseWeazleManager/Forms/NewJobDialog.cs
git commit -m "feat: device-group configuration in new job dialog"
```

---

### Task 12: MainForm — start and wire group jobs

**Files:**
- Modify: `GreaseWeazleManager/Services/Localizer.cs`
- Modify: `GreaseWeazleManager/Forms/MainForm.cs`

**Interfaces:**
- Consumes: `NewJobDialog.GroupResult` (Task 11), `GroupJobService` + events (Task 9), `BatchInsertDialog` (Task 10), `DriveProber` (Task 6), `JobPanel` (existing).

- [ ] **Step 1: Add localization keys**

`_en`, after the `status.jobs_count` key:

```csharp
            ["status.batch_running"]   = "Batch {0} running on {1} drive(s)…",
            ["status.group_done"]      = "✓ Group job finished — {0} disk(s) imaged.",
```

`_de`, after the German `status.jobs_count` key:

```csharp
            ["status.batch_running"]   = "Stapel {0} läuft auf {1} Laufwerk(en)…",
            ["status.group_done"]      = "✓ Gruppenauftrag beendet — {0} Diskette(n) abgebildet.",
```

- [ ] **Step 2: Route group results from the dialog**

In `OpenNewJobDialog`, replace the `if (dlg.ShowDialog(this) == ...)` block with:

```csharp
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                if (dlg.GroupResult != null) StartGroupJob(dlg.GroupResult);
                else if (dlg.Result != null) StartJob(dlg.Result);
            }
```

- [ ] **Step 3: Implement StartGroupJob**

Add after `StartJob`:

```csharp
        /// <summary>
        /// Starts a group repetitive job: wires a <see cref="GroupJobService"/>, creates a
        /// job panel per member, shows the <see cref="BatchInsertDialog"/> before each
        /// batch, and reports completion in the status bar.
        /// </summary>
        /// <param name="group">The group job to run.</param>
        private void StartGroupJob(GroupRepetitiveJob group)
        {
            var cts = new CancellationTokenSource();
            _cts.Add(cts);

            var service = new GroupJobService(_gwService);
            var prober  = new DriveProber(_gwService.GwExePath);

            service.MemberJobsCreated += (s, e) => SafeInvoke(() =>
            {
                foreach (var m in e.Group.Members)
                {
                    var job = m.Job!;
                    _jobs.Add(job);
                    var member = m;
                    var panel = new JobPanel(job,
                        cancelJob => member.BatchCts?.Cancel(),
                        logJob =>
                        {
                            if (Directory.Exists(logJob.LogFolder))
                                System.Diagnostics.Process.Start("explorer.exe", logJob.LogFolder);
                            else
                                MessageBox.Show(
                                    L10n.T("job.log_unavailable"),
                                    L10n.T("job.log_caption"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                        },
                        restartJob => { });
                    _jobPanels[job.Id] = panel;
                    _jobsFlow.Controls.Add(panel);
                }
                UpdateJobCount();
            });

            service.BatchPromptRequested += (s, e) => SafeInvoke(() =>
            {
                using var dlg = new BatchInsertDialog(e.Group, prober);
                dlg.ShowDialog(this);
                if (dlg.StartBatchChosen)
                    SetStatus(string.Format(L10n.T("status.batch_running"),
                            e.Group.BatchNumber + 1,
                            e.Group.Members.Count(m => m.IncludedThisBatch && m.Verified)),
                        Color.FromArgb(100, 200, 255));
                e.Signal(dlg.StartBatchChosen);
            });

            service.GroupCompleted += (s, e) => SafeInvoke(() =>
            {
                foreach (var m in e.Group.Members)
                    if (m.Job != null && _jobPanels.TryGetValue(m.Job.Id, out var p))
                        p.UpdateFromJob();
                SetStatus(string.Format(L10n.T("status.group_done"),
                        e.Group.Members.Sum(m => m.Job?.DisksCompleted ?? 0)),
                    Color.FromArgb(80, 220, 100));
                SoundService.PlaySuccess();
                UpdateJobCount();
            });

            Task.Run(async () =>
            {
                try { await service.RunAsync(group, cts.Token); }
                catch (Exception ex)
                {
                    SafeInvoke(() => SetStatus(
                        string.Format(L10n.T("status.exception"), ex.Message),
                        Color.FromArgb(240, 80, 80)));
                }
            });
        }
```

**Threading note:** `BatchPromptRequested` fires from a background task; `SafeInvoke` marshals the dialog onto the UI thread, and `Signal` resumes the service loop — the same pattern the existing `DiskCompleted` handler uses. The `GroupCompleted` disk count sums `DisksCompleted` over the member jobs, which `GroupJobService` increments only on successful disks.

- [ ] **Step 4: Build and test**

Run: `dotnet build T8BBGWCopyPro.sln` then `dotnet test GreaseWeazleManager.Tests/GreaseWeazleManager.Tests.csproj`
Expected: build succeeds, tests pass.

- [ ] **Step 5: Commit**

```bash
git add GreaseWeazleManager/Services/Localizer.cs GreaseWeazleManager/Forms/MainForm.cs
git commit -m "feat: wire group repetitive jobs into the main window"
```

---

### Task 13: Device tile Blink button

**Files:**
- Modify: `GreaseWeazleManager/Services/Localizer.cs`
- Modify: `GreaseWeazleManager/Controls/DevicePanel.cs`
- Modify: `GreaseWeazleManager/Forms/MainForm.cs` (`RefreshDeviceBar` + new method)

**Interfaces:**
- Consumes: `IDriveProber.BlinkOnceAsync` (Task 6).
- Produces: `DevicePanel` constructor gains a fourth callback `Action<GreaseWeazleDevice> blinkCallback`; the panel exposes `void SetBlinkBusy(bool busy)` to disable its button during a sequence.

- [ ] **Step 1: Add localization keys**

`_en`, after `["dev.new_job"]`:

```csharp
            ["dev.blink"]              = "⚡ Blink",
            ["status.blinking_dev"]    = "Blinking {0} ({1})…",
            ["status.blink_done"]      = "Blink sequence finished on {0}.",
            ["status.blink_error"]     = "Blink failed on {0}: device busy or unplugged.",
```

`_de`, after the German `["dev.new_job"]`:

```csharp
            ["dev.blink"]              = "⚡ Blinken",
            ["status.blinking_dev"]    = "Blinke {0} ({1})…",
            ["status.blink_done"]      = "Blinksequenz auf {0} beendet.",
            ["status.blink_error"]     = "Blinken auf {0} fehlgeschlagen: Gerät belegt oder getrennt.",
```

- [ ] **Step 2: Add the button to DevicePanel**

In `DevicePanel.cs`:

Add fields next to `_btnRemove`:

```csharp
        private readonly Button               _btnBlink;
        private Action<GreaseWeazleDevice>?   _blinkCallback;
```

Extend the constructor signature:

```csharp
        public DevicePanel(
            GreaseWeazleDevice       device,
            Action<GreaseWeazleDevice> removeCallback,
            Action<GreaseWeazleDevice> newJobCallback,
            Action<GreaseWeazleDevice> blinkCallback)
```

and assign `_blinkCallback = blinkCallback;` next to the other callbacks.

After the `_btnRemove` construction block:

```csharp
            _btnBlink = new Button
            {
                Text      = L10n.T("dev.blink"),
                Location  = new Point(76, 110),
                Size      = new Size(124, 18),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 35, 20),
                ForeColor = Color.FromArgb(220, 180, 80),
                Font      = new Font("Consolas", 7.5f),
                Enabled   = device.IsConnected
            };
            _btnBlink.FlatAppearance.BorderColor = Color.FromArgb(120, 100, 40);
            _btnBlink.Click += (s, e) => _blinkCallback?.Invoke(_device);
```

Add `_btnBlink` to the `Controls.AddRange` array.

Add a public method after the constructor:

```csharp
        /// <summary>Disables the Blink button while an identify sequence runs on this device.</summary>
        public void SetBlinkBusy(bool busy) => _btnBlink.Enabled = !busy && _device.IsConnected;
```

- [ ] **Step 3: Wire the identify sequence in MainForm**

In `RefreshDeviceBar()`, extend the `DevicePanel` construction:

```csharp
                var dp = new DevicePanel(
                    dev,
                    d => { _devices.Remove(d); RefreshDeviceBar(); },
                    d => OpenNewJobDialog(preselectedDevice: d),
                    d => BlinkIdentify(d));
```

Add a field near `_gwService`:

```csharp
        private bool _blinkInProgress;
```

Add the method after `RefreshDeviceBar()`:

```csharp
        /// <summary>
        /// Runs a short identify sequence on the device: three blink pulses alternating
        /// drive 0 and drive 1, covering both unit-select lines so any attached drive
        /// lights regardless of its 0/1/a/b addressing. Only one sequence runs at a time.
        /// </summary>
        /// <param name="dev">The device whose drive should blink.</param>
        private void BlinkIdentify(GreaseWeazleDevice dev)
        {
            if (_blinkInProgress) return;
            _blinkInProgress = true;
            SetPanelsBlinkBusy(true);
            SetStatus(string.Format(L10n.T("status.blinking_dev"), dev.Name, dev.SerialPort),
                Color.FromArgb(220, 180, 80));

            var prober = new DriveProber(_gwService.GwExePath);
            Task.Run(async () =>
            {
                bool ok = true;
                try
                {
                    for (int i = 0; i < 3 && ok; i++)
                    {
                        await prober.BlinkOnceAsync(dev.SerialPort, "0", CancellationToken.None);
                        await Task.Delay(350);
                        await prober.BlinkOnceAsync(dev.SerialPort, "1", CancellationToken.None);
                        await Task.Delay(350);
                    }
                }
                catch { ok = false; }

                SafeInvoke(() =>
                {
                    _blinkInProgress = false;
                    SetPanelsBlinkBusy(false);
                    SetStatus(
                        ok ? string.Format(L10n.T("status.blink_done"), dev.Name)
                           : string.Format(L10n.T("status.blink_error"), dev.Name),
                        ok ? Color.FromArgb(80, 220, 120) : Color.FromArgb(240, 80, 80));
                });
            });
        }

        /// <summary>Toggles the Blink button on every device tile.</summary>
        private void SetPanelsBlinkBusy(bool busy)
        {
            foreach (Control c in _deviceBar.Controls)
                if (c is DevicePanel dp) dp.SetBlinkBusy(busy);
        }
```

(`BlinkOnceAsync` swallows process errors internally and returns normally; a dead device simply produces no LED activity, and the watchdog timeout keeps the sequence short. The `ok` flag catches unexpected exceptions.)

- [ ] **Step 4: Build and test**

Run: `dotnet build T8BBGWCopyPro.sln` then `dotnet test GreaseWeazleManager.Tests/GreaseWeazleManager.Tests.csproj`
Expected: build succeeds, tests pass.

- [ ] **Step 5: Commit**

```bash
git add GreaseWeazleManager/Services/Localizer.cs GreaseWeazleManager/Controls/DevicePanel.cs GreaseWeazleManager/Forms/MainForm.cs
git commit -m "feat: blink-identify button on device tiles"
```

---

### Task 14: Final verification

**Files:** none new.

- [ ] **Step 1: Full build and test run**

```bash
dotnet build T8BBGWCopyPro.sln -c Release
dotnet test GreaseWeazleManager.Tests/GreaseWeazleManager.Tests.csproj
```

Expected: Release build succeeds; all tests pass.

- [ ] **Step 2: Localization completeness check**

Run a search for every new key (`settings.ok`, `nextdisk.device`, `nextdisk.drive_auto`, `batch.`, `job_dlg.use_group`, `job_dlg.group_`, `dev.blink`, `status.blink`, `status.batch_running`, `status.group_done`) and confirm each appears exactly twice in `Localizer.cs` (once in `_en`, once in `_de`).

- [ ] **Step 3: Manual smoke checklist (requires hardware; report to user for anything untestable without devices)**

1. Settings → change language → Save → Cancel button now reads "OK"; click it — dialog closes.
2. Single-drive repetitive job → complete one disk → NextDiskDialog shows "Device: … — drive …".
3. Device tile → ⚡ Blink → the attached drive's LED pulses ~6 times; status bar reports start/finish.
4. New Job → Repeat tab → enable "Use device group", add 2+ devices, Start → BatchInsertDialog appears, first drive blinks; "Disk inserted" without a disk keeps it blinking with a hint; with a disk it advances; excluding a row advances; when all included rows are detected, "Start batch" enables; batch runs in parallel with one panel per drive; next batch dialog shows last-batch results; "Finish job" ends the job with all panels Completed.
5. Save a group preset, reload it, confirm members and drives round-trip.

- [ ] **Step 4: Update the spec status line and commit**

Change `Status: Approved by user` to `Status: Implemented` in the spec, then:

```bash
git add docs/superpowers/specs/2026-08-03-group-repetitive-jobs-design.md
git commit -m "docs: mark group repetitive jobs spec implemented"
```
