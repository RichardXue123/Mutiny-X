# 06.09 · Cannon

[返回上级模块](../README.md)

总览规则和跨武器差异另见 [武器逻辑实现细节](../IMPLEMENTATION_DETAILS.md#65-cannon--cannonball)。

## 职责

记录大炮的范围预览、碰撞摆放、拉栓蓄力、炮弹发射、镜头跟踪和结束流程。

## 原版来源

- `Cannon.as:31-178`：大炮构造、拖动、拉栓、开火和淡出。
- `Cannonball.as:23-127`：炮弹物理、碰撞、烟雾、爆炸和越界。
- `Solid.as:130-379`：拖动物体时使用速度推进并进行地形/箱体碰撞。
- `TileSystem.as:291-310`：大炮范围圆的显示、位置、缩放和淡入淡出。
- `TileSystem.as:732-763`：鼠标拖动与松开入口。
- `Controller.as:56`：创建独立的 `rangeCircle` 实例。
- `placements.csv`：大炮 symbol 850 内含名为 `pin` 的独立子节点 847，初始 X 为 -18 px。
- `sprite-origins.csv`：大炮为 53×38 px，炮弹为 20×20 px，范围圆和拉栓均有独立资源。

原版证据根目录：`Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/`。

## 原版流程

1. 选中大炮后，在角色 `(x, y-10)` 创建大炮。范围圆和实际部署约束都以 `(owner.x, owner.y-100)` 为中心，即原始 100 px 圆的底部落在角色坐标；不能把角色本身误当作圆心。
2. 原始范围圆的可见半径为 100 px，`dragRange=130` 是 Flash 的 130% 缩放，所以 Unity 显示的可见半径保持 130 px。
3. 范围圆是 Controller 层的独立对象，不应继承炮身的移动或旋转；每 tick 以 20 alpha 淡入，大炮发射后以相同速度淡出。
4. 点击大炮本体 20 px 范围并持续按住左键开始移动；点击 `pin` 的 8 px 范围开始拉栓，拉栓优先于本体拖动。
5. 移动大炮时，通过 `Solid.advanceMotion()` 每个原版 tick 向鼠标移动剩余距离的一半，并使用默认 10 px 四向 extent 与地形、木箱/火药桶碰撞。释放发生在两个 25 Hz tick 之间、且最新鼠标位置尚未被 tick 消费时，补执行一次同源碰撞步；已消费的位置不会在松开时重复移动。
6. 请求位置先限制到以 `(owner.x, owner.y-100)` 为中心的 120 px 圆内，再执行碰撞移动；不能先移动后直接把炮身坐标钳回圆内，否则可能穿墙。原版 `Solid.as` 在采样指针越过 130 px 时自动解除拖动；现代鼠标快速移动时这会造成非预期脱手，因此 Unity 授权差异为“越界钳制、保持抓取，直到真实 MouseUp”。
7. 拉栓时整门炮转向鼠标反方向，`pin._x` 限制在 `[-40,-21]`。
8. 松开时只有 `pin._x < -30` 才装填 30 力；之后拉栓每 tick 回弹 15 px，回到 -21 时开火。
9. 开火会在大炮当前位置创建一个独立 `Cannonball`。炮弹以 30 px/tick 发射，大炮本体留在原地，并播放原版逻辑音效 `cannon explosion`；Unity 导入资源键为 `cannon_explosion`。
10. 大炮先保持完全可见 10 tick，再淡出 10 tick；炮弹结束且大炮完全透明后，武器流程结束。
11. 镜头跟随炮弹坐标。原版通过更新大炮的 `trackX/trackY` 实现，不修改大炮的 `x/y`。

## 行为规格与实现映射

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| CAN-PLACE-01 | 提示圆与实际部署区域统一以 `(owner.x, owner.y-100)` 为中心，使原始 100 px 圆的底部位于角色坐标；可见资源按原版缩放为 130 px 半径，提示层不继承炮身变换 | `TileSystem.as:291-310`、`Cannon.as:93-100`、symbol 1900 | `GetPlacementCenterPixels()`、独立 `RangeCircle` 对象 | `VerifyCannon()` 校验角色 `(100,200)` 对应圆心 `(100,100)`，且越界拖动按该中心的 120 px 约束裁剪 | 已实现；自动回归已写，待 Unity 运行验证 |
| CAN-PLACE-02 | 左键选中炮身并持续按住时，每 tick 向鼠标移动一半距离；短拖释放时提交尚未消费的最后一个碰撞步；全程参与地形、木箱及火药桶碰撞 | `Solid.as:130-379` | `TryBeginBodyDrag/DragBodyTo/ReleasePointer` → `AdvanceBodyDragOriginalTick()` | 短拖在一个 tick 内仍移动；在 x=128 实心墙前，10 px 炮身中心停在 x=117.9 | 已实现；待 Unity 运行验证 |
| CAN-PLACE-03 | 炮身目标限制在以 `(owner.x,owner.y-100)` 为中心的 120 px 圆内；快速指针采样越界时钳到圆周但持续保持拖动，真实松开左键才结束。原版越过 130 px 自动释放，作为明确授权差异保留记录 | `Cannon.as:91-101`、`Solid.as:138-160`、本次缺陷反馈 | `ClampToPlacementCircle()`、`ReleasePointer()` | 把鼠标一步移动到范围外，断言炮身仍处于拖动状态，且首个半距离 tick 朝该圆周目标移动 | 已实现；自动回归已写，待 Unity 运行验证 |
| CAN-PIN-01 | 炮身与拉栓是独立显示层；拖动拉栓时其局部 X 随 `PinX` 变化 | symbol 850、child 847、shape 849 | `BuildOriginalVisualLayers()`、`UpdatePinVisual()` | 校验炮身 46×38、拉栓 35×16，拉至 -30 时 Transform 位于 -30 px | 已实现；待 Unity 运行验证 |
| CAN-PIN-02 | `PinX` 限制在 `[-40,-21]`；只有 `<-30` 才装填；以 15 px/tick 回弹并以 30 力发射 | `Cannon.as:123-163` | `DragPinTo()`、`ReleasePointer()`、`AdvanceOriginalTick()` | 等于 -30 不提交，-31 提交并产生 30 力炮弹 | 已实现；待 Unity 运行验证 |
| CAN-FIRE-01 | 开火在炮口位置创建独立 Cannonball，炮身留在摆放位置 | `Cannon.as:167-178` | `FireCannon()` | 手动推进炮弹后断言炮弹已移动而炮身坐标未改变 | 已实现；待 Unity 运行验证 |
| CAN-AUD-01 | 炮弹创建时播放一次 `cannon explosion`；Unity 资源名映射为 `cannon_explosion.wav` | `Cannon.as:177`、音频导出表 | `FireCannon()` → `PlaySFX(FireSoundName)` | 监听 `SfxPlayed` 并断言收到 `cannon_explosion` | 已实现；待 Unity 运行验证 |
| CAN-CAM-01 | ActionExecuting/Settling 阶段镜头跟随独立炮弹 | `Cannon.as:45-53` | `MutinyCameraController.FindActionTarget()` | `FindActionTargetForVerification()` 必须返回 `Cannonball.transform` | 已实现；待 Unity 运行验证 |
| CAN-LIFE-01 | 炮身保持全不透明 10 tick，再淡出 10 tick；炮弹结束且炮身透明后完成 | `Cannon.as:71-87` | `MutinyCannon.AdvanceOriginalTick()` | 验证第 19 个淡出 tick 尚未完成，第 20 个完成 | 已实现；待 Unity 运行验证 |
| CAN-AI-01 | AI 候选摆放先用炮身碰撞修正，再从修正位置模拟炮弹 | `Cannon.as:179-225` | `MutinyAIController.EvaluateCannon()`、`MutinyCannon.BeginAiFire()` | AI 请求穿过 x=128 墙体时炮身同样停在 x=117.9 | 已实现；待 Unity 运行验证 |
| CAN-AI-02 | AI 的摆放方向和发射方向使用两次独立随机角度 | `Cannon.as:190`、`Cannon.as:199` | `MutinyAIController.EvaluateCannon()` | 静态检查两个独立 `Random.Range(0,360)` 采样 | 已实现；静态确认 |

## 修复边界

- 大炮本体和炮弹必须保持为两个独立对象。
- 大炮本体使用独立炮身与拉栓显示层，不能继续用包含静止拉栓的扁平合成图完成动态拉栓。
- 摆放运动必须走与原版 `Solid.advanceMotion()` 同源的碰撞计算。
- 范围内钳制必须发生在碰撞移动之前，不能用最终位置直写绕过墙体。
- 镜头需要读取炮弹目标，不能通过移动大炮本体来间接跟踪。

## Unity 资源

- `Assets/Mutiny/Resources/Art/Weapons/Cannon/Body.png`：原版 shape 849，46×38，注册点 `(19,19)`。
- `Assets/Mutiny/Resources/Art/Weapons/Cannon/Pin.png`：原版 sprite 847，35×16，注册点 `(8,8)`。
- `Assets/Mutiny/Resources/Art/Weapons/Cannon/RangeCircle.png`：原版 sprite 1900，365×365；可见圆直径 200 px，运行时缩放到 1.3。
- 旧 `1.png` 是 53×38 合成参照图，不再用于运行时炮身显示。

## 验证状态

- 原版 AS2、资源层级和数值：静态确认。
- Unity C# 编译：`dotnet build Assembly-CSharp.csproj --no-restore` 通过；仅有既存 `MutinyLevelTest.levelXml` 未赋值警告。
- `VerifyCannon()` 已补生产入口回归，但本次没有在 Unity 中执行。
- 范围锚点已按原版恢复为 `owner.y-100`；“快速越界仍保持抓取”仍是为避免高速鼠标脱手而保留的授权差异。
- Unity Play Mode 与原版运行画面对照：待手动验证。
