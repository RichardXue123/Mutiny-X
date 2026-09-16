# AI 决策流程（原版对照）

## 范围与证据

- 原版基线：`Mutiny Source/mutiny-flash-game/mutiny.swf`，指纹见 `REPLICATION_STANDARD.md`。
- 静态证据（S）：
  - `Docs/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/throwgame/Team.as`：`advance`、`startTurn`、`continueTurn`。
  - `Docs/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/throwgame/Character.as`：`randomThrows`、`aiStart`、`aiThink`。
  - `Docs/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/throwgame/Weapon.as`：`randomThrows`、`aiPerform`。
  - `Character.pcode`：确认移动评分中樱桃炸弹循环仍以移动落点而非炸弹落点计算距离。

本文件不以既有 C# 行为作为原版证据。随机序列的实现与 Flash 不同，因此本次验收比较候选集合、评分公式和状态转换，不要求逐次抽样相同。

## 规则

| ID | 原版可观察/状态规则 | Unity 入口 | 验收用例 | 状态 |
| --- | --- | --- | --- | --- |
| AI-SEL-01 | AI 回合开始时，对每名角色调用 `aiStart`；所有存活角色的移动和武器候选汇总后，严格以 `>` 取全局最高分，平分保留先出现的候选。得分最高者成为本回合行动角色。 | `MutinyAIController.EvaluateBestMove` | 两名敌方角色均可行动，日志应显示候选总数和最终角色；结果可来自任一角色而非预选角色。 | S 已确认；已实现；待 Unity 运行验证 |
| AI-SEL-02 | 新回合 `aiCanBailOut=false`，即使全体候选分数不大于 0，仍执行最高分候选。 | `MutinyAIController.EvaluateBestMove` | 无正分攻击且可跳跃时，AI 仍选择一次跳跃。 | S 已确认；已实现；生产入口用例待 Unity 执行 |
| AI-SEL-03 | 跳跃静止后 `continueTurn` 只允许刚跳跃的角色射击，禁止所有角色跳跃，并设 `aiCanBailOut=true`；此时最高分 `<=0` 结束回合。 | `MutinyAIController.EvaluateBestMove`、`MutinyTurnManager` 续回合入口 | AI 跳跃后没有正分武器候选，日志应记录续行动放弃并交给下一队。 | S 已确认；已实现；生产入口用例待 Unity 执行 |
| AI-MOVE-01 | 每名可跳跃角色随机模拟 50 次：角度 `180 + int(random*180)`，力度 `5 + random*15`，直至静止或落水。落水候选扣 2 分，不能直接过滤。 | `MutinyAIController.EvaluateCharacterSelfThrow` | 在水边并且大部分跳跃落水时，日志仍显示 50 个已评分候选。 | S 已确认；已实现；待 Unity 运行验证 |
| AI-MOVE-02 | 跳跃评分包含高低差、到敌军质心的横/纵距离变化、敌方距离的奖励及近身风险、己方距离惩罚、未完成宝箱 40px 内 `+0.5`，以及移动距离修正；没有“必须靠近 15px”、“离敌人不得超过 230px”或“敌人不得近于 70px”的拒绝条件。 | `MutinyAIController.EvaluateCharacterSelfThrow` | 角色离敌人较远且没有可直接命中的武器时，AI 仍产生可选移动候选。 | S 已确认；已实现；待 Unity 运行验证 |
| AI-MOVE-03 | 原版在角色有 `cherryBomb` 时额外抽样 10 次；反编译和 pcode 均表明该循环错误地用移动落点计算敌距。此已知原版特性单列复刻。 | `MutinyAIController.ScoreCherryBombFollowUpAtMoveLanding` | 有樱桃炸弹且移动落点接近敌人时，候选日志显示额外评分。 | S 已确认；已实现；待 Unity 运行验证 |
| AI-WPN-01 | 对库存中每一种不同名称的武器生成候选；抽样数为 `floor(luck * teamCharacterCount / teamAliveCount)`。通用武器从随机弹道落点评分：初值 -0.01，70px 内敌人奖励，40px 内友军扣分；每次敌人奖励后按该敌人的 `evilness` 放大。它们是评分项，不是硬性淘汰。 | `MutinyAIController.EvaluateCharacterWeapons`、`MutinyCharacter.Luck/Evilness` | 角色持有多种武器时，日志列出每种可执行武器与候选数；友军近处候选仍被评分。 | S 已确认；已实现；待 Unity 运行验证 |
| AI-WPN-02 | `anchor` 将通用武器评分乘 0.5；`piecesOfEight` 使用 `(score - 0.5) * 1.2`。 | `MutinyAIController.ScoreGenericWeaponCandidate` | 固定同一落点的评分检查。 | S 已确认；`piecesOfEight` 已实现，`anchor` 专用执行仍待 AI-WPN-03 |
| AI-WPN-03 | `tidalWave`、`voodooDoll`、`seagull`、`woodenCrate`/`gunpowderBarrel`、`cannon` 各自覆盖候选生成和执行参数，不能假定通用抛物线执行等价。 | `MutinyAIController`、各武器 `aiPerform` 对应实现 | 逐武器运行验证。 | S 已确认；部分待实现 |

## 本次实现边界

本次实现 AI-SEL-01..03、AI-MOVE-01..03、AI-WPN-01..02，并为可由 `MutinyWeaponFactory.SpawnAndFire` 正常生产的普通抛射武器建立统一候选；`tidalWave` 与 `voodooDoll` 已接入各自的候选和执行参数。`seagull`、箱类、`anchor` 与尚未注册的 `cannon` 仍需要其原版专用 `aiPerform` 参数；在生产执行入口完整接入前，本次不让 AI 把它们错误地当作普通弹道武器使用，保留 AI-WPN-03 为待实现。

## 回归记录

- BUG-AI-001：旧 Unity AI 只在 `dynamite` 或 `cherryBomb` 中二选一，且以 0 分为初始最佳分，导致其他武器、低分跳跃和首阶段负分动作均无法选中。
- BUG-AI-002：旧 Unity AI 用水面、自己/友军伤害半径、离敌阈值、最小前进距离等硬过滤替代原版评分，造成“移动后打不到人就几乎不移动”。

实际 Unity Play Mode 和与原版同输入的运行对照尚未执行。
