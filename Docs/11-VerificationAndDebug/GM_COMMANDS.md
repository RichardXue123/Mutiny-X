# Mutiny GM 命令说明书

状态：当前 Unity 调试扩展。它不属于 Flash 原版行为，不应作为原版一致性结论。

## 使用方式

进入游戏后，屏幕左侧可见画布的垂直中央会有圆形 `GM` 按钮；该按钮也会显示在菜单等非战斗场景。打开后，面板左侧选择关卡、AI、武器、镜头或系统分类；右侧命令按钮只把命令模板填入输入框，并将光标移到末尾。带参数的按钮会在命令词后预填恰好一个空格；比如点击 `enterlevel {level_id}` 会填入 `enterlevel `，仍需输入编号并按 Enter 或点击 `Run` 执行。悬停按钮可看与 `Help` 相同的中文详细说明；触摸选择后中文说明留在按钮区下方。命令输出独立显示在输入框下方，长内容可滚动。再次点击 `GM` 或点击面板 `X` 可关闭。

五个最近成功命令按钮已移除。英文命令不区分大小写，输入会先去除首尾空格；空输入不会执行。命令词及参数之间沿用各命令现有的空白规则；易读简写只匹配完整命令词。分类、模板和验证状态见 [GM 控制台分类规格](05-GMTools/GM_CONSOLE_REDESIGN.md)。

命令由 [MutinyGMManager.cs](../../Assets/Mutiny/Scripts/Presentation/MutinyGMManager.cs) 的 `ExecuteCommand` 解释。控制台按钮与 `Help` 共用命令定义；新增或修改命令时，必须同时更新定义、解析、本说明书及 GM 生产入口回归用例。

## 命令总览

