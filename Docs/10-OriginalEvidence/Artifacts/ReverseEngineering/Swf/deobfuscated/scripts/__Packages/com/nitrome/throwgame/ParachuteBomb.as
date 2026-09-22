var §\x01§ = 284;
var §\x0f§ = 1;
class com.nitrome.throwgame.ParachuteBomb extends com.nitrome.throwgame.Weapon
{
   var bottomExtent;
   var draggable;
   var hide;
   var hitsBoxes;
   var leftExtent;
   var mc;
   var rightExtent;
   var show;
   var topExtent;
   var twangMaxForce;
   var twangable;
   var velocityX;
   var velocityY;
   var weight;
   var x;
   var y;
   var chuteOpen = false;
   var framesFromFire = 0;
   function ParachuteBomb()
   {
      super("parachuteBomb");
      this.leftExtent = this.rightExtent = this.topExtent = this.bottomExtent = 11;
      this.show();
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
      if(this.fired && !this.finished)
      {
         this.finished = true;
         this.hide();
         new com.nitrome.throwgame.Explosion(this.x,this.y,160,50,this.owner);
         _root.sfx_manager.playSound("pop");
      }
   }
   function twangPrediction(obj)
   {
      if(obj.vy > 1)
      {
         obj.vy -= 2;
         if(obj.vy < 1)
         {
            obj.vy = 1;
         }
      }
      obj.vy += this.weight;
      obj.vx *= 0.95;
   }
   function advanceMotion()
   {
      if(this.fired || this.simulation)
      {
         if(this.velocityY > 1)
         {
            this.velocityY -= 2;
            if(this.velocityY < 1)
            {
               this.velocityY = 1;
            }
         }
         this.velocityX *= 0.95;
         if(!this.chuteOpen && this.velocityY > -10 && !this.simulation)
         {
            this.chuteOpen = true;
            this.mc.gotoAndPlay("opening");
            trace("chute opened [" + this.mc + "]");
         }
         if(this.fired)
         {
            this.framesFromFire++;
         }
      }
      var _loc4_;
      if(this.fired)
      {
         if(!this.simulation && !this.owner.team.aiControlled)
         {
            if(com.nitrome.throwgame.Controller.tileSystem.mouseButtonDown)
            {
               _loc4_ = com.nitrome.throwgame.Controller.content._xmouse - this.x;
               if(_loc4_ < 0)
               {
                  this.velocityX += 0.2;
               }
               else
               {
                  this.velocityX -= 0.2;
               }
               com.nitrome.throwgame.Controller.root.cursor.fan.play();
               if(this.framesFromFire % 12 == 0)
               {
                  _root.sfx_manager.playSound("fan");
               }
            }
            else
            {
               com.nitrome.throwgame.Controller.root.cursor.fan.stop();
            }
         }
      }
      super.advanceMotion();
      if(this.y < -300)
      {
         this.y = -300;
         if(this.velocityY < 0)
         {
            this.velocityY = 0;
         }
      }
   }
   function advance()
   {
      super.advance();
      var _loc3_;
      if(!this.simulation && !this.finished && !this.chuteOpen)
      {
         _loc3_ = new com.nitrome.throwgame.Debris(com.nitrome.throwgame.Controller.effectsLayer,"cannonSmokeTrail");
         _loc3_.x = this.x;
         _loc3_.y = this.y;
         _loc3_.show();
      }
   }
}
