# 09.04 · 镜头

[返回上级模块](../README.md)

## 职责

管理跟随、边缘滚屏、空投运镜、震动和目标优先级。

## 边界

镜头不影响逻辑坐标。

## 行为规格

### 蓄力期间桌面边缘滚屏（2026-09-29）

实现前规格 `CAM-AIM-EDGE-01`：玩家用鼠标蓄力跳跃或可投掷武器时，鼠标进入四边热区或窗口内黑边仍可通过生产边缘滚屏移动画布、显示方向箭头；回到中央停止滚屏输入。保持蓄力、轨迹、库存与资格，只有正式松手才提交跳跃/武器。鼠标蓄力不自动回角色；原有 speech/空投/武器分支在鼠标蓄力结束后恢复。手柄蓄力保留右摇杆策略；移动端不新增悬停滚屏或箭头，已有多指适配不变。过场、前端、AI、行动菜单及显式 `excamera` 击退接管仍沿用原资格。

原版静态依据：`TileSystem.as::advanceScrolling:420-493` 只在 `Controller.dragging` 时早退，并未检查 `Controller.twanging`。跳跃/普通投掷属于 Twang，当前 Unity 错把统一 `IsAiming` 当成 dragging 的禁止滚屏门；本条修复恢复其区别，不改变大炮炮身拖动等专门操作。补核 `Cannon.as::advance:91-161`：拉栓只设 `draggingPin`，不会设 `Controller.dragging`，所以拉栓蓄力也应允许边缘滚屏；只有实际炮身拖动继续锁镜。证据均为 `Artifacts/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/throwgame/` 下 AS2。Unity 入口为 `AdvanceCamera → AdvanceEdgeScrolling`，回归须经生产选中、蓄力、Input System 鼠标设备、实际镜头推进和松手提交检查，而非只测试方向计算。

2026-09-29 已实现及实际结果：Unity 6000.6.0f1 隔离工程 `Validate Scroll Arrows Play Mode` 113/113 通过（本规则新增 102、既有箭头/黑边/安卓绘制门/前端光标 11）；`Validate Camera Movement` 11/11 复跑通过。滚屏使用实际显示帧的 deltaTime，速度/加速度公式不变；蓄力期间不自动回移，松手后恢复行动跟随。主工程 PIE 的鼠标持续拖动、系统光标与 Android/实物手柄仍待目视验收。本次只修复蓄力滚屏资格，不宣称全部镜头优先级与 Flash 一致；保留既有 Unity 蓄力自动平移抑制和用户授权的黑边/高刷新率/`excamera` 策略。

### 可选爆炸击退镜头（2026-09-27，原版无此行为）

原版来源：不适用，用户明确授权扩展。实现前规格：

| ID | 可观察行为与状态转换 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- |
| EXT-EXCAM-01 / GM-11 | 默认关闭；`excamera 1` 或 `blastcam 1` 开启，`excamera`、`excamera 0` 或 `blastcam 0` 关闭。非法参数不改状态；开关跨关卡保留，新 Play/重启关闭，不写存档。关闭立即清除当前跟随与候选。 | GM 正式解析、镜头会话开关 | 默认、大小写、关闭、非法参数、易读别名、重置 | 2026-10-02 隔离 Play Mode GM 81/81 含 `blastcam` 正式解析；实际画面待运行 |
| EXT-EXCAM-02 | 只消费实际 `Explosion.ApplyHit` 的非零击退，不从速度推断爆炸。下一显示帧立即接管，同帧各爆炸候选按显示坐标到镜头中心的二维距离选择唯一最近角色；等距以 Unity EntityId 稳定决胜。复用 30 px/tick 上限、y-50 构图和高帧率插值。 | 爆炸击退批事件、镜头 LateUpdate | 单人、多目标、同帧多爆炸、60/120 FPS、正常跳跃及零冲量排除 | 已实现；上述生产入口数值检查通过；含真实 Cherry Bomb 生成的爆炸链 |
| EXT-EXCAM-03 | 一批击退只选一次；选中者静止、落水、销毁或物理停用后释放，但其余被击退者尚未停止期间不接力选人，连锁爆炸也不抢换目标。全部本批角色停止后，新爆炸才可开始新批。生命耗尽但尚在空中且可见的角色仍可跟随。 | 镜头批次锁与释放 | 不接力、连锁不抢占、新批、致死、落水、禁用、切关/换队清理 | 已实现；真实重力/地形落地、两种接力旁路阻止、连锁、新批、致死、落水、停用、销毁和关卡重置通过；实际换队待验收 |
| EXT-EXCAM-04 | 开启后的击退目标优先于瞄准、speech、AI 思考、空投、AI 箱体和普通武器；过场仍暂停镜头。关闭时原优先级不变，不改写物理、伤害或回合计时公式；显式镜头抵达门仍保留。 | `AdvanceCamera`、手动平移资格门 | 与武器/空投竞争、关闭回归、生产伤害与运动 | 已实现；武器/空投竞争和默认关闭镜头回归通过；speech/AI 思考同场画面待验收 |

