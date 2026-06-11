# PATCH-012-02 A/B Hypothesis

| Field | Content |
| --- | --- |
| Patch ID | PATCH-012-02 |
| Patch name | 扩大救援反制窗口 |
| Baseline problem | v0.1 本地双控测试中观察到，Survivor 进入救援交互时，椅上淘汰倒计时仍继续减少，该现象可能压缩救援完成机会。 |
| Baseline evidence | v0.1 baseline commit: `c3c930f9e71aa25ecc406b9c8f506b9507605ab1`；baseline 文档 commit: `b60f5200839e7fcecd9e2835b2ffbf2b414d0e71`；有效局平均对局时长约 126.9 秒；逃生率为 4 名 Survivor 逃生 / 16 个 Survivor 席位 = 25%；救援成功率样本不足，仅作辅助观察。 |
| Hypothesis | 如果扩大救援反制窗口，则 Hunter 不能过快通过挂椅流程结束对局，对局时长应增加，救援完成机会应增加。 |
| Target metric | 对局时长；逃生率；救援观察记录。 |
| Guardrail metric | 首倒时间；修机完成率；异常局数量。 |
| Planned change | 可选实现方向包括降低救援交互时长、延长椅上淘汰时间、或实现救援中倒计时暂停。正式实现前必须选择一个最小、可回滚的改动，不在本 A/B 文档中写死具体代码实现或新数值。 |
| Expected direction | v0.2 有效局平均对局时长高于 v0.1 baseline 的 126.9 秒；救援完成机会在观察记录中增加；逃生率可作为方向性参考，但救援成功率样本不足，不能作为唯一成功判据。 |
| Risk | 如果救援窗口增加过多，Hunter 可能难以通过挂椅流程制造淘汰压力；如果只看救援成功率，可能因样本不足得到不可靠结论。 |
| Rollback plan | 将本 Patch 实现时确认的救援交互、椅上淘汰倒计时或救援中倒计时机制恢复到 v0.1 baseline 行为；PatchRollback 只在具体字段、机制和值获批后补入真实回滚内容。 |
| Validation method | 使用 `baseline_v0.1_log.csv` 作为 v0.1 对照；v0.2 使用同一异常规则，少于 90 秒的对局标记为 `is_abnormal=TRUE` 并从核心指标计算中剔除；记录对局时长、逃生率、救援观察记录、首倒时间和异常原因。 |
| Decision rule | 若 v0.2 有效局平均对局时长高于 v0.1 baseline，且救援观察记录显示救援完成机会增加，同时首倒时间和修机完成率没有出现反向极端变化，则该方向可进入保留候选；若对局时长没有增加，或救援观察无法支持窗口变化，则该 Patch 方向不成立。 |
