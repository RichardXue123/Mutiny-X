# Mutiny X

Mutiny X 是使用 Unity 6 复刻 Nitrome Flash 游戏《Mutiny》的项目。玩家在二维海盗战场上轮流选择角色、调整站位并使用武器，目标是让对方队伍全员出局。项目以原版 SWF、ActionScript、关卡 XML 和视听资源为依据，重建可观察的游戏规则与画面表现。

## 原游戏

《Mutiny》是一款回合制对战游戏。每回合可以选择本队角色，将角色抛向新位置，或使用武器攻击。地形高低差、爆炸击退和水面都会影响结果；角色落水也会出局。战场中还会出现补充武器的空投宝箱。原版提供单人对 AI 和同一设备轮流操作的双人模式。

原版使用 550 × 400 的 Flash 舞台，以 25 FPS 运行。单人选关覆盖第 1–15 关，双人选关覆盖第 16–33 关。

## 复刻内容

- **关卡与世界**：解析原版 XML，构建 33 个编号关卡的地形、碰撞、角色、物件和水域。
- **回合与战斗**：实现选人、抛掷移动、武器操作、行动结算、伤害、击退、落水和胜负判定。模拟使用与原版时序对应的固定步长。
- **武器与事件**：实现樱桃炸弹、炸药、香蕉、大炮、船锚、海啸等 15 种武器，以及空投宝箱与拾取。
- **对手 AI**：为角色移动和武器生成候选，预测落点并比较收益，再通过正式行动入口执行；复杂武器有对应的专用决策路径。
- **界面与反馈**：重建主菜单、单人和双人选关、战斗 HUD、结果弹窗、角色动画、镜头、音效和音乐。双人模式保留双方胜局计数；单人进度与双人对战分开。
- **输入**：桌面端使用鼠标操作；工程也包含 Android 触摸输入适配。Android 真机表现仍需进一步验证。

主要流程已经可以在 Unity 中运行。部分原版细节、复杂武器和 AI 场景仍在逐项对照，现有实现不代表全部关卡已完成原版一致性验收。规则、证据和实际验证状态见 [Docs](Docs/README.md)。

## 逆向工程

项目保留了原版 `mutiny.swf` 和关卡 XML，并将逆向产物与 Unity 代码分开管理。使用 FFDec 导出 ActionScript 2、pcode、SWF 时间轴及图像和音频资源；遇到反编译歧义时结合字节码、资源与运行画面核对。Unity 侧将原版关卡数据解析为独立模型，再由 C# 系统构建世界、推进回合和绘制表现。

关卡、按钮和动画资源均按原版来源建立映射。第 16–33 关的原始 XML 及 SHA-256 记录在 [双人关卡证据](Docs/10-OriginalEvidence/Artifacts/TwoPlayerLevels/README.md)。完整的脚本、时间轴和资源索引见 [原版证据](Docs/10-OriginalEvidence/README.md)；各模块的行为规格和回归结果见 [项目文档](Docs/README.md)。

## 使用 Unity 运行

1. 克隆仓库：`git clone <仓库地址>`。安装 **Unity 6000.6.0f1**，用 Unity Hub 打开克隆后包含 `Assets/`、`Packages/` 和 `ProjectSettings/` 的目录。具体编辑器版本见 [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt)。
2. 等待 Unity 导入资源、解析包并完成脚本编译。工程使用 URP 2D；依赖由 [Packages/manifest.json](Packages/manifest.json) 管理。
3. 打开 [Assets/Scenes/Main.unity](Assets/Scenes/Main.unity)，点击编辑器的 **Play**。从标题画面进入单人或双人模式，再选择关卡。`Main` 已列入 Build Settings，无需手动挂载关卡 XML。

桌面端用鼠标选择当前队伍角色，并通过画面中的行动与武器界面操作；抛掷时按住拖动以调整方向和力度，松开提交。游戏内的 **GM** 按钮提供调试命令，说明见 [GM 命令文档](Docs/11-VerificationAndDebug/GM_COMMANDS.md)。

## 构建与发布

固定版本 tag 后，可使用 [本地一键发布流程](BuildTools/Release/README.md) 自动构建 Windows 便携包、安装包与 Android APK，校验后上传并发布 GitHub Release。支持 `-BuildOnly` 本地验证和 `-Resume` 中断恢复。

## 目录

| 路径 | 内容 |
| --- | --- |
| `Assets/Mutiny/Scripts/Level/` | XML 解析、关卡构建与加载 |
| `Assets/Mutiny/Scripts/Simulation/` | 回合、角色、物理、武器与 AI |
| `Assets/Mutiny/Scripts/Presentation/` | 前端、输入、HUD、镜头、动画与音频 |
| `Assets/Mutiny/Scripts/Persistence/` | 单人进度保存 |
| `Assets/Mutiny/Scripts/Verification/` | 生产入口回归与编辑器验证 |
| `Assets/Mutiny/Resources/` | 运行时关卡和视听资源 |
| `Mutiny Source/` | 原版 SWF 与早期关卡材料 |
| `Docs/` | 原版证据、行为规格、验收记录与持续计划 |

原游戏及其原始美术、音频版权归 Nitrome 等相应权利人所有。本项目用于研究与复刻实践，未取得原版内容的商业发布授权。
