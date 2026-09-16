# WPN-10 — Rum Bottle（烧火瓶）

基线：原版 SWF `mutiny.swf`，SHA256 `c034d2cfa50272a1aac93ad75114b542ca5993dd7b70fa8901aa195de7820586`；Unity 工作区基线为本次 WPN-10 修改前的当前工作区。坐标均为 Flash 像素，逻辑 tick 为 25 Hz。

## 入口与证据

| 原版入口 | 证据 | Unity 入口 |
| --- | --- | --- |
| `RumBottle.as` 构造、`advance()`、`contact(side)` | S | `MutinyRumBottle.Awake/AdvanceOriginalPresentationTick/OnContact` |
| `SweepingFlame.as` 构造、`createNext()` | S | `MutinySweepingFlame.Initialize/CreateNext` |
| `DefineSprite_905_sweepingFlame` 帧 4 / 帧 11 `DoAction.as` | S+A | `MutinySweepingFlame.AdvanceOriginalTick` |
| `Weapon.as release()` | S | `MutinyRumBottle.Twang` |
| `Explosion.as hit()` | S | `MutinyExplosion` |

## 行为规则

### WPN-10-EFF-01 — 创建、瞄准与飞行

- **前置状态**：角色已选择 `rumBottle`，且该武器尚未发射。
- **原版规则（S，`RumBottle.as` 构造；`Weapon.as release()`）**：linkage 为 `rumBottle`；左右上下 extent 均为 14；可拉力表最大值是 30；每 tick 飞行时执行 `rotation += velocityX * 2`；`Weapon.release()` 实际把释放速度限制为 20 px/tick。
- **Unity 入口**：`MutinyRumBottle.Awake`、`Twang`；公共 25 Hz 旋转入口为 `MutinyWeapon.AdvanceOriginalRotationTick`。
- **验收**：生产 `Twang` 用 400 px 拖拽，速度为 `(20,0)`；飞行时连续检查旋转增量为 `velocityX * 2`。
- **实际结果**：自动回归 WPN-10-EFF-01 已加入，尚未在 Unity Editor 执行。

### WPN-10-EFF-02 — 接触、爆炸与声音时序

- **输入/条件**：发射后的任意 `contact(side)`；模拟预测模式仅结束模拟，不产生效果。
- **原版规则（S，`RumBottle.as contact`）**：首次接触立刻完成酒瓶、隐藏它、在 `(x,y)` 创建 `Explosion(x,y,80,25,owner)`，并立即调用 `playSound("pop")`。这不是 `Explosion.hit()` 的第 3 帧声音。
- **可观察结果**：爆炸尺寸 80，最大伤害 25，爆炸半径由 `Explosion` 的 `size / 2 + 20` 计算；一次且仅一次破瓶 `pop`。
- **Unity 入口**：`MutinyRumBottle.Explode`；`MutinyExplosion.Spawn(..., playPopOnHit:false)`。
- **验收**：从 `MutinyPhysicsBody.AdvanceSimulationTick` 驱动地面接触，断言酒瓶完成、生成 80/25 爆炸和一次立即声音；用音频事件日志对照原版 tick。
- **实际结果**：逻辑回归已加入，Unity Editor 与原版录制对照未执行。

### WPN-10-EFF-03 — 仅地面接触点火

- **原版规则（S，`RumBottle.as contact`）**：只有 `side == Solid.FLOOR` 才点火。列为 `x >> 5`，行从 `(y >> 5) + 1` 开始，向上越过相连的实心 tile 至最上方；在该 tile 的 `(col << 5,row << 5)` 同时创建左右两个 `SweepingFlame`。墙、顶和水接触不创建火焰。
- **Unity 入口**：`MutinyRumBottle.FindOriginalFlameOrigin`、`MutinySweepingFlame.Spawn`。
- **验收**：分别覆盖平台中心、边缘、墙、水面；地面中心得到两个初始火焰，其余三类为零。
- **实际结果**：中心点逻辑回归已加入；边缘、墙、水面的 Unity Play Mode 与原版对照未执行。

