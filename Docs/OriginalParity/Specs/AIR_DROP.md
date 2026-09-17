# 空投生成、拾取、镜头与 AI 规格

## 范围

本规格覆盖原版 `TreasureChest` 的回合生成、关卡配置、90 帧显示、三个声音事件、角色拾取、镜头锁定和 AI 移动评分。2026-09-16 的临时停用已结束；历史记录见 `AIR_DROP_DEBUG_DISABLE.md`。

## 原版证据

| ID | 类型 | 来源 | 静态结论 |
| --- | --- | --- | --- |
| AIR-E01 | S | `Controller.as:181-194` | 换队后、下一队 `startTurn()` 前调用一次 `TreasureChest.dropNew()`；关卡首次 `startTurn()` 前不调用。 |
| AIR-E02 | S | `TreasureChest.as:37-115` | 场上最多 3 个；每次只随机一个有效列。该列无地面或被角色、同列空投、32 px 内 box 占用时，本次直接失败，不重抽。原版没有额外百分比掷骰。 |
| AIR-E03 | S | `TileSystem.as:173-174, 235-279` | `potentialWeapons` 中除 `x/y/type/luck/maxChests` 外的属性按数值展开为有放回权重池；有效列从上向下扫描，先遇到 `antichest` 就排除，先遇到实体 tile 就接受。 |
| AIR-E04 | S | `TreasureChest.as:16-35` | 每箱均匀随机 1–3 件，再从该关卡权重池逐件有放回抽取；出现时播放 `chest_appear`。 |
| AIR-E05 | S | `TreasureChest.as:126-142` | 从 `y=-300` 开始，每个 25 Hz tick 下落 3 px，在 `floorY-15` 停止并播放 `touchdown`。 |
| AIR-E06 | S | `frame-labels.csv`、DefineSprite 1725 的 frame 1/34/43/80/90 actions | `static=1`、`falling=10`、`touchdown=20`、`open=35`、`weapon_out=44`、`fade_out=81`；34 回 static，43/80 stop，90 destroy。 |
| AIR-E07 | S | `TreasureChest.as:143-224` | 存活且不是 `Controller.dragging` 的角色按箱体 AABB 拾取；开箱播放 `click`，10 tick 后给第一件，之后每 40 tick 一件，每件播放 `icon_collect`。 |
| AIR-E08 | S | `TileSystem.as:423-452` | 主动拖拽时镜头逻辑先返回；AI 尚未完成决策时也先返回。其余情况下，下落空投在 `timeTaken<100` 时以 50 px/tick 镜头速度跟随，并阻止同帧边缘滚屏。 |
| AIR-E09 | S | `Character.as` 的移动候选评分 | AI 的模拟落点与未结束空投的 `(x,floorY)` 严格小于 40 px 时加 `0.5`。 |
| AIR-E10 | A | `Art/raster/sprites/DefineSprite_1725_treasureChest/1..90.png` | 90 张根时间轴合成帧均为 62×82；Flash 注册点为 `(29,54.05)`，Unity pivot 为 `(29/62,27.95/82)`。 |
| AIR-E11 | A | `Resources/Audio/SFX/chest_appear.wav`、`click.wav`、`icon_collect.wav` | 三个原版调用名均有运行时音频资源。 |

`luck` 是角色 AI 采样参数，`maxChests` 在空投权重解析时被明确跳过。原版关卡加载代码把空投上限固定为 3。因此 Unity 不把这两个 XML 属性解释为刷新概率或每关上限。各关差异来自 `antichest`/地形布局和各自的加权武器属性；每箱 1–3 件与场上最多 3 箱是全关统一规则。

## 可观察行为与 Unity 入口

| ID | 前置/入口 | 可观察结果 | Unity 入口 |
| --- | --- | --- | --- |
| AIR-01 | 一次行动结算完毕并切到另一队 | 先尝试一次空投，再开始新队回合；初始回合不生成 | `MutinyTurnManager.TryAdvanceTurn` → `MutinyTreasureChestManager.TryDropNew` |
| AIR-02 | 管理器读取关卡 | 从该关 XML 构建加权池和有效列；`antichest` 列排除 | `Initialize`、`BuildPotentialWeaponList`、`BuildValidDropColumns` |
| AIR-03 | 场上不足 3 箱且配置有效 | 只抽一个列；被占用则记录拒绝原因并结束本次尝试 | `TryDropNew`、`IsDropPositionClear` |
| AIR-04 | 生成成功 | `y=-300`、播放 `chest_appear`；内容为加权有放回抽取的 1–3 件 | `TryDropNew`、`MutinyTreasureChest.Initialize` |
| AIR-05 | 箱子下落 | 每 tick `y+=3`；10–19 帧循环表现降落伞活动 | `AdvanceOriginalTick`、`AdvanceVisualTimeline` |
| AIR-06 | 到达地面 | 注册点停在 `floorY-15`；播放 20–34，随后回到静态帧 1，降落伞消失 | 同上 |
| AIR-07 | 角色与落地箱相交 | 主动 Throw Self 鼠标拖拽时不拾取；跳跃后飞行、爆炸击飞或碰撞滚动仍可拾取；播放 `click` | `TryTouchCharacter`、`MutinyPlayerInput.IsCharacterThrowDragInProgress` |
| AIR-08 | 开箱后的领取计时到点 | 10 tick 发第一件，此后每 40 tick 发一件；每次库存 +1、播放 `icon_collect`、44–80 播放实际武器及阵营色框弹出动画 | `AdvanceOriginalTick`、`UpdateReleasedWeaponVisual` |
| AIR-09 | 内容取完 | 播放 81–90 和透明度衰减，frame 90 销毁并从管理器注销 | `AdvanceVisualTimeline`、`OnDestroy` |
| AIR-10 | 下落或正在领取且未结束 | 持续重置 inactivity，不能在展示期间提前换回合 | `AdvanceOriginalTick` → `NotifyAmbientActivity` |
| AIR-11 | 存在 `falling && timeTaken<100` 的箱子 | 玩家未拖拽且当前不处于 AI 选行动阶段时，镜头每 tick 最多移动 50 px 到箱子，且该帧不执行边缘滚屏；100 tick 后恢复常规镜头资格 | `MutinyCameraController.FindFallingChest/LateUpdate` |
| AIR-12 | AI 评估 Throw Self 候选 | 落点距未结束空投严格 `<40 px` 时评分 `+0.5`，因此即使移动后暂时打不到人也可能为了空投移动 | `MutinyAIController.ScoreSelfThrow/ScoreChestMoveLanding` |

