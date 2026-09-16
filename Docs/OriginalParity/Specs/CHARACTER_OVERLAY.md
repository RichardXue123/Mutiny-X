# 角色头顶标识与血条

## 原版证据

- S：`Docs/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/throwgame/Character.as`，`updateOverlay`（约 116–144 行）和 `advance`（约 152–165 行）。
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

## 当前实现边界

本次使用原版 P1/P2/CPU 位图和健康条外框，按原版 27 段离散填充合成健康条。对话框目标和巫毒娃娃 target 图层属于原版 overlay 的其它子层，不在本次用户报告的 P1/CPU 与血条范围内。

Unity Play Mode 与原版逐帧截图对照尚未执行。

## 缺陷回归

| ID | 基线与最早分歧 | 原版期望及证据 | Unity 修复与用例 | 状态 |
| --- | --- | --- | --- | --- |
| BUG-CHAR-OVR-001 | 2026-09-16：爆炸击飞时，Unity 以速度非零隐藏标识和血条。 | `Character.updateOverlay` 第 126–131 行只使用 `Controller.dragging == this` 与 `this.thrown`；没有速度条件。用户报告确认被动爆炸位移时仍应显示。 | `MutinyCharacter.IsSelfThrown` 仅由玩家/AI 自身跳跃入口设置；覆盖层不再检查速度。`VerifyCharacterOverlay` 覆盖被动速度、提交自身跳跃与续行动恢复。 | 已实现；Unity 待运行验证 |
