var §\x01§ = 982;
var §\x0f§ = 1;
class com.nitrome.buttons.OnePlayerButton extends com.nitrome.buttons.SimpleButton
{
   function OnePlayerButton()
   {
      super();
   }
   function onPress()
   {
      _root.game_type = 1;
      _root.tt.doTween("level_select_1p");
   }
}
