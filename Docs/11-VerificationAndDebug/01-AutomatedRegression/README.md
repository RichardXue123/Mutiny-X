# 11.01 · 自动回归

## 增强 AI 完整效果初版（2026-09-29）

入口：`Mutiny/Parity/Validate Enhanced AI Effects Play Mode`，必须在隔离空场景运行；批处理使用 `MutinyAiEnhancedVerificationMenu.RunBatch`。测试经过真实 GM、生产预算泵、武器工厂/发射、物理推进与效果回调，不靠修改 fired/finished/active 标志模拟成功。覆盖 15 武器、连续计划、现有雷与桶连锁、平台火焰、HP 评分、分帧预算、回放、取消及旧模式独立基线。

实际结果、受控 tick 时钟口径、Rum 随机差异及待实机项见 [EXT-AI-FX-01..10](../../08-ActionAgents/05-AIWeaponSelection/AI_ENHANCED_SIMULATION_V1.md)。该扩展不属于 Flash 原版一致性验收。

## 蓄力被炸飞时实时预览（2026-09-29）

沿用 `Validate Selected Death Settlement Play Mode` 的真实 Mine/物理夹具，新增 `AIM-MOVE-01/02` 四条断言：非致命炸飞后不再提供新的指针帧，生产 `PlayerInput.LateUpdate` 必须跨至少四个真实物理 tick 刷新拉线/首条虚线；由晚序测试观察器核对它们贴合生产插值显示姿态，并使用生产预测入口检查当前方向。再经正式 `ResolveAimRelease` 检查当前位置零拉距取消，以及重新拉线后空中提交的实际初速。未修改角色位置、存活/蓄力布尔量来替代真实爆炸，不重写发射公式。

本轮组合 **49/49**、既有覆盖层/取消 **32/32**、镜头 **11/11** 实际通过；完整证据和硬件边界见 [实时轨迹验证](../02-PlayModeValidation/AIM_MOVING_ORIGIN_20260929.md)。

## 蓄力期间选中角色致死结算（2026-09-29）

入口：`Mutiny/Parity/Validate Selected Death Settlement Play Mode`；在空场景运行 `MutinySelectedDeathVerificationRunner`。地雷在本回合开始前经正式 `Initialize/Fire` 存在，随后通过生产 `TurnManager.Initialize` 开始回合，实际选人、蓄力触发 Mine，正常 Update 推进倒计时、Explosion 命中/销毁、角色重力/地形和血条。死亡后不模拟鼠标释放也必须退出蓄力。身体与血条真正结算后只关闭回合管理器的自动计时，经正式 `AdvanceSimulationTick` 精确验证 inactivity <=10 不结束，第 11 tick 才切换。

用例覆盖 `TURN-DEATH-01..03`、`TURN-DEATH-INPUT-01` 和 `MIN-AIM-01`：按住死亡、死亡后旧 release、未发射武器不扣库存、不能替换死者、正常换队只触发一次、最后角色死亡转 GameOver、非致命炸飞空中起跳后继续武器行动、未选人不自动跳过、未选中队友死亡不误结束。原版死亡结算和用户确认的输入保护分别记录于行为规格。

本轮实际结果及早期夹具/隔离依赖问题记录于 [死亡结算验证](../02-PlayModeValidation/SELECTED_DEATH_SETTLEMENT_20260929.md)。完整主工程对局、鼠标硬件、Android/手柄真机和胜负对白触摸仍待验收，不因同源入口回归自动计为通过。

## 可选爆炸击退镜头（2026-09-27，授权扩展）

`Validate Explosion Camera Play Mode` 经生产 GM `excamera` 解析、爆炸 `ApplyHit` 批事件和镜头 `AdvanceCamera` 检查默认关闭、合法/非法参数、历史重放、关闭立即释放、最近角色、同帧多爆炸、60/120 FPS、显示姿态、武器/空投抢占、连锁不抢换及停止后不接力。落地由真实重力、地形和摩擦完成，另检验普通角色跟随与显式 `PanToCharacter` 两个接力旁路、全批结束后新爆炸、致死飞行、落水、停用、销毁、关卡重置、零力边界、普通跳跃不触发，以及 Cherry Bomb 实际创建爆炸的生产链。隔离 Unity 6000.6.0f1 Play Mode 29/29 通过；编译通过；完整真实关卡、GM UI 及 Android 真机待验收。

## 底部提示高刷新率回归（2026-09-27）

`Mutiny/Parity/Validate Bottom Notices Play Mode` 调用生产同源 `MutinyTurnManager.Initialize → OnTurnStarted → MutinyIngameTextArea.AdvanceOriginalTick/AdvancePresentationFrame`，并排入 `SayCollected("tidalWave")`。检查 speech 暂停、原版帧 1/9/10/59/60/70/71 的位置与 FIFO 切换、25/60/120 FPS 的首 tick 与帧间显示坐标，确认渲染插值不提前消费队列。Unity 6000.6.0f1 隔离工程 Play Mode 11/11 通过；同工程 `Validate Battle HUD` 广域回归 27/27 通过。IMGUI 真机画面与帧时间波动待验收。


## 海鸥投弹高刷新率回归（2026-09-26）

`Mutiny/Parity/Validate Seagull Presentation Play Mode` 调用 `RunSeagullPresentation()`，包含既有原版海鸥出生、点击排队、图层、时间轴、碰撞与落水测试，以及 `SEA-PRES-01/02` 的 25/60/120 FPS 显示扩展回归。经真实 `RequestShot → AdvanceSimulationFrame → OnSimulationStep → 子弹 AdvanceSimulationTick` 链路检查出生 tick 一次、首可见起点、半 tick 实际 Transform、父子相位、连续投放、自身 Update 不多推进、延迟 Start 不改写权威位置；最后比较实际碰撞和落水结束 tick 及爆炸数。2026-09-26 Unity 6000.6.0f1 隔离工程 Play Mode 31/31 断言通过；主工程完整投弹画面及 Android 真机未运行。

