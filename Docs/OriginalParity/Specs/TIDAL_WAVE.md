# WPN-12 — Tidal Wave（潮汐波）

基线：原版 `mutiny.swf`，SHA256 `c034d2cfa50272a1aac93ad75114b542ca5993dd7b70fa8901aa195de7820586`。坐标为 Flash px，逻辑 tick 为 25 Hz。

## 入口与证据

| 原版入口 | 证据 | Unity 入口 |
| --- | --- | --- |
| `TidalWave.as` 构造、`advance`、`startWave`、`aiPerform` | S | `MutinyTidalWave` |
| `Character.as` tidalWave AI 分支 | S | `MutinyAIController.ScoreTidalWave/ExecuteMove` |
| `DefineSprite_1058_tidalWave` frame 6/15/24 | S+A | `MutinyTidalWave.AdvanceAnimation` |
| `TileSystem.as` 天空色的关卡区间 | S | `MutinyTidalWave.ResolveOriginalSkyColour` |
| `WeaponSelectButton.as hover_tidalWave` | S | `MutinyPlayerInput.TryActivateClickWeapon` |

## 行为规则

### WPN-12-INT-01 — 一次点击启动固定方向波浪

- **前置状态**：角色已选中 tidalWave，未发射。
- **输入与状态变化（S，`TidalWave.as advance/startWave`）**：玩家第一次鼠标按下触发 `startWave()`；该方法不读取鼠标坐标。它设 `fired/track`，禁用 `canThrow/canShoot`。
- **原版结果**：位置总为 `(-550, Controller.water.y)`，速度总为 `(20,0)`，无反向分支。
- **Unity 入口**：`MutinyPlayerInput.TryActivateClickWeapon`、`MutinyTidalWave.StartWave`。
- **验收**：在任意屏幕坐标点击都产生右向 20 px/tick 波浪，且角色两项行动均耗尽。
- **实际结果**：生产入口回归已加入，Unity Play Mode 未执行。

### WPN-12-EFF-01 — 每 tick 的伤害窗

- **原版规则（S，`TidalWave.as advance`）**：每个已发射 tick，遍历所有队伍的所有存活角色；若 `character.y >= water.y - 300`，同时 `wave.x - 150 <= character.x <= wave.x + 150`，执行 `subtractHealth(5)`。原版 `hitPlayers=[]` 仅初始化、未用于去重，因此命中角色会在每个满足条件的 tick 持续受伤。
- **Unity 入口**：`MutinyTidalWave.ApplyOriginalDamageWindow`，由 `PhysicsBody.OnSimulationStep` 在移动后调用。
- **验收**：窗口内角色第一 tick 从 100 到 95；横坐标刚超过右边界或 y 小于 `water-300` 的角色不受伤；停在窗口内两个 tick 后生命为 90。
- **实际结果**：第一 tick 与横向边界的生产物理回归已加入；其他边界与原版对照未执行。

### WPN-12-EFF-02 — 物理和离场

- **原版规则（S，`TidalWave.as`）**：`placeableWeapon=true`、`hitsTiles=false`、`weight=0`、`showCircle=false`、`draggable=false`。当 `x > (levelWidth << 5) + 550` 时设 `finished=true` 并隐藏波浪。
- **Unity 入口**：`MutinyTidalWave.Initialize/StartWave/AdvanceOriginalTick`。
- **验收**：波浪不碰 terrain；仅严格大于右边界加 550 时结束并隐藏。
- **实际结果**：生产回归已加入，Unity Play Mode 未执行。

### WPN-12-ANI-01 — 三套天空色动画

- **原版规则（S+A，`TidalWave.as startWave`，`DefineSprite_1058_tidalWave`）**：开始时执行 `gotoAndPlay("anim" + skyColour)`。资源有 27 帧，三个 9 帧段分别对应天空色 1/2/3；每段帧 6、15、24 跳回各自 `anim` 标签，故每套可见循环为前 5 帧。
- **Unity 入口**：`MutinyTidalWave.LoadFrames/ResolveOriginalSkyColour/AdvanceAnimation`。
- **验收**：关卡 1–5、6–10、11–15 分别取资源 1–5、10–14、19–23；每个逻辑 tick 前进一帧并循环。
- **实际结果**：27 帧资源加载和 skyColour 区间已接入；截图/时间轴对照未执行。

### WPN-12-AI-01 — AI 效益

- **原版规则（S，`Character.as`）**：对水面上方 300 px 范围内的每名敌人，加 `health/maxHealth * 0.5`；对己方每名角色减 `1.5`。无路径或投射采样；`aiPerform` 直接 `startWave()`。
- **Unity 入口**：既有 `MutinyAIController.ScoreTidalWave/ExecuteMove`，现与 `MutinyTidalWave.StartWave` 使用同一固定右向入口。
- **验收**：固定角色布局下比较得分和 AI 启动的 x/y/vx。
- **实际结果**：静态映射已核对；固定 AI 场景未执行。

## 音频与资源

- `TidalWave.as` 未调用 `playSound`，原版 SFX 目录也没有专属 tidal/wave 音频；本武器不添加臆测的循环、命中或离场声音。
- 已存在 `Assets/Mutiny/Resources/Art/Weapons/TidalWave/1.png` 至 `27.png`。

## 待运行验证与已知差异

- `track` 目标与镜头控制器的实际跟随、波浪动画的像素注册点、角色持续受伤时的受击表现尚需 Unity/原版逐 tick 录像对照。
- 自动回归尚未在 Unity Editor 实际执行；编译通过不等于玩法验收通过。
