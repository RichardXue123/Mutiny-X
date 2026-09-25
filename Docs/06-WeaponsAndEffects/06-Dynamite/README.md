# 06.06 · Dynamite

[返回上级模块](../README.md)

## 职责

记录 Dynamite 的滚动、停稳、入水画面和爆炸行为。原版没有独立倒计时。

## 边界

参数和时序以原版证据为准。

## 逻辑摘要

- extent 11、friction 1.7、旋转 `vx*2`；静止阈值 `vx==0 && abs(vy)<.2` 时爆炸 `250/70`。
- 选中后立即播放 `lit`：可见 frame 1..4 循环；frame 5 只执行 `gotoAndPlay("lit")`，不是额外停留帧。该动画不以 `fired` 为前提。
- 入水执行 `gotoAndStop("unlit")` 并停在 frame 6。
- 入水静态代码只切到 `unlit` 帧，没有“熄灭后禁止爆炸”的判断。
- Unity 已移除先前“入水 dud 自动结束”的非原版分支；原版静态代码证实入水只改变画面，停稳仍触发爆炸。隔离 Unity Play Mode 逻辑专项已通过，主工程画面仍待验收。

来源：`Dynamite.as`。规则：`DYN-*`，见 [完整审计](../IMPLEMENTATION_DETAILS.md#62-dynamite)。

## 表现规格

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| DYN-ANI-01 | ready 与飞行阶段均循环显示 lit frame 1..4；frame 5 动作立即回 frame 1 | `Dynamite.as::Dynamite`；symbol 881 label `lit`；frame 5 action | `MutinyDynamite.AdvanceOriginalPresentationTick` | 保持 `IsFired=false` 推进四个生产物理 tick，核对 1→2→3→4→1 | 已实现；Unity 定向回归通过 |
| DYN-ANI-02 | 入水立即停到 unlit frame 6，后续 tick 不再推进 lit 循环 | `Dynamite.as::advanceMotion`；symbol 881 label `unlit`；frame 6 action | `MutinyDynamite.OnWaterSubmerged` | 触发真实水线判定后推进 tick，断言仍为 frame 6 | 已实现；Unity 定向回归通过 |
| DYN-WATER-02 | 入水不变成 dud，停稳仍爆炸 `250/70`；没有 0.35 秒或深度截止 | `Dynamite.as::advanceMotion` | `AdvanceOriginalDetonationTick` | 正式发射穿过水线，确认先显示 unlit、未提前结束，再由地形碰撞停稳引爆 | 静态确认、已实现；隔离 Unity 6000.6 Play Mode 专项通过，主工程画面待验收 |
