# 06.11 · Mine

[返回上级模块](../README.md)

## 职责

记录地雷的布置、触发、警告和爆炸行为。

## 边界

参数和时序以原版证据为准。

## 逻辑摘要

- extent 14、friction 1.5、dragRange 180、跨回合存在。
- `in_throw` 固定显示 frame 1；落稳后播放 `arm` frame 11..16 并停在 16；触发后循环 `warn` 的可见 frame 21..29（frame 30 跳回 `warn`）。
- 原版投掷、落地和布置动画都没有专用音效；触发瞬间及后续指定 tick 播放 `mine_beep`，爆炸播放 `pop`。
- 发射 10 tick 后检查角色脚点 60 px 内的运动目标；由爆炸/碰撞造成的被动速度同样满足条件。
- 普通静止角色不触发；但正在拉 Throw Self 的角色等价于原版 `Controller.twanging==character`，即使速度为零也立即触发，且 Mine 不取消这次拉线。
- 激活后倒数 60 tick，按 `[0,15,30,38,45,49,53,55,57,59]` 蜂鸣，最后爆炸 `250/70`。

来源：`Mine.as`。规则：`MIN-*`，见 [完整审计](../IMPLEMENTATION_DETAILS.md#67-mine)。

## 行为规格与实现映射

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| MIN-ANI-01 | 飞行保持 frame 1；静止播放 arm 11..16；触发循环 warn 21..29 | symbol 1024 labels/actions；frames 1/16/30 actions | `MutinyMine.AdvancePresentationTick` | 生产投掷后逐 tick 断言 1→11..16，触发后断言 frame 21 与循环状态 | 已实现；待 Unity 运行验证 |
| MIN-AUD-01 | 投掷/布置无声音；触发起点和指定 elapsed tick 播放十次 `mine_beep`；爆炸 `pop` | `Mine.as::advance/explode`；symbol 1024 无 StartSound | `MutinyPlayerInput.Launch`、`PlayInitialBeep`、倒计时调度 | 监听生产 AudioManager，断言 Mine 投掷零请求、倒计时十次 beep | 已实现；待 Unity 运行验证 |
| MIN-TRG-01 | 范围内移动角色或正在拉 Throw Self 的静止角色触发；普通静止角色不触发 | `Mine.as::checkForProximity` | `TryActivateForCharacter`、`NotifyCharacterBeganSelfThrowAim` | 实际 PlayerInput 开始拉线，断言 warning 启动且输入仍为 Aiming | 已实现；待 Unity 运行验证 |
| MIN-AIM-01 | Mine 触发/爆炸不取消 Throw Self；角色被炸到空中仍能松开并提交蓄力结果 | `Controller.twanging` 只参与触发资格；Mine 不清除它 | `MutinyPlayerInput` 原 Aiming/commit 状态机 | 触发倒计时、应用 250/70 爆炸后断言仍 Aiming，再调用生产提交入口 | 已实现；待 Unity 运行验证 |
