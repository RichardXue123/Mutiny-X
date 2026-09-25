# Mutiny GM 命令说明书

状态：当前 Unity 调试扩展。它不属于 Flash 原版行为，不应作为原版一致性结论。

## 使用方式

进入游戏后，屏幕左侧中央会有圆形 `GM` 按钮。点击后输入命令，按 Enter 或 `Run` 执行。英文命令不区分大小写；带空格的命令会先去除首尾空格。

命令由 [MutinyGMManager.cs](../../Assets/Mutiny/Scripts/Presentation/MutinyGMManager.cs) 的 `ExecuteCommand` 解释。新增或修改命令时，必须同时更新本说明书、控制台 `Help` 文案与 `MutinyTurnActionUiVerificationTest` 中的 GM 回归用例。

## 命令总览

| ID | 推荐命令 | 可用别名 | 效果 | 持久化范围 |
| --- | --- | --- | --- | --- |
| GM-01 | `UnlockWeapons` | `UnlockAllWeapons`、`InfiniteWeapons`、`AllWeapons`、`Weapons`、`解锁武器`、`无限武器`，以及以 `UnlockWeapon` 或 `InfiniteWeapon` 开头的英文输入 | 给目标角色开启 15 种可选武器的无限弹药；大炮所需的 `cannonball` 内部别名也同时可用，并恢复该角色的武器行动资格。 | 仅当前运行时角色；切场景、重开或重启游戏后不保留。 |
| GM-02 | `UnlockWeaponsAllTeam` | 任何已被 GM-01 识别，且同时含 `all` 和 `team` 或 `player` 的英文输入，例如 `UnlockWeaponsAllPlayers` | 给场景中所有存活的 Team 1 角色开启 GM-01 的无限武器。 | 仅当前运行时角色。 |
| GM-03 | `UnlockAllLevels` | `UnlockAll`、`Unlock All` | 将最高已解锁关卡设为当前最大关卡 `18`。 | 写入 `MutinySaveSystem`；跨场景和重启保留，直到被重置。 |
| GM-04 | `ResetLevels` | `ResetProgress`、`LockAll` | 清除关卡解锁进度，恢复为只解锁第 1 关。 | 写入 `MutinySaveSystem`；跨场景和重启保留。 |
| GM-05 | `Help` | `?` | 在 GM 面板显示当前可用命令的简表。 | 无状态修改。 |
| GM-07 | `aiforceusewaepon {weaponid}` | 无；命令拼写按测试约定保留 `waepon` | `1..15`：所有 AI 队伍的角色在 AI 决策中仅将对应编号武器视为无限可用；跳跃仍参与竞争，跳跃后仍可放弃开火。`0`：关闭覆盖，恢复实际库存候选。 | 当前运行会话；不改角色真实库存与存档。 |

## GM-07 · 强制 AI 武器候选

原版来源：无，此项是用户授权的 Unity 调试扩展。Unity 入口为 `MutinyGMManager.ExecuteCommand` → `MutinyAIController` 的正式候选评估、动作选择和执行路径。

编号按武器菜单的 15 项顺序：`1 cherryBomb`、`2 boulder`、`3 dynamite`、`4 piecesOfEight`、`5 rumBottle`、`6 banana`、`7 parachuteBomb`、`8 woodenCrate`、`9 gunpowderBarrel`、`10 seagull`、`11 mine`、`12 cannon`、`13 anchor`、`14 voodooDoll`、`15 tidalWave`。`cannonball` 只是大炮内部别名，不另占编号。

输入示例：`aiforceusewaepon 1`。覆盖只改变 AI 的有效候选及 AI 发射时的扣弹行为，既不向 `WeaponInventory` 或 `InfiniteWeapons` 写值，也不给玩家控制的角色解锁武器。AI 仍通过原本的评分选择跳跃或武器；跳跃后的续行动阶段若最佳武器收益不大于零，仍会放弃开火。输入 `aiforceusewaepon 0` 后，后续决策重新使用真实库存，原有弹药数不变。编号缺失、超出 `0..15` 或非整数时显示错误且保持原设置。已开始的决策使用开始时的覆盖快照；已选动作执行时保持当次快照的无限弹药语义。

验收：通过 GM 正式解析入口设置 `1`；无樱桃炸弹库存的 AI 也产生且只产生樱桃炸弹武器候选，真实库存不变；首行动的跳跃候选仍存在，续行动没有合格武器候选时仍 Pass；执行不扣真实弹药；设 `0` 后真实库存候选恢复；非法输入不改变覆盖。非正收益 Pass 分支由既有 AI 续行动规则维持，GM-07 专项用例未单独构造“有候选但评分非正”的局面。

## `UnlockWeapons` 的目标选择

没有指定全队后缀时，GM-01 按以下顺序找到一个存活角色：

1. 当前回合已选角色。
2. Team 1 已选角色。
3. 当前队伍的第一个存活角色。
4. Team 1 的第一个存活角色。
5. 场景中标记为 selected 的角色。
6. 场景中 Team 1 的第一个存活角色。
7. 场景中任意第一个存活角色。

若目标属于当前回合队伍，命令会同步选中它，以便立即从行动菜单使用武器。没有存活角色时，控制台显示错误，不会修改存档。

## 武器集合

`UnlockWeapons` 调用 `MutinyCharacter.UnlockAllWeapons(infinite: true)`。其可选武器为：

`cherryBomb`、`boulder`、`dynamite`、`piecesOfEight`、`rumBottle`、`banana`、`parachuteBomb`、`woodenCrate`、`gunpowderBarrel`、`seagull`、`mine`、`cannon`、`anchor`、`voodooDoll`、`tidalWave`。

`cannonball` 同时被注册为大炮的内部库存别名，因此可直接使用大炮，不把它当作第 16 种独立的菜单武器。

## 维护规则

- 命令的唯一生产入口是 `MutinyGMManager.ExecuteCommand`；不要在 UI 按钮中另写一套分支。
- 新命令先分配 `GM-xx` ID，记录输入、目标、状态变化、存档影响和失败行为。
- 用户授权的调试命令与 Flash 原版规则分开记录。
- 修改已存在命令的别名或语义时，保留旧别名，除非有明确的兼容性移除需求。

## 验证入口

GM 的生产回归位于 [MutinyTurnActionUiVerificationTest.cs](../../Assets/Mutiny/Scripts/Verification/MutinyTurnActionUiVerificationTest.cs)。在 Unity Play Mode 下，菜单 `Mutiny → Parity → Validate GM Commands` 可单独执行 GM-07；`Validate Turn Action UI` 包含 GM-01 及 GM-07。2026-09-25 在当前工程 Unity Play Mode 实际执行 GM-07 专项回归，结果 `7/7 assertions passed`。完整 `Validate Turn Action UI` 未在本次执行；旧 GM-01 用例在已有战斗角色的场景中可能选中真实角色，不属于本次通过结果。
