# Mutiny SWF 反编译

导出日期：2026-09-15。原始 SWF 未修改，导出前后 SHA256 一致：

```text
c034d2cfa50272a1aac93ad75114b542ca5993dd7b70fa8901aa195de7820586
```

## 工具与复现

使用工作区 `Tools/ffdec-26.3.0/ffdec.jar` 和独立 Temurin Java 17 JRE（17.0.20.1+1）。未安装系统服务或更改系统环境变量。下载地址及压缩包 SHA256 见 [tool-downloads.json](tool-downloads.json)。

在 Unity 工程根目录运行：

```powershell
python Tools/ReverseEngineering/decompile_swf.py
```

脚本要求工作区父目录 Tools 下各有一份 FFDec 与 Java，并读取同级 `Mutiny Source/mutiny-flash-game/mutiny.swf`。重新运行会覆盖同名导出。更换 SWF 或工具版本时建议使用新的产物目录，以免旧文件残留。

实际命令、返回码见 [commands.json](commands.json)，工具和输入指纹见 [metadata.json](metadata.json)。其中路径以 Unity 工程父目录（工作区根目录）为基准记录为相对路径；旧命令中的 `Mutiny X/Docs/ReverseEngineering/Swf/` 是当时的输出位置，产物后来归档到本目录。重新导出请运行脚本。全部 9 项命令返回码为 0。FFDec CLI 导出格式与自动去混淆配置参考 [官方命令行说明](https://github.com/jindrapetrik/jpexs-decompiler/wiki/Commandline-arguments)。

## 产物

| 路径 | 用途 |
| --- | --- |
| source/scripts/ | 默认配置的原始 AS 导出，保留混淆和反编译异常作为对照 |
| deobfuscated/scripts/ | autoDeobfuscate=true 的 AS 导出，后续分析优先阅读 |
| pcode/scripts/ | 原版字节码反汇编及 hex，核对 AS 语义时使用 |
| mutiny.swf.xml | 完整 SWF 标签结构、资源数据与脚本字节；不是关卡 XML |
| symbols/ | 导出的符号链接表 |
| logs/ | 命令帮助、配置、脚本索引、标签清单和各项 stdout/stderr |
| script-files.csv | 全部三种脚本导出的路径、大小及 SHA256 |
| coverage.json | 按索引逐路径核对覆盖情况 |
| class-readability.csv | 99 个包脚本的类声明与继承提取，以及乱码标记检查 |
| diagnostic-candidates.csv | 源文件和日志中诊断关键词的候选位置，需要人工区分误报 |

## 覆盖与质量检查

- `dumpAS2 -exportNames` 索引共 494 项。
- source、deobfuscated、pcode 均导出 494 个文件，逐路径核对无遗漏或额外文件。
- 99 个包脚本均在去混淆后恢复 class 声明；这只是阅读准备，不代表已经完成类树与依赖分析。
- 唯一空 AS 为 `DefineSprite_135/frame_1/DoAction.as`，对应 pcode 仅 `; 00`（ActionEnd），并非缺失脚本。
- 默认导出中 73 个文件含 invalid_utf8 标记、2 个文件含替换字符；默认日志存在 TransitionTween 无效跳转和属性索引异常。
- 去混淆导出中未检出 invalid_utf8、Unicode 替换字符或双节号栈伪操作标记，stderr 为空。
- 去混淆后的类文件仍可能保留开头的控制变量赋值和转义名称；不应直接当作原始开发源码。
- 所有游戏规则的语义仍需结合 pcode、调用关系与原版行为核对。未执行 SWF 内的代码或调用其外部服务。

## 首批阅读入口

目录：`deobfuscated/scripts/__Packages/com/nitrome/throwgame/`。

- `Controller.as`：游戏初始化、层级创建、逐帧入口及回合衔接。
- `TileSystem.as`：关卡加载、XML 读取、地图网格和摄像机。
- `Map.as`、`Tile.as`：地图显示与单 tile 行为。
- `Solid.as`、`Character.as`：运动、碰撞及角色行为。
- `Team.as`：队伍、回合与 AI 线索。
- `Weapon.as`、`CherryBomb.as`、`Dynamite.as`、`Explosion.as`、`Water.as`：首个可玩闭环需要的武器和环境规则。

已确认的代码线索：Controller.startGame 将 enterFrame 挂到 holder.onEnterFrame；TileSystem.loadLevel 从 SWF 路径下 levels/ 加载 XML；TileSystem.getValidDropColumns 在扫描列时遇到背景 antichest 会排除该列，参与宝箱掉落位置筛选。具体全流程在下一步类树与规则分析中继续核对。

下一项：建立 AS2 class tree，整理继承、关键方法、调用依赖与时间轴入口。美术和音频尚未导出。
