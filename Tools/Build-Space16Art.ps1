$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = Split-Path $PSScriptRoot -Parent
$source = Join-Path $root 'Docs/09-PresentationAndFeedback/Space16Art/Sources'
$output = Join-Path $root 'Assets/Mutiny/Resources/Art/Space16'
New-Item -ItemType Directory -Force $output | Out-Null
# Mechanical atlas extraction and nearest-neighbour sizing only. All art is imagegen output.
function Export-Pixels($image, $rect, [int]$width, [int]$height, $path) {
    $bitmap = New-Object System.Drawing.Bitmap($width, $height)
    for ($y = 0; $y -lt $height; $y++) {
        for ($x = 0; $x -lt $width; $x++) {
            $sx = [Math]::Min($image.Width-1, [int][Math]::Floor($rect.X + ($x + 0.5) * $rect.Width / $width))
            $sy = [Math]::Min($image.Height-1, [int][Math]::Floor($rect.Y + ($y + 0.5) * $rect.Height / $height))
            $bitmap.SetPixel($x, $y, $image.GetPixel($sx, $sy))
        }
    }
    $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
}
$atlas = [System.Drawing.Bitmap]::FromFile((Join-Path $source 'tiles-generated.png'))
$names = @('hull_a','hull_b','edge_top','edge_bottom','edge_left','edge_right','interior','cannon')
for ($i=0; $i -lt $names.Count; $i++) {
    $rect = [System.Drawing.RectangleF]::new(($i%4)*$atlas.Width/4, [Math]::Floor($i/4)*$atlas.Height/2, $atlas.Width/4, $atlas.Height/2)
    Export-Pixels $atlas $rect 32 32 (Join-Path $output ($names[$i]+'.png'))
}
$atlas.Dispose()
foreach ($name in @('sky','galaxy')) {
    $inputImage = [System.Drawing.Bitmap]::FromFile((Join-Path $source ($name+'-generated.png')))
    Export-Pixels $inputImage ([System.Drawing.RectangleF]::new(0,0,$inputImage.Width,$inputImage.Height)) 384 256 (Join-Path $output ($name+'.png'))
    $inputImage.Dispose()
}
Write-Output 'Exported 8 x 32px tiles and 2 x 384x256 backgrounds.'
& (Join-Path $PSScriptRoot 'Build-SpaceParallaxArt.ps1')
