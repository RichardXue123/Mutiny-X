# 02.01 · 关卡数据解析

[返回上级模块](../README.md)

## 职责

把原版 XML 转换为完整的关卡数据模型。

## 边界

保留未知原始字段，不推测玩法语义。
## 可选扩展字段

Unity用户授权关卡可在 `<level>` 声明 `gravityScale`（有限正数，默认1）；本轮单人16配置为0.5。解析保留原地图结构，规则与验证见 [太空16初稿](../04-ObjectsAndSpawns/LEVEL_1_16_SPACE_DRAFT.md)。原版XML与证据未改写。

`visualTheme="space"` 为可选静态美术扩展，当前仅单人16启用；默认空字符串，未知主题仍使用原显示。只更换瓦片贴图、远景和海面的显示，不更改逻辑 tile 名、碰撞或对象坐标。见 [EXT-SPACE-ART 规格](../../09-PresentationAndFeedback/Space16Art/README.md)。

`<speechAudio line="1" clips="daftpunk_01,daftpunk_02,daftpunk_03,daftpunk_04" gapSeconds="0.5" />` 是用户授权的可选对白音频扩展。line按原对白0..3索引，同一关不得重复；clips为非空资源ID列表；gapSeconds为有限非负数，省略默认0.5。配置会序列化到LevelRoot，保留Scene预览保存/重开。未配置的对白沿用原队伍声音，配置只覆盖音频、不改变文字与胜负。见[固定Speech序列规格](../../09-PresentationAndFeedback/07-Audio/ROBOT_VOICE_SEQUENCE.md)。
