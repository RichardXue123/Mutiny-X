# 11.05 · GM 工具

[返回上级模块](../README.md) · [玩家命令说明](../GM_COMMANDS.md)

## 定位与证据边界

GM 是当前 Unity 工程的调试扩展，不是 Flash 原版玩法规则。以下规则的“原版来源”均为**不适用**；本页只陈述从当前生产 C# 入口静态核对到的 Unity 行为。历史 Play Mode 结果单独登记，不用现有代码反推原版。核对入口为 [MutinyGMManager.cs](../../../Assets/Mutiny/Scripts/Presentation/MutinyGMManager.cs)、[MutinyCharacter.cs](../../../Assets/Mutiny/Scripts/Simulation/MutinyCharacter.cs)、[MutinyAIController.cs](../../../Assets/Mutiny/Scripts/Simulation/MutinyAIController.cs)、[MutinySaveSystem.cs](../../../Assets/Mutiny/Scripts/Persistence/MutinySaveSystem.cs) 和 [MutinyTurnActionUiVerificationTest.cs](../../../Assets/Mutiny/Scripts/Verification/MutinyTurnActionUiVerificationTest.cs)。

## 生产链路

1. `MutinyGMManager` 在场景加载后自动创建并 `DontDestroyOnLoad`，代码中没有开发构建或战斗场景开关。`OnGUI` 绘制以 550×400 游戏画布为基准的 30 px 按钮，实际尺寸随画布缩放；按钮切换面板开闭，面板 `X` 关闭。
2. 面板通过 `Run` 或输入框聚焦时的 Enter 调用 `SubmitCommand()`。输入被 `Trim()`，空输入直接返回；随后交给公开的 `ExecuteCommand()`。`ExecuteCommand()` 再次去除首尾空格、转为小写匹配，并向 Unity 日志写一条执行记录。命令结果只显示在面板状态文字与颜色中。
3. 命令解析依次检查关卡解锁、关卡重置、AI 单回合接管、AI 强制武器、角色武器解锁、帮助，最后为未知命令。英文大小写不敏感；`Unlock All` 是精确匹配，`UnlockWeapon*` / `InfiniteWeapon*` 是前缀匹配，AI 命令要求恰好两个空白分隔的字段。除 AI 参数解析外，内部空格不会统一折叠。
4. 进度命令调用 `MutinySaveSystem`；角色武器命令调用 `MutinyCharacter.UnlockAllWeapons(true)`；AI 命令调用静态的 `MutinyAIController.TrySetForcedWeaponId()`，由生产 AI 决策和执行路径读取。各路径没有第二套 UI 专用实现。

## 可观察规则与状态转换

### GM-UI-02 · 最近成功命令（2026-09-26 新规格）

原版来源：不适用，用户授权的 Unity GM 交互扩展。打开 GM 面板时，在状态区与输入框之间显示**最多五个**历史按钮，按成功执行时间从新到旧排列；尚不足五次时只显示已有项。每次成功执行保存去掉首尾空格后的原输入文本，重复执行也各占一次记录。有效的 `Help`、进度重置和当前返回成功的全队零目标命令均计入；空输入、未知命令、无目标的单角色解锁和非法 AI 参数不计入。点击历史按钮应经同一个 `ExecuteCommand` 生产入口重新执行，只有再次成功才把该次执行追加为最新项，超过五项删除最旧项；关闭再打开或切场景保留，重新启动 Play / 游戏时清空，不写入存档。按钮应完整呈现命令文本，避免与状态信息、输入框相互遮挡。

验收用例：经正式解析入口依次执行六条有效命令及错误命令，断言只保留最近五次且顺序正确；执行重复命令应保留两次；点击旧历史项后断言实际命令效果、状态文字和新顺序；关闭/打开面板和切场景后仍显示历史，重启后为空；分别在 550×400 与宽屏下核对五个按钮的可见性、文字、悬停、命中区域、焦点及点击时序。实际结果：已实现并通过 C# 编译；2026-09-26 隔离 Unity 6000.6.0f1 Play Mode 的新增 3 条生产入口断言通过；图形模式下 701×531 Game View 截图确认五项文字完整、布局没有相互遮挡。实际鼠标点击/悬停及跨场景/重启仍待验收。

