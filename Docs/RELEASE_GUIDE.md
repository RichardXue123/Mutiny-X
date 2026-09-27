# Mutiny X 完整发布流程与报错排查

适用于本项目的 Windows 本地 PowerShell 发布流程。入口为 `BuildTools/Release/Publish-Release.ps1`，默认构建 Windows 和 Android，并发布到 GitHub Release。

本指南根据 v1.0.2、v1.0.3 的实际报错和后续发布记录整理，更新日期：2026-09-28。

**版本进展：v1.0.5 / 构建号 9 已正式发布。** 下文保留 v1.0.4 作为完整操作示例；准备下一次发布时，请将示例版本换为 `v1.0.6 / 1.0.6`，构建号换为 `10`，并同步修改说明文件名、命令和输出路径。

## 1. 先理解发布顺序

```text
完成游戏改动并验证
  → 保存 Unity 版本和 Android 构建号
  → 创建并填写本次更新说明
  → 提交代码、资源、版本配置、更新说明
  → 检查提交内容
  → 创建版本 tag
  → 推送 main 和 tag
  → 一条命令构建、打包、上传、校验并发布
```

**tag 对应的提交决定打包内容。** 修改工作目录中的版本、代码或说明文件，不会改变已经创建的 tag。

现有“一键发布”自动完成构建、打包及 GitHub 发布；版本设置、更新说明、提交和 tag 需要先准备好。脚本不会自动修改版本、递增构建号、创建或推送 tag。

### 版本、tag 与文件名的对应关系

| 项目 | v1.0.3 的实际值 | 本指南示例（v1.0.4 已发布） |
| --- | --- | --- |
| Unity Version / `bundleVersion` | `1.0.3` | `1.0.4` |
| Git tag | `v1.0.3` | `v1.0.4` |
| Android Bundle Version Code | `7` | `8` |
| 更新说明 | `BuildTools/Release/Notes/v1.0.3.md` | `BuildTools/Release/Notes/v1.0.4.md` |

`v1.0.3`、`v1.0.4` 已正式发布，正常下一次发布请使用新版本。Android 构建号应高于此前发布的值，不能只修改 Version；当前脚本检查其为正整数，不会替你检查所有历史版本的递增关系。

## 2. 首次环境准备

- Windows，PowerShell 5.1 或更高版本。
- Git；`origin` 指向 `https://github.com/RichardXue123/Mutiny-X.git`。
- `ProjectSettings/ProjectVersion.txt` 对应的 Unity Editor；当前项目为 `6000.6.0f1`。通过 Unity Hub 安装 Windows / Android 构建模块以及 Android SDK、NDK、OpenJDK。
- Inno Setup 7 或兼容的 6，用于生成 Windows 安装包。
- GitHub CLI `gh`，账号具有本仓库发布权限。

从实际 Unity 工程目录执行命令，目录内应包含 `Assets`、`Packages`、`ProjectSettings` 和 `BuildTools`：

```powershell
Set-Location -LiteralPath 'D:\My Project\Mutiny X\Mutiny X'
git --version
gh --version
git remote -v
gh auth status
```

GitHub 登录不可用时执行：

```powershell
gh auth login
```

发布脚本也支持 `GH_TOKEN`，并可在 gh 凭据无效时临时复用系统 Git 缓存凭据；完成或失败后恢复原环境变量。令牌、签名密码不要写入更新说明或提交到 Git。

Unity 编辑器可以继续打开当前工程，流水线会在独立 worktree 构建。首次构建需冷导入资源，等待时间包括导入、编译、打包与上传。

## 3. 每次发布的完整操作（以 v1.0.4 为例）

以下命令按顺序在同一个 PowerShell 窗口执行。下一次发布时改 `$releaseTag` 和 `$releaseCode`。任何步骤报错都先处理，不能继续创建 tag。

### 第一步：定义版本，并检查本次改动

