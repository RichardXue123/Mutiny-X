# 05.04 · 爆炸、伤害与击退

[返回上级模块](../README.md)

## 职责

计算爆炸范围、伤害衰减、冲量和连锁反应。

## 边界

武器只提交爆炸参数。

## 箱体与角色的命中顺序

| ID | 可观察行为 | 原版来源 | Unity 入口 | 当前结果 |
| --- | --- | --- | --- | --- |
| CRT-EXP-01 | 爆炸效果第 3 帧调用 `hit()`；同一次调用先为全部存活角色写入伤害、冲量和受击状态，再检查 boxes。命中木箱同步退出碰撞集合，所以角色首次使用爆炸速度移动时不会再撞到该箱 | `Explosion.as::hit`、`BoxWeapon.as::explode` | `MutinyExplosion.ApplyHit()`、`MutinyWoodenCrate.Explode()` | 已实现；待 Unity 运行验证 |
| CRT-EXP-02 | 木箱退出碰撞后立即跳到 symbol 965 的 `explode` 标签 frame 11，frame 18 销毁；角色飞行与破损动画并行 | symbol 965 SWF 时间轴、frame 18 `DoAction.as` | `OriginalExplodeFirstFrame=11`、`AdvanceExplosionTimelineFrame()` | 已实现；待 Unity 运行验证 |
