var §\x01§ = 68;
var §\x0f§ = 1;
class com.nitrome.throwgame.Controller
{
   static var backTileLayer;
   static var boxes;
   static var characterLayer;
   static var chests;
   static var content;
   static var effectsLayer;
   static var holder;
   static var map;
   static var mines;
   static var objectLayer;
   static var objects;
   static var popup;
   static var rangeCircle;
   static var root;
   static var speechBubble;
   static var speechLineNumber;
   static var teams;
   static var text;
   static var tileLayer;
   static var tileSystem;
   static var water;
   static var waterLayer;
   static var skyColour = 1;
   static var currentTeam = null;
   static var dragging = null;
   static var twanging = null;
   static var hoverCharacter = null;
   static var inactivity = 0;
   function Controller()
   {
   }
   static function startGame(rootClip, raw)
   {
      if(raw)
      {
         _root.game_type = Number(raw.firstChild.attributes.players);
         trace(_root.game_type);
      }
      com.nitrome.util.Trig.setup();
      com.nitrome.throwgame.Controller.root = rootClip;
      com.nitrome.throwgame.Controller.holder = rootClip.holder;
      com.nitrome.throwgame.Controller.holder.onEnterFrame = com.nitrome.throwgame.Controller.enterFrame;
      com.nitrome.throwgame.Controller.content = com.nitrome.util.Clip.createEmpty(com.nitrome.throwgame.Controller.holder);
      com.nitrome.throwgame.Controller.text = com.nitrome.throwgame.Controller.root.text;
      com.nitrome.throwgame.Controller.popup = com.nitrome.throwgame.Controller.root.popup;
      com.nitrome.throwgame.Controller.backTileLayer = com.nitrome.util.Clip.createEmpty(com.nitrome.throwgame.Controller.content);
      com.nitrome.throwgame.Controller.objectLayer = com.nitrome.util.Clip.createEmpty(com.nitrome.throwgame.Controller.content);
      com.nitrome.throwgame.Controller.effectsLayer = com.nitrome.util.Clip.createEmpty(com.nitrome.throwgame.Controller.content);
      com.nitrome.throwgame.Controller.tileLayer = com.nitrome.util.Clip.createEmpty(com.nitrome.throwgame.Controller.content);
      com.nitrome.throwgame.Controller.characterLayer = com.nitrome.util.Clip.createEmpty(com.nitrome.throwgame.Controller.content);
      com.nitrome.throwgame.Controller.waterLayer = com.nitrome.util.Clip.createEmpty(com.nitrome.throwgame.Controller.content);
      com.nitrome.throwgame.Controller.rangeCircle = com.nitrome.throwgame.Controller.content.attachMovie("rangeCircle","rangeCircle",com.nitrome.throwgame.Controller.content.getNextHighestDepth());
      com.nitrome.throwgame.Controller.map = new com.nitrome.throwgame.Map();
      _quality = "LOW";
      com.nitrome.throwgame.Controller.teams = [new com.nitrome.throwgame.Team(1),new com.nitrome.throwgame.Team(2)];
      com.nitrome.throwgame.Controller.currentTeam = com.nitrome.throwgame.Controller.teams[0];
      if(!com.nitrome.throwgame.Controller.teams[1].aiControlled && Math.random() >= 0.5)
      {
         com.nitrome.throwgame.Controller.currentTeam = com.nitrome.throwgame.Controller.teams[1];
      }
      com.nitrome.throwgame.Controller.tileSystem = new com.nitrome.throwgame.TileSystem();
      if(raw)
      {
         com.nitrome.throwgame.Controller.tileSystem.readXML(raw);
      }
      else
      {
         com.nitrome.throwgame.Controller.tileSystem.loadLevel(_root.selected_level);
      }
   }
   static function changeLevel(levelNumber)
   {
      com.nitrome.throwgame.Controller.unloadLevel();
      com.nitrome.throwgame.Controller.teams = [new com.nitrome.throwgame.Team(1),new com.nitrome.throwgame.Team(2)];
      com.nitrome.throwgame.Controller.currentTeam = com.nitrome.throwgame.Controller.teams[0];
      if(!com.nitrome.throwgame.Controller.teams[1].aiControlled && Math.random() >= 0.5)
      {
         com.nitrome.throwgame.Controller.currentTeam = com.nitrome.throwgame.Controller.teams[1];
      }
      com.nitrome.throwgame.Controller.tileSystem = new com.nitrome.throwgame.TileSystem();
      com.nitrome.throwgame.Controller.tileSystem.loadLevel(levelNumber);
      com.nitrome.throwgame.Controller.currentTeam.startTurn();
   }
   static function unloadLevel()
   {
      var _loc1_;
      com.nitrome.throwgame.Controller.tileSystem.destroy();
      _loc1_ = 0;
      while(_loc1_ < com.nitrome.throwgame.Controller.teams.length)
      {
         com.nitrome.throwgame.Controller.teams[_loc1_].destroy();
         _loc1_ = _loc1_ + 1;
      }
      _loc1_ = 0;
      while(_loc1_ < com.nitrome.throwgame.Controller.mines.length)
      {
         com.nitrome.throwgame.Controller.mines[_loc1_].destroy();
         _loc1_ = _loc1_ + 1;
      }
      _loc1_ = com.nitrome.throwgame.Controller.chests.length - 1;
      while(_loc1_ >= 0)
      {
         com.nitrome.throwgame.Controller.chests[_loc1_].destroy();
         _loc1_ = _loc1_ - 1;
      }
      _loc1_ = 0;
      while(_loc1_ < com.nitrome.throwgame.Controller.boxes.length)
      {
         com.nitrome.throwgame.Controller.boxes[_loc1_].destroy();
         _loc1_ = _loc1_ + 1;
      }
      com.nitrome.throwgame.Controller.water.destroy();
   }
   static function endGame()
   {
      com.nitrome.throwgame.Controller.unloadLevel();
      com.nitrome.throwgame.Controller.holder.onEnterFrame = null;
      com.nitrome.util.CustomCursor.restoreCursor();
      _quality = "HIGH";
   }
   static function get1PLevelScore()
   {
      var _loc2_ = com.nitrome.throwgame.Controller.teams[0].getAverageHealth() * 20 - com.nitrome.throwgame.Controller.teams[0].totalTurnsTaken * 25;
      _loc2_ = Math.floor(_loc2_);
      var _loc3_ = _root.selected_level * 10;
      if(_loc2_ < _loc3_)
      {
         _loc2_ = _loc3_;
      }
      return _loc2_;
   }
   static function nextTurn()
   {
      com.nitrome.throwgame.Controller.currentTeam.finishTurn();
      if(!com.nitrome.throwgame.Controller.teams[0].anyAlive() || !com.nitrome.throwgame.Controller.teams[1].anyAlive())
      {
         if(com.nitrome.throwgame.Controller.teams[1].aiControlled)
         {
            if(com.nitrome.throwgame.Controller.teams[0].anyAlive())
            {
               com.nitrome.throwgame.Controller.speechBubble = new com.nitrome.throwgame.SpeechBubble();
               com.nitrome.throwgame.Controller.speechBubble.text = com.nitrome.throwgame.Controller.getLine(2);
               com.nitrome.throwgame.Controller.speechBubble.setTarget(com.nitrome.throwgame.Controller.teams[0].speaker());
               com.nitrome.throwgame.Controller.speechLineNumber = 3;
               NitromeGame(_root.ng).setLevelUnlocked(_root.selected_level + 1);
               _root.score += com.nitrome.throwgame.Controller.get1PLevelScore();
            }
            else if(com.nitrome.throwgame.Controller.teams[1].anyAlive())
            {
               com.nitrome.throwgame.Controller.speechBubble = new com.nitrome.throwgame.SpeechBubble();
               com.nitrome.throwgame.Controller.speechBubble.text = com.nitrome.throwgame.Controller.getLine(3);
               com.nitrome.throwgame.Controller.speechBubble.setTarget(com.nitrome.throwgame.Controller.teams[1].speaker());
               com.nitrome.throwgame.Controller.speechLineNumber = 3;
            }
            else
            {
               com.nitrome.throwgame.Controller.popup.gotoAndStop("level_failed_1p");
               com.nitrome.throwgame.Controller.popup.show = true;
            }
            return undefined;
         }
         if(com.nitrome.throwgame.Controller.teams[0].anyAlive())
         {
            _root.won_p1++;
            com.nitrome.throwgame.Controller.popup.gotoAndStop("vs_1p_wins");
            com.nitrome.throwgame.Controller.popup.show = true;
         }
         else if(com.nitrome.throwgame.Controller.teams[1].anyAlive())
         {
            _root.won_p2++;
            com.nitrome.throwgame.Controller.popup.gotoAndStop("vs_2p_wins");
            com.nitrome.throwgame.Controller.popup.show = true;
         }
         else
         {
            com.nitrome.throwgame.Controller.popup.gotoAndStop("vs_draw");
            com.nitrome.throwgame.Controller.popup.show = true;
         }
         return undefined;
      }
      if(com.nitrome.throwgame.Controller.currentTeam.number == 1)
      {
         com.nitrome.throwgame.Controller.currentTeam = com.nitrome.throwgame.Controller.teams[1];
      }
      else
      {
         com.nitrome.throwgame.Controller.currentTeam = com.nitrome.throwgame.Controller.teams[0];
      }
      com.nitrome.throwgame.TreasureChest.dropNew();
      com.nitrome.throwgame.Controller.currentTeam.startTurn();
   }
   static function getLine(line)
   {
      return com.nitrome.throwgame.Team.lines[com.nitrome.throwgame.Controller.teams[1].getTeamType()][line];
   }
   static function enterFrame()
   {
      var _loc2_;
      com.nitrome.throwgame.Controller.inactivity++;
      _loc2_ = 0;
      while(_loc2_ < com.nitrome.throwgame.Controller.teams.length)
      {
         com.nitrome.throwgame.Controller.teams[_loc2_].advance();
         _loc2_ = _loc2_ + 1;
      }
      com.nitrome.throwgame.Controller.tileSystem.advance();
      _loc2_ = 0;
      while(_loc2_ < com.nitrome.throwgame.Controller.mines.length)
      {
         com.nitrome.throwgame.Controller.mines[_loc2_].advance();
         _loc2_ = _loc2_ + 1;
      }
      _loc2_ = 0;
      while(_loc2_ < com.nitrome.throwgame.Controller.chests.length)
      {
         com.nitrome.throwgame.Controller.chests[_loc2_].advance();
         _loc2_ = _loc2_ + 1;
      }
      _loc2_ = 0;
      while(_loc2_ < com.nitrome.throwgame.Controller.boxes.length)
      {
         com.nitrome.throwgame.Controller.boxes[_loc2_].advanceMotion();
         _loc2_ = _loc2_ + 1;
      }
      com.nitrome.throwgame.Controller.water.advance();
      com.nitrome.throwgame.Controller.map.drawActive();
      if(com.nitrome.throwgame.Controller.inactivity > 10)
      {
         com.nitrome.throwgame.Controller.inactivity = 0;
         if(com.nitrome.throwgame.Controller.currentTeam.isTurnComplete())
         {
            com.nitrome.throwgame.Controller.nextTurn();
         }
         else
         {
            com.nitrome.throwgame.Controller.currentTeam.continueTurn();
         }
      }
      if(com.nitrome.throwgame.Controller.speechBubble)
      {
         com.nitrome.throwgame.Controller.speechBubble.advance();
         if(com.nitrome.throwgame.Controller.speechBubble.completeTime > 160)
         {
            com.nitrome.throwgame.Controller.speechBubble.destroy();
            if(com.nitrome.throwgame.Controller.speechLineNumber == 1)
            {
               com.nitrome.throwgame.Controller.speechBubble = new com.nitrome.throwgame.SpeechBubble();
               com.nitrome.throwgame.Controller.speechBubble.text = com.nitrome.throwgame.Controller.getLine(1);
               com.nitrome.throwgame.Controller.speechBubble.setTarget(com.nitrome.throwgame.Controller.teams[1].speaker());
               com.nitrome.throwgame.Controller.speechLineNumber = 2;
            }
            else if(com.nitrome.throwgame.Controller.speechLineNumber == 2)
            {
               com.nitrome.throwgame.Controller.speechBubble = null;
            }
            else if(com.nitrome.throwgame.Controller.speechLineNumber == 3)
            {
               com.nitrome.throwgame.Controller.speechBubble = null;
               if(com.nitrome.throwgame.Controller.teams[1].aiControlled)
               {
                  if(com.nitrome.throwgame.Controller.teams[0].anyAlive())
                  {
                     if(_root.selected_level == 15)
                     {
                        com.nitrome.throwgame.Controller.popup.gotoAndStop("game_complete_1p");
                     }
                     else
                     {
                        com.nitrome.throwgame.Controller.popup.gotoAndStop("level_complete_1p");
                     }
                  }
                  else
                  {
                     com.nitrome.throwgame.Controller.popup.gotoAndStop("level_failed_1p");
                  }
                  com.nitrome.throwgame.Controller.popup.show = true;
                  com.nitrome.throwgame.Controller.popup.reset();
               }
            }
         }
      }
   }
   static function setSkyColour(colour)
   {
      trace("setSkyColour: " + colour);
      com.nitrome.throwgame.Controller.skyColour = colour;
      com.nitrome.throwgame.Controller.water.mc.gotoAndStop("water" + com.nitrome.throwgame.Controller.skyColour.toString());
      com.nitrome.throwgame.Controller.root.background.gotoAndStop("background" + colour.toString());
   }
}
