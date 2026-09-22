var §\x01§ = 993;
var §\x0f§ = 1;
class com.nitrome.buttons.NextLevelButton extends com.nitrome.buttons.SimpleButton
{
   function NextLevelButton()
   {
      super();
   }
   function onRelease()
   {
      com.nitrome.throwgame.Controller.popup.show = false;
      com.nitrome.game.TransitionTween(_root.tt).doTweenWithFunction(function()
      {
         _root.selected_level++;
         com.nitrome.throwgame.Controller.changeLevel(_root.selected_level);
      }
      );
   }
}
