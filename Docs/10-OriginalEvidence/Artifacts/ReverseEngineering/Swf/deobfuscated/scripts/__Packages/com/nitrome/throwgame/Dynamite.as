var §\x01§ = 776;
var §\x0f§ = 1;
class com.nitrome.throwgame.Dynamite extends com.nitrome.throwgame.Weapon
{
   var bottomExtent;
   var draggable;
   var friction;
   var hide;
   var hitsBoxes;
   var leftExtent;
   var mc;
   var rightExtent;
   var rotation;
   var show;
   var topExtent;
   var twangable;
   var velocityX;
   var velocityY;
   var x;
   var y;
   function Dynamite()
   {
      super("dynamite");
      this.leftExtent = this.rightExtent = this.topExtent = this.bottomExtent = 11;
      this.show();
      this.friction = 1.7;
      this.hitsBoxes = true;
      this.mc.gotoAndPlay("lit");
      this.draggable = false;
      this.twangable = true;
   }
   function advanceMotion()
   {
      if(this.fired)
      {
         this.rotation += this.velocityX * 2;
      }
      super.advanceMotion();
      if(this.velocityX == 0 && Math.abs(this.velocityY) < 0.2)
      {
         if(this.simulation)
         {
            this.simulationFinished = true;
         }
         else if(this.fired && !this.finished)
         {
            this.finished = true;
            this.hide();
            new com.nitrome.throwgame.Explosion(this.x,this.y,250,70,this.owner);
            _root.sfx_manager.playSound("pop");
         }
      }
      if(!this.simulation && this.y > com.nitrome.throwgame.Controller.water.y)
      {
         this.mc.gotoAndStop("unlit");
      }
   }
   function advance()
   {
      super.advance();
      var _loc3_;
      if(!this.simulation && !this.finished)
      {
         _loc3_ = new com.nitrome.throwgame.Debris(com.nitrome.throwgame.Controller.effectsLayer,"cannonSmokeTrail");
         _loc3_.x = this.x;
         _loc3_.y = this.y;
         _loc3_.show();
      }
   }
}