## 时间轴与声音契约

| 阶段 | 根帧 | 结束行为 | 声音 |
| --- | --- | --- | --- |
| 生成/下落 | 10–19 循环 | 到地面切 frame 20 | `chest_appear` 仅生成一次 |
| 落地 | 20–34 | frame 34 action 回 frame 1 | 无新增声音 |
| 静态 | 1 | stop | 无 |
| 开箱 | 35–43 | frame 43 stop | 初次合资格相交播放 `click` |
| 单件弹出 | 44–80 | frame 80 stop；下一件重播 frame 44 | 每件实际入库时播放 `icon_collect` |
| 消失 | 81–90 | frame 90 destroy | 无 |

运行时使用 62×82 根帧和原注册点，因此 `PixelY=floorY-15` 代表原版注册位置，不再用贴图中心对齐地面。弹出层使用抽中的 `UI/WeaponIcons/<weapon>`，并按拾取角色的 Team 1 红色或 Team 2 蓝色显示阵营框。

## 回归用例

| ID | 操作与断言 | 当前结果 |
| --- | --- | --- |
| AIR-T01 | 通过管理器生产入口载入 2 列测试关卡；一列 `antichest`，权重 banana×2/dynamite×1；断言只剩列 0、池长度 3、生成 1 箱 | 已写入 `MutinyTurnActionUiVerificationTest`，待 Unity 执行 |
| AIR-T02 | 新箱推进 1 tick；断言仍下落、frame 11、`timeTaken=1` | 已写入，待 Unity 执行 |
| AIR-T03 | 推进到地面；断言 `y=floorY-15`、frame 20 | 已写入，待 Unity 执行 |
| AIR-T04 | 给重叠角色非零被动速度；断言仍打开箱子并进入 frame 35 | 已写入，待 Unity 执行 |
| AIR-T05 | 开箱推进 10 tick；断言库存取得 banana、内容为空、frame 44 | 已写入，待 Unity 执行 |
| AIR-T06 | 再推进 3 tick；断言 frame 47 且实际武器弹出 renderer 可见 | 已写入，待 Unity 执行 |
| AIR-T07 | 从首次发放累计 40 tick；断言进入 finished/frame 81 | 已写入，待 Unity 执行 |
| AIR-T08 | AI 落点分别距箱子 39.99/40 px；断言加分分别为 0.5/0 | 已写入，待 Unity 执行 |
| AIR-T09 | Level 1 完成至少 8 次换队，记录成功/拒绝日志；成功箱应随关卡位置落地且不会穿地 | 待 Play Mode 人工验证 |
| AIR-T10 | 分别主动拖拽穿过箱子、松手飞过箱子、爆炸击飞穿过箱子；仅第一种不得领取 | 待 Play Mode 人工验证 |
| AIR-T11 | 观察 1 箱含 3 件时的三次弹出、阵营框、三个声音调用和最终淡出 | 待 Play Mode 人工验证 |
| AIR-T12 | 玩家回合生成空投；前 100 tick 尝试边缘滚屏，应由空投镜头接管，之后恢复 | 待 Play Mode 人工验证 |
| AIR-T13 | AI 与空投并存，固定种子记录 50 个移动候选和最终选择，确认 `<40 px` 候选得到 +0.5 | 待 Play Mode/原版逐 tick 对照 |

## 实现与验证状态

- **静态确认**：AIR-E01～E11；生成没有独立概率、XML 差异、全部数值、帧标签、声音调用、镜头和 AI 加分均由原版 AS2/时间轴/资源确认。
- **已实现**：AIR-01～AIR-12；临时总开关已恢复，核心生成/拒绝/落地/开箱/逐件领取均有 `[Mutiny][Chest]` 日志。
- **实际测试通过**：`dotnet build Assembly-CSharp.csproj`，0 error（2026-09-17）。
- **待运行验证**：AIR-T01～T13 尚未在本轮 Unity Play Mode 执行；用户负责 Unity 手动验证。
- **已知差异**：根时间轴 90 帧来自原版导出；逐件弹出时的武器图标改用同源 UI 武器图标覆盖导出帧中无法动态绑定的嵌套占位图。需用原版运行画面核对遮罩边界和阵营框的逐像素效果。
