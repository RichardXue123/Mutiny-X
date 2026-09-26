# 本地化初版实施记录（feature/Localization）

基线：从 `main` 的 `561161691f8d78c631a0b791527386a4edd78976` 创建独立分支。原设计和 LOC-01～08 行为规格见 [LOCALIZATION_PLAN.md](LOCALIZATION_PLAN.md)。英语是原版对照；简体中文是授权扩展。菜单中的 `MUTINY` 标志、人名、网址暂保留原样。

## 架构与内容

- Unity `6000.6.0f1` 使用 `com.unity.localization` `1.5.13`。`Mutiny.tsv` 是人工维护的键、英语、简中三列源文件；编辑器菜单 **Mutiny/Localization/Rebuild String Tables** 生成两个 Locale、String Table Collection、运行时 Localization Settings 和 Addressables 组。改完 TSV 必须重建表并提交生成资产。当前共有 82 个成对键。运行时按保存语言、设备语言、英语的顺序选择；本版设备自动识别简体中文，其他设备语言用英语。
- `MutinyLocalization` 缓存两个表，切换时保存偏好并发送变更事件；单条缺译回退到英语，初始表未就绪时使用调用点英语，缺键只警告一次。`MutinySaveSystem` 的语言键独立于进度。未来增加 `ja`、`zh-Hant` 时，需在语言目录、GM 参数映射中增加代码和相应表，不改武器、关卡等业务 ID。
- `MutinyLocalizedText` 对英语继续使用 PirateFont/DangleFont 原图集；简中使用随包 Noto Sans CJK SC Regular OTF 动态字形。字体随资源打包，运行时按实际文本绘制，不预烘焙全部汉字。`OFL.txt` 随字体保存。今后日语、繁体中文应分别配置 JP、TC 地区字体，不能把 SC 当成通用汉字字形。字形检查记录具体码点；本版还未实现专门的画面溢出检测。
- 已接入前端主要导航/说明/成绩/制作页、载入文字、战斗武器名和说明、回合/拾取提示、退出和结算弹窗。按用户更新要求，手动切换语言仅保留 GM 的 `setlanguage cn` / `setlanguage en` 入口；标题页不显示语言按钮。武器内部 ID 不翻译。事件队列保留待解析文案，切换语言后未显示及当前显示的提示会按新语言解析，且不重新开始其动画计时。
- 船长对话、部分烘焙于贴图的英语提示、若干帮助图内字和 GM 工具仍显示英语；品牌标志保持英语。上述是初版已知内容范围，不能视为简中完整覆盖。

## 来源与可观察行为

| 规格 ID | 原版/扩展来源 | Unity 生产入口 | 初版状态 | 验收方法与结果 |
| --- | --- | --- | --- | --- |
| LOC-01 保存及首启语言 | 用户授权；原存档字段见 `MutinySaveSystem` | `MutinyFrontendController.Initialize` → `MutinyLocalization.Initialize` | 已实现 | 编辑器静态检查通过；设备语言与重启的运行验收待做 |
| LOC-02 当前页切换 | 用户授权；原版菜单时间轴 | `MutinyGMManager.ExecuteCommand` → `MutinyLocalization.Select` | 已实现 GM 切换和主要导航 | GM-09 生产入口 7/7 通过；各页画面验收待做 |
| LOC-03 英语外观 | 原版 `PirateFont`、`DangleFont` 图集和 SWF 帧脚本 | `MutinyLocalizedText` → `MutinyBitmapFont` | 已保留绘制路径 | 截图逐态对比待做；标题页语言按钮按新要求移除 |
| LOC-UI-01 仅 GM 手动切换入口 | 用户本轮要求，原版来源不适用 | `MutinyFrontendController.DrawTitle`、`MutinyGMManager.ExecuteCommand` | 已删除标题页按钮及专用绘制方法 | 静态检查确认没有语言按钮调用和命中代码，生产手动切换仅剩 GM；本轮没有运行画面验收。GM-09 的 7/7 为此前结果，本轮未重跑 |
| LOC-04 武器、回合、拾取 | SWF `WeaponSelectButton.as::hoverText`、`Team.as::startTurn`、`TreasureChest.as`；简中译文为授权扩展 | `MutinyGameHUD.GetLocalizedActionCopy`、`MutinyIngameTextArea` | 15 种武器及事件已接入 | 编辑器校验 15 个英文武器条目与 SWF 一致，82 组译文/字形通过；实战悬停、拾取、结束回合待运行 |
| LOC-05 船长对话 | SWF `Team.as::lines` | `MutinySpeechController`、`MutinyGameHUD` | 本版未翻译；气泡支持 Unicode 绘制 | 台词及逐字过程仍待实施、待运行 |
| LOC-06 结算等页面 | 原版页面时间轴/资源；简中译文为授权扩展 | `MutinyFrontendController`、`MutinyGameHUD` | 主要活文本已接入 | 全页面与带字贴图的实机审计待做 |
| LOC-07 缺译、缺字 | 用户授权质量标准 | `MutinyLocalization.Text`、`MutinyLocalizedText`、编辑器校验 | 回退与日志已实现 | 表、占位符和当前简中字形校验通过；故意缺译及超长文本的运行画面待做 |
| LOC-08 日繁扩展 | 用户授权的未来范围 | Locale/表/字体目录 | 架构预留，内容未实施 | `ja`、`zh-Hant` 译文、字体和地区字形验收待做 |

