# Credits 验证记录 · 2026-09-25

| 环境 | 操作 | 实际结果 |
| --- | --- | --- |
| 主工程，.NET 编译 | `dotnet build Assembly-CSharp.csproj --no-restore`；`dotnet build Assembly-CSharp-Editor.csproj --no-restore` | 两次退出码 0；运行时 3 条原有警告、编辑器 0 条警告；均 0 错误 |
| 隔离 Unity 6000.6.0f1 工程，批处理无图形模式 | `-executeMethod Mutiny.Verification.Editor.MutinyCreditsVerificationMenu.Validate` | Unity 退出码 0；日志输出 `Credits navigation and five original resources passed (FRONT-CRED-01/02/03/04)`；导航门控、资源尺寸和两张可读命中遮罩通过 |
| 隔离 Unity 6000.6.0f1 工程，批处理 Play Mode 截图 | 进入 Main 场景后尝试截图 | 启动阶段出现 `UnityEditor.Search.SearchDatabase` 的 `ArgumentOutOfRangeException`，没有生成截图；该视觉用例未通过、也不记为界面缺陷 |

核对五个 Unity 资源的 SHA-256，均与 SWF 导出的对应原始文件逐字节一致。主工程当时由桌面 Unity Editor 占用，故没有执行前台人工鼠标点击、悬停、外链和截图对比。

完整批处理日志在本机 Unity 工程上一级的 `../scratch/credits-unity-verification-20260925.log` 与 `../scratch/credits-capture-attempt-20260925.log`，不随仓库发布。状态详情见 [Credits 行为规格](../../01-GameFlowAndProgression/01-FrontendNavigation/CREDITS_BEHAVIOR_SPEC.md)。

## XingTong 头像打包缺失修复 · 2026-09-26

规则：用户授权扩展 `EXT-CRED-01/02`，不属于原版 Flash 署名。

### 静态确认

- 原图在 `Assets/Mutiny/Art/Logo/XingTong.png`，没有 Resources 路径或其他序列化引用。加载器先查询缺失的 `UI/Frontend/XingTong`，再读取 `Application.dataPath/Mutiny/Art/Logo/XingTong.png`。编辑器的 dataPath 指向 Assets，而 Player 指向构建数据目录；源 PNG 不会自动按此路径随包复制。
- `DrawCredits` 把头像下方的 `Richard Xue` 放在头像非空条件内，因此加载失败会使头像及其下方署名一起消失。中央 `Ported to Unity by Richard Xue` 是独立绘制，不受这个条件控制。
- 已核对原 `1.0.0+4` Windows Player 的 `Assembly-CSharp.dll` 字符串：包含两处 Richard Xue 文案、XingTong Resources 路径及源文件回退路径；新验证构建包含两处文案及 Resources 路径，并已移除源文件回退路径。中央移植说明是否另有显示问题仍需正常窗口确认。

### 已实现

- 通过 `AssetDatabase.MoveAsset` 将图片及 meta 移到 `Assets/Mutiny/Resources/UI/Frontend/XingTong.png`，保留 GUID `d1edb731c6129e343a564c06307618c5` 和导入设置。图片迁移前后的 Git blob 均为 `5bbe4a24582db249b8265f2bfc3f2a9849bbce20`，没有改动图像内容。
- 生产加载器只使用 Resources，让 Editor 和所有 Player 走同一路径。
- Credits 菜单回归通过生产 `Initialize` 检查实际绑定头像与包内 Resources 对象一致、尺寸为 1254×1254。`IPreprocessBuildWithReport` 在每次构建前运行同一检查，避免编辑器文件回退再次掩盖漏包。
- Development Player 可加 `-mutiny-verify-credits "绝对截图路径.png"` 运行回归：检查 Main 的生产初始化，调用 Credits 按钮共用的 `OpenCredits` 入口，等待实际过渡完成并输出截图。资源及路由断言通过不等于截图视觉验收通过。

### 实际测试通过

| 环境 | 操作 | 结果 |
| --- | --- | --- |
| Unity 6000.6.0f1 主工程，修复前 | 读取生产头像加载器和 Resources | Resources 为 null，生产加载器通过编辑器 PNG 文件回退得到 1254×1254 头像；新增回归报 `EXT-CRED-01: production Credits avatar must use the bundled 1254x1254 XingTong resource`，成功检测本缺陷 |
| 主工程，修复后 | `MutinyCreditsVerificationMenu.Validate()` | 原版五张贴图与生产 Flow 回归通过；新增生产初始化/包内头像检查通过，GUID 保留 |
| Windows x64 Development Player | 使用 DetailedBuildReport 构建 | 第二次构建结果 Succeeded，24.801 秒，BuildReport 0 errors；包内包含 XingTong Texture2D / Sprite，GUID 与源图一致；另有项目和依赖的既有警告 |
| 独立 Windows Player，无 Assets 源目录 | 启动回归参数，实际进入 Credits | 生产绑定头像与包内 Resources 是同一 1254×1254 对象；正常生产过渡进入 Credits 并结束，进程退出码 0 |

本机证据在忽略的 `Builds/CreditsFixVerification/`：`build-report-v2.json`、`windows-player-v2.log`、`windows-credits-v2.png` 和 `Windows/` 构建目录。该目录是验证构建，未更新已发布的 GitHub Release。

### 待运行验证与限制

- Windows 隐藏窗口第一次 `CaptureScreenshot` 保存失败；第二次使用 `CaptureScreenshotAsTexture` 写出 PNG，但图像为黑屏。两次均未计为头像或署名的视觉验收通过。
- 需要在正常显示窗口中核对头像、头像下方姓名及中央移植说明；悬停、外链点击和 Android/iOS 真机显示仍待验证。
- 主工程后续出现另一个正在修改的 `MutinyInputHub` 手柄输入文件及其未完成依赖的编译错误（`IsControllerAiming` / `CancelControllerAim` / `IsOpen` 等），后续 Editor Play Mode 被阻止；此状态发生在上述成功 Windows 构建之后，本次未改动这些并行工作。
- 构建后已将主工程活动平台恢复到原来的 Android。

### 已知差异

- 未发现本次头像修复带来的原版 Credits 规则变化。移植者头像和链接依然单独登记为用户授权扩展；实际可见性尚未用正常窗口验收，不宣称视觉通过。