“一批”指从首次爆炸击退到这批角色全部停止的连续运动段；同帧候选汇总后才选人，避免按对象执行顺序抢占。此解释用于避免用户明确禁止的停止后立即接力；不为普通跳跃、碰撞、海啸推动或巫毒速度转移触发新镜头。

已有显式 `panToCharacter` 请求若指向该批其他移动者，只推迟到本批结束再恢复，不取消原有 AI/巫毒镜头抵达握手。扩展不改写伤害、冲量、物理 tick 或放置倒计时；原本依赖镜头抵达的动作可能因镜头接管而等待更久，不承诺这类同时发生的画面保持原版墙钟时长。

2026-09-27 实际结果：Unity 6000.6.0f1 隔离工程 `Validate Explosion Camera Play Mode` 29/29 通过，默认关闭时的既有 `Validate Camera Movement` 11/11 复跑通过，两个 C# 程序集编译通过。回归先检出空投优先级遗漏，修正后复跑通过，未降低断言标准；落地用例驱动正式物理 tick，不直接改静止布尔量。主工程完整武器对局、GM 面板实际点击/输入、speech/AI 同场抢占、实际换队、跨场景与重启画面和 Android 真机仍待验收。此规则整体为授权扩展，不能计为 Flash 一致性通过。

### AI 连续箱体运镜（2026-09-27）

| ID | 可观察行为及状态转换 | 原版来源 / 授权差异 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| AI-BOX-CAM-01 | 木箱/火药桶 AI 每次选定下一处位置后，在 40 tick 准备期就向该位置平移；目标来自根序列的计划坐标，不来自已放下的第一箱。保留 10 tick 间隔、30 px/tick 速度及 `y-50` 构图。 | `BoxWeapon.as::aiPerform/aiContinue/advance`、`TileSystem.as::advanceScrolling:455-460` | `AiPlacementCameraTargetPixels`、`MutinyCameraController.TryGetAiBoxPlacementCameraTarget` | `RunAiBoxCamera` 正式序列验证三木箱/两火药桶不同目标、首次放置前运镜和 60/120 FPS 连续步长 | 原版静态确认；已实现；隔离 Play Mode 专项 125/125 通过 |
| EXT-AI-BOX-CAM-02 | 每次向上堆叠 48 px 也重新接管跟随；准备及后续箱尚未完成的 10 tick 间隙保持最近计划点。完成/无合法候选/换队/使用者死亡即停止此目标，通用扫描不再跟随留下的箱体。 | 用户本轮确认；原版堆叠早退只修改 `trackY`、不重设 `track=true`，根箱首次 `place` 会清除 `track`，因此统一连续跟随是明确授权扩展 | 两种箱体目标 getter 与镜头筛选 | 正式堆叠、完成、异常与 owner 资格边界；玩家 `BOX-CAM-01` 不变 | 已实现；堆叠、无合法候选、取消选中、生产伤害死亡及完成释放回归通过；实际换队画面待验收 |

`BOX-CAM-01` 的自由滚屏结论仅针对玩家手动摆放，不适用于 AI 计划点。speech、AI 求值暂停及下降空投保持既有较高优先级；不添加“镜头抵达才放置”的逻辑等待。

