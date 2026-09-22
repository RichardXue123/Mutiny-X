var §\x01§ = 943;
var §\x0f§ = 1;
class com.nitrome.throwgame.SweepingFlame extends com.nitrome.util.Clip
{
   var toRight;
   function SweepingFlame(x, y, toRight)
   {
      super(com.nitrome.throwgame.Controller.characterLayer,"sweepingFlame");
      this.x = x;
      this.y = y;
      this.toRight = toRight;
      this.show();
      var _loc9_ = 0;
      var _loc7_;
      var _loc4_;
      var _loc3_;
      var _loc6_;
      var _loc5_;
      var _loc8_;
      while(_loc9_ < com.nitrome.throwgame.Controller.teams.length)
      {
         _loc7_ = com.nitrome.throwgame.Controller.teams[_loc9_];
         _loc4_ = 0;
         while(_loc4_ < _loc7_.characters.length)
         {
            _loc3_ = _loc7_.characters[_loc4_];
            if(_loc3_.alive)
            {
               _loc6_ = _loc3_.x - x;
               _loc5_ = _loc3_.y + _loc3_.bottomExtent - y;
               _loc8_ = _loc6_ * _loc6_ + _loc5_ * _loc5_;
               if(_loc8_ < 64)
               {
                  _loc3_.velocityX = (Math.random() - 0.5) * 8;
                  _loc3_.velocityY = - (Math.random() * 2 + 6);
                  _loc3_.subtractHealth(30);
               }
            }
            _loc4_ = _loc4_ + 1;
         }
         _loc9_ = _loc9_ + 1;
      }
   }
   function createNext()
   {
      var _loc4_ = this.x + 8 * (!this.toRight ? -1 : 1);
      var _loc3_ = _loc4_ >> 5;
      var _loc2_ = this.y >> 5;
      if(!com.nitrome.throwgame.Controller.tileSystem.tileGrid[_loc3_][_loc2_])
      {
         return undefined;
      }
      if(com.nitrome.throwgame.Controller.tileSystem.tileGrid[_loc3_][_loc2_ - 1])
      {
         return undefined;
      }
      new com.nitrome.throwgame.SweepingFlame(_loc4_,this.y,this.toRight);
      com.nitrome.throwgame.Controller.inactivity = 0;
   }
}
