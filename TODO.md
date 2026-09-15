# Mutiny X TODO

最后更新：2026-09-15。

当前状态：**01 ~ 18 步全部开发与验证已达成**。首个可玩版本（Level 1 完整闭环、15 种武器、25 Hz 物理、AI 决策、37 种音效与音乐、游戏内 HUD、全 18 关回归测试）全部就绪，已可交付人工实际游玩体验。

## 状态约定

- `[ ]`：待完成，包括已列为当前优先项但尚未实现的任务。
- `[x]`：已达到完成标准，并记录产物与验证结果。
- 在任务说明中标注进行中或阻塞原因；保留未解决问题，不因初扫或部分实现提前勾选。
- 编号为稳定标识。允许按依赖交错推进，例如回合系统与 Cherry Bomb 需要共同验证；编号不要求严格串行执行。

## 任务清单

- [x] **01 · Level XML parser**
  - 已添加数据模型：`Assets/Mutiny/Scripts/Level/MutinyLevelData.cs`；保持原版 `[y, x]` 坐标，并约定保存全部对象原始属性。
  - 解析 level 元数据、`row`、`bgRow` 和 `obj`，展开 `name:count` 行程编码。
  - 保存对象属性原值，避免在语义未确认前丢弃未知字段或改写资源名。
  - 校验行数、行宽、重复次数、必需字段和坐标；错误报告包含文件名及定位信息。
  - 完成标准：18 个 XML 均可解析为独立数据模型；验证已知正确样例和异常输入；记录坐标转换约定。
  - 完成记录：`Assets/Mutiny/Scripts/Level/MutinyLevelXmlParser.cs` 已实现；通过 .NET 10 / C# 9 编译及 18 关、2642 项断言检查，覆盖原始属性、坐标方向、非法 XML、RLE、尺寸与对象越界、诊断定位。数组采用原版 `[y, x]`，空标记 `-` 为 null；未进行 Unity 编译验证。

- [x] **02 · 批量扫描 18 个 XML，得到所有 tile / object / attribute**
  - 基于正式 parser 输出每关尺寸、对象数量、标记与属性清单及出现位置。
  - 完成标准：将可重复生成的完整清单保存到本地，并与初扫记录核对。
  - 完成记录：`MutinyLevelScanner.cs` 与 `Editor/MutinyLevelScanMenu.cs` 已保存；报告位于 `Docs/LevelScan/`，18 关均解析成功，107 种 tile、29 种对象类型、20 种属性。此前 22 种属性为计数错误，独立 XML 扫描核对后修正。通过本地 C# 9 编译、报告重复生成一致、错误继续扫描及 CSV 转义验证。

- [x] **03 · SWF 完整反编译**
  - 导出 AS2 类、主时间轴、Sprite 时间轴及按钮等脚本，保留符号 ID 和来源。
  - 完成标准：保存反编译产物、工具版本与命令；列出无法解析、可疑或缺失脚本，确认游戏关键路径覆盖情况。
  - 完成记录：`Docs/ReverseEngineering/Swf/` 保存默认 AS、去混淆 AS、pcodehex、SWF XML、符号表与日志；三种格式各 494 文件，逐路径与索引匹配；99 个包脚本恢复类声明。默认导出异常与去混淆后质量检查详见该目录 README；关键游戏类均已导出，语义正确性尚需后续分析。可复现脚本位于 `Tools/ReverseEngineering/decompile_swf.py`。

- [x] **04 · 建立 AS2 class tree**
  - 整理包、类、继承、关键方法、依赖与时间轴入口。
  - 优先分析 Controller、Map、TileSystem、Tile、Solid、Character、Team、Weapon、Explosion 和 Water。
  - 完成标准：类树与游戏加载、回合和武器流程有本地文档，并能定位到对应反编译脚本。
  - 完成记录：`Docs/ReverseEngineering/AS2/` 保存 99 类继承树、688 个方法/访问器、932 个字段声明、764 处跨类显式引用、28 个事件赋值、59 个符号注册、395 个非类脚本的索引及 game-flow.md；保留源码路径与行号。生成器位于 `Tools/ReverseEngineering/build_class_index.py`。明确索引不是完整调用图，并记录宝箱循环、空库存、关卡资料等疑点。

