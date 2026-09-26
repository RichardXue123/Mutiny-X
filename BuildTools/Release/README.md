# 本地一键发布

固定 Git tag → 独立 Unity 工作目录 → Windows / Android → 本地 SHA256 → GitHub 草稿 → 上传并校验 → 正式 Release。

默认发布 Windows 便携 ZIP、Windows Setup EXE、Android APK、`SHA256SUMS.txt`。不构建 iOS，不自动递增版本或构建号，不创建或推送 tag。Unity 编辑器可以继续打开当前开发工程；脚本构建的是 tag 对应的独立目录。

## 首次配置

- Windows，PowerShell 5.1 或更高版本，Git。
- 与 `ProjectSettings/ProjectVersion.txt` 匹配的已激活 Unity Editor，以及 Windows / Android 模块。脚本默认查找 `C:\Program Files\Unity\Hub\Editor\<version>\Editor\Unity.exe`。
- Inno Setup 7（兼容 6），自动搜索常用安装目录。
- GitHub CLI（`gh`）：运行 `gh auth login`，登录有仓库发布权限的账号。已配置的 `GH_TOKEN` 也可使用；令牌不要提交到仓库。
- 若 gh 凭据无效而 Git Credential Manager 中已有可用 GitHub 凭据，脚本会仅在本次进程中复用，完成或失败后恢复原环境变量；不会写入仓库、manifest 或日志。
- `origin` 指向你的 github.com 仓库；可用 `-Remote <名称>` 选择其他远端。

如果当前 PowerShell 的策略阻止执行本地脚本，可以从工程目录运行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\BuildTools\Release\Publish-Release.ps1 -Tag v1.0.2
```

该参数只影响这次进程，不更改系统执行策略。

## 每次发布（以 v1.0.2 为例）

1. 合并并验证待发布内容。Unity Player Settings 中设 `Version = 1.0.2`、Android `Bundle Version Code = 6`（高于已发布的 5）。保存设置。通过本流水线构建时不要先点击旧构建菜单，它会递增构建号。
2. 将 `Notes/TEMPLATE.md` 复制为 `Notes/v1.0.2.md`，填写本次简要说明。版本配置、说明和流水线脚本都要包含在发布提交中。
3. 从工程仓库根目录提交，再打 tag 并推送：

```powershell
git add ProjectSettings/ProjectSettings.asset BuildTools/Release/Notes/v1.0.2.md
git commit -m "Prepare release v1.0.2"
git tag -a v1.0.2 -m "Release v1.0.2"
git push origin main
git push origin v1.0.2
```

首次采用时，先提交这次新增的流水线文件和文档。上面命令假设游戏改动已提交、当前分支为 main。脚本只接受 `v主.次.修订` 的正式版 tag。

`-Tag v1.0.2` 是选择已有 tag，不会自动创建它。若直接复制示例命令而本地尚无该 tag，先完成上面的版本提交、创建及推送步骤。已发布的 `v1.0.0` / `v1.0.1` 不包含本次新增的批处理入口，不能用本流水线重新打包；首次采用应使用包含流水线文件的新发布提交和新 tag。

4. 一条命令构建并发布：

```powershell
.\BuildTools\Release\Publish-Release.ps1 -Tag v1.0.2
```

可显式选择平台、说明或工具位置：

```powershell
.\BuildTools\Release\Publish-Release.ps1 -Tag v1.0.2 `
  -Platforms Windows,Android `
  -NotesFile .\BuildTools\Release\Notes\v1.0.2.md `
  -UnityPath 'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe'
```

说明默认读取 **tag 提交中** 的 `BuildTools/Release/Notes/<tag>.md`。`-NotesFile` 允许覆盖为一个本地 UTF-8 文件，其内容会随本次产物保存。

`-InnoSetupPath` 可指定 ISCC.exe，`INNO_SETUP_COMPILER` 和 `UNITY_EDITOR_PATH` 环境变量也可指定工具位置。

## 先验证，再发布 / 中断恢复

