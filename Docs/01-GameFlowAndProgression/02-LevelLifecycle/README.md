# 01.02 · 关卡生命周期

[返回上级模块](../README.md)

## 职责

管理进入、加载、重开和退出关卡。

## 边界

协调流程与关卡控制器，不实现关卡内容。

## 箱体结束清理规格

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| BOX-END-01 | 胜利、失败或平局进入 `GameOver` 的同一结算边界时，所有已放置和仍待放置的 WoodenCrate/GunpowderBarrel 立即停止显示与碰撞，并清空共享箱体注册；不能等待玩家点击下一关、重试或退出 | `Controller.as::endGame → unloadLevel:88-120`，其中逐项 `destroy()` `Controller.boxes` | `MutinyTurnManager.EvaluateTurnOrGameOver()` → `MutinyBoxRegistry.ClearForLevelEnd()` | 分别驱动生产胜利与失败判定，断言两类对象均失活且 `MutinyBoxRegistry.Count==0` | 已实现；自动回归已写，待 Unity 运行验证 |
