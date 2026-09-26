# 09.02 · 角色覆盖层

[返回上级模块](../README.md)

## 职责

为每个活着的角色显示世界空间叠加 UI：回合指示器（P1/P2/CPU 三角形）、离散血条、选择框、巫毒标靶和取消武器按钮。

## 行为规格

| ID | 可观察行为 | 原版来源 | Unity 入口 | 验收用例 | 当前结果 |
| --- | --- | --- | --- | --- | --- |
| CHAR-OVR-AIM-01 | Throw Self/跳跃拉线蓄力属于 `twanging` 而不是 `dragging`，当前角色 P1/P2 标记、选择框、血条和取消叉保持可见；松开正式起跳才按 `thrown` 隐藏。取消蓄力不消耗跳跃，其他角色不受影响 | `Character.as:68-69,126-143,176-179,742-747`；`TileSystem.as:740-746,760-763`；对应 pcode；用户提供的原版截图 | `MutinyCharacterOverlay.LateUpdate`、`MutinyPlayerInput.TryBeginAimFromPrimaryPointer/ResolveAimRelease/ShouldShowCancelWeapon` | 生产选取跳跃→蓄力并显示轨迹→检查四项 UI→右键/叉取消→重新蓄力并正常松开起跳→检查隐藏与行动结算恢复 | 原版静态确认与用户截图一致；已实现；2026-09-26 Unity 6000.6.0f1 隔离 Play Mode 共 25/25 断言通过（本规则 7 条、既有覆盖层 13 条、触屏取消 5 条）；主工程画面及安卓真机待验收 |

2026-09-26 更正：此前把 `Aiming` 等同于 `Controller.dragging` 的结论错误。原版角色 `draggable=false、twangable=true`；`updateOverlay` 没有用 `Controller.twanging` 隐藏标记、血条、选择框或取消叉。安卓第二指取消/松手落在叉上取消仍是输入适配扩展，但“蓄力显示叉”本身不是扩展。

## 原版 Flash 对应

| Unity 类 | Flash 对应 | 说明 |
|---|---|---|
| `MutinyCharacterOverlay` | `Character.characterOverlay` MovieClip | 附着在 `mcHolder` 上，不继承角色旋转 |
| 指示器 | `characterOverlay.triangle` | P1/P2/CPU 三种帧 |
| 血条 | `characterOverlay.health` (symbol 1864/1877) | 28 帧离散动画 |
| 选择框 | `characterOverlay.corners` | 四角 L 形白线 |
| 巫毒标靶 | `characterOverlay.target` (symbol 1871) | 41×42 px 十字标记 |
| 取消武器 | `characterOverlay.cancelWeapon` | 红/蓝取消按钮 |

## 架构与类关系

```
MutinyCharacterOverlay (MonoBehaviour)
├── RequireComponent: MutinyCharacter
├── 创建 OverlayRoot (GameObject)  → 保持世界正向旋转
│   ├── TurnIndicator              → SpriteRenderer (P1/P2/CPU)
│   ├── HealthBar                  → 背景帧 + 离散填充条
│   ├── SelectionCorners           → 4个 LineRenderer L 形
│   ├── VoodooTarget               → SpriteRenderer (十字标记)
│   └── CancelWeapon               → SpriteRenderer (红/蓝按钮)
└── 监听 MutinyCharacter 事件
    ├── OnHealthChanged → UpdateHealthBar()
    └── OnDeath         → SetOverlayVisible(false)
```

## 核心常量

| 常量 | 值 | 说明 |
|---|---|---|
| `PixelsPerUnit` | 与 `MutinyPhysics.PixelsPerUnit` 一致 | 坐标转换基准 |
| `HealthSegments` | 27 | 血条离散段数（原版 28 帧含 1 帧空状态） |
| `OriginalIndicatorTopY` | -38.05 px | 指示器三角形顶部 Y 偏移（twips: -761） |
| `OriginalHealthCenterY` | 18 px | 血条中心 Y 偏移（twips: 360） |
| `OriginalCancelWeaponCenterY` | 33 px | 取消按钮中心 Y 偏移 |

## 详细逻辑流程

### 1. 初始化 (`CreateOverlayUI`)

在 `Awake` 中创建完整 UI 层级：

1. **OverlayRoot**：空 GameObject，挂载在角色 `transform` 下，`localPosition = Vector3.zero`
2. **TurnIndicator**：位于 `(0, +38.05/PPU, 0)` 世界空间上方，使用顶部中心 pivot
3. **HealthBar**：位于 `(0, -18/PPU, 0)` 世界空间下方
   - 背景帧：加载 `UI/CharacterOverlay/health_background`，pivot `(0.5, 0.5)`
   - 填充条：白色像素精灵，左对齐 `localPosition.x = -13/PPU`
4. **VoodooTarget**：位于 `(-1/PPU, 0, 0)`（原版 translateX=-20 twips）
5. **CancelWeapon**：位于 `(0, -33/PPU, 0)`
6. **SelectionCorners**：4 角 L 形白线

### 2. 保持正向旋转 (`KeepOverlayUpright`)

每帧 `LateUpdate` 强制 `m_OverlayRoot.transform.rotation = Quaternion.identity`。

原因：Flash 中 `Clip.update()` 只对 `mc._rotation` 施加旋转，`characterOverlay` 挂在未旋转的 `mcHolder` 上。Unity 中角色 transform 会传递旋转，必须手动抵消。

### 3. 可见性判断 (`LateUpdate`)

每帧执行以下逻辑：

