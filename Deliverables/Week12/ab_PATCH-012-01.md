# PATCH-012-01 A/B Hypothesis

| Field | Content |
| --- | --- |
| Patch ID | PATCH-012-01 |
| Patch name | 降低 Hunter Shift / 追击移动压力 |
| Baseline problem | v0.1 有效样本显示对局收束速度偏快，Hunter 压力较早进入对局。 |
| Baseline evidence | v0.1 baseline commit: `c3c930f9e71aa25ecc406b9c8f506b9507605ab1`；baseline 文档 commit: `b60f5200839e7fcecd9e2835b2ffbf2b414d0e71`；有效局平均对局时长约 126.9 秒；7/8 个有效局发生首倒；首倒均值约 50.9 秒；本地双控观察到 Hunter Shift 移动增幅偏高，Hunter 容易较快完成追击闭环。 |
| Hypothesis | 如果降低 Hunter Shift / 冲刺移动压力，则首倒时间应延后，对局时长应增加。 |
| Target metric | 对局时长；首倒时间。 |
| Guardrail metric | 修机完成率；逃生率；异常局数量。 |
| Planned change | 候选字段待实现前确认：Hunter movement / sprint / run speed 相关字段。正式实现必须选择一个最小、可回滚字段，不在本 A/B 文档中写死具体代码实现或新数值。 |
| Expected direction | v0.2 有效局平均对局时长高于 v0.1 baseline 的 126.9 秒；有效局首倒均值高于 v0.1 baseline 的 50.9 秒；异常局比例不应增加。 |
| Risk | 移动压力降低可能导致 Hunter 难以形成追击闭环，进而使修机完成后逃生率上升过高；本地双控样本也可能低估真实多人对抗中的走位、协作和技能博弈。 |
| Rollback plan | 将本 Patch 实现时确认的 Hunter movement / sprint / run speed 字段恢复到 v0.1 baseline 值；PatchRollback 只在具体字段和值获批后补入真实回滚内容。 |
| Validation method | 使用 `baseline_v0.1_log.csv` 作为 v0.1 对照；v0.2 使用同一异常规则，少于 90 秒的对局标记为 `is_abnormal=TRUE` 并从核心指标计算中剔除；记录对局时长、首倒时间、修机完成率、逃生率和异常原因。 |
| Decision rule | 若 v0.2 有效局平均对局时长和首倒均值同时高于 v0.1 baseline，且修机完成率、逃生率和异常局数量没有出现反向极端变化，则该方向可进入保留候选；若首倒时间没有延后或对局时长没有增加，则该 Patch 方向不成立。 |
