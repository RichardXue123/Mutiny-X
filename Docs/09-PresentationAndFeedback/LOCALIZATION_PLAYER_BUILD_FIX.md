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
