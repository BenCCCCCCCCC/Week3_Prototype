# PATCH-012-01 A/B 假设

## Patch 摘要

| 字段 | 内容 |
| --- | --- |
| Patch ID | PATCH-012-01 |
| Patch 类型 | 数值改动 |
| Patch 名称 | 修机节奏数值改动 |
| 最终文件 | `Assets/Configs/InteractionStats_Default.asset` |
| 最终字段 | `repairHoldSeconds` |
| v0.1 baseline 值 | `2` |
| v0.2 final 值 | `10` |
| 最终实现 commit | `9fb827ac698501fdd6ad28ee5bff24e47db4276c` |

## Baseline 问题

在 v0.1 中，`repairHoldSeconds = 2` 仍是 debug 速度。Week 12 的测试条件是 Map2、4 台密码机、2 名 Survivor、本地双控；在该条件下，主目标阶段过短，难以稳定观察 Hunter 巡逻、追击、打断修机与中段对抗。

## Baseline 证据

- v0.1 有效局平均对局时长：`126.9s`。
- v0.1 有效局中位数对局时长：`114.5s`。
- v0.1 有效局修机完成率均值：`78.1%`。
- v0.1 逃生率：`4 / 16 = 25%`。
- Baseline 数据显示，短对局与较高修机进度可以同时出现，因此 `repairHoldSeconds` 需要与对局时长、修机完成率和逃生结果一起解释。

## 假设

如果将 `repairHoldSeconds` 从 `2` 调整为 `10`，主目标阶段应延长。Hunter 应获得更多巡逻、启动追击和打断修机的时间。

## 目标指标

- 对局时长。
- 修机完成率。

## 护栏指标

- 逃生率。
- 首倒时间。
- 异常局数量。

## 计划改动

只改动：

- `Assets/Configs/InteractionStats_Default.asset`
- `repairHoldSeconds: 2 -> 10`

实现历史说明：

- 正式组合前曾存在 correction：`repairHoldSeconds: 2 -> 6`。
- 最终正式 PATCH-012-01 记录为 baseline 到 final 的 `2 -> 10`。
- 最终实现 commit 将工作树中的值从 `6` 收束为 `10`。

## 预期方向

- v0.2 有效局平均对局时长应高于 v0.1 baseline 的 `126.9s`。
- 修机完成率不应接近 `0%`。
- 有效局中仍应能观察到修机进度推进。

## 风险

- 如果该值相对当前小地图测试条件过高，修机完成率可能下降到难以推进。
- 如果该值仍过低，主目标阶段仍可能不足以支撑稳定 post-patch 对比。
- 本地双控限制了对多人协作、技能时机和临场反应的结论强度。

## Rollback 计划

正式 v0.2 到 v0.1 回滚：将 `repairHoldSeconds` 从 `10` 恢复为 `2`。

## 验证方法

- v0.2 post-patch 测试沿用相同异常规则：单局时长少于 90 秒的局不进入核心指标计算。
- 对比 v0.2 有效样本的对局时长和修机完成率与 v0.1 baseline。
- 异常局单独记录。
- CSV 中继续标注 `本地双控` 测试模式。

## 判定规则

PATCH-012-01 支持假设的条件：

- 有效局平均对局时长高于 `126.9s`。
- 修机完成率不接近 `0%`。
- 异常局数量不成为样本主体。

如果对局时长上升，但修机完成率接近 `0%`，则说明该数值对主目标阶段造成过度拉长。

## Superseded 记录

`Hunter/PlayerController.externalSpeedMultiplier: 1 -> 0.93` 保留在 git 历史中作为早期 calibration attempt。它不是最终 PATCH-012-01，不进入 v0.2 post-patch 指标归因。