![GM 面板显示五条最近成功命令](Artifacts/GM-UI-02-20260926.png)

截图由隔离验证启动器经生产 `ExecuteCommand` 输入五条 AI 命令、设置面板为打开状态后在图形模式 Game View 捕获；它验证渲染布局，不作为点击或焦点行为通过证据。第一次无图形批处理截图未生成，改用隐藏的图形模式 Editor 后成功捕获。

下表中的命令 ID 与回归断言的显示文字是两种编号。旧测试把 `GM-02` 至 `GM-06` 用作 GM-01 和按钮的断言标题；命令注册表目前没有 `GM-06`。

| 规则 ID | 输入与前态 → 可观察结果 | Unity 入口 | 验收用例与实际结果 |
| --- | --- | --- | --- |
| `GM-UI-01` | 任意场景点 `GM`：面板开/关；开面板后输入框聚焦；`X` 关闭；`Run` 或聚焦输入框按 Enter 提交；空输入不执行命令。按钮在参考画布为 30 px，随分辨率等比缩放。 | `AutoInitialize`、`OnGUI`、`DrawCommandPanel`、`SubmitCommand` | 完整回归有 2 条按钮矩形断言，已按当前缩放规则更新；未驱动实际点击、聚焦或提交；本轮未运行。 |
| `GM-UI-02` | 成功执行 → 历史列表头部插入原命令，保留最多 5 项；失败不入列；点击历史项通过同一解析入口再次执行并更新顺序。 | `DrawRecentCommands` → `RunRecentCommand` → `ExecuteCommand` | 新增 3 条断言覆盖容量、重复、失败排除和重放效果；2026-09-26 隔离 Unity Play Mode 通过；实际面板交互待验收。 |
| `GM-PARSE-01` | 输入先去除首尾空格、英文大小写不敏感；合法命令按固定顺序分支。未知文本显示 `[ERROR]` 和帮助提示。AI 编号缺失、非整数、越界或多余参数显示用法错误，原覆盖保持不变。 | `MutinyGMManager.ExecuteCommand` | GM-07 专项覆盖 3 种非法输入及覆盖不变；一般别名、空输入、未知命令和状态文字待验收。 |
| `GM-01` | `UnlockWeapons` 等单目标别名：按下述优先级找到一名存活角色 → 15 种菜单武器及 `cannonball` 内部别名无限，`CanShoot=true`；如果该角色在当前回合队伍，同步选中。无存活角色显示错误。 | `FindTargetCharacter` → `MutinyCharacter.UnlockAllWeapons(true)` → `MutinyTeam.SelectCharacter` | 旧完整回归有 4 条武器效果断言，但本轮未运行；目标优先级、无目标及立即操作待验收。 |
| `GM-02` | `UnlockWeaponsAllTeam` 等全队别名：遍历场景中所有存活且 `TeamIndex==1` 的角色并执行 GM-01 的武器写入；没有匹配角色时显示成功、计数 0；不更改当前选中角色。 | `MutinyGMManager.ExecuteCommand` 的 `targetAll` 分支 | 当前没有独立生产入口回归，待验收。 |
| `GM-03` | `unlockalllevels` / `UnlockAll` / `Unlock All`：最高已解锁关卡 → `MaxLevel=18`，通过存档层持久化，1..18 全部可进入；执行成功计入历史按钮。 | `ExecuteCommand` → `MutinySaveSystem.HighestUnlockedLevel` | 由仅第 1 关解锁开始，经精确小写命令检查全部 18 关资格与存档键，再经历史按钮重放检查相同效果；测试结束恢复原进度。2026-09-26 隔离 Unity Play Mode 新增 2 条断言通过；重启/实际选关 UI 待验收。 |
| `GM-04` | `ResetLevels` / `ResetProgress` / `LockAll`：删除最高已解锁关卡键，下次读取返回默认值 1；完成分数和音频设置不受影响。 | `ExecuteCommand` → `MutinySaveSystem.ResetProgress` | 当前没有独立 GM 命令回归，待验收。 |
| `GM-05` | `Help` / `?`：状态文字变为命令简表，不修改战斗与存档。简表只列推荐命令，不覆盖所有别名。 | `MutinyGMManager.ExecuteCommand` | 当前没有独立面板文字回归，待验收。 |
| `GM-07` | `aiforceusewaepon 1..15`：全局 AI 武器候选只含对应菜单武器，按无限弹药执行且不扣真实库存；跳跃、Pass 与通常 `CanShoot` 门仍有效。`0` 清除覆盖。非法输入保持原值。 | `ExecuteCommand` → `MutinyAIController.ForcedWeaponId` → 决策候选 → `AIMove.UsesForcedWeaponSupply` → `ExecuteMove` | 2026-09-25 当前工程 Play Mode 专项回归历史结果 7/7；本轮未重新运行。完整战斗画面与“有候选但收益非正”的续行动分支待验收。 |
| `GM-08` | `aitakeover 1`：当前人类回合剩余行动交给 AI；允许真实跳跃飞行中/落地后接管，不重置资格；完整回合结束恢复人类控制。 | `ExecuteCommand` → `TryTakeOverCurrentPlayerTurn` → 通常 AI 流程；`RestorePlayerControl` | 2026-09-26 隔离 Play Mode 新增 12 条生产入口断言通过；专项详情见下文。 |

