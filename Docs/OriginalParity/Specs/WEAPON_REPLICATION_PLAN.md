# 十五种武器 1:1 复刻计划

范围：15 种可选武器从菜单选中、角色持有、瞄准、释放、飞行或放置、碰撞、特殊阶段、伤害与击退、动画、音效、镜头到回合结算的完整生命周期。

当前状态以各武器条目记录的最新实际证据为准。现有 C# 类、旧 TODO 的勾选和能生成对象都不能提高状态。

> 每次“复刻/修复武器”必须先执行 [`WEAPON_REPLICATION_WORKFLOW.md`](WEAPON_REPLICATION_WORKFLOW.md)：建立原版来源图、调用路径状态机、25 Hz 顺序、显示契约、声音映射和生产入口用例。只完成代码首版或编译通过时，状态只能是“实现中（静态初版）”。

## 状态定义

| 状态 | 进入条件 |
|---|---|
| 未实现 | 尚未按原版证据完成效果、动画或音效中的必要部分；占位类和近似逻辑也属于未实现 |
| 实现中 | 已开始按 AS2、pcode、时间轴和资源修复，但至少一个必要维度仍缺失，或自动用例尚未建立 |
| 已实现 | 效果、动画/装备实例、音效和交互已接入生产入口，代码与资源检查通过，但尚未完成全部自动场景验证 |
| 待人工验证 | 生产入口自动用例已经通过，只剩原版与 Unity 的实际画面、操作、声音和节奏对照 |
| 已完成 | 自动验证、Play Mode 操作和原版对照全部通过；已保存结果，且没有未登记差异 |

状态只能按顺序推进。某一武器的 EFF、ANI、AUD、INT、VER 任一必要项未完成时，整项不能标为“已实现”。完成状态还要求满足 `WPN-PROC-01..10`；尤其不得因只检查本体类而忽略父类、调用者、AI、时间轴、显示深度或音频事件。

## 所有武器必须执行的复刻步骤

### WPN-COM-EFF：效果与数值

- 从该武器 AS2 与 pcode 提取全部初始值、覆盖方法和分支，不能只读取 `Weapon.as` 的默认值。
- 记录并复刻重量、最大拉力、初速度换算、重力、空气/水中行为、摩擦、反弹、旋转和停止阈值。
- 记录并复刻直接伤害、爆炸或非爆炸作用范围、距离衰减、角色击飞力度/方向、对箱子/地形/其他武器的影响和连锁反应。
- 对非爆炸武器单独描述其实际作用方式，不套用统一 Explosion 公式。
- 固定 25 Hz 更新顺序；预测线与实际武器必须使用同一套该武器参数。
- 为每个数值保存“原版来源 → Unity 字段/方法 → 固定场景期望”，禁止以手感调参替代原版数据。

### WPN-COM-ANI：动画、装备实例与表现

- 核对武器 linkage、父/子时间轴、帧标签、帧脚本、注册点、朝向、缩放、层级和每段帧率。
- 选择武器后，按原版在角色身上创建并显示武器实例；核对持有位置、角色朝向、拖拽起点、取消时销毁和释放时从角色层切换到场景层。
- 分别复刻待机/持有、瞄准、释放、飞行、翻滚/旋转、碰撞、引信、爆炸或特殊效果、消失和残骸阶段。
- 动画触发伤害、生成子对象、播放声音或结束行动的帧必须保持原版 tick，不得只播放外观相似的循环动画。
- 原版可取得的 SWF 图像和时间轴必须优先使用；不能用程序圆形、占位 Sprite 或统一武器动画替代。

### WPN-COM-AUD：音效

- 枚举该武器所有 `playSound` 调用、时间轴帧声音和随机变体，建立事件到原版音频文件的映射。
- 分别核对选中/装备、释放、飞行循环、碰墙、落地、反弹、引信、爆炸、特殊阶段、入水和结束声音。
- 每个声音记录触发 tick、是否允许并发、循环/停止条件、音量和随机规则。
- 不同武器不得共享未经原版证明的通用碰撞声或爆炸声；例如 Dynamite 的碰撞/爆炸事件不能因实现方便而沿用 Banana 的声音链。

### WPN-COM-INT：输入、镜头与回合

