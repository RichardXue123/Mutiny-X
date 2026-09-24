# 06.02 · 待抛射、瞄准与取消

[返回上级模块](../README.md)

## 职责

管理装备预览、拉力、轨迹、提交和取消阶段。

## 边界

不同武器的专用预览规则由对应子模块声明。

## 已确认规则

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

原版来源：`Solid.as::twang/drawTwangLine`、`TileSystem.as::mouseDown/mouseUp`。Unity 入口：`MutinyPlayerInput.cs`、`MutinyTrajectoryRenderer.cs`。完整审计见 [共享瞄准规则](../IMPLEMENTATION_DETAILS.md#41-拉拽投掷)。