2026-09-27 实际结果：Unity 6000.6.0f1 隔离工程 `Validate AI Box Camera Play Mode` 125/125 通过，其中新的 AI 镜头规则 116 条、既有玩家木箱/火药桶生产交互 9 条。通过生产 `MutinyAIController.ExecuteMove` 启动序列，检查每处位置的准备/放置/间隔、60/120 FPS 的连续镜头步长、空投优先级、序列完成后的显式旧箱目标抑制。既有 `Validate Camera Movement` 在本次改动后复跑 11/11 通过。两个 C# 程序集编译通过。主工程完整对局、speech 同场抢占、实际换队和 Android 真机仍待目视验收，不计为已通过。

### 关卡初始镜头（2026-09-26）

| ID | 可观察行为及状态转换 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| CAM-INIT-01 | 每次进入、重开、切关，在开始自动运镜前同步重置到原版视口左上角 `(0,0)` 对应的镜头中心 `(275/32,-200/32)`；不继承上一局位置或滚屏速度、武器/角色/气泡目标 | `TileSystem.as::readXML:178-187`；`Controller.as::changeLevel` 新建 TileSystem | `MutinyLevelController.BuildLevel/Start`、`MutinyCameraController.ResetForLevel` | 从任意旧镜头位置经实际编号加载、重开、下一关和退出再加载入口检查重置、绑定新关和清除旧目标；烘焙旧场景走实际 Awake/Start | 静态确认、已实现；Unity 6000.6.0f1 隔离 Play Mode 专项 13/13 通过；主工程画面待验收 |
| CAM-INIT-02 | 初始位置仍走原版边界与水位裁切；第 7/30 关视口左上角为 `(0,-32)`，第 13 关为 `(0,-192)`，其余关为 `(0,0)`；没有逐关独立镜头配置 | `TileSystem.as::panCamera:497-518`；原始 XML 的 water.y | `MutinyCameraController.ResetForLevel/ClampPosition` | 用真实关卡 1/7/13/30 资源加载，检查初始镜头像素中心分别为 `(275,200)/(275,168)/(275,8)/(275,168)` | 静态确认、已实现；上述真实资源加载断言通过；其余关卡画面未逐关运行 |
| CAM-INIT-03 | 开场重置不瞬移到 speaker；speech 激活后从重置起点按 50 px/tick（显示帧等效速度）移向气泡记录位置，气泡隐藏的 10 tick 也可运镜 | `TileSystem.as::readXML/advanceScrolling:429-432`；`SpeechBubble.as::setTarget/advance` | `MutinySpeechController.Update/AdvanceSpeech`、`MutinyCameraController.AdvanceCamera` | 生产 StartGame、Update 同源 speech 推进、镜头更新链检查第一帧位移上限、方向和气泡未显示时运镜；实际画面另验收 | 静态确认、已实现；专项 120 FPS 首帧数值断言通过；完整两方 speech 画面待验收 |

战斗背景由 `MutinyBattleBackground` 在镜头定位之后读取实际摄像机位置，以原版 550×400 舞台、32 PPU 和 `Water.as::advance` 的分层取模公式更新；镜头本身仍只修改视图，不修改物理坐标。三套图层按 `TileSystem.as` 的关卡编号分组选择。`VIS-BG-01` 已接入，待 Unity Play Mode 在关卡 1/6/11/16 逐帧核对。

