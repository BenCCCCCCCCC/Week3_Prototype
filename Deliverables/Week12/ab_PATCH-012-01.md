# PATCH-012-01 A/B Hypothesis

## Patch Summary

| Field | Content |
| --- | --- |
| Patch ID | PATCH-012-01 |
| Patch type | Numeric patch |
| Patch name | Repair pacing numeric change |
| Final file | `Assets/Configs/InteractionStats_Default.asset` |
| Final field | `repairHoldSeconds` |
| v0.1 baseline value | `2` |
| v0.2 final value | `10` |
| Final implementation commit | `9fb827ac698501fdd6ad28ee5bff24e47db4276c` |

## Baseline Problem

In v0.1, `repairHoldSeconds = 2` was still a debug-speed value. Under the Week 12 test condition of Map2, 4 ciphers, 2 Survivors, and local dual-control testing, this made the main objective phase too short to observe stable patrol, chase interruption, and mid-match pressure.

## Baseline Evidence

- v0.1 valid sample mean match duration: `126.9` seconds.
- v0.1 valid sample median match duration: `114.5` seconds.
- v0.1 valid sample repair completion mean: `78.1%`.
- v0.1 escape rate: `4 / 16` Survivor seats, or `25%`.
- The baseline data indicates that short matches and high repair progress can coexist, so repair pacing must be evaluated together with match duration and escape outcome.

## Hypothesis

If `repairHoldSeconds` is changed from `2` to `10`, the main objective phase should last longer. Hunter should have more time to patrol, start chases, and interrupt repairs before all ciphers are completed.

## Target Metric

- Match duration.
- Repair completion rate.

## Guardrail Metric

- Escape rate.
- First down time.
- Abnormal match count.

## Planned Change

Change only:

- `Assets/Configs/InteractionStats_Default.asset`
- `repairHoldSeconds: 2 -> 10`

Implementation history note:

- A correction value `2 -> 6` existed before final Week 12 composition.
- The final official Week 12 PATCH-012-01 is documented as `2 -> 10`.
- The final commit changed the working value from `6 -> 10`.

## Expected Direction

- Valid mean match duration should be higher than the v0.1 baseline value of `126.9` seconds.
- Repair completion rate should not collapse near `0%`.
- Repair progress should remain observable across valid matches.

## Risk

- If the value is too high for the current small Map2 test condition, repair completion rate may fall too far.
- If the value is still too low, the main objective phase may remain too short for stable post-patch comparison.
- Local dual-control testing limits conclusions about team coordination and skill timing.

## Rollback Plan

- Formal v0.2 to v0.1 rollback: set `repairHoldSeconds` from `10` back to `2`.
- If reverting only the final PATCH-012-01 commit: set `repairHoldSeconds` from `10` back to `6`.

## Validation Method

- Run v0.2 post-patch matches with the same abnormal rule: matches shorter than 90 seconds are excluded from core metric calculation.
- Compare valid-sample match duration and repair completion rate against v0.1 baseline.
- Record abnormal matches separately.
- Keep local dual-control mode clearly marked in the CSV.

## Decision Rule

PATCH-012-01 supports the Week 12 hypothesis if:

- Valid mean match duration is higher than `126.9` seconds.
- Repair completion rate does not approach `0%`.
- Abnormal match count does not become the dominant sample group.

If match duration rises while repair completion collapses, the patch should be treated as over-correcting the main objective phase.

## Superseded Calibration Note

`Hunter/PlayerController.externalSpeedMultiplier: 1 -> 0.93` remains in git history as an early calibration attempt. It is not the final PATCH-012-01 and must not be used in v0.2 post-patch metric attribution.
