# 战斗 HUD：小地图与团队总血量

范围：原版战斗画面左上角小地图、左下角玩家队总血量、右下角敌队总血量与敌方头像。逻辑视口为 550×400。

## 原版静态证据

- **S / `Map.as` `Map.show/reset/drawTiles/drawActive/dot`**：小地图挂在 `root.mapHolder`，使用 linkage `map`；每格为 3×3 px。地形从 `x/y = -2` 扫到关卡宽高 `+2`，有 tile 时绘制 50% 黑色，无 tile 时绘制 20% 黑色。宝箱为黄色、offset=-1、最高 50% alpha；一队角色为 `0xFF3829`，二队为 `0x335FFF`，offset=-2。角色和宝箱坐标按 `(pixel * 3 >> 5)` 投影。`Controller.enterFrame` 每个原版 tick 调用 `map.drawActive()`。
- **A / `mutiny.swf.xml` 根时间轴**：`mapHolder` 位于 `(20,20)`；`team1` symbol 2004 位于 `(71.95,380)`；`team2` symbol 2011 位于 `(477.85,380)`。两队 HUD 均为 97 帧。
- **S / `Team.as` `advance`**：每 tick 汇总队内所有角色的 `health` 和 `maxHealth`，目标帧为 `1 + floor(96 * totalHealth / totalMaxHealth)`；通过 `Global.slide(currentFrame,targetFrame,1)` 每 tick 只移动一帧。
- **A / symbols 2004、2011、2009**：玩家面板由底图 bitmap 2000 与独立头像 bitmap 1999 合成；敌队面板由 bitmap 2005 与 `opponent_image` 合成。`TileSystem.readXML` 在载入关卡后调用 `team2.opponent_image.gotoAndStop(selected_level)`，头像 symbol 2009 共 33 帧。
- **A / linkage `map` symbol 1895**：边框是 1 px 白线及约 20% 黑色阴影，由四角和四边组成，宽高随关卡尺寸变化。

## 行为规则

| ID | 前置与事件 | 可观察结果 | Unity 入口 | 验收状态 |
|---|---|---|---|---|
| HUD-01 | 关卡完成解析；每帧/每 tick 角色或宝箱位置可能变化 | 左上角按 3/32 比例绘制地形；红蓝角色点和黄色宝箱点更新；死去角色不再保留常亮点 | `MutinyGameHUD.DrawOriginalMap`, `ProjectMapCoordinate` | 已实现并添加自动断言；Unity 验证未执行 |
| HUD-02 | 两队与角色已建立；生命值发生变化 | 左下红条和右下蓝条使用全队 `Health / MaxHealth`；目标为 1..97 帧，25 Hz 下每 tick 向目标移动一帧 | `ResolveTeamHealthTargetFrame`, `AdvanceTeamHealthAnimationTick`, `DrawOriginalTeamHealth` | 已实现并添加自动断言；Unity 验证未执行 |
| HUD-03 | 进入某一关 | 两侧使用原版面板；右侧显示与关卡编号相同帧的敌方头像；位置保持原版 550×400 舞台坐标 | `InitStyles`, `DrawOriginalTeamHealth` | 资源已接入并添加检查；Unity 验证未执行 |
| HUD-04 | 任意屏幕宽高 | 先等比缩放 550×400，再居中显示；HUD 不随世界镜头移动 | `DrawOriginalBattleHud` | 代码已实现；Play Mode 宽高比对照未执行 |

## 状态与边界

- 队伍为空或总最大生命为 0 时，血量目标帧为 1，避免除零。
- 满血目标帧为 97，半血目标帧为 49，零血目标帧为 1。
- 两队血条方向按原时间轴：红条由左向右增长，蓝条由右向左增长。
- 原版 `Map.as` 会让死亡角色的地图点按 `mapVisibility` 渐隐。当前 Unity 角色没有独立 `mapVisibility` 状态，因此 HUD 在死亡时立即移除该点；这是已知差异，待角色死亡可见性时序进入 P5 后补齐。
- 当前空投系统按用户授权临时禁用；HUD 已保留宝箱地图点入口，重新启用空投后会从生产宝箱对象读取位置。

## 自动验收

执行 `Mutiny/Parity/Validate Battle HUD`，断言直接调用生产入口并检查：

1. 两张原版面板尺寸均为 132×34，玩家头像为 22×21，敌方头像资源为 33 帧。
2. 两名角色 100/100 与 50/100 时，团队目标帧为 73。
3. 当前帧向上或向下都只移动 1 帧。
4. 64 px 坐标和 offset=-2 投影为小地图坐标 4。

## 待运行对照

- 在 Level 1 满血开局录制前 97 个 25 Hz tick，确认两条血条从第 1 帧逐帧填满的实际原版表现。
- 对同一伤害事件记录原版与 Unity 的首次变化 tick、目标帧和到达 tick。
- 截取 550×400 的 Level 1 与至少一个非 Level 1 关卡，对比小地图边框、地形点、角色点、两侧面板和敌方头像。