同一隔离工程同步最新生产代码，复跑 `Validate Camera Movement` 11/11 与 `Validate Pieces Of Eight Presentation Play Mode` 4/4 通过；未改这些既有用例的期望值。显示仍有与海鸥一致的一个已完成物理 tick 延迟，这是已授权高刷新率策略，不是原版新增物理规则。

## 关卡初始镜头回归（2026-09-26）

`Mutiny/Parity/Validate Camera Initialization Play Mode` 调用 `RunCameraInitialization()`。从任意旧位置经真实 `TryLoadLevel` 加载 1/7/13/30 关，在旧关仍待帧末销毁时检查新关回合/输入/speech 引用和旧角色/武器跟随释放；继续驱动 `RestartCurrentLevel`、`LoadNextLevel`、`ClearLevel → TryLoadLevel`。额外覆盖旧版烘焙根的真实 `Awake` 接管/`Start` 重置，以及 `StartGame → Update 同源 AdvanceSpeech → AdvanceCamera` 的 120 FPS 首帧 speech 平移。

Unity 6000.6.0f1 隔离工程 Play Mode 实际 13/13 断言通过；现有 `Validate Camera Movement` 同时复跑 11/11 通过。首轮未控制首帧时间，使用编辑器真实 `unscaledDeltaTime` 执行单次 Update 时“气泡未显示”检查失败；最终使用 Update 同源入口传入指定的 `1/120s`，未修改局部布尔状态或重写生产运镜公式。主工程画面及 Android 真机仍待验收。

[返回上级模块](../README.md)

## 职责

管理生产入口测试和缺陷回归用例。

## 边界

测试不得重写生产公式证明自身正确。

## BoxWeapon 缺陷回归

- `BOX-COL-01`：通过 `MutinyPhysicsBody.AdvanceSimulationTick` 驱动角色向上运动和 Cannonball 横向运动，验证共享箱体集合确实产生 Ceiling/Wall 接触与位置校正。
- `BOX-COL-02/CRT-BOX-02`：把已落地箱体与站在承托 tile 上的角色构造成爆炸/边角运动可能产生的少量重叠，再通过生产 `MutinyPhysicsBody.AdvanceSimulationTick()` 驱动向上运动；原始箱体候选会把角色中心改到地面内部，用例断言修复后仍产生 Ceiling 接触，但本轴保留 tick 起点且角色底边不进入 tile。
- `BOX-LVL-01`：在已注册箱体尚未由 Unity 延迟销毁时调用生产 `MutinyLevelBuilder.BuildLevel`，验证新关开始前同步清空集合。
- `BOX-PLC-01`：首个箱/桶放置后，用生产光标解析与生产点击入口检查同一原位置，二者都必须拒绝 pending 子对象与已放根对象重叠。
- `BOX-PLC-02`：在至少已放一件后提交重叠位置的非法输入，验证数量、共享注册表和 pending 子对象均不改变；同时断言“左键仍按住、但本帧没有新按下沿”不会重试摆放，只有后续新的合法点击才会继续。
- `BOX-SUP-01`：候选箱横跨两列、仅一列下方有承托 tile 时，生产 `CanPlace` 必须返回合法。
- `BOX-WAIT-01`：第一只木箱提交后，连续驱动生产 `MutinyTurnManager.AdvanceSimulationTick()` 超过安全阈值 150 tick；断言根箱未被强制 `Finish/Destroy`、pending 链仍存在，并可继续完成第二、第三箱。火药桶共享同一无超时规则。
- `BOX-CAM-01`：第一箱提交并进入 `ActionExecuting` 后，经生产镜头目标解析断言没有箱体跟随目标、没有角色回移门；`CanAcceptManualScrollingForVerification()` 必须为真，覆盖鼠标边缘、方向键和 WASD 共用的滚屏资格。
- `CRT-EXP-01/02`：让角色与第一只木箱同时落入生产 `MutinyExplosion.ApplyHit` 范围；断言同次命中已给角色向上速度、只移除命中箱的共享碰撞注册、木箱立即位于原版 frame 11。随后各推进一个角色物理 tick 和箱子时间轴 tick，断言角色已经飞离且箱子进入 frame 12；推进至 frame 18 时箱子隐藏。
- `LVL-PERSIST-01`：先经生产 `TryPlaceAt` 完成三只木箱、两只火药桶的放置链，并将地雷实际发射至 `IsStored`；再经生产 `MutinyTurnManager` 分别击败敌方和玩家方，断言 `GameOver`/结果弹窗时对象仍有效、箱体碰撞注册仍在。另保留未放置的箱体实例，检查结算不会误删它。
- `LVL-RESET-01`：上述旧物体仍存在时经 `MutinyLevelController.TryLoadLevel()` 初始化新关，断言旧箱体/地雷失活、共享箱体注册清空。
- 本轮 `LVL-PERSIST-01/LVL-RESET-01` 已加入 `MutinyTurnActionUiVerificationTest.RunLevelLifecycle()`；2026-09-26 在 Unity 6000.6.0f1 隔离工程 Play Mode 跑通最终 8/8 断言，含显式 `ClearLevel()` 卸载。其余 BoxWeapon 用例仍以各自的实际运行记录为准。

## 蓄力取消回归

