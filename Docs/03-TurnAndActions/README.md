# 03 · 回合与行动

## 职责

- 管理当前队伍、当前角色和回合阶段。
- 管理移动、跳跃、武器及取消操作的资格与状态转换。
- 等待动态对象结算，并触发换队或胜负判断。

## 当前入口

- `Assets/Mutiny/Scripts/Simulation/MutinyTurnManager.cs`
- `Assets/Mutiny/Scripts/Simulation/MutinyTeam.cs`

## 边界

行动资格只由本模块维护；UI、玩家输入和 AI 不自行复制回合规则。

## 子模块

| 目录 | 内容 |
| --- | --- |
| [01-TurnStateMachine](01-TurnStateMachine/README.md) | 回合状态机 |
| [02-CharacterSelection](02-CharacterSelection/README.md) | 角色选择 |
| [03-ActionEligibility](03-ActionEligibility/README.md) | 行动资格 |
| [04-ActionSettlement](04-ActionSettlement/README.md) | 行动结算 |
| [05-VictoryAndDefeat](05-VictoryAndDefeat/README.md) | 胜负判定 |

## 后续文档

完整状态图、行动命令、结算条件和异常中断规则。
