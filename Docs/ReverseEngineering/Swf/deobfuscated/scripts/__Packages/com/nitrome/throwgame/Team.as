var §\x01§ = 518;
var §\x0f§ = 1;
class com.nitrome.throwgame.Team
{
   var aiMoveDetails;
   var characters;
   var number;
   var selectedCharacter = null;
   var aiControlled = false;
   var aiFinished = false;
   var aiCanBailOut = false;
   var totalTurnsTaken = 0;
   static var lines = {squid:["Arrrgh what sort o\' sea|beastie be this?","Splund splund... I\'m gonna|ink you up... splund splund.","Yearggh take that you|varmint, we be eating squid|rings tonight.","Splund splund... I\'m so|happy I\'ve just inked|myself... splund."],crab:["Looks like we\'ve got a|bad case of the crabs|here, mateys!","Prepare to lose your|beards... snip snip!","I think we shell-shocked|them, mateys!","Looks like they fell for|our deadly pincer|manoeuvre... at last their|beards be ours."],shark:["Ahoy. I think there be|something fishy here.","We\'re gonna hammer you!|You\'re Shark Bait!","Ya Har! \'Twas too easy!|But thar be plenty more|fish in the sea!","Stop! ...Hammer-time!"],parrot:["Arrrgh!... Who be a pretty|boy then? ...Does Polly|want a cracker?","Squawk!... Who\'s a pretty|boy then? ...Squawk!...|Polly want a cracker!|...Squawk!","Arrrgh... \'tiz a shame.|They could have made fine|shoulder enhancers.","Squawk!... Polly want a|cracker!"],monkey:["Guard the fruit mateys...|If the monkeys get it we\'ll|be up for another bout|of scurvy!","Oooh Oooh Ahh Ahh Bananas!","Yarrgh! Yer monkey business|ain\'t foolin\' any of us!","Oooh Oooh Ahh Ahh More|Bananas!"],soldier:["These boys look too clean|to be in a real fight! And|look... they\'ve still got|teeth to knock out!","We fight for King and|Country! ...and because we|get a decent benefits|package and pension plan!","They don\'t look so smart|now lads... I think We\'ll|be using his toupee to swab|the decks!","You pirates will get what|you deserve... a short drop|and a sudden stop!"],blindPirate:["Yargh... These guys have|one too many eye patches!|This should be easy! Let\'s|get \'em!","Get who? Where am I...|how did I get here?","Yarrgh!|They didn\'t see us coming!","Gar... I can\'t hear them...|Does anyone know if we|won?"],femalePirate:["Garrrgh... Looks like we\'ve|found ourselves some|mighty fine booty here,|mateys!","Yarp... The only booty you|will be getting is on the|end of my foot!","Arrgh... shame... what a|waste of such lovely|sea legs!","Yarp... Everybody knows|girls are better than boys!"],oldPirate:["Arrrrggggh... I think we|found some old sea dogs!|Ready for retirement!","We\'re not Old, we\'re|experienced... and now we\'re|gonna kick your... erm...|erm... what was I saying|again... who are you?","Garrrrgh! Ok, Grandads...|time for your afternoon|nap!","That\'ll show you... in my|day we used to fight a|Kraken with nothin\' but a|couple o\' stones. Arrrgh...|Those be the days..."],rainbowBeard:["This guy be looking a little|too jolly to be flying the|ol\' jolly roger, eh mateys?","Ohhhh look at you... don\'t|you look tough? Anyway,|haven\'t you heard? Fancy|colours are the new black!","I\'d have beaten you sooner if|your fancy beards didn\'t|keep catchin\' the light!","Oo-er! Now we\'re finished|with them, it\'s time for a|bit of retail therapy!"],cabinBoy:["Yarrgh! ...ye be wanting to|grow a proper beard before|taking on the likes of us,|boys...","We\'re no boys! I had a couple|of hairs only last month on|me chin, just ask me mum!","Yarrgh! Best be running|along home now boys! I think|it be past yer bedtime?","That was fun... just like a|Nitrome video game! LOL!"],tribe:["Garrgh! I\'ve got a bad feelin\'|about this lads! They be|lookin\' kinda hungry!","Get the cookin\' pot ready...|pirate stew tonight!","Good work lads! we\'ve|avoided being lunch... looks|like they\'ll have to stick|with mangos and coconuts|from now on!","mmmmmmm! can\'t remember|the last time we had a good|pirate stew!|...has anyone got the salt?"],skeletonPirate:["This crew looks like they|could do with a good feed|more than a fight...","you\'ll have to catch us all|first ...fatty!","I am rubber, you are glue!","Sticks and stones may break|my bones but pirates never|hurt me!"],bossGuy:["Garrrgh! what a big hat...|I think he be trying to|make up for something, lads!","Argh! ye not be winning with|a hat like that. And once I|win I\'ll be taking your hats.","I think he was a little|hot-headed...|maybe it was his hat!","These hats \'ll be a mighty|fine addition to my|collection!"],bossGuyZombie:["I think he\'s been playing|with a few too many|voodoo dolls, lads! he\'s back|from Davey Jones locker.","aaaaaaaaahhhhhhhh...|hhhhhaaaaattttttss...|errrr... I mean...|bbbbrrraaaiiinnsss...","Yo-Ho-Ho... we\'ve done it|lads! that finished him off...|eerrrrm didn\'t we?","Yar hats be mine after all!|hhhhhaaaaattttttss!!!"]};
   function Team(teamNumber)
   {
      this.number = teamNumber;
      this.characters = [];
      if(_root.game_type == 1)
      {
         this.aiControlled = teamNumber == 2;
      }
      else
      {
         this.aiControlled = false;
      }
   }
   function advance()
   {
      var _loc2_;
      var _loc4_;
      var _loc3_;
      var _loc5_;
      var _loc6_;
      if(this.aiControlled)
      {
         if(!this.aiFinished && com.nitrome.throwgame.Controller.currentTeam == this)
         {
            _loc2_ = 0;
            while(_loc2_ < this.characters.length)
            {
               if(!this.characters[_loc2_].aiFinished)
               {
                  _loc4_ = getTimer();
                  do
                  {
                     this.characters[_loc2_].aiThink();
                  }
                  while(!this.characters[_loc2_].aiFinished && getTimer() < _loc4_ + 30);
                  break;
               }
               _loc2_ = _loc2_ + 1;
            }
            this.aiFinished = this.characters[this.characters.length - 1].aiFinished;
            if(this.aiFinished)
            {
               _loc3_ = [];
               _loc2_ = 0;
               while(_loc2_ < this.characters.length)
               {
                  _loc3_ = _loc3_.concat(this.characters[_loc2_].aiMoveList);
                  _loc2_ = _loc2_ + 1;
               }
               _loc5_ = null;
               _loc6_ = - Infinity;
               _loc2_ = 0;
               while(_loc2_ < _loc3_.length)
               {
                  if(_loc3_[_loc2_].success > _loc6_)
                  {
                     _loc6_ = _loc3_[_loc2_].success;
                     _loc5_ = _loc3_[_loc2_];
                  }
                  _loc2_ = _loc2_ + 1;
               }
               if(_loc6_ > 0 || !this.aiCanBailOut)
               {
                  this.select(_loc5_.player);
                  this.aiMoveDetails = _loc5_;
                  trace("best rating: " + _loc5_.success);
                  trace("best weapon: " + (_loc5_.weapon !== -1 ? _loc5_.player.hasWeapons[_loc5_.weapon] : "throw"));
                  com.nitrome.throwgame.Controller.tileSystem.panToCharacter = _loc5_.player;
               }
               else
               {
                  com.nitrome.throwgame.Controller.nextTurn();
               }
            }
         }
         else if(this.aiMoveDetails && com.nitrome.throwgame.Controller.tileSystem.panToCharacter == null)
         {
            if(this.aiMoveDetails.weapon >= 0)
            {
               this.aiMoveDetails.player.equip(this.aiMoveDetails.weapon);
               this.aiMoveDetails.player.equippedWeapon.aiPerform(this.aiMoveDetails);
               this.aiMoveDetails.player.canThrow = false;
               this.aiMoveDetails.player.canShoot = false;
            }
            else
            {
               this.aiMoveDetails.player.unequip();
               this.aiMoveDetails.player.velocityX = this.aiMoveDetails.vx;
               this.aiMoveDetails.player.velocityY = this.aiMoveDetails.vy;
               this.aiMoveDetails.player.thrown = true;
               this.aiMoveDetails.player.canThrow = false;
            }
            this.aiMoveDetails = null;
         }
      }
      _loc2_ = 0;
      while(_loc2_ < this.characters.length)
      {
         this.characters[_loc2_].advance();
         _loc2_ = _loc2_ + 1;
      }
      if(com.nitrome.throwgame.Controller.currentTeam == this && !this.selectedCharacter)
      {
         com.nitrome.throwgame.Controller.inactivity = 0;
      }
      var _loc7_ = 0;
      var _loc8_ = 0;
      _loc2_ = 0;
      while(_loc2_ < this.characters.length)
      {
         _loc8_ += this.characters[_loc2_].health;
         _loc7_ += this.characters[_loc2_].maxHealth;
         _loc2_ = _loc2_ + 1;
      }
      var _loc9_ = com.nitrome.throwgame.Controller.root["team" + this.number];
      var _loc10_ = 1 + Math.floor(96 * _loc8_ / _loc7_);
      _loc9_.gotoAndStop(com.nitrome.util.Global.slide(_loc9_._currentframe,_loc10_,1));
   }
   function anyAlive()
   {
      var _loc2_ = 0;
      while(_loc2_ < this.characters.length)
      {
         if(this.characters[_loc2_].alive)
         {
            return true;
         }
         _loc2_ = _loc2_ + 1;
      }
      return false;
   }
   function countAlive()
   {
      var _loc3_ = 0;
      var _loc2_ = 0;
      while(_loc2_ < this.characters.length)
      {
         if(this.characters[_loc2_].alive)
         {
            _loc3_ = _loc3_ + 1;
         }
         _loc2_ = _loc2_ + 1;
      }
      return _loc3_;
   }
   function speaker()
   {
      var _loc2_ = 0;
      while(_loc2_ < this.characters.length)
      {
         if(!(this.characters[_loc2_].linkageName.indexOf("Captain") == -1 && this.characters[_loc2_].linkageName != "tribeChief"))
         {
            if(this.characters[_loc2_].alive)
            {
               return this.characters[_loc2_];
            }
         }
         _loc2_ = _loc2_ + 1;
      }
      _loc2_ = 0;
      while(_loc2_ < this.characters.length)
      {
         if(this.characters[_loc2_].alive)
         {
            return this.characters[_loc2_];
         }
         _loc2_ = _loc2_ + 1;
      }
      return null;
   }
   function getCaptain()
   {
      var _loc2_ = 0;
      while(_loc2_ < this.characters.length)
      {
         if(this.characters[_loc2_].linkageName.indexOf("Captain") != -1)
         {
            return this.characters[_loc2_];
         }
         if(this.characters[_loc2_].linkageName == "tribeChief")
         {
            return this.characters[_loc2_];
         }
         _loc2_ = _loc2_ + 1;
      }
      return null;
   }
   function select(character, again)
   {
      if(!character.alive)
      {
         return undefined;
      }
      this.selectedCharacter = character;
      this.selectedCharacter.thrown = false;
      this.selectedCharacter.throwFinished = false;
      this.selectedCharacter.weaponSelected = false;
      if(!again)
      {
         _root.sfx_manager.playSound(this.getTeamType());
      }
   }
   function unselect()
   {
      this.selectedCharacter = null;
   }
   function startTurn()
   {
      this.totalTurnsTaken++;
      this.selectedCharacter = null;
      var _loc2_ = 0;
      while(_loc2_ < this.characters.length)
      {
         this.characters[_loc2_].mc.num.text = _loc2_.toString();
         _loc2_ = _loc2_ + 1;
      }
      if(this.aiControlled)
      {
         this.aiFinished = false;
         _loc2_ = 0;
         while(_loc2_ < this.characters.length)
         {
            this.characters[_loc2_].aiStart();
            _loc2_ = _loc2_ + 1;
         }
         this.aiCanBailOut = false;
      }
      var _loc7_ = null;
      var _loc6_ = Infinity;
      _loc2_ = 0;
      var _loc4_;
      var _loc3_;
      var _loc5_;
      while(_loc2_ < this.characters.length)
      {
         this.characters[_loc2_].mc.num._visible = false;
         if(this.characters[_loc2_].hasWeapons.length < 1)
         {
            this.characters[_loc2_].hasWeapons.push("cannonball");
         }
         this.characters[_loc2_].canShoot = true;
         this.characters[_loc2_].canThrow = true;
         this.characters[_loc2_].weaponSelected = false;
         this.characters[_loc2_].weaponLocked = false;
         this.characters[_loc2_].evilness = 0;
         if(this.characters[_loc2_].alive)
         {
            _loc4_ = com.nitrome.throwgame.Controller.tileSystem.cameraX + 275 - this.characters[_loc2_].x;
            _loc3_ = com.nitrome.throwgame.Controller.tileSystem.cameraY + 275 - this.characters[_loc2_].y;
            _loc5_ = _loc4_ * _loc4_ + _loc3_ * _loc3_;
            if(_loc5_ < _loc6_)
            {
               _loc6_ = _loc5_;
               _loc7_ = this.characters[_loc2_];
            }
         }
         _loc2_ = _loc2_ + 1;
      }
      var _loc8_;
      if(!this.selectedCharacter)
      {
         com.nitrome.throwgame.Controller.tileSystem.panToCharacter = _loc7_;
         _loc8_ = this.getCaptain();
         if(this.totalTurnsTaken == 1 && _loc8_)
         {
            com.nitrome.throwgame.Controller.tileSystem.panToCharacter = _loc8_;
         }
      }
      com.nitrome.throwgame.Controller.root.weapons.gotoAndStop(this.number != 2 ? "red" : "blue");
      if(this.aiControlled)
      {
         com.nitrome.throwgame.Controller.text.say("Computer, take your turn");
      }
      else
      {
         com.nitrome.throwgame.Controller.text.say("Player " + this.number + ", take your turn");
      }
   }
   function finishTurn()
   {
      this.selectedCharacter.thrown = false;
      this.selectedCharacter.throwFinished = false;
      this.selectedCharacter.weaponExpired();
      this.selectedCharacter = null;
   }
   function isTurnComplete()
   {
      if(!this.selectedCharacter.alive)
      {
         return true;
      }
      return !(this.selectedCharacter.canThrow || this.selectedCharacter.canShoot);
   }
   function continueTurn()
   {
      this.select(this.selectedCharacter,true);
      var _loc2_;
      if(this.aiControlled)
      {
         this.aiFinished = false;
         _loc2_ = 0;
         while(_loc2_ < this.characters.length)
         {
            this.characters[_loc2_].aiStart();
            this.characters[_loc2_].canShoot = this.characters[_loc2_] == this.selectedCharacter;
            this.characters[_loc2_].canThrow = false;
            _loc2_ = _loc2_ + 1;
         }
         this.aiCanBailOut = true;
      }
   }
   function getTeamType()
   {
      var _loc2_ = this.characters[0].linkageName;
      _loc2_ = _loc2_.split("Captain").join("");
      _loc2_ = _loc2_.split("Chief").join("");
      return _loc2_;
   }
   function getTotalHealth()
   {
      var _loc3_ = 0;
      var _loc2_ = 0;
      while(_loc2_ < this.characters.length)
      {
         if(this.characters[_loc2_].alive)
         {
            _loc3_ += this.characters[_loc2_].health;
         }
         _loc2_ = _loc2_ + 1;
      }
      return _loc3_;
   }
   function getAverageHealth()
   {
      return this.getTotalHealth() / this.characters.length;
   }
   function destroy()
   {
      var _loc2_ = 0;
      while(_loc2_ < this.characters.length)
      {
         this.characters[_loc2_].unequip();
         this.characters[_loc2_].destroy();
         _loc2_ = _loc2_ + 1;
      }
   }
}
