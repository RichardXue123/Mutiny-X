# WPN-13 — Voodoo Doll（巫毒娃娃）

基线：原版 SWF `mutiny.swf`，SHA256 `c034d2cfa50272a1aac93ad75114b542ca5993dd7b70fa8901aa195de7820586`。逻辑 tick 为 25 Hz；位置、速度和阈值均为 Flash 像素。

## 证据与范围

| 证据 ID | 类型 | 原版定位 | 结论 |
| --- | --- | --- | --- |
| WPN-13-S-01 | S | `VoodooDoll.as` 构造、`setTargetCharacter`、`twang`、`advance`、`aiPerform` | 默认四向 extent 为 `Solid` 的 10 px；先选目标才可拉动；记录释放速度；投掷 10 tick 后转镜头，目标镜头到位后再等 10 tick；一次性把记录速度交给目标，娃娃随后每 tick 淡出 10%。 |
| WPN-13-S-02 | S | `TileSystem.as` 约 366–413、673–700 行 | 人类目标候选仅来自敌队存活角色；鼠标到候选的平方距离严格小于 900（半径 30 px）。未发射且没有目标时单击目标，调用 `setTargetCharacter` 并播放 `voodoo`。 |
| WPN-13-S-03 | S | `Character.as` 约 121–150、287–290、580–605 行 | 目标选择阶段敌人可被点击；目标或 30 px 内悬停敌人显示 `overlay.target`，并隐藏角框。AI 对每个存活敌人抽两条轨迹，以其最终落水与否评分。 |
| WPN-13-A-01 | A | `mutiny.swf.xml` sprites 1027、1871、1872、1884 | 娃娃为 symbol 1027；目标十字为 symbol 1872。shape 1871 bounds 为 `x=-20..21/y=-21..21` px，故 41×42 导出图使用 x pivot `20/41`；它位于 overlay `(−1,0)` px；overlay 1884 共有 30 帧。 |
| WPN-13-A-02 | A | `Art/raster/sprites/DefineSprite_1027_voodooDoll/1.png`、`DefineSprite_1872/1.png` | 导出的娃娃和目标十字原图可直接作为 Unity Sprite。 |

## 状态与转换

| 规则 ID | 前置状态 | 输入与允许条件 | 状态变化与 tick | 可观察结果 | Unity 入口 |
| --- | --- | --- | --- | --- | --- |
| WPN-13-INT-01 | 当前人类角色拥有 `voodooDoll` 且可射击 | 从行动菜单选择武器 | 创建未发射娃娃；不可拉动；库存不变 | 进入敌人目标选择，敌人可悬停 | `MutinyPlayerInput.SelectWeapon` |
| WPN-13-INT-02 | 未发射、尚无目标 | 单击存活敌人，距离严格 `< 30 px`，且不是娃娃本身命中区域 | 设置目标并允许拉动；播放一次 `voodoo` | 悬停/已选敌人显示原版 target 十字；角框隐藏 | `MutinyPlayerInput`、`MutinyCharacterOverlay`、`MutinyVoodooDoll.BindTarget` |
| WPN-13-INT-03 | 已选目标且娃娃未发射 | 从娃娃位置拖拽并释放 | `Weapon.release` 速度上限 20 px/tick；消耗一次库存，角色两种行动均结束 | 娃娃按 `rotation += velocityX * 2` 飞行 | `MutinyVoodooDoll.Twang/Fire` |
| WPN-13-EFF-01 | 娃娃已发射 | 每个 25 Hz tick | 前 10 tick 仍跟随娃娃；第 10 tick 请求镜头转向目标；镜头完成后计 10 tick | 镜头先跟随娃娃，再切换目标 | `MutinyVoodooDoll.AdvanceOriginalTick`、`MutinyCameraController` |
| WPN-13-EFF-02 | 目标镜头到位后等待完成 | 下一 tick | 一次性将发射时保存的 `(vx,vy)` 写入目标角色；不直接扣血 | 目标被按同一方向击飞 | `MutinyVoodooDoll.TransferVelocityToTarget` |
| WPN-13-ANI-01 | 目标已接收速度 | 每 tick | 娃娃 alpha 减 10；到 0 时完成 | 娃娃逐帧淡出 | `MutinyVoodooDoll.AdvanceOriginalTick` |
| WPN-13-INT-04 | 未发射的目标选择或瞄准阶段 | 右键取消、目标死亡、无合法目标 | 取消销毁未发射娃娃且不扣库存；不可发射 | 回到行动菜单 | `MutinyPlayerInput.CancelWeaponSelection` |

## 明确边界

- AS2 的 `VoodooDoll.advance()` 覆盖 `Weapon.advance()`，未调用通用的水面结束、静止结束或超时结束逻辑；娃娃触水本身不能令目标溺水。Unity 不得把当前娃娃瞬时速度持续复制给目标。
- AI `aiPerform()` 不经过人类的目标单击流程，但会设置目标、记录速度并播放一次 `voodoo`。
- 原版记录的目标镜头完成条件依赖 `TileSystem.panToCharacter` 的实际镜头状态；Unity 相机边界钳制下的逐 tick 对照尚未执行。

## 验收用例与状态

| 用例 ID | 规则 | 前置与操作 | 独立期望 | 验收层 | 当前状态 |
| --- | --- | --- | --- | --- |
| WPN-13-VER-01 | INT-01/02/04 | 人类选中娃娃；在 29、30、31 px 的敌人位置单击；再取消 | 仅 29 px 设目标并允许拉动；取消不扣库存 | 输入/场景 | 未执行 |
| WPN-13-VER-02 | EFF-01/02/ANI-01 | 生产工厂创建并发射已绑定目标的娃娃，逐 tick 驱动 | 10 tick 后请求目标镜头；镜头完成后等 10 tick；仅一次传速；alpha 每 tick −0.1 | 逻辑/模拟 | 未执行 |
| WPN-13-VER-03 | INT-02 | 未发射娃娃悬停/选中敌人 | target 十字出现、普通角框隐藏；发射后消失 | 输入/场景 | 未执行 |
| WPN-13-VER-04 | S-03 | AI 针对每个存活敌人取两条候选 | 目标与两次随机轨迹的评分流程一致 | 逻辑/模拟 + 原版对照 | 未执行 |
| WPN-13-VER-05 | 全部 | 固定关卡、同一拖拽与目标 | 目标、镜头节奏、击飞速度、淡出和声音逐 tick 对照 | 原版对照 | 未执行 |

## 完成记录

- 静态确认：WPN-13-S-01..03、A-01..02。
- 已实现：待本次实现完成后登记。
- 已执行并通过：无；Unity Play Mode 与原版对照由后续验证记录。
- 已知差异：角色持有姿势、目标鼠标光标、命中箱体以及镜头边界下的完整时序仍待运行对照。
