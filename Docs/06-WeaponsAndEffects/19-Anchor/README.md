# 06.19 · Anchor

[返回上级模块](../README.md)

## 职责

记录锚的横坐标选择、坠落、压砸和收尾行为。

## 边界

参数和时序以原版证据为准。

## 逻辑摘要

- 点击只取 X；Y 强制为 -200。extent：left/right 48、top 96、bottom 0，每 tick vy=40。
- 首次落地对 `abs(dx)<48` 且位于锚上方 64 px 内的角色造成 60，不是沿途直接秒杀。
- 落地后 hold 30 tick，再 whiteOut 10 tick；AI 提交后先等 20 tick 才下落。

来源：`Anchor.as`。规则：`ANC-*`，见 [完整审计](../IMPLEMENTATION_DETAILS.md#615-anchor)。
