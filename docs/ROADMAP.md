# Constance Save Editor — Roadmap

## Progress

- [x] Phase 0 — Discovery and setup
- [x] Phase 1 — Core codec
- [x] Phase 2 — Document model
- [x] Phase 3 — Safe save and backups
- [x] Phase 4 — CLI
- [ ] Phase 5 — Basic UI
- [ ] Phase 6 — Structured editors
- [ ] Phase 7 — Backup and history UI
- [ ] Phase 8 — Polish and release

---

## Project goal

Build a save editor for **Constance** that can:

- read `.save` files
- decode and extract their contents
- let the user inspect and edit save data
- save back to the original format
- keep backups and allow revert/undo where sensible
- share all core logic through one reusable library

---

## Proposed solution structure

```text
ConstanceSaveEditor.sln
├─ ConstanceSaveEditor.Core
├─ ConstanceSaveEditor.Cli
├─ ConstanceSaveEditor.Ui
├─ ConstanceSaveEditor.Tests
└─ docs
```

## Purpose of each project

### `ConstanceSaveEditor.Core`

The main library. Contains all actual logic.

Responsibilities:

- detect file format
- decode and encode save files
- extract ZIP content
- read and write `save.json`
- read and write optional binary parts
- expose editable document model
- validation
- backup and recovery support
- import and export helpers

### `ConstanceSaveEditor.Cli`

Command-line app for power use and development.

Responsibilities:

- inspect save metadata
- extract a save into a folder
- rebuild a save from extracted files
- pretty-print JSON
- run validation
- patch simple values later if needed

### `ConstanceSaveEditor.Ui`

Desktop app for normal usage.

Responsibilities:

- open, save, and save as
- edit known save values
- maybe raw JSON tab
- show backups and history
- revert unsaved changes
- restore previous saved version

### `ConstanceSaveEditor.Tests`

Unit and integration tests.

Responsibilities:

- verify decode and encode correctness
- ensure round-trip works
- prevent corrupt saves
- verify backup and revert behavior
- validate against sample save files

### `docs`

Project documentation.

Responsibilities:

- file format notes
- architecture overview
- save and revert design
- known game-specific findings
- future reverse engineering notes

---

## Recommended roadmap

### Phase 0 — Discovery and setup

Goal: prepare the project and confirm assumptions.

Tasks:

- create solution and projects
- define coding conventions
- collect sample save files
- verify round-trip manually with a small script
- document file format in `docs/file-format.md`

Deliverable:

- working proof that a save can be loaded and saved unchanged

### Phase 1 — Core codec

Goal: make binary format handling reliable.

Tasks:

- implement XOR codec
- implement header detection
- implement ZIP load and save
- load `save.json`
- load `eventlog.bin`
- load `mapfog.bin`
- support plain JSON fallback
- define `SaveArchiveData`
- write codec unit tests

Deliverable:

- `Core` can decode and re-encode save files

### Phase 2 — Document model

Goal: create editable in-memory representation.

Tasks:

- define `SaveDocument`
- parse `save.json` into `JsonNode`
- deep clone support
- dirty tracking
- source path metadata
- format metadata

Deliverable:

- editable document model separated from raw file codec

### Phase 3 — Safe save and backups

Goal: prevent file corruption and data loss.

Tasks:

- implement save service
- implement temp-file safe write
- implement backup creation
- implement backup cleanup policy
- implement restore and open-backup support
- implement revert unsaved changes

Deliverable:

- reliable save workflow with recovery support

### Phase 4 — CLI

Goal: make development and testing easy.

Tasks:

- create `extract`
- create `rebuild`
- create `show`
- create `validate`
- add readable console output
- add exit codes for automation

Deliverable:

- usable CLI tool for power users and testing

### Phase 5 — Basic UI

Goal: make the editor usable.

Tasks:

- create desktop shell
- file open, save, and save as
- recent files
- show save metadata
- show JSON tree
- edit primitive values
- raw JSON tab
- validation panel
- dirty indicator
- revert button

Deliverable:

- first usable desktop save editor

### Phase 6 — Structured editors

Goal: improve usability beyond raw JSON.

Tasks:

- identify common editable sections
- build typed editor panels
- add field labels and descriptions
- add range checks and warnings
- add search and filter

Deliverable:

- user-friendly editor for common save changes

### Phase 7 — Backup and history UI

Goal: make restore easy and safe.

Tasks:

- list backups per file
- preview metadata
- open backup read-only
- restore backup
- compare current vs backup

Deliverable:

- practical version recovery experience

### Phase 8 — Polish and release

Goal: make the project stable and shippable.

Tasks:

- add integration tests
- test with many real save files
- improve error messages
- handle corrupted saves gracefully
- add logging
- package app
- write user docs

Deliverable:

- release-ready tool

---

## CLI feature roadmap

### First CLI version

Commands:

```text
cse extract <savefile> <outdir>
cse rebuild <dir> <outfile>
cse show <savefile>
cse validate <savefile>
```

### Later CLI commands

```text
cse backup list <savefile>
cse backup restore <savefile> <backup-id>
cse json export <savefile> <jsonfile>
cse json import <savefile> <jsonfile> <outfile>
```

Maybe later:

```text
cse patch <savefile> <jsonpath> <value>
```

---

## UI feature roadmap

### First UI version

Core functionality only:

- open save file
- show metadata
- show JSON tree
- edit values
- save
- save as
- revert unsaved changes
- backup browser
- restore backup
- validation errors panel

### Good initial layout

#### Left

Navigation tree or known sections

#### Center

Editor area

#### Right or bottom

Details, validation, or change status

#### Tabs

Good tab ideas:

- Overview
- Structured editor
- Raw JSON
- Binary info
- Backups

---

## Minimal MVP definition

A good MVP is:

- open `.save`
- decode and parse it
- display JSON
- edit primitive values
- save as valid `.save`
- create backup before overwrite
- revert unsaved changes
- restore old backup

That is already a very useful tool.

---

## Nice-to-have later

- diff view between versions
- field search
- JSONPath-like patching
- plugin system for game-specific typed editors
- schema discovery helpers
- compare two saves
- read-only mode for backup inspection

---

## Very short implementation order

1. `Core` codec
2. tests for round-trip
3. `SaveDocument` and snapshot and revert
4. backup-safe save
5. CLI
6. UI raw editor
7. structured editors
8. history and compare polish