- 核对选中资格、库存扣除时机、取消、重新选择、左键按住/释放、特殊多阶段点击和 AI 入口。
- 核对镜头从角色到武器/子对象/爆炸中心的切换、跟随速度、震屏、打断条件和回到当前队伍的时机。
- 核对 inactivity 重置、物理静止判定、特殊对象存续、行动资格消耗和回合结束条件。
- 右键取消是用户授权扩展，单列 EXT 行为，不改变原版生命周期的验收期望。

### WPN-COM-VER：每种武器的完成门槛

1. **逻辑/模拟**：从生产工厂与生产输入方法创建武器，逐 25 Hz tick 检查轨迹、状态、伤害、击退、对象生成/销毁、声音事件和结束条件。
2. **输入/场景**：在真实关卡中经行动菜单选中，确认武器先出现在角色身上，再完成瞄准、释放、特殊输入、取消和回合结算。
3. **原版对照**：使用相同关卡、角色位置、瞄准向量和事件序列，保存原版/Unity 的逐 tick 日志、关键帧截图与音频事件。
4. 覆盖正常命中、擦边、墙/地/角色/箱子/水、地图外、零库存、取消和跨回合残留；仅适用于该武器的边界场景不得省略。

## 当前总表

| ID | 武器 | 当前状态 | 原版主入口 | Unity 主入口 |
|---|---|---|---|---|
| WPN-01 | Cherry Bomb | 实现中 | `CherryBomb.as`、`Weapon.as`、`Explosion.as` | `MutinyCherryBomb.cs`、`MutinyExplosion.cs` |
| WPN-02 | Dynamite | 实现中 | `Dynamite.as`、`Weapon.as`、`Explosion.as` | `MutinyDynamite.cs`、`MutinyExplosion.cs` |
| WPN-03 | Banana | 实现中 | `Banana.as`、`Solid.as`、`Weapon.as`、symbol 921 | `MutinyBanana.cs`、`MutinyPlayerInput.cs` |
| WPN-04 | Boulder | 实现中 | `Boulder.as`、`Weapon.as`、`Solid.as`、DefineSprite 872 | `MutinyBoulder.cs`、`MutinyPlayerInput.cs`、`MutinyPhysicsBody.cs` |
| WPN-05 | Cannon | 实现中 | `Cannon.as`、`Cannonball.as`、sprites 850/853/866 | `MutinyCannon.cs`、`MutinyCannonball.cs`、`MutinyPlayerInput.cs` |
| WPN-06 | Gunpowder Barrel | 实现中 | `BoxWeapon.as`、`GunpowderBarrel.as`、sprite 968 | `MutinyGunpowderBarrel.cs`、`MutinyPlayerInput.cs`、`MutinyPhysicsBody.cs`、`MutinyAIController.cs` |
| WPN-07 | Mine | 未实现 | `Mine.as` | `MutinyMine.cs` |
| WPN-08 | Parachute Bomb | 实现中 | `ParachuteBomb.as`、`Weapon.as`、`Solid.as`、DefineSprite 939 | `MutinyParachuteBomb.cs`、`MutinyPlayerInput.cs` |
| WPN-09 | Pieces of Eight | 实现中 | `PiecesOfEight.as`、`Weapon.as`、`Character.as`、`Controller.as`、DefineSprite 884 | `MutinyPiecesOfEight.cs`、`MutinyPlayerInput.cs`、`MutinyTurnManager.cs` |
| WPN-10 | Rum Bottle | 实现中 | `RumBottle.as`、`SweepingFlame.as`、`DefineSprite_905_sweepingFlame` 帧 4/11 | `MutinyRumBottle.cs`、`MutinySweepingFlame.cs`、`MutinyRumBottleSmokeTrail.cs` |
| WPN-11 | Seagull | 实现中 | `Seagull.as`、`Character.as`、`DefineSprite_982_seagull` | `MutinySeagull.cs`、`MutinySeagullFire.cs`、`MutinyAIController.cs` |
| WPN-12 | Tidal Wave | 实现中 | `TidalWave.as`、`Character.as`、`DefineSprite_1058_tidalWave` | `MutinyTidalWave.cs`、`MutinyAIController.cs` |
| WPN-13 | Voodoo Doll | 实现中 | `VoodooDoll.as`、`TileSystem.as`、`Character.as`、sprites 1027/1872 | `MutinyVoodooDoll.cs`、`MutinyPlayerInput.cs`、`MutinyCharacterOverlay.cs`、`MutinyCameraController.cs` |
| WPN-14 | Wooden Crate | 实现中 | `BoxWeapon.as`、`WoodenCrate.as`、`Solid.as`、`Explosion.as` | `MutinyWoodenCrate.cs`、`MutinyPlayerInput.cs`、`MutinyPhysics.cs`、`MutinyExplosion.cs` |
| WPN-15 | Anchor | 实现中 | `Anchor.as`、`Solid.as`、DefineSprite 1003 | `MutinyAnchor.cs`、`MutinyPlayerInput.cs`、`MutinyPhysicsBody.cs` |

