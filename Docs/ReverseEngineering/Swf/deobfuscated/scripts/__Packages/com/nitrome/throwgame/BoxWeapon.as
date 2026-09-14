var §\x01§ = 77;
var §\x0f§ = 1;
class com.nitrome.throwgame.BoxWeapon extends com.nitrome.throwgame.Weapon
{
   var aiList;
   var aiNextX;
   var aiNextY;
   var bottomExtent;
   var createMore;
   var draggable;
   var hitsBoxes;
   var leftExtent;
   var mc;
   var rightExtent;
   var show;
   var topExtent;
   var nextBox = null;
   var aiDelay = 0;
   var aiDelayAfter = 0;
   var aiIndex = 0;
   var aiOffset = 0;
   function BoxWeapon(linkageName)
   {
      super(linkageName);
      this.leftExtent = this.topExtent = 16;
      this.rightExtent = this.bottomExtent = 15;
      this.createMore = 2;
      this.limitedToTurn = false;
      this.draggable = false;
      this.hitsBoxes = true;
      this.showCircle = false;
   }
   function advance()
   {
      var _loc2_;
      if(this.owner.team.aiControlled && this.aiList && this.owner.equippedWeapon == this && !this.finished)
      {
         if(this.aiDelay > 0)
         {
            this.aiDelay--;
            if(this.aiDelay < 1)
            {
               _loc2_ = this;
               while(_loc2_.nextBox)
               {
                  _loc2_ = _loc2_.nextBox;
               }
               _loc2_.place(this.aiNextX,this.aiNextY);
               _loc2_.show();
               this.aiDelayAfter = 10;
            }
         }
         else if(this.aiDelayAfter > 0)
         {
            this.aiDelayAfter--;
            if(this.aiDelayAfter < 1)
            {
               this.aiContinue();
            }
         }
         this.trackX = this.aiNextX;
         this.trackY = this.aiNextY;
      }
      if(this.nextBox)
      {
         this.nextBox.advance();
         if(this.nextBox.finished && !this.finished)
         {
            this.finished = true;
         }
      }
      if(!this.fired && !this.owner.team.aiControlled)
      {
         if(com.nitrome.throwgame.Controller.tileSystem.mouseButtonDown)
         {
            if(this.canPlace(com.nitrome.throwgame.Controller.content._xmouse,com.nitrome.throwgame.Controller.content._ymouse))
            {
               this.place(com.nitrome.throwgame.Controller.content._xmouse,com.nitrome.throwgame.Controller.content._ymouse);
               this.show();
               com.nitrome.throwgame.Controller.tileSystem.mouseButtonDown = false;
            }
         }
      }
   }
   function advanceMotion()
   {
      if(this.fired)
      {
         super.advanceMotion();
         this.update();
      }
   }
   function place(x, y)
   {
      super.place(x,y);
      com.nitrome.throwgame.Controller.boxes.push(this);
      this.fired = true;
      this.track = false;
      this.owner.canShoot = false;
      this.owner.canThrow = false;
      if(this.createMore > 0)
      {
         if(this instanceof com.nitrome.throwgame.GunpowderBarrel)
         {
            this.nextBox = new com.nitrome.throwgame.GunpowderBarrel();
         }
         else
         {
            this.nextBox = new com.nitrome.throwgame.WoodenCrate();
         }
         this.nextBox.owner = this.owner;
         this.nextBox.createMore = this.createMore - 1;
      }
      else
      {
         this.finished = true;
      }
   }
   function canPlace(px, py)
   {
      var _loc16_ = px - 16 >> 5;
      var _loc15_ = px + 16 >> 5;
      var _loc18_ = py - 16 >> 5;
      var _loc14_ = py + 16 >> 5;
      var _loc4_;
      var _loc2_;
      _loc4_ = _loc16_;
      loop0:
      while(_loc4_ <= _loc15_)
      {
         _loc2_ = _loc18_;
         while(true)
         {
            if(_loc2_ <= _loc14_)
            {
               if(com.nitrome.throwgame.Controller.tileSystem.tileGrid[_loc4_][_loc2_])
               {
                  break;
               }
               _loc2_ = _loc2_ + 1;
               continue;
            }
            _loc4_ = _loc4_ + 1;
            continue loop0;
         }
         return false;
      }
      var _loc9_ = _loc14_ + 1 << 5;
      var _loc17_ = false;
      _loc2_ = _loc14_;
      while(_loc2_ < com.nitrome.throwgame.Controller.tileSystem.levelHeight)
      {
         _loc9_ = _loc2_ + 1 << 5;
         _loc4_ = _loc16_;
         while(_loc4_ <= _loc15_)
         {
            if(com.nitrome.throwgame.Controller.tileSystem.tileGrid[_loc4_][_loc2_])
            {
               _loc17_ = true;
               break;
            }
            _loc4_ = _loc4_ + 1;
         }
         if(_loc17_)
         {
            break;
         }
         _loc2_ = _loc2_ + 1;
      }
      var _loc10_ = 0;
      var _loc3_;
      while(_loc10_ < com.nitrome.throwgame.Controller.boxes.length)
      {
         _loc3_ = com.nitrome.throwgame.Controller.boxes[_loc10_];
         if(_loc3_.x - _loc3_.leftExtent <= px + 16)
         {
            if(_loc3_.x + _loc3_.rightExtent >= px - 15)
            {
               if(_loc3_.y + _loc3_.bottomExtent >= py - 15)
               {
                  if(_loc3_.y - _loc3_.topExtent < _loc9_)
                  {
                     _loc9_ = _loc3_.y - _loc3_.topExtent;
                  }
                  if(_loc3_.y - _loc3_.topExtent <= py + 16)
                  {
                     return false;
                  }
               }
            }
         }
         _loc10_ = _loc10_ + 1;
      }
      var _loc13_ = 0;
      var _loc12_;
      while(_loc13_ < com.nitrome.throwgame.Controller.chests.length)
      {
         _loc12_ = com.nitrome.throwgame.Controller.chests[_loc13_];
         if(_loc12_.x - 16 <= px + 16)
         {
            if(_loc12_.x + 16 >= px - 15)
            {
               if(_loc12_.y + 16 >= py - 16)
               {
                  return false;
               }
            }
         }
         _loc13_ = _loc13_ + 1;
      }
      var _loc11_ = 0;
      var _loc7_;
      var _loc5_;
      var _loc1_;
      loop6:
      while(_loc11_ < com.nitrome.throwgame.Controller.teams.length)
      {
         _loc7_ = com.nitrome.throwgame.Controller.teams[_loc11_];
         _loc5_ = 0;
         while(true)
         {
            if(_loc5_ < _loc7_.characters.length)
            {
               _loc1_ = _loc7_.characters[_loc5_];
               if(_loc1_.alive)
               {
                  if(_loc1_.x - _loc1_.leftExtent <= px + 16)
                  {
                     if(_loc1_.x + _loc1_.rightExtent >= px - 15)
                     {
                        if(_loc1_.y + _loc1_.bottomExtent >= py - 15)
                        {
                           if(_loc1_.y - _loc1_.topExtent <= _loc9_)
                           {
                              break;
                           }
                        }
                     }
                  }
               }
               continue;
            }
            continue loop6;
            _loc11_ = _loc11_ + 1;
            _loc5_ = _loc5_ + 1;
         }
         return false;
      }
      return true;
   }
   function explode()
   {
      var _loc2_ = com.nitrome.throwgame.Controller.boxes.length - 1;
      while(_loc2_ >= 0)
      {
         if(com.nitrome.throwgame.Controller.boxes[_loc2_] == this)
         {
            com.nitrome.throwgame.Controller.boxes.splice(_loc2_,1);
         }
         _loc2_ = _loc2_ - 1;
      }
      this.mc.gotoAndPlay("explode");
   }
   function aiSimulation()
   {
      var _loc17_ = 10;
      var _loc12_ = 50;
      var _loc10_ = 0;
      var _loc16_ = 0;
      var _loc6_ = 0;
      while(_loc6_ < this.owner.team.characters.length)
      {
         if(this.owner.team.characters[_loc6_].alive)
         {
            _loc10_ += this.owner.team.characters[_loc6_].x;
            _loc16_ = _loc16_ + 1;
         }
         _loc6_ = _loc6_ + 1;
      }
      _loc10_ /= _loc16_;
      var _loc3_ = com.nitrome.throwgame.Controller.teams[2 - this.owner.team.number].characters;
      var _loc11_ = [];
      _loc6_ = 0;
      while(_loc6_ < _loc3_.length)
      {
         if(_loc3_[_loc6_].alive)
         {
            _loc11_.push(_loc3_[_loc6_]);
         }
         _loc6_ = _loc6_ + 1;
      }
      var _loc15_ = [];
      _loc6_ = 0;
      var _loc9_;
      var _loc2_;
      var _loc8_;
      var _loc7_;
      var _loc5_;
      var _loc4_;
      while(_loc6_ < _loc17_)
      {
         _loc9_ = Math.floor(Math.random() * _loc11_.length);
         _loc2_ = _loc3_[_loc9_];
         _loc8_ = com.nitrome.util.Global.sign(_loc10_ - _loc2_.x) * Math.floor(16 + Math.random() * 48);
         _loc7_ = Math.floor(Math.random() * _loc12_ * 2) - _loc12_;
         _loc5_ = _loc2_.x + _loc8_;
         _loc4_ = _loc2_.y + _loc7_;
         if(this.canPlace(_loc5_,_loc4_))
         {
            _loc15_.push({x:_loc5_,y:_loc4_});
         }
         _loc6_ = _loc6_ + 1;
      }
      return _loc15_;
   }
   function aiPerform(details)
   {
      this.aiList = details.possibilities.slice(0,3);
      this.aiContinue();
   }
   function aiContinue()
   {
      var _loc4_ = false;
      this.aiDelay = 40;
      if(this.canPlace(this.aiNextX,this.aiNextY - 48) && Math.random() >= 0.4)
      {
         this.aiOffset -= 48;
         this.aiNextY -= 48;
         this.trackY -= 48;
         return undefined;
      }
      var _loc3_;
      var _loc2_;
      while(!_loc4_)
      {
         this.aiIndex++;
         if(this.aiIndex >= this.aiList.length)
         {
            this.aiIndex = 0;
            this.aiOffset -= 32;
         }
         _loc3_ = this.aiList[this.aiIndex].x;
         _loc2_ = this.aiList[this.aiIndex].y + this.aiOffset;
         if(this.canPlace(_loc3_,_loc2_))
         {
            this.aiNextX = _loc3_;
            this.aiNextY = _loc2_;
            this.trackX = _loc3_;
            this.trackY = _loc2_;
            this.track = true;
            _loc4_ = true;
            break;
         }
      }
   }
}
