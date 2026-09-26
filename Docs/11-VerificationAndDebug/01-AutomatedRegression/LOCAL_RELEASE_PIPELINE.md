# 本地 tag 发布流水线

本功能是用户授权的发布工具扩展，不涉及 Flash 原版规则。来源：用户要求实现本地一键发布流程。

## 行为规格（实现前登记）

| ID | 可观察行为 / 状态转换 | 生产入口 | 验收用例 | 当前实际结果 |
| --- | --- | --- | --- | --- |
| EXT-REL-01 | 只接受 `v主.次.修订`；tag 源码中的 bundleVersion 必须相同，Android 构建号必须为正数；发布前远端 tag 必须与本地指向相同提交 | Publish-Release.ps1 | 无效 tag、版本不匹配、远端缺失或不匹配时失败，且不构建/发布 | 已实现；生产脚本回归通过 |
| EXT-REL-02 | 从 tag 提交创建独立 Git worktree，默认 Windows + Android；当前工作目录的未提交修改不进入产物 | Publish-Release.ps1 / MutinyReleaseBuild.Build | 脏工作目录不污染构建；真实 Unity 批处理构建两个目标 | 已实现；隔离回归与两个真实平台构建通过 |
| EXT-REL-03 | 各平台在独立 Unity 进程中通过实际 BuildPipeline 入口构建；版本与构建号不自增；进程失败、报告缺失或失败立即停止 | MutinyReleaseBuild.Build / MutinyBuild.BuildTargetNow | 模拟进程失败/缺报告；真实平台构建报告成功 | 已实现；真实成功构建和错误版本退出码 1 均通过 |
| EXT-REL-04 | Windows staging 排除 PDB、Unity 备份和 Burst 调试目录；生成 ZIP、Inno Setup EXE；Android 固定 APK；全部文件记录大小和 SHA256 | ReleaseTools.psm1 | ZIP 内容与安装包来源检查；真实 Inno 编译；损坏资产校验失败 | 已实现；真实 Inno 编译、223 个 ZIP 条目过滤、四个资产 SHA256 均通过 |
| EXT-REL-05 | 发布先创建草稿，上传指定资产并验证大小和 GitHub SHA256；完整验证后转为正式 Release；已发布 tag 拒绝覆盖 | ReleaseTools.psm1 | 以 GitHub CLI 替身驱动真实发布逻辑，验证草稿→上传→校验→发布的顺序，以及失败不发布 | 已实现；CLI 替身回归与 v1.0.2 真实线上发布、四个资产校验通过 |
| EXT-REL-06 | `-Resume` 仅复用同 tag/提交/平台且 SHA256 匹配的完整 manifest；重复上传可跳过匹配资产；只操作可核验身份的草稿 | Publish-Release.ps1 / ReleaseTools.psm1 | 中断上传后恢复、文件被改动、草稿标题/说明/提交不匹配、额外资产 | 已实现；真实本地产物恢复、v1.0.2 网络中断后的发布恢复及 CLI 替身恢复通过 |
| EXT-REL-07 | `-BuildOnly` 完成构建和本地校验，无需 GitHub 登录且不创建/推送 tag 或发布 Release | Publish-Release.ps1 | 实际本地构建并确认无 GitHub 写操作 | 已实现；本机 gh 未登录时真实构建及恢复通过 |
| EXT-REL-08 | Windows 下长原版证据路径也能检出；临时源码目录采用短路径，Git worktree 操作启用 core.longpaths | Publish-Release.ps1 | 含长 AS2 路径的 Git 仓库检出；仅删除本次创建且根目录核验通过的临时 worktree | 已实现；首次真实验证发现问题后修复，长路径回归及真实检出通过 |
| EXT-REL-09 | 中文更新说明在非 UTF-8 控制台及恢复读取时保持原文 | Invoke-ReleaseTool / manifest 读取 | 控制台使用 CP936，tag 说明包含“加入头像”，构建、恢复、CLI 草稿说明均保持一致 | 已实现；Windows PowerShell 与 PowerShell 7 回归通过 |
| EXT-REL-10 | 指定的本地 tag 不存在时，立即提示 tag 名称、现有 tag 和准备步骤；已存在但不指向提交的 tag 单独提示；旧 tag 缺少流水线入口时，在 GitHub 登录前说明原因 | Get-ReleaseLocalTag / Publish-Release.ps1 | 缺少 tag、tree tag、实际旧 tag 均只进入前置检查，不构建、不发布，不显示缺 tag 引起的 Git 128 堆栈 | 已修复；新增 3 项回归通过；实际仓库缺 tag / 旧 tag 检查通过 |
| EXT-REL-11 | 版本不匹配时，提示 tag 提交中的实际版本和期望版本，明确当前工作目录的修改尚未进入 tag | Get-ReleaseSettings | 错误包含实际版本、期望版本和提交步骤；错误不放宽固定源码规则 | 已实现；生产入口回归通过 |
| EXT-REL-12 | gh 登录无效时，可在本次进程中复用系统 Git 凭据；失败或完成后恢复原 GH_TOKEN，凭据不进入日志、manifest 或提交 | Initialize-ReleaseGitHubAuth / Publish-Release.ps1 | 有效 gh 登录、缓存凭据回退、缓存不可用、结束后的环境恢复 | 已实现；有效登录、缓存回退、失败恢复回归通过 |

