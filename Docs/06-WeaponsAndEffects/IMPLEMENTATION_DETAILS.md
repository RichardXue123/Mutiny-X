# 武器逻辑实现细节与一致性审计

[返回模块入口](README.md)

## 1. 文档范围与证据优先级

本文专门描述武器逻辑：注册、库存、装备、瞄准、提交、飞行/放置、碰撞、持续效果、结束以及 AI 所复用的入口。美术像素、音频素材本身和 HUD 排版不在本文展开，但会记录触发时机。

证据优先级如下：

1. 原版反编译类：`Weapon.as`、`Solid.as`、`Character.as`、`TileSystem.as`、`Explosion.as` 和 15 种武器类。
2. 原版时间轴脚本与符号层级。
3. 关卡 XML 的库存字段。
4. Unity 生产入口和回归代码仅用于判断“当前实现到了哪里”，不用于推导原版规则。

原版类根目录：`Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/throwgame/`。

状态用语：

- **静态确认**：已经由 AS2/pcode/时间轴直接确认。
- **已实现**：当前 Unity 有对应生产代码，不代表已经运行通过。
- **待运行验证**：已有或已列出用例，但本轮没有运行 Unity。
- **已知差异**：Unity 与原版静态规则不一致，不能标成一致性通过。

## 2. 总体调用链

`关卡 XML 库存 → MutinyCharacter → 武器面板/玩家或 AI 选择 → MutinyWeaponFactory.SpawnWeapon → PrepareForEquip → 瞄准/放置专用入口 → Fire/Twang/Place → MutinyPhysicsBody 25 Hz → 专用碰撞/时序 → MutinyExplosion 或持续效果 → Finish → MutinyTurnManager`

主要 Unity 入口：

- 注册与实例化：`MutinyWeaponFactory.cs`
- 共享状态与发射：`MutinyWeapon.cs`
- 玩家状态机：`MutinyPlayerInput.cs`
- AI 候选与提交：`MutinyAIController.cs`
- 固定步物理：`MutinyPhysics.cs`、`MutinyPhysicsBody.cs`
- 爆炸结算：`MutinyExplosion.cs`
- 回合收尾：`MutinyTurnManager.cs`
- 生产入口回归：`MutinyTurnActionUiVerificationTest.cs`

## 3. 注册、库存和装备

| ID | 原版规则 | 原版来源 | Unity 入口 | 状态 |
| --- | --- | --- | --- | --- |
| WPN-REG-01 | 可装备 ID 恰为 15 个：`anchor`、`banana`、`boulder`、`cannon`、`cherryBomb`、`dynamite`、`gunpowderBarrel`、`mine`、`parachuteBomb`、`piecesOfEight`、`rumBottle`、`seagull`、`tidalWave`、`voodooDoll`、`woodenCrate` | `Character.as::equip` | `MutinyWeaponFactory.SpawnWeapon` | 静态确认；已实现 |
| WPN-INV-01 | XML 数量 `10` 表示无限；其他正整数按次数将同一 ID 重复加入 `hasWeapons` | `Character.as::setWeapons` | `MutinyCharacter` 库存初始化、`IsInfinite` | 静态确认；待运行验证 |
| WPN-INV-02 | 武器提交后，非无限库存移除一次；`limitedToTurn` 决定回合到期是否销毁对象 | `Character.as::weaponExpired` | `ConsumeWeapon`、回合状态机 | 已实现；跨回合武器待运行验证 |
| WPN-EQUIP-01 | 普通武器初始点为角色 `(x,y-10)`；Boulder 额外上移 20 px，即 `(x,y-30)` | `Character.as::equip` | `MutinyWeapon.GetOriginalEquipOffsetPixels` | 静态确认；已实现 |
| WPN-EQUIP-02 | 已发射武器或已主动抛出角色时，不能重新装备/卸下 | `Character.as::equip/unequip` | `MutinyPlayerInput` 的 armed/committed 判断 | 已实现；待运行验证 |

`cannonball` 是内部投射物，不是第 16 种库存武器。

## 4. 共享瞄准、物理与结束语义

### 4.1 拉拽投掷

`Solid.twang()` 使用：

`velocity = (mouse - weapon) * -0.25`

再将长度限制到该武器的 `twangMaxForce`。鼠标只有在武器中心 30 px 内按下才开始 twang；预测线绘制 15 个离散点。普通最大力为 20；Banana、ParachuteBomb、RumBottle 改为 30。

重要：正常投掷路径由 `TileSystem.mouseUp → twanging.twang()` 提交，不经过 `Weapon.release()`。因此 `Weapon.release()` 的 20 速度上限不能用来把三种 30 力武器统一截成 20。

| ID | 可观察行为 | 来源 | Unity 入口 | 状态 |
| --- | --- | --- | --- | --- |
| JUMP-CAN-01 | 选择 Throw Self/跳跃但尚未拉线时，角色下方显示与武器相同的取消叉；点击回到行动菜单且不消耗跳跃 | `Character.as::updateOverlay` 的 `weaponSelected` 条件；`CancelWeaponButton.as::onPress` | `ShouldShowCancelWeapon`、`TryCancelWeaponFromOverlay` | 已实现；待运行验证 |
| EXT-JUMP-CAN-01 | Throw Self 蓄力时右键只撤销当前拉线，返回跳跃待命且不消耗行动 | 用户授权扩展；原版 AS2 无右键输入规则 | `TryCancelAimFromSecondaryPointer`、`CancelCurrentAim` | 已实现；待运行验证 |

| ID | 可观察行为 | 来源 | Unity 状态 |
| --- | --- | --- | --- |
| WPN-AIM-01 | 30 px 命中半径内才进入拉拽；松开立即提交 | `TileSystem.as::mouseDown/mouseUp` | 已实现；待运行验证 |
| WPN-AIM-02 | 拉力系数 0.25，预测 15 tick，默认上限 20 | `Solid.as::twang/drawTwangLine` | 已实现 |
| WPN-AIM-03 | Banana、ParachuteBomb、RumBottle 的实际提交上限是 30 | 三个武器构造器 + `Solid.twang` | 三者均已实现；待 Unity 运行验证 |
| WPN-AIM-04 | 放置型武器不进入通用 twang；各自消费点击/拖动 | `TileSystem.as` 与专用类 | 已实现 |
| WPN-VEL-01 | 仅普通拉拽受默认 20 上限；Banana、ParachuteBomb、RumBottle 各为 30，预览与提交一致 | `Solid.as::twang/drawTwangLine`、三种武器构造器 | `MutinyWeapon.Twang`、`MutinyWeaponFactory.GetTwangMaxForce` 已实现；待运行验证 |
| WPN-VEL-02 | 直接 `Weapon.fire(vx,vy)` 不限速；`Weapon.release` 的 20 限速属于另一种拖拽提交路径 | `Weapon.as::fire/release`、`TileSystem.as::mouseUp` | `MutinyWeapon.Fire` 已修复；待运行验证 |

