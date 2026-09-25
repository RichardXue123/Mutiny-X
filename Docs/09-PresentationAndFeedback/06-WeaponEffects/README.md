# 09.06 · 武器画面效果

[返回上级模块](../README.md)

## 职责

表现武器时间轴、粒子、火焰、烟雾和特殊效果。

## 边界

命中时机由武器逻辑事件决定。

## 原版表现模型

原版不使用统一粒子系统，而是由导出名称的 MovieClip 、嵌套子时间轴和帧脚本组成。逻辑帧必须与画面帧同步：例如 explosion 第 3 帧 `cl.hit()`，sweepingFlame 第 4 帧 `cl.createNext()`，效果结束帧再 `cl.destroy()`。

Unity 主要用 PNG 序列 + `SpriteRenderer` 复现这些时间轴，以 `MutinyPhysics.TimeStep`/25 Hz 推进。

## 通用效果规格

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| VIS-EXP-01 | explosion 按 `size/100` 等比缩放；frame 3 命中，frame 8 停止并销毁 | `Explosion.as`；symbol 1785 frame 3/8 | `MutinyExplosion.Spawn`、`Update`、`ApplyHit` | 从真实武器碰撞生成，断言缩放、帧号、一次命中与销毁 tick | 已实现；局部回归已写，本轮未运行 |
| VIS-SMOKE-01 | Cannonball、CherryBomb、Dynamite、ParachuteBomb（未开伞）、RumBottle 的 `advance` 路径每 tick 创建 `cannonSmokeTrail`；烟迹 frame 19 销毁。Cannonball 的高刷新率可见轨迹另按 `CAN-SMOKE-02` 对齐 | 五个武器 AS2；symbol 866 frame 19 | `MutinyRumBottleSmokeTrail`；五种武器已调用 | 核对 19 帧原版资源、五种武器逐 tick 出烟、装备阶段出烟及开伞后停烟 | 已接入；隔离工程 Unity Play Mode 19/19 大炮与共享烟迹专项断言通过，主工程画面待验收 |
| VIS-SMOKE-02 | Cherry Bomb、Dynamite、Rum Bottle、Parachute Bomb 的新烟团与该 tick 开始时的可见弹体重合；弹体在后续渲染帧向前移动而烟留在身后。ready 阶段也使用同一位置规则；开伞后仍不出烟 | 四个武器 AS2 的 `advanceMotion` 后 `advance` 出烟；用户授权的高刷新率弹体显示插值，参照 `CAN-SMOKE-02` | `MutinyPhysicsBody.CurrentStepStartPositionPixels`、四种武器的生产出烟回调 | 正式武器物理 tick 后检查烟团与可见弹体同位，半 tick 表现采样后检查烟仍在轨迹后方；分别覆盖四种武器及 ready 态 | 已实现；2026-09-26 隔离 Unity Play Mode 19/19 专项断言通过；主工程画面待验收 |
| VIS-FLAME-01 | sweepingFlame 创建即命中附近角色，frame 4 沿地表传播 8 px，frame 11 销毁 | `SweepingFlame.as`；symbol 905 frame 4/11 | `MutinySweepingFlame` | RumBottle 地面碰撞生成左右两条，逐 tick 核对命中/传播/销毁 | 已实现；待 Unity 运行验证 |
| VIS-SPLASH-02 | 已发射且未完成的普通武器在注册点跨水线时生成 splash；水线穿越不调用 `contact`，CherryBomb 不因入水而爆炸；Boulder/Parachute Bomb/金币按各自原版 `advance` 保留专用调用，Cannonball 不走继承的 `Weapon.advance` | `Weapon.as::advance`、`Solid.as::splashCheck`、`CherryBomb.as::advance`、`Cannonball.as::advance` | `MutinyWeapon.AdvanceInheritedSplashCheck`、各武器 `AdvanceSplashCheck` | 对投射物分别测试相等水线、入水、出水、地形碰撞 | 已接入；待 Unity Play Mode 验证 |
| VIS-WPN-IDLE-01 | 可见武器在 ready/idle 阶段仍按原版时间轴播放：Cherry Bomb 1..4、Dynamite `lit` 1..4、Rum Bottle 1..12、Parachute Bomb 闭伞引信 1..4；动作帧不作为额外可见帧 | symbols 844/881/918/939；`Dynamite.as` 构造函数；symbol 881 frame 5/6 actions；symbol 939/930 | `MutinyCherryBomb`、`MutinyDynamite.AdvanceOriginalPresentationTick`、`MutinyRumBottle`、`MutinyParachuteBomb` | 武器保持 `IsFired=false`，推进生产 tick，核对帧循环；Dynamite 入水后固定 frame 6 | 已实现；Dynamite 定向 Unity 回归 2/2 通过 |

## Ready / idle 动画审计

| 武器 | 原版 ready 状态 | Unity 审计结果 |
| --- | --- | --- |
| Cherry Bomb | 4 帧主时间轴自动循环 | 已在未发射状态播放 |
| Dynamite | 构造函数 `gotoAndPlay("lit")`；可见帧 1..4，frame 5 跳回 `lit`；入水 `gotoAndStop("unlit")` 到 frame 6 | 已移除 `IsFired` 门控并排除动作帧 5；定向 Unity 回归通过 |
| Rum Bottle | 12 帧主时间轴自动循环 | 已由 25 Hz 表现 tick 在 ready 状态播放 |
| Parachute Bomb | 外层 frame 1 停止，但嵌套 fuse 仍以 4 帧循环 | 已在闭伞 ready 状态播放 |
| Banana、Boulder、Cannon、Pieces of Eight、Voodoo Doll | 单帧主体；变化来自交互、位移或发射后状态 | 不需要 idle 帧播放器 |
| Mine、Anchor、Wooden Crate、Gunpowder Barrel | 原版显式停在初始帧，事件发生后才播放 | 不需要 idle 帧播放器 |
| Seagull、Tidal Wave | ready 阶段主体隐藏/只显示选择辅助；提交后才显示动画主体 | 不需要可见武器 idle 帧播放器 |