| ID | 推荐命令 | 可用别名 | 效果 | 持久化范围 |
| --- | --- | --- | --- | --- |
| GM-01 | `UnlockWeapons` | `UnlockAllWeapons`、`InfiniteWeapons`、`AllWeapons`、`Weapons`、`解锁武器`、`无限武器`，以及以 `UnlockWeapon` 或 `InfiniteWeapon` 开头的英文输入 | 给目标角色开启 15 种可选武器的无限弹药；大炮所需的 `cannonball` 内部别名也同时可用，并恢复该角色的武器行动资格。 | 仅当前运行时角色；切场景、重开或重启游戏后不保留。 |
| GM-02 | `UnlockWeaponsAllTeam` | `unlockteamweapons`；原有全队武器别名继续可用 | 给场景中所有存活的 Team 1 角色开启 GM-01 的无限武器；没有符合角色时仍显示成功，但人数为 0。 | 仅当前运行时角色。 |
| GM-03 | `unlockalllevels` | `unlocklevels`、`UnlockAll`、`Unlock All` | 解锁全部关卡 `1..18`，将最高已解锁关卡设为 `MutinySaveSystem.MaxLevel`。 | 写入 `MutinySaveSystem`；跨场景和重启保留，直到被重置。 |
| GM-04 | `ResetLevels` | `ResetProgress`、`LockAll` | 仅删除最高已解锁关卡的存档键，读取时恢复默认值 1；不会删除已完成分数或音频设置。 | 影响 `MutinySaveSystem` 的关卡进度；跨场景和重启保留。 |
| GM-05 | `Help` | `?` | 在 GM 面板显示当前可用命令、简写及中文详细说明。 | 无状态修改。 |
| GM-07 | `aiweapon {weaponid}` | `aiforceusewaepon`、`aiforceuseweapon` 均保留兼容 | `1..15`：所有 AI 队伍的角色在 AI 决策中仅将对应编号武器视为无限可用；跳跃仍参与竞争，跳跃后仍可放弃开火。`0`：关闭覆盖，恢复实际库存候选。 | 当前运行会话；不改角色真实库存与存档。 |
| GM-08 | `aitakeover {luck}` | `aitakeoverwithluck {luck}`；范围 `0..99999`，允许英文小数点 | AI 以指定 Luck 接管当前人类队伍这一回合的剩余行动，允许跳跃前、跳跃飞行中及落地后使用；完整回合结束自动恢复人类控制。 | 仅本回合 AI 决策；不改变角色原始 Luck、未来回合、真实库存或存档。 |
| GM-09 | `lang en` / `lang zh-cn` / `lang zh-hk` | 大小写不敏感；空白分隔参数；`setlanguage` 为兼容别名 | 英文 / 简体中文 / 香港繁体中文；内部代码为 `en` / `zh-Hans` / `zh-HK`，即时刷新文本与地区字体。缺参、旧 `cn`、未知代码及额外参数拒绝，保留原语言。 | 单独语言偏好，跨场景及重启保留；不影响进度和成绩。 |
| GM-10 | `ailog 1` / `ailog 0` | 大小写不敏感 | 开启／关闭每次 AI 实际行动的一条 `[Mutiny:AI-Action]` 详细日志；含行动参数、胜出分数、各行动类别最佳分和选择原因。 | 当前运行会话；切关保留，新 Play／重启默认关闭；不写存档。 |
| GM-11 | `excamera 1` / `excamera` | `blastcam 1/0` 或无参；`excamera 0` 也关闭 | 可选爆炸击退运镜：同帧最近被炸飞者优先，一批只选一人，不接力；关闭立即释放。 | 当前运行会话；切关保留，新 Play／重启默认关闭；不写存档。 |
| GM-12 | `aisetluck {luck}` | `ailuck {luck}`；范围 `0..99999`，允许英文小数点 | 统一覆盖当前单人关卡敌方 AI 的决策 Luck，`0` 是有效覆盖。 | 当前关卡；重开/下一关不继承，不改角色原始 Luck/XML/存档。 |
| GM-13 | `airesetluck` | 大小写不敏感；不带参数 | 清除当前单人关卡覆盖，恢复每名敌方角色各自的默认 Luck。 | 只影响当前关卡敌方，不影响玩家单回合接管。 |
| GM-14 | `aienhance 1` / `aienhance 0` | 大小写不敏感；恰好一个 `0/1` 参数 | 开启 effects-v1 完整效果模拟初版／恢复原兼容策略；增强评分以模拟后双方 HP 为主。 | 会话设置；跨关卡保留，新 Play／重启默认关闭，不写存档，不自动接管人类。 |
| GM-15 | `enterlevel 16` | `level 16`；`enterlevel 1_16`、`enterlevel level_1_16`；`enterlevel 2_01` / `level_2_01` 指定双人 | 绕过菜单上限和解锁直接进入指定关卡；裸数字始终为单人。仅接受一个正整数或明确模式编号，资源必须存在。 | 替换当前对局；新单人会话分数重置，不修改存档解锁和成绩。 |

`GM-06` 尚未分配给命令。旧回归中的 `GM-02` 至 `GM-06` 字样是断言标题，分别检查 GM-01 的武器效果和按钮几何，并非同名命令 ID；新增命令不得据此复用现有 ID。

## GM-15 · 直接进入关卡

`enterlevel 1`–`enterlevel 15` 进入现有单人关；`enterlevel 16` 进入新的临时单人关，2026-10-01 已更新为 [115×36 格三舰布局初稿](../02-LevelAndWorld/04-ObjectsAndSpawns/LEVEL_1_16_SPACE_DRAFT.md)，玩家10人对机器人11人。完整身份格式为 `level_1_XX`（单人）和 `level_2_XX`（双人），允许省略 `level_`。双人局部编号 01–18 对应原版全局编号 16–33；例如 `enterlevel 2_01` 进入原版双人第 16 关，`enterlevel 2_16` 进入原版第 31 关，不会与单人 16 冲突。

命令在标题、选关、结局及战斗页均可使用；大小写不敏感，允许参数间多个空白。成功经前端正式入口加载角色、AI、镜头和战斗音乐。切关时立即停用旧根，重开保留模式与编号。缺参、多参、非正整数、未知模式、溢出或资源不存在均失败，不替换对局、不切换页面/模式、不清空分数；过场进行中拒绝并提示等待。直接进入不解锁关卡或写入已完成成绩，后续真实胜负结算仍使用现有规则。