### 4.2 Weapon 公共状态

原版 `Weapon` 初值：`fired=false`、`finished=false`、`simulation=false`、`placeableWeapon=false`、`limitedToTurn=true`、`dragRange=130`、`dragOffset=-100`、`showCircle=true`。`fire`/`place` 只接受首次提交，并开启镜头跟踪。

继承 `Weapon.advance()` 的武器每 tick：先走 `Solid.advanceMotion()`，再更新显示；发射后若低于地图底部或 `vx==0 && abs(vy)<0.2` 则结束；随后只做水面穿越 splash。原版没有共享 8 秒超时，也没有“入水 0.4 秒自动销毁”的公共规则。

先前 `MutinyWeapon.Update()` 增加过入水失效、固定世界坐标和 8 秒保险超时，`MutinyTurnManager` 还增加过 150 tick 强制结束。本轮已移除这些非原版规则；CherryBomb、Dynamite、RumBottle 在 25 Hz 物理 tick 执行继承的原版结束判断，专属状态机仍由各武器负责。原版 `Solid.splashCheck` 只产生水花，武器不再套用角色的水下速度衰减。

### 4.3 爆炸

`Explosion(x,y,size,maxDamage,caster)` 的有效半径为 `size/2 + 20`。命中帧对每个存活角色计算 `ratio=1-distance/radius`，伤害为 `maxDamage*ratio`；冲量为：

- `vx += nx * 5 * (0.06 * ratio * maxDamage)`
- `vy += ny * 5 * force - 6 * force`

同一命中帧以圆到箱体 AABB 最近点检测 WoodenCrate/GunpowderBarrel，并调用其 `explode()` 形成连锁。

`Explosion.hit()` 的内部顺序固定为“全部角色伤害/冲量/`hit=true` → 倒序检查 boxes”。木箱命中后在同一次调用内先退出 `Controller.boxes`，再从 symbol 965 的 `explode` 标签第 11 帧开始播放；因此角色首次按新速度推进时木箱已经不再碰撞，而第 11～18 帧破损表现与角色飞行并行。

| ID | 可观察行为 | 来源 | Unity 状态 |
| --- | --- | --- | --- |
| FX-EXP-01 | 半径为 `size/2+20`，伤害线性衰减 | `Explosion.as::constructor/hit` | `MutinyExplosion.Spawn/ApplyHit` 已实现 |
| FX-EXP-02 | 所有角色均可被波及，包括施放者；施放者累计 `evilness += ratio` | `Explosion.as::hit` | 已实现；中心零距离方向做了安全定义 |
| FX-EXP-03 | 箱体使用最近点圆-AABB 检测并可递归连锁 | `Explosion.as::hit` | 已实现；待运行验证 |
| FX-EXP-04 | 爆炸命中发生在效果时间轴第 3 帧，效果第 8 帧销毁 | 爆炸时间轴脚本 | 已实现；待运行验证 |

## 5. 15 种武器速查

| ID | 类型 | 关键尺寸/运动 | 生效与结束 |
| --- | --- | --- | --- |
| `cherryBomb` | 拉拽投掷 | extent 9，默认物理 | 任一 Solid 接触爆炸 80/40 |
| `dynamite` | 拉拽投掷 | extent 11，friction 1.7 | 静止时爆炸 250/70；入水只切 `unlit` 画面 |
| `banana` | 拉拽+二次点击 | extent 7，bounce .8，friction .5，max 30 | 静止、玩家再次点击或 AI 条件时爆炸 160/80 |
| `boulder` | 拉拽投掷 | extent 31，weight 1.5，friction .25 | 横向挤压，伤害 `abs(vx)*1.5`，无爆炸 |
| `cannon` | 放置/拉栓 | 炮身默认 extent 10；炮弹 weight 0 | 拉栓阈值发射 30 速炮弹，接触爆炸 100/50 |
| `gunpowderBarrel` | 连续放置 | 16/15 非对称 extent，共 2 个 | 被爆炸波及后爆炸 150/30 |
| `mine` | 拉拽投掷/跨回合 | extent 14，friction 1.5 | 运动角色 60 px 内激活，60 tick 后爆炸 250/70 |
| `parachuteBomb` | 拉拽+风扇控制 | extent 11，max 30，vx×.95 | vy>-10 开伞；接触爆炸 160/50 |
| `piecesOfEight` | 8 连投 | extent 7 | 每次接触爆炸 50/25；入水不爆；第 8 发结束 |
| `rumBottle` | 拉拽投掷 | extent 14，max 30 | 接触爆炸 80/25；仅地面接触生成双向扫火 |
| `seagull` | 定高空袭 | x=-300，vx=10 | 飞行中每次点击投一枚 50/50 弹；鸟离场且弹清空结束 |
| `tidalWave` | 点击召唤 | x=-550，vx=20 | 水线上 300 px、横向 ±150 px 内每 tick 5 伤害 |
| `voodooDoll` | 选目标后拉拽 | 默认 extent 10 | 镜头延迟后把保存的投掷速度一次性写给目标 |
| `woodenCrate` | 连续放置 | 16/15 非对称 extent，共 3 个 | 作为碰撞障碍；爆炸只播放销毁时间轴 |
| `anchor` | 选 X 坐标 | L/R 48、top 96、bottom 0；y=-200、vy=40 | 落地对特定矩形内角色造成 60，停 30 tick 后淡出 10 tick |

## 6. 逐武器行为规格与 Unity 映射

### 6.1 Cherry Bomb

