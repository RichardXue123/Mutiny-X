# AI 逻辑实现进度与实现说明

> 基线日期：2026-09-25
> 范围：原版 Flash 的角色选择、是否移动、移动落点评价、空投价值、武器选择与 AI 专用执行；以及 Unity 当前实现的对应关系。  
> 结论口径：本文严格区分“静态确认”“已实现”“实际测试通过”“待运行验证”和“已知差异”。

## 1. 当前结论

Unity 已经具备可工作的 AI 主干：每个可行动角色生成“投掷自己”和“使用武器”候选，使用原版风格的分数挑出全队最佳候选，再经生产武器入口执行。移动评分中的高度、敌方中心、落水、敌我距离、空投和移动距离项都已接入；15 种库存武器现在都能进入普通或专用候选路径。Wooden Crate 与 Anchor 已按原版专用逻辑接入，不再被候选分发器排除。

当前仍需完整的关卡运行验收，主要边界是：

- 普通武器候选使用正式武器初始化后的物理状态，预测物理步进与正式对象共用 `MutinyPhysics.Step`；Parachute Bomb 的逐 tick 空气修正和顶部边界也已抽为正式/预测共用函数。候选仍使用无副作用的状态副本，专用武器的完整生命周期不在预测中播放。
- Unity 已按原版角色顺序、先移动后武器建立可恢复的候选迭代器，每帧约 30 ms 后暂停；原版单次 `aiThink()` 本身可能超过预算，Unity 的每件武器求值同样可能超过预算。
- AI 独立随机流可使用固定种子；每次随机抽样、候选和胜出结果可保存为 JSON 并逐项重放。它能复现同一 Unity 棋盘，不代表取得了原版 Flash 的 `Math.random()` 内部状态。
- 已有若干生产入口回归断言，但没有找到本基线下 AI 整回合 Play Mode 实际通过记录。

## 2. 证据与代码入口

### 2.1 原版静态证据

- `Team.as::advance/startTurn/continueTurn`：全队候选汇总、最高分选择、首次行动不得放弃、移动后的第二阶段可以放弃、镜头等待和最终执行。
- `Character.as::randomThrows/aiStart/aiThink`：50 次移动采样、移动评分、空投评分、武器遍历、普通武器评分与专用武器分支。
- `Weapon.as::randomThrows/aiPerform`：普通武器随机投掷与执行。
- `BoxWeapon.as`、`Cannon.as`、`Seagull.as`、`TidalWave.as`、`VoodooDoll.as`、`Anchor.as`、`PiecesOfEight.as`：专用 AI 模拟或执行。
- `TreasureChest.as`：空投生成、落地、拾取和完成状态；AI 只读取宝箱位置与完成状态，不决定是否生成空投。

证据根目录：`Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/throwgame/`。

### 2.2 Unity 生产入口

- 总决策与执行：`Assets/Mutiny/Scripts/Simulation/MutinyAIController.cs`
- 队伍与两阶段行动：`MutinyTeam.cs`、`MutinyTurnManager.cs`
- 共用弹道：`MutinyPhysics.cs`、`MutinyPhysicsBody.cs`
- 武器创建与正式效果：`MutinyWeaponFactory.cs` 及各 `Mutiny*.cs` 武器类
- 空投世界状态：`MutinyTreasureChestManager.cs`、`MutinyTreasureChest.cs`
- 当前回归入口：`Assets/Mutiny/Scripts/Verification/MutinyTurnActionUiVerificationTest.cs`

## 3. AI 一回合的总体流程

### 3.1 原版行为

1. 回合开始时，队内每个角色执行 `aiStart()`，清空上次候选和思考进度。
2. `Team.advance()` 每帧最多给当前角色约 30 ms 的 `aiThink()` 预算；一个角色完成后再处理下一个。
3. 每个存活且有资格的角色生成：
   - 50 个 Throw Self 候选；
   - 每种不重复库存武器的候选。