## 逐武器工作项

### WPN-01 — Cherry Bomb — 实现中

- [ ] WPN-01-EFF：确认并复刻原版碰撞触发、伤害、爆炸尺寸/实际半径、距离衰减、击飞和对象影响。
- [ ] WPN-01-ANI：恢复选中后角色手持实例、拖拽姿势、飞行动画、命中爆炸时间轴与销毁帧。
- [ ] WPN-01-AUD：核对释放、碰撞、`pop`、入水及可能的时间轴声音，不以其他武器声音替代。
- [ ] WPN-01-INT：核对库存、取消、镜头和静止 10 tick 后的回合结算。
- [ ] WPN-01-VER：建立固定向量的轨迹、爆炸、伤害/击退和声音事件对照；完成 Play Mode 与原版对照。

### WPN-02 — Dynamite — 实现中

- [ ] WPN-02-EFF：确认并复刻重量、拉力、碰撞响应、停止判定、引信时长、伤害、范围和击飞。
- [ ] WPN-02-ANI：恢复角色手持实例、飞行翻滚、撞墙/落地表现、停稳后的亮灭/倒计时帧和爆炸时间轴。
- [ ] WPN-02-AUD：逐项恢复碰墙、落地/滚动、引信与爆炸声音；与 Banana 的碰撞、反弹和爆炸声音分开验证。
- [ ] WPN-02-INT：核对释放后镜头跟随、停止后仍保持行动占用以及爆炸后的结算时机。
- [ ] WPN-02-VER：固定墙撞、地面滚动、立即停稳、入水和爆炸五类场景，与原版逐 tick 对照。

### WPN-03 — Banana — 实现中

- [~] WPN-03-EFF：已按 `Banana.as` 接入 7 px extent、bounce 0.8、friction 0.5、静止判定、AI `<400`（20 px）引爆和 160/80 爆炸；墙、顶、水、箱子与静止边界待 Unity/原版验证。
- [~] WPN-03-ANI：已使用原版 921 的单帧 sprite 和公共飞行旋转；手持姿势、爆炸画面和排序待画面对照。
- [~] WPN-03-AUD：已接入每次接触 `banana_bounce` 与引爆 tick 的单次 `pop`；实际声音并发与水面行为待对照。
- [~] WPN-03-INT：已接入“抛掷后、CanShoot 已消耗时的下一次左键”生产输入；镜头和完整回合占用待对照。
- [~] WPN-03-VER：工厂发射、生产二次点击、物理反弹和 AI 近距离场景回归已加入；Unity Editor、Play Mode 与原版逐 tick 对照未执行。详见 `BANANA.md`。

### WPN-04 — Boulder — 实现中