| ID | 可观察行为 | 原版来源 | Unity 入口 | 状态 |
| --- | --- | --- | --- | --- |
| CHB-PHY-01 | extent 四向均为 9，`hitsBoxes=true`，可 twang | `CherryBomb.as::constructor` | `MutinyCherryBomb.Initialize`/基类 | 已实现 |
| CHB-HIT-01 | 发射后任何 Solid 接触立即隐藏并生成 80/40 爆炸、播放 `pop` | `CherryBomb.as::contact` | `OnContact → Explode` | 已实现；待运行验证 |
| CHB-FX-01 | 未结束期间每 tick 生成一份 `cannonSmokeTrail`，源码没有 `fired` 条件 | `CherryBomb.as::advance` | `MutinyCherryBomb.EmitOriginalSmokeTrail`，装备阶段也生成 | 已接入；待 Unity 运行验证 |
| CHB-WATER-01 | 入水只来自共享 splash/越界逻辑，不等价于 Solid contact | `Weapon.as::advance`、`Solid.as::splashCheck` | 当前把 Water 传给 `OnContact` 并会爆炸 | 已知差异，需原版运行确认入水表现 |

### 6.2 Dynamite

| ID | 可观察行为 | 来源 | Unity 入口 | 状态 |
| --- | --- | --- | --- | --- |
| DYN-PHY-01 | extent 11、friction 1.7；飞行时旋转 `vx*2` | `Dynamite.as` | `MutinyDynamite` + rotation rules | 已实现 |
| DYN-END-01 | `vx==0 && abs(vy)<.2` 时生成 250/70 爆炸 | `Dynamite.as::advanceMotion` | `AdvanceOriginalDetonationTick → Explode` | 已实现；隔离 Unity Play Mode 水中停稳专项通过 |
| DYN-ANI-01 | 构造后无论是否已发射都播放 `lit`；可见 frame 1..4，frame 5 动作跳回 frame 1 | `Dynamite.as::Dynamite`；symbol 881 frame 5 | `MutinyDynamite.AdvanceOriginalPresentationTick` | ready 状态推进生产物理 tick，验证 1→2→3→4→1 | 已实现；Unity 定向回归通过 |
| DYN-ANI-02 | 入水停在 `unlit` frame 6 | `Dynamite.as::advanceMotion`；symbol 881 frame 6 | `MutinyDynamite.OnWaterSubmerged` | 真实水线进入后继续推进，画面保持 frame 6 | 已实现；Unity 定向回归通过 |
| DYN-WATER-01 | `y>water.y` 只执行 `gotoAndStop("unlit")`；源码没有“熄灭后禁止爆炸”分支 | `Dynamite.as::advanceMotion` | `OnWaterSubmerged` 只切帧；停稳仍 `Explode` | 静态确认、已实现；隔离 Unity Play Mode 专项通过 |
| DYN-WATER-02 | 入水后不按时间/深度作为 dud 消失，最终由真实静止或地图底部结束 | `Dynamite.as::advanceMotion`、`Weapon.as::advance` | `AdvanceOriginalDetonationTick`、`AdvanceInheritedFinishTick` | 已实现；隔离 Unity Play Mode 水中停稳爆炸专项通过 |
| DYN-FX-01 | 未结束期间每 tick 生成烟迹，源码没有 `fired` 条件 | `Dynamite.as::advance` | `MutinyDynamite.EmitOriginalSmokeTrail`，装备阶段也生成 | 已接入；待 Unity 运行验证 |

### 6.3 Banana

| ID | 可观察行为 | 来源 | Unity 入口 | 状态 |
| --- | --- | --- | --- | --- |
| BAN-PHY-01 | extent 7、bounce .8、friction .5、twangMaxForce 30；正常 twang 提交不经过 `Weapon.release` 的 20 上限 | `Banana.as::constructor`、`TileSystem.mouseUp`、`Solid.twang` | `MutinyBanana` 继承共享 `Twang` | 已实现；自动回归已写，待 Unity 运行验证 |
| BAN-TRAJ-01 | 预览与实际发射共用 30 力初速和“先加 weight、再位移”的 25 Hz 步进顺序 | `Solid.as::drawTwangLine/twangPrediction/advanceMotion` | `MutinyTrajectoryRenderer.PredictVelocityTick`、`MutinyPhysicsBody.AdvanceSimulationTick` | 满拉力连续比较前 5 tick 的预览点与实际位置 | 已实现；自动回归已写，待 Unity 运行验证 |
| BAN-HIT-01 | 每次 Solid 接触只播放 `banana_bounce`，不会按反弹次数自动爆炸 | `Banana.as::contact` | `OnContact` | 已实现 |
| BAN-DET-01 | 静止时自动爆；人类玩家飞行中下一次全局鼠标按下触发爆炸 | `Banana.as::advanceMotion/fire` | `TryRequestPlayerDetonation` | 已实现；待运行验证 |
| BAN-AI-01 | AI 最近角色距离 `<400` 时爆；“开始远离且 `<2500`”分支存在，但本 SWF 的 `lastSqDistance` 未更新，实际不可达 | `Banana.as::advanceMotion` | 保留 Infinity，不发明历史更新 | 已实现；静态确认 |
| BAN-LIFE-01 | 长途飞行不受固定 150 tick、8 秒、横向坐标或入水短计时中断；香蕉自身引爆判断之后，仅在下降越过 `levelHeight × 32` 时按继承 `Weapon.advance` 结束 | `Banana.as::advanceMotion`、`Weapon.as::advance` | `MutinyBanana.AdvanceOriginalTick`、`MutinyTurnManager.AdvanceSimulationTick` | 已实现；隔离 Unity Play Mode 205 tick 长飞行专项通过 |

### 6.4 Boulder

| ID | 可观察行为 | 来源 | Unity 入口 | 状态 |
| --- | --- | --- | --- | --- |
| BLD-PHY-01 | extent 31、weight 1.5、friction .25；旋转子层 `vx*2.5` | `Boulder.as` | `MutinyBoulder` | 已实现 |
| BLD-HIT-01 | 不命中 owner；纵向与巨石中心 ±32 且横向距离 ≤32 时，把角色推到巨石边缘 | `Boulder.as::advanceMotion` | `ApplyCharacterContacts` | 已实现；待运行验证 |
| BLD-DMG-01 | 每个接触 tick 伤害为 `abs(vx)*1.5`，只沿巨石当前运动方向追加横向速度 | 同上 | 同上 | 已实现 |
| BLD-END-01 | 无爆炸；继承 Weapon 的静止/地图底部结束。源码另有白化计数，但共享静止检查会在同 tick 结束，不能单独假定完整淡出 | `Boulder.as` + `Weapon.as` | `AdvanceOriginalTick` | 已实现；运行时序待验证 |

