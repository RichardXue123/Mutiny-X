var §\x01§ = 701;
var §\x0f§ = 1;
class com.nitrome.util.Trig
{
   static var cosTable;
   static var sinTable;
   function Trig()
   {
   }
   static function setup()
   {
      com.nitrome.util.Trig.sinTable = [];
      com.nitrome.util.Trig.cosTable = [];
      var _loc1_ = 0;
      while(_loc1_ <= 360)
      {
         com.nitrome.util.Trig.sinTable.push(Math.sin(_loc1_ * 3.141592653589793 / 180));
         com.nitrome.util.Trig.cosTable.push(Math.cos(_loc1_ * 3.141592653589793 / 180));
         _loc1_ = _loc1_ + 1;
      }
   }
}