4. 队伍合并所有角色的候选，以严格的 `>` 选择最高 `success`；同分时先出现的候选获胜。
5. 首次行动 `aiCanBailOut=false`，即使最佳分不大于 0 也必须执行；移动后的续行动 `aiCanBailOut=true`，只有最佳武器分大于 0 才开火，否则结束本回合。
6. 选中候选角色，镜头平移到该角色；镜头到达后才执行 Throw Self 或武器的 `aiPerform`。
7. Throw Self 只消耗 `canThrow`，保留 `canShoot`，因此角色落稳后可能进入第二阶段再使用武器。

### 3.2 Unity 当前行为

`ExecuteAITurnRoutine()` 进入 `IsEvaluatingCandidates=true` 后等待回合可执行门，逐步推进 `EvaluateDecisionSteps()`；每帧给重复求值约 30 ms，超预算则下一帧继续。移动的 50 条原始弹道会在角色的首次步骤中成批生成，再逐条评分；每件武器是另一个步骤，保持原版调用粒度。候选求值期间，镜头会像原版一样在 speech/popup 后提前返回。选出候选后调用 `MutinyTeam.SelectCharacter()`，用胜出角色覆盖镜头目标，并等平移目标清空后执行。

`EvaluateBestMove()` 已实现两个阶段：

- 第一阶段：按队伍角色顺序，先比较该角色的 50 个移动候选，再按库存顺序逐件武器比较；初始最佳分为负无穷，所以非正分候选仍可获选。
- 第二阶段：若已有选中角色，且 `CanThrow=false、CanShoot=true`，只计算该角色的武器；最佳分不大于 0 时返回 Pass。

`ResolveTurnGate()` 将门分为：

- `Execute`：仍是本 AI 队且阶段为 `TurnActive`；
- `Wait`：仍是本队，但正处于 `ActionExecuting` 等暂态；
- `Cancel`：回合已换队、队伍不再由 AI 控制、队伍战败或游戏未开始/已结束。

### 3.3 当前时序差异

原版是“每帧最多重复调用当前角色的 `aiThink()` 约 30 ms → 完成思考 → 选人 → 镜头移到选中的人 → 执行”。Unity 现有分帧协程遵循这一粒度。单次 50 条弹道生成或单件复杂武器求值本身仍可能超过 30 ms；原版也不在一次 `aiThink()` 中途抢占。尚需用复杂关卡运行日志测量实际帧耗时。

## 4. 如何判断是否移动

AI 没有独立的“先判断要不要移动”布尔开关。正确模型是：把 Throw Self 当成一种候选行动，与所有武器候选放在同一个分数池中竞争。

### 4.1 移动候选如何生成

对每个 `CanThrow=true` 的存活角色固定生成 50 次随机投掷：

- 角度：`180 + random(0..179)` 度；
- 力度：`[5, 20)`；
- 以角色正式物理参数模拟到静止、入水或预测上限，得到落点 `(ex, ey)`；
- Unity 将候选速度和分数写入 `AIMove`，类型为 `SelfThrow`。

### 4.2 移动落点如何评分

设起点为 `(x,y)`、落点为 `(ex,ey)`、敌方全体角色的坐标中心为 `(cx,cy)`。原版及 Unity 当前实现包含以下项：

1. **高度收益**：`-(ey-y) * 0.003`。游戏像素坐标向下为正，因此向上移动会加分。
2. **靠近敌方中心**：横向和纵向分别比较移动前后的绝对距离，每轴除以 `-500`；靠近加分，远离扣分。
3. **落水风险**：落点达到或低于水面时 `-2`。
4. **与每个存活敌人的距离**：
   - 距离 `<200` 时给予至多 `0.2` 的接近收益；
   - 距离 `<100` 时收益会再次衰减，并额外加入最高 `-3` 的贴身惩罚，避免直接跳到敌人身上；
   - 距离 `>=200` 时用 `0.3 * 0.75^(distance/200)` 给出远距离接近收益；
   - 每处理一个敌人后，以 `1 + enemy.Evilness` 放大当前分数。
5. **己方避让**：落在自己 80 px 内最多 `-1`；落在其他队友 40 px 内最多 `-0.1`。
6. **空投价值**：落点严格位于未完成宝箱落地点 40 px 内时，每个宝箱 `+0.5`。
7. **Cherry Bomb 后续价值**：拥有 Cherry Bomb 时，原版会做 10 次后续采样；但源码实际反复用“移动落点”而不是 Cherry Bomb 模拟落点评分。Unity保留了这个源码怪癖，只累加正收益。
8. **移动距离项**：移动距离 `<300` 时为 `0.3 * distance / 300 - 0.3`；大于等于 300 时固定 `-0.3`。这会惩罚几乎原地不动，也会惩罚过远移动，偏好接近但略小于 300 px 的有效位移。

