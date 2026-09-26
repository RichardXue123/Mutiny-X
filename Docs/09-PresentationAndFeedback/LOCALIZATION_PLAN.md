# 多语言本地化方案（设计基线，2026-09-26）

此文保留实施前的调查与目标；`feature/Localization` 的初版实现及验证状态见 [LOCALIZATION_INITIAL_IMPLEMENTATION.md](LOCALIZATION_INITIAL_IMPLEMENTATION.md)。

2026-09-27 用户将默认语言改为英语，覆盖本文“设备语言优先”的旧设计。发行版故障及新规格见 [LOCALIZATION_PLAYER_BUILD_FIX.md](LOCALIZATION_PLAYER_BUILD_FIX.md)。

本轮仅调查与设计，未安装包、导入字体、修改代码或执行 Unity 运行验收。英语保留为原版对照语言；简体中文是用户授权的新语言。日语、繁体中文先预留数据和字体架构，未列为当前翻译完成范围。

## 1. 已确认的工程基线

- 工程为 Unity `6000.6.0f1`。`Packages/manifest.json` 目前没有 `com.unity.localization`，也没有单列 TextMesh Pro 依赖。
- 主要游戏 UI 在 `MutinyFrontendController`、`MutinyGameHUD` 中用 IMGUI `OnGUI` 绘制，逻辑画布为 550×400。文字多数经 `MutinyBitmapFont` 从 `pirate_font.png`、`dangle_font.png` 绘制；两个图集只有有限的拉丁字符，字形不存在时当前代码会跳过该字符。现有中文文字不能直接交给此渲染器。
- 原版英语来源可以回溯到 SWF 的 `frame_91/PlaceObject2_319_PirateFont_36`（`select game`）、同帧 DangleFont 文案、`WeaponSelectButton.as::hoverText`、`Team.as::lines` 等。将这些来源作为英语表的取证入口，不从现有 C# 文案反推原版。各字符串的逐项取证仍需在实施时完成。
- 前端按钮、武器标题和说明、帮助说明、分数页、制作人员、回合提示、角色对话、结算弹窗、载入字样、控制器提示都含玩家可见英语。部分提示文字烘焙在右上角 `*_over.png` 中；`title_logo.png` 含 `MUTINY` 品牌文字。已抽样检查这些图片，尚未对全部图片完成逐张文字审计。
- `MutinySaveSystem` 当前保存进度、分数和音频设置，尚无语言设置。`ResetProgress()` 只删除关卡进度键；后续语言设置不应随进度重置。
- 现有文本兼作部分交互判断：`DrawOriginalButton` 用显示文本生成控制器 ID，并用 `StartsWith("back")` 决定返回按钮行为；拾取提示会用英语标题组成整句。实施时必须先把稳定行为 ID 与本地化显示文本拆开。

## 2. 技术选型

**文本数据：Unity Localization 包。** 待实施时经 Package Manager 选择与本工程 Unity 版本兼容的正式版，使用 Locale、String Table、Smart String；少量语言专属图片用 Asset Table。该包适合管理翻译、参数、伪本地化与导出；由于目前是 IMGUI，须写一层薄的查询/刷新适配器，不能假设给现有位图绘制函数换一个字符串就能显示中文。

**渲染：保留英语点阵字体，新增 Unicode 动态字体路径。** 英语仍走 PirateFont/DangleFont，以保持原版像素画面。中文使用导入工程的、授权可分发的 CJK 字体，由 Unity 动态栅格化实际用到的字形；无需把所有汉字预先烘焙到图集。当前 IMGUI 可由 `GUIStyle.font` 走动态 `Font`，并在独立的文本绘制适配层处理测量、换行、颜色、描边/阴影、溢出检测。以后若整体迁移到 UGUI/TMP，再复用同一批字符串表和语义键，并改用 TMP 动态字库及 fallback；不为本轮方案要求整体重做 UI。

**字体来源：** 首选随游戏打包的 Noto Sans SC 常规字重作为简体中文原型，经画面测试后再定最终字体。日语用 JP、繁体中文用 TC 的地区字形资源；简繁日的同一汉字也可能需要不同字形，不能只共用一个 SC 字体。Noto CJK 有地区版本和 OFL 许可。可把操作系统字体作为编辑器原型或故障兜底，但发行版不依赖玩家设备上恰好安装了某字体。动态字库只降低图集预烘焙压力；完整源字体仍会进入构建并占空间，包体预算要单独实测。必要时从已翻译的文本导出字符集做静态子集，并保留动态兜底和缺字检查。

## 3. 数据与运行时设计