## 验证记录

- **静态确认：** 15 个武器英文名/说明已从 `Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/game/WeaponSelectButton.as` 对照；回合英语来自同目录的 `throwgame/Team.as`，拾取结构来自 `throwgame/TreasureChest.as`。其它前端文案仍需逐项完成原版证据索引。
- **已实现：** 语言选择持久化、表载入及回退、双字体绘制路径、82 组英/简中文案和上述页面入口。
- **实际测试通过：** Unity 编辑器批处理已编译并执行表生成；`MutinyLocalizationVerification.Validate` 检查了 82 组表项、占位符、简中字形和 15 种武器的 SWF 英文对照，日志输出通过。
- **待运行验证：** Play Mode 批处理已尝试；编辑器到达 `EnteredPlayMode` 后未继续执行检查回调，且 Unity Search 启动索引曾抛 `ArgumentOutOfRangeException`。`ValidatePlayMode` 留在 **Mutiny/Localization** 菜单供可交互编辑器重跑。标题页可读性、缩放/触控、保存后重启、战斗和结算截图、Windows/Android 构建与字体包体均未验收，不登记为通过。
- **已知差异：** 原版武器面板空闲标题为 `Weapons`，目前 Unity 实现和英语表为 `weapons`；沿用既有差异，未在本地化任务中改变。贴图英语和对话未完成，简中字号/溢出尚需实机调整。初版曾在标题页增加语言按钮，已由本轮仅保留 GM 入口的规格替代。

## 后续扩展步骤

### 2026-09-26 GM 语言切换补充

最新 `main` 的手柄支持和修复已合入本分支。新增 `setlanguage cn` / `setlanguage en`，从 GM 唯一解析入口选择语言，包含持久化、错误参数拒绝及历史重放。Unity 6000.6.0f1 的 `Main.unity` Play Mode 已实际验证 **7/7**；前端、表和字体均真实载入，详见 [GM-09 记录](../11-VerificationAndDebug/05-GMTools/Artifacts/GM-09-20260926.txt)。验证启动器现在能在编辑器额外重载程序集后恢复检查，初版记录中的批处理未完成状态是历史结果。该结果不覆盖鼠标点击、重启或所有中文页面画面。

1. 在 `Mutiny.tsv` 中继续补齐船长对话、图片内文案审计和前端细项。每个英语条目补原版 SWF 路径与上下文；译文改变后重建表并运行校验。
2. 将 `MutinySpeechController` 的台词替换为稳定键、参数和语言切换重显；保持语音、计时及状态转换不变。带文字的交互贴图可拆为底图加动态文本或用 Asset Table 选择语言专属资源。
3. 实测 550×400 画布与 Android 横屏布局，记录截图、缺字、溢出、内存及包体。完成 LOC-01～07 的运行验收后再标记通过。
4. 加入 `ja`、`zh-Hant` 表和地区字体，为两种语言跑同一批验收。日语需检查假名和禁则，繁体需检查异体字与地区标点。
