# 08.06 · AI 武器执行

[返回上级模块](../README.md)

## 职责

还原瞄准、延迟、专用 aiPerform 分支和结束等待。

## 边界

通过与玩家相同的武器入口执行。

## 当前规格与进度

参见 [执行与行动消耗](../AI_IMPLEMENTATION_STATUS.md#8-执行与行动消耗)、[已知差异与风险](../AI_IMPLEMENTATION_STATUS.md#9-已知差异与风险) 和 `AI-EXE-*` 验收矩阵。

2026-09-29 策略边界：`AIMove.StrategyContext` 随正式输出提交，绑定到实际武器及连续箱体/炮弹子对象；已提交武器不因 `aienhance` 全局切换中断或改写身份。历史框架规格见 [EXT-AI-STRAT-04](../05-AIWeaponSelection/AI_STRATEGY_BOUNDARY_20260929.md)。

后续 [effects-v1 初版](../05-AIWeaponSelection/AI_ENHANCED_SIMULATION_V1.md) 增加不可变 `ActionPlan`：气球持续左/不扇/右、箱体具体摆放点随胜出方案提交。金币在 2026-09-30 改为 [不可变贪心策略参数](../05-AIWeaponSelection/AI_GREEDY_COINS.md)：八发预计速度保留作诊断，但真实续射分帧读取最新局面择优，不回放旧速度。实际仍调用生产武器入口；玩家及 Legacy 没有该计划，保留原操作。关闭开关不改写已经提交的策略和采样数。
