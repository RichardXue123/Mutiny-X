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

## 音频规格

- **BLD-AUD-01（原版一致性）**：`Boulder.as`、对应 pcode 与 symbol 872/869 时间轴没有专属发射、滚动或撞击声音。巨石只继承 `Solid.splashCheck` 的跨水面 `splash`；被推飞的角色以后撞上地形时，可由 `Character.contact` 产生 `hitwall`。
- **BLD-AUD-EXT-01（用户授权扩展）**：有横向撞击速度的巨石与非 owner 存活角色开始一次新的接触时，播放原版已导出的 `smack`。同一角色持续重叠期间只播放一次；离开后再次接触可重播。同一 tick 同时撞到多个角色只发出一次，避免叠音峰值；静止重叠不发声。

验收：通过玩家生产入口选择并抛出 Boulder，推进真实物理 tick 使其接触角色；首次接触恰好收到一次 `smack`，下一连续接触 tick 不重播，分离后重入再播放一次。SFX 关闭时仍必须静默。

来源：`Boulder.as`、`Weapon.as`。规则：`BLD-*`，见 [完整审计](../IMPLEMENTATION_DETAILS.md#64-boulder)。
