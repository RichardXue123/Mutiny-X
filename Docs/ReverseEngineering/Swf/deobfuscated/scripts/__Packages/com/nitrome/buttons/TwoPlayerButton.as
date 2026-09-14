var §\x01§ = 962;
var §\x0f§ = 1;
class com.nitrome.buttons.TwoPlayerButton extends com.nitrome.buttons.SimpleButton
{
   function TwoPlayerButton()
   {
      super();
   }
   function onPress()
   {
      _root.game_type = 2;
      _root.won_p1 = 0;
      _root.won_p2 = 0;
      _root.tt.doTween("level_select_2p");
   }
}
