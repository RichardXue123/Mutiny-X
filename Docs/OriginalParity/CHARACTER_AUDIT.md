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

945 张原版角色帧现已放入 Unity 运行时资源。`MutinyCharacterAnimator` 播放各角色 frame 1–14 的 idle 和 frame 15–35 的 hit 段，并在受击飞行时保持原版 hit 起始姿态。角色飞行旋转已按原版每 tick `vx*3` 接入。

尚未签收：原版死亡使用独立 `deadCharacter` symbol；落地后的角度归正、hit 时间轴 action、装备遮挡层及逐角色声音仍需运行画面对比。当前静态 Preview 不再是唯一角色画面来源。