- [~] WPN-04-EFF：已按 `Boulder.as` 接入四向 31 px extent、weight 1.5、friction .25、`hitsBoxes=true`、角色闭区间 32 px 推挤与每 tick `abs(vx)*1.5` HP。人类 `twang` 和 AI `fire` 均保留 20 px/tick clamp 后的原始速度，不再误用只属于不可达 drag-release 的 `.5`；已恢复 `Weapon.advance` 的水面 crossing splash 和地图底部结束。物理斜坡/木箱、多人连续接触和 AI 模拟待运行对照。
- [~] WPN-04-ANI：已从 DefineSprite 872 拆出 869 可旋转石体和 depth 3 的静止 871 上层，25 Hz `vx*2.5` 只作用 `rotating` child；停稳从 visibility=2 以 .1/tick 调用 whiteOut 到两个子层。两个原版 `64×64` 子层现以中心 pivot、32 PPU 导入，恢复与 31 px Solid 对应的 `2×2` world-unit 画面尺寸；`blendMode="layer"` 的混色、白化 additive 前半段和装备 `(x,y-30)` 注册点待画面对照。
- [ ] WPN-04-AUD：静态 `Boulder.as` 未见独立 release/滚动/碰撞/结束声音；水面继续走通用 `splash`。运行对照前未臆加其他声音。
- [~] WPN-04-INT：已通过通用菜单选中和拖拽 twang 生产路径、AI `aiPerform` 路径提交库存/两行动；镜头和物理静止后的回合结算待对照。
- [~] WPN-04-VER：已新增人类 twang 20 px/tick 速度、AI 预测/执行速度、原始子层、Solid 参数、32 px 临界推挤/伤害、水面/底部和静止时序回归；`dotnet build Assembly-CSharp.csproj --no-restore` 通过。Unity Editor、Play Mode、坡度/木箱/水、镜头和原版逐 tick 对照未执行。详见 `BOULDER.md`。

### WPN-05 — Cannon — 实现中

- [~] WPN-05-EFF：已建立独立炮体，接入 120 px 放置、整数角、随旋转换算的 pin 阈值、Cannonball 0 weight/box 碰撞、未被通用 release 限制的 30 force、100/50 爆炸及水/地图边界无爆炸；AI `randomThrows`/25 tick `aiPerform` 已接入，边界场景待运行对照。
- [~] WPN-05-ANI：已接入 Cannon/Cannonball 单帧、每 tick smoke、Cannon 跟随与 20 tick淡出，ball 隐藏后保持到 Cannon 结束；Cannonball sorting order 固定为 later-created 原版 clip 的 next-highest depth（Cannon +1），消除共面闪烁；pin/炮口子层、注册点与 layer blend待画面对照。
- [~] WPN-05-AUD：已生产接入 fire `cannon explosion`、terrain/box contact `pop`；角色接触与水/边界分支不播放 `pop`，待运行对照。
- [~] WPN-05-INT：已实现“放置 → 炮尾 pin → 回弹到 -21 后开火”专用输入并修复工厂映射；pin 成功拉栓时扣除一次库存。AI已接入，取消、镜头待对照。
- [~] WPN-05-VER：已加入 30-force、ball/fade 生命周期和 25-tick AI 的生产对象回归，C# 编译通过；Unity/原版对照未执行。详见 `CANNON.md`。

### WPN-06 — Gunpowder Barrel — 实现中

- [~] WPN-06-EFF：已按 `BoxWeapon.as`/`GunpowderBarrel.as` 接入一次行动两桶、16/15 extent、tile/chest/活角色/box 放置拒绝、共享 Controller.boxes 碰撞、150/30/null-caster 连锁 Explosion；角色、chest、木箱边界和伤害/击退待运行对照。
- [~] WPN-06-ANI：已用 sprite 968 的 12 帧资源，静止于第 1 帧并按 `explode` 标签切至第 11 帧、下一 tick 移除；手持注册点和实际视觉层级待画面对照。
- [~] WPN-06-AUD：原类没有独立放置或入水声音；爆炸沿 `Explosion` 第 3 帧的既有 `pop` 入口。实际时序和并发待原版运行对照。
- [~] WPN-06-INT：已接入菜单选择、两次生产点击、首次单次库存扣除、非法点击不消耗、AI 候选抽样与 40/10 tick 放置；取消、跨回合和 AI 实战待验证。
- [~] WPN-06-VER：已加入生产输入、碰撞登记、帧 11 和两桶连锁回归；`dotnet build Assembly-CSharp.csproj --no-restore` 通过。Unity Editor、Play Mode 与原版对照未执行。详见 `GUNPOWDER_BARREL.md`。

### WPN-07 — Mine — 未实现

