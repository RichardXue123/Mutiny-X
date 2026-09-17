# 美术资源锚点偏移与显示异常审计及修复方案

## 1. 概述与核心根因

在本项目中，游戏画面存在多处视觉对齐问题（角色在选中框中偏左、高身位角色下半身埋地、燃烧瓶火焰沉入地板、铁锚落地嵌入地面、武器预览位置异常、落水水花半截沉在水下等）。

### 核心根因：Flash SWF 注册点 (Registration Point) 与 Unity 默认中心锚点 (0.5, 0.5) 冲突
1. **Flash SWF 坐标系规范**：
   * 原版《Mutiny》（Flash 2008）中，每个 `MovieClip / DefineSprite` 拥有独一无二的**局部坐标系原点（即注册点 Registration Point `(0, 0)`）**。
   * 当 ActionScript 代码执行 `this.x = targetX; this.y = targetY;` 时，是将元件的**注册点**精确对准物理世界的 `(targetX, targetY)`。
   * 角色、武器、特效的碰撞盒底部（如角色的 `BottomExtent = 8`、Anchor 的 `BottomExtent = 0`、Water 的 `water.y`）均严格以注册点为参照基准。
2. **Unity 导入现状**：
   * SWF 矢量与帧图在提取为独立 PNG 切片时，裁剪了透明边界生成固定矩形。
   * Unity 的 `Sprite.Create`、`Resources.Load<Sprite>` 以及多数 `.meta` 导入配置中，均采用了默认的**中心锚点 `Pivot = (0.5, 0.5)`**。
   * **只要切片的中心点与其在 Flash 内的注册点不重合，就会发生不可避免的像素平移与视觉错位。**

---

## 2. 具体 Bug 成因深度剖析

### Bug 1: 角色在白色虚线选中框中偏左 (Selection Frame Horizontal Drift)
* **表现现象**：当玩家或鼠标悬停选中角色时，生成的白色虚线角标框（`SelectionCorners`）明显偏在角色右侧，角色主体偏在虚线框的左半侧。
* **数据与机理**：
  * 原版 `redPirate`（Symbol 1324）、`bluePirate`（Symbol 1077）等标准海盗的切片尺寸为 **28 × 30** 像素。
  * Flash 原版的注册点位于图像内部的 **`x = 12.0` 像素（距左边界 12px）**，`y = 15.0` 像素。
  * Unity 代码 `MutinyCharacterAnimator.LoadFrames` 使用了硬编码的 `new Vector2(0.5f, 0.5f)`。对于 28px 宽的图像，`0.5` 锚点对应的 X 像素为 **`14.0px`**。
  * **误差计算**：由于把 X 轴锚点设在 14px（比原本的 12px 偏右了 2px），渲染时角色图像被整体**向左拉移了 2 个像素**（`12 - 14 = -2px`）。
  * 虚线框 `SelectionCorners` 挂载在 `m_OverlayRoot`（世界坐标 `(0, 0)`），以 `[-14, +14]` 对称绘制，因此角色显得偏左。

---

### Bug 2: 较高角色下陷埋地 & 燃烧瓶火焰、铁锚落地过低

#### (1) 较高角色下陷埋地（第 2、4、7 关等）
* **表现现象**：第 2 关的 `squid`（乌贼）、第 4 关的 `soldier`（红衣士兵）/ `soldierCaptain`（军官）、第 7 关的 `bossGuy`（大胡子 Boss）等角色生成或站立在地面上时，脚部与部分躯干直接穿模沉入地下瓦片中。
* **数据与机理**：
  * 原版所有角色的物理碰撞盒是统一的（`LeftExtent = 6, RightExtent = 6, TopExtent = 8, BottomExtent = 8`），物理静止时脚底到原点的距离固定为 **8 像素**。
  * 原版美术在绘制高个子/异形角色时，将注册点统一对齐在角色的**脚踝/下躯干处**，上方多出的头部和高帽子自然向上延伸：
    * **`soldier`（红衣士兵，46px 高）**：原版注册点在 `y = 31.0`（距顶部 31px）。Unity 使用 `0.5`（23px），导致士兵贴图被**向下压低了 8 个整像素**（`31 - 23 = +8px`）！脚和膝盖全部插入地面。
    * **`squid`（乌贼，39px 高）**：原版注册点在 `y = 24.0`（距顶部 24px）。Unity 使用 `0.5`（19.5px），导致乌贼被**向下压低了 4.5 个像素**。
    * **`bossGuy`（Boss，62px 高）**：原版注册点在 `y = 47.0`（距顶部 47px）。Unity 使用 `0.5`（31px），导致 Boss **向下深陷了 16 个像素**，半截身子埋在地下。
  * **27 种角色 Flash 原版尺寸与注册点数据对照表**：