| ID | 可观察行为 | 原版来源 | Unity 入口 | 当前结果 |
| --- | --- | --- | --- | --- |
| CAM-POE-01 | Pieces of Eight 每枚发射显式接管钱币跟随；前 7 枚结算立即释放跟随并自动回使用者；抵达后连续武器仍处于执行阶段也允许鼠标边缘、方向键和 WASD 滚屏；下一枚发射再次接管 | `Weapon.as::fire/twang`；`PiecesOfEight.as::next`；`TileSystem.as::advanceScrolling:455-493` | `RequestTrackWeapon`、`ReleaseWeaponTracking`、`RequestPanToCharacter`、`AdvanceEdgeScrolling` | 已实现；C# 编译通过，待 Unity 运行验证 |
| BOX-CAM-01 | 木箱/火药桶每次放置在同一调用中把通用 `track=true` 覆盖为 `false`，且不设置 `panToCharacter`；连续摆放间隙直接进入普通手动滚屏 | `Weapon.as::place:140-148`；`BoxWeapon.as::place:93-100`；`TileSystem.as::advanceScrolling:455-493` | `MutinyPlayerInput.IsAwaitingBoxPlacement`、`FindActionTarget`、`CanUseManualScrolling` | 第一箱/桶后保持执行阶段，断言镜头不跟随已放物且边缘/方向键/WASD 门开放 | 已实现；待 Unity 运行验证 |
| VOO-CAM-01/02/03 | 选定目标后回使用者；投掷阶段跟随娃娃；10 tick 后关闭娃娃跟随并平移到目标；到达后等待 10 tick 再传递速度，且不重新跟随娃娃或持续锁定移动目标 | `VoodooDoll.as::setTargetCharacter/advance`；`Weapon.as::twang`；`TileSystem.as::advanceScrolling:455-468` | `RequestTrackWeapon`、`ReleaseWeaponTracking`、`RequestPanToCharacter`、Voodoo handoff guard | 分阶段断言跟随所有权、平移门、速度传递与最终手动滚屏资格 | 已实现；待 Unity 运行验证 |
| CAM-SPEECH-01 | 单人开场与胜方结算时，镜头优先平滑跟随气泡记录的世界位置，每原版 tick 最多移动 50 px；气泡结束后恢复原有运镜优先级 | `TileSystem.as::advanceScrolling`；`SpeechBubble.as::setTarget` | `MutinySpeechController`、`MutinyCameraController.LateUpdate` | 开场两方轮流发话、结算胜方发话时检查镜头目标和边界 | 已接入；待 Unity Play Mode 验证 |
| MINE-CAM-01 | 投掷中的地雷可被跟随；落地安置后即使继续监测靠近目标，也不再抢占镜头 | `Mine.as::advanceMotion` 设置 `finished=true`；`TileSystem.as::advanceScrolling` 只跟随未完成的武器 | `MutinyCameraController.FindActionTarget` | 校验飞行中跟随、安置后释放普通与显式跟随 | 已实现；待 Unity 运行验证 |
| AI-CAM-01 | AI 完成候选选择后，镜头显式平移到胜出角色；只有平移目标清空才执行行动 | `Team.as::advance` | `MutinyAIController.ExecuteAITurnRoutine`、`PanToCharacter` | 已实现；局部回归已写，待 Unity 运行 |
| AI-CAM-02 | 当前 AI 尚在求值时，镜头在 speech/popup 后立即返回，不进入下降宝箱、行动目标或回合角色分支 | `TileSystem.as::advanceScrolling:438-445` | `IsEvaluatingCandidates`、`ShouldPauseForAiThinking` | 已实现；状态真值表回归已写，待 Unity 运行 |
| CAN-AI-TURN-01 | AI 大炮正式发射后，以当前选中角色的大炮子炮弹为跟随目标，而不是场景中先枚举到的其他武器；25 tick 待开火时不因提前切换回合生成新空投。原版已在下降且 `timeTaken<100` 的空投仍优先于炮弹 | `Cannon.as::update/advance/aiPerform`；`Character.as::advance`；`TileSystem.as::advanceScrolling:438-459`；`Controller.as::nextTurn` | `MutinyTurnManager.CheckAllBodiesAtRest`、`MutinyCameraController.FindActionTarget` | AI 提交大炮后 24/25 tick 与炮弹跟踪目标、结算后切回合 | 已实现、C# 编译通过；待 Unity 运行验证 |

## 高刷新率跟随规格（feature/cameramovement）

原版 SWF 为 25 FPS；`Controller.as::enterFrame` 每帧先推进队伍角色和武器，再调用 `TileSystem.as::advanceScrolling`。原版自动跟随角色、武器和平移目标使用 `panTowards(..., 30)`，空投使用 50 px/tick；`Seagull.as::advance` 将独立的跟随点设为 `trackY = y + 100`。以下显示插值属于用户授权的高刷新率扩展，并非原版行为。

