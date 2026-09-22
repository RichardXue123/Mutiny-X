var §\x01§ = 56;
var §\x0f§ = 1;
class com.nitrome.throwgame.Explosion extends com.nitrome.util.Clip
{
   var caster;
   var maxDamage;
   var radius;
   function Explosion(x, y, size, maxDamage, caster)
   {
      super(com.nitrome.throwgame.Controller.content,"explosion");
      this.x = x;
      this.y = y;
      this.show();
      this.mc._xscale = this.mc._yscale = size;
      this.radius = size / 2;
      this.radius += 20;
      this.maxDamage = maxDamage;
      this.caster = caster;
   }
   function hit()
   {
      var _loc11_ = 0;
      var _loc10_;
      var _loc4_;
      var _loc2_;
      var _loc14_;
      var _loc13_;
      var _loc15_;
      var _loc7_;
      var _loc6_;
      var _loc5_;
      while(_loc11_ < com.nitrome.throwgame.Controller.teams.length)
      {
         _loc10_ = com.nitrome.throwgame.Controller.teams[_loc11_];
         _loc4_ = 0;
         while(_loc4_ < _loc10_.characters.length)
         {
            _loc2_ = _loc10_.characters[_loc4_];
            if(_loc2_.alive)
            {
               _loc14_ = _loc2_.x - this.x;
               _loc13_ = _loc2_.y - this.y;
               _loc15_ = _loc14_ * _loc14_ + _loc13_ * _loc13_;
               if(_loc15_ <= this.radius * this.radius)
               {
                  _loc7_ = Math.sqrt(_loc15_);
                  _loc14_ /= _loc7_;
                  _loc13_ /= _loc7_;
                  _loc6_ = 1 - _loc7_ / this.radius;
                  _loc5_ = 0.06 * _loc6_ * this.maxDamage;
                  _loc2_.velocityX += _loc14_ * 5 * _loc5_;
                  _loc2_.velocityY += _loc13_ * 5 * _loc5_;
                  _loc2_.velocityY -= _loc5_ * 6;
                  _loc2_.subtractHealth(this.maxDamage * _loc6_);
                  this.caster.evilness += _loc6_;
                  _loc2_.hit = true;
               }
            }
            _loc4_ = _loc4_ + 1;
         }
         _loc11_ = _loc11_ + 1;
      }
      var _loc12_ = com.nitrome.throwgame.Controller.boxes.length - 1;
      var _loc3_;
      var _loc9_;
      var _loc8_;
      while(_loc12_ >= 0)
      {
         _loc3_ = com.nitrome.throwgame.Controller.boxes[_loc12_];
         _loc9_ = this.x;
         _loc8_ = this.y;
         if(_loc9_ < _loc3_.x - _loc3_.leftExtent)
         {
            _loc9_ = _loc3_.x - _loc3_.leftExtent;
         }
         if(_loc9_ > _loc3_.x + _loc3_.rightExtent)
         {
            _loc9_ = _loc3_.x + _loc3_.rightExtent;
         }
         if(_loc8_ < _loc3_.y - _loc3_.topExtent)
         {
            _loc8_ = _loc3_.y - _loc3_.topExtent;
         }
         if(_loc8_ > _loc3_.y + _loc3_.bottomExtent)
         {
            _loc8_ = _loc3_.y + _loc3_.bottomExtent;
         }
         _loc14_ = this.x - _loc9_;
         _loc13_ = this.y - _loc8_;
         _loc15_ = _loc14_ * _loc14_ + _loc13_ * _loc13_;
         if(_loc15_ <= this.radius * this.radius)
         {
            _loc3_.explode();
         }
         _loc12_ = _loc12_ - 1;
      }
   }
}
