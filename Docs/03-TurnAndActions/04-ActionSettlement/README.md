# 03.04 · 行动结算

[返回上级模块](../README.md)

## 职责

等待角色、武器和世界对象稳定后结束行动。

## 边界

不复制物理模块的静止公式。

## Cannon AI 待开火结算门

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| CAN-AI-TURN-01 | 已提交的 AI Cannon 在 25 tick 倒计时内即使所有物理体静止也不开始回合静止计时；炮弹发射、结算及炮身结束后再按通常 11 tick 静止门切换回合，下一回合空投不得提前生成；大炮及炮弹不因 Unity 通用 150 tick 超时被强制结束 | `Cannon.as::aiPerform/advance`、`Cannonball.as::advance`；`Character.as::advance` 在选中武器未发射或未结束时重置 `Controller.inactivity`；`Controller.as::enterFrame/nextTurn` | `MutinyCannon.IsAiFirePending`、`MutinyTurnManager.CheckAllBodiesAtRest()`、两个对象的安全超时资格 | AI 正式执行入口提交后推进 24/25 tick，等待炮弹超过 150 tick，再结束炮弹与炮身并推进静止门 | 已实现、C# 编译通过；待 Unity 运行验证 |