| ID | 可观察行为及状态转换 | 原版来源 / 扩展依据 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| CAM-PRES-01 | 物理仍按 0.04 秒推进；每次 tick 保存起止权威位置，渲染帧对目标画面及镜头目标读取同一已完成 tick 的插值位置。位置被外部放置/瞬移时不得跨旧轨迹插值 | SWF `frameRate=25.0`；`Controller.as::enterFrame`；用户授权的显示扩展 | `MutinyPhysicsBody`、`MutinyCameraController.LateUpdate` | 经生产物理 tick 与显示入口核对 tick 边界、半 tick、外部改位，断言显示位置与镜头同源且 `State` 不被插值改写 | 已实现；Unity 6.6 隔离工程 Play Mode 数值回归通过；主工程真实画面待验收 |
| CAM-PRES-02 | 海鸥、海啸、跳跃角色和炮弹以渲染帧频率更新画面/镜头位置；远距离平移仍受原版 30 px/tick 速度上限约束，空投继续使用现有 50 px/tick 分支 | `TileSystem.as::advanceScrolling/panTowards`；`Seagull.as`；`TidalWave.as`；用户反馈 | `MutinyCameraController`、相应 `MutinyPhysicsBody` | 生产入口测试 60/120 FPS 中间帧、原版跟随上限与空投优先级；25/60/120 FPS 主工程画面检查待做 | 已实现；Unity 6.6 隔离工程 Play Mode 数值回归通过；画面待验收 |
| CAM-TRACK-SEA-01 | 跟随海鸥使用 `trackY=y+100`，不复用普通武器的 `y-50` 目标 | `Seagull.as::advance:118`、`TileSystem.as::advanceScrolling:455-460` | `MutinyCameraController` | 经真实海鸥飞行物理 tick 和镜头入口检查目标纵向偏移 | 原版静态确认、已实现；Unity 6.6 隔离工程 Play Mode 断言通过 |

一个已完成物理 tick 的显示延迟（至多 40 ms）是该扩展的明确取舍；画面与镜头必须共用该延迟，不允许只平滑镜头或只预测目标。碰撞、伤害、行动资格继续读取权威状态。

## 画面比例与视口适配规格（11:8 Letterbox / Pillarbox）

原版 Flash 游戏舞台固定为 550×400 像素（宽高比 $11:8 = 1.375$）。为了在任意屏幕（16:9 PC 显示器、20:9 移动端屏幕、4:3 平板等）保持 100% 原版构图和视口，摄像机与 UI 统一采用动态 Letterbox/Pillarbox 黑边填充：

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 / 断言 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| CAM-ASPECT-01 | 屏幕比例正好为 11:8 时（如 1100×800），相机使用全视口 `rect = (0, 0, 1, 1)`，无任何黑边 | Flash 原始舞台 550×400 | `MutinyCameraController.CalculateViewportRect` | `exact11x8Rect == (0,0,1,1)` | 已实现；编译与测试通过 |
| CAM-ASPECT-02 | 屏幕比例宽于 11:8 时（如 16:9 1920×1080），相机视口横向居中收缩并保持 11:8，左右两侧产生纯黑 Pillarbox 遮罩；游戏世界与 HUD 100% 对齐 | 保持 550×400 原始关卡视野 | `CalculateViewportRect`、`DrawLetterboxBars` | `widescreen16x9Rect == (0.11328, 0, 0.77344, 1)` | 已实现；编译与测试通过 |
| CAM-ASPECT-03 | 屏幕比例高于 11:8 时（如 4:3 1024×768），相机视口纵向居中收缩并保持 11:8，上下两侧产生纯黑 Letterbox 遮罩 | 保持 550×400 原始关卡视野 | `CalculateViewportRect`、`DrawLetterboxBars` | `taller4x3Rect == (0, 0.01515, 1, 0.96970)` | 已实现；编译与测试通过 |
| CAM-EDGE-01 | 鼠标边缘滚屏的 40px 触发边界绑定于相机的实际像素视口 `pixelRect`，在宽屏有黑边时鼠标移动至游戏画面边缘即可触发滚屏，无需移到整个显示器窗口边缘 | 原版边缘滚屏 40px 区域 | `MutinyCameraController.CalculateMouseEdgeScroll` | `CAM-EDGE-01` ~ `CAM-EDGE-04` 断言视口边界与窗口边界过滤 | 已实现；编译与测试通过 |
| CAM-EDGE-05 | 鼠标进入游戏窗口内、视口外的左右或上下黑边时，继续按最近的视口边缘方向滚屏并显示箭头；角落可组合两个方向，窗口外不触发。40px 内侧热区与自动跟随/AI/移动端门禁不变 | 用户授权的固定 11:8 画布适配扩展；原版 `TileSystem.as::advanceScrolling` 的四边方向规则 | `MutinyCameraController.CalculateMouseEdgeScroll`、`AdvanceEdgeScrolling` | 生产边缘计算检查左右黑边、上下黑边、角落、视口中心、窗口外；专项 Play Mode 10/10 断言通过 | 已实现；编译与隔离 Unity Play Mode 通过；主工程 PIE 画面待验收 |
| CAM-BARS-01 | 在 `OnGUI()` 的 `Repaint` 事件中精确绘制视口外侧纯黑矩形（`GUI.DrawTexture`），杜绝黑边区域任何残影或未清屏像素 | 原版全屏/黑边表现 | `MutinyCameraController.DrawLetterboxBars` | 视口外边距覆盖率 100% | 已实现；编译通过 |

