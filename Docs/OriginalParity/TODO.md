# Mutiny 原版完整流程复刻 TODO

## 当前新增：单机前端最小流程

- [~] P8-05 / FRONT-01..08：已接入标题页 → 人数选择 → 15 关单人选关 → 现有关卡的生产流程；使用原版 Logo、面板、按钮底图及 15 帧敌方海盗预览，锁定黑影、问号、两位编号、Back 和逐关解锁逻辑已实现。PirateFont 已恢复原版逐字形注册点，修复 R/K 被整体上移 4 px；C# 编译与 Unity Play Mode 视觉验收状态见 `VALIDATION.md`。规格见 `Specs/FRONTEND_SINGLE_PLAYER_FLOW.md`。

当前工作：**P4 十五种武器按效果、动画/装备实例、音效和完整生命周期逐项 1:1 复刻**。每次武器修改先按 `Specs/WEAPON_REPLICATION_WORKFLOW.md` 建立来源图、状态机、显示/声音契约和生产入口用例；逐武器范围与状态见 `Specs/WEAPON_REPLICATION_PLAN.md`。

完成标准：遵循 `REPLICATION_STANDARD.md`，每个行为条目必须有原版证据、Unity 对应入口及所需验收层的实际通过记录。仅写出人工验收步骤属于未执行，不能计为完成；旧清单中“类已存在”或已有步骤的项目不自动继承通过状态。

新增优先缺陷：**BUG-TURN-UI-001 未行动时跳跃图像仍为灰色**。证据和用例见 `Specs/TURN_ACTION_UI.md`，涉及 P2-04 / P2-06 / P8-02 / P10-02；原版按钮状态、面板点击时序和回合阈值已实现，Unity 自动验证 29/29 通过，受控场景截图及原版运行对照仍待执行。

## P0 基线与证据

- [x] P0-01 建立独立的一致性任务、审计和阻塞记录。
- [x] P0-02 保存 18/18 XML 扫描基线：107 tiles、29 objects、20 attributes、0 errors。
- [~] P0-03 建立 AS2 类/资源符号/Unity 类三方索引。
- [ ] P0-04 固定 25 Hz 模拟时钟、随机数种子记录和可重复回放输入格式。
- [人工] P0-05 录制原版 Level 1 完整回合参考视频，包含选人、Throw Self、至少一种武器、敌方回合和空投。

## P1 关卡初始化

- [~] P1-01 按 XML 原坐标生成前景、背景和对象，核对 32 px 网格及角色脚底偏移。
- [~] P1-02 核对全部 107 种 tile 的 SWF symbol、层级、原点、碰撞类型和动画。
- [ ] P1-03 核对背景 `antichest`、水面、地图边界及各关卡天空色。
- [~] P1-04 按角色 XML type 分队；红海盗/红船长为玩家队，其余为敌队。
- [~] P1-05 忠实读取每个角色的初始武器数量，不能用统一默认背包覆盖 XML。
- [~] P1-06 按 XML 权重展开关卡空投武器池，并计算有效空投列。
- [ ] P1-07 核对箱子、木箱、水和其他 level object 的初始状态及排序。
- [~] P1-08 去除运行时对 `UnityEditor.AssetDatabase` 的依赖，保证 Player build 能加载关卡和全部美术。

## P2 回合生命周期和行动

