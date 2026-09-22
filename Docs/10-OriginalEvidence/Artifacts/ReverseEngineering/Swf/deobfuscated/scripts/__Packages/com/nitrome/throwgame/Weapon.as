var §\x01§ = 469;
var §\x0f§ = 1;
class com.nitrome.throwgame.Weapon extends com.nitrome.throwgame.Solid
{
   var advanceMotion;
   var drawTwangLine;
   var isWeapon;
   var splashCheck;
   var twangMaxForce;
   var velocityX;
   var velocityY;
   var x;
   var y;
   var owner = null;
   var fired = false;
   var finished = false;
   var simulation = false;
   var simulationFinished = false;
   var placeableWeapon = false;
   var limitedToTurn = true;
   var trackX = 0;
   var trackY = 0;
   var track = false;
   var dragRange = 130;
   var dragOffset = -100;
   var showCircle = true;
   function Weapon(linkageName)
   {
      super(com.nitrome.throwgame.Controller.characterLayer,linkageName);
      this.isWeapon = true;
   }
   function update()
   {
      super.update();
      this.trackX = this.x;
      this.trackY = this.y;
   }
   function release()
   {
      var _loc3_ = Math.sqrt(this.velocityX * this.velocityX + this.velocityY * this.velocityY);
      var _loc2_ = 20;
      if(_loc3_ > _loc2_)
      {
         this.velocityX = _loc2_ * this.velocityX / _loc3_;
         this.velocityY = _loc2_ * this.velocityY / _loc3_;
      }
      this.fire(this.velocityX,this.velocityY);
      this.finished = false;
      this.owner.canShoot = false;
      this.owner.canThrow = false;
   }
   function twang()
   {
      super.twang();
      this.fired = true;
      this.track = true;
      this.owner.canShoot = false;
      this.owner.canThrow = false;
   }
   function advance()
   {
      this.advanceMotion();
      this.update();
      if(this.fired && !this.finished)
      {
         if(this.velocityY > 0 && this.y > com.nitrome.throwgame.Controller.tileSystem.levelHeight << 5)
         {
            this.finished = true;
         }
         else if(this.velocityX == 0 && Math.abs(this.velocityY) < 0.2)
         {
            this.finished = true;
         }
         if(!this.simulation)
         {
            this.splashCheck();
         }
      }
      if(!this.fired && com.nitrome.throwgame.Controller.twanging == this)
      {
         this.drawTwangLine();
      }
   }
   function randomThrows(count)
   {
      var _loc10_ = [];
      var _loc12_ = this.x;
      var _loc11_ = this.y;
      this.simulation = true;
      var _loc3_ = 0;
      var _loc4_;
      var _loc5_;
      var _loc7_;
      var _loc6_;
      var _loc2_;
      while(_loc3_ < count)
      {
         _loc4_ = 180 + int(Math.random() * 180);
         _loc5_ = 5 + Math.random() * (this.twangMaxForce - 5);
         _loc7_ = com.nitrome.util.Trig.cosTable[_loc4_] * _loc5_;
         _loc6_ = com.nitrome.util.Trig.sinTable[_loc4_] * _loc5_;
         _loc2_ = 0;
         this.x = _loc12_;
         this.y = _loc11_;
         this.velocityX = _loc7_;
         this.velocityY = _loc6_;
         this.simulationFinished = false;
         while(!this.simulationFinished)
         {
            this.advanceMotion();
            if((_loc2_ = _loc2_ + 1) > 100)
            {
               break;
            }
         }
         _loc10_.push({vx:_loc7_,vy:_loc6_,ex:this.x,ey:this.y});
         _loc3_ = _loc3_ + 1;
      }
      this.x = _loc12_;
      this.y = _loc11_;
      this.velocityX = 0;
      this.velocityY = 0;
      this.simulation = false;
      return _loc10_;
   }
   function aiPerform(details)
   {
      this.fire(details.vx,details.vy);
   }
   function fire(vx, vy)
   {
      if(!this.fired)
      {
         this.velocityX = vx;
         this.velocityY = vy;
         this.fired = true;
         this.track = true;
      }
   }
   function place(x, y)
   {
      if(!this.fired)
      {
         this.x = x;
         this.y = y;
         this.fired = true;
         this.track = true;
      }
   }
}
