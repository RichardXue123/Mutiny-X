var §\x01§ = 573;
var §\x0f§ = 1;
class com.nitrome.throwgame.TileSystem
{
   var bgTileGrid;
   var bgTileList;
   var lastMouseX;
   var lastMouseY;
   var levelHeight;
   var levelWidth;
   var tileGrid;
   var tileList;
   var validDropColumns;
   var xml;
   var cameraX = 0;
   var cameraY = 0;
   var panToCharacter = null;
   var scrollingX = 0;
   var scrollingY = 0;
   var scrollDirectionX = 0;
   var scrollDirectionY = 0;
   var mouseButtonDown = false;
   var xmlStoredNumber = -1;
   var animationCounter = 0;
   function TileSystem()
   {
      com.nitrome.throwgame.Controller.content.onMouseDown = mx.utils.Delegate.create(this,this.mouseDown);
      com.nitrome.throwgame.Controller.content.onMouseUp = mx.utils.Delegate.create(this,this.mouseUp);
   }
   function loadLevel(number)
   {
      if(number == this.xmlStoredNumber)
      {
         this.readXML(this.xml);
         return undefined;
      }
      var _loc3_ = NitromeGame(_root.ng).getSwfPath() + "levels/" + NitromeGame(_root.ng).getLevelName(number,".xml");
      trace("loading level: " + _loc3_);
      com.nitrome.game.TransitionTween(_root.tt).stop();
      _root.loading._visible = true;
      this.xml = new XML();
      this.xml.onLoad = function(success)
      {
         com.nitrome.throwgame.Controller.tileSystem.readXML(com.nitrome.throwgame.Controller.tileSystem.xml);
         com.nitrome.game.TransitionTween(_root.tt).play();
      };
      this.xml.load(_loc3_);
   }
   function readXML(xml)
   {
      var _loc3_ = 0;
      var _loc6_ = 0;
      var _loc9_;
      var _loc7_;
      var _loc15_ = 0;
      var _loc14_ = 0;
      var _loc13_ = xml.firstChild;
      this.levelWidth = Number(_loc13_.attributes.width);
      this.levelHeight = Number(_loc13_.attributes.height);
      this.tileList = [];
      this.tileGrid = [];
      this.bgTileList = [];
      this.bgTileGrid = [];
      _loc3_ = 0;
      while(_loc3_ < this.levelWidth)
      {
         this.tileGrid[_loc3_] = [];
         this.bgTileGrid[_loc3_] = [];
         _loc3_ = _loc3_ + 1;
      }
      com.nitrome.throwgame.Controller.objects = [];
      com.nitrome.throwgame.Controller.boxes = [];
      com.nitrome.throwgame.Controller.mines = [];
      com.nitrome.throwgame.Controller.chests = [];
      var _loc4_ = _loc13_.firstChild;
      var _loc12_;
      var _loc10_;
      var _loc8_;
      var _loc5_;
      var _loc11_;
      for(; _loc4_; _loc4_ = _loc4_.nextSibling)
      {
         if(_loc4_.nodeName == "row")
         {
            _loc6_ = _loc15_++;
            _loc12_ = _loc4_.firstChild.nodeValue;
            _loc10_ = this.unserialize(_loc12_);
            _loc3_ = 0;
            while(_loc3_ < this.levelWidth)
            {
               _loc8_ = _loc10_[_loc3_];
               if(!(_loc8_ == "-" || !_loc8_))
               {
                  _loc9_ = new com.nitrome.throwgame.Tile(_loc8_,_loc3_,_loc6_);
                  this.tileGrid[_loc3_][_loc6_] = _loc9_;
                  this.tileList.push(_loc9_);
               }
               _loc3_ = _loc3_ + 1;
            }
            continue;
         }
         if(_loc4_.nodeName == "bgRow")
         {
            _loc6_ = _loc14_++;
            _loc12_ = _loc4_.firstChild.nodeValue;
            _loc10_ = this.unserialize(_loc12_);
            _loc3_ = 0;
            while(_loc3_ < this.levelWidth)
            {
               _loc8_ = _loc10_[_loc3_];
               if(!(_loc8_ == "-" || !_loc8_))
               {
                  _loc7_ = new com.nitrome.util.Clip(com.nitrome.throwgame.Controller.backTileLayer,_loc8_);
                  _loc7_.x = _loc3_ << 5;
                  _loc7_.y = _loc6_ << 5;
                  this.bgTileGrid[_loc3_][_loc6_] = _loc7_;
                  this.bgTileList.push(_loc7_);
               }
               _loc3_ = _loc3_ + 1;
            }
            continue;
         }
         if(_loc4_.nodeName != "obj")
         {
            continue;
         }
         switch(_loc4_.attributes.type)
         {
            case "redPirate":
            case "redPirateCaptain":
            case "bluePirate":
            case "bluePirateCaptain":
            case "blindPirate":
            case "blindPirateCaptain":
            case "bossGuy":
            case "bossGuyZombie":
            case "cabinBoy":
            case "cabinBoyCaptain":
            case "crab":
            case "femalePirate":
            case "femalePirateCaptain":
            case "monkey":
            case "oldPirate":
            case "oldPirateCaptain":
            case "parrot":
            case "rainbowBeard":
            case "rainbowBeardCaptain":
            case "shark":
            case "skeletonPirate":
            case "skeletonPirateCaptain":
            case "soldier":
            case "soldierCaptain":
            case "squid":
            case "tribe":
            case "tribeChief":
               _loc5_ = new com.nitrome.throwgame.Character(_loc4_.attributes.type);
               _loc5_.x = (Number(_loc4_.attributes.x) + 0.5) * 32;
               _loc5_.y = (Number(_loc4_.attributes.y) + 0.5) * 32;
               _loc5_.y += 16 - _loc5_.bottomExtent;
               _loc5_.update();
               _loc5_.setWeapons(_loc4_.attributes);
               _loc11_ = !(_loc4_.attributes.type == "redPirate" || _loc4_.attributes.type == "redPirateCaptain") ? 1 : 0;
               _loc5_.team = com.nitrome.throwgame.Controller.teams[_loc11_];
               _loc5_.team.characters.push(_loc5_);
               _loc5_.index = com.nitrome.throwgame.Controller.teams[_loc11_].characters.length - 1;
               break;
            case "water":
               com.nitrome.throwgame.Controller.water = new com.nitrome.throwgame.Water();
               com.nitrome.throwgame.Controller.water.y = Number(_loc4_.attributes.y) * 32;
               com.nitrome.throwgame.Controller.water.update();
               break;
            case "potentialWeapons":
               com.nitrome.throwgame.TreasureChest.potentialWeaponList = this.getPotentialWeaponList(_loc4_.attributes);
               com.nitrome.throwgame.TreasureChest.maxCount = 3;
         }
      }
      this.validDropColumns = this.getValidDropColumns();
      com.nitrome.throwgame.Controller.map.reset();
      if(com.nitrome.throwgame.Controller.teams[1].aiControlled)
      {
         com.nitrome.throwgame.Controller.speechBubble = new com.nitrome.throwgame.SpeechBubble();
         com.nitrome.throwgame.Controller.speechBubble.text = com.nitrome.throwgame.Controller.getLine(0);
         com.nitrome.throwgame.Controller.speechBubble.setTarget(com.nitrome.throwgame.Controller.teams[0].speaker());
         com.nitrome.throwgame.Controller.speechLineNumber = 1;
      }
      this.animationCounter = 0;
      this.panCamera(0,0,true);
      if(_root.selected_level >= 1 && _root.selected_level <= 5 || _root.selected_level >= 16 && _root.selected_level <= 21)
      {
         com.nitrome.throwgame.Controller.setSkyColour(1);
      }
      else if(_root.selected_level >= 6 && _root.selected_level <= 10 || _root.selected_level >= 22 && _root.selected_level <= 27)
      {
         com.nitrome.throwgame.Controller.setSkyColour(2);
      }
      else if(_root.selected_level >= 11 && _root.selected_level <= 15 || _root.selected_level >= 28 && _root.selected_level <= 33)
      {
         com.nitrome.throwgame.Controller.setSkyColour(3);
      }
      _root.team2.opponent_image.gotoAndStop(_root.selected_level);
      com.nitrome.throwgame.Controller.currentTeam.startTurn();
      _root.loading._visible = false;
   }
   function unserialize(s)
   {
      var _loc8_ = s.split(",");
      var _loc7_ = [];
      var _loc3_ = 0;
      var _loc2_;
      var _loc4_;
      var _loc6_;
      var _loc5_;
      var _loc1_;
      while(_loc3_ < _loc8_.length)
      {
         _loc2_ = _loc8_[_loc3_];
         if(_loc2_.indexOf(":") == -1)
         {
            _loc7_.push(_loc2_);
         }
         else
         {
            _loc4_ = _loc2_.split(":");
            _loc6_ = _loc4_[0];
            _loc5_ = Number(_loc4_[1]);
            _loc1_ = 0;
            while(_loc1_ < _loc5_)
            {
               _loc7_.push(_loc6_);
               _loc1_ = _loc1_ + 1;
            }
         }
         _loc3_ = _loc3_ + 1;
      }
      return _loc7_;
   }
   function getPotentialWeaponList(attributes)
   {
      var _loc3_ = [];
      var _loc2_;
      var _loc1_;
      for(var _loc5_ in attributes)
      {
         if(!(_loc5_ == "x" || _loc5_ == "y" || _loc5_ == "type" || _loc5_ == "luck" || _loc5_ == "maxChests"))
         {
            _loc2_ = Number(attributes[_loc5_]);
            _loc1_ = 0;
            while(_loc1_ < _loc2_)
            {
               _loc3_.push(_loc5_);
               _loc1_ = _loc1_ + 1;
            }
         }
      }
      return _loc3_;
   }
   function getValidDropColumns()
   {
      var _loc6_ = [];
      var _loc3_ = 0;
      var _loc5_;
      var _loc4_;
      var _loc2_;
      while(_loc3_ < this.levelWidth)
      {
         _loc5_ = false;
         _loc4_ = false;
         _loc2_ = 0;
         while(_loc2_ < this.levelHeight)
         {
            if(this.bgTileGrid[_loc3_][_loc2_].linkageName == "antichest")
            {
               _loc4_ = true;
               break;
            }
            if(this.tileGrid[_loc3_][_loc2_])
            {
               _loc5_ = true;
               break;
            }
            _loc2_ = _loc2_ + 1;
         }
         if(_loc5_ && !_loc4_)
         {
            _loc6_.push(_loc3_);
         }
         _loc3_ = _loc3_ + 1;
      }
      return _loc6_;
   }
   function advance()
   {
      this.animationCounter++;
      this.advanceScrolling();
      var _loc13_ = com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon;
      var _loc15_ = _loc13_.weaponType == "cannon" && !_loc13_.fired;
      if(_loc15_)
      {
         if(com.nitrome.throwgame.Controller.rangeCircle._alpha < 100)
         {
            com.nitrome.throwgame.Controller.rangeCircle._alpha += 20;
         }
         com.nitrome.throwgame.Controller.rangeCircle._x = com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.x;
         com.nitrome.throwgame.Controller.rangeCircle._y = com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.y + _loc13_.dragOffset;
         com.nitrome.throwgame.Controller.rangeCircle._xscale = com.nitrome.throwgame.Controller.rangeCircle._yscale = _loc13_.dragRange;
      }
      else if(com.nitrome.throwgame.Controller.rangeCircle._alpha > 0)
      {
         com.nitrome.throwgame.Controller.rangeCircle._alpha -= 20;
      }
      var _loc10_ = false;
      var _loc12_ = com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon.weaponType;
      var _loc11_ = 0;
      if(!com.nitrome.throwgame.Controller.currentTeam.aiControlled)
      {
         switch(com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon.weaponType)
         {
            case "anchor":
            case "seagull":
            case "tidalWave":
               _loc10_ = !com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon.fired;
               break;
            case "gunpowderBarrel":
            case "woodenCrate":
               _loc10_ = !com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon.finished;
               if(!com.nitrome.throwgame.BoxWeapon(com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon).canPlace(com.nitrome.throwgame.Controller.content._xmouse,com.nitrome.throwgame.Controller.content._ymouse))
               {
                  _loc12_ = "cross";
               }
               break;
            case "parachuteBomb":
               if(com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon.fired && !com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon.finished)
               {
                  _loc10_ = true;
                  _loc12_ = "fan";
                  _loc11_ = com.nitrome.throwgame.Controller.content._xmouse >= com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon.x ? -90 : 90;
               }
               break;
            case "voodooDoll":
               _loc10_ = !com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon.fired && !com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon.mc.hitTest(_level0._xmouse,_level0._ymouse) && !com.nitrome.throwgame.VoodooDoll(com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon).targetCharacter;
         }
      }
      if(this.scrollDirectionX != 0 || this.scrollDirectionY != 0)
      {
         _loc11_ = [[225,270,315],[180,0,0],[135,90,45]][this.scrollDirectionY + 1][this.scrollDirectionX + 1];
         if(_loc11_ % 90 == 0)
         {
            _loc12_ = "scroll1";
         }
         else
         {
            _loc11_ += 45;
            _loc12_ = "scroll2";
         }
         _loc10_ = true;
      }
      if(_loc10_)
      {
         com.nitrome.util.CustomCursor.setCursor(_loc12_);
         com.nitrome.throwgame.Controller.root.cursor._rotation = _loc11_;
      }
      else
      {
         com.nitrome.util.CustomCursor.restoreCursor();
      }
      var _loc14_ = com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon instanceof com.nitrome.throwgame.VoodooDoll;
      var _loc7_;
      var _loc5_;
      var _loc4_;
      var _loc6_;
      var _loc9_;
      var _loc8_;
      var _loc3_;
      var _loc2_;
      if((!com.nitrome.throwgame.Controller.currentTeam.selectedCharacter || _loc14_) && !com.nitrome.throwgame.Controller.currentTeam.aiControlled)
      {
         if(_loc14_)
         {
            _loc7_ = com.nitrome.throwgame.Controller.teams[2 - com.nitrome.throwgame.Controller.currentTeam.number].characters;
            if(com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon.fired)
            {
               com.nitrome.throwgame.Controller.hoverCharacter = com.nitrome.throwgame.VoodooDoll(com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon).targetCharacter;
            }
         }
         else
         {
            _loc7_ = com.nitrome.throwgame.Controller.currentTeam.characters;
         }
         _loc9_ = null;
         _loc8_ = 900;
         if(_loc14_)
         {
            _loc9_ = com.nitrome.throwgame.VoodooDoll(com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon).targetCharacter;
         }
         _loc3_ = 0;
         while(_loc3_ < _loc7_.length)
         {
            _loc2_ = _loc7_[_loc3_];
            _loc5_ = _loc2_.x - com.nitrome.throwgame.Controller.content._xmouse;
            _loc4_ = _loc2_.y - com.nitrome.throwgame.Controller.content._ymouse;
            _loc6_ = _loc5_ * _loc5_ + _loc4_ * _loc4_;
            if(_loc6_ < _loc8_)
            {
               _loc8_ = _loc6_;
               _loc9_ = _loc2_;
            }
            _loc3_ = _loc3_ + 1;
         }
         com.nitrome.throwgame.Controller.hoverCharacter = _loc9_;
         if(_loc9_ && _loc8_ < 900 && _loc14_)
         {
            com.nitrome.util.CustomCursor.setCursor("voodooDoll");
         }
      }
      else
      {
         com.nitrome.throwgame.Controller.hoverCharacter = null;
      }
   }
   function advanceScrolling()
   {
      if(com.nitrome.throwgame.Controller.dragging)
      {
         return undefined;
      }
      this.scrollDirectionX = 0;
      this.scrollDirectionY = 0;
      var _loc3_ = com.nitrome.throwgame.TreasureChest.fallingChest();
      if(com.nitrome.throwgame.Controller.speechBubble)
      {
         this.panTowards(com.nitrome.throwgame.Controller.speechBubble.x - 275,com.nitrome.throwgame.Controller.speechBubble.y - 200,50);
         return undefined;
      }
      if(com.nitrome.throwgame.Controller.popup.show)
      {
         return undefined;
      }
      if(com.nitrome.throwgame.Controller.currentTeam.aiControlled && !com.nitrome.throwgame.Controller.currentTeam.aiFinished)
      {
         return undefined;
      }
      if(_loc3_ && _loc3_.timeTaken < 100)
      {
         this.panTowards(_loc3_.x - 275,_loc3_.y - 200,50);
         return undefined;
      }
      var _loc5_;
      if(com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.thrown && !com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.throwFinished)
      {
         _loc5_ = com.nitrome.throwgame.Controller.currentTeam.selectedCharacter;
         this.panTowards(_loc5_.x - 275,_loc5_.y - 50 - 200,30);
         return undefined;
      }
      var _loc6_;
      if(!com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon.finished && com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon.track)
      {
         _loc6_ = com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon;
         this.panTowards(_loc6_.trackX - 275,_loc6_.trackY - 50 - 200,30);
         return undefined;
      }
      if(this.panToCharacter)
      {
         if(this.panTowards(this.panToCharacter.x - 275,this.panToCharacter.y - 50 - 200,30) == 0)
         {
            this.panToCharacter = null;
         }
         return undefined;
      }
      var _loc4_ = 40;
      if(_root._ymouse > 400 - _loc4_ || Key.isDown(40) || Key.isDown(83))
      {
         this.scrollDirectionY++;
      }
      if(_root._ymouse < _loc4_ || Key.isDown(38) || Key.isDown(87))
      {
         this.scrollDirectionY--;
      }
      if(_root._xmouse < _loc4_ || Key.isDown(37) || Key.isDown(65))
      {
         this.scrollDirectionX--;
      }
      if(_root._xmouse > 550 - _loc4_ || Key.isDown(39) || Key.isDown(68))
      {
         this.scrollDirectionX++;
      }
      if(com.nitrome.throwgame.Controller.root.weapons._visible)
      {
         this.scrollDirectionX = 0;
         this.scrollDirectionY = 0;
      }
      this.scrollingX = com.nitrome.util.Global.slide(this.scrollingX,this.scrollDirectionX * 10,1);
      this.scrollingY = com.nitrome.util.Global.slide(this.scrollingY,this.scrollDirectionY * 10,1);
      this.panCamera(this.cameraX + this.scrollingX,this.cameraY + this.scrollingY,false);
      this.lastMouseX = _root._xmouse;
      this.lastMouseY = _root._ymouse;
   }
   function panCamera(newX, newY, forceVisibility)
   {
      if(newX < -275)
      {
         newX = -275;
      }
      if(newX > -275 + (this.levelWidth << 5))
      {
         newX = -275 + (this.levelWidth << 5);
      }
      if(newY < -200)
      {
         newY = -200;
      }
      if(newY > -200 + (this.levelHeight << 5))
      {
         newY = -200 + (this.levelHeight << 5);
      }
      if(newY > com.nitrome.throwgame.Controller.water.y - 320)
      {
         newY = com.nitrome.throwgame.Controller.water.y - 320;
      }
      if(newX == this.cameraX && newY == this.cameraY && !forceVisibility)
      {
         return undefined;
      }
      var _loc16_ = this.cameraX >> 5;
      var _loc17_ = this.cameraY >> 5;
      var _loc6_ = newX >> 5;
      var _loc7_ = newY >> 5;
      var _loc18_;
      var _loc19_;
      var _loc8_;
      var _loc9_;
      var _loc14_;
      var _loc12_;
      var _loc11_;
      var _loc10_;
      var _loc3_;
      var _loc2_;
      var _loc4_;
      var _loc5_;
      if(_loc16_ != _loc6_ || _loc17_ != _loc7_ || forceVisibility)
      {
         _loc18_ = _loc16_ + 18;
         _loc19_ = _loc17_ + 13;
         _loc8_ = _loc6_ + 18;
         _loc9_ = _loc7_ + 13;
         _loc16_ = _loc16_ - 1;
         _loc17_ = _loc17_ - 1;
         _loc6_ = _loc6_ - 1;
         _loc7_ = _loc7_ - 1;
         _loc14_ = _loc16_ >= _loc6_ ? _loc6_ : _loc16_;
         _loc12_ = _loc18_ <= _loc8_ ? _loc8_ : _loc18_;
         _loc11_ = _loc17_ >= _loc7_ ? _loc7_ : _loc17_;
         _loc10_ = _loc19_ <= _loc9_ ? _loc9_ : _loc19_;
         if(forceVisibility)
         {
            _loc14_ = 0;
            _loc12_ = this.levelWidth - 1;
            _loc11_ = 0;
            _loc10_ = this.levelHeight - 1;
         }
         _loc3_ = _loc14_;
         while(_loc3_ <= _loc12_)
         {
            _loc2_ = _loc11_;
            while(_loc2_ <= _loc10_)
            {
               _loc4_ = this.tileGrid[_loc3_][_loc2_];
               if(_loc4_)
               {
                  if(_loc3_ >= _loc6_ && _loc3_ <= _loc8_ && _loc2_ >= _loc7_ && _loc2_ <= _loc9_)
                  {
                     _loc4_.show();
                  }
                  else
                  {
                     _loc4_.hide();
                  }
               }
               _loc5_ = this.bgTileGrid[_loc3_][_loc2_];
               if(_loc5_)
               {
                  if(_loc3_ >= _loc6_ && _loc3_ <= _loc8_ && _loc2_ >= _loc7_ && _loc2_ <= _loc9_)
                  {
                     _loc5_.show();
                  }
                  else
                  {
                     _loc5_.hide();
                  }
               }
               _loc2_ = _loc2_ + 1;
            }
            _loc3_ = _loc3_ + 1;
         }
      }
      this.cameraX = newX;
      this.cameraY = newY;
      com.nitrome.throwgame.Controller.content._x = - this.cameraX;
      com.nitrome.throwgame.Controller.content._y = - this.cameraY;
   }
   function panTowards(tx, ty, step)
   {
      var _loc4_ = tx - this.cameraX;
      var _loc3_ = ty - this.cameraY;
      var _loc2_ = Math.sqrt(_loc4_ * _loc4_ + _loc3_ * _loc3_);
      if(_loc2_ <= step)
      {
         this.panCamera(tx,ty,false);
         return 0;
      }
      this.panCamera(this.cameraX + step * _loc4_ / _loc2_,this.cameraY + step * _loc3_ / _loc2_,false);
      return _loc2_ - step;
   }
   function mouseDown()
   {
      var _loc4_;
      if(com.nitrome.throwgame.Controller.speechBubble)
      {
         if(com.nitrome.throwgame.Controller.speechBubble.completeTime > 1)
         {
            com.nitrome.throwgame.Controller.speechBubble.completeTime = Infinity;
         }
         else
         {
            com.nitrome.throwgame.Controller.speechBubble.mc.textField.text = com.nitrome.throwgame.Controller.speechBubble.text;
         }
         return undefined;
      }
      if(com.nitrome.throwgame.Controller.root.weapons._visible && com.nitrome.throwgame.Controller.root.weapons.hitTest(_level0._xmouse,_level0._ymouse))
      {
         return undefined;
      }
      if(com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.weaponSelected && com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.mcHolder.overlay.cancelWeapon.hitTest(_level0._xmouse,_level0._ymouse))
      {
         return undefined;
      }
      this.mouseButtonDown = true;
      if(com.nitrome.throwgame.Controller.currentTeam.aiControlled)
      {
         return undefined;
      }
      var _loc7_;
      var _loc6_;
      var _loc3_;
      var _loc11_;
      var _loc10_;
      var _loc5_;
      if(!com.nitrome.throwgame.Controller.currentTeam.selectedCharacter)
      {
         _loc7_ = null;
         _loc6_ = 900;
         _loc4_ = 0;
         while(_loc4_ < com.nitrome.throwgame.Controller.currentTeam.characters.length)
         {
            _loc3_ = com.nitrome.throwgame.Controller.currentTeam.characters[_loc4_];
            _loc11_ = _loc3_.x - com.nitrome.throwgame.Controller.content._xmouse;
            _loc10_ = _loc3_.y - com.nitrome.throwgame.Controller.content._ymouse;
            _loc5_ = _loc11_ * _loc11_ + _loc10_ * _loc10_;
            if(_loc5_ < _loc6_)
            {
               _loc6_ = _loc5_;
               _loc7_ = _loc3_;
            }
            _loc4_ = _loc4_ + 1;
         }
         if(_loc7_)
         {
            com.nitrome.throwgame.Controller.currentTeam.select(_loc7_);
         }
         return undefined;
      }
      var _loc9_;
      var _loc12_;
      if(com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon instanceof com.nitrome.throwgame.VoodooDoll && !com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon.fired && !com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon.mc.hitTest(_level0._xmouse,_level0._ymouse))
      {
         _loc7_ = null;
         _loc6_ = 900;
         _loc9_ = com.nitrome.throwgame.Controller.teams[2 - com.nitrome.throwgame.Controller.currentTeam.number].characters;
         _loc4_ = 0;
         while(_loc4_ < _loc9_.length)
         {
            _loc3_ = _loc9_[_loc4_];
            _loc11_ = _loc3_.x - com.nitrome.throwgame.Controller.content._xmouse;
            _loc10_ = _loc3_.y - com.nitrome.throwgame.Controller.content._ymouse;
            _loc5_ = _loc11_ * _loc11_ + _loc10_ * _loc10_;
            if(_loc5_ < _loc6_)
            {
               _loc6_ = _loc5_;
               _loc7_ = _loc3_;
            }
            _loc4_ = _loc4_ + 1;
         }
         if(_loc7_)
         {
            _loc12_ = com.nitrome.throwgame.VoodooDoll(com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon);
            if(_loc12_.targetCharacter != _loc7_)
            {
               _loc12_.setTargetCharacter(_loc7_);
               _root.sfx_manager.playSound("voodoo");
            }
            return undefined;
         }
      }
      if(!com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.weaponSelected)
      {
         return undefined;
      }
      var _loc8_;
      if(com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon)
      {
         _loc8_ = com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon;
         if(com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon.fired)
         {
            return undefined;
         }
         if(com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equippedWeapon.placeableWeapon)
         {
            return undefined;
         }
      }
      else
      {
         _loc8_ = com.nitrome.throwgame.Controller.currentTeam.selectedCharacter;
         if(com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.thrown)
         {
            return undefined;
         }
         if(!com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.canThrow)
         {
            return undefined;
         }
      }
      if(!_loc8_.draggable && !_loc8_.twangable)
      {
         return undefined;
      }
      _loc11_ = _loc8_.x - com.nitrome.throwgame.Controller.content._xmouse;
      _loc10_ = _loc8_.y - com.nitrome.throwgame.Controller.content._ymouse;
      if(_loc11_ * _loc11_ + _loc10_ * _loc10_ < 900)
      {
         if(_loc8_.twangable)
         {
            com.nitrome.throwgame.Controller.twanging = _loc8_;
         }
         else
         {
            com.nitrome.throwgame.Controller.dragging = _loc8_;
         }
         _loc8_.dragStartX = _loc8_.x;
         _loc8_.dragStartY = _loc8_.y;
      }
   }
   function mouseUp()
   {
      this.mouseButtonDown = false;
      if(com.nitrome.throwgame.Controller.dragging)
      {
         com.nitrome.throwgame.Controller.dragging.release();
         com.nitrome.throwgame.Controller.dragging = null;
      }
      if(com.nitrome.throwgame.Controller.twanging)
      {
         com.nitrome.throwgame.Controller.twanging.twang();
         com.nitrome.throwgame.Controller.twanging = null;
      }
   }
   function destroy()
   {
      var _loc2_;
      _loc2_ = 0;
      while(_loc2_ < this.tileList.length)
      {
         this.tileList[_loc2_].destroy();
         _loc2_ = _loc2_ + 1;
      }
      _loc2_ = 0;
      while(_loc2_ < this.bgTileList.length)
      {
         this.bgTileList[_loc2_].destroy();
         _loc2_ = _loc2_ + 1;
      }
      this.tileList = [];
      this.tileGrid = [];
      this.bgTileList = [];
      this.bgTileGrid = [];
   }
}
