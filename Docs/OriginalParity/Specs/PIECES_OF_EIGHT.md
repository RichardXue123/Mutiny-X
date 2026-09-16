# WPN-09 Pieces of Eight（八枚硬币）行为规格

## 基线与范围

- 模块 ID：WPN-09。
- 原版：\`Mutiny Source/mutiny-flash-game/mutiny.swf\`，SHA256 \`c034d2cfa50272a1aac93ad75114b542ca5993dd7b70fa8901aa195de7820586\`。
- 范围：单次装备后的八枚连续投掷、碰撞/水面、锁定、回合占用、AI 连续瞄准、单帧表现和声音。
- 不包含：Unity Play Mode 和原版运行录像对照；均待执行。

## 证据与结论

| 证据 ID | 类型 | 原版定位 | 结论 |
| --- | --- | --- | --- |
| W09-S-01 | S | \`PiecesOfEight.as:25-33\` | 创建后四向 extent 都为 7，\`hitsBoxes=true\`、\`draggable=false\`、\`twangable=true\`。 |
| W09-S-02 | S | \`PiecesOfEight.as:46-71\`、\`Weapon.as:53-72\`、\`Solid.as:splashCheck\` | 未发射且未被拉拽时，每 tick 跟随 owner 到 \`(owner.x, owner.y + 5)\`；继承普通武器运动。穿越水面会走一次通用 splash，但不会调用 \`contact\`，随后调用 \`next(false)\`。 |
| W09-S-03 | S | \`PiecesOfEight.as:35-44,73-97\` | terrain/box 接触时，若已发射且未结束，立即创建 \`Explosion(x,y,50,25,owner)\` 并播放一次 \`pop\`。每次接触或入水递增 \`timesFired\`；前 7 枚复用同一武器实例并置 \`fired=false, track=false\`，第 8 枚才 \`finished=true; hide()\`。 |
| W09-S-04 | S | \`WeaponSelectButton.as:16\`、\`PiecesOfEight.as:81-91\`、\`Character.as:215-248\`、\`Controller.as:200-240\` | 说明文字称为 “eight turns”，实现实际是**同一角色、同一游戏回合内的八次连续瞄准**：前 7 枚将 \`weaponLocked=true\`，且已装备但 \`!fired && !finished\` 会持续把全局 inactivity 置零，因此不能换人、换武器或结算到下队。 |
| W09-S-05 | S | \`PiecesOfEight.as:99-153\` | AI 在前 7 枚结算后等 20 tick，再采样 10 条轨迹；每条在 70 px 内的敌人加 \`1-distance/70\`，友军减 \`1.5-distance/70\`，严格较大者胜出并发射。 |
| W09-A-01 | A | \`DefineSprite_884_piecesOfEight\` | 时间轴为单帧，使用 \`1.png\`；未发现专属飞行、命中或投掷声音。 |

冲突说明：按钮文案的 “eight turns” 与实际回合机制文字不一致。这里以 \`PiecesOfEight.next\`、\`Character.advance\` 和 \`Controller.enterFrame\` 的执行链为准，记录为同回合八次投掷。

## 状态、输入与转换

| 规则 ID | 前置状态 | 输入/事件及允许条件 | 状态变化/触发 tick | 表现、声音、镜头 | 来源 |
| --- | --- | --- | --- | --- | --- |
| WPN-09-EFF-01 | 被装备 | 创建 | 设置 7 px extents 和 box 碰撞 | 单帧 symbol 884 | W09-S-01/W09-A-01 |
| WPN-09-INT-01 | 当前角色可射击且库存至少一件 | 选择后按住、拖拽、释放 | 第一枚发射时库存只扣一次，\`CanThrow/CanShoot=false\` | 普通瞄准轨迹与 click | W09-S-03/W09-S-04 |
| WPN-09-EFF-02 | 飞行中的一枚 | terrain/box contact | 同 tick 50/25 爆炸、立即 pop、次数 +1 | 硬币隐藏/回到持有位置 | W09-S-03 |
| WPN-09-EFF-03 | 飞行中的一枚 | \`y >= water.y\` | 通用 splash 后次数 +1；不创建爆炸、不播放 pop | 水花一次 | W09-S-02/W09-S-03 |
| WPN-09-INT-02 | 已完成第 1–7 枚、角色存活 | 下一次拖拽释放 | 同一实例重新就绪，保持行动阶段，\`weaponLocked=true\`；不允许取消或选择其他武器 | 镜头返回角色 | W09-S-03/W09-S-04 |
| WPN-09-INT-03 | 已完成第 8 枚或 owner 死亡 | 物理和爆炸静止后 | 武器 finished/hidden，解除该序列对 inactivity 的占用，正常回合结算 | 无额外声效 | W09-S-03/W09-S-04 |
| WPN-09-AI-01 | AI owner、第 1–7 枚完成 | 等 20 个 25 Hz tick | 10 条候选按 W09-S-05 评分后自动发射下一枚 | 无专属声音 | W09-S-05 |

拒绝与边界：第 8 枚后不允许第 9 次；owner 死亡时不再阻塞回合；无库存不能开始；水面不是碰撞爆炸；相等 AI 分数保留第一条候选；右键取消仍是项目已授权扩展，但在原版 \`weaponLocked\` 阶段不得取消本序列。

## 表现资源与实现映射

| 规则 ID | 原版资源/时间轴 | Unity 入口 | 当前差异 |
| --- | --- | --- | --- |
| WPN-09-ANI-01 | DefineSprite 884，单帧 | \`Resources/Art/Weapons/PiecesOfEight/1.png\`、\`MutinyPiecesOfEight\` | 持有注册点和渲染排序待画面对照。 |
| WPN-09-INT-01..03 | \`PiecesOfEight.as\`、\`Character.as\`、\`Controller.as\` | \`MutinyPlayerInput\`、\`MutinyTurnManager\`、\`MutinyCharacter\` | Play Mode 待执行。 |

## 用例与实际结果

| 用例 ID | 规则 ID | 操作 | 独立期望 | 验收层 | 实际结果 |
| --- | --- | --- | --- | --- | --- |
| W09-T-01 | WPN-09-EFF-01 | 工厂创建 | 7 px、box、单帧资源 | 逻辑/资源 | 待执行 |
| W09-T-02 | WPN-09-INT-01..03 | 生产输入启动，连续结算并再次通过生产输入发射 8 枚 | 仅第一次扣库存；前 7 次锁定且占用回合；第 8 次完成 | 逻辑/输入 | 待执行 |
| W09-T-03 | WPN-09-EFF-02/03 | 分别经实际 physics contact 与水面 crossing | contact 为 50/25 + pop；水仅切下一枚 | 逻辑/模拟 | 待执行 |
| W09-T-04 | WPN-09-AI-01 | AI 连续七次结算 | 每次等待 20 tick、十样本评分、无第九枚 | 逻辑/模拟 | 待执行 |
| W09-T-05 | 全部 | Level 1 实机连续投掷 | 状态、音效、镜头和 UI 与原版逐 tick 对照 | 场景/原版 | 未执行 |

- tick 对齐：Unity \`MutinyPhysics.TimeStep=0.04s\` 对原版 25 Hz \`enterFrame\`。
- 关联回归：WPN-COM-INT、P2-06、P2-07、行动 UI 锁定显示、AI-WPN-01。
- 明确扩展：右键取消不是原版输入；只在八枚序列开始前保留。

## 完成记录

- 已静态确认：W09-S-01..05、W09-A-01。
- 已实现：八枚同实例序列、原版碰撞/水面分支、输入锁定、回合占用、AI 续投和单帧资源。
- 已执行并通过：`dotnet build Assembly-CSharp.csproj --no-restore`，0 errors；仅保留既有 `MutinyLevelTest.levelXml` CS0649 warning。
- 待运行验证：W09-T-01..05，包括 Unity Editor 回归入口、Play Mode、实际碰撞/水面、AI 20 tick 与原版逐 tick 对照。
- 已知差异：尚未完成持有注册点、飞行旋转和逐帧画面对照。