本轮不增加单人选关按钮、不迁移第 15 关结局、不改变存档规格。太空主题、低重力、机器人和新音频尚未实现。规格与实际验收状态见 [EXT-LVL-ID-01..03 / GM-15](../02-LevelAndWorld/01-LevelDataParsing/LEVEL_IDENTITY_AND_GM_ENTRY.md)。

## GM-14 · AI 策略开关

`aienhance 1` 使用 `enhanced-effects-v1`，实际算法为 `effects-v1`、`fallback=False`；`aienhance 0` 恢复原兼容算法。增强初版包含 15 种武器的作用过程、爆炸击退后的落水、桶连锁、朗姆酒平台火焰、八枚金币和气球固定扇风控制。它是有界近似模型，不是逐帧权威副本或全局最优解；朗姆酒的随机击飞尤其可能改变落水结果。

增强 Luck 仍接受 `0..99999`，普通武器随机粗筛次数按原 Luck/存活比例计算后限制到 `4..128`；最多 256 次完整模拟，每次最多 2048 tick，不完整的试验排除。默认沿用每帧 3 ms 软预算，不能把 99999 理解为增强模式穷举 99999 次。每名可行动角色的武器候选按轮次交错精算。主分为敌方损失 HP 减 1.15 倍友方损失，再加击杀/胜负、库存消耗及小幅移动分；首次与续行动均可在无正收益时 Pass。

只接受一个精确的 `0/1` 参数；缺参、额外参数、负值、`2`、非数字失败，不改变模式。重复设置相同值成功但不使正在运行的搜索失效。切关/重开保留模式，重启默认关闭；不写存档、不改 XML、库存、行动资格和角色 Luck，也不会自动接管人类。已是 AI 的队伍和 `aitakeover` 接管统一使用当前路由；强制武器、Luck、日志开关保持独立。

切换实际改变时使未提交搜索和镜头等待中的旧结果失效，随后重新求值；即使在两帧间开→关，也不可恢复旧版本。已经提交的跳跃/发射不撤销；连续武器保留提交时的策略上下文直到结束，下一独立决策采集最新设置。`ailog 1` 的实际行动行显示 `mode`、`strategy`、`algorithm`、`fallback` 与 `strategyVersion`。

开关历史规格见 [GM-14 / EXT-AI-STRAT-01..05](../08-ActionAgents/05-AIWeaponSelection/AI_STRATEGY_BOUNDARY_20260929.md)；现行算法、具体武器、生产入口回归和近似边界见 [EXT-AI-FX-01..10](../08-ActionAgents/05-AIWeaponSelection/AI_ENHANCED_SIMULATION_V1.md)。

## GM-11 · 爆炸击退运镜

原版来源：不适用，用户授权的可选镜头扩展。`excamera 1` 开启；不带参数的 `excamera` 关闭，兼容 `excamera 0` 和易读别名 `blastcam`。恰好零个参数或一个 `0/1` 参数才合法，其他参数拒绝并保持开关不变。关闭同步清空已选目标与待选候选；开启不追溯此前已飞行的角色。默认关闭，跨关卡保留开关，新 Play／重启关闭，无存档写入。

只由实际爆炸的非零击退触发。下一显示帧按角色显示坐标到镜头中心的距离选最近一人，同帧多个爆炸也一起比较；选中后不因其他人更近而换人。目标静止、落水、销毁或物理停用即结束跟随，该批其他人仍移动时不会接力，连锁爆炸也不抢换；整批停止后的新爆炸可以重新选人。沿用现有高帧率插值与跟随速度，开启时优先于武器及空投。完整规格与验收结果见 [EXT-EXCAM-01..04](../09-PresentationAndFeedback/04-Camera/README.md)。

生产链路：`MutinyGMManager.ExecuteCommand` → `MutinyCameraController.SetExplosionCameraEnabled`；`MutinyExplosion.ApplyHit` → 批击退事件 → `MutinyCameraController.AdvanceCamera`。验证入口为 `Mutiny → Parity → Validate Explosion Camera Play Mode`，GM 解析检查同时接入 `Validate GM Commands`。2026-09-27 隔离 Unity 6000.6.0f1 Play Mode 专项 29/29 通过，含 6 条 GM 正式解析/历史/初始化检查；编译通过。主工程实际 GM 按钮与键盘、完整对局及 Android 真机待验收。

