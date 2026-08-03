# Design: Group Repetitive Jobs, Device Info in Next-Disk Dialog, Settings OK Button

Date: 2026-08-03
Status: Implemented

## Scope

Three changes to GWCopyPro (WinForms .NET 8 front-end for gw.exe):

1. The next-disk dialog shown between disks in repetitive jobs indicates which
   Grease Weazle device and drive is in use.
2. Repetitive jobs can run against a **device group**: several Grease Weazle
   devices (each with a drive address 0/1/a/b) imaging one batch of disks in
   parallel, with an LED-guided insert sequence and per-batch drive exclusion.
3. In the Settings dialog, after saving, the Cancel button reads a localized
   "OK" instead of "Cancel".
4. Each device tile in the main window's device strip gets a **Blink** button
   that pulses the attached drive's LED, so the user can see which physical
   drive belongs to which COM port.

## Decisions made during brainstorming

- **Blink + confirm + verify**: drives signal "insert a disk here" by blinking
  their LED. The user confirms insertion with a button; the app then verifies a
  disk is physically present. A drive keeps blinking after confirmation if no
  disk is detected, unless the user explicitly excludes it.
- **Parallel batches**: all drives in the group image simultaneously.
- **Group definition in the New Job dialog**, persisted with job presets. No
  separate group-manager UI.
- **Exclusion is per batch and reversible**: an excluded drive can be
  re-included in a later batch.
- **Batch errors**: if one drive fails, the others finish their disks; the
  failure is reported in the next batch dialog; its disk number is not reused;
  the job continues.

## 1. Data model