### 单目标选择顺序

`GM-01` 依次查找：当前队伍已选存活角色、Team 1 已选存活角色、当前队伍首个存活角色、Team 1 首个存活角色、场景中标记为已选的存活角色、场景中首个 Team 1 存活角色、场景中首个任意存活角色。因此在 AI 回合或只有 AI 角色时，单目标命令也可能作用于 AI；命令名称本身不限定玩家队伍。若因此切换当前队伍选中角色，生产 `SelectCharacter` 还会清除该角色的自投掷标记、发送选中事件并请求角色语音；目前 GM 回归未单独覆盖这些间接效果。

### 状态存活期

| 状态 | 写入位置 | 切场景 | 重新开始 Play / 重启游戏 |
| --- | --- | --- | --- |
| GM 面板开闭与文字 | `MutinyGMManager` 常驻实例 | 保留 | 重置 |
| GM-UI-02 最近成功命令 | 同一 `MutinyGMManager` 实例内的最多 5 条历史 | 保留 | 清空，不写入存档 |
| GM-01/02 武器库存与无限标记 | 当前 `MutinyCharacter` 对象 | 对象随场景销毁时消失；新关角色不继承 | 重置 |
| GM-03/04 最高关卡进度 | `MutinySaveSystem` 的 `mutiny_highest_unlocked_level` | 保留 | 保留；除非再次重置 |
| GM-07 强制武器编号 | `MutinyAIController` 静态字段 | 保留 | `SubsystemRegistration` 重置为 0 |
| GM-08 当前人类回合接管 | `MutinyTurnManager.m_AiTakeoverTeam`，临时队伍 AI 标记 | 管理器销毁即恢复；不转移至新关 | 重置；同一场景完整回合结束、GameOver 或 StartGame 也恢复 |

GM-07 在决策开始时把武器类型复制到工作对象；已选射击动作另存是否使用强制供给。因此在决策或执行途中改变命令，不追溯改写那次动作。GM-01/02 则直接修改角色的 `WeaponInventory`、`InfiniteWeapons` 和 `CanShoot`；`GetAmmunition()` 对无限武器返回 `-1`，底层库存值为 `int.MaxValue`。`cannonball` 是大炮内部库存别名，不占第 16 个菜单编号。

