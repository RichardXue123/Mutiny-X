# TURN：回合行动与菜单状态

状态：**原版静态证据和资源已检查；Unity 显示与面板时序已修复；自动验证 29/29 通过；受控的完整场景与原版画面对照仍未执行。**

范围：玩家回合选人、Throw Character（用户称跳跃）、通用武器行动与面板。特殊武器覆盖需各自补规格，不据此宣称十五种武器都一致。

基线：2026-09-15，Unity `bf2ea63`；原版指纹见 [总规范](../REPLICATION_STANDARD.md)。本文行号对应本次检查。

## 1. 原版证据

以下 S 根目录为 `Docs/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/`，A 为 `Docs/ReverseEngineering/Art/`。

| ID | 类型 | 定位 | 确认内容 |
| --- | --- | --- | --- |
| E01 | S | `throwgame/Team.as`，startTurn:220，赋值:254 | 新回合清空选人，canShoot/canThrow=true，weaponSelected/weaponLocked=false |
| E02 | S | `game/WeaponSelectButton.as`，hoverText:16，onEnterFrame:78 | 提示明确跳跃每回合一次且在武器之前；throw.disabled = !canThrow |
| E03 | S | 同文件 updateGraphic:47 | disabled / red_up / red_over / blue_up / blue_over；禁用时不用手形指针 |
| E04 | S | 同文件 onPress:106 | disabled 或面板 contentsActive=false 时拒绝；选择 throw 仅 unequip 并设置 weaponSelected |
| E05 | S | `throwgame/Character.as`，release:721、twang:742 | 有效角色投掷只消耗 canThrow；release 中 x/y 位移均小于 5 时提前返回 |
| E06 | S | `throwgame/Weapon.as`，release:38、twang:52 | 通用武器提交会同时将 canShoot、canThrow 置 false |
| E07 | S | `Swf/pcode/scripts/__Packages/com/nitrome/throwgame/Weapon.pcode`，2404/2412、2441/2449 | pcode 同样保存了两组 canShoot=false、canThrow=false 赋值 |
| E08 | S | `throwgame/Team.as`，isTurnComplete:299、continueTurn:307 | 存活选中角色仍有资格时继续回合；continueTurn 重新 select 同一角色 |
| E09 | S | `throwgame/Controller.as`，enterFrame:200，条件:231 附近 | inactivity 先递增并经过各模块更新；只有 >10 才判定继续或结束 |
| E10 | S | `game/WeaponSelectPanel.as`，onEnterFrame:12 | 人类已选人且未选择行动时淡入；每次 alpha 增加 25，达到 100 才启用内容；淡出时立即禁用内容 |
| E11 | A | `placements.csv`，父 1855、子 1839、实例 button_throw | throw 是面板内可独立变帧的嵌套子元素 |
| E12 | A | `frame-labels.csv`，symbol 1839 | disabled=1、red_up=10、red_over=19、blue_up=29、blue_over=38 |
| E13 | S | `game/WeaponSelectButton.as`，onEnterFrame:87 附近 | 普通武器格按库存是否为 0 禁用；有限数量显示两位数，无限另显 infinity |
| E14 | U | 本次用户报告 | 跳跃之前显示灰色；期望跳跃消耗后才变灰，同回合还能用武器 |

重要区分：E13 不是 `canShoot && 有库存` 这个单一 UI 公式。原版还有面板显隐、contentsActive 和回合阶段约束，必须联合验证，不能只替换一个布尔条件。

## 2. 状态转换合同

表中 T=canThrow、S=canShoot；“武器可选”还要求库存及面板交互条件。图像正常状态区分红蓝队与 hover。

