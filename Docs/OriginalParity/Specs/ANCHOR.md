# WPN-15 Anchor（锚）行为规格

## 基线与范围

- 模块 ID：WPN-15。
- 原版：`Mutiny Source/mutiny-flash-game/mutiny.swf`，SHA256 `c034d2cfa50272a1aac93ad75114b542ca5993dd7b70fa8901aa195de7820586`。
- Unity 工作区：实现前含已有未提交改动；本规格只覆盖 WPN-15。
- 包含：人类选中/取消、点击横坐标、`y=-200` 生成、40 px/tick 垂落、terrain/木箱底部、60 HP 压砸、12 帧时间轴、30+10 tick 收尾和锚音效。
- 不包含：原版 AI `randomThrows/aiPerform`、镜头逐 tick 对照、无地面时的长期行为、真实画面对照。
- 证据状态：静态确认；实现状态：实现中；验证状态：C# 编译通过，Unity 测试/Play Mode 未执行。

## 证据与冲突

| 证据 ID | 类型 | 文件、函数/符号 | 结论与适用条件 |
| --- | --- | --- | --- |
| W15-S-01 | S | `Anchor.as` constructor | `left/right=48`、`top=96`、`bottom=0`、`placeableWeapon=true`、`hitsBoxes=true`、`showCircle=false`、`draggable=false`；构造时停止锚 MovieClip。 |
| W15-S-02 | S | `Anchor.as advance/place` | 首次左键把 x 放在原始鼠标 x，随后强制 y=-200；每个未命中 tick 设 `velocityY=40` 并执行 Solid 碰撞；点击即清除 owner 的 shoot/throw。 |
| W15-S-03 | S | `Anchor.as contact(FLOOR)` | 仅首次 floor contact 设 `hitBottom`；对满足 `abs(character.x-anchor.x)<48`、`anchor.y-64 < character.y < anchor.y` 的每个角色扣 60 HP；播放锚 MovieClip。每次 floor contact 播放 `anchor` 声。 |
| W15-S-04 | S | `Anchor.as advance` | 命中后 holdTime 从 30 递减；之后 fadeTime 从 10 递减并调用 `Global.whiteOut(fadeTime/10)`；两个计时结束后 hide 并 finished。 |
| W15-S-05 | S | `Solid.as advanceMotion`; `Anchor.as randomThrows` | Anchor 继承 terrain 和 `Controller.boxes` 碰撞；AI 模拟从随机 x/y=-200/vy=40 直到 floor 或水面。常规 Anchor.advance 没有调用 `Weapon.advance`，因此未见通用入水 splash/超时分支。 |
| W15-A-01 | A | `DefineSprite_1003_anchor`，12 帧 | frame 1 和 frame 12 各有 `stop` action；frame 3 加入两侧 effect，导出 1–12 帧均已在 Unity Resources。 |
| W15-S-06 | S | `Global.as whiteOut` | `visibility>0.5` 时原图向白色 additive 过渡且保持 alpha；`<=0.5` 时转为白色并 alpha=`2*visibility`。 |
| W15-S-07 | S | `Anchor.as aiPerform` | AI 使用 `details.x` 放置，先等 20 tick 再下落。 |

无冲突。Unity 当前的通用武器水面失效/安全超时不属于原版 Anchor.advance，必须由本实现绕开。

## 状态、输入和转换

| 规则 ID | 前置状态 | 输入/允许条件 | 状态变化 / tick | 可观察结果 | 来源 |
| --- | --- | --- | --- | --- | --- |
| WPN-15-INT-01 | 活着人类已选锚、可射击、有库存 | 选中 | 在角色位置建立停止于 frame 1 的未发射锚；库存不扣 | 可右键取消，不产生锚 | W15-S-01/02 |
| WPN-15-INT-02 | 未发射锚 | 任意场景左键 | x=鼠标 px、y=-200，进入垂落；扣 1 库存并禁用 owner 两行动 | 点击 y 不影响落点高度 | W15-S-02 |
| WPN-15-EFF-01 | 已发射且未到底 | 每 25Hz tick | vy=40、vx=0，按 48/96/0 extent 碰 terrain 和木箱 | 垂直直落；不套用普通投掷轨迹 | W15-S-01/02/05 |
| WPN-15-EFF-02 | 首次 floor contact | 角色中心满足严格 x/y 范围 | 每个命中角色减 60；锚停止 | 无直接击退公式；仅该矩形命中窗 | W15-S-03 |
| WPN-15-ANI-01 | floor contact 后 | 12 帧从 frame 1 播到 frame 12 并停留 | 第 30 个 hold tick 后做 10 tick whiteOut，再隐藏并结束 | frame 3 出现两侧 effect；播放 `anchor` 声 | W15-S-03/04; W15-A-01 |
| WPN-15-AI-01 | AI 选择锚 | `aiPerform(details)` | x=details.x、等 20 tick，之后同人类垂落 | AI 不使用玩家鼠标 | W15-S-07 |

