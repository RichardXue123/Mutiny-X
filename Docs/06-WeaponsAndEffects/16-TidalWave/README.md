# 06.16 · Tidal Wave

[返回上级模块](../README.md)

## 职责

记录海啸生成、固定方向移动、逐 tick 伤害窗和离场行为。

## 边界

参数和时序以原版证据为准。

## 逻辑摘要

- 点击后从 `(-550,waterY)` 以 vx=20 向右移动，动画按 skyColour 选择。
- 角色位于水线上方 300 px 范围且横向距浪中心不超过 150 px 时，每 tick 扣 5。
- 原版逻辑不是一次性水平推水冲量；越过地图右侧 550 px 后结束。

来源：`TidalWave.as`。规则：`TID-*`，见 [完整审计](../IMPLEMENTATION_DETAILS.md#612-tidal-wave)。