| 规则 ID | 前置/触发 | T,S 的结果 | 面板/交互期望 | 证据 |
| --- | --- | --- | --- | --- |
| TURN-01 | 本队新回合开始 | true,true | 尚未选人，不能直接出现角色行动菜单 | E01,E10 |
| TURN-02 | 选中本队存活角色，未行动 | true,true | 面板淡入到可交互后，跳跃显示正常帧且可选 | E01–E04,E10 |
| TURN-03 | 点击跳跃选项，尚未实际投掷 | 不消耗 | 进入角色操作，面板淡出；不得因选择选项就标为 USED | E04 |
| TURN-04 | 实际提交有效角色投掷 | false,true | 运动/结算期间遵循原版输入限制，不能再次投掷 | E05,E08–E10 |
| TURN-05 | 角色投掷后满足继续回合条件，且角色存活 | false,true | 同一角色菜单返回，跳跃为 disabled，仍可选择有库存武器 | E02,E08,E13 |
| TURN-06 | 提交通用武器，不论之前是否跳跃 | false,false | 等待结算后结束该回合；不重新给予跳跃 | E06–E09 |
| TURN-07 | 再次轮到本队 | true,true | 消除上次禁用状态；选人后正常显示 | E01–E03 |
| TURN-08 | 面板未完全淡入、disabled 状态、或库存为 0 | 不应因被拒绝点击消耗 | 命中操作无效；是否显示 hover 提示按原版逐状态核对 | E03,E04,E10,E13 |
| TURN-09 | inactivity=9、10、11，期间无活动重置 | 按回合规则 | 在 >10 分支才结算；不能在等于 10 时提前结算 | E09 |
| TURN-10 | Character.release 位移 x/y 均 <5 | 本次 release 不消耗 | 该分支归位、清零速度、提前返回；不泛化到 twang 路径 | E05 |

TURN-09 应在同一更新观察点比较计数；不能把“释放后第 11 帧”作为所有场景的固定结束时间，因为活动和动画会重置 inactivity。

### 行为冲突记录 C01

旧 `VALIDATION.md` 第 3 步写“先用武器，结算后仍保留 Throw Self”。E02 的原文包含 `before you use a weapon`，E06/E07 同时消耗两项资格。此次修正文档为“先跳跃再武器；普通武器优先使用后不再跳跃”。用户“跳跃+武器”的描述与这个顺序兼容；用户未要求两个顺序都可用。

结论范围：通用 Weapon 路径静态确认，原版运行尚未验证；特殊武器逐类审计。如果指定版本的原版实测不同，重新查对应覆盖方法/版本，不能直接套用旧说明。

## 3. 缺陷卡 BUG-TURN-UI-001：未行动也显示灰色

- 期望：TURN-02 / E03 / E12，正常跳跃按钮按队伍显示对应 up/over 帧。
- 静态确认的当前表现来源：`Assets/Mutiny/Resources/UI/weapon_select_red.png` 已含灰色跳跃图像；检查了该图及 `button_throw_disabled.png`。
- Unity 入口：`Assets/Mutiny/Scripts/Presentation/MutinyGameHUD.cs` InitStyles:119–125 加载面板及 disabled 图；DrawBottomBar:265 附近画整张面板，272 附近在 CanThrow=true 时仅绘制空内容 GUI.Button。
- 关键原因：按钮样式来自 `GUIStyle.none`，可用分支没有正常/hover 纹理覆盖，仍显示底图中的灰色图像。不是只把 CanThrow 改成 true 就能修好的问题。
- 原版正常及悬停导出已存在：symbol 1839，帧 10/19（红）、29/38（蓝），1（禁用）。本次查看了红方正常帧及悬停帧。
- 修复要求：正确合成面板和动态子元素，使每个状态都显示对应资源；保留注册点、缩放和点击区域。可拆分底板与按钮，也可用完整正确的覆盖层，但必须证明没有底图残留或错位。
- 验证要求：同时记录实际 CanThrow、按钮图像和经生产输入路径的操作结果，覆盖正常→悬停→使用后禁用→下回合恢复。
- 修复：接入 symbol 1839 的 red_up/red_over/blue_up/blue_over 原版帧；HUD 根据 team/canThrow/hover 覆盖面板内的灰色底图。面板按 25 Hz、每 tick 25% 淡入淡出，完全展开后才启用点击，开始淡出时立即禁用。
- 自动复测：2026-09-15 在 Unity 6.6 菜单 `Mutiny > Parity > Validate Turn Action UI` 实际执行，29/29 assertions，Console 0 error。覆盖资源尺寸与加载、状态映射、淡入门控、生产输入方法及回合资格转换。
- 状态：**实现完成，自动验证通过；受控 Play Mode 前后截图与原版运行对照未执行。** 不排除尚未覆盖的特殊武器或场景状态问题。

