var §\x01§ = 409;
var §\x0f§ = 1;
class com.nitrome.game.WeaponSelectPanel extends MovieClip
{
   var contentsActive = false;
   function WeaponSelectPanel()
   {
      super();
      this.blendMode = "layer";
      this._alpha = 0;
   }
   function onEnterFrame()
   {
      if(com.nitrome.throwgame.Controller.currentTeam.selectedCharacter && !com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.weaponSelected && !com.nitrome.throwgame.Controller.currentTeam.aiControlled)
      {
         this._alpha += 25;
         if(this._alpha >= 100)
         {
            this._alpha = 100;
            this.contentsActive = true;
         }
         this._visible = true;
      }
      else
      {
         this._alpha -= 25;
         if(this._alpha <= 0)
         {
            this._alpha = -25;
            this._visible = false;
         }
         this.contentsActive = false;
      }
   }
}
