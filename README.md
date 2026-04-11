# Constance Save Editor

A save file editor for the game **Constance**. Comes with a desktop UI and a CLI.

## Download

Grab the latest build from the [Releases](../../releases/latest) page.

| File | Platform |
|---|---|
| `ConstanceSaveEditor-UI-win-x64.zip` | Windows |
| `ConstanceSaveEditor-UI-linux-x64.tar.gz` | Linux |
| `ConstanceSaveEditor-UI-osx-x64.zip` | macOS (Intel) |
| `ConstanceSaveEditor-UI-osx-arm64.zip` | macOS (Apple Silicon) |

CLI builds follow the same naming with `-CLI-` instead of `-UI-`.

> **macOS:** binaries are not code-signed. On first launch, right-click → Open, or run:
> ```bash
> xattr -d com.apple.quarantine ConstanceSaveEditor.UI
> ```

## Features

- Open, edit, and save `.save` files
- Structured editor for player stats and currency (Glimmer)
- Raw JSON editor with search and highlight
- Automatic backup before every save, with restore from backup history
- Revert unsaved changes
- Dark / Light / System theme
- English and German UI

## CLI

```
cse show <file>              display save metadata and player info
cse extract <file> <outdir>  decode a save to a folder
cse rebuild <indir> <file>   re-encode a folder back into a save file
cse validate <file>          check file integrity
```

## Build from source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
git clone https://github.com/srenner06/ConstanceSaveEditor.git
cd ConstanceSaveEditor
git config core.hooksPath .githooks
dotnet run --project src/ConstanceSaveEditor.UI/
```
