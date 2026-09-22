# 09.08 · 光标与轨迹

[返回上级模块](../README.md)

## 职责

显示普通及特殊光标、虚线轨迹和方向提示。

## 边界

预测使用物理模块提供的同源公式。

## 行为规格

| ID | 可观察行为 | 原版来源 | Unity 入口 | 当前结果 |
| --- | --- | --- | --- | --- |
| CUR-BOX-01 | Wooden Crate、Gunpowder Barrel 待放置期间隐藏系统鼠标并显示对应原版武器图标；落点非法时两者统一显示原版 `cross` | `TileSystem.as::advance`；DefineSprite 1813 labels `woodenCrate`、`gunpowderBarrel`、`cross` | `MutinyPlayerInput.UpdateSpecialWeaponCursor()`、`MutinySpecialWeaponCursor` | 已实现；待 Unity 运行验证 |