| 领域 | 建议表 | 示例稳定键 | 备注 |
| --- | --- | --- | --- |
| 主菜单、导航、设置 | `Frontend` | `frontend.play`、`frontend.language` | 键与原文、按钮序号、显示文本解耦 |
| 关卡、计分、结算 | `Progress` | `result.level_complete`、`score.points` | 分数存档格式保持不变，展示时可按语言格式化 |
| 武器和 HUD | `Battle`、`Weapons` | `weapon.cherry_bomb.name`、`weapon.cherry_bomb.desc` | 武器内部 ID `cherryBomb` 不翻译；拾取语句引用本地化武器名 |
| 回合与动态提示 | `Events` | `turn.player`、`pickup.weapon` | 以 `{player}`、`{weapon}` 等占位符组织完整句，不拼英语片段 |
| 船长对话 | `Dialogue` | `speech.squid.intro.player` | 以阵营/角色/阶段稳定键索引，译者决定换行 |
| 帮助、制作人员 | `Help`、`Credits` | `help.throw_move`、`credits.heading.artwork` | 人名、品牌、网址分别标记为不翻译或可翻译 |

- Locale 第一批为 `en`、`zh-Hans`；今后增加 `ja`、`zh-Hant`。地区差异确有需要时再加 `zh-Hant-TW`、`zh-Hant-HK` 等，不用数组下标当语言 ID。各表中的英语需有对应的原版证据路径和必要的上下文注释。
- 选择顺序：玩家已保存的受支持语言 → 设备语言中受支持的语言 → 英语。缺少单条译文时退回英语并记录缺失键；缺字时显示明确的替代符并记录具体码点，禁止静默丢字。语言切换写入单独偏好，不影响关卡、成绩、音量。
- 主菜单首帧前预加载必需表和字体；运行时按语言切换异步装载、缓存结果，完成后统一通知当前 UI 刷新。`OnGUI` 只读取已缓存的字符串和字体，不在每次重绘时同步加载表或对整个语言字符集做预热；常见菜单字形可在切换完成时预热。
- 语言变更事件让当前页面文字、悬停说明、弹窗、帮助和图片重新取值。事件队列存“键 + 参数”，而非已经拼好的英语句子；切换后未展示的事件按新语言解析。对话当前行切换的重显方式应先定规格，再实现，且不能重复触发台词/语音或改动回合状态。
- 对话正文、说明文统一使用正常换行和区域测量；原版英语中的 `|`、`||` 仅在取证导入时解释为换行意图，不要求中文沿用原英文断行。颜色、悬停、禁用、命中区域继续由 UI 状态控制，不从译文推导。
- 文本角色至少区分 `Title`、`Button`、`Body`、`Tooltip`、`Speech`、`Number`，每个语言可配字体、字号、行高和最大行数。品牌 Logo 与人名可保留原样，但必须在资源清单中明确决定。

## 4. 字体与布局约束

1. 原版英文标题的 23 px 点阵字与正文的 11 px 点阵字不能直接作为中文字高标准。中文需要在实际 550×400 画布和 Android 横屏设备上试出最低可读字号，再测量各按钮、武器说明区（约 238×66）、对话气泡（约 220×92）、右上角窄提示气泡。长文优先重写译文、自动换行或针对该语言调整文本区域；缩到看不清不算完成。
2. 保持按钮底图、悬停、点击和游戏状态逻辑独立于文字。若翻译后必须扩大按钮或气泡，重新验证可见性、悬停、触控命中和遮挡；不能只让字超出原矩形。
3. 图内英语分为三类：可保留的品牌/标志，拆成纯背景加动态字的 UI，以及必须由 Asset Table 切换的少量贴图（例如 `music_*_over.png` 的烘焙提示）。先逐张审计，再决定；不要把带动态子按钮的整张面板做语言版 PNG。
4. 验证繁体和日语时检查异体字、全角标点、假名、禁则换行和字体 fallback 顺序。不得仅凭“字体包含该 Unicode 码点”判定地区字形正确。

## 5. 分期实施（待后续授权实施）

1. **建索引与规格。** 从 SWF 构造脚本、`WeaponSelectButton.as`、`Team.as`、时间轴资源生成“原版英语 → 稳定键 → Unity 入口 → 翻译上下文”清单；盘点所有 `GUI.Label`/位图文字、图片内文字和调试界面。列出英语原版与 Unity 当前文案不一致项。
2. **接入底座。** 安装 Localization 包，建 Locale 和各表；在持久化中增加独立语言偏好；把交互 ID、武器 ID、队伍 ID 与显示文本分离；做统一查询、缺译/缺字日志和语言变更刷新。
3. **打通字体与核心流程。** 先完成主菜单、语言选择、关卡选择、战斗 HUD、结算的英语/简体中文切换；导入 SC 字体并做动态字形渲染、尺寸测量和溢出标记。英语原版渲染做截图回归。
4. **补齐内容。** 武器说明、帮助、船长对话、回合/拾取提示、分数、制作人员、控制器提示和带字图片逐项本地化。完成译文校对和用语表。
5. **扩展验证。** 使用伪本地化与长文压力用例发现溢出；再加入 `ja`、`zh-Hant` 的翻译表和对应字体配置，而非复制 UI 逻辑。构建 Windows/Android，记录包体、内存、字形图集与帧率。