```powershell
Set-Location -LiteralPath 'D:\My Project\Mutiny X\Mutiny X'
$ErrorActionPreference = 'Stop'
Import-Module '.\BuildTools\Release\ReleaseTools.psm1' -Force -DisableNameChecking
$releaseGit = (Get-Command git.exe -CommandType Application).Source
$releaseTag = 'v1.0.4'
$releaseVersion = Get-ReleaseVersion $releaseTag
$releaseCode = 8
$releaseNotesPath = "BuildTools/Release/Notes/$releaseTag.md"

Invoke-ReleaseTool $releaseGit @('status', '--short', '--branch')
Invoke-ReleaseTool $releaseGit @('diff', '--stat')
```

本指南使用已有的 `Invoke-ReleaseTool` 包装 Git 命令；Git 非零退出时立即报错，避免 PowerShell 中“提交失败后仍继续打 tag”。

确认正在 `main`，待发布的功能已合并并验证。用 Git 客户端选择并提交本次要发布的游戏代码、资源和配套文档。只有确认所有工作目录改动都属于本次发布时，才使用 `git add -A`；否则逐个选择文件。未提交的改动不会进入打包产物。

### 第二步：保存 Unity 版本与构建号

在 Unity 的 `Edit → Project Settings → Player` 中设置：

- Version：`1.0.4`，不带 `v`。
- Android 的 Bundle Version Code：`8`。

保存设置，然后从磁盘确认：

```powershell
Select-String -LiteralPath 'ProjectSettings/ProjectSettings.asset' `
  -Pattern '^\s*bundleVersion:','^\s*AndroidBundleVersionCode:'
```

应该看到 `bundleVersion: 1.0.4` 和 `AndroidBundleVersionCode: 8`。通过流水线发布时不要先点击旧的手动构建菜单，它会递增构建号。

### 第三步：实际创建更新说明文件

```powershell
if (!(Test-Path -LiteralPath $releaseNotesPath)) {
    Copy-Item -LiteralPath 'BuildTools/Release/Notes/TEMPLATE.md' -Destination $releaseNotesPath
}
notepad.exe $releaseNotesPath
```

在编辑器中把模板的“填写本次更新”等占位内容替换成本次真实改动，保存为 UTF-8。文件名必须与 tag 一致：`v1.0.4.md`。关闭编辑器后核对：

```powershell
Get-Content -LiteralPath $releaseNotesPath -Encoding UTF8
```

**这一步需要创建、填写、保存文件，再把文件提交；仅复制发布命令不会生成说明。**

### 第四步：提交发布配置和说明

以下两项与已提交的游戏改动一起构成本次发布源码：

```powershell
Invoke-ReleaseTool $releaseGit @('add', '--', 'ProjectSettings/ProjectSettings.asset', $releaseNotesPath)
Invoke-ReleaseTool $releaseGit @('diff', '--cached', '--stat')
Invoke-ReleaseTool $releaseGit @('diff', '--cached', '--', 'ProjectSettings/ProjectSettings.asset', $releaseNotesPath)
```

查看暂存内容，确认版本、构建号、说明都正确，再提交：

```powershell
Invoke-ReleaseTool $releaseGit @('commit', '-m', "Prepare $releaseTag release")
```

如果之前已经提交这些内容，不必重复提交；直接执行下一步，核验 `HEAD` 内容。若报 `nothing to commit`，先确认内容确实在提交内，不要直接假定准备成功。

### 第五步：在打 tag 前核验提交内容

```powershell
$releaseSettings = Invoke-ReleaseTool $releaseGit @('show', 'HEAD:ProjectSettings/ProjectSettings.asset')
$committedCode = Get-ReleaseSettings $releaseSettings $releaseVersion
if ($committedCode -ne $releaseCode) {
    throw "HEAD 中的 Android 构建号应为 $releaseCode，实际为 $committedCode。先修正并提交。"
}
Invoke-ReleaseTool $releaseGit @('show', "HEAD:$releaseNotesPath")
Invoke-ReleaseTool $releaseGit @('ls-tree', '--name-only', 'HEAD', 'Assets/Editor/MutinyReleaseBuild.cs')
Invoke-ReleaseTool $releaseGit @('status', '--short', '--branch')
```

必须满足：

- `Get-ReleaseSettings` 没有版本不匹配错误。
- Android 构建号等于本次计划的值。
- `git show` 打印出本次真实更新说明，而非不存在或模板占位文字。
- 构建入口 `Assets/Editor/MutinyReleaseBuild.cs` 在提交树中。
- 所有希望随本版发布的改动都已提交。

### 第六步：创建并推送 tag

先检查本地和远端没有同名 tag：

```powershell
$existingLocalTag = Invoke-ReleaseTool $releaseGit @('tag', '--list', $releaseTag)
if ($existingLocalTag) { throw "$releaseTag 已在本地存在。先检查 tag 状态，不要重复创建。" }
$existingRemoteTag = Invoke-ReleaseTool $releaseGit @('ls-remote', 'origin', "refs/tags/$releaseTag")
if ($existingRemoteTag) { throw "$releaseTag 已在远端存在。先检查 tag 状态，不要覆盖。" }