- `CAM-AIM-EDGE-01`：跳跃/Cherry Bomb 经正式选人、选择行动、开始蓄力入口，在真实 Input System Mouse 按住时驱动生产镜头入口。四边 × 25/60/120 FPS 验证每显示帧滚屏、箭头、轨迹起点不漂移、蓄力资格/库存不变；鼠标回中央后减速停止且不自动回移，实际松手才提交一次并恢复跟随。另外经正式大炮选择与 pin/body 开始、释放入口，分别检验拉栓可滚屏、炮身拖动仍锁镜。2026-09-29 隔离 Unity 6000.6.0f1 `Validate Scroll Arrows Play Mode` 113/113 通过（新增 102、既有 11），既有镜头 11/11 通过；主工程 PIE/真机画面待验收。

- `WPN-CAN-PRESS-01`：武器/跳跃分别通过生产取消事件门校验“无新按下不取消、新按下才取消”；在正式蓄力入口开始后，按住移入叉不取消，通过生产松手入口在叉上释放仍发射/起跳，并核对库存或 `CanThrow` 的真实提交。2026-09-26 Unity 6000.6.0f1 隔离 Play Mode 复跑 `Validate Character Aim Overlay Play Mode`，31/31 通过：新增按下语义 6 条、覆盖层与更新后的触屏取消 25 条。主工程真实鼠标与安卓真机待验收。
- `CHAR-OVR-AIM-01`：通过生产选角色、选 Throw Self、按下开始蓄力及持续按住绘制轨迹的同源入口，检查 P1 标记、选择框、血条、取消叉同时可见，队友标记/血条不受影响；右键/叉取消后轨迹消失但选中角色 UI 保留；正常松开发射才设 `IsSelfThrown` 并隐藏四项 UI，生产行动延续再恢复标记、血条和选择框。2026-09-26 Unity 6000.6.0f1 隔离 Play Mode 执行 `Validate Character Aim Overlay Play Mode`，25/25 通过：本规则 7 条、既有覆盖层 13 条、触屏取消 5 条。主工程画面与安卓真机待验收。
- 原版来源更正：`Character` 构造设 `draggable=false、twangable=true`，`TileSystem.mouseDown` 进入 `Controller.twanging`；覆盖层的 `Controller.dragging` 隐藏门不适用于正常跳跃蓄力。此前取消专项仅证明叉的显示/操作，不曾验证 P1、选择框和血条；不能用其 5/5 结果证明旧覆盖层正确。
- `EXT-AIM-CAN-02`：武器/跳跃蓄力期间取消叉仍可见；第二指在叉外不取消、在叉内新按下取消后主指松开不补发射；右键仍仅退回待命。此次更新后的 5 条触屏回归包含在上述 31/31 中。此前 `Validate Aim Cancel Touch Play Mode` 的 5/5 曾包含错误的“主指在叉上松开取消跳跃”规格，现已撤销；该历史结果不能证明当前按下语义。尚未在 Android 真机验证物理多指事件及画面。
- 同日尝试运行既有 `Validate Weapon Ready And Cancel` 广域用例时，`WRDY-T01` 装备初始姿态/资源综合断言失败；新增取消断言未报失败。该旧断言原因未在本轮查明，不计入 `EXT-AIM-CAN-02` 的 5/5 专项结果。

## Cannon 范围锚点回归

- `ANC-PRES-01/02`：通过玩家正式点击、AI 正式执行及锚的生产显示帧推进入口，检查 25/60/120 FPS 的出生与 AI 等待、40 px/tick 权威位置、半 tick 插值、延迟 Start、不重复自主物理、镜头同源、触地立即吸附、同 tick 一次 60 HP 伤害和原版 hold/whiteOut。2026-09-29 Unity 6000.6.0f1 隔离 `Validate Anchor Animation Play Mode` 60/60 通过（新增 51 + 既有 9）；主工程实际画面与 Android 真机待验收。

- `ANC-ANI-02/03`：通过生产 `MutinyPlayerInput` 投放 Anchor，逐 25 Hz tick 检查下落 frame 1、触地后主 frame 3 的双侧镜像 1002 碎屑、主 frame 12 停止后子帧继续、子 frame 17 移除，以及 30+10 tick 的 `Global.whiteOut` 乘色/加色/透明度。2026-09-26 Unity 6000.6.0f1 隔离 Play Mode 执行 `Validate Anchor Animation Play Mode`，9/9 断言通过；实际战斗画面待验收。

