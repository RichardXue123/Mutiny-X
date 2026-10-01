param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$taskDir = Join-Path $ProjectRoot 'Docs/02-LevelAndWorld/04-ObjectsAndSpawns/Level16Draft'
$taskLayout = Get-Content -LiteralPath (Join-Path $taskDir 'layout.json') -Raw | ConvertFrom-Json
$taskMapping = @{}
Import-Csv -LiteralPath (Join-Path $ProjectRoot 'Assets/Mutiny/Resources/Data/Tiles/tile-mapping.csv') | ForEach-Object { $taskMapping[$_.tile_name] = $_ }
$taskImages = @{}
$taskCell = 16
$taskBoard = [Drawing.Bitmap]::new([int]($taskLayout.width*$taskCell),[int]($taskLayout.height*$taskCell))
$taskGraphics = [Drawing.Graphics]::FromImage($taskBoard)
$taskGraphics.Clear([Drawing.Color]::FromArgb(255,19,25,38))
$taskGraphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$taskGraphics.PixelOffsetMode = [Drawing.Drawing2D.PixelOffsetMode]::Half
$spaceTheme = $taskLayout.visualTheme -eq 'space'
$waterY = [int]$taskLayout.waterY
function Draw-SpacePlane($name,$width,$height,$period,$anchorX,$anchorY,$parallaxX,$parallaxY,$repeatY=$false,$mirror=$false) {
 $img=[Drawing.Bitmap]::FromFile((Join-Path $ProjectRoot ('Assets/Mutiny/Resources/Art/Space16/'+$name+'.png')))
 $flipped=$null
 if($mirror){$flipped=$img.Clone();$flipped.RotateFlip([Drawing.RotateFlipType]::RotateNoneFlipX)}
 $originX=$anchorX+$taskLayout.width*0.5*(1-$parallaxX)
 $originY=-$anchorY+$taskLayout.height*0.5*(1-$parallaxY)
 $range=[int][Math]::Ceiling($taskLayout.width/$period)+2
 $rows=if($repeatY){-4..4}else{@(0)}
 foreach($row in $rows){for($column=-$range;$column -le $range;$column++){
  $x=$originX+$column*$period;$y=$originY+$row*$height
  if($x+$width -lt 0 -or $x -gt $taskLayout.width -or $y+$height -lt 0 -or $y -gt $taskLayout.height){continue}
  $imageToDraw=if($mirror -and (($column -band 1) -ne 0)){$flipped}else{$img}
  $taskGraphics.DrawImage($imageToDraw,[Drawing.Rectangle]::new([int]($x*$taskCell),[int]($y*$taskCell),[int]($width*$taskCell),[int]($height*$taskCell)),0,0,$img.Width,$img.Height,[Drawing.GraphicsUnit]::Pixel)
 }}
 if($flipped){$flipped.Dispose()};$img.Dispose()
}
if ($spaceTheme) {
  $bgImage = [Drawing.Image]::FromFile((Join-Path $ProjectRoot 'Assets/Mutiny/Resources/Art/Space16/Parallax/deep_space.png'))
  $bgH=[int]($taskBoard.Width*$bgImage.Height/$bgImage.Width)
  $taskGraphics.DrawImage($bgImage,[Drawing.Rectangle]::new(0,0,$taskBoard.Width,$bgH),0,0,$bgImage.Width,$bgImage.Height,[Drawing.GraphicsUnit]::Pixel)
  $bgImage.Dispose()
  Draw-SpacePlane 'Parallax/starfield' 24 16 24 0 0 0.08 0.08 $true
  Draw-SpacePlane 'Parallax/sun' 2.5 2.5 32 9 3 0.12 0.10
  Draw-SpacePlane 'Parallax/moon' 2.5 2.5 34 15 -1 0.20 0.18
  Draw-SpacePlane 'Parallax/ring_planet' 7.5 6 32 3 1 0.35 0.28
  Draw-SpacePlane 'galaxy' 24 8 24 0 (-$waterY+2.4) 0.3 1 $false $true
}
function Has-Solid([int]$x,[int]$y) {
 if($x -lt 0 -or $y -lt 0 -or $x -ge $taskLayout.width -or $y -ge $taskLayout.height){return $false}
 return $taskLayout.terrain[$y][$x] -ne '-'
}
# Static assembled overview. Separate Unity camera captures verify the production rendering.
$layerIndex=0
foreach ($taskLayer in @($taskLayout.background,$taskLayout.terrain)) {
 for($taskY=0;$taskY -lt $taskLayout.height;$taskY++){for($taskX=0;$taskX -lt $taskLayout.width;$taskX++){
  $taskName = $taskLayer[$taskY][$taskX]
  if ($taskName -eq '-' -or $taskName -eq 'antichest') {continue}
  $taskDef=$taskMapping[$taskName]
  $spaceTile = $spaceTheme -and $taskX -ge [int]$taskLayout.spaceThemeMinX
  if ($spaceTile) {
   if ($taskName -like 'cannon_port_*') { $taskName='cannon' }
   elseif ($taskName -notin 'ship_tile_1','ship_tile_2','ship_top_middle','cave_middle_2') { $spaceTile=$false }
   elseif ($layerIndex -eq 0) { $taskName='interior' }
   elseif (!(Has-Solid $taskX ($taskY-1))) { $taskName='edge_top' }
   elseif (!(Has-Solid $taskX ($taskY+1))) { $taskName='edge_bottom' }
   elseif (!(Has-Solid ($taskX-1) $taskY)) { $taskName='edge_left' }
   elseif (!(Has-Solid ($taskX+1) $taskY)) { $taskName='edge_right' }
   elseif ($taskName -eq 'ship_tile_2') { $taskName='hull_b' }
   else { $taskName='hull_a' }
  }
  if (!$taskImages.ContainsKey($taskName)) {
   $taskPath=Join-Path $ProjectRoot ('Assets/Mutiny/Resources/Art/Tiles/Single/'+$taskName+'.png')
   if ($spaceTile) { $taskPath=Join-Path $ProjectRoot ('Assets/Mutiny/Resources/Art/Space16/'+$taskName+'.png') }
   $taskImages[$taskName]=[Drawing.Image]::FromFile($taskPath)
   if ($spaceTheme -and $taskName -match '^eart') {
    $sourceRock=$taskImages[$taskName]
    $coldRock=[Drawing.Bitmap]::new($sourceRock.Width,$sourceRock.Height)
    for($py=0;$py -lt $sourceRock.Height;$py++){for($px=0;$px -lt $sourceRock.Width;$px++){
     $pixel=$sourceRock.GetPixel($px,$py)
     $coldRock.SetPixel($px,$py,[Drawing.Color]::FromArgb($pixel.A,[int]($pixel.R*.52),[int]($pixel.R*.60),[int]($pixel.R*.74)))
    }}
    $sourceRock.Dispose();$taskImages[$taskName]=$coldRock
   }
  }
  $taskImage=$taskImages[$taskName]
  $taskLeft=[int][Math]::Round(($taskX*32-[double]$taskDef.origin_x_from_left_px)/2)
  $taskTop=[int][Math]::Round(($taskY*32-[double]$taskDef.origin_y_from_top_px)/2)
  $taskGraphics.DrawImage($taskImage,(New-Object Drawing.Rectangle($taskLeft,$taskTop,($taskImage.Width/2),($taskImage.Height/2))),0,0,$taskImage.Width,$taskImage.Height,[Drawing.GraphicsUnit]::Pixel)
 }}
 $layerIndex++
}
$taskRedPen = New-Object Drawing.Pen([Drawing.Color]::FromArgb(255,255,89,103),2)
$taskBluePen = New-Object Drawing.Pen([Drawing.Color]::FromArgb(255,102,220,255),2)
foreach($taskObj in $taskLayout.objects){
 if($taskObj.type -in 'water','potentialWeapons'){continue}
 $taskCharacter=[Drawing.Image]::FromFile((Join-Path $ProjectRoot ('Assets/Mutiny/Resources/Art/Characters/Preview/'+$taskObj.type+'.png')))
 $taskPlayer=$taskObj.type -like 'redPirate*'
 $taskPivotX=if($taskObj.type -eq 'redPirate'){12}else{if($taskObj.type -eq 'redPirateCaptain'){14}else{16}}
 $taskLeft=($taskObj.x*32+16-$taskPivotX)/2
 $taskTop=($taskObj.y*32+24-($taskCharacter.Height-15))/2
 $taskGraphics.DrawImage($taskCharacter,(New-Object Drawing.Rectangle([int]$taskLeft,[int]$taskTop,([int]($taskCharacter.Width/2)),([int]($taskCharacter.Height/2)))),0,0,$taskCharacter.Width,$taskCharacter.Height,[Drawing.GraphicsUnit]::Pixel)
 $taskPen=if($taskPlayer){$taskRedPen}else{$taskBluePen}
 $taskGraphics.DrawEllipse($taskPen,([int]($taskObj.x*$taskCell+1)),([int]($taskObj.y*$taskCell)),14,16)
 $taskCharacter.Dispose()
}
$taskWaterPen=New-Object Drawing.Pen([Drawing.Color]::FromArgb(160,103,147,201),1)
if ($spaceTheme) { Draw-SpacePlane 'Parallax/galaxy_surface' 12 6.75 12 0 (-$waterY+1) 1 1 $false $true }
if (!$spaceTheme) { $taskGraphics.DrawLine($taskWaterPen,0,$waterY*$taskCell,$taskBoard.Width,$waterY*$taskCell) }
$taskBoard.Save((Join-Path $taskDir 'overview.png'),[Drawing.Imaging.ImageFormat]::Png)
$taskRedPen.Dispose();$taskBluePen.Dispose();$taskWaterPen.Dispose()
$taskGraphics.Dispose();$taskBoard.Dispose()
foreach($taskImage in $taskImages.Values){$taskImage.Dispose()}
Write-Output 'Saved Level 16 overview from actual tiles and character previews, at half game-pixel scale.'