### 4.3 最终怎样决定移动

- 第一阶段：若最高分候选是 `SelfThrow`，AI 就移动；若某个武器候选分更高，则直接开火。
- 原版和 Unity 当前实现都允许第一阶段执行非正分的“最不坏”行动。
- 移动落稳后，仍有 `CanShoot`，AI 只为同一角色重新评估武器；此时只有正分武器才使用，否则 Pass。

这意味着“移动”不是固定优先，也不是只有为了捡空投才发生；它可能因为占高地、靠近合适距离、避开贴身敌人、接近宝箱或为 Cherry Bomb 创造后续位置而胜出。

## 5. 对空投的判断

必须区分两个职责：

- **空投是否生成**由 `MutinyTreasureChestManager.TryDropNew()` 和回合切换流程负责，不由 AI 决策。当前每次换队时尝试一次，最多同时存在 3 个宝箱；随机抽一个有效列，若该列被角色、宝箱或箱体阻挡，本次直接失败，不重抽。
- **AI 是否值得靠近空投**只体现在移动评分中。对每个 `IsFinished=false` 的宝箱，若模拟落点到 `(PixelX, FloorPixelY)` 的欧氏距离严格 `<40 px`，加 `0.5`；恰好 40 px 不加分。

当前实现会评估所有未完成宝箱，包括仍在下降但已经具有 `FloorPixelY` 的宝箱。`ResolveTurnGate()` 只检查回合阶段，没有等待宝箱落地的门；因此 AI 可以在宝箱下降期间按最终落地点完成决策。镜头在 `IsEvaluatingCandidates=true` 时与原版一样先返回，不跟随下降宝箱；候选完成并建立胜出角色镜头目标后才恢复后续运镜优先级。

目前 AI 不会：

- 根据宝箱内具体武器调整价值；
- 根据己方库存稀缺度调整价值；
- 预测敌方会先捡到宝箱；
- 单独规划多回合寻路。

这些都不是 `Character.as::aiThink` 中已经确认的原版规则，不应作为“修复”静默加入；如需增强，应登记为用户授权扩展。

## 6. 如何选择与使用武器

### 6.1 普通武器候选

每个角色的普通武器采样数为：

`floor(character.Luck * teamCharacterCount / aliveCharacterCount)`

同一种库存武器只评估一次。每个样本的角度为 `180..359` 度，力度为 `[5, twangMaxForce)`，模拟得到终点后从 `-0.01` 开始评分：

- 每个 70 px 内的存活敌人：`+1.5-distance/70`，随后乘以 `1+Evilness`；
- 每个 40 px 内的存活队友：`-(1.5-distance/40)`；
- Anchor：总分乘 `0.5`；
- Pieces of Eight：`(score-0.5)*1.2`。

Unity 普通执行走 `MutinyWeaponFactory.SpawnAndFire()`，因此不是由 AI 直接改伤害或命中结果；AI 只提供武器类型和初速度。

### 6.2 专用武器候选

- **Tidal Wave**：敌方在水面上方 300 px 范围内时，按当前生命比例最多 `+0.5`；范围内每个己方角色 `-1.5`。不需要弹道参数。
- **Voodoo Doll**：对每个存活敌人模拟 2 个角色投掷；若目标落水，得分为 `[1,1.2)`，否则为 `[-0.5,-0.3)`；候选同时保存目标角色与速度。
- **Seagull**：飞行高度设为最高敌人上方 `100..199 px`，在全图随机 10 个 X 并排序；只保留单发得分大于 0 的投弹点，至少有 2 个点才生成整件武器候选。
- **Cannon**：随机部署到角色上方部署圆内，再随机独立炮口角度；部署体先经过一次碰撞校正，炮弹以固定 30 力度模拟，保存部署点、角度和炮弹速度。
- **Wooden Crate / Gunpowder Barrel**：原版共享 `BoxWeapon.aiSimulation()`，围绕存活敌人随机 10 个合法位置，至少得到 3 个才以随机分数加入候选；执行时取前三个位置，按 40 tick 节奏继续摆放。
- **Anchor**：原版在全图随机 X，从 `y=-200` 以 40 px/tick 下落，只把实际命中地面的样本加入普通落点评分；执行前额外等 20 tick。

