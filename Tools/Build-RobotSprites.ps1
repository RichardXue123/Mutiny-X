param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = 'Stop'
if ($PSVersionTable.PSEdition -eq 'Core') {
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath -ProjectRoot $ProjectRoot
    exit $LASTEXITCODE
}
Add-Type -AssemblyName System.Drawing
# Mechanical export only. The character drawings and poses come from retained imagegen sheets.
Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
public static class RobotSpriteExport {
    public sealed class Cell {
        public Bitmap Image;
        public bool[] Mask;
        public Rectangle Bounds;
        public double FootX;
    }
    static readonly Color[] Palette = Array.ConvertAll(new string[] {
        "FFFDF4", "151719", "27292B", "3A3D40", "55585D",
        "604419", "936719", "BD891E", "E7AE28", "FFD34A", "FFEAA0",
        "5D6979", "929DAD", "BCC6D3", "E5EBF0", "C4EDFF"
    }, s => Color.FromArgb(255, Convert.ToInt32(s.Substring(0,2),16),
        Convert.ToInt32(s.Substring(2,2),16), Convert.ToInt32(s.Substring(4,2),16)));
    static Color Quantize(Color c) {
        Color best=Palette[0]; int distance=int.MaxValue;
        foreach (Color p in Palette) {
            int dr=c.R-p.R, dg=c.G-p.G, db=c.B-p.B;
            int d=dr*dr+dg*dg+db*db;
            if(d<distance){distance=d;best=p;}
        }
        return best;
    }
    static Cell Extract(Bitmap sheet,int index) {
        int x0=(int)Math.Round((index%4)*sheet.Width/4.0);
        int x1=(int)Math.Round((index%4+1)*sheet.Width/4.0);
        int y0=(int)Math.Round((index/4)*sheet.Height/2.0);
        int y1=(int)Math.Round((index/4+1)*sheet.Height/2.0);
        Bitmap b=sheet.Clone(new Rectangle(x0,y0,x1-x0,y1-y0),PixelFormat.Format32bppArgb);
        int w=b.Width,h=b.Height; var opaque=new bool[w*h]; var seen=new bool[w*h];
        for(int y=0;y<h;y++)for(int x=0;x<w;x++)opaque[y*w+x]=b.GetPixel(x,y).A>=128;
        var largest=new List<int>();
        for(int i=0;i<opaque.Length;i++) {
            if(!opaque[i]||seen[i])continue;
            var component=new List<int>(); var queue=new Queue<int>(); queue.Enqueue(i); seen[i]=true;
            while(queue.Count>0){
                int p=queue.Dequeue(); component.Add(p); int x=p%w,y=p/w;
                int[] adjacent={x>0?p-1:-1,x+1<w?p+1:-1,y>0?p-w:-1,y+1<h?p+w:-1};
                foreach(int n in adjacent)if(n>=0&&opaque[n]&&!seen[n]){seen[n]=true;queue.Enqueue(n);}
            }
            if(component.Count>largest.Count)largest=component;
        }
        if(largest.Count<100)throw new Exception("Empty generated cell "+index);
        var mask=new bool[w*h]; int minX=w,minY=h,maxX=0,maxY=0;
        foreach(int p in largest){mask[p]=true;int x=p%w,y=p/w;minX=Math.Min(minX,x);maxX=Math.Max(maxX,x);minY=Math.Min(minY,y);maxY=Math.Max(maxY,y);}
        int footLeft=w,footRight=0;
        // Register using the lowest opaque strip, rather than the changing head/arm silhouette.
        int footDepth=Math.Max(2,(maxY-minY+1)/14);
        foreach(int p in largest)if(p/w>=maxY-footDepth){footLeft=Math.Min(footLeft,p%w);footRight=Math.Max(footRight,p%w);}
        return new Cell{Image=b,Mask=mask,Bounds=Rectangle.FromLTRB(minX,minY,maxX+1,maxY+1),FootX=(footLeft+footRight+1)/2.0};
    }
    static Bitmap Raster(Cell c,double scale) {
        var output=new Bitmap(32,36,PixelFormat.Format32bppArgb);
        for(int y=0;y<36;y++)for(int x=0;x<32;x++){
            int sx=(int)Math.Floor((x+0.5-16)/scale+c.FootX);
            int sy=(int)Math.Floor((y+0.5-32)/scale+c.Bounds.Bottom);
            if(sx<0||sx>=c.Image.Width||sy<0||sy>=c.Image.Height||!c.Mask[sy*c.Image.Width+sx])continue;
            output.SetPixel(x,y,Quantize(c.Image.GetPixel(sx,sy)));
        }
        return output;
    }
    public static void Export(string source,string output,string preview,string keyOutput,int neutralHeight) {
        Directory.CreateDirectory(output);Directory.CreateDirectory(keyOutput);
        using(var sheet=new Bitmap(source)){
            var cells=new Cell[8]; for(int i=0;i<8;i++)cells[i]=Extract(sheet,i);
            double scale=neutralHeight/(double)cells[0].Bounds.Height;
            var poses=new Bitmap[8];for(int i=0;i<8;i++)poses[i]=Raster(cells[i],scale);
            // Reuse the exact neutral pose at every neutral slot to prevent generative identity drift.
            poses[2].Dispose();poses[2]=(Bitmap)poses[0].Clone();
            poses[7].Dispose();poses[7]=(Bitmap)poses[0].Clone();
            for(int i=0;i<8;i++)poses[i].Save(Path.Combine(keyOutput,(i+1)+".png"),ImageFormat.Png);
            for(int f=1;f<=35;f++){
                int pose=f<=3?0:f<=6?1:f<=9?0:f<=12?3:f<=14?-1:f<=20?4:f<=25?5:f<=30?6:0;
                if(pose<0){using(var blank=new Bitmap(32,36))blank.Save(Path.Combine(output,f+".png"),ImageFormat.Png);}
                else poses[pose].Save(Path.Combine(output,f+".png"),ImageFormat.Png);
            }
            poses[0].Save(preview,ImageFormat.Png);
            for(int i=0;i<8;i++){poses[i].Dispose();cells[i].Image.Dispose();}
        }
    }
}
'@
$taskArtRoot = Join-Path $ProjectRoot 'Assets/Mutiny/Resources/Art/Characters'
$taskReviewRoot = Join-Path $ProjectRoot 'Docs/09-PresentationAndFeedback/05-CharacterAnimation/RobotAssets'
foreach ($taskType in @('RobotCaptain','Robot')) {
    $taskFrames = Join-Path $taskArtRoot ('Animations/' + $taskType)
    $taskPreview = Join-Path $taskArtRoot ('Preview/' + $taskType + '.png')
    $taskHeight = if ($taskType -eq 'RobotCaptain') {26} else {24}
    [RobotSpriteExport]::Export((Join-Path $taskReviewRoot ('Sources/' + $taskType + '-generated.png')), $taskFrames, $taskPreview, (Join-Path $taskReviewRoot ('Keyframes/' + $taskType)), $taskHeight)
    $taskFolderMeta = $taskFrames + '.meta'
    if (!(Test-Path -LiteralPath $taskFolderMeta)) {
        [IO.File]::WriteAllText($taskFolderMeta, "fileFormatVersion: 2`nguid: $([guid]::NewGuid().ToString('N'))`nfolderAsset: yes`nDefaultImporter:`n  externalObjects: {}`n  userData: `n  assetBundleName: `n  assetBundleVariant: `n")
    }
    $taskImages = @(Get-ChildItem -LiteralPath $taskFrames -Filter '*.png') + @(Get-Item -LiteralPath $taskPreview)
    foreach ($taskImage in $taskImages) {
        $taskMetaPath = $taskImage.FullName + '.meta'
        if (Test-Path -LiteralPath $taskMetaPath) {continue}
        $taskMeta = @"
fileFormatVersion: 2
guid: $([guid]::NewGuid().ToString('N'))
TextureImporter:
  serializedVersion: 13
  internalIDToNameTable: []
  externalObjects: {}
  mipmaps:
    enableMipMap: 0
    sRGBTexture: 1
  isReadable: 0
  textureSettings:
    filterMode: 0
    aniso: 1
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  maxTextureSize: 2048
  textureCompression: 0
  spriteMode: 1
  spriteMeshType: 0
  alignment: 9
  spritePivot: {x: 0.5, y: 0.41666667}
  spritePixelsToUnits: 32
  alphaUsage: 1
  alphaIsTransparency: 1
  textureType: 8
  textureShape: 1
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 100
    overridden: 0
  userData: imagegen robot extension 2026-10-01; 32x36; pivot 16,15; 25Hz
  assetBundleName:
  assetBundleVariant:
"@
        [IO.File]::WriteAllText($taskMetaPath, $taskMeta + "`n")
    }
}
Write-Output 'Exported RobotCaptain + Robot: 70 timeline PNGs, 2 previews, 16 retained keyframes.'
