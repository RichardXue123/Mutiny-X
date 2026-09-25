# 11.01 · 自动回归

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

## Cannon 范围锚点回归

- `ANC-ANI-02/03`：通过生产 `MutinyPlayerInput` 投放 Anchor，逐 25 Hz tick 检查下落 frame 1、触地后主 frame 3 的双侧镜像 1002 碎屑、主 frame 12 停止后子帧继续、子 frame 17 移除，以及 30+10 tick 的 `Global.whiteOut` 乘色/加色/透明度。2026-09-26 Unity 6000.6.0f1 隔离 Play Mode 执行 `Validate Anchor Animation Play Mode`，9/9 断言通过；实际战斗画面待验收。

- `CAN-SMOKE-02`：发射正式炮弹、推进 25 Hz 物理 tick 与半 tick 表现采样；确认首团烟与炮弹显示起点重合，随后炮弹向前移动且烟保持在身后，权威位置仍独立推进。
- `CAN-AUD-02`：以正式物理地形接触和角色包围盒重叠分别触发炮弹爆炸，监听 `SfxPlayed`，再执行正式爆炸命中入口；地形接触 `pop` 总计一次，直接命中角色零次。
- 实际结果：2026-09-25，Unity 6000.6.0f1 隔离临时工程 Play Mode 执行 `Validate Cannon Effects Play Mode`，连同原有烟迹检查共 15/15 断言通过；主工程实机画面与听感仍待验收。
- `VIS-SMOKE-02`：2026-09-26 将 Cherry Bomb、Dynamite、Rum Bottle、Parachute Bomb 的生产出烟回调对齐当前物理 tick 的可见起点。使用正式 `MutinyPhysicsBody.AdvanceSimulationFrameForVerification`，分别在飞行和 ready 状态核对新烟团与首个可见弹体位置一致、半 tick 后弹体前进而烟团静止；复跑 `Validate Cannon Effects Play Mode`，Unity 6000.6.0f1 隔离工程 19/19 断言通过。主工程 PIE 逐武器画面验收尚未运行。
- `CAN-AI-TURN-01`：经 `MutinyAIController.ExecuteMove` 同源执行入口提交大炮，并交替推进正式 `MutinyTurnManager.AdvanceSimulationTick` 与大炮 25 Hz tick；前 24 tick 不得换回合或积累静止计数，第 25 tick 生成炮弹且镜头目标为该炮弹；炮弹未结束时再等待 151 tick 不得被通用安全超时强制结束，然后驱动正式炮弹物理跨出原版边界、炮身结束，最后才通过通常 11 tick 静止门。用例已添加，待 Unity 运行验证。
- `CAN-PLACE-01`：角色位于 `(100,200)` 时，通过生产 `MutinyCannon.PlacementCenterPixels` 与独立 `RangeCircle` Transform 断言范围中心均为 `(100,100)`，即原始 100 px 圆的底部落在角色坐标。
- `CAN-PLACE-03`：从炮身初始 `(100,190)` 按住并把指针快速移到 `(100,400)`，驱动生产 25 Hz tick；以 `(100,100)` 为中心的 120 px 约束应先把目标裁到 `(100,220)`，再按原版半距离移动至 `(100,205)`，并保持拖动资格。
- 当前状态：用例已加入 `MutinyTurnActionUiVerificationTest`；待 Unity Play Mode 实际执行，不能登记为通过。

## AI 武器候选缺口回归

- `GM-07`：通过 GM 正式解析入口覆盖 AI 武器候选；验证无库存也能评估强制武器、首行动仍保留跳跃、续行动无合格武器候选时仍可 Pass、正式开火不扣真实弹药、`0` 恢复库存选择、非法编号不改设置及 1..15 菜单映射。此项是 Unity 调试扩展，不是原版一致性规则。2026-09-25 在当前工程 Unity Play Mode 通过专项回归 `7/7`；“有候选但评分非正”的续行动分支尚未由此专项用例单独覆盖。

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
- `EXT-JUMP-CAN-01`：进入实际 Aiming 状态后调用与鼠标右键共用的取消处理，断言轨迹蓄力撤销、回到跳跃待命、取消叉恢复且 `CanThrow` 未消耗。
- 当前状态：用例已加入 `MutinyTurnActionUiVerificationTest`，尚未在 Unity Play Mode 实际执行，不能登记为通过。

## 高刷新率镜头跟随回归（feature/cameramovement）

- `CAM-PRES-01`：驱动生产 `MutinyPhysicsBody.AdvanceSimulationFrame` 与显示入口，验证角色/武器显示位置由相邻已完成 tick 插值，权威 `State` 不随中间渲染帧改变；外部改位不复用旧轨迹。
- `CAM-PRES-02`：驱动生产 `MutinyCameraController.AdvanceCamera`，验证跳跃角色、海鸥、海啸、炮弹的镜头目标与其显示位置同源；60/120 FPS 中间帧仍前进，远距离跟随不超过 30 px/tick；空投优先于武器并保持 50 px/tick 切入。
- `CAM-TRACK-SEA-01`：验证海鸥使用原版 `trackY=y+100` 偏移。
- 实际结果：Unity 6000.6.0f1 独立临时工程 Play Mode 批处理运行 `Validate Camera Movement`，11/11 断言通过；两个 C# 工程编译通过。该结果证明数值及生产方法路径，不等于主工程真实 25/60/120 FPS 画面验收。

## Android 镜头缺陷回归

- `CUR-SCROLL-01/AND-CUR-SCROLL-01`：通过正式镜头箭头资源解码、八方向映射、视口边缘定位和移动平移核心，核对 31×22 原版帧、方向角、宽屏黑边输入门、实际位移方向及边界阻挡时隐藏。2026-09-25 Unity 6000.6.0f1 隔离临时工程 Play Mode 专项 `Validate Scroll Arrows Play Mode` 通过 5/5 断言；桌面实际鼠标画面与 Android 单指/第二触点真机绘制尚未运行。
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

## 双人关卡资源缺失回归（2P-ASSET-01）

- 缺陷：双人选关第 19 关起显示 `data unavailable`。生产 `HasNumberedLevelData` 从 `Resources/Data/Levels/level_NN.xml` 查找，而该目录此前仅有 01–18。补入按原版哈希名取得的 19–33 原始 XML 后，用 `MutinyTwoPlayerVerificationTest` 从生产选择资格和 `TryLoadLevel` 逐关核对 16–33：关卡编号不串关、红蓝双方存在且存活、菜单双人会话下双方均为人类、天色按编号匹配；另断言第 34 关被拒绝且当前关保持不变。
- 原件与运行资源的 URL、SHA-256 和 `players` 见 [原版清单](../../10-OriginalEvidence/Artifacts/TwoPlayerLevels/catalog.csv)。
- 实际结果：2026-09-25，Unity 6000.6.0f1 主工程编辑器菜单 `Mutiny → Parity → Validate Two Player Mode` 通过 **52/52** 断言；此数字含其他双人 Flow/结算断言。`Application.isPlaying` 专属回合/输入检查在这次编辑器态运行中跳过，实际 UI 点击和完整 Play Mode 对局待验收。