## GM-10 · AI 行动决策日志

输入 `ailog 1` 开启，`ailog 0` 关闭；只接受恰好一个 `0` 或 `1` 参数。日志直接出现在 Unity Console 和玩家版本的 `Player.log`，以 `[Mutiny:AI-Action]` 开头。每次**实际提交**的 AI 跳跃、射击、主动 Pass 各打印一条；镜头等待中取消的动作不打印。日志写出队伍、决策序号／随机种子、首次或续行动阶段、候选数、角色、武器、有效 Luck、该动作分数和适用的速度／预测位置／专用参数，并列出跳跃与每种武器的最高候选分及完整候选 JSON 路径；分块后台保存的文件状态为 `traceStatus=pending/ready/failed`，`ready` 后才可重放。Legacy 首次行动即使最高分为负仍会执行，续行动最佳分不大于 0 则 Pass；增强模式首次也可 Pass。日志显示被拒绝的最高分与原因。

增强行动额外记录 `coarseCandidates/fullSimulations/truncated/budgetSkipped`、双方模拟 HP 前后、伤害/击杀/终局/资源/位置分项、`simulationSeed`、结算 tick、扇风方向、八枚金币的控制计划及 `modelLimits`。在启用决策文件保存时，完整精算详情写到原 trace 路径追加 `.enhanced.json` 的独立 sidecar；原 Legacy JSON 字段不变。sidecar 的写盘是异步的，不以原 trace 的 `ready` 保证 sidecar 同时存在。所有分数均是预测，不是真实伤害预言。

关闭后只停止这条详细日志；已有普通 AI 运行日志和 JSON 决策轨迹各有自己的行为，不受 `ailog` 控制。缺参、`2`、负值、非数字、额外参数报错且保持开关原值。此项是 Unity 调试扩展，原版来源不适用。隔离 Play Mode 的 GM 解析 39/39（含 3 条新断言）及实际 AI 协程 5/5 已通过；范围和未验收项见 [GM-10 规格](04-Logging/AI_ACTION_LOG_SPEC.md)。

## GM-09 · 切换语言

手动语言切换入口仅保留 GM 命令；主标题页不再显示语言选择按钮。

原版来源：不适用，用户授权的 Unity 本地化和 GM 扩展。菜单及战斗均可输入 `lang en`、`lang zh-cn` 或 `lang zh-hk`，经唯一生产入口 `MutinyGMManager.ExecuteCommand` 调用 `MutinyLocalization.Initialize/Select`，刷新所接入的文本和字体路径；旧 `setlanguage` 兼容同样的三个参数。内部代码为 `en` / `zh-Hans` / `zh-HK`，现有保存的简中代码仍可读；运行时 IMGUI 从随包三语文本读取，Unity String Table 保留作编辑器资源和内容核验。英语使用原位图字库，简中和香港繁中分别使用 SC/HK 地区动态字体。无保存值时默认英语。Windows 等平台沿用 `mutiny_language_v1`，Android 将语言偏好写入不备份的安装局部文件，避免卸载重装时恢复旧选择；游戏进度存档仍使用原路径。再次选择当前语言同样成功并保存。非法输入显示 `Usage: lang en | zh-cn | zh-hk`，不改语言和偏好。GM 面板分类 Tab、悬停详细说明和 `Help` 描述使用中文；命令词保留英文。

验收用例：从实际 GM 入口依次执行 `lang en → lang zh-cn → lang zh-hk → lang en → lang zh-hk → lang zh-cn`，检查每步菜单/对白/悬停、内部代码、地区字体和保存值；执行大小写/空白变化、重复命令和兼容别名；分别拒绝缺参、旧 `cn`、未知地区及额外参数；重启核对保存。旧 `cn`/双语言验收为历史结果，不证明当前三语言通过；本轮运行验收由用户执行，见 [香港繁体中文规格](../09-PresentationAndFeedback/LOCALIZATION_HONG_KONG.md)。

## GM-08 · 当前玩家单回合 AI 接管

