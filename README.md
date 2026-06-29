# GreaseWeazle Manager

A Windows Forms application for managing multiple [GreaseWeazle](https://github.com/keirf/greaseweazle) devices and disk-image operations with a dark, industrial UI.

## What is GreaseWeazle Manager?

[GreaseWeazle](https://github.com/keirf/greaseweazle) is an open-source USB floppy-controller that reads and writes raw magnetic flux from virtually any floppy disk format. The official tool (`gw.exe`) is a command-line utility.

**GreaseWeazle Manager** wraps `gw.exe` in a graphical front-end that lets you:

- Manage **multiple GreaseWeazle devices** connected simultaneously via separate COM ports
- Create, queue, and monitor **read and write jobs** with a live track-by-track visualiser
- Run **repetitive imaging sessions** — insert a disk, image it, swap, repeat — with automatic file naming
- Attach **post-processing actions** (executables, batch scripts, PowerShell scripts) that run automatically after each successful job
- Save and reload **job presets** so common configurations (formats, track ranges, flags) are a single click away

## Requirements

- Windows 10 / 11
- [.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (or SDK to build)
- `gw.exe` v0.24+ from the [GreaseWeazle firmware/tools package](https://github.com/keirf/greaseweazle)

## Build

```
dotnet build GreaseWeazleManager/GreaseWeazleManager.csproj -c Release
```

Or open `GreaseWeazleManager.sln` in Visual Studio 2022+ and press **F5**.

## Getting Started

1. Connect one or more GreaseWeazle devices via USB.
2. Launch **GreaseWeazle Manager**.
3. Open **Device Manager** (toolbar button) and add each device — select its COM port and give it a friendly name.
4. Click **New Job**, configure the read or write parameters, and press **Start**.
5. Watch the track grid update in real time as `gw.exe` processes each cylinder/head.

## Features

### Multi-Device Management

- Add unlimited GreaseWeazle devices, each with its own COM port and display name.
- Pulsing LED indicator per device reflects live connection state (green = connected, red = disconnected).
- **Device Manager** dialog lets you add, remove, and refresh COM-port assignments at runtime without restarting the application.

### Multi-Threaded Job Execution

- Every read/write job runs in its own background thread (`Task.Run` + `CancellationTokenSource`).
- Multiple jobs can run simultaneously on different devices — there is no artificial limit.
- Each job gets its own isolated log folder: `Logs/Job_<Type>_<ID>_<DateTime>/`.
- Jobs can be cancelled individually; closing the application cancels all running jobs gracefully.

### Full gw.exe Parameter Support (v0.24+)

The New Job dialog exposes every relevant `gw.exe` flag via dedicated controls. Track selection uses the `--tracks=` compound syntax introduced in v0.24.

| Parameter | UI Control |
|---|---|
| `--device` | Device selector (COM port) |
| `--drive` | Drive selector (a / b / 0 / 1 / 2 / 3) |
| `--format` | Format text box + quick-select combo |
| `--tracks=c=…:h=…` | Start/End Cylinder, Head selector, Step spinner |
| `--tracks=…:hswap` | HSwap checkbox |
| `--tracks=…:h0.off=N` | Head-0 offset (flippy drives) |
| `--tracks=…:h1.off=N` | Head-1 offset (flippy drives) |
| `--revs` | Revolutions spinner |
| `--densel` | Density-select combo (hd / dd / ed) |
| `--bitrate` | Bitrate spinner (0 = auto) |
| `--retries` | Retries checkbox + count spinner |
| `--no-clobber` | Skip tracks already present in the image |
| `--raw` | Write raw flux, bypassing format codec |
| `--reverse` | Reverse track data (e.g. Side B of flippy disks) |
| `--hard-sectors` | Hard-sectored disk support |
| `--erase` | Erase disk before writing |
| `--verify` | Verify disk after write |
| `--precomp` | Write precompensation (µs) |
| `--gen-tg43` | Generate /TG43 signal for 8″ drives |
| Extra args | Free-text field appended verbatim |

A **live command preview** at the bottom of the dialog shows the exact `gw.exe` invocation that will be run as you adjust controls.

### Disk Visualiser

Each job panel renders a per-track status grid:

- **Side 0 (Head 0 – Upper)** — 84-cell horizontal bar
- **Side 1 (Head 1 – Lower)** — 84-cell horizontal bar

| Cell colour | Meaning |
|---|---|
| Dark grey | Unknown / not yet started |
| Mid grey | Pending |
| Blue | Actively reading / writing |
| Green | Good |
| Red | Error |

Cells update in real time by parsing `gw.exe` output lines (e.g. `T00.0: ok`, `Cyl 0, Head 0: reading`) and a progress-fraction fallback for versions that emit different output.

### Job Presets

- Save any job configuration as a named preset (JSON stored in `%APPDATA%\GreaseWeazleManager\Presets\`).
- Reload a preset to recreate the exact same job — device, format, track range, flags, post-actions, and file pattern all included.
- The **Restart** button on a completed or failed job re-creates it from its saved preset snapshot.

### Repetitive Mode

Designed for bulk-imaging runs (e.g. digitising a box of floppies):

- Enable **Repetitive Mode** in the New Job dialog.
- Set a **file pattern** with optional tokens: `{n}`, `{n:D3}` (zero-padded counter), `{dt}` (date/time stamp).
- Choose an **output folder** and a **start index**.
- After each disk completes, the app prompts you to insert the next disk and continues automatically, incrementing the counter.
- A pattern preview in the dialog shows how the file names will look (e.g. `Disk_001_20260101_120000.scp`).

### Post-Actions (Sequential)

After a successful job, run any number of actions in sequence:

| Type | How it runs |
|---|---|
| **Executable** | Called directly with your arguments |
| **Batch Script** | Launched via `cmd.exe /c` |
| **PowerShell Script** | Launched via `powershell.exe -File` |

Available tokens in arguments:

| Token | Expands to |
|---|---|
| `{ImageFile}` | Full path to the disk image |
| `{LogFolder}` | Full path to the job's log folder |
| `{JobId}` | Unique job identifier |

Actions can be reordered (▲ ▼), individually enabled or disabled, and edited inline. Use cases include checksum verification, automatic archiving, upload scripts, or format conversion.

### Logging

- `gw.exe` stdout and stderr are captured live and written to `Logs/Job_<Type>_<ID>_<timestamp>/gw_output.log`.
- Post-action output is appended to the same log file.
- The **View Log** button on each job panel opens the log folder in Windows Explorer.

### Audio & Visual Feedback

| Event | Sound | Visual |
|---|---|---|
| Job started | Two ascending beeps | Status bar update |
| Job completed | Three ascending beeps | Green border glow on job panel |
| Job error | Three descending beeps | Red border + form background flash |
| Track error | — | Red cell in disk visualiser |

### Settings

- **gw.exe path** — point to your local installation; defaults to `gw.exe` on `PATH`.
- **Language** — English or German UI.
- Settings are stored as JSON in `%APPDATA%\GreaseWeazleManager\settings.json`.

## Log Folder Structure

```
Logs/
  Job_Read_a1b2c3d4_20260601_143022/
    gw_output.log
  Job_Write_e5f6g7h8_20260601_143155/
    gw_output.log
```

## Architecture

The project is a single .NET 8 Windows Forms application (`net8.0-windows`) with three layers:

| Layer | Namespace | Responsibility |
|---|---|---|
| **Models** | `GreaseWeazleManager.Models` | `GwParameters`, `GwJob`, `JobPreset`, `PostAction`, track cells, enums, file-pattern expander |
| **Services** | `GreaseWeazleManager.Services` | `GwService` (process management, output parsing, event dispatch), `GwDetector` (COM-port detection), `AppSettings`, `SoundService` |
| **UI** | `GreaseWeazleManager.Forms` / `.Controls` | Main form, `NewJobDialog`, `DeviceManagerDialog`, `SettingsDialog`, `NextDiskDialog`, `JobPanel`, `DevicePanel`, `FloppyDiskControl` |

### Dependencies

| Package | Purpose |
|---|---|
| [NAudio](https://github.com/naudio/NAudio) | Cross-thread PC-speaker / WinMM beep generation |
| [Foundation](https://github.com/NickeManarin/Foundation) | — |
| [unar / lsar](https://theunarchiver.com/command-line) | Bundled extraction tools (optional post-action use) |

## Notes

- The app targets `gw.exe` v0.24+ and generates `--tracks=` compound selectors. The old `--scyl` / `--ecyl` / `--shead` / `--ehead` / `--single-sided` flags (removed in v0.24) are not emitted.
- If your version of `gw.exe` produces different output, the progress-fraction parser (`n/m` lines) keeps the job advancing; only the per-cell colour coding requires track-specific log lines.
- All state (presets, settings, logs) lives under `%APPDATA%\GreaseWeazleManager\` and the `Logs\` folder beside the executable.

## License

MIT — see [LICENSE](LICENSE) for details.