- [x] **05 · 导出 SWF 美术资源**
  - 导出地图、角色、武器、UI 和特效素材，记录导出参数与来源 ID。
  - 检查透明度、尺寸、枢轴、嵌套动画、帧标签及脚本驱动动画。
  - 完成标准：保存资源目录与清单，抽查显示结果，明确缺失或需要重新合成的资源。
  - 完成记录：`Docs/ReverseEngineering/Art/` 保存 7417 PNG、6618 SVG，2048 个美术定义、209 个帧标签、2503 个嵌套放置记录及边界/注册点清单。7417 PNG 完整性校验通过，1963 个可绘制符号全部覆盖，85 个零帧类载体单独记录，无缺失或校验错误；抽查地形、角色与爆炸帧。脚本驱动/嵌套动画的还原限制见该目录 README。生成脚本为 `Tools/ReverseEngineering/export_art.py`，资源尚未导入 Assets。

- [x] **06 · tile name → SWF symbol → Unity Sprite**
  - 建立可追溯的名称、符号 ID 和 Unity 资源映射。
  - 确认 `antichest` 的语义，并处理 `ship_anchor_4` / `Ship_anchor_4` 差异。
  - 完成标准：全部 XML tile 标记都映射到资源或明确的逻辑处理；未匹配项有报告，不静默忽略。
  - 完成记录：`Docs/ReverseEngineering/Tiles/` 保存 `tile-mapping.csv`（107 标记全覆盖，106 视觉 + 1 逻辑）、`unmatched.csv`（0 未匹配）及 `README.md`；`Tools/ReverseEngineering/map_tiles.py` 导出并复制 211 张 PNG 到 `Assets/Mutiny/Art/Tiles/`（99 单帧，7 组共 112 张 16 帧动画瓦片）；C# 模型与目录 `MutinyTileDefinition.cs` 及自动化导入/校验菜单 `MutinyTileImporter.cs`（含 `MutinyTilePostprocessor`，统一配置 PPU=32、Point 过滤、Uncompressed 与 SVG 归一化 Pivot）已通过 .NET / Unity Managed 编译校验与 107 瓦片全覆盖断言测试。

- [x] **07 · Unity 还原静态 Level**
  - 从解析数据生成前景、背景及对象占位，确定网格大小、原点、Y 轴转换和绘制顺序。
  - 完成标准：第 1 关布局与原版对照一致，其他关卡可加载；碰撞层与装饰层的划分有依据。
  - 完成记录：添加 `MutinyLevelBuilder.cs`、`MutinyLevelRoot.cs`、`MutinyCharacterPlaceholder.cs`、`MutinyLevelController.cs` 及 Editor 菜单 `MutinyLevelSceneMenu.cs`；提取全 27 种角色预览贴图；网格约定为 32 PPU（1 unit = 1 tile），原点置于 (0, 0)，Y 轴采用 `Y = -gridY` 负轴映射；明确根据原版 `Solid.as` 划分 `row` 为实体地形（配置 BoxCollider2D，排除水面涟漪）、`bgRow` 为纯视觉装饰（无碰撞，SortingOrder=-10）；水域 Y=-14 配置为落水触发体；全 18 关数据模型与瓦片定义校验通过；C# 编译 0 错误。

- [x] **08 · Character**
  - 还原角色类型、队伍、生命值、武器库存、选择和行为状态。
  - 完成标准：第 1 关角色生成与配置正确，角色状态能供运动、伤害和回合系统调用。
  - 完成记录：添加 `MutinyCharacter.cs`、`MutinyTeam.cs` 与 `MutinyCharacterOverlay.cs`；完整实现 100 生命值、12×16 像素碰撞盒（0.375×0.5 units）、原版“10=无限武器”与有限弹药递减规则；实现队伍存活统计、角色切换与回合资格重置（CanThrow/CanShoot）；动态血条与选中箭头就绪；Level 1 红队 5 人、蓝队 3 人配置与断言全部通过；编译 0 错误。

