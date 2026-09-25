# 双人选关画面对照

2026-09-25 用户提供的 [原版截图](original-reference.png) 与 [修复前 Unity 截图](unity-before.png)。两张截图的显示缩放和所选关卡不同，比较布局时以原版 SWF 坐标及独立图层为准。

原版 `mutiny.swf.xml` 根帧 111：shape 1959 是 460×350 的双人专用面板，包含比分红框及底部分隔线；shape 1960 是 177×23 的红蓝海盗图标；`won_1`、`vs`、`won_2` 分别位于 x=245/275/304、y=87。sprite 629 中的 shape 578 是 202×152 的红边预览框，中心为 (275,201)；sprite 628 是逐关地图预览和动态名称，必须把预览白色区域绘制在红框内，并把名称以白色画在红色栏上。sprite 629 的 Play 锚点为 (275,296)；根帧的 `back_ls_button` 锚点为 (275,334)。这些独立资源位于 `../ReverseEngineering/Art/raster/shapes/`，SWF 时间轴结构位于 `../ReverseEngineering/Swf/mutiny.swf.xml`。
