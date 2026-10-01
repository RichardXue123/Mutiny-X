param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$taskRows = @()
foreach ($taskType in 'RobotCaptain','Robot') {
 for ($taskFrame=1; $taskFrame -le 35; $taskFrame++) {
  $taskPath = Join-Path $ProjectRoot ('Assets/Mutiny/Resources/Art/Characters/Animations/'+$taskType+'/'+$taskFrame+'.png')
  $taskBitmap = [Drawing.Bitmap]::FromFile($taskPath)
  if ($taskBitmap.Width -ne 32 -or $taskBitmap.Height -ne 36) {throw "Wrong dimensions: $taskType $taskFrame"}
  $taskOpaque=0; $taskPartial=0; $taskMinX=32; $taskMaxX=-1; $taskMinY=36; $taskMaxY=-1
  $taskGold=0; $taskSilver=0; $taskHead=0; $taskDarkHead=0
  $taskColors=[Collections.Generic.HashSet[int]]::new()
  for($taskY=0;$taskY -lt 36;$taskY++){for($taskX=0;$taskX -lt 32;$taskX++){
   $taskPixel=$taskBitmap.GetPixel($taskX,$taskY)
   if($taskPixel.A -eq 0){continue}
   $taskOpaque++;[void]$taskColors.Add($taskPixel.ToArgb())
   $taskMinX=[Math]::Min($taskMinX,$taskX);$taskMaxX=[Math]::Max($taskMaxX,$taskX)
   $taskMinY=[Math]::Min($taskMinY,$taskY);$taskMaxY=[Math]::Max($taskMaxY,$taskY)
   if($taskPixel.A -ne 255){$taskPartial++}
   if($taskPixel.R -gt $taskPixel.G*1.1 -and $taskPixel.G -gt $taskPixel.B*1.25){$taskGold++}
   if($taskPixel.B -gt $taskPixel.R -and $taskPixel.R -gt 60 -and [Math]::Abs($taskPixel.R-$taskPixel.G) -lt 55 -and [Math]::Abs($taskPixel.G-$taskPixel.B) -lt 55){$taskSilver++}
  }}
  $taskBlank = $taskFrame -eq 13 -or $taskFrame -eq 14
  if($taskPartial -ne 0 -or $taskColors.Count -gt 16 -or ($taskBlank -and $taskOpaque -ne 0) -or (!$taskBlank -and ($taskOpaque -lt 100 -or $taskMinX -le 0 -or $taskMaxX -ge 31 -or $taskMinY -le 0 -or $taskMaxY -ne 31))){throw "Invalid export: $taskType $taskFrame"}
  if(!$taskBlank){
   if($taskType -eq 'RobotCaptain' -and ($taskGold -lt 20 -or $taskGold -le $taskSilver)){throw "Captain must have predominantly gold metal: frame $taskFrame"}
   if($taskType -eq 'Robot' -and ($taskGold -ne 0 -or $taskSilver -lt 20)){throw "Robot must have silver metal without gold: frame $taskFrame"}
   # Top 70% of the visible silhouette measures the helmet, excluding the black suit.
   $taskHeadBottom = $taskMinY + [Math]::Floor(($taskMaxY-$taskMinY+1)*0.7)
   for($taskY=$taskMinY;$taskY -lt $taskHeadBottom;$taskY++){for($taskX=0;$taskX -lt 32;$taskX++){
    $taskPixel=$taskBitmap.GetPixel($taskX,$taskY)
    if($taskPixel.A -gt 0){$taskHead++;if($taskPixel.R -lt 70 -and $taskPixel.G -lt 70 -and $taskPixel.B -lt 70){$taskDarkHead++}}
   }}
  }
  $taskRatio = if($taskHead -gt 0){$taskDarkHead/[double]$taskHead}else{0}
  $taskRows += [pscustomobject]@{type=$taskType;frame=$taskFrame;width=32;height=36;opaque_pixels=$taskOpaque;colors=$taskColors.Count;gold_pixels=$taskGold;silver_pixels=$taskSilver;head_dark_fraction=$taskRatio;bounds=if($taskBlank){'empty separator'}else{"$taskMinX,$taskMinY,$taskMaxX,$taskMaxY"};sha256=(Get-FileHash -LiteralPath $taskPath -Algorithm SHA256).Hash.ToLowerInvariant()}
  $taskBitmap.Dispose()
 }
}
$taskCaptain = $taskRows | Where-Object { $_.type -eq 'RobotCaptain' -and $_.frame -eq 1 }
$taskRobot = $taskRows | Where-Object { $_.type -eq 'Robot' -and $_.frame -eq 1 }
if($taskCaptain.head_dark_fraction -le $taskRobot.head_dark_fraction+0.05){throw 'EXT-ROBOT-ART-02: gold Captain must have larger black faceplate than silver Robot'}
$taskReview = Join-Path $ProjectRoot 'Docs/09-PresentationAndFeedback/05-CharacterAnimation/RobotAssets'
$taskRows | Export-Csv -LiteralPath (Join-Path $taskReview 'MANIFEST.csv') -NoTypeInformation -Encoding utf8
$taskReport = "PASS 70 frames: dimensions, binary alpha, palette <=16, margins, foot baseline; 13/14 blank.`nPASS EXT-ROBOT-ART-02: gold Captain, silver Robot on every visible frame.`nPASS head dark fraction: Captain=$($taskCaptain.head_dark_fraction), Robot=$($taskRobot.head_dark_fraction).`n"
[IO.File]::WriteAllText((Join-Path $taskReview 'ASSET-VERIFICATION.txt'),$taskReport)
Write-Output $taskReport
