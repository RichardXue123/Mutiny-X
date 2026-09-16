# WPN-08 — Parachute Bomb（降落伞炸弹）行为规格

## 基线与范围

- 模块 ID：WPN-08。
- 原版基线：`Mutiny Source/mutiny-flash-game/mutiny.swf`，SHA256 `c034d2cfa50272a1aac93ad75114b542ca5993dd7b70fa8901aa195de7820586`。
- Unity 入口：`Assets/Mutiny/Scripts/Simulation/MutinyWeaponFactory.cs` → `MutinyParachuteBomb.cs`；常规拖拽输入在 `Presentation/MutinyPlayerInput.cs`。
- 证据状态：静态确认；实现状态：实现中；验证状态：未执行。
- 范围：投掷、25 Hz 飞行、开伞、玩家扇风、碰撞爆炸、入水水花、烟雾及 sprite 时间轴。AI 的泛用抛物线选择和镜头跟随不在本次闭环范围。

## 证据与冲突

| 证据 ID | 类型 | 文件、符号 | 结论与适用条件 |
| --- | --- | --- | --- |
| WPN-08-S-01 | S | `.../throwgame/ParachuteBomb.as`：构造函数 | 四向 extent 均为 11，`hitsBoxes=true`、不可拖拽、可 twang，拖拽上限 30。 |
| WPN-08-S-02 | S | `ParachuteBomb.as`：`twangPrediction`、`advanceMotion` | 每 tick 先仅在 `vy>1` 时减 2（下限 1），`vx*=.95`，再由 `Solid.advanceMotion` 加 weight=1 并移动。`vy>-10` 时开伞。 |
| WPN-08-S-03 | S | `ParachuteBomb.as`：`advanceMotion` | 已发射且人类队伍时，左键持续按下：鼠标在炸弹左边令 `vx+=.2`，其余位置令 `vx-=.2`；每第 12 个发射 tick 播放 `fan`。 |
| WPN-08-S-04 | S | `ParachuteBomb.as`：`contact` | terrain 或箱体任一 contact 在已发射且未完成时立即隐藏本体，创建 `Explosion(x,y,160,50,owner)` 并播放一次 `pop`。 |
| WPN-08-S-05 | S | `Weapon.as`：`advance`；`Solid.as`：`splashCheck` | 水面穿越只产生 `splash` 和水花，不是 `contact`，因此不爆炸；`y > levelHeight*32 && vy>0` 或 `vx==0 && abs(vy)<.2` 结束。 |
| WPN-08-S-06 | S/A | `ParachuteBomb.as`：`advance`；`mutiny.swf.xml` DefineSprite 939 | 未开伞、未结束时每 tick 创建 `cannonSmokeTrail`。Sprite 939 共 30 帧：`closed` 第 1 帧，`opening` 第 11 帧，`open` 第 26 帧，第 30 帧 `gotoAndPlay("open")`。 |
| WPN-08-S-07 | S | `ParachuteBomb.as`：`advanceMotion` | Solid 运动后，若 `y<-300`，设回 -300；若 `vy<0`，设为 0。 |

无冲突。原版运行录屏、音频并发与镜头证据尚未取得。

## 状态、输入和转换

