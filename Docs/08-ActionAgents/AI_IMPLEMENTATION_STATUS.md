# AI 逻辑实现进度与实现说明

> 基线日期：2026-09-23  
> 范围：原版 Flash 的角色选择、是否移动、移动落点评价、空投价值、武器选择与 AI 专用执行；以及 Unity 当前实现的对应关系。  
> 结论口径：本文严格区分“静态确认”“已实现”“实际测试通过”“待运行验证”和“已知差异”。

## 1. 当前结论

Unity 已经具备可工作的 AI 主干：每个可行动角色生成“投掷自己”和“使用武器”候选，使用原版风格的分数挑出全队最佳候选，再经生产武器入口执行。移动评分中的高度、敌方中心、落水、敌我距离、空投和移动距离项都已接入；15 种库存武器现在都能进入普通或专用候选路径。Wooden Crate 与 Anchor 已按原版专用逻辑接入，不再被候选分发器排除。

当前不能称为完整复刻，主要原因是：

- 普通武器候选使用统一的有限步数弹道预测，没有完整复用每种武器的正式运行状态机；降落伞炸弹等复杂武器的预测尤其仍是近似。
- AI 预测大多没有把现存箱体障碍传给物理步进；大炮部署是例外。
- 原版按每帧 30 ms 分片思考，Unity 当前一次性完成求值，并额外使用秒级等待。
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

`ExecuteAITurnRoutine()` 先等待 `ThinkDelay`（默认 0.8 秒）和回合可执行门，再等待当前回合初始镜头目标结束，然后一次性调用 `EvaluateBestMove()`。选出候选后调用 `MutinyTeam.SelectCharacter()`，再等待 0.35 秒并执行。

`EvaluateBestMove()` 已实现两个阶段：

- 第一阶段：遍历己方全部存活角色，同时比较武器与 50 个移动候选；初始最佳分为负无穷，所以非正分候选仍可获选。
- 第二阶段：若已有选中角色，且 `CanThrow=false、CanShoot=true`，只计算该角色的武器；最佳分不大于 0 时返回 Pass。

`ResolveTurnGate()` 将门分为：

- `Execute`：仍是本 AI 队且阶段为 `TurnActive`；
- `Wait`：仍是本队，但正处于 `ActionExecuting` 等暂态；
- `Cancel`：回合已换队、队伍不再由 AI 控制、队伍战败或游戏未开始/已结束。

### 3.3 当前时序差异

原版是“完成思考 → 选人 → 镜头移到选中的人 → 执行”。Unity 当前等待的是回合切换时已有的镜头目标；选出新的最佳角色后，没有在 AI 控制器内显式请求再平移到该角色，仅固定等待 0.35 秒。因此当最佳角色不是回合初始镜头所选角色时，表现时序可能不同。

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

当前实现会评估所有未完成宝箱，包括仍在下降但已经具有 `FloorPixelY` 的宝箱。`ResolveTurnGate()` 只检查回合阶段，没有等待宝箱落地的门；因此 AI 可能在宝箱下降期间就按最终落地点计算并执行行动。原版也会在新回合开始后思考，但 `TileSystem.advanceScrolling()` 在“AI 尚未完成思考”时优先返回，AI 完成后才进入下降宝箱运镜；Unity 当前则始终优先跟随下降宝箱，决策与运镜的相对时序不同。

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
| Cherry Bomb | 普通随机弹道；移动阶段另有源码怪癖式后续加分 | 统一弹道评分；`SpawnAndFire` | 已实现，预测近似，待整回合验证 |
| Dynamite | 普通随机弹道，滚动至模拟完成 | 80 步统一预测；正式炸药类执行 | 已实现，终止条件近似 |
| Banana | 普通初投；AI 运行中接近角色或静止时引爆 | 普通候选；`MutinyBanana` 有 AI 自动引爆 | 已实现；初投预测与后续引爆未做一体化评分 |
| Boulder | 普通随机弹道；AI 直接用候选速度 | 专用物理参数预测；执行保留原速度，不套玩家 `.5` 转换 | 已实现；有生产入口回归断言，待实际运行 |
| Cannon | 专用部署点、角度和 Cannonball 模拟 | `EvaluateCannon` + `BeginAiFire` | 已实现；候选到执行链完整，待运行对照 |
| Gunpowder Barrel | `BoxWeapon` 位置采样和连续摆放 | `EvaluateBoxWeapon` + `BeginAiPlacement` | 已实现；连续节奏已接入，待运行对照 |
| Mine | 普通随机弹道，正式对象负责布雷与触发 | 统一弹道评分；正式 Mine 执行 | 已实现；预测未覆盖未来触发价值 |
| Parachute Bomb | 普通随机投掷；运行时有降落伞/风扇规则 | 统一弹道评分；正式武器执行 | 部分实现；候选预测未模拟完整降落伞状态机 |
| Pieces of Eight | 首枚普通评分；后 7 枚各等 20 tick，再从 10 次投掷选最佳 | 首枚统一评分；`MutinyPiecesOfEight` 自动完成后续 7 枚 | 已实现；后续链待整回合验证 |
| Rum Bottle | 普通随机投掷，正式对象产生 Sweeping Flame | 统一弹道评分；正式武器执行 | 已实现；只按首个落点评分 |
| Seagull | 专用高度、10 个 X、逐发筛选 | `EvaluateSeagull` + `PlaceForAi` | 已实现；己方单发惩罚公式存在差异，见第 9 节 |
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
| AI-DIFF-03 | 原版普通武器最多推进 101 次且由各自 `advanceMotion/contact` 决定完成；Unity 通常固定 50 步，Dynamite 为 80 步 | `Weapon.as::randomThrows` | `SimulateWeaponImpact` | 近似实现 |
| AI-DIFF-04 | 原版角色移动模拟持续到静止或落水；Unity 固定最多 70 步 | `Character.as::randomThrows` | `SimulateCharacterLanding` | 近似实现 |
| AI-DIFF-05 | 原版正式模拟可受 `hitsBoxes` 和武器专用接触逻辑影响；Unity多数 AI 预测未传入箱体障碍 | `Solid.as::advanceMotion` 与各武器类 | `SimulateWeaponImpact`、`SimulateCharacterLanding` | 待补齐 |
| AI-DIFF-06 | 原版海鸥对队友单发惩罚为 `-(1.5-distance/40)`；Unity 当前为 `-1.5*(1-distance/40)` | `Seagull.as::aiSimulation` | `ScoreSeagullShot` | 公式差异 |
| AI-DIFF-07 | 原版选出角色后等待镜头移到该角色再执行；Unity 主要等待回合初始镜头目标，选人后固定等 0.35 秒 | `Team.as::advance` | `ExecuteAITurnRoutine`、`MutinyCameraController` | 时序差异 |
| AI-DIFF-08 | 原版思考按每帧 30 ms 分片；Unity 一次性求值，复杂局面可能产生帧尖峰 | `Team.as::advance` | `EvaluateBestMove` | 架构差异 |
| AI-DIFF-09 | Unity 使用 `UnityEngine.Random` 且没有 AI 决策种子/候选快照，失败难以完全重放 | 原版同样使用随机，但取证需记录序列 | 全部候选生成入口 | 可观测性缺口 |
| AI-DIFF-10 | 原版 AI 未思考完时不进入下降宝箱运镜；Unity 镜头优先跟随下降宝箱，但 AI 回合门并不等待宝箱落地 | `TileSystem.as::advanceScrolling` | `MutinyCameraController.LateUpdate`、`ResolveTurnGate` | 时序差异 |

