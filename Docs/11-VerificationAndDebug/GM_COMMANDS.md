# Mutiny GM 命令说明书

状态：当前 Unity 调试扩展。它不属于 Flash 原版行为，不应作为原版一致性结论。

## 使用方式

进入游戏后，屏幕左侧可见画布的垂直中央会有圆形 `GM` 按钮；该按钮也会显示在菜单等非战斗场景。点击后输入命令，输入框获得焦点时按 Enter，或点击 `Run` 执行；再次点击 `GM` 或点击面板 `X` 可关闭。英文命令不区分大小写，输入会先去除首尾空格；空输入不会执行。除 `Unlock All` 和 AI 命令的参数分隔外，内部空格不会被规范化。

面板中显示最近**最多五次成功执行**的命令按钮，最新一次排最前；不足五次时只显示已有记录。重复命令会占据多个位置。点击按钮会重新执行该命令，若成功便再次成为最新记录。无效命令和执行失败的命令不会进入历史。历史在当前游戏运行时跨场景保留，重启后清空，不写入存档。当前把有效的 `Help`、`ResetLevels`，以及返回成功但命中 0 人的 `UnlockWeaponsAllTeam` 也算作成功执行。

命令由 [MutinyGMManager.cs](../../Assets/Mutiny/Scripts/Presentation/MutinyGMManager.cs) 的 `ExecuteCommand` 解释。新增或修改命令时，必须同时更新本说明书、控制台 `Help` 文案与 `MutinyTurnActionUiVerificationTest` 中的 GM 回归用例。

## 命令总览

| ID | 推荐命令 | 可用别名 | 效果 | 持久化范围 |
| --- | --- | --- | --- | --- |
| GM-01 | `UnlockWeapons` | `UnlockAllWeapons`、`InfiniteWeapons`、`AllWeapons`、`Weapons`、`解锁武器`、`无限武器`，以及以 `UnlockWeapon` 或 `InfiniteWeapon` 开头的英文输入 | 给目标角色开启 15 种可选武器的无限弹药；大炮所需的 `cannonball` 内部别名也同时可用，并恢复该角色的武器行动资格。 | 仅当前运行时角色；切场景、重开或重启游戏后不保留。 |
| GM-02 | `UnlockWeaponsAllTeam` | 任何已被 GM-01 识别，且同时含 `all` 和 `team` 或 `player` 的英文输入，例如 `UnlockWeaponsAllPlayers` | 给场景中所有存活的 Team 1 角色开启 GM-01 的无限武器；没有符合角色时仍显示成功，但人数为 0。 | 仅当前运行时角色。 |
| GM-03 | `unlockalllevels` | `UnlockAllLevels`（大小写不敏感）、`UnlockAll`、`Unlock All` | 解锁全部关卡 `1..18`，将最高已解锁关卡设为 `MutinySaveSystem.MaxLevel`；成功后加入最近命令按钮。 | 写入 `MutinySaveSystem`；跨场景和重启保留，直到被重置。 |
| GM-04 | `ResetLevels` | `ResetProgress`、`LockAll` | 仅删除最高已解锁关卡的存档键，读取时恢复默认值 1；不会删除已完成分数或音频设置。 | 影响 `MutinySaveSystem` 的关卡进度；跨场景和重启保留。 |
| GM-05 | `Help` | `?` | 在 GM 面板显示当前可用命令的简表。 | 无状态修改。 |
| GM-07 | `aiforceusewaepon {weaponid}` | 无；命令拼写按测试约定保留 `waepon` | `1..15`：所有 AI 队伍的角色在 AI 决策中仅将对应编号武器视为无限可用；跳跃仍参与竞争，跳跃后仍可放弃开火。`0`：关闭覆盖，恢复实际库存候选。 | 当前运行会话；不改角色真实库存与存档。 |
| GM-08 | `aitakeoverwithluck {luck}` | 大小写不敏感；`luck` 使用英文小数点 | AI 以指定 Luck 接管当前人类队伍这一回合的剩余行动，允许跳跃前、跳跃飞行中及落地后使用；完整回合结束自动恢复人类控制。 | 仅本回合 AI 决策；不改变角色原始 Luck、未来回合、真实库存或存档。 |
| GM-09 | `setlanguage zh-cn` / `setlanguage en` | 大小写不敏感；允许多个参数分隔空白 | `zh-cn` 选择简体中文（内部 Locale `zh-Hans`），`en` 选择英语；复用本地化服务刷新当前页面。缺参、旧 `cn`、未知代码及额外参数拒绝，保留原语言。 | 单独语言偏好，跨场景及重启保留；不影响进度和成绩。 |
| GM-10 | `ailog 1` / `ailog 0` | 大小写不敏感 | 开启／关闭每次 AI 实际行动的一条 `[Mutiny:AI-Action]` 详细日志；含行动参数、胜出分数、各行动类别最佳分和选择原因。 | 当前运行会话；切关保留，新 Play／重启默认关闭；不写存档。 |

