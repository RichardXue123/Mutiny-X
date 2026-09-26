# 11 · 验证与调试

## 职责

- 管理生产入口回归、Unity Play Mode 验收和原版对照。
- 记录关键模块日志、状态快照和已知差异。
- 维护 GM 命令及可重复的测试条件。

## 当前入口

- `Assets/Mutiny/Scripts/Verification/`
- `Assets/Mutiny/Scripts/Presentation/MutinyGMManager.cs`
- [GM 命令说明](GM_COMMANDS.md)
- [本地 tag 发布流水线验收](01-AutomatedRegression/LOCAL_RELEASE_PIPELINE.md)

## 完成口径

写出测试步骤不等于测试通过；必须记录实际执行环境、结果和失败证据。

## 子模块

| 目录 | 内容 |
| --- | --- |
| [01-AutomatedRegression](01-AutomatedRegression/README.md) | 自动回归 |
| [02-PlayModeValidation](02-PlayModeValidation/README.md) | Play Mode 验证 |
| [03-OriginalComparison](03-OriginalComparison/README.md) | 原版对照 |
| [04-Logging](04-Logging/README.md) | 日志 |
| [05-GMTools](05-GMTools/README.md) | GM 工具 |
| [06-KnownDifferences](06-KnownDifferences/README.md) | 已知差异 |

## 后续文档

模块测试矩阵、日志规范、人工验收清单和缺陷回归记录。
