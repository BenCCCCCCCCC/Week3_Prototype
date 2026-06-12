# Week 12 Implementation Self Check

## Final Official PATCH-012 Set

| Patch ID | Type | File | Field or object | v0.1 baseline value | v0.2 final value | Commit | Rollback |
| --- | --- | --- | --- | --- | --- | --- | --- |
| PATCH-012-01 | Numeric | `Assets/Configs/InteractionStats_Default.asset` | `repairHoldSeconds` | `2` | `10` | `9fb827ac698501fdd6ad28ee5bff24e47db4276c` | Set `repairHoldSeconds` from `10` back to `2`. If reverting only the final commit, set `10` back to `6`. |
| PATCH-012-02 | Mechanism parameter | `Assets/Configs/InteractionStats_Default.asset` | `rescueHoldSeconds` | `3.5` | `2.8` | `7c9011a96911e757a5ea6f9dae086644e302db57` | Set `rescueHoldSeconds` from `2.8` back to `3.5`. |
| PATCH-012-03 | Map | `Assets/Scenes/Map2_W7.unity` | `Map2_W7 / Cover1` `m_LocalPosition` | `{ x: 6.99, y: 0.51, z: -4.97 }` | `{ x: 6.2, y: 0.51, z: -4.2 }` | `0b97e1466696240889f0b3e4400765f556d3bdd3` | Set Cover1 position from `{ x: 6.2, y: 0.51, z: -4.2 }` back to `{ x: 6.99, y: 0.51, z: -4.97 }`. |

## Implementation History Notes

- PATCH-012-01 had a correction step from `repairHoldSeconds = 2` to `6` before the final value was set to `10`.
- The final Week 12 documentation records PATCH-012-01 as the official baseline-to-final change `2 -> 10`.
- PATCH-012-02 is a rescue interaction hold-time parameter change. It does not pause chair countdown logic.
- PATCH-012-03 moves one existing map object. It does not change Cover1 rotation, scale, collider, scripts, or object count.

## Superseded Changes

The following early calibration attempts were removed from the formal v0.2 patch set before post-patch testing:

- `Hunter/PlayerController.externalSpeedMultiplier: 1 -> 0.93`
- `MatchManager.endgameDuration: 15 -> 30`

They remain in git history only as attempts. They are not part of the final PATCH-012 set and must not be included in v0.2 post-patch metric attribution.

## Boundary Check

- Final PATCH-012-01 changes one numeric field: `repairHoldSeconds`.
- Final PATCH-012-02 changes one mechanism parameter: `rescueHoldSeconds`.
- Final PATCH-012-03 changes one map Transform position: `Map2_W7 / Cover1`.
- No C# script change is part of the final PATCH-012 set.
- No telemetry logic change is part of the final PATCH-012 set.
- No win/loss logic change is part of the final PATCH-012 set.
- No CSV result file is part of the final PATCH-012 implementation set.
- `PatchRollback.cs` still requires final rollback TODO alignment after documentation sync.

## Post-Patch Test Status

Formal v0.2 post-patch testing has not started. Any post-patch result fields must remain placeholders until testing is complete.
