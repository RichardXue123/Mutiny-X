# WPN-04 Boulder（巨石）行为规格

## 基线与范围

- 模块 ID：WPN-04。
- 原版：`Mutiny Source/mutiny-flash-game/mutiny.swf`，SHA256 `c034d2cfa50272a1aac93ad75114b542ca5993dd7b70fa8901aa195de7820586`。
- 本轮范围：普通拖拽投掷、释放减速、62 px Solid、滚动旋转、角色推挤/伤害、停稳 whiteOut；保留原版通用 Weapon 的入水 splash 和越过关卡底部结束分支。
- 证据状态：静态确认；实现状态：实现中；验证状态：未执行。

## 原版证据

| 证据 ID | 类型 | 文件、函数/符号 | 结论 |
| --- | --- | --- | --- |
| W04-S-01 | S | `Boulder.as` constructor | 四个 extent 均为 31；`friction=.25`、`weight=1.5`、`hitsBoxes=true`、`draggable=false`、`twangable=true`；内部 `mc.blendMode="layer"`。 |
| W04-S-02 | S | `Boulder.as release`；`Weapon.as release/fire` | `Boulder.release` 会在通用 drag-release 的 20 px/tick clamp/fire 后将两轴乘 `.5`；该方法本身不代表通常的 twang 发射路径。 |
| W04-S-03 | S | `Boulder.as advanceMotion`；`Solid.as advanceMotion` | 已发射时，在该 tick 物理推进前内部 `rotating` 每 tick 加 `velocityX*2.5`；随后执行通用 Solid terrain/box 碰撞和摩擦/反弹。 |
| W04-S-04 | S | `Boulder.as advanceMotion` | `vx==0 && abs(vy)<.5` 时：AI 模拟结束；真实物体 `visibility-=.1`，低于 1 时调用 `Global.whiteOut(visibility)`，低于 0 时 finished。 |
| W04-S-05 | S | `Boulder.as advanceMotion` | 每 tick 遍历所有 team 的活角色，跳过 owner；严格条件为 `char.y-top <= rock.y+32`、`char.y+bottom >= rock.y-32`、`abs(char.x-rock.x)<=32`。右侧角色设 `x=rock.x+32` 且仅 `rock.vx>0` 时加 vx；左侧对称且仅 vx<0 时加 vx；每次 `subtractHealth(abs(rock.vx)*1.5)`。 |
| W04-S-06 | S | `Weapon.as advance`、`Solid.as splashCheck` | Boulder 不覆盖 `Weapon.advance`：继续走通用底部结束和水面 crossing splash；原版没有 Boulder 专用水下淡出。 |
| W04-S-07 | S | `Character.as selectWeapon` lines 785–786, 825–829；`WeaponSelectButton.as hover_boulder` | 角色装备 Boulder；装备位置为 `(character.x, character.y-30)`；菜单提示为“Large boulder which can bash other players out of the way.”并要求拖拽瞄准。 |
| W04-A-01 | A | `DefineSprite_872_boulder` | 单帧 Boulder art 已导入 `Resources/Art/Weapons/Boulder/1.png`。 |
| W04-S-08 | S | `Character.as advance` lines 172–175；`Weapon.as advance` | 角色每 tick 调用 `equippedWeapon.advance()`，且 Boulder 未覆盖它。因而 Boulder 的 `.5` 可见度分支之后，同一 tick 仍会执行通用 `.2` 静止结束；不能把 Boulder 的分支误读为必然完整淡出 21 tick。 |
| W04-S-09 | S | `TileSystem.as mouseDown/mouseUp` lines 740–763；`Weapon.as twang/fire`；`Team.as advance` lines 91–96 | Boulder 是 `twangable=true`、`draggable=false`。人类松键走 `twanging.twang()`，其继承 `Weapon.twang` 仅调用 `Solid.twang` 并设 fired；AI 走 `aiPerform → Weapon.fire(vx,vy)`。两条正常发射路径均不会进入 `Boulder.release` 的 `.5` 分支。 |
| W04-A-02 | A | `mutiny.swf.xml` DefineSprite 872 lines 20980–20989；DefineSprite 869；bitmap 870 | Boulder 根 symbol 先放 depth 1、名为 `rotating` 的 sprite 869，再放 depth 3 的 shape 871。869 是可旋转石体，871 是独立的静止上层；根 symbol 只有一帧。 |

## 状态与转换