### 6.5 Cannon / Cannonball

详细规格见 [09-Cannon](09-Cannon/README.md)。核心结论：大炮和炮弹是独立对象；范围圆对象和实际 120 px 部署约束统一以角色 `(x,y-100)` 为中心，使原始 100 px 圆的底部落在角色坐标，并保持范围提示独立于炮身变换。炮身默认四向 extent 10，持续按住左键拖动时通过 Solid 碰撞步移动；快速指针采样越界会钳到圆周但不再伪造松手（这是相对原版 130 px 自动释放的授权差异）。拉栓局部 X 为 `[-40,-21]`，松开 `<-30` 才提交；回弹到 -21 后以 30 速度发射并播放 Unity 资源键 `cannon_explosion`。炮弹 weight 0、hitsBoxes=true，碰到地形/箱体/非 owner 角色立即 100/50 爆炸；源码没有“穿透障碍”的行为。

### 6.6 Gunpowder Barrel

| ID | 可观察行为 | 来源 | Unity 入口 | 状态 |
| --- | --- | --- | --- | --- |
| GPB-COUNT-01 | `createMore=1` 表示首个之后再建 1 个，总数为 2 | `GunpowderBarrel.as`、`BoxWeapon.as::place` | `OriginalPlacementCount=2` | 已实现 |
| GPB-PLACE-01 | extent 为 left/top 16、right/bottom 15；位置内不能有 tile、箱、宝箱或角色，并计算下方首个阻挡面 | `BoxWeapon.as::canPlace` | `MutinyGunpowderBarrel.CanPlace` | 已实现；待运行验证 |
| GPB-CUR-01 | 待放置时使用原版火药桶鼠标；非法点切 `cross` 并拒绝放置；第一桶后仍保持输入直至第二桶 | `TileSystem.as::advance`、`BoxWeapon.as::advance/place`、cursor 1813 frames 11/70 | `MutinySpecialWeaponCursor`、`HasPendingBoxPlacement` | 已实现；待运行验证 |
| BOX-PLC-01/02 | 光标和点击统一检查 pending 子对象；非法按下整次忽略且不改变已有桶，必须重新点击合法位置 | `BoxWeapon.as::canPlace`；原版运行行为 | `CanPlaceNext`、`ShouldHandleWeaponReadyPrimaryInput`、`TryActivateClickWeapon`、`TryPlaceAt` | 已实现；待运行验证 |
| BOX-WAIT-01 | 连续摆放阶段没有等待超时；第一桶后不能被非原版 150 tick 卡死武器保护强制结束 | `BoxWeapon.as::advance/place` | `MutinyTurnManager.AdvanceSimulationTick` | 已实现；待运行验证 |
| BOX-CAM-01 | 每次摆放同步 `track=false`，等待下一桶时不跟随桶、不回角色并直接允许手动滚屏 | `Weapon.as::place`、`BoxWeapon.as::place`、`TileSystem.as::advanceScrolling` | `IsAwaitingBoxPlacement`、镜头 action-target/manual-scroll 门 | 已实现；待运行验证 |
| BOX-SUP-01 | 下方承托面扫描为横向任意列命中，不要求全宽承托 | `BoxWeapon.as::canPlace` | `CanPlace` 横向 support scan | 已实现；待运行验证 |
| GPB-BOX-01 | 木箱和火药桶共同属于 `Controller.boxes`；所有 `hitsBoxes` Solid（含角色、炮弹等武器）把它们作为地形式 AABB 障碍 | `BoxWeapon.as::canPlace/place`、`Solid.as::advanceMotion` | `MutinyBoxRegistry`、`MutinyPhysicsBody.AdvanceSimulationTick` | 已实现；`BOX-COL-01` 已加入生产物理入口回归，待 Unity 运行验证 |
| GPB-LIFE-01 | `limitedToTurn=false`；放置后进入全局 boxes 并受普通 Solid 重力 | `BoxWeapon.as` | `MutinyBoxRegistry.Register` 与 PhysicsBody | 已实现；跨回合待验证 |
| GPB-RESET-01 | 重开或进入下一关时，旧关卡全部箱/桶状态在新关构建前同步清空 | `TileSystem.as::readXML`：`Controller.boxes=[]` | `MutinyLevelController.ClearLevel`、`MutinyLevelBuilder.BuildLevel` | 已实现；`BOX-LVL-01` 待 Unity 运行验证 |
| GPB-EXP-01 | 外部爆炸命中调用 `explode`，先从 boxes 移除，再生成 150/30、caster=null 爆炸 | 两类 AS2 | `MutinyExplosion.ApplyHit → MutinyGunpowderBarrel.Explode` | 已实现；连锁待运行验证 |

### 6.7 Mine

| ID | 可观察行为 | 来源 | Unity 入口 | 状态 |
| --- | --- | --- | --- | --- |
| MIN-PHY-01 | extent 14、friction 1.5、dragRange 180、`limitedToTurn=false` | `Mine.as::constructor` | `MutinyMine.Initialize` | 物理已实现；dragRange 表现待核对 |
| MIN-ARM-01 | `in_throw` 停 frame 1；静止时播放 arm 11..16 并停在 16 | `Mine.as::constructor/advanceMotion`；symbol 1024 frame labels/actions | `StartArmAnimation`、`AdvancePresentationTick` | 已实现；`MIN-ANI-01` 待 Unity 运行验证 |
| MIN-WARN-01 | 激活立即进入 warn；可见帧 21..29 循环，frame 30 动作跳回 warn，形成红灯闪烁 | `Mine.as::checkForProximity`；symbol 1024 frame 30 | `StartWarningAnimation`、`AdvancePresentationTick` | 已实现；`MIN-ANI-01` 待 Unity 运行验证 |
| MIN-TRG-01 | 角色脚点距雷 `<60` 且角色在移动时激活；正在拉 Throw Self 的静止角色也激活，普通静止角色不激活 | `Mine.as::checkForProximity` 的 velocity/`Controller.twanging` 条件 | `TryActivateForCharacter`、`NotifyCharacterBeganSelfThrowAim` | 已实现；`MIN-TRG-01/02` 待 Unity 运行验证 |
| MIN-AIM-01 | 触发和爆炸不清除当前 Throw Self 拉线；被炸到空中仍可松开提交 | Mine 只读 `Controller.twanging`，不写该状态 | PlayerInput Aiming 状态保持与 `TryCommitCharacterThrow` | 已实现；待 Unity 运行验证 |
| MIN-TMR-01 | 激活后倒数 60 tick；经过 tick `[0,15,30,38,45,49,53,55,57,59]` 蜂鸣；到 0 爆炸 250/70 | `Mine.as::advance/explode` | `PlayInitialBeep`、倒计时调度、`Explode` | 已实现；十次 beep 与空中松开回归已写，待 Unity 运行验证 |
| MIN-AUD-01 | 原版投掷/arm 无专用声音；只有 warn 的 `mine_beep` 与爆炸 `pop` | `Mine.as`；symbol 1024 无 StartSound | Mine 跳过通用 launch `click` | 已实现；待 Unity 运行验证 |

