# 05.02 · 运动与碰撞

[返回上级模块](../README.md)

## 职责

处理速度积分、地形接触、反弹和摩擦。

## 边界

不决定角色的行动资格。

## 用户授权的关卡重力倍率

`<level gravityScale="0.5">` 将世界重力加速度乘0.5；省略属性默认为1。只接受有限正数；原始XML保持不变。倍率保存于LevelData/LevelRoot与PhysicsBodyState，积分使用 `Weight × GravityScale`，保留各物体的原始重量（包括0重力特殊武器）、弹力、摩擦与发射力。AI状态快照、实际预测器、增强效果模拟及定向弹道使用同一倍率，不修改全局重力常量。旧场景PhysicsBodyState缺少倍率时以1兼容。

本轮行为ID、测试及边界见 [EXT-SPACE16-GRAVITY-01](../../02-LevelAndWorld/04-ObjectsAndSpawns/LEVEL_1_16_SPACE_DRAFT.md)。

## BoxWeapon 地形式碰撞

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| BOX-COL-01 | 木箱和火药桶属于同一 `Controller.boxes`；任何 `hitsBoxes=true` 的 Solid 都在纵向和横向扫描中与其 AABB 碰撞，因此会阻挡跳跃和武器投掷 | `Solid.as::advanceMotion` lines 221/319；`BoxWeapon.as::place` line 96 | `MutinyBoxRegistry.GetObstacles` → `MutinyPhysicsBody.AdvanceSimulationTick` → `MutinyPhysics.Step` | 放置 3 箱后，驱动实际 Character 向上 tick 和 Cannonball 横向 tick，分别断言 Ceiling/RightWall 接触及校正坐标 | 已实现；自动回归已写，待 Unity 运行验证 |
| BOX-COL-02 | 当角色已因爆炸/边角运动与落地箱体发生少量重叠时，箱体的逆向推出位置不得令角色 AABB 新进入实体 tile；若箱体候选位置与地形冲突，本轴保留 tick 起点并仍结算箱体接触 | 原版 `Solid.as::advanceMotion` 确认箱体候选会覆盖同轴位置且没有二次 tile 校验；防止 Unity 复刻中角色被箱体推出地形的授权稳定性保护 | `MutinyPhysics.Step` → `WouldOverlapSolidTerrain` | 让角色在实体地面上与已落地箱体重叠并向上运动，驱动实际 `MutinyPhysicsBody.AdvanceSimulationTick()`；断言发生 Ceiling 接触但角色底边仍在地面之上 | 已实现；自动回归已写，待 Unity 运行验证 |
