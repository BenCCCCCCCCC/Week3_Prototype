# Patch Note v0.2

## Build Scope

v0.2 contains the final Week 12 PATCH-012 set:

- PATCH-012-01: repair pacing numeric change.
- PATCH-012-02: rescue window mechanism parameter change.
- PATCH-012-03: Cover1 map routing position change.

Formal v0.2 post-patch testing has not started. Measured results are placeholders until the post-patch CSV is filled.

## Change List

| Patch ID | Type | File | Change | Commit | Post-patch result |
| --- | --- | --- | --- | --- | --- |
| PATCH-012-01 | Numeric | `Assets/Configs/InteractionStats_Default.asset` | `repairHoldSeconds: 2 -> 10` | `9fb827ac698501fdd6ad28ee5bff24e47db4276c` | 待 v0.2 post-patch 测试填写 |
| PATCH-012-02 | Mechanism parameter | `Assets/Configs/InteractionStats_Default.asset` | `rescueHoldSeconds: 3.5 -> 2.8` | `7c9011a96911e757a5ea6f9dae086644e302db57` | 待 v0.2 post-patch 测试填写 |
| PATCH-012-03 | Map | `Assets/Scenes/Map2_W7.unity` | `Map2_W7 / Cover1` position `{ x: 6.99, y: 0.51, z: -4.97 } -> { x: 6.2, y: 0.51, z: -4.2 }` | `0b97e1466696240889f0b3e4400765f556d3bdd3` | 待 v0.2 post-patch 测试填写 |

## Intended Measurement

| Patch ID | Primary metric | Guardrail or auxiliary metric |
| --- | --- | --- |
| PATCH-012-01 | Match duration; repair completion rate | Escape rate; first down time; abnormal match count |
| PATCH-012-02 | Match duration; escape rate; rescue process observations | First down time; repair completion rate; abnormal match count |
| PATCH-012-03 | First down time; match duration | Repair completion rate; escape rate; route-choice observations; abnormal match count |

## Superseded Calibration Attempts

The following changes remain in git history as early calibration attempts, but they are not part of the final v0.2 patch note:

- `Hunter/PlayerController.externalSpeedMultiplier: 1 -> 0.93`
- `MatchManager.endgameDuration: 15 -> 30`

They were removed before formal v0.2 post-patch testing and must not be used in v0.2 metric explanation.

## Rollback Summary

- PATCH-012-01: set `repairHoldSeconds` from `10` back to `2`. If reverting only the final commit, set `10` back to `6`.
- PATCH-012-02: set `rescueHoldSeconds` from `2.8` back to `3.5`.
- PATCH-012-03: set Cover1 position from `{ x: 6.2, y: 0.51, z: -4.2 }` back to `{ x: 6.99, y: 0.51, z: -4.97 }`.

## Post-Patch Result Placeholder

- Valid sample count: 待 v0.2 post-patch 测试填写
- Abnormal sample count: 待 v0.2 post-patch 测试填写
- Mean match duration: 待 v0.2 post-patch 测试填写
- Mean first down time: 待 v0.2 post-patch 测试填写
- Mean repair completion rate: 待 v0.2 post-patch 测试填写
- Escape rate: 待 v0.2 post-patch 测试填写
- Rescue process notes: 待 v0.2 post-patch 测试填写

No post-patch success judgment should be written until testing is complete.
