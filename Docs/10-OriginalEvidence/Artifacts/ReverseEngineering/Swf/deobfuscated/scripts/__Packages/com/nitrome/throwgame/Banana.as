var §\x01§ = 342;
var §\x0f§ = 1;
class com.nitrome.throwgame.Banana extends com.nitrome.throwgame.Weapon
{
   var bottomExtent;
   var bounce;
   var draggable;
   var friction;
   var hide;
   var hitsBoxes;
   var leftExtent;
   var rightExtent;
   var rotation;
   var show;
   var topExtent;
   var twangMaxForce;
   var twangable;
   var velocityX;
   var velocityY;
   var x;
   var y;
   var lastSqDistance = Infinity;
   function Banana()
   {
      super("banana");
      this.leftExtent = this.rightExtent = this.topExtent = this.bottomExtent = 7;
      this.show();
      this.bounce = 0.8;
      this.friction = 0.5;
      this.hitsBoxes = true;
      this.draggable = false;
      this.twangable = true;
      this.twangMaxForce = 30;
   }
   function contact(side)
   {
      super.contact(side);
      if(this.fired && !this.finished)
      {
         _root.sfx_manager.playSound("banana_bounce");
      }
   }
   function advanceMotion()
   {
      if(this.fired)
      {
         this.rotation += this.velocityX * 2;
      }
      super.advanceMotion();
      var _loc12_ = false;
      if(this.velocityX == 0 && Math.abs(this.velocityY) < 0.5)
      {
         _loc12_ = true;
      }
      var _loc11_;
      var _loc10_;
      var _loc9_;
      var _loc4_;
      var _loc5_;
      var _loc7_;
      var _loc6_;
      var _loc8_;
      if(!_loc12_)
      {
         if(this.owner.team.aiControlled)
         {
            _loc11_ = Infinity;
            _loc10_ = 0;
            while(_loc10_ < com.nitrome.throwgame.Controller.teams.length)
            {
               _loc9_ = com.nitrome.throwgame.Controller.teams[_loc10_];
               _loc4_ = 0;
               while(_loc4_ < _loc9_.characters.length)
               {
                  _loc5_ = _loc9_.characters[_loc4_];
                  _loc7_ = _loc5_.x - this.x;
                  _loc6_ = _loc5_.y - this.y;
                  _loc8_ = _loc7_ * _loc7_ + _loc6_ * _loc6_;
                  if(_loc8_ < _loc11_)
                  {
                     _loc11_ = _loc8_;
                  }
                  _loc4_ = _loc4_ + 1;
               }
               _loc10_ = _loc10_ + 1;
            }
            if(_loc11_ > this.lastSqDistance && _loc11_ < 2500)
            {
               _loc12_ = true;
            }
            if(_loc11_ < 400)
            {
               _loc12_ = true;
            }
         }
         else if(com.nitrome.throwgame.Controller.tileSystem.mouseButtonDown)
         {
            _loc12_ = true;
         }
      }
      if(_loc12_)
      {
         if(this.simulation)
         {
            this.simulationFinished = true;
         }
         else if(this.fired && !this.finished)
         {
            this.finished = true;
            this.hide();
            new com.nitrome.throwgame.Explosion(this.x,this.y,160,80,this.owner);
            _root.sfx_manager.playSound("pop");
         }
      }
   }
   function fire(vx, vy)
   {
      super.fire(vx,vy);
      com.nitrome.throwgame.Controller.tileSystem.mouseButtonDown = false;
   }
}