- [~] P2-01 回合开始时重置存活角色行动能力，但不自动选中角色。
- [~] P2-02 首回合镜头优先船长，后续回合选择距离当前视觉中心最近的存活角色作为镜头目标。
- [ ] P2-03 玩家悬停角色显示原版白色角框；点击本队存活角色才进入行动菜单。
- [~] P2-04 原版行动菜单：Throw Self、武器格、数量、不可用状态、关闭和切换角色。
- [~] P2-05 Throw Self 按住左键拉力、15 tick 白色虚线预测、松开投掷。
- [~] P2-06 投掷自身后仍能选择并使用武器；两种行动均用尽后才允许结束回合。
- [ ] P2-07 武器行动与角色行动在物理、爆炸和动画完全静止 10 tick 后结算。
- [~] P2-08 角色死亡、落水、无可选角色、双方同时死亡和胜负流程与原版一致。Level 1 落水判定、海面、水花和 splash 音效已实现，见 `Specs/WATER_DROWNING.md`；Play Mode 及其余死亡/胜负分支仍待验收。
- [~] P2-09 单人结算已按 `Controller.nextTurn/enterFrame`、`IngamePopup` 接入 level complete / level failed / 第 15 关 game complete、单局累计计分、下一关、重开清零和单人选关返回；`END-POP-T01..04` 已写入生产入口回归并且 C# 编译通过。原版胜负台词的约 161 tick 前置时序、失败后的外部提交分数页、最终 congratulations 页面及 Unity Play Mode/原版运行对照仍待完成。见 `Specs/GAME_END_POPUP.md`。
- [~] P2-10 AI 的选人、移动/投掷、选武器、瞄准、等待和结束条件逐项复刻。AI-SEL-01..03、AI-MOVE-01..03、AI-WPN-01..02 的静态原版证据和 Unity 实现已记录在 `Specs/AI_DECISION_FLOW.md`；C# 编译通过，生产入口回归用例已加入 `MutinyTurnActionUiVerificationTest`，仍待 Unity 执行该菜单用例及原版运行对照。海鸥、箱类、锚和大炮的专用 `aiPerform` 分支仍属于 AI-WPN-03。
- [x] P2-EXT-01 用户授权的右键取消：仅在拉力瞄准中取消当前抛射并回到待抛射阶段，保留武器实例、库存与行动资格；返回武器菜单使用角色下方叉号。见 `Specs/WEAPON_READY_AND_CANCEL.md`。
- [x] P2-EXT-02 建立 GM 调试命令说明书，记录命令、别名、目标选择、存档范围和维护要求；见 `../GM_COMMANDS.md`。

## P3 武器公共流程

状态说明：未实现 `[ ]`、实现中 `[~]`、已实现 `[实现]`、待人工验证 `[人工]`、已完成 `[x]`。只有完成效果、动画/装备实例、音效、输入/镜头/回合和验证五个维度后，单种武器才能逐级推进。静态代码首版不能越过“实现中”；来源图、调用路径、25 Hz、显示层、音效和交付门槛见 `Specs/WEAPON_REPLICATION_WORKFLOW.md`，逐武器状态见 `Specs/WEAPON_REPLICATION_PLAN.md`。

- [~] P3-01 从原版 `Weapon`/`Solid` 提取每种武器的重量、拉力上限、反弹、跟踪、伤害和冲量参数。
- [~] P3-02 为每种武器使用其自身重量生成预测线，预测与 25 Hz 实际轨迹逐 tick 一致。
- [ ] P3-03 核对有限库存扣除时机、取消时恢复、拾取增加和耗尽后的 UI。
- [ ] P3-04 核对 terrain、角色、箱子、水面和地图外碰撞分支。
- [ ] P3-05 核对爆炸半径、距离衰减、击退、连锁反应、镜头震动和残骸。
- [ ] P3-06 核对所有武器的发射、飞行、命中、爆炸、特殊阶段和结束音效。
- [~] P3-07 还原选中武器后的统一待抛射阶段、角色装备点实例、下方阵营色取消叉号，以及拉力中右键只取消本次拉力的授权扩展；BUG-WRDY-POS-001 已恢复原版“初始 `(x,y-10)`/Boulder `(x,y-30)` 后继续执行各武器未发射 `advance`”的行为，香蕉等普通武器会按各自 extent/weight/碰撞移动，金币执行 `owner.y+5` 后同 tick 重力，Box/Anchor/TidalWave 保持其专用待命路径；Anchor/Box/Seagull/TidalWave 依据构造函数未调用 `show()` 而只在提交后显示本体。资源注册点逐 linkage 校验。专项生产回归已加入，C# 编译与 Unity Play Mode/原版运行对照状态见 `Specs/WEAPON_READY_AND_CANCEL.md`。
- [x] P3-08 建立武器专项复刻工作流：每种武器必须追踪本体/父类/调用者/AI/物理/时间轴/声音，先列状态机与 25 Hz 顺序，再以生产入口和原版对照验收；见 `Specs/WEAPON_REPLICATION_WORKFLOW.md`。

## P4 十五种武器

