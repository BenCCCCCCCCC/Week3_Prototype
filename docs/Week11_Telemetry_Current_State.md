# Week 11 Telemetry Current State

> Markdown extraction from `docs/Week11_Telemetry_Current_State.docx` for Week 12 reference consistency.


## 1. 五个核心指标定义


### 1.1 指标定义表

指标
数学定义
数据来源（具体字段）
粒度
健康区间
警戒区间
异常区间
对局时长
MatchStats.surviveTime
MatchStatsManager.EndMatch 写入 Time.time - matchStartTime
单局
6-12 分钟
4-6 分钟或 12-15 分钟
< 3 分钟或 > 20 分钟
逃生率
sum(escaped) / total_match_count
MatchStats.escaped 多局聚合
多局，最小 50 局
35%-55%
25%-35% 或 55%-65%
< 20% 或 > 70%
首倒时间
第一次 AddDown 时的 Time.time - matchStartTime
MatchStats.firstDownTime（默认 -1f；第一次倒地时写入真实秒数）
单局
60-180 秒
30-60 秒或 180-300 秒
< 30 秒；firstDownTime = -1 表示本局未发生倒地，单独归类，不进入均值
救援成功率
rescueCount / rescueAttemptCount
MatchStats.rescueCount / MatchStats.rescueAttemptCount
多局，最小 30 局
50%-75%
35%-50% 或 75%-85%
< 30%
修机完成率
totalRepairProgress / 100
MatchStats.totalRepairProgress（Clamp 至 100）
单局
Survivor 视角 70%-100%；Hunter 视角 30%-60%
Survivor 50%-70%；Hunter 60%-80%
< 30% 或 > 80%

### 1.2 双视角解读

指标
Survivor 视角解读
Hunter 视角解读
对偶指标/补充检查
对局时长
低于 4 分钟：进入救援或开门阶段前已失去主要胜利路径；高于 15 分钟：撤离推进效率不足。
低于 4 分钟：Hunter 压制窗口大于当前 Survivor 反制窗口；高于 15 分钟：Hunter 定位或拦截效率不足。
同时查看首倒时间、修机完成率、survivor_eliminated。
逃生率
sum(escaped) / total_match_count
低于 25%：Survivor 胜利路径成本高于当前救援/修机收益；高于 65%：撤离成本低于 Hunter 拦截成本。
低于 25%：Hunter 单局收益超过对称区间；高于 65%：Hunter 对修机、救援或开门的控制不足。
样本量 ≥ 50 局后判定，同时查看对局时长。
首倒时间
< 30 秒：开局信息、出生距离或转点空间不足；> 300 秒：Hunter 首追定位成本高。
< 30 秒：开局压迫强于设计目标；> 300 秒：侦查链或地图路线需要缩短。
firstDownTime = -1 表示本局未发生倒地，单独标记，不进入首倒时间均值。
救援成功率
< 30%：救援风险大于救援收益；> 85%：上椅阶段惩罚不足。
< 30%：守椅收益超过救援反制窗口；> 85%：二次阻断窗口不足。
样本量 ≥ 30 局后判定，同时查看 survivor_eliminated。
修机完成率
< 30%：主线目标推进失败；> 80% 且逃生率低于 35%：终局门区或救援链路阻断过强。
< 30%：早期干扰达到设计目标；> 80%：对多点修机干扰不足。
Survivor 与 Hunter 阈值分开解释。
补充逃生率方面：当前 Unity 还没有“持久化多局统计层”

### 1.3 单局打表与多局聚合区分

指标
支持单局打表
多局聚合最小样本量
备注
对局时长
是
即时
debug_f9 与 abnormal_match 需剔除。
首倒时间
是
即时
firstDownTime = -1 单列为未发生倒地；firstDownTime >= 0 才进入首倒时间统计。
修机完成率
是
即时
单局可由 totalRepairProgress / 100 展示。
逃生率
否
≥ 50 局
单局只展示 escaped_count / eliminated_count。
救援成功率
否
≥ 30 局
单局只展示 rescueCount / rescueAttemptCount。

