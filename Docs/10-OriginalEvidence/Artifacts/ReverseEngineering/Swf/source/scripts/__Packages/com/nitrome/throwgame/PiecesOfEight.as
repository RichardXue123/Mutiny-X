function §\x04\x05§()
{
   set("\x03",350 % 511 * true);
   return eval("\x03");
}
var §\x01§ = -333 + "\x04\x05"();
var _loc2_;
while(true)
{
   if(eval("\x01") == 17)
   {
      set("\x01",eval("\x01") + 361);
      §§push(true);
   }
   else if(eval("\x01") == 996)
   {
      set("\x01",eval("\x01") - 22);
      var §§pop() = §§pop();
   }
   else if(eval("\x01") == 974)
   {
      set("\x01",eval("\x01") - 676);
      §§push("\x0f");
   }
   else if(eval("\x01") == 975)
   {
      set("\x01",eval("\x01") - 953);
   }
   else if(eval("\x01") == 581)
   {
      set("\x01",eval("\x01") - 506);
      §§push(!§§pop());
   }
   else if(eval("\x01") == 75)
   {
      set("\x01",eval("\x01") + 900);
      if(§§pop())
      {
         set("\x01",eval("\x01") - 953);
      }
   }
   else if(eval("\x01") == 378)
   {
      set("\x01",eval("\x01") + 282);
      if(§§pop())
      {
         set("\x01",eval("\x01") + 167);
      }
   }
   else if(eval("\x01") == 827)
   {
      set("\x01",eval("\x01") - 419);
   }
   else
   {
      if(eval("\x01") == 660)
      {
         set("\x01",eval("\x01") + 167);
         break;
      }
      if(eval("\x01") == 22)
      {
         set("\x01",eval("\x01") + 247);
         if(!_global.com)
         {
            _global.com = new Object();
         }
         §§pop();
         if(!_global.com.nitrome)
         {
            _global.com.nitrome = new Object();
         }
         §§pop();
         if(!_global.com.nitrome.throwgame)
         {
            _global.com.nitrome.throwgame = new Object();
         }
         §§pop();
         if(!_global.com.nitrome.throwgame.PiecesOfEight)
         {
            com.nitrome.throwgame.PiecesOfEight extends com.nitrome.throwgame.Weapon;
            _loc2_ = com.nitrome.throwgame.PiecesOfEight = function()
            {
               super("piecesOfEight");
               this.leftExtent = this.rightExtent = this.topExtent = this.bottomExtent = 7;
               this.show();
               this.hitsBoxes = true;
               this.draggable = false;
               this.twangable = true;
            }.prototype;
            _loc2_.contact = function(side)
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
            };
            _loc2_.advance = function()
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
                  this.aiWait = this.aiWait - 1;
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
            };
            _loc2_.next = function(explode)
            {
               if(explode)
               {
                  new com.nitrome.throwgame.Explosion(this.x,this.y,50,25,this.owner);
                  _root.sfx_manager.playSound("pop");
               }
               this.timesFired = this.timesFired + 1;
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
            };
            _loc2_.aiContinue = function()
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
            };
            _loc2_.timesFired = 0;
            _loc2_.aiWait = 0;
            §§push(ASSetPropFlags(com.nitrome.throwgame.PiecesOfEight.prototype,null,1));
         }
         §§pop();
         break;
      }
      if(eval("\x01") == 298)
      {
         set("\x01",eval("\x01") + 283);
         §§push(eval(§§pop()));
      }
      else if(eval("\x01") == 408)
      {
         set("\x01",eval("\x01") + 588);
         §§push("\x0f");
         §§push(1);
      }
      else
      {
         if(eval("\x01") == 269)
         {
            set("\x01",eval("\x01") - 269);
            break;
         }
         if(eval("\x01") != 620)
         {
            break;
         }
         set("\x01",eval("\x01") - 212);
      }
   }
}
