var §\x01§ = 759;
var §\x0f§ = 1;
class com.nitrome.buttons.SubmitScoreButton extends com.nitrome.buttons.SimpleButton
{
   var _visible;
   function SubmitScoreButton()
   {
      super();
   }
   function onLoad()
   {
      this._visible = Boolean(_root.score > 0);
   }
   function onRelease()
   {
      if(_root.score > 0)
      {
         com.nitrome.game.TransitionTween(_root.tt).doTweenWithFunction(function()
         {
            com.nitrome.throwgame.Controller.endGame();
            _root.gotoAndStop("submit_score");
         }
         );
      }
   }
}