## 2. 埋点表（官方 22 条）


### 2.1 埋点字段规范

字段
说明
范例
event_name
蛇形命名，全小写，动词在后
machine_repair_complete
触发 call site
文件 : 方法
CipherMachine.cs : CompleteCipher()
触发时机
严格时序，写到具体状态变化帧
progress01 从 < 1f 跨越到 = 1f 的那一帧
必填参数
≥ 3 个，至少含一个可关联 ID
match_id, machine_id, repair_seconds
可选参数
附加维度，不替代必填参数
loadout_multiplier, repairer_count
关联指标
引用第 1.1 节核心指标或数据质量项
修机完成率
采样率
默认 100%；阈值事件按跨越帧记录
100%
受 is_abnormal 隔离
异常局默认不进入聚合
是
统一输出格式：[Telemetry] event=<event_name> | match_id=<id> | <key1>=<v1> | <key2>=<v2> | ts=<unix_seconds>

### 2.2 完整埋点表

组别
event_name
触发 call site
触发时机
必填参数
可选参数
关联指标
采样率
is_abnormal 隔离
Unity 方面
说明
A 对局生命周期
match_start
MatchStatsManager.cs : StartMatch()
正式进入 gameplay run 并生成 match_id 后
match_id, map_name, hunter_player_id, survivor_player_ids[]
scene_name
对局时长
100%
是
已实现并验证
Console 已出现 match_start。
A 对局生命周期
match_end
MatchStatsManager.cs : EndMatch()
胜负条件触发且 EndMatch 写入统计时
match_id, duration_seconds, escaped_count, eliminated_count
end_reason
对局时长、逃生率
100%
是
已实现并验证
Console 已出现 match_end。
A 对局生命周期
match_abnormal_flagged
MatchSettlement.cs : SettleMatch()
IsAbnormalMatch() 为 true 时，同一结算流程内
match_id, abnormal_reason, trigger_source
duration_seconds
数据质量
100%
是
条件分支已实现
当前普通局 IsAbnormalMatch() 返回 false，因此演示中不触发。
B 修机事件
machine_repair_start
CipherMachine.cs : BeginRepair()
本局本机第一次有效 repairer 进入修机状态时
match_id, machine_id, repairer_id
repairer_count
修机完成率
100%
是
已实现并验证
已用 hasEmittedRepairStartTelemetry 防止同一机器刷屏。
B 修机事件
machine_repair_progress_25
CipherMachine.cs : Update()
progress01 从 < 0.25 跨越到 >= 0.25 的那一帧
match_id, machine_id, progress_percent, repairer_count, elapsed_seconds
loadout_multiplier
修机完成率
100%
是
已实现并验证
Console 已出现 progress_25。
B 修机事件
machine_repair_progress_50
CipherMachine.cs : Update()
progress01 从 < 0.50 跨越到 >= 0.50 的那一帧
match_id, machine_id, progress_percent, repairer_count, elapsed_seconds
loadout_multiplier
修机完成率
100%
是
已实现并验证
Console 已出现 progress_50。
B 修机事件
machine_repair_progress_75
CipherMachine.cs : Update()
progress01 从 < 0.75 跨越到 >= 0.75 的那一帧
match_id, machine_id, progress_percent, repairer_count, elapsed_seconds
loadout_multiplier
修机完成率
100%
是
已实现并验证
Console 已出现 progress_75。
B 修机事件
machine_repair_complete
CipherMachine.cs : CompleteCipher()
progress01 从 < 1f 跨越到 = 1f 后调用 CompleteCipher()
match_id, machine_id, repair_seconds, max_repairer_count
final_progress_percent
修机完成率
100%
是
已实现并验证
Console 已出现 machine_repair_complete。
B 修机事件
machine_repair_interrupt
CipherMachine.cs : EndRepair()
activeRepairers.Count 从 > 0 变为 0 且 isCompleted=false 的那一帧
match_id, machine_id, progress_at_interrupt, last_repairer_id
interrupt_reason
修机完成率
100%（文档定义；Unity 未接入）
是
仅文档定义，未实现
当前 EndRepair 存在 remove/re-add 帧间循环风险，尚未解决脚本冲突。
C 救援与下椅
survivor_chaired
ChairController.cs : PlaceSurvivor()
PlaceSurvivor 成功占用椅子并写入 occupant 后
match_id, chair_id, survivor_id, hunter_id, time_since_match_start
chair_count_for_survivor
首倒时间、救援成功率
100%
是
已实现并验证
Console 已出现 survivor_chaired。
C 救援与下椅
rescue_attempt_start
InteractionUI.cs / RescueAutoTest.cs
救援进度从 0 开始推进的第一帧
match_id, chair_id, rescuer_id, target_id
entry_type
救援成功率
100%
是
已实现并验证
Console 已出现 rescue_attempt_start。
C 救援与下椅
rescue_attempt_complete
ChairController.cs : RescueOccupant()
RescueOccupant() 返回 true 后
match_id, chair_id, rescuer_id, target_id, rescue_duration_seconds
chair_duration_before_rescue
救援成功率
100%
是
已实现并验证
Console 已出现 rescue_attempt_complete。
C 救援与下椅
rescue_attempt_interrupt
RescueAutoTest.cs : ForceInterruptAutoRescue()
救援进度 > 0 且被命中、离开范围或目标状态改变时
match_id, chair_id, rescuer_id, target_id, progress_at_interrupt, interrupt_reason
elapsed_seconds
救援成功率
100%
是
已实现并验证
Console 已出现 rescue_attempt_interrupt。
C 救援与下椅
survivor_eliminated
ChairController.cs : EliminateOccupant()
椅上计时达到淘汰阈值并完成淘汰状态写入时
match_id, chair_id, survivor_id, hunter_id, chair_duration_at_elim
chair_stage
逃生率反指标
100%
是
已实现并验证
Console 已出现 survivor_eliminated。
D Hunter 主动行为
hunter_hit_landed
HunterBasicAttack.cs : DoHitCheck()
命中判定成功并调用 AddHunterHit() 时
match_id, hunter_id, target_id, hit_position
target_state_before_hit
Hunter 输出强度
100%
是
已实现并验证
Console 已出现 hunter_hit_landed。
D Hunter 主动行为
survivor_downed
MatchManager.cs : OnSurvivorDowned()
Survivor 状态进入 downed 且 AddDown() 被调用时
match_id, hunter_id, survivor_id, time_since_match_start, is_first_down
damage_source
首倒时间
100%
是
已实现并验证
Console 已出现 survivor_downed，第一次写 firstDownTime。
D Hunter 主动行为
survivor_carried
HunterCarryController.cs : TryPickUpNearestDownedSurvivor()
StartCarry() 成功后，目标进入 carried 状态时
match_id, hunter_id, survivor_id
distance_to_nearest_chair
Hunter 巡场效率
100%
是
已实现并验证
Console 已出现 survivor_carried。
D Hunter 主动行为
hunter_skill_used
HunterSlowSkill.cs / HunterDetectSkill.cs
Q 或 F 技能被 accepted activation 接受时
match_id, hunter_id, skill_name, position
cooldown_remaining_before_use
技能使用频次
100%
是
已实现并验证
Console 已出现 skill_name=slow 与 skill_name=detect。
E 终局与结算
gate_opened
MatchManager.cs : AddGateOpen()
开门进度达到 100% 并调用 AddGateOpen() 时
match_id, gate_id, opener_id, time_since_match_start
open_duration_seconds
逃生率
100%（文档定义；Unity 未接入）
是
仅文档定义，未实现
当前 AddGateOpen() 处缺少稳定 gate_id 与 opener_id。后续需在开门交互 call site 保留 gate_id / opener_id 后再接入。
E 终局与结算
survivor_escaped
MatchManager.cs : OnSurvivorEscaped()
Survivor 进入 EscapeZone 且 escapedSurvivorSet 去重通过后
match_id, survivor_id, time_since_match_start
gate_id
逃生率
100%
是
已实现并验证
Console 已出现 survivor_escaped。
E 终局与结算
task_reward_granted
MatchSettlement.cs : SettleMatch()
[TaskReward] 日志同一处完成奖励累计时
match_id, player_id, task_id, soft_amount, material_amount
premium_amount
任务系统健康度
100%（文档定义；Unity 未接入）
是
仅文档定义，未实现
当前仅有普通 [TaskReward] 日志，缺少稳定 player_id，且不是统一 [Telemetry] 格式。后续需在 MatchSettlement.cs 的任务奖励累计处补充 player_id 后再接入。
E 终局与结算
settlement_complete
MatchSettlement.cs : SettleMatch()
奖励汇总与任务结算完成后的最后一处埋点
match_id, total_soft, total_material, total_premium, task_count_completed, is_abnormal, trigger_source
settlement_result
经济节奏
100%
是
已实现并验证
Console 已出现 trigger_source=natural 与 debug_f9。

