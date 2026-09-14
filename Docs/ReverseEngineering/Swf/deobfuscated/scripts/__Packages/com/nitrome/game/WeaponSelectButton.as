var §\x01§ = 147;
var §\x0f§ = 1;
class com.nitrome.game.WeaponSelectButton extends com.nitrome.game.SimpleButton
{
   var _name;
   var _parent;
   var count;
   var gotoAndStop;
   var icon;
   var infinity;
   var useHandCursor;
   var weapon;
   var hover = false;
   var disabled = false;
   var blue = false;
   static var hoverText = {hover_cherryBomb:["cherry bomb","Basic weak weapon which|explodes on impact.|Click the cherry bomb and drag|with your mouse to aim and set|the power."],hover_dynamite:["dynamite","This weapon explodes when it|comes to rest.|Click the dynamite and drag|with your mouse to aim and set|the power."],hover_boulder:["boulder","Large boulder which can bash|other players out of the way.|Click the boulder and drag|with your mouse to aim and set|the power."],hover_piecesOfEight:["pieces of eight","Eight coins which inflict a small|amount of damage. You get|eight turns with this weapon.|Click each one and drag to aim|and set the power."],hover_rumBottle:["rum bottle","This weapon explodes on impact|and sets the nearby area on fire.|Click it and drag|with your mouse to aim and set|the power."],hover_banana:["banana","This weapon is very bouncy.|Click and drag with the mouse|to aim and set the power.|Then click your mouse again to|make it explode."],hover_parachuteBomb:["parachute bomb","Click and drag with the mouse|to aim and set the power.|As it drifts down use|your cursor as a fan to push it|left or right."],hover_woodenCrate:["crates","Click anywhere on the stage to|place down three crates.|use these to form a wall to|protect your characters."],hover_gunpowderBarrel:["gunpowder barrels","Click anywhere on the stage to|place down two barrels.|these will explode when hit by|a weapon."],hover_seagull:["seagull","Click on the screen to choose a|path for the seagull to fly.|Then click repeatedly to poop|on the enemy!"],hover_mine:["mine","Click the mine and drag|with the mouse to aim and|set the power.|It will detonate when|another player moves nearby."],hover_cannon:["cannon","Drag the cannon into position|within the circle.|click and drag the pin at the|back to turn the cannon.|release it to fire!"],hover_anchor:["anchor","Click anywhere in the stage to|drop a huge anchor down|onto enemies!"],hover_voodooDoll:["voodoo doll","choose an enemy player by|clicking on them. then click|and drag to throw the doll and|watch the enemy helplessly|fly off in the same direction!"],hover_tidalWave:["tidal wave","click to send a huge tidal wave|across the bottom of the stage.|It will affect all players it|hits."],hover_throw:["throw character","Click your character and drag|with the mouse to aim and set|the power.|you get to use this once per|turn before you use a weapon."],hover_endTurn:["end go","click here if you want to finish|your turn without using a|weapon."]};
   function WeaponSelectButton()
   {
      super();
      this.weapon = this._name.substr(7);
   }
   function onLoad()
   {
      this.updateGraphic();
   }
   function onRollOver()
   {
      this.hover = true;
      this.updateGraphic();
      if(!this.disabled)
      {
         if(!com.nitrome.game.WeaponSelectButton.hoverText["hover_" + this.weapon])
         {
            return undefined;
         }
         com.nitrome.game.WeaponSelectPanel(this._parent).title.text = com.nitrome.game.WeaponSelectButton.hoverText["hover_" + this.weapon][0];
         com.nitrome.game.WeaponSelectPanel(this._parent).description.text = com.nitrome.game.WeaponSelectButton.hoverText["hover_" + this.weapon][1];
      }
   }
   function onRollOut()
   {
      this.hover = false;
      this.updateGraphic();
      com.nitrome.game.WeaponSelectPanel(this._parent).title.text = "Weapons";
      com.nitrome.game.WeaponSelectPanel(this._parent).description.text = "Click one of the options above|to select it.";
   }
   function updateGraphic()
   {
      var _loc2_ = !this.blue ? "red" : "blue";
      if(this.disabled)
      {
         this.gotoAndStop("disabled");
      }
      else if(this.hover)
      {
         this.gotoAndStop(_loc2_ + "_over");
      }
      else
      {
         this.gotoAndStop(_loc2_ + "_up");
      }
      this.useHandCursor = !this.disabled;
   }
   function onEnterFrame()
   {
      if(!com.nitrome.throwgame.Controller.currentTeam.selectedCharacter)
      {
         return undefined;
      }
      if(this.blue != (com.nitrome.throwgame.Controller.currentTeam.number == 2))
      {
         this.blue = com.nitrome.throwgame.Controller.currentTeam.number == 2;
         this.updateGraphic();
      }
      var _loc2_;
      if(this.weapon == "throw")
      {
         if(this.disabled != !com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.canThrow)
         {
            this.disabled = !com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.canThrow;
            this.updateGraphic();
         }
      }
      else if(this.weapon != "endTurn")
      {
         _loc2_ = com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.numberOfWeapon(this.weapon);
         if(this.disabled != (_loc2_ == 0))
         {
            this.disabled = _loc2_ == 0;
            this.updateGraphic();
         }
         if(com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.weaponIsInfinite(this.weapon))
         {
            this.infinity._visible = true;
            this.count._visible = false;
         }
         else
         {
            this.count.text = (_loc2_ >= 10 ? "" : "0") + _loc2_.toString();
            this.count._visible = true;
            this.infinity._visible = false;
         }
         this.icon.gotoAndStop(this.weapon);
      }
   }
   function onPress()
   {
      if(this.disabled)
      {
         return undefined;
      }
      if(!com.nitrome.game.WeaponSelectPanel(this._parent).contentsActive)
      {
         return undefined;
      }
      var _loc2_;
      if(this.weapon == "throw")
      {
         com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.unequip();
         com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.weaponSelected = true;
      }
      else if(this.weapon == "endTurn")
      {
         com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.canThrow = false;
         com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.canShoot = false;
         com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.unequip();
         com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.thrown = true;
         com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.throwFinished = true;
         com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.weaponSelected = true;
      }
      else if(com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.numberOfWeapon(this.weapon) > 0)
      {
         _loc2_ = com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.firstIndexOfWeapon(this.weapon);
         com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.equip(_loc2_);
         com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.weaponSelected = true;
      }
   }
}
