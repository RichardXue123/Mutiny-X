var §\x01§ = 82;
var §\x0f§ = 1;
class com.nitrome.buttons.SimpleButton extends MovieClip
{
   function SimpleButton()
   {
      super();
   }
   function onLoad()
   {
      this.gotoAndStop("up");
   }
   function onRollOver()
   {
      this.gotoAndStop("over");
   }
   function onRollOut()
   {
      this.gotoAndStop("up");
   }
   function onReleaseOutside()
   {
      this.gotoAndStop("up");
   }
}
