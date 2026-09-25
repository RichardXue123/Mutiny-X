# 10 · 原版证据

## 职责

- 保存原版 AS2、pcode、SWF XML、符号表和时间轴证据。
- 保存导出的图片、声音、注册点、关卡扫描和映射表。
- 为其他模块提供可追溯的原版来源。

## 证据目录

- `Artifacts/ReverseEngineering/`：反编译、资源导出和映射结果。
- `Artifacts/LevelScan/`：18 关 XML 扫描结果。
- [Artifacts/TwoPlayerLevels/](Artifacts/TwoPlayerLevels/README.md)：原站 16–33 关 XML、编号与 SHA-256 清单。
- `Artifacts/OriginalParityTables/`：旧流程留下且仍有取证价值的结构化表格。

## 维护规则

原始导出物不根据 Unity 当前实现修改。重新导出时记录工具、输入文件和基线变化。

## 子模块

| 目录 | 内容 |
| --- | --- |
| [01-AS2AndPCode](01-AS2AndPCode/README.md) | AS2 与 pcode |
| [02-SwfTimelines](02-SwfTimelines/README.md) | SWF 时间轴 |
| [03-ArtAndAnchors](03-ArtAndAnchors/README.md) | 美术与锚点 |
| [04-AudioEvidence](04-AudioEvidence/README.md) | 音频证据 |
| [05-LevelScan](05-LevelScan/README.md) | 关卡扫描 |
| [06-EvidenceIndexing](06-EvidenceIndexing/README.md) | 证据索引 |

## 后续文档

统一证据索引、资源来源表和运行取证记录。
