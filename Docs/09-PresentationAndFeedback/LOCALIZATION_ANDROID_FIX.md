# Android 语言切换与首装默认值（2026-09-27）

## 实施前行为规格

| 稳定 ID | 可观察行为及状态转换 | 来源 | Unity 生产入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| LOC-ANDROID-01 | 在标题页反复执行 `setlanguage en → zh-cn → en → zh-cn → en`，每次按钮和对白译文与语言一致，所有按钮及 GM 仍可操作；任何表资源异常不得中断 IMGUI 绘制 | 用户 Android 故障报告；本地化为用户授权扩展，原版来源不适用 | `MutinyGMManager.ExecuteCommand` → `MutinyLocalization.Select/Text` → `MutinyFrontendController.OnGUI` | Android 实机逐步切换，检查四个标题按钮、GM 开关、Play 点击及异常日志 | 已实现；待用户运行验收 |
| LOC-ANDROID-02 | 全新安装且本机没有选过语言时，首屏为英文；本机用 GM 选定语言后，关闭并重新打开仍为该语言。云备份恢复旧语言偏好不得被误认为本机本次安装的选择 | 用户明确要求；原版标题英文，简中为授权扩展 | `MutinySaveSystem.LanguageCode` → `MutinyLocalization.Initialize` | Android 卸载重装首启核对英语；选简中后重启核对中文；再切英语后重启核对英语 | 已实现；待用户运行验收 |
| LOC-ANDROID-03 | GM 语言参数保持 `en` / `zh-cn`；内部标识仍为 `en` / `zh-Hans`，其他存档字段不受语言存储调整影响 | 用户既定 GM 规格 | `MutinyGMManager.ExecuteCommand`、`MutinySaveSystem.LanguageCode` | 从生产命令入口核对两种代码和无效输入；检查进度与成绩 | 已实现；待用户运行验收 |

## 静态证据与修复方向

- 当前 APK 已包含完整双语运行时文本，先前检查仅证明资源字节存在，不证明 Android 设备上的 String Table 安全。当前 `MutinyLocalization.Text` 仍先调用 Unity String Table，异常会在前端 `OnGUI` 中断绘制；该路径与空白按钮、无法点击的症状吻合，但缺少 Android logcat，不能把它写成已确认根因。
- 当前 Unity Localization 设置的启动选择器包含设备语言，简体中文系统可能先选 `zh-Hans`。语言持久化目前使用 Android SharedPreferences；系统自动备份可能在重装时恢复旧选择。需要让当前 UI 的默认值及安装局部选择不依赖上述两个路径。
- 以项目 `Mutiny.tsv` 派生且随包的运行时文本作为 IMGUI 唯一查询来源；String Table 保留作编辑器资产和内容校验。Android 语言偏好存入系统不备份目录，避免旧 SharedPreferences 在重装后被当成当前安装的选择。该调整仅影响语言偏好，进度及成绩仍走原存档层。

## 验收状态

- **静态确认**：代码路径和随包文本已核对；Android 抛错栈尚未取得。
- **已实现**：运行时 `Text` 只查随包资源，不进入 Addressables/String Table；切换只更新语言代码、安装偏好和对白刷新事件；前端 `OnGUI` 使用 `finally` 恢复矩阵和颜色。Unity Localization 的启动选择器只保留英语。Android 语言偏好使用 `getNoBackupFilesDir()`，不再读可能被系统恢复的旧 `PlayerPrefs` 语言键。生产 GM 回归和可选 Player 回归均新增五次来回切换断言。
- **实际测试通过**：`Assembly-CSharp.csproj` 编译 0 错误（11 个既有警告）；`Assembly-CSharp-Editor.csproj` 编译 0 错误、0 警告。未执行游戏或 Android 真机运行。
- **待运行验证**：由用户执行 Android 首装、重启、反复 GM 切换及点击验收；开发侧遵照用户要求不启动游戏。
- **已知差异**：安装局部语言偏好不随 Android 系统备份恢复；更新到本版时，旧 Android SharedPreferences 中的语言选择将不再生效，首次显示英语，之后的 GM 选择继续跨重启保存。