| 角色标识 (CharacterType) | SWF Symbol | 贴图尺寸 (W×H) | Flash 注册点 (ox, oy) | 正确 Unity Pivot (X, Y) | 默认 (0.5, 0.5) 导致的偏移误差 |
|---|---|---|---|---|---|
| `redPirate` | 1324 | 28×30 | (12.0, 15.0) | (0.4286, 0.5000) | X 偏左 2.0px，Y 无偏移 |
| `bluePirate` | 1077 | 28×30 | (12.0, 15.0) | (0.4286, 0.5000) | X 偏左 2.0px，Y 无偏移 |
| `cabinBoy` | 1097 | 28×30 | (12.0, 15.0) | (0.4286, 0.5000) | X 偏左 2.0px，Y 无偏移 |
| `skeletonPirate` | 1248 | 28×30 | (12.0, 15.0) | (0.4286, 0.5000) | X 偏左 2.0px，Y 无偏移 |
| `rainbowBeard` | 1117 | 27×30 | (12.0, 15.0) | (0.4444, 0.5000) | X 偏左 1.5px，Y 无偏移 |
| `femalePirate` | 1126 | 29×30 | (13.0, 15.0) | (0.4483, 0.5000) | X 偏左 1.5px，Y 无偏移 |
| `blindPirate` | 1137 | 29×30 | (13.0, 15.0) | (0.4483, 0.5000) | X 偏左 1.5px，Y 无偏移 |
| `soldier` | 1148 | 24×46 | (12.0, 31.0) | (0.5000, 0.3261) | **Y 下沉 8.0px**，X 无偏移 |
| `bossGuy` | 1157 | 40×62 | (21.0, 47.0) | (0.5250, 0.2419) | **Y 下沉 16.0px**，X 偏右 1.0px |
| `bossGuyZombie` | 1166 | 40×62 | (21.0, 47.0) | (0.5250, 0.2419) | **Y 下沉 16.0px**，X 偏右 1.0px |
| `soldierCaptain` | 1175 | 34×36 | (17.0, 21.0) | (0.5000, 0.4167) | **Y 下沉 3.0px**，X 无偏移 |
| `blindPirateCaptain` | 1186 | 28×34 | (14.0, 19.0) | (0.5000, 0.4412) | **Y 下沉 2.0px**，X 无偏移 |
| `femalePirateCaptain` | 1197 | 31×32 | (15.0, 17.0) | (0.4839, 0.4688) | **Y 下沉 1.0px**，X 偏左 0.5px |
| `rainbowBeardCaptain`| 1206 | 36×39 | (18.0, 24.0) | (0.5000, 0.3846) | **Y 下沉 4.5px**，X 无偏移 |
| `oldPirateCaptain` | 1217 | 36×34 | (17.0, 19.0) | (0.4722, 0.4412) | **Y 下沉 2.0px**，X 偏左 1.0px |
| `cabinBoyCaptain` | 1228 | 24×33 | (12.0, 18.0) | (0.5000, 0.4545) | **Y 下沉 1.5px**，X 无偏移 |
| `tribeChief` | 1237 | 28×39 | (14.0, 24.0) | (0.5000, 0.3846) | **Y 下沉 4.5px**，X 无偏移 |
| `skeletonPirateCaptain`| 1259 | 30×35 | (14.0, 20.0) | (0.4667, 0.4286) | **Y 下沉 2.5px**，X 偏左 1.0px |
| `squid` | 1299 | 24×39 | (12.0, 24.0) | (0.5000, 0.3846) | **Y 下沉 4.5px**，X 无偏移 |
| `bluePirateCaptain` | 1321 | 29×35 | (14.0, 20.0) | (0.4828, 0.4286) | **Y 下沉 2.5px**，X 偏左 0.5px |
| `redPirateCaptain` | 1331 | 29×35 | (14.0, 20.0) | (0.4828, 0.4286) | **Y 下沉 2.5px**，X 偏左 0.5px |
| `oldPirate` | 1106 | 36×30 | (18.0, 15.0) | (0.5000, 0.5000) | 无偏移 |
| `tribe` | 1086 | 24×30 | (12.0, 15.0) | (0.5000, 0.5000) | 无偏移 |
| `monkey` | 1270 | 29×30 | (14.0, 15.0) | (0.4828, 0.5000) | X 偏左 0.5px，Y 无偏移 |
| `crab` | 1281 | 40×30 | (20.0, 15.0) | (0.5000, 0.5000) | 无偏移 |
| `shark` | 1290 | 32×30 | (16.0, 15.0) | (0.5000, 0.5000) | 无偏移 |
| `parrot` | 1310 | 24×30 | (12.0, 15.0) | (0.5000, 0.5000) | 无偏移 |