### WPN-10-EFF-04 — 火焰伤害与地形传播

- **原版规则（S，`SweepingFlame.as`）**：创建时遍历活着角色；若 `(character.x-x)^2 + (character.y+bottomExtent-y)^2 < 64`，将其速度设为 `((random()-.5)*8, -(random()*2+6))` 并扣 30 HP。第 4 帧向指定方向前进 8 px；只有新位置 tile 实心且其正上方 tile 为空时才创建下一段。每次创建下一段重置 `Controller.inactivity=0`。
- **Unity 入口**：`MutinySweepingFlame.ApplyOriginalSpawnHit`、`CreateNext`、`MutinyTurnManager.NotifyAmbientActivity`。
- **验收**：原点角色被左右两段各命中一次，生命从 100 到 40；距火焰正好 8 px 的角色不命中（严格小于）；第 4 帧在无遮挡地面扩展 8 px，顶面被占用时停止。
- **实际结果**：原点伤害和第 4 帧双向传播回归已加入，尚未在 Unity Editor 执行。

### WPN-10-ANI-01 — 酒瓶与烟雾

- **原版规则（S+A，`RumBottle.as advance`；symbol 918）**：酒瓶时间轴 12 帧；飞行的每个 tick 创建一个 `cannonSmokeTrail` 残片。
- **Unity 入口**：`MutinyRumBottle.AdvanceOriginalPresentationTick`、`MutinyRumBottleSmokeTrail`。
- **验收**：12 帧资源存在；发射后逐 tick 检查 12 帧循环和每 tick 一条烟雾。
- **实际结果**：酒瓶 12 帧资源回归已加入，尚未执行。烟雾帧导入待人工完成，见“资源状态”。

### WPN-10-ANI-02 — 火焰时间轴

- **原版规则（S+A，`DefineSprite_905_sweepingFlame/frame_4/DoAction.as` 与 `frame_11/DoAction.as`）**：第 4 帧 `createNext()`，第 11 帧 `destroy()`；共 11 帧。
- **Unity 入口**：`MutinySweepingFlame.AdvanceOriginalTick`。
- **验收**：手动推进三个 tick 后产生后继段；第 11 帧销毁当前段。
- **实际结果**：原版 11 帧 PNG 已导入 `Resources`，生产物理入口回归会断言全部帧可加载、初始为第 1 帧、传播时为第 4 帧；Unity Editor 与画面对照未执行。

## 资源状态

已存在：`Assets/Mutiny/Resources/Art/Weapons/RumBottle/1.png` 至 `12.png`，以及火焰的原版逐帧资源。

| 状态 | 原始证据目录 | Unity Resources 目标目录 | 帧数 |
| --- | --- | --- | --- |
| 已导入 | `Docs/ReverseEngineering/Art/raster/sprites/DefineSprite_905_sweepingFlame/` | `Assets/Mutiny/Resources/Art/Effects/SweepingFlame/` | 11 |
| 待导入 | `Docs/ReverseEngineering/Art/raster/sprites/DefineSprite_866_cannonSmokeTrail/` | `Assets/Mutiny/Resources/Art/Effects/CannonSmokeTrail/` | 19 |

烟雾资源仍缺失；`MutinyRumBottleSmokeTrail` 会保留逻辑残片，但在资源导入前不会显示逐帧烟雾视觉。

## 已知差异与后续验收

- 未取得原版运行录像/音频日志，故声音并发、镜头跟随、inactivity 的完整实机时序仍为待验证项。
- 需在真实行动菜单中覆盖：库存扣除、取消、地/墙/水命中、火焰存续期间的回合结算和实际帧画面。
- 自动验证通过只能说明生产物理入口与固定场景；不得替代 Unity Play Mode 与原版逐 tick 对照。
