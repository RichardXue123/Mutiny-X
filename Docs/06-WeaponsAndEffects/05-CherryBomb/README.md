# 06.05 · Cherry Bomb

[返回上级模块](../README.md)

## 职责

记录 Cherry Bomb 的发射、碰撞和爆炸行为。

## 边界

参数和时序以原版证据为准。

## 逻辑摘要

- extent 9；接触任一 Solid 后爆炸 `80/40`。
- 原版未结束期间每 tick 生成 `cannonSmokeTrail`。
- 当前 Unity 已实现接触爆炸，但缺烟迹，并把 Water 当作 contact；后者需要原版运行确认。

来源：`CherryBomb.as`。规则：`CHB-*`，见 [完整审计](../IMPLEMENTATION_DETAILS.md#61-cherry-bomb)。本轮未运行 Unity。
