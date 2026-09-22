# 06.04 · 共享武器效果

[返回上级模块](../README.md)

## 职责

管理通用爆炸、烟雾、火焰、白化和镜头事件。

## 边界

效果不得自行改变回合阶段。

## 爆炸公式

- 半径：`size/2+20`。
- 衰减：`ratio=1-distance/radius`，伤害 `maxDamage*ratio`。
- 冲量：`force=.06*ratio*maxDamage`，`vx+=nx*5*force`，`vy+=ny*5*force-6*force`。
- 第 3 帧统一结算角色和箱体；第 8 帧销毁效果。
- 箱体以圆到非对称 AABB 的最近点判定，可触发火药桶连锁。

原版来源：`Explosion.as` 与爆炸时间轴；Unity 入口：`MutinyExplosion.cs`。规则和验收见 [共享爆炸](../IMPLEMENTATION_DETAILS.md#43-爆炸)。
