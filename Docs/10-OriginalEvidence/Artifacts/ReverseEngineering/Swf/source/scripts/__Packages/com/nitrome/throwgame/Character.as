function §\x04\x05§()
{
   set("\x03",1232 % 511 * true);
   return eval("\x03");
}
var §\x01§ = 67 + "\x04\x05"();
var _loc2_;
loop0:
while(true)
{
   if(eval("\x01") == 277)
   {
      set("\x01",eval("\x01") - 146);
      §§push(true);
   }
   else if(eval("\x01") == 786)
   {
      set("\x01",eval("\x01") - 660);
      §§push(true);
   }
   else if(eval("\x01") == 760)
   {
      set("\x01",eval("\x01") - 508);
      §§push("\x0f");
   }
   else if(eval("\x01") == 672)
   {
      set("\x01",eval("\x01") + 181);
   }
   else if(eval("\x01") == 335)
   {
      set("\x01",eval("\x01") + 425);
      var §§pop() = §§pop();
   }
   else if(eval("\x01") == 131)
   {
      set("\x01",eval("\x01") + 180);
      if(§§pop())
      {
         set("\x01",eval("\x01") - 125);
      }
   }
   else
   {
      if(eval("\x01") == 445)
      {
         set("\x01",eval("\x01") - 386);
         break;
      }
      if(eval("\x01") == 367)
      {
         set("\x01",eval("\x01") + 78);
         if(§§pop())
         {
            set("\x01",eval("\x01") - 386);
         }
      }
      else if(eval("\x01") == 781)
      {
         set("\x01",eval("\x01") - 446);
         §§push("\x0f");
         §§push(1);
      }
      else if(eval("\x01") == 520)
      {
         set("\x01",eval("\x01") - 153);
         §§push(true);
      }
      else if(eval("\x01") == 411)
      {
         set("\x01",eval("\x01") - 291);
         §§push(!§§pop());
      }
      else
      {
         if(eval("\x01") != 120)
         {
            while(true)
            {
               if(eval("\x01") == 853)
               {
                  set("\x01",eval("\x01") - 754);
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
                  if(_global.com.nitrome.throwgame.Character)
                  {
                     break;
                  }
                  com.nitrome.throwgame.Character extends com.nitrome.throwgame.Solid;
                  _loc2_ = com.nitrome.throwgame.Character = function(linkageName)
                  {
                     super(com.nitrome.throwgame.Controller.characterLayer,linkageName);
                     this.leftExtent = this.rightExtent = 6;
                     this.topExtent = 8;
                     this.bottomExtent = 8;
                     this.isCharacter = true;
                     this.useHolder = true;
                     this.show();
                     this.mc.onPress = null;
                     this.friction = 2;
                     this.hitsBoxes = true;
                     this.draggable = false;
                     this.twangable = true;
                  }.prototype;
                  _loc2_.setWeapons = function(weaponsAttributes)
                  {
                     this.hasWeapons = [];
                     this.infiniteWeapons = [];
                     var _loc3_;
                     var _loc2_;
                     for(var _loc5_ in weaponsAttributes)
                     {
                        if(!(_loc5_ == "x" || _loc5_ == "y" || _loc5_ == "type" || _loc5_ == "luck" || _loc5_ == "maxChests"))
                        {
                           _loc3_ = Number(weaponsAttributes[_loc5_]);
                           if(_loc3_ == 10)
                           {
                              this.hasWeapons.push(_loc5_);
                              this.infiniteWeapons.push(_loc5_);
                           }
                           else
                           {
                              _loc2_ = 0;
                              while(_loc2_ < _loc3_)
                              {
                                 this.hasWeapons.push(_loc5_);
                                 _loc2_ = _loc2_ + 1;
                              }
                           }
                        }
                     }
                     this.luck = !weaponsAttributes.luck ? 5 : weaponsAttributes.luck;
                  };
                  _loc2_.show = function()
                  {
                     super.show();
                     this.overlay = this.mcHolder.attachMovie("characterOverlay","overlay",this.mcHolder.getNextHighestDepth());
                  };
                  _loc2_.hide = function()
                  {
                     super.hide();
                     this.overlay = null;
                  };
                  _loc2_.updateOverlay = function()
                  {
                     if(!this.overlay)
                     {
                        return undefined;
                     }
                     if(this.team.aiControlled)
                     {
                        this.overlay.gotoAndStop("cpu");
                     }
                     else
                     {
                        this.overlay.gotoAndStop("p" + this.team.number.toString());
                     }
                     var _loc5_ = com.nitrome.throwgame.Controller.currentTeam == this.team;
                     var _loc4_ = com.nitrome.throwgame.Controller.currentTeam.selectedCharacter == this;
                     var _loc7_ = com.nitrome.throwgame.Controller.dragging == this || this.equippedWeapon && com.nitrome.throwgame.Controller.dragging == this.equippedWeapon;
                     var _loc6_ = this.thrown || this.equippedWeapon.fired;
                     var _loc3_ = com.nitrome.throwgame.Controller.dragging == this || this.thrown;
                     this.overlay.triangle._visible = _loc5_ && !_loc3_ && this.alive && com.nitrome.throwgame.Controller.speechBubble.target != this;
                     this.overlay.health.gotoAndStop(1 + Math.ceil(27 * this.shownHealth / this.maxHealth));
                     this.overlay.health._visible = this.alive && !_loc3_;
                     this.overlay.cancelWeapon._visible = _loc4_ && this.weaponSelected && !this.weaponLocked && !_loc7_ && !_loc6_ && this.alive;
                     var _loc2_;
                     if(com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon.weaponType == "voodooDoll")
                     {
                        _loc2_ = com.nitrome.throwgame.VoodooDoll(com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon);
                        this.overlay.target._visible = (_loc2_.targetCharacter == this || _loc2_.targetCharacter == null && com.nitrome.throwgame.Controller.hoverCharacter == this) && !_loc2_.fired;
                        this.overlay.corners._visible = false;
                     }
                     else
                     {
                        this.overlay.target._visible = false;
                        this.overlay.corners._visible = (_loc4_ && !_loc3_ || com.nitrome.throwgame.Controller.hoverCharacter == this) && this.alive;
                     }
                  };
                  _loc2_.advance = function()
                  {
                     var _loc5_ = this.team == com.nitrome.throwgame.Controller.currentTeam && !this.team.selectedCharacter && this.alive;
                     var _loc6_ = this.team != com.nitrome.throwgame.Controller.currentTeam && com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.targetingVoodoo();
                     this.mc.useHandCursor = _loc5_ || _loc6_;
                     var _loc4_;
                     if(this.shownHealth != this.health && this.velocityX == 0 && Math.abs(this.velocityY) < 0.2)
                     {
                        this.shownHealth = com.nitrome.util.Global.slide(this.shownHealth,this.health,1);
                        if(this.shownHealth < 1)
                        {
                           this.mc._visible = false;
                           _loc4_ = new com.nitrome.util.Clip(this.mcHolder,"deadCharacter");
                           _loc4_.y = this.bottomExtent;
                           _loc4_.show();
                           this.alive = false;
                           _root.sfx_manager.playSound("die");
                        }
                     }
                     this.updateOverlay();
                     this.advanceMotion();
                     if(com.nitrome.throwgame.Controller.dragging != this)
                     {
                        this.rotation += this.velocityX * 3;
                     }
                     this.update();
                     if(this.equippedWeapon)
                     {
                        this.equippedWeapon.advance();
                     }
                     if(!this.thrown && com.nitrome.throwgame.Controller.twanging == this)
                     {
                        this.drawTwangLine();
                     }
                     if(this.y > com.nitrome.throwgame.Controller.water.y)
                     {
                        this.health = 0;
                        this.shownHealth = 1;
                        this.alive = false;
                        this.velocityX *= 0.8;
                        this.velocityY *= 0.8;
                        if(this.velocityY > 1.5)
                        {
                           this.velocityY -= 4;
                           if(this.velocityY < 1.5)
                           {
                              this.velocityY = 1.5;
                           }
                           this.rotation += (this.velocityX + this.velocityY) * 4;
                        }
                     }
                     this.splashCheck();
                     if(this.hit)
                     {
                        if(this.velocityX == 0 && Math.abs(this.velocityY) < 0.2)
                        {
                           this.hit = false;
                        }
                        else
                        {
                           this.mc.gotoAndPlay("hit");
                        }
                     }
                     if(!this.alive && this.mapVisibility > 0)
                     {
                        this.mapVisibility -= 0.1;
                     }
                     var _loc3_ = true;
                     if(this.velocityX != 0)
                     {
                        _loc3_ = false;
                     }
                     if(Math.abs(this.velocityY) > 0.2)
                     {
                        _loc3_ = false;
                     }
                     if(this.shownHealth != this.health)
                     {
                        _loc3_ = false;
                     }
                     if(this.team == com.nitrome.throwgame.Controller.currentTeam && this.team.selectedCharacter == this)
                     {
                        if(this.equippedWeapon)
                        {
                           if(!this.equippedWeapon.fired || !this.equippedWeapon.finished)
                           {
                              _loc3_ = false;
                           }
                        }
                        else if(!this.thrown)
                        {
                           _loc3_ = false;
                        }
                     }
                     if(!this.alive && (!this.equippedWeapon.fired || this.equippedWeapon.finished))
                     {
                        _loc3_ = true;
                     }
                     if(this.shownHealth != this.health && this.y < com.nitrome.throwgame.Controller.water.y)
                     {
                        _loc3_ = false;
                     }
                     if(!_loc3_)
                     {
                        com.nitrome.throwgame.Controller.inactivity = 0;
                     }
                     this.contactTime = this.contactTime + 1;
                  };
                  _loc2_.randomThrows = function(count)
                  {
                     var _loc9_ = [];
                     var _loc11_ = this.x;
                     var _loc10_ = this.y;
                     var _loc2_ = 0;
                     var _loc3_;
                     var _loc4_;
                     var _loc6_;
                     var _loc5_;
                     while(_loc2_ < count)
                     {
                        _loc3_ = 180 + int(Math.random() * 180);
                        _loc4_ = 5 + Math.random() * 15;
                        _loc6_ = com.nitrome.util.Trig.cosTable[_loc3_] * _loc4_;
                        _loc5_ = com.nitrome.util.Trig.sinTable[_loc3_] * _loc4_;
                        this.x = _loc11_;
                        this.y = _loc10_;
                        this.velocityX = _loc6_;
                        this.velocityY = _loc5_;
                        while((this.velocityX != 0 || Math.abs(this.velocityY) > 0.2) && this.y < com.nitrome.throwgame.Controller.water.y)
                        {
                           this.advanceMotion();
                        }
                        _loc9_.push({vx:_loc6_,vy:_loc5_,ex:this.x,ey:this.y});
                        _loc2_ = _loc2_ + 1;
                     }
                     this.x = _loc11_;
                     this.y = _loc10_;
                     this.velocityX = 0;
                     this.velocityY = 0;
                     return _loc9_;
                  };
                  _loc2_.targetingVoodoo = function()
                  {
                     return this.equippedWeapon instanceof com.nitrome.throwgame.VoodooDoll && !this.equippedWeapon.fired;
                  };
                  _loc2_.aiStart = function()
                  {
                     this.aiThrowChecked = false;
                     this.aiWeaponsChecked = 0;
                     this.aiFinished = false;
                     this.aiMoveList = [];
                     this.aiRemember = null;
                  };
                  _loc2_.aiThink = function()
                  {
                     if(!this.team.aiControlled)
                     {
                        return undefined;
                     }
                     if(!this.alive)
                     {
                        this.aiFinished = true;
                        return undefined;
                     }
                     var _loc17_ = this.team.characters;
                     var _loc10_ = com.nitrome.throwgame.Controller.teams[2 - this.team.number].characters;
                     var _loc12_;
                     var _loc11_;
                     var _loc3_;
                     var _loc2_;
                     var _loc23_;
                     var _loc22_;
                     var _loc6_;
                     var _loc5_;
                     var _loc4_;
                     var _loc7_;
                     var _loc9_;
                     var _loc40_;
                     var _loc13_;
                     var _loc32_ = this.luck;
                     _loc32_ = Math.floor(_loc32_ * this.team.characters.length / this.team.countAlive());
                     var _loc19_;
                     var _loc29_ = 0;
                     var _loc30_ = 0;
                     _loc3_ = 0;
                     while(_loc3_ < _loc10_.length)
                     {
                        _loc29_ += _loc10_[_loc3_].x;
                        _loc30_ += _loc10_[_loc3_].y;
                        _loc3_ = _loc3_ + 1;
                     }
                     _loc29_ /= _loc10_.length;
                     _loc30_ /= _loc10_.length;
                     var _loc8_;
                     var _loc14_;
                     var _loc18_;
                     var _loc34_;
                     var _loc27_;
                     var _loc28_;
                     var _loc21_;
                     if(!this.aiThrowChecked)
                     {
                        if(this.canThrow)
                        {
                           if(!this.aiThrowsToCheck)
                           {
                              this.aiThrowsToCheck = this.randomThrows(50);
                              this.aiMoveList = [];
                           }
                           _loc8_ = this.aiThrowsToCheck.splice(0,1)[0];
                           _loc14_ = 0;
                           _loc14_ += (_loc8_.ey - this.y) * -0.003;
                           _loc14_ += (Math.abs(_loc8_.ex - _loc29_) - Math.abs(this.x - _loc29_)) / -500;
                           _loc14_ += (Math.abs(_loc8_.ey - _loc30_) - Math.abs(this.y - _loc30_)) / -500;
                           if(_loc8_.ey >= com.nitrome.throwgame.Controller.water.y)
                           {
                              _loc14_ -= 2;
                           }
                           _loc3_ = 0;
                           while(_loc3_ < _loc10_.length)
                           {
                              _loc2_ = _loc10_[_loc3_];
                              if(_loc2_.alive)
                              {
                                 _loc6_ = _loc2_.x - _loc8_.ex;
                                 _loc5_ = _loc2_.y - _loc8_.ey;
                                 _loc4_ = _loc6_ * _loc6_ + _loc5_ * _loc5_;
                                 _loc7_ = Math.sqrt(_loc4_);
                                 if(_loc4_ < 40000)
                                 {
                                    _loc18_ = 0.2 * (1 - _loc7_ / 200);
                                    if(_loc4_ < 10000)
                                    {
                                       _loc18_ *= _loc7_ / 100;
                                       _loc14_ -= (1 - _loc7_ / 100) * 3;
                                    }
                                 }
                                 else
                                 {
                                    _loc18_ = 0.3 * Math.pow(0.75,_loc7_ / 200);
                                 }
                                 _loc14_ += _loc18_;
                                 _loc14_ *= 1 + _loc2_.evilness;
                              }
                              _loc3_ = _loc3_ + 1;
                           }
                           _loc12_ = 0;
                           while(_loc12_ < _loc17_.length)
                           {
                              _loc11_ = _loc17_[_loc12_];
                              if(_loc11_.alive)
                              {
                                 _loc6_ = _loc11_.x - _loc8_.ex;
                                 _loc5_ = _loc11_.y - _loc8_.ey;
                                 _loc4_ = _loc6_ * _loc6_ + _loc5_ * _loc5_;
                                 if(_loc11_ == this)
                                 {
                                    if(_loc4_ < 6400)
                                    {
                                       _loc7_ = Math.sqrt(_loc4_);
                                       _loc14_ -= 1 * (1 - _loc7_ / 80);
                                    }
                                 }
                                 else if(_loc4_ < 1600)
                                 {
                                    _loc7_ = Math.sqrt(_loc4_);
                                    _loc14_ -= 0.1 * (1 - _loc7_ / 40);
                                 }
                              }
                              _loc12_ = _loc12_ + 1;
                           }
                           _loc23_ = 0;
                           while(_loc23_ < com.nitrome.throwgame.Controller.chests.length)
                           {
                              _loc22_ = com.nitrome.throwgame.Controller.chests[_loc23_];
                              if(!_loc22_.finished)
                              {
                                 _loc6_ = _loc22_.x - _loc8_.ex;
                                 _loc5_ = _loc22_.floorY - _loc8_.ey;
                                 _loc4_ = _loc6_ * _loc6_ + _loc5_ * _loc5_;
                                 if(_loc4_ < 1600)
                                 {
                                    _loc14_ += 0.5;
                                 }
                              }
                              _loc23_ = _loc23_ + 1;
                           }
                           _loc34_ = this.firstIndexOfWeapon("cherryBomb");
                           this.equip(_loc34_);
                           _loc27_ = this.equippedWeapon.randomThrows(10);
                           _loc28_ = 0;
                           _loc21_ = 0;
                           while(_loc21_ < _loc27_.length)
                           {
                              _loc18_ = 0;
                              _loc3_ = 0;
                              while(_loc3_ < _loc10_.length)
                              {
                                 _loc2_ = _loc10_[_loc3_];
                                 if(_loc2_.alive)
                                 {
                                    _loc6_ = _loc2_.x - _loc8_.ex;
                                    _loc5_ = _loc2_.y - _loc8_.ey;
                                    _loc4_ = _loc6_ * _loc6_ + _loc5_ * _loc5_;
                                    if(_loc4_ < 10000)
                                    {
                                       _loc7_ = Math.sqrt(_loc4_);
                                       _loc18_ += (1 - _loc7_ / 100) * (1 + _loc2_.evilness) * 0.5;
                                    }
                                 }
                                 _loc3_ = _loc3_ + 1;
                              }
                              _loc6_ = this.x - _loc8_.ex;
                              _loc5_ = this.y - _loc8_.ey;
                              _loc4_ = _loc6_ * _loc6_ + _loc5_ * _loc5_;
                              if(_loc4_ < 6400)
                              {
                                 _loc7_ = Math.sqrt(_loc4_);
                                 _loc18_ -= (1 - _loc7_ / 80) * 1;
                              }
                              if(_loc18_ > 0)
                              {
                                 _loc14_ += _loc18_;
                                 if(_loc14_ > _loc28_)
                                 {
                                    _loc28_ = _loc14_;
                                    this.aiRemember = _loc27_[_loc21_];
                                 }
                              }
                              _loc21_ = _loc21_ + 1;
                           }
                           this.unequip();
                           _loc6_ = _loc8_.ex - this.x;
                           _loc5_ = _loc8_.ey - this.y;
                           _loc4_ = _loc6_ * _loc6_ + _loc5_ * _loc5_;
                           if(_loc4_ < 90000)
                           {
                              _loc7_ = Math.sqrt(_loc4_);
                              _loc14_ += 0.3 * _loc7_ / 300 - 0.3;
                           }
                           else
                           {
                              _loc14_ -= 0.3;
                           }
                           _loc8_.success = _loc14_;
                           _loc8_.player = this;
                           _loc8_.weapon = -1;
                           this.aiMoveList.push(_loc8_);
                           if(this.aiThrowsToCheck.length < 1)
                           {
                              this.aiThrowChecked = true;
                              if(this.hasWeapons.length == 0)
                              {
                                 this.aiFinished = true;
                              }
                              this.aiThrowsToCheck = null;
                           }
                        }
                        else
                        {
                           this.aiMoveList = [];
                           this.aiThrowChecked = true;
                           this.aiFinished = this.hasWeapons.length < 1;
                           this.aiThrowsToCheck = null;
                        }
                        return undefined;
                     }
                     if(!this.canShoot)
                     {
                        this.aiWeaponsChecked = this.hasWeapons.length;
                        this.aiFinished = true;
                        return undefined;
                     }
                     _loc13_ = this.aiWeaponsChecked;
                     var _loc20_;
                     var _loc16_;
                     while(_loc13_ < this.hasWeapons.length)
                     {
                        _loc20_ = false;
                        _loc16_ = 0;
                        while(_loc16_ < _loc13_)
                        {
                           if(this.hasWeapons[_loc16_] == this.hasWeapons[_loc13_])
                           {
                              _loc20_ = true;
                           }
                           _loc16_ = _loc16_ + 1;
                        }
                        if(!_loc20_)
                        {
                           break;
                        }
                        _loc13_ = _loc13_ + 1;
                     }
                     if(_loc13_ >= this.hasWeapons.length)
                     {
                        this.aiWeaponsChecked = this.hasWeapons.length;
                        this.aiFinished = true;
                        return undefined;
                     }
                     var _loc15_;
                     var _loc31_;
                     var _loc33_;
                     if(this.hasWeapons[_loc13_] == "tidalWave")
                     {
                        _loc14_ = 0;
                        _loc3_ = 0;
                        while(_loc3_ < _loc10_.length)
                        {
                           _loc2_ = _loc10_[_loc3_];
                           if(_loc2_.alive)
                           {
                              if(_loc2_.y >= com.nitrome.throwgame.Controller.water.y - 300)
                              {
                                 _loc14_ += _loc2_.health / _loc2_.maxHealth * 0.5;
                              }
                           }
                           _loc3_ = _loc3_ + 1;
                        }
                        _loc12_ = 0;
                        while(_loc12_ < _loc17_.length)
                        {
                           _loc11_ = _loc17_[_loc12_];
                           if(_loc11_.alive)
                           {
                              if(_loc11_.y >= com.nitrome.throwgame.Controller.water.y - 300)
                              {
                                 _loc14_ -= 1.5;
                              }
                           }
                           _loc12_ = _loc12_ + 1;
                        }
                        this.aiMoveList.push({success:_loc14_,player:this,weapon:_loc13_});
                     }
                     else if(this.hasWeapons[_loc13_] == "voodooDoll")
                     {
                        _loc3_ = 0;
                        while(_loc3_ < _loc10_.length)
                        {
                           _loc2_ = _loc10_[_loc3_];
                           if(_loc2_.alive)
                           {
                              _loc15_ = _loc2_.randomThrows(2);
                              _loc9_ = 0;
                              while(_loc9_ < _loc15_.length)
                              {
                                 if(_loc15_[_loc9_].ey >= com.nitrome.throwgame.Controller.water.y)
                                 {
                                    _loc14_ = 1 + Math.random() * 0.2;
                                 }
                                 else
                                 {
                                    _loc14_ = Math.random() * 0.2 - 0.5;
                                 }
                                 this.aiMoveList.push({success:_loc14_,player:this,weapon:_loc13_,vx:_loc15_[_loc9_].vx,vy:_loc15_[_loc9_].vy,target:_loc2_});
                                 _loc9_ = _loc9_ + 1;
                              }
                           }
                           _loc3_ = _loc3_ + 1;
                        }
                     }
                     else if(this.hasWeapons[_loc13_] == "seagull")
                     {
                        this.equip(_loc13_);
                        _loc31_ = com.nitrome.throwgame.Seagull(this.equippedWeapon).aiSimulation();
                        if(_loc31_.shots.length > 1)
                        {
                           this.aiMoveList.push({success:_loc31_.success,player:this,weapon:_loc13_,shots:_loc31_.shots,height:_loc31_.height});
                        }
                        this.unequip();
                     }
                     else if(this.hasWeapons[_loc13_] == "woodenCrate" || this.hasWeapons[_loc13_] == "gunpowderBarrel")
                     {
                        this.equip(_loc13_);
                        _loc33_ = com.nitrome.throwgame.BoxWeapon(this.equippedWeapon).aiSimulation();
                        if(_loc33_.length >= 3)
                        {
                           this.aiMoveList.push({success:Math.random(),player:this,weapon:_loc13_,possibilities:_loc33_});
                        }
                        this.unequip();
                     }
                     else
                     {
                        this.equip(_loc13_);
                        _loc19_ = this.equippedWeapon.randomThrows(_loc32_);
                        if(this.aiRemember && this.aiRemember.weapon == _loc13_)
                        {
                           _loc19_.push(this.aiRemember);
                        }
                        _loc9_ = 0;
                        while(_loc9_ < _loc19_.length)
                        {
                           _loc8_ = _loc19_[_loc9_];
                           _loc14_ = -0.01;
                           _loc3_ = 0;
                           while(_loc3_ < _loc10_.length)
                           {
                              _loc2_ = _loc10_[_loc3_];
                              if(_loc2_.alive)
                              {
                                 _loc6_ = _loc2_.x - _loc8_.ex;
                                 _loc5_ = _loc2_.y - _loc8_.ey;
                                 _loc4_ = _loc6_ * _loc6_ + _loc5_ * _loc5_;
                                 if(_loc4_ < 4900)
                                 {
                                    _loc7_ = Math.sqrt(_loc4_);
                                    _loc14_ += 1.5 - _loc7_ / 70;
                                    _loc14_ *= 1 + _loc2_.evilness;
                                 }
                              }
                              _loc3_ = _loc3_ + 1;
                           }
                           _loc12_ = 0;
                           while(_loc12_ < _loc17_.length)
                           {
                              _loc11_ = _loc17_[_loc12_];
                              if(_loc11_.alive)
                              {
                                 _loc6_ = _loc11_.x - _loc8_.ex;
                                 _loc5_ = _loc11_.y - _loc8_.ey;
                                 _loc4_ = _loc6_ * _loc6_ + _loc5_ * _loc5_;
                                 if(_loc4_ < 1600)
                                 {
                                    _loc7_ = Math.sqrt(_loc4_);
                                    _loc14_ -= 1.5 - _loc7_ / 40;
                                 }
                              }
                              _loc12_ = _loc12_ + 1;
                           }
                           if(this.hasWeapons[_loc13_] == "anchor")
                           {
                              _loc14_ *= 0.5;
                           }
                           if(this.hasWeapons[_loc13_] == "piecesOfEight")
                           {
                              _loc14_ = (_loc14_ - 0.5) * 1.2;
                           }
                           _loc19_[_loc9_].success = _loc14_;
                           _loc19_[_loc9_].player = this;
                           _loc19_[_loc9_].weapon = _loc13_;
                           _loc9_ = _loc9_ + 1;
                        }
                        this.unequip();
                        this.aiMoveList = this.aiMoveList.concat(_loc19_);
                     }
                     this.aiWeaponsChecked = _loc13_ + 1;
                     if(this.aiWeaponsChecked >= this.hasWeapons.length)
                     {
                        this.aiFinished = true;
                     }
                  };
                  _loc2_.contact = function(side)
                  {
                     if(side == com.nitrome.throwgame.Solid.FLOOR)
                     {
                        while(this.rotation < -180)
                        {
                           this.rotation += 360;
                        }
                        while(this.rotation > 180)
                        {
                           this.rotation -= 360;
                        }
                        this.rotation *= 0.5;
                        if(this.rotation > -1 && this.rotation < 1)
                        {
                           this.rotation = 0;
                        }
                     }
                     if(this.contactTime > 5)
                     {
                        _root.sfx_manager.playSound("hitwall");
                     }
                     this.contactTime = 0;
                  };
                  _loc2_.release = function()
                  {
                     if(Math.abs(this.x - this.dragStartX) < 5 && Math.abs(this.y - this.dragStartY) < 5)
                     {
                        this.x = this.dragStartX;
                        this.y = this.dragStartY;
                        this.velocityX = 0;
                        this.velocityY = 0;
                        return undefined;
                     }
                     this.thrown = true;
                     this.throwFinished = false;
                     var _loc3_ = Math.sqrt(this.velocityX * this.velocityX + this.velocityY * this.velocityY);
                     var _loc2_ = 20;
                     if(_loc3_ > _loc2_)
                     {
                        this.velocityX = _loc2_ * this.velocityX / _loc3_;
                        this.velocityY = _loc2_ * this.velocityY / _loc3_;
                     }
                     this.canThrow = false;
                  };
                  _loc2_.twang = function()
                  {
                     super.twang();
                     this.thrown = true;
                     this.canThrow = false;
                  };
                  _loc2_.weaponOrPlayer = function()
                  {
                     if(this.equippedWeapon)
                     {
                        return this.equippedWeapon;
                     }
                     return this;
                  };
                  §§push(_loc2_);
                  §§push("equip");
                  function(index)
                  {
                     if(this.equippedWeapon.fired || this.thrown)
                     {
                        return undefined;
                     }
                     if(index < 0 || index >= this.hasWeapons.length)
                     {
                        if(this.equippedWeapon)
                        {
                           this.unequip();
                        }
                        return undefined;
                     }
                     if(this.hasWeapons[this.equippedWeaponIndex] != this.hasWeapons[index])
                     {
                        if(this.equippedWeapon)
                        {
                           this.unequip();
                        }
                        this.equippedWeaponIndex = index;
                        switch(this.hasWeapons[index])
                        {
                           case "anchor":
                              this.equippedWeapon = new com.nitrome.throwgame.Anchor();
                              break;
                           case "banana":
                              this.equippedWeapon = new com.nitrome.throwgame.Banana();
                              break;
                           case "boulder":
                              this.equippedWeapon = new com.nitrome.throwgame.Boulder();
                              break;
                           case "cannon":
                              this.equippedWeapon = new com.nitrome.throwgame.Cannon();
                              break;
                           case "cherryBomb":
                              this.equippedWeapon = new com.nitrome.throwgame.CherryBomb();
                              break;
                           case "dynamite":
                              this.equippedWeapon = new com.nitrome.throwgame.Dynamite();
                              break;
                           case "gunpowderBarrel":
                              this.equippedWeapon = new com.nitrome.throwgame.GunpowderBarrel();
                              break;
                           case "mine":
                              this.equippedWeapon = new com.nitrome.throwgame.Mine();
                              break;
                           case "parachuteBomb":
                              this.equippedWeapon = new com.nitrome.throwgame.ParachuteBomb();
                              break;
                           case "piecesOfEight":
                              this.equippedWeapon = new com.nitrome.throwgame.PiecesOfEight();
                              break;
                           case "rumBottle":
                              this.equippedWeapon = new com.nitrome.throwgame.RumBottle();
                              break;
                           case "seagull":
                              this.equippedWeapon = new com.nitrome.throwgame.Seagull();
                              break;
                           case "tidalWave":
                              this.equippedWeapon = new com.nitrome.throwgame.TidalWave();
                              break;
                           case "voodooDoll":
                              this.equippedWeapon = new com.nitrome.throwgame.VoodooDoll();
                              break;
                           case "woodenCrate":
                              this.equippedWeapon = new com.nitrome.throwgame.WoodenCrate();
                        }
                     }
                     this.equippedWeapon.x = this.x;
                     this.equippedWeapon.y = this.y - 10;
                     if(this.hasWeapons[index] == "boulder")
                     {
                        this.equippedWeapon.y -= 20;
                     }
                     this.equippedWeapon.velocityX = 0;
                     this.equippedWeapon.velocityY = 0;
                     this.equippedWeapon.owner = this;
                     this.equippedWeapon.weaponType = this.hasWeapons[index];
                     this.equippedWeapon.update();
                  }
               }
               else
               {
                  if(eval("\x01") == 710)
                  {
                     set("\x01",eval("\x01") - 141);
                     return;
                  }
                  if(eval("\x01") == 126)
                  {
                     set("\x01",eval("\x01") + 584);
                     if(§§pop())
                     {
                        set("\x01",eval("\x01") - 141);
                     }
                     continue loop0;
                  }
                  if(eval("\x01") == 762)
                  {
                     set("\x01",eval("\x01") - 242);
                     continue loop0;
                  }
                  if(eval("\x01") == 186)
                  {
                     set("\x01",eval("\x01") + 334);
                     continue loop0;
                  }
                  if(eval("\x01") == 252)
                  {
                     set("\x01",eval("\x01") + 159);
                     §§push(eval(§§pop()));
                     continue loop0;
                  }
                  if(eval("\x01") == 543)
                  {
                     set("\x01",eval("\x01") + 243);
                     continue loop0;
                  }
                  if(eval("\x01") == 59)
                  {
                     set("\x01",eval("\x01") + 727);
                     continue loop0;
                  }
                  if(eval("\x01") == 569)
                  {
                     set("\x01",eval("\x01") + 212);
                     continue loop0;
                  }
                  if(eval("\x01") == 473)
                  {
                     set("\x01",eval("\x01") + 308);
                     continue loop0;
                  }
                  if(eval("\x01") != 311)
                  {
                     if(eval("\x01") == 99)
                     {
                        set("\x01",eval("\x01") - 99);
                     }
                     return;
                  }
                  set("\x01",eval("\x01") - 125);
                  toggleHighQuality();
                  nextFrame();
                  §§pop()[§§pop()] = new §§pop()[§§pop()].GunpowderBarrel();
                  com.nitrome.throwgame.Character = function(linkageName)
                  {
                     super(com.nitrome.throwgame.Controller.characterLayer,linkageName);
                     this.leftExtent = this.rightExtent = 6;
                     this.topExtent = 8;
                     this.bottomExtent = 8;
                     this.isCharacter = true;
                     this.useHolder = true;
                     this.show();
                     this.mc.onPress = null;
                     this.friction = 2;
                     this.hitsBoxes = true;
                     this.draggable = false;
                     this.twangable = true;
                  }.equippedWeapon.x = com.nitrome.throwgame.Character = function(linkageName)
                  {
                     super(com.nitrome.throwgame.Controller.characterLayer,linkageName);
                     this.leftExtent = this.rightExtent = 6;
                     this.topExtent = 8;
                     this.bottomExtent = 8;
                     this.isCharacter = true;
                     this.useHolder = true;
                     this.show();
                     this.mc.onPress = null;
                     this.friction = 2;
                     this.hitsBoxes = true;
                     this.draggable = false;
                     this.twangable = true;
                  }.x;
                  com.nitrome.throwgame.Character = function(linkageName)
                  {
                     super(com.nitrome.throwgame.Controller.characterLayer,linkageName);
                     this.leftExtent = this.rightExtent = 6;
                     this.topExtent = 8;
                     this.bottomExtent = 8;
                     this.isCharacter = true;
                     this.useHolder = true;
                     this.show();
                     this.mc.onPress = null;
                     this.friction = 2;
                     this.hitsBoxes = true;
                     this.draggable = false;
                     this.twangable = true;
                  }.equippedWeapon.y = com.nitrome.throwgame.Character = function(linkageName)
                  {
                     super(com.nitrome.throwgame.Controller.characterLayer,linkageName);
                     this.leftExtent = this.rightExtent = 6;
                     this.topExtent = 8;
                     this.bottomExtent = 8;
                     this.isCharacter = true;
                     this.useHolder = true;
                     this.show();
                     this.mc.onPress = null;
                     this.friction = 2;
                     this.hitsBoxes = true;
                     this.draggable = false;
                     this.twangable = true;
                  }.y - 10;
                  if(com.nitrome.throwgame.Character = function(linkageName)
                  {
                     super(com.nitrome.throwgame.Controller.characterLayer,linkageName);
                     this.leftExtent = this.rightExtent = 6;
                     this.topExtent = 8;
                     this.bottomExtent = 8;
                     this.isCharacter = true;
                     this.useHolder = true;
                     this.show();
                     this.mc.onPress = null;
                     this.friction = 2;
                     this.hitsBoxes = true;
                     this.draggable = false;
                     this.twangable = true;
                  }.hasWeapons[index] == "boulder")
                  {
                     com.nitrome.throwgame.Character = function(linkageName)
                     {
                        super(com.nitrome.throwgame.Controller.characterLayer,linkageName);
                        this.leftExtent = this.rightExtent = 6;
                        this.topExtent = 8;
                        this.bottomExtent = 8;
                        this.isCharacter = true;
                        this.useHolder = true;
                        this.show();
                        this.mc.onPress = null;
                        this.friction = 2;
                        this.hitsBoxes = true;
                        this.draggable = false;
                        this.twangable = true;
                     }.equippedWeapon.y = com.nitrome.throwgame.Character = function(linkageName)
                     {
                        super(com.nitrome.throwgame.Controller.characterLayer,linkageName);
                        this.leftExtent = this.rightExtent = 6;
                        this.topExtent = 8;
                        this.bottomExtent = 8;
                        this.isCharacter = true;
                        this.useHolder = true;
                        this.show();
                        this.mc.onPress = null;
                        this.friction = 2;
                        this.hitsBoxes = true;
                        this.draggable = false;
                        this.twangable = true;
                     }.equippedWeapon.y - 20;
                  }
                  com.nitrome.throwgame.Character = function(linkageName)
                  {
                     super(com.nitrome.throwgame.Controller.characterLayer,linkageName);
                     this.leftExtent = this.rightExtent = 6;
                     this.topExtent = 8;
                     this.bottomExtent = 8;
                     this.isCharacter = true;
                     this.useHolder = true;
                     this.show();
                     this.mc.onPress = null;
                     this.friction = 2;
                     this.hitsBoxes = true;
                     this.draggable = false;
                     this.twangable = true;
                  }.equippedWeapon.velocityX = 0;
                  com.nitrome.throwgame.Character = function(linkageName)
                  {
                     super(com.nitrome.throwgame.Controller.characterLayer,linkageName);
                     this.leftExtent = this.rightExtent = 6;
                     this.topExtent = 8;
                     this.bottomExtent = 8;
                     this.isCharacter = true;
                     this.useHolder = true;
                     this.show();
                     this.mc.onPress = null;
                     this.friction = 2;
                     this.hitsBoxes = true;
                     this.draggable = false;
                     this.twangable = true;
                  }.equippedWeapon.velocityY = 0;
                  com.nitrome.throwgame.Character = function(linkageName)
                  {
                     super(com.nitrome.throwgame.Controller.characterLayer,linkageName);
                     this.leftExtent = this.rightExtent = 6;
                     this.topExtent = 8;
                     this.bottomExtent = 8;
                     this.isCharacter = true;
                     this.useHolder = true;
                     this.show();
                     this.mc.onPress = null;
                     this.friction = 2;
                     this.hitsBoxes = true;
                     this.draggable = false;
                     this.twangable = true;
                  }.equippedWeapon.owner = com.nitrome.throwgame.Character = function(linkageName)
                  {
                     super(com.nitrome.throwgame.Controller.characterLayer,linkageName);
                     this.leftExtent = this.rightExtent = 6;
                     this.topExtent = 8;
                     this.bottomExtent = 8;
                     this.isCharacter = true;
                     this.useHolder = true;
                     this.show();
                     this.mc.onPress = null;
                     this.friction = 2;
                     this.hitsBoxes = true;
                     this.draggable = false;
                     this.twangable = true;
                  };
                  com.nitrome.throwgame.Character = function(linkageName)
                  {
                     super(com.nitrome.throwgame.Controller.characterLayer,linkageName);
                     this.leftExtent = this.rightExtent = 6;
                     this.topExtent = 8;
                     this.bottomExtent = 8;
                     this.isCharacter = true;
                     this.useHolder = true;
                     this.show();
                     this.mc.onPress = null;
                     this.friction = 2;
                     this.hitsBoxes = true;
                     this.draggable = false;
                     this.twangable = true;
                  }.equippedWeapon.weaponType = com.nitrome.throwgame.Character = function(linkageName)
                  {
                     super(com.nitrome.throwgame.Controller.characterLayer,linkageName);
                     this.leftExtent = this.rightExtent = 6;
                     this.topExtent = 8;
                     this.bottomExtent = 8;
                     this.isCharacter = true;
                     this.useHolder = true;
                     this.show();
                     this.mc.onPress = null;
                     this.friction = 2;
                     this.hitsBoxes = true;
                     this.draggable = false;
                     this.twangable = true;
                  }.hasWeapons[index];
                  com.nitrome.throwgame.Character = function(linkageName)
                  {
                     super(com.nitrome.throwgame.Controller.characterLayer,linkageName);
                     this.leftExtent = this.rightExtent = 6;
                     this.topExtent = 8;
                     this.bottomExtent = 8;
                     this.isCharacter = true;
                     this.useHolder = true;
                     this.show();
                     this.mc.onPress = null;
                     this.friction = 2;
                     this.hitsBoxes = true;
                     this.draggable = false;
                     this.twangable = true;
                  }.equippedWeapon.update();
               }
               §§pop()[§§pop()] = §§pop();
               _loc2_.unequip = function()
               {
                  if(this.equippedWeapon.fired || this.thrown)
                  {
                     return undefined;
                  }
                  this.equippedWeapon.destroy();
                  this.equippedWeapon = null;
                  this.equippedWeaponIndex = -1;
               };
               _loc2_.weaponExpired = function()
               {
                  if(this.equippedWeaponIndex >= 0 && this.equippedWeaponIndex < this.hasWeapons.length)
                  {
                     if(!this.weaponIsInfinite(this.hasWeapons[this.equippedWeaponIndex]))
                     {
                        this.hasWeapons.splice(this.equippedWeaponIndex,1);
                     }
                     if(this.equippedWeapon.limitedToTurn)
                     {
                        this.equippedWeapon.destroy();
                     }
                     this.equippedWeapon = null;
                     this.equippedWeaponIndex = -1;
                  }
               };
               _loc2_.subtractHealth = function(amount)
               {
                  this.health -= Math.round(amount);
                  if(this.health <= 0)
                  {
                     this.health = 0;
                     this.alive = false;
                  }
               };
               _loc2_.numberOfWeapon = function(weapon)
               {
                  var _loc3_ = 0;
                  var _loc2_ = 0;
                  while(_loc2_ < this.hasWeapons.length)
                  {
                     if(this.hasWeapons[_loc2_] == weapon)
                     {
                        _loc3_ = _loc3_ + 1;
                     }
                     _loc2_ = _loc2_ + 1;
                  }
                  return _loc3_;
               };
               _loc2_.firstIndexOfWeapon = function(weapon)
               {
                  var _loc2_ = 0;
                  while(_loc2_ < this.hasWeapons.length)
                  {
                     if(this.hasWeapons[_loc2_] == weapon)
                     {
                        return _loc2_;
                     }
                     _loc2_ = _loc2_ + 1;
                  }
                  return -1;
               };
               _loc2_.weaponIsInfinite = function(weapon)
               {
                  var _loc2_ = 0;
                  while(_loc2_ < this.infiniteWeapons.length)
                  {
                     if(this.infiniteWeapons[_loc2_] == weapon)
                     {
                        return true;
                     }
                     _loc2_ = _loc2_ + 1;
                  }
                  return false;
               };
               _loc2_.index = 0;
               _loc2_.alive = true;
               _loc2_.health = 100;
               _loc2_.maxHealth = 100;
               _loc2_.shownHealth = 100;
               _loc2_.overlay = null;
               _loc2_.equippedWeapon = null;
               _loc2_.equippedWeaponIndex = -1;
               _loc2_.weaponSelected = false;
               _loc2_.weaponLocked = false;
               _loc2_.thrown = false;
               _loc2_.throwFinished = false;
               _loc2_.hit = false;
               _loc2_.mapVisibility = 1;
               _loc2_.aiFinished = true;
               _loc2_.aiThrowChecked = false;
               _loc2_.aiWeaponsChecked = 0;
               _loc2_.aiRemember = null;
               _loc2_.canThrow = true;
               _loc2_.canShoot = true;
               _loc2_.evilness = 0;
               _loc2_.luck = 5;
               _loc2_.contactTime = 0;
               §§push(ASSetPropFlags(com.nitrome.throwgame.Character.prototype,null,1));
               break;
            }
            §§pop();
            break;
         }
         set("\x01",eval("\x01") + 552);
         if(§§pop())
         {
            set("\x01",eval("\x01") + 181);
         }
      }
   }
}
