# 09.04 · 镜头

[返回上级模块](../README.md)

## 职责

管理跟随、边缘滚屏、空投运镜、震动和目标优先级。

## 边界

镜头不影响逻辑坐标。

## 行为规格

| ID | 可观察行为 | 原版来源 | Unity 入口 | 当前结果 |
| --- | --- | --- | --- | --- |
| CAM-POE-01 | Pieces of Eight 每枚发射显式接管钱币跟随；前 7 枚结算立即释放跟随并自动回使用者；抵达后连续武器仍处于执行阶段也允许鼠标边缘、方向键和 WASD 滚屏；下一枚发射再次接管 | `Weapon.as::fire/twang`；`PiecesOfEight.as::next`；`TileSystem.as::advanceScrolling:455-493` | `RequestTrackWeapon`、`ReleaseWeaponTracking`、`RequestPanToCharacter`、`AdvanceEdgeScrolling` | 已实现；C# 编译通过，待 Unity 运行验证 |
| BOX-CAM-01 | 木箱/火药桶每次放置在同一调用中把通用 `track=true` 覆盖为 `false`，且不设置 `panToCharacter`；连续摆放间隙直接进入普通手动滚屏 | `Weapon.as::place:140-148`；`BoxWeapon.as::place:93-100`；`TileSystem.as::advanceScrolling:455-493` | `MutinyPlayerInput.IsAwaitingBoxPlacement`、`FindActionTarget`、`CanUseManualScrolling` | 第一箱/桶后保持执行阶段，断言镜头不跟随已放物且边缘/方向键/WASD 门开放 | 已实现；待 Unity 运行验证 |

## Android 授权适配

| ID | 可观察行为 | 来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| AND-CAM-01 | Android 等移动平台没有桌面悬停光标；无触摸时的空闲/默认指针位置不得触发屏幕边缘滚屏。桌面端鼠标边缘滚屏保持原版行为 | 用户报告：Android 开场镜头持续左移；现有 `Munity.apk` 的 IL2CPP 产物 `Assembly-CSharp__3.cpp:20371-20443` 确认直接读取 `Mouse.current.position`，且无移动平台门禁 | `MutinyCameraController.AdvanceEdgeScrolling`、`ShouldUseMouseEdgeScrolling` | 生产判定在 `isMobilePlatform=true` 且存在 pointer/mouse 状态时仍返回 false；桌面真实鼠标返回 true | 已实现；`Assembly-CSharp` 与 `Assembly-CSharp-Editor` 编译通过；Android 真机运行待验证 |

该条是移动端适配，不属于 Flash 原版规则变更。移动端镜头手势将另行定义，不能复用依赖“悬停”的桌面边缘滚屏。
