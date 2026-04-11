# Save Format

## File container

```
.save file
├─ header: "Xornstance!" (11 bytes, ASCII)
└─ payload: ZIP bytes XOR'd with repeating key "Xornstance!"
```

To decode: strip the 11-byte header, XOR the rest with the repeating key, then open as a ZIP archive.

Inside the ZIP:

```
├─ save.json       required
├─ eventlog.bin    optional
└─ mapfog.bin      optional
```

Plain JSON files (no header, no XOR) are also supported as a fallback for debug saves.

## Known JSON fields

### Currency (Glimmer)

These three fields change together when the player receives currency:

| Field | Description |
|---|---|
| `ps_item_Currency` | Current held amount |
| `ps_TotalCollectCount_Currency` | Total ever collected |
| `ps_StatId_AllTimeCollectedGlimmer` | Lifetime stat tracker |

The game may enforce `ps_item_Currency <= ps_TotalCollectCount_Currency`. The editor raises both automatically when needed.

---

*Add new findings here as more fields are identified.*