```
角色已死亡 → 隐藏全部
├── showIndicator = 当前回合队伍 && !自抛中 && 非当前说话角色
├── showHealth   = !自抛中
├── showVoodooTarget = 巫毒瞄准中 && (目标==本角色 || (无目标&&本角色被悬停))
├── showCancelWeapon = PlayerInput.ShouldShowCancelWeapon(本角色)
└── showCorners  = !巫毒瞄准全局 && (被选中||被悬停) && !自抛中
```

关键边界：
- **巫毒瞄准全局隐藏选择框**：`Character.updateOverlay` 在任何角色有未发射巫毒娃娃时强制关闭所有角色的 corners
- **物理运动不隐藏指示器**：爆炸/碰撞推动不算 `Character.thrown`，原版中不关闭 overlay
- **拉线蓄力不隐藏覆盖层**：角色只能 `twang`，不能 `drag`；原版 `Controller.dragging == this` 对正常角色蓄力不成立。只有已经提交的自抛才抑制角色覆盖层。

### 4. 指示器精灵切换 (`UpdateIndicatorSprite`)

| 条件 | 资源路径 |
|---|---|
| AI 控制队伍 | `UI/CharacterOverlay/cpu_indicator` |
| Team2（蓝） | `UI/CharacterOverlay/p2_indicator` |
| Team1（红） | `UI/CharacterOverlay/p1_indicator` |

使用字符串缓存 `m_IndicatorResource` 避免重复加载。

### 5. 离散血条 (`UpdateHealthBar`)

**帧计算公式**：
```
frame = 1 + Clamp(Ceil(27 * shownHealth / Max(1, maxHealth)), 0, 27)
```
- 共 28 帧（1=空，28=满）
- 使用 `ShownHealth` 而非 `Health`，因为 ShownHealth 以 25Hz 逐步趋近实际值

**填充条渲染**：
- `localScale = (segments, 4, 1)`，其中 `segments = frame - 1`
- 白色像素精灵被缩放为 `segments × 4 px`
- 左对齐起始于 `x = -13 px`

**颜色**：
- Team1：`rgba(230, 49, 19)` 红色（原版 symbol 1864）
- Team2：`rgba(51, 95, 255)` 蓝色（原版 symbol 1877）

### 6. 选择框 (`CreateSelectionCorners`)

四角 L 形白色线段，使用 `LineRenderer`：

| 角 | 像素坐标 | 方向 |
|---|---|---|
| TopLeft | (-14, 14) | (+1, -1) |
| TopRight | (14, 14) | (-1, -1) |
| BottomLeft | (-14, -14) | (+1, +1) |
| BottomRight | (14, -14) | (-1, +1) |

每条 L 线 3 个顶点、线宽 2px/PPU，6px 臂长。

### 7. 取消武器按钮 (`UpdateCancelWeaponSprite`)

- Team1：`UI/button_cancel_red`
- Team2：`UI/button_cancel_blue`
- 显示条件由 `MutinyPlayerInput.ShouldShowCancelWeapon()` 控制：
  - 角色是当前选中角色
  - 角色存活且武器未锁定
  - 交互状态为 `WeaponReady` 或 `Aiming`，且角色尚未正式自抛
  - 有已装备但未发射的武器，或已选择可用的 Throw Self/跳跃

## SortingOrder 分层

所有排序基于角色自身的 `SpriteRenderer.sortingOrder` + `CharacterOverlaySortingOffset`：

| 元素 | 相对偏移 |
|---|---|
| 血条背景 | +0 |
| 血条填充 | +1 |
| 指示器 | +2 |
| 选择框 | +3 |
| 巫毒标靶 | +4 |
| 取消按钮 | +5 |

## 资源依赖

| 路径 | 用途 |
|---|---|
| `UI/CharacterOverlay/health_background` | 血条背景帧 |
| `UI/CharacterOverlay/p1_indicator` | P1 红色三角 |
| `UI/CharacterOverlay/p2_indicator` | P2 蓝色三角 |
| `UI/CharacterOverlay/cpu_indicator` | CPU 标记 |
| `UI/CharacterOverlay/voodoo_target` | 巫毒十字标靶 (41×42 px) |
| `UI/button_cancel_red` | 红色取消按钮 |
| `UI/button_cancel_blue` | 蓝色取消按钮 |

## 与其他模块的接口

| 方向 | 模块 | 接口 |
|---|---|---|
| 读取 | `MutinyCharacter` | `IsAlive`、`Health`、`ShownHealth`、`MaxHealth`、`IsSelected`、`IsHovered`、`TeamIndex`、`IsSelfThrown` |
| 读取 | `MutinyTurnManager` | `CurrentTeam` |
| 读取 | `MutinyPlayerInput` | `ArmedVoodooDoll`、`ShouldShowCancelWeapon()` |
| 读取 | `MutinyTeam` | `IsAiControlled`、`Characters` |
| 监听 | `MutinyCharacter.OnHealthChanged` | 触发血条刷新 |
| 监听 | `MutinyCharacter.OnDeath` | 隐藏全部覆盖层 |

## 验证要点

- 角色旋转时覆盖层保持正向（overlay root rotation = identity）
- 血条帧 1-28 与 ShownHealth 线性映射，满血=28、0 血=1
- 巫毒瞄准全局隐藏所有角色的选择框
- 取消按钮在 WeaponReady/Aiming + 未锁定且未自抛时显示：既包括未发射的装备武器，也包括已选择的 Throw Self/跳跃
- 正常跳跃蓄力保留 P1/P2 标记、选择框、血条和取消叉；正式提交自抛后才隐藏，行动延续时恢复对应资格下的 UI