- `CAN-SMOKE-02`：发射正式炮弹、推进 25 Hz 物理 tick 与半 tick 表现采样；确认首团烟与炮弹显示起点重合，随后炮弹向前移动且烟保持在身后，权威位置仍独立推进。
- `CAN-AUD-02`：原版 AS2 的地形 `contact` 分支播放 `pop`，角色重叠 `advance` 分支静音，`Explosion.hit` 不发声；此项为原版静态结论，Unity 直击分支按用户授权的 `CAN-AUD-03` 处理。
- `CAN-AUD-03`：以正式物理地形接触和角色包围盒重叠分别触发炮弹爆炸，监听 `SfxPlayed`；两条路线均应各播一次 `pop`。再执行正式爆炸命中入口及重复炮弹结束调用，断言不追加音效，覆盖此前直击静音及双响缺陷。
- 实际结果：2026-09-26，Unity 6000.6.0f1 隔离工程 Play Mode 执行 `Validate Cannon Effects Play Mode`，含烟迹回归共 19/19 断言通过；主工程实机画面与听感仍待验收。2026-09-25 的 15/15 为修改前的历史结果，不代表当前直击音效规格。
- `VIS-SMOKE-02`：2026-09-26 将 Cherry Bomb、Dynamite、Rum Bottle、Parachute Bomb 的生产出烟回调对齐当前物理 tick 的可见起点。使用正式 `MutinyPhysicsBody.AdvanceSimulationFrameForVerification`，分别在飞行和 ready 状态核对新烟团与首个可见弹体位置一致、半 tick 后弹体前进而烟团静止；复跑 `Validate Cannon Effects Play Mode`，Unity 6000.6.0f1 隔离工程 19/19 断言通过。主工程 PIE 逐武器画面验收尚未运行。
- `CAN-AI-TURN-01`：经 `MutinyAIController.ExecuteMove` 同源执行入口提交大炮，并交替推进正式 `MutinyTurnManager.AdvanceSimulationTick` 与大炮 25 Hz tick；前 24 tick 不得换回合或积累静止计数，第 25 tick 生成炮弹且镜头目标为该炮弹；炮弹未结束时再等待 151 tick 不得被通用安全超时强制结束，然后驱动正式炮弹物理跨出原版边界、炮身结束，最后才通过通常 11 tick 静止门。用例已添加，待 Unity 运行验证。
- `CAN-PLACE-01`：角色位于 `(100,200)` 时，通过生产 `MutinyCannon.PlacementCenterPixels` 与独立 `RangeCircle` Transform 断言范围中心均为 `(100,100)`，即原始 100 px 圆的底部落在角色坐标。
- `CAN-PLACE-03`：从炮身初始 `(100,190)` 按住并把指针快速移到 `(100,400)`，驱动生产 25 Hz tick；以 `(100,100)` 为中心的 120 px 约束应先把目标裁到 `(100,220)`，再按原版半距离移动至 `(100,205)`，并保持拖动资格。
- 当前状态：用例已加入 `MutinyTurnActionUiVerificationTest`；待 Unity Play Mode 实际执行，不能登记为通过。

## AI 武器候选缺口回归

- `GM-07`：通过 GM 正式解析入口覆盖 AI 武器候选；验证无库存也能评估强制武器、首行动仍保留跳跃、续行动无合格武器候选时仍可 Pass、正式开火不扣真实弹药、`0` 恢复库存选择、非法编号不改设置及 1..15 菜单映射。此项是 Unity 调试扩展，不是原版一致性规则。2026-09-26 在 Unity 6000.6.0f1 隔离工程 Play Mode 随当前 `RunGM()` 重跑通过；“有候选但评分非正”的续行动分支尚未由此专项用例单独覆盖。
- `GM-UI-02`：新增生产 `ExecuteCommand` 和历史按钮同源的 `RunRecentCommand` 回归，检查六次成功执行后仅保留最近五次、重复命令各占记录、失败不入列、点击旧命令后真实效果及顺序更新。3 条断言已加入 `RunGM()`；2026-09-26 隔离 Unity Play Mode 最新连同 GM-07 与 GM-03 共 `12/12` 通过，此次没有屏蔽断言；面板实际点击尚未运行。
- `GM-03`：经精确小写 `unlockalllevels` 的生产解析入口，从只解锁第 1 关开始检查 `1..18` 全部 `IsLevelUnlocked` 资格、最高关卡为 `MaxLevel`、真实 PlayerPrefs 存档键及成功历史；再通过历史按钮同源入口重放验证全部解锁。2 条断言加入 `RunGM()`，测试结束恢复原进度；2026-09-26 Unity 6000.6.0f1 隔离 Play Mode 随专项 `12/12` 通过。重启及实际选关 UI 待验收。

- `AI-WPN-04`：通过 `EvaluateCharacterWeapons` 的生产分发与 Anchor 正式执行入口，在固定随机种子和地面上验证全图垂直采样会生成 Anchor 候选；胜出后创建已发射的正式 Anchor，并消费库存。
- `AI-WPN-05`：通过同一生产分发器验证 Wooden Crate 获得至少 3 个合法 BoxWeapon 位置后进入候选；随后启动正式 `BeginAiPlacement` 并推进原版 40 tick 延迟，断言第一箱落地且三箱序列仍处于活动状态。
- 当前状态：用例已加入 `MutinyTurnActionUiVerificationTest`；`Assembly-CSharp` 与 `Assembly-CSharp-Editor` 编译通过；尚未在 Unity Play Mode 实际执行，不能登记为通过。

## AI 原版预测与镜头回归

- `AI-SLICE-01`：通过生产可恢复决策迭代器，在同一固定种子棋盘上与同步入口比较胜出行动、分数和速度；断言移动评价至少跨越 51 个可恢复步骤。真实 30 ms 帧预算仍需 Unity Play Mode 计时。
- `AI-RNG-01`：记录一次生产决策的全部随机抽样和候选，用该记录逐项重放；篡改一个抽样边界必须显式失败。JSON 路径、跨进程加载和复杂关卡还需运行验收。
- `AI-PHY-03`：候选从正式 Mine 初始化结果读取重量、弹性、摩擦、边界和箱体属性，确认候选没有消耗库存/射击资格/生命；与正式 Mine 以同一初态推进 3 tick 比较位置与速度。Parachute Bomb 的空气修正和顶部边界由正式/预测共用纯函数，再比较 3 tick 轨迹。

- `AI-PHY-01`：通过生产 `SimulateWeaponImpact` 验证 Rum Bottle 首次接触即结束，Boulder/普通 Weapon 在无专用完成条件时执行原版 101 个预测 tick，且普通武器不会因仅仅越过水线提前结束模拟。
- `AI-PHY-02`：长距离角色预测必须超过旧 70 tick 并持续到水线；注册共享箱体后，`hitsBoxes=true` 的 Cherry Bomb 候选必须在箱体处完成模拟。
- `AI-WPN-06`：生产 Seagull 距离公式在 20 px 时分别得到敌方 `+0.5`、己方 `-1.0`，40 px 边界为 0。
- `AI-CAM-01/02`：胜出角色通过生产镜头入口建立完成门；AI 控制与候选求值状态共同决定是否暂停宝箱等自动镜头分支。
- 当前状态：用例已加入 `MutinyTurnActionUiVerificationTest`；程序集编译通过；尚未在 Unity Play Mode 实际执行，不能登记为通过。