```powershell
# 只打包并校验；不要求远端 tag 或 GitHub 登录
.\BuildTools\Release\Publish-Release.ps1 -Tag v1.0.2 -BuildOnly

# 复用完整产物，校验 SHA256 后发布；也用于上传中断后的恢复
.\BuildTools\Release\Publish-Release.ps1 -Tag v1.0.2 -Resume
```

使用单平台构建后，恢复时需要传入相同的 `-Platforms`。有完整 manifest 时必须使用 `-Resume`，不会静默重新构建或覆盖。构建失败而未生成 manifest 时，修复环境后重跑原命令即可。

已正式发布的 tag 不允许覆盖。`-Resume` 遇到已发布且所有身份、说明、资产均匹配的 Release 时，只验证并返回链接，适用于“发布已成功但最终响应丢失”。本地/远端 tag 不同、资产被修改、草稿身份或说明不同、草稿包含额外文件时均停止。

## 输出与失败定位

- `Local tag ... does not exist`：运行 `git tag --list` 确认；远端已有 tag 时先执行 `git fetch origin tag <tag>`。新版本需要先提交版本/构建号、流水线和说明，再创建 tag。
- `Tag ... predates this pipeline`：该 tag 的源码没有新批处理入口；提交流水线后为下一版创建新 tag。
- `The tag's bundleVersion does not match`：tag 名称与其提交中的 Unity 版本不一致。修改当前工作目录不会改变已存在的 tag 指向；应在正确的版本提交上创建新 tag。

**不要先创建 tag 再修改 Unity 版本。** 如果已误将尚未发布的 tag 推送到旧提交，需要先提交正确版本、递增构建号、流水线和更新说明，再核验 GitHub 没有该版本的 Release 后修正 tag；远端更新使用 `--force-with-lease` 限定已核验的旧 tag 对象。已发布的 tag 不移动，应使用新版本号。

`Builds/Release/<tag>/`（Git 忽略）：

- `manifest.json`：固定源码提交、版本、构建号、平台、说明、资产路径/大小/SHA256。
- `release-notes.md`：本次说明。
- `runs/<运行编号>/assets/`：待上传的所有文件。
- `runs/<运行编号>/<平台>.log`、`<平台>-report.json`、`inno.log`：构建结果。
- `github-verified.json`：GitHub 校验结果。

任一构建失败即中止，不会创建草稿。上传/校验失败会保留草稿；下次 `-Resume` 跳过已校验的文件，对其余文件最多重试三次。校验使用 GitHub 返回的上传状态、大小和 SHA256 digest；缺少 digest 也视为失败。

Windows ZIP 与安装包使用相同的过滤后目录，排除 `*.pdb`、Unity 备份及 Burst 调试文件。Android 固定为 APK；签名沿用 tag 中的项目设置和本机 Unity 密钥配置。自定义 keystore 的本机文件/密码仍需自行配置，流水线不会把密码写入 manifest。

临时 worktree 创建在工程的上一级目录，名称为 `_release-<tag>-<随机编号>`，缩短 Windows / Gradle 路径；Git worktree 操作启用长路径支持。其路径记录在 `runs/<运行编号>/source-path.txt`。构建成功后自动移除本次创建的临时 worktree；失败时保留以便排查，`-KeepWorktree` 可以在成功后也保留。日志仅保存在本地，默认不上传。单个平台默认超时 60 分钟，可用 `-BuildTimeoutMinutes` 调整。

## 回归

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\BuildTools\Release\Tests\Test-ReleasePipeline.ps1
```

测试直接驱动生产脚本和发布函数，使用本地 Git 仓库及 Unity / Inno / GitHub CLI 替身，不访问或创建线上 Release。真实 Unity / Inno 构建结果另见 [验收记录](../../Docs/11-VerificationAndDebug/01-AutomatedRegression/LOCAL_RELEASE_PIPELINE.md)。

参考：[Unity 批处理参数](https://docs.unity3d.com/6000.0/Documentation/Manual/EditorCommandLineArguments.html)、[GitHub CLI 发布](https://cli.github.com/manual/gh_release_create)。