### 2.3 埋点输出格式规范

所有埋点统一通过 Console 打印，格式严格如下：
[Telemetry] event=<event_name> | match_id=<id> | <key1>=<v1> | <key2>=<v2> | ts=<unix_seconds>
硬性规则：
· 每个参数 key=value 用 “ | ” 分隔，竖线前后各一空格。
· 不允许把所有参数挤进一个字符串字段。
· 不允许打印 C# 对象的 ToString()；位置字段使用数值数组或标量。
· 不允许换行；每条埋点必须是一行 [Telemetry]。
· ts 使用 (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds())。
· 下周聚合时按 ^\[Telemetry\] 一行一条解析；不符合格式的行不进入聚合。

### 2.4 当前 Unity 实现数量

类别
数量/状态
说明
官方埋点总数
22 条
严格对应 Week 11 A/B/C/D/E 五组。
已实现或条件分支已接入
19 条
包括 match_start、修机 25/50/75、救援完成/中断、Hunter 命中/技能、逃生、结算等。
仅文档定义未实现
3 条
machine_repair_interrupt、gate_opened、task_reward_granted。
Unity 最低落地清单
已覆盖
TelemetryLogger、F8、F9 trigger_source、firstDownTime、rescueAttemptCount、最低生命周期/修机/救援埋点均完成。