| 规则 ID | 前置状态 | 输入 / tick | 状态变化与可观察结果 | 来源 |
| --- | --- | --- | --- | --- |
| WPN-04-INT-01 | 活着人类角色有 Boulder 且可射击 | 选中、按住并松开 | `twang` 路径创建并以 Solid.twang 的 clamp 后速度发射；装备阶段中心在角色上方 30 px；库存一次扣除、两行动禁用 | W04-S-07/09 |
| WPN-04-INT-02 | AI 已选择 Boulder 候选 | AI 执行 tick | `aiPerform → Weapon.fire(vx,vy)` 直接使用候选速度；不套用 `.5` | W04-S-09 |
| WPN-04-EFF-01 | 发射 | 每 25 Hz tick | 31/31/31/31、weight 1.5、friction .25、可撞木箱；仅内部 `rotating` 图层按 `vx*2.5` 旋转，depth 3 上层保持不转 | W04-S-01/03、W04-A-02 |
| WPN-04-EFF-02 | 发射且角色满足边界 | 每 tick | 推至巨石左右 32 px；仅向接触方向把 vx 加给角色；扣 `abs(vx)*1.5` HP | W04-S-05 |
| WPN-04-ANI-01 | 物理后 `vx==0 && abs(vy)<.5` | Boulder `advanceMotion` | 先将 `visibility` 减 .1；若此时也满足继承 Weapon 的 `.2` 条件，则同 tick 结束。只有未达到 `.2` 的停稳帧才会继续走 whiteOut/后续淡出。 | W04-S-04/08 |
| WPN-04-EFF-03 | 发射后越过水面或关卡底部 | 通用 Weapon tick | 水面任一方向 crossing 播 splash、更新 `overWater`，但不施加角色水下阻尼；向下越过 level height 后结束。 | W04-S-06 |

边界：x 差等于 32、纵向边界等于比较值时仍命中；owner 始终免疫。vx 为 0 时仍可将重叠角色推至左侧并造成 0 HP，这是 AS2 循环的直接结果。`Global.whiteOut` 的 additive 前半段需独立 Unity material 才能像素级一致。

## Unity 映射与验证

| 规则 | Unity 入口 | 用例 | 实际状态 |
| --- | --- | --- | --- |
| WPN-04-INT-01..02 | `MutinyPlayerInput.Launch` / `MutinyAIController.ExecuteMove` → `MutinyBoulder.Twang/Fire` | W04-T-01 / W04-T-06 | 未执行 |
| WPN-04-EFF-01..03 | `MutinyBoulder`、`MutinyPhysicsBody` | W04-T-02..04 | 未执行 |
| WPN-04-ANI-01 | `MutinyBoulder`、`Art/Weapons/Boulder/1` | W04-T-05 | 未执行 |

| 用例 ID | 操作 | 期望 |
| --- | --- | --- |
| W04-T-01 | 经生产输入拖拽，给出超过/未超过 20 的向量 | `twang` 的 20 px/tick clamp 后直接发射；库存/行动一次提交，不能错误减半 |
| W04-T-02 | 固定地面与木箱，推进 25 Hz | 31 px extent、1.5 weight、.25 friction、box 碰撞和 `vx*2.5` 旋转 |
| W04-T-03 | 放置 owner、左右角色和 x/y 临界角色 | 仅原版闭区间命中；推位、速度和伤害精确 |
| W04-T-04 | 越过水面和地图底部 | 触发通用 splash；水面后保持 Solid 原速度而不使用角色阻尼；仅底部结束 |
| W04-T-05 | 从 `vy=-1.5` 进入无碰撞 tick，使物理后速度为 0 | Boulder 先将 visibility 2→1.9，继承 Weapon 随即在同 tick 以 `.2` 静止条件结束；不得等待完整淡出 |
| W04-T-06 | 经 AI 生产执行以固定 `(12,-8)` 候选发射 | 物理初速度精确为 `(12,-8)`，AI 预测使用相同速度，不套用 `.5` |

## 完成记录

- 已静态确认：W04-S-01..09、W04-A-01..02。W04-S-08 修正了旧规格对 Boulder 淡出时序的错误推论；W04-S-09 区分了未被正常 Boulder 使用的 drag-release 分支与人类/AI 实际发射路径。
- 已实现：`MutinyBoulder` 的四向 31 px Solid、hitsBoxes、角色接触、停稳 whiteOut、原版通用 splash/底部结束、无角色水阻尼的跨水面处理，以及 869 可旋转石体与 871 静止上层的原始时间轴结构；人类和 AI 速度修复待本轮代码/回归完成。
- 已执行并通过：`dotnet build Assembly-CSharp.csproj --no-restore`（0 error；现存 `MutinyLevelTest.levelXml` CS0649 warning）。
- 待验证：W04-T-01..05 的 Unity 执行、原版实际画面/音效/镜头对照。
