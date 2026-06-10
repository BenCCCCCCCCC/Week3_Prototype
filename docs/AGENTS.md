# Project Nightfile - Codex Instructions

This is a Unity C# project for Project Nightfile / 夜行档案.

Current task: Week 12 Balance Patch v0.2.

The project currently runs without obvious blocking bugs. Do not start by fixing unrelated systems.

## Active Codex skills

Use Superpowers for:

* careful planning,
* checking task constraints,
* preventing accidental broad changes,
* reviewing changed files before final output.

Use Karpathy Guidelines for:

* simple readable code,
* minimal implementation,
* clear comments,
* avoiding over-engineered solutions.

These skills do not override the project rules below.

## Required reading before editing

Before changing any file, read:

1. `docs/Week11_Telemetry_Current_State.md`
2. `docs/Week12_Patch_Requirements.md`
3. this `docs/AGENTS.md`

`docs/Week11_Telemetry_Current_State.md` is the Week 11 source of truth for the current telemetry and metrics delivery state. It is a Markdown copy of `docs/Week11_Telemetry_Current_State.docx`.

After reading, first report:

* what files are relevant,
* what can be safely changed,
* what must not be changed,
* what information is still missing.

## Week 12 goal

Complete a data-driven balance patch loop:

v0.1 baseline measurement → A/B hypothesis → 3 patch changes → v0.2 validation → patch review.

This week is not for adding new gameplay.

## Hard constraints

Do not change gameplay values before v0.1 baseline data is collected.

Do not invent baseline data.

Do not use Week 11 old test data as Week 12 baseline data.

Do not create new success metrics.

Week 12 must reuse these five Week 11 core metrics only:

1. Match duration
2. First down time
3. Machine repair completion rate
4. Escape rate
5. Rescue success rate

Every valid test match must have `[Telemetry]` Console output.

Matches shorter than 90 seconds must be marked as abnormal.

Abnormal matches must be excluded from baseline and post-patch calculation.

## Patch change rules

Week 12 requires exactly 3 patch changes:

1. `PATCH-012-01`: one numeric change.
2. `PATCH-012-02`: one mechanism change.
3. `PATCH-012-03`: one map change.

Each patch change must:

* be isolated,
* be rollbackable,
* have one independent commit,
* have a clear old value and new value,
* map to existing Week 11 telemetry,
* affect at least one main metric and one guardrail metric.

Commit message format:

`[PATCH-012-NN] <change name>`

Do not combine several patch changes in one commit.

## Forbidden vague language

Do not use these words in documents or commit messages:

* optimize
* improve
* enhance
* better feel
* comfortable
* slightly
* minor tweak
* fix some bugs
* 优化
* 改善
* 加强
* 提升手感
* 增强体验
* 微调
* 稍微
* 略有
* 适度
* 修复部分问题
* 解决一些 bug

Every description must point to:

* a specific field,
* a specific value,
* a specific metric,
* or a specific observable behavior.

## Required Week 12 implementation support

Add or verify these items:

1. Version display:

   * Show `v0.1` or `v0.2` in main menu or settlement UI.

2. Rollback editor script:

   * Add `Assets/Editor/PatchRollback.cs`.
   * Add menu item:
     `Tools/Patch/Rollback v0.2 → v0.1`
   * The rollback must restore the old values of all 3 patch changes.

3. Data folder:

   * Ensure this folder exists:
     `Deliverables/Week12/`

4. CSV files:

   * `Deliverables/Week12/baseline_v0.1_log.csv`
   * `Deliverables/Week12/postpatch_v0.2_log.csv`

## First Codex task

For the first pass, do not apply the 3 patch changes.

Only scan the project and prepare Week 12 support structure.

Allowed first-pass changes:

* create `Deliverables/Week12/`,
* create empty CSV templates,
* add version display if safe,
* add rollback script skeleton if safe,
* document candidate fields for later patch changes.

Not allowed in first pass:

* changing repair time,
* changing hunter speed,
* changing rescue values,
* moving map objects,
* changing win/loss logic,
* changing telemetry definitions,
* rewriting multiple gameplay scripts.
