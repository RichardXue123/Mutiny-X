# 结局角色动画验证（2026-09-26）

- 环境：Unity 6000.6.0f1，隔离工程，Windows batch mode + Play Mode，`-executeMethod Mutiny.Verification.Editor.MutinyEndingVerificationMenu.Validate`。
- 实际结果：Unity 输出 `[Mutiny Parity] Ending sequence Play Mode verification passed: 15/15 assertions.`；未出现 C# 编译错误。
- 生产入口：`MutinyTurnManager.AdvanceSimulationTick` 完成单人第 15 关胜利，`MutinyGameHUD.SynchronizeGameEndPopup` 打开最终弹窗，`CompleteCampaignAndShowEnding` 进入结局。结局时钟推到 774 后再推进一次，外层帧保持 774，角色子时钟继续。
- 覆盖：91 张原始船部件载入尺寸、12 帧角色与 16 帧波纹循环、28 个图层配置、对白切换、分数面板和返回标题。
- 独立静态核对：91 张图片与 SWF 原始导出逐个 SHA-256 相同；28 个图层的符号 ID、顺序、twip 坐标与 SWF XML 相同。
- 待运行：原版与 Unity 同步录屏的逐像素画面对照；鼠标点击结局对白时不跳字的手动观察；音乐与悬停反馈。

原测试日志位于隔离工程，该临时工程已清理；以上通过状态来自本次 Unity 进程的实际控制台输出。
