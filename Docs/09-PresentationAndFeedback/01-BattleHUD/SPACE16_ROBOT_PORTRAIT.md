# 单人第十六关敌方头像

用户授权扩展；没有 Flash 原版第十六个单人关卡。

| ID | 可观察行为 / 状态转换 | 依据 | Unity 入口 | 验收用例 | 结果 |
| --- | --- | --- | --- | --- | --- |
| FIX-SPACE-HUD-PORTRAIT-01 | 进入单人 16 后，敌方蓝色队伍血条旁显示银色 Robot；保留素材比例、透明度和像素风 | 用户指定 robot；既有 `Art/Characters/Preview/Robot` | `MutinyGameHUD.ResolveOpponentPortrait`、`DrawOriginalTeamHealth` | 正式 GM `enterlevel 16` 检查绘制入口使用 Robot 纹理 | 已静态确认缺陷；实施前规格，待验证 |
| FIX-SPACE-HUD-PORTRAIT-02 | 从单人 16 切至单人 1/6/15 或双人 1/16，恢复原版对应头像；重新进入单人 16 再显示 Robot | 保留既有关卡头像，模式编号不得混淆 | 同上 | 正式 GM 切换关卡并检查实际 HUD 的头像解析 | 待验证 |

缺陷原因：HUD 仅用 `OriginalLevelIndex` 查询原版头像数组，单人 16 与原版第 16 关（双人 1）发生头像映射重叠。扩展头像独立引用现有 Robot 素材，不加入原版数字头像数组。

## 2026-10-01 结果

- 静态确认：上述旧编号重叠；Robot 是既有银色头盔素材，32×36、透明背景。
- 已实现：先解析模式和关卡编号，单人 16 取 Robot；绘制采用等比例适配，原版头像仍使用原版资源及位置。
- 实际测试通过：Unity 6000.6.0f1 隔离 Play Mode 17/17。实际 GM 依次进入 `16 → 1 → 6 → 15 → 2_1 → 2_16 → 16`，生产关卡的 HUD 解析结果分别正确；Robot 保留 Point 过滤和原始尺寸。脚本见 `Tools/RobotPortraitVerification.cs`，[记录](../../../11-VerificationAndDebug/02-PlayModeValidation/Artifacts/ROBOT-HUD-PORTRAIT-20261001.txt)。
- 待运行验证：主工程 PIE 中 IMGUI 头像的最终目视效果、Android 真机。
- 已知差异：单人 16 的 Robot 头像是授权扩展；测试为无图形 Play Mode，头像映射通过不等于实际画面验收通过。