`GM-06` 尚未分配给命令。旧回归中的 `GM-02` 至 `GM-06` 字样是断言标题，分别检查 GM-01 的武器效果和按钮几何，并非同名命令 ID；新增命令不得据此复用现有 ID。

## GM-10 · AI 行动决策日志

输入 `ailog 1` 开启，`ailog 0` 关闭；只接受恰好一个 `0` 或 `1` 参数。日志直接出现在 Unity Console 和玩家版本的 `Player.log`，以 `[Mutiny:AI-Action]` 开头。每次**实际提交**的 AI 跳跃、射击、主动 Pass 各打印一条；镜头等待中取消的动作不打印。日志写出队伍、决策序号／随机种子、首次或续行动阶段、候选数、角色、武器、有效 Luck、该动作分数和适用的速度／预测位置／专用参数，并列出跳跃与每种武器的最高候选分及完整候选 JSON 路径（保存成功时）。首次行动即使最高分为负仍会执行；续行动最佳分不大于 0 则 Pass，日志显示被拒绝的最高分与原因。评分是原版风格的候选评分，不是真实伤害预言。

关闭后只停止这条详细日志；已有普通 AI 运行日志和 JSON 决策轨迹各有自己的行为，不受 `ailog` 控制。缺参、`2`、负值、非数字、额外参数报错且保持开关原值。此项是 Unity 调试扩展，原版来源不适用。隔离 Play Mode 的 GM 解析 39/39（含 3 条新断言）及实际 AI 协程 5/5 已通过；范围和未验收项见 [GM-10 规格](04-Logging/AI_ACTION_LOG_SPEC.md)。

## GM-09 · 切换语言

手动语言切换入口仅保留 GM 命令；主标题页不再显示语言选择按钮。

原版来源：不适用，用户授权的 Unity 本地化和 GM 扩展。菜单及战斗均可输入 `setlanguage zh-cn` 或 `setlanguage en`，经唯一生产入口 `MutinyGMManager.ExecuteCommand` 调用 `MutinyLocalization.Initialize/Select`，刷新所接入的文本和字体路径。内部语言代码仍为 `zh-Hans` / `en`；运行时 IMGUI 从随包双语文本读取，Unity String Table 保留作编辑器资源和内容核验。Windows 等平台沿用 `mutiny_language_v1`，Android 将语言偏好写入不备份的安装局部文件，避免卸载重装时恢复旧选择；游戏进度存档仍使用原路径。合法命令计入最近成功命令，可重放；再次选择当前语言同样成功并保存。非法输入显示 `Usage: setlanguage zh-cn | en`，不改语言、偏好和成功历史。GM 面板自身保持开发英语。

验收用例：从菜单实际 GM 解析入口依次执行 `zh-cn → en → zh-cn → en → zh-cn`，检查每步本地化文本、内部语言代码、字体路径和保存值；执行大小写/空白变化及重复命令；分别拒绝缺参、旧 `cn`、`jp`、额外参数；通过最近成功命令再次切换。2026-09-26 对旧 `cn` 命令的隔离 Play Mode **7/7** 仅为历史结果；新命令和 Android 回切待用户运行验收，见 [Android 修复规格](../09-PresentationAndFeedback/LOCALIZATION_ANDROID_FIX.md)。

## GM-08 · 当前玩家单回合 AI 接管

原版来源：不适用，用户授权的 Unity 调试扩展。唯一有效命令为 `aitakeoverwithluck {luck}`，例如 `aitakeoverwithluck 8` 或 `aitakeoverwithluck 7.5`。参数是本回合 AI 的 Luck，接受不含 NaN/Infinity 的 `0..100` 有限数字（小数使用英文 `.`）。旧命令 `aitakeover 1` 被取消，必须报错而不接管；缺参、额外参数、负值、超过 100、非数字也拒绝且不改变状态。

