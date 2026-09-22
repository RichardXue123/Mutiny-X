var §\x01§ = 783;
var §\x0f§ = 1;
class com.nitrome.buttons.BackButton extends com.nitrome.buttons.SimpleButton
{
   var _name;
   function BackButton()
   {
      super();
   }
   function onRelease()
   {
      if(this._name == "back_ls_button")
      {
         _root.tt.doTween("game_select");
      }
      else
      {
         _root.tt.doTween("title_screen");
      }
   }
}
