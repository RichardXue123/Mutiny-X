var §\x01§ = 396;
var §\x0f§ = 1;
class com.nitrome.util.Global
{
   function Global()
   {
   }
   static function slide(from, to, by)
   {
      if(from < to)
      {
         from += by;
         if(from > to)
         {
            from = to;
         }
         return from;
      }
      from -= by;
      if(from < to)
      {
         from = to;
      }
      return from;
   }
   static function deceleratingSweep(current, target, speed, acceleration, deceleration)
   {
      var _loc1_ = speed * speed / (2 * deceleration);
      if(current < target)
      {
         if(_loc1_ > target)
         {
            return - deceleration;
         }
         return acceleration;
      }
      if(_loc1_ < target)
      {
         return deceleration;
      }
      return - acceleration;
   }
   static function localToLocal(pt, fromContext, toContext)
   {
      var _loc1_ = new flash.geom.Point(pt.x,pt.y);
      fromContext.localToGlobal(_loc1_);
      toContext.globalToLocal(_loc1_);
      return _loc1_;
   }
   static function negativeModulo(a, b)
   {
      if(a >= 0)
      {
         return a % b - b;
      }
      return a % b;
   }
   static function sign(n)
   {
      if(n > 0)
      {
         return 1;
      }
      if(n < 0)
      {
         return -1;
      }
      return 0;
   }
   static function whiteOut(visibility)
   {
      var _loc1_;
      var _loc2_;
      if(visibility > 0.5)
      {
         _loc1_ = (visibility - 0.5) * 2;
         _loc2_ = 255 - _loc1_ * 255;
         return new flash.geom.ColorTransform(_loc1_,_loc1_,_loc1_,1,_loc2_,_loc2_,_loc2_,0);
      }
      return new flash.geom.ColorTransform(0,0,0,visibility * 2,255,255,255,0);
   }
}
