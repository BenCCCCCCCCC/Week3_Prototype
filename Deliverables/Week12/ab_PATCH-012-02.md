# PATCH-012-02 A/B Hypothesis

## Patch Summary

| Field | Content |
| --- | --- |
| Patch ID | PATCH-012-02 |
| Patch type | Mechanism parameter patch |
| Patch name | Rescue window parameter change |
| Final file | `Assets/Configs/InteractionStats_Default.asset` |
| Final field | `rescueHoldSeconds` |
| v0.1 baseline value | `3.5` |
| v0.2 final value | `2.8` |
| Final implementation commit | `7c9011a96911e757a5ea6f9dae086644e302db57` |

## Baseline Problem

In v0.1 local dual-control testing, rescue opportunities were difficult to evaluate because chair pressure could close the match quickly after a down. The rescue success sample was insufficient, so rescue success rate cannot be the only success criterion for this patch.

## Baseline Evidence

- v0.1 valid sample mean match duration: `126.9` seconds.
- v0.1 escape rate: `4 / 16` Survivor seats, or `25%`.
- Rescue success rate sample size was insufficient for a primary metric.
- Observed rescue attempts were constrained by local dual-control testing and limited coordination.

## Hypothesis

If `rescueHoldSeconds` is changed from `3.5` to `2.8`, Survivor rescue interaction should require less continuous hold time. This should create a wider rescue response window after a chair event without changing chair countdown logic.

## Target Metric

- Match duration.
- Escape rate.
- Rescue process observation records.

## Guardrail Metric

- First down time.
- Repair completion rate.
- Abnormal match count.

## Planned Change

Change only:

- `Assets/Configs/InteractionStats_Default.asset`
- `rescueHoldSeconds: 3.5 -> 2.8`

This patch is a rescue window mechanism parameter change. It is not an implementation of rescue-time chair countdown pause.

## Expected Direction

- Valid mean match duration should increase if chair flow stops ending matches too quickly.
- Escape rate may rise if additional rescue opportunities convert into continued play.
- Rescue process notes should show whether Survivors can complete rescue interactions more often.

## Risk

- Because rescue success rate has insufficient v0.1 sample support, a single rescue outcome cannot prove the patch.
- Shorter rescue hold time may interact with repair pacing and map routing, so attribution must be written as part of the v0.2 combined patch result when needed.
- Local dual-control testing may underuse rescue coordination and skills.

## Rollback Plan

Set `rescueHoldSeconds` from `2.8` back to `3.5` in `Assets/Configs/InteractionStats_Default.asset`.

## Validation Method

- Run v0.2 post-patch matches using the same abnormal rule as v0.1.
- Compare valid-sample match duration and escape rate.
- Record chair and rescue process observations in match notes.
- Do not use rescue success rate as the only pass/fail criterion.

## Decision Rule

PATCH-012-02 supports the Week 12 hypothesis if:

- Valid mean match duration is higher than v0.1 baseline directionally.
- Escape outcomes or rescue process notes show that chair events do not immediately close the match in most valid samples.
- Guardrail metrics do not show a new failure pattern, such as first down time collapsing or abnormal matches dominating the sample.

If rescue observations remain too sparse, the result should be documented as inconclusive for rescue-specific proof while still contributing to the combined v0.2 analysis.
