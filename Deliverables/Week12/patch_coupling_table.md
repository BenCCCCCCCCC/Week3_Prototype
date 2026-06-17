# Week 12 Patch 耦合表

## 最终 PATCH-012 组合

| Patch ID | 类型 | 文件 | 字段或对象 | v0.1 baseline | v0.2 final |
| --- | --- | --- | --- | --- | --- |
| PATCH-012-01 | 数值改动 | `Assets/Configs/InteractionStats_Default.asset` | `repairHoldSeconds` | `2` | `10` |
| PATCH-012-02 | 机制参数改动 | `Assets/Configs/InteractionStats_Default.asset` | `rescueHoldSeconds` | `3.5` | `2.8` |
| PATCH-012-03 | 地图改动 | `Assets/Scenes/Map2_W7.unity` | `Map2_W7 / Cover1` position | `{ x: 6.99, y: 0.51, z: -4.97 }` | `{ x: 6.2, y: 0.51, z: -4.2 }` |

## 耦合矩阵

| Patch 组合 | 耦合风险 | 耦合机制 | 归因策略 |
| --- | --- | --- | --- |
| PATCH-012-01 + PATCH-012-02 | 中 | 更长的 repair pacing 会给倒地、挂椅与救援创造更多时间；更短的救援 hold time 可能让挂椅后对局继续延长。 | 如果对局时长上升，除非救援过程记录能单独隔离挂椅阶段影响，否则按 v0.2 组合结果解释。 |
| PATCH-012-01 + PATCH-012-03 | 高 | 更长的 repair pacing 会创造更多巡逻与追击时间；Cover1 位置会影响追击路线和首倒时间。 | 使用修机完成率解释 PATCH-012-01 的上下文，使用首倒时间与路线记录解释 PATCH-012-03 的上下文；不要把全部时长变化归给单一 Patch。 |
| PATCH-012-02 + PATCH-012-03 | 中 | Cover1 可能影响上椅前的追击与首倒；rescue hold time 作用于挂椅后的救援阶段。 | 在记录中区分上椅前追击观察和上椅后救援观察。 |

## 指标耦合说明

- Repair pacing 会影响对局时长、修机完成率和逃生率。
- Rescue window 会影响挂椅后的对局收束、逃生率和救援过程记录。
- Cover1 position 会影响追击路线、首倒时间和 Survivor 路线选择记录。
- 逃生率会同时受到三条 Patch 影响；除非单局记录能明确隔离阶段，否则应按 v0.2 组合结果解释。

## Superseded calibration attempts

以下改动保留在 git 历史中作为早期 calibration attempt，但不属于最终 Week 12 PATCH-012 组合：

- `Hunter/PlayerController.externalSpeedMultiplier: 1 -> 0.93`
- `MatchManager.endgameDuration: 15 -> 30`

这两项已在正式 v0.2 post-patch 测试前移出最终组合，不进入 v0.2 指标解释或成功判定。

## 归因规则

- 当指标能直接对应某条 Patch 的行为阶段时，优先使用该 Patch 的专项解释。
- 当多条 Patch 都可能影响同一指标时，使用 v0.2 组合结果表述。
- v0.1 救援成功率样本不足，因此不能把救援成功率作为 PATCH-012-02 的唯一判据。
- 地图丰富程度不作为独立成功指标；路线选择记录只作为 PATCH-012-03 的辅助证据。
