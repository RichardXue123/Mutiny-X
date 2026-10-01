# GM 控制台分类与快捷输入规格

状态：用户授权的 Unity GM 扩展。原版来源：不适用；不作为 Flash 一致性结论。本文先于实现记录目标行为；旧 GM-UI-02 五条最近成功命令的截图和验收只代表历史基线。2026-10-02 已完成代码修改、C# 编译和隔离 Play Mode 生产入口回归；Unity 图形交互尚待运行。

## 2026-10-02 交互修订规格（实现前记录）

| 规则 ID | 输入与前态 → 可观察行为 | Unity 入口 | 验收用例 | 实际结果 |
| --- | --- | --- | --- | --- |
| GM-UI-04-A | 点击任何带参数占位符的命令按钮 → 输入框填入完整命令词，词尾**恰好一个空格**，光标位于空格后；无参数按钮不追加空格。按钮点击仍不执行命令。 | `DrawCommandCategories` → `ChooseCommandTemplate` → 输入框 | 逐一检查 `enterlevel`、`aienhance`、`aisetluck`、`aiweapon`、`aitakeover`、`ailog`、`excamera`、`lang` 和无参数按钮；补充真实 GUI 点击与焦点验收。 | 已实现；隔离 Play Mode 的模板入口断言通过。真实 GUI 点击、空格保留及焦点待运行。 |
| GM-UI-05-A | 悬停命令按钮 → 中文详细说明；触摸点击后的说明和 `Help` 对应描述使用同一中文文本。参数取值与命令效果保留在说明中。 | 命令定义 → `GUIContent.tooltip`、选中说明、`BuildHelpMessage` | 逐项检查中文说明和 Help 文本一致、中文字体可显示；实际悬停及触摸待图形/设备验证。 | 已实现；隔离 Play Mode 检查 14 条中文说明、Help 同源文本及随包中文字体通过。实际悬停、字形和触摸待运行。 |

| 规则 ID | 输入与前态 → 可观察行为 | Unity 生产入口 | 验收用例 | 实际结果 |
| --- | --- | --- | --- | --- |
| GM-UI-02-R | 打开 GM 面板 → 不再出现最近执行按钮；成功或失败的命令均不创建可重放历史。 | `MutinyGMManager.DrawCommandPanel`、`ExecuteCommand` | 连续执行六条命令后检查面板无历史按钮；旧回归改以真实状态和返回值判断命令效果。 | 已实现、静态确认旧入口及字段移除；图形交互待运行。 |
| GM-PARSE-02 | 输入易读别名 → 与对应完整命令使用同一参数验证和执行效果；旧命令与原兼容别名继续有效。命令词完整匹配，不能把任意后缀当作新别名。 | `MutinyGMManager.ExecuteCommand` | 逐条验证新旧写法、大小写、缺参、多参、非法值及实际状态；检查相似但不完整的命令词被拒绝。 | 已实现；隔离 Play Mode 的 GM 82/82、关卡入口 68/68 通过。 |
| GM-UI-04 | 打开面板 → 左侧显示分类 Tab，右侧显示该类全部命令按钮；切换 Tab 不执行命令。点击按钮只把完整命令或带一个尾随空格的参数前缀填入输入框，光标移到末尾；实际执行仍由 `Run` 或输入框内 Enter 触发。 | `DrawCommandPanel` → 模板填入 → `SubmitCommand` → `ExecuteCommand` | 在关卡与 AI Tab 点击 `enterlevel {level_id}`、`aienhance {flag}`、`aisetluck {luck}`；核对文字、焦点、未执行状态和提交后的真实效果。 | 已实现；模板入口在 GM 82/82 中通过，真实 GUI 点击及焦点待运行。 |
| GM-UI-05 | 鼠标停在命令按钮上 → 显示与 `Help` 共用的该命令说明；触摸设备选择按钮后也显示该说明。参数范围和无效前态应可读。 | 命令元数据 → 按钮说明与 `Help` | 对照每项按钮说明与 Help；检查悬停、命中区域和触摸替代信息。 | 已实现；Help/说明共源检查在 GM 82/82 中通过，实际悬停和触摸待运行。 |
| GM-UI-06 | 命令按钮区、输入行、输出区从上到下分离；最新命令结果或 Help 显示在输入框下方，长内容可滚动查看。点模板不会清空现有输出。 | `DrawCommandPanel` 的输出区域 | 550×400、701×531 和宽屏核对可见性、遮挡、滚动、输入焦点、关闭及固定面板时序。 | 已实现；实际画面待运行。 |

