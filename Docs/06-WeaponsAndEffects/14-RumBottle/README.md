# 06.14 · Rum Bottle

[返回上级模块](../README.md)

## 职责

记录朗姆酒瓶的爆炸、点火和火焰传播。

## 边界

参数和时序以原版证据为准。

## 逻辑摘要

- extent 14、twangMaxForce 30、飞行旋转 `vx*2`；当前 Unity 错误地把提交速度再次截到 20。
- 任一 Solid 接触爆炸 `80/25`；只有 Floor 接触才产生火焰。
- 火焰从地表同一点向左右递归，每段相隔 8 px；下一格需有实体且上方为空。
- 每段生成时对脚点距离平方 `<64` 的角色造成 30 伤害并改写速度。

来源：`RumBottle.as`、`SweepingFlame.as`。规则：`RUM-*`，见 [完整审计](../IMPLEMENTATION_DETAILS.md#610-rum-bottle--sweeping-flame)。
