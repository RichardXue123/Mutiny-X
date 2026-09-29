# 08.06 · AI 武器执行

[返回上级模块](../README.md)

## 职责

还原瞄准、延迟、专用 aiPerform 分支和结束等待。

## 边界

通过与玩家相同的武器入口执行。

## 当前规格与进度

参见 [执行与行动消耗](../AI_IMPLEMENTATION_STATUS.md#8-执行与行动消耗)、[已知差异与风险](../AI_IMPLEMENTATION_STATUS.md#9-已知差异与风险) 和 `AI-EXE-*` 验收矩阵。

2026-09-29 策略边界：`AIMove.StrategyContext` 随正式输出提交，绑定到实际武器及连续箱体/炮弹子对象；已提交武器不因 `aienhance` 全局切换中断或改写身份。此次仅新增身份上下文，专用武器控制仍保持现有实现；详见 [EXT-AI-STRAT-04](../05-AIWeaponSelection/AI_STRATEGY_BOUNDARY_20260929.md)。