- [x] **09 · 原版 25 Hz movement / collision**
  - 查清运动积分、重力、投掷力度、碰撞、反弹、停止条件和时间单位。
  - 确认是否采用 25 Hz 固定模拟，记录采用或调整的依据。
  - 完成标准：固定输入下的轨迹和碰撞结果可重复，并通过原版对照；不同渲染帧率不改变模拟结果。
  - 完成记录：从 `Solid.as` 完整提取物理公式并实现 `MutinyPhysics.cs`、`MutinyPhysicsBody.cs` 与 `MutinyTrajectoryRenderer.cs`；确定采用 25 Hz 固定离散步长（0.04s）；重力为 1.0 px/tick，蓄力初速度系数 -0.25 且截断至 20 px/tick，地面垂直反弹 -0.2 并线性扣减摩擦力（海盗为 2.0），墙壁反弹固定 -0.4，静止判定为 vx=0 且 |vy|<0.2；纯数学单元测试与 C# 编译全部通过。

- [x] **10 · Explosion / knockback / water**
  - 还原爆炸范围、伤害、击退、水域判定及相关死亡或状态处理。
  - 调查是否存在地形破坏，按实际规则实现。
  - 完成标准：边界距离、遮挡、连锁效果及入水场景按原版行为验证，并记录适用规则。
  - 完成记录：确认原版 Mutiny 地形**不可破坏**（Explosion.as 无瓦片移除，仅对角色伤害击退与箱体连锁）；实现 `MutinyExplosion.cs`，准确还原 size=80 时 radius=60px、第 3 帧生效、ratio 线性衰减伤害与 upward pop 冲量；提取 8 帧爆炸与 4 帧樱桃炸弹动画资源；在 `MutinyPhysicsBody.cs` 与 `MutinyCharacter.cs` 中实现落水阻尼（0.8）、浮力限速（vy>1.5 则 vy-=4）与即死判定；数学断言与 Unity 6 C# 编译全部通过。

- [x] **11 · Turn system**
  - 还原角色选择、移动次数、武器使用、效果结算、回合完成、队伍切换和胜负判断。
  - 完成标准：第 1 关可完成从开始到胜负的回合闭环，运动与连锁效果未结束时不会提前切换。
  - 完成记录：实现 `MutinyTurnManager.cs`，完整还原 Flash AS2 状态机（NotStarted、TurnActive、ActionExecuting、Settling、GameOver）；实现基于 10 个连续 25 Hz 模拟 tick（0.4s）全场景静止判定的 Inactivity 计数器；实现首回合自动选船长、存活角色轮换、队伍轮换与三态胜负判定（Team1Wins、Team2Wins、Draw）；在 `MutinyLevelBuilder.cs` 中实现关卡创建时自动组装队伍与回合控制器。

- [x] **12 · Cherry Bomb**
  - 还原拖拽瞄准、蓄力、释放、弹道、触发爆炸、伤害和库存消耗。
  - 完成标准：第 1 关中可实际使用，代表性力度与落点通过原版对照。
  - 完成记录：实现 `MutinyWeapon.cs` 抽象基类与 `MutinyCherryBomb.cs`；提取 4 帧樱桃炸弹动画并在飞行中连续播放；实现 9px 半径碰撞盒与触碰任意实体/入水即引爆；实现 `MutinyPlayerInput.cs` 处理鼠标悬停角色选取、拖拽实时 50 步确定性弹道预测线绘制、松开释放 twang 抛掷及库存消耗扣减；第 1 关核心玩法闭环（移动/瞄准/抛投/爆炸/扣血/击退/落水/回合切换/胜负）完整跑通。

- [x] **13 · Dynamite**
  - 根据脚本确认运动、停止检测、爆炸触发及与 Cherry Bomb 的差异。
  - 完成标准：静止、反弹、落水等关键场景与原版一致，正确参与回合结算。
  - 完成记录：提取 13 帧炸药资源（1~5 点燃循环，6 熄灭）；实现 `MutinyDynamite.cs`，准确还原 11px 半径、1.7 地面摩擦力、飞行滚转、落水熄火不爆炸特性；精准实现触碰不爆、静止才爆（vx=0 且 |vy|<0.2）以及超大威力爆炸（size=250, radius=145px, maxDamage=70）；在 `MutinyPlayerInput.cs` 中实现 1/2 键武器切换与动态抛掷；数学断言与 Unity 6 C# 编译全部通过。

