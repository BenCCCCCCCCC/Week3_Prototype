# PATCH-012-02 A/B 假设

## Week 11 A/B 模板 12 字段

| 字段 | 内容 |
| --- | --- |
| Experiment ID | PATCH-012-02 |
| Experiment name | 救援窗口机制参数改动 |
| Observed problem | v0.1 baseline 有效局平均对局时长为 `126.9s`，逃生率为 `4 / 16 = 25%`；救援成功率样本不足，不能作为主要触发理由，但挂椅后的救援窗口仍需要作为过程观察项。 |
| Hypothesis | 如果将 `rescueHoldSeconds` 从 `3.5` 调整为 `2.8`，Survivor 完成救援交互所需连续按住时间会减少；挂椅后的救援反制窗口应扩大，但不改变椅上倒计时逻辑。 |
| Change content | 文件：`Assets/Configs/InteractionStats_Default.asset`；字段：`rescueHoldSeconds`；旧值：`3.5`；新值：`2.8`。 |
| Affected telemetry | `match_start`、`match_end`、`survivor_chair_start`、`survivor_rescue_start`、`survivor_rescue_complete`、`survivor_rescue_cancel`。 |
| Affected core metrics | 主指标：对局时长、逃生率、救援成功率。护栏指标：首倒时间、修机完成率。 |
| Expected change for main metric | v0.2 有效局平均对局时长应高于 v0.1 的 `126.9s`；救援过程记录中应观察到可完成救援交互的机会。 |
| Expected change for guardrail metric | 首倒时间不应进入 `< 30s` 异常区间；修机完成率不应接近 `0%`；逃生率和救援成功率在小样本中只作方向性观察。 |
| Success criteria | v0.2 剔除异常局后有效样本至少 8 局；对局时长方向上升；救援过程记录显示挂椅事件没有在多数有效样本中立即结束对局。 |
| Failure criteria | v0.2 有效局平均对局时长 `<= 126.9s`，或救援过程记录不足以支持观察，或剔除异常局后有效样本少于 8 局。 |
| Sample size / duration / split method | v0.2 post-patch 至少 10 局；剔除异常局后有效样本至少 8 局；Hunter 与 Survivor 视角都覆盖；继续使用本地双控模式并在 CSV 中标注。 |

## 实现与判定记录

- 最终实现 commit：`7c9011a96911e757a5ea6f9dae086644e302db57`。
- 正式 v0.2 到 v0.1 回滚：将 `rescueHoldSeconds` 从 `2.8` 恢复为 `3.5`。
- 该 Patch 是救援窗口机制参数改动，不是救援中暂停椅上倒计时。
- 救援成功率样本不足，不能作为 PATCH-012-02 的唯一成功判据。
- v0.2 实测判定：🚫 样本不足 / 继续观察。
