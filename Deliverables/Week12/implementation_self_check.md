# Week 12 实现一致性自检

## 提交与版本边界

| 项目 | commit |
| --- | --- |
| v0.1 baseline 测试版本 | `c3c930f9e71aa25ecc406b9c8f506b9507605ab1` |
| v0.1 baseline 数据与风险分析文档 | `b60f5200839e7fcecd9e2835b2ffbf2b414d0e71` |
| PATCH-012-01 正式数值改动 | `9fb827ac698501fdd6ad28ee5bff24e47db4276c` |
| PATCH-012-02 正式机制改动 | `7c9011a96911e757a5ea6f9dae086644e302db57` |
| PATCH-012-03 正式地图改动 | `0b97e1466696240889f0b3e4400765f556d3bdd3` |
| Week 12 Patch 文档同步 | `59dbb3f1134cc7727bea5e629d582c5677053f04` |
| `PatchRollback.cs` | `60f7c635a4eafc81b283db3cf2302d5fe403e5a1` |
| v0.2 post-patch 测试日志 | `9a604bfb9d039f7b75a9c9998cde75223ee50e2b` |

## 最终正式 PATCH-012

| Patch ID | 类型 | 文件 | 字段或对象 | v0.1 baseline 值 | v0.2 final 值 | 判定 | Rollback |
| --- | --- | --- | --- | --- | --- | --- | --- |
| PATCH-012-01 | 数值改动 | `Assets/Configs/InteractionStats_Default.asset` | `repairHoldSeconds` | `2` | `10` | ⚠ 部分达标 | 将 `repairHoldSeconds` 从 `10` 恢复为 `2`。 |
| PATCH-012-02 | 机制参数改动 | `Assets/Configs/InteractionStats_Default.asset` | `rescueHoldSeconds` | `3.5` | `2.8` | 🚫 样本不足 / 继续观察 | 将 `rescueHoldSeconds` 从 `2.8` 恢复为 `3.5`。 |
| PATCH-012-03 | 地图改动 | `Assets/Scenes/Map2_W7.unity` | `Map2_W7 / Cover1` `m_LocalPosition` | `{6.99, 0.51, -4.97}` | `{6.2, 0.51, -4.2}` | ⚠ 部分达标 | 将 `Cover1` position 从 `{6.2, 0.51, -4.2}` 恢复为 `{6.99, 0.51, -4.97}`。 |

## 实测数据对齐

| 指标 | v0.1 baseline | v0.2 post-patch | 变化 |
| --- | --- | --- | --- |
| 有效局数 | 8 | 10 | `+2` |
| 异常局数 | 8 | 0 | `-8` |
| 平均对局时长 | `126.9s` | `144.5s` | `+17.6s` |
| 中位数对局时长 | `114.5s` | `140.7s` | `+26.2s` |
| 首倒时间均值 | `50.9s` | `85.1s` | `+34.2s` |
| 修机完成率均值 | `78.1%` | `87.5%` | `+9.4pp` |
| 逃生率 | `4 / 16 = 25%` | `4 / 20 = 20%` | `-5pp` |
| 救援成功率 | 样本不足 | 样本不足 | 不能判定 |

## 历史尝试与正式组合边界

- `Hunter/PlayerController.externalSpeedMultiplier: 1 -> 0.93` 已从正式 PATCH-012 中移除，不进入 v0.2 post-patch 指标解释。
- `MatchManager.endgameDuration: 15 -> 30` 已从正式 PATCH-012 中移除，不进入 v0.2 post-patch 指标解释。
- `repairHoldSeconds: 2 -> 6` 仅为中间 correction 记录，最终正式 PATCH-012-01 为 `2 -> 10`。
- PATCH-012-02 是 `rescueHoldSeconds` 参数改动，不包含救援中暂停椅上倒计时逻辑。
- PATCH-012-03 只移动 `Cover1` 的 position，不修改 rotation、scale、collider、脚本或对象数量。

## Rollback 检查

- `Assets/Editor/PatchRollback.cs` 存在。
- 菜单路径为 `Tools/Patch/Rollback v0.2 -> v0.1`。
- 回滚目标值：
  - `repairHoldSeconds: 10 -> 2`
  - `rescueHoldSeconds: 2.8 -> 3.5`
  - `Cover1 localPosition: {6.2, 0.51, -4.2} -> {6.99, 0.51, -4.97}`
- Rollback demo 现在还会把 `BuildVersionLabel` 从 `v0.2` 回滚为 `v0.1`，用于演示版本归属。
- Rollback 不包含 superseded 的 `Hunter/PlayerController.externalSpeedMultiplier` 或 `MatchManager.endgameDuration`。

## 版本显示检查

- 结算页版本号显示为 `v0.2`。
- 版本号显示只用于测试归属，不属于 PATCH-012 成功判据。

## 测试前修复 / 版本边界说明

- v0.1 baseline 正式测试前，Map2 做过可通行性修复：1F 在 2F 楼板下方、`Cipher_H3`、`Cipher_M`、`Cipher_H2`、`Chair_R`、`Chair_L`、`Cover1`、右侧楼梯附近曾出现卡边或被 invisible collider 阻挡的情况。
- 该问题通过调整 2F slab collider clearance，以及将 Map2 中部分 `RangeVisual` `SphereCollider` 改为 non-blocking trigger 的方式处理。
- 这些处理属于 pre-baseline bugfix，不属于 PATCH-012-01 / PATCH-012-02 / PATCH-012-03，不作为平衡改动，也不作为 v0.2 成功判据。
- v0.1 baseline 正式测试前，Survivor 视角开门后出现过 `Endgame` 倒计时与修机进度 UI 重叠；该问题通过移动 `Endgame` timer UI 位置处理。
- UI 可读性修复属于 pre-baseline UI bugfix，不属于 PATCH-012。

## 文件边界检查

- 最终三条 PATCH-012 没有修改 Telemetry call site。
- 最终三条 PATCH-012 没有修改 win/loss 逻辑。
- 最终三条 PATCH-012 没有修改逃生触发逻辑。
- 最终三条 PATCH-012 没有新增 gameplay 系统。
- CSV 文件只作为测试数据与统计来源，不属于实现改动。
