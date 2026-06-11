# PATCH-012-03 A/B Hypothesis

| Field | Content |
| --- | --- |
| Patch ID | PATCH-012-03 |
| Patch name | 延长终局撤离窗口 / 降低终局失败转化 |
| Baseline problem | v0.1 中修机完成或进入开门阶段后，逃生没有稳定转化；终局窗口可能过短。 |
| Baseline evidence | v0.1 baseline commit: `c3c930f9e71aa25ecc406b9c8f506b9507605ab1`；baseline 文档 commit: `b60f5200839e7fcecd9e2835b2ffbf2b414d0e71`；有效局修机完成率均值为 78.1%；逃生率为 4 名 Survivor 逃生 / 16 个 Survivor 席位 = 25%；第 14 局出现“开门后未撤离”；日志中出现 endgame countdown duration = 15 的终局倒计时行为。 |
| Hypothesis | 如果延长终局撤离窗口，则修机完成后成功逃生的概率应提高，对局时长也应更接近 Week 11 目标区间。 |
| Target metric | 逃生率；对局时长。 |
| Guardrail metric | 修机完成率；首倒时间；异常局数量。 |
| Planned change | 候选字段待实现前确认：endgame countdown / gate escape window / endgame timer 相关字段。正式实现必须选择一个最小、可回滚的小数值窗口，不在本 A/B 文档中写死具体代码实现或新数值。 |
| Expected direction | v0.2 逃生率高于 v0.1 baseline 的 25%；v0.2 有效局平均对局时长高于 v0.1 baseline 的 126.9 秒；修机完成率作为辅助解释，不作为单独成功判据。 |
| Risk | 终局窗口延长可能让修完机后的逃生转化过高；地图活动空间少会放大终局压力，但本 Patch 只处理可回滚的小数值窗口，不把地图丰富程度写成独立成功指标。 |
| Rollback plan | 将本 Patch 实现时确认的 endgame countdown / gate escape window / endgame timer 字段恢复到 v0.1 baseline 值；PatchRollback 只在具体字段和值获批后补入真实回滚内容。 |
| Validation method | 使用 `baseline_v0.1_log.csv` 作为 v0.1 对照；v0.2 使用同一异常规则，少于 90 秒的对局标记为 `is_abnormal=TRUE` 并从核心指标计算中剔除；记录逃生率、对局时长、修机完成率、首倒时间、开门后未撤离情况和异常原因。 |
| Decision rule | 若 v0.2 逃生率高于 v0.1 baseline，且有效局平均对局时长高于 v0.1 baseline，同时修机完成率没有成为唯一解释来源，则该方向可进入保留候选；若逃生率没有上升或对局时长没有增加，则该 Patch 方向不成立。 |
