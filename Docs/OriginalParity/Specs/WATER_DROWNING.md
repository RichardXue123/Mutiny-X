# WATER：海水、落水判定与水花

状态：**原版静态证据已确认；Level 1 路径已实现并编译；自动用例已添加但尚未在 Unity 菜单中执行；画面和音频待 Play Mode 人工验证。**

## 1. 原版证据

| ID | 来源 | 确认内容 |
| --- | --- | --- |
| WATER-E01 | `throwgame/Controller.as` 的 `readXML` water 分支 | 水线坐标为 XML water 对象的 `y * 32` 像素。 |
| WATER-E02 | `throwgame/Character.as:180-196` | 角色中心 `y > water.y` 后，生命归零并立即 `alive=false`；随后应用 0.8 水阻和向下速度限制。 |
| WATER-E03 | `throwgame/Solid.as` 的 `splashCheck` | 物体跨越水线时只生成一次 `splash`，放在物体 x 和水线 y，并播放 `splash` 音效。 |
| WATER-E04 | `throwgame/Water.as` | 海面根据 `skyColour` 使用 `water1/2/3`，并以 49 px 周期跟随横向镜头。 |
| WATER-E05 | `symbols.csv` / `frame-labels.csv` | water 为 symbol 1651，共 30 帧，标签位于 1/11/21；splash 为 symbol 1780，共 72 帧，标签位于 1/25/49。 |
| WATER-E06 | 原版导出资源 | water 每帧 1024×384；splash 每帧 48×44；`Audio/SFX/splash.wav` 已存在。 |
| WATER-U01 | 2026-09-16 Unity 运行日志 | `inWater=True` 但 `alive=True drowned=False`，位置从 y=549 持续增加到 1801，速度固定为 1.5，回合一直被同一角色阻塞。 |

## 2. 行为规则

| 规则 ID | 前置/触发 | 预期结果 | Unity 入口 |
| --- | --- | --- | --- |
| WATER-01 | 活着的角色从 `y <= waterY` 进入 `y > waterY` | 当 tick 立即 `IsDrowned=true`、`IsAlive=false`、生命值为 0；死亡角色不再阻塞回合静止检查。 | `MutinyPhysicsBody.EvaluateWaterState` → `MutinyCharacter.Drown` |
| WATER-02 | 首次跨越水线 | 在角色 x、水线 y 生成一次当前天空色水花；24 tick 后销毁。持续位于水下不得重复生成。 | `MutinyWaterSurface.SpawnSplash` / `MutinySplashEffect` |
| WATER-03 | WATER-02 同一时刻 | 播放一次 `splash.wav`。 | `MutinyAudioManager.PlaySFX("splash")` |
| WATER-04 | 关卡由 XML 构建或从旧烘焙场景进入 Play Mode | 从关卡 water 对象恢复水线；旧场景的 C# 事件必须在 `Awake` 重新订阅。 | `MutinyLevelBuilder.BuildWater` / `MutinyLevelRoot.EnsureRuntimeWater` / `MutinyCharacter.Awake` |
| WATER-05 | Level 1 进入运行 | 在 XML 水线显示原版 `water1` 的 10 帧循环海面，覆盖可滚动关卡宽度。 | `MutinyWaterSurface.Initialize` |

## 3. 缺陷单 BUG-WATER-001

- 根因：旧烘焙场景中的 `MutinyCharacter.Initialize` 曾在编辑期订阅 `OnEnterWater`，但 C# event 不会写入场景。Play Mode 的 `Awake` 原先没有重绑，所以物理体进入水区后只有 `IsInWater` 和水阻发生变化，`Drown` 永远不会执行。
- 修复：事件在 `Awake` 和 `Initialize` 均通过同一幂等方法重绑；关卡根节点在 `Start` 明确设置所有角色的 water pixel Y，并为旧烘焙场景补建海面渲染器。
- 防回归：验证代码创建真实 `MutinyCharacter`/`MutinyPhysicsBody`，经生产 `EvaluateWaterState` 跨线，断言 drowned/dead，并断言真实 `MutinySplashEffect` 被生成；另检查 water、splash 和音频资源。

## 4. 验收用例

| 用例 | 操作与预期 | 当前结果 |
| --- | --- | --- |
| WATER-TC01 | 让敌方角色被爆炸击落；越过海面后立即死亡，回合不再出现该角色的 `still waiting`。 | 待 Unity Play Mode 验证 |
| WATER-TC02 | 录制跨线前后；水花只出现一次，位置锁定于角色 x 和海面 y，约 24/25 秒后消失。 | 待 Unity Play Mode 视觉验证 |
| WATER-TC03 | 打开 SFX，跨线时只听到一次原版 splash。 | 待 Unity Play Mode 音频验证 |
| WATER-TC04 | 启动 Level 1；海面位于 XML y=14（448 px），覆盖镜头可见宽度并循环动画。 | 待 Unity Play Mode 视觉验证 |
| WATER-TC05 | 经生产水事件执行 drowned/dead/splash 断言，并加载 1024×384 water、48×44 splash、splash AudioClip。 | 用例已添加，尚未在 Unity 菜单执行 |

## 5. 已知差异

- 本次完成当前 Level 1 默认 `skyColour=1` 路径。原版 `water2/3` 和 `splash2/3` 资源已保留，其他关卡何时调用 `setSkyColour` 仍需结合主时间轴逐关确认。
- 原版海面横向位置每帧按 49 px 对镜头取整；当前先在整个关卡范围平铺同一原版宽帧。水面图像与动画帧已一致，49 px 镜头跟随时序仍待后续镜头规格验收。