## Banana 轨迹一致性回归

- `BAN-PHY-01`：通过生产 `MutinyWeaponFactory.SpawnAndLaunch()` 提交 400 px 满拉力，断言 Banana 实际初速为原版 `twangMaxForce=30`，不再错误套用 `Weapon.release` 的 20 上限。
- `BAN-TRAJ-01`：以同一初速分别驱动生产预览 `PredictVelocityTick()` 和实际 `MutinyPhysicsBody.AdvanceSimulationTick()`，连续比较前 5 个无碰撞 tick 的坐标完全一致。
- `BAN-LIFE-01`：在无碰撞长地图中，经生产工厂发射香蕉，交替推进正式物理及回合 tick；205 tick 后已超过旧 150 tick 看门狗、旧 3500 px 横向阈值仍不得完成或换回合，继续下降越过 `levelHeight × 32` 后才结束。通用 8 秒和入水短计时已从 Banana 的 `Update()` 排除；实际墙钟 8 秒及入水表现仍需 Play Mode 验证。
- 当前状态：用例已加入 `MutinyTurnActionUiVerificationTest`；待 Unity Play Mode 实际执行，不能登记为通过。

## 原版武器生命周期回归

- `WPN-LIFE-01`：经生产工厂分别发射 CherryBomb、Dynamite、RumBottle，在无碰撞长地图各推进 205 个正式物理 tick；越过旧 3500 px/8 秒对应距离后必须仍活动，随后仅在下降越过 `levelHeight × 32` 时结束。
- `WPN-LIFE-02`：经生产工厂创建并启动 Tidal Wave，交替推进正式物理和回合 tick；205 tick 后不得被旧 150 tick 看门狗结束；继续飞过 `levelWidth × 32 + 550`，再由通常 11 tick 静止门换回合。
- `WPN-WATER-01`：RumBottle 正式发射跨越水线后，确认武器没有角色专属的水下水平阻力，也没有通用入水截止。
- `DYN-WATER-02`：Dynamite 正式发射入水、显示 unlit 并在地形上停稳后，仍生成 250/70 爆炸，不因时间/水深变成 dud。
- 实际结果：2026-09-25，Unity 6000.6.0f1 隔离临时工程 Play Mode 执行 `Validate Weapon Lifecycle Play Mode`，`BAN-LIFE-01`、`WPN-LIFE-01/02`、`WPN-WATER-01`、`DYN-WATER-02` 共 16/16 断言通过。主工程正在运行的编辑器、真实关卡画面、墙钟 8 秒持续表现及其他武器的原有长等待回归仍待验收；不能把这些未跑场景登记为通过。

## 武器共享初速限速回归

- `WPN-VEL-01`：经生产 `MutinyCherryBomb.Twang()` 满拉力提交，断言普通拉拽仍限制为 20；核对工厂预览上限中 Banana、Parachute Bomb、Rum Bottle 各为 30，Voodoo Doll 为 20。三种 30 武器的实际发射另由 `BAN-PHY-01`、`WPN-08-INT-01`、`RUM-PHY-01` 覆盖。
- `WPN-VEL-02`：经生产 `MutinyWeapon.Fire()` 分别给普通 Cherry Bomb 和 30 力 Banana 直接传入模长 45 的速度，断言各轴原样保留；确认原版 `Weapon.fire` 未调用 `Weapon.release` 或 `Solid.twang` 的截断。
- 当前状态：用例已加入 `MutinyTurnActionUiVerificationTest`；待 Unity Play Mode 实际执行，不能登记为通过。

## Rum Bottle 弹道与爆炸位置回归

- `RUM-PHY-01/RUM-TRAJ-01`：调用生产 `MutinyRumBottle.Twang()` 提交 400 px 满拉力，验证初速 30；随后驱动生产物理 tick，将无碰撞的前 5 个位置逐一与生产预览速度计算比较。
- `RUM-HIT-02`：让燃烧瓶斜向落地，并在同一个物理 tick 保持横向位移；断言瓶体继续横移，但生产爆炸与两条初始火焰使用纵向碰撞时的横移前坐标。
- 当前状态：用例已加入 `MutinyTurnActionUiVerificationTest`；待 Unity Play Mode 实际执行，不能登记为通过。

## Seagull 飞行与投弹时序回归

- `SEA-END-02`：在长地图上通过生产 `MutinyTurnManager.AdvanceSimulationTick()` 连续等待超过 150 tick，断言仍在正常飞行的海鸥不会被卡死武器看门狗强制完成或销毁。
- `SEA-SHOT-02`：点击只登记一次投弹请求；驱动一次生产 `MutinyPhysicsBody.AdvanceSimulationTick()` 后，断言海鸥先前进 10 px，炸弹从移动后位置的 `x-10` 创建，并在同一 tick 前进 10 px、下落 1 px。子弹物理体保持非自治更新，防止同帧双步。
- `SEA-VIS-01`：通过同一生产投弹入口生成炸弹后，断言其 SpriteRenderer 与海鸥处于相同 Sorting Layer，但 order 更低；覆盖原先 `WeaponSortingOrder + 1` 导致炸弹盖住海鸥的回归。
- `SEA-ANI-01`：通过同一生产投弹入口与 25 Hz 物理 tick，读取海鸥实际 SpriteRenderer 的纹理，断言投弹时 11→12→13→14→飞行 1，随后飞行 2..8→1；不显示原版跳转/空白帧 9、10。
- 当前状态：用例已加入 `MutinyTurnActionUiVerificationTest`；待 Unity Play Mode 实际执行，不能登记为通过。