- `GwJob` remains single-device; its structure is unchanged.
- New `DeviceGroupMember` (Models):
  - `GreaseWeazleDevice Device`
  - `string Drive` (`"0" | "1" | "a" | "b"`)
  - Runtime state: `bool IncludedThisBatch`, `bool Verified`,
    `bool LastBatchFailed`, `string? LastBatchError`, `string? LastBatchFile`,
    `GwJob? Job` (the member's job for the current batch)
- New `GroupRepetitiveJob` (Models):
  - Shared job settings: `GwParameters` template (format, tracks, revs,
    retries, etc. — everything except `Device`, `Drive`, `ImageFile`),
    `JobType`, `FilePattern`, `OutputFolder`, `DateTimeFormat`,
    `List<PostAction>`
  - `List<DeviceGroupMember> Members` — list order is the blink order
  - `int NextDiskNumber` (1-based, monotonically increasing, never reused)
  - `int BatchNumber` (1-based)
- `JobPreset` gains:
  - `bool UseDeviceGroup`
  - `List<GroupMemberPreset>` where `GroupMemberPreset` = device `Id`, device
    `Name` (for display when the device is absent), and `Drive`
- **Validation**: a group must contain at least 2 members, and may not contain
  the same Grease Weazle device (same device `Id`) twice — parallel imaging
  requires one drive per device, because gw.exe holds the COM port exclusively
  and drive select on one Weazle is exclusive.

## 2. New Job dialog

On the Repetitive tab, below the existing repetitive checkbox:

- Checkbox **"Use device group"**, enabled only while repetitive mode is
  checked.
- When checked, a member grid appears: rows of (device dropdown, drive
  dropdown 0/1/a/b), plus Add row / Remove row buttons. Row order = blink
  order. The single-device and single-drive selectors elsewhere in the dialog
  are ignored for group jobs (visually disabled while the group box is
  checked).
- Validation on Start: at least 2 members, no duplicate devices, all selected
  devices currently connected.
- The standard repetitive flow is unchanged when the checkbox is off.
- Presets round-trip the group (save and load). A preset whose device ids are
  no longer present loads with those rows marked missing; the user must fix
  them before starting.

## 3. DriveProber service (blink + verify)

New `Services/DriveProber.cs`, an instantiable class behind an interface
`IDriveProber` so the batch state machine can be unit-tested without hardware:

- `Task BlinkOnceAsync(device, drive, ct)` — runs
  `gw seek --device COMx --drive N 0`. Selecting the drive lights its LED for
  the duration of the command; called on a ~1.5 s cycle by the insert-phase
  controller, producing a visible blink. Uses the configured gw.exe path.
- `Task<bool> ProbeDiskAsync(device, drive, ct)` — runs
  `gw rpm --device COMx --drive N`. Returns true when gw.exe reports an RPM
  value (disk present, index pulses seen), false on failure/timeout
  (~5 s) — no disk, or lever open on 5.25" drives.
- Any process-level failure (device unplugged, port busy) surfaces as a
  distinct "device not responding" result so the UI can show it.
- Only one gw.exe call runs at a time during the insert phase (only the
  currently blinking drive is probed).

## 4. Insert phase and BatchInsertDialog

New `Forms/BatchInsertDialog.cs`, shown at job start and again after every
completed batch. It replaces `NextDiskDialog` for group jobs only.

Contents:

- Header: batch number; preview of the file names the included drives will
  produce (expanded via the existing `FilePattern.Expand`).
- One row per member, in group order:
  - Device name, COM port, drive (e.g. `GreaseWeazle 1 (COM3) — drive 0`)
  - Status: `Waiting` / `Blinking…` / `Disk detected` / `Excluded` /
    `Device not responding`, plus last batch's result where applicable
    (`OK — <filename>` or `FAILED — <error>`)
  - Include checkbox, togglable every batch (re-inclusion allowed)
- Buttons: **Disk inserted** (confirms the currently blinking drive),
  **Start batch** (enabled only when every included drive is `Disk detected`),
  **Finish job** (ends the group job; also the dialog's close/cancel action).

Behavior:

- Drives blink strictly one at a time, in row order, skipping excluded rows.
- **Disk inserted** triggers `ProbeDiskAsync` on the blinking drive. Success:
  the row becomes `Disk detected`, blinking advances to the next included,
  unverified row. Failure: the row keeps blinking and shows a
  "no disk detected" hint.
- Excluding the blinking drive advances the blink immediately. Re-including a
  drive appends it to the blink queue. Toggling include clears that row's
  `Verified` state only when switching to excluded.
- If every row is excluded, **Start batch** is disabled; **Finish job** ends
  the job.
- The insert-phase sequencing (queue, statuses, verification transitions) is
  implemented as a plain state-machine class (`BatchInsertController`) that
  the dialog binds to, with `IDriveProber` injected.

## 5. GroupJobService (batch execution)

New `Services/GroupJobService.cs` orchestrating the whole group job:

1. Loop: show BatchInsertDialog (via an event to MainForm, mirroring the
   existing `DiskCompleted` event pattern with a `TaskCompletionSource`).
2. On Start batch: for each included, verified member in group order:
   - assign disk number `NextDiskNumber++`
   - expand the file name and resolve the output folder using the same rules
     as `RunRepetitiveAsync` today
   - create/reset the member's `GwJob` (clone of the parameter template +
     member's device/drive + file), register it so MainForm creates a job
     panel for it
   - run it via the existing `GwService.RunSingleDiskAsync`
3. `Task.WhenAll` over the member runs. Per-member failure sets
   `LastBatchFailed`/`LastBatchError` on the member; successes record
   `LastBatchFile`. Failed disk numbers are not reused.
4. Post-actions run per member disk exactly as they do for single jobs today
   (inside `RunSingleDiskAsync`'s flow).
5. Repeat from step 1 until Finish job, cancellation, or all members excluded.

Cancellation uses one `CancellationTokenSource` for the group, linked into
every member run; the existing process-kill logic applies. `RunSingleDiskAsync`
changes from `private` to `internal` (or is exposed via a small public
wrapper) — its body is untouched.

MainForm: member jobs appear in `_jobPanels` keyed by job id like any other
job, giving each drive its own live track grid. The group itself is
represented by one summary line in the status bar (batch number, disks done)
— no new panel type.

## 6. NextDiskDialog device info (standard repetitive jobs)

- Constructor gains device display info (device name, COM port, drive).
- A new line under the title area: localized
  `nextdisk.device` = `Device: {0} — drive {1}` (drive falls back to
  `(auto)` when unset).
- `MainForm.WireEvents` passes `e.Job.Device` and `e.Job.Parameters.Drive`.
- Group jobs never show this dialog; the BatchInsertDialog rows carry the
  per-device info instead.

## 7. Settings dialog OK button

- Keep a field reference to the Cancel button.
- In `BtnSave_Click`, after a successful save, set its text to the new
  localized key `settings.ok` (using the just-applied language). Its
  `DialogResult` remains `Cancel`; closing behavior is unchanged.
- If the user edits values again after saving, the button text stays "OK"
  (the last save persisted; the button only closes the dialog).

## 7a. Device tile Blink button (identify drive)

- `DevicePanel` gains a **Blink** button next to the Remove button (localized
  key `dev.blink`), enabled only while the device is connected.
- Clicking it invokes a callback provided by `MainForm` (same pattern as the
  existing remove/new-job callbacks). `MainForm` runs the identify sequence
  through `IDriveProber` on a background task: three blink pulses alternating
  drive `0` and drive `1` (`gw seek --device COMx --drive N 0`, ~1.5 s cycle).
  Pulsing both unit-select lines lights any single attached drive regardless
  of whether it is addressed as 0/1 or a/b, so no drive selector is needed.
- While the sequence runs, the tile's Blink button is disabled; the status bar
  shows "Blinking <device> (<port>)…". If gw.exe fails (device busy because a
  job is running, or unplugged), the status bar shows a localized error and
  the button re-enables. Only one identify sequence runs at a time.

## 8. Error handling summary

- Blink/verify process errors → `Device not responding` row state; the row
  stays unverified; the user can exclude it and continue with the rest.
- Mid-batch failure of one drive never interrupts the others.
- All-excluded or Finish job → group job completes with the usual completed
  status and sound.
- Group job cancellation kills all member gw.exe processes.

## 9. Localization

All new strings added to `Services/Localizer.cs` in both English and German:
`settings.ok`, `nextdisk.device`, `job_dlg.use_group`, `job_dlg.group_add`,
`job_dlg.group_remove`, and the `batch.*` family for the BatchInsertDialog
(title, statuses, buttons, hints, file preview, batch number).

## 10. Testing

- New test project `GreaseWeazleManager.Tests` added to the solution
  (xUnit, net8.0).
- Unit-tested pure logic: `BatchInsertController` state machine (blink queue
  order, exclusion/re-inclusion, verification transitions, start-batch
  eligibility), disk-number assignment across batches with failures and
  exclusions, group validation rules, `GroupMemberPreset` round-trip.
- `IDriveProber` is faked in tests; no hardware or gw.exe needed.
- Hardware-facing code (`DriveProber`, process launching) is kept thin and
  excluded from unit-test scope.