## 7. 15 种库存武器的当前 AI 覆盖

| 武器 | 原版 AI 方式 | Unity 当前选择/执行 | 状态 |
| --- | --- | --- | --- |
| Cherry Bomb | 普通随机弹道，首次接触完成；移动阶段另有源码怪癖式后续加分 | 候选恢复首次接触与箱体碰撞；`SpawnAndFire` | 已实现；待整回合验证 |
| Dynamite | 普通随机弹道，滚动至模拟完成 | 预测持续至静止或原版 101-step 安全上限；正式炸药类执行 | 已实现；待运行对照 |
| Banana | 普通初投；模拟在静止时结束，正式 AI 运行中还会按接近/远离角色引爆 | 普通候选恢复静止终止；`MutinyBanana` 有 AI 自动引爆 | 已实现；待运行对照 |
| Boulder | 普通随机弹道；AI 直接用候选速度 | 专用物理参数预测；执行保留原速度，不套玩家 `.5` 转换 | 已实现；有生产入口回归断言，待实际运行 |
| Cannon | 专用部署点、角度和 Cannonball 模拟 | `EvaluateCannon` + `BeginAiFire` | 已实现；候选到执行链完整，待运行对照 |
| Gunpowder Barrel | `BoxWeapon` 位置采样和连续摆放 | `EvaluateBoxWeapon` + `BeginAiPlacement` | 已实现；连续节奏已接入，待运行对照 |
| Mine | 普通随机弹道在静止时完成，正式对象负责布雷与触发 | 候选恢复静止终止；正式 Mine 执行 | 已实现；原版候选本身同样不评价未来触发收益 |
| Parachute Bomb | 模拟阶段每 tick 调整 `vy` 与 `vx`，接触即完成；正式运行才开伞/接收玩家风扇 | 候选已恢复模拟阶段速度修正、顶部边界、碰撞终止；正式武器执行 | 已实现；待运行对照 |
| Pieces of Eight | 首枚普通评分；后 7 枚各等 20 tick，再从 10 次投掷选最佳 | 首枚统一评分；`MutinyPiecesOfEight` 自动完成后续 7 枚 | 已实现；后续链待整回合验证 |
| Rum Bottle | 普通随机投掷，首次接触完成模拟；正式对象产生 Sweeping Flame | 候选恢复首次接触终止；正式武器执行 | 已实现；与原版一样只按瓶体首次接触点评分 |
| Seagull | 专用高度、10 个 X、逐发筛选 | `EvaluateSeagull` + `PlaceForAi`；敌我公式分开且投弹预测读取箱体 | 已实现；待运行对照 |
| Tidal Wave | 专用水线评分并直接发动 | `ScoreTidalWave` + `StartWave` | 已实现，待运行对照 |
| Voodoo Doll | 对每个敌人做 2 次角色投掷并绑定目标 | `EvaluateVoodoo` + `BindTarget/FireForAi` | 已实现，待运行对照 |
| Wooden Crate | 与火药桶相同的 `BoxWeapon` AI，连续放 3 箱 | `EvaluateBoxWeapon` + `BeginAiPlacement`；回合门等待三箱序列完成 | 已实现；生产入口回归已写，待 Unity 运行 |
| Anchor | 全图随机 X 的垂直下落模拟；执行等 20 tick | `EvaluateAnchor` + `DropForAi`；只接纳实际触地样本并沿用 Anchor `×0.5` 评分 | 已实现；生产入口回归已写，待 Unity 运行 |

`cannonball` 是 Cannon 的内部投射物，不是第 16 种库存武器。

## 8. 执行与行动消耗

武器候选胜出后，Unity 根据类型调用正式入口：