Invoke-ReleaseTool $releaseGit @('tag', '-a', $releaseTag, '-m', "Release $releaseTag")
Invoke-ReleaseTool $releaseGit @('push', '--atomic', 'origin', 'main', "refs/tags/${releaseTag}:refs/tags/${releaseTag}")
```

`--atomic` 将 main 和 tag 一起推送。推送失败时先解决原因，再推送现有 tag；不要重新运行创建 tag 的命令。

再确认 tag 对应提交、设置、说明以及远端引用：

```powershell
Invoke-ReleaseTool $releaseGit @('rev-parse', $releaseTag, "${releaseTag}^{commit}")
Invoke-ReleaseTool $releaseGit @('show', "${releaseTag}:$releaseNotesPath")
Invoke-ReleaseTool $releaseGit @('ls-remote', 'origin', "refs/tags/$releaseTag", "refs/tags/$releaseTag^{}")
```

注解 tag 的对象 SHA 与它指向的提交 SHA 不相同，这是正常现象；本地 tag 对象和提交应分别与远端相应引用一致。

### 第七步：一条命令构建并发布

```powershell
.\BuildTools\Release\Publish-Release.ps1 -Tag $releaseTag
```

下次打开新 PowerShell 窗口时，变量不再存在，可以直接使用实际 tag：

```powershell
.\BuildTools\Release\Publish-Release.ps1 -Tag v1.0.4
```

流水线从 tag 创建独立源码目录，逐个平台执行 Unity，生成 ZIP、安装包、APK 和校验文件；先创建 GitHub 草稿，上传并验证每个文件的大小与 SHA256，全部成功才转为正式 Release。

正式发布成功的日志包含：

```text
[Release] Published and verified: https://github.com/RichardXue123/Mutiny-X/releases/tag/v1.0.4
```

## 4. 发布产物与成功检查

以上示例的 GitHub Release 应包含四个文件，不包含 iOS：

| 文件 | 用途 |
| --- | --- |
| `MutinyX-Windows-1.0.4+8.zip` | Windows 便携版本，完整解压后运行游戏 |
| `MutinyX-Setup-1.0.4+8.exe` | Windows 安装包 |
| `MutinyX-Android-1.0.4+8.apk` | Android 安装包 |
| `SHA256SUMS.txt` | 上述三个产物的 SHA256 |

本地输出在 `Builds/Release/v1.0.4/`：

| 路径 | 内容 |
| --- | --- |
| `manifest.json` | tag 对象、源码提交、版本、构建号、平台、说明、资产大小及 SHA256 |
| `release-notes.md` | 本次保存的发布说明 |
| `runs/<运行编号>/assets/` | 最终 ZIP / EXE / APK / SHA256 文件 |
| `runs/<运行编号>/Android.log`、`Windows.log` | Unity 构建日志 |
| `runs/<运行编号>/Android-report.json`、`Windows-report.json` | 成功/失败及版本的结构化报告 |
| `runs/<运行编号>/inno.log` | 安装包编译日志 |
| `github-verified.json` | 最后一次 GitHub 校验快照；成功后应为 `draft=false` 且有发布时间 |

GitHub 页面也应显示正确 tag、说明和完整的四个文件。构建和上传校验通过，不等于已执行游戏功能、Windows 安装/卸载或 Android 真机验收。

## 5. 网络中断与恢复

先看完整 manifest 是否存在：

```powershell
Test-Path -LiteralPath 'Builds/Release/v1.0.4/manifest.json'
```

| 当前状态 | 操作 |
| --- | --- |
| 没有完整 manifest；版本/tag/说明前置检查失败 | 修正准备步骤，再运行原发布命令 |
| 没有完整 manifest；Unity 或安装包构建失败 | 看对应日志，修复环境/代码后再构建；tag 中的代码需修复时，为正确提交准备新 tag |
| 已有完整 manifest；网络超时、上传中断、远端校验中断 | 保留本地产物，执行 `-Resume` |
| 已有完整 manifest；仅执行过 `-BuildOnly` | 推送对应 tag 后用 `-Resume` 发布 |
| 已正式发布；最终响应丢失 | `-Resume` 核验同一 Release，匹配时返回链接 |
| 已正式发布；要更改代码、版本或发布文件 | 使用新版本、新 tag |

恢复命令：

```powershell
.\BuildTools\Release\Publish-Release.ps1 -Tag v1.0.4 -Resume
```

恢复会校验本地 SHA256，跳过已经完整上传并匹配的文件。保持原 tag、版本、平台、说明和本地产物不变；不能修改 manifest 或更换文件后强行恢复。若网络仍失败，恢复连接后重跑同一命令。

**先等上一轮退出再恢复。** 如果请求长时间无响应，确认是本次发布进程后在它的终端中断；不要同时启动两个同 tag 的发布进程。仅存在 `publish.lock` 文件不表示进程仍运行，脚本使用文件句柄锁。

## 6. 之前遇到的错误及正确处理

### 6.1 `fatal: Needed a single revision` / `Local tag ... does not exist`

原因：指定的本地 tag 不存在，早期脚本解析不存在的引用时只显示 Git 128 错误；当前脚本已增加明确提示。

检查：

```powershell
git tag --list
git ls-remote origin refs/tags/v1.0.4
```

远端已经有正确 tag 时获取它：

```powershell
git fetch origin tag v1.0.4
```

本次是新版本且两边都没有 tag 时，先完成第 3 节的版本、说明、提交检查，再创建 tag。`-Tag` 只选择 tag，不会替你创建。

### 6.2 `The tag's bundleVersion does not match ...`

