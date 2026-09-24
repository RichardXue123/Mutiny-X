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
- `BOX-END-01`：在生产 `MutinyTurnManager` 中分别击败敌方和玩家方；每次都同时创建已注册箱体与未注册待放置 BoxWeapon，断言进入 `GameOver` 时 WoodenCrate、GunpowderBarrel 均同步失活且共享注册数归零。
- 当前状态：用例已加入 `MutinyTurnActionUiVerificationTest`，尚未在 Unity Play Mode 实际执行，不能登记为通过。

## Cannon 范围锚点回归

- `CAN-PLACE-01`：角色位于 `(100,200)` 时，通过生产 `MutinyCannon.PlacementCenterPixels` 与独立 `RangeCircle` Transform 断言范围中心均为 `(100,100)`，即原始 100 px 圆的底部落在角色坐标。
- `CAN-PLACE-03`：从炮身初始 `(100,190)` 按住并把指针快速移到 `(100,400)`，驱动生产 25 Hz tick；以 `(100,100)` 为中心的 120 px 约束应先把目标裁到 `(100,220)`，再按原版半距离移动至 `(100,205)`，并保持拖动资格。
- 当前状态：用例已加入 `MutinyTurnActionUiVerificationTest`；待 Unity Play Mode 实际执行，不能登记为通过。

## AI 武器候选缺口回归

- `AI-WPN-04`：通过 `EvaluateCharacterWeapons` 的生产分发与 Anchor 正式执行入口，在固定随机种子和地面上验证全图垂直采样会生成 Anchor 候选；胜出后创建已发射的正式 Anchor，并消费库存。
- `AI-WPN-05`：通过同一生产分发器验证 Wooden Crate 获得至少 3 个合法 BoxWeapon 位置后进入候选；随后启动正式 `BeginAiPlacement` 并推进原版 40 tick 延迟，断言第一箱落地且三箱序列仍处于活动状态。
- 当前状态：用例已加入 `MutinyTurnActionUiVerificationTest`；`Assembly-CSharp` 与 `Assembly-CSharp-Editor` 编译通过；尚未在 Unity Play Mode 实际执行，不能登记为通过。

## AI 原版预测与镜头回归

- `AI-PHY-01`：通过生产 `SimulateWeaponImpact` 验证 Rum Bottle 首次接触即结束，Boulder/普通 Weapon 在无专用完成条件时执行原版 101 个预测 tick，且普通武器不会因仅仅越过水线提前结束模拟。
- `AI-PHY-02`：长距离角色预测必须超过旧 70 tick 并持续到水线；注册共享箱体后，`hitsBoxes=true` 的 Cherry Bomb 候选必须在箱体处完成模拟。
- `AI-WPN-06`：生产 Seagull 距离公式在 20 px 时分别得到敌方 `+0.5`、己方 `-1.0`，40 px 边界为 0。
- `AI-CAM-01/02`：胜出角色通过生产镜头入口建立完成门；AI 控制与候选求值状态共同决定是否暂停宝箱等自动镜头分支。
- 当前状态：用例已加入 `MutinyTurnActionUiVerificationTest`；程序集编译通过；尚未在 Unity Play Mode 实际执行，不能登记为通过。

## Banana 轨迹一致性回归

- `BAN-PHY-01`：通过生产 `MutinyWeaponFactory.SpawnAndLaunch()` 提交 400 px 满拉力，断言 Banana 实际初速为原版 `twangMaxForce=30`，不再错误套用 `Weapon.release` 的 20 上限。
- `BAN-TRAJ-01`：以同一初速分别驱动生产预览 `PredictVelocityTick()` 和实际 `MutinyPhysicsBody.AdvanceSimulationTick()`，连续比较前 5 个无碰撞 tick 的坐标完全一致。
- 当前状态：用例已加入 `MutinyTurnActionUiVerificationTest`；待 Unity Play Mode 实际执行，不能登记为通过。

## Seagull 飞行与投弹时序回归

- `SEA-END-02`：在长地图上通过生产 `MutinyTurnManager.AdvanceSimulationTick()` 连续等待超过 150 tick，断言仍在正常飞行的海鸥不会被卡死武器看门狗强制完成或销毁。
- `SEA-SHOT-02`：点击只登记一次投弹请求；驱动一次生产 `MutinyPhysicsBody.AdvanceSimulationTick()` 后，断言海鸥先前进 10 px，炸弹从移动后位置的 `x-10` 创建，并在同一 tick 前进 10 px、下落 1 px。子弹物理体保持非自治更新，防止同帧双步。
- 当前状态：用例已加入 `MutinyTurnActionUiVerificationTest`；待 Unity Play Mode 实际执行，不能登记为通过。

## Voodoo Doll 运镜回归

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

## Android 镜头缺陷回归

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