拒绝与边界：选中后未点击的右键取消为既有授权扩展。原版没有 `canPlace` 限制，任意 x 可下锚。严格边界：x 差等于 48、角色 y 等于锚 y 或 y-64 都不受伤。原版对水面无 floor 的长期收尾未见静态终止条件，待运行对照。

## 表现资源与实现映射

| 规则 ID | 原版资源 | Unity 生产入口 | 当前差异 |
| --- | --- | --- | --- |
| WPN-15-ANI-01 | `DefineSprite_1003_anchor` 1–12、frame 1/12 stop | `Resources/Art/Weapons/Anchor/1..12.png`; `MutinyAnchor` | additive whiteOut 需 Unity sprite material 才能精确，默认 renderer 近似待对照。 |
| WPN-15-INT-01..02 | `Anchor.place/advance`; `TileSystem` | `MutinyPlayerInput`, `MutinyAnchor` | 点击光标与手持注册点待画面对照。 |
| WPN-15-EFF-01..02 | `Anchor.contact`; `Solid.advanceMotion` | `MutinyAnchor`, `MutinyPhysics` | AI 20 tick 专用入口待完成。 |

## 用例与实际结果

| 用例 ID | 规则 ID | 前置条件与操作 | 独立期望 | 验收层 | 实际结果 |
| --- | --- | --- | --- | --- | --- |
| W15-T-01 | INT-01/02 | 经生产选择及点击入口，点击不同 y 的同一 x | 只扣一件库存，落点均 x 相同/y=-200 | 逻辑/输入 | 未执行 |
| W15-T-02 | EFF-01 | 固定 tile/木箱平台，推进 25Hz tick | vy=40、48/96/0 extent，底部准确停于 floor | 逻辑 | 未执行 |
| W15-T-03 | EFF-02 | 放置 x/y 临界内外的角色并 floor contact | 仅严格命中窗内角色受 60 HP | 逻辑 | 未执行 |
| W15-T-04 | ANI-01 | 命中后重放 40 tick | 12 帧停止边界，30 hold + 10 fade，锚声发生在 floor contact | 逻辑/场景 | 未执行 |
| W15-T-05 | AI-01 | 固定 AI details.x | 20 tick 等待后从 y=-200 下落 | 逻辑/场景 | 未执行 |

- 比较字段：tick、x/y、vx/vy、extent、命中角色和 HP、动画帧、fade、音频事件、finished。
- 关联回归：回合结算、物理 boxes、镜头、音频。
- 未决：无 floor 时水面/地图外、完整 AI 候选与评分、加色 whiteOut、原版实际截图/音频。

## 完成记录

- 已静态确认：W15-S-01..07、W15-A-01。
- 已实现：`MutinyAnchor` 的强制起始高度、自驱动 25 Hz 下落、terrain/木箱 floor contact、严格伤害窗、12 帧、30+10 tick 收尾；`MutinyPlayerInput` 的选中/取消/单次点击提交和 `anchor` SFX 入口。锚禁用通用水面物理与武器飞行旋转，以匹配未调用 `Weapon.advance` 的 `Anchor.advance`。
- 已执行并通过：`dotnet build Assembly-CSharp.csproj --no-restore`（0 error；现存 `MutinyLevelTest.levelXml` CS0649 warning）。
- 失败或待验证：W15-T-01..05 的 Unity 执行、Play Mode 与原版对照未执行；W15-T-05 的 AI 调度尚未实现。
- 受影响文档：`WEAPON_REPLICATION_PLAN.md`、`TODO.md`。
