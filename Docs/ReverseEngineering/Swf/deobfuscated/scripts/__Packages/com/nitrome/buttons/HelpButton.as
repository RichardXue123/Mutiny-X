var §\x01§ = 803;
var §\x0f§ = 1;
class com.nitrome.buttons.HelpButton extends com.nitrome.buttons.SimpleButton
{
   function HelpButton()
   {
      super();
   }
   function onRelease()
   {
      _root.tt.doTween("help");
   }
}
