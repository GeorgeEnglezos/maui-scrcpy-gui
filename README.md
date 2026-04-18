# Scrcpy GUI — .NET MAUI

A Windows desktop application that provides a visual command builder for [scrcpy](https://github.com/Genymobile/scrcpy) — the open-source Android screen mirroring tool.

Instead of constructing scrcpy commands by hand, users select options across categorised panels (display, audio, recording, virtual display, package launch) and the app builds and previews the command in real time with syntax highlighting, then executes it via ADB.

---

## Architecture

```
Pages/
  MainPage          — Responsive two-panel layout: command builder + output
  CommandsPage      — Saved favourite commands with syntax highlighting
  SettingsPage      — Path configuration and UI visibility toggles
  InfoPage          — Documentation links

Controls/
  OptionsPanel      — Left panel: groups of scrcpy option controls
  OutputPanel       — Right panel: live command preview, run, save
  SettingsPanelChildren/
    GeneralPanel         — Window, display, video codec/encoder
    AudioPanel           — Audio codec, bitrate, buffer
    ScreenRecordingPanel — Output file, format, bitrate
    PackageSelectionPanel — App picker via ADB package list
    VirtualDisplayPanel  — Virtual display resolution, DPI, flags
  OutputChildren/
    StatusPanel              — ADB/scrcpy version checks, device status
    WirelessConnectionPanel  — TCP/IP pairing flow
  SharedControls/
    CustomButton, CustomCheckbox, CustomTextInput,
    FolderSelector, BorderTitle, FixedHeader, FixedFooter

Services/
  AdbCmdService   — Static utility: process execution, ADB/scrcpy commands, device parsing
  DeviceService   — DI singleton: connected device list, selected device, runtime paths
  DataStorage     — Static: JSON settings persistence, path validation
  ClipboardHelper          — Static: clipboard write with platform error handling
  CommandSyntaxHighlighter — Static: shared colour mappings and FormattedString builder for command syntax highlighting

Models/
  ScrcpyGuiData       — Root settings model (favourites, app settings)
  ConnectedDevice     — ADB device with codec/encoder capability lists
  CmdCommandResponse  — Process execution result (output, error, exit code)
```

---

## Key Engineering Decisions

- **Single-instance enforcement** — Named mutex prevents duplicate windows; existing instance is brought to foreground via Win32 P/Invoke on second launch.
- **DeviceService DI singleton** — Device state (connected list, selected device, scrcpy path) is held in a MAUI DI-registered singleton, accessible both via DI injection and via a static `Instance` property for use in static service classes.
- **Responsive layout** — Both `MainPage` and `OutputPanel` adjust between single-column and two-column grids at runtime based on window width breakpoints (1250px and 750px respectively), using `SizeChanged` handlers that clear and rebuild `ColumnDefinitions`/`RowDefinitions`.
- **Syntax-highlighted command preview** — `CommandSyntaxHighlighter` is the single source of truth for three colour mappings (complete, partial, package-only), each a `Dictionary<string, Color>` keyed on parameter prefixes. Both `OutputPanel` and `CommandsPage` delegate to it to build `FormattedString` spans; the active mapping is selected from settings.
- **Codec/encoder detection** — On device connect, `AdbCmdService.GetCodecsEncodersForEachDevice` runs `scrcpy --list-encoders --serial <id>` per device and parses the output with a regex to populate `VideoCodecEncoderPairs` and `AudioCodecEncoderPairs` on each `ConnectedDevice`.

---

## Tech Stack

- **.NET MAUI 10** (WinUI, Windows 10 19041+)
- **C#** with nullable reference types
- **XAML** for UI layout
- **UraniumUI.Material** — Material Design components and handlers
- **CommunityToolkit.Maui** — behaviours and converters
- **Newtonsoft.Json** — settings persistence

---

## Building

Requirements:
- Visual Studio 2022 with the **.NET MAUI** workload
- Windows 10 SDK (19041+)

```
dotnet build ScrcpyGUI.csproj -f net10.0-windows10.0.19041.0
```

---

## Storage

Settings and favourites are stored at `%APPDATA%\ScrcpyGui\ScrcpyGui-Data.json`.

---

## Context

This is v1.5.1 of Scrcpy GUI — the original Windows-only .NET MAUI implementation. It has since been rewritten in Flutter (v1.6+) for cross-platform support. See the active repository at [GeorgeEnglezos/Scrcpy-GUI](https://github.com/GeorgeEnglezos/Scrcpy-GUI).
