# Mutiny X

使用 Unity 6（6000）重制 Nitrome 的 Flash 游戏《Mutiny》。通过分析原版 SWF 的 ActionScript、关卡数据和资源，逐步还原地图、角色、武器、回合规则与操作手感。

当前处于逆向准备阶段。Unity 工程基本为 URP 2D 模板，尚未实现游戏玩法。详细任务与完成标准见 [TODO.md](TODO.md)。

## 目录与开发环境

```text
D:\My Project\Mutiny X\
├─ Mutiny Source\mutiny-flash-game\   原版 SWF、XML 关卡及归档文件
└─ Mutiny X\                         本 Unity 工程
   ├─ Assets\
   ├─ Packages\
   ├─ ProjectSettings\
   ├─ README.md
   └─ TODO.md
```

- Unity 编辑器：`6000.6.0f1`，以 `ProjectSettings/ProjectVersion.txt` 为准。
- 渲染：URP 2D。
- 工程已配置 Tilemap、2D Animation、Pixel Perfect、Input System 和 UGUI；具体版本见 `Packages/manifest.json`。
- 使用 Unity Hub 打开本目录，完成包解析和编译后打开 `Assets/Scenes/Main.unity`。该场景目前只有模板内容。
- 历史日志存在 `packages.unity.com` DNS 失败记录，开始开发前需确认当前包解析正常。

## 原版材料与已验证信息

以下为 2026-09-15 的文件、SWF 结构和 XML 检查结果，不代表已完成运行验证或完整反编译。

| 项目 | 检查结果 |
| --- | --- |
| 源文件 | 当前材料中没有发现 `.as` 或 `.fla`，需要从 SWF 反编译 |
| 主程序 | `mutiny.swf`，压缩 SWF，版本 8，AVM1 脚本；按 AS1/AS2 路线分析 |
| 舞台 | 550 × 400，25 FPS，主时间轴 140 帧 |
| 脚本线索 | 保留 `com.nitrome.throwgame` 等包名，以及 Controller、Character、TileSystem、Team、武器类等名称 |
| 资源结构 | 574 个 Flash Sprite 定义，其中 172 个有多帧；39 个声音定义；421 个导出链接 |
| 关卡 | `mutiny_levels/level_01.xml` 至 `level_18.xml`，共 18 个，均声明 `players="1"` |
| XML 扫描 | 107 种非空 tile 标记、29 种 object type、20 种 object 属性；前景与背景行数、展开宽度均匹配声明。此前记为 22 种属性，正式扫描与独立核对后修正为 20 |
| 重复程序 | `mutiny.swf`、`mutinyASp.swf`、`mutinyfpa.swf` 的 SHA256 相同 |

主 SWF 的 SHA256：

```text
c034d2cfa50272a1aac93ad75114b542ca5993dd7b70fa8901aa195de7820586
```

`skywire` 属于另一个游戏，不纳入本项目逆向范围。现有 18 个 XML 不能作为原版双人模式地图已经齐全的证据。

XML 使用 `tile:数量` 的行程编码，例如 `-:50` 表示 50 个空格。`row`、`bgRow` 分别存放前景和背景，`obj` 包含类型、坐标及武器等配置。标记的实际玩法语义需要结合原版脚本确认。

已发现两个需要处理的映射问题：

- `antichest` 没有同名 SWF 导出链接，需要确认是否为逻辑标记。
- XML 中的 `ship_anchor_4` 与 SWF 链接 `Ship_anchor_4` 大小写不同，需要显式映射。

## 实现路线

工作目录已建立：

```text
Assets/Mutiny/
├─ Data/
│  └─ Levels/           level_01.xml 至 level_18.xml 的原版副本
├─ Scripts/
│  ├─ Core/             核心数据与公共规则
│  ├─ Level/            XML 解析、关卡数据与加载
│  ├─ Simulation/       运动、碰撞、伤害、武器与回合模拟
│  └─ Presentation/     显示、输入、UI 与动画衔接
├─ Art/                 待导出的美术资源
└─ Audio/               待导出的音乐与音效
```

关卡副本与原文件逐一核对 SHA256，一致；原版材料保留在源目录。目录建立不代表对应模块已经实现。

1. 编写 Level XML parser，将关卡转换为与 Unity 场景对象分离的数据模型，并保存批量扫描结果。
2. 完整反编译 SWF，整理类树、时间轴脚本、游戏状态与外部依赖。
3. 导出资源，建立 `tile name → SWF symbol → Unity Sprite` 映射，记录资源 ID、枢轴和动画信息。
4. 在 Unity 中还原第 1 关静态地图，再实现角色、运动碰撞、爆炸、水域和回合闭环。
5. 依次还原 Cherry Bomb、Dynamite 和剩余武器，再补齐 AI、UI、动画与声音。
6. 对照原版逐项验证第 1 关行为，再扩展并回归全部关卡。