### 6.8 Parachute Bomb

| ID | 可观察行为 | 来源 | Unity 入口 | 状态 |
| --- | --- | --- | --- | --- |
| PCB-PHY-01 | extent 11、twangMaxForce 30；每 tick 若 vy>1 则减 2 但不低于 1，随后走重力；vx×.95 | `ParachuteBomb.as` | before-simulation tick | 已实现 |
| PCB-CHUTE-01 | 非模拟状态下 `vy>-10` 首次开伞；并非“到最高点才开伞” | 同上 | `ChuteOpenVelocityThreshold=-10` | 已实现 |
| PCB-ANI-01 | 外层 closed 帧停止时，内层四帧引信仍循环；开伞后显示 11..29，frame 30 的跳转动作不渲染，开放状态循环 26..29 | symbol 939/930；frame 1/11/30 actions | `AdvanceClosedFusePresentation`、`AdvanceChuteAnimation` | 已实现；待运行验证 |
| PCB-CUR-01 | 人类炸弹飞行期间使用四帧 fan 鼠标；按相对炸弹的左右方向旋转 ±90°，按住播放、松开停止 | `TileSystem.as`、cursor symbol 1813/1806 | `MutinySpecialWeaponCursor`、ActionExecuting 输入门 | 已实现；待运行验证 |
| PCB-FAN-01 | 人类按住鼠标时，根据鼠标在炸弹左右反向加 `vx ±=.2`；每 12 tick 播放 fan | `ParachuteBomb.as` | `ApplyFanInput` | 已实现；待运行验证 |
| PCB-HIT-01 | Solid 接触爆炸 160/50；上边界 y 限制为 -300 | 同上 | `OnContact`、ceiling clamp | 已实现 |
| PCB-LIFE-01 | 飞行无固定时长上限；仅遵循 `Weapon.advance` 的地图底部/静止结束与自身 Solid 接触爆炸，不受非原版 150 tick 卡死恢复销毁 | `Weapon.as::advance`、`ParachuteBomb.as::contact/advance` | `MutinyParachuteBomb.AdvanceOriginalTick`、`MutinyTurnManager.AdvanceSimulationTick` | 已实现；待运行验证 |

### 6.9 Pieces of Eight

| ID | 可观察行为 | 来源 | Unity 入口 | 状态 |
| --- | --- | --- | --- | --- |
| POE-COUNT-01 | 一次库存武器连续投出 8 枚，不是 3 枚 | `PiecesOfEight.as::next` | `TotalCoins=8` | 已实现 |
| POE-HIT-01 | Solid 接触产生 50/25 爆炸；到达水面调用 `next(false)`，不爆炸 | `contact/advance/next` | `ResolveCoin` | 已实现；待运行验证 |
| POE-LOOP-01 | 前 7 枚结束后复用同一对象，回到 owner `(x,y+5)`，锁定换武器并重新瞄准；第 8 枚后隐藏结束 | 同上 | `IsAwaitingNextCoin`、`WeaponLocked` | 已实现；待运行验证 |
| POE-CAM-01 | 每枚飞行时 `track=true`，镜头显式跟随当前复用的钱币实例；下一枚发射会清除尚存的角色回移目标并重新跟随 | `Weapon.as::fire/twang`、`TileSystem.as::advanceScrolling:455-459` | `RequestTrackWeapon(this)`、`m_TrackedWeapon` | 已实现；C# 编译通过，待运行验证 |
| POE-CAM-02 | 前 7 枚结算立即 `track=false` 并 `panToCharacter=owner`；回移期间钱币和仍在运动的 owner 都不得重新抢占 action target | `PiecesOfEight.as::next:127-146`、`TileSystem.as::advanceScrolling:461-468` | `ReleaseWeaponTracking`、`RequestPanToCharacter`、awaiting guard | 已实现；C# 编译通过，待运行验证 |
| POE-CAM-03 | `panToCharacter` 抵达并清空后，不受 `ActionExecuting` 总锁影响，鼠标边缘、方向键、WASD 均可滚屏 | `TileSystem.as::advanceScrolling:463-493` | `CanUseManualScrolling`、`AdvanceEdgeScrolling` | 已实现；C# 编译通过，待运行验证 |
| POE-AI-01 | AI 每枚间隔 20 tick，再从 10 次随机投掷中评分 | `aiContinue` | Unity AI re-aim | 已实现；随机一致性待验证 |

### 6.10 Rum Bottle / Sweeping Flame

