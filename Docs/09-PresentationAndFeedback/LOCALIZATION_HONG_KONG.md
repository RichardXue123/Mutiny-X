# 香港繁体中文与 lang 命令（2026-10-01）

## 实施前行为规格

| ID | 可观察行为及状态转换 | 来源 | Unity 入口 | 验收用例 | 结果 |
| --- | --- | --- | --- | --- | --- |
| LOC-HK-01 | 以用户 CSV 中的英文、简体中文、香港繁体中文三列替换当前译文，保留 150 个键、格式参数、实际换行及 `\n` 换行；注释不作为文案 | 用户提供 `Mutiny_Gemini_3_8_flash_zh-hk.csv`；翻译是授权扩展，原版来源不适用 | `Mutiny.tsv` → 表生成器/运行时 `MutinyLocalization.Text` | 逐项对照原 CSV，检查键集、三语言非空、占位符和转义后的换行 | 已实现；逐项数据及表资源静态检查通过 |
| LOC-HK-02 | `lang en`、`lang zh-cn`、`lang zh-hk` 分别选择英文、简中、香港繁中，切换立即更新菜单、对白、提示及字体并保存；首启继续英文 | 用户当前要求 | `MutinyGMManager.ExecuteCommand` → `MutinyLocalization.Select` | 经实际 GM 入口往返三语言，重放最近命令，重启核对保存值 | 已实现及编译；生产入口回归用例已补充，运行验收由用户执行 |
| LOC-HK-03 | 繁中使用 HK 地区的随包动态字体，粤语字及标点均有字形；英语仍用原版位图字库 | 用户香港本地化要求；Noto CJK 地区字体设计 | `MutinyLocalizedText` → `Resources/Localization/Fonts` | 检查全部繁中字符及对白布局；实际屏幕核对可读性、悬停、命中区域、按钮点击、揭示/停留时序 | 字形及自适应布局静态检查通过；画面与点击待用户运行 |
| GM-09-REV2 | 推荐命令简化为 `lang`，大小写及空白分隔不敏感；缺参、未知代码及额外参数失败且不改语言/存档/成功历史；旧 `setlanguage` 作为兼容别名沿用同一解析入口 | 用户新命令要求；保留旧别名遵循 GM 文档兼容约定 | `MutinyGMManager.ExecuteCommand` | `LANG ZH-HK`、重复选择、三语言、非法参数、旧命令、历史重放 | 已实现及编译；运行验收由用户执行 |

## 证据和验收边界

- 静态检查发现 `speech.ending.0` 的香港译文在固定 15 px 下超过 98 px 文本区；补充 LOC-HK-03 规格：正文默认 15 px，长句按整段文案在 15–13 px 内选择能够容纳的最大字号，逐字揭示期间字号固定，原文、气泡、点击区域和时序不变。验收检查每段完整文案高度及字号范围，画面仍待用户运行。

- CSV 除表头及两条注释外共 150 项，和当前键集一致；英文列未改，简中列按用户文件更新。
- 运行时继续读独立的随包文本，避免重新引入此前 Android 的 String Table 查询异常路径。地区标识使用 `zh-HK`，保留简中既有内部标识 `zh-Hans`。
- 当前工作区已有大量关卡、美术及 GM 未提交修改；本次只补充本地化相关内容，不把它们回退或混入发布。
- **静态确认**：CSV 原件 SHA256 为 `83c5454c4193581a41df9eb16dc9aac189d2ea022442754fb086f4f36e6e8140`；150 × 3 译文逐项一致，TSV/运行时文本字节相同。字体 cmap 覆盖简中 610 个、香港繁中 723 个非空白字符，无缺字（包括 `㗎`）。
- **已实现**：三语文案、香港 Locale/String Table/Addressables 编辑资源、运行时文本与地区动态字体、`lang` 命令/帮助、往返及对白回归用例、长对白按完整文案固定字号。
- **实际测试通过**：仅静态工具执行：当前工作区运行时/Editor C# 项目编译为 0 错误；隔离编辑器批处理 `RebuildAndValidate` 退出码 0，核对 150 个键、三张表、占位符、Unity 字形/气泡高度，以及 15 武器/60 战斗/5 结局英文原版来源。未运行游戏用例。见 [静态验证记录](../11-VerificationAndDebug/02-PlayModeValidation/Artifacts/LOCALIZATION-HK-STATIC-20261001.txt)。
- **待运行验证**：菜单/对白/提示往返、语言重启保留、Windows/Android 交互与画面；遵照用户要求不自行启动游戏。
- **已知差异**：香港译文沿用 CSV 的粤语表达；标题品牌与图片内英文沿用现有资源。旧 GM 别名兼容。