- [ ] WPN-07-EFF：复刻投掷参数、落地武装条件、感应范围、目标筛选、蜂鸣倒计时、伤害和击飞。
- [ ] WPN-07-ANI：恢复手持、飞行、落地、武装、感应闪烁、倒计时和爆炸动画。
- [ ] WPN-07-AUD：恢复落地/武装、每次 `mine_beep`、触发和爆炸声音。
- [ ] WPN-07-INT：核对跨回合存续、镜头、接近触发与回合 inactivity 的关系。
- [ ] WPN-07-VER：覆盖敌我接近、边界距离、投掷后未武装、跨回合和连锁触发。

### WPN-08 — Parachute Bomb — 实现中

- [~] WPN-08-EFF：已按 `ParachuteBomb.as advanceMotion/contact` 接入 11 px 四向 extent、30-force twang、`vy>1` 减 2（下限 1）、`.95` 横向阻尼、严格 `vy>-10` 开伞、碰撞 160/50 爆炸和 y=-300 限制；木箱、地图底部与逐 tick 对照待执行。
- [~] WPN-08-ANI：已加载 DefineSprite 939 的 30 帧，保持 closed 第 1 帧、opening 第 11 帧、open 第 26 帧及第 30 帧回跳 open；未开伞每 tick 创建 cannonSmokeTrail。持有注册点、光标风扇和爆炸画面对照待执行。
- [~] WPN-08-AUD：已在第 12、24…个持续扇风 tick 接入 `fan`，contact 同 tick 接入一次 `pop`，水面 crossing 接入 `splash`；Unity/原版音频并发待对照。
- [~] WPN-08-INT：已通过普通菜单/拖拽生产路径接入人类持有者的持续左右鼠标扇风（鼠标左推右，反之亦然）；镜头跟随和 AI 评分待核对。
- [~] WPN-08-VER：生产输入、阈值、左右风力、30 帧回环和 terrain 爆炸回归已加入；`dotnet build Assembly-CSharp.csproj --no-restore` 通过。Unity Editor 实际执行、Play Mode、木箱/水/顶端/底部和原版逐 tick 对照未执行。详见 `PARACHUTE_BOMB.md`。

### WPN-09 — Pieces of Eight — 实现中

- [~] WPN-09-EFF：已按 `PiecesOfEight.as` 接入 7 px 四向 extent、`hitsBoxes=true`、terrain/box contact 的 `Explosion(50,25,owner)` 与同 tick `pop`、入水只 splash 并切换下一枚、前 7 枚回到 owner `(x,y+5)`、第 8 枚 finished/hide。实际 physics contact、水面和逐 tick 对照待执行。
- [~] WPN-09-ANI：已使用 DefineSprite 884 的单帧 `Resources/Art/Weapons/PiecesOfEight/1.png`；持有注册点、飞行旋转、排序和画面对照待执行。
- [~] WPN-09-AUD：已接入 terrain/box contact 当 tick 的唯一 `pop` 与水面的 `splash`；原版静态代码未见独立投掷、飞行或结束声音，需运行对照确认。
- [~] WPN-09-INT：已接入生产菜单/拖拽；第一次发射仅扣一件库存，前 7 枚同一对象仍可在 `ActionExecuting` 阶段重瞄，锁定取消/换武器并阻止 inactivity 结算。AI 每枚后 20 tick 等待，使用十样本/70 px 原版评分。Play Mode 和 AI 场景待验证。
- [~] WPN-09-VER：已新增生产输入的 8 枚连续序列、库存一次扣除、锁定、回合占用、最终结束和 7 px/box 断言；`dotnet build Assembly-CSharp.csproj --no-restore` 通过。Unity Editor、Play Mode 和原版逐 tick 对照未执行。详见 `PIECES_OF_EIGHT.md`。

### WPN-10 — Rum Bottle — 实现中

