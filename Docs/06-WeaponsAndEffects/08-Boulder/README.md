# 06.08 · Boulder

[返回上级模块](../README.md)

## 职责

记录 Boulder 的尺寸、滚动、推挤和伤害行为。

## 边界

参数和时序以原版证据为准。

## 逻辑摘要

- extent 31、weight 1.5、friction .25；内部 rotating 子层每 tick 转 `vx*2.5`。
- 不爆炸；排除 owner，对重叠角色横向挤出并造成 `abs(vx)*1.5` 伤害。
- 继承 Weapon 的静止/地图底部结束；白化时序需与共享结束检查一起验证。

来源：`Boulder.as`、`Weapon.as`。规则：`BLD-*`，见 [完整审计](../IMPLEMENTATION_DETAILS.md#64-boulder)。
