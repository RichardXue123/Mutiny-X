var §\x01§ = 111;
var §\x0f§ = 1;
class com.nitrome.buttons.CreditsButton extends com.nitrome.buttons.SimpleButton
{
   function CreditsButton()
   {
      super();
   }
   function onRelease()
   {
      _root.tt.doTween("credits");
   }
}