## 6. 行为与验收登记

这些是未来要执行的验收用例，**本轮没有运行，均为待运行验证**。原版英语的来源只用于英语一致性验收；新增语言和切换行为来自用户授权扩展。

| ID | 可观察行为及状态转换 | 来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| LOC-01 | 首次启动选择受支持的设备语言；再次启动优先使用玩家保存的语言 | 用户授权扩展；现有存档结构 | Localization 初始化、`MutinySaveSystem` | 分别以英语/简中系统语言首启，手选另一语言后重启，重置进度后再启动 | 待实现、待运行验证 |
| LOC-02 | 标题页切换语言后，导航、悬停文字、当前弹窗即时改为目标语言，按钮仍可点击/返回 | 用户授权扩展；原版主菜单时间轴 | `MutinyFrontendController`、控制器 UI | 鼠标、触控、手柄切换并进入/返回各页，检查焦点与命中区 | 待实现、待运行验证 |
| LOC-03 | 英语模式仍显示原版点阵字、原版位置与状态图 | SWF `PirateFont`/`DangleFont`、帧构造脚本 | `MutinyBitmapFont`、前端/HUD | 原版截图与 Unity 550×400、宽屏截图对照，逐态检查 | 静态来源部分确认；待运行验证 |
| LOC-04 | 简中武器名、说明、拾取和回合提示完整可读，数值及行动不受影响 | 原版 `WeaponSelectButton.hoverText`、`Team.startTurn`；用户授权译文 | `MutinyGameHUD`、`MutinyIngameTextArea` | 实际开菜单、悬停 15 种武器、拾取、结束回合；查缺字、截断及生产入口行动结果 | 待实现、待运行验证 |
| LOC-05 | 对话按所选语言出现、逐字和点击完成行为可用，语言切换不重复推进状态 | 原版 `Team.lines`、`SpeechBubble`；用户授权译文 | `MutinySpeechController`、`MutinyGameHUD` | 进入关卡触发双方台词、点击完成、胜负台词；核对事件和气泡布局 | 待实现、待运行验证 |
| LOC-06 | 结算、分数、帮助、Credits 及图片内文字按语言策略显示 | 原版时间轴/资源；用户授权译文 | `MutinyFrontendController`、`MutinyGameHUD`、Asset Table | 遍历页面和结算分支，检查所有图片、长文和版权/人名策略 | 待实现、待运行验证 |
| LOC-07 | 缺翻译、缺字、长文能被检测；单项缺译回退英语但不影响操作 | 用户授权质量标准 | 本地化适配器、验证脚本 | 删除测试表中的一个键、注入罕字与伪本地化，查日志与画面 | 待实现、待运行验证 |
| LOC-08 | 新增 `ja` / `zh-Hant` 时不改业务路由，字体和地区字形正确 | 用户授权的可扩展目标 | Locale、String/Asset Tables、字体配置 | 加测试表/字体，切换语言并跑同一 UI、回合、结算用例 | 预留设计；待实现、待运行验证 |

## 7. 当前状态和待决定项

- **静态确认：** 工程版本、IMGUI/点阵字体限制、主要文本入口和抽样图片文字；英语若干原版字符串有 SWF/AS2 直接来源。
- **已实现：** 本轮仅形成方案文档；游戏本地化功能未实现。
- **实际测试通过：** 本轮没有运行测试，不登记通过。
- **待运行验证：** 上表 LOC-01 至 LOC-08、全部图片文字审计、Windows/Android 字体效果和包体测量。
- **已知差异：** 当前没有简体中文字体/翻译，英语点阵渲染会跳过汉字；原版文本与当前 Unity 文案可能存在差异，实施时逐项对照登记。
- **待产品决定：** `MUTINY` Logo 是否保持英文品牌；中文目标风格偏原版像素风还是优先小屏可读性；是否把 GM/调试界面列入玩家语言范围。建议先保留品牌原样，正文优先可读，GM 工具维持开发英语。

## 8. 参考资料

- Unity 官方：[Localization 包概览](https://docs.unity3d.com/ja/6000.0/Manual/com.unity.localization.html)、[Unity 6 本地化指南](https://docs.unity.com/en-us/engine/6000.3/manual/best-practice-guides/bpg-uiad-index/localization)、[动态/静态/系统字体图集模式](https://docs.unity3d.com/cn/6000.0/ScriptReference/TextCore.Text.AtlasPopulationMode.html)、[IMGUI 动态字体字号](https://docs.unity3d.com/cn/6000.0/ScriptReference/GUIStyle-fontSize.html)。
- Noto 官方：[CJK 地区字体选择](https://github.com/notofonts/noto-docs/blob/main/docs/website/use.md)、[地区子集和下载格式](https://github.com/notofonts/noto-cjk/blob/main/Sans/README.md)、[OFL 许可](https://github.com/notofonts/noto-cjk/blob/main/Sans/LICENSE)。
