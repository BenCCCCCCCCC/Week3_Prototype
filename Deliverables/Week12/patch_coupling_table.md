# Week 12 Patch Coupling Table

## Final PATCH-012 Set

| Patch ID | Type | File | Field or object | v0.1 baseline | v0.2 final |
| --- | --- | --- | --- | --- | --- |
| PATCH-012-01 | Numeric | `Assets/Configs/InteractionStats_Default.asset` | `repairHoldSeconds` | `2` | `10` |
| PATCH-012-02 | Mechanism parameter | `Assets/Configs/InteractionStats_Default.asset` | `rescueHoldSeconds` | `3.5` | `2.8` |
| PATCH-012-03 | Map | `Assets/Scenes/Map2_W7.unity` | `Map2_W7 / Cover1` position | `{ x: 6.99, y: 0.51, z: -4.97 }` | `{ x: 6.2, y: 0.51, z: -4.2 }` |

## Coupling Matrix

| Patch pair | Coupling risk | Coupling mechanism | Metric attribution rule |
| --- | --- | --- | --- |
| PATCH-012-01 + PATCH-012-02 | Medium | Longer repair pacing can create more time for downs, chairs, and rescue opportunities. Shorter rescue hold time can then extend the match after a chair event. | If match duration rises, attribute the result to the combined v0.2 set unless rescue process notes isolate chair-stage impact. |
| PATCH-012-01 + PATCH-012-03 | High | Longer repair pacing creates more patrol and chase time. Cover1 position can change chase routes and first down timing during that longer objective phase. | Use repair completion rate for PATCH-012-01 context and first down or route notes for PATCH-012-03 context. Do not assign all duration change to one patch. |
| PATCH-012-02 + PATCH-012-03 | Medium | Cover1 can delay first down, while rescue hold time acts after a chair event. Both can reduce early match closure through different stages. | Separate pre-chair observations from post-chair rescue observations in notes. |

## Metric Coupling Notes

- Repair pacing affects match duration, repair completion rate, and escape rate.
- Rescue window affects post-chair match closure, escape rate, and rescue process observations.
- Cover1 position affects chase route, first down time, and Survivor route-choice observations.
- Escape rate is coupled across all three patches and should be interpreted as a combined v0.2 outcome unless notes clearly isolate a stage.

## Superseded Calibration Attempts

The following changes remain in git history as early calibration attempts, but they are not part of the final Week 12 PATCH-012 set:

- `Hunter/PlayerController.externalSpeedMultiplier: 1 -> 0.93`
- `MatchManager.endgameDuration: 15 -> 30`

These two changes were removed before formal v0.2 post-patch testing. They must not be used in v0.2 post-patch metric explanation or success judgment.

## Attribution Policy

- Use PATCH-specific metrics when they are directly observable.
- Use combined v0.2 wording when multiple patches plausibly affect the same metric.
- Do not treat rescue success rate as the only PATCH-012-02 criterion because v0.1 rescue sample size was insufficient.
- Do not treat map richness as an independent success metric; use route-choice notes only as auxiliary evidence for PATCH-012-03.
