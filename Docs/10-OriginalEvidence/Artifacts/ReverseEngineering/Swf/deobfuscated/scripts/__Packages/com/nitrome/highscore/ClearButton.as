var §\x01§ = 479;
var §\x0f§ = 1;
class com.nitrome.highscore.ClearButton extends com.nitrome.buttons.SimpleButton
{
   var _parent;
   function ClearButton()
   {
      super();
   }
   function onPress()
   {
      this._parent.clearName();
   }
}
