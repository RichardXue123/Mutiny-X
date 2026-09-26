# 发行版语言切换故障规格（2026-09-27）

## 实施前行为规格

| 稳定 ID | 可观察行为与状态转换 | 来源 | Unity 生产入口 | 验收用例 | 当前验证状态 |
| --- | --- | --- | --- | --- | --- |
| LOC-BUILD-01 | 没有保存的语言时首启显示英语；GM 选过的受支持语言在重启后继续生效 | 用户本次要求；原版菜单/字体为英语 | `MutinyLocalization.Initialize`、`MutinySaveSystem.LanguageCode` | 独立产品标识下启动 Windows Player，检查首屏语言；GM 切换后重启 | 已实现；Player 首启英文、保存中文重启、保存英文重启均通过 |
| LOC-BUILD-02 | `setlanguage cn/en` 后标题页文字、按钮及 GM 保持可见、可点击，切换可反复执行 | 用户本次故障报告；英语菜单取证见 `03-FrontendUI/README.md`；中文为授权扩展 | `MutinyGMManager.ExecuteCommand` → `MutinyLocalization.Select` → `MutinyFrontendController.OnGUI` | Windows Player 实际启动、切换两次并检查文字、异常与交互 | 已实现；Player 标题文字与 GM 命令三阶段通过，未出现 OnGUI 异常；鼠标实际点击仍待人工运行 |
| LOC-BUILD-03 | 字符串表须具备共享键数据；若资源异常，显示调用点英语并保持输入可用，错误只记录一次 | Unity Localization 1.5.13 表与 SharedTableData 资源；用户本次故障 | `MutinyLocalization.LoadTables/Text`、发行构建资产 | Windows Player 真实构建运行；资源缺失时检查安全回退 | 已实现；Player 实际资源载入成功；资源缺失的负面故障注入待运行 |

## 故障证据

本机 `C:/Users/27487/AppData/LocalLow/RichardXue/Mutiny X/Player.log` 记录发行版 `v1.0.2`：首次加载后中文键缺译，随后 `UnityEngine.Localization.Tables.LocalizationTable.VerifySharedTableDataIsNotNull` 通过 `MutinyLocalization.Text` 在标题页按钮绘制时反复抛异常。发行版共享资源 Bundle 文件存在，但运行时 StringTable 的 SharedData 引用无效。这个证据说明当前打包运行失败，不把此前编辑器 Play Mode 的 124/124 当作发行版通过。

## 验收记录

### 静态确认

- 旧版 `Player.log` 在首屏的 `MutinyLocalization.Text` → `LocalizationTable.GetEntry` → `VerifySharedTableDataIsNotNull` 抛异常。按钮底图先绘制、文字查询后抛异常，后续按钮和 `GUI.matrix` 还原都不执行，符合用户观察到的空白按钮与失去交互。
- 两张 StringTable 资产的 `m_SharedData` 均指向 `Mutiny Shared Data.asset`；发布 Windows 包中存在独立的 `localization-assets-shared_assets_all.bundle`。运行时引用可能为空，不能用“表对象非空”判断就绪。
- 原设计按保存语言、设备语言、英文选首启语言；用户现在指定英文默认，本规格优先。

### 已实现

- 首次没有保存的语言时选择 `en`；`setlanguage cn` / `en` 仍写入独立偏好，后续启动尊重用户已保存的语言。
- 显式通过 Addressables 地址加载并持有 `SharedTableData`，在表对象的跨 Bundle 引用为空时补回；只有共享数据有效的表才进入可用缓存。查询及字体路径检查表的可用性，缺失时退回调用点英文，避免从 `OnGUI` 抛出本次异常。
- 增加可选的 standalone Player 验证入口 `-verifyLocalization first|saved-cn|saved-en`，只在指定参数下执行。验证走真实 GM 命令及标题文本查询；隔离构建使用独立产品名，不改动正式版存档。

### 实际测试通过