| ID | 可观察行为 | 来源 | Unity 入口 | 状态 |
| --- | --- | --- | --- | --- |
| RUM-PHY-01 | extent 14、twangMaxForce 30；正常松手走 `TileSystem.mouseUp → Weapon.twang → Solid.twang`，实际初速上限 30；飞行旋转 `vx*2` | `RumBottle.as`、`TileSystem.as::mouseUp`、`Solid.as::twang` | `MutinyRumBottle.Twang` | 已实现；待运行验证 |
| RUM-TRAJ-01 | 预览和正常松手共用 30 上限及逐 tick 先加 weight=1 再移动的轨迹 | `Solid.as::drawTwangLine/twangPrediction/advanceMotion` | `MutinyTrajectoryRenderer`、`MutinyRumBottle.Twang`、`MutinyPhysicsBody.AdvanceSimulationTick` | 已实现；待运行验证 |
| RUM-HIT-01 | 任一 Solid 接触产生 80/25 爆炸；只有 `side==FLOOR` 才生成火焰 | `RumBottle.as::contact` | `Explode(side)` | 已实现 |
| RUM-HIT-02 | `Solid.advanceMotion` 先处理纵向碰撞并立即调用 `contact`，再处理横向位移；地面/天花板爆炸中心和地面火焰格取横移前 X，墙面碰撞取横移后 X | `Solid.as::advanceMotion`、`RumBottle.as::contact` | `MutinyRumBottle.OnContact`、`Explode` | 已实现；待运行验证 |
| RUM-FIRE-01 | 地面格向上找到表面后，在同一点创建左右各一个 SweepingFlame | 同上 | `FindOriginalFlameOrigin` + 双 Spawn | 已实现 |
| RUM-FIRE-02 | 每段火焰创建时，脚点距离平方 `<64` 的角色受 30 伤害，速度改为随机 `vx∈[-4,4)`、`vy∈[-8,-6)` | `SweepingFlame.as::constructor` | `MutinySweepingFlame` | 已实现；随机边界待验证 |
| RUM-FIRE-03 | 时间轴第 4 帧沿方向前进 8 px；下一格必须有实体且其上方为空，否则停止 | `SweepingFlame.as::createNext` + 时间轴 | propagation tick | 已实现；待运行验证 |

### 6.11 Seagull

| ID | 可观察行为 | 来源 | Unity 入口 | 状态 |
| --- | --- | --- | --- | --- |
| SEA-PLACE-01 | 未发射时水平虚线跟随鼠标 Y；首次点击在 `x=-300`、所选 Y 开始，vx=10 | `Seagull.as::constructor/place/advance` | `PlaceAtFlightHeight` | 飞行逻辑已实现；虚线表现另验 |
| SEA-SHOT-01 | 飞行中每次点击都可投一枚弹，没有固定弹数；弹起点 `(bird.x-10,bird.y)`，继承 vx=10、weight=1 | `Seagull.as::advance` | `TryRequestPlayerShot`、`MutinySeagullFire.Spawn` | 已实现 |
| SEA-SHOT-02 | 投弹输入在海鸥本 tick 完成移动后消费；新弹从 `bird.x-10` 创建，并在同一个 `Seagull.advance` 中立刻前进一次，之后每 tick 继续由海鸥统一推进 | `Seagull.as::advance` 中 `super.advance`、创建 shot、遍历 `shots[].advance` 的顺序 | `RequestShot`、`AdvanceOriginalTick`、`MutinySeagullFire.AdvanceOriginalTick` | 已实现；待运行验证 |
| SEA-VIS-01 | 炸弹 `show()` 后海鸥立刻 `hide(); show()`，在相同 Character 层重新取得更高深度，因此重叠时海鸥遮住炸弹 | `Seagull.as::advance`、`Clip.as::show` | `MutinySeagullFire.Initialize` 的 SpriteRenderer 排序 | 原版静态确认；已修复，待 Unity 运行验证 |
| SEA-ANI-01 | 飞行显示帧 1..8，帧 9 的 Action 立即跳回 `flying`，透明帧 9/10 不显示；`shot` 标签在帧 11，投弹显示 11..14 后回到帧 1 | DefineSprite 982 时间轴、frame 9 `DoAction.as`、原始帧图 | `MutinySeagull.SpawnShot/AdvanceAnimation` | 原版静态确认；已修复，待 Unity 运行验证 |
| SEA-HIT-01 | 弹碰 Solid 爆炸 50/50；落水只销毁不爆 | 动态 shot 函数 | `MutinySeagullFire` | 已实现；待运行验证 |
| SEA-END-01 | 鸟越过 `levelWidth*32+275` 且所有弹已结束，武器才结束 | `Seagull.as::advance/endShot` | `AdvanceOriginalTick` | 已实现 |
| SEA-END-02 | 原版没有飞行时限；即使阻塞回合超过旧 Unity 的 150 tick 安全阈值，也不得强制回收正常飞行的海鸥 | `Seagull.as::advance` 仅有 `levelWidth*32+275 && shots.length<1` 完成条件 | `MutinyTurnManager.AdvanceSimulationTick` | 已实现；待运行验证 |

### 6.12 Tidal Wave

| ID | 可观察行为 | 来源 | Unity 入口 | 状态 |
| --- | --- | --- | --- | --- |
| TID-START-01 | 任意点击从 `(-550,waterY)` 开始，固定 vx=20，动画按 skyColour 选 `anim1..3` | `TidalWave.as::startWave` | `StartWave` | 已实现 |
| TID-HIT-01 | 每 tick 遍历存活角色：`y>=waterY-300` 且 `x∈[waveX-150,waveX+150]` 时扣 5 | `TidalWave.as::advance` | `ApplyCharacterDamage` | 已实现；不是推水冲量 |
| TID-END-01 | `x>levelWidth*32+550` 时隐藏结束 | 同上 | `AdvanceOriginalTick` | 已实现 |

### 6.13 Voodoo Doll