## Voodoo Doll 运镜回归

- `CUR-SCALE-01`：经生产武器选中/投掷后的特殊光标刷新入口，分别取 Anchor 与 Parachute Bomb fan 的实际绘制矩形；在模拟 1100×800 窗口下，31×22 / 31×21 原版图应分别绘制为 62×44 / 62×42，鼠标热点偏移也加倍。用例已添加，待 Unity 运行，尚不能登记为通过。
- `ANC-CUR-01/VOO-CUR-01`：经生产武器选中、取消及巫毒目标绑定入口刷新特殊光标；检查 Anchor 和 Voodoo Doll 分别使用原版 31×22 光标图，巫毒鼠标移到娃娃自身上及选定目标后恢复普通光标。
- `VOO-CAM-01`：生产目标绑定入口把镜头平移目标设为使用者；正式发射后清除该平移并显式跟随娃娃。
- `VOO-CAM-02`：第 10 个娃娃物理 tick 同步释放娃娃跟随并把目标角色设为平移目标；平移未清除期间，目标等待计数不得推进。
- `VOO-CAM-03`：目标取得保存速度后，通用 action-target 搜索不得重新选中仍在淡出的娃娃，也不持续锁定移动目标；没有其他高优先级目标时恢复手动滚屏资格。
- `VOO-END-02`：目标镜头仍在远距离平移时连续驱动回合管理器超过 150 tick，娃娃不得被卡死武器看门狗强制完成。
- 当前状态：用例已加入 `MutinyTurnActionUiVerificationTest`；待 Unity Play Mode 实际执行，不能登记为通过。

## Mine 缺陷回归

- `MIN-ANI-01/AUD-01`：通过生产选中/投掷入口确认 Mine 投掷无音效请求，随后逐原版 tick 验证 frame 1、arm 11..16、warn 21..29 和十次 `mine_beep`。
- `MIN-TRG-01/MIN-AIM-01`：通过 PlayerInput 的实际 Throw Self 拉线入口触发静止角色附近的 Mine；倒计时爆炸后确认角色已在空中、输入仍处于 Aiming，并可调用生产提交入口完成 Throw Self。
- `MIN-TRG-02`：让角色携带非零速度进入半径，验证被动移动同样启动 warning/countdown。
- 当前状态：用例已加入 `MutinyTurnActionUiVerificationTest`，尚未在 Unity Play Mode 实际执行，不能登记为通过。

## 武器 ready / idle 动画回归

- `VIS-WPN-IDLE-01/DYN-ANI-01`：创建正式 `MutinyDynamite`，保持 `IsFired=false`，通过生产 `MutinyPhysicsBody.AdvanceSimulationTick()` 验证 `lit` 可见帧严格按 1→2→3→4→1 循环，frame 5 动作帧不被显示。
- `DYN-ANI-02`：通过生产 `EvaluateWaterState()` 触发入水回调，再推进正式物理 tick，断言画面保持原版 `unlit` frame 6。
- 当前状态：`Assembly-CSharp`、`Assembly-CSharp-Editor` 编译通过；Unity 6000.6.0f1 batchmode 定向回归 2/2 通过。

## Pieces of Eight 镜头缺陷回归

- `POE-CAM-01`：通过 `MutinyPlayerInput.TryLaunchWeaponForVerification` 的生产发射路径验证第 1 枚和第 2 枚钱币都会显式成为镜头目标，且发射会取消此前的角色回移目标。
- `POE-CAM-02`：通过 `ResolveCoin` 的生产结算状态转换验证前 7 枚会先释放钱币跟随再请求回到 owner；回移完成前手动滚屏分支不可用。
- `POE-CAM-03`：把镜头驱动到生产 `PanTowards` 的到达判定，验证 `panToCharacter` 清空后，即使回合仍为 `ActionExecuting` 也进入鼠标边缘/方向键/WASD 的普通滚屏分支；随后下一枚重新接管跟随。
- 当前状态：用例已加入 `MutinyTurnActionUiVerificationTest`；`Assembly-CSharp` 与 `Assembly-CSharp-Editor` 编译通过，尚未在 Unity Play Mode 实际执行，不能登记为通过。

## 跳跃取消缺陷回归

- `JUMP-CAN-01`：通过生产 `SelectCharacterThrow` 进入跳跃待命，刷新实际角色覆盖层并断言取消叉可见；点击原版 20 px 命中区后断言回到行动菜单且 `CanThrow` 未消耗。
- `EXT-JUMP-CAN-01`：进入实际 Aiming 状态后调用与鼠标右键共用的取消处理，断言轨迹蓄力撤销、回到跳跃待命、取消叉保持可见且 `CanThrow` 未消耗。
- 当前状态：用例已加入 `MutinyTurnActionUiVerificationTest`，尚未在 Unity Play Mode 实际执行，不能登记为通过。

## 高刷新率镜头跟随回归（feature/cameramovement）

- `AI-BOX-CAM-01/EXT-AI-BOX-CAM-02`：`Validate AI Box Camera Play Mode` 经实际 AI 执行入口启动三木箱/两火药桶序列；换位置、向上堆叠、39/40 tick 出生门、9/10 tick 间隔、60/120 FPS 连续平移、空投优先级、无合法候选、owner 取消选中/死亡和完成后的旧箱跟随抑制均有回归。2026-09-27 Unity 6000.6.0f1 隔离工程 125/125 通过（AI 116 条、既有玩家箱体交互 9 条）；编译通过；主工程与 Android 实际画面待验收。玩家火药桶旧完成断言现驱动正式序列 tick 后检查根节点，原因与原版来源记录于火药桶模块，未改生产放置时序。