## 验证记录

### 静态确认

- tag 的项目设置与源码提交是版本来源，批处理入口复用生产 `MutinyBuild.BuildTargetNow` / `BuildPipeline.BuildPlayer`。
- 两个平台分别启动 Unity，启动时指定目标，避免 executeMethod 内切换平台导致脚本域重载。
- GitHub 发布前后均核验身份及完整资产；已发布版本不覆盖。没有新版本发布指令时不创建测试用线上 Release。

### 已实现

- `BuildTools/Release/Publish-Release.ps1`、`ReleaseTools.psm1`、说明模板、使用文档及回归入口。
- `Assets/Editor/MutinyReleaseBuild.cs` 单平台批处理入口与结构化结果、明确退出码。
- 原版规则未修改，本功能为用户授权扩展。

### 实际测试通过（2026-09-26）

- Windows PowerShell 5.1、PowerShell 7.6.5 各 **32 项**生产入口回归通过。外部 Unity / Inno / GitHub CLI 由有状态替身提供；Git 仓库、子进程、隔离目录、打包、SHA256、恢复及失败处理使用实际生产脚本。
- 基于游戏提交 `cd6ad27` 加本次构建入口制作独立本地验证快照，版本 `1.0.1+5`；验证 tag 仅存在于该临时仓库，未推送。真实 Unity `6000.6.0f1` 构建 Android、Windows 均成功。
- BuildPipeline 用时：Android `316.010 秒`，Windows `95.345 秒`，不含冷导入及进程启动时间。
- 真实 Inno Setup 7 编译成功；Windows ZIP 223 个条目，禁止发布的 PDB / 备份 / Burst 调试内容为 0。
- APK `com.RichardXue.MutinyX`，版本名 `1.0.1`，构建号 `5`，ARM64，minSdk 26、targetSdk 36。
- 使用最终脚本 `-Resume -BuildOnly` 复用四个真实产物，大小和 SHA256 再次校验成功。
- 真实 Unity 请求 `1.0.2` 而源码冻结为 `1.0.1` 时，生产入口写出失败报告并以 **退出码 1** 结束，没有生成错误版本的程序。
- 摘要证据：[LOCAL-RELEASE-PIPELINE-20260926.json](Artifacts/LOCAL-RELEASE-PIPELINE-20260926.json)。构建报告、日志和产物保存在本地验证目录 `D:/My Project/Mutiny X/_release_check_260926/Builds/Release/v1.0.1/`。

### 待运行验证

- 安装包安装/卸载及 Android 真机运行不在本次流水线验收范围内，未执行。

### 缺少本地 tag 的诊断修复（2026-09-26）

- 静态确认：用户报告 `git rev-parse --verify` 的 `Needed a single revision`；实际仓库仅有 `v1.0.0`、`v1.0.1`，没有文档示例 `v1.0.2`。流水线尚为未提交文件，Unity 版本仍为 `1.0.1+5`。
- 已实现：先查本地 tag，再解析提交；缺少 tag 提示现有 tag、远端获取方式及首次准备步骤；tree tag 单独提示。缺构建入口或更新说明的 tag 在 GitHub 登录前中止，并给出原因。
- 实际测试：两个 PowerShell 版本各 **35 项**回归通过（原 32 项加缺 tag 提示、无输出目录、非提交 tag 的 3 项）。实际工程执行 `-Tag v1.0.2` 得到明确缺 tag 提示，`-Tag v1.0.1` 得到缺新批处理入口提示；均在无效 GhPath 下仍先返回正确诊断，无构建/发布操作。
- 该轮仅修复前置检查和使用说明，未重复打包或创建正式 tag / Release；后续版本准备和真实发布结果见下文。

