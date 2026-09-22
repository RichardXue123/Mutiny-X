# 05 · 物理与战斗结算

## 职责

- 运行原版固定步长运动模拟。
- 处理重力、碰撞、摩擦、反弹、旋转衰减和静止判断。
- 处理爆炸伤害、击退、落水和连锁结算。

## 当前入口

- `Assets/Mutiny/Scripts/Simulation/MutinyPhysics.cs`
- `Assets/Mutiny/Scripts/Simulation/MutinyPhysicsBody.cs`
- `Assets/Mutiny/Scripts/Simulation/MutinyExplosion.cs`

## 边界

逻辑物理结果独立于渲染帧率；表现层不得反向修改结算结果。

## 子模块

| 目录 | 内容 |
| --- | --- |
| [01-SimulationClock](01-SimulationClock/README.md) | 模拟时钟 |
| [02-MovementAndCollision](02-MovementAndCollision/README.md) | 运动与碰撞 |
| [03-RotationAndResting](03-RotationAndResting/README.md) | 旋转与静止 |
| [04-ExplosionDamageAndKnockback](04-ExplosionDamageAndKnockback/README.md) | 爆炸、伤害与击退 |
| [05-WaterPhysics](05-WaterPhysics/README.md) | 水域物理 |

## 后续文档

25 Hz 更新顺序、碰撞矩阵、伤害公式和逐 tick 对照用例。
