var §\x01§ = 269;
var §\x0f§ = 1;
class com.nitrome.throwgame.PiecesOfEight extends com.nitrome.throwgame.Weapon
{
   var aiPerform;
   var bottomExtent;
   var draggable;
   var finished;
   var fired;
   var hide;
   var hitsBoxes;
   var leftExtent;
   var owner;
   var randomThrows;
   var rightExtent;
   var show;
   var simulation;
   var simulationFinished;
   var topExtent;
   var track;
   var twangable;
   var velocityX;
   var velocityY;
   var x;
   var y;
   var timesFired = 0;
   var aiWait = 0;
   function PiecesOfEight()
   {
      super("piecesOfEight");
      this.leftExtent = this.rightExtent = this.topExtent = this.bottomExtent = 7;
      this.show();
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
         this.next(true);
      }
   }
   function advance()
   {
      if(com.nitrome.throwgame.Controller.twanging != this && !this.fired)
      {
         this.x = this.owner.x;
         this.y = this.owner.y + 5;
         this.velocityX = 0;
         this.velocityY = 0;
      }
      if(this.aiWait > 0)
      {
         this.aiWait--;
         if(this.aiWait < 1)
         {
            this.aiContinue();
         }
      }
      super.advance();
      if(this.y >= com.nitrome.throwgame.Controller.water.y)
      {
         this.next(false);
      }
   }
   function next(explode)
   {
      if(explode)
      {
         new com.nitrome.throwgame.Explosion(this.x,this.y,50,25,this.owner);
         _root.sfx_manager.playSound("pop");
      }
      this.timesFired++;
      if(this.timesFired < 8)
      {
         this.fired = false;
         this.track = false;
         this.owner.equip(this.owner.equippedWeaponIndex);
         com.nitrome.throwgame.Controller.tileSystem.panToCharacter = this.owner;
         if(this.owner.team.aiControlled)
         {
            this.aiWait = 20;
         }
         this.owner.weaponLocked = true;
      }
      else
      {
         this.finished = true;
         this.hide();
      }
   }
   function aiContinue()
   {
      var _loc15_ = this.randomThrows(10);
      var _loc17_ = _loc15_[0];
      var _loc16_ = - Infinity;
      var _loc14_ = this.owner.team.characters;
      var _loc13_ = com.nitrome.throwgame.Controller.teams[2 - this.owner.team.number].characters;
      var _loc9_;
      var _loc7_;
      var _loc6_;
      var _loc8_;
      var _loc3_;
      var _loc2_;
      var _loc4_;
      var _loc10_;
      var _loc11_;
      var _loc12_ = 0;
      var _loc5_;
      while(_loc12_ < _loc15_.length)
      {
         _loc5_ = _loc15_[_loc12_];
         _loc11_ = 0;
         _loc6_ = 0;
         while(_loc6_ < _loc13_.length)
         {
            _loc8_ = _loc13_[_loc6_];
            if(_loc8_.alive)
            {
               _loc3_ = _loc5_.ex - _loc8_.x;
               _loc2_ = _loc5_.ey - _loc8_.y;
               _loc4_ = _loc3_ * _loc3_ + _loc2_ * _loc2_;
               if(_loc4_ < 4900)
               {
                  _loc10_ = Math.sqrt(_loc4_);
                  _loc11_ += 1 - _loc10_ / 70;
               }
            }
            _loc6_ = _loc6_ + 1;
         }
         _loc9_ = 0;
         while(_loc9_ < _loc14_.length)
         {
            _loc7_ = _loc14_[_loc9_];
            if(_loc7_.alive)
            {
               _loc3_ = _loc5_.ex - _loc7_.x;
               _loc2_ = _loc5_.ey - _loc7_.y;
               _loc4_ = _loc3_ * _loc3_ + _loc2_ * _loc2_;
               if(_loc4_ < 4900)
               {
                  _loc10_ = Math.sqrt(_loc4_);
                  _loc11_ -= 1.5 - _loc10_ / 70;
               }
            }
            _loc9_ = _loc9_ + 1;
         }
         if(_loc11_ > _loc16_)
         {
            _loc16_ = _loc11_;
            _loc17_ = _loc5_;
         }
         _loc12_ = _loc12_ + 1;
      }
      this.aiPerform(_loc17_);
   }
}
