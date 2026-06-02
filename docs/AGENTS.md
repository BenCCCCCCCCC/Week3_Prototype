# Project Nightfile - Codex Rules

This is a Windows Unity C# project for Project Nightfile / 夜行档案.

Current task:
Week 11 Telemetry & Metrics.

Before editing:
- Read Docs/Week11_Telemetry_Requirements.md completely.
- Do not edit files before producing a scan result and implementation plan.
- Prefer small, safe changes.

Hard rules:
- Do not add new gameplay.
- Do not modify scenes, prefabs, ScriptableObjects, or .meta files unless explicitly asked.
- Do not add new MatchStats fields except firstDownTime and rescueAttemptCount.
- Do not create a new MonoBehaviour for telemetry.
- Do not add new Update loops only for telemetry.
- All telemetry must attach to existing call sites.
- Every telemetry payload must include is_abnormal.
- Console telemetry format must exactly match:
  [Telemetry] event=<event_name> | match_id=<id> | key=value | ts=<unix_seconds>
- Do not print C# object ToString().
- Do not use multiline telemetry logs.

Week 11 required implementation:
- Add Assets/Scripts/Telemetry/TelemetryLogger.cs as a static utility class.
- Add firstDownTime and rescueAttemptCount to MatchStats.cs only.
- Add match_start, match_end, and match_abnormal_flagged telemetry.
- Add machine_repair_start and machine_repair_complete telemetry.
- Add survivor_chaired, rescue_attempt_start if safe, and rescue_attempt_complete telemetry.
- Add survivor_downed telemetry and write firstDownTime on the first down.
- F8 prints five metrics.
- F9 remains Week 10 forced settlement.
- settlement_complete must include trigger_source.
- trigger_source must be one of: natural, debug_f9, abnormal_match.

Coding style:
- Keep C# simple and beginner-friendly.
- Prefer readable code over clever abstractions.
- After editing, show changed files and explain Play Mode verification steps.