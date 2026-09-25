# 03.04 · 行动结算

[返回上级模块](../README.md)

## 职责

等待角色、武器和世界对象稳定后结束行动。

## 边界

不复制物理模块的静止公式。

原版 `Controller.enterFrame` 的 `inactivity > 10` 只在已静止时评估行动/回合，活动角色持有未完成武器会持续把 `inactivity` 清零。不存在“同一武器阻塞 150 tick 后强制结束”的第二个门槛；对应公共规则为 `WPN-LIFE-02`，见[投射物生命周期](../../06-WeaponsAndEffects/03-ProjectileLifecycle/README.md)。

## Cannon AI 待开火结算门

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| CAN-AI-TURN-01 | 已提交的 AI Cannon 在 25 tick 倒计时内即使所有物理体静止也不开始回合静止计时；炮弹发射、结算及炮身结束后再按通常 11 tick 静止门切换回合，下一回合空投不得提前生成；大炮及炮弹不因非原版 150 tick 超时被强制结束 | `Cannon.as::aiPerform/advance`、`Cannonball.as::advance`；`Character.as::advance` 在选中武器未发射或未结束时重置 `Controller.inactivity`；`Controller.as::enterFrame/nextTurn` | `MutinyCannon.IsAiFirePending`、`MutinyTurnManager.CheckAllBodiesAtRest()` | AI 正式执行入口提交后推进 24/25 tick，等待炮弹超过 150 tick，再结束炮弹与炮身并推进静止门 | 已实现、C# 编译通过；待 Unity 运行验证 |