- [~] WPN-10-EFF：已按 `RumBottle.as contact` 与 `SweepingFlame.as` 接入 80/25 爆炸、仅地面双向点火、顶部 tile 起点、严格 `< 64` 命中、30 HP 和 8 px 地形传播；平台边缘、墙和水的生产场景尚待验证。
- [~] WPN-10-ANI：已加载 12 帧酒瓶和原版 11 帧火焰，按飞行 tick 生成烟雾、按火焰第 4/11 帧传播/销毁；烟雾原版 19 帧尚待导入 Unity Resources。
- [~] WPN-10-AUD：已在酒瓶 `contact` 当 tick 立即播放 `pop`，且抑制该瓶在爆炸伤害帧的重复播放；AS2 中未发现独立的点火、燃烧、烧伤或熄灭声音，仍待运行对照确认。
- [ ] WPN-10-INT：核对火焰存续期间的镜头、inactivity 与回合结算，避免爆炸后立即结束而跳过燃烧。
- [~] WPN-10-VER：固定地面场景的生产物理入口回归已加入；Unity Editor、平台边缘/墙/水、角色进入火焰、行动菜单输入及原版逐 tick 对照未执行。详见 `RUM_BOTTLE.md`。

### WPN-11 — Seagull — 实现中

- [~] WPN-11-EFF：已接入 `x=-300`、`vx=10`、零重力飞行、右侧 `+275` 且无子弹才结束，以及 `(x-10,y)` 的 `vx=10/weight=1` 子弹、50/50 接触爆炸和落水销毁；箱体碰撞与实际镜头待验证。
- [~] WPN-11-ANI：已加载 14 帧海鸥、投放 shot 段和 1 帧 seagullFire；原版 dottedLine 路径预览帧尚未导入 Unity Resources。
- [~] WPN-11-AUD：每次成功投放立即随机播放 `poop1..3`；AS2 未显示独立飞行、命中或离场声音，待原版运行对照。
- [~] WPN-11-INT：已接入“第一次点击路径、飞行中后续点击投放”的生产输入与 AI 专用 `aiSimulation/aiPerform` 路径；镜头跟随偏移待完成。
- [~] WPN-11-VER：路径、子弹、碰撞和水面生产回归已加入；Unity Editor、固定随机 AI、真实菜单及原版逐 tick 对照未执行。详见 `SEAGULL.md`。

### WPN-12 — Tidal Wave — 实现中

- [~] WPN-12-EFF：已接入固定 `x=-550/y=water/vx=20`、零重力且不碰 terrain 的波；每 tick 对 `±150 px`、水面上方 300 px 内角色造成 5 HP，严格在 `levelWidth+550` 后结束。角色受击和镜头待场景对照。
- [~] WPN-12-ANI：已加载 27 原版帧并按关卡 skyColour 选择 1–5、10–14 或 19–23 的五帧循环；像素注册点待画面对照。
- [~] WPN-12-AUD：静态 AS2 与资源核对未发现专属声效，未添加臆测声音；原版运行对照待执行。
- [~] WPN-12-INT：已通过原生产点击入口启动，忽略点击位置并固定向右；镜头跟随偏移和回合结算待完成。
- [~] WPN-12-VER：生产启动、物理伤害窗、动画步进与离场阈值回归已加入；Unity Editor、AI 固定场景、真实菜单和原版逐 tick 对照未执行。详见 `TIDAL_WAVE.md`。

### WPN-13 — Voodoo Doll — 实现中

- [~] WPN-13-EFF：已按 `VoodooDoll.as` 接入默认 10 px extent、保存释放速度、前 10 tick 娃娃阶段、镜头完成后的 10 tick 延迟与一次目标传速；去除了旧实现持续镜像速度、直接伤害和“娃娃入水使目标溺水”的错误分支。目标在发射后死亡、箱体接触及镜头边界时序待运行对照。
- [~] WPN-13-ANI：已加载原版娃娃 sprite 1027 和 overlay target sprite 1872，按 `velocityX * 2` 使用公共飞行旋转，并在传速后每 tick 淡出 10%。角色手持姿势与原版逐帧画面对照待完成。
- [~] WPN-13-AUD：人类选中有效目标、AI `aiPerform` 各播放一次原版 `voodoo`；命中、受击和入水没有在静态 `VoodooDoll.as` 发现独立声音，待原版运行对照确认。
- [~] WPN-13-INT：已通过生产输入接入“选择武器 → 30 px 内敌人 → 拖拽释放”、严格半径、右键取消不扣库存、目标十字/角框互斥和镜头从娃娃切至目标。真实菜单与鼠标光标待 Unity Play Mode 验收。
- [~] WPN-13-VER：生产对象回归覆盖资源、30 px 临界、取消、无目标拒绝、两段 10 tick、一次传速、非持续同步和淡出；Unity Editor、真实菜单、目标死亡/落水/箱体与原版逐 tick 对照未执行。详见 `VOODOO_DOLL.md`。

