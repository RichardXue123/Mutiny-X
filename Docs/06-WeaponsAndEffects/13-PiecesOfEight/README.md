# 06.13 · Pieces of Eight

[返回上级模块](../README.md)

## 职责

记录连续 8 枚投射物的循环和结束条件。

## 边界

参数和时序以原版证据为准。

## 逻辑摘要

- 一次库存连续投 8 枚；每次 Solid 接触爆炸 `50/25`，入水不爆。
- 前 7 枚完成后同一对象回到 owner `(x,y+5)`，锁定更换武器并重新瞄准；第 8 枚才结束。
- 每枚飞行时镜头跟随钱币；前 7 枚结算后关闭钱币跟随并自动平移回使用者，到达后即使回合仍处于连续武器执行状态，也允许边缘/按键手动滚屏。
- AI 每枚间隔 20 tick，再在 10 个随机投掷中评分。

来源：`PiecesOfEight.as`。规则：`POE-*`，见 [完整审计](../IMPLEMENTATION_DETAILS.md#69-pieces-of-eight)。

## 行为规格与实现映射

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| POE-CAM-01 | 每枚钱币发射时 `track=true`，镜头只跟随当前复用的钱币实例 | `Weapon.as::fire/twang`；`TileSystem.as::advanceScrolling:455-459` | `Fire()` → `RequestTrackWeapon(this)` | 第 1、2 枚分别发射后断言显式跟随目标为当前钱币且回角色目标已清空 | 已实现；C# 编译通过，待 Unity 运行验证 |
| POE-CAM-02 | 前 7 枚结算时先 `track=false`，再设 `panToCharacter=owner`；回移未到达前不接受手动滚屏 | `PiecesOfEight.as::next:127-146`；`TileSystem.as::advanceScrolling:461-468` | `ResolveCoin()` → `ReleaseWeaponTracking()` → `RequestPanToCharacter()` | 第一枚结算后断言钱币目标已释放、角色回移仍在进行、手动滚屏分支尚未激活 | 已实现；C# 编译通过，待 Unity 运行验证 |
| POE-CAM-03 | 镜头到达 owner 并清空 `panToCharacter` 后，即使仍为 `ActionExecuting`，也允许鼠标边缘、方向键和 WASD 滚屏；下一枚发射后重新进入钱币跟随 | `TileSystem.as::advanceScrolling:463-493` | `CanAcceptManualScrollingForVerification()`、`AdvanceEdgeScrolling()` | 驱动生产回移到达分支，断言手动滚屏激活；再发第 2 枚，断言重新跟随钱币 | 已实现；C# 编译通过，待 Unity 运行验证 |
