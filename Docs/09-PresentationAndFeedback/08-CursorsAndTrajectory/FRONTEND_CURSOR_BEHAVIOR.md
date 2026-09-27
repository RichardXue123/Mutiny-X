# 前端边缘光标（CUR-FRONT-01）

| ID | 可观察行为及状态转换 | 原版来源 / 用户要求 | Unity 入口 | 验收用例 | 实际结果 |
| --- | --- | --- | --- | --- | --- |
| CUR-FRONT-01 | 标题、模式/关卡选择、Help、Credits、Scores 和结尾页面中，桌面鼠标移到游戏画面边缘与黑边时仍显示系统光标，不显示滚屏箭头；悬停按钮仍切换手形。进入 Gameplay 后保留原版边缘滚屏箭头，离开 Gameplay 时即使相机保留上一关引用也恢复系统光标；手柄模式仍可隐藏系统鼠标 | 用户报告主界面边缘鼠标消失；原版 `TileSystem.as::advance` 只在关卡滚屏方向非零时设置 `scroll1/scroll2`，`Controller.as::endGame` 调用 `CustomCursor.restoreCursor()`；前端页面由原版根时间轴独立绘制 | `MutinyCameraController.AdvanceEdgeScrolling` / `IsDesktopScrollArrowVisible`；`MutinyCursorManager.LateUpdate`；`MutinyFrontendController.CurrentPage` | 生产前端保持 Title、Scores 等页面，模拟上一关残留的相机/鼠标隐藏状态并让鼠标位于四边与黑边，检查系统鼠标可见、无箭头且无滚屏；进入实际 Gameplay 后检查原版箭头仍出现，退出时立即恢复；鼠标/手柄切换各检查一次 | 已实现；隔离 Unity Play Mode 光标专项 11/11 通过，含 Title 在保留活动回合引用时拒绝边缘滚屏、恢复系统光标；真实页面/鼠标/手柄画面待目视验证 |

本修复只调整滚屏光标的页面资格和离开游戏时的可见性恢复，不改变 Gameplay 内的方向、角度、边缘阈值或滚屏速度。

## 验收记录

- **静态确认：** 原版滚屏箭头来自关卡 `TileSystem.advance`，退出关卡时 `Controller.endGame` 显式恢复普通光标。Unity 前端复用游戏相机；原相机边缘输入与箭头可见性没有检查前端页面，仍持有关卡引用时可能进入滚屏分支。
- **已实现：** 非 Gameplay 页面拒绝边缘滚屏，并直接阻止滚屏箭头显示。`MutinyCursorManager` 的生产 LateUpdate 在鼠标模式的前端页面恢复系统鼠标；手柄隐藏分支仍优先执行，系统手形/箭头图案切换路径不变。
- **实际测试通过：** 2026-09-27，`Assembly-CSharp`、`Assembly-CSharp-Editor` 编译通过。Unity 6000.6.0f1 隔离工程 `Validate Scroll Arrows Play Mode` 11/11 通过。新增用例通过真实 Input System Mouse 设备将指针置于边缘，驱动生产 `AdvanceCamera` 与系统鼠标可见性更新，在 Title 保留 TurnActive 回合、PlayerInput 和关卡引用时，滚屏方向为零、箭头不可见、系统鼠标恢复。专项同时覆盖原版箭头资源、八方向、黑边与窗口外门、移动端不绘制箭头及特殊光标恢复。
- **待运行验证：** 主工程/玩家构建中，实际鼠标经过标题及各前端页四边/黑边，悬停按钮、从 Gameplay 退出以及鼠标/手柄切换的画面尚未目视验收。
- **已知差异：** 本修复没有新增原版行为差异；沿用已有桌面黑边滚屏扩展与手柄输入支持。