原版来源：不适用，用户授权的 Unity 调试扩展。当前推荐 `aitakeover {luck}`，保留 `aitakeoverwithluck {luck}` 别名，例如 `aitakeover 10000` 或 `aitakeover 7.5`。参数是本回合 AI 的 Luck，接受不含 NaN/Infinity 的 `0..99999` 有限数字（小数使用英文 `.`）。`aitakeover 1` 现在表示以 Luck=1 接管，而非无参数开关。缺参、额外参数、负值、超过 99999、非数字拒绝且不改变状态。

前态为当前存活的人类队伍的 `TurnActive`，或其已选角色跳跃后仍可射击的 `ActionExecuting/Settling`：临时改为 AI 控制，通过现有候选、评分、镜头与正式武器执行入口继续本回合；不调用 `StartTurn`，不重置资格，不赠送武器。跳跃前允许全队候选；跳跃后只能为原已选角色评价武器，保持通常正分/Pass 规则。飞行途中接受命令但等待稳定后再思考。已提交的武器序列、AI 原生回合、无战斗和重复接管均拒绝。接管开始清理尚未提交的玩家武器/瞄准和光标，阻止手动行动，但保留已提交的跳跃。

接管时不覆写角色的 `Luck` 字段，而给该 AI 控制器设置只在本次接管有效的 Luck。普通武器、Anchor 和 Cannon 的采样数按 `floor(指定 Luck × 当前队伍角色总数 / 存活人数)` 计算；固定 50 次的跳跃、海啸及其他专用候选维持原规则。首次行动与跳跃后续均使用同一指定值。接管持续到完整回合结束（包括 AI 跳跃后的射击阶段和武器后续效果）；换队、GameOver、重新初始化或管理器销毁时清除覆盖并恢复人类控制，下一轮不继承。与 GM-07 同用时，接管期间同样服从当前 AI 强制武器设置。Unity 入口：`MutinyGMManager.ExecuteCommand` → `MutinyTurnManager.TryTakeOverCurrentPlayerTurn` → `MutinyAIController`。接管期间队伍的 `IsAiControlled` 临时为真，现有输入、镜头和武器内部 AI 分支均复用；回合结束恢复为假。新增或复用的 AI 组件留在队伍上，恢复人类控制后不求值。

历史验证：2026-09-27，旧 `0..100` 规格的 `RunGM()` 在隔离 Unity 6000.6.0f1 Play Mode 通过 **35/35** 断言；见 [旧范围验证记录](05-GMTools/Artifacts/GM-08-LUCK-20260927.txt)。2026-09-26，旧无指定 Luck 的 `aitakeover 1` 曾有 24/24 条断言通过、实际协程 2/2 通过；这些结果不能作为当前扩大范围与新增命令的通过证据。当前结果见 [Luck 扩展规格与验收](05-GMTools/AI_LUCK_COMMANDS_SPEC.md)。

## GM-12 / GM-13 · 当前单人关卡 AI Luck

`aisetluck 10000`（或 `ailuck 10000`）将当前单人关卡所有敌方 AI 的有效 Luck 设为 10000；`airesetluck` 恢复每名敌人关卡自带的默认值。只作用当前关卡，重开/换关自动回到默认。不改玩家 Luck；与单回合接管各自独立。缺少参数、多余参数、非有限值、越界、无关卡及双人模式均拒绝。

本次决策开始时固定 Luck，跳跃后的武器续行动属于新的决策。2026-09-28 非阻塞搜索扩展：思考中改变 Luck/强制武器会丢弃旧快照，并重新开始一次决策使用新值，不提交旧参数结果。Luck 增加普通武器/Anchor/Cannon 的采样数量，不改变固定跳跃候选与评分公式。99999 是可输入上限，不保证最优解；生产搜索默认约 3 ms/tick 分帧，极高值增加总等待时间。专项 37/37、高 Luck 10000/99999 真实续行动通过，全武器/真机性能仍待验收；见 [非阻塞搜索规格与实测](../08-ActionAgents/05-AIWeaponSelection/AI_RESPONSIVE_SEARCH_20260928.md)。

