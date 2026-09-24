# 06.17 · Voodoo Doll

[返回上级模块](../README.md)

## 职责

记录目标选择、娃娃投掷和速度传递行为。

## 边界

参数和时序以原版证据为准。

## 逻辑摘要

- 先选目标才启用 twang；提交时保存娃娃的初始投掷速度。
- 选定目标后镜头先回使用者；投掷后镜头跟随娃娃。
- 飞行 10 tick 后关闭娃娃跟随并平移到目标；镜头到达后再等 10 tick。
- 随后只把保存速度一次性赋给目标，再每 tick alpha 减 10 直到结束。
- 目标开始运动后原版不持续跟随目标，也不会重新跟随仍在淡出的娃娃。
- 不同步娃娃的伤害、后续碰撞或位置；旧“100%伤害同调”结论错误。

来源：`VoodooDoll.as`。规则：`VOO-*`，见 [完整审计](../IMPLEMENTATION_DETAILS.md#613-voodoo-doll)。

## 目标选择光标

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| VOO-CUR-01 | 人类尚未选目标且鼠标不在娃娃自身上时，显示原版 `voodooDoll` 光标；鼠标移到娃娃上、选定目标、投掷或取消后恢复普通鼠标 | `TileSystem.as::advance` 的 `voodooDoll` 分支、DefineSprite 1813 帧 40 | `MutinyPlayerInput.UpdateSpecialWeaponCursor`、`MutinySpecialWeaponCursor` | 经生产选中/绑定目标流程检查 Mode，鼠标越过娃娃边界时检查恢复；校验 31×22 原版图 | 原版静态确认；已修复，待 Unity 运行验证 |
