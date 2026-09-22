# 02.02 · 世界构建

[返回上级模块](../README.md)

## 职责

根据数据模型创建关卡根对象和运行时内容。

## 边界

只组装对象，不承担对象内部玩法。

## 关卡级动态状态重置

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| BOX-LVL-01 | 重新开始或进入下一关时，旧关卡的全部木箱/火药桶状态在新关构建前清空 | `TileSystem.as::readXML` line 72：`Controller.boxes=[]` | `MutinyLevelController.ClearLevel`、`MutinyLevelBuilder.BuildLevel` | 旧箱仍处于 Unity 延迟销毁窗口时调用生产 `BuildLevel`，断言 `MutinyBoxRegistry.Count==0` | 已实现；自动回归已写，待 Unity 运行验证 |
