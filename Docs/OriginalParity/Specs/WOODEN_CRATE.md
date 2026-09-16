# WPN-14 Wooden Crate（木箱）行为规格

## 基线与范围

- 模块 ID：WPN-14。
- 原版：`Mutiny Source/mutiny-flash-game/mutiny.swf`，SHA256 `c034d2cfa50272a1aac93ad75114b542ca5993dd7b70fa8901aa195de7820586`（`REPLICATION_STANDARD.md` 基线）。
- Unity 工作区：实现前状态含既有未提交改动；本规格只覆盖本次 WPN-14 修改。
- 包含：玩家选中、三次放置、合法性、箱体实体、爆炸销毁、木箱时间轴资源、行动占用。
- 不包含：AI 的 `aiSimulation/aiPerform`、关卡初始箱、宝箱、运行时原版逐帧画面和音频对照；它们不因本次实现而视为通过。
- 证据状态：静态确认；实现状态：实现中；验证状态：未执行。

## 证据与冲突

| 证据 ID | 类型 | 文件、函数/符号 | 结论与适用条件 |
| --- | --- | --- | --- |
| W14-S-01 | S | `BoxWeapon.as` constructor | 箱体 left/top extent=16、right/bottom extent=15，`createMore=2`、`limitedToTurn=false`、`draggable=false`、`hitsBoxes=true`、`showCircle=false`。 |
| W14-S-02 | S | `BoxWeapon.as advance/place` | 人类每次合法鼠标按下放一个箱；根箱创建后续箱，合计三箱；每次放置都禁用 owner 的 throw/shoot；最后一箱放置后才完成整组武器。 |
| W14-S-03 | S | `BoxWeapon.as canPlace` | 放置坐标保持鼠标像素坐标，不做网格吸附；候选 32×32 范围不得碰 solid tile、已有箱、宝箱或活角色；角色检查使用其脚底/头顶与第一个下方障碍之间的区间。 |
| W14-S-04 | S | `BoxWeapon.as advanceMotion/explode`; `Solid.as advanceMotion` | 已放置箱受重力、terrain/其他箱碰撞，且作为 `Controller.boxes` 对所有 `hitsBoxes=true` 的 Solid 形成阻挡；爆炸时从 boxes 移除并播放 `explode` 时间轴标签。 |
| W14-S-05 | S | `Explosion.as hit` | 爆炸以箱体 AABB 上离爆心最近点判定；距离不超过爆炸 radius 时调用该箱 `explode()`。 |
| W14-A-01 | A | `DefineSprite_965_woodenCrate`, raster frames 1–18 | 原版木箱共有 18 帧导出图；`explode` 标签所指帧区间、帧脚本、音效尚待时间轴核对。 |
| W14-S-06 | S | `WeaponSelectButton.as hover_woodenCrate` | 菜单说明为“Click anywhere on the stage to place down three crates.”，确认三次点击的玩家语义。 |

冲突：无。`BoxWeapon` 未在静态源码中调用放置、碰撞、爆炸或入水音效；在运行对照前不得臆造该音效。木箱接触水面后的长期行为未由 `BoxWeapon` 覆盖明确，列为待验证。

## 状态、输入和转换

| 规则 ID | 前置状态 | 输入/允许条件 | 状态变化/触发 tick | 可观察结果 | 来源 |
| --- | --- | --- | --- | --- | --- |
| WPN-14-INT-01 | 活着的人类角色已选木箱，`CanShoot` 和库存有效 | 选中武器 | 建立未放置根箱，库存不扣 | 角色可进入放置状态；右键取消仍不扣库存（EXT 复用） | W14-S-02；EXT-P2-01 |
| WPN-14-INT-02 | 尚有未放置箱 | 左键；`CanPlace(px,py)` 为真 | 在原始鼠标 px/py 放一个箱，首次成功时库存扣 1；owner `CanShoot/CanThrow=false`；生成下一个待放置箱 | 已放置箱显现并受物理；非法点击不改变数量或库存 | W14-S-02/03 |
| WPN-14-INT-03 | 前两箱已放置 | 第二、第三次合法左键 | 第三箱设置完成，父箱逐模拟 tick 传播完成状态 | 三箱属于同一件武器/一次行动；最后放置后按既有静止规则结算 | W14-S-02 |
| WPN-14-EFF-01 | 任何已放置箱 | 每个 25Hz 物理 tick | 重力 `weight=1`、反弹 `0.2`、摩擦默认 `0.3`；与 terrain/已放置箱碰撞 | 木箱能掉落、堆叠、形成阻挡墙 | W14-S-01/04；`Solid.as` |
| WPN-14-EFF-02 | 爆炸存在且箱还注册 | 最近 AABB 点到爆心距离 `<= radius` | 从箱登记表移除，进入 `explode` 动画 | 被爆炸范围触及的箱销毁；未触及的保持 | W14-S-04/05 |
| WPN-14-ANI-01 | 木箱创建或被爆炸 | 待放置/放置/爆炸 | 静态使用原版 sprite 965 帧 1；破坏使用后续原版帧序列（具体标签边界待验证） | 使用原版导出资源，非通用抛射物图 | W14-A-01 |