原因：tag 名是新版本，但它指向的提交仍保存旧 Unity Version。把当前工程改为新版本不会更新旧 tag。

检查 tag 中的真实设置：

```powershell
git show v1.0.4:ProjectSettings/ProjectSettings.asset |
    Select-String 'bundleVersion:|AndroidBundleVersionCode:'
```

应看到 `1.0.4 / 8`。不一致时先保存并提交正确配置，再准备指向正确提交的发布 tag。已正式发布的 tag 不移动；未发布 tag 的修复条件见第 7 节。

### 6.3 `Tag ... has no BuildTools/Release/Notes/<tag>.md`

原因：说明文件没有创建、没有提交，或是在创建 tag 后才提交。v1.0.3 曾因此在前置检查失败。

检查工作目录和 tag 提交：

```powershell
Test-Path -LiteralPath 'BuildTools/Release/Notes/v1.0.4.md'
git show v1.0.4:BuildTools/Release/Notes/v1.0.4.md
```

工作目录里存在文件不代表 tag 中有。推荐按第 3 节先创建、填写、提交说明，再创建新 tag。

如果现有 tag 的源码、版本和构建号都正确，只缺说明，可以显式使用本地 UTF-8 文件：

```powershell
.\BuildTools\Release\Publish-Release.ps1 -Tag v1.0.4 `
    -NotesFile .\BuildTools\Release\Notes\v1.0.4.md