同日新命令的实际 AI 协程场景 **2/2** 通过：未跳跃时指定 `7.5` 自动完成行动、玩家跳跃飞行中指定 `9.25` 后自动同角色开火，均保持原角色 Luck 并在完整回合结束交还控制。使用强制海啸稳定胜出以检验时序；不把该场景当成所有武器轨迹的验证。

## GM-07 · 强制 AI 武器候选

原版来源：无，此项是用户授权的 Unity 调试扩展。Unity 入口为 `MutinyGMManager.ExecuteCommand` → `MutinyAIController` 的正式候选评估、动作选择和执行路径。

编号按武器菜单的 15 项顺序：`1 cherryBomb`、`2 boulder`、`3 dynamite`、`4 piecesOfEight`、`5 rumBottle`、`6 banana`、`7 parachuteBomb`、`8 woodenCrate`、`9 gunpowderBarrel`、`10 seagull`、`11 mine`、`12 cannon`、`13 anchor`、`14 voodooDoll`、`15 tidalWave`。`cannonball` 只是大炮内部别名，不另占编号。

输入示例：`aiforceusewaepon 1`。覆盖只改变 AI 的有效候选及 AI 发射时的扣弹行为，既不向 `WeaponInventory` 或 `InfiniteWeapons` 写值，也不给玩家控制的角色解锁武器；AI 角色仍须满足 `CanShoot` 等通常行动资格。AI 仍通过原本的评分选择跳跃或武器；跳跃后的续行动阶段若最佳武器收益不大于零，仍会放弃开火。输入 `aiforceusewaepon 0` 后，后续决策重新使用真实库存，原有弹药数不变。编号缺失、超出 `0..15` 或非整数时显示错误且保持原设置。已开始的决策使用开始时的覆盖快照；已选动作执行时保持当次快照的无限弹药语义。静态覆盖在新一轮 Play 启动时重置为 `0`，切场景不会重置。

2026-09-28 授权的快照失效扩展：若搜索或镜头等待期间改变强制武器，旧候选不再提交，重算使用新设置；有效快照仍保持上述无限弹药语义。

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

当前扩大范围及关卡 Luck 命令的结果见 [Luck 扩展规格与验收](05-GMTools/AI_LUCK_COMMANDS_SPEC.md)。历史验证：2026-09-27 旧 `0..100` 规格 `RunGM()` 在隔离 Unity 6000.6.0f1 Play Mode 通过 **35/35**，实际协程 **2/2**；见 [GM-08 旧范围验证](05-GMTools/Artifacts/GM-08-LUCK-20260927.txt)。2026-09-26 旧 `aitakeover 1` 曾通过 24/24（更早为 12/12），旧结果见 [GM-08 历史记录](05-GMTools/Artifacts/GM-08-20260926.txt)，不作为当前新范围/新命令依据。

另有实际 AI 协程场景 **2/2** 通过：普通接管自动射击并换队恢复；玩家真实跳跃途中接管，自动进入同角色武器阶段并在 GameOver 恢复。此项自然运行 AI、物理和回合的 `Update`，没有直接调用 AI 求值或执行测试入口；仍不等于真实关卡鼠标/手机操作已验收。

GM 的生产回归位于 [MutinyTurnActionUiVerificationTest.cs](../../Assets/Mutiny/Scripts/Verification/MutinyTurnActionUiVerificationTest.cs)。在 Unity Play Mode 下，菜单 `Mutiny → Parity → Validate GM Commands` 调用 `RunGM()`；2026-10-02 独立工程 GM **82/82**、关卡入口 **68/68**，连同关联回归总计 **236/236**。当前 C# 两个程序集已编译。回归包含参数按钮模板末尾恰好一个空格、14 条中文说明与 Help 共源及字体资源；分类面板的实际鼠标/触摸、焦点、滚动和输出布局，以及 GM-03 的重启后实际选关 UI、GM-02/04/05 的完整命令效果仍需运行验收。详见 [GM 控制台新规格](05-GMTools/GM_CONSOLE_REDESIGN.md) 与 [完整测试记录](05-GMTools/Artifacts/GM-CONSOLE-TEMPLATES-ZH-20261002.txt)。
