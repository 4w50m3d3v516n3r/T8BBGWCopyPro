[English](README.md) | [Deutsch](README.de.md)

![ScreenShot](/images/gw-copy-pro-banner.png)



![HIER RUNTERLADEN](https://github.com/4w50m3d3v516n3r/T8BBGWCopyPro/releases/tag/v1.0.0))

# GWCopyPro

Eine Windows-Forms-Anwendung zur Verwaltung mehrerer [GreaseWeazle](https://github.com/keirf/greaseweazle)-Geräte und Disk-Image-Operationen mit einer dunklen, industriellen Benutzeroberfläche.

## Was ist der GWCopyPro?

[GreaseWeazle](https://github.com/keirf/greaseweazle) ist ein Open-Source-USB-Floppy-Controller, der rohen magnetischen Fluss von praktisch jedem Diskettenformat lesen und schreiben kann. Das offizielle Tool (`gw.exe`) ist ein Kommandozeilenprogramm.

Der **GWCopyPro** stellt ein grafisches Frontend für `gw.exe` bereit, mit dem Sie:

* **mehrere GreaseWeazle-Geräte** gleichzeitig über separate COM-Ports verwalten
* **Lese- und Schreibjobs** erstellen, in die Warteschlange stellen und mit einer Live-Track-Visualisierung überwachen
* **repetitive Imaging-Sitzungen** durchführen — Diskette einlegen, imagen, wechseln, wiederholen — mit automatischer Dateibenennung
* **Post-Processing-Aktionen** (Programme, Batch-Skripte, PowerShell-Skripte) anhängen, die nach jedem erfolgreichen Job automatisch ausgeführt werden
* **Job-Presets** speichern und laden, sodass häufige Konfigurationen (Formate, Track-Bereiche, Flags) nur einen Klick entfernt sind

## Voraussetzungen

* Windows 10 / 11
* [.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (bzw. das SDK zum Kompilieren)
* `gw.exe` v0.24+ aus dem [GreaseWeazle-Firmware-/Tools-Paket](https://github.com/keirf/greaseweazle)

## Kompilieren

```
dotnet build GWCopyPro/GWCopyPro.csproj -c Release
```

Oder `GWCopyPro.sln` in Visual Studio 2022+ öffnen und **F5** drücken.

## Erste Schritte

1. Ein oder mehrere GreaseWeazle-Geräte per USB anschließen.
2. **GWCopyPro** starten.
3. Den **Geräte-Manager** öffnen (Toolbar-Schaltfläche) und jedes Gerät hinzufügen — COM-Port auswählen und einen sprechenden Namen vergeben.
4. Auf **Neuer Job** klicken, die Lese- oder Schreibparameter konfigurieren und **Start** drücken.
5. Das Track-Raster aktualisiert sich in Echtzeit, während `gw.exe` jeden Zylinder/Kopf verarbeitet.

## Funktionen

### Verwaltung mehrerer Geräte

* Beliebig viele GreaseWeazle-Geräte hinzufügen, jedes mit eigenem COM-Port und Anzeigenamen.
* Pulsierende LED-Anzeige pro Gerät zeigt den aktuellen Verbindungsstatus (grün = verbunden, rot = getrennt).
* Der **Geräte-Manager**-Dialog erlaubt das Hinzufügen, Entfernen und Aktualisieren von COM-Port-Zuweisungen zur Laufzeit — ohne Neustart der Anwendung.

### Multithread-Job-Ausführung

* Jeder Lese-/Schreibjob läuft in einem eigenen Hintergrund-Thread (`Task.Run` + `CancellationTokenSource`).
* Mehrere Jobs können gleichzeitig auf verschiedenen Geräten laufen — es gibt keine künstliche Begrenzung.
* Jeder Job erhält einen eigenen, isolierten Log-Ordner: `Logs/Job\_<Typ>\_<ID>\_<DatumZeit>/`.
* Jobs können einzeln abgebrochen werden; beim Schließen der Anwendung werden alle laufenden Jobs sauber beendet.

### Vollständige Unterstützung der gw.exe-Parameter (v0.24+)

Der Dialog „Neuer Job" bietet für jedes relevante `gw.exe`-Flag ein eigenes Bedienelement. Die Track-Auswahl verwendet die mit v0.24 eingeführte `--tracks=`-Verbundsyntax.

|Parameter|UI-Element|
|-|-|
|`--device`|Geräteauswahl (COM-Port)|
|`--drive`|Laufwerksauswahl (a / b / 0 / 1 / 2 / 3)|
|`--format`|Format-Textfeld + Schnellauswahl-Combo|
|`--tracks=c=…:h=…`|Start-/End-Zylinder, Kopfauswahl, Step-Spinner|
|`--tracks=…:hswap`|HSwap-Checkbox|
|`--tracks=…:h0.off=N`|Kopf-0-Offset (Flippy-Laufwerke)|
|`--tracks=…:h1.off=N`|Kopf-1-Offset (Flippy-Laufwerke)|
|`--revs`|Umdrehungs-Spinner|
|`--densel`|Dichteauswahl-Combo (hd / dd / ed)|
|`--bitrate`|Bitraten-Spinner (0 = automatisch)|
|`--retries`|Wiederholungs-Checkbox + Anzahl-Spinner|
|`--no-clobber`|Bereits im Image vorhandene Tracks überspringen|
|`--raw`|Rohen Fluss schreiben, Format-Codec umgehen|
|`--reverse`|Track-Daten umkehren (z. B. Seite B von Flippy-Disketten)|
|`--hard-sectors`|Unterstützung für hartsektorierte Disketten|
|`--erase`|Diskette vor dem Schreiben löschen|
|`--verify`|Diskette nach dem Schreiben verifizieren|
|`--precomp`|Schreib-Präkompensation (µs)|
|`--gen-tg43`|/TG43-Signal für 8″-Laufwerke erzeugen|
|Zusätzliche Argumente|Freitextfeld, wird unverändert angehängt|

Eine **Live-Befehlsvorschau** am unteren Rand des Dialogs zeigt beim Anpassen der Einstellungen den exakten `gw.exe`-Aufruf, der ausgeführt wird.

### Disk-Visualisierung

Jedes Job-Panel zeigt ein Statusraster pro Track:

* **Seite 0 (Kopf 0 – Oben)** — horizontale Leiste mit 84 Zellen
* **Seite 1 (Kopf 1 – Unten)** — horizontale Leiste mit 84 Zellen

|Zellfarbe|Bedeutung|
|-|-|
|Dunkelgrau|Unbekannt / noch nicht gestartet|
|Mittelgrau|Ausstehend|
|Blau|Wird gerade gelesen / geschrieben|
|Grün|In Ordnung|
|Rot|Fehler|

Die Zellen aktualisieren sich in Echtzeit durch das Parsen der `gw.exe`-Ausgabezeilen (z. B. `T00.0: ok`, `Cyl 0, Head 0: reading`); ein Fortschrittsanteil-Fallback greift bei Versionen mit abweichender Ausgabe.

### Job-Presets

* Jede Job-Konfiguration lässt sich als benanntes Preset speichern (JSON unter `%APPDATA%\\GWCopyPro\\Presets\\`).
* Ein Preset laden, um exakt denselben Job wiederherzustellen — Gerät, Format, Track-Bereich, Flags, Post-Aktionen und Dateimuster inklusive.
* Die **Neustart**-Schaltfläche eines abgeschlossenen oder fehlgeschlagenen Jobs erzeugt ihn aus dem gespeicherten Preset-Schnappschuss neu.

### Repetitiver Modus

Konzipiert für Massen-Imaging (z. B. das Digitalisieren einer ganzen Diskettenbox):

* **Repetitiven Modus** im Dialog „Neuer Job" aktivieren.
* Ein **Dateimuster** mit optionalen Platzhaltern festlegen: `{n}`, `{n:D3}` (Zähler mit führenden Nullen), `{dt}` (Datums-/Zeitstempel).
* **Ausgabeordner** und **Startindex** wählen.
* Nach jeder abgeschlossenen Diskette fordert die App zum Einlegen der nächsten auf und fährt automatisch fort, wobei der Zähler hochgezählt wird.
* Eine Mustervorschau im Dialog zeigt, wie die Dateinamen aussehen werden (z. B. `Disk\_001\_20260101\_120000.scp`).

### Post-Aktionen (sequenziell)

Nach einem erfolgreichen Job können beliebig viele Aktionen nacheinander ausgeführt werden:

|Typ|Ausführung|
|-|-|
|**Programm**|Direkter Aufruf mit Ihren Argumenten|
|**Batch-Skript**|Start über `cmd.exe /c`|
|**PowerShell-Skript**|Start über `powershell.exe -File`|

Verfügbare Platzhalter in den Argumenten:

|Platzhalter|Wird ersetzt durch|
|-|-|
|`{ImageFile}`|Vollständiger Pfad zum Disk-Image|
|`{LogFolder}`|Vollständiger Pfad zum Log-Ordner des Jobs|
|`{JobId}`|Eindeutige Job-Kennung|

Aktionen lassen sich umsortieren (▲ ▼), einzeln aktivieren/deaktivieren und direkt bearbeiten. Anwendungsfälle sind z. B. Prüfsummen-Verifikation, automatische Archivierung, Upload-Skripte oder Formatkonvertierung.

### Logging

* stdout und stderr von `gw.exe` werden live erfasst und nach `Logs/Job\_<Typ>\_<ID>\_<Zeitstempel>/gw\_output.log` geschrieben.
* Die Ausgabe der Post-Aktionen wird an dieselbe Log-Datei angehängt.
* Die Schaltfläche **Log anzeigen** auf jedem Job-Panel öffnet den Log-Ordner im Windows Explorer.

### Audio- und visuelles Feedback

|Ereignis|Ton|Visuell|
|-|-|-|
|Job gestartet|Zwei aufsteigende Pieptöne|Aktualisierung der Statusleiste|
|Job abgeschlossen|Drei aufsteigende Pieptöne|Grün leuchtender Rahmen am Job-Panel|
|Job-Fehler|Drei absteigende Pieptöne|Roter Rahmen + Blinken des Fensterhintergrunds|
|Track-Fehler|—|Rote Zelle in der Disk-Visualisierung|

### Einstellungen

* **Pfad zu gw.exe** — auf die lokale Installation verweisen; standardmäßig wird `gw.exe` im `PATH` verwendet.
* **Sprache** — Benutzeroberfläche auf Englisch oder Deutsch.
* Einstellungen werden als JSON unter `%APPDATA%\\GWCopyPro\\settings.json` gespeichert.

## Struktur des Log-Ordners

```
Logs/
  Job\_Read\_a1b2c3d4\_20260601\_143022/
    gw\_output.log
  Job\_Write\_e5f6g7h8\_20260601\_143155/
    gw\_output.log
```

## Architektur

Das Projekt ist eine einzelne .NET-8-Windows-Forms-Anwendung (`net8.0-windows`) mit drei Schichten:

|Schicht|Namespace|Verantwortlichkeit|
|-|-|-|
|**Models**|`GWCopyPro.Models`|`GwParameters`, `GwJob`, `JobPreset`, `PostAction`, Track-Zellen, Enums, Dateimuster-Expander|
|**Services**|`GWCopyPro.Services`|`GwService` (Prozessverwaltung, Ausgabe-Parsing, Event-Dispatch), `GwDetector` (COM-Port-Erkennung), `AppSettings`, `SoundService`|
|**UI**|`GWCopyPro.Forms` / `.Controls`|Hauptfenster, `NewJobDialog`, `DeviceManagerDialog`, `SettingsDialog`, `NextDiskDialog`, `JobPanel`, `DevicePanel`, `FloppyDiskControl`|

### Abhängigkeiten

|Paket|Zweck|
|-|-|
|[NAudio](https://github.com/naudio/NAudio)|Thread-übergreifende Erzeugung von PC-Speaker-/WinMM-Pieptönen|
|[Foundation](https://github.com/NickeManarin/Foundation)|—|
|[unar / lsar](https://theunarchiver.com/command-line)|Mitgelieferte Entpack-Tools (optional für Post-Aktionen)|

## Hinweise

* Die App richtet sich an `gw.exe` v0.24+ und erzeugt `--tracks=`-Verbundselektoren. Die alten Flags `--scyl` / `--ecyl` / `--shead` / `--ehead` / `--single-sided` (in v0.24 entfernt) werden nicht mehr verwendet.
* Falls Ihre `gw.exe`-Version abweichende Ausgaben erzeugt, hält der Fortschrittsanteil-Parser (`n/m`-Zeilen) den Job am Laufen; nur die farbliche Kennzeichnung einzelner Zellen benötigt track-spezifische Log-Zeilen.
* Sämtlicher Zustand (Presets, Einstellungen, Logs) liegt unter `%APPDATA%\\GWCopyPro\\` sowie im Ordner `Logs\\` neben der ausführbaren Datei.

## Lizenz

MIT — siehe [LICENSE](LICENSE) für Details.