- [~] P4-01 / WPN-01 Cherry Bomb — **实现中**：碰撞爆炸、伤害/范围/击退、手持与飞行动画、音效及逐 tick 对照仍须按原版逐项闭环。
- [~] P4-02 / WPN-02 Dynamite — **实现中**：翻滚、碰墙/落地、停止倒计时、亮灭帧、爆炸参数和独立音效链仍须逐项闭环。
- [~] P4-03 / WPN-03 Banana — **实现中**：已按 `Banana.as` 接入 7 px extent、0.8 反弹、0.5 摩擦、第二次点击引爆、AI 20 px 近距引爆、160/80 爆炸与 `banana_bounce`/立即 `pop` 时序；BUG-WPN-03-001 已修复 `ActionExecuting` 提前返回导致第二击无法到达香蕉的问题，并加入生产输入门控回归。Unity Play Mode 与原版对照未执行。规格见 `Specs/BANANA.md`。
- [~] P4-04 / WPN-04 Boulder — **实现中**：已按 `Boulder.as` 接入 31 px 四向 extent、1.5 weight、.25 friction、木箱碰撞、`vx*2.5` 旋转、角色 32 px 闭区间推挤/`abs(vx)*1.5` 伤害、通用水面 crossing（无角色水阻尼）与地图底部结束；人类 twang 和 AI fire 已修正为直接使用原始速度，不再错误套用不可达 `Boulder.release` 的 `.5`。DefineSprite 872 已拆为可旋转石体与静止上层，并修正为原版 64 px、中心注册点、32 PPU 的尺寸，避免 Unity 默认 100 PPU 缩小画面。`Weapon.advance` 的 `.2` 静止结束会紧接 Boulder 的 `.5` 可见度分支，已修正规格与回归预期。白化 additive 材质、装备注册点、Unity Play Mode 与原版逐 tick 对照未执行。规格见 `Specs/BOULDER.md`。
- [~] P4-05 / WPN-05 Cannon — **实现中**：已按 `Cannon.as`/`Cannonball.as` 建立独立 Cannon 工厂、120 px 拖放、随炮身旋转换算的 pin `[-40,-21]`、严格 `<-30` 装填、回弹后不受通用 20-force 限制的 30 力开火、Cannonball 0 weight/box 碰撞、100/50 爆炸、边界无爆炸、烟雾、Cannon 淡出与 `cannon explosion`/`pop` 时序；隐藏炮弹保持至淡出结束。生产鼠标输入和原版 AI 选点/25 tick 延迟发射已接入。准确注册点/层级、Play Mode 与原版逐 tick 对照未执行；规格见 `Specs/CANNON.md`。
- [~] P4-06 / WPN-06 Gunpowder Barrel — **实现中**：已按 `BoxWeapon.as`/`GunpowderBarrel.as`/sprite 968 接入一次行动两桶、16/15px box、tile/chest/活角色/box 拒绝、共享 box 碰撞、150/30/null-caster 连锁爆炸、explode 第 11 帧及 AI 40/10 tick 链路；生产回归和 C# 编译已完成，Unity Play Mode、音效/视觉、AI 实战及原版逐 tick 对照待执行。规格见 `Specs/GUNPOWDER_BARREL.md`。
- [~] P4-07 / WPN-07 Mine — **实现中**：已按 `Mine.as` 接入 14 px、1.5 friction、10 tick 忽略期、仅移动角色严格 `<60 px` 触发、60 tick 倒计时、固定蜂鸣序列、250/70/owner 爆炸和静止跨回合存续；移除了无原版依据的入水中和。C# 编译通过；原版 arm/warn/explode 时间轴、生产回归、Play Mode 与逐 tick 对照待执行。规格见 `Specs/MINE.md`。
- [~] P4-08 / WPN-08 Parachute Bomb — **实现中**：已按 `ParachuteBomb.as`/`Weapon.as`/`Solid.as` 接入 11 px extent、30-force twang、每 tick `vy>1` 的减 2 和 `.95` 横向阻尼、严格 `vy>-10` 开伞、鼠标左右反向 `.2` 扇风、每 12 tick `fan`、160/50 即时 contact 爆炸、仅水花的水面 crossing、未开伞烟雾、`closed/opening/open` 30 帧循环及 `y=-300` 顶端限制。生产输入回归已加入，C# 编译通过；Unity Editor 执行、Play Mode、光标风扇画面和原版逐 tick 对照未执行。规格见 `Specs/PARACHUTE_BOMB.md`。
- [~] P4-09 / WPN-09 Pieces of Eight — **实现中**：已按 `PiecesOfEight.as`/`Weapon.as`/`Character.as`/`Controller.as` 接入单回合同一实例的 8 枚连续投掷（非跨回合）、7 px/box collision、terrain/box 的 50/25 立即 pop、入水仅 splash、前 7 枚 `weaponLocked` 回到 owner `(x,y+5)`、第 8 枚结束、AI 20 tick 等待与十样本 70 px 评分，以及生产输入回归。`dotnet build Assembly-CSharp.csproj --no-restore` 通过；Unity Editor、Play Mode、实际物理碰撞/水面、AI 和原版逐 tick 对照未执行。规格见 `Specs/PIECES_OF_EIGHT.md`。
- [~] P4-10 / WPN-10 Rum Bottle — **实现中**：已按 `RumBottle.as`/`SweepingFlame.as` 接入 80/25 小爆炸、仅地面双向点火、每段 30 伤害、8 px 传播、12 帧酒瓶、11 帧原版火焰与立即 `pop` 时序；生产物理入口回归已加入。烟雾原版帧、Unity Play Mode 和原版逐 tick 对照未执行；规格见 `Specs/RUM_BOTTLE.md`。
- [~] P4-11 / WPN-11 Seagull — **实现中**：已按 `Seagull.as` 接入路径选择、从 `x=-300` 的 10 px/tick 飞行、重复投放、50/50 粪弹爆炸、落水不爆炸、`poop1..3` 随机声和 AI 十点采样；生产入口回归已加入。原版虚线路径预览、镜头偏移、箱体碰撞及 Unity/原版对照待完成；规格见 `Specs/SEAGULL.md`。
- [~] P4-12 / WPN-12 Tidal Wave — **实现中**：已按 `TidalWave.as` 接入固定 `(-550,water.y)` 右向 20 px/tick 波、每 tick `±150/300 px` 的 5 HP 伤害窗、离场阈值、三套天空色动画和原版 AI 得分入口；生产物理回归已加入。镜头、受击表现和 Unity/原版对照待完成；规格见 `Specs/TIDAL_WAVE.md`。
- [~] P4-13 / WPN-13 Voodoo Doll — **实现中**：已按 `VoodooDoll.as`/`TileSystem.as` 接入先选 30 px 内敌人、目标十字、拖拽投掷、10 tick 娃娃阶段、镜头后 10 tick 延迟、一次目标传速、10%/tick 淡出及人类/AI 的 `voodoo` 声音；生产对象回归已加入。真实菜单、镜头边界、目标死亡/落水/箱体与原版逐 tick 对照未执行；规格见 `Specs/VOODOO_DOLL.md`。
- [~] P4-14 / WPN-14 Wooden Crate — **实现中**：已按 `BoxWeapon.as` 接入一次选中三次原坐标合法放置、16/15 px 箱体、角色/箱体阻挡、爆炸 AABB 销毁、原版 965 帧资源和生产入口回归；AI、`explode` 标签帧界、入水、音效、Play Mode 与原版逐 tick 对照未执行。规格见 `Specs/WOODEN_CRATE.md`。
- [~] P4-15 / WPN-15 Anchor — **实现中**：已按 `Anchor.as` 接入横坐标点击、强制 `y=-200`、每 25 Hz `vy=40` 的自驱动下落、terrain/木箱碰撞、严格 48×64 命中窗的 60 HP 压砸、12 帧落地时间轴、30 tick 停留、10 tick whiteOut 收尾和 floor contact 的 `anchor` 音效；生产输入/物理回归已加入，C# 编译通过。原版 AI 20 tick 专用入口尚未接到 AI 决策，additive whiteOut 材质、镜头、无地面长期行为、Unity Play Mode 与原版逐 tick 对照未执行。规格见 `Specs/ANCHOR.md`。