- 普通武器：`MutinyWeaponFactory.SpawnAndFire`；
- 潮汐：`MutinyTidalWave.StartWave`；
- 海鸥：`MutinySeagull.PlaceForAi`；
- 火药桶：`MutinyGunpowderBarrel.BeginAiPlacement`；
- 木箱：`MutinyWoodenCrate.BeginAiPlacement`，按 40 tick 间隔连续放置三箱；
- 船锚：`MutinyAnchor.DropForAi`，先等待 20 tick，再从候选 X 的 `y=-200` 垂直下落；
- 大炮：`MutinyCannon.BeginAiFire`；
- 巫毒：绑定目标后 `MutinyVoodooDoll.FireForAi`。

提交武器后设置 `CanThrow=false、CanShoot=false` 并通知 `MutinyTurnManager.NotifyActionStarted()`。Throw Self 则写入角色正式物理速度、调用 `MarkSelfThrown`、只设置 `CanThrow=false`，因此保留第二阶段射击资格。

## 9. 已知差异与风险

| ID | 可观察差异/风险 | 原版来源 | Unity 入口 | 当前状态 |
| --- | --- | --- | --- | --- |
| AI-DIFF-01 | Wooden Crate 原版可被 AI 选择并放 3 个；旧 Unity AI 永远不生成该候选 | `Character.as::aiThink`、`BoxWeapon.as` | `EvaluateBoxWeapon`、`MutinyWoodenCrate.BeginAiPlacement` | 已修复；待 Unity 运行验收 |
| AI-DIFF-02 | Anchor 原版可随机全图落点并延迟 20 tick 执行；旧 Unity 控制器不选择它 | `Anchor.as::randomThrows/aiPerform` | `EvaluateAnchor`、`MutinyAnchor.DropForAi` | 已修复；待 Unity 运行验收 |
| AI-DIFF-03 | 旧 Unity 普通武器固定 50 步、Dynamite 80 步，未按原版各自 `advanceMotion/contact` 完成 | `Weapon.as::randomThrows` | `SimulateWeaponImpact` | 已修复为 101-step 与武器专用终止语义；待 Unity 运行验收 |
| AI-DIFF-04 | 旧 Unity 角色移动预测固定最多 70 步，而原版持续到静止或落水 | `Character.as::randomThrows` | `SimulateCharacterLanding` | 已修复；仅保留 4096 malformed-board 安全门；待 Unity 运行验收 |
| AI-DIFF-05 | 旧 Unity 多数 AI 预测未传入箱体障碍 | `Solid.as::advanceMotion` 与各武器类 | 普通武器、角色、Voodoo、Seagull、Cannon、Anchor 预测 | 已修复为读取 `MutinyBoxRegistry`；待 Unity 运行验收 |
| AI-DIFF-06 | 旧 Unity 海鸥对队友使用 `-1.5*(1-distance/40)`，原版为 `-(1.5-distance/40)` | `Seagull.as::aiSimulation` | `ScoreSeagullShot` | 已修复；待 Unity 运行验收 |
| AI-DIFF-07 | 旧 Unity 选人后固定等 0.35 秒，没有等待胜出角色镜头目标完成 | `Team.as::advance` | `ExecuteAITurnRoutine`、`MutinyCameraController` | 已修复为显式 Pan 并等待清空；待 Unity 运行验收 |
| AI-DIFF-08 | 旧 Unity 一次性求值，原版按每帧约 30 ms 重复调用当前角色的 `aiThink()` | `Team.as::advance` | `EvaluateDecisionSteps`、`ExecuteAITurnRoutine` | 已实现相同粒度的可恢复步骤；复杂关卡帧耗时待 Unity 运行验收 |
| AI-DIFF-09 | 旧 Unity 用全局随机且只记录胜出摘要，无法重放样本 | 原版各 `randomThrows/aiSimulation`；Unity 调试扩展 | `MutinyAIRandomStream`、决策 JSON | 已实现独立种子、全部抽样与候选重放；原版 PRNG 内部状态仍未知，待 Unity 运行验收 |
| AI-DIFF-10 | 旧 Unity 在 AI 尚未完成候选时仍优先跟随下降宝箱 | `TileSystem.as::advanceScrolling:438-445` | `IsEvaluatingCandidates`、`MutinyCameraController.LateUpdate` | 已修复思考门；待 Unity 运行验收 |

