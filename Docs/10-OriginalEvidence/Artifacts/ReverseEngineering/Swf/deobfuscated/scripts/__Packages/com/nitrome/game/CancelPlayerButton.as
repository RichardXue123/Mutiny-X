var §\x01§ = 89;
var §\x0f§ = 1;
class com.nitrome.game.CancelPlayerButton extends com.nitrome.game.SimpleButton
{
   var _parent;
   var _visible;
   function CancelPlayerButton()
   {
      super();
   }
   function onPress()
   {
      if(com.nitrome.throwgame.Controller.currentTeam.selectedCharacter)
      {
         com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.unequip();
         if(com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.canThrow)
         {
            com.nitrome.throwgame.Controller.currentTeam.selectedCharacter = null;
         }
      }
   }
   function onEnterFrame()
   {
      this._visible = com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.canThrow;
   }
   function onRollOver()
   {
      com.nitrome.game.WeaponSelectPanel(this._parent).title.text = "close";
      com.nitrome.game.WeaponSelectPanel(this._parent).description.text = "click here to cancel and select|another player.";
   }
   function onRollOut()
   {
      com.nitrome.game.WeaponSelectPanel(this._parent).title.text = "weapons";
      com.nitrome.game.WeaponSelectPanel(this._parent).description.text = "Click one of the options above|to select it.";
   }
}
