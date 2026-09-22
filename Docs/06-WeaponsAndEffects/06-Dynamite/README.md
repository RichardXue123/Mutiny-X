# 06.06 · Dynamite

[返回上级模块](../README.md)

## 职责

记录 Dynamite 的滚动、停稳、入水画面和爆炸行为。原版没有独立倒计时。

## 边界

参数和时序以原版证据为准。

## 逻辑摘要

- extent 11、friction 1.7、旋转 `vx*2`；静止阈值 `vx==0 && abs(vy)<.2` 时爆炸 `250/70`。
- 入水静态代码只切到 `unlit` 帧，没有“熄灭后禁止爆炸”的判断。
- 当前 Unity 把入水炸药当作 dud 自动结束，属于已知差异，必须先做原版运行取证再修。

来源：`Dynamite.as`。规则：`DYN-*`，见 [完整审计](../IMPLEMENTATION_DETAILS.md#62-dynamite)。