## 10. 行为规格与验收矩阵

### 本轮实现前规格（2026-09-24）

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| AI-SLICE-01 | 按队伍角色顺序，先逐个评价 50 个移动落点，再逐件武器评价；每帧给当前角色约 30 ms 的重复工作预算，完成后才汇总严格最高分 | `Team.as::advance:36-82`、`Character.as::aiThink:346-695` | AI 决策协程与候选迭代器 | 固定种子比较同步与分帧胜出参数，并确认迭代器跨至少 51 步；复杂关卡测帧 | 已实现；回归已写；待 Unity 运行 |
| AI-RNG-01 | AI 决策使用独立随机流，记录种子及每次随机调用的类型、边界和值；相同棋盘和记录可重放同一候选序列 | 原版各 `randomThrows/aiSimulation` 的 `Math.random()` 调用顺序；Unity 调试扩展 | `MutinyAIRandomStream`、决策 JSON | 同种子同步/分帧比较全部抽样和胜出参数；重放后校验每个候选；篡改边界应失败 | 已实现；回归已写；待 Unity 运行 |
| AI-PHY-03 | 普通候选从正式武器初始化结果读取物理参数；预测与正式运动共用可纯调用的武器逐 tick 修正规则，候选不会触发正式武器的伤害、音效和库存副作用 | `Weapon.as::randomThrows`、各武器 `advanceMotion/contact` | `CreateFormalWeaponPredictionTemplate`、`MutinyPhysics.Step`、`MutinyParachuteBomb.ApplyOriginalAirMotion` | 比较正式 Mine 与候选模板的参数及 3 tick 轨迹；Parachute Bomb 也比较 3 tick 轨迹 | 已实现普通武器模板与共用步进；局部回归已写；待 Unity 运行 |

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| AI-SEL-01 | 首次行动合并所有存活角色的移动和武器候选，以严格 `>` 选最高分，同分保留先出现者 | `Team.as::advance` | `EvaluateBestMove`、`ConsiderCandidate` | 固定随机序列，构造跨角色同分与高低分候选，验证角色和行动类型 | 静态确认；已实现；待运行验证 |
| AI-SEL-02 | 首次行动不可因最佳分非正而放弃 | `Team.as::advance: aiCanBailOut=false` | 初始分负无穷 | 全部移动落水，断言仍选 50 个样本中的最佳移动 | 回归代码已加入；未记录实际运行通过 |
| AI-SEL-03 | 移动后的第二阶段只评估已选角色武器，最佳分不大于 0 时结束 | `Team.as::continueTurn` | continuation 分支 | 设置 `CanThrow=false、CanShoot=true` 且无正分武器 | 回归代码已加入；未记录实际运行通过 |
| AI-MOVE-01 | 每个可移动角色生成 50 个 `[5,20)`、角度 `180..359` 的 Throw Self 样本 | `Character.as::randomThrows/aiThink` | `EvaluateCharacterSelfThrow`、`RandomArc` | 注入可记录 RNG，核对样本数、范围和胜出速度 | 静态确认；已实现；缺少可重放运行验收 |
| AI-MOVE-02 | 移动评分同时考虑高度、敌方中心、水、敌我距离、宝箱、Cherry 后续与移动距离 | `Character.as::aiThink` | `ScoreSelfThrow` | 为每个项构造只改变一个变量的场景，核对分差 | 静态确认；已实现；待补独立回归 |
| AI-AIR-01 | 未完成宝箱的落地点严格 40 px 内给移动候选 `+0.5` | `Character.as::aiThink` | `ScoreChestMoveLanding` | 比较 39.99、40、40.01 px | 回归代码已加入；未记录实际运行通过 |
| AI-WPN-01 | 普通武器采样数按 Luck、队伍总人数和存活数计算，同种库存只评估一次 | `Character.as::aiThink` | `EvaluateCharacterWeapons` | 不同 Luck/死亡人数/重复库存组合核对候选数 | 静态确认；已实现；待补回归 |
| AI-WPN-02 | 普通落点从 `-0.01` 起算，敌方 70 px、己方 40 px，并应用 Evilness/Anchor/POE 修正 | `Character.as::aiThink` | `ScoreGenericWeaponCandidate` | 固定落点逐项核对边界和分数 | 静态确认；已实现；待补回归 |
| AI-WPN-03 | 专用武器携带执行所需的额外参数，而不是伪装成普通投射物 | 各专用武器 `aiSimulation/aiPerform` | Cannon/Seagull/Tidal/Voodoo/BoxWeapon/Anchor 分支 | 通过生产执行入口验证参数传递、库存消耗和完成状态 | 已实现六类；待 Play Mode 整链验证 |
| AI-WPN-04 | Anchor 在全图采样 X，从 `y=-200` 垂直模拟，只将触地样本按普通武器公式和 `×0.5` 修正加入候选；胜出后走正式 20 tick 延迟入口 | `Anchor.as::randomThrows/aiPerform` | `EvaluateAnchor`、`DropForAi` | 固定随机种子和地面，断言样本数、候选类型、X 范围、正式对象与库存消耗 | 回归代码已加入；程序集编译通过；待 Unity 运行 |
| AI-WPN-05 | Wooden Crate 复用 BoxWeapon 合法位置采样，至少 3 个位置才加入候选；执行时按原版节奏连续放 3 箱 | `Character.as::aiThink`、`BoxWeapon.as::aiSimulation/aiPerform/aiContinue` | `EvaluateBoxWeapon`、`MutinyWoodenCrate.BeginAiPlacement` | 固定随机种子和宽地面，断言候选及前三位置，并推进 40 tick 验证第一箱落地 | 回归代码已加入；程序集编译通过；待 Unity 运行 |
| AI-PHY-01 | 普通武器候选使用武器自己的原版终止语义：接触型遇任意碰撞结束，Dynamite/Mine/Banana 静止结束，其余最多推进 101 次 | `Weapon.as::randomThrows` 及各武器 `contact/advanceMotion` | `SimulateWeaponImpact` | 分别构造接触、静止、无接触三类轨迹，核对终止 tick 与最终落点 | 已实现；回归已写；待 Unity 运行 |
| AI-PHY-02 | 所有 `hitsBoxes=true` 的 AI 武器和角色移动预测都读取与正式物理相同的共享箱体障碍；角色持续模拟到静止或入水 | `Solid.as::advanceMotion`、`Character.as::randomThrows` | `SimulateWeaponImpact`、`SimulateCharacterLanding`、`MutinyBoxRegistry` | 在无 tile 场景放置箱体，断言武器预测在箱体处终止；长轨迹超过旧 70 tick 后仍继续 | 已实现；回归已写；待 Unity 运行 |
| AI-WPN-06 | Seagull 对敌方单发收益为 `1-distance/40`，对己方惩罚为 `-(1.5-distance/40)`，不是统一倍率公式 | `Seagull.as::aiSimulation` | `ScoreSeagullShot` | 比较距离 20、40 px 的敌我分数，断言队友 20 px 为 `-1` | 已实现；回归已写；待 Unity 运行 |
| AI-CAM-01 | AI 选出最终角色后显式请求镜头移向该角色，并等镜头目标清空才执行，不使用固定秒数替代 | `Team.as::advance` | `ExecuteAITurnRoutine`、`MutinyCameraController.PanToCharacter` | 通过生产胜出角色镜头入口断言设置完成门；整协程时序待 Play Mode | 已实现；局部回归已写；待 Unity 运行 |
| AI-CAM-02 | 当前队由 AI 控制且候选尚未完成时，镜头在 speech/popup 之后立即返回，不进入下降宝箱、行动目标或角色平移分支 | `TileSystem.as::advanceScrolling:438-445` | `MutinyAIController.IsEvaluatingCandidates`、`MutinyCameraController.LateUpdate` | 核对 AI/思考状态真值表；下降宝箱整链待 Play Mode | 已实现；局部回归已写；待 Unity 运行 |
| AI-OBS-01 | 每次决策输出可关联的决策序号、阶段、候选数、胜出角色/武器/分数和关键参数，便于运行对照 | 原版 `trace(best rating/best weapon)`；Unity 扩展 | `MutinyAIController.LogDecisionSummary` | 连续决策检查 `AI-Decision` 日志中的 id、阶段与胜出摘要 | 已实现；待运行日志验收 |
| AI-EXE-01 | Throw Self 只消耗移动，落稳后可进入武器续行动 | `Team.as::advance/continueTurn` | `ExecuteMove`、回合续行动 | 生产入口完成一次 AI 移动并等待落稳，再断言只为同一角色评估武器 | 局部回归代码已加入；整回合未验证 |
| AI-EXE-02 | 武器执行同时消耗移动与射击资格 | `Team.as::advance` | `ExecuteMove` | 对普通和每类专用武器核对行动资格与回合收束 | 静态确认；已实现；待运行验证 |