| ID | 可观察行为 | 来源 | Unity 入口 | 状态 |
| --- | --- | --- | --- | --- |
| VOO-TGT-01 | 初始不可 twang；先选择一个角色，随后启用 twang 并把镜头拉回 owner | `VoodooDoll.as::setTargetCharacter` | `BindTarget` + 玩家输入 | 已实现；敌我资格由输入层验收 |
| VOO-CUR-01 | 人类未选定目标、未发射且鼠标不在娃娃自身 hitTest 内时显示 `voodooDoll` 光标；其它时刻恢复普通鼠标 | `TileSystem.as::advance`、DefineSprite 1813 `voodooDoll` 标签帧 40 | `UpdateSpecialWeaponCursor`、`MutinySpecialWeaponCursor` | 原版静态确认；已修复，待 Unity 运行验证 |
| VOO-THR-01 | 提交时保存娃娃最初 `velocityX/Y` | `VoodooDoll.as::twang` | `m_ThrowVelocity` | 已实现 |
| VOO-CAM-01 | 选定目标时设置 `panToCharacter=owner`；娃娃发射后 `track=true`，镜头跟随娃娃 | `VoodooDoll.as::setTargetCharacter`、`Weapon.as::twang` | `BindTarget`、`Fire`、`RequestTrackWeapon` | 已实现；待运行验证 |
| VOO-CAM-02 | 飞行 10 tick 后在同一状态转换中设置 `panToCharacter=targetCharacter`、`track=false`；镜头到达并清除该平移目标后再等待 10 tick | `VoodooDoll.as::advance`、`TileSystem.as::advanceScrolling` | `ReleaseWeaponTracking`、`RequestPanToCharacter`、target-pan gate | 已实现；待运行验证 |
| VOO-CAM-03 | 目标获得速度后不持续跟随目标，也不得由通用未完成武器兜底重新跟随娃娃；镜头恢复普通手动滚屏 | `VoodooDoll.as::advance` 保持 `track=false`，且 `panToCharacter` 到达后已清空 | `FindActionTarget` 的 Voodoo handoff guard | 已实现；待运行验证 |
| VOO-END-02 | 等待远距离目标运镜期间没有时间上限，不受非原版 150 tick 卡死武器看门狗强制回收 | `VoodooDoll.as::advance` 以 `panToCharacter` 清除为门，未定义超时 | `MutinyTurnManager.AdvanceSimulationTick` | 已实现；待运行验证 |
| VOO-XFER-01 | 只把保存速度一次性赋给目标；不复制娃娃后续碰撞、伤害或位置 | 同上 | `TransferVelocityToTarget` | 已实现；纠正旧“100%伤害同调”描述 |
| VOO-END-01 | 速度传递后每 tick alpha 减 10，归零结束 | 同上 | `FadeAndFinish` | 已实现 |

### 6.14 Wooden Crate

| ID | 可观察行为 | 来源 | Unity 入口 | 状态 |
| --- | --- | --- | --- | --- |
| CRT-COUNT-01 | 默认 `createMore=2`，所以一次选择共放 3 个 | `BoxWeapon.as`、`WoodenCrate.as` | `OriginalPlacementCount=3` | 已实现 |
| CRT-PLACE-01 | 合法性与火药桶相同；每次按原始鼠标像素坐标放置，不自动吸附网格 | `BoxWeapon.as::canPlace/place` | `MutinyWoodenCrate.CanPlace/TryPlaceAt` | 已实现；待运行验证 |
| CRT-CUR-01 | 待放置时使用原版木箱鼠标；非法点切 `cross` 并拒绝放置；前两箱后保持输入 | `TileSystem.as::advance`、`BoxWeapon.as::advance/place`、cursor 1813 frames 50/70 | `MutinySpecialWeaponCursor`、`HasPendingBoxPlacement` | 已实现；待运行验证 |
| BOX-PLC-01/02 | 每次提示与提交指向同一 pending 箱；非法按下忽略整次请求并保留已放箱，合法位置必须重新点击 | `BoxWeapon.as::canPlace`；原版运行行为 | `CanPlaceNext`、`ShouldHandleWeaponReadyPrimaryInput`、`TryActivateClickWeapon`、`TryPlaceAt` | 已实现；待运行验证 |
| BOX-WAIT-01 | 前两箱后可无限等待下一次点击，不受非原版 150 tick 卡死武器保护影响 | `BoxWeapon.as::advance/place` | `MutinyTurnManager.AdvanceSimulationTick` | 已实现；待运行验证 |
| BOX-CAM-01 | 每次摆放同步 `track=false`，等待下一箱期间不跟随箱、不自动回角色，立即允许鼠标边缘/方向键/WASD 滚屏 | `Weapon.as::place`、`BoxWeapon.as::place`、`TileSystem.as::advanceScrolling` | `IsAwaitingBoxPlacement`、镜头 action-target/manual-scroll 门 | 已实现；待运行验证 |
| BOX-SUP-01 | 横向覆盖区只有部分下方存在承托也合法 | `BoxWeapon.as::canPlace` | `CanPlace` support scan | 已实现；待运行验证 |
| CRT-LIFE-01 | `limitedToTurn=false`；进入共享 boxes，作为角色跳跃和所有 `hitsBoxes` 武器运动的地形式障碍 | `BoxWeapon.as`、`Solid.as::advanceMotion` | `MutinyBoxRegistry`、`MutinyPhysicsBody.AdvanceSimulationTick` | 已实现；`BOX-COL-01` 待 Unity 运行验证 |
| CRT-RESET-01 | 重开或进入下一关时，旧关卡箱状态不进入新关卡 | `TileSystem.as::readXML`：`Controller.boxes=[]` | `MutinyLevelController.ClearLevel`、`MutinyLevelBuilder.BuildLevel` | 已实现；`BOX-LVL-01` 待 Unity 运行验证 |
| CRT-EXP-01 | 同一爆炸命中调用先写角色伤害/冲量，再从 boxes 移除命中木箱；角色首次按爆炸速度移动时不再与该箱碰撞 | `Explosion.as::hit`、`BoxWeapon.as::explode` | `MutinyExplosion.ApplyHit` → `MutinyWoodenCrate.Explode` | 已实现；待运行验证 |
| CRT-EXP-02 | 木箱立即从 `explode` 标签第 11 帧开始，至第 18 帧 `cl.destroy()`；破损时间轴与角色飞行并行，本身不生成爆炸 | symbol 965 时间轴；frame 18 `DoAction.as` | `OriginalExplodeFirstFrame`、`AdvanceExplosionTimelineFrame` | 已实现；待运行验证 |

### 6.15 Anchor

