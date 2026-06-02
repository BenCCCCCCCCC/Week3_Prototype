Week 11｜埋点 & 指标
延续 Week 10 的任务系统与 7 天活动，本周搭建一套可测量的指标体系：5 个核心指标 + ≥ 20 条埋点 + A/B 假设模板。本周不做新玩法，做"判断玩法好不好"的度量层。
完成后必须能在 24 小时内回答以下三连问：这次改动看哪个指标？预期变多少？怎么判断成功/失败？
前置约束
1.	不新增 MatchStats 字段，除非 4. Unity 落地清单明确列出。本周只允许新增 2 个字段：firstDownTime、rescueAttemptCount，其余字段一律复用 Week 10 的 MatchStats.cs / MatchStatsManager.cs。
2.	所有埋点必须挂在已有 call site 上：CipherMachine.CompleteCipher、ChairController.RescueOccupant、MatchManager.OnSurvivorDowned 等。本周禁止为埋点新增 Update() 循环或新 MonoBehaviour，必须复用现有触发点。
3.	**双视角覆盖。**任何指标如果只能从 Survivor 或 Hunter 单方观察到，必须显式标注"另一方该看什么对偶指标"。
4.	禁用模糊语言。"对局太短"、"逃生率偏低"、"显著提升"、"明显改善"这类表述出现一处扣分。所有阈值必须是具体数字，所有"显著"必须给出绝对值或百分点。
5.	**异常局必须打标隔离。**Week 10 的 MatchSettlement.IsAbnormalMatch() 返回 true 的对局，所有埋点 payload 必须带 is_abnormal=true 字段，下游聚合默认剔除。
6.	**章节编号不允许跳号。**Week 10 学生作业 4 → 6 跳号的情况本周不再容忍。
具体任务
1. 五个核心指标定义（必交）
1.1 指标定义表
每条指标必须给出：数学定义 / 数据来源（具体字段名）/ 单局或多局粒度 / 健康-警戒-异常三档阈值。下表为标准模板，所有阈值学生可微调但必须保留三档结构。
指标	数学定义	数据来源（具体字段）	粒度	健康区间	警戒区间	异常区间
对局时长	MatchStats.surviveTime	MatchStatsManager.EndMatch 写入 Time.time - matchStartTime	单局	6–12 分钟	4–6 或 12–15 分钟	< 3 或 > 20 分钟
逃生率	sum(escaped) / total_match_count	MatchStats.escaped 多局聚合	多局，最小 50 局	35%–55%	25%–35% 或 55%–65%	< 20% 或 > 70%
首倒时间	第一次 AddDown 时的 Time.time - matchStartTime	需新增 MatchStats.firstDownTime	单局	60–180 秒	30–60 或 180–300 秒	< 30 秒，或 = 0（本局未发生倒地）
救援成功率	rescueCount / rescueAttemptCount	分子已有；需新增 MatchStats.rescueAttemptCount	多局，最小 30 局	50%–75%	35%–50% 或 75%–85%	< 30%
修机完成率	totalRepairProgress / 100	MatchStats.totalRepairProgress（已 Clamp 至 100）	单局	Survivor 视角 70%–100%；Hunter 视角 30%–60%	50%–70% / 60%–80%	< 30% 或 > 80%

1.2 双视角解读（必填，每条指标都必须填）
每条指标必须给出 Survivor 视角解读和 Hunter 视角解读各一句。范例：
•	对局时长——Survivor 低 = 早期被淘汰节奏过快；Hunter 高 = 找不到目标，地图过大或追击技能偏弱
•	逃生率——35%–55% 是设计师定义的对称区间，偏离任何一端都需平衡
•	首倒时间——< 30 秒说明 Hunter 初始压迫过强 或地图刷新点距 Hunter 过近，需对应检查地图配置
•	救援成功率——低 = 救援机制本身风险过高 或 Hunter 蹲椅战术效益过高（对应 Week 10 滥用场景 B2）
•	修机完成率——Survivor 70%–100% 是设计预期；Hunter 视角如果读到 Survivor > 80% 但自己胜率仍达标，说明终局阶段 Hunter 翻盘机制有效
1.3 单局打表与多局聚合区分
指标	支持单局打表	多局聚合最小样本量
对局时长	✅	即时
首倒时间	✅	即时
修机完成率	✅	即时
逃生率	❌	≥ 50 局
救援成功率	❌	≥ 30 局

