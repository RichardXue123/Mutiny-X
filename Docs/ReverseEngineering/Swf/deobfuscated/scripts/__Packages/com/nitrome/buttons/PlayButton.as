var §\x01§ = 859;
var §\x0f§ = 1;
class com.nitrome.buttons.PlayButton extends com.nitrome.buttons.SimpleButton
{
   function PlayButton()
   {
      super();
   }
   function onRelease()
   {
      _root.tt.doTween("game_select");
   }
}