### 已知差异 / 限制

- 只支持 Windows 本地环境、正式版 `v主.次.修订` tag、Windows / Android；不构建 iOS。
- Android 签名继承项目及本机 Unity 配置；自定义 keystore 文件和密码仍是本机配置。
- 首次独立构建需冷导入资源，耗时高于使用现有 Library 缓存。验证中出现既有着色器和编辑器退出警告，两个构建结果均为 Succeeded。

### v1.0.2 的版本准备修复

- 静态确认：本地及远端 `v1.0.2` 对象为 `75f2eed6b9ec9277de83b32661d780b18742de02`，指向 `cd6ad27`；该提交版本为 `1.0.1+5`。用户工作目录改为 `1.0.2`，但版本及新流水线尚未提交。
- 已核验：通过已认证 GitHub API 查询，`v1.0.2` 无正式 Release，也无草稿。因此可将这次误打的未发布 tag 更新到正确发布提交，使用旧 tag 对象作为远端 force-with-lease 条件，并在本地 `refs/backup/releases/v1.0.2-before-pipeline` 保留旧引用。
- 已准备：Unity 版本 `1.0.2`、Android 构建号 `6`；用户确认说明为手柄支持、中英文界面与对白、本地一键发布流程。说明保存在 `BuildTools/Release/Notes/v1.0.2.md`。
- 回归：Windows PowerShell 5.1、PowerShell 7.6.5 各 **40 项**通过，包括版本诊断、gh 凭据有效、Git 缓存凭据回退、无缓存的失败提示、成功/失败后的 GH_TOKEN 恢复。
- 已执行：发布源码提交为 `613744bfa5a0ab009dbac21310cf58eb6c34d69f`，已推送 main；本地及远端 `v1.0.2` 更新为注解 tag 对象 `ac204a21b90c654c7eac41515c7d26a307906f32`，指向该提交。之后的验收文档提交不移动已发布 tag。

### v1.0.2 真实构建与 GitHub 发布

- 生产入口：在主工程执行 `Publish-Release.ps1 -Tag v1.0.2`，从上述固定提交创建独立 worktree。真实 Unity `6000.6.0f1` 分别构建 Android、Windows，两个结构化报告均为成功，版本均为 `1.0.2+6`。
- 实际 BuildPipeline 用时：Android `300.395 秒`、Windows `85.011 秒`，不含导入及进程启动。Inno Setup 7 编译成功，用时 `25.062 秒`。Windows ZIP 223 个条目，禁止发布的 PDB / 备份 / Burst 调试内容为 0；安装包 ProductVersion 为 `1.0.2`。
- 实际 APK 检查：包名 `com.RichardXue.MutinyX`，versionName `1.0.2`、versionCode `6`，ARM64，minSdk 26、targetSdk 36。
- 网络中断：完整 manifest 已生成后，读取 GitHub Release 列表时出现 `TLS handshake timeout`，该轮退出。随后通过生产 `-Resume` 校验并复用全部本地资产，没有重新构建；完成真实草稿创建、上传、逐个大小/SHA256 核验及正式发布。
- GitHub 最终状态：Release ID `397296081`，`draft=false`、`prerelease=false`，发布时间 `2026-09-26T15:56:12Z`，源码提交与 tag 一致；四个资产均为 `uploaded`，远端大小与 digest 和本地 manifest 全部一致。
- 发布内容：Windows ZIP、Windows Setup EXE、Android APK、`SHA256SUMS.txt`；更新说明为用户确认的“加入手柄支持 / 加入中英文界面与对白 / 加入本地一键发布流程”。[正式 Release](https://github.com/RichardXue123/Mutiny-X/releases/tag/v1.0.2)。
- 摘要证据：[LOCAL-RELEASE-V1.0.2-20260926.json](Artifacts/LOCAL-RELEASE-V1.0.2-20260926.json)。完整构建报告、日志、manifest 和 GitHub 最终校验快照保存在 `Builds/Release/v1.0.2/`。本次临时源码 worktree 已自动移除。
- 原版一致性规则未修改；本轮证明版本准备、生产构建及发布流水线通过，安装/卸载和 Android 真机运行仍未执行。
