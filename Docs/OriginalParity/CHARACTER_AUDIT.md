# 角色资源与动画审计

18 个 XML 共使用 27 种角色。每种角色在 SWF 中都有独立 35 帧 symbol；共同帧标签为 `static`（frame 1）和 `hit`（frame 15），但每种角色的逐帧画面不同。

| XML type | SWF symbol |
|---|---:|
| bluePirate | 1077 |
| tribe | 1086 |
| cabinBoy | 1097 |
| oldPirate | 1106 |
| rainbowBeard | 1117 |
| femalePirate | 1126 |
| blindPirate | 1137 |
| soldier | 1148 |
| bossGuy | 1157 |
| bossGuyZombie | 1166 |
| soldierCaptain | 1175 |
| blindPirateCaptain | 1186 |
| femalePirateCaptain | 1197 |
| rainbowBeardCaptain | 1206 |
| oldPirateCaptain | 1217 |
| cabinBoyCaptain | 1228 |
| tribeChief | 1237 |
| skeletonPirate | 1248 |
| skeletonPirateCaptain | 1259 |
| monkey | 1270 |
| crab | 1281 |
| shark | 1290 |
| squid | 1299 |
| parrot | 1310 |
| bluePirateCaptain | 1321 |
| redPirate | 1324 |
| redPirateCaptain | 1331 |

945 张原版角色帧现已放入 Unity 运行时资源。`MutinyCharacterAnimator` 以 25 Hz 播放各角色 frame 1–12 的 static 段：第 1、4、7、10 帧切换姿势且各保持 3 tick，第 13 帧动作直接跳回 static；hit 可见段为 frame 15–34，第 35 帧动作跳回 static。初始烘焙角色与运行时重建角色均从自身 `CharacterType` 初始化同一时间轴，并在受击飞行时保持原版 hit 起始姿态。角色飞行旋转已按原版每 tick `vx*3` 接入。

尚未签收：装备遮挡层、内部嵌套 MovieClip 的独立播放相位及逐角色声音仍需运行画面对比。当前静态 Preview 不再是唯一角色画面来源；陆地死亡、整体旋转和父级 static/hit 时间轴已有独立规格记录各自的验证状态。