## 待执行验收用例

以下是完整人工验收定义，自动断言和布局截图的已运行范围见下文，未覆盖部分仍待运行。

- `GM-UI-01`：分别在菜单和战斗场景观察按钮可见性；核对常态、悬停、圆形图像、实际矩形按钮命中区与带黑边画布定位；点击打开、关闭，检查焦点时序；分别用 `Run`、聚焦后的 Enter 和空输入提交，检查命令只执行一次。
- `GM-UI-02`：通过 `Run` 和 Enter 积累五次以上成功命令，并穿插错误命令及重复命令；核对五个按钮从新到旧排列、各项完整显示、悬停和矩形命中区。点击一条较旧记录，检查该命令真实再执行且移至首位；关闭再打开、切场景和重启时核对存活期。
- `GM-PARSE-01`：从面板输入前后带空格及大小写变化的合法命令、未知命令；逐一提交缺参、额外参数、非整数与越界的 AI 命令，读取面板错误文字并确认 `ForcedWeaponId` 保持原值。
- `GM-01`：用正式 `ExecuteCommand` 在当前队伍已选、仅 Team 1 已选、场景内仅有存活 AI、完全无存活角色等局面执行；检查目标顺序、15 种武器与大炮别名、`CanShoot`、选中事件及无目标错误。
- `GM-02`：构造两个存活 Team 1 角色、一个死亡 Team 1 角色及一个其他队伍角色，执行全队命令；检查仅两名目标改变、原选中角色不变。再于零目标场景执行，核对当前实现的成功计数 0。
- `GM-03/04`：先保存测试前的存档进度与已完成分数，经 GM 生产命令分别解锁和重置；检查当前读取值、切场景与重新启动后的值，并确认已完成分数和音频设置不变；验收后恢复测试前存档。
- `GM-05`：通过面板执行 `Help` 与 `?`，核对显示的推荐命令和无战斗/存档状态修改。
- `GM-07`：沿用专项生产解析与 AI 候选/执行回归；另在真实回合构造“有强制武器候选但评分非正”的续行动和跨场景情形，核对 Pass、库存不扣减、`0` 关闭及重新开始 Play 时清零。

## 验收状态与缺口

### GM-08 · 单回合接管（2026-09-26）

