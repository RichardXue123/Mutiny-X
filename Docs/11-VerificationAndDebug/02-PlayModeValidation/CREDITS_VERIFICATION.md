# Credits 验证记录 · 2026-09-25

| 环境 | 操作 | 实际结果 |
| --- | --- | --- |
| 主工程，.NET 编译 | `dotnet build Assembly-CSharp.csproj --no-restore`；`dotnet build Assembly-CSharp-Editor.csproj --no-restore` | 两次退出码 0；运行时 3 条原有警告、编辑器 0 条警告；均 0 错误 |
| 隔离 Unity 6000.6.0f1 工程，批处理无图形模式 | `-executeMethod Mutiny.Verification.Editor.MutinyCreditsVerificationMenu.Validate` | Unity 退出码 0；日志输出 `Credits navigation and five original resources passed (FRONT-CRED-01/02/03/04)`；导航门控、资源尺寸和两张可读命中遮罩通过 |
| 隔离 Unity 6000.6.0f1 工程，批处理 Play Mode 截图 | 进入 Main 场景后尝试截图 | 启动阶段出现 `UnityEditor.Search.SearchDatabase` 的 `ArgumentOutOfRangeException`，没有生成截图；该视觉用例未通过、也不记为界面缺陷 |

核对五个 Unity 资源的 SHA-256，均与 SWF 导出的对应原始文件逐字节一致。主工程当时由桌面 Unity Editor 占用，故没有执行前台人工鼠标点击、悬停、外链和截图对比。

完整批处理日志在本机 Unity 工程上一级的 `../scratch/credits-unity-verification-20260925.log` 与 `../scratch/credits-capture-attempt-20260925.log`，不随仓库发布。状态详情见 [Credits 行为规格](../../01-GameFlowAndProgression/01-FrontendNavigation/CREDITS_BEHAVIOR_SPEC.md)。
