# 09.04 · 镜头

[返回上级模块](../README.md)

## 职责

管理跟随、边缘滚屏、空投运镜、震动和目标优先级。

## 边界

镜头不影响逻辑坐标。

## 行为规格

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
| CAM-BARS-01 | 在 `OnGUI()` 的 `Repaint` 事件中精确绘制视口外侧纯黑矩形（`GUI.DrawTexture`），杜绝黑边区域任何残影或未清屏像素 | 原版全屏/黑边表现 | `MutinyCameraController.DrawLetterboxBars` | 视口外边距覆盖率 100% | 已实现；编译通过 |

## Android 授权适配

| ID | 可观察行为 | 来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| AND-CAM-01 | Android 等移动平台没有桌面悬停光标；无触摸时的空闲/默认指针位置不得触发屏幕边缘滚屏。桌面端鼠标边缘滚屏保持原版行为 | 用户报告：Android 开场镜头持续左移；现有 `Munity.apk` 的 IL2CPP 产物 `Assembly-CSharp__3.cpp:20371-20443` 确认直接读取 `Mouse.current.position`，且无移动平台门禁 | `MutinyCameraController.AdvanceEdgeScrolling`、`ShouldUseMouseEdgeScrolling` | 生产判定在 `isMobilePlatform=true` 且存在 pointer/mouse 状态时仍返回 false；桌面真实鼠标返回 true | 已实现；`Assembly-CSharp` 与 `Assembly-CSharp-Editor` 编译通过；Android 真机运行待验证 |
| AND-CAM-02 | Android 空白处单指拖动提供桌面边缘滚屏的移动端替代操作；手指拖动地图内容，镜头作反向等比例移动，并继续使用生产镜头边界 | 用户授权的 Android 镜头适配要求 | `CanStartMobileTouchPan`、`PanByMobileTouchDelta`、`ScreenDeltaToWorldDelta` | 以不同屏幕宽高换算拖动量，断言相机正交可视范围与拖动比例一致；生产入口调用 `SetClampedPosition` | 已实现；C# 编译通过，待 Android 真机验证 |
| AND-CAM-03 | 玩家用第一指蓄力时，第二指镜头拖动可越过原版“dragging 时不滚屏”的门；仅用户授权的第二触点可使用该分支，自动跟随目标、AI 回合和关卡边界仍保持生产约束 | 用户授权的 Android 多指扩展 | `CanStartMobileTouchPan(true)`、`PanByMobileTouchDelta(..., true)` | 普通触点在 aiming 时仍被拒绝；明确标记的第二镜头触点可平移 | 已实现；C# 编译通过，待 Android 真机验证 |

该条是移动端适配，不属于 Flash 原版规则变更。移动端镜头手势将另行定义，不能复用依赖“悬停”的桌面边缘滚屏。
