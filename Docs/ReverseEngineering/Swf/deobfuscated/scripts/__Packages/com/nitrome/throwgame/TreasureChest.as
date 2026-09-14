var §\x01§ = 813;
var §\x0f§ = 1;
class com.nitrome.throwgame.TreasureChest extends com.nitrome.util.Clip
{
   var containsWeapons;
   var floorY;
   var releasedWeapon;
   var toNextWeapon;
   static var maxCount;
   static var potentialWeaponList;
   var falling = true;
   var characterTouched = null;
   var finished = false;
   var timeTaken = 0;
   var visibility = 1;
   function TreasureChest(x, floorY)
   {
      super(com.nitrome.throwgame.Controller.objectLayer,"treasureChest");
      var _loc6_ = 1 + Math.floor(Math.random() * 3);
      this.containsWeapons = [];
      var _loc4_ = 0;
      var _loc5_;
      while(_loc4_ < _loc6_)
      {
         _loc5_ = Math.floor(Math.random() * com.nitrome.throwgame.TreasureChest.potentialWeaponList.length);
         this.containsWeapons.push(com.nitrome.throwgame.TreasureChest.potentialWeaponList[_loc5_]);
         trace(this.containsWeapons[_loc4_]);
         _loc4_ = _loc4_ + 1;
      }
      this.x = x;
      this.y = -300;
      this.floorY = floorY;
      this.show();
      this.mc.gotoAndStop("falling");
      _root.sfx_manager.playSound("chest_appear");
   }
   static function dropNew()
   {
      if(com.nitrome.throwgame.Controller.chests.length >= com.nitrome.throwgame.TreasureChest.maxCount)
      {
         return undefined;
      }
      var _loc12_ = Math.floor(Math.random() * com.nitrome.throwgame.Controller.tileSystem.validDropColumns.length);
      var _loc10_ = com.nitrome.throwgame.Controller.tileSystem.validDropColumns[_loc12_];
      var _loc11_ = false;
      var _loc5_ = 0;
      while(_loc5_ < com.nitrome.throwgame.Controller.tileSystem.levelHeight)
      {
         if(com.nitrome.throwgame.Controller.tileSystem.tileGrid[_loc10_][_loc5_])
         {
            _loc11_ = true;
            break;
         }
         _loc5_ = _loc5_ + 1;
      }
      if(!_loc11_)
      {
         return undefined;
      }
      var _loc9_ = _loc5_ << 5;
      var _loc3_ = (_loc10_ << 5) + 16;
      var _loc7_ = 0;
      var _loc4_;
      var _loc2_;
      var _loc1_;
      loop1:
      while(_loc7_ < com.nitrome.throwgame.Controller.teams.length)
      {
         _loc4_ = com.nitrome.throwgame.Controller.teams[_loc7_];
         _loc2_ = 0;
         while(true)
         {
            if(_loc2_ < _loc4_.characters.length)
            {
               _loc1_ = _loc4_.characters[_loc2_];
               if(_loc1_.alive)
               {
                  if(_loc1_.x >= _loc3_ - 32)
                  {
                     if(_loc1_.x <= _loc3_ + 32)
                     {
                        if(_loc1_.y <= _loc9_ + 32)
                        {
                           break;
                        }
                     }
                  }
               }
               continue;
            }
            continue loop1;
            _loc7_ = _loc7_ + 1;
            _loc2_ = _loc2_ + 1;
         }
         return undefined;
      }
      var _loc8_ = 0;
      while(_loc8_ < com.nitrome.throwgame.Controller.chests.length)
      {
         if(com.nitrome.throwgame.Controller.chests[_loc8_].x == _loc3_)
         {
            return undefined;
         }
         _loc8_ = _loc8_ + 1;
      }
      var _loc6_ = 0;
      while(_loc6_ < com.nitrome.throwgame.Controller.boxes.length)
      {
         if(Math.abs(com.nitrome.throwgame.Controller.boxes[_loc6_].x - _loc3_) < 32)
         {
            return undefined;
         }
         _loc6_ = _loc6_ + 1;
      }
      com.nitrome.throwgame.Controller.chests.push(new com.nitrome.throwgame.TreasureChest(_loc3_,_loc9_));
   }
   static function fallingChest()
   {
      var _loc1_ = 0;
      while(_loc1_ < com.nitrome.throwgame.Controller.chests.length)
      {
         if(com.nitrome.throwgame.Controller.chests[_loc1_].falling)
         {
            return com.nitrome.throwgame.Controller.chests[_loc1_];
         }
         _loc1_ = _loc1_ + 1;
      }
      return null;
   }
   function advance()
   {
      var _loc6_;
      var _loc5_;
      var _loc4_;
      var _loc3_;
      if(this.finished)
      {
         this.visibility -= 0.1;
      }
      else if(this.falling)
      {
         this.y += 3;
         if(this.y >= this.floorY - 15)
         {
            this.y = this.floorY - 15;
            this.falling = false;
            this.mc.gotoAndPlay("touchdown");
         }
         this.update();
      }
      else if(this.characterTouched)
      {
         this.toNextWeapon--;
         if(this.toNextWeapon <= 0)
         {
            if(this.containsWeapons.length > 0)
            {
               this.toNextWeapon = 40;
               this.mc.gotoAndPlay("weapon_out");
               this.releasedWeapon = this.containsWeapons[0];
               this.containsWeapons.splice(0,1);
               this.characterTouched.hasWeapons.push(this.releasedWeapon);
               com.nitrome.throwgame.Controller.text.say("collected " + com.nitrome.game.WeaponSelectButton.hoverText["hover_" + this.releasedWeapon][0]);
               _root.sfx_manager.playSound("icon_collect");
            }
            else
            {
               this.mc.gotoAndPlay("fade_out");
               this.finished = true;
            }
         }
         this.mc.weapon.gotoAndStop(this.characterTouched.team.number != 1 ? "blue" : "red");
         this.mc.weapon.weapon.gotoAndStop(this.releasedWeapon);
         if(!this.characterTouched.alive)
         {
            this.mc._alpha -= 5;
            this.mc.blendMode = "layer";
            this.visibility -= 0.05;
         }
      }
      else if(!this.falling)
      {
         _loc6_ = 0;
         while(_loc6_ < com.nitrome.throwgame.Controller.teams.length)
         {
            _loc5_ = com.nitrome.throwgame.Controller.teams[_loc6_];
            _loc4_ = 0;
            while(_loc4_ < _loc5_.characters.length)
            {
               _loc3_ = _loc5_.characters[_loc4_];
               if(_loc3_.alive)
               {
                  if(com.nitrome.throwgame.Controller.dragging != _loc3_)
                  {
                     if(_loc3_.x + _loc3_.rightExtent >= this.x - 20)
                     {
                        if(_loc3_.x - _loc3_.leftExtent <= this.x + 20)
                        {
                           if(_loc3_.y + _loc3_.bottomExtent >= this.y - 16)
                           {
                              if(_loc3_.y - _loc3_.topExtent <= this.y + 16)
                              {
                                 this.characterTouched = _loc3_;
                                 break;
                              }
                           }
                        }
                     }
                  }
               }
               _loc4_ = _loc4_ + 1;
            }
            _loc6_ = _loc6_ + 1;
         }
         if(this.characterTouched)
         {
            this.mc.gotoAndPlay("open");
            this.toNextWeapon = 10;
            _root.sfx_manager.playSound("click");
         }
      }
      if((this.falling || this.characterTouched) && !this.finished)
      {
         com.nitrome.throwgame.Controller.inactivity = 0;
      }
      this.timeTaken++;
   }
   function destroy()
   {
      super.destroy();
      var _loc3_ = com.nitrome.throwgame.Controller.chests.length - 1;
      while(_loc3_ >= 0)
      {
         if(com.nitrome.throwgame.Controller.chests[_loc3_] == this)
         {
            com.nitrome.throwgame.Controller.chests.splice(_loc3_,1);
         }
         _loc3_ = _loc3_ - 1;
      }
   }
}
