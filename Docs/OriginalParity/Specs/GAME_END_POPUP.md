# 单人关卡结算弹窗

范围：原版单人游戏在一方全灭后显示的 `popup`。双人 `vs_*` 帧和外部排行榜提交页不在本次实现范围。

## 原版来源

| 证据 | 定位 | 结论 |
| --- | --- | --- |
| S | `Controller.as::nextTurn`、`Controller.as::enterFrame` | 敌方 AI 队全灭时解锁下一关、累加分数；玩家队全灭时为失败。双方全灭立即失败。普通胜负台词结束后才进入 popup。 |
| S | `Controller.as::get1PLevelScore` | 本关分数为 `max(floor(存活角色生命总和/全队人数 * 20 - 玩家队已行动回合数 * 25), selected_level * 10)`。 |
| S | `IngamePopup.as::onEnterFrame/reset` | `popup.show` 每个 25 Hz tick 改变 alpha 25；胜利框的本关分数每 tick 加 287、累计分数每 tick 加 347，均从 0 开始。 |
| A/S | `DefineSprite_342_popup` 的 `level_complete_1p`、`level_failed_1p`、`game_complete_1p` 帧及 `NextLevelButton`、`QuitGameButton`、`RestartLevelButton`、`CongratulationsButton` | 胜利框含 `level complete`、`level score`、`total score`、Next level、Back to title；失败框含 `level failed`、`final score`、Restart level；第 15 关胜利为 `game_complete_1p`。 |

## 规则

| ID | 前置与输入 | 原版状态变化和可见结果 | Unity 入口 / 用例 | 状态 |
| --- | --- | --- | --- | --- |
| END-POP-01 | 单人游戏、结算时敌方全灭 | 解锁 `selected_level + 1`，按原公式加入 session score；正常台词结束后进入 `level_complete_1p`，第 15 关进入 `game_complete_1p`。 | `MutinyTurnManager.EvaluateTurnOrGameOver` → `MutinyLevelController.AwardSinglePlayerLevelWin` → `MutinyGameHUD`；`END-POP-T01` | 已实现；Unity Play Mode 待执行 |
| END-POP-02 | 单人游戏、玩家全灭或双方全灭 | 显示 `level_failed_1p`；累计分数不增加。 | `MutinyGameHUD.ResolveGameEndPopupKind`；`END-POP-T02` | 已实现；Unity Play Mode 待执行 |
| END-POP-03 | popup 显示/隐藏 | alpha 以每 25 Hz tick 25% 变化，四 tick 完成。 | `MutinyGameHUD.AdvanceGameEndPopupState`；`END-POP-T03` | 已实现；Unity Play Mode 待执行 |
| END-POP-04 | 胜利 popup 显示 | `level score` 和 `total score` 都从 0 分别以 287 / 347 每 tick 递增至目标。 | `MutinyGameHUD.AdvanceDisplayedScore`；`END-POP-T04` | 已实现；Unity Play Mode 待执行 |
| END-POP-05 | 点击 Next level / Back to title | Next level 增加关卡号并加载下一关；Back to title 结束关卡并去单人选关。 | `MutinyGameHUD.AdvanceToNextLevel` / `BackToSinglePlayerMenu`；`END-POP-T05` | 已实现；Unity Play Mode 待执行 |
| END-POP-06 | 点击 Restart level | 重载本关并将 session score 清为 0。 | `MutinyGameHUD.RestartFromGameEndPopup` → `MutinyLevelController.RestartCurrentLevel`；`END-POP-T06` | 已实现；Unity Play Mode 待执行 |

## 时序差异与待验收

- 原版在通常的胜利或失败前先显示约 161 个模拟 tick 的胜负台词；双方同时死亡直接显示失败框。当前 Unity 尚未实现这一段原版结算台词，因此在生产结算进入 `GameOver` 后立即请求 popup 淡入。该差异保持为待处理项，不能把当前弹窗延迟误当成已还原台词。
- 原版失败时若既有累计分数大于 0，会额外提供 `submit score` 并跳转外部高分页；当前没有该服务，所以没有伪造该按钮。
- 原版第 15 关的 Congratulations 按钮去专门的 `congratulations` 时间轴。当前没有该页面，按钮返回单人选关；该分支待实现完整结尾页时复核。