2. 埋点表（≥ 20 条，必交）
2.1 埋点字段规范
每条埋点必须包含以下 8 个字段，缺一视为该条不计入 20 条总数：
字段	说明	范例
event_name	蛇形命名，全小写，动词在后	machine_repair_complete
触发 call site	文件 : 方法	CipherMachine.cs : CompleteCipher()
触发时机	严格时序，不允许写"完成时"	progress01 从 < 1f 跨越到 = 1f 那一帧
必填参数	≥ 3 个，至少含一个可关联 ID	match_id, machine_id, repair_seconds
可选参数	加分项	loadout_multiplier, repairer_count
关联指标	引用 1.1 节某条指标	修机完成率
采样率	默认 100%，高频事件可降采	100%
受 is_abnormal 隔离	异常局是否进入聚合	是

2.2 完整埋点表（20 条最低，超出加分）
按以下五组分布，每组必须达到指定下限：
A 组 · 对局生命周期（≥ 3 条）
event_name	触发 call site	必填参数	关联指标
match_start	MatchStatsManager.cs : StartMatch()	match_id, map_name, hunter_player_id, survivor_player_ids[]	对局时长
match_end	MatchStatsManager.cs : EndMatch()	match_id, duration_seconds, escaped_count, eliminated_count	对局时长、逃生率
match_abnormal_flagged	MatchSettlement.cs : SettleMatch() 中 abnormalMatch 判定为 true 时	match_id, abnormal_reason	数据质量

B 组 · 修机事件（≥ 4 条）
event_name	触发 call site	必填参数	关联指标
machine_repair_start	CipherMachine.cs : BeginRepair() 第一次添加 repairer 时	match_id, machine_id, repairer_id	修机完成率
machine_repair_progress_25 / _50 / _75	CipherMachine.cs : Update() 中 progress01 跨越阈值的那一帧	match_id, machine_id, progress_percent, repairer_count, elapsed_seconds	修机完成率
machine_repair_complete	CipherMachine.cs : CompleteCipher()	match_id, machine_id, repair_seconds, max_repairer_count	修机完成率
machine_repair_interrupt	CipherMachine.cs : EndRepair() 中 activeRepairers.Count == 0 && !isCompleted	match_id, machine_id, progress_at_interrupt	修机完成率

C 组 · 救援与下椅事件（≥ 5 条）
event_name	触发 call site	必填参数	关联指标
survivor_chaired	ChairController.cs : PlaceSurvivor() 成功 occupy 时	match_id, chair_id, survivor_id, hunter_id, time_since_match_start	首倒时间、救援成功率分母
rescue_attempt_start	RescueAutoTest.cs 首次 currentChair = targetChair / InteractionUI 救援开始处	match_id, chair_id, rescuer_id, target_id	救援成功率（分母）
rescue_attempt_complete	ChairController.cs : RescueOccupant() 返回 true	match_id, chair_id, rescuer_id, target_id, rescue_duration_seconds	救援成功率（分子）
rescue_attempt_interrupt	RescueAutoTest.cs : ForceInterruptAutoRescue()	match_id, chair_id, rescuer_id, target_id, progress_at_interrupt, interrupt_reason	救援成功率（分母 +1，分子不 +）
survivor_eliminated	ChairController.cs : EliminateOccupant()	match_id, chair_id, survivor_id, hunter_id, chair_duration_at_elim	逃生率反指标

D 组 · Hunter 主动行为（≥ 4 条）
event_name	触发 call site	必填参数	关联指标
hunter_hit_landed	HunterBasicAttack.cs 中 AddHunterHit() 同一行处	match_id, hunter_id, target_id, hit_position	Hunter 输出强度
survivor_downed	MatchManager.cs 中 AddDown() 同一行处	match_id, hunter_id, survivor_id, time_since_match_start, is_first_down	首倒时间（仅 is_first_down=true 那一次写入 MatchStats.firstDownTime）
survivor_carried	HunterCarryController.cspicking up 时	match_id, hunter_id, survivor_id	Hunter 巡场效率
hunter_skill_used	HunterSlowSkill.cs / HunterDetectSkill.cs 技能触发处	match_id, hunter_id, skill_name, position	技能使用频次

E 组 · 终局与结算（≥ 4 条）
event_name	触发 call site	必填参数	关联指标
gate_opened	MatchManager.cs 中 AddGateOpen() 同一行处	match_id, gate_id, opener_id, time_since_match_start	逃生率
survivor_escaped	EscapeZone.cs 中玩家进入 trigger 处	match_id, survivor_id, time_since_match_start	逃生率（分子）
task_reward_granted	MatchSettlement.cs : SettleMatch() 中 [TaskReward] 日志同一处	match_id, player_id, task_id, soft_amount, material_amount	任务系统健康度
settlement_complete	MatchSettlement.cs : SettleMatch() 末尾	match_id, total_soft, total_material, total_premium, task_count_completed, is_abnormal, trigger_source	经济节奏

2.3 埋点输出格式规范
所有埋点统一通过 Console 打印，格式严格如下：


