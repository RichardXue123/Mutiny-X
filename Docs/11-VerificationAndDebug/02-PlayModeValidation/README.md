# 11.02 · Play Mode 验证

[返回上级模块](../README.md)

## 职责

记录 Unity 中实际操作、画面、声音和状态结果。

## 边界

未运行的步骤不得标记通过。

## 本轮记录

- [Credits 页面验证（2026-09-25）](CREDITS_VERIFICATION.md)
- [结局角色动画验证（2026-09-26）](ENDING_IDLE_VERIFICATION.md)
- 2026-09-26：Unity 6000.6.0f1 隔离工程执行 `Mutiny/Parity/Validate Level Lifecycle Play Mode`，最终 8/8 断言通过：经生产放置链摆放的木箱/火药桶和已布置地雷在胜负 `GameOver`/结果弹窗期间保留，新关初始化及显式 `ClearLevel()` 卸载时清除旧对象。胜利 speech 的实际画面/角色站箱观感仍待手动验收。
- 2026-09-26：Unity 6000.6.0f1 隔离工程执行 `Mutiny/Parity/Validate Anchor Animation Play Mode`，9/9 断言通过：实际投放路径下落静帧、触地双侧碎屑子时间轴与 10 tick 原版白化参数均通过。使用项目原有的 `AnchorImpact` PNG/GUID；实际战斗画面与原版逐帧截图待验收。
