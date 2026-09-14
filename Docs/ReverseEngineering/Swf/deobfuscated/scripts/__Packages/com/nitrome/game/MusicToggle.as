var §\x01§ = 241;
var §\x0f§ = 1;
class com.nitrome.game.MusicToggle extends MovieClip
{
   function MusicToggle()
   {
      super();
      if(_root.mc.getMusicOn() == false)
      {
         this.gotoAndStop("_off_up");
      }
      else
      {
         this.gotoAndStop("_on_up");
      }
   }
   function onRollOver()
   {
      this.updateGraphic(true);
      _root.sfx_manager.playSound("rollover");
   }
   function onRollOut()
   {
      this.updateGraphic(false);
   }
   function onPress()
   {
      _root.mc.toggleMusic();
      this.updateGraphic(true);
   }
   function updateGraphic(mouse_is_over)
   {
      if(mouse_is_over == true)
      {
         if(_root.mc.getMusicOn() == true)
         {
            this.gotoAndStop("_on_over");
         }
         else if(_root.mc.getMusicOn() == false)
         {
            this.gotoAndStop("_off_over");
         }
      }
      else if(mouse_is_over == false)
      {
         if(_root.mc.getMusicOn() == true)
         {
            this.gotoAndStop("_on_up");
         }
         else if(_root.mc.getMusicOn() == false)
         {
            this.gotoAndStop("_off_up");
         }
      }
   }
}
