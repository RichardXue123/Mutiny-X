# 03.03 · 行动资格

[返回上级模块](../README.md)

## 职责

判断移动、跳跃、武器和取消操作是否可用。

## 边界

按钮显示只读取这里的结果。

## 取消规则

| ID | 可观察行为 | 原版来源 | Unity 入口 | 当前结果 |
| --- | --- | --- | --- | --- |
| JUMP-CAN-01 | 选择跳跃但尚未拉线/提交时显示角色下方取消叉；点击后回到行动选择，跳跃资格保持 | `Character.as::updateOverlay`、`CancelWeaponButton.as::onPress` | `ShouldShowCancelWeapon`、`TryCancelWeaponFromOverlay` | 已实现；待 Unity 运行验证 |
| EXT-JUMP-CAN-01 | 跳跃蓄力时右键撤销拉线并回到跳跃待命，不消耗行动 | 用户授权扩展；原版无右键规则 | `TryCancelAimFromSecondaryPointer` | 已实现；待 Unity 运行验证 |
| CHAR-OVR-AIM-01 | 跳跃拉线属于 `twanging`，当前角色标记、选择框、血条和取消叉保持可见；正式起跳才按 `thrown` 隐藏 | `Character.as::Character/updateOverlay/twang`、`TileSystem.as::mouseDown/mouseUp`、用户原版截图；更正此前误把蓄力当成 dragging 的结论 | `MutinyCharacterOverlay.LateUpdate`、`ShouldShowCancelWeapon` | 原版静态确认；已实现；隔离 Unity Play Mode 共 25/25 通过；主工程画面待验收 |
| WPN-CAN-PRESS-01 | 叉只响应命中区域内的新按下沿；按住移入或在叉上松开不能取消，后者仍按正常武器/跳跃规则提交 | `CancelWeaponButton.as::onPress`、`TileSystem.as::mouseUp`；用户本次明确要求 | `TryHandleCancelOverlayPrimaryPointer`、`ResolveAimRelease` | 原版静态确认；已实现；隔离 Play Mode 31/31 组合专项通过 |
| EXT-AIM-CAN-02 | 蓄力时第二指在叉上新按下取消并返回行动选择；第一指随后松开不发射。右键仍只退回待命 | 用户授权的安卓输入适配；叉可见遵循 `CHAR-OVR-AIM-01`，触发时机遵循 `WPN-CAN-PRESS-01` | `TryCancelWeaponFromOverlay`、`UpdateSecondaryMobileCameraTouch` | 已实现；隔离 Play Mode 31/31 组合专项通过；安卓真机待验证 |