JPEXS 支持 AS1/AS2 反编译及图片、动画帧、声音等导出，可作为逆向工具；本 SWF 的实际导出质量仍需验证。参考 [JPEXS 官方功能说明](https://github.com/jindrapetrik/jpexs-decompiler/wiki/Features)。

## 还原原则

- 保留原版材料，提取与转换结果放在独立目录，记录来源和转换方式。
- 先查清规则和公式，再实现 C# 行为；避免仅凭类名推断玩法。
- 原版 25 FPS 作为逻辑时序调查基准。是否采用固定 25 Hz 模拟，需要确认原版运动、计时和碰撞实现；Unity 渲染帧率可独立设置。
- 不直接假设默认 Rigidbody2D 能还原原版弹道、反弹和击退；根据逆向结果选择自定义模拟或 Unity 物理。
- 分别记录地图装饰、碰撞形状和逻辑标记，不把每个非空 tile 都视为实体地形。
- 动画提取同时关注嵌套时间轴、帧标签和脚本事件，避免仅保存画面而丢失行为。
- 排行榜接口、域名检查和 Flash SharedObject 需要单独梳理，确定 Unity 中的对应处理。
- 原版代码、图像与声音的来源为 Nitrome 游戏材料；本仓库文档不表示已取得其使用或发布授权。

## 首个可玩版本的目标

第 1 关地图与角色可正确加载；完成角色操作、Cherry Bomb 投掷、碰撞、爆炸伤害、击退、水域处理、回合切换及胜负判断。通过原版对照确认核心规则后，再扩大内容范围。

## Level XML parser

`Assets/Mutiny/Scripts/Level/MutinyLevelXmlParser.cs` 不依赖 Unity API，可直接解析字符串：

```csharp
using Mutiny.Levels;

MutinyLevelData level = MutinyLevelXmlParser.Parse(xmlText, "level_01.xml");
string tile = level.Terrain[y, x];
string[] row = MutinyLevelXmlParser.DecodeRow("grass:2,-:3", 5);
```

数组保持 XML 的 `[y, x]`：x 从左到右、y 从上到下。非空 tile 名称不改写，空标记 `-` 解码为 `null`。`Properties` 保存全部对象属性，包括 `type/x/y` 的原始字符串；`Type/X/Y` 提供类型化读取入口。缺失的关卡名称使用空字符串。

Parser 校验根节点、正整数尺寸与玩家数、层行数、RLE 名称和正整数重复次数、展开宽度及对象坐标范围。数据错误抛出 `FormatException`；Parse 的可选 sourceName 与 XML 行列信息用于定位错误。已通过本地 .NET 10 / C# 9 编译、18 关解析及异常输入验证；用户已确认单关卡 Unity 验证组件运行通过。

## 批量扫描关卡

在 Unity 菜单选择 **Mutiny → Levels → Scan All XML**。工具读取 `Assets/Mutiny/Data/Levels/level_*.xml`，重新生成 [Docs/LevelScan](Docs/LevelScan/README.md) 下的报告，Console 输出解析数量与错误数量。报告位于 Assets 外，不作为游戏资源导入。

- `levels.csv`：各关元数据、对象数量、前景与背景种类数及 XML SHA256。
- `tile-types.csv`、`tile-occurrences.csv`：全部非空标记、出现次数与每次出现的关卡、层、原版坐标。
- `object-types.csv`、`objects.csv`：对象类型统计及各对象的索引和坐标。
- `attribute-values.csv`、`object-properties.csv`：全部原始属性取值、次数及所在对象。
- `errors.csv`：读取或解析失败的文件与诊断；一个文件失败后继续扫描其他文件。

扫描保留大小写和逻辑标记，不推断资源对应关系。空 tile 不计入清单，属性包括 `type/x/y`。每次覆盖报告，排序固定。已使用正式 parser 生成 18 关报告，并验证重复生成一致、错误后继续扫描及 CSV 转义；用户已确认 Unity 菜单运行通过。

## SWF 反编译产物

已完成脚本导出与自动去混淆，阅读入口见 [Docs/ReverseEngineering/Swf](Docs/ReverseEngineering/Swf/README.md)。默认 AS、去混淆 AS 与带 hex 的 pcode 各 494 份，逐路径匹配脚本索引；99 个包脚本恢复类声明。原始 SWF 保持不变。后续分析优先阅读 deobfuscated，重要公式与行为结合 pcode 核对；当前尚未完成规则语义验证或美术音频提取。

## 进度维护

原始美术资源与来源、动画、注册点清单见 [美术导出说明](Docs/ReverseEngineering/Art/README.md)。共保存 7417 PNG 和 6618 SVG；1963 个可绘制符号全部覆盖。资源暂留 Assets 外，下一阶段按 XML 标记筛选并配置 Unity Sprite；脚本驱动动画仍需另行重建。

完整 AS2 类树、方法与依赖索引，以及加载、逐帧更新、回合和武器流程见 [AS2 分析文档](Docs/ReverseEngineering/AS2/README.md)。当前确认 32 像素网格与爆炸动画帧事件；部分反编译循环、空库存行为及 33 关程序与本地 18 XML 的范围差异仍待核对。

### 在 Unity 中验证单个关卡

1. 在场景中创建空 GameObject，添加 `MutinyLevelTest` 组件。
2. 将 `Assets/Mutiny/Data/Levels/level_01.xml` 拖入组件的 `Level Xml` 字段。
3. 进入 Play Mode，在 Console 查看关卡尺寸、玩家数、对象数量、非空地形种类数及每个对象的原始属性。

第 1 关应输出 `50 x 17, players=1, objects=10`。未指定 XML 或解析失败时会输出错误并停止该次验证。此组件仅验证单个关卡并打印数据，不生成地图；场景需按上述步骤手动挂载。

[TODO.md](TODO.md) 是任务状态的统一入口。每次推进后更新任务状态、产物路径、验证结果与未解决问题。只有达到完成标准并保存必要产物后才勾选完成；初步检查不替代正式导出、实现或测试。
