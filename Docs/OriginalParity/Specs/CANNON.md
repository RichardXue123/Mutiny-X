# WPN-05 Cannon（大炮）行为规格

## 基线与范围

- 模块 ID：WPN-05。
- 原版：`Mutiny Source/mutiny-flash-game/mutiny.swf`，SHA256 `c034d2cfa50272a1aac93ad75114b542ca5993dd7b70fa8901aa195de7820586`。
- 证据状态：静态确认；实现状态：实现中；验证状态：C# 编译通过，Unity 测试/Play Mode 未执行。
- 本规格覆盖 Cannon 本体、放置拖拽、炮尾 pin、延迟开火、Cannonball、烟雾和回合/AI链路。现有 Unity 把 `cannon` 错映射为 `MutinyCannonball`，不能作为原版行为依据。

## 原版证据

| 证据 ID | 类型 | 文件、函数/符号 | 结论 |
| --- | --- | --- | --- |
| W05-S-01 | S | `Cannon.as` constructor/release | Cannon 为 `placeableWeapon`、weight=0、hitsBoxes=true、blendMode=layer；release 强制 vx/vy=0。 |
| W05-S-02 | S | `Cannon.as advance` 未发射分支 | 拖动本体时，以 owner `(x,y-100)` 为圆心限制在 120 px；点击本地 20 px 圆开始拖本体。pin 命中为相对 pin 的严格 `<8 px` 圆。 |
| W05-S-03 | S | `Cannon.as advance` pin 分支 | 拖 pin 时炮口角=round((atan2(mouse-cannon)+180°) mod 360)；pin x 限制 `[-40,-21]`。释放时只有 pin x `<-30` 才使 owner 两行动失效、fireStrength=30；否则 fireStrength=0。未拖 pin 时 pin 每 tick +15 回到 -21，抵达且 strength>4 才开火。 |
| W05-S-04 | S | `Cannon.as fire/advance/update` | fire 创建 Cannonball 于 cannon x/y，设置 owner 和速度，播放 `cannon explosion`；Cannon 跟随 ball；发射后 Cannon alpha 从 visibility=2 每 tick减 .1，低于 1 时 alpha=100*visibility；ball finished 且 visibility=0 时 cannon hidden/finished。 |
| W05-S-05 | S | `Cannonball.as` constructor/contact/advanceMotion/advance | ball weight=0、hitsBoxes=true；terrain/box contact 只要 fired/unfinished 即 hide、finished、创建 Explosion(x,y,100,50,owner)、速度归零并播放 `pop`。出 x<-300、x>levelWidth+300、y<-300 或 y>water.y 时仅 hide/finished。飞行每 tick生一条 cannonSmokeTrail。角色 AABB 与 ball extent 相交也引爆 100/50，但该分支没有 `pop`。 |
| W05-S-06 | S | `Cannon.as randomThrows/aiPerform` | AI 先将 cannon 放至 `details.startX/startY`，设置 angle，再等 25 tick 后以 details vx/vy 开火。 |
| W05-S-07 | S | `Character.as selectWeapon`; `WeaponSelectButton.as hover_cannon` | 角色选中时 Cannon 实例位于 `(char.x,char.y-10)`；说明要求“Drag the cannon into position within the circle; click and drag the pin at the back to turn the cannon; release it to fire!” |
| W05-A-01 | A | DefineSprite 850/853/866 | Cannon、Cannonball 均为单帧；cannonSmokeTrail 为 19 帧。Cannon/Cannonball art、`cannon_explosion.wav` 已有；烟雾帧需逐项确认 Unity 导入。 |
| W05-S-08 | S | `Clip.as show`; `Cannon.as fire` | Cannonball 在 Cannon 已显示后才 `new`/`show`，因此得到 characterLayer 的 next-highest depth，显示深度高于 Cannon。 |

## 状态与规则

| ID | 前置 / 输入 | 状态转换与可观察结果 |
| --- | --- | --- |
| WPN-05-INT-01 | 选中有库存 Cannon | 建立炮本体；先拖放在 owner 上方 100 px 圆半径 120 内，不能用通用抛掷发射。 |
| WPN-05-INT-02 | 本体已放置，点击 pin 后拖/释放 | angle 严格按原版整数角；pin `<-30` 才装填 30 力度并消耗行动；pin 回弹抵达 -21 后才开火。 |
| WPN-05-EFF-01 | Cannon 发射 | 创建 0 重力/box 碰撞 Cannonball，Cannon 跟随其位置；Cannon自身 20 tick淡出后，等待 ball结束才结束。 |
| WPN-05-EFF-02 | Cannonball terrain/box/角色/边界 | terrain/box 接触爆 100/50 + `pop`；角色 AABB 相交爆 100/50但不额外 `pop`；水/越界直接隐藏，不爆炸。 |
| WPN-05-ANI-01 | ball 飞行 | 每 25Hz 生成一条 19帧 smoke trail；Cannon/Cannonball显示/隐藏和 Cannon alpha依 W05-S-04。 |
| WPN-05-AI-01 | AI details | 原坐标 placement + 25 tick wait + fixed velocity fire。 |

## 预定用例

| 用例 | 操作 | 期望 |
| --- | --- | --- |
| W05-T-01 | 拖本体到半径 120 内外 | 精确限制位置，初始 y 偏移与命中半径一致。 |
| W05-T-02 | pin x=-30 与 -30.1 后释放 | 前者不射，后者 30 力、行动失效，pin回弹后开火。 |
| W05-T-03 | terrain、box、角色、水和四个边界 | 爆炸/音效/无爆炸分支和 100/50 参数精确。 |
| W05-T-04 | 固定 ball 生命周期 | 烟雾逐 tick、Cannon alpha、跟随和结束条件精确。 |
| W05-T-05 | 固定 AI details | 第 25 等待 tick 及发射 tick精确。 |

## 完成记录

- 已静态确认：W05-S-01..07、W05-A-01。
- 已实现：独立 `MutinyCannon`、工厂映射、鼠标拖放/pin、Cannonball、烟雾、音效和生命周期；pin 位置改为随炮身旋转的局部坐标。Cannonball 直达原版 30 force，不再走 Unity 通用 20 force clamp；隐藏后的 ball 保留到 Cannon 的 20 tick 淡出结束。Cannonball 的 Unity sorting order 固定为 Cannon +1，复原原版 later-created next-highest depth，消除两模型同层重叠闪烁。AI 已接入 `randomThrows` 的放置/30-force 模拟和 `aiPerform` 的 25 tick 延迟发射。旧的 `cannon`→`MutinyCannonball` 工厂误映射已移除。
- 已执行并通过：`dotnet build Assembly-CSharp.csproj --no-restore`（0 error）。
- 待验证：W05-T-01..05、Unity/原版运行对照。
