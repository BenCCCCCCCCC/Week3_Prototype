# PATCH-012-01 A/B 假设

## Week 11 A/B 模板 12 字段

| 字段 | 内容 |
| --- | --- |
| Experiment ID | PATCH-012-01 |
| Experiment name | 修机节奏数值改动 |
| Observed problem | v0.1 baseline 有效局平均对局时长为 `126.9s`，中位数为 `114.5s`，低于 Week 11 对局时长健康区间；同时修机完成率均值为 `78.1%`，说明短对局与较高修机进度同时存在。 |
| Hypothesis | 如果将 `repairHoldSeconds` 从 `2` 调整为 `10`，主目标阶段会延长，Hunter 会获得更多巡逻、追击和打断修机时间；有效局平均对局时长应高于 `126.9s`，修机完成率不应接近 `0%`。 |
| Change content | 文件：`Assets/Configs/InteractionStats_Default.asset`；字段：`repairHoldSeconds`；旧值：`2`；新值：`10`。 |
| Affected telemetry | `match_start`、`match_end`、`machine_repair_start`、`machine_repair_progress_25`、`machine_repair_progress_50`、`machine_repair_progress_75`、`machine_repair_complete`。 |
| Affected core metrics | 主指标：对局时长、修机完成率。护栏指标：逃生率、首倒时间、救援成功率。 |
| Expected change for main metric | v0.2 有效局平均对局时长应高于 v0.1 的 `126.9s`；修机完成率应保持可推进，不接近 `0%`。 |
| Expected change for guardrail metric | 逃生率只作方向性观察，不要求在 10 局样本内达标；首倒时间不应进入 `< 30s` 异常区间；救援成功率若样本不足则继续标记为 `样本不足`。 |
| Success criteria | v0.2 剔除异常局后有效样本至少 8 局；有效局平均对局时长 `> 126.9s`；修机完成率不接近 `0%`；异常局不成为样本主体。 |
| Failure criteria | v0.2 有效局平均对局时长 `<= 126.9s`，或修机完成率接近 `0%`，或剔除异常局后有效样本少于 8 局。 |
| Sample size / duration / split method | v0.2 post-patch 至少 10 局；剔除异常局后有效样本至少 8 局；Hunter 与 Survivor 视角都覆盖；继续使用本地双控模式并在 CSV 中标注。 |

## 实现与判定记录

- 最终实现 commit：`9fb827ac698501fdd6ad28ee5bff24e47db4276c`。
- 正式 v0.2 到 v0.1 回滚：将 `repairHoldSeconds` 从 `10` 恢复为 `2`。
- 实现历史中曾存在 correction：`repairHoldSeconds: 2 -> 6`；最终正式 PATCH-012-01 记录为 `2 -> 10`。
- `Hunter/PlayerController.externalSpeedMultiplier: 1 -> 0.93` 是早期 calibration attempt，不进入 PATCH-012-01 的 v0.2 指标归因。
- v0.2 实测判定：⚠ 部分达标。
