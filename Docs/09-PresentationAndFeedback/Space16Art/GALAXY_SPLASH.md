# 落入星河动画（用户授权扩展）

后续用户追加海洋层级要求：涟漪排序299，当前在远星河上方、近表层300下方；旧版「全部星河之上」的视觉关系已明确被该要求替代。新版42项回归和自然入水录像见 [SPACE_PARALLAX.md](SPACE_PARALLAX.md)。

来源：用户在星河循环动画完成后要求「入水动画也做一下」。仅太空主题替换现有跨水线水花视觉，不修改伤害、溺水、投射物触水逻辑与音效。

| ID | 可观察行为及状态转换 | Unity入口 | 验收 |
| --- | --- | --- | --- |
| EXT-GALAXY-SPLASH-01 | 角色/已有水花投射物跨越原水线，触发青紫色能量冲击→扩散涟漪→星点消散，锚点为接触X和原水线Y | CheckSplashCrossing / SpawnSplash | 生产角色和真实武器物理步触发；检查资源、坐标及一次性 |
| EXT-GALAXY-SPLASH-02 | 使用6张64×64透明PNG，每张3个25Hz显示步，共18步/0.72秒后销毁；显示在星河之上；切关时随关卡销毁 | GalaxySplashEffect | 自然播放和生产帧推进验证首尾/销毁及切关隔离 |
| EXT-GALAXY-SPLASH-03 | 原关依旧播放原18帧水花；星河效果不改变角色死亡、阻尼、投射物行为或共享语音计数 | 原角色/武器入口 | 第16关与第6关对照；10:11、重力和Luck保持 |

## 完成口径

- 静态确认：六张64×64图均有真实透明通道、统一32 PPU和接触锚点(32,12)，Point过滤；透明像素数和SHA256见 [清单](SPLASH-MANIFEST.csv)。原版Splash资源未改。现有角色/武器跨线入口和水下规则沿用，扩展仅在SpawnSplash选择主题。
- 已实现：`Art/Space16/Splash/01..06.png`；`MutinySplashEffect` 可按太空主题加载六姿态、每姿态保持三步；继续使用原有25Hz播放器、18步隐藏并销毁。太空效果挂到当前关卡，切关立即失活并随根节点清理。
- 实际测试通过：Unity6000.6.0f1隔离工程GPU Play Mode，42/42；真实GM加载、角色生产物理穿线→死亡→一次水花、实际CherryBomb工厂发射→穿线且不因入水引爆、18步完整序列与结束隐藏、带活跃水花切关、第六关原sky2水花、自然Update六姿态与销毁。见 [运行记录](../../11-VerificationAndDebug/02-PlayModeValidation/Artifacts/GALAXY-SPLASH-20261001.txt)。测试只布置初始坐标与速度，没有手动设置跨线/溺水布尔量。
- 待运行验证：用户当前可见PIE中全武器实战、主观视觉与音画搭配。专项只覆盖角色和CherryBomb，其他使用同一SpawnSplash入口的武器为静态确认。
- 已知差异：新增星河冲击/涟漪/星点动画；角色水下旋转与死亡动作、原splash音效、浪潮武器主体维持已有表现。不会推进机器人语音轮播计数。

## 资源与复现

- 使用内置 `image_gen__imagegen`，透明底；[原始图](Sources/splash-generated.png)、[完整提示词](SPLASH-PROMPT.txt)。
- `Tools/Build-GalaxySplash.ps1`：仅做3×2分格、最近邻64px采样和第二行基线向下4px的机械注册点对齐；保留alpha，不重新绘制。
- `Tools/GalaxySplashVerification.cs`：隔离工程生产入口测试及相机录像。
- [实际角色坠落录像GIF](galaxy-splash.gif)：GPU渲染自然播放片段，36张画面以约22.5fps压制；用于展示，不是手动拼接角色与效果。
- PIE输入`enterlevel 16`，角色/已有水花武器穿过星河边界自动触发。