- `CAN-PRES-01/02`：正式 `TryBeginBodyDrag/DragBodyTo/ReleasePointer/CancelPointer`、碰撞和大炮帧推进入口驱动 25/60/120 FPS 显示采样，验证中间帧不额外推进物理、拉栓跟随炮身、范围圆不移动、撞墙不穿透、短拖释放补交一次碰撞步、取消和开火清除旧插值。2026-09-27 Unity 6000.6.0f1 隔离工程 `Validate Cannon Presentation Play Mode` 45/45 通过，含新增 21 条及既有大炮 24 条；两个 C# 程序集编译通过。详见 [大炮规格](../../06-WeaponsAndEffects/09-Cannon/README.md)；主工程真实鼠标与 Android 真机观感待验收。

- `AIR-PRES-01/02`：生产 `MutinyTreasureChestManager.Update` 同源推进入口创建、下降并落地宝箱；检查 `-300` 起点、每 tick 3 px、120/60 Hz 中间显示帧只移动位置不重复增加 `TimeTaken` 或推进降落伞帧、落地时钳到 `floorY-15` 且立即清除旧插值。2026-09-27 Unity 6000.6.0f1 隔离工程 `Validate Air Drop Presentation Play Mode` 5/5 通过；`Assembly-CSharp-Editor` 编译通过。原版离散规则和用户授权显示扩展分开登记于 [宝箱降落规格](../../07-DynamicWorldEvents/02-ChestDescentAndLanding/README.md)；主工程实际画面和 Android 真机待验收。

- `CAM-PRES-01`：驱动生产 `MutinyPhysicsBody.AdvanceSimulationFrame` 与显示入口，验证角色/武器显示位置由相邻已完成 tick 插值，权威 `State` 不随中间渲染帧改变；外部改位不复用旧轨迹。
- `CAM-PRES-02`：驱动生产 `MutinyCameraController.AdvanceCamera`，验证跳跃角色、海鸥、海啸、炮弹的镜头目标与其显示位置同源；60/120 FPS 中间帧仍前进，远距离跟随不超过 30 px/tick；空投优先于武器并保持 50 px/tick 切入。
- `CAM-TRACK-SEA-01`：验证海鸥使用原版 `trackY=y+100` 偏移。
- 实际结果：Unity 6000.6.0f1 独立临时工程 Play Mode 批处理运行 `Validate Camera Movement`，11/11 断言通过；两个 C# 工程编译通过。该结果证明数值及生产方法路径，不等于主工程真实 25/60/120 FPS 画面验收。

## Android 镜头缺陷回归

- `CUR-FRONT-01`：创建真实 Input System Mouse 并把位置设在窗口边缘，在生产 Title 页保留活动回合与关卡引用，驱动相机更新及系统光标可见性更新，断言不滚屏、不显示箭头且鼠标恢复可见。2026-09-27 隔离 Unity 6000.6.0f1 `Validate Scroll Arrows Play Mode` 光标专项 11/11 通过；实际鼠标/手柄页面画面待验收。见 [前端光标规格](../../09-PresentationAndFeedback/08-CursorsAndTrajectory/FRONTEND_CURSOR_BEHAVIOR.md)。

- `CUR-SCROLL-01/AND-CUR-SCROLL-01`：2026-09-25 的历史基线通过箭头资源、八方向、视口边缘及移动平移专项 5/5 断言；其中安卓边缘箭头要求已于 2026-09-26 被用户撤回，不再作为当前验收结果。桌面实际鼠标画面仍待验收。
- `AND-CUR-SCROLL-02`：2026-09-26 移除安卓拖动方向箭头的状态与绘制分支，保留触摸平移和桌面滚屏箭头。Unity 6000.6.0f1 隔离临时工程 `Validate Scroll Arrows Play Mode` 通过 10/10 断言，覆盖生产绘制门、移动平移与边界钳制；`Assembly-CSharp-Editor` 编译通过。Android 真机画面尚未运行。
- `CAM-EDGE-05`：固定 11:8 画布的用户授权黑边扩展；生产 `CalculateMouseEdgeScroll` 回归覆盖左右 Pillarbox、上下 Letterbox、黑色角落组合方向和游戏窗口外抑制。2026-09-26 Unity 6000.6.0f1 隔离临时工程 `Validate Scroll Arrows Play Mode` 通过 10/10 断言（含既有资源、方向及移动端用例）；`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q` 通过。主工程 PIE 实际鼠标画面尚未目视验收。
- `CUR-SCROLL-02`：原版 `CustomCursor` 同类型早退；Unity 武器特殊光标经生产 `SetMode(None)`、`Clear()` 重复调用时，镜头已经隐藏的系统鼠标保持隐藏，切换到武器特殊光标及真正退出时才改变可见性。2026-09-25 隔离 Unity 6000.6.0f1 Play Mode `Validate Scroll Arrows Play Mode` 复跑共 6/6 断言通过；主工程 PIE 连续滚屏无闪烁待目视复验。
- `AND-CAM-01`：调用生产镜头的移动平台指针资格判定，断言 Android/移动平台即使暴露 mouse/pointer 状态也不得进入桌面悬停边缘滚屏；同时断言桌面真实鼠标仍保留原版边缘滚屏资格。
- 当前状态：用例已加入 `MutinyTurnActionUiVerificationTest`；`Assembly-CSharp` 与 `Assembly-CSharp-Editor` 编译通过；尚未在 Android 真机实际执行，不能登记为通过。

## Android 触摸蓄力回归

