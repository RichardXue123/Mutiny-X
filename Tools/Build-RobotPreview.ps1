param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = 'Stop'
$taskReview = Join-Path $ProjectRoot 'Docs/09-PresentationAndFeedback/05-CharacterAnimation/RobotAssets'
$taskPayload = [ordered]@{}
foreach ($taskType in 'RobotCaptain','Robot') {
    $taskPayload[$taskType] = @(1..35 | ForEach-Object {
        $taskPath = Join-Path $ProjectRoot ('Assets/Mutiny/Resources/Art/Characters/Animations/'+$taskType+'/'+$_+'.png')
        'data:image/png;base64,' + [Convert]::ToBase64String([IO.File]::ReadAllBytes($taskPath))
    })
}
$taskReference = Join-Path $ProjectRoot 'Assets/Mutiny/Resources/Art/Characters/Preview/redPirate.png'
$taskReferenceData = 'data:image/png;base64,' + [Convert]::ToBase64String([IO.File]::ReadAllBytes($taskReference))
$taskHtml = @'
<!doctype html><html lang="zh-CN"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Robot / RobotCaptain 动画预览</title>
<style>
*{box-sizing:border-box}body{margin:0;background:#151a22;color:#eaf0f8;font:16px system-ui,sans-serif;padding:36px;max-width:1150px;margin:auto}h1{font-size:27px;margin:0 0 8px}p{color:#aebaca;line-height:1.7}.grid{display:grid;grid-template-columns:repeat(2,1fr);gap:24px;margin-top:26px}.card{background:#212936;border:1px solid #394456;border-radius:16px;padding:24px}.stage{height:310px;display:flex;justify-content:center;align-items:center;background-color:#303947;background-image:linear-gradient(45deg,#374352 25%,transparent 25%),linear-gradient(-45deg,#374352 25%,transparent 25%),linear-gradient(45deg,transparent 75%,#374352 75%),linear-gradient(-45deg,transparent 75%,#374352 75%);background-size:24px 24px;background-position:0 0,0 12px,12px -12px,-12px 0;border-radius:10px}.sprite{width:256px;height:288px;image-rendering:pixelated}.native img{image-rendering:pixelated;vertical-align:bottom;margin:0 15px}.native{display:flex;align-items:end;height:62px;margin:14px 0;color:#9eafc5;font-size:13px}button,select{background:#3a495e;border:1px solid #647289;color:white;border-radius:7px;padding:9px 14px;margin-right:8px;cursor:pointer}button.active{background:#355d8c}.state{font-variant-numeric:tabular-nums;color:#c2cee0;margin:15px 0 0;font-size:14px}.gold{color:#f5cb69}.silver{color:#cbddec}.controls{margin:22px 0}footer{color:#93a3b8;font-size:13px;line-height:1.8;margin-top:22px}@media(max-width:720px){body{padding:18px}.grid{grid-template-columns:1fr}}
</style>
<h1>机器人角色动画</h1><p>RobotCaptain · 金色头盔　/　Robot · 银色头盔<br>透明像素素材，包含待机循环和受击恢复。下方小图为游戏原尺寸，旁边放置现有海盗作比较。</p>
<div class="controls"><label>播放速度 <select id="speed"><option value="1">正常</option><option value="0.5">半速</option><option value="0.25">四分之一</option></select></label><button id="pause">暂停</button><button id="both">同时受击</button></div>
<div class="grid" id="cards"></div>
<footer>受击播放一次后返回待机。放大预览使用最近邻显示；交付文件为 32 × 36 透明 PNG。声音不包含在此预览中。</footer>
<script>
const frames=__PAYLOAD__;
const reference='__REFERENCE__';
const actors=[];
for(const [name,urls] of Object.entries(frames)){
 const c=document.createElement('section');c.className='card';c.innerHTML=`<h2 class="${name==='RobotCaptain'?'gold':'silver'}">${name}</h2><div class="stage"><img class="sprite" alt="${name} 动画"></div><div class="native"><span>原尺寸</span><img class="small" alt="${name} 原尺寸"><img src="${reference}" alt="原版海盗参考"></div><button class="idle">待机</button><button class="hit">受击</button><div class="state"></div>`;document.querySelector('#cards').append(c);
 const actor={name,urls,frame:1,mode:'idle',card:c};actors.push(actor);
 c.querySelector('.idle').onclick=()=>{actor.mode='idle';actor.frame=1;draw(actor)};
 c.querySelector('.hit').onclick=()=>hit(actor);
 draw(actor);
}
function draw(a){const src=a.urls[a.frame-1];a.card.querySelector('.sprite').src=src;a.card.querySelector('.small').src=src;a.card.querySelector('.state').textContent=(a.mode==='idle'?'待机循环':'受击恢复')+' · 帧 '+a.frame;a.card.querySelector('.idle').classList.toggle('active',a.mode==='idle');a.card.querySelector('.hit').classList.toggle('active',a.mode==='hit')}
function hit(a){a.mode='hit';a.frame=15;draw(a)}
let paused=false,acc=0,last=performance.now();
document.querySelector('#pause').onclick=e=>{paused=!paused;e.target.textContent=paused?'继续':'暂停'};
document.querySelector('#both').onclick=()=>actors.forEach(hit);
function tick(now){const dt=Math.min(100,now-last);last=now;if(!paused){acc+=dt*Number(document.querySelector('#speed').value);while(acc>=40){acc-=40;for(const a of actors){a.frame++;if(a.mode==='idle'&&a.frame>12)a.frame=1;if(a.mode==='hit'&&a.frame>34){a.frame=1;a.mode='idle'}draw(a)}}}requestAnimationFrame(tick)}requestAnimationFrame(tick);
</script></html>
'@
$taskHtml = $taskHtml.Replace('repeat(2,1fr)', 'repeat(2,minmax(0,1fr))').Replace('.card{', '.card{min-width:0;').Replace('width:256px;height:288px;', 'width:256px;max-width:100%;height:auto;')
$taskHtml = $taskHtml.Replace('__PAYLOAD__', ($taskPayload | ConvertTo-Json -Depth 4 -Compress)).Replace('__REFERENCE__',$taskReferenceData)
[IO.File]::WriteAllText((Join-Path $taskReview 'preview.html'), $taskHtml, (New-Object Text.UTF8Encoding($false)))
Write-Output 'Saved self-contained robot animation preview.html.'
