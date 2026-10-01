$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$root=Split-Path $PSScriptRoot -Parent
$source=Join-Path $root 'Docs/09-PresentationAndFeedback/Space16Art/Sources/splash-generated.png'
$output=Join-Path $root 'Assets/Mutiny/Resources/Art/Space16/Splash'
New-Item -ItemType Directory -Force $output | Out-Null
$atlas=[Drawing.Bitmap]::FromFile($source)
# Mechanical 3x2 atlas extraction, nearest-neighbour 64px sizing and baseline registration.
# Generated row two's impact ring is four logical pixels higher; align both rows at y=52.
for($i=0;$i -lt 6;$i++){
 $bitmap=[Drawing.Bitmap]::new(64,64)
 $row=[int][Math]::Floor($i/3);$column=$i%3
 for($y=0;$y -lt 64;$y++){for($x=0;$x -lt 64;$x++){
  $localY=$y-$(if($row -eq 1){4}else{0})
  if($localY -lt 0){continue}
  $sx=[int][Math]::Floor(($column+($x+0.5)/64)*$atlas.Width/3)
  $sy=[int][Math]::Floor(($row+($localY+0.5)/64)*$atlas.Height/2)
  $bitmap.SetPixel($x,$y,$atlas.GetPixel($sx,$sy))
 }}
 $bitmap.Save((Join-Path $output ('{0:D2}.png' -f ($i+1))),[Drawing.Imaging.ImageFormat]::Png)
 $bitmap.Dispose()
}
$atlas.Dispose()
Write-Output '6 transparent 64x64 galaxy splash poses exported; impact baseline y=52.'