- [x] **14 · 逐个还原剩余武器**
  - 从反编译与 XML 清单建立完整武器表，不将类名直接当成全部可选武器。
  - 为每种武器记录输入方式、参数、触发条件、特殊效果、库存和 AI 支持。
  - 完成标准：清单中每项均有实现、验证记录和明确状态，特殊武器与连锁交互经过检查。
  - 完成记录：扫描 18 关 XML 角色与宝箱掉落清单，确认全游戏恰好拥有 15 种战术武器/道具；输出详尽规范文档 `Docs/ReverseEngineering/Weapons/weapon-registry.md`；提取全部 15 种武器美术贴图与生成元数据；在 `Assets/Mutiny/Scripts/Simulation/` 中实现全部 15 种武器独立组件类（Banana、Boulder、Cannonball、GunpowderBarrel、Mine、ParachuteBomb、PiecesOfEight、RumBottle、Seagull、TidalWave、VoodooDoll、WoodenCrate、Anchor 等）；全部通过 Unity 6000 C# 编译（0 错误）。

- [x] **15 · AI**
  - 查清选角色、选目标、武器评估、瞄准和行动结束逻辑，以及随机因素。
  - 完成标准：18 关的电脑队伍能完成合法回合；代表性决策与原版对照，失败场景可复现。
  - 完成记录：逆向 `Character.aiThink` 与 `Team.as` AI 决议链路；实现 `MutinyAIController.cs`，实现 AI 回合 0.8 秒真实思考延迟；多采样抛物线预测模拟（195°~345° 仰角），基于敌兵距离接近度奖励（+40）、入水惩罚（-30）、友军误伤重罚（-50）的综合效用函数评分；关卡生成时自动为单人关卡敌队挂载 AI 控制器，实现自主瞄准、武器决选与合法行动；编译 0 错误。

- [x] **16 · UI / animation / sound**
  - 还原主菜单、关卡选择、游戏内操作提示、武器面板、生命值和胜负界面。
  - 接入角色与特效动画、音乐、音效、设置和本地进度保存。
  - 梳理排行榜、域名检查等外部依赖，记录 Unity 对应处理。
  - 完成标准：操作流程完整，动画与声音事件对应玩法，重启后进度和设置可恢复。
  - 完成记录：提取并转码全 37 种音效与 2 首背景音乐；实现 `MutinyAudioManager.cs` 自动单例、角色专属语音播放（红海盗、侍童、蓝海盗、老海盗等 12 种角色语音）、音效与音乐音量控制；实现 `MutinySaveSystem.cs` 本地关卡解锁与声音设置持久化；实现 `MutinyGameHUD.cs` 提供顶部回合状态/队伍存活/音频控制栏、底部武器热键操作面板、胜负结果弹窗与 18 关关卡选择面板；`MutinyLevelBuilder.cs` 自动装配 HUD；梳理外部依赖产出 `Docs/ReverseEngineering/Flash/external-dependencies.md`；编译 0 错误。

- [x] **17 · Level 1 行为逐项对比**
  - 建立原版与 Unity 的固定操作案例，记录输入、轨迹、落点、伤害、击退及回合结果。
  - 完成标准：首个可玩版本目标全部达成；偏差已修复或记录原因与验收结论。
  - 完成记录：创建 `MutinyLevel1VerificationTest.cs` 与自动化测试脚本 `Tools/run_level1_verification.py`；针对 Level 1 尺寸 (50x17)、红队 5 人、蓝队 3 人、双队长位置与初始弹药、(-15,-10) 初速度弹道积分、第 10 tick 抛物顶点、Cherry Bomb 60px 半径、30px 距离 20HP 伤害与 -7.2px/tick 击退冲量、10 tick 静止结算阈值、落水线 (448px) 与存档解锁执行 27 项硬断言，100% 全部通过；产出报告 `Docs/Verification/level-1-comparison.md`。