## 3.1 缺陷卡 BUG-TURN-AI-001：切到 AI 回合后不行动

- 用户表现：玩家完成行动并换到敌方 AI 后，AI 回合停住。
- Unity 根因：`MutinyAIController.ExecuteAITurnRoutine` 固定等待后，只要 `CurrentPhase != TurnActive` 就永久退出；短暂的角色/武器/场景活动会把阶段切到 `ActionExecuting`，但阶段恢复后没有重启该协程。
- 修复：AI 在仍拥有当前回合时等待阶段恢复；只在回合切走、队伍失败、游戏结束或管理器失效时取消。`Update` watchdog 同时恢复因运行时初始化顺序而漏掉的回合开始事件。
- 回归入口：`MutinyAIController.ResolveTurnGate` 由生产协程直接使用；自动断言覆盖同一 AI 队 `ActionExecuting -> Wait`、`TurnActive -> Execute`、换队后 `Cancel`。
- 调试日志：关键路径使用 `[Mutiny:Turn]`、`[Mutiny:AI]`、`[Mutiny:Team]`、`[Mutiny:Input]`、`[Mutiny:Weapon]`、`[Mutiny:Chest]` 前缀；只记录阶段变化、选择、提交、阻塞源、发射、完成和宝箱事件。
- 2026-09-15 验证：运行时与编辑器 C# 程序集编译通过；Unity 场景回归由用户手动执行。

## 4. 最小验收用例

共同准备：固定原版 SWF/XML、原版播放器和 Unity 版本、550×400 逻辑视口；选可安全落地的角色与投掷轨迹，记录角色 ID、起始坐标、库存、输入 tick 和坐标。实际执行时填写具体值，不能只写“随便跳一下”。

以下是待执行用例；“预期”来自前述证据，“结果”不是通过声明。

| 用例 | 操作与独立预期 | 层/规则 | 结果 |
| --- | --- | --- | --- |
| TC01 | 新回合不输入，不能自动选人或因为空选择直接换队；选人后 T/S=true,true，正常图可见且可选 | 场景+原版对照；01/02 | 部分：生产 StartTurn 重置通过；场景/原版未执行 |
| TC02 | 在跳跃上移入/移出鼠标；红队显示 10/19 对应状态，蓝队显示 29/38，正常状态不灰 | 场景+视觉；02/08 | 部分：资源、尺寸、状态映射通过；受控场景截图/原版未执行 |
| TC03 | 选择跳跃但尚未提交，T/S 不变；面板淡出且内容不再接收选项点击 | 输入+对照；03 | 部分：生产选择方法和面板时序通过；场景/原版未执行 |
| TC04 | 驱动实际角色投掷并安全落地；T=false,S=true，同角色菜单返回，跳跃灰、有库存武器仍可选 | 三层；04/05 | 部分：生产提交方法只消耗 T、随后可选择武器通过；落地场景/原版未执行 |
| TC05 | TC04 后重复点击灰色跳跃，不产生第二次投掷，不消耗武器；随后正常使用武器 | 三层；05/08 | 部分：disabled 映射及跳跃后武器选择通过；场景重复点击/原版未执行 |
| TC06 | 新回合跳过跳跃直接提交通用武器，T/S=false,false；结算后不再给跳跃操作 | 三层；06/C01 | 未执行 |
| TC07 | TC04 后提交通用武器，待结算换队；再轮到本队，选人后跳跃图与资格均恢复 | 三层；06/07 | 部分：生产 StartTurn 恢复 T/S 且清除选人通过；完整流程/原版未执行 |
| TC08 | 在生产模拟入口构造无活动的计数边界 9/10/11，并比较原版同阶段；等于 10 时不得结算 | 逻辑+对照；09 | 部分：生产阈值方法 10=false、11=true 通过；完整场景/原版未执行 |
| TC09 | 有限库存 0/1/9/10 及无限库存，检查禁用、两位数量/无限标记；初始 XML 的 10 特殊值与获得十件有限库存分开设定 | 场景+对照；08/E13 | 未执行 |
| TC10 | 面板淡入未达到 contentsActive 时点击；不得选择/消耗。分别测初始 alpha=0 与隐藏后 alpha=-25 的恢复时序 | 三层；08/E10 | 部分：生产面板状态推进 6 项断言通过；真实点击/原版未执行 |
| TC11 | 分别走实际 dragging.release 与 twanging.twang 输入路径，测 release 位移各轴 <5、=5 及正常输入；核对消耗差异 | 三层；10 | 未执行 |
| TC12 | 跳跃中死亡/落水、投掷后拾取武器、武器/爆炸尚未结束、取消和尝试换角色 | 先补相关调用链证据和独立预期，再执行 | 待补证据 |

