var §\x01§ = 161;
var §\x0f§ = 1;
class com.nitrome.throwgame.Seagull extends com.nitrome.throwgame.Weapon
{
   var aiShotXs;
   var dottedLine;
   var draggable;
   var hide;
   var hitsTiles;
   var mc;
   var shots;
   var show;
   var velocityX;
   var weight;
   var x;
   var y;
   function Seagull()
   {
      super("seagull");
      this.placeableWeapon = true;
      this.shots = [];
      this.hitsTiles = false;
      this.weight = 0;
      this.showCircle = false;
      this.draggable = false;
      this.dottedLine = new com.nitrome.util.Clip(com.nitrome.throwgame.Controller.characterLayer,"dottedLine");
      this.dottedLine.x = 0;
      this.dottedLine.y = com.nitrome.throwgame.Controller.content._ymouse;
      this.dottedLine.show();
   }
   function place(x, y)
   {
      if(!this.fired)
      {
         this.fired = true;
         this.track = true;
         this.owner.canShoot = false;
         this.owner.canThrow = false;
         this.x = x;
         this.y = y;
         com.nitrome.throwgame.Controller.tileSystem.mouseButtonDown = false;
         this.velocityX = 10;
         this.show();
         this.dottedLine.hide();
      }
   }
   function advance()
   {
      if(!this.fired)
      {
         this.dottedLine.x = - com.nitrome.throwgame.Controller.content._x;
         this.dottedLine.y = com.nitrome.throwgame.Controller.content._ymouse;
         this.dottedLine.update();
      }
      super.advance();
      var _loc5_ = false;
      if(this.owner.team.aiControlled)
      {
         if(this.aiShotXs.length > 0 && this.x >= this.aiShotXs[0])
         {
            this.aiShotXs.splice(0,1);
            _loc5_ = true;
         }
      }
      else
      {
         _loc5_ = com.nitrome.throwgame.Controller.tileSystem.mouseButtonDown;
      }
      if(_loc5_)
      {
         if(!this.fired)
         {
            this.place(-300,com.nitrome.throwgame.Controller.content._ymouse);
         }
         else if(!this.finished && this.x < com.nitrome.throwgame.Controller.tileSystem.levelWidth << 5)
         {
            var shot = new com.nitrome.throwgame.Weapon("seagullFire");
            shot.x = this.x - 10;
            shot.y = this.y;
            shot.velocityX = this.velocityX;
            shot.weight = 1;
            shot.owner = this.owner;
            shot.contact = function(side)
            {
               com.nitrome.throwgame.Seagull(shot.owner.equippedWeapon).endShot(this);
               new com.nitrome.throwgame.Explosion(this.x,this.y,50,50,this.owner);
               this.destroy();
            };
            shot.advance = function()
            {
               super.advance();
               if(this.y > com.nitrome.throwgame.Controller.water.y)
               {
                  com.nitrome.throwgame.Seagull(shot.owner.equippedWeapon).endShot(this);
                  this.destroy();
               }
            };
            shot.hitsBoxes = true;
            shot.show();
            this.shots.push(shot);
            this.hide();
            this.show();
            this.mc.gotoAndPlay("shot");
            _root.sfx_manager.playSound("poop" + (Math.floor(Math.random() * 3) + 1));
         }
         com.nitrome.throwgame.Controller.tileSystem.mouseButtonDown = false;
      }
      var _loc4_ = 0;
      while(_loc4_ < this.shots.length)
      {
         this.shots[_loc4_].advance();
         _loc4_ = _loc4_ + 1;
      }
      if(this.x > (com.nitrome.throwgame.Controller.tileSystem.levelWidth << 5) + 275 && this.shots.length < 1)
      {
         this.finished = true;
      }
      this.trackY = this.y + 100;
   }
   function endShot(shot)
   {
      var _loc2_ = this.shots.length - 1;
      while(_loc2_ >= 0)
      {
         if(this.shots[_loc2_] == shot)
         {
            this.shots.splice(_loc2_,1);
         }
         _loc2_ = _loc2_ - 1;
      }
   }
   function destroy()
   {
      super.destroy();
      this.dottedLine.destroy();
   }
   function aiSimulation()
   {
      var _loc15_ = this.owner.team.characters;
      var _loc12_ = com.nitrome.throwgame.Controller.teams[2 - this.owner.team.number].characters;
      var _loc8_;
      var _loc9_;
      var _loc6_;
      var _loc3_;
      var _loc5_;
      var _loc4_;
      var _loc7_;
      var _loc10_;
      var _loc21_ = Infinity;
      _loc3_ = 0;
      while(_loc3_ < _loc12_.length)
      {
         _loc6_ = _loc12_[_loc3_];
         if(_loc6_.alive)
         {
            if(_loc6_.y < _loc21_)
            {
               _loc21_ = _loc6_.y;
            }
         }
         _loc3_ = _loc3_ + 1;
      }
      _loc21_ -= 100;
      _loc21_ -= Math.floor(Math.random() * 100);
      var _loc14_ = [];
      var _loc13_ = 0;
      while(_loc13_ < 10)
      {
         _loc14_.push(Math.floor(Math.random() * (com.nitrome.throwgame.Controller.tileSystem.levelWidth << 5)));
         _loc13_ = _loc13_ + 1;
      }
      _loc14_ = _loc14_.sort(Array.NUMERIC);
      var _loc17_ = [];
      var _loc18_ = 0;
      _loc13_ = 0;
      var _loc16_;
      var _loc2_;
      var _loc11_;
      while(_loc13_ < _loc14_.length)
      {
         _loc16_ = _loc14_[_loc13_];
         _loc2_ = new com.nitrome.throwgame.Weapon();
         _loc2_.x = _loc16_ - 10;
         _loc2_.y = _loc21_;
         _loc2_.velocityX = 10;
         _loc2_.weight = 1;
         _loc2_.contact = function(side)
         {
            this.simulationFinished = true;
         };
         _loc2_.hitsBoxes = true;
         while(!_loc2_.simulationFinished && _loc2_.y < com.nitrome.throwgame.Controller.water.y)
         {
            _loc2_.advanceMotion();
         }
         _loc11_ = 0;
         _loc3_ = 0;
         while(_loc3_ < _loc12_.length)
         {
            _loc6_ = _loc12_[_loc3_];
            if(_loc6_.alive)
            {
               _loc5_ = _loc6_.x - _loc2_.x;
               _loc4_ = _loc6_.y - _loc2_.y;
               _loc7_ = _loc5_ * _loc5_ + _loc4_ * _loc4_;
               if(_loc7_ < 1600)
               {
                  _loc10_ = Math.sqrt(_loc7_);
                  _loc11_ += 1 - _loc10_ / 40;
               }
            }
            _loc3_ = _loc3_ + 1;
         }
         _loc9_ = 0;
         while(_loc9_ < _loc15_.length)
         {
            _loc8_ = _loc15_[_loc9_];
            if(_loc8_.alive)
            {
               _loc5_ = _loc8_.x - _loc2_.x;
               _loc4_ = _loc8_.y - _loc2_.y;
               _loc7_ = _loc5_ * _loc5_ + _loc4_ * _loc4_;
               if(_loc7_ < 1600)
               {
                  _loc10_ = Math.sqrt(_loc7_);
                  _loc11_ -= 1.5 - _loc10_ / 40;
               }
            }
            _loc9_ = _loc9_ + 1;
         }
         if(_loc11_ > 0)
         {
            _loc17_.push(_loc14_[_loc13_]);
            _loc18_ += _loc11_;
         }
         _loc2_.destroy();
         _loc13_ = _loc13_ + 1;
      }
      return {success:_loc18_,shots:_loc17_,height:_loc21_};
   }
   function aiPerform(details)
   {
      this.place(-300,details.height);
      this.aiShotXs = details.shots;
   }
}
