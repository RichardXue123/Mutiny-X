# 第十六关太空美术（用户授权扩展）

> 当前地图为[第四版悬浮扁平货船](../../02-LevelAndWorld/04-ObjectsAndSpawns/LEVEL_1_16_FLAT_FREIGHTER.md)：80×27、5 对 8、水线 y=24；左船与小行星带保持，右侧金属敌舰船腹悬浮 3 格。下述 712 格、10:11、y=34 和旧相机图/测试属于三舰初稿历史基线。分层机制仍沿用，当前地图验收单独记录。

日期：2026-10-01。来源：用户要求新建太空船体边缘、内部、炮位、行星恒星和星河资源；不属于原版一致性结论。

| ID | 可观察行为 / 状态转换 | Unity 入口 | 验收 |
| --- | --- | --- | --- |
| EXT-SPACE-ART-01 | level_1_16 的 visualTheme="space" 经 XML 解析和 BuildLevel 后使用新的 32px 金属瓦片；边缘按相邻实体格选择，背景舱壁较暗，炮位使用新资源 | XML parser / LevelBuilder / SpaceVisuals | 真实 GM enterlevel 16 后检查所有船体资源、边缘、炮位和数量 |
| EXT-SPACE-ART-02 | 太空主题使用不含天体的深空底图，星点/恒星/月球/环形行星为独立视差层；镜头移动持续覆盖视野 | SpaceBackground / SpaceParallaxLayer | 当前修订见FIX-SPACE-PARALLAX；旧平面背景68项验收为历史基线，不代表分层通过 |
| EXT-SPACE-ART-03 | 原海面以紫蓝星河替代；坠落边界仍在原位置；编辑器和 PIE 一致。后续新增循环动画见 EXT-GALAXY-ANIM | BuildWater / EnsureRuntimeWater / SpaceBackground | 实际构建、重新加载；不产生原海面 renderer |
| EXT-SPACE-ART-04 | 无主题的单人及双人关卡保持原美术；第十六关规模、碰撞、10:11、0.5 重力、Luck50 不变 | production GM entry | 16→6→双人16→16 回归 |

美术：内置 imagegen 生成；原始生成图存 Sources，工程 PNG 使用 Point、32 PPU、无压缩和无 mipmap。规格先登记，运行结果另记，不把待验证写作通过。

## 完成口径

- 静态确认：原 tile 使用 32×32、左上锚点、32 PPU；原版 XML 和原始导出贴图未改写；两份第十六关 XML 一致。新增主题仅改显示映射。
- 已实现：8张32×32船体PNG、6张入水姿态、星河及六项独立分层资源。旧sky.png含天体，仅保留为历史素材，生产已切换Parallax/deep_space。全部Point、无mipmap、无压缩；内置imagegen生成，脚本仅机械裁切及最近邻转换。当前分层布局和前景水线见专项规格。
- 实际测试通过：Unity 6000.6.0f1 隔离工程，68/68。生产 Scene 加载→保存→重开→自然 Play→GM16→单人6→双人16→GM16。712实体格/碰撞体、10:11、0.5重力、Luck50和语音计数重置均核对；主菜单正常；旧关保留原贴图和十帧海面。真实 GPU 相机渲染三张预览并完成目视检查。见 [运行记录](../../11-VerificationAndDebug/02-PlayModeValidation/Artifacts/SPACE16-ART-20261001.txt)。
- 待运行验证：用户当前可见 PIE 窗口的长时间镜头移动、完整对局及主观美术验收。本次相机渲染不等于完成整局游戏。
- 已知差异：授权太空主题扩展；落水伤害与浪潮武器规则仍沿用已有玩法。天体独立视差、远星河在船体后、近星河在角色和入水涟漪前，近表层固定世界尺寸及y=34水线。浪潮武器主体仍为原海浪。

## 文件与使用

- **当前版本**：[太空视差与星河前后层](SPACE_PARALLAX.md)，[相机移动GIF](parallax-camera.gif)、[星河远近GIF](galaxy-depth.gif)、[全关卡相机图](parallax-overview.png)、[新版入水遮挡GIF](galaxy-splash-layered.gif)。

- [星河循环动画](GALAXY_ANIMATION.md)：沿用本页星河图，通过像素级波动和星点亮度变化实现 25 Hz、4秒周期动画；行星与船体保持静态。
- [落入星河动画](GALAXY_SPLASH.md)：六张透明像素图，冲击、涟漪和星点消散；接在既有角色/武器水线穿越入口。

- 工程素材：`Assets/Mutiny/Resources/Art/Space16/`，SHA256 见 [资源清单](ASSET-MANIFEST.csv)。
- 原始生成图：[Sources](Sources/)。[完整提示词](PROMPTS.json)，工具：内置 `image_gen__imagegen`。
- 历史平面背景相机截图：[全关卡](overview.png)、[船体近景](deck-detail.png)、[星河近景](galaxy-detail.png)，不代表当前分层版。当前截图见上；相机审阅改变位置/范围，不改变地形，也没有合成补画；不含OnGUI菜单。
- 在 PIE 中输入 `enterlevel 16`；若 Scene 中已烘焙旧版本，使用 `Mutiny → Levels → Load Level to Active Scene...` 再加载单人16。Main 的 Play 仍先进入主菜单。

初次资源导入曾暴露 TextureShape 默认值不适合 Sprite 的问题；导入器现显式设为 Texture2D，并复跑上述全部检查通过。旧失败尝试没有登记为通过。
