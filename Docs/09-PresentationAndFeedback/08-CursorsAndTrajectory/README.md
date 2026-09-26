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
| CUR-SCALE-01 | Anchor、Seagull、Tidal Wave、Voodoo Doll、Wooden Crate、Gunpowder Barrel、非法落点 cross 及 Parachute Bomb fan 的光标图案与鼠标热点，随原版 550×400 游戏画面的统一比例缩放；原版舞台 550×400、cursor 时间轴放置矩阵无独立缩放，`CustomCursor.as` 只切帧和跟随鼠标。 | SWF `displayRect`、`DefineSprite_1813_cursor` 放置矩阵；`CustomCursor.as::setCursor/onEnterFrame`；光标帧导出 31×22（fan 31×21） | `MutinySpecialWeaponCursor.OnGUI()` | 静态确认，已实现；自动回归已添加，待 Unity 运行验证 |
| WPN-10-EFF-01 | Rum Bottle 的轨迹预览与实际 `twang` 都以 30 像素/刻为上限 | `RumBottle.as` 的 `twangMaxForce=30`；`Weapon.as::twang` 与 `Weapon.as::release` 是不同入口 | `MutinyTrajectoryRenderer.ShowTrajectory`、`MutinyWeapon.Twang` | 已统一发射速度与预览；待 Unity 运行验证 |
| CUR-SCROLL-01 | 桌面端只有手动滚屏方向非零时才用原版 `scroll1`（水平/垂直）或 `scroll2`（斜向）替换光标；八方向角度遵循原版方向矩阵，滚屏结束恢复普通/武器光标；按 `CAM-EDGE-05`，窗口内黑边可触发，窗口外不触发 | `TileSystem.as::advance:343-363`、`advanceScrolling:420-493`；cursor symbol 1813 labels `scroll1` frame 78、`scroll2` frame 87；用户授权的黑边扩展 | `MutinyCameraController.AdvanceEdgeScrolling/OnGUI`、`MutinySpecialWeaponCursor.OnGUI` | 生产资源解码、八方向选择及窗口/黑边输入门；隔离 Unity Play Mode 10/10 断言通过；桌面真实鼠标与特殊武器光标切换待画面验收 | 原版箭头静态确认、黑边扩展已实现；主工程画面待验收 |
| AND-CUR-SCROLL-01 | 安卓拖动时按镜头位移向量在对应视口边缘显示滚屏箭头 | 先前用户授权的移动端扩展；2026-09-26 用户明确撤回 | 原 `MutinyCameraController.PanByMobileTouchDelta/OnGUI` | 不再作为当前验收要求；保留历史记录 | 已撤回；实现与对应断言已移除 |
| AND-CUR-SCROLL-02 | 安卓端无论触摸平移方向如何都不绘制 `scroll1/scroll2` 箭头；单指／第二触点镜头平移及桌面鼠标滚屏箭头保持原状 | 用户撤回 `AND-CUR-SCROLL-01`；原版箭头仅对应鼠标滚屏 | `MutinyCameraController.DrawScrollArrow`、`PanByMobileTouchDelta` | 驱动生产箭头绘制门，确认移动平台有方向也不绘制、桌面有方向仍绘制；驱动移动平移入口确认位移和边界保持 | 已实现；2026-09-26 隔离 Unity Play Mode 箭头专项 10/10 断言通过；Android 真机画面待验收 |
| CUR-SCROLL-02 | 滚屏箭头持续显示期间系统鼠标保持隐藏；武器光标无状态变化或被反复清空时不得每帧重新显示系统鼠标；退出滚屏后按当前武器光标状态恢复 | `CustomCursor.as::setCursor/restoreCursor`：同一光标类型提前返回，不重复调用 `Mouse.show()`；`TileSystem.as::advance` 的 scroll 光标优先级 | `MutinySpecialWeaponCursor.SetMode/OnDisable`、`MutinyCameraController.UpdateScrollCursorVisibility` | 经正式 `SetMode/Clear` 重复调用，检查已隐藏鼠标保持隐藏；状态切换后恢复正确可见性；主工程 PIE 视觉复验待做 | 原版静态确认、已实现；Unity 6.6 隔离 Play Mode 6/6 专项断言通过；主工程 PIE 无闪烁待目视确认 |

`scroll1/scroll2` 使用 `DefineSprite_1813_cursor/78.png` 与 `87.png` 的原始 31×22 PNG 字节，运行时以 Point 过滤和原版 550×400 舞台比例绘制。该箭头仅用于桌面手动滚屏；安卓触摸平移仍可用，但不显示箭头。