#### (2) 燃烧瓶火焰特效（SweepingFlame）沉入地板
* **数据与机理**：
  * Symbol 905（`sweepingFlame`），贴图尺寸 **19 × 31** 像素。
  * 火焰是从地面向上燃烧的蔓延特效，原版注册点位于火焰的最底边 **`y = 30.0`**（正确 Unity Pivot Y = `(31 - 30) / 31 = 0.0323`，即底边对齐）。
  * Unity 的 `SweepingFlame/1.png.meta` 设为了 `{x: 0.5, y: 0.5}`。
  * 导致地面瓦片对准了火焰的中心高度，**整整 15.5 像素（一半高度）的火焰被画在地面之下**。

#### (3) 铁锚（Anchor）落地深深插入地面
* **数据与机理**：
  * Symbol 1003（`anchor`），贴图尺寸 **104 × 100** 像素。
  * 铁锚物理参数为 `BottomExtent = 0`，落地判定点 `State.Y` 就是地面高度；原版注册点在铁锚最尖端底部 **`(52.0, 98.0)`**（正确 Unity Pivot Y = `(100 - 98) / 100 = 0.0200`）。
  * Unity 的 `Anchor/1.png.meta` 设为了 `{x: 0.5, y: 0.5}`。
  * 导致铁锚落地时，铁锚正中心被对齐到地面，**下半部 48 个像素深深埋入地下**。

---

### Bug 3: 选中武器准备抛射时的预览偏移 (Equipped Weapon Preview Drift)
* **表现现象**：选中武器准备瞄准抛射时，部分武器不在角色身体中心或手部，而是漂浮在奇怪的位置。
* **数据与机理**：
  * 原版 Flash 中，角色装备武器时分为两种机制：
    1. 巨石（Boulder）等大型武器：挂载在头顶上方（`yOffset = -30px`）。
    2. 普通投掷武器（CherryBomb、RumBottle、Banana 等）：挂载在角色手部附近（`yOffset = -10px`）。
  * 但这些武器本身的 Sprite 切片在 Unity 中也被赋予了默认的 `(0.5, 0.5)` Pivot，而非原版各武器握把/几何原点；
  * 加上角色自身（见 Bug 1）向左偏移了 2px，两个错误的 Pivot 互相叠加，导致武器预览点出现不自然的左右或上下飘移。

---

### Bug 4: 落水水花动画位置一半在海面下 (Water Splash Submersion)
* **表现现象**：角色或物体落水时，触发的水花动画有一半淹没在海面波浪以下。
* **数据与机理**：
  * Symbol 1780（`splash`），贴图尺寸 **48 × 44** 像素。
  * 原版 Flash `Solid.splashCheck()` 在物体穿过水线时，直接以 `_loc3_.y = Controller.water.y` 生成水花；水花动画的注册点位于最底部 **`(24.0, 44.0)`**（向上喷射，正确 Unity Pivot Y = `0.0`）。
  * Unity 中 `MutinyLevelBuilder.BuildWater` 设定的 `waterUnityY` 对应海面高度；但若水花贴图的 `.meta` 或生成时 Sprite Pivot 仍为 `(0.5, 0.5)`（或者未与海浪波峰对齐），水花中心就会被放到水线处，导致 **22 像素（一半水花）淹没在水下**。

---

## 3. 完整系统化修复方案

### 方案 1：角色动画系统动态 Pivot 映射表 (`MutinyCharacterAnimator.cs`)
在 `MutinyCharacterAnimator` 中建立 27 种角色的精确 Pivot 查找字典（基于 `sprite-origins.csv` 计算得出的精准浮点值），在 `LoadFrames` 创建 Sprite 时传入正确 Pivot：

