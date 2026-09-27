# 取消叉透明度规格

## 行为规格（实现前登记，2026-09-27）

| ID | 可观察行为与状态转换 | 原版来源 / 用户依据 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| CHAR-OVR-CANCEL-ALPHA-01 | 红、蓝取消叉仅显示圆形图标；圆形外的透明像素不改变背景，不产生方形暗底。显示、蓄力和取消时均遵守此规则 | 用户截图及确认：去掉圆形按钮外侧的方形暗底；原版 `Art/raster/sprites/DefineSprite_1817_cancel weapon button red/1.png`、`1820`、`1847`、`1850` 的同帧与 SVG 均为 20×20，外侧 92 像素 alpha=0，无方形底层；原始证据保持不变 | `MutinyCharacterOverlay.CreateOverlayUI/UpdateCancelWeaponSprite`；`Resources/UI/button_cancel_*`、`Resources/UI/cancel_button_transparent` | 通过生产选角色→选跳跃入口显示红/蓝叉；在明、暗背景上使用实际 SpriteRenderer 渲染，检查 92 个外围源像素的所有放大像素与未绘制按钮时相同；确认中心图标仍可见、取消入口仍生效 | 材质及导入设置已实现；2026-09-27 隔离 Play Mode 覆盖层专项 32/32 通过，含新增实际 SpriteRenderer 材质配置断言；修复后的外围像素渲染及主工程用户报告的画面仍待验证 |

## 定位证据

- 生产 PNG 是原版 cancel player 红/蓝按钮第 1 帧的逐字节副本：红 SHA-256 `61c2374852524187604080ec385f95a58bb56524806d877c6e74eba1403cff8c`，蓝 `6ff057f23ebfa5ac4aa776d8cd02a406af290d4ea0fef20d4df46592c0cdb95c`。两者透明区均为 alpha=0，圆形内均为 alpha=255。
- 修复前 Unity 6000.6.0f1 的 Windows 导入为 DXT5；逐像素比对源 PNG，红/蓝的 alpha 差异均为 0。不能把此次缺陷归因于压缩破坏 alpha。
- 生产取消叉没有附加黑色底板；隔离 Play Mode 中实际 SpriteRenderer 继承 `Universal Render Pipeline/2D/Sprite-Lit-Default`。黄色背景隔离渲染暂未复现用户截图中的暗底，因此该画面缺陷的直接成因尚未确认。

## 实现约束

- 取消叉使用明确的透明、无光照材质，关闭深度写入；保留源贴图自身的 alpha，避免继承场景默认受光照材质。
- 两张生产贴图使用 Point、无 mipmap、无压缩导入，保留原版像素颜色与透明边界。该设置不是关于上述 DXT5 alpha 的错误归因。
- 不修改圆形内图案，也不修改位置、大小、20×20 命中区域、显示门或取消事件时序。行动面板中的取消角色叉仍使用这两张贴图和现有透明 IMGUI 命中样式。

## 验收记录

- 静态确认：原版圆形外透明；生产 PNG 外透明；修复前导入 alpha 完整。
- 已实现：生产取消叉显式使用 `cancel_button_transparent.mat`，引用 URP 的透明无光照 Sprite shader，`_ZWrite=0`；两张 PNG 的导入设置改为 Point、无压缩，未修改 PNG 字节。
- 实际测试通过：2026-09-27 Unity 6000.6.0f1 隔离 Play Mode `RunCharacterAimOverlay()` 共 32/32 通过。新增断言从真实跳跃选取后的 SpriteRenderer 读取并验证透明无光照材质、关闭深度写入、Point 采样；其余 31 条继续通过，包括生产蓄力、取消、松开发射入口。日志 `C:/Users/27487/AppData/Local/Temp/current-ui-adjustment-20260927.log`。该结果只证明配置和状态流程，不证明用户截图中的暗底已在主工程消失。
- 待运行验证：主工程战斗画面中用户报告的暗底；红/蓝叉普通、悬停、按住、蓄力状态；Android 真机。
- 已知差异：生产角色取消叉当前复用 cancel player 图案，原版 cancel weapon 图案有少量边框像素差异；本次按用户要求保留图案。此次基线更新只重验透明渲染，不重标未执行的真机或完整状态图像为通过。
