var §\x01§ = 672;
var §\x0f§ = 1;
class com.nitrome.game.SimpleButton extends MovieClip
{
   function SimpleButton()
   {
      super();
   }
   function onLoad()
   {
      this.gotoAndStop("_up");
   }
   function onRollOver()
   {
      this.gotoAndStop("_over");
   }
   function onRollOut()
   {
      this.gotoAndStop("_up");
   }
}
