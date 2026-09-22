# Nitrome Mutiny 完整武器注册表与规则规范 (Weapon Registry)

本规范基于原版 SWF 反编译源码（`com.nitrome.throwgame.*`）与全 18 关 XML 深度扫描建立。

---

## 1. 武器全览与清单（15 种武器）

通过遍历全部 18 关 XML 中的 `<obj>` 角色自带库存与 `<obj type="potentialWeapons">` 宝箱可掉落列表，确认全游戏**仅有且恰好有以下 15 种可用武器/战术道具**：

| 武器标识 (ID) | AS2 类名 | 输入形式 | 碰撞盒半宽/半高 | 弹力 / 摩擦 | 爆炸尺寸 / 半径 / 伤害 | 关卡出现次数 (角色/宝箱) | 核心机制简述 |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `cherryBomb` | `CherryBomb` | 蓄力抛掷 | 9×9 px | 0.2 / 0.3 | 80 / 60px / 40 | 16 关 / 8 关 | 触碰任意瓦片/水体立即引爆，向上弹跳击退 |
| `dynamite` | `Dynamite` | 蓄力抛掷 | 11×11 px | 0.2 / 1.7 | 250 / 145px / 70 | 13 关 / 13 关 | **触碰不爆**，滚动停稳后巨爆；落水熄灭不爆 |
| `banana` | `Banana` | 蓄力抛掷 | 7×7 px | **0.8** / 0.5 | 160 / 100px / 80 | 9 关 / 12 关 | 高弹跳大香蕉，多次弹跳或碰触角色引爆 |
| `boulder` | `Boulder` | 蓄力抛掷 | 31×31 px | 0.2 / 0.25 | 无爆炸 (质量冲撞) | 2 关 / 4 关 | 重量 1.5 巨石碾压，不爆炸，碰撞推飞角色并扣血 |
| `cannon` | `Cannon` / `Cannonball` | 水平发射 | 9×9 px | 0.0 / 0.0 | 100 / 70px / 50 | 6 关 / 7 关 | 固定炮台直线高速平射穿甲炮弹 |
| `gunpowderBarrel` | `GunpowderBarrel` | 放置箱体 | 16×16 px | 0.2 / 0.3 | 150 / 95px / 30 | 12 关 / 6 关 | 烈性炸药桶，受任意外部爆炸触碰诱爆连锁 |
| `mine` | `Mine` | 蓄力抛掷 | 14×14 px | 0.2 / 1.5 | 250 / 145px / 70 | 9 关 / 6 关 | 地雷；落稳后进入感应，接近敌人或超时引爆 |
| `parachuteBomb` | `ParachuteBomb` | 蓄力高抛 | 11×11 px | 0.2 / 0.3 | 160 / 100px / 50 | 9 关 / 12 关 | 达到抛物线顶点后撑伞缓慢下落，触地引爆 |
| `piecesOfEight` | `PiecesOfEight` | 连掷抛掷 | 7×7 px | 0.2 / 0.3 | 50 / 45px / 25 × 3发 | 7 关 / 8 关 | 金币连击；单回合内连续 3 次瞄准发射小爆破 |
| `rumBottle` | `RumBottle` | 蓄力抛掷 | 14×14 px | 0.2 / 0.3 | 80 / 60px / 25 + 流火 | 9 关 / 7 关 | 朗姆酒瓶，砸碎产生流火火焰碎片二次烧伤 |
| `seagull` | `Seagull` | 标线空袭 | 高空召唤 | 无重力 | 50 / 45px / 50 空投 | 3 关 / 11 关 | 设定飞行高度，海鸥飞过目标上空定点投弹 |
| `tidalWave` | `TidalWave` | 水面召唤 | 水平推进 | 贴水面 | 强力推水冲量 | 5 关 / 9 关 | 在海平面掀起巨浪横扫，将近水敌兵推入海中溺死 |
| `voodooDoll` | `VoodooDoll` | 远程诅咒 | 选敌拉拽 | 替身同调 | 伤害与位移同调 | 3 关 / 9 关 | 选中敌兵建立契约，抛掷娃娃使敌兵承受相同受创 |
| `woodenCrate` | `BoxWeapon` | 连续放置 | 16×16 px | 0.2 / 0.3 | 无 (实体方块掩体) | 14 关 / 6 关 | 连续放置 3 个木箱阻挡弹道、垫脚、造掩体 |
| `anchor` | `Anchor` | 天顶定点 | 48×96 px | 极重下坠 | 沿途碾压粉碎 | 7 关 / 11 关 | 召唤重型铁锚从天际垂直坠击砸烂目标 |

