# 04 · 角色与队伍

## 职责

- 角色属性、生命值、库存、方向和所属队伍。
- 主动移动、被动击飞、受伤、死亡及落水状态。
- 队伍成员、存活统计和角色生命周期。

## 当前入口

- `Assets/Mutiny/Scripts/Simulation/MutinyCharacter.cs`
- `Assets/Mutiny/Scripts/Simulation/MutinyCharacterAnimator.cs`
- `Assets/Mutiny/Scripts/Simulation/MutinyTeam.cs`

## 边界

角色保存权威状态；覆盖层、动画和音频只表现这些状态。

## 子模块

| 目录 | 内容 |
| --- | --- |
| [01-CharacterState](01-CharacterState/README.md) | 角色状态 |
| [02-TeamState](02-TeamState/README.md) | 队伍状态 |
| [03-Inventory](03-Inventory/README.md) | 角色库存 |
| [04-ActiveAndPassiveMovement](04-ActiveAndPassiveMovement/README.md) | 主动与被动移动 |
| [05-DamageDeathAndDrowning](05-DamageDeathAndDrowning/README.md) | 伤害、死亡与落水 |

## 后续文档

角色状态表、27 种角色映射、死亡流程和库存规则。
