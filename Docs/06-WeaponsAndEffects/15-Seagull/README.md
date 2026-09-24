# 06.15 · Seagull

[返回上级模块](../README.md)

## 职责

记录海鸥路径、飞行、投弹和重复发射。

## 边界

参数和时序以原版证据为准。

## 逻辑摘要

- 首次点击选择飞行高度，从 x=-300 以 vx=10 向右飞。
- 飞行中每次点击都可投一枚弹，原版没有固定弹数；弹从 `(bird.x-10,bird.y)` 生成。
- 每个原版 tick 先移动海鸥，再消费投弹输入；新弹会在同一 tick 内立即推进一次，后续子弹也由海鸥逐 tick 统一推进。
- 弹碰 Solid 爆炸 `50/50`，落水只销毁；鸟越界且活动弹清空后才结束。
- 海鸥没有飞行时间上限，不参与 Unity 的 150 tick 卡死武器强制回收。

来源：`Seagull.as`。规则：`SEA-*`，见 [完整审计](../IMPLEMENTATION_DETAILS.md#611-seagull)。

## AI 候选

| ID | 可观察行为 | 原版来源 | Unity 入口 | 当前结果 |
| --- | --- | --- | --- | --- |
| AI-WPN-06 | AI 在最高敌人上方选择飞行高度，随机并排序 10 个投弹 X；敌方单发为 `1-distance/40`，己方单发为 `-(1.5-distance/40)`，仅保留正分投弹点，至少两点才形成候选 | `Seagull.as::aiSimulation` | `MutinyAIController.EvaluateSeagull`、`ScoreSeagullShot` | 已实现；炸弹预测读取共享箱体；回归已写，待 Unity 运行 |
