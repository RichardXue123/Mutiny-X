# 06.14 · Rum Bottle

[返回上级模块](../README.md)

## 职责

记录朗姆酒瓶的爆炸、点火和火焰传播。

## 边界

参数和时序以原版证据为准。

## 逻辑摘要

- extent 14、twangMaxForce 30、飞行旋转 `vx*2`；正常松手通过 `Solid.twang`，预览与实际初速都以 30 为上限。
- 任一 Solid 接触爆炸 `80/25`；只有 Floor 接触才产生火焰。
- 纵向碰撞在原版横向位移之前触发 `contact`；地面/天花板的爆炸与火焰使用该 tick 的横移前 X，墙面碰撞使用横移后的 X。
- 火焰从地表同一点向左右递归，每段相隔 8 px；下一格需有实体且上方为空。
- 每段生成时对脚点距离平方 `<64` 的角色造成 30 伤害并改写速度。

来源：`RumBottle.as`、`SweepingFlame.as`。规则：`RUM-*`，见 [完整审计](../IMPLEMENTATION_DETAILS.md#610-rum-bottle--sweeping-flame)。

## 验收

| ID | 原版来源 | Unity 入口 | 验收用例 | 状态 |
| --- | --- | --- | --- | --- |
| RUM-PHY-01 / RUM-TRAJ-01 | `TileSystem.as::mouseUp` → `Weapon.twang` → `Solid.twang`；`Solid.drawTwangLine` | `MutinyRumBottle.Twang`、`MutinyTrajectoryRenderer.PredictVelocityTick` | 400 px 满拉力得到 30 px/tick 初速；无碰撞的前 5 tick 与预览点一致 | 已实现；待 Unity 运行验证 |
| RUM-HIT-02 | `Solid.as::advanceMotion` 先纵向 `contact`、后横向移动；`RumBottle.as::contact` 当场创建效果 | `MutinyRumBottle` 的纵向接触坐标 | 斜向落地且本 tick 仍横移时，爆炸与火焰使用横移前 X，而瓶体可继续横移 | 已实现；待 Unity 运行验证 |
