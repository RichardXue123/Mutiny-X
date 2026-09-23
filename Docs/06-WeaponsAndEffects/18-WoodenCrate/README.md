# 06.18 · Wooden Crate

[返回上级模块](../README.md)

## 职责

记录木箱的多次放置、合法性和销毁行为。

## 边界

参数和时序以原版证据为准。

## 逻辑摘要

- extent 为 left/top 16、right/bottom 15；默认 `createMore=2`，一次共放 3 个。
- 使用原始鼠标像素坐标，不自动吸附网格；位置内不能有 tile、箱、宝箱或角色。
- 待放置实体本身隐藏；合法区域显示原版木箱鼠标，非法区域切换为 `cross`，非法点击不放置也不消费库存。
- 木箱与火药桶共享原版全局 `boxes` 碰撞集合，不能互相重叠。
- 共享集合由关卡级 `MutinyBoxRegistry` 表示；角色跳跃和 `hitsBoxes=true` 的武器投掷均按 Solid 地形与箱体碰撞。
- 箱体碰撞候选若会把角色从当前合法位置反向推出到实体 tile 中，则保留该轴 tick 起点，避免爆炸/边角重叠后被箱子挤入地下。
- 跨回合保留并作为碰撞障碍；爆炸命中帧先结算角色伤害/冲量，随后同步从 boxes 移除木箱并直接跳到 `explode` 标签第 11 帧。
- 角色第一次按爆炸速度移动时，木箱已不再参与碰撞；角色飞行与木箱第 11～18 帧破损动画并行，木箱本身不再产生爆炸。
- 重开/下一关时，在新关构建前同步清空旧关卡全部箱体状态。
- 胜利、失败或平局进入结算时立即隐藏并销毁全部已放置/待放置木箱，同时同步退出共享碰撞注册。
- AI 与火药桶共用 `BoxWeapon` 合法位置候选；至少得到 3 个位置才加入候选，胜出后按 40 tick 放置、10 tick 间隔继续，直到三箱完成，期间回合不能提前收束。