前态为当前存活的人类队伍的 `TurnActive`，或其已选角色跳跃后仍可射击的 `ActionExecuting/Settling`：临时改为 AI 控制，通过现有候选、评分、镜头与正式武器执行入口继续本回合；不调用 `StartTurn`，不重置资格，不赠送武器。跳跃前允许全队候选；跳跃后只能为原已选角色评价武器，保持通常正分/Pass 规则。飞行途中接受命令但等待稳定后再思考。已提交的武器序列、AI 原生回合、无战斗和重复接管均拒绝。接管开始清理尚未提交的玩家武器/瞄准和光标，阻止手动行动，但保留已提交的跳跃。

接管时不覆写角色的 `Luck` 字段，而给该 AI 控制器设置只在本次接管有效的 Luck。普通武器、Anchor 和 Cannon 的采样数按 `floor(指定 Luck × 当前队伍角色总数 / 存活人数)` 计算；固定 50 次的跳跃、海啸及其他专用候选维持原规则。首次行动与跳跃后续均使用同一指定值。接管持续到完整回合结束（包括 AI 跳跃后的射击阶段和武器后续效果）；换队、GameOver、重新初始化或管理器销毁时清除覆盖并恢复人类控制，下一轮不继承。与 GM-07 同用时，接管期间同样服从当前 AI 强制武器设置。Unity 入口：`MutinyGMManager.ExecuteCommand` → `MutinyTurnManager.TryTakeOverCurrentPlayerTurn` → `MutinyAIController`。接管期间队伍的 `IsAiControlled` 临时为真，现有输入、镜头和武器内部 AI 分支均复用；回合结束恢复为假。新增或复用的 AI 组件留在队伍上，恢复人类控制后不求值。

2026-09-27：新 `RunGM()` 在隔离 Unity 6000.6.0f1 Play Mode 通过 **35/35** 断言，经正式 GM 解析核对旧命令拒绝、参数边界、`7.5 → 7` 和 `9.25 → 9` 的实际候选采样、首次/续行动相同覆盖、原角色 Luck 不变、换队/重开/GameOver 清理及下一轮恢复原 Luck；见 [新命令验证记录](05-GMTools/Artifacts/GM-08-LUCK-20260927.txt)。历史结果（已被本次命令替换）：2026-09-26，旧 `aitakeover 1` 曾有 24/24 条断言通过、实际协程 2/2 通过；这些结果不能作为新语法或 Luck 覆盖的通过证据。

同日新命令的实际 AI 协程场景 **2/2** 通过：未跳跃时指定 `7.5` 自动完成行动、玩家跳跃飞行中指定 `9.25` 后自动同角色开火，均保持原角色 Luck 并在完整回合结束交还控制。使用强制海啸稳定胜出以检验时序；不把该场景当成所有武器轨迹的验证。

## GM-07 · 强制 AI 武器候选

原版来源：无，此项是用户授权的 Unity 调试扩展。Unity 入口为 `MutinyGMManager.ExecuteCommand` → `MutinyAIController` 的正式候选评估、动作选择和执行路径。

编号按武器菜单的 15 项顺序：`1 cherryBomb`、`2 boulder`、`3 dynamite`、`4 piecesOfEight`、`5 rumBottle`、`6 banana`、`7 parachuteBomb`、`8 woodenCrate`、`9 gunpowderBarrel`、`10 seagull`、`11 mine`、`12 cannon`、`13 anchor`、`14 voodooDoll`、`15 tidalWave`。`cannonball` 只是大炮内部别名，不另占编号。

输入示例：`aiforceusewaepon 1`。覆盖只改变 AI 的有效候选及 AI 发射时的扣弹行为，既不向 `WeaponInventory` 或 `InfiniteWeapons` 写值，也不给玩家控制的角色解锁武器；AI 角色仍须满足 `CanShoot` 等通常行动资格。AI 仍通过原本的评分选择跳跃或武器；跳跃后的续行动阶段若最佳武器收益不大于零，仍会放弃开火。输入 `aiforceusewaepon 0` 后，后续决策重新使用真实库存，原有弹药数不变。编号缺失、超出 `0..15` 或非整数时显示错误且保持原设置。已开始的决策使用开始时的覆盖快照；已选动作执行时保持当次快照的无限弹药语义。静态覆盖在新一轮 Play 启动时重置为 `0`，切场景不会重置。