| 规则 ID | 前置状态 | 输入/事件及允许条件 | 状态变化/触发 tick | 视觉、声音、镜头结果 | 来源 |
| --- | --- | --- | --- | --- | --- |
| WPN-08-EFF-01 | 未发射、持有者有库存 | 普通武器 twang 拖拽释放 | `Solid.twang` 以 30 为上限；该武器不可 draggable，因此不进入 `Weapon.release` 的 20 上限分支 | 发射后消耗武器和两项行动资格 | S-01、`Weapon.as` |
| WPN-08-EFF-02 | 已发射、未完成 | 每个 25 Hz tick | 预测线和实际运动均先减速下落/横向 .95，再加重力并碰撞；第一次处理后 `vy>-10` 即开伞 | 从第 11 帧开始播放开伞，最终循环 open 段 | S-02、S-06 |
| WPN-08-INT-01 | 已发射、持有者为玩家队 | 左键持续按住且鼠标在炸弹左/右 | 每 tick 向与鼠标相反的水平方向加 .2；第 12、24… tick 发 `fan` | 光标风扇播放/停止；音效为 `fan` | S-03 |
| WPN-08-EFF-03 | 已发射、未完成 | terrain 或箱体任一 contact | 同 tick 结束并生成 160/50 explosion | 本体隐藏，立即一声 `pop` | S-04 |
| WPN-08-EFF-04 | 已发射、未完成 | 穿越水面、地图底部或静止 | 水面仅 splash；底部/精确静止结束，均不生成爆炸 | 水花和 `splash`；结束时本体移除 | S-05 |
| WPN-08-EFF-05 | 已发射、未完成 | Solid 运动完成 | `y<-300` 回设 -300，向上速度归零 | 防止飞离屏幕上方 | S-07 |

拒绝与边界：`vy=-10` 不开伞，`vy>-10` 开伞；扇风左右判定以严格 `<0` 为左侧，鼠标恰在 x 位置走向左的 `vx-=.2` 分支；水面 crossing 不调用爆炸；AI 不执行玩家风扇分支。

## 表现资源与实现映射

| 规则 ID | 原版 symbol/标签/帧段 | Unity 资源和生产入口 | 当前差异 |
| --- | --- | --- | --- |
| WPN-08-ANI-01 | DefineSprite 939，`closed` 1，`opening` 11，`open` 26–30→26 | `Resources/Art/Weapons/ParachuteBomb/1..30.png`，`MutinyParachuteBomb.AdvanceChuteAnimation` | 注册点、嵌套层/光标风扇的像素级对照未执行。 |
| WPN-08-ANI-02 | `advance` 创建 `cannonSmokeTrail` | `MutinyRumBottleSmokeTrail.Spawn` | 水平排序与原版视觉对照未执行。 |
| WPN-08-AUD-01 | `fan`、`pop`，并继承 `splash` | `MutinyAudioManager` 对应生产调用 | Unity Play Mode 未确认音频资源与并发。 |

## 用例与实际结果

| 用例 ID | 规则 ID | 可复现前置条件及操作 | 独立期望及来源 | 验收层 | 实际结果 |
| --- | --- | --- | --- | --- | --- |
| WPN-08-VER-01 | EFF-01 | 生产输入选择 `parachuteBomb`，拖拽到超过 30-force 距离 | twang 初速度上限为 30；S-01/`Solid.twang` | 逻辑/输入 | 未执行 |
| WPN-08-VER-02 | EFF-02/INT-01 | 固定初速，逐 tick 执行无风、鼠标左、鼠标右 | .95、`vy>-10`、`.2` 和第 12 tick fan；S-02/S-03 | 逻辑 | 未执行 |
| WPN-08-VER-03 | EFF-03/04/05 | terrain、木箱、水面、顶端、底部和静止场景 | contact 爆炸；水只 splash；边界按 S-04–07 | 逻辑/场景 | 未执行 |
| WPN-08-VER-04 | ANI-01/AUD-01 | Unity Play Mode 重放完整发射、开伞、扇风、撞击 | 关键帧、音频 tick 与原版录屏逐项对照 | 原版对照 | 未执行 |

- 比较字段：tick、`x/y/vx/vy`、`chuteOpen`、动画帧、风扇输入、SFX 事件、爆炸参数与结束原因。
- 未决事项：AI 的瞄准评分、光标风扇图层和完整镜头路径需由原版运行记录闭环。

## 完成记录

- 已静态确认：WPN-08-S-01..07。
- 已实现：按原版顺序的飞行/开伞/扇风、30 帧循环、烟雾、水面 crossing、碰撞爆炸及日志。
- 已执行并通过：无。
- 待验证：生产输入回归、Unity Play Mode、原版逐 tick 对照。