## 内容更新与字体来源

### 机器人对白增补（LOC-ROBOT-01，2026-10-01）

实施前规格：用户在项目 CSV 中新增 `speech.robot.0`～`.3`；按 CSV 同步三语全文，保留既有键，不改写用户文案。授权扩展，不作为原版对白。`Robot`/`RobotCaptain` 队伍使用四条对白，对应开场我方、开场敌方、我方胜利、敌方胜利；经既有 `StartLine`/刷新语言/逐字显示入口展示。导入器接受新增键，继续拒绝缺失旧键、重复键、空译文及参数不一致；未知队伍只有具备完整四条英文源文案才接入对白。

验收用例：核对 CSV/TSV/运行时/三张 Unity 表中的四个键和三语原文；从生产对白解析入口核对机器人四条英文及未知序列/越界索引拒绝；检查字体及气泡高度；用户运行 RobotCaptain 关卡，切换三语言、点击完成及触发双方胜利对白。

补充显示规格：CSV 新增的机器人英文没有原版 `|` 手动分行。英文对白在渲染时按完整句子及原位图字体宽度将空格换为换行，保持字符数量和逐字揭示进度；弯引号/长横线在渲染时使用字库已有的直引号/短横线。CSV、TSV 与运行时原文不改写，既有手动换行保留。验收检查行宽不超过 220 px、块高不超过 92 px。

- **静态确认**：当前 CSV 共 154 项，新增键仅为四条机器人对白；源文件 SHA256 `291c7fc410778d3f8869e61d31122d1101c57ffc1b91c1cd8ecc3b2937181dbc`。前述 150 项证据为本轮增补前的历史结果。
- **已实现**：三语 TSV/运行时文本/Unity 表同步；导入器支持新增键，编辑项目 CSV 时不重复写回源文件；机器人读取 CSV 英文作为扩展对白源，`Robot` 角色类型映射到 `speech.robot.*`；补充双方胜负与三语言生产入口用例。
- **实际测试通过**：静态 CSV/TSV/运行时逐项一致性检查（154 × 3）、当前 C# 编译 0 错误、隔离 Unity 编辑器 `RebuildAndValidate` 退出码 0（三张表、字形、双中文高度、机器人生产解析入口及英文换行宽高）。未运行游戏。日志：`D:\My Project\Mutiny X\_android_localization_check\localization-robot-static-2.log`。
- **待运行验证**：RobotCaptain 关卡开场两段、双方胜利段、三语言切换及逐字/点击/字体画面；已添加用例，未登记运行通过。
- **已知差异**：机器人是用户授权的新内容；英文渲染中的标点使用原位图字库对应形式，源文案原样保留。

原 CSV 保存在 `Assets/Mutiny/Localization/Mutiny.csv`。以后编辑该文件，执行 `python Tools/Import-MutinyLocalization.py`，再用编辑器菜单 `Mutiny/Localization/Rebuild String Tables` 同步表资源；运行时继续使用生成的 `Resources/Localization/MutinyRuntime.txt`。导入器支持 CSV 引号中的实际换行，并校验空值、重复键及格式参数。

香港字体使用官方 [Noto Sans CJK HK Regular v2.004](https://github.com/notofonts/noto-cjk/blob/Sans2.004/Sans/OTF/TraditionalChineseHK/NotoSansCJKhk-Regular.otf)，随包动态加载，地区字形来源见 [官方说明](https://github.com/notofonts/noto-cjk/blob/main/Sans/README.md)。资源路径为 `Assets/Mutiny/Resources/Localization/Fonts/NotoSansCJKhk-Regular.otf`，SHA256 为 `97c937514d645eae90415d30ba025e08a94d5bdffdc627404864f90aa0c7d83b`，许可证沿用同目录 `OFL.txt`。
