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

## 选中角色在未提交行动时死亡（2026-09-29）

| ID | 可观察行为及状态转换 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| TURN-DEATH-01 | 当前选中角色在跳跃蓄力或待命期间死亡，即使未提交行动，也进入结算；保留选中死者引用直到回合结束，不能重新选同队角色继续本回合 | `Character.as::advance:240-247` 死亡解除未投掷门但继续等血条；`Team.as::isTurnComplete:299-305`；`Controller.as::enterFrame:231-241` | `MutinyTurnManager.AdvanceSimulationTick/EvaluateTurnOrGameOver` | 正式选人、跳跃拉线触发实际 Mine 倒计时及爆炸；按住不松开也正常换队且只换一次 | 静态确认；已实现；隔离 Play Mode 专项 45/45 通过；主工程/真机待验收 |
| TURN-DEATH-02 | 死亡不伪造跳跃提交，不立即换队；继续等爆炸、血条和其他活动对象，然后经过通常 11 tick 静止门。队伍全灭时走胜负而非新回合 | 同上；`Controller.as::nextTurn` | 通常活动对象门、胜负判断、`MutinyTeam.IsTurnComplete` | 真实爆炸/地形运动/血条推进期间不换队；最后一人致死进入 GameOver | 静态确认；已实现；专项实际验证 10/11 tick 边界、血条及 GameOver；主工程/真机待验收 |
| TURN-DEATH-03 | 只对实际选中的死者终止回合；开局未选人、活着的角色仍在拉线、未选中队友死亡均不得误结束回合 | `Team.as::advance:115-118` 未选人重置 inactivity；`Character.as::advance:226-247` | 回合死亡条件 | 未选人静止等待、未选中队友死亡、非致命 Mine 爆炸后继续拉线并松开起跳 | 静态确认；已实现；专项边界用例实际通过；主工程/真机待验收 |

输入侧死亡清理只释放蓄力、轨迹、未提交武器和鼠标/触屏/手柄手势，不扣库存、不起跳、不销毁已提交武器；`MIN-AIM-01` 的存活炸飞规则保持不变。新增验证必须驱动生产选人、蓄力、Mine/Explosion、物理和回合流程，不直接设置死亡、行动提交或静止布尔量。

证据边界：AS2 静态确认的是死亡无需等待提交跳跃即可结束回合；`Mine.as` 本身不会清除 `Controller.twanging`。Unity 的死亡手势清理及死亡后旧 release 不再投掷死者是用户已确认修复方案中的输入保护，不宣称 AS2 存在同名清理步骤；存活角色的原版拉线行为单独由 `MIN-AIM-01` 验收。

实际结果：Unity 6000.6.0f1 隔离 Play Mode 死亡结算 **45/45**、蓄力覆盖层/取消兼容 **32/32**、镜头 **11/11**；仅在临时工程回退旧“未提交行动”条件时，死亡换队相关六条断言失败。本次修改的回合管理器、玩家输入及新增两份测试脚本逐文件 SHA-256 对比一致。详见 [验证记录](../../11-VerificationAndDebug/02-PlayModeValidation/SELECTED_DEATH_SETTLEMENT_20260929.md)。
