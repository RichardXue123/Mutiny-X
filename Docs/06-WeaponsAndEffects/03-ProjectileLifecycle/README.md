# 06.03 · 投射物生命周期

[返回上级模块](../README.md)

## 职责

定义生成、飞行、碰撞、入水、结束和结算回报。

## 边界

只抽取原版真正共享的流程。

## 已确认规则

- `fire/place` 只接受首次提交，并打开镜头跟踪。
- 继承 `Weapon.advance()` 的对象先做固定步物理，再更新显示；地图底部越界或静止阈值成立时结束。
- 水面公共逻辑只检测穿越、生成 splash；原版没有共享“入水 0.4 秒销毁”或“8 秒超时”。
- 各武器覆写 `advance()` 后，不应无条件套用公共水中/结束规则。

原版来源：`Weapon.as`、`Solid.as`、`Character.as`、`Controller.as`。先前 Unity 通用水中失效、固定坐标和保险超时已移除；不同武器仍保留各自 AS2 的完成条件。

## 公共结束规则（本轮修复规格）

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| WPN-LIFE-01 | 已发射且未完成的继承型武器仅在下降越过 `levelHeight × 32` 或 `vx == 0 && abs(vy) < 0.2` 时按公共规则结束；不因 8 秒、固定 X/Y 坐标或入水短计时结束 | `Weapon.as::advance`；`Solid.as::splashCheck` | `MutinyWeapon.AdvanceInheritedFinishTick`；CherryBomb、Dynamite、RumBottle 的生产 tick | 三种武器经工厂发射，在无碰撞长地图持续 205 tick 后再越过地图底部 | 静态确认、已实现；隔离 Unity 6000.6 Play Mode 专项通过，主工程画面待验收 |
| WPN-LIFE-02 | 专属 `advance` 状态机优先决定结束，公共回合等待不因 150 tick 强制销毁仍在活动的武器 | `Controller.as::enterFrame`；`Character.as::advance` 与各武器 `advance` | `MutinyTurnManager.AdvanceSimulationTick` | 海啸经生产入口连续飞行超过 205 回合 tick，直到自己的地图宽度条件结束后才经 11 tick 静止门换回合 | 静态确认、已实现；隔离 Unity 6000.6 Play Mode 专项通过，主工程画面待验收 |
| DYN-WATER-02 | Dynamite 入水只切到 `unlit` 画面；静止时仍按原版 `advanceMotion` 生成 250/70 爆炸，不存在 dud/0.35 秒销毁 | `Dynamite.as::advanceMotion` | `MutinyDynamite.OnWaterSubmerged`、`AdvanceOriginalDetonationTick` | 正式投掷后过水线并停稳，核对先切帧、未提前结束、随后爆炸 | 静态确认、已实现；隔离 Unity 6000.6 Play Mode 专项通过，主工程画面待验收 |
| WPN-WATER-01 | `Solid.splashCheck` 只产生穿越水面的反馈；只有角色 `Character.advance` 在水下应用速度衰减，普通武器入水不被施加角色专属水下阻力 | `Solid.as::splashCheck`；`Character.as::advance` | `MutinyPhysicsBody.EvaluateWaterState`、`MutinyWeapon.Initialize` | RumBottle 正式发射并越过水线后继续按原初速水平飞行，未结束；角色专属阻力仍由原开关控制 | 静态确认、已实现；隔离 Unity 6000.6 Play Mode 专项通过，角色水中画面待验收 |