### WPN-14 — Wooden Crate — 实现中

- [~] WPN-14-EFF：已接入三箱、32 px 非对称 16/15 extent、terrain/角色/箱/宝箱放置拒绝、重力/反弹/摩擦、角色与箱子 `hitsBoxes` 阻挡和爆炸 AABB 销毁；入水和非木箱武器的箱体碰撞待原版验证。
- [~] WPN-14-ANI：已加载原版 DefineSprite 965 的 18 个导出帧，使用 frame 1 静态箱和后续销毁帧序列；`explode` 标签帧界、持有注册点、残骸和画面对照待完成。
- [ ] WPN-14-AUD：静态 `BoxWeapon.as` 未发现音效调用；放置、碰撞、破坏和入水须经时间轴/运行对照确认，未臆加声音。
- [~] WPN-14-INT：已接入菜单选中、未放置时右键取消、三次点击和首次成功时单次库存扣除；首次放置进入行动占用但允许后两次放置。非法鼠标光标、AI、结算时序待运行对照。
- [~] WPN-14-VER：已加入生产选择/点击、非法 terrain、三箱库存、extent/碰撞标志与爆炸 AABB 回归。C# 编译通过；Unity batch-mode 验证在进入方法前以 code 1 退出，具体原因未记录；Play Mode、堆叠/角色/水和原版对照未执行。详见 `WOODEN_CRATE.md`。

### WPN-15 — Anchor — 实现中

- [~] WPN-15-EFF：已按 `Anchor.as place/advance/contact` 接入原始鼠标 x、强制 `y=-200`、每 tick `vx=0/vy=40`、48/96/0 extent、terrain/木箱 floor contact，以及 `abs(dx)<48 && anchor.y-64<character.y<anchor.y` 的每角色 60 HP 压砸。该类自驱动 `Solid` tick，绕过 `Weapon.advance` 的水面/超时和飞行旋转；无 floor 的长期行为和 AI `randomThrows` 评分待原版运行对照。
- [~] WPN-15-ANI：已加载 DefineSprite 1003 的 12 个原始帧，floor contact 从 frame 1 推进至 frame 12，之后按 30 hold + 10 `Global.whiteOut` tick 隐藏。默认 Sprite material 无法表达原版前半段 additive 白化，材质与画面对照待完成；目标列提示和注册点待对照。
- [~] WPN-15-AUD：已在每次 floor contact 生产路径调用原版 `anchor` SFX；静态 AS2 未见独立选择、下落、入水或结束声音，仍待运行对照。
- [~] WPN-15-INT：已接入行动菜单选中、未点击时右键取消、任意场景 x 点击、单次库存扣除、两行动消耗和回合提交。镜头跟随偏移与 AI `aiPerform(details.x)` 的 20 tick 等待尚未接入 AI 决策。
- [~] WPN-15-VER：已新增生产选择/点击、`y=-200`、物理 floor、严格边界伤害、12 帧、30+10 tick 的回归用例；`dotnet build Assembly-CSharp.csproj --no-restore` 通过。Unity Editor 实际执行、Play Mode、木箱/水/地图边缘和原版逐 tick 对照未执行。详见 `ANCHOR.md`。

## 原版证据根目录

- AS2：`Docs/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/throwgame/`
- pcode：`Docs/ReverseEngineering/Swf/pcode/scripts/__Packages/com/nitrome/throwgame/`
- symbol 与帧资源：`Docs/ReverseEngineering/Art/`
- Unity 生产入口：`Assets/Mutiny/Scripts/Simulation/MutinyWeaponFactory.cs`、各 `Mutiny*.cs`、`Presentation/MutinyPlayerInput.cs`

每次推进某种武器时，在本文件对应 EFF/ANI/AUD/INT/VER 项下补充具体来源、Unity 方法、用例和实际结果；`TODO.md` 只保留总状态。
