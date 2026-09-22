var §\x01§ = 796;
var §\x0f§ = 1;
class com.nitrome.throwgame.Mine extends com.nitrome.throwgame.Weapon
{
   var beepTimes;
   var bottomExtent;
   var destroy;
   var dragRange;
   var draggable;
   var finished;
   var fired;
   var friction;
   var hitsBoxes;
   var leftExtent;
   var limitedToTurn;
   var mc;
   var owner;
   var rightExtent;
   var show;
   var simulation;
   var simulationFinished;
   var topExtent;
   var twangable;
   var velocityX;
   var velocityY;
   var x;
   var y;
   var ignoreTime = 10;
   var countdown = 60;
   var active = false;
   var exploded = false;
   function Mine()
   {
      super("mine");
      this.leftExtent = this.rightExtent = this.topExtent = this.bottomExtent = 14;
      this.friction = 1.5;
      this.show();
      this.limitedToTurn = false;
      this.hitsBoxes = true;
      this.dragRange = 180;
      this.beepTimes = [0,15,30,38,45,49,53,55,57,59];
      this.mc.gotoAndStop("in_throw");
      this.draggable = false;
      this.twangable = true;
   }
   function advance()
   {
      super.advance();
      if(this.active && this.beepTimes.length > 0 && 60 - this.countdown >= this.beepTimes[0])
      {
         _root.sfx_manager.playSound("mine_beep");
         this.beepTimes.splice(0,1);
      }
   }
   function advanceMotion()
   {
      super.advanceMotion();
      if(this.fired)
      {
         this.checkForProximity();
      }
      if(this.velocityX == 0 && Math.abs(this.velocityY) < 0.2)
      {
         if(this.simulation)
         {
            this.simulationFinished = true;
         }
         else if(this.fired && !this.finished)
         {
            this.finished = true;
            com.nitrome.throwgame.Controller.mines.push(this);
            if(!this.active)
            {
               this.mc.gotoAndPlay("arm");
            }
         }
      }
      if(this.active && !this.exploded)
      {
         this.countdown--;
         if(this.countdown <= 0)
         {
            this.explode();
         }
         com.nitrome.throwgame.Controller.inactivity = 0;
      }
   }
   function checkForProximity()
   {
      if(!this.fired)
      {
         return undefined;
      }
      if(this.active)
      {
         return undefined;
      }
      if(--this.ignoreTime > 0)
      {
         return undefined;
      }
      var _loc8_ = 0;
      var _loc6_;
      var _loc3_;
      var _loc2_;
      var _loc5_;
      var _loc4_;
      var _loc7_;
      loop0:
      while(_loc8_ < com.nitrome.throwgame.Controller.teams.length)
      {
         _loc6_ = com.nitrome.throwgame.Controller.teams[_loc8_];
         _loc3_ = 0;
         while(true)
         {
            if(_loc3_ < _loc6_.characters.length)
            {
               _loc2_ = _loc6_.characters[_loc3_];
               if(_loc2_.alive)
               {
                  if(!(_loc2_.velocityX == 0 && Math.abs(_loc2_.velocityY) <= 0.2 && com.nitrome.throwgame.Controller.twanging != _loc2_))
                  {
                     _loc5_ = _loc2_.x - this.x;
                     _loc4_ = _loc2_.y + _loc2_.bottomExtent - this.y;
                     _loc7_ = _loc5_ * _loc5_ + _loc4_ * _loc4_;
                     if(_loc7_ < 3600)
                     {
                        break;
                     }
                  }
               }
               continue;
            }
            continue loop0;
            _loc8_ = _loc8_ + 1;
            _loc3_ = _loc3_ + 1;
         }
         this.active = true;
         this.mc.gotoAndPlay("warn");
         return undefined;
      }
   }
   function explode()
   {
      if(this.simulation)
      {
         this.simulationFinished = true;
         return undefined;
      }
      if(this.exploded)
      {
         return undefined;
      }
      new com.nitrome.throwgame.Explosion(this.x,this.y,250,70,this.owner);
      _root.sfx_manager.playSound("pop");
      var _loc3_ = com.nitrome.throwgame.Controller.mines.length - 1;
      while(_loc3_ >= 0)
      {
         if(com.nitrome.throwgame.Controller.mines[_loc3_] == this)
         {
            com.nitrome.throwgame.Controller.mines.splice(_loc3_,1);
         }
         _loc3_ = _loc3_ - 1;
      }
      this.finished = true;
      this.exploded = true;
      this.destroy();
   }
}
