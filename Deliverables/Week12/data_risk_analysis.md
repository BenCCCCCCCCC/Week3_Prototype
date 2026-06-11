# Data Risk Analysis

## C Perspective - Baseline Method Limits

### 本地双控导致技能与协作行为低估

* Scenario: 本次 baseline 由同一名测试者本地双控完成，无法稳定模拟真实多人对抗中的协作、救援沟通、技能博弈和临场反应。
* Pollution mechanism: Survivor 技能、Hunter 技能、团队转点与救援配合的使用频率可能低于真实多人样本。
* Biased conclusion: 本次数据可以支持“v0.1 对局收束偏快”的判断，但不能单独证明“技能系统无效”。
* Mitigation: 在日志中明确标注“本地双控”；将技能使用不足写为测试限制；后续若有多人样本，需要重新验证技能使用频率和救援成功率。
* Cost: 本次 Week 12 只能将该问题作为数据限制记录，不能作为单独 Patch 成功判据。

---

## A Perspective - Patch Validation Noise

### A1 测试者熟练度影响 v0.2 对比

* Scenario: v0.1 baseline 与 v0.2 post-patch 都由同一名测试者完成。测试者在多轮测试后会更熟悉地图、出生点、密码机位置、椅子位置和开门路线。
* Pollution mechanism: 测试者熟练度提高会影响对局时长、首倒时间、修机完成率和逃生率。v0.2 中的指标变化可能部分来自学习效应，而不完全来自 Patch。
* Biased conclusion: 如果 v0.2 对局时长增加，可能被误判为 Patch 拉长了对局；如果 v0.2 首倒时间延后，可能被误判为 Hunter 压力被数值改动削弱。
* Mitigation: v0.2 测试继续使用“本地双控”标注；保留同一异常剔除规则；记录测试者熟练度限制；优先比较方向性变化，不把单局结果作为结论。
* Cost: 无法完全消除学习效应，需要在复盘中声明样本限制。

### A2 三条 Patch 同时进入 v0.2 导致归因混淆

* Scenario: Week 12 最终会将三条 PATCH-012 改动一起形成 v0.2，再跑 post-patch 测试。
* Pollution mechanism: 如果 v0.2 对局时长、首倒时间或逃生率发生变化，很难完全判断是某一条 Patch 单独造成，还是三条 Patch 叠加造成。
* Biased conclusion: 可能把整体指标变化错误归因到某一条 Patch。例如对局时长变长，可能来自 Hunter 移动数值变化，也可能来自救援窗口变化，或来自终局撤离窗口变化。
* Mitigation: 三条 Patch 文档中分别写清对应指标、预期方向和回滚字段；Patch Note 中标明改动间可能互相影响；如果某项指标变化无法单独归因，则写“组合改动影响”，不写成单一 Patch 结论。
* Cost: 本轮只能验证 v0.2 组合效果，不能证明每条 Patch 的独立效果。

---

## B Perspective - Process Misuse

### B1 为满足有效样本而人为拖延对局

* Reverse incentive: Week 12 要求单局时长少于 90 秒必须标异常，测试者可能为了让局数有效而刻意等待或拖延结算。
* Biased conclusion: 如果测试者只为了超过 90 秒而等待，对局时长会被拉长，但这不代表正常玩法下的节奏。
* Mitigation: 在日志中标注“本地双控”；保留所有少于 90 秒的异常局；有效局只用于最低 baseline 计算；在结论中说明本地测试存在人为节奏控制。
* Cost: 对局时长指标可用于判断 v0.1 是否偏短，但不能等同于真实多人对战时长。

### B2 用异常局问题直接决定 Patch

* Reverse incentive: v0.1 中有多局少于 90 秒的异常局，容易诱导测试者直接根据异常局做数值改动。
* Biased conclusion: 如果直接用异常局判断 Hunter 过强，可能忽略异常局本身已经被剔除，不应进入核心指标计算。
* Mitigation: 五核心指标只统计 `is_abnormal=FALSE` 的 8 局有效样本；异常局只作为现象记录；Patch 触发理由必须引用有效样本中的平均对局时长、首倒时间、修机完成率和逃生率。
* Cost: 异常局仍然能说明 v0.1 存在过快收束现象，但不能单独作为成功判据。
