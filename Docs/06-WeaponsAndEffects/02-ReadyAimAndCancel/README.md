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
- 已提交或 `weaponLocked` 的序列不能取消。
- `JUMP-CAN-01`：选择 Throw Self/跳跃后与未发射武器相同，角色下方显示取消叉；点击叉清除行动选择并回到行动菜单，且不消耗 `canThrow`。原版依据为 `Character.as::updateOverlay` 的 `weaponSelected` 条件与 `CancelWeaponButton.as::onPress`，该条件不要求存在 `equippedWeapon`。
- `EXT-JUMP-CAN-01`：用户授权扩展——Throw Self 拉线蓄力时按右键，只取消本次蓄力并回到跳跃待命状态；不提交投掷、不消耗 `canThrow`。原版 AS2 未定义浏览器右键输入，因此与原版一致性规则分开登记。

原版来源：`Solid.as::twang/drawTwangLine`、`TileSystem.as::mouseDown/mouseUp`。Unity 入口：`MutinyPlayerInput.cs`、`MutinyTrajectoryRenderer.cs`。完整审计见 [共享瞄准规则](../IMPLEMENTATION_DETAILS.md#41-拉拽投掷)。