```csharp
private static readonly Dictionary<string, Vector2> s_CharacterPivots =
    new Dictionary<string, Vector2>(StringComparer.OrdinalIgnoreCase)
{
    { "redPirate", new Vector2(12f / 28f, 15f / 30f) },              // (0.4286, 0.5000)
    { "bluePirate", new Vector2(12f / 28f, 15f / 30f) },             // (0.4286, 0.5000)
    { "cabinBoy", new Vector2(12f / 28f, 15f / 30f) },               // (0.4286, 0.5000)
    { "skeletonPirate", new Vector2(12f / 28f, 15f / 30f) },         // (0.4286, 0.5000)
    { "rainbowBeard", new Vector2(12f / 27f, 15f / 30f) },           // (0.4444, 0.5000)
    { "femalePirate", new Vector2(13f / 29f, 15f / 30f) },           // (0.4483, 0.5000)
    { "blindPirate", new Vector2(13f / 29f, 15f / 30f) },            // (0.4483, 0.5000)
    { "soldier", new Vector2(12f / 24f, 15f / 46f) },                // (0.5000, 0.3261) -> 彻底修复 soldier 埋地
    { "bossGuy", new Vector2(21f / 40f, 15f / 62f) },                // (0.5250, 0.2419) -> 彻底修复 bossGuy 埋地
    { "bossGuyZombie", new Vector2(21f / 40f, 15f / 62f) },          // (0.5250, 0.2419)
    { "soldierCaptain", new Vector2(17f / 34f, 15f / 36f) },         // (0.5000, 0.4167)
    { "blindPirateCaptain", new Vector2(14f / 28f, 15f / 34f) },     // (0.5000, 0.4412)
    { "femalePirateCaptain", new Vector2(15f / 31f, 15f / 32f) },    // (0.4839, 0.4688)
    { "rainbowBeardCaptain", new Vector2(18f / 36f, 15f / 39f) },    // (0.5000, 0.3846)
    { "oldPirateCaptain", new Vector2(17f / 36f, 15f / 34f) },       // (0.4722, 0.4412)
    { "cabinBoyCaptain", new Vector2(12f / 24f, 15f / 33f) },        // (0.5000, 0.4545)
    { "tribeChief", new Vector2(14f / 28f, 15f / 39f) },             // (0.5000, 0.3846)
    { "skeletonPirateCaptain", new Vector2(14f / 30f, 15f / 35f) },  // (0.4667, 0.4286)
    { "squid", new Vector2(12f / 24f, 15f / 39f) },                  // (0.5000, 0.3846) -> 彻底修复 squid 埋地
    { "bluePirateCaptain", new Vector2(14f / 29f, 15f / 35f) },      // (0.4828, 0.4286)
    { "redPirateCaptain", new Vector2(14f / 29f, 15f / 35f) },       // (0.4828, 0.4286)
    { "oldPirate", new Vector2(18f / 36f, 15f / 30f) },              // (0.5000, 0.5000)
    { "tribe", new Vector2(12f / 24f, 15f / 30f) },                  // (0.5000, 0.5000)
    { "monkey", new Vector2(14f / 29f, 15f / 30f) },                 // (0.4828, 0.5000)
    { "crab", new Vector2(20f / 40f, 15f / 30f) },                   // (0.5000, 0.5000)
    { "shark", new Vector2(16f / 32f, 15f / 30f) },                  // (0.5000, 0.5000)
    { "parrot", new Vector2(12f / 24f, 15f / 30f) }                  // (0.5000, 0.5000)
};
```

### 方案 2：特效与武器切片 Pivot 全面修正
1. **SweepingFlame**：
   * 在 `MutinySweepingFlame.cs` / `.meta` 中，将 Sprite Pivot 设为 `new Vector2(9f / 19f, 1f / 31f)`（即 `(0.4737f, 0.0323f)`，底边居中对齐）。
2. **Anchor**：
   * 在 `MutinyAnchor.cs` / `.meta` 中，将 Sprite Pivot 设为 `new Vector2(52f / 104f, 2f / 100f)`（即 `(0.5000f, 0.0200f)`，锚尖底端对齐）。
3. **Splash**：
   * 在 `MutinySplashEffect.cs` / `MutinyWaterSurface.LoadFrameRange` 中，确保加载时使用 `new Vector2(24f / 48f, 0f / 44f)`（即 `(0.5f, 0f)` 底边居中），并确保水花生成世界坐标精准对齐海浪表面。

### 方案 3：武器装备点与预览对齐 (`MutinyWeapon.cs`)
* 武器自身的 Sprite 切片若通过 `Resources.Load<Sprite>` 加载，更新对应的 `.meta` 文件或在武器代码中指定原版挂载点，消除叠加在角色身上的二次偏移。

### 方案 4：测试与自动化回归校验 (`MutinyTurnActionUiVerificationTest.cs`)
在回归测试套件中加入 Pivot 与对齐断言：
1. 断言 `redPirate`、`soldier`、`squid`、`bossGuy` 角色动画帧的 Sprite Pivot 与原版 `sprite-origins.csv` 误差小于 0.001。
2. 断言 `SweepingFlame`、`Anchor`、`Splash` 的底部顶点在落地/落水时严格切齐地面/海面网格线。
3. 运行全关卡回归测试确保无穿模和回归破坏。