## P5 角色和动画

- [x] P5-01 建立全部 27 个 XML 角色 type 到 SWF character symbol 的映射。
- [~] P5-02 导出并重组嵌套 MovieClip 时间轴，保留帧标签、原点、帧率和循环范围。角色父时间轴已按第 13/35 帧 `gotoAndPlay("static")` 动作修正可见边界，见 `Specs/CHARACTER_TIMELINE.md`；内部嵌套时间轴仍待逐角色核对。
- [~] P5-03 为每个角色分别实现 idle；不得以同一张预览图或统一摆动代替。27 个角色的四段三 tick 弹动节奏已接入初始烘焙场景和 Restart 重建路径；透明第 13/14 帧造成的闪烁已修复，自动验证与 Play Mode 验收状态见 `Specs/CHARACTER_TIMELINE.md`。
- [~] P5-04 为每个角色分别实现 selected/drag/throw/airborne/land。角色飞行旋转与落地逐次回正已按原版 25 Hz 时序实现；BUG-ROT-002 将权威逻辑角与 Unity 渲染角分离，在相邻 tick 之间插值，保留 `vx*3`、角色 2 px/tick 摩擦和同 tick 落地回正，使旋转快→慢过程连续可见。见 `Specs/ROTATION_ANIMATION.md`；各角色姿势时间轴及 Play Mode/原版对照仍待完成。
- [~] P5-05 为每个角色分别实现 hit、墙面碰撞、爆炸击飞、低血量和死亡。受击/碰撞后的整体旋转、落水附加旋转，以及陆地死亡的 24 帧骨头动画、脚底偏移和 `die` 音效已按原版实现；被动旋转同样使用 BUG-ROT-002 的逻辑角/渲染角路径。BUG-CHAR-AUD-001 已将错误的“扣血即 hitwall”改为原版统一物理接触路径：地面/墙/天花板仅在距上次接触 `>5` tick 时播放 `hitwall`，主动移动/滚动无独立声音。见 `Specs/ROTATION_ANIMATION.md`、`Specs/LAND_DEATH.md` 与 `Specs/CHARACTER_COLLISION_AUDIO.md`；逐角色受击时间轴和 Play Mode/原版听感对照仍待完成。
- [ ] P5-06 还原角色方向、随机 idle 变体、帧事件和表情差异。
- [~] P5-07 还原 character overlay：队伍箭头、生命条帧、白角框、目标标记和取消标记。P1/P2/CPU 原版位图、回合/死亡可见性与 27 段血条已实现；仅玩家拖拽或主动自身跳跃隐藏覆盖层，爆炸/碰撞被动位移保持显示。BUG-CHAR-OVR-002 已恢复标识和血条注册点；BUG-CHAR-OVR-003 已使覆盖层跟随角色位置但不继承炸飞、碰撞、落水旋转；BUG-CHAR-LAYER-001 已将重合角色按原始 XML 创建顺序分配稳定显示层，overlay 随所属角色层派生。代码断言已加入，待用户 Unity/原版截图对照。对话框目标和 voodoo target 子层仍待完成，见 `Specs/CHARACTER_OVERLAY.md`、`Specs/CHARACTER_LAYERING.md`。
- [ ] P5-08 逐角色核对声音映射；船长等变体不得通过简单删除名称后缀猜测。

