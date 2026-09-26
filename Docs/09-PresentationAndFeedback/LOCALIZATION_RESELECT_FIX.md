# 打包版语言反复切换与 GM 代码修复（2026-09-27）

## 实施前行为规格

| 稳定 ID | 可观察行为与状态转换 | 来源 | Unity 生产入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| LOC-SWITCH-01 | 已载入的标题和对白从英语切到简中、再切回英语、再切到简中时，每次均显示对应译文；按钮和 GM 保持可操作 | 用户本次故障报告；简中为用户授权扩展，原版来源不适用 | `MutinyGMManager.ExecuteCommand` → `MutinyLocalization.Select/Text` → IMGUI/对白 | 在打包版依次执行 `setlanguage en`、`setlanguage zh-cn`、`setlanguage en`、`setlanguage zh-cn`，逐步检查 `frontend.play`、对白和交互 | 已实现；待用户运行验收 |
| LOC-SWITCH-02 | 打包的本地化内容能解析 `frontend.play`：英语为 `play`，简中为 `开始游戏`；Unity String Table 键关联异常时改用同源随包文本；两路都缺失时退回调用点英语并记录故障 | 本机候选版 `Player.log`；Unity Localization 表资产及人工维护的 `Mutiny.tsv` | `MutinyLocalization.LoadTables/Text`、Resources 内运行时文本 | 以实际构建资源检查随包文本与 TSV 一致，并在打包版查询中英文键值及缺失回退 | 已实现；Windows/Android 静态内容检查通过，运行待用户验收 |
| GM-09-REV1 | 简体中文 GM 参数改为 `zh-cn`，英语仍为 `en`；缺参、旧 `cn`、未知代码及额外参数拒绝且不改当前语言；已保存的内部 Locale `zh-Hans` 继续可读 | 用户本次新要求；原版来源不适用 | `MutinyGMManager.ExecuteCommand`、`MutinySaveSystem.LanguageCode` | 从实际 GM 入口检查新参数、大小写/空白、重复命令、最近历史和错误输入 | 已实现；待用户运行验收 |

## 静态证据

- 用户运行的候选版 `C:/Users/27487/AppData/LocalLow/RichardXue/Mutiny X/Player.log` 于 2026-09-27 00:57 记录英文、中文来回命令均执行，但 `zh-Hans:frontend.play` 及 `en:frontend.play` 均报缺译；因调用点回退英文，切回中文看似无效。日志没有新的 `NullReferenceException`。
- 源 String Table 与共享键数据均有 `frontend.play` 且 ID 同为 `1194319872`，因此须核对打包载入与查找路径，不能把用户报告仅归因于 GM 解析。
- 旧 Player 验证只覆盖首启后一次切换，未覆盖 `en → zh` 回切；旧通过记录不证明本规则。

## 验收状态

- **静态确认**：上述日志、表资产及测试缺口。
- **已实现**：在 `Resources/Localization/MutinyRuntime.txt` 随包提供与源 TSV 相同的 150 条双语文本，编辑器重建表时同步生成，生产构建入口拒绝过期或未被 TextAsset 导入的副本。Unity String Table 仅在能查到 `frontend.play` 时进入可用缓存；其余键查询失败时使用随包文本。字体路径与语言就绪判断也识别该文本。GM 和帮助文案改为 `zh-cn`，旧 `cn` 不再接受。生产 GM 回归及可选 Player 回归增加英文→中文回切和译文断言。
- **静态测试**：源 TSV 与随包文本 SHA-256 一致；`Assembly-CSharp.csproj` 编译 0 错误（11 个既有警告），`Assembly-CSharp-Editor.csproj` 编译 0 错误、0 警告；`git diff --check` 无补丁空白错误。
- **候选构建检查**：从提交 `68470c8` 构建正式产品名 `Mutiny X` 的 Windows 和 Android `1.0.2+6`，两个 Unity 批量构建报告均成功。Windows ZIP 223 条目、无 PDB；完整 TSV 字节存在于 Windows `resources.assets` 及 Android APK 的 `assets/bin/Data/c399df976a7a4fa8bfa5abac6f11aa31`。APK 包名 `com.RichardXue.MutinyX`、versionCode `6`、versionName `1.0.2`，签名证书 SHA-256 `7a5abcaf6fbb841a111a6b88cd9eed052ecf6d6c14f7e514cf8f3cce21e9d226` 与旧线上包一致。
- **候选文件**：`D:/My Project/Mutiny X/_localization_release_candidate/Builds/CandidateFix/assets/`。APK SHA-256 `a50fc29c5ff99b873774cb4480c34dc13c0c23cb061c36ed4aebcb7bffa04416`；Windows Setup `1e5327543c8607ef62ce3e97a2b10bbdc7175230f7c6528ba6155c11d74f2479`；Windows ZIP `a8fc50c9accb0a867bd82d708473874d15da9e654566c70a6e03e26abce048c5`；`SHA256SUMS.txt` `8ec2569aebd7fc5139b73940e575d73e7c9911963446b843eeed389c113e1555`。四个文件的实算大小与 SHA-256 均与候选 manifest 一致。尚未发布或运行候选游戏。
- **实际测试通过**：生产 Unity 批量构建入口的 Windows 与 Android 构建成功，候选包静态内容、版本、签名和 SHA-256 检查通过。游戏内语言切换与交互尚无新版本运行结果；按用户要求不由开发侧启动游戏。
- **待运行验证**：用户在 Windows 构建和 Android 设备依次执行 `setlanguage en → zh-cn → en → zh-cn`，每步检查标题 `play/开始游戏`、按钮与 GM 可操作；进入对白后重复切换，检查文本和字色；再执行旧 `setlanguage cn`，确认提示用法且当前语言不变。回报画面和 `Player.log`。开发侧未启动候选游戏。
- **已知差异**：GM 参数 `zh-cn` 仅为玩家输入代码；Unity Locale 标识与既有保存值继续使用 `zh-Hans`，避免重新生成本地化资源和丢失旧偏好。
- **未完全定位**：旧候选的 Unity String Table 查询为何在打包版对中英文键均返回缺译，尚无直接的 Bundle 内部诊断；本次随包文本路径确保 UI 有独立译文来源，不把根因推断写成已证实。
