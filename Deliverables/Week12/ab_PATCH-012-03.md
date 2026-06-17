# PATCH-012-03 A/B 假设

## Week 11 A/B 模板 12 字段

| 字段 | 内容 |
| --- | --- |
| Experiment ID | PATCH-012-03 |
| Experiment name | Cover1 追击路线位置调整 |
| Observed problem | v0.1 baseline 有效局首倒时间均值为 `50.9s`，不包含 `-1`；有效局中 `7 / 8` 局发生首倒。补充观察显示 Map2 局部追击路线和二次选择点较少。 |
| Hypothesis | 如果将 `Cover1` 从 `{ x: 6.99, y: 0.51, z: -4.97 }` 移动到 `{ x: 6.2, y: 0.51, z: -4.2 }`，右侧局部路线应给 Survivor 提供更多二次选择空间；首倒时间应高于 `50.9s`，有效局平均对局时长应高于 `126.9s`。 |
| Change content | 文件：`Assets/Scenes/Map2_W7.unity`；对象：`Map2_W7 / Cover1`；字段：`m_LocalPosition`；旧值：`{ x: 6.99, y: 0.51, z: -4.97 }`；新值：`{ x: 6.2, y: 0.51, z: -4.2 }`。 |
| Affected telemetry | `match_start`、`match_end`、`survivor_down`、`survivor_eliminated`、`machine_repair_progress_25`、`machine_repair_progress_50`、`machine_repair_progress_75`、`machine_repair_complete`。 |
| Affected core metrics | 主指标：首倒时间、对局时长。护栏指标：修机完成率、逃生率、救援成功率。 |
| Expected change for main metric | v0.2 首倒时间均值应高于 v0.1 的 `50.9s`；有效局平均对局时长应高于 v0.1 的 `126.9s`。 |
| Expected change for guardrail metric | 修机完成率不应接近 `0%`；逃生率只作方向性观察；救援成功率若样本不足则继续标记为 `样本不足`。 |
| Success criteria | v0.2 剔除异常局后有效样本至少 8 局；首倒时间均值 `> 50.9s`；有效局平均对局时长 `> 126.9s`；Cover1 附近未出现新的卡边或碰撞阻挡。 |
| Failure criteria | v0.2 首倒时间均值 `<= 50.9s`，或有效局平均对局时长 `<= 126.9s`，或 Cover1 附近出现新的卡边 / 碰撞阻挡，或剔除异常局后有效样本少于 8 局。 |
| Sample size / duration / split method | v0.2 post-patch 至少 10 局；剔除异常局后有效样本至少 8 局；Hunter 与 Survivor 视角都覆盖；继续使用本地双控模式并在 CSV 中标注。 |

## 实现与判定记录

- 最终实现 commit：`0b97e1466696240889f0b3e4400765f556d3bdd3`。
- 正式 v0.2 到 v0.1 回滚：将 `Map2_W7 / Cover1` position 从 `{ x: 6.2, y: 0.51, z: -4.2 }` 恢复为 `{ x: 6.99, y: 0.51, z: -4.97 }`。
- PATCH-012-03 只移动 `Cover1` position，不修改 rotation、scale、collider、脚本或对象数量。
- `MatchManager.endgameDuration: 15 -> 30` 是早期 calibration attempt，不进入 PATCH-012-03 的 v0.2 指标归因。
- v0.2 实测判定：⚠ 部分达标。