来源：`BoxWeapon.as`、`WoodenCrate.as`。规则：`CRT-*`，见 [完整审计](../IMPLEMENTATION_DETAILS.md#614-wooden-crate)。

## 行为规格与实现映射

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| CRT-CUR-01 | 未完成三箱放置时显示木箱光标；`canPlace=false` 时显示 `cross` 并拒绝点击 | `TileSystem.as::advance`；cursor symbol 1813 frames 50/70；`BoxWeapon.as::advance` | `ResolveBoxPlacementCursorMode()`、`TryPlaceAt()` | 地形重叠点断言 Cross、数量和库存不变；合法点断言 WoodenCrate | 已实现；待 Unity 运行验证 |
| BOX-PLC-01 | 光标与点击都对当前链中第一个未放置对象执行合法性判断；已放置的根箱不能因“排除 requester”从光标预判中消失 | `TileSystem.as::advance`、`BoxWeapon.as::advance/canPlace` | `CanPlaceNext()`、`TryPlaceAt()` | 第一箱后把鼠标移回其位置，断言光标为 Cross 且点击同样拒绝 | 已实现；待 Unity 运行验证 |
| BOX-PLC-02 | 非法位置按下时整次输入请求被忽略；继续按住移到合法区也不补交，且此前已放盒子保持不变；必须松开后重新点击 | 原版运行行为；`BoxWeapon.as::canPlace` 的拒绝分支 | `ShouldHandleWeaponReadyPrimaryInput()`、`TryActivateClickWeapon()`、`TryPlaceAt()` | 第一箱后在其重叠位置按下，断言数量、注册盒和 pending 子箱均不变；另断言“仍按住但无新按下沿”不能重试，随后新一次合法点击才放置 | 已实现；待 Unity 运行验证 |
| BOX-SUP-01 | 搜索下方承托面时，箱体横向覆盖的任意一列存在承托即有效，不要求整个底边都有支撑 | `BoxWeapon.as::canPlace` 的内层横向扫描命中即 `break` | `CanPlace()` support scan | 仅左半边下方有实体 tile 时仍判定可放 | 已实现；待 Unity 运行验证 |
| CRT-SEQ-01 | 一次库存连续放置恰好 3 个，前两次提交后保持输入 | `BoxWeapon.as::place` | `HasPendingBoxPlacement()`、`OriginalPlacementCount=3` | 第一箱后在 `ActionExecuting` 断言输入仍开放，第三箱后断言总数为 3 | 已实现；待 Unity 运行验证 |
| BOX-WAIT-01 | 第一/第二箱后可无限等待下一次玩家点击；原版没有 150 tick/6 秒强制结束，非法点击和等待都不能销毁箱链 | `BoxWeapon.as::advance/place`；无超时分支 | `CanExpireFromTurnSafetyTimeout=false` | 第一箱后驱动 `MutinyTurnManager.AdvanceSimulationTick()` 超过 150 tick，断言箱链仍 pending 且后续两次合法点击可完成 3 箱 | 已实现；待 Unity 运行验证 |
| BOX-CAM-01 | 每次放箱在同一 `place()` 内令 `track=false`，不跟随已放箱且不设置回角色平移；等待下一箱时立即允许鼠标边缘、方向键和 WASD 自由滚屏 | `Weapon.as::place:140-148`；`BoxWeapon.as::place:93-100`；`TileSystem.as::advanceScrolling:455-493` | `IsAwaitingBoxPlacement`、`FindActionTarget()`、`CanUseManualScrolling()` | 第一箱后保持 `ActionExecuting`，断言无 action target、无角色回移目标且手动滚屏门开放 | 已实现；待 Unity 运行验证 |
| CRT-BOX-01 | 木箱作为共享 Solid 地形，阻挡角色跳跃和武器投掷 | `BoxWeapon.as::place`；`Solid.as::advanceMotion` | `MutinyBoxRegistry`、`MutinyPhysicsBody` | `BOX-COL-01` 驱动角色与炮弹实际物理 tick 撞箱 | 已实现；待 Unity 运行验证 |
| CRT-BOX-02 | 箱体推出与 tile 地形组成联合约束，箱体纠正不得把角色送入地面或墙体 | 原版 `Solid.as::advanceMotion` 的逐轴扫描；Unity 侧稳定性保护见 `BOX-COL-02` | `MutinyPhysics.Step` | 已落地箱体与角色重叠的向上 tick 仍报告 Ceiling，但角色底边不穿入地面 | 已实现；自动回归已写，待 Unity 运行验证 |
| CRT-RESET-01 | 重开/下一关时旧木箱不参与新关卡 | `TileSystem.as::readXML` line 72 | `ClearLevel()`、`BuildLevel()` | `BOX-LVL-01` 通过生产关卡构建入口验证同步清空 | 已实现；待 Unity 运行验证 |
| BOX-END-01 | 关卡胜利、失败或平局确认时，已放置及待放置木箱在结算弹窗出现前同步失活并清空碰撞注册 | `Controller.as::endGame → unloadLevel:88-120` | `EvaluateTurnOrGameOver()` → `ClearForLevelEnd()` | 生产胜/负判定后断言木箱对象失活且共享注册为空 | 已实现；自动回归已写，待 Unity 运行验证 |
| CRT-EXP-01 | 爆炸第 3 帧的同一次 `hit()` 中，先写角色伤害/击退，再把命中木箱从 `Controller.boxes` 移除；角色首次爆炸位移不会再撞到该箱 | `Explosion.as::hit`、`BoxWeapon.as::explode` | `MutinyExplosion.ApplyHit()` → `MutinyWoodenCrate.Explode()` | 同次生产命中后断言角色已有向上速度、注册箱数减一；再推进角色物理 tick，确认正常飞离 | 已实现；待 Unity 运行验证 |
| CRT-EXP-02 | `explode` 标签位于 symbol 965 第 11 帧，命中时立即显示第 11 帧；第 18 帧执行 `cl.destroy()`，破损动画与角色飞行并行 | symbol 965 SWF 时间轴；frame 18 `DoAction.as` | `OriginalExplodeFirstFrame=11`、`AdvanceExplosionTimelineFrame()` | 命中当刻断言 frame 11；同步推进后角色已移动且箱为 frame 12；第 18 帧隐藏 | 已实现；待 Unity 运行验证 |
| AI-WPN-05 | AI 至少得到 3 个合法 BoxWeapon 位置后才能选择木箱，并按原版延迟序列连续放置三箱 | `Character.as::aiThink`、`BoxWeapon.as::aiSimulation/aiPerform/aiContinue` | `MutinyAIController.EvaluateBoxWeapon()`、`BeginAiPlacement()`、`CheckAllBodiesAtRest()` | 固定种子生成候选并推进正式序列 40 tick，断言第一箱落地且回合仍等待后续箱 | 已实现；自动回归已写；待 Unity 运行验证 |
