# 01 · 游戏流程与进度

## 职责

- 主菜单、人数选择和关卡选择。
- 进入、重开、退出关卡及胜负结算。
- 关卡解锁、分数和持久化进度。

## 当前入口

- `Assets/Mutiny/Scripts/Presentation/MutinyFrontendController.cs`
- `Assets/Mutiny/Scripts/Presentation/MutinyFrontendFlow.cs`
- `Assets/Mutiny/Scripts/Level/MutinyLevelController.cs`
- `Assets/Mutiny/Scripts/Persistence/MutinySaveSystem.cs`

## 边界

本模块决定“进入哪个游戏状态”，不实现回合、武器或具体画面效果。

## 子模块

| 目录 | 内容 |
| --- | --- |
| [01-FrontendNavigation](01-FrontendNavigation/README.md) | 前端导航 |
| [02-LevelLifecycle](02-LevelLifecycle/README.md) | 关卡生命周期 |
| [03-ResultsAndProgression](03-ResultsAndProgression/README.md) | 结算与进度 |
| [04-Persistence](04-Persistence/README.md) | 持久化 |
| [05-TwoPlayerMode](05-TwoPlayerMode/README.md) | 原版双人模式行为规格与[复刻计划](05-TwoPlayerMode/PLAN.md) |

## 后续文档

菜单状态图、结算分支、解锁规则和存档字段。
