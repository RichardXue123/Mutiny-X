var §\x01§ = 312;
var §\x0f§ = 1;
class com.nitrome.game.CornerQuitButton extends com.nitrome.buttons.SimpleButton
{
   function CornerQuitButton()
   {
      super();
   }
   function onRelease()
   {
      if(com.nitrome.throwgame.Controller.popup.show)
      {
         return undefined;
      }
      com.nitrome.throwgame.Controller.popup.gotoAndStop("quit_prompt");
      com.nitrome.throwgame.Controller.popup.show = true;
   }
}