## 3. A/B 假设模板


### 3.1 模板字段规范

字段
类型
填写要求
实验 ID
字符串 EXP-NNN
唯一编号，不复用
实验名
中文短句
一句话说明改动对象
观察到的问题
数据描述 + 具体数值
用样本、基线和区间描述
假设
因果陈述
如果 X，则 Y 将变化 Z
改动内容
具体配置
文件 : 字段，旧值 -> 新值
影响的埋点
引用第 2.2 节
至少 1 条
影响的核心指标
引用第 1.1 节
含主指标和护栏指标，至少 2 条
预期变化（主指标）
具体数值
+10pp 或 -20 秒
预期变化（护栏指标）
具体数值
另一方指标变动不得超过阈值
判定标准 - 成功
数值阈值
主指标达到阈值且护栏指标不越线
判定标准 - 失败
数值阈值
主指标未达到阈值或护栏指标越线
样本量 / 持续时间 / 分流方式
具体配置
不得留空

### 3.2 完整范例

字段
内容
实验 ID
EXP-001
实验名
救援交互时长调整测试
观察到的问题
近 100 局救援成功率为 38%，处于 35%-50% 警戒区间；同一批对局 survivor_eliminated 占比为 62%。
假设
如果 InteractionStatsSO.rescueHoldSeconds 从 2.5 秒调整为 3.5 秒，则救援成功率从 38% 提升到 48%-55%。
改动内容
InteractionStatsSO.rescueHoldSeconds：2.5 -> 3.5。
影响的埋点
rescue_attempt_start、rescue_attempt_complete、rescue_attempt_interrupt、survivor_eliminated。
影响的核心指标
救援成功率（主指标）；逃生率、对局时长、Hunter 平均淘汰数（护栏指标）。
预期变化（主指标）
救援成功率 +10pp：38% -> 48%。
预期变化（护栏指标）
逃生率 +5pp：40% -> 45%；Hunter 平均淘汰数 -0.3：2.1 -> 1.8；对局时长 +45 秒以内。
判定标准 - 成功
救援成功率提升 ≥ 8pp，且 Hunter 平均淘汰数下降 ≤ 0.5，且对局时长增加 ≤ 60 秒，样本 ≥ 200 局/组。
判定标准 - 失败
救援成功率提升 < 3pp，或 Hunter 平均淘汰数下降 > 0.8，或对局时长增加 > 90 秒。
判定标准 - 待定
未触发成功或失败阈值时，扩样本到 400 局/组。
样本量 / 持续时间 / 分流方式
对照组 200 局 + 实验组 200 局；持续 14 天；玩家档案 ID 末位偶数进入对照组，末位奇数进入实验组。

