# 角色头顶标识与血条

## 原版证据

- S：`Docs/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/throwgame/Character.as`，`updateOverlay`（约 116–144 行）和 `advance`（约 152–165 行）。
- S：`Docs/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/util/Clip.as`，`update` 第 70–83 行：使用 holder 时只更新 `mcHolder._x/_y`，旋转仅写入内部 `mc._rotation`；characterOverlay 挂在外层 `mcHolder`，因此跟随坐标但不跟随角色图像旋转。
- S/A：`Docs/ReverseEngineering/Swf/mutiny.swf.xml`：导出符号 `characterOverlay`（1884）、`health`（1864）；标签 `p1`、`p2`、`cpu`；健康条 28 帧。
- A：SWF 位图 1867、1878、1881 分别为 P1、P2、CPU 加向下三角；已抽取为 Unity Resources 的 `UI/CharacterOverlay/*_indicator.png`。位图 1859 是 30×6 的健康条外框。
- U：用户提供的原版截图显示 P1/CPU 标识、队伍色三角和像素风血条。

## 规则

| ID | 原版规则 | Unity 入口 | 验收用例 | 状态 |
| --- | --- | --- | --- | --- |
| CHAR-OVR-01 | 每个角色创建 `characterOverlay`。AI 队显示 `cpu`；非 AI 队显示 `p` 加队号，即单人游戏中为 P1。 | `MutinyCharacterOverlay` | 红队静止且为当前队时显示原版 P1 位图；蓝色 AI 队对应显示 CPU 位图。 | S/A 已确认；已实现；生产入口回归用例已添加，Unity 待运行验证 |
| CHAR-OVR-02 | 三角标识仅在该角色队伍为当前队、角色正被玩家拖拽或已主动抛出以外、角色存活且对话框没有指向该角色时可见。该条件不读取速度。 | `MutinyCharacterOverlay` + `MutinyPlayerInput` + `MutinyAIController` + `MutinyTurnManager` | 轮到另一队、玩家拖拽自身或已提交自身跳跃时隐藏；爆炸/碰撞造成的非零速度仍显示；第二行动阶段开始时恢复。 | S/U 已确认；已实现；生产入口回归用例已添加，Unity 待运行验证 |
| CHAR-OVR-03 | 血条在角色存活且正被玩家拖拽或已主动抛出以外时显示；它不以“是否当前队”或速度作为条件。死亡后覆盖层消失。 | `MutinyCharacterOverlay` + `MutinyCharacter` | 非当前队或被动移动的存活角色仍显示血条；自身跳跃时隐藏；死亡后两者均隐藏。 | S/U 已确认；已实现；生产入口回归用例已添加，Unity 待运行验证 |
| CHAR-OVR-04 | 健康条帧为 `1 + ceil(27 * shownHealth / maxHealth)`；`shownHealth` 仅在静止时按每个 25 Hz tick 向 `health` 移动 1。 | `MutinyCharacter.AdvanceOriginalHealthTick`、`MutinyCharacterOverlay` | 100、99、50、1、0 点显示相应 27 段离散填充；受击未静止时不前进。 | S/A 已确认；已实现；生产入口回归用例已添加，Unity 待运行验证 |
| CHAR-OVR-POS-01 | `Character.show()` 将 `characterOverlay` 直接 attach 到 `mcHolder`，运行时根坐标为 `(0,0)`；根时间轴中用于展示导出 symbol 的 `(0.5,-2.4)` 不是游戏实例坐标。 | `MutinyCharacterOverlay.CreateOverlayUI` | 覆盖层根节点 localPosition 为零。 | S/A 已确认；已修复；Unity 待运行验证 |
| CHAR-OVR-POS-02 | `triangle` 位于 `(0,-761 twips)`，其内部图形从该顶部注册点向下延伸；P1/P2 为 16×23，CPU 为 24×23。 | `MutinyCharacterOverlay.UpdateIndicatorSprite` | 图片使用顶部中心 pivot，顶部相对角色为 `-38.05 px`。 | A 已确认；已修复；Unity 待运行验证 |
| CHAR-OVR-POS-03 | `health` 位于 `(0,360 twips)`，30×6 外框以自身中心注册。 | `MutinyCharacterOverlay.CreateOverlayUI` | 血条中心相对角色为原版向下 `18 px`。 | A 已确认；已修复；Unity 待运行验证 |
| CHAR-OVR-POS-04 | 角色移动和旋转时，overlay 跟随 `mcHolder` 的角色坐标；角色角度只作用于内部 `mc`，不作用于 triangle、health、corners 等 overlay 子层。 | `MutinyCharacterOverlay.KeepOverlayUpright` | 将角色旋转 73° 并移动后，overlay 世界位置仍与角色注册点一致，世界 Z 角保持 0°。 | S 已确认；已修复并添加生产对象断言；Unity 待运行验证 |
| CHAR-OVR-05 / WPN-13-INT-02 | 当前玩家选中未发射巫毒娃娃时，`targetCharacter` 或 30 px 内悬停敌人显示 `overlay.target`；所有角色的普通角框隐藏。target 子层 matrix 为 `(-20,0)` twips。 | `MutinyCharacterOverlay` + `MutinyPlayerInput` | 选中敌人前后检查 target 十字与角框互斥；发射或取消后 target 消失。 | S/A 已确认；已实现；Unity 待运行验证 |