## 11. 推荐实现顺序

1. **运行新增 AI 回归**：验证 `AI-SLICE-01`、`AI-RNG-01`、`AI-PHY-03` 及之前的 `AI-PHY-01/02`、`AI-WPN-04..06`、`AI-CAM-01/02` 在 Unity Play Mode 中通过。
2. **用复杂关卡测帧耗时与实际轨迹**：检查单件武器、50 条移动弹道初始生成，以及 Parachute Bomb 正式/预测轨迹。
3. **做生产整回合回归**：覆盖“直接开火”“先移动再开火”“先移动后放弃”“追空投”“每种专用武器”“所有候选非正”六类场景。

### 决策重放方法

默认每次 AI 决策在 `Application.persistentDataPath/ai-decisions/` 保存 JSON，日志中的 `AI-Decision trace=` 给出完整路径。文件包含种子、每次随机抽样的类型/边界/值、全部候选参数与最终胜者。把该路径填入 AI 控制器的 `ReplayDecisionTracePath` 后，下一次同队、同阶段决策会逐项读取记录并比较每个候选；该路径只消费一次。也可启用 `UseFixedDecisionSeed` 并设置 `FixedDecisionSeed`，在相同棋盘上重新生成同一随机流。若棋盘、角色属性或库存已变化，重放会显式报错；此功能复现 Unity 决策，不声明能重建 Flash 原版的未知随机状态。

