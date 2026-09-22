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
- Unity 二次把提交速度截到 20，是射程差异。

来源：`Banana.as`。规则：`BAN-*`，见 [完整审计](../IMPLEMENTATION_DETAILS.md#63-banana)。
