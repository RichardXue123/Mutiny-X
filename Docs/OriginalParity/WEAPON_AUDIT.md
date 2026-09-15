# 武器逐项审计

证据源为反编译的 `Weapon.as`、`Solid.as` 和各武器 AS2 类。状态表示当前 Unity 与原版的接近程度，不表示类是否存在。

| 武器 | 原版关键流程 | Unity 审计结果 | 当前处理 |
|---|---|---|---|
| Cherry Bomb | 9 px extent；首次接触即产生 size 80 / damage 40 爆炸；飞行持续烟迹 | 主爆炸参数已匹配；烟迹、出水/越界细节和完整动画未匹配 | 继续 |
| Dynamite | 11 px；friction 1.7；完全静止时 size 250 / damage 70；水下只切换 unlit；持续烟迹 | 核心停止爆炸已存在；烟迹和时间轴仍缺 | 继续 |
| Banana | bounce 0.8、friction 0.5、max force 30；每次接触播放 bounce；玩家下一次左键或静止时引爆；AI 近距离触发 | 已删除错误的“四次反弹”规则，恢复再次左键/静止/AI 20 px 触发及音效；待逐 tick 验证 | 已修核心 |
| Boulder | extent 31、weight 1.5、friction 0.25；release 后速度减半；滚动为 vx×2.5；AABB 内每 tick 造成 `abs(vx)*1.5` 并传递同向 vx；静止后渐隐 | 已恢复上述数值与逐 tick 处理；白化滤镜暂以 alpha 表示 | 已修核心 |
| Cannon | 先放置炮体，再拖炮栓/炮口决定 Cannonball；炮弹追踪镜头；发射音为 `cannon explosion` | 没有 `MutinyCannon`；已移除错误的 Cherry Bomb fallback并把 Cannonball weight 修为 0，但炮体流程仍缺 | 严重差异，下一批 |
| Gunpowder Barrel | `BoxWeapon` 点击合法位置放置两个；不走抛物线；爆炸 size 150 / damage 30；可连锁 | 当前当作普通抛射物，交互和数量均错误 | 严重差异 |
| Mine | 投掷后静止完成本次武器动作，随后独立武装；感应后 60 tick 倒计时并按表蜂鸣 | 当前接近轮廓，但 proximity、蜂鸣时刻、武装动画和回合独立性未对齐 | 继续 |
| Parachute Bomb | max force 30；vy>-10 时开伞；vy>1 时减 2 并限制到 1，vx×0.95；按住鼠标每 tick 加减 vx 0.2；每 12 tick 播放 fan | 已恢复数值、开伞阈值、鼠标风控和 fan 时序；开伞/烟迹动画仍缺 | 已修核心 |
| Pieces of Eight | 同一个武器连续投掷 **8 次**；每次碰撞 size 50 / damage 25；每发后锁定该武器并重新瞄准 | 当前只实际投一发且字段错误写为 3 | 严重差异 |
| Rum Bottle | max force 30；接触后 size 30 / damage 10，并向两侧创建 SweepingFlame 链 | 当前只产生普通爆炸，缺少两侧扫动火焰 | 严重差异 |
| Seagull | 先点击高度，从 x=-300 以 vx=10 飞行；之后每次点击投下一枚 weight 1 的 shot；随机 `poop1..3`；飞出地图才结束 | 速度已修为 10；当前仍是单目标点、一次投弹 | 严重差异 |
| Tidal Wave | 点击后固定从 x=-550、水面处开始，vx=20；对水面上方 300 px 内、波中心 ±150 px 的角色每 tick 伤害 5；按天空色选择动画 | 已接入单击启动及原版速度/范围/逐 tick 伤害；天空色动画仍缺 | 已修核心 |
| Voodoo Doll | 先点选目标角色再拉动投掷；命中后先展示本体 10 tick，再跟目标 10 tick，然后把投掷速度赋给目标 | 当前目标选择没有接入 UI，阶段时序不完整 | 严重差异 |
| Wooden Crate | `BoxWeapon`；点击合法位置连续放置三个；检查 terrain、box、chest、character 堆叠 | 当前当作普通抛射物，缺合法位置和连续放置 | 严重差异 |
| Anchor | 点击任意 x 后从 y=-200 落下，vy=40；extent 为左右48/上96/下0；落地范围伤害60，停留30 tick、淡出10 tick | 已接入单击横坐标并恢复上述核心参数、伤害、停留和音效；白化/落地时间轴仍缺 | 已修核心 |

## 已确认的公共差异

- 原版 `Weapon.release/twang` 只将 `owner.canShoot=false`，不会消耗 `canThrow`。Unity 已修复，因此可以先用武器再 Throw Self，也可以先 Throw Self 再用武器。
- 原版预测线使用所选武器的 `twangMaxForce` 和 `weight`。Unity 已把 Banana、Parachute Bomb、Rum Bottle 的 max force 30 和 Boulder 的 weight 1.5 接入预测。
- `MutinyWeaponFactory` 对未知/尚未接入的特殊武器静默创建 Cherry Bomb 是危险行为。Cannon 和所有点击放置类完成专用交互前，不可把通用抛射结果视为实现完成。