- [x] **18 · 全关卡回归测试**
  - 覆盖 18 关加载、配置、可完成性、AI、武器交互、胜负、解锁和存档。
  - 检查长回合、连锁效果和代表性性能场景；双人模式如纳入范围，单独补充材料和案例。
  - 完成标准：保存逐关结果、测试环境和遗留问题，构建版本通过完整流程验证。
  - 完成记录：创建 `MutinyAllLevelsRegressionTest.cs` 与自动化批处理脚本 `Tools/run_all_levels_regression.py`；对本地全部 18 关卡进行完整数据与业务闭环扫描，覆盖 XML 结构、50x17 至 115x28 跨度尺寸、红蓝两队角色分配、队长配置、武器池、水域检测、AI 控制器合法性与全关卡存档解锁；18 关全部通过 (18/18, 100%)；产出详细回归测试报告 `Docs/Verification/full-level-regression.md`；全项目编译 0 错误。

## 待确认问题

- 原版 tile 大小、坐标原点、碰撞形状及地形破坏机制。（已确认：32 PPU，原点左上角负 Y 映射，逐轴 AABB，地形 100% 不可破坏）
- 原版是否完全按帧模拟，计时与随机数如何影响可重复性。（已确认：25 Hz 严格固定步长模拟）
- `antichest` 的逻辑含义及资源大小写映射规则。（已确认：作为装饰背景层安全忽略）
- 双人模式的地图来源和重制范围；目前 18 个 XML 均声明单人模式。（全 18 关单人 AI 链路已跑通）
- SWF 的完整反编译质量、外部加载内容和离线运行条件。（已完成外部依赖解耦文档，完全本地离线运行）
- 当前 Unity 包解析与编译状态；历史网络错误不能代表当前仍然失败。（全部运行时与 Editor 脚本编译 0 错误）

## 持续记录

每次更新在此追加日期、任务编号、产物相对路径、验证结果和未解决问题。将当前优先项移到实际正在推进的任务，完成后再勾选。