[Telemetry] event=<event_name> | match_id=<id> | <key1>=<v1> | <key2>=<v2> | ... | ts=<unix_seconds>
硬性规则：
•	每个参数 key=value 用 |（前后各一空格）分隔
•	不允许把所有参数挤进一个字符串字段
•	不允许打印 C# 对象的 ToString()
•	不允许换行
•	时间戳 ts 用 (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
下周做聚合时需要 grep ^\[Telemetry\] 一行一条解析，任何不符合格式的埋点都会被丢弃。
3. A/B 假设模板（必交）
3.1 模板字段规范
必填 12 个字段，缺一不可：
字段	类型	说明
实验 ID	字符串 EXP-NNN	唯一
实验名	中文短句	一句话说清改什么
观察到的问题	数据描述 + 具体数值	不允许写"逃生率有点低"
假设	因果陈述	"如果 X，则 Y 将变化 Z"
改动内容	具体配置	文件 : 字段，旧值 → 新值
影响的埋点	引用 2.2 节	≥ 1 条
影响的核心指标	引用 1.1 节，含主指标和护栏指标	≥ 2 条
预期变化（主指标）	具体数值	"+10pp"，不允许写"显著提升"
预期变化（护栏指标）	具体数值	另一方不能崩坏
判定标准 - 成功	数值阈值	主指标 ≥ X 且 护栏指标变动 ≤ Y
判定标准 - 失败	数值阈值	主指标 ≤ X 或 护栏指标变动 ≥ Y
样本量 / 持续时间 / 分流方式	具体配置	不允许留空

3.2 完整范例（必交，至少 1 个）


实验 ID：       EXP-001
实验名：        救援硬直延长测试
观察到的问题：   周常 W-S-01 完成率本周 32%，低于设计目标 50%；
              近 100 局救援成功率 38%，处于警戒区间 35%–50%。
假设：          若救援硬直时间从 2.5 秒提升至 3.5 秒，Hunter 难以连续阻断救援，
              救援成功率将上升至 48%–55% 区间。
改动内容：      InteractionStatsSO.rescueHoldSeconds：2.5 → 3.5
影响的埋点：    rescue_attempt_complete、rescue_attempt_interrupt、survivor_eliminated
影响的核心指标： 救援成功率（主）、Hunter 单局击杀数（护栏）、逃生率（次）
预期变化(主)：   救援成功率 +10pp（38% → 48%）
预期变化(护栏)： Hunter 平均击杀数 -0.3（2.1 → 1.8）；逃生率 +5pp（40% → 45%）
判定 - 成功：    救援成功率提升 ≥ 8pp 且 Hunter 平均击杀数下降 ≤ 0.5，样本 ≥ 200 局
判定 - 失败：    救援成功率提升 < 3pp 或 Hunter 平均击杀数下降 > 0.8
判定 - 待定：    以上之间，扩样本至 400 局继续观察
最小样本量：     对照组 200 局 + 实验组 200 局
持续时间：       14 天
分流方式：       玩家档案 ID 末位奇偶（0/2/4/6/8 → 对照；1/3/5/7/9 → 实验）
4. Unity 落地（最低实现）
模块	最低要求	挂接文件
埋点工具类	新增 TelemetryLogger.cs，提供 static void Emit(string eventName, Dictionary<string, object> payload)，输出严格按 2.3 节格式	新增 Assets/Scripts/Telemetry/TelemetryLogger.cs
对局生命周期埋点	A 组 3 条全部接入	MatchStatsManager.cs / MatchSettlement.cs
修机埋点	B 组至少接入 machine_repair_start 与 machine_repair_complete 两条	CipherMachine.cs
救援埋点	C 组至少接入 survivor_chaired 与 rescue_attempt_complete 两条	ChairController.cs / RescueAutoTest.cs
firstDownTime 字段	新增到 MatchStats.cs；在 MatchManager.cs 中 AddDown 第一次调用时写入	MatchStats.cs / MatchManager.cs
rescueAttemptCount字段	新增到 MatchStats.cs；在 rescue_attempt_start 埋点同一处 ++	MatchStats.cs / MatchStatsManager.cs
五指标本地打印	F8 调试键（与 Week 10 的 F9 区分）触发 PrintMatchMetrics()，按 1.1 节五指标格式打印	MatchSettlement.cs
trigger_source 字段	settlement_complete 埋点 payload 必须带 trigger_source ∈ {natural, debug_f9, abnormal_match}；F9 强制结算来源标记为 debug_f9	MatchSettlement.cs

不要求把数据真发到服务器或文件，本周全部 Console 打印即可。但格式必须严格符合 2.3 节，否则下周做聚合无法解析。
5. 数据陷阱与滥用点分析（必交）
延续 Week 10 的双视角分析框架，本周专门看"指标本身可被污染或被反向利用"的场景。
A 视角：数据被污染（≥ 3 场景）
每个场景必须包含：场景描述 / 污染机制 / 对哪条指标产生偏差 / 修补方案 / 修补代价。
范例 A1：F9 强制结算污染对局时长
•	污染机制：F9 触发时 matchStartTime 可能刚开始几秒，但 settlement_complete 事件已被打出，与正常结束局难以区分
•	偏差指标：对局时长（系统性偏低）、修机完成率（系统性偏低）、settlement_complete 计数
•	修补方案：埋点 payload 增加 trigger_source 字段（已在 4. 节列出），值域 {natural, debug_f9, abnormal_match}；聚合时默认剔除 debug_f9
•	修补代价：所有 SettleMatch 调用处需要传入来源参数，约 3-5 处 call site 改动
另外至少 2 个，自行补充。建议方向：自动测试脚本污染、AFK 局对首倒时间的拖偏、Loadout 修机加速对 max_repairer_count 的统计扭曲。
B 视角：指标被反向利用（≥ 3 场景）
范例 B1：逃生率指标驱动数值膨胀
•	反向引导机制：设计师只看"逃生率 35%–55%健康区间"，每次偏离就调救援硬直/Hunter 攻速，可能陷入"补丁追逐"循环，对局长度持续被压缩
•	偏差指标：逃生率本身稳住，但对局时长持续下降，长尾用户体验恶化
•	修补方案：A/B 假设模板强制要求至少 1 条护栏指标；逃生率改动审批时必须同时检查对局时长是否偏离基线 ≥ 1 分钟
•	修补代价：A/B 流程审批多 1 步，可能延缓快速迭代
另外至少 2 个，自行补充。建议方向：救援成功率被工作室刷救援场污染、修机完成率被 Loadout 优势装备扭曲、首倒时间被新手保护机制扭曲。
总计 ≥ 6 场景。
6. 文档-实现一致性自检表（必交）
延续 Week 9 / Week 10 的自检要求，本周专门检查"埋点表里写的"和"代码里跑的"是否对齐。所有 ≥ 20 条埋点必须全部列出，包括只在文档定义、未在代码触发的。
6.1 埋点自检（覆盖全部 20+ 条）
event_name	文档定义位置	代码触发状态	偏差类型	偏差说明
match_start	A 组第 1 条	✅ MatchStatsManager.StartMatch 已 emit	无	—
machine_repair_complete	B 组第 3 条	✅ CipherMachine.CompleteCipher已 emit	无	—
hunter_skill_used	D 组第 4 条	❌ 仅文档定义未实现	仅文档	Week 11 时间不足，挂接点已找好但未接入
...	...	...	...	...

6.2 指标自检（5 条全部覆盖）
指标	文档定义	数据可采集状态	缺什么
对局时长	1.1 第 1 条	✅ 立即可采（surviveTime 已存在）	—
逃生率	1.1 第 2 条	⚠️ 需多局聚合层	Week 12 引入持久化存储后才可计算
首倒时间	1.1 第 3 条	⚠️ 需新增字段	MatchStats.firstDownTime + MatchManager.AddDown 处的写入逻辑
救援成功率	1.1 第 4 条	⚠️ 需新增字段	MatchStats.rescueAttemptCount + rescue_attempt_start 处的 ++
修机完成率	1.1 第 5 条	✅ 立即可采	—

只在文档定义、未在代码实现的埋点和指标必须列出，不允许隐藏。
交付物清单
提交时必须包含以下全部 5 项，缺一即视为未完成：
1.	《指标与埋点表》 Excel 或 Word，包含：
◦	五核心指标定义表（含数学定义、数据来源、三档阈值、双视角解读）
◦	≥ 20 条埋点表（按 A/B/C/D/E 五组分布，每条含 8 字段）
◦	A/B 假设模板字段规范 + 至少 1 个完整范例
2.	数据陷阱分析文档（A 视角 ≥ 3 场景 + B 视角 ≥ 3 场景）
3.	Unity 工程包，包含：
◦	新增 TelemetryLogger.cs
◦	第 4 节"Unity 落地"清单中所有"最低要求"项的代码改动
◦	MatchStats 新增 firstDownTime / rescueAttemptCount 字段
◦	F8 调试键 PrintMatchMetrics() 实现
◦	settlement_complete 埋点带 trigger_source 字段
4.	文档-实现一致性自检表（涵盖全部 ≥ 20 条埋点 + 5 条指标）
5.	演示视频或 GIF（≤ 90 秒），展示：
◦	进入一局对局
◦	Console 至少出现 match_start / machine_repair_complete / rescue_attempt_complete / survivor_downed / match_end 五条 [Telemetry] 输出
◦	按 F8 打印五指标
◦	按 F9 触发 Week 10 结算（验证两者不冲突；同时验证 settlement_complete 的 trigger_source=debug_f9）

