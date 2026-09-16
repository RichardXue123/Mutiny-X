# WPN-06 Gunpowder Barrel（火药桶）行为规格

- 模块 ID：WPN-06；原版 SWF SHA256：`c034d2cfa50272a1aac93ad75114b542ca5993dd7b70fa8901aa195de7820586`。
- 证据：静态确认；实现：实现中；验证：未执行。

| 证据 | 原版定位 | 确认结论 |
|---|---|---|
| W06-S-01 | `BoxWeapon.as` constructor/place/canPlace | left/top=16、right/bottom=15、hitsBoxes=true、draggable=false、showCircle=false；放置即清 owner 两行动；GunpowderBarrel 每次序列只额外创建一个子桶，合计两桶。 |
| W06-S-02 | `BoxWeapon.as canPlace` | 中心 32px AABB 不得与 tile、现有 box、chest 或活角色冲突；按下非法位置不放置。 |
| W06-S-03 | `BoxWeapon.as advance/advanceMotion` | 人类依次点击可放置位置；根桶推进子桶，最后子桶 finished 后根桶 finished。原版桶可跨回合存续，且 `BoxWeapon` 未走 `Weapon.splashCheck`。 |
| W06-S-04 | `GunpowderBarrel.as explode` | explode 先从 box registry 移除并播放 `explode` 时间轴，再创建 `Explosion(x,y,150,30,null)`；caster 必须为 null。 |
| W06-S-05 | `BoxWeapon.as aiSimulation/aiPerform` | AI 从候选位置取最多三项，40 tick 后逐桶放置，每桶后等 10 tick，再继续；位置会向上尝试偏移。 |
| W06-A-01 | `mutiny.swf.xml` sprite 968、`DefineSprite_968_gunpowderBarrel` | 12 帧原版资源已在 `Resources/Art/Weapons/GunpowderBarrel/1..12.png`；帧 1–11 是静态 child，`explode` 标签在第 11 帧，帧 12 移除 child 并执行 destroy。 |

| 规则 | 状态转换 / 可观察结果 |
|---|---|
| WPN-06-INT-01 | 选中后建立未放置根桶；第一次成功点击扣一库存、耗尽 owner 两行动，第二次成功点击放置子桶。 |
| WPN-06-EFF-01 | 两个桶都为 16/15px、terrain/box 碰撞实体；非法点击保持当前桶和库存。 |
| WPN-06-EFF-02 | 任何原版 Explosion 命中桶时，桶自移除并产生独立 150/30、null-caster 爆炸，可连锁。 |
| WPN-06-ANI-01 | 静态显示第 1 帧；受击立即切到 `explode` 标签第 11 帧，下一 25 Hz tick 进入移除帧，随后销毁完成的末桶。 |
| WPN-06-AI-01 | AI 的 40/10 tick 两桶链路。 |

## Unity 实现映射

| 原版入口 | Unity 入口 | 本次实现 |
|---|---|---|
| `BoxWeapon.place/canPlace` | `MutinyGunpowderBarrel.TryPlaceAt/CanPlace`、`MutinyPlayerInput.TryActivateClickWeapon` | 两次原始像素坐标点击；第一次扣一库存并由放置入口提交回合；tile、chest、活角色及两类 box 都拒绝重叠。 |
| `Controller.boxes` | `MutinyGunpowderBarrel.GetPhysicsObstacles`、`MutinyPhysicsBody.AdvanceSimulationTick` | 木箱和火药桶合并为同一碰撞障碍集合。 |
| `GunpowderBarrel.explode` | `MutinyGunpowderBarrel.Explode`、`MutinyExplosion.ApplyHit` | 先注销桶，再切到帧 11，产生 150/30/null-caster Explosion；后者可继续触发另一桶。 |
| `BoxWeapon.aiSimulation/aiPerform/aiContinue` | `MutinyAIController.EvaluateGunpowderBarrel/ExecuteMove`、`MutinyGunpowderBarrel.BeginAiPlacement` | 十次候选抽样、前三候选、40 tick 首次放置、每桶后 10 tick，并保留向上 48 px/每轮 32 px 搜索。 |

## 验收记录

| 用例 | 生产入口与断言 | 实际结果 |
|---|---|---|
| W06-T-01 | `MutinyTurnActionUiVerificationTest.VerifyGunpowderBarrel` 经菜单选择和生产 click 路径，非法 tile 点击后两次合法放置；断言一次库存、两行动、两桶和 ActionExecuting。 | 已加入；Unity Editor 未执行。 |
| W06-T-03 | 同一生产回归从外部 Explosion 命中第一桶，取其 150/30/null-caster 爆炸并命中第二桶；同时断言 explode 第 11 帧。 | 已加入；Unity Editor 未执行。 |
| W06-T-02/04/05 | 角色、chest、木箱边界；跨回合；AI 的 40/10 tick。 | 待运行验证。 |

2026-09-16 静态确认和实现完成；`dotnet build Assembly-CSharp.csproj --no-restore` 通过（项目既有 `MutinyLevelTest.levelXml` 未赋值警告）。Unity Play Mode、AI 实战、视觉/音效与原版逐 tick 对照均未执行。
