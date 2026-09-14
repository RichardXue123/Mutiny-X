function §\x04\x05§()
{
   set("\x03",582 % 511 * true);
   return eval("\x03");
}
var §\x01§ = 61 + "\x04\x05"();
var _loc5_;
var _loc4_;
var _loc2_;
var _loc3_;
while(true)
{
   if(eval("\x01") == 132)
   {
      set("\x01",eval("\x01") + 183);
      §§push(true);
   }
   else if(eval("\x01") == 715)
   {
      set("\x01",eval("\x01") - 156);
      §§push(true);
   }
   else
   {
      if(eval("\x01") == 561)
      {
         set("\x01",eval("\x01") + 361);
         toggleHighQuality();
         _loc5_ = §§pop() * §§pop()[§§pop() < §§pop()];
         _loc4_ = _loc9_ * _loc1_[§§constant(15)];
         _loc2_ = 0;
         while(_loc2_ < _loc8_)
         {
            if(_loc1_[§§constant(8)])
            {
               _loc1_[§§constant(26)](_loc1_[§§constant(10)][§§constant(11)] + _loc7_,_loc1_[§§constant(10)][§§constant(12)] + _loc6_);
               _loc1_[§§constant(19)](_loc1_[§§constant(10)][§§constant(11)] + _loc5_,_loc1_[§§constant(10)][§§constant(12)] + _loc4_);
            }
            else
            {
               _loc1_[§§constant(19)](_loc1_[§§constant(10)][§§constant(11)] + _loc5_,_loc1_[§§constant(10)][§§constant(12)] + _loc4_);
               _loc1_[§§constant(26)](_loc1_[§§constant(10)][§§constant(11)] + _loc7_,_loc1_[§§constant(10)][§§constant(12)] + _loc6_);
            }
            _loc2_ = _loc2_ + 1;
         }
         _loc3_ -= _loc1_[§§constant(16)] * _loc8_;
         if(_loc1_[§§constant(8)])
         {
            if(_loc3_ > _loc1_[§§constant(14)])
            {
               _loc1_[§§constant(26)](_loc1_[§§constant(10)][§§constant(11)] + _loc11_ * _loc1_[§§constant(14)],_loc1_[§§constant(10)][§§constant(12)] + _loc9_ * _loc1_[§§constant(14)]);
               _loc1_[§§constant(19)](_loc12_,_loc10_);
               _loc1_[§§constant(9)] = _loc1_[§§constant(15)] - (_loc3_ - _loc1_[§§constant(14)]);
               _loc1_[§§constant(8)] = false;
            }
            else
            {
               _loc1_[§§constant(26)](_loc12_,_loc10_);
               if(_loc3_ == _loc1_[§§constant(14)])
               {
                  _loc1_[§§constant(9)] = 0;
                  _loc1_[§§constant(8)] = !_loc1_[§§constant(8)];
               }
               else
               {
                  _loc1_[§§constant(9)] = _loc1_[§§constant(14)] - _loc3_;
                  _loc1_[§§constant(19)](_loc12_,_loc10_);
               }
            }
         }
         else if(_loc3_ > _loc1_[§§constant(15)])
         {
            _loc1_[§§constant(19)](_loc1_[§§constant(10)][§§constant(11)] + _loc11_ * _loc1_[§§constant(15)],_loc1_[§§constant(10)][§§constant(12)] + _loc9_ * _loc1_[§§constant(15)]);
            _loc1_[§§constant(26)](_loc12_,_loc10_);
            _loc1_[§§constant(9)] = _loc1_[§§constant(14)] - (_loc3_ - _loc1_[§§constant(15)]);
            _loc1_[§§constant(8)] = true;
         }
         else
         {
            _loc1_[§§constant(19)](_loc12_,_loc10_);
            if(_loc3_ == _loc1_[§§constant(15)])
            {
               _loc1_[§§constant(9)] = 0;
               _loc1_[§§constant(8)] = !_loc1_[§§constant(8)];
            }
            else
            {
               _loc1_[§§constant(9)] = _loc1_[§§constant(15)] - _loc3_;
            }
         }
         §§pop()[§§pop()] = §§pop();
         _loc2_[§§constant(28)] = function(cx, cy, x, y)
         {
            var _loc8_ = this[§§constant(10)][§§constant(11)];
            var _loc7_ = this[§§constant(10)][§§constant(12)];
            var _loc14_ = this[§§constant(29)](_loc8_,_loc7_,cx,cy,x,y);
            var _loc3_ = 0;
            var _loc4_ = 0;
            var _loc2_;
            if(this[§§constant(9)])
            {
               if(this[§§constant(9)] > _loc14_)
               {
                  if(this[§§constant(8)])
                  {
                     this[§§constant(30)](cx,cy,x,y);
                  }
                  else
                  {
                     this[§§constant(19)](x,y);
                  }
                  this[§§constant(9)] -= _loc14_;
                  return undefined;
               }
               _loc3_ = this[§§constant(9)] / _loc14_;
               _loc2_ = this[§§constant(31)](_loc8_,_loc7_,cx,cy,x,y,_loc3_);
               if(this[§§constant(8)])
               {
                  this[§§constant(30)](_loc2_[2],_loc2_[3],_loc2_[4],_loc2_[5]);
               }
               else
               {
                  this[§§constant(19)](_loc2_[4],_loc2_[5]);
               }
               this[§§constant(9)] = 0;
               this[§§constant(8)] = !this[§§constant(8)];
               if(!_loc14_)
               {
                  return undefined;
               }
            }
            var _loc15_ = _loc14_ - _loc14_ * _loc3_;
            var _loc16_ = eval(§§constant(21))[§§constant(27)](_loc15_ / this[§§constant(16)]);
            var _loc12_ = this[§§constant(14)] / _loc14_;
            var _loc13_ = this[§§constant(15)] / _loc14_;
            var _loc11_;
            if(_loc16_)
            {
               _loc11_ = 0;
               while(_loc11_ < _loc16_)
               {
                  if(this[§§constant(8)])
                  {
                     _loc4_ = _loc3_ + _loc12_;
                     _loc2_ = this[§§constant(32)](_loc8_,_loc7_,cx,cy,x,y,_loc3_,_loc4_);
                     this[§§constant(30)](_loc2_[2],_loc2_[3],_loc2_[4],_loc2_[5]);
                     _loc3_ = _loc4_;
                     _loc4_ = _loc3_ + _loc13_;
                     _loc2_ = this[§§constant(32)](_loc8_,_loc7_,cx,cy,x,y,_loc3_,_loc4_);
                     this[§§constant(19)](_loc2_[4],_loc2_[5]);
                  }
                  else
                  {
                     _loc4_ = _loc3_ + _loc13_;
                     _loc2_ = this[§§constant(32)](_loc8_,_loc7_,cx,cy,x,y,_loc3_,_loc4_);
                     this[§§constant(19)](_loc2_[4],_loc2_[5]);
                     _loc3_ = _loc4_;
                     _loc4_ = _loc3_ + _loc12_;
                     _loc2_ = this[§§constant(32)](_loc8_,_loc7_,cx,cy,x,y,_loc3_,_loc4_);
                     this[§§constant(30)](_loc2_[2],_loc2_[3],_loc2_[4],_loc2_[5]);
                  }
                  _loc3_ = _loc4_;
                  _loc11_ = _loc11_ + 1;
               }
            }
            _loc15_ = _loc14_ - _loc14_ * _loc3_;
            if(this[§§constant(8)])
            {
               if(_loc15_ > this[§§constant(14)])
               {
                  _loc4_ = _loc3_ + _loc12_;
                  _loc2_ = this[§§constant(32)](_loc8_,_loc7_,cx,cy,x,y,_loc3_,_loc4_);
                  this[§§constant(30)](_loc2_[2],_loc2_[3],_loc2_[4],_loc2_[5]);
                  this[§§constant(19)](x,y);
                  this[§§constant(9)] = this[§§constant(15)] - (_loc15_ - this[§§constant(14)]);
                  this[§§constant(8)] = false;
               }
               else
               {
                  _loc2_ = this[§§constant(33)](_loc8_,_loc7_,cx,cy,x,y,_loc3_);
                  this[§§constant(30)](_loc2_[2],_loc2_[3],_loc2_[4],_loc2_[5]);
                  if(_loc14_ == this[§§constant(14)])
                  {
                     this[§§constant(9)] = 0;
                     this[§§constant(8)] = !this[§§constant(8)];
                  }
                  else
                  {
                     this[§§constant(9)] = this[§§constant(14)] - _loc15_;
                     this[§§constant(19)](x,y);
                  }
               }
            }
            else if(_loc15_ > this[§§constant(15)])
            {
               _loc4_ = _loc3_ + _loc13_;
               _loc2_ = this[§§constant(32)](_loc8_,_loc7_,cx,cy,x,y,_loc3_,_loc4_);
               this[§§constant(19)](_loc2_[4],_loc2_[5]);
               _loc2_ = this[§§constant(33)](_loc8_,_loc7_,cx,cy,x,y,_loc4_);
               this[§§constant(30)](_loc2_[2],_loc2_[3],_loc2_[4],_loc2_[5]);
               this[§§constant(9)] = this[§§constant(14)] - (_loc15_ - this[§§constant(15)]);
               this[§§constant(8)] = true;
            }
            else
            {
               this[§§constant(19)](x,y);
               if(_loc15_ == this[§§constant(15)])
               {
                  this[§§constant(9)] = 0;
                  this[§§constant(8)] = !this[§§constant(8)];
               }
               else
               {
                  this[§§constant(9)] = this[§§constant(15)] - _loc15_;
               }
            }
         };
         _loc2_[§§constant(34)] = function(Void)
         {
            this[§§constant(6)][§§constant(34)]();
         };
         _loc2_[§§constant(35)] = function(thickness, rgb, alpha)
         {
            this[§§constant(6)][§§constant(35)](thickness,rgb,alpha);
         };
         _loc2_[§§constant(36)] = function(rgb, alpha)
         {
            this[§§constant(6)][§§constant(36)](rgb,alpha);
         };
         _loc2_[§§constant(37)] = function(fillType, colors, alphas, ratios, matrix)
         {
            this[§§constant(6)][§§constant(37)](fillType,colors,alphas,ratios,matrix);
         };
         _loc2_[§§constant(38)] = function(Void)
         {
            this[§§constant(6)][§§constant(38)]();
         };
         _loc2_[§§constant(25)] = function(sx, sy, ex, ey)
         {
            if(arguments[§§constant(39)] == 2)
            {
               return eval(§§constant(21))[§§constant(40)](sx * sx + sy * sy);
            }
            var _loc3_ = ex - sx;
            var _loc2_ = ey - sy;
            return eval(§§constant(21))[§§constant(40)](_loc3_ * _loc3_ + _loc2_ * _loc2_);
         };
         _loc2_[§§constant(29)] = function(sx, sy, cx, cy, ex, ey, accuracy)
         {
            var _loc13_ = 0;
            var _loc11_ = sx;
            var _loc10_ = sy;
            var _loc9_;
            var _loc8_;
            var _loc2_;
            var _loc4_;
            var _loc7_;
            var _loc6_;
            var _loc5_;
            var _loc12_ = !accuracy ? this[§§constant(41)] : accuracy;
            var _loc3_ = 1;
            while(_loc3_ <= _loc12_)
            {
               _loc2_ = _loc3_ / _loc12_;
               _loc4_ = 1 - _loc2_;
               _loc7_ = _loc4_ * _loc4_;
               _loc6_ = 2 * _loc2_ * _loc4_;
               _loc5_ = _loc2_ * _loc2_;
               _loc9_ = _loc7_ * sx + _loc6_ * cx + _loc5_ * ex;
               _loc8_ = _loc7_ * sy + _loc6_ * cy + _loc5_ * ey;
               _loc13_ += this[§§constant(25)](_loc11_,_loc10_,_loc9_,_loc8_);
               _loc11_ = _loc9_;
               _loc10_ = _loc8_;
               _loc3_ = _loc3_ + 1;
            }
            return _loc13_;
         };
         _loc2_[§§constant(32)] = function(sx, sy, cx, cy, ex, ey, t1, t2)
         {
            if(t1 == 0)
            {
               return this[§§constant(31)](sx,sy,cx,cy,ex,ey,t2);
            }
            if(t2 == 1)
            {
               return this[§§constant(33)](sx,sy,cx,cy,ex,ey,t1);
            }
            var _loc2_ = this[§§constant(31)](sx,sy,cx,cy,ex,ey,t2);
            _loc2_[§§constant(42)](t1 / t2);
            return this[§§constant(33)][§§constant(43)](this,_loc2_);
         };
         _loc2_[§§constant(31)] = function(sx, sy, cx, cy, ex, ey, t)
         {
            if(t == undefined)
            {
               t = 1;
            }
            var _loc5_;
            var _loc4_;
            if(t != 1)
            {
               _loc5_ = cx + (ex - cx) * t;
               _loc4_ = cy + (ey - cy) * t;
               cx = sx + (cx - sx) * t;
               cy = sy + (cy - sy) * t;
               ex = cx + (_loc5_ - cx) * t;
               ey = cy + (_loc4_ - cy) * t;
            }
            return [sx,sy,cx,cy,ex,ey];
         };
         _loc2_[§§constant(33)] = function(sx, sy, cx, cy, ex, ey, t)
         {
            if(t == undefined)
            {
               t = 1;
            }
            var _loc5_;
            var _loc4_;
            if(t != 1)
            {
               _loc5_ = sx + (cx - sx) * t;
               _loc4_ = sy + (cy - sy) * t;
               cx += (ex - cx) * t;
               cy += (ey - cy) * t;
               sx = _loc5_ + (cx - _loc5_) * t;
               sy = _loc4_ + (cy - _loc4_) * t;
            }
            return [sx,sy,cx,cy,ex,ey];
         };
         _loc2_[§§constant(19)] = function(x, y)
         {
            this[§§constant(10)] = {(§§constant(11)):x,(§§constant(12)):y};
            this[§§constant(6)][§§constant(18)](x,y);
         };
         _loc2_[§§constant(26)] = function(x, y)
         {
            if(x == this[§§constant(10)][§§constant(11)] && y == this[§§constant(10)][§§constant(12)])
            {
               return undefined;
            }
            this[§§constant(10)] = {(§§constant(11)):x,(§§constant(12)):y};
            this[§§constant(6)][§§constant(20)](x,y);
         };
         _loc2_[§§constant(30)] = function(cx, cy, x, y)
         {
            if(cx == x && cy == y && x == this[§§constant(10)][§§constant(11)] && y == this[§§constant(10)][§§constant(12)])
            {
               return undefined;
            }
            this[§§constant(10)] = {(§§constant(11)):x,(§§constant(12)):y};
            this[§§constant(6)][§§constant(28)](cx,cy,x,y);
         };
         _loc2_[§§constant(41)] = 6;
         _loc2_[§§constant(8)] = true;
         _loc2_[§§constant(9)] = 0;
         _loc2_[§§constant(15)] = 0;
         _loc2_[§§constant(14)] = 0;
         _loc2_[§§constant(16)] = 0;
         §§constant(44)(eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(5)][§§constant(13)],null,1);
         break;
      }
      if(eval("\x01") == 114)
      {
         set("\x01",eval("\x01") - 18);
         _loc2_ = §§pop() + §§pop() * §§pop()[§§pop()];
         if(_loc2_ > _loc1_[§§constant(38)] * _loc1_[§§constant(38)])
         {
            _loc3_ = eval(§§constant(39))[§§constant(40)](_loc2_);
            _loc1_[§§constant(33)] *= _loc1_[§§constant(38)] / _loc3_;
            _loc1_[§§constant(36)] *= _loc1_[§§constant(38)] / _loc3_;
         }
         _loc1_[§§constant(41)][§§constant(42)]();
         _loc1_[§§constant(43)][§§constant(44)]();
         _loc1_[§§constant(43)] = null;
         §§pop()[§§pop()] = §§pop();
         _loc2_[§§constant(45)] = function()
         {
            var _loc8_ = eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(34)][§§constant(35)] - this[§§constant(21)];
            var _loc7_ = eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(34)][§§constant(37)] - this[§§constant(14)];
            var _loc12_ = _loc8_ * _loc8_ + _loc7_ * _loc7_;
            var _loc9_ = this[§§constant(38)] * 4;
            var _loc13_;
            if(_loc12_ > _loc9_ * _loc9_)
            {
               _loc13_ = eval(§§constant(39))[§§constant(40)](_loc12_);
               _loc8_ *= _loc9_ / _loc13_;
               _loc7_ *= _loc9_ / _loc13_;
            }
            this[§§constant(41)][§§constant(42)]();
            this[§§constant(41)][§§constant(46)](2,16777215);
            this[§§constant(41)][§§constant(47)](_loc8_,_loc7_);
            this[§§constant(41)][§§constant(48)](0,0);
            var _loc6_ = new eval(§§constant(1))[§§constant(49)][§§constant(50)][§§constant(51)](this[§§constant(41)],8,8);
            var _loc15_ = (eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(34)][§§constant(35)] - this[§§constant(21)]) * -0.25;
            var _loc14_ = (eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(34)][§§constant(37)] - this[§§constant(14)]) * -0.25;
            var _loc10_ = _loc15_ * _loc15_ + _loc14_ * _loc14_;
            var _loc11_;
            if(_loc10_ > this[§§constant(38)] * this[§§constant(38)])
            {
               _loc11_ = eval(§§constant(39))[§§constant(40)](_loc10_);
               _loc15_ *= this[§§constant(38)] / _loc11_;
               _loc14_ *= this[§§constant(38)] / _loc11_;
            }
            var _loc5_ = 0;
            var _loc4_ = 0;
            var _loc3_ = {(§§constant(52)):_loc15_,(§§constant(53)):_loc14_};
            var _loc2_ = 0;
            while(_loc2_ < 15)
            {
               _loc6_[§§constant(46)](2,16777215,100 - 100 * _loc2_ / 15);
               this[§§constant(54)](_loc3_);
               _loc5_ += _loc3_[§§constant(52)];
               _loc4_ += _loc3_[§§constant(53)];
               _loc6_[§§constant(47)](_loc5_,_loc4_);
               _loc2_ = _loc2_ + 1;
            }
            if(!this[§§constant(43)])
            {
               this[§§constant(43)] = new eval(§§constant(1))[§§constant(3)][§§constant(7)][§§constant(8)](this[§§constant(41)],§§constant(55));
               this[§§constant(43)][§§constant(22)]();
            }
            this[§§constant(43)][§§constant(21)] = _loc8_;
            this[§§constant(43)][§§constant(14)] = _loc7_;
            this[§§constant(43)][§§constant(12)]();
         };
         _loc2_[§§constant(54)] = function(obj)
         {
            obj[§§constant(53)] += this[§§constant(56)];
         };
         _loc2_[§§constant(11)] = function()
         {
            var _loc20_;
            var _loc21_;
            var _loc26_;
            var _loc24_;
            if(eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(57)] == this)
            {
               _loc20_ = eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(34)][§§constant(35)] - this[§§constant(58)];
               _loc21_ = eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(34)][§§constant(37)] - this[§§constant(59)] + 100;
               if(eval(§§constant(39))[§§constant(60)](_loc20_) < 5 && eval(§§constant(39))[§§constant(60)](eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(34)][§§constant(37)] - this[§§constant(59)]) < 5)
               {
                  this[§§constant(21)] = this[§§constant(58)];
                  this[§§constant(14)] = this[§§constant(59)];
                  return undefined;
               }
               if(_loc20_ * _loc20_ + _loc21_ * _loc21_ <= 16900)
               {
                  _loc26_ = this[§§constant(21)];
                  _loc24_ = this[§§constant(14)];
                  this[§§constant(33)] = (eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(34)][§§constant(35)] - this[§§constant(21)]) / 2;
                  this[§§constant(36)] = (eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(34)][§§constant(37)] - this[§§constant(14)]) / 2;
                  eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(57)] = null;
                  this[§§constant(11)]();
                  eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(57)] = this;
                  this[§§constant(33)] = this[§§constant(21)] - _loc26_;
                  this[§§constant(36)] = this[§§constant(14)] - _loc24_;
                  return undefined;
               }
               eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(57)] = null;
               this[§§constant(31)]();
            }
            this[§§constant(36)] += this[§§constant(56)];
            if(this[§§constant(33)] == 0 && this[§§constant(36)] == 0)
            {
               return undefined;
            }
            var _loc23_ = this[§§constant(21)];
            var _loc22_ = this[§§constant(14)];
            var _loc27_ = this[§§constant(21)] + this[§§constant(33)];
            var _loc25_ = this[§§constant(14)] + this[§§constant(36)];
            var _loc10_ = this[§§constant(33)] < 0 ? -1 : 1;
            var _loc11_ = this[§§constant(36)] < 0 ? -1 : 1;
            var _loc19_ = _loc23_ - this[§§constant(61)] * _loc10_ >> 5;
            var _loc17_ = _loc27_ + this[§§constant(62)] * _loc10_ >> 5;
            var _loc18_ = _loc22_ - this[§§constant(63)] * _loc11_ >> 5;
            var _loc16_ = _loc25_ + this[§§constant(64)] * _loc11_ >> 5;
            var _loc8_;
            var _loc2_;
            var _loc7_;
            var _loc4_;
            var _loc3_;
            var _loc9_;
            var _loc5_ = false;
            var _loc6_;
            var _loc15_;
            var _loc14_;
            if(this[§§constant(36)] != 0)
            {
               _loc15_ = this[§§constant(21)] - this[§§constant(61)] >> 5;
               _loc14_ = this[§§constant(21)] + this[§§constant(62)] >> 5;
               if(this[§§constant(65)])
               {
                  _loc3_ = _loc18_ + _loc11_;
                  while(_loc3_ != _loc16_ + _loc11_)
                  {
                     _loc4_ = _loc15_;
                     while(_loc4_ <= _loc14_)
                     {
                        if(_loc9_ = eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(66)][§§constant(67)][_loc4_][_loc3_])
                        {
                           _loc5_ = true;
                        }
                        _loc4_ = _loc4_ + 1;
                     }
                     if(_loc5_)
                     {
                        if(this[§§constant(36)] > 0)
                        {
                           _loc6_ = (_loc3_ << 5) - this[§§constant(64)] - 0.1;
                        }
                        else
                        {
                           _loc6_ = (_loc3_ << 5) + this[§§constant(63)] + 32.1;
                        }
                        break;
                     }
                     _loc3_ += _loc11_;
                  }
               }
               if(this[§§constant(68)])
               {
                  _loc8_ = 0;
                  while(_loc8_ < eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(69)][§§constant(70)])
                  {
                     _loc2_ = eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(69)][_loc8_];
                     if(_loc2_ != this)
                     {
                        if(_loc2_[§§constant(21)] - _loc2_[§§constant(61)] <= this[§§constant(21)] + this[§§constant(62)])
                        {
                           if(_loc2_[§§constant(21)] + _loc2_[§§constant(62)] >= this[§§constant(21)] - this[§§constant(61)])
                           {
                              if(this[§§constant(36)] > 0)
                              {
                                 if(_loc2_[§§constant(14)] >= this[§§constant(14)])
                                 {
                                    if(_loc2_[§§constant(14)] - _loc2_[§§constant(63)] <= this[§§constant(14)] + this[§§constant(36)] + this[§§constant(64)])
                                    {
                                       _loc7_ = _loc2_[§§constant(14)] - _loc2_[§§constant(63)] - this[§§constant(64)] - 0.1;
                                       if(_loc6_ > _loc7_ || !_loc5_)
                                       {
                                          _loc6_ = _loc7_;
                                          _loc5_ = true;
                                       }
                                    }
                                 }
                              }
                              else if(_loc2_[§§constant(14)] <= this[§§constant(14)])
                              {
                                 if(_loc2_[§§constant(14)] + _loc2_[§§constant(64)] >= this[§§constant(14)] + this[§§constant(36)] - this[§§constant(63)])
                                 {
                                    _loc7_ = _loc2_[§§constant(14)] + _loc2_[§§constant(64)] + this[§§constant(63)] + 0.1;
                                    if(_loc6_ < _loc7_ || !_loc5_)
                                    {
                                       _loc6_ = _loc7_;
                                       _loc5_ = true;
                                    }
                                 }
                              }
                           }
                        }
                     }
                     _loc8_ = _loc8_ + 1;
                  }
               }
               if(_loc5_)
               {
                  this[§§constant(36)] *= - this[§§constant(71)];
                  this[§§constant(14)] = _loc6_;
                  if(this[§§constant(36)] < 0)
                  {
                     this[§§constant(33)] = eval(§§constant(39))[§§constant(73)](eval(§§constant(39))[§§constant(60)](this[§§constant(33)]) - this[§§constant(72)],0) * (this[§§constant(33)] < 0 ? -1 : 1);
                     this[§§constant(75)](eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(5)][§§constant(74)]);
                  }
                  else
                  {
                     this[§§constant(75)](eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(5)][§§constant(76)]);
                  }
               }
               else
               {
                  this[§§constant(14)] += this[§§constant(36)];
               }
            }
            _loc5_ = false;
            var _loc12_;
            var _loc13_;
            if(this[§§constant(33)] != 0)
            {
               _loc12_ = this[§§constant(14)] - this[§§constant(63)] >> 5;
               _loc13_ = this[§§constant(14)] + this[§§constant(64)] >> 5;
               if(this[§§constant(65)])
               {
                  _loc4_ = _loc19_ + _loc10_;
                  while(_loc4_ != _loc17_ + _loc10_)
                  {
                     _loc3_ = _loc12_;
                     while(_loc3_ <= _loc13_)
                     {
                        if(_loc9_ = eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(66)][§§constant(67)][_loc4_][_loc3_])
                        {
                           _loc5_ = true;
                        }
                        _loc3_ = _loc3_ + 1;
                     }
                     if(_loc5_)
                     {
                        if(this[§§constant(33)] > 0)
                        {
                           _loc6_ = (_loc4_ << 5) - this[§§constant(62)] - 0.1;
                        }
                        else
                        {
                           _loc6_ = (_loc4_ << 5) + this[§§constant(61)] + 32.1;
                        }
                        break;
                     }
                     _loc4_ += _loc10_;
                  }
               }
               if(this[§§constant(68)])
               {
                  _loc8_ = 0;
                  while(_loc8_ < eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(69)][§§constant(70)])
                  {
                     _loc2_ = eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(69)][_loc8_];
                     if(_loc2_ != this)
                     {
                        if(_loc2_[§§constant(14)] - _loc2_[§§constant(63)] <= this[§§constant(14)] + this[§§constant(64)])
                        {
                           if(_loc2_[§§constant(14)] + _loc2_[§§constant(64)] >= this[§§constant(14)] - this[§§constant(63)])
                           {
                              if(this[§§constant(33)] > 0)
                              {
                                 if(_loc2_[§§constant(21)] >= this[§§constant(21)])
                                 {
                                    if(_loc2_[§§constant(21)] - _loc2_[§§constant(61)] <= this[§§constant(21)] + this[§§constant(33)] + this[§§constant(62)])
                                    {
                                       _loc7_ = _loc2_[§§constant(21)] - _loc2_[§§constant(61)] - this[§§constant(62)] - 0.1;
                                       if(_loc6_ > _loc7_ || !_loc5_)
                                       {
                                          _loc6_ = _loc7_;
                                          _loc5_ = true;
                                       }
                                    }
                                 }
                              }
                              else if(_loc2_[§§constant(21)] <= this[§§constant(21)])
                              {
                                 if(_loc2_[§§constant(21)] + _loc2_[§§constant(62)] >= this[§§constant(21)] + this[§§constant(33)] - this[§§constant(61)])
                                 {
                                    _loc7_ = _loc2_[§§constant(21)] + _loc2_[§§constant(62)] + this[§§constant(61)] + 0.1;
                                    if(_loc6_ < _loc7_ || !_loc5_)
                                    {
                                       _loc6_ = _loc7_;
                                       _loc5_ = true;
                                    }
                                 }
                              }
                           }
                        }
                     }
                     _loc8_ = _loc8_ + 1;
                  }
               }
               if(_loc5_)
               {
                  this[§§constant(33)] *= -0.4;
                  this[§§constant(21)] = _loc6_;
                  if(this[§§constant(33)] < 0)
                  {
                     this[§§constant(75)](eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(5)][§§constant(77)]);
                  }
                  else
                  {
                     this[§§constant(75)](eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(5)][§§constant(78)]);
                  }
               }
               else
               {
                  this[§§constant(21)] += this[§§constant(33)];
               }
            }
         };
         _loc2_[§§constant(75)] = function(side)
         {
         };
         _loc2_[§§constant(61)] = 10;
         _loc2_[§§constant(62)] = 10;
         _loc2_[§§constant(63)] = 10;
         _loc2_[§§constant(64)] = 10;
         _loc2_[§§constant(33)] = 0;
         _loc2_[§§constant(36)] = 0;
         _loc2_[§§constant(56)] = 1;
         _loc2_[§§constant(72)] = 0.3;
         _loc2_[§§constant(71)] = 0.2;
         _loc2_[§§constant(79)] = true;
         _loc2_[§§constant(80)] = false;
         _loc2_[§§constant(38)] = 20;
         _loc2_[§§constant(43)] = null;
         _loc2_[§§constant(81)] = false;
         _loc2_[§§constant(82)] = false;
         _loc2_[§§constant(65)] = true;
         _loc2_[§§constant(68)] = false;
         _loc2_[§§constant(17)] = true;
         _loc1_[§§constant(74)] = 0;
         _loc1_[§§constant(77)] = 1;
         _loc1_[§§constant(76)] = 2;
         _loc1_[§§constant(78)] = 3;
         §§constant(83)(eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(5)][§§constant(9)],null,1);
         break;
      }
      if(eval("\x01") == 152)
      {
         set("\x01",eval("\x01") + 11);
         §§push("\x0f");
      }
      else if(eval("\x01") == 315)
      {
         set("\x01",eval("\x01") + 246);
         if(§§pop())
         {
            set("\x01",eval("\x01") + 361);
         }
      }
      else if(eval("\x01") == 922)
      {
         set("\x01",eval("\x01") - 207);
      }
      else if(eval("\x01") == 38)
      {
         set("\x01",eval("\x01") + 677);
      }
      else if(eval("\x01") == 96)
      {
         set("\x01",eval("\x01") + 515);
      }
      else if(eval("\x01") == 996)
      {
         set("\x01",eval("\x01") - 844);
         var §§pop() = §§pop();
      }
      else if(eval("\x01") == 559)
      {
         set("\x01",eval("\x01") - 445);
         if(§§pop())
         {
            set("\x01",eval("\x01") - 18);
         }
      }
      else if(eval("\x01") == 945)
      {
         set("\x01",eval("\x01") - 334);
      }
      else if(eval("\x01") == 163)
      {
         set("\x01",eval("\x01") + 788);
         §§push(eval(§§pop()));
      }
      else if(eval("\x01") == 611)
      {
         set("\x01",eval("\x01") + 385);
         §§push("\x0f");
         §§push(1);
      }
      else if(eval("\x01") == 951)
      {
         set("\x01",eval("\x01") - 262);
         §§push(!§§pop());
      }
      else if(eval("\x01") == 689)
      {
         set("\x01",eval("\x01") - 348);
         if(§§pop())
         {
            set("\x01",eval("\x01") + 108);
         }
      }
      else
      {
         if(eval("\x01") != 341)
         {
            if(eval("\x01") == 449)
            {
               set("\x01",eval("\x01") + 252);
               if(!_global.com)
               {
                  _global.com = new Object();
               }
               §§pop();
               if(!_global.com.nitrome)
               {
                  _global.com.nitrome = new Object();
               }
               §§pop();
               if(!_global.com.nitrome.util)
               {
                  _global.com.nitrome.util = new Object();
               }
               §§pop();
               if(!_global.com.nitrome.util.Trig)
               {
                  _loc2_ = com.nitrome.util.Trig = function()
                  {
                  }.prototype;
                  com.nitrome.util.Trig = function()
                  {
                  }.setup = function()
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
                  };
                  §§push(ASSetPropFlags(com.nitrome.util.Trig.prototype,null,1));
               }
               §§pop();
               break;
            }
            if(eval("\x01") == 701)
            {
               set("\x01",eval("\x01") - 701);
            }
            break;
         }
         set("\x01",eval("\x01") + 108);
      }
   }
}