## 当前实现边界

本次使用原版 P1/P2/CPU 位图、健康条外框和巫毒娃娃 target 位图，按原版 27 段离散填充合成健康条。对话框目标仍不在当前实现范围内。

角色重叠时，overlay 不再使用全局固定层级；它从所属角色的 Flash holder 槽位派生，因此高于自身模型但低于后创建角色。详见 `CHARACTER_LAYERING.md`。

Unity Play Mode 与原版逐帧截图对照尚未执行。

## 缺陷回归

| ID | 基线与最早分歧 | 原版期望及证据 | Unity 修复与用例 | 状态 |
| --- | --- | --- | --- | --- |
| BUG-CHAR-OVR-001 | 2026-09-16：爆炸击飞时，Unity 以速度非零隐藏标识和血条。 | `Character.updateOverlay` 第 126–131 行只使用 `Controller.dragging == this` 与 `this.thrown`；没有速度条件。用户报告确认被动爆炸位移时仍应显示。 | `MutinyCharacter.IsSelfThrown` 仅由玩家/AI 自身跳跃入口设置；覆盖层不再检查速度。`VerifyCharacterOverlay` 覆盖被动速度、提交自身跳跃与续行动恢复。 | 已实现；Unity 待运行验证 |
| BUG-CHAR-OVR-002 | 2026-09-16：头顶标识和角色下方血条相对角色偏移。 | `Character.show()` 的 attachMovie 未传入坐标，故运行根为 `(0,0)`；symbol 1884 中 triangle/health 分别位于 `-761/+360 twips`。旧实现误用了根时间轴预览实例的 `(10,-48 twips)`，并将顶部注册的标识当作中心注册。 | 去除预览偏移；标识改为顶部中心 pivot；血条保持中心注册并落在 `+18 px`。新增 CHAR-OVR-POS-01..03 生产对象断言。 | 已实现；Unity 与原版截图对照待用户执行 |
| BUG-CHAR-OVR-003 | 2026-09-16：角色被炸移动、翻滚或落水旋转时，头顶标识和血条跟着旋转。 | `Clip.update()` 在 `useHolder` 分支只旋转内部 `mc`；`Character.show()` 把 overlay attach 到不旋转的 `mcHolder`。 | `LateUpdate` 将 overlay 根的世界旋转保持为 identity，同时保留对角色世界位置的跟随；新增 73° 旋转与移动断言。 | 已实现；Unity 爆炸/碰墙/落水场景待用户验证 |