- `AND-INP-01`：把生产触摸阶段映射为统一主指针，断言 `Began` 只有按下沿且保持按住，`Moved/Stationary` 只保持按住，`Ended` 只有松开沿；桌面 Mouse 仍走同一消费状态机。
- `AND-INP-02`：生产触摸读取只锁定首个 `Began` 的 touch id，其他触点不能替换本次手势；真机多点干扰仍待验证。
- `AND-INP-03`：断言 `Canceled` 只产生取消标记而不伪造正常松开；正在拖动的火炮取消后不提交发射。
- 当前状态：用例已加入 `MutinyTurnActionUiVerificationTest`，覆盖生产触摸状态构造、触点取得资格和火炮取消入口；`Assembly-CSharp` 与 `Assembly-CSharp-Editor` 编译通过；尚未在 Unity Play Mode 与 Android 真机实际执行，不能登记为通过。

## Android 触摸镜头回归

- `AND-INP-04/AND-CAM-02`：验证屏幕拖动量按正交相机当前可视宽高换算为世界位移，且镜头反向移动以形成“内容跟手”；生产拖动入口继续经过回合资格、自动跟随门和关卡边界。
- 当前状态：用例已加入 `MutinyTurnActionUiVerificationTest`；`Assembly-CSharp` 与 `Assembly-CSharp-Editor` 编译通过；尚未在 Android 真机实际执行，不能登记为通过。

## Android 点击/拖动与多指回归

- `AND-INP-05`：点击型武器的触摸按下不立即提交；累计屏幕位移不超过阈值的松手才调用生产武器入口，超过阈值转为镜头拖动且不消费武器。
- `AND-INP-06/AND-CAM-03`：行动触点处于 Aiming/火炮拖动时，第二 touch id 可取得独立镜头角色；镜头门只对这个明确角色放宽 aiming 限制，其余回合、AI、自动目标和边界门保持不变。
- 当前状态：阈值分类与 aiming 镜头角色门用例已加入 `MutinyTurnActionUiVerificationTest`；`Assembly-CSharp` 与 `Assembly-CSharp-Editor` 编译通过；多指设备行为尚未在 Android 真机实际执行，不能登记为通过。

## Android 降落伞炸弹扇风回归

- `AND-PCB-FAN-01`：验证移动触点持续按住时 fan 输入为 active；短触摸只有 `Began` 锁存、在下一物理 tick 前已松开时仍 active 一次；脉冲消费后无持续触摸则恢复 inactive。方向与 `±0.2` 继续由现有 `PCB-FAN-01` 生产物理用例覆盖。
- 当前状态：用例已加入 `VerifyParachuteBomb()`，覆盖持续触摸、短点击首 tick 生效及第二 tick 不重复消费；`Assembly-CSharp` 与 `Assembly-CSharp-Editor` 编译通过；尚未在 Android 真机实际执行，不能登记为通过。

## 简中按钮文字垂直对齐回归（LOC-BTN-01）

- 最新规格按用户要求：简中按钮文字相对旧版下移 1 个原版像素，Y 偏移由 -3 改为 -2。从生产 GM `setlanguage zh-cn/en` 切换后读取 163×24 和 280×24 两种按钮的实际文字矩形，并检查 1×/2× 画布变换后增量为 +1/+2 屏幕像素；底图/命中区域尺寸保持不变，英文 PirateFont 不偏移。
- 2026-09-27 Unity 6000.6.0f1 隔离 Play Mode `RunGMLanguage()` 当前 11/11 通过，含本规则三条布局断言；日志 `C:/Users/27487/AppData/Local/Temp/current-ui-adjustment-20260927.log`。先前 -3 偏移的 10/10 是历史基线；对白与提示 124/124 为旧专项结果，本次未重跑。主工程/玩家构建的实际像素画面待目视验收。

## 失败结算提交按钮回归（SCORE-EXT-04）

- 缺陷：失败结算页仍显示原版 `submitScoreButton`，与仅通关自动记录成绩的用户规则冲突。
- 生产入口：`MutinyGameHUD.DrawOriginalGameEndPopup` 从 `FailedPopupButtons` 绘制并注册交互；`MutinyTurnManager` 进入最终关失败结果后，由 `MutinyGameHUD.SynchronizeGameEndPopup` 打开弹窗，并从 `MutinySaveSystem` 读取前后榜单。
- 断言：失败页按钮列表只有重试与返回，原提交区域没有绘制或命中矩形；最终关失败后弹窗为 `LevelFailed`，历史分数及时间戳不变。
- 实际结果：2026-09-27，Unity 6000.6.0f1 隔离工程 Play Mode 批处理 `Validate Level Lifecycle Play Mode` 通过 10/10 断言（含既有结算回归）。主工程实际鼠标悬停/点击画面与完整最终关胜利回合仍待运行。

## 双人关卡资源缺失回归（2P-ASSET-01）

- 缺陷：双人选关第 19 关起显示 `data unavailable`。生产 `HasNumberedLevelData` 从 `Resources/Data/Levels/level_NN.xml` 查找，而该目录此前仅有 01–18。补入按原版哈希名取得的 19–33 原始 XML 后，用 `MutinyTwoPlayerVerificationTest` 从生产选择资格和 `TryLoadLevel` 逐关核对 16–33：关卡编号不串关、红蓝双方存在且存活、菜单双人会话下双方均为人类、天色按编号匹配；另断言第 34 关被拒绝且当前关保持不变。
- 原件与运行资源的 URL、SHA-256 和 `players` 见 [原版清单](../../10-OriginalEvidence/Artifacts/TwoPlayerLevels/catalog.csv)。
- 实际结果：2026-09-25，Unity 6000.6.0f1 主工程编辑器菜单 `Mutiny → Parity → Validate Two Player Mode` 通过 **52/52** 断言；此数字含其他双人 Flow/结算断言。`Application.isPlaying` 专属回合/输入检查在这次编辑器态运行中跳过，实际 UI 点击和完整 Play Mode 对局待验收。
