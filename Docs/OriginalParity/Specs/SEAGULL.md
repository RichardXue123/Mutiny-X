# WPN-11 — Seagull（海鸥）

基线：原版 `mutiny.swf`，SHA256 `c034d2cfa50272a1aac93ad75114b542ca5993dd7b70fa8901aa195de7820586`。坐标单位为 Flash px，逻辑 tick 为 25 Hz。

## 入口与证据

| 原版入口 | 证据 | Unity 入口 |
| --- | --- | --- |
| `Seagull.as` 构造、`place`、`advance`、`endShot`、`destroy` | S | `MutinySeagull` |
| `Seagull.as` 内联 `Weapon("seagullFire")` 的 `contact`、`advance` | S | `MutinySeagullFire` |
| `Seagull.aiSimulation`、`aiPerform` | S | `MutinyAIController.EvaluateSeagull`、`ExecuteMove` |
| `Character.as` seagull AI 分支 | S | `MutinyAIController.EvaluateCharacterWeapons` |
| `DefineSprite_982_seagull` frame 9 `gotoAndPlay("flying")` | S+A | `MutinySeagull.AdvanceAnimation` |
| `WeaponSelectButton.as hover_seagull` | S | `MutinyGameHUD` 既有文案、`MutinyPlayerInput` |

## 行为规则

### WPN-11-INT-01 — 先选路径，再重复投放

- **前置状态**：角色已在行动菜单选中 seagull，尚未发射。
- **输入与条件（S，`Seagull.as advance/place`）**：第一次鼠标按下调用 `place(-300, content._ymouse)`；设置 `fired/track`，禁用角色 `canShoot/canThrow`，隐藏路径虚线。之后每次鼠标按下，在海鸥尚未结束且 `x < levelWidth << 5` 时创建一个子弹；同一按下只处理一次。
- **Unity 入口**：`MutinyPlayerInput.TryActivateClickWeapon`、`MutinySeagull.PlaceAtFlightHeight`、`TryRequestPlayerShot`。
- **验收**：第一次点击仅开始航线；第二和后续点击各新增一个 `seagullFire`；飞到关卡右边界后点击被拒绝。
- **实际结果**：生产路径和固定场景回归已加入，尚未在 Unity Editor 运行。

### WPN-11-EFF-01 — 海鸥飞行与结束

- **原版规则（S，`Seagull.as`）**：鸟的 `weight=0`、`hitsTiles=false`、`draggable=false`、`showCircle=false`；首次放置后 `velocityX=10`。仅在 `x > (levelWidth << 5) + 275` 且当前子弹数组为空时，才设 `finished=true`。每 tick `trackY = y + 100`。
- **Unity 入口**：`MutinySeagull.BeginFlight`、`AdvanceOriginalTick`。
- **验收**：初始位置 `(-300,pathY)`、速度 `(10,0)`；未结束子弹存在时，即使鸟已经出场也继续等待。
- **实际结果**：代码已实现，待 Unity 运行与原版镜头对照。

### WPN-11-EFF-02 — seagullFire 子弹

- **原版规则（S，`Seagull.as advance`）**：每发由 `Weapon("seagullFire")` 创建于 `(bird.x-10,bird.y)`，继承 `velocityX=10`，`weight=1`，`hitsBoxes=true`。任何 `contact(side)` 都先从鸟的 shots 中移除该子弹，再创建 `Explosion(x,y,50,50,owner)` 并销毁子弹。自定义 `advance` 在 `y > water.y` 时移除并销毁子弹，**不爆炸**。
- **Unity 入口**：`MutinySeagull.RequestShot`、`MutinySeagullFire.Initialize/ExplodeOnContact/EndInWater`。
- **验收**：检查生成坐标、速度、重量；地面或墙接触生成 50/50 爆炸；水面下销毁而没有新爆炸。
- **实际结果**：生产物理入口回归已加入，尚未在 Unity Editor 运行。

### WPN-11-AUD-01 — 每次投放随机 poop 声

- **原版规则（S，`Seagull.as`）**：子弹创建后立即调用 `playSound("poop" + (floor(random()*3)+1))`，即独立均匀选择 `poop1`、`poop2`、`poop3`。
- **Unity 入口**：`MutinySeagull.RequestShot`。
- **验收**：每次成功投放只记录一次 `poop1..3`；被拒绝点击、子弹碰撞和落水不额外播放。
- **实际结果**：调用和资源加载检查已接入，随机序列的 Unity/原版重放待执行。

### WPN-11-ANI-01 — 鸟和投放姿势

- **原版规则（S+A）**：`DefineSprite_982_seagull` 有 14 帧；frame 9 返回 `flying` 标签；子弹创建后执行 `mc.gotoAndPlay("shot")`。
- **Unity 入口**：`MutinySeagull.LoadSprites/AdvanceAnimation`。
- **验收**：14 帧全部能从 Resources 加载；飞行循环与每次投放后的 shot 段分别截图对照。
- **实际结果**：资源与 25 Hz 动画入口已接入，原版画面对照未执行。

### WPN-11-AI-01 — 原版 AI 路径采样

- **原版规则（S，`Seagull.aiSimulation`）**：取存活敌人最小 y，航线高度为 `minY - 100 - floor(random()*100)`；随机抽 10 个 `[0,levelWidth)` 横坐标并数值排序。每个候选模拟从 `(x-10,height)` 出发、`vx=10`、`weight=1` 的子弹。距离敌人小于 40 时加 `1-distance/40`；距离己方小于 40 时减 `1.5-distance/40`。仅保留分数大于 0 的坐标；只有保留数大于 1，才提交整个鸟的行动。
- **Unity 入口**：`MutinyAIController.EvaluateSeagull`、`SimulateSeagullShotImpact`、`ExecuteMove`、`MutinySeagull.PlaceForAi`。
- **验收**：固定随机序列下检查飞行高度、十个候选排序、保留的投放坐标和“少于两发不行动”边界。
- **实际结果**：生产实现已接入；固定随机重放未执行。

## 资源状态

已验证可加载：

- `Assets/Mutiny/Resources/Art/Weapons/Seagull/1.png` 至 `14.png`
- `Assets/Mutiny/Resources/Art/Weapons/SeagullFire/1.png`
- `Assets/Mutiny/Resources/Audio/SFX/poop1.wav` 至 `poop3.wav`

原版路径选择虚线资源为 `Docs/ReverseEngineering/Art/raster/sprites/DefineSprite_1898_dottedLine/1.png`。当前 Unity Resources 尚未包含该帧，因此路径预览的原版像素视觉是待补项；不影响路径选择和飞行玩法。

## 待验证与已知差异

- 未执行 Unity Play Mode 与原版逐 tick 对照，不能标为通过。
- 子弹的 `hitsBoxes=true` 已记录为原版规则；Unity 当前物理系统没有通用 box 碰撞入口，箱子接触需要在 P3-04/WPN-11 的后续场景验收中补齐。
- `trackY=y+100` 的镜头偏移尚未接入镜头控制器，须与实际飞行镜头一起对照。
