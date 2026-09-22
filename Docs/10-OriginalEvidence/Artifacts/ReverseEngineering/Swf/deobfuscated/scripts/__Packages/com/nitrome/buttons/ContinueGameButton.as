var §\x01§ = 734;
var §\x0f§ = 1;
class com.nitrome.buttons.ContinueGameButton extends com.nitrome.buttons.SimpleButton
{
   function ContinueGameButton()
   {
      super();
   }
   function onRelease()
   {
      com.nitrome.throwgame.Controller.popup.show = false;
   }
}