## 4. Unity 落地清单与当前检查

模块
最低要求
挂接文件
当前状态
说明
埋点工具类
新增 TelemetryLogger.cs，static Emit，按 2.3 格式输出
Assets/Scripts/Telemetry/TelemetryLogger.cs
已完成
所有 Telemetry 行带 match_id、is_abnormal、ts。
对局生命周期埋点
A 组 3 条全部接入
MatchStatsManager.cs / MatchSettlement.cs
已完成
match_abnormal_flagged 为异常分支。
修机埋点
至少 machine_repair_start 与 machine_repair_complete；当前含 25/50/75
CipherMachine.cs
已完成
start 已防重复；progress 使用阈值跨越 latch。
救援埋点
至少 survivor_chaired 与 rescue_attempt_complete；当前含 start/interrupt/complete
ChairController.cs / RescueAutoTest.cs / InteractionUI.cs
已完成
Console 已验证 rescue_attempt_complete 与 interrupt。
firstDownTime 字段
新增到 MatchStats.cs，默认 -1f，第一次 AddDown 写入
MatchStats.cs / MatchManager.cs
已完成
survivor_downed 输出 is_first_down；未发生倒地时 firstDownTime 保持 -1f。
rescueAttemptCount 字段
新增到 MatchStats.cs，rescue_attempt_start 同处 ++
MatchStats.cs / MatchStatsManager.cs
已完成
后续可进一步做跨手动/自动统一去重审计。
五指标本地打印
F8 触发 PrintMatchMetrics()
MatchSettlement.cs
已完成
已输出 5 行 [Metrics]。
trigger_source 字段
settlement_complete 带 natural/debug_f9/abnormal_match
MatchSettlement.cs
已完成
F9 已验证 trigger_source=debug_f9。

## 5. 数据陷阱与滥用点分析


### 5.1 A 视角：数据被污染（4 场景）

编号/场景
场景描述
污染机制
偏差指标
修补方案
修补代价
A1 F9 强制结算污染
测试者按 F9 后立即产生 settlement_complete。
debug_f9 局会缩短 duration_seconds，并降低 totalRepairProgress。
对局时长、修机完成率、settlement_complete 计数。
payload 增加 trigger_source=debug_f9；聚合时剔除 debug_f9。
SettleMatch 调用处需要传入来源参数。
A2 自动修机脚本污染修机开始次数
SurvivorAutoRepairTest 与手动 E 输入在帧间反复 BeginRepair / EndRepair。
machine_repair_start 计数被放大，repair_seconds 出现伪会话。
修机完成率、machine_repair_start 计数、repair_seconds。
CipherMachine 使用 hasEmittedRepairStartTelemetry；只在 CompleteCipher 或 ResetCipherForNewMatch 重置。
每台密码机维护 1 个布尔 latch。
A3 AFK 局污染首倒时间
Survivor 或 Hunter 开局后 120 秒无有效移动。
firstDownTime = -1（未发生倒地）或首倒时间被拉长，不能代表正常追击链。
首倒时间、对局时长。
增加 afk_candidate 标记：主控角色连续 120 秒输入为 0 时标记。
需要记录最后输入时间。
A4 Loadout 修机加速污染修机效率
某个 Loadout 将 repair multiplier 从 1.00 提到 1.25。
repair_seconds 缩短，但不是地图或基础交互变好。
修机完成率、machine_repair_complete、repair_seconds。
machine_repair_complete 增加 loadout_multiplier；聚合时按 multiplier 分层。
需要把 Loadout 配置传入修机 payload。

