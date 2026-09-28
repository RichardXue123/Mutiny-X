# AI 高 Luck 非阻塞搜索（2026-09-28）

实现前建立的规格，现已补充实现与实际验收。用户授权：按推荐计划将搜索细化为可暂停分帧计算，全部候选完成后才执行；不减少采样、不改变评分/随机顺序。3 ms 默认预算及棋盘快照/失效重算为 Unity 性能扩展，不宣称原版时序完全相同。

后续回归修复：第 12 关落水尸体使旧快照不断失效，1000/9999 均可重启循环。现在仅校验存活角色的物理漂移；死亡条目仍保留本次快照位置。新增失效原因日志，专项扩展至 54/54，见 [EXT-AI-SNAPSHOT-02/03 修复与实测](AI_SNAPSHOT_CORPSE_FIX_20260928.md)。下列 37/37 和性能表为首次实施的历史记录，不替代此回归记录。

## 来源与边界

- 原版 `Team.as::advance` 每帧重复调用 `aiThink` 约 30 ms，合并后严格 `>` 取胜者；首次不要求正分，续行动要求正分，等待胜者镜头后执行。
- `Character.as::aiThink/randomThrows`、`Weapon.as::randomThrows` 及各专用武器规定采样顺序、数量、物理和评分。原始 AS2/pcode 不修改。
- 当前问题：`EvaluateDecisionSteps` 在整件武器之后才让出；`BuildSelfThrowSamples` 整批 50 条；完成时 `JsonUtility.ToJson + File.WriteAllText` 同步处理全部记录。
- 只改变调度、快照和记录存储。正式武器/回合/伤害入口、香蕉 20 px 与装备起点规则不改变。

| 规则 ID | 可观察行为及状态转换 | 原版来源 / 授权 | Unity 入口 | 验收用例 | 实际结果 |
| --- | --- | --- | --- | --- | --- |
| EXT-AI-SLICE-02 | 首次/续行动搜索在单样本及弹道 tick 边界可恢复；默认每帧约 3 ms，时间到后下一帧继续；普通、Anchor、Cannon、跳跃、海鸥、巫毒、箱体均走同一调度 | 用户授权；原版 Team/Character/Weapon 为行为基线，预算/粒度不同 | MutinyAIController 决策迭代器与共享预测步进 | 固定 seed 比较全部随机、候选及最终参数；高 Luck 10000/99999 测每帧搜索耗时且候选完整 | 已实现；15/15 分帧重放、旧实现 60/60 完整记录对比、高 Luck 实际续行动通过 |
| EXT-AI-SNAPSHOT-01 | 搜索固定角色物理/生命/Evilness/资格/库存、箱体、水位、宝箱最终位置；每次继续和执行前校验回合及棋盘；变更丢弃旧结果，下一次重新评估；纯显示/宝箱下落不使快照失效 | 用户授权；不冻结世界，不等待宝箱落地 | PrepareDecisionWork / 回合协程 | 真实生产输入改变库存/位置、销毁角色/箱体、GM 参数变化后旧结果不执行；换关/停用取消 | 已实现；库存/Luck 变化、停用、真实换回合、第 7 关重开通过；宝箱下落与各世界对象变更单独实战待验收 |
| EXT-AI-TRACE-02 | 逐步累计各行动最佳摘要；完整随机/候选记录分块序列化与后台写盘，完成搜索不整批序列化，不等最终文件完成才执行；文件完成后可按既有 JSON 格式重放；大记录内存有界，未完成搜索取消不发布完整 trace | Unity 调试扩展；ailog 原资格不变 | MutinyAIRandomStream / 决策 trace writer / LogCommittedAction | 保存大决策、解析和重放；关闭 ailog 不改变存储；各行动最高分一致；取消/IO 异常不卡死/不写错误完整结果 | 已实现；流式 JSON 解析/重放、高 Luck 完整保存、取消不发布、IO 失败解除背压通过 |
| AI-EXE-01/AI-SEL-03 回归 | 全部候选完成后才选角色和执行；跳跃落稳后新快照重新算武器；可 Pass；正式执行/库存消耗一次 | Team.as::advance/continueTurn | ExecuteAITurnRoutine / MutinyTurnManager | 生产整回合直接开火、跳后开火、跳后 Pass、GM 接管；中途取消不提交 | 实际生产协程通过直接海啸、跳后 Cherry Bomb、零 Luck 跳后 Pass、GM 接管与取消 |

## 实施及验收顺序

1. 保留一个生产算法；同步诊断入口只是不让出的同一迭代器，避免两套预测漂移。
2. 保存单次预测进度，分帧维持 RNG 调用顺序和同分先到先得。50 条跳跃先生成、后评分；巫毒先生成两个落点、再生成评分扰动。
3. 固定快照与显式取消、回合身份和执行前校验；不以思考时间作为自动 Pass 条件。
4. 回放流式写盘、摘要增量统计；仅后台处理已脱离 Unity 对象的字节/文件 IO。
5. 编译 + Unity 隔离 Play Mode 专项、GM/预测兼容回归、生产高 Luck 整协程性能记录。主工程画面与 Android 单独列为待验收。

