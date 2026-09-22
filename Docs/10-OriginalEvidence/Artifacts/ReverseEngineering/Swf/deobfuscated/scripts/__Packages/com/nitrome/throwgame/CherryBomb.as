var §\x01§ = 955;
var §\x0f§ = 1;
class com.nitrome.throwgame.CherryBomb extends com.nitrome.throwgame.Weapon
{
   var bottomExtent;
   var draggable;
   var hide;
   var hitsBoxes;
   var leftExtent;
   var rightExtent;
   var show;
   var topExtent;
   var twangable;
   var x;
   var y;
   function CherryBomb()
   {
      super("cherryBomb");
      this.show();
      this.leftExtent = this.rightExtent = this.topExtent = this.bottomExtent = 9;
      this.hitsBoxes = true;
      this.draggable = false;
      this.twangable = true;
   }
   function contact(side)
   {
      if(this.simulation)
      {
         this.simulationFinished = true;
         return undefined;
      }
      super.contact(side);
      if(this.fired && !this.finished)
      {
         this.finished = true;
         this.hide();
         new com.nitrome.throwgame.Explosion(this.x,this.y,80,40,this.owner);
         _root.sfx_manager.playSound("pop");
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