## 武器时间轴盘点

| 武器/效果 | 原版可观察动画 | Unity 实现 | 状态 |
| --- | --- | --- | --- |
| Cherry Bomb | 4 帧弹体循环 + 每 tick 烟迹 + 80/40 explosion | `MutinyCherryBomb` 循环 4 帧，未结束时留烟，碰撞生成 explosion | 烟迹已接入；待运行验证 |
| Dynamite | `lit` 可见帧 1..4 循环，frame 5 跳回；入水停在 `unlit` frame 6；每 tick 烟迹 | `MutinyDynamite` 以生产物理 tick 推进 lit 帧，未结束时留烟 | ready 动画定向回归通过；烟迹待运行验证；入水 dud 仍是逻辑差异 |
| Banana / Pieces of Eight / Voodoo Doll | 主体是单帧，动态来自位移/旋转/透明度与爆炸 | 对应 `Mutiny*` 类使用单 Sprite；Voodoo 调 alpha | 已实现，待运行对照 |
| Boulder | 外层 overlay + 内层 rotating 复合旋转 | `MutinyBoulder` 用两个 SpriteRenderer 分层 | 已实现，待层级/角度对照 |
| Cannon / Cannonball | 炮身、pin、range circle 分离；炮弹在炮身上层并留烟 | `MutinyCannon` 分层组件；`MutinyCannonball` sortingOrder+1、逐 tick 留烟 | 烟迹已接入；待运行验证 |
| Gunpowder Barrel | 静态 frame 1；爆破直接跳 frame 11，frame 12 销毁 | `MutinyGunpowderBarrel` 加载 12 帧并跳标签 | 已实现，待运行验证 |
| Wooden Crate | 静态 frame 1；破损从 frame 11 播至 frame 18 销毁 | `MutinyWoodenCrate` | 已实现，待运行验证 |
| Mine | 投掷 frame 1；arm 11..16 并停；warn 21..29，frame 30 跳回 warn | `MutinyMine` 表现状态机 | 已实现，待运行验证 |
| Parachute Bomb | closed 外层停止但四帧引信子时间轴循环；未开伞时留烟；opening 11..29；30 跳回 open 26 | `MutinyParachuteBomb` 分开引信与开伞推进，未开伞时留烟 | 烟迹已接入；待运行验证 |
| Rum Bottle | 12 帧瓶体循环并留烟；地面命中生成双向 sweepingFlame | `MutinyRumBottle` + `MutinySweepingFlame`，未结束时留烟 | 烟迹已接入；待运行验证 |
| Seagull | flying 1..8 循环；frame 9 动作回 flying；shot 10..14 | `MutinySeagull` 分 flying/shot 帧段 | 已实现，待运行验证 |
| Tidal Wave | 根据 skyColour 选 anim1/2/3；每段 9 帧区间中前 5 帧可见循环，动作帧回标签 | `MutinyTidalWave` 加载 27 帧，每色显示 5 帧 | 已实现，待运行验证 |
| Anchor | 下落 frame 1 停止；命中后主帧播至 12，双侧 1002 碎屑各自播至 frame 17 移除，再 hold/whiteOut/隐藏 | `MutinyAnchor`、`SpriteColorTransform.shader` | 父子时间轴和白化参数隔离 Play Mode 专项通过；像素级画面对照待验收 |

各武器的逻辑规则与更精确的 AS2 条目见 [06 武器实现审计](../../06-WeaponsAndEffects/IMPLEMENTATION_DETAILS.md)和各武器子目录。

## 资源基线

- 已有通用效果：Explosion 8 帧、Splash 72 张导出图、SweepingFlame 11 帧、Water 30 帧。
- 已有复合武器帧：Anchor 主 12 + 撞击子 18、CherryBomb 4、Dynamite 13、GunpowderBarrel 12、Mine 30、ParachuteBomb 30、RumBottle 12、Seagull 14、TidalWave 27、WoodenCrate 18。
- 单帧/组件化资源：Banana、PiecesOfEight、VoodooDoll、SeagullFire、Cannonball；Boulder 和 Cannon 为多部件图层。
- 烟迹：`Assets/Mutiny/Resources/Art/Effects/CannonSmokeTrail/1..19` 已在项目中。

## 完成状态

- 静态确认：爆炸、扫火、烟迹、Mine、Parachute Bomb、Seagull、Tidal Wave、箱体与 Anchor 的关键帧脚本。
- 已实现：主要帧播放器与生产事件连接；本轮修正水花帧长和跨线调用。
- 实际测试通过：`VIS-WPN-IDLE-01/DYN-ANI-01/02` 定向 Unity batchmode 回归 2/2 通过；`VIS-SMOKE-01/02` 与 `CAN-SMOKE-02` 隔离 Unity Play Mode 共享烟迹专项 19/19 断言通过。
- 待运行验证：通用效果与上表所有时间轴的生产入口逐帧对照。
