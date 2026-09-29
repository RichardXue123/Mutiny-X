# 11.04 · 日志

[返回上级模块](../README.md)

## 职责

定义各模块核心状态转换和关键参数日志。

## 边界

日志应可关闭且不改变模拟时序。

## AI 行动诊断

[GM-10 · AI 行动决策日志规格](AI_ACTION_LOG_SPEC.md)：`ailog 1/0` 控制每次已提交 AI 行动的一条完整参数、评分和竞争候选摘要日志。它是 Unity 调试扩展，不属于原版玩法。

2026-09-29 策略边界扩展：实际行动行增加 `mode/strategy/algorithm/fallback/strategyVersion`；增强入口尚未实现完整效果模拟时明确显示 `enhanced-bootstrap`、`algorithm=legacy`、`fallback=True`。取消的策略任务不打印提交日志；原有候选 trace JSON 不新增字段，保持现有算法重放兼容。