## 验证记录

2026-09-28，Windows Unity 6000.6.0f1，独立空场景验证工程，以当前 Scripts/Resources 副本运行；未操作用户主工程正在运行的对局。Runtime / Editor 程序集编译通过。

- 专项 Play Mode **37/37**：15 种候选逐随机/候选重放；流式文件完整解析/重放；真实跳跃落稳后 GM 接管 `10000/99999`、全部搜索后一次正式扣弹；库存/Luck 失效、停用、Pass 换回合、直接海啸、零 Luck 续行动 Pass；取消文件、IO 失败、第 7 关实际重开。
- 兼容回归：预测 **18/18**、GM **66/66**、箱体摆放/桶/箱体镜头 **125/125**。
- 与修改前 `f3ffecd657182f5b729d5fead85966b0085ff261` 控制器在隔离工程直接比对：Luck `0/1/7/33` × 15 种武器，共 **60/60** 的完整 JSON（所有随机、候选、参数、分数、胜者）逐字相同。这是 Unity 实现前后对比，不是原版 Flash 运行对照。

| Luck | 续行动完整候选数 | 搜索分片数 | 实测最大搜索分片耗时 |
| --- | --- | --- | --- |
| 10000 | 10000 | 41 | 3.6949 ms |
| 99999 | 99999 | 414 | 5.3022 ms |

以上使用 Cherry Bomb 单武器、固定 seed、完整日志保存开启；没有降低候选数，随机数分别为 20000/199998 次。耗时是预算泵搜索分片，不包括快照、回合等待、镜头、渲染；headless 无帧率限制，不把其总 frameCount 当作用户等待时间或 FPS。证据摘要见 [实际验收记录](../../11-VerificationAndDebug/02-PlayModeValidation/Artifacts/AI-RESPONSIVE-20260928.txt)。

## 使用与代码职责

- 无需新命令；原生 AI、`aitakeover {luck}`、`aisetluck {luck}` 自动使用分帧搜索。`DecisionBudgetMilliseconds` 默认 3，可在 AI 控制器 Inspector 调整；预算无效时回退默认值。
- `MutinyAIController`：回合门、生成顺序、胜者、镜头、正式提交；`MutinyAISearchSteps`：各武器可恢复候选；`MutinyAIPrediction`：共享单 tick 预测；`MutinyAISearchState`：快照、失效、取消和预算泵；`MutinyAITraceWriter`：有界记录队列、后台 IO。
- `MutinyBoxPlacementRules`：正式箱体/桶和 AI 快照共用同一摆放公式，没有另一份评分或摆放算法。
- 生产搜索只保留计数和胜者/行动最高分，完整记录在 JSON 中；`LastDecisionTrace.Draws/Candidates` 不再保留全部生产样本。同步诊断与重放测试入口仍可保留数组。
- `ailog 1` 保持每次实际行动一条日志，文件状态为 `pending/ready/failed`；待文件完成后再重放。已完整求值的决策允许在换回合后完成保存，即使其镜头等待阶段被取消；未完成求值的旧任务不发布 JSON。
- 搜索中更改 Luck 或强制武器会使旧任务失效并重新搜索，避免提交旧设置结果；世界仍正常运行，不冻结宝箱。
- 回归入口：`Mutiny → Parity → Validate AI Responsive Search Play Mode`，仅在空场景运行；批处理 `Mutiny.Verification.Editor.MutinyAiResponsiveVerificationMenu.RunBatch` 仅用于隔离测试工程。

## 待运行与已知差异

- 主工程可视化对局、Android 真机帧率/内存/慢存储、全武器极值、多角色/死亡倍率、长期多关卡压力测试待验收；不把已通过的单武器高 Luck 用例推广为全部性能保证。
- 3 ms 为软预算，检查点在物理 tick/样本之间；工厂首次资源加载、单个 tick、评分与 GC 可能越过预算。已实测出现 5.3022 ms；不承诺所有帧严格小于 3 ms。
- 极高 Luck 总思考时间仍会增加。队列有界，慢磁盘产生背压时分帧等待可入队，不等待最终文件拼接/发布；不强制超时 Pass。
- `EvaluateBestMove()` 与显式加载完整重放 JSON 是同步诊断入口，不用于生产回合非阻塞路径；超大重放文件仍可能有解析尖峰。
- 不更换 Flash/Unity PRNG，不修改原版评分、武器模拟上限、同分优先与续行动正分门；仅调度、记录方式、快照失效是此次授权扩展。
