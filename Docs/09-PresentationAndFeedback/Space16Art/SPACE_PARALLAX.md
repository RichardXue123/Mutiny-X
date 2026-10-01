# 太空背景独立分层与视差（用户授权修正）

来源：用户要求底图不含恒星/行星，它们应像山脉/云层一样分层并随镜头产生层次感。先登记规格，再实现。

原版参考：`Docs/10-OriginalEvidence/Artifacts/ReverseEngineering/Swf/deobfuscated/scripts/__Packages/com/nitrome/throwgame/Water.as::advance`，云与山分层、横向以0.2/0.25/0.3移动并循环，纵向以0.2/0.3/0.4/0.5移动。本扩展借用分层机制，太空布局/数值由本次设计定义，不声称复刻原版天体。

| ID | 可观察行为及状态转换 | Unity入口 | 验收 |
| --- | --- | --- | --- |
| FIX-SPACE-PARALLAX-01 | 底图仅含深空色调与淡星云，不再含行星、恒星或亮星点；亮星点、恒星、小行星、环形行星为独立透明资源与对象 | SpaceBackground / SpaceParallaxLayer | 资源alpha与人工图像检查；实际GM16检查层级资源 |
| FIX-SPACE-PARALLAX-02 | 镜头右移时背景各层相对屏幕左移；星点0.08、恒星0.12、小行星0.20、环形行星0.35倍；纵向同样具有独立视差 | RefreshForCamera | 经实际相机刷新核对位移方向、倍率和不同层相对位移；录制平移预览 |
| FIX-SPACE-PARALLAX-03 | 星点双轴循环，天体横向稀疏循环；只在视野之外增减副本；大地图全范围和宽视口不露背景空隙、不发生可见回卷跳变 | layer repeat bounds | 地图四角、超宽相机及周期附近实际渲染/覆盖检查 |
| FIX-SPACE-PARALLAX-04 | Scene保存重开/既有烘焙背景自动补全新层；GM重入不重复层；其他关卡维持原云山；星河动画、入水动画、坠落边界与关卡状态保留 | Initialize/OnEnable/GM | 生产Scene保存重开与旧场景夹具、GM16→6→双人16→16、星河时钟与生产角色入水 |
| FIX-SPACE-PARALLAX-05 | 用户追加：星河与海洋具有同样层级和远近关系。远星河在船体/角色后方；近星河表层为排序300，入水涟漪299、角色20起；近表层以世界坐标跟随原水线和地形，镜头移动不拉伸或贴屏，水线以下遮挡角色，波峰上方alpha透明 | SpaceBackground / GalaxySurface repeated sprites | 核对生产排序、相机移动前后表层世界坐标/尺寸、真实角色入水GPU遮挡；保留y=34 |

旧sky资源和原生成图作为历史记录保留，运行时使用新分层资源；生成脚本与预览脚本已同步更新，避免重新生成旧版平面背景。

星河表层采用独立透明边缘贴图，固定12世界单位宽的交替镜像铺贴保证连接处连续，底图原银河改作远层；统一25Hz动画驱动表层，而非拉伸同一张图到整个视口。排序和跟随关系参照现有海洋生产管线（原版证据见上），属于授权视觉替换。

## 最终分层配置

| 层 | 排序 | 屏幕相对位移/镜头位移(X,Y) | 固定世界尺寸 | 循环 |
| --- | --- | --- | --- | --- |
| 深空底色 | -100 | 0,0 | 随视口覆盖 | 无天体/亮星点 |
| 远星点 | -90 | -0.08,-0.08 | 24×16 | 双轴 |
| 恒星 | -80 | -0.12,-0.10 | 2.5×2.5 | 横向32单位 |
| 小行星/月球 | -75 | -0.20,-0.18 | 2.5×2.5 | 横向34单位 |
| 环形行星 | -70 | -0.35,-0.28 | 7.5×6 | 横向32单位 |
| 远星河 | -20 | -0.30,-1 | 24×8 | 横向交替镜像 |
| 飞船/角色 | -10/10/20起 | -1,-1 | 既有 | 既有 |
| 入水涟漪 | 299 | -1,-1 | 既有64px | 一次性 |
| 近星河表层 | 300 | -1,-1 | 12×6.75 | 横向交替镜像 |

