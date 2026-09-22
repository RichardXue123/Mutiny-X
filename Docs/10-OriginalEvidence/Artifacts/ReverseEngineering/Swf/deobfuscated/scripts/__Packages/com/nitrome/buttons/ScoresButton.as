var §\x01§ = 884;
var §\x0f§ = 1;
class com.nitrome.buttons.ScoresButton extends com.nitrome.buttons.SimpleButton
{
   function ScoresButton()
   {
      super();
   }
   function onRelease()
   {
      _root.tt.doTween("view_scores");
   }
}
