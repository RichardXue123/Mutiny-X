# 11.04 · 日志

[返回上级模块](../README.md)

## 职责

定义各模块核心状态转换和关键参数日志。

## 边界

日志应可关闭且不改变模拟时序。

## AI 行动诊断

[GM-10 · AI 行动决策日志规格](AI_ACTION_LOG_SPEC.md)：`ailog 1/0` 控制每次已提交 AI 行动的一条完整参数、评分和竞争候选摘要日志。它是 Unity 调试扩展，不属于原版玩法。

2026-09-29 策略边界扩展：实际行动行增加 `mode/strategy/algorithm/fallback/strategyVersion`。随后 [effects-v1 初版](../../08-ActionAgents/05-AIWeaponSelection/AI_ENHANCED_SIMULATION_V1.md) 使用 `enhanced-effects-v1 / effects-v1 / fallback=False`，新增 HP 前后、分项分数、预算、模拟种子和连续计划，并把全部精算结果写入独立 `.enhanced.json`。取消任务不打印提交日志、不发布未完成文件；原有 Legacy 候选 JSON 字段不变。