---

## 2. 详细武器机制与参数规范

### 1. 樱桃炸弹 (`CherryBomb`)
* **类继承**：`Weapon` $\to$ `Solid`
* **碰撞规格**：`leftExtent = rightExtent = topExtent = bottomExtent = 9`（总尺寸 18×18 px = 0.5625 units）。
* **发射物理**：
  * `twangable = true`, `twangMaxForce = 20.0`
  * $\vec{v} = (\text{mouse} - \text{start}) \times -0.25$
* **接触逻辑 (`contact`)**：
  * 接触地面、墙面或水体立即生效；
  * 引爆 `Explosion(x, y, 80, 40, owner)`；
  * `size = 80, radius = 60px, maxDamage = 40`。

### 2. 炸药 (`Dynamite`)
* **碰撞规格**：`leftExtent = rightExtent = topExtent = bottomExtent = 11`（总尺寸 22×22 px）。
* **物理特征**：
  * 地面摩擦 `friction = 1.7`（滚动阻力大）；
  * 飞行旋转 `rotation += velocityX * 2`；
* **引爆时机**：
  * **触碰反弹不爆炸**；
  * 仅当速度接近静止（$v_x = 0 \land |v_y| < 0.2$）时引爆：
  * 引爆 `Explosion(x, y, 250, 70, owner)`（`radius = 145px, maxDamage = 70`）；
* **落水判定**：
  * 当 $y > \text{water.y}$ 时，引信熄灭（`mc.gotoAndStop("unlit")`），落水沉底完全失效，不发生爆炸。

### 3. 香蕉 (`Banana`)
* **碰撞规格**：`extents = 7`，`bounce = 0.8`（超高弹性），`friction = 0.5`，`twangMaxForce = 30`。
* **运动与反弹**：
  * 在墙体与地面间疯狂弹射；每次碰撞播放 `banana_bounce` 音效；
  * 反弹 4 次后或触碰到任何存活角色时引爆：
  * 引爆 `Explosion(x, y, 160, 80, owner)`（`radius = 100px, maxDamage = 80`）。

### 4. 巨石 (`Boulder`)
* **碰撞规格**：`extents = 31`（直径 62 px = 近 2 个瓦片），`weight = 1.5`, `friction = 0.25`。
* **物理冲撞机制**：
  * 纯动量冲撞物，内部无 `Explosion`；
  * 顺坡高速滚落，碰撞角色时按质量转移冲量并将角色强力击退，造成基于接触速度的直接重创。

### 5. 加农炮与炮弹 (`Cannon` / `Cannonball`)
* **发射机制**：
  * 角色就地架设加农炮台（`weight = 0, placeableWeapon = true`）；
  * 玩家调整角度后发射高速炮弹 `Cannonball`（初速高达 35 px/tick，几乎呈水平直线）；
  * 炮弹穿透障碍物或命中目标引爆 `Explosion(x, y, 100, 50, owner)`。

### 6. 火药桶 (`GunpowderBarrel`)
* **作为场景道具或放置物**：
  * 具备实体木箱物理碰撞（16×16 px）；
  * 受任何外部爆炸波及（在 `Explosion.hit` 中调用 `box.explode()`）触发连环爆炸；
  * 引爆 `Explosion(x, y, 150, 30, null)`，造成连锁扩散。

### 7. 地雷 (`Mine`)
* **规格**：`extents = 14, friction = 1.5, limitedToTurn = false`（跨回合持续存在）。
* **触发机制**：
  * 投掷着地停稳后进入布设状态（`active = true`）；
  * 当任何角色进入感应半径（60 px）或倒计时（60 ticks）耗尽时触发尖锐蜂鸣并引爆；
  * 引爆 `Explosion(x, y, 250, 70, owner)`（巨爆 70 伤害）。

