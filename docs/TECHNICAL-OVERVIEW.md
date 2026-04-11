# Constance Save Editor — Technical Overview

## Save file format

A normal save file uses this structure:

```text
.save file
├─ header: "Xornstance!" (ASCII)
└─ payload: ZIP bytes XORed with repeating key "Xornstance!"
```

Inside the ZIP:

```text
ZIP archive
├─ save.json
├─ eventlog.bin   (optional)
└─ mapfog.bin     (optional)
```

---

## Loading flow

When a save file is opened:

1. Read all bytes
2. Check whether file starts with `Xornstance!`
3. If yes:
   - strip header
   - XOR-decode remaining bytes with repeating key
   - open as ZIP archive
   - load `save.json`
   - load `eventlog.bin` if present
   - load `mapfog.bin` if present
4. If not:
   - try to treat file as plain JSON legacy or debug save
5. Build an in-memory document model

---

## Saving flow

When saving:

1. Serialize edited save data to `save.json`
2. Create ZIP archive
3. Add `eventlog.bin` if present and non-empty
4. Add `mapfog.bin` if present and non-empty
5. XOR ZIP bytes with repeating key
6. Prefix with header `Xornstance!`
7. Write final bytes to disk

---

## Document model design

A good design is to separate three layers.

### 1. File layer

Deals only with bytes and file containers.

Example types:

- `SaveFileCodec`
- `EncodedSaveFile`
- `SaveArchiveData`

### 2. Document layer

Represents an editable save in memory.

Example types:

- `SaveDocument`
- `SaveMetadata`
- `BinaryAttachment`

### 3. Editor layer

Provides editing, validation, history, and UI-friendly APIs.

Example types:

- `SaveEditorSession`
- `SaveValidationResult`
- `SaveChangeTracker`

---

## Recommended core types

### `SaveFileCodec`

Responsible for binary encode and decode.

```csharp
public interface ISaveFileCodec
{
    SaveArchiveData Decode(byte[] fileBytes);
    byte[] Encode(SaveArchiveData archiveData);
}
```

### `SaveArchiveData`

Represents extracted save content before editing.

```csharp
public sealed class SaveArchiveData
{
    public string SaveJsonText { get; init; } = "";
    public byte[]? EventLog { get; init; }
    public byte[]? MapFog { get; init; }
    public SaveContainerFormat Format { get; init; }
}
```

### `SaveDocument`

Represents the editable save.

At first, keep it flexible.

```csharp
public sealed class SaveDocument
{
    public string? SourcePath { get; set; }
    public JsonNode SaveJson { get; set; } = new JsonObject();
    public byte[]? EventLog { get; set; }
    public byte[]? MapFog { get; set; }

    public bool IsDirty { get; set; }
    public DateTimeOffset LoadedAt { get; init; }
}
```

Using `JsonNode` is a good first step because you do not yet know the full schema.

### `SaveEditorSession`

Wraps document editing behavior.

```csharp
public sealed class SaveEditorSession
{
    public SaveDocument Document { get; }
    public SaveDocument OriginalSnapshot { get; }

    public void MarkDirty();
    public void RevertUnsavedChanges();
    public SaveDocument CreateSaveSnapshot();
}
```

---

## How save data is saved and reverted

There are three separate concepts.

### 1. Undo and redo inside the current session

For changes made after opening the save, before closing the app.

Example:

- user changes gold from 100 to 200
- user presses Undo
- value becomes 100 again

This is UI or session state, not file history.

#### Recommendation

Implement this later unless the editor becomes very form-heavy.

For the first version:

- support “Revert unsaved changes”
- skip full granular undo and redo at the start

### 2. Revert unsaved changes

Restore document state to what it was when the file was opened, or to the last successful save.

Example:

- user opens save
- edits many values
- clicks “Revert”
- in-memory document returns to original snapshot

#### Recommendation

Implement from the beginning.

#### How

When loading a save:

- store a deep copy as `OriginalSnapshot`

When user clicks revert:

- replace current document state with a deep clone of `OriginalSnapshot`

This is simple and robust.

### 3. Restore previous saved file version

Recover an earlier file from disk after the user has already saved over it.

Example:

