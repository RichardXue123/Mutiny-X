# 06.02 · 待抛射、瞄准与取消

[返回上级模块](../README.md)

## 职责

管理装备预览、拉力、轨迹、提交和取消阶段。

## 边界

不同武器的专用预览规则由对应子模块声明。

## 已确认规则

- `WPN-CAN-PRESS-01`：取消叉只响应按钮命中区域内的新按下沿；蓄力手势在按钮外开始、持续按住后拖到叉上，无论停留还是松开都不触发取消，松开仍按正常武器/跳跃提交规则处理。Android 第二指在叉上新按下仍可取消。原版来源：`com/nitrome/game/CancelWeaponButton.as::onPress`、`TileSystem.as::mouseUp`；Unity 入口：`TryHandleCancelOverlayPrimaryPointer`、`UpdateSecondaryMobileCameraTouch`、`ResolveAimRelease`；验收：分别对武器和跳跃测试待命按叉取消、蓄力拖到叉上持续按住不取消、叉上松开正常提交、第二指新按叉取消及随后主指松开不补发射。状态：原版静态确认；已实现；2026-09-26 Unity 6000.6.0f1 隔离 Play Mode 组合专项 31/31 通过（本规则 6 条）；真实鼠标与 Android 操作待验收。按用户要求撤销此前误加的“拖到叉上松开取消”，此前包含该错误规则的 5/5、25/25 记录仅作历史结果，不证明当前按下语义。
- twang 初速为 `(mouse-weapon)*-0.25`，普通上限 20；Banana、ParachuteBomb、RumBottle 上限 30。
- 预测线推进 15 个固定 tick；按下点必须在武器中心 30 px 内。
- 正常松手调用 `twang()`，不经过 `Weapon.release()`；不能把三种 30 力武器再次截成 20。
- `WPN-VEL-01`：`Solid.twang()` 才按该对象的 `twangMaxForce` 限制初速；普通拉拽武器为 20，Banana、ParachuteBomb、RumBottle 为 30。预览与正常松手共用此上限。验收：分别满拉力提交并核对初速。原版来源：`Solid.as::twang/drawTwangLine`、三种武器构造器；Unity 入口：`MutinyWeapon.Twang`、`MutinyWeaponFactory.GetTwangMaxForce`。状态：静态确认、已实现；待 Unity 运行验证。
- `WPN-VEL-02`：直接调用 `Weapon.fire(vx,vy)` 只赋予输入速度，不套用 `twangMaxForce` 或 `Weapon.release()` 的 20 限速；后者仅属于拖拽 `release` 路径。验收：直接给普通武器高于 20 的速度，断言发射时原样保留。原版来源：`Weapon.as::fire/release`、`TileSystem.as::mouseUp`；Unity 入口：`MutinyWeapon.Fire`。状态：静态确认、已修复；待 Unity 运行验证。

## 本轮逐类限速审计

| 武器路径 | 原版拉拽上限 | 当前 Unity 路径 | 结论 |
| --- | --- | --- | --- |
| Cherry Bomb、Dynamite、Boulder、Mine、Pieces of Eight、Voodoo Doll | 继承 `Solid.twangMaxForce=20` | `MutinyWeapon.Twang`；巫毒娃娃通过绑定目标后调用基类 | 20 是原版规则，不应取消 |
| Banana、Parachute Bomb、Rum Bottle | 各构造器设为 30 | 基类 `Twang`；工厂预览上限也为 30 | 没有额外 20 截断；保留现有 30 |
| Cannon → Cannonball | 固定炮力；`Cannonball.fire` 接收原速度 | `MutinyCannonball.Fire` | 不经通用拉拽上限 |
| Wooden Crate、Gunpowder Barrel、Anchor、Seagull、Tidal Wave | 放置、下落或专用移动，不走拉拽 | 各自专用入口 | 20 拉拽上限不适用 |
| AI / 直接调用 `Weapon.fire` | `Weapon.fire` 本身不限速；`Weapon.release` 的 20 不适用于此 | `MutinyWeapon.Fire` | 原先共享层多截一次，现已移除；AI 自身候选速度范围不变 |
- 已提交或 `weaponLocked` 的序列不能取消。
- `JUMP-CAN-01`：选择 Throw Self/跳跃后与未发射武器相同，角色下方显示取消叉；点击叉清除行动选择并回到行动菜单，且不消耗 `canThrow`。原版依据为 `Character.as::updateOverlay` 的 `weaponSelected` 条件与 `CancelWeaponButton.as::onPress`，该条件不要求存在 `equippedWeapon`。
- `EXT-JUMP-CAN-01`：用户授权扩展——Throw Self 拉线蓄力时按右键，只取消本次蓄力并回到跳跃待命状态；不提交投掷、不消耗 `canThrow`。原版 AS2 未定义浏览器右键输入，因此与原版一致性规则分开登记。
- `CHAR-OVR-AIM-01`（原版规则）：Throw Self 拉线蓄力属于 `Controller.twanging`，不属于 `Controller.dragging`；原版角色 `draggable=false、twangable=true`，蓄力期间 P1/P2 标记、选择框、血条和取消叉保持可见，松开起跳进入 `thrown` 后才隐藏。来源：`Character.as::Character/updateOverlay/advance/twang`、`TileSystem.as::mouseDown/mouseUp`、用户原版截图。已实现；专项运行状态见 [角色覆盖层](../../09-PresentationAndFeedback/02-CharacterOverlay/README.md)。
- `EXT-AIM-CAN-02`（触屏输入扩展）：武器与 Throw Self 蓄力时，Android 第二指在可见的 20×20 px 取消叉上新按下优先取消；按住拖到叉上停留或松开不能取消（`WPN-CAN-PRESS-01`）。取消走正式 `CancelWeaponSelection`，清除轨迹与未发射装备、返回行动选择，不提交跳跃/武器、不扣库存或行动；第一指随后松开不得迟发射。桌面右键仍只撤销拉线、保留待命武器。Unity 入口：`MutinyPlayerInput.TryCancelWeaponFromOverlay/UpdateSecondaryMobileCameraTouch`；验收：第二指新按叉后回到行动菜单、轨迹消失且无发射，主指在叉上松开仍正常提交。状态：已实现；隔离 Unity Play Mode 组合专项 31/31 通过；安卓真机触控与画面待验收。更正此前来源解读：蓄力时叉可见本身是原版规则，不是扩展；不能把 `twanging` 当成用于隐藏叉的 `dragging`。

原版来源：`Solid.as::twang/drawTwangLine`、`TileSystem.as::mouseDown/mouseUp`。Unity 入口：`MutinyPlayerInput.cs`、`MutinyTrajectoryRenderer.cs`。完整审计见 [共享瞄准规则](../IMPLEMENTATION_DETAILS.md#41-拉拽投掷)。
