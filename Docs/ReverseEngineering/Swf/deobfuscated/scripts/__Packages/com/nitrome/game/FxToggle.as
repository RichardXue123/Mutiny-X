var §\x01§ = 150;
var §\x0f§ = 1;
class com.nitrome.game.FxToggle extends MovieClip
{
   function FxToggle()
   {
      super();
      if(_root.mc.getSfxOn() == false)
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
      _root.mc.toggleSfx();
      this.updateGraphic(true);
   }
   function updateGraphic(mouse_is_over)
   {
      if(mouse_is_over == true)
      {
         if(_root.mc.getSfxOn() == true)
         {
            this.gotoAndStop("_on_over");
         }
         else if(_root.mc.getSfxOn() == false)
         {
            this.gotoAndStop("_off_over");
         }
      }
      else if(mouse_is_over == false)
      {
         if(_root.mc.getSfxOn() == true)
         {
            this.gotoAndStop("_on_up");
         }
         else if(_root.mc.getSfxOn() == false)
         {
            this.gotoAndStop("_off_up");
         }
      }
   }
}
