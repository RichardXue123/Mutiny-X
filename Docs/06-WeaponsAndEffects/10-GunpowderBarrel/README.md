# 06.10 · Gunpowder Barrel

[返回上级模块](../README.md)

## 职责

记录火药桶的放置、碰撞和连锁爆炸行为。

## 边界

参数和时序以原版证据为准。

## 逻辑摘要

- 继承 BoxWeapon；extent 为 left/top 16、right/bottom 15。
- `createMore=1`，因此一次选择总共放 2 个，而不是 1 个或 3 个。
- `limitedToTurn=false`；合法位置排除 tile、箱、宝箱和角色。
- 待放置实体本身隐藏；合法区域显示原版火药桶鼠标，非法区域切换为 `cross`，点击只拒绝放置且不消费库存。
- 第一桶放下后即使回合进入 `ActionExecuting`，仍保持输入直到第二桶完成。
- 与木箱共同注册到关卡级 `MutinyBoxRegistry`；角色跳跃及 `hitsBoxes=true` 的投掷物都按 Solid 地形碰撞。
- 重开/下一关的生产构建入口会同步清空注册表，不等待旧关卡对象的延迟 `OnDestroy`。
- 胜负 speech 和结算弹窗期间保留已放置火药桶及其碰撞；重开、下一关或退出当前对局卸载关卡时才销毁旧桶并清空注册。
- 被爆炸命中后从 boxes 移除，并生成 caster=null 的 `150/30` 连锁爆炸。

来源：`GunpowderBarrel.as`、`BoxWeapon.as`、`Explosion.as`。规则：`GPB-*`，见 [完整审计](../IMPLEMENTATION_DETAILS.md#66-gunpowder-barrel)。

## 行为规格与实现映射

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| GPB-CUR-01 | 未完成两桶放置时显示火药桶光标；`canPlace=false` 时显示 `cross` 并拒绝点击 | `TileSystem.as::advance`；cursor symbol 1813 frames 11/70；`BoxWeapon.as::advance` | `ResolveBoxPlacementCursorMode()`、`TryPlaceAt()` | 地形重叠点断言 Cross、数量和库存不变；合法点断言 GunpowderBarrel | 已实现；待 Unity 运行验证 |
| BOX-PLC-01 | 光标与点击统一检查当前待放置桶，而不是已经注册到 boxes 的根桶 | `TileSystem.as::advance`、`BoxWeapon.as::advance/canPlace` | `CanPlaceNext()`、`TryPlaceAt()` | 第一桶后原位置同时显示 Cross 并拒绝第二桶 | 已实现；待 Unity 运行验证 |
| BOX-PLC-02 | 非法位置按下会忽略整次请求，不因继续按住并移动而摆放，也不清除此前已放桶；合法位置需要新的点击 | 原版运行行为；`BoxWeapon.as::canPlace` 的拒绝分支 | `ShouldHandleWeaponReadyPrimaryInput()`、`TryActivateClickWeapon()`、`TryPlaceAt()` | 第一桶后在重叠位置点击，断言桶链和注册状态不变；共享输入门要求新的按下沿 | 已实现；待 Unity 运行验证 |
| BOX-WAIT-01 | 第一桶后可无限等待第二次点击；原版没有 150 tick/6 秒安全超时，等待或非法点击不能强制结束桶链 | `BoxWeapon.as::advance/place`；无超时分支 | `MutinyTurnManager.AdvanceSimulationTick` | 第一桶后确认不会被回合安全超时判为卡死武器，随后仍可放置第二桶 | 已实现；待 Unity 运行验证 |
| BOX-CAM-01 | 每次放桶立即清除 `track`，不跟随桶、不强制回角色；等待第二桶期间直接允许边缘、方向键和 WASD 滚屏 | `Weapon.as::place`；`BoxWeapon.as::place`；`TileSystem.as::advanceScrolling:455-493` | `IsAwaitingBoxPlacement`、`FindActionTarget()`、`CanUseManualScrolling()` | 第一桶后在 `ActionExecuting` 断言无自动目标且手动滚屏可用 | 已实现；待 Unity 运行验证 |
| BOX-SUP-01 | 横向范围内部分下方有承托面即可，不要求全宽支撑 | `BoxWeapon.as::canPlace` | `CanPlace()` support scan | 单列承托 tile 判定合法 | 已实现；待 Unity 运行验证 |
| GPB-SEQ-01 | 第一桶提交后保持玩家输入，第二桶完成后才结束序列；一次库存恰好放 2 桶 | `GunpowderBarrel.as`；`BoxWeapon.as::place` | `HasPendingBoxPlacement()`、`OriginalPlacementCount=2` | 第一桶后在 `ActionExecuting` 断言输入仍开放，第二桶后断言总数为 2 | 已实现；待 Unity 运行验证 |
| GPB-BOX-01 | 火药桶与木箱进入同一个 `Controller.boxes`，阻挡角色和武器 Solid | `BoxWeapon.as::place`；`Solid.as::advanceMotion` | `MutinyBoxRegistry`、`MutinyPhysicsBody` | `BOX-COL-01` 驱动角色与炮弹生产物理入口撞箱 | 已实现；待 Unity 运行验证 |
| GPB-RESET-01 | 每次关卡重建前清空旧箱体状态 | `TileSystem.as::readXML` line 72 | `ClearLevel()`、`BuildLevel()` | `BOX-LVL-01` 在旧箱仍等待销毁时构建新关并断言注册表为空 | 已实现；待 Unity 运行验证 |
| LVL-PERSIST-01 | 关卡胜负 speech 和结果弹窗期间，已放置火药桶保持显示与碰撞，待放置桶也不因结算提前销毁 | `Controller.as::nextTurn/enterFrame` 不卸载关卡 | `MutinyTurnManager.EvaluateTurnOrGameOver()` | 生产胜/负判定后断言桶对象仍有效且共享碰撞注册保留 | 已实现；Play Mode 回归通过 |
| LVL-RESET-01 | 重开、下一关或退出对局卸载时才销毁旧桶 | `Controller.as::changeLevel/endGame → unloadLevel`；`TileSystem.as::readXML` | `MutinyLevelController.ClearLevel()`、`MutinyLevelBuilder.BuildLevel()` | 生产加载下一关后断言旧桶失活、注册为空 | 已实现；Play Mode 回归通过 |
