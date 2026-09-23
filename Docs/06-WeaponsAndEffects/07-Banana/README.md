# 06.07 · Banana

[返回上级模块](../README.md)

## 职责

记录 Banana 的反弹、二次点击和引爆行为。

## 边界

参数和时序以原版证据为准。

## 逻辑摘要

- extent 7、bounce .8、friction .5、twangMaxForce 30。
- 碰撞只播放反弹音；没有“反弹若干次后爆炸”。
- 静止自动爆；人类玩家飞行中再次点击爆；AI 在近角色条件下爆，规格为 `160/80`。
- 正常松手走 `TileSystem.mouseUp → twanging.twang() → Solid.twang()`，不会经过 `Weapon.release()`；预览与实际提交都使用 Banana 自身的 30 力上限。

来源：`Banana.as`。规则：`BAN-*`，见 [完整审计](../IMPLEMENTATION_DETAILS.md#63-banana)。

## 行为规格与实现映射

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| BAN-PHY-01 | extent 7、bounce .8、friction .5，拉拽预览和正常松手的实际初速上限均为 30 | `Banana.as::constructor`；`TileSystem.as::mouseUp`；`Solid.as::twang` | `MutinyTrajectoryRenderer`、`MutinyBanana` 继承共享 `Twang` | 400 px 满拉力时断言提交 `vx=30`，而不是 20 | 已实现；自动回归已写，待 Unity 运行验证 |
| BAN-TRAJ-01 | 无碰撞时，预览的每个离散点与香蕉从相同起点发射后的实际 25 Hz 物理位置一致；每 tick 都先加 weight=1 再移动 | `Solid.as::drawTwangLine:94-115`、`twangPrediction:126-129`、`advanceMotion:162` | `PredictVelocityTick()`、`MutinyPhysicsBody.AdvanceSimulationTick()` | 用相同起点和满拉力连续比较前 5 个预览 tick 与实际位置 | 已实现；自动回归已写，待 Unity 运行验证 |