## P6 空投和拾取

空投系统已于 2026-09-17 按用户要求恢复。原版触发、关卡权重/位置、90 帧显示、三段声音、被动位移拾取、镜头和 AI 评分见 `Specs/AIR_DROP.md`；C# 编译通过，Unity Play Mode/原版运行对照待执行。历史停用记录见 `Specs/AIR_DROP_DEBUG_DISABLE.md`。

- [~] P6-01 还原空投触发时机和最大同时存在数 3。
- [~] P6-02 从有效列随机选 x，找到首个实体地面，并执行角色/箱子/已有空投避让。
- [~] P6-03 从 XML 权重池有放回抽取 1–3 件内容。
- [~] P6-04 从 y=-300 px 以每 tick 3 px 下落；10–19 降落伞循环，在地面上方 15 px 停止并播放 20–34 落地动画。
- [~] P6-05 存活且未被鼠标主动拖拽的角色进入范围后打开；跳跃/爆炸/碰撞造成的被动移动可以拾取；10 tick 给第一件、之后每 40 tick 给一件。
- [~] P6-06 拾取逐件增加角色库存，按实际武器及队伍色播放 44–80 弹出动画和 `icon_collect`，结束后播放 81–90 并淡出。
- [~] P6-07 下落和拾取阶段正确重置 inactivity，避免回合提前结束。
- [~] P6-08 AI 移动落点距未结束空投严格小于 40 px 时增加 0.5 评分；生产评分入口已接入，实战选择待 Play Mode/原版对照。

## P7 镜头

- [~] P7-01 鼠标距 550×400 视口边缘 40 px 时，以原版加速度趋向 10 px/tick。
- [ ] P7-02 行动面板显示时禁用边缘滚屏，拖拽瞄准时停止自动滚屏。
- [ ] P7-03 角色投掷、武器飞行、敌方行动分别按原版目标偏移和速度 30 跟踪。
- [ ] P7-04 回合开始、角色选中、台词气泡按原版优先级和速度平移。
- [~] P7-05 空投下落前 100 tick 以速度 50 跟踪空投。
- [ ] P7-06 镜头限制在地图、水面和原版视觉中心边界内。
- [ ] P7-07 爆炸震屏、目标切换及多个动态对象并存时的优先级。

## P8 UI

