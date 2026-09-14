var §\x01§ = 250;
var §\x0f§ = 1;
class com.nitrome.throwgame.RumBottle extends com.nitrome.throwgame.Weapon
{
   var bottomExtent;
   var draggable;
   var finished;
   var fired;
   var hide;
   var hitsBoxes;
   var leftExtent;
   var owner;
   var rightExtent;
   var rotation;
   var show;
   var simulation;
   var simulationFinished;
   var topExtent;
   var twangMaxForce;
   var twangable;
   var update;
   var velocityX;
   var x;
   var y;
   function RumBottle()
   {
      super("rumBottle");
      this.show();
      this.leftExtent = this.rightExtent = this.topExtent = this.bottomExtent = 14;
      this.hitsBoxes = true;
      this.draggable = false;
      this.twangable = true;
      this.twangMaxForce = 30;
   }
   function contact(side)
   {
      if(this.simulation)
      {
         this.simulationFinished = true;
         return undefined;
      }
      super.contact(side);
      var _loc5_;
      var _loc4_;
      var _loc6_;
      if(this.fired && !this.finished)
      {
         this.finished = true;
         this.hide();
         new com.nitrome.throwgame.Explosion(this.x,this.y,80,25,this.owner);
         _root.sfx_manager.playSound("pop");
         if(side == com.nitrome.throwgame.Solid.FLOOR)
         {
            _loc5_ = this.x >> 5;
            _loc4_ = (this.y >> 5) + 1;
            _loc6_ = com.nitrome.throwgame.Controller.tileSystem.tileGrid[_loc5_][_loc4_];
            while(com.nitrome.throwgame.Controller.tileSystem.tileGrid[_loc5_][_loc4_ - 1])
            {
               _loc4_ = _loc4_ - 1;
               _loc6_ = com.nitrome.throwgame.Controller.tileSystem.tileGrid[_loc5_][_loc4_];
            }
            new com.nitrome.throwgame.SweepingFlame(_loc5_ << 5,_loc4_ << 5,true);
            new com.nitrome.throwgame.SweepingFlame(_loc5_ << 5,_loc4_ << 5,false);
         }
      }
   }
   function advance()
   {
      if(!this.simulation && this.fired)
      {
         this.rotation += this.velocityX * 2;
         this.update();
      }
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