```

此方式允许说明不在 tag 中，实际文字会保存到 manifest；它不会修正 tag 中的代码或版本。不要传入尚未编辑的模板，也不要用它覆盖已发布版本。有完整 manifest 时，恢复使用之前保存的说明。

### 6.4 Android 构建号没有递增

只修改 `bundleVersion` 不会修改 `AndroidBundleVersionCode`。v1.0.3 初次准备时沿用了 v1.0.2 的 `6`，修复后使用 `7`；本指南中的 v1.0.4 使用 `8`，之后新版本继续递增。

在 Player Settings 保存新构建号，并确认提交及 tag 中的值也更新。流水线冻结构建号，不会在打包时自动加一。

### 6.5 `Push tag ...` / `Local and remote tags differ`

原因：只推送了 main，没推送 tag，或同名 tag 在本地与远端不同。

```powershell
git rev-parse v1.0.4 'v1.0.4^{commit}'
git ls-remote origin refs/tags/v1.0.4 'refs/tags/v1.0.4^{}'
```

确认只是远端缺少正确本地 tag 后执行 `git push origin v1.0.4`。若引用不同，先判断哪一个是实际发布来源；不要直接普通 force push 覆盖它。

### 6.6 `A complete build already exists ...`

本地已有完整 manifest。重复原命令不会重新构建，应执行 `-Resume`。需要发布不同内容时使用新版本。

### 6.7 `No complete manifest to resume`

只有完整构建才能恢复。先修复前置检查或构建失败原因，再运行不带 `-Resume` 的命令。

### 6.8 `TLS handshake timeout` / `Connection was reset` / 请求无响应

v1.0.2、v1.0.3 真实发布均遇到过 GitHub 网络问题。按第 5 节根据 manifest 状态决定重跑或恢复。保留草稿和本地产物；网络恢复后 `-Resume` 会重新校验服务器状态。

### 6.9 登录、工具、平台或安全校验错误

| 提示 | 处理 |
| --- | --- |
| GitHub authentication unavailable | 运行 `gh auth login`，确认账号有仓库发布权限 |
| Unity Editor / ISCC not found | 安装对应版本，或显式传入工具路径，见第 8 节 |
| 缺少平台模块 / Unity report failed | 在 Unity Hub 安装对应模块，查看平台日志 |
| Tag predates this pipeline | 该提交没有批处理入口；为包含流水线的新提交创建新发布 tag |
| Another release process | 等上一进程结束后重试；不要删除文件来绕过仍持有的锁 |
| Local asset changed / remote SHA256 mismatch | 对照 manifest 与日志找出被修改或不完整的文件；校验通过前不能发布 |
| Draft identity/notes differ / unexpected asset | 检查草稿所属版本、提交、说明和文件；脚本拒绝操作不匹配的 Release |

## 7. 已经误打 tag 的处理原则

- **已经发布**：保留 tag 和原版资产；用下一版本包含修复。不要把已发布 `v1.0.3` 改成后来的代码。
- **没有正式 Release、也没有草稿**：先核验 GitHub 的实际状态，提交正确版本、构建号和说明；需要沿用该未发布版本名时，记录并备份旧 tag 对象，再修正引用。远端更新使用 `--force-with-lease=refs/tags/<tag>:<已核验旧tag对象SHA>`，限定此前核验过的引用；更新后再比较本地/远端 tag。此前 v1.0.2、v1.0.3 的准备错误按此方式修复。
- **已经有草稿或完整 manifest**：不要直接移动 tag。先判断是否可以用原 manifest 恢复；源码需要更改时用新的版本 tag。

核验 GitHub 是否有正式版或草稿时，使用有权限账号登录后的 Releases 页面，或脚本模块的已认证 `Get-ReleaseOnGitHub`；该函数会分页查找并包含可见草稿。未登录页面、仅按 tag 查询的 API 返回 404，都不能单独证明没有草稿。

具体历史 tag、提交、核验结果见 [发布验收记录](11-VerificationAndDebug/01-AutomatedRegression/LOCAL_RELEASE_PIPELINE.md)。

## 8. 可选参数与先构建后发布

只在本地构建并校验，暂不发布：

```powershell
.\BuildTools\Release\Publish-Release.ps1 -Tag v1.0.4 -BuildOnly
```

此时仍要求本地 tag 的版本、说明、构建入口正确，但不要求 GitHub 登录或远端 tag。准备好远端 tag 后，用 `-Resume` 发布。

只构建 Windows：

```powershell
.\BuildTools\Release\Publish-Release.ps1 -Tag v1.0.4 -Platforms Windows
# 中断后的恢复也必须传相同的平台
.\BuildTools\Release\Publish-Release.ps1 -Tag v1.0.4 -Platforms Windows -Resume
```

显式指定工具：

```powershell
.\BuildTools\Release\Publish-Release.ps1 -Tag v1.0.4 `
    -UnityPath 'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe' `
    -InnoSetupPath 'C:\Users\27487\AppData\Local\Programs\Inno Setup 7\ISCC.exe' `
    -GhPath 'C:\Program Files\GitHub CLI\gh.exe'
```

