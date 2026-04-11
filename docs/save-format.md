# Save Format Notes

Game-specific observations from reverse engineering real save files.

---

## File container

See `TECHNICAL-OVERVIEW.md` for the full container format (Xornstance header, XOR codec, ZIP layout).

---

## Known JSON fields

### Currency (Glimmer)

Three fields change together when the player receives currency:

| Field | Description |
|---|---|
| `ps_item_Currency` | Current held amount |
| `ps_TotalCollectCount_Currency` | Total ever collected |
| `ps_StatId_AllTimeCollectedGlimmer` | Lifetime stat tracker |

**Possible constraint:** the game may enforce `ps_item_Currency <= ps_TotalCollectCount_Currency`.
Do not set the current amount above the total collected until this is confirmed safe.

---

*Add new findings here as more fields are identified.*
