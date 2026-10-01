$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$root=Split-Path $PSScriptRoot -Parent
$source=Join-Path $root 'Docs/09-PresentationAndFeedback/Space16Art/Sources'
$output=Join-Path $root 'Assets/Mutiny/Resources/Art/Space16/Parallax'
New-Item -ItemType Directory -Force $output | Out-Null
function Export-Region($image,$rect,[int]$width,[int]$height,$name){
 $bitmap=[Drawing.Bitmap]::new($width,$height)
 for($y=0;$y -lt $height;$y++){for($x=0;$x -lt $width;$x++){
  $sx=[int][Math]::Floor($rect.X+($x+0.5)*$rect.Width/$width)
  $sy=[int][Math]::Floor($rect.Y+($y+0.5)*$rect.Height/$height)
  $bitmap.SetPixel($x,$y,$image.GetPixel($sx,$sy))
 }}
 $bitmap.Save((Join-Path $output ($name+'.png')),[Drawing.Imaging.ImageFormat]::Png);$bitmap.Dispose()
}
foreach($name in @('deep_space','starfield')){
 $img=[Drawing.Bitmap]::FromFile((Join-Path $source ($name+'-generated.png')))
 Export-Region $img ([Drawing.Rectangle]::new(0,0,$img.Width,$img.Height)) 384 256 $name
 $img.Dispose()
}
# Crop the actual non-overlapping atlas regions; generated ring extends beyond equal thirds.
$img=[Drawing.Bitmap]::FromFile((Join-Path $source 'celestials-generated.png'))
Export-Region $img ([Drawing.Rectangle]::new(0,0,704,724)) 128 128 'sun'
Export-Region $img ([Drawing.Rectangle]::new(704,0,912,724)) 160 128 'ring_planet'
Export-Region $img ([Drawing.Rectangle]::new(1616,96,556,556)) 128 128 'moon'
$img.Dispose()
# Remove excess transparent top margin; generated bright edge y=288 becomes runtime y=32.
$img=[Drawing.Bitmap]::FromFile((Join-Path $source 'galaxy_surface-generated.png'))
Export-Region $img ([Drawing.Rectangle]::new(0,160,1536,864)) 384 216 'galaxy_surface'
$img.Dispose()
Write-Output 'Parallax art extracted; original generation files and alpha preserved.'
