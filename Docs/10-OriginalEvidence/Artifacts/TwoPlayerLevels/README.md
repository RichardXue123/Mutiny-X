# 原版双人关卡 XML（16–33）

取得日期：2026-09-25。原版 `NitromeGame.getLevelName` 使用 `MD5("yoho" + 关卡编号) + ".xml"` 命名，`TileSystem.loadLevel` 从 `levels/` 目录加载。本目录 `OriginalXml/` 保存按该规则从 Nitrome 原站取得的 18 份 XML 原始字节；[catalog.csv](catalog.csv) 逐关记录原站 URL、原文件名、SHA-256、`players`、地图尺寸和对象数。

Unity 运行资源位于 `Assets/Mutiny/Resources/Data/Levels/level_19.xml` 至 `level_33.xml`，同编号编辑源位于 `Assets/Mutiny/Data/Levels/`。这两套文件均与 `OriginalXml/` 相应原件逐字节一致；原有 16–18 的运行资源 SHA-256 也与新取得的原件一致，故编号映射得到核验。`players` 是原版 XML 字段，不代表菜单选择的单人/双人会话模式。

本次只确认文件来源、编号、字节一致性以及 Unity 编号加载；地图画面、角色位置和完整对战流程仍需逐关运行对照。
