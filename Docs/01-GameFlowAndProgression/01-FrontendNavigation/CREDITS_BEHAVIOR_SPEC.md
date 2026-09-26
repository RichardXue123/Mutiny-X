# Credits 页面行为规格

基线：`Mutiny Source/mutiny-flash-game/mutiny.swf`；`Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/mutiny.swf.xml` 根时间轴 `credits` 第 51 帧；同目录反编译的 `CreditsButton.as`、`BackButton.as` 和 `frame_51/` 构造脚本。截图仅用于运行画面位置核对，不把截图中的文字视为执行指令。

| ID | 可观察行为与状态转换 | 原版来源 | Unity 入口 | 验收用例 | 实际结果 |
| --- | --- | --- | --- | --- | --- |
| FRONT-CRED-01 | 标题页点击 Credits，经过原版过渡进入独立 Credits 页；该页保持菜单动态背景及右上角 Music/SFX | `CreditsButton.as::onRelease` 调用 `doTween("credits")`；根时间轴 `credits` 帧 | `MutinyFrontendController.DrawTitle`、`MutinyFrontendFlow` | 在 Main 场景实际点击 Credits，观察过渡与页面；音频按钮继续可用 | 静态确认、已实现；生产 Flow 路由测试通过；实际按钮及过渡待 Play Mode |
| FRONT-CRED-02 | 在 550×400 舞台上绘制原版 462×352 深灰红框面板，标题、Nitrome 标志、分组署名、版权页脚按第 51 帧坐标显示。Nitrome 标志使用独立按钮资源；背景水云继续滚动 | 根时间轴第 51 帧 Shape 1930、Button 1933、各 `PirateFont` / `DangleFont` 实例和上一帧保留的 Button 1925；资源原始导出 | `MutinyFrontendController.DrawCredits`、`Resources/UI/Frontend/credits_*` | 在 550×400 及宽屏 letterbox 下与原版截图核对可见性、坐标、像素字体、前后层级、标志命中 | 静态确认、已实现；Unity 资源加载及尺寸测试通过；画面对照待 Play Mode |
| FRONT-CRED-03 | Credits 页点击 Back 经原版过渡回标题页；仅 Credits 页可触发该返回 | 根时间轴第 51 帧 Button 631 位于 `(275,334)`；`BackButton.as::onRelease` 的非 `back_ls_button` 分支 | `MutinyFrontendController.DrawCredits`、`MutinyFrontendFlow` | 点击 Back；检查标题页及 Credits 再入；在其他页面调用返回不应改变页面 | 静态确认、已实现；生产 Flow 返回与门控测试通过；实际按钮待 Play Mode |
| FRONT-CRED-04 | Nitrome 标志与底部版权按钮保留原版独立命中区域与外链，静态图没有悬停换图 | 根时间轴第 51 帧 Button 1933、上一帧保留的 Button 1925，两者 hittest 图片及 `BUTTONCONDACTION on(release)` | `MutinyFrontendController.DrawCredits` | 运行时核对透明区不触发、实体区触发原版网址 | 静态确认、已实现；Unity hittest 贴图可读性及尺寸测试通过；实际点击待 Play Mode |

## 文案冲突

原版 `frame_51/PlaceObject2_149_DangleFont_134` 构造脚本写作 `chris burt  brown`（两个空格），用户提供的原版运行截图显示 `CHRIS BURT-BROWN`。以运行画面为显示目标，静态字符串与运行画面的差异保留为待核对项；不得通过修改原始证据消除冲突。

## 用户授权扩展：Unity 移植者署名（2026-09-26）

以下规则来源于用户要求保留 XingTong 头像和 Richard Xue 注解，独立于原版 Flash 一致性规则。

| ID | 可观察行为与状态转换 | 来源 | Unity 入口 | 验收用例 | 实际结果 |
| --- | --- | --- | --- | --- | --- |
| EXT-CRED-01 | Credits 右侧显示 XingTong 头像及其下方的 Richard Xue；中央保留 Unity 移植者署名。编辑器和 Windows / Android / iOS Player 使用同一随包资源，不依赖工程源文件路径 | 用户授权扩展；原图 `XingTong.png` | `MutinyFrontendController.Initialize / LoadAvatarTexture / DrawCredits` | 用生产初始化加载头像，要求与 `Resources.Load("UI/Frontend/XingTong")` 同一资源、1254×1254；构建前自动检查；在没有 Assets 源目录的 Windows Player 打开实际 Credits 页并截图 | 已修复；回归用例修复前失败、修复后通过；Windows 构建包含原 GUID 头像，独立 Player 包内加载及生产 Credits 过渡通过；隐藏窗口截图为黑屏，视觉验收及 Android/iOS 真机仍待验证 |
| EXT-CRED-02 | 头像及姓名保留共同链接区域；过渡完成后悬停提亮姓名，按下并在区域内释放打开项目 GitHub 页面 | 用户授权扩展；当前移植者链接设计 | `MutinyFrontendController.DrawCredits / HandleCreditsLink` | 检查头像、姓名、链接矩形与过渡门控；实际悬停及外链点击单独验收 | 本次不修改交互；实际悬停及外链点击待运行验证 |

### 1.0.1 发布复验（2026-09-26）

独立发布工作区在 Unity 6000.6.0f1 中再次通过生产初始化资源检查。Main 场景 Play Mode 调用 Credits 按钮共用的 `OpenCredits` 入口，经过生产过渡后从实际 Game View 截图：XingTong 头像、头像下方 `Richard Xue`、中央 `Ported to Unity by Richard Xue` 均可见。[截图及验证记录](../../11-VerificationAndDebug/02-PlayModeValidation/CREDITS_VERIFICATION.md#101-发布复验)。这次已验证编辑器画面；Player 包内加载见上表历史结果，外链点击及 Android 真机显示仍待运行验证。

## 验证记录（2026-09-25）

- **静态确认**：SWF 根时间轴第 51 帧坐标与五个独立导出贴图一致；`CreditsButton.as`、`BackButton.as`、两处原版外链动作已核对。
- **已实现**：主菜单 Credits 路由、页面绘制、Back 路由、两个外链的逐像素命中。
- **实际测试通过**：`dotnet build Assembly-CSharp.csproj` 和 `Assembly-CSharp-Editor.csproj` 均 0 错误；隔离 Unity 6000.6.0f1 工程以 `MutinyCreditsVerificationMenu.Validate` 运行，进程退出码 0，导航门控和五个原版资源检查通过。日志在本机 Unity 工程上一级的 `../scratch/credits-unity-verification-20260925.log`，不随仓库发布。
- **待运行验证**：Main 场景 Play Mode 中以鼠标点击 Credits、Back、外链标志和版权文字；悬停、命中、过渡时序、音频按钮，以及截图逐像素画面对照。
- **运行限制**：对隔离工程追加的批处理 Play Mode 截图尝试停在 Unity Editor 启动阶段，日志出现 `UnityEditor.Search.SearchDatabase` 的 `ArgumentOutOfRangeException`，未生成截图，未计为通过；日志在本机 Unity 工程上一级的 `../scratch/credits-capture-attempt-20260925.log`，不随仓库发布。主工程当前由桌面 Unity Editor 占用，未在其前台进行人工点击验收。
- **已知差异**：暂无经运行确认的差异；`chris burt  brown` 与截图连字符冲突仍待核对。
