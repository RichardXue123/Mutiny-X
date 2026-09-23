# 08 · 行动决策入口

## 职责

- 将鼠标输入转换为标准行动请求。
- 让 AI 选择角色、移动目标、武器和瞄准参数。
- 通过统一入口提交、取消或完成行动。

## 当前入口

- `Assets/Mutiny/Scripts/Presentation/MutinyPlayerInput.cs`
- `Assets/Mutiny/Scripts/Simulation/MutinyAIController.cs`
- [AI 逻辑实现进度与实现说明](AI_IMPLEMENTATION_STATUS.md)

## 边界

玩家和 AI 负责“选择行动”，不直接改写角色、武器或回合的权威状态。

## 子模块

| 目录 | 内容 |
| --- | --- |
| [01-ActionCommands](01-ActionCommands/README.md) | 行动命令 |
| [02-PlayerInput](02-PlayerInput/README.md) | 玩家输入 |
| [03-AICharacterSelection](03-AICharacterSelection/README.md) | AI 角色选择 |
| [04-AIMovement](04-AIMovement/README.md) | AI 移动 |
| [05-AIWeaponSelection](05-AIWeaponSelection/README.md) | AI 武器选择 |
| [06-AIWeaponExecution](06-AIWeaponExecution/README.md) | AI 武器执行 |

## 后续文档

行动请求接口和输入映射。AI 决策树、原版评分规则、当前覆盖和差异见 [AI 逻辑实现进度与实现说明](AI_IMPLEMENTATION_STATUS.md)。
