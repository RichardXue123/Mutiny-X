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
