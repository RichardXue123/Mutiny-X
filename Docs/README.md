# Mutiny X 文档

本目录按运行时职责组织。11 个一级模块下已建立 73 个子模块入口；当前版本只定义模块边界，详细行为规格将在后续逐模块补充。

## 模块映射

| 编号 | 模块 | 说明 |
| --- | --- | --- |
| 01 | [游戏流程与进度](01-GameFlowAndProgression/README.md) | 菜单、关卡进入、结算、解锁和存档 |
| 02 | [关卡与世界构建](02-LevelAndWorld/README.md) | XML、地形、对象和世界边界 |
| 03 | [回合与行动](03-TurnAndActions/README.md) | 回合状态机和行动资格 |
| 04 | [角色与队伍](04-CharactersAndTeams/README.md) | 角色状态、生命、队伍和死亡 |
| 05 | [物理与战斗结算](05-PhysicsAndCombat/README.md) | 固定步长、碰撞、爆炸和落水 |
| 06 | [武器与效果](06-WeaponsAndEffects/README.md) | 武器公共生命周期和专用规则 |
| 07 | [动态世界事件](07-DynamicWorldEvents/README.md) | 空投、拾取和环境事件 |
| 08 | [行动决策入口](08-ActionAgents/README.md) | 玩家输入和 AI 决策 |
| 09 | [表现与反馈](09-PresentationAndFeedback/README.md) | UI、镜头、动画和音频 |
| 10 | [原版证据](10-OriginalEvidence/README.md) | AS2、pcode、资源和扫描结果 |
| 11 | [验证与调试](11-VerificationAndDebug/README.md) | 回归、日志和 GM 命令 |

## 基本约定

- 每条行为规则使用稳定 ID，并记录原版来源、Unity 入口、验收步骤和实际结果。
- 原版结论以 AS2、pcode、时间轴、资源及运行证据为准，不从当前 C# 反推。
- “静态确认”“已实现”“实际测试通过”“待运行验证”“已知差异”分别记录。
- 玩家和 AI 通过同一行动接口驱动游戏；表现层只消费状态和事件。
- 原始证据只增补或重新生成，不因文档整理而删除。

## 主要依赖方向

`流程 → 关卡 → 回合 → 角色/武器/世界事件 → 物理结算 → 表现`；玩家输入和 AI 从行动入口提交请求，结算结果再写入进度。
