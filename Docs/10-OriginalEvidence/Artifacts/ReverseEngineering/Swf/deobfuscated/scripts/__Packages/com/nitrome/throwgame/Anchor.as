var §\x01§ = 895;
var §\x0f§ = 1;
class com.nitrome.throwgame.Anchor extends com.nitrome.throwgame.Weapon
{
   var advanceMotion;
   var bottomExtent;
   var draggable;
   var finished;
   var fired;
   var hide;
   var hitsBoxes;
   var leftExtent;
   var mc;
   var owner;
   var placeableWeapon;
   var rightExtent;
   var show;
   var showCircle;
   var simulation;
   var simulationFinished;
   var topExtent;
   var update;
   var velocityX;
   var velocityY;
   var x;
   var y;
   var hitBottom = false;
   var holdTime = 30;
   var fadeTime = 10;
   var aiWaitTime = 0;
   function Anchor()
   {
      super("anchor");
      this.mc.stop();
      this.leftExtent = this.rightExtent = 48;
      this.topExtent = 96;
      this.bottomExtent = 0;
      this.placeableWeapon = true;
      this.hitsBoxes = true;
      this.showCircle = false;
      this.draggable = false;
   }
   function advance()
   {
      if(this.aiWaitTime > 0 && this.owner.team.aiControlled)
      {
         this.aiWaitTime--;
         return undefined;
      }
      if(!this.fired && com.nitrome.throwgame.Controller.tileSystem.mouseButtonDown)
      {
         this.place(com.nitrome.throwgame.Controller.content._xmouse,com.nitrome.throwgame.Controller.content._ymouse);
         this.show();
         this.owner.canShoot = false;
         this.owner.canThrow = false;
      }
      if(this.fired && !this.hitBottom)
      {
         this.velocityY = 40;
         this.advanceMotion();
         this.update();
      }
      if(this.hitBottom)
      {
         if(this.holdTime > 0)
         {
            this.holdTime--;
         }
         else if(this.fadeTime > 0)
         {
            this.fadeTime--;
            this.mc.transform.colorTransform = com.nitrome.util.Global.whiteOut(this.fadeTime / 10);
         }
         else
         {
            this.hide();
            this.finished = true;
         }
      }
   }
   function contact(side)
   {
      if(this.simulation && side == com.nitrome.throwgame.Solid.FLOOR)
      {
         this.simulationFinished = true;
         return undefined;
      }
      super.contact(side);
      var _loc7_;
      var _loc6_;
      var _loc5_;
      var _loc4_;
      if(side == com.nitrome.throwgame.Solid.FLOOR)
      {
         if(!this.hitBottom)
         {
            this.hitBottom = true;
            _loc7_ = 0;
            while(_loc7_ < com.nitrome.throwgame.Controller.teams.length)
            {
               _loc6_ = com.nitrome.throwgame.Controller.teams[_loc7_];
               _loc5_ = 0;
               while(_loc5_ < _loc6_.characters.length)
               {
                  _loc4_ = _loc6_.characters[_loc5_];
                  if(Math.abs(_loc4_.x - this.x) < 48 && _loc4_.y < this.y && _loc4_.y > this.y - 64)
                  {
                     _loc4_.subtractHealth(60);
                  }
                  _loc5_ = _loc5_ + 1;
               }
               _loc7_ = _loc7_ + 1;
            }
            this.mc.play();
         }
         this.velocityY = 0;
         _root.sfx_manager.playSound("anchor");
      }
   }
   function place(x, y)
   {
      super.place(x,y);
      this.y = -200;
   }
   function randomThrows(count)
   {
      var _loc5_ = [];
      this.simulation = true;
      var _loc2_ = 0;
      while(_loc2_ < count)
      {
         this.x = Math.floor(Math.random() * (com.nitrome.throwgame.Controller.tileSystem.levelWidth << 5));
         this.y = -200;
         this.velocityX = 0;
         this.velocityY = 40;
         this.simulationFinished = false;
         while(!this.simulationFinished && this.y < com.nitrome.throwgame.Controller.water.y)
         {
            this.advanceMotion();
         }
         if(this.simulationFinished)
         {
            _loc5_.push({x:this.x,ex:this.x,ey:this.y});
         }
         _loc2_ = _loc2_ + 1;
      }
      this.simulation = false;
      return _loc5_;
   }
   function aiPerform(details)
   {
      this.place(details.x,0);
      this.aiWaitTime = 20;
      this.show();
   }
}
