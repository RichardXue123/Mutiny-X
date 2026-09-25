# 01.02 · 关卡生命周期

[返回上级模块](../README.md)

## 职责

管理进入、加载、重开和退出关卡。

## 边界

协调流程与关卡控制器，不实现关卡内容。

## 关卡内持久物体的结算与刷新规格

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| LVL-PERSIST-01 | 胜负判定、胜利/失败 speech 及结果弹窗期间，已放置的木箱、火药桶、地雷继续属于当前关卡；不能因 `GameOver` 提前隐藏或移除箱体碰撞 | `Controller.as::nextTurn` 只创建 `speechBubble`；`Controller.as::enterFrame` 在 `completeTime>160` 后显示 popup，仍未调用 `unloadLevel` | `MutinyTurnManager.EvaluateTurnOrGameOver()`、`MutinyGameHUD.SynchronizeGameEndPopup()` | 通过生产回合胜/负判定，断言 speech/弹窗期间两类箱体仍显示且碰撞注册保留，地雷仍存在 | 已实现；Unity Play Mode 回归通过 |
| LVL-RESET-01 | 重开、下一关或明确退出当前对局时才卸载旧关，销毁旧木箱、火药桶与地雷；新关 XML 读取时重新建立空集合 | `Controller.as::changeLevel → unloadLevel`、`Controller.as::endGame → unloadLevel`；`TileSystem.as::readXML` 重设 `Controller.boxes/mines=[]` | `MutinyLevelController.ClearLevel()`、`MutinyLevelBuilder.BuildLevel()` | 由关卡控制器加载下一关及显式卸载，断言旧对象失活、箱体注册为空 | 已实现；Unity Play Mode 回归通过；实际剧情画面待验收 |