### 8. 降落伞炸弹 (`ParachuteBomb`)
* **规格**：`extents = 11, twangMaxForce = 30`。
* **开伞动力学**：
  * 向上抛射时为常规自由抛体运动；
  * 一旦垂直速度由负转正（跨过抛物线最高顶点向下落）时展开降落伞：
    * 垂直限速 $v_y \leftarrow \min(v_y, 2.0)$（缓速空降）；
    * 水平受微风微幅漂移；
  * 触地引爆 `Explosion(x, y, 160, 50, owner)`（50 伤害）。

### 9. 八瓣金币 (`PiecesOfEight`)
* **规格**：`extents = 7`。
* **3连射机制**：
  * 单回合内 `timesFired` 记录发射次数（最多 3 次）；
  * 每次触碰引爆轻量级金币爆炸 `Explosion(x, y, 50, 25, owner)`（25 伤害）；
  * 爆炸结束后，当前回合并不终结，角色立即重新获得下一次金币抛掷机会，直至 3 枚耗尽。

### 10. 朗姆酒瓶 (`RumBottle`)
* **规格**：`extents = 14, twangMaxForce = 30`。
* **碎裂流火**：
  * 触碰瓦片/箱体碎裂，引爆 `Explosion(x, y, 80, 25, owner)`；
  * 同时在碰撞点向四周溅射 5~8 片带有重力和弹跳的燃烧火焰碎片（`SweepingFlame`）；
  * 火焰碎片触地持续燃烧 2 秒，经过的角色每秒受到多段灼烧扣血。

### 11. 海鸥空袭 (`Seagull`)
* **标线空袭机制**：
  * `placeableWeapon = true`；
  * 玩家在画面中上下拖动一条水平虚线标定飞行高度；
  * 确认后，一只海鸥从屏幕边缘水平飞掠而过，飞至正上方时投下炸弹；
  * 炸弹触碰目标引爆 `Explosion(x, y, 50, 50, owner)`。

### 12. 巨浪海啸 (`TidalWave`)
* **水体召唤机制**：
  * 贴近水面发动，海平面涌起一道高耸海浪，从屏幕一侧向另一侧席卷推进；
  * 处于低洼处或水面浮桥上的海盗被巨浪卷入并赋予极大水平冲量，直接冲下海峡溺亡。

### 13. 巫毒娃娃 (`VoodooDoll`)
* **契约与受创转移**：
  * 玩家首先点击敌方一名角色建立诅咒同调目标；
  * 随后将巫毒娃娃作为抛体投掷出去；
  * 巫毒娃娃在空中飞行、撞击瓦片或坠入深水时，所有位移冲量和所受物理伤害**按 100% 比例隔空同步作用于被诅咒的敌方目标**！

### 14. 木箱掩体 (`BoxWeapon` / `WoodenCrate`)
* **规格**：32×32 px（`extents = 16`）。
* **战术工事机制**：
  * 连续放置 3 个（`createMore = 2`）；
  * 玩家可将木箱放置在自身周围 1~2 格无障碍物处，立即生成实体碰撞地块；
  * 可阻挡火箭炮弹、构筑防爆掩体或搭建梯级登高。

### 15. 巨锚坠击 (`Anchor`)
* **高空天罚机制**：
  * 48×96 px（宽 1.5 格，高 3 格）的超重型钢铁巨锚；
  * 玩家在水平轴指定下落横坐标 $X$；
  * 巨锚从天空顶端垂直以高速 $v_y = 25\text{ px/tick}$ 呼啸坠落，砸碎下方的所有箱体并直接压扁压死沿途海盗。

---

## 3. 武器架构与实现规范

在 Unity 6000 中，所有武器统一继承自 `MutinyWeapon.cs` 抽象基类：
* **投掷型武器（Twangable）**：继承 `MutinyWeapon`，通过 `MutinyPhysicsBody` 驱动，复用 25 Hz 积分与反弹公式；
* **放置/召唤型武器（Placeable）**：实现 `IPlaceableWeapon` 接口，在玩家输入阶段展示对齐指示器，放置后启动其专属运动与时序逻辑；
* **爆炸结算**：所有产生爆炸的武器统一调用 `MutinyExplosion.Spawn(posPx, size, maxDamage, owner)`，确保击退、伤害衰减与向上弹射手感 100% 统一。

