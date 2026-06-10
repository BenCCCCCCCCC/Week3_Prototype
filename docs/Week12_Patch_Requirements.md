# Week 12 Patch Requirements - Project Nightfile

## Goal

Create v0.2 Patch through a complete data-driven loop:

measurement → hypothesis → change → validation → review.

No new gameplay systems should be added this week.

## Required baseline rule

v0.1 baseline data must be collected before any patch value is changed.

Patch Note must use v0.1 baseline values from the Week 12 baseline test package.

Do not use old Week 11 data as v0.1 baseline.

## Core metrics

Only use the five Week 11 core metrics:

1. Match duration
2. First down time
3. Machine repair completion rate
4. Escape rate
5. Rescue success rate

Do not create new Week 12 success metrics.

## Baseline test requirements

Run at least 10 matches.

After removing abnormal matches, valid samples must be at least 8.

Hunter and Survivor perspectives must each appear in at least 3 matches.

Every valid match must include `[Telemetry]` Console output.

A match shorter than 90 seconds must be marked as `is_abnormal=true`.

A match without required telemetry must not count as a valid sample.

## Baseline log format

Each match must have one row:

* match_no
* mode
* duration_seconds
* escaped_count
* first_down_time_seconds
* repair_progress_percent
* is_abnormal
* abnormal_reason
* observed_issue_cn_30_chars

## Baseline metric summary

After abnormal matches are removed, calculate:

* v0.1 measured value
* health / warning / abnormal classification
* deviation direction

If the sample size is insufficient, write `样本量不足`.

Insufficient metrics must not be used as the main reason for a patch change.

## Required patch distribution

There must be exactly 3 patch changes:

1. Numeric change:

   * example: repairSeconds, hunter chase speed, cooldown, interaction duration.

2. Mechanism change:

   * example: rescue window, down limit, repair check mechanism.

3. Map change:

   * example: one machine position, one obstacle prefab, one Transform value.

Do not count multiple changes in the same file as separate patch changes.

Do not move 5 machines and count them as 5 changes.

## A/B hypothesis requirements

Each patch must have one full A/B hypothesis document.

Experiment IDs:

* PATCH-012-01
* PATCH-012-02
* PATCH-012-03

Each hypothesis must include the Week 11 12 fields:

1. Experiment ID
2. Experiment name
3. Observed problem
4. Hypothesis
5. Change content
6. Affected telemetry
7. Affected core metrics
8. Expected change for main metric
9. Expected change for guardrail metric
10. Success criteria
11. Failure criteria
12. Sample size / duration / split method

Week 12 extra requirements:

* observed problem must cite v0.1 baseline value,
* change content must include file, field, old value, and new value,
* affected telemetry must already exist in Week 11,
* main metric must come from warning or abnormal baseline range,
* expected change must include absolute value and percentage point when applicable,
* post-patch test must have at least 8 valid matches for a full decision.

## Interaction between patch changes

The 3 changes must include a coupling table.

For each pair, state:

* whether they are coupled,
* coupling mechanism,
* attribution strategy.

Attribution strategy can be:

* sequential A/B test,
* separate regression test,
* accept joint attribution.

## Post-patch test requirements

After all 3 patch changes are applied, run at least 10 matches.

After removing abnormal matches, valid samples must be at least 8.

Hunter and Survivor perspectives must both be covered.

Use the same telemetry and log format as baseline.

## Patch decision labels

Each patch must be classified as one of:

* ✅ 达标
* ⚠ 部分达标
* ❌ 未达标
* 🚫 样本不足

If a patch fails, the Patch Note must say:

* 下版本回滚

or

* 下版本继续观察，并写明条件

## Required deliverables

Create or update files under:

`Deliverables/Week12/`

Required files:

1. `baseline_v0.1_log.csv`
2. `baseline_v0.1_metrics.md`
3. `ab_PATCH-012-01.md`
4. `ab_PATCH-012-02.md`
5. `ab_PATCH-012-03.md`
6. `patch_coupling_table.md`
7. `postpatch_v0.2_log.csv`
8. `v0.1_vs_v0.2_comparison.md`
9. `patch_note_v0.2.md`
10. `data_risk_analysis.md`
11. `implementation_self_check.md`

## Unity requirements

Unity project must include:

1. Three independent patch commits.
2. `Assets/Editor/PatchRollback.cs`.
3. Editor menu:
   `Tools/Patch/Rollback v0.2 → v0.1`
4. Version number display:
   `v0.1` or `v0.2`
5. Week 12 CSV files under:
   `Deliverables/Week12/`

## Demo requirement

Final demo video or GIF must be no longer than 90 seconds.

It must show:

1. v0.1 and v0.2 each running one match.
2. Version number visible in settlement or main menu.
3. `[Telemetry]` Console output in both versions.
4. Rollback menu triggered once in Unity Editor.