### 5.2 B 视角：指标被反向利用（4 场景）

编号/场景
反向引导机制
偏差表现
偏差指标
修补方案
修补代价
B1 逃生率驱动数值追逐
只看逃生率 35%-55%，每次越界就改救援时长或 Hunter 攻击间隔。
逃生率回到区间，但对局时长减少 ≥ 60 秒。
逃生率、对局时长。
逃生率实验必须设置对局时长护栏：变动 ≤ 60 秒。
A/B 审批多检查 1 条护栏指标。
B2 刷救援场污染救援成功率
两个账号反复制造上椅与救援，增加 rescue_attempt_complete。
救援成功率上升，但不是正常对抗样本。
救援成功率、rescue_attempt_complete。
同一 player_pair 每 24 小时最多计入 5 次救援样本。
需要 player_pair 与每日去重逻辑。
B3 修机完成率被优势 Loadout 推高
玩家集中使用高修机倍率装备，使 totalRepairProgress 接近 100%。
修机完成率进入健康区间，但基础修机交互未达标。
修机完成率、repair_seconds。
修机数据按 loadout_multiplier 分层，默认报告 multiplier=1.00 基线样本。
需要在修机 payload 增加 loadout_multiplier。
B4 首倒时间被新手保护扭曲
新手保护令前 60 秒 Hunter 无法快速打出倒地。
首倒时间上升，但不是地图路线或追击技能导致。
首倒时间、survivor_downed。
payload 增加 protection_active；聚合时分保护组和非保护组。
需要在 MatchManager 写入保护状态。

## 6. 文档-实现一致性自检表


### 6.1 埋点自检（覆盖 22 条）

event_name
文档定义位置
代码触发状态
偏差类型
偏差说明
match_start
A 对局生命周期 第 1 条
已实现并验证
无
Console 已出现 match_start。
match_end
A 对局生命周期 第 2 条
已实现并验证
无
Console 已出现 match_end。
match_abnormal_flagged
A 对局生命周期 第 3 条
条件分支已实现
条件触发
当前普通局 IsAbnormalMatch() 返回 false，因此演示中不触发。
machine_repair_start
B 修机事件 第 4 条
已实现并验证
无
已用 hasEmittedRepairStartTelemetry 防止同一机器刷屏。
machine_repair_progress_25
B 修机事件 第 5 条
已实现并验证
无
Console 已出现 progress_25。
machine_repair_progress_50
B 修机事件 第 6 条
已实现并验证
无
Console 已出现 progress_50。
machine_repair_progress_75
B 修机事件 第 7 条
已实现并验证
无
Console 已出现 progress_75。
machine_repair_complete
B 修机事件 第 8 条
已实现并验证
无
Console 已出现 machine_repair_complete。
machine_repair_interrupt
B 修机事件 第 9 条
仅文档定义未实现
仅文档
当前 EndRepair 存在 remove/re-add 帧间循环风险，保留为后续补做。
survivor_chaired
C 救援与下椅 第 10 条
已实现并验证
无
Console 已出现 survivor_chaired。
rescue_attempt_start
C 救援与下椅 第 11 条
已实现并验证
无
Console 已出现 rescue_attempt_start。
rescue_attempt_complete
C 救援与下椅 第 12 条
已实现并验证
无
Console 已出现 rescue_attempt_complete。
rescue_attempt_interrupt
C 救援与下椅 第 13 条
已实现并验证
无
Console 已出现 rescue_attempt_interrupt。
survivor_eliminated
C 救援与下椅 第 14 条
已实现并验证
无
Console 已出现 survivor_eliminated。
hunter_hit_landed
D Hunter 主动行为 第 15 条
已实现并验证
无
Console 已出现 hunter_hit_landed。
survivor_downed
D Hunter 主动行为 第 16 条
已实现并验证
无
Console 已出现 survivor_downed，第一次写 firstDownTime。
survivor_carried
D Hunter 主动行为 第 17 条
已实现并验证
无
Console 已出现 survivor_carried。
hunter_skill_used
D Hunter 主动行为 第 18 条
已实现并验证
无
Console 已出现 skill_name=slow 与 skill_name=detect。
gate_opened
E 终局与结算 第 19 条
仅文档定义未实现
仅文档
当前 AddGateOpen 调用前缺稳定 opener_id，保留为后续补做。
survivor_escaped
E 终局与结算 第 20 条
已实现并验证
无
Console 已出现 survivor_escaped。
task_reward_granted
E 终局与结算 第 21 条
仅文档定义未实现
仅文档
目前仅有普通 [TaskReward] 日志，缺稳定 player_id。
settlement_complete
E 终局与结算 第 22 条
已实现并验证
无
Console 已出现 trigger_source=natural 与 debug_f9。

