var §\x01§ = 137;
var §\x0f§ = 1;
class com.nitrome.throwgame.Tile extends com.nitrome.util.Clip
{
   var hide;
   var link;
   var mc;
   var x;
   var y;
   var gridX = 0;
   var gridY = 0;
   function Tile(type, gridX, gridY)
   {
      super(com.nitrome.throwgame.Controller.tileLayer);
      this.gridX = gridX;
      this.gridY = gridY;
      this.x = gridX * 32;
      this.y = gridY * 32;
      this.link(type);
      this.hide();
   }
   function show()
   {
      super.show();
      if(this.mc._totalframes > 1)
      {
         this.mc.gotoAndPlay(1 + com.nitrome.throwgame.Controller.tileSystem.animationCounter % this.mc._totalframes);
      }
   }
}
