# WPN-07 Mine（地雷）行为规格

原版证据：`Mine.as`、`Weapon.as`、`Solid.as`、DefineSprite_1024；SWF SHA256 `c034d2cfa50272a1aac93ad75114b542ca5993dd7b70fa8901aa195de7820586`。

| ID | 原版结论 |
|---|---|
| W07-S-01 | extent 四向=14、friction=1.5、hitsBoxes=true、dragRange=180、`limitedToTurn=false`；初始 frame 为 `in_throw`。 |
| W07-S-02 | 飞行时每 tick `checkForProximity`；未 active 前先递减 ignoreTime，只有第 10 tick 后才可检测。角色中心脚底 `(char.x,char.y+bottomExtent)` 到 mine 距离严格 `<60`，且角色必须在运动或正在 twang。 |
| W07-S-03 | 条件满足时仅 `active=true`、播放 `warn`，不立即爆炸；active 后 countdown 从 60 每 tick减一、重置 inactivity；蜂鸣发生于经过 tick `0,15,30,38,45,49,53,55,57,59`。 |
| W07-S-04 | countdown 到 0 创建 `Explosion(x,y,250,70,owner)`、播放 `pop`、移出 mines、destroy。静止阈值 vx=0 且 abs(vy)<.2 时登记至 mines 并播放 `arm`；不应把普通静止直接当爆炸。 |
| W07-A-01 | DefineSprite 1024 的 30 帧已导入 `Resources/Art/Weapons/Mine/1..30.png`；`mine_beep.wav` 已存在。 |

规则：WPN-07-INT-01 通用拖拽释放并一次消耗行动；WPN-07-EFF-01 静止后跨回合存续；WPN-07-EFF-02 移动角色严格 `<60` 触发 warn/60 tick；WPN-07-AUD-01 十个 beep tick 与 pop；WPN-07-ANI-01 in_throw→arm→warn→explode。

用例：W07-T-01 第 9/10 tick ignore 边界；W07-T-02 静止角色、移动角色与 60px 临界；W07-T-03 十个蜂鸣 tick；W07-T-04 60 tick、250/70/owner 爆炸和跨回合；W07-T-05 terrain/box/水。全部 Unity 与原版运行对照待执行。
