# 选中角色在蓄力中死亡：结算验证（2026-09-29）

## 规格与修改

- 原版静态确认：`Character.as::advance:240-247` 死亡解除未投掷活动门但保留血条结算；`Team.as::isTurnComplete:299-305` 选中者死亡即完成；`Controller.as::enterFrame:231-241` 在正常 inactivity >10 后判断。原件位于 `Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/throwgame/`。
- 已实现：`MutinyTurnManager.HasSelectedCharacterDied` 区分真正的选中死者与未选人；死亡进入 Settling，正常活动对象门和 11 tick 静止门仍保留，不伪造行动提交。
- 已实现的用户确认输入保护：`PlayerInput.Update` 无需指针释放就清理死者蓄力、轨迹、未提交武器和手势，不清队伍选中引用，不消费未发射武器；已提交效果保留。modal speech/quit 输入仍走正常处理。

## 实际测试与证据

Unity 6000.6.0f1，独立临时工程：
`C:/Users/27487/AppData/Local/Temp/mutiny-selected-death-68b8d053c2cd4e7d85d311a2cbb277f1`。
使用主工程当前生产脚本及新增回归。未打开或停止用户的主工程编辑器；只终止了本测试自己创建的首轮超时进程。

- 首个完整修复运行 `selected-death-fixed-3.log`：死亡结算专项实际 **45/45 通过**。
- 缺陷检出验证 `selected-death-old-guard.log`：只在临时工程将 `EvaluateTurnOrGameOver` 的前置门回退为 `if (!m_ActionCommittedThisTurn)`，其他修复仍保留。实际 **39/45**，按住死亡/死亡后松手/武器蓄力死亡各有两条换回合断言失败。此结果证明用例检出旧门造成的卡回合，不代表整个历史版本被重新验收。
- 上述检出运行同时复跑兼容检查：镜头 **11/11 通过**，角色蓄力覆盖层/取消 **31/32**，唯一失败为透明取消叉材质。隔离工程 manifest 未含主工程的 URP 17.6.0，不能解析原有 URP Sprite-Unlit shader。只补齐临时工程包依赖，没有改生产材质、着色器或测试期望。
- 最终补齐 URP 17.6.0 并恢复修复条件后的 `selected-death-final.log`：**死亡结算 45/45、蓄力覆盖层/取消 32/32、镜头 11/11 全部通过，共 88/88**；Unity 实际编译运行通过，无验证失败。回合管理器、玩家输入及新增 runtime/editor 测试脚本逐文件 SHA-256 与当前工作区一致。
- 最终核对时发现工作区另有镜头滚动改动，本次保留、不归入死亡结算修复；同步该镜头脚本到隔离工程后单独复跑生产镜头入口，`selected-death-camera-current.log` 第 500 行确认 **11/11 通过**。

摘录：[TURN-DEATH-20260929.txt](Artifacts/TURN-DEATH-20260929.txt)。原始完整日志保留在上述临时工程，不将大量音频/旋转日志复制进仓库。

首轮夹具错误也不计通过：在当前回合调用 `Mine.Fire` 会通过生产入口提交一次行动，不能代表旧回合已放置地雷；已改为在 TurnManager 创建前发射，随后启动真实回合。第二轮暴露两个夹具时序问题：显示帧可能不含 25 Hz 回合 tick，及 1 px/tick 的被动速度会先被摩擦消除。最终分别等待正式回合 tick，并使用真实冲量保留移动触发条件，未修改生产规则或降低死亡/换队断言。

## 后续基线复验

本记录的 45/45 与哈希一致指上述死亡修复运行当时的工作区。随后 `AIM-MOVE-01/02` 修改输入/轨迹并新增四条真实 Mine 用例，死亡与移动预览组合实际 **49/49**，取消 **32/32**、镜头 **11/11** 复验通过，见 [实时轨迹验证](AIM_MOVING_ORIGIN_20260929.md)。旧 45/45 不单独证明后续基线。

## 待运行验证与已知边界

- 主工程真实地图完整对局、实际鼠标按住/松手、Android 多指及手柄蓄力未运行。
- Android 胜负 speech 的触摸推进仅静态检查保留处理优先级，未计为真机通过。
- 本专项不验收其他持续武器在使用者死亡后的完整专用序列，也不改地雷触发、伤害、蜂鸣或持久对象刷新规则。