## Android 授权适配

| ID | 可观察行为 | 来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| AND-CAM-01 | Android 等移动平台没有桌面悬停光标；无触摸时的空闲/默认指针位置不得触发屏幕边缘滚屏。桌面端鼠标边缘滚屏保持原版行为 | 用户报告：Android 开场镜头持续左移；现有 `Munity.apk` 的 IL2CPP 产物 `Assembly-CSharp__3.cpp:20371-20443` 确认直接读取 `Mouse.current.position`，且无移动平台门禁 | `MutinyCameraController.AdvanceEdgeScrolling`、`ShouldUseMouseEdgeScrolling` | 生产判定在 `isMobilePlatform=true` 且存在 pointer/mouse 状态时仍返回 false；桌面真实鼠标返回 true | 已实现；`Assembly-CSharp` 与 `Assembly-CSharp-Editor` 编译通过；Android 真机运行待验证 |
| AND-CAM-02 | Android 空白处单指拖动提供桌面边缘滚屏的移动端替代操作；手指拖动地图内容，镜头作反向等比例移动，并继续使用生产镜头边界 | 用户授权的 Android 镜头适配要求 | `CanStartMobileTouchPan`、`PanByMobileTouchDelta`、`ScreenDeltaToWorldDelta` | 以不同屏幕宽高换算拖动量，断言相机正交可视范围与拖动比例一致；生产入口调用 `SetClampedPosition` | 已实现；C# 编译通过，待 Android 真机验证 |
| AND-CAM-03 | 玩家用第一指蓄力时，第二指镜头拖动可越过原版“dragging 时不滚屏”的门；仅用户授权的第二触点可使用该分支，自动跟随目标、AI 回合和关卡边界仍保持生产约束 | 用户授权的 Android 多指扩展 | `CanStartMobileTouchPan(true)`、`PanByMobileTouchDelta(..., true)` | 普通触点在 aiming 时仍被拒绝；明确标记的第二镜头触点可平移 | 已实现；C# 编译通过，待 Android 真机验证 |

桌面手动滚屏时的原版 `scroll1/scroll2` 箭头按 [CUR-SCROLL-01](../08-CursorsAndTrajectory/README.md) 验收；用户已撤回安卓拖动方向箭头，安卓只保留触摸平移，按 [AND-CUR-SCROLL-02](../08-CursorsAndTrajectory/README.md) 验收。

该条是移动端适配，不属于 Flash 原版规则变更。移动端镜头手势将另行定义，不能复用依赖“悬停”的桌面边缘滚屏。
