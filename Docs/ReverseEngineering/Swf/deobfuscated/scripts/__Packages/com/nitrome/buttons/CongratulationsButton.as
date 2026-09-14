var §\x01§ = 566;
var §\x0f§ = 1;
class com.nitrome.buttons.CongratulationsButton extends com.nitrome.buttons.SimpleButton
{
   function CongratulationsButton()
   {
      super();
   }
   function onRelease()
   {
      com.nitrome.throwgame.Controller.popup.show = false;
      com.nitrome.game.TransitionTween(_root.tt).doTweenWithFunction(function()
      {
         com.nitrome.throwgame.Controller.endGame();
         _root.gotoAndStop("congratulations");
      }
      );
   }
}
