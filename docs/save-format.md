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

### Completion (`src/ConstanceSaveEditor.Core/Editing/completion.json`)

A save only records what was **done** — never what is still out there. The checklist is baked into
the game's own `SConLevelInfo` and `SConQuest` ScriptableObjects, one per level and quest, each
holding `ConCompletionEntry[]` (a persistence id + a category). `completion.json` is a dump of
those, extracted with [UnityPy](https://github.com/K0lb3/UnityPy) from the asset bundles under
`Constance/CONSTANCE_Data/StreamingAssets/aa/StandaloneWindows64/`.

`CompletionCatalog.Evaluate` mirrors `ConCompletionData.Recalc` from `Constance.Core.dll`, so its
numbers are the ones the in-game map shows:

- every level with `countTowardsCompletion` contributes `VisitedLevel-<id>` plus its completion
  entries, **excluding** the `LightStone` category (`includeLightStones: false`)
- an entry counts as done when its save value is `B1` or an int `> 0`
- each quest contributes `intel + 1` points: the number of intel letters in `ps_quest_<name>_intel`,
  or the full amount once `ps_quest_<name>_status` is `I2` (Completed)
- a region's percentage is `floor(done / total * 100)`, summed over the levels whose `mapBiome`
  (falling back to `biomeType`) is that region — Voids levels mostly map onto a neighbouring region

Categories are `Chest`, `Milkshake`, `Collectable`, `SecretWall`, `ManualTrigger`, `VisitedLevel`
and `LightStone`. Note that a Milkshake Challenge is **not** stored under the entity's own GUID: the
game writes `ps_MilkShake_Prod_<Level>` instead.

To regenerate after a game patch, re-run the extraction against the new bundles; the entries are
keyed by persistence id, which is stable across patches.


---

*Add new findings here as more fields are identified.*
