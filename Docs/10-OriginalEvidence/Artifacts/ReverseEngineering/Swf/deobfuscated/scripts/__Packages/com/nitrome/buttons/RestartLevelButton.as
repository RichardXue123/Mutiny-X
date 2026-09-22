var §\x01§ = 407;
var §\x0f§ = 1;
class com.nitrome.buttons.RestartLevelButton extends com.nitrome.buttons.SimpleButton
{
   function RestartLevelButton()
   {
      super();
   }
   function onRelease()
   {
      com.nitrome.throwgame.Controller.popup.show = false;
      com.nitrome.game.TransitionTween(_root.tt).doTweenWithFunction(function()
      {
         com.nitrome.throwgame.Controller.changeLevel(_root.selected_level);
         _root.score = 0;
      }
      );
   }
}