- user saved a broken edit
- wants yesterday’s version back

#### Recommendation

Implement from the beginning with backups.

#### How

Before overwriting a file:

- create a timestamped backup copy in a backup folder

Example backup naming:

```text
slot1.save
Backups/
  slot1_2026-04-11_14-33-20.save
  slot1_2026-04-11_14-40-05.save
```

Then the UI can offer:

- restore latest backup
- restore selected backup
- open backup as separate file

---

## Recommended save strategy

Use **safe save**, not direct overwrite.

### Safe save algorithm

When saving a file:

1. Validate document
2. Serialize document to final byte array
3. Create backup of old file if it exists
4. Write new file to temp path in same directory
5. Replace original with temp file
6. On success:
   - update `OriginalSnapshot`
   - clear dirty flag

This reduces corruption risk.

### Why temp-write first

If serialization or disk write fails, the original file remains intact.

---

## Backup strategy

### Goals

- prevent user data loss
- allow restore of earlier versions
- keep it simple

### Proposed design

For every source file, keep a backup folder:

```text
<MySaveFolder>/
  current.save
  .cse-backups/
    current_2026-04-11_14-33-20.save
    current_2026-04-11_14-40-05.save
```

Or a global app backup folder:

```text
%AppData%/ConstanceSaveEditor/Backups/
```

Per-file local backups are easier for transparency.

### Retention

Keep, for example:

- latest 20 backups per file
- optionally configurable later

### Restore behavior

When restoring a backup:

- either overwrite current file after confirmation
- or open backup as a separate document first

Safer default: open backup first, then user saves intentionally.

---

## JSON editing strategy

Because the full schema is not known yet, use a layered approach.

### Phase 1

Support raw and flexible editing.

Features:

- inspect parsed JSON tree
- edit primitive fields
- raw JSON view
- export and import JSON

### Phase 2

Add typed editors for known important sections.

For example:

- player stats
- resources
- inventory
- map state
- quest flags

### Why this approach is good

You do not block the project on full reverse engineering of the schema.

---

## Validation strategy

The editor should validate before save.

### Basic validation

Check:

- JSON is valid
- required root structure exists
- binary blobs can still be packed
- save can be re-encoded

### Stronger validation

Later, add game-specific rules:

- value ranges
- expected field types
- required arrays and objects
- cross-field consistency

### Validation result model

```csharp
public sealed class SaveValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<string> Errors { get; } = [];
    public List<string> Warnings { get; } = [];
}
```

---

## Suggested folder structure inside `Core`

```text
ConstanceSaveEditor.Core
├─ Abstractions
├─ Backup
├─ Codec
├─ Documents
├─ Json
├─ Services
├─ Validation
└─ Utilities
```

Example classes:

```text
Abstractions/
  ISaveFileCodec.cs
  ISaveDocumentService.cs
  IBackupService.cs

Codec/
  SaveFileCodec.cs
  SaveArchiveReader.cs
  SaveArchiveWriter.cs
  XorObfuscation.cs

Documents/
  SaveDocument.cs
  SaveMetadata.cs
  SaveContainerFormat.cs

Backup/
  BackupService.cs
  BackupEntry.cs

Services/
  SaveDocumentService.cs
  SaveSessionService.cs

Validation/
  SaveValidationResult.cs
  SaveValidator.cs
```

---

## Technology recommendations

### Core

Use:

- `System.Text.Json`
- `JsonNode`
- `System.IO.Compression`

No strong need for Newtonsoft unless you want specific dynamic behavior.

### CLI

Use:

- plain args parsing first, or
- `System.CommandLine` later

### UI

For you, likely best:

- **Avalonia** if you want cross-platform
- **WPF** if Windows-only and fastest setup

Given your background, either is fine. If this tool is mainly for yourself on Windows, WPF is probably faster.

---

## Recommended decisions

These are the decisions worth locking in early.

### 1. Language

C#

### 2. JSON model

Flexible first with `JsonNode`

### 3. Save strategy

Safe save via temp file plus backup

### 4. Revert strategy

Two levels:

- revert unsaved changes from in-memory snapshot
- restore previous saved versions from backup files

### 5. UI strategy

Basic raw editor first, structured editors later