- Unity `6000.6.0f1` 使用生产 `MutinyReleaseBuild.Build` / `BuildPipeline.BuildPlayer` 构建 Windows Player 成功，版本设置为 `1.0.2+6`，`BuildReport` 成功，产物 350,979,314 字节。
- 隔离产品 `Mutiny X Localization Smoke` 的真实 Player 三次运行，退出码均为 0：首次 `en → setlanguage cn`；保存的 `zh-Hans → setlanguage en`；保存的 `en → setlanguage en`。标题键分别解析为 `play`、`开始游戏`、`play`；四帧渲染后无异常。旧版 4 MB 日志中的反复 `NullReferenceException` 未复现。
- [构建与 Player 精简日志](../11-VerificationAndDebug/02-PlayModeValidation/Artifacts/LOCALIZATION-PLAYER-BUILD-20260927.txt)。

### 待运行验证

- 当前发布的 `v1.0.2` 安装包仍包含旧代码；此修复尚未制作或发布新版本。需从本次修复提交重新打包，不能把旧包视为已更新。
- 鼠标对标题按钮与 GM 的实际点击、视觉截图、Android 设备上的启动与切换，以及故意移除共享资源的负面故障注入尚未完成。
- 按用户 2026-09-27 的要求，后续游戏运行验收由用户执行；开发侧不自行启动游戏。如果以后确需代为启动，须先屏蔽音乐和音效。

### 用户运行验收步骤

1. 使用包含本修复的新构建和未保存语言偏好的测试存档启动，确认标题按钮为英文，`Play` 可点击。
2. 打开 GM，输入 `setlanguage cn`，确认 `开始游戏` 可见，GM 仍可打开且按钮可点击；再输入 `setlanguage en`，确认英文恢复。
3. 再切换到中文并重启新构建，确认保留中文；切换回英文并重启，确认保留英文。
4. 检查 `Player.log` 没有 `VerifySharedTableDataIsNotNull`、`NullReferenceException`，并记录按钮及 GM 实际点击结果。

### 已知差异

- 设备为简体中文但没有保存偏好时，现在默认英语。这是用户明确要求的行为变更；已手动选择并保存的简体中文仍会在重启后保留。

### v1.0.2 下载文件替换候选（尚未发布）

- 用户选择在亲自运行验收后覆盖现有 GitHub v1.0.2 下载文件。此操作是本次授权的发布例外；现有一键发布脚本按 `EXT-REL-05` 拒绝覆盖已发布 tag，不能把候选误报为已上线。
- 以干净隔离 worktree 的 `d39495e` 构建正式产品名 `Mutiny X`，保留版本 `1.0.2+6`。Unity `6000.6.0f1` 的 Windows 和 Android `BuildReport` 均成功；Windows ZIP 为 223 个条目，无 PDB；Inno Setup 7 安装包版本为 `1.0.2`；APK 包名 `com.RichardXue.MutinyX`，versionCode `6`，versionName `1.0.2`，签名证书 SHA-256 与当前线上 APK 相同。
- 本地四个候选资产的文件大小和 SHA-256 均与候选 manifest 一致；文件位于 `D:/My Project/Mutiny X/_localization_release_candidate/Builds/Candidate/assets/`，详细值见该目录的 `candidate-manifest.json` 和 `SHA256SUMS.txt`。
- 候选 SHA-256：APK `07e1af9ef3b0d472fd0972686b3f8eedb0a65c1dfa09bfb296f906a5476879df`；Windows Setup `2b82e6aba0b06e879e5f2940f7ac52500bef2adc1f78faa3ece1a6ae3a35f318`；Windows ZIP `491a96c0a7fbeb9072e189dd8fe8c5dbdfb32e756ba5b2cacc5c4ad66927ef14`；校验清单 `474b094b6a12667fd1ccf5bbb1f0e026676aa89a812c6c93ba6ecbc74b70b65f`。
- 当前线上旧版四个资产对应的本地原件全部存在，实算 SHA-256 与原发布 manifest 和 GitHub 当前 digest 三方一致，可在替换失败时恢复。
- 用户要求自行运行游戏验收。候选包生成后没有启动 Windows Player、Android APK 或 Unity Play Mode；标题页实际点击、GM 再次切换和安卓真机待用户确认。确认前不替换线上资产。
- GitHub v1.0.2 Release 与 tag 仍指向旧提交 `613744b`，候选二进制来自 `d39495e`。若按用户选择覆盖同名文件，应保留旧资产备份、更新四个资产及校验清单并核对远端 SHA-256，同时在 Release 说明中注明源码 tag 与替换包的提交差异。