近星河亮线：素材y=32，放置顶部为水线+1世界单位，故亮线落在原水线。图像顶端透明，下部在材质中保证完全遮挡；表层后的天体/角色只在透明区域可见。近层不使用视口拉伸，宽视口按需增加池化副本；可见副本均连续保持世界坐标。

## 完成口径

- 静态确认：新底图没有恒星、行星及亮星点；独立透明资源和alpha数量见 [PARALLAX-MANIFEST.csv](PARALLAX-MANIFEST.csv)。固定层级、parallax参数和原Water.as分层参考已登记。新图均Point/32PPU/无压缩；原关资源未改。
- 已实现：`MutinySpaceParallaxLayer`复用Renderer池与双轴/横向重复；`MutinySpaceBackground`迁移旧平面组件、配置六个独立平面；近表层复用25Hz动画并正确覆盖水线以下物体。素材裁切脚本、静态布局审阅脚本、历史回归的资源/排序断言同步更新。
- 实际测试通过：Unity6000.6.0f1隔离工程真实GPU，分层专项82/82。覆盖旧版真实保存Scene迁移、新Scene保存重开无重复、Main主菜单、GM进关/切关、每层实际摄像机相对位移、地图四角和3.2宽高比覆盖、世界尺寸固定、回卷附近连续位置、实际角色穿线与299/300层级。像素对照证明水线下角色被表层完全遮挡；仅关掉近表层时，同一角色又能被渲染。记录：[SPACE-PARALLAX-20261001.txt](../../11-VerificationAndDebug/02-PlayModeValidation/Artifacts/SPACE-PARALLAX-20261001.txt)。
- 实际回归通过：新版星河循环19/19、入水动画42/42，合计143项；分别见 [循环](../../11-VerificationAndDebug/02-PlayModeValidation/Artifacts/GALAXY-ANIMATION-LAYERED-20261001.txt) 与 [入水](../../11-VerificationAndDebug/02-PlayModeValidation/Artifacts/GALAXY-SPLASH-LAYERED-20261001.txt)。动态表层像素改变、船体区域不变；自然播放全部100步、6个水花姿态、结束销毁通过。其余34张XML、21角色、712格、0.5重力、Luck50配置未由本次改变。
- 待运行验证：主工程可见PIE的长时间手动镜头与完整对局观感。专项直接移动真实Camera并调用生产Refresh渲染，未伪称鼠标/手柄输入验收。
- 已知差异：宇宙美术为授权扩展，远处天体稀疏重复；近星河用镜像拼接而非原海水的980px周期；新增天体数值并非原版云山倍率逐项复制。浪潮武器主体与splash音效仍保留原版。

## 素材与预览

内置imagegen生成4张源图，机械切分为6项资源；[完整提示词](PARALLAX-PROMPTS.json)，原图位于Sources下`deep_space-generated`、`celestials-generated`、`starfield-generated`、`galaxy_surface-generated`。`Tools/Build-SpaceParallaxArt.ps1`保留alpha并最近邻缩放，不用程序补画。生产素材在`Assets/Mutiny/Resources/Art/Space16/Parallax/`。

- [相机平移视差GIF](parallax-camera.gif)：真实Unity相机26→42→26，60帧；展示视差，捕获期间不推进玩法。
- [星河前后层GIF](galaxy-depth.gif)：真实Unity相机横向/纵向移动，40帧；显示船体与近表层同速、远星河慢速。
- [全景](parallax-overview.png)、[自然入水与遮挡GIF](galaxy-splash-layered.gif)。入水录像使用真实物理与Update。

PIE重新`enterlevel 16`即可；旧烘焙Scene的SpaceBackground第一次刷新自动替换旧sky并补全独立层，旧flat银河Renderer失活。