原版来源：不适用，用户授权扩展。规格与错误条件见 [命令说明](../GM_COMMANDS.md#gm-08--当前玩家单回合-ai-接管)。生产入口为 `ExecuteCommand` → `TryTakeOverCurrentPlayerTurn`；不调用 `StartTurn`，已跳跃者保留所选角色及剩余射击资格，飞行中等待正式结算；未提交的玩家瞄准、武器和光标经输入生产清理入口撤销。现有 AI 组件负责候选/评分/镜头与武器分支，回合管理器在完整回合结束、GameOver、重新初始化和销毁时恢复人类控制。错误输入及重复请求不入成功历史，不延长接管。

- 静态确认与已实现：GM 解析、Help、输入清理、临时 AI 控制、自动恢复及通常两阶段流程已接入；`Assembly-CSharp-Editor` 连同运行时程序集编译通过，3 个原有警告、0 错误。
- 实际测试通过：Unity 6000.6.0f1 隔离 Play Mode 执行 `RunGM()`，24/24 断言通过，其中新增 GM-08 12 条。覆盖错误参数、未提交武器清理、跳跃候选保留、重复拒绝、生产 Pass 后恢复、下一轮不继承、真实玩家跳跃空中接管、落地后仅原角色的武器候选、海啸正式生命周期期间维持接管及结束后恢复、重开和 GameOver。候选/执行使用生产测试入口，跳跃和换队没有在用例中手动重写行动布尔量。
- 实际协程通过：同一隔离工程额外运行 `GmTakeoverLiveBatchRunner.Run`，自然驱动 AI `Start/Update` 协程、物理和回合 `Update`，2/2 场景通过：未跳跃接管自动开火并换队恢复；重开后真实玩家跳跃途中接管，落地后自动同角色射击并在 GameOver 恢复。该运行未直接调用 AI 求值/执行测试入口。日志与验证边界见 [GM-08 记录](Artifacts/GM-08-20260926.txt)。
- 待运行验证：真实鼠标/触摸提交、双人模式玩家 2 的实际画面、拾取空投现场、15 种武器各自的接管画面；已实现不表示这些场景全部运行过。
- 已知差异：此命令无 Flash 原版对应；仅接受参数 1，不接管未来多个回合，不在已经提交的武器序列中途转换操作者。复用/新增 AI 组件保持在队伍上，恢复后由通常 AI 资格门阻止求值。

- **静态确认**：上述入口、匹配顺序、写入位置和状态存活期均已按当前 C# 核对；原版来源不适用。
- **已实现**：GM-UI-01/02、GM-PARSE-01、GM-01 至 GM-05、GM-07 的对应生产路径存在；GM-UI-02 已在打开面板时绘制最多五项历史、由同一解析入口重放。GM-03 精确小写 `unlockalllevels` 已支持，帮助文案也显示该拼写。2026-09-26 当前 `Assembly-CSharp.csproj` C# 编译通过（3 个已有警告，0 错误）。
- **实际测试通过**：2026-09-26，Unity 6000.6.0f1 隔离工程 Play Mode 执行当前 `RunGM()`，`12/12` 断言通过，含 GM-07 的 7 条、GM-UI-02 的 3 条及 GM-03 的 2 条。最新验证复制当前生产文件与回归文件，没有屏蔽断言；此前历史按钮初次验证 `10/10` 时屏蔽过无关的未完成相机断言，仅为历史记录。GM-03 验证全部关卡资格、实际存档键、历史按钮同源重放；专项菜单没有执行 6 条旧 GM-01/按钮断言。隔离工程图形模式 701×531 Game View 截图确认历史五按钮可见、文字完整、与状态/输入区不重叠。
- **待运行验证**：GM-UI-01/02 的鼠标悬停、按钮命中区域、键盘焦点、实际点击与时序，以及 GM-UI-02 的跨场景和重启存活期；GM-01 的目标优先级及错误分支；GM-02 的人数和全队边界；GM-03 的重启及实际选关 UI、GM-04 的完整存档流程；GM-05 与未知命令的实际文字；GM-07 在完整回合及非正收益续行动中的表现。
- **已知差异与边界**：GM 系列整体为 Unity 扩展，不能计入 Flash 原版一致性。按钮视觉为圆形，`GUI.Button` 的命中区为矩形；GM-02 在零目标时仍显示成功；武器前缀匹配会接受任意后缀；公开 `ExecuteCommand(null)` 没有空值保护，正常 UI 提交路径不会传入 null。以上是当前行为，后续如决定调整，须先固定规格并补能检出缺陷的生产入口用例。

## 维护入口

本轮运行日志摘录：[GM 专项 12 条断言结果（2026-09-26）](Artifacts/GM-12Assertions-20260926.txt)。保留 Unity 版本、精确小写命令执行和最终通过数；测试恢复原进度，不替用户实际解锁主工程存档。

修改命令时同步核对 [玩家命令说明](../GM_COMMANDS.md)、`MutinyGMManager.ExecuteCommand` 中的 `Help` 文案及 [GM 回归](../../../Assets/Mutiny/Scripts/Verification/MutinyTurnActionUiVerificationTest.cs)。专项菜单 `Mutiny → Parity → Validate GM Commands` 见 [MutinyParityValidationMenu.cs](../../../Assets/Mutiny/Scripts/Verification/Editor/MutinyParityValidationMenu.cs)。新增命令先分配稳定 ID，写明输入、目标、前后态、持久化范围、错误行为、原版来源（扩展写不适用）、生产入口及实际验收结果；不得沿用旧断言标题充当命令 ID。