| ID | 可观察行为 | 来源 | Unity 入口 | 状态 |
| --- | --- | --- | --- | --- |
| ANC-PLACE-01 | 点击只使用 X；Y 被强制改为 -200，下降前不可见 | `Anchor.as::place/constructor` | `DropAt` | 已实现 |
| ANC-CUR-01 | 人类选择 Anchor 且未投放时显示 `anchor` 光标，投放或取消后恢复普通鼠标 | `TileSystem.as::advance`、DefineSprite 1813 `anchor` 标签帧 1 | `UpdateSpecialWeaponCursor`、`MutinySpecialWeaponCursor` | 原版静态确认；已修复，待 Unity 运行验证 |
| ANC-PHY-01 | extent 为 left/right 48、top 96、bottom 0；每 tick 强制 vy=40 | `Anchor.as::advance` | `AdvanceOriginalTick` | 已实现 |
| ANC-HIT-01 | 首次 Floor 接触，对 `abs(char.x-anchor.x)<48` 且 `anchor.y-64<char.y<anchor.y` 的角色造成 60 | `Anchor.as::contact` | `HitFloor` | 已实现 |
| ANC-END-01 | 落地动画开始，hold 30 tick，再 whiteOut 10 tick，随后隐藏结束 | 同上 | impact timeline | 已实现；隔离 Unity Play Mode 专项通过 |
| ANC-ANI-02 | 落地 frame 3 生成镜像的双侧 1002 子时间轴，子 frame 17 移除；下落中只显示主 frame 1 | `Anchor.as::contact`；1003/1002 时间轴 | `MutinyAnchor.AdvanceImpactTimelineTick` | 已实现；隔离 Unity Play Mode 专项通过 |
| ANC-ANI-03 | 30 tick hold 后按 `Global.whiteOut` 逐 tick 先加白再淡出 | `Anchor.as::advance`；`Global.as::whiteOut` | `MutinyAnchor.ApplyWhiteOut`、`SpriteColorTransform.shader` | 已实现；隔离 Unity Play Mode 参数专项通过；实际画面对照待验收 |
| ANC-AI-01 | AI 先 place/show，再等待 20 tick 才开始下落 | `Anchor.as::aiPerform/advance` | `DropForAi` | 已实现；待运行验证 |

## 7. 当前已知差异与实现优先级

| 优先级 | 差异 | 影响 | 建议修复入口 |
| --- | --- | --- | --- |
| P0 | RumBottle 仍被二次截速到 20，原版 twangMaxForce 为 30 | 射程和 AI/玩家落点错误 | 移除 `Twang` 中额外的 20 截断；补 30 力生产入口用例 |
| P1 | CherryBomb 的 Water 被当作 contact 爆炸 | 原版静态路径不支持该结论 | 原版运行对照；不要仅据 Unity 现状定规格 |
| P1 | Mine 忽略所有静止角色，未保留“当前 twanging 的静止角色仍触发”例外 | 少数交互时序不一致 | 给 `CheckForProximity` 传入当前 twang 目标 |
| P2 | Boulder 的 `Global.whiteOut` 加色阶段仍由普通 SpriteRenderer 近似；Anchor 已使用专用材质 | Boulder 淡出颜色不完全一致；Anchor 需实际画面对照 | Boulder 专用材质；Anchor 手动截图验收 |

## 8. 验收矩阵

所有用例必须从 `MutinyPlayerInput`、`MutinyAIController` 或真实回合入口驱动，不能只手改武器私有布尔量证明流程通过。

| 用例 ID | 生产路径 | 核心断言 | 当前结果 |
| --- | --- | --- | --- |
| WPN-T01 | 装备→取消→再装备 | 不消耗库存；未提交对象销毁 | 已有回归；本轮未运行 |
| WPN-T02 | 普通 20 力与三种 30 力完整拉拽 | 实际初速分别达到上限 | 需补 Banana/Rum 修复回归 |
| WPN-T03 | 15 个 ID 从工厂生成 | 类型、owner、初始偏移正确 | 部分已有；本轮未运行 |
| WPN-T04 | 每种爆炸在第 3 帧结算 | 伤害、冲量、箱体连锁只发生一次 | 已有局部回归；本轮未运行 |
| WPN-T05 | Banana 飞行中全局点击 | 点击优先被香蕉消费并在下一原版 tick 爆炸 | 已有 `VerifyBanana`；未运行 |
| WPN-T06 | Mine 跨回合 | 静止登记、运动目标激活、蜂鸣序列、60 tick 爆炸 | 缺独立生产入口回归 |
| WPN-T07 | PiecesOfEight 完整 8 发 | 前 7 发锁武器，第 8 发才结束且只消耗 1 库存 | 已有 `VerifyPiecesOfEight`；未运行 |
| WPN-T08 | Seagull 连续多次点击 | 每次生成独立弹；鸟离场后等待所有弹结束 | 已有 `VerifySeagull`；未运行 |
| WPN-T08A | Seagull 长地图飞行与同 tick 投弹 | 超过 150 回合管理 tick 仍存活；点击后的下一生产物理 tick 中，鸟先移动、弹以 `x-10` 创建并立刻推进一次，且不会由子组件重复推进 | 已有 `VerifySeagull`；未运行 |
| WPN-T09 | RumBottle 地面/墙面各一次 | 只有地面产生双向 8 px 扫火 | 已有 `VerifyRumBottle`；未运行 |
| WPN-T10 | 两桶/三箱连续放置 | 合法性、数量、跨回合、连锁爆炸正确 | 已有回归；未运行 |
| WPN-T10A | 角色与木箱被同一爆炸命中 | 先写角色冲量，再移除箱碰撞；木箱立即为 frame 11，角色首个物理 tick 与破损 11..18 并行 | 已有 `CRT-EXP-01/02` 回归；未运行 |
| WPN-T11 | Voodoo 完整镜头门控 | 10+10 tick 后仅复制一次初速，无直接伤害 | 已有 `VerifyVoodooDoll`；未运行 |
| WPN-T12 | Anchor 玩家与 AI | 点击 Y 被忽略；AI 延迟 20 tick；60 伤害和 30+10 收尾 | 已有 `VerifyAnchor`；未运行 |

## 9. 本轮完成状态

- 静态确认：15 个库存武器、公共 twang/爆炸规则、全部专用 AS2 状态机和关键常量。
- 已实现：Unity 已有 15 种武器类、炮弹/火焰子效果、工厂、输入与 AI 接口。
- 实际测试通过：2026-09-25，Unity 6000.6.0f1 隔离 Play Mode 武器生命周期专项 16/16 断言通过，覆盖 `BAN-LIFE-01`、`WPN-LIFE-01/02`、`WPN-WATER-01`、`DYN-WATER-02`。
- 待运行验证：第 8 节其他用例、主工程真实关卡画面及长时间连续运行表现；专项逻辑通过不等于全部武器已完成实机验收。
- 已知差异：第 7 节；在修复前不得把相关武器标记为原版一致。
