# 星河循环动画（用户授权扩展）

后续分层修订：当前动画驱动`Space_GalaxySurface`的固定世界尺寸重复片，远星河独立视差；本页原平面GIF为历史记录。新版19项回归和前后关系见 [SPACE_PARALLAX.md](SPACE_PARALLAX.md)。

来源：用户要求「星河也要和海水一样有动画」。这是 EXT 扩展，非原版星河规则；现有 Unity 海面采用 0.04 秒显示步长作为工程节奏参考，不据此声称原版一致性。

| ID | 可观察行为及状态转换 | Unity 入口 | 验收与当前状态 |
| --- | --- | --- | --- |
| EXT-GALAXY-ANIM-01 | 加载太空关后，原星河图产生横向流动的亮带、轻微起伏和星点闪烁；25 Hz，100步/4秒周期，最后一步自然衔接首步 | SpaceBackground + GalaxyFlow shader | 自然 Play 观察全部100步及回卷；GPU实测变化95214像素，通过 |
| EXT-GALAXY-ANIM-02 | Scene 加载/保存/重开和旧版本烘焙的 SpaceBackground 均能自动获得动画；切走或销毁停止编辑器回调；重新入关从首步开始 | Initialize / OnEnable / OnDisable | 新Scene保存重开自然推进且不产生dirty；GM切关和重置通过。旧版本烘焙场景兼容路径为静态确认，未另建旧文件运行夹具 |
| EXT-GALAXY-ANIM-03 | 动画只改变星河采样与亮度，背景矩形、y=34坠落边界、船体、行星、人数、重力和Luck不变；单人6/双人16仍使用原海面 | material property block / GM | GPU船体区域变化0像素；边界、0.5重力、10:11、Luck50及原关十帧海面保留检查通过。行星材质未接动画，静态确认 |

实现方案：在现有 imagegen 星河 PNG 上使用像素对齐的周期 UV 位移及局部亮度变化，无需重新生成或修改位图。位移在图像四边逐渐归零以免露边。运行时使用每关独立累计器及 MaterialPropertyBlock，共享只读材质。编辑器用专门时间驱动显示预览，不写入场景动画相位；PIE 用 Time.deltaTime 与现有海面一致。

## 完成口径

- 静态确认：25 Hz、100步周期；着色器仅作用于 Space_galaxy，正弦参数均为整周期；四边采样位移归零。原始PNG字节未变；海面生产代码未改。
- 已实现：`MutinySpaceBackground` 的关卡独立时钟、编辑器预览驱动、材质自动绑定；`GalaxyFlow.shader` 与 `GalaxyAnimated.mat`。没有生成额外纹理帧或逐帧分配材质。
- 实际测试通过：Unity 6000.6.0f1，隔离工程 GPU batchmode，19/19；真实 Scene 工具保存重开、自然 Play、Main主菜单、GM进入和切关，实测全部100步并完成回卷。自然播放期间录制52张GPU画面，两个时刻星河变化95214像素、船体变化0像素。报告：[GALAXY-ANIMATION-20261001.txt](../../11-VerificationAndDebug/02-PlayModeValidation/Artifacts/GALAXY-ANIMATION-20261001.txt)；测试源：`Tools/GalaxyAnimationVerification.cs`。
- 预览：[实际运行录像GIF](galaxy-animation.gif)，约12.5 fps采样压制，展示自然播放片段；GIF剪辑首尾不用于判定运行时循环接缝。
- 待运行验证：用户当前PIE窗口的整体观感、旧版本烘焙场景单独回归、长时间对战。着色器循环相位的数学连续性已静态确认，未逐对测量所有相邻帧的像素差。
- 已知差异：这是授权太空动画，沿用已有星河图做周期形变和闪烁；没有替换成原海水逐帧图。本次不改落水/浪潮动态效果。

最初测试因Unity启动时再次重载域而丢失测试回调，已补测试恢复逻辑后重跑通过；该中断不作为成功测试登记。
