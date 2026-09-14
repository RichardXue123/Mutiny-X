var §\x01§ = 140;
var §\x0f§ = 1;
class com.nitrome.throwgame.Water extends com.nitrome.util.Clip
{
   var mc;
   var show;
   var update;
   var x;
   var y;
   function Water()
   {
      super(com.nitrome.throwgame.Controller.waterLayer,"water");
      this.show();
      this.mc.gotoAndStop("water" + com.nitrome.throwgame.Controller.skyColour.toString());
   }
   function advance()
   {
      this.x = -49 * Math.ceil(com.nitrome.throwgame.Controller.content._x / 49);
      this.update();
      com.nitrome.throwgame.Controller.root.background.waterBackground._y = this.y + com.nitrome.throwgame.Controller.content._y;
      var _loc2_ = this.y + com.nitrome.throwgame.Controller.content._y - 350;
      com.nitrome.throwgame.Controller.root.background.cloudBase._y = Math.floor(_loc2_ * 0.5);
      com.nitrome.throwgame.Controller.root.background.hills._y = Math.floor(_loc2_ * 0.3 + 45);
      com.nitrome.throwgame.Controller.root.background.frontClouds._y = Math.floor(_loc2_ * 0.4 - 30);
      com.nitrome.throwgame.Controller.root.background.backClouds._y = Math.floor(_loc2_ * 0.2 - 30);
      _loc2_ = com.nitrome.throwgame.Controller.content._x;
      com.nitrome.throwgame.Controller.root.background.cloudBase._x = Math.floor(com.nitrome.util.Global.negativeModulo(_loc2_ * 0.3,550));
      com.nitrome.throwgame.Controller.root.background.hills._x = Math.floor(com.nitrome.util.Global.negativeModulo(_loc2_ * 0.25,840));
      com.nitrome.throwgame.Controller.root.background.frontClouds._x = Math.floor(com.nitrome.util.Global.negativeModulo(_loc2_ * 0.3,1000));
      com.nitrome.throwgame.Controller.root.background.backClouds._x = Math.floor(com.nitrome.util.Global.negativeModulo(_loc2_ * 0.2,900)) - 450;
      this.mc._visible = com.nitrome.throwgame.Controller.tileSystem.cameraY > this.y - 420;
   }
}
