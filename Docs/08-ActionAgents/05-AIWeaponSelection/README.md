# 08.05 · AI 武器选择

[返回上级模块](../README.md)

## 职责

还原 AI 的库存评估、武器评分和选择规则。

## 边界

不执行具体武器效果。

## 当前规格与进度

参见 [如何选择与使用武器](../AI_IMPLEMENTATION_STATUS.md#6-如何选择与使用武器)、[15 种库存武器的当前 AI 覆盖](../AI_IMPLEMENTATION_STATUS.md#7-15-种库存武器的当前-ai-覆盖)、[AI 完整逻辑流程（HTML）](../AI_LOGIC_FLOW.html) 和 `AI-WPN-*` 验收矩阵。

2026-09-28：香蕉近角色预测结束及普通武器装备起点修复见 [AI-PHY-04/05 规格与结果](PREDICTION_PARITY_20260928.md)。本次专项 18/18 通过；扩大回归有 4 条修复前后均失败的旧用例，不能写成全量通过。

高 Luck 分帧搜索见 [实现与验收](AI_RESPONSIVE_SEARCH_20260928.md)；第 12 关尸体导致反复重算的修复、具体失效日志及 54/54 实测见 [EXT-AI-SNAPSHOT-02/03](AI_SNAPSHOT_CORPSE_FIX_20260928.md)。

2026-09-29：`aienhance` 选择独立策略入口，兼容模式保持原有候选/评分/随机顺序；增强入口为明确的 `enhanced-bootstrap / legacy-fallback`，不代表完整效果模拟已完成。切换与输出绑定规格、当前实现及实际验收见 [EXT-AI-STRAT-01..05](AI_STRATEGY_BOUNDARY_20260929.md)。
