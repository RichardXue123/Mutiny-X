# 07 · 动态世界事件

## 职责

- 空投刷新、降落、落地、拾取和奖励发放。
- 回合期间独立存在的环境对象与事件。
- 向回合、AI、镜头和音频发布事件结果。

## 当前入口

- `Assets/Mutiny/Scripts/Simulation/MutinyTreasureChestManager.cs`
- `Assets/Mutiny/Scripts/Simulation/MutinyTreasureChest.cs`

## 边界

本模块管理世界事件本身；AI 只评估其价值，镜头和音频只表现事件。

## 子模块

| 目录 | 内容 |
| --- | --- |
| [01-AirDropScheduling](01-AirDropScheduling/README.md) | 空投调度 |
| [02-ChestDescentAndLanding](02-ChestDescentAndLanding/README.md) | 宝箱降落与落地 |
| [03-PickupAndRewards](03-PickupAndRewards/README.md) | 拾取与奖励 |
| [04-WorldEventCoordination](04-WorldEventCoordination/README.md) | 世界事件协调 |

## 后续文档

空投概率、关卡生成表、拾取判定和事件时序。