TC09 的库存编码细节需结合 Character.setWeapons / numberOfWeapon / weaponIsInfinite 补定位。TC12 不能只断言“不崩溃”就标通过。

右键取消是旧文档登记的 Unity 扩展，应另测且单列 EXT，不计作原版已有该输入的证据。原版取消按钮本身仍须核对其显示条件及事件。

## 5. 验收后的交付材料

每个已执行用例至少保存前置数据、输入记录、实际断言结果；视觉项保存相同状态的原版/Unity 画面，时序项保存对齐的事件或录像帧。记录第一处分歧并关联 BUG ID。

“逻辑布尔量正确但显示灰色”必须判失败；“正常图显示但不能点击”也必须判失败。本规格的所有未执行用例保持未执行，直到有实际证据。

## 6. 缺陷单 BUG-EDITOR-AI-001：回合结算时出现 Unity AI 云服务错误

- 用户表现：玩家行动结束、场景仍在结算时，Console 连续出现 `SettingsResult` / `PointsBalanceResult` 请求失败，容易被判断为敌方 AI 回合报错。
- 日志证据：堆栈只进入 `Unity.AI.Toolkit.Accounts.Services.Core.AccountApi`，请求目标为 `https://generators.ai.unity.com`；同一日志中的游戏流程最后停在 `[Mutiny:Turn] waiting ... blocker=character:Char_02_cabinBoy_T2/cabinBoy`，尚未出现 `turn switched ... to=T2(AI)`，因此该云服务错误不是游戏 AI 的调用栈。
- 工程来源：`Packages/manifest.json` 直接引用了未被游戏代码使用的 `com.unity.ai.assistant 2.19.0-pre.2`。2026-09-16 已从 manifest 和 packages lock 中移除；保留独立的 `com.unity.ai.inference`，因为它不是本次报错来源。
- 诊断补强：`MutinyTurnManager` 在同一阻塞源持续 50 tick（2 秒）时输出一次详细警告，之后每 125 tick（5 秒）重复一次。角色阻塞会记录 alive/drowned/inWater、像素位置和速度，以便区分正常爆炸结算与永不静止。
- 静态确认：原版 `Character.as:214-250` 以 `velocityX == 0`、`abs(velocityY) <= 0.2`、生命值展示及武器状态决定是否重置 inactivity；落水角色在 `Character.as:180-196` 被置为死亡。当前角色静止阈值和死亡角色排除规则与这些静态证据一致，本次不根据无关的编辑器联网错误修改物理规则。
- 自动验证：package JSON 解析通过；Runtime C# 使用独立中间目录编译通过（0 error，保留既有 `MutinyLevelTest.levelXml` 未赋值 warning）。
- 待运行验证：Unity 完成 package resolve 后清空 Console，重走玩家投掷加炸药流程；应不再出现 `Unity.AI.Toolkit` 云服务错误，并应看到 `turn switched ... to=T2(AI)`。若仍停住，保存新出现的 `[Mutiny:Turn] still waiting ...` 行作为下一条运行证据。
