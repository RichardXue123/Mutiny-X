# 1.0.1 发布验收（2026-09-26）

版本：`1.0.1`，构建号 `5`。发布源码从 main 建立，仅纳入 XingTong / Richard Xue Credits 修复和 GM-08 `aitakeover 1` 及对应回归。Unity 为 6000.6.0f1；生产构建队列目标为 Windows x64、Android，调用现有 `MutinyBuild.StartQueue / ContinueQueue`，不构建 iOS。

## 静态确认

- EXT-CRED-01：头像原 PNG 与 meta 移入 Resources，保留 GUID 和图片内容；Editor 与 Player 共用资源加载路径。头像姓名与中央移植署名保留。
- GM-08：正式命令入口临时交给现有 AI 接管当前人类回合，完整回合结束后恢复；行为规格见 [GM 命令说明](../GM_COMMANDS.md)。
- bundleVersion 为 1.0.1，Android versionCode 为 5。现有构建脚本同时维护平台共用计数器，不代表本次生成或发布 iOS 产物。

## 已实现

- Windows Inno Setup 安装包、便携 ZIP、Android ARM64 APK 和 SHA256SUMS.txt。
- Windows 发布封装使用过滤后的构建暂存目录，排除 DoNotShip 备份目录和 PDB。
- Credits 资源检查在每次构建前经生产初始化自动执行。

## 实际测试通过

| ID | 入口与环境 | 实际结果 |
| --- | --- | --- |
| EXT-CRED-01 | 独立发布工程，`MutinyCreditsVerificationMenu.Validate` | 原版五张资源 / Flow 和新增生产头像绑定检查通过 |
| EXT-CRED-01-VIS | Main 场景 Play Mode，Credits 按钮共用的生产过渡入口与 Game View 截图 | 头像、头像下方 Richard Xue、中央移植署名均可见；[截图记录](CREDITS_VERIFICATION.md#101-发布复验) |
| GM-08 | 独立工程 Play Mode，`MutinyTurnActionUiVerificationTest.RunGM()` | 24/24 断言通过；覆盖既有 12 条和新增接管 12 条 |
| REL-WIN-BUILD-01 | 生产 BuildPipeline，Windows x64 | Succeeded，98.394 秒 |
| REL-WIN-INSTALL-01 | 现有 MutinyX.iss + Inno Setup 7，过滤后的发布目录 | 编译器退出码 0；安装包生成 |
| REL-WIN-ARCHIVE-01 | 便携 ZIP 目录与完整 CRC 检查 | 209 文件；CRC 通过；DoNotShip / PDB 为 0 |
| REL-WIN-START-01 | 实际发布暂存 Player，默认 D3D12；另测 D3D11 | 两次均运行至生产标题页和回合初始化，未发现托管异常 |
| REL-AND-BUILD-01 | 生产 BuildPipeline，Android ARM64 | Succeeded，296.729 秒 |
| REL-AND-META-01 | SDK aapt dump badging | com.RichardXue.MutinyX；versionName=1.0.1；versionCode=5；arm64-v8a；minSdk=26；targetSdk=36 |
| REL-AND-SIGN-01 | SDK apksigner verify | APK v2 签名验证通过，证书为 Android Debug |

完整构建、签名和启动日志在本机独立发布工作区的忽略目录 `Builds/Release/` 与 `Logs/Editor.log`。本次构建自动生成的 263 个未覆盖 iOS 默认纹理设置块已逐文件比对后还原，临时性能测试资源 JSON 已清理。

## 待运行验证

- Windows 干净环境安装 / 卸载，长时间玩法、Credits 外链与实际鼠标悬停。
- Android 真机安装及实际操作：ADB 本次未检测到设备。
- Player 可见窗口中 Credits 的完整交互；本次实际画面验收来自 Editor Game View，包内资源加载已有独立 Player 回归。

## 已知差异

- 头像、移植署名和 GM-08 为用户授权扩展，不属于原版 Flash 一致性规则。
- Android 沿用工程的调试签名配置。
- 默认 D3D12 启动存在信息队列接口查询告警，随后实际进入标题页；第一次固定 10 秒启动检查未等待到标题页，后续以标题日志为条件重测通过。

发布目标：[GitHub v1.0.1](https://github.com/RichardXue123/Mutiny-X/releases/tag/v1.0.1)。远端附件及说明的最终校验记录存于 `Builds/Release/github-release-verified.json`。