## 12. 验证状态汇总

### 静态确认

- 原版全队候选、两阶段行动、移动评分、空投 `+0.5/<40 px`、普通武器评分和各专用分支已从 AS2 源码核对。
- Unity 当前 AI 控制器、武器专用入口、空投状态和局部验证代码已逐项阅读。

### 已实现

- 全队角色候选竞争、首次行动不得放弃、移动后只为已选角色评估武器。
- 50 次移动采样及主要评分项。
- 空投接近价值。
- 普通武器候选已恢复原版终止语义、专用运动修正与箱体碰撞；角色、Voodoo 和 Seagull 预测同样读取箱体。
- 30 ms 可恢复候选迭代器、独立可保存/重放随机流、正式武器初始化状态模板与正式/预测共用的 Parachute Bomb 运动修正。
- Tidal Wave、Voodoo Doll、Seagull、Cannon、BoxWeapon、Anchor 的专用候选/执行；15 种库存武器均有候选入口。
- 胜出角色镜头完成门、AI 思考期间的空投镜头门和决策摘要日志。
- Banana AI 引爆和 Pieces of Eight 后续重瞄等武器内部 AI 行为。

### 实际测试通过

- `Assembly-CSharp` 与 `Assembly-CSharp-Editor` 均已编译通过（0 error）。这只证明代码可编译，不登记为 Play Mode 行为通过。

### 待运行验证

- `AI-SEL-01..03`、`AI-MOVE-01..02`、`AI-AIR-01`、`AI-WPN-01..06`、`AI-PHY-01..03`、`AI-SLICE-01`、`AI-RNG-01`、`AI-CAM-01..02`、`AI-OBS-01`、`AI-EXE-01..02`。
- 15 种库存武器在真实关卡、真实回合门、真实镜头和真实库存消耗下的逐项行为。

### 已知差异

- 见第 9 节 `AI-DIFF-01..10`；相关代码已接入，仍待 Unity 行为验收。原版 Flash `Math.random()` 的内部状态无法从当前静态证据恢复；复杂武器候选与正式生命周期仍需逐项运行对照。