## 分类与输入模板

| 左侧 Tab | 右侧按钮（显示文本） | 点击后填入 |
| --- | --- | --- |
| 关卡 | `enterlevel {level_id}`、`unlockalllevels`、`resetlevels` | `enterlevel `、`unlockalllevels`、`resetlevels` |
| AI | `aienhance {flag}`、`aisetluck {luck}`、`airesetluck`、`aiweapon {weapon_id}`、`aitakeover {luck}`、`ailog {flag}` | 对应完整命令；有参数时以一个空格结尾 |
| 武器 | `unlockweapons`、`unlockweaponsallteam` | 对应完整命令 |
| 镜头 | `excamera {flag}` | `excamera `；说明 `1` 开启，`0` 或原无参命令关闭 |
| 系统 | `lang {code}`、`help` | `lang `、`help` |

别名只为手输减负，不替换按钮上的可读完整命令。新别名：`level` → `enterlevel`，`unlocklevels` → `unlockalllevels`，`ailuck` → `aisetluck`，`unlockteamweapons` → `unlockweaponsallteam`，`blastcam` → `excamera`。`resetlevels`、`aienhance`、`airesetluck`、`aiweapon`、`aitakeover`、`ailog`、`unlockweapons`、`lang`、`help` 已简短清楚，沿用原名。原来允许的 `aiforceusewaepon` 拼写仍需兼容。

命令定义应共用稳定 ID、分类、按钮文字、填入文本、Help/悬停说明；执行层继续调用原有生产处理路径。输入模板覆盖输入框中尚未提交的文字，随后聚焦并把光标放到末尾。填入及 Tab 切换不修改游戏状态。无参数按钮也只填入，不自动执行。输出区继续呈现最新一次命令结果，不在此次需求中新增多条输出历史。

## 验收记录要求

实现后分别登记静态确认、已实现、实际测试通过、待运行验证和已知差异。自动测试必须驱动按钮所用的模板入口及 `ExecuteCommand`，并检查实际游戏状态；只读取元数据或手动设置 AI/存档字段不能证明完整命令流程。实际鼠标悬停、点击、焦点和触摸尚需图形模式或设备验证，未运行前保持“待运行”。

## 2026-10-02 实际结果

- **静态确认**：`DrawRecentCommands`、`RunRecentCommand` 与最近成功历史字段已从生产和当前回归中移除。`Help` 与按钮说明读取相同定义；简写只替换完整的首个命令词。
- **已实现**：五个分类 Tab、参数模板填入、中文悬停说明、触摸点击后的中文说明、输入框下方的可滚动输出及五条易读简写。带参数的模板统一在命令词后生成一个空格，按钮在输入框之后处理点击，避免该帧旧编辑状态覆盖填入文本。旧完整命令与兼容拼写仍由同一执行入口处理。
- **实际测试通过**：Unity 6000.6.0f1 独立副本的 Play Mode，资源扫描 1/1、关卡入口 68/68、GM 82/82、双人 56/56、镜头初始化 13/13、结局 16/16，总计 236/236；[本次完整断言记录](Artifacts/GM-CONSOLE-TEMPLATES-ZH-20261002.txt)。GM 项覆盖全部带参/无参模板、14 条中文说明、Help 同源文本、随包中文字体、简写、边界和真实状态；`level` 经完整关卡加载链验证。两个 C# 程序集编译 0 错误。原先 235/235 的[记录](Artifacts/GM-CONSOLE-20261002.txt)为修订前基线。
- **待运行验证**：真实鼠标悬停/命中、Tab 和模板按钮点击后输入框空格与光标位置、中文说明的实际字形、长 Help 输出滚动、550×400 与宽屏画面、Android 触摸说明。图形批处理没有提供 Game View 帧；没有把截图尝试记为通过。
- **已知差异**：GM 为用户授权的 Unity 调试扩展，无 Flash 对应行为；命令按钮保留英文命令词，分类标签与详细说明使用中文。