`-UnityPath` 必须指向 Unity Editor 的 `Unity.exe`，不能使用同名的 `unity` 自动化 CLI。其他电脑上根据实际安装目录修改。

脚本默认每个平台超时 60 分钟，可传 `-BuildTimeoutMinutes`；`-KeepWorktree` 保留成功构建的临时源码目录。Android 签名继承项目与本机配置，自定义 keystore 文件和密码需在本机配置好。

如果执行策略阻止 `.ps1`，仅为本次进程放行：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\BuildTools\Release\Publish-Release.ps1 -Tag v1.0.4
```

## 9. 每次发布前的简要检查

- [ ] 要发布的游戏改动已验证并提交。
- [ ] Unity Version 与去掉 `v` 后的 tag 一致。
- [ ] Android 构建号高于上一已发布版本，且已保存、提交。
- [ ] `Notes/<tag>.md` 已创建、填写真实说明、UTF-8 保存并提交。
- [ ] `git show HEAD:<说明路径>` 可读到本次说明。
- [ ] 在上述提交之后创建新 tag，且本地与远端引用相同。
- [ ] 再执行发布命令；失败时按 manifest 状态重跑或 `-Resume`。
- [ ] 最终出现 `Published and verified`，GitHub 页面说明与文件完整。

## 10. 本指南的验证依据

- 静态确认：按当前 `Publish-Release.ps1`、`ReleaseTools.psm1`、批处理入口与使用文档核对顺序、参数和恢复条件。
- 已实现：发布流水线已支持固定 tag、独立 worktree、Windows / Android 构建、SHA256、GitHub 草稿上传、完整核验后发布与恢复。
- 实际通过：v1.0.2、v1.0.3 的真实 Unity / Inno 构建及 GitHub 发布；v1.0.3 修复后的生产脚本回归在 PowerShell 5.1、7.6.5 各 42/42 通过。详情见 [验收记录](11-VerificationAndDebug/01-AutomatedRegression/LOCAL_RELEASE_PIPELINE.md)。
- 后续实际通过：v1.0.4 已按此流程完成真实构建和 GitHub 发布，记录见 [验收记录](11-VerificationAndDebug/01-AutomatedRegression/LOCAL_RELEASE_PIPELINE.md)。下一次新版本仍需单独构建、发布和验收。
- 已知限制：当前流水线只支持 Windows 本地环境、正式语义版本 tag、Windows / Android；安装/卸载与真机功能验收另行执行。
