var §\x01§ = 183;
var §\x0f§ = 1;
class com.nitrome.throwgame.Cannon extends com.nitrome.throwgame.Weapon
{
   var advanceMotion;
   var aiFireDetails;
   var dragOffset;
   var dragRange;
   var finished;
   var fired;
   var hide;
   var hitsBoxes;
   var mc;
   var owner;
   var placeableWeapon;
   var rotation;
   var show;
   var track;
   var trackX;
   var trackY;
   var velocityX;
   var velocityY;
   var weight;
   var x;
   var y;
   var draggingPin = false;
   var fireStrength = 0;
   var cannonball = null;
   var visibility = 2;
   var aiFireTime = 0;
   function Cannon()
   {
      super("cannon");
      this.show();
      this.placeableWeapon = true;
      this.weight = 0;
      this.hitsBoxes = true;
      this.mc.blendMode = "layer";
   }
   function release()
   {
      this.velocityX = 0;
      this.velocityY = 0;
   }
   function update()
   {
      super.update();
      if(this.cannonball)
      {
         this.trackX = this.cannonball.x;
         this.trackY = this.cannonball.y;
         this.track = true;
      }
   }
   function advance()
   {
      var _loc3_;
      var _loc2_;
      var _loc4_;
      this.advanceMotion();
      var _loc6_;
      var _loc5_;
      if(this.aiFireTime > 0)
      {
         this.aiFireTime--;
         if(this.aiFireTime < 1)
         {
            this.fire(this.aiFireDetails.vx,this.aiFireDetails.vy);
         }
      }
      else if(this.fired)
      {
         this.cannonball.advance();
         this.visibility -= 0.1;
         if(this.visibility < 0)
         {
            this.visibility = 0;
         }
         if(this.visibility < 1)
         {
            this.mc._alpha = this.visibility * 100;
         }
         if(this.cannonball.finished && this.visibility == 0)
         {
            this.finished = true;
            this.hide();
         }
      }
      else
      {
         if(com.nitrome.throwgame.Controller.dragging == this)
         {
            _loc3_ = this.x - this.owner.x;
            _loc2_ = this.y - this.owner.y + 100;
            _loc4_ = _loc3_ * _loc3_ + _loc2_ * _loc2_;
            if(_loc4_ > 14400)
            {
               _loc6_ = Math.sqrt(_loc4_);
               this.x = this.owner.x + _loc3_ * 120 / _loc6_;
               this.y = this.owner.y - 100 + _loc2_ * 120 / _loc6_;
            }
         }
         if(com.nitrome.throwgame.Controller.tileSystem.mouseButtonDown && !this.draggingPin && com.nitrome.throwgame.Controller.dragging != this)
         {
            _loc3_ = this.mc._xmouse - this.mc.pin._x;
            _loc2_ = this.mc._ymouse;
            _loc4_ = _loc3_ * _loc3_ + _loc2_ * _loc2_;
            if(_loc4_ < 64)
            {
               this.draggingPin = true;
            }
            else
            {
               _loc3_ = this.mc._xmouse;
               _loc2_ = this.mc._ymouse;
               _loc4_ = _loc3_ * _loc3_ + _loc2_ * _loc2_;
               if(_loc4_ < 400)
               {
                  com.nitrome.throwgame.Controller.dragging = this;
               }
            }
         }
         if(this.draggingPin)
         {
            _loc3_ = com.nitrome.throwgame.Controller.content._xmouse - this.x;
            _loc2_ = com.nitrome.throwgame.Controller.content._ymouse - this.y;
            this.rotation = Math.atan2(_loc2_,_loc3_) * 180 / 3.141592653589793;
            this.rotation += 180;
            this.rotation %= 360;
            this.rotation = Math.round(this.rotation);
            this.mc.pin._x = this.mc._xmouse;
            if(this.mc.pin._x < -40)
            {
               this.mc.pin._x = -40;
            }
            if(this.mc.pin._x > -21)
            {
               this.mc.pin._x = -21;
            }
            if(!com.nitrome.throwgame.Controller.tileSystem.mouseButtonDown)
            {
               _loc5_ = this.mc.pin._x < -30;
               this.fireStrength = !_loc5_ ? 0 : 30;
               this.draggingPin = false;
               if(_loc5_)
               {
                  this.owner.canShoot = false;
                  this.owner.canThrow = false;
               }
            }
         }
         else
         {
            this.mc.pin._x += 15;
            if(this.mc.pin._x >= -21)
            {
               this.mc.pin._x = -21;
               if(this.fireStrength > 4)
               {
                  this.fire(this.fireStrength * com.nitrome.util.Trig.cosTable[this.rotation],this.fireStrength * com.nitrome.util.Trig.sinTable[this.rotation]);
               }
            }
         }
      }
      this.update();
   }
   function fire(vx, vy)
   {
      this.fired = true;
      this.cannonball = new com.nitrome.throwgame.Cannonball();
      this.cannonball.x = this.x;
      this.cannonball.y = this.y;
      this.cannonball.show();
      this.cannonball.update();
      this.cannonball.owner = this.owner;
      this.cannonball.fire(vx,vy);
      _root.sfx_manager.playSound("cannon explosion");
   }
   function randomThrows(count)
   {
      var _loc12_ = [];
      var _loc4_ = 0;
      var _loc11_;
      var _loc3_;
      var _loc10_;
      var _loc8_;
      var _loc2_;
      while(_loc4_ < count)
      {
         _loc11_ = Math.floor(Math.random() * 360);
         _loc3_ = Math.random() * this.dragRange / 2;
         _loc10_ = this.owner.x + com.nitrome.util.Trig.cosTable[_loc11_] * _loc3_;
         _loc8_ = this.owner.y + this.dragOffset + com.nitrome.util.Trig.sinTable[_loc11_] * _loc3_;
         this.velocityX = _loc10_ - this.x;
         this.velocityY = _loc8_ - this.y;
         this.advanceMotion();
         this.velocityX = 0;
         this.velocityY = 0;
         _loc11_ = Math.floor(Math.random() * 360);
         _loc2_ = new com.nitrome.throwgame.Cannonball();
         _loc2_.x = this.x;
         _loc2_.y = this.y;
         _loc2_.setSimulation();
         _loc2_.fire(30 * com.nitrome.util.Trig.cosTable[_loc11_],30 * com.nitrome.util.Trig.sinTable[_loc11_]);
         while(!_loc2_.isSimulationFinished())
         {
            _loc2_.advanceMotion();
         }
         _loc12_.push({startX:_loc10_,startY:_loc8_,vx:30 * com.nitrome.util.Trig.cosTable[_loc11_],vy:30 * com.nitrome.util.Trig.sinTable[_loc11_],ex:_loc2_.x,ey:_loc2_.y,angle:_loc11_});
         _loc2_.destroy();
         _loc4_ = _loc4_ + 1;
      }
      return _loc12_;
   }
   function aiPerform(details)
   {
      this.velocityX = details.startX - this.x;
      this.velocityY = details.startY - this.y;
      this.advanceMotion();
      this.velocityX = 0;
      this.velocityY = 0;
      this.rotation = details.angle;
      this.aiFireDetails = details;
      this.aiFireTime = 25;
   }
}
