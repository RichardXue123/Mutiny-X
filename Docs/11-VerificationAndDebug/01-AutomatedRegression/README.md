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
- 当前状态：用例已加入 `MutinyTurnActionUiVerificationTest`，尚未在 Unity Play Mode 实际执行，不能登记为通过。

## Mine 缺陷回归

- `MIN-ANI-01/AUD-01`：通过生产选中/投掷入口确认 Mine 投掷无音效请求，随后逐原版 tick 验证 frame 1、arm 11..16、warn 21..29 和十次 `mine_beep`。
- `MIN-TRG-01/MIN-AIM-01`：通过 PlayerInput 的实际 Throw Self 拉线入口触发静止角色附近的 Mine；倒计时爆炸后确认角色已在空中、输入仍处于 Aiming，并可调用生产提交入口完成 Throw Self。
- `MIN-TRG-02`：让角色携带非零速度进入半径，验证被动移动同样启动 warning/countdown。
- 当前状态：用例已加入 `MutinyTurnActionUiVerificationTest`，尚未在 Unity Play Mode 实际执行，不能登记为通过。

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