验收：通过 GM 正式解析入口设置 `1`；无樱桃炸弹库存的 AI 也产生且只产生樱桃炸弹武器候选，真实库存不变；首行动的跳跃候选仍存在，续行动没有合格武器候选时仍 Pass；执行不扣真实弹药；设 `0` 后真实库存候选恢复；非法输入不改变覆盖。非正收益 Pass 分支由既有 AI 续行动规则维持，GM-07 专项用例未单独构造“有候选但评分非正”的局面。

## `UnlockWeapons` 的目标选择

没有指定全队后缀时，GM-01 按以下顺序找到一个存活角色：

1. 当前回合已选角色。
2. Team 1 已选角色。
3. 当前队伍的第一个存活角色。
4. Team 1 的第一个存活角色。
5. 场景中标记为 selected 的角色。
6. 场景中 Team 1 的第一个存活角色。
7. 场景中任意第一个存活角色。

若目标属于当前回合队伍，命令会同步选中它，以便立即从行动菜单使用武器。这个优先级也可能在 AI 回合选中 AI 角色，`UnlockWeapons` 并不保证只作用于玩家。没有存活角色时，控制台显示错误，不会修改存档。`UnlockWeaponsAllTeam` 不会切换当前选中角色。

## 武器集合

`UnlockWeapons` 调用 `MutinyCharacter.UnlockAllWeapons(infinite: true)`。其可选武器为：

`cherryBomb`、`boulder`、`dynamite`、`piecesOfEight`、`rumBottle`、`banana`、`parachuteBomb`、`woodenCrate`、`gunpowderBarrel`、`seagull`、`mine`、`cannon`、`anchor`、`voodooDoll`、`tidalWave`。

`cannonball` 同时被注册为大炮的内部库存别名，因此可直接使用大炮，不把它当作第 16 种独立的菜单武器。

## 维护规则

- 命令的唯一生产入口是 `MutinyGMManager.ExecuteCommand`；不要在 UI 按钮中另写一套分支。
- 新命令先分配 `GM-xx` ID，记录输入、目标、状态变化、存档影响和失败行为。
- 用户授权的调试命令与 Flash 原版规则分开记录。
- 修改已存在命令的别名或语义时，保留旧别名，除非有明确的兼容性移除需求。

## 验证入口

当前验证：2026-09-27 新命令 `RunGM()` 在隔离 Unity 6000.6.0f1 Play Mode 通过 **35/35**，实际协程 **2/2**；见 [GM-08 指定 Luck 验证](05-GMTools/Artifacts/GM-08-LUCK-20260927.txt)。历史验证：2026-09-26 旧 `aitakeover 1` 曾通过 24/24（更早为 12/12），旧结果见 [GM-08 历史记录](05-GMTools/Artifacts/GM-08-20260926.txt)，不作为新语法依据。

另有实际 AI 协程场景 **2/2** 通过：普通接管自动射击并换队恢复；玩家真实跳跃途中接管，自动进入同角色武器阶段并在 GameOver 恢复。此项自然运行 AI、物理和回合的 `Update`，没有直接调用 AI 求值或执行测试入口；仍不等于真实关卡鼠标/手机操作已验收。

GM 的生产回归位于 [MutinyTurnActionUiVerificationTest.cs](../../Assets/Mutiny/Scripts/Verification/MutinyTurnActionUiVerificationTest.cs)。在 Unity Play Mode 下，菜单 `Mutiny → Parity → Validate GM Commands` 调用 `RunGM()`；最新隔离工程结果为 **35/35**，覆盖 GM-08 新命令、GM-07、最近成功命令、关卡解锁及当前其他 GM 专项。完整 `Validate Turn Action UI` 才额外执行 GM-01 武器效果与按钮几何的旧断言。GM-03 的重启后实际选关 UI、GM-02/04/05 的完整命令效果、面板实际点击/键盘交互及 GM-01 完整目标优先级仍待验收。详见 [GM 工具规则与验收矩阵](05-GMTools/README.md)。
