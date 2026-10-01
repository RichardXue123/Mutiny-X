# Robot / RobotCaptain 美术资源（2026-10-01）

用户授权的新增角色，不属于原版 Flash 资源。仅制作角色美术与动画资源；声音由用户另行完成。

## 制作前规格

| ID | 可观察行为 | 来源 | Unity 入口 | 验收 | 实际结果 |
| --- | --- | --- | --- | --- | --- |
| EXT-ROBOT-ART-01 | RobotCaptain 金色头盔，Robot 银色头盔；小尺寸、透明背景、有限色阶和像素轮廓与现有角色相容 | 用户 2026-10-01 指定；现有角色 PNG 为画风参考 | Art/Characters/Preview、Animations | 原尺寸与放大图检查，透明度及资源尺寸检查 | 已生成；70 帧尺寸/透明度/边界检查通过，参考对比图及浏览器预览已检查 |
| EXT-ROBOT-ANI-01 | 1..12 为待机循环；统一画布、稳定脚底及身体注册点 | 用户授权；原版帧接口见本目录 README | MutinyCharacterAnimator.LoadFrames / Initialize | 加载完整帧组，运行生产待机入口，检查帧序与预览循环 | 已制作；生产加载器、初始化与 12 tick 循环 Play Mode 通过 |
| EXT-ROBOT-ANI-02 | 15..34 为受击姿势及恢复段，末帧衔接待机；13、14、35 保留原格式槽位 | 同上 | MutinyCharacterAnimator.PlayHit | 生产受击入口播放并回到待机；可见帧无空帧 | 已制作；PlayHit、20 tick 恢复与再次受击 Play Mode 通过 |

形象以用户修正为准：RobotCaptain 是大面积黑色圆弧面罩、金色金属外壳；Robot 是小面积黑色横向面罩、银色金属外壳。上一版把造型与配色对应反了，已保存在 `RobotAssets/Previous-v1/`，不作为当前交付。

## 第二版修正规格

`EXT-ROBOT-ART-02`：由用户本次纠正授权，以原大面积黑面罩图集生成金色 Captain，以原横向小面罩图集生成银色 Robot；保持待机与受击姿势、透明格式和帧接口。验收入口为两角色的生成图集、运行帧和生产动画加载器。新增回归检查每个可见帧的金/银色归属，并比较中立头盔区域的黑色面积比例，防止再次颠倒。第二版已重新生成并导出：70 帧静态检查与金银配色、黑面罩面积比例回归通过；生产动画与导入 Play Mode 的 215 项断言重新通过。报告已更新为第二版。

## 状态

- 静态确认：现有加载器使用 35 张编号 PNG，25 Hz，待机 1..12、受击 15..34。现有 redPirate/cabinBoyCaptain 的 idle 有三个不同图像，hit 15..34 为相同图像。
- 已实现：两套 AI 绘制角色、各 35 张运行 PNG、各 1 张 Preview、锚点注册、GIF 与交互 HTML 预览。仅运行代码改动为两个角色锚点注册。
- 实际测试通过：70/70 PNG 为 32×36、二值透明度、最多 16 色；可见帧均有透明边距且脚底在 y=31；13/14 为空白接口槽位。Unity 6000.6.0f1 隔离 batchmode/nographics Play Mode 的 215 项断言通过，覆盖生产资源加载、纹理导入、PPU/pivot、关卡 Preview 入口、待机与受击帧序。见 [报告](RobotAssets/UNITY-VERIFICATION.txt) 与 [逐帧清单](RobotAssets/MANIFEST.csv)。放大对比图及浏览器的原尺寸、待机、受击预览已查看。
- 待运行验证：在实际战斗关卡中的位置与旋转观感、受击飞行过程、移动端显示，以及用户对角色造型的最终审阅。无图形 Play Mode 通过不代表这些场景已经人工验收。
- 已知差异：新角色为授权扩展；原图角色尺寸各异，本次统一画布 32×36，Captain 可见中立高度 26 像素、Robot 24 像素。受击新增三阶段恢复姿势。未加入关卡，也未定义新的声音或玩法。

## 交付与复现

- 运行帧：`Assets/Mutiny/Resources/Art/Characters/Animations/RobotCaptain/1.png..35.png` 及 `Robot/1.png..35.png`。
- 静态预览：`Assets/Mutiny/Resources/Art/Characters/Preview/RobotCaptain.png`、`Robot.png`。
- 两者使用 32 PPU、Point、无 mipmaps、不压缩、透明 PNG；注册点为左下坐标 `(16,15)`，所有帧同画布。角色类型标识保留用户大小写。
- 帧映射：1..3 中立 A，4..6 下沉 B，7..9 A，10..12 上抬 C；13/14 透明；15..20 受击峰值，21..25 初期恢复，26..30 后期恢复，31..35 精确复用 A。生成图集中两个重复中立姿势不用于运行，以避免细节漂移。
- [交互预览](RobotAssets/preview.html)、[Captain GIF](RobotAssets/RobotCaptain.gif)、[Robot GIF](RobotAssets/Robot.gif)、[与原角色比较](RobotAssets/comparison.png)。GIF 连播 12 帧待机和 20 帧受击，用于审阅；运行时由原动画器的事件决定切换。
- [第二版生成提示词](RobotAssets/PROMPTS-v2.md)；`RobotAssets/Sources/` 保留内置 imagegen 的原始透明图集。未使用 API/CLI 图片生成。
- `Tools/Build-RobotSprites.ps1` 可由 Windows PowerShell 或 PowerShell 7 调用，负责机械切帧、统一缩放和 16 色调色板、透明连通主体提取、脚底注册及 PNG/meta 输出；已有 GUID 不重建。`Tools/Build-RobotPreview.ps1` 生成自包含 HTML。
- `Tools/RobotAssetVerification.cs` 为隔离工程验证入口，复制到隔离工程的 `Assets/Editor/` 后执行 `Mutiny.Verification.Editor.RobotAssetVerification.RunBatch`；它调用生产动画方法，不替代动画规则。
