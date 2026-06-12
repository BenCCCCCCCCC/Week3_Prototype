# PATCH-012-03 A/B Hypothesis

## Patch Summary

| Field | Content |
| --- | --- |
| Patch ID | PATCH-012-03 |
| Patch type | Map patch |
| Patch name | Cover1 chase routing position change |
| Final file | `Assets/Scenes/Map2_W7.unity` |
| Final object | `Map2_W7 / Cover1` |
| Final field | `m_LocalPosition` |
| v0.1 baseline value | `{ x: 6.99, y: 0.51, z: -4.97 }` |
| v0.2 final value | `{ x: 6.2, y: 0.51, z: -4.2 }` |
| Final implementation commit | `0b97e1466696240889f0b3e4400765f556d3bdd3` |

## Baseline Problem

v0.1 qualitative observation showed limited route choices and short chase loops in parts of Map2. This can make Hunter pressure arrive early and can reduce Survivor second-choice routing during chase.

## Baseline Evidence

- v0.1 valid sample mean match duration: `126.9` seconds.
- v0.1 valid samples had first downs in `7 / 8` matches.
- v0.1 valid first down mean, excluding `-1`: `50.9` seconds.
- Qualitative notes describe limited cover, routing, and secondary choice points.

## Hypothesis

If `Cover1` is moved from `{ x: 6.99, y: 0.51, z: -4.97 }` to `{ x: 6.2, y: 0.51, z: -4.2 }`, the right-side local route should create more Survivor second-choice movement during chase. This may delay first down and increase match duration.

## Target Metric

- First down time.
- Match duration.

## Guardrail Metric

- Repair completion rate.
- Escape rate.
- Abnormal match count.

## Auxiliary Observation

- Whether Survivor shows more route choices around the Cover1 area during chase.
- Whether the adjusted cover position creates any new collision snag.

## Planned Change

Change only:

- `Assets/Scenes/Map2_W7.unity`
- Object: `Map2_W7 / Cover1`
- Field: `m_LocalPosition`
- Old position: `{ x: 6.99, y: 0.51, z: -4.97 }`
- New position: `{ x: 6.2, y: 0.51, z: -4.2 }`

Do not change:

- Rotation.
- Scale.
- Collider.
- Scripts.
- Object count.

## Expected Direction

- First down time should move later than v0.1 directionally.
- Valid mean match duration should be higher than v0.1 baseline directionally.
- Survivor route notes should show more second-choice movement near the right-side route.

## Risk

- A single cover movement may have limited impact if repair pacing dominates match duration.
- The map change can couple with repair pacing because longer repair time creates more chase opportunities.
- The map change can couple with rescue timing because delayed first down changes chair timing.

## Rollback Plan

Set `Map2_W7 / Cover1` position from `{ x: 6.2, y: 0.51, z: -4.2 }` back to `{ x: 6.99, y: 0.51, z: -4.97 }`.

## Validation Method

- Run v0.2 post-patch matches using the same abnormal rule as v0.1.
- Compare first down time and match duration against v0.1 baseline.
- Record route-choice observations near Cover1.
- Watch for any new collision snag around Cover1.

## Decision Rule

PATCH-012-03 supports the Week 12 hypothesis if:

- Valid first down time moves later than the v0.1 mean of `50.9` seconds directionally.
- Valid mean match duration is higher than `126.9` seconds.
- Route notes show that Cover1 creates additional second-choice movement without introducing a new blocker.

If first down time does not move and no route-choice difference is observed, the map patch should be treated as low impact in the combined v0.2 result.

## Superseded Calibration Note

`MatchManager.endgameDuration: 15 -> 30` remains in git history as an early calibration attempt. It is not the final PATCH-012-03 and must not be used in v0.2 post-patch metric attribution.
