# Implementation Self Check

## Patch Application

| Patch ID | Documented | Code state | v0.1 valid samples | v0.2 valid samples | Decision | Notes |
| --- | --- | --- | --- | --- | --- | --- |
| PATCH-012-01 | 待填写 | not applied in preparation pass | 待填写 | 待填写 | 待填写 | 待填写 |
| PATCH-012-02 | 待填写 | not applied in preparation pass | 待填写 | 待填写 | 待填写 | 待填写 |
| PATCH-012-03 | 待填写 | not applied in preparation pass | 待填写 | 待填写 | 待填写 | 待填写 |

## Metric Observability

| Metric | v0.1 observable | v0.2 observable | Decision usable | Missing item |
| --- | --- | --- | --- | --- |
| Match duration | yes | yes | yes | - |
| First down time | yes | yes | yes | - |
| Machine repair completion rate | yes | yes | yes | - |
| Escape rate | yes | yes | sample-size dependent | 50-match threshold for full conclusion |
| Rescue success rate | yes | yes | sample-size dependent | 30-match threshold for full conclusion |

## Preparation Pass Check

- Version display: added to settlement output as `Version: v0.1`.
- Rollback menu: `Tools/Patch/Rollback v0.2 -> v0.1` source path added; exact Unity menu label uses the arrow character required by Week 12.
- Gameplay values changed in preparation pass: no.
- Telemetry definitions changed in preparation pass: no.
