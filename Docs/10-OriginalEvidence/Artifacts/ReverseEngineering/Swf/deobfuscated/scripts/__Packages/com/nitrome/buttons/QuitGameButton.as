var §\x01§ = 886;
var §\x0f§ = 1;
class com.nitrome.buttons.QuitGameButton extends com.nitrome.buttons.SimpleButton
{
   function QuitGameButton()
   {
      super();
   }
   function onRelease()
   {
      com.nitrome.game.TransitionTween(_root.tt).doTweenWithFunction(function()
      {
         com.nitrome.throwgame.Controller.endGame();
         if(_root.game_type == 2)
         {
            _root.gotoAndStop("level_select_2p");
         }
         else
         {
            _root.gotoAndStop("level_select_1p");
         }
      }
      );
   }
}