拒绝与边界：已放置前可按既有右键扩展取消；首次放置后原版 `owner.canShoot/canThrow` 已清除，不能切换其他行动。零库存、死亡、重启由现有行动层限制，需场景验证。木箱不使用常规拖拽/抛射物输入；不应套用公共武器的水面超时销毁。

## 表现资源与实现映射

| 规则 ID | 原版资源 | Unity 资源和生产入口 | 当前差异 |
| --- | --- | --- | --- |
| WPN-14-ANI-01 | `DefineSprite_965_woodenCrate`, 1–18 | `Assets/Mutiny/Resources/Art/Weapons/WoodenCrate/1..18.png`; `MutinyWoodenCrate` | `explode` 帧标签边界与节奏待运行/时间轴验证。 |
| WPN-14-INT-01..03 | `BoxWeapon.advance/place/canPlace` | `MutinyPlayerInput`, `MutinyWoodenCrate` | AI 尚未接入。 |
| WPN-14-EFF-01..02 | `Solid.advanceMotion`, `Explosion.hit` | `MutinyPhysicsBody`, `MutinyWoodenCrate`, `MutinyExplosion` | 非木箱武器的 hitsBoxes 覆盖待逐武器实现。 |

## 用例与实际结果

| 用例 ID | 规则 ID | 前置条件及操作 | 独立期望 | 验收层 | 实际结果 |
| --- | --- | --- | --- | --- | --- |
| W14-T-01 | INT-01/02 | 经 `SelectWeapon` 和生产点击入口，在平地连续三次合法点击 | 初次成功才扣 1 库存；按原点击坐标生成三箱；owner 两项行动资格均清除 | 逻辑/输入 | 未执行 |
| W14-T-02 | INT-02/03 | 在 terrain、既有箱、角色范围内点击后再合法点击 | 非法点击不放置、不扣库存；第三次合法点击后整组完成 | 逻辑 | 未执行 |
| W14-T-03 | EFF-01 | 在空中和另一已放置箱上方生成箱 | 采用 16/15 extent、1/0.2/0.3 参数落地并堆叠 | 逻辑/场景 | 未执行 |
| W14-T-04 | EFF-02 | 在木箱边缘内/外各制造一个固定半径爆炸 | 仅 AABB 最近点位于 radius 内的箱进入销毁动画 | 逻辑 | 未执行 |
| W14-T-05 | ANI-01 | Play Mode：选中、三次放置、爆炸 | 原版 frame 1 静态图和爆炸帧序列可见；无凭空音效 | 场景/原版对照 | 未执行 |

- 比较字段：像素坐标、已放置数量、库存、`CanShoot/CanThrow`、每箱 extent/速度、注册表、完成 tick。
- tick 对齐：Unity 固定 25Hz 生产模拟入口；原版按其 `advance` 调用逐 tick 对比。
- 关联回归：行动菜单、TurnManager 静止结算、爆炸、Solid 箱体碰撞。
- 已知差异：AI、多武器 hitsBoxes、时间轴 explode 标签、入水、音效和真实 UI/画面对照待完成。

## 完成记录

- 已静态确认：W14-S-01..06。
- 已实现：`MutinyWoodenCrate`、`MutinyPlayerInput`、`MutinyPhysics`/`MutinyPhysicsBody`、`MutinyExplosion` 和生产验证用例已接入。
- 已执行并通过：`dotnet build Assembly-CSharp.csproj --no-restore`，0 error；它只证明 C# 编译。
- 失败或待验证：W14-T-01..05 的 Unity/原版运行层未执行。尝试 Unity batch-mode 验证时，Unity 在进入验证方法前以 return code 1 退出；日志未给出具体原因，见 `Temp/wooden-crate-validation.log`。
- 受影响文档：`WEAPON_REPLICATION_PLAN.md`、`TODO.md`。


