var §\x01§ = 116;
var §\x0f§ = 1;
class com.nitrome.game.CancelWeaponButton extends com.nitrome.game.SimpleButton
{
   function CancelWeaponButton()
   {
      super();
   }
   function onPress()
   {
      com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.weaponSelected = false;
      com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.unequip();
   }
}