### 6.2 指标自检（5 条全部覆盖）

指标
文档定义
数据可采集状态
缺什么
说明
对局时长

### 1.1 第 1 条

立即可采
无
surviveTime 已由 EndMatch 写入；debug_f9 与 abnormal_match 聚合剔除。
逃生率

### 1.1 第 2 条

字段可采；可靠结论需 ≥ 50 局
多局聚合层
当前单局展示 escaped_count / eliminated_count；Week 12 引入持久化或日志解析后计算。
首倒时间

### 1.1 第 3 条

立即可采
无
MatchStats.firstDownTime 默认值为 -1f；第一次 survivor_downed 写入真实秒数；整局未发生倒地时保持 -1f。
救援成功率

### 1.1 第 4 条

字段可采；可靠结论需 ≥ 30 局
多局聚合层
rescueAttemptCount 已新增；单局可展示分子/分母，不直接判定整体平衡。
修机完成率

### 1.1 第 5 条

立即可采
无
totalRepairProgress 已存在；machine_repair_start 与 25/50/75/complete 已接入。

### 6.3 仅文档定义未实现项

优先级
未实现项
建议挂接位置
补做条件
P0
gate_opened
MatchManager.cs : AddGateOpen()
需在开门 call site 保留 gate_id 与 opener_id；当前 opener_id 在回调前丢失。
P0
task_reward_granted
MatchSettlement.cs : [TaskReward] 同一处
需要稳定 player_id；当前仅有普通 [TaskReward] 日志。
P1
machine_repair_interrupt
CipherMachine.cs : EndRepair()
需要区分真实中断与 remove/re-add 帧间循环，避免产生误导样本。

## 7. 交付物与视频检查清单


### 7.1 Word 文档检查

· 5 个核心指标定义已完成，包含数学定义、数据来源、粒度和健康/警戒/异常三档阈值。
· 22 条埋点表已完成，按 A/B/C/D/E 五组分布，每条包含 8 个必填字段。
· A/B 假设模板与 EXP-001 完整范例已完成。
· 数据陷阱 A 视角 4 个、B 视角 4 个已完成。
· 文档-实现一致性自检表已覆盖 22 条埋点与 5 条指标。

### 7.2 Unity 工程检查

· TelemetryLogger.cs 已新增，统一输出 [Telemetry] 单行日志。
· MatchStats.cs 仅新增 firstDownTime 与 rescueAttemptCount；firstDownTime 默认值为 -1f。
· F8 PrintMatchMetrics() 已实现，并输出 5 行 [Metrics]。
· F9 settlement_complete 已带 trigger_source=debug_f9。
· 提交工程时不包含 Assets/_Recovery、Week11_Exports、Editor.log 或临时导出文件。

### 7.3 演示视频检查（≤ 90 秒）

· 进入一局后 Console 出现 match_start。
· 修完至少一台密码机，Console 出现 machine_repair_complete。
· 完成一次救援，Console 出现 rescue_attempt_complete。
· Hunter 击倒一次 Survivor，Console 出现 survivor_downed。
· 结束一局，Console 出现 match_end。
· 按 F8 输出 5 行 [Metrics]。
· 按 F9 输出 settlement_complete，且 trigger_source=debug_f9。