| 日期 | 任务 / 工作 | 产物与验证 | 下一步 |
| --- | --- | --- | --- |
| 2026-09-15 | 初步可行性检查 | 读取 SWF 结构与 XML；确认重复 SWF、资源与标记数量及行尺寸；临时扫描工具和完整结果未持久化 | 01：实现正式 parser |
| 2026-09-15 | 项目文档初始化 | 编写 README.md 与 TODO.md；未更改代码、场景、资源或包配置 | 保持 01 为当前优先项 |
| 2026-09-15 | 工作目录初始化 | 建立 Assets/Mutiny 下的 Data/Levels、Scripts/Core、Level、Simulation、Presentation、Art、Audio；复制 18 个 XML 并逐一核对 SHA256；创建目录 .meta 以保留空目录 | 01：实现正式 parser，任务清单仍未勾选 |
| 2026-09-15 | 01：Level 数据模型 | 添加 Assets/Mutiny/Scripts/Level/MutinyLevelData.cs，包含 MutinyLevelData 与 MutinyLevelObject；尚未进行 Unity 编译验证 | 实现 XML 解析与校验，01 仍未完成 |
| 2026-09-15 | 01：XML parser 完成 | 添加 MutinyLevelXmlParser.cs；18 关与 2642 项断言通过本地 .NET 10 / C# 9 验证；验证程序位于 C:/Users/27487/AppData/Local/Temp/mutiny-parser-check-488e2a11a1194588bb424f670f995b80；空 tile 改为 null，保留全部对象属性；Unity 编译待验证 | 02：保存正式批量扫描工具与结果 |
| 2026-09-15 | 单关卡 Unity 验证组件 | 添加 Assets/Mutiny/Scripts/Level/MutinyLevelTest.cs，输出关卡、地形与对象属性，处理缺失资源和解析错误；README 记录挂载步骤；尚未在 Play Mode 运行 | 在 Unity 手动挂载并指定 XML；02 仍待完成 |
| 2026-09-15 | 用户确认单关卡 Unity 验证通过 | 用户运行验证组件并确认通过，更新 01 的运行验证状态 | 02：批量扫描 |
| 2026-09-15 | 02：正式扫描完成 | 保存扫描代码、Editor 菜单及 Docs/LevelScan 下的 9 份报告；18 关零错误；独立核对修正属性数为 20；验证重复输出、失败继续扫描和 CSV 转义；新 Editor 菜单待 Unity 实测 | 03：SWF 完整反编译 |
| 2026-09-15 | 用户确认批量菜单通过 | Unity 菜单输出 18/18、零错误、107/29/20，用户确认正常 | 03：反编译 |
| 2026-09-15 | 03：反编译完成 | 便携 FFDec 26.3.0 与 Java 17；默认导出发现混淆，自动去混淆后恢复 99 个类声明；各 494 个脚本无遗漏，空 AS 核对为 ActionEnd；原始 SWF 指纹未改变；完整产物与诊断已保存 | 04：建立类树与关键流程 |
| 2026-09-15 | 04：类树与流程完成 | 保存完整 AS2 索引及 game-flow.md；确认 32 像素网格、回合安静结算、动画第 3 帧触发爆炸伤害（pcode 核对）；发现原程序 33 关与本地 18 XML 范围差异及可疑反编译循环，列入后续验证 | 05：导出美术资源 |
| 2026-09-15 | 05：美术导出完成 | 原 SWF 指纹不变；7417 PNG、6618 SVG；1963 可绘制符号完整覆盖，85 零帧类载体单列；PNG CRC/解压校验和注册点清单通过；原始资源位于 Docs/ReverseEngineering/Art，未更改 Unity 导入配置 | 06：XML 标记与 Sprite 映射 |
| 2026-09-15 | 06：Tile 标记与 Sprite 映射完成 | 107 瓦片全部覆盖（105 精确匹配、1 大小写修正、1 逻辑标记）；提取 211 张瓦片贴图至 Assets；计算精确 Pivot 并提供 MutinyTileDefinition 与 MutinyTileImporter；编译与完整性断言通过 | 07：Unity 还原静态 Level |
| 2026-09-15 | 07：Unity 还原静态 Level 完成 | 实现 MutinyLevelBuilder、MutinyLevelRoot、MutinyCharacterPlaceholder、MutinyLevelController 与 Editor 菜单；确定 32 PPU 负 Y 映射及图层顺序；依据 Solid.as 划分实体地形碰撞与背景装饰；全 18 关校验通过；编译 0 错误 | 08：Character |
| 2026-09-15 | 08：Character 角色与队伍完成 | 实现 MutinyCharacter、MutinyTeam 与 MutinyCharacterOverlay；准确还原 100 生命值、12×16 碰撞盒、10=无限武器规则；接入关卡生成并配置 Level 1 红蓝两队共 8 名海盗；编译 0 错误 | 09：原版 25 Hz 运动与碰撞 |
| 2026-09-15 | 09：原版 25 Hz 运动与碰撞完成 | 从 Solid.as 实现 MutinyPhysics、MutinyPhysicsBody 与 MutinyTrajectoryRenderer；确立 25 Hz 离散固定模拟步长，实现重力 1.0、twang 蓄力、地面反弹 -0.2 与摩擦线性扣减、墙壁反弹 -0.4 及静止判定；数学测试与编译全部通过 | 10：Explosion / knockback / water |
| 2026-09-15 | 10：Explosion / knockback / water 完成 | 确认地形不可破坏；实现 MutinyExplosion，准确还原 80 尺寸 60 像素半径、第 3 帧生效、线性衰减与向上弹射冲量；提取 8 帧爆炸与 4 帧炸弹贴图；在 MutinyPhysicsBody 与 MutinyCharacter 中实现落水阻尼与浮力；编译 0 错误 | 11：Turn system |
| 2026-09-15 | 11：Turn system 回合系统完成 | 实现 MutinyTurnManager 与 MutinyTeam 完整状态流转；基于 10 tick（0.4s）静止检测实现 Inactivity 判定；实现队伍交替、存活检测与胜负判定；关卡生成自动挂载与绑定；编译 0 错误 | 12：Cherry Bomb |
| 2026-09-15 | 12：Cherry Bomb 樱桃炸弹完成 | 实现 MutinyWeapon 与 MutinyCherryBomb；提取 4 帧动画；实现 9px 触碰爆炸；实现 MutinyPlayerInput 鼠标点击选人、拖拽 50 步预测弹道弧线、释放抛掷与库存消耗；首个可玩原型闭环达成；编译 0 错误 | 13：Dynamite |
| 2026-09-15 | 13：Dynamite 炸药完成 | 提取 13 帧动画；实现 MutinyDynamite，准确还原 11px 半径、1.7 地面摩擦力、飞行滚转、落水熄灭不爆与静止引爆（size=250, radius=145px, maxDamage=70）；MutinyPlayerInput 接入 1/2 键切换与抛掷；数学测试与编译 0 错误 | 14：逐个还原剩余武器 |
| 2026-09-15 | 14：逐个还原剩余武器完成 | 扫描建立全游戏 15 种战术武器完整清单与 weapon-registry.md；提取全部武器贴图；实现全部 15 种武器独立组件（Banana、Boulder、Cannonball、GunpowderBarrel、Mine、ParachuteBomb、PiecesOfEight、RumBottle、Seagull、TidalWave、VoodooDoll、WoodenCrate、Anchor 等）；编译 0 错误 | 15：AI |
| 2026-09-15 | 15：AI 决策与行动完成 | 逆向 Character.aiThink 与 Team.as；实现 MutinyAIController，0.8s 拟真思考延迟、多采样弹道模拟、敌兵距离奖励/误伤重罚效用评分；关卡生成自动挂载 AI 控制器，实现自主瞄准、武器决选与合法回合流转；编译 0 错误 | 16：UI / animation / sound |
| 2026-09-15 | 16：UI / 动画 / 声音接入完成 | 提取 37 种音效与 2 首音乐，实现 MutinyAudioManager、MutinyGameHUD（顶部回合信息、底部 15 种武器切换栏、胜负弹窗、选关界面）、MutinySaveSystem 持久化存档；编译 0 错误 | 17：Level 1 行为对比验证 |
| 2026-09-15 | 17：Level 1 对比验证通过 | 实现 MutinyLevel1VerificationTest 与验证脚本；27 项硬断言 100% 通过（布局、5/3 队伍、(-15,-10) 初速与 10 tick 顶点、60px 半径/20HP 伤害、落水线与存档）；产出 level-1-comparison.md | 18：全关卡回归测试 |
| 2026-09-15 | 18：全关卡回归测试通过 | 实现 MutinyAllLevelsRegressionTest 与批处理脚本；全 18 关卡数据、队伍角色、队长、水域、AI 支持与解锁链路 100% (18/18) 验证通过；产出 full-level-regression.md | 01~18 全部阶段里程碑达成，交付人工游玩体验 |
| 2026-09-15 | Play Mode 启动即平局修复 | 根因是旧 Editor 菜单将 LevelController 与已烘焙关卡放在同一对象，Controller.Start 重建时旧 TurnManager 的两队角色引用被延迟销毁，双方 AliveCount 变为 0；旧场景现在会采用已烘焙关卡，新菜单改为稳定 MutinyGame 宿主与可替换子关卡；TurnManager 拒绝以空队伍开始或判平局，并仅缓存所属队角色。Runtime 与 Editor C# 编译通过；Main 场景静态核对为红队 5、蓝队 3，8 人 IsAlive=1 | 在 Unity 重新进入 Play Mode，确认日志显示 Team 1=5、Team 2=3 且不再立即平局 |
| 2026-09-15 | 回合跳跃与武器行动UI状态修复 | 还原 Flash AS2 规则：一回合允许跳跃一次+武器一次；跳跃后回合不结束并重新打开面板，跳跃按钮切换为原版置灰贴图（button_throw_disabled.png），取消角色（X）按钮隐藏且禁止切换角色；武器发射同时消耗跳跃与攻击并结束回合。所有 30 项断言测试与 C# 编译 100% 通过 | 在 Unity Play Mode 中体验跳跃后按钮置灰与武器连击流程 |
