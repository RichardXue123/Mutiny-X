var §\x01§ = 667;
var §\x0f§ = 1;
class com.nitrome.game.Preloader extends MovieClip
{
   var load_bar;
   var load_text;
   var onEnterFrame;
   var start_t;
   function Preloader()
   {
      super();
      this.load_bar._xscale = 1;
      this.start_t = getTimer();
      this.onEnterFrame = function()
      {
         var _loc6_ = _root.getBytesLoaded();
         var _loc5_ = _root.getBytesTotal();
         var _loc4_ = Math.round(_loc6_ / _loc5_ * 100);
         var _loc7_ = getTimer();
         var _loc3_ = Math.round((_loc7_ - this.start_t) / 250);
         if(_loc3_ > 100)
         {
            _loc3_ = 100;
         }
         if(_loc4_ < _loc3_)
         {
            trace("percent:" + _loc4_);
            this.load_text.text = String(_loc4_);
            this.load_bar._xscale = _loc4_;
         }
         else
         {
            trace("tt:" + _loc3_);
            this.load_text.text = String(_loc3_);
            this.load_bar._xscale = _loc3_;
         }
         trace(_loc3_);
         trace(_loc6_ == _loc5_);
         trace(_loc3_ >= 100);
         if(_loc6_ == _loc5_ && _loc3_ >= 100)
         {
            _root.tt.doTween("mtv");
            delete this.onEnterFrame;
         }
      };
   }
}