## 10. 行为规格与验收矩阵

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
| AI-EXE-01 | Throw Self 只消耗移动，落稳后可进入武器续行动 | `Team.as::advance/continueTurn` | `ExecuteMove`、回合续行动 | 生产入口完成一次 AI 移动并等待落稳，再断言只为同一角色评估武器 | 局部回归代码已加入；整回合未验证 |
| AI-EXE-02 | 武器执行同时消耗移动与射击资格 | `Team.as::advance` | `ExecuteMove` | 对普通和每类专用武器核对行动资格与回合收束 | 静态确认；已实现；待运行验证 |

## 11. 推荐实现顺序

1. **先补可重放性**：为 AI 求值注入或封装随机源，日志记录回合、角色、武器、样本参数、落点、分项得分和胜出原因；不改变原版随机分布。
2. **运行 Wooden Crate 和 Anchor 的新增生产回归**：确认候选生成、正式执行、库存消耗与回合收束在 Unity 中通过。
3. **统一预测与正式物理**：让候选模拟调用与正式对象相同的状态推进/碰撞规则，至少先覆盖箱体障碍、Parachute Bomb、Dynamite 和 Banana。
4. **修正 Seagull 队友惩罚公式**，添加能区分两个公式的边界用例。
5. **修正选人后的镜头门**：明确请求平移到胜出角色，并以“镜头目标清空”而非固定 0.35 秒作为执行条件。
6. **做生产整回合回归**：覆盖“直接开火”“先移动再开火”“先移动后放弃”“追空投”“每种专用武器”“所有候选非正”六类场景。

## 12. 验证状态汇总

### 静态确认

- 原版全队候选、两阶段行动、移动评分、空投 `+0.5/<40 px`、普通武器评分和各专用分支已从 AS2 源码核对。
- Unity 当前 AI 控制器、武器专用入口、空投状态和局部验证代码已逐项阅读。

### 已实现

- 全队角色候选竞争、首次行动不得放弃、移动后只为已选角色评估武器。
- 50 次移动采样及主要评分项。
- 空投接近价值。
- 普通武器候选，以及 Tidal Wave、Voodoo Doll、Seagull、Cannon、BoxWeapon、Anchor 的专用候选/执行；15 种库存武器均有候选入口。
- Banana AI 引爆和 Pieces of Eight 后续重瞄等武器内部 AI 行为。

### 实际测试通过

- `Assembly-CSharp` 与 `Assembly-CSharp-Editor` 均已编译通过（0 error）。这只证明代码可编译，不登记为 Play Mode 行为通过。

### 待运行验证

- `AI-SEL-01..03`、`AI-MOVE-01..02`、`AI-AIR-01`、`AI-WPN-01..05`、`AI-EXE-01..02`。
- 15 种库存武器在真实关卡、真实回合门、真实镜头和真实库存消耗下的逐项行为。

### 已知差异

- 见第 9 节 `AI-DIFF-01..10`；`AI-DIFF-01/02` 已完成代码修复但仍待 Unity 行为验收，其余差异保持不变。
