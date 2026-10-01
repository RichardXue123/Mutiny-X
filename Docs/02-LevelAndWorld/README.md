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

- [关卡模式编号与 GM 直达](01-LevelDataParsing/LEVEL_IDENTITY_AND_GM_ENTRY.md)：`level_1_XX` / `level_2_XX`、旧资源迁移及临时单人 16。
- [单人 16 悬浮扁平货船（当前第四版）](04-ObjectsAndSpawns/LEVEL_1_16_FLAT_FREIGHTER.md)：80×27 格，保留木船与 13 组小行星，右侧扁平敌舰悬浮 3 格，5 对 8。
- [三舰初稿历史记录](04-ObjectsAndSpawns/LEVEL_1_16_SPACE_DRAFT.md)：旧 115×36 / 10 对 11 基线。

坐标规范、对象映射、碰撞分类和 18 关构建验收表。