- [~] P8-01 用已导出的红/蓝武器面板和 15 个图标替换占位 UI。
- [ ] P8-02 从 SWF 时间轴恢复按钮帧、hover/down/disabled 状态和准确点击区域。
- [ ] P8-03 还原武器数量字体、位置、格子排序、帮助文字和队伍配色。
- [ ] P8-04 还原回合提示、角色生命、气泡、关卡开始/结束和暂停界面。
- [~] P8-05 还原主菜单、关卡选择、说明、设置、胜负和计分 UI。`HUD-CORNER-01..06` 已按 `CornerQuitButton`、`IngamePopup`、`ContinueGameButton`、`QuitGameButton` 及两个 toggle 的状态机接入关卡右上角 Quit Level / Music / SFX：Quit 弹窗以 25 Hz 四 tick 淡入淡出，Continue 仅关闭，单人 Back to menu 返回单人选关，按钮保留 on/off/hover 四态。`END-POP-01..06` 已用原版 popup 帧的独立标题、分数文本和按钮层替换简化 Game Over 窗口；胜利/失败/最终胜利的分支、25 Hz 淡入、分数滚动、Next/Restart/Back 已接入。C# 编译和生产入口回归待 Unity 执行；Unity Play Mode、原版画面/音频对照和原版胜负台词时序待用户执行。见 `Specs/CORNER_LEVEL_CONTROLS.md`、`Specs/GAME_END_POPUP.md`。
- [ ] P8-06 检查全部可取得的原版 UI symbol，禁止可用资源被程序绘制占位图替代。
- [~] P8-07 / HUD-01..04 / HUD-POS-01..03：左上小地图、左右团队总血量和逐关敌方头像已按 `Map.as`、`Team.as` 与根时间轴接入；BUG-HUD-POS-001 已恢复小地图 `(10,10)` 外框以及面板、血条、头像的原版注册点矩形。代码断言已加入，Unity Play Mode 像素及时序对照待用户执行。规格见 `Specs/BATTLE_HUD_MAP_TEAM_HEALTH.md`。

## P9 音乐和音效

- [~] P9-01 让 37 个 SFX 和 2 首音乐在 Editor 与 Player build 中均可加载。
- [ ] P9-02 建立 AS2 `playSound` 调用到 Unity 事件的完整映射表。
- [~] P9-03 还原菜单/游戏音乐切换、循环、暂停和音量。`MusicController.turnOffMusic` 的 Stop（非 Pause）和重新开启当前 menu/game 曲目的路径已接入；Music/SFX 保存开关和 hover 状态见 `Specs/CORNER_LEVEL_CONTROLS.md`。Unity Play Mode 音频对照待执行。
- [~] P9-04 还原角色选择、受击、墙撞、死亡、落水和环境碰撞音效。CHAR-AUD-01..04 已静态确认并接入角色物理碰撞 `hitwall` 节流；BUG-CHAR-THROW-AUD-001 已移除玩家/AI Throw Self 起始时错误播放的空投箱 `click`，实际空投箱 `click`/`icon_collect` 路径保留。选择与逐角色声音映射、Unity/原版听感对照待完成。见 `Specs/CHARACTER_COLLISION_AUDIO.md`、`Specs/CHARACTER_THROW_AUDIO.md`。
- [~] P9-05 还原每种武器各阶段音效、空投出现/拾取及 UI 点击音效。空投的 `chest_appear`（生成）、`click`（首次触碰开箱）、`icon_collect`（每件实际入库）已恢复并记录在 `Specs/AIR_DROP.md`；武器与其余 UI 声音仍按各模块验收。
- [ ] P9-06 核对同类音效的随机变体、并发、音量和触发 tick。

## P10 验证和发布

- [ ] P10-01 Level 1 初始化逐对象、逐 tile、逐库存快照比较。
- [ ] P10-02 Level 1 完整玩家回合逐 tick 状态/位置比较。
- [ ] P10-03 每种武器建立固定场景逐 tick 轨迹、伤害和结束条件测试。
- [ ] P10-04 27 个角色动画和声音逐项人工验收表。
- [ ] P10-05 空投统计和固定随机种子测试。
- [ ] P10-06 18 个现有关卡自动回归，无异常、无提前平局、无软锁。
- [ ] P10-07 Windows Player build 冒烟测试，确认无 `UnityEditor` 运行时依赖。
- [人工] P10-07A 退出当前 Play Mode，让 Unity 导入新增脚本和 1035 张原版帧；重新进入 Level 1 执行 `VALIDATION.md` 的检查。
- [阻塞] P10-08 对原版未提供的额外关卡做全关卡回归；见 `BLOCKERS.md`。
