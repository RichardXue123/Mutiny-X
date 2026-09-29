# 06.19 · Anchor

[返回上级模块](../README.md)

## 职责

记录锚的横坐标选择、坠落、压砸和收尾行为。

## 边界

参数和时序以原版证据为准。

## 逻辑摘要

- 点击只取 X；Y 强制为 -200。extent：left/right 48、top 96、bottom 0，每 tick vy=40。
- 首次落地对 `abs(dx)<48` 且位于锚上方 64 px 内的角色造成 60，不是沿途直接秒杀。
- 落地后 hold 30 tick，再 whiteOut 10 tick；AI 提交后先等 20 tick 才下落。
- AI 按角色 Luck 样本数在全图随机 X，从 `y=-200` 垂直模拟；仅触地样本进入普通落点评分，并按原版将总分乘 `0.5`。
- 下落阶段主时间轴停在 frame 1；触地后播放到 frame 12，frame 3 生成左右镜像的 18 帧碎屑子时间轴，子时间轴在自己的 frame 17 清空画面。
- 落地后保持 30 tick，再用原版加白→白色透明的 `Global.whiteOut` 变换经过 10 tick 消失。

来源：`Anchor.as`。规则：`ANC-*`，见 [完整审计](../IMPLEMENTATION_DETAILS.md#615-anchor)。

## 行为规格与实现映射

### 下落高刷新率显示（2026-09-28）

实现前规格：`ANC-PRES-01/02` 为用户授权的显示扩展，不是 Flash 原版新增行为。AS2 静态确认 `Anchor.advance` 每 tick 设置 `velocityY=40` 并调用 `advanceMotion/update`，`aiPerform` 保留 20 tick 等待；`contact(FLOOR)` 即刻压砸、播放主时间轴并停止下落。Unity 当前由锚自己的时钟推进物理，通用 body 非自主更新，之前未进入共享显示插值。

| ID | 可观察行为及状态转换 | 原版来源 / 授权差异 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| ANC-PRES-01 | 玩家点击/AI 提交仍从 `y=-200` 出生、每逻辑 tick 下落 40 px；AI 20 tick 等待不变。非自主物理体使用锚时钟的剩余比例，在实际渲染帧插值最近已完成 tick 的起止位置；锚身和镜头共用显示采样，不多推进物理或命中。 | `Anchor.as::place/advance/aiPerform`；用户授权高刷新率扩展 | 锚 `AdvanceSimulationFrame`、共享 body 的外部显示时钟 | 玩家正式点击与 AI 正式执行分别在 25/60/120 FPS 检查出生、AI 等待、首 tick/半 tick、显示帧不改权威状态、连续中间帧、镜头同源、无二次自主更新和延迟 Start | 已实现；2026-09-29 隔离 Play Mode 专项 60/60 通过；主工程画面待验收 |
| ANC-PRES-02 | 首次碰撞仍按生产物理在同一 tick 造成 60 HP 压砸并开始原版撞击/hold；该 tick 显示立即吸附实际落点，停止插值。主时间轴、子碎屑、30 tick hold 和 10 tick whiteOut 保持原版 25 Hz，不由渲染帧重复推进。 | `Anchor.as::contact/advance`；显示吸附为扩展过渡规则 | `HitFloor`、外部显示时钟采样与原版后续时序 | 比较各 FPS 的触地物理 tick/位置/一次伤害；触地首帧及后续中间帧静止；既有 ANC-ANI-02/03 回归 | 已实现；上述生产触地/伤害/收尾及既有动画断言通过；实际画面待验收 |

使用与既有角色/武器一致的一个已完成物理 tick 显示延迟（至多 40 ms）；接触时立即清除这段延迟。此次只平滑下落位移，不创造原版没有的精灵补帧，不修改触地碎屑/白化的逻辑时钟。

2026-09-29 实际结果：Unity 6000.6.0f1 隔离工程 `Validate Anchor Animation Play Mode` 60/60 通过，其中新增显示规则 51 条、既有触地动画 9 条。未在主工程 PIE 或 Android 真机目视验收；不将该用户授权插值扩展记为 Flash 一致性通过。

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| AI-WPN-04 | AI 在全图随机 X，从顶部垂直模拟 Anchor；只接纳触地样本，评分乘 `0.5`，胜出后等待 20 tick 再下落 | `Anchor.as::randomThrows/aiPerform` | `MutinyAIController.EvaluateAnchor()`、`MutinyAnchor.DropForAi()` | 固定种子与地面，核对样本数、X 范围、胜出类型、正式对象和库存消耗 | 已实现；自动回归已写；待 Unity 运行验证 |
| ANC-CUR-01 | 人类选中 Anchor 且尚未投放时，显示原版 cursor 时间轴 `anchor` 标签图标；投放或取消后恢复普通鼠标 | `TileSystem.as::advance` 的 `anchor` 分支、DefineSprite 1813 帧 1 | `MutinyPlayerInput.UpdateSpecialWeaponCursor`、`MutinySpecialWeaponCursor` | 生产选中后检查 Anchor Mode 与 31×22 原版图，取消/投放后检查 None | 原版静态确认；已修复，待 Unity 运行验证 |
| ANC-ANI-02 | 下落始终停在锚主时间轴第 1 帧；首次触地后主时间轴开始播放，帧 3 在锚两侧各生成一条镜像的 `DefineSprite_1002` 碎屑子时间轴，子时间轴独立推进 1..16 帧并在第 17 帧移除，即使主时间轴在帧 12 停止也继续播放 | `Anchor.as::constructor/contact` 的 `mc.stop()/mc.play()`；`mutiny.swf.xml` 中 DefineSprite 1003 的 frame 1/3/12 和两处 1002 子符号；1002 frame 17 RemoveObject | `MutinyAnchor.HitFloor/AdvanceImpactTimelineTick` 与独立碎屑 SpriteRenderer | 生产落锚触地后逐 tick 检查主帧、两侧碎屑的出现/镜像/独立推进/移除；下落前无碎屑 | 原版静态确认；已实现；隔离 Unity Play Mode 9/9 专项通过；实机画面对照待验收 |
| ANC-ANI-03 | 触地保持 30 tick 后，10 tick `Global.whiteOut(fadeTime/10)`：前半段 RGB 向白色加色，后半段白色 alpha 递减；第 10 个淡出 tick 完全透明，下一 tick 隐藏并结束 | `Anchor.as::advance`；`Global.as::whiteOut` | `MutinyAnchor.ApplyWhiteOut`、Sprite ColorTransform 材质 | 生产触地后逐 tick 检查颜色变换乘数/加数与 alpha，最后检查结束帧 | 原版静态确认；已实现；隔离 Unity Play Mode 9/9 专项通过；像素级画面对照待验收 |
