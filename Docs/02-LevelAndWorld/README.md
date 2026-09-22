# 02 · 关卡与世界构建

## 职责

- 解析原版关卡 XML。
- 创建地形、背景、碰撞体、角色出生点和关卡对象。
- 定义坐标换算、地图边界、水面及对象排序。

## 当前入口

- `Assets/Mutiny/Scripts/Level/MutinyLevelXmlParser.cs`
- `Assets/Mutiny/Scripts/Level/MutinyLevelBuilder.cs`
- `Assets/Mutiny/Scripts/Level/MutinyLevelRoot.cs`
- `Assets/Mutiny/Scripts/Level/MutinyTileDefinition.cs`

## 边界

本模块负责构造世界，不决定角色本回合可以执行什么行动。

## 子模块

| 目录 | 内容 |
| --- | --- |
| [01-LevelDataParsing](01-LevelDataParsing/README.md) | 关卡数据解析 |
| [02-WorldBuilding](02-WorldBuilding/README.md) | 世界构建 |
| [03-TilesAndCollision](03-TilesAndCollision/README.md) | 瓦片与碰撞 |
| [04-ObjectsAndSpawns](04-ObjectsAndSpawns/README.md) | 对象与出生点 |
| [05-WorldBoundsAndWater](05-WorldBoundsAndWater/README.md) | 世界边界与水面 |

## 后续文档

坐标规范、对象映射、碰撞分类和 18 关构建验收表。
