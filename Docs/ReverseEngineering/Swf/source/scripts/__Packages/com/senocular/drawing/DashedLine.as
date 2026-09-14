function §\x04\x05§()
{
   set("\x03",117 % 511 * true);
   return eval("\x03");
}
var §\x01§ = 697 + "\x04\x05"();
var _loc15_;
var _loc14_;
var _loc10_;
var _loc11_;
var _loc5_;
var _loc4_;
var _loc3_;
var _loc2_;
while(true)
{
   if(eval("\x01") == 814)
   {
      set("\x01",eval("\x01") - 446);
      §§push(true);
   }
   else if(eval("\x01") == 634)
   {
      set("\x01",eval("\x01") - 607);
      §§push("\x0f");
      §§push(1);
   }
   else if(eval("\x01") == 702)
   {
      set("\x01",eval("\x01") - 68);
   }
   else if(eval("\x01") == 1)
   {
      set("\x01",eval("\x01") + 881);
      §§push("\x0f");
   }
   else if(eval("\x01") == 368)
   {
      set("\x01",eval("\x01") + 167);
      if(§§pop())
      {
         set("\x01",eval("\x01") - 190);
      }
   }
   else if(eval("\x01") == 448)
   {
      set("\x01",eval("\x01") + 370);
      §§push(!§§pop());
   }
   else if(eval("\x01") == 241)
   {
      set("\x01",eval("\x01") + 521);
      §§push(true);
   }
   else
   {
      if(eval("\x01") == 535)
      {
         set("\x01",eval("\x01") - 190);
         toggleHighQuality();
         _loc15_ = (eval(§§pop())[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(34)][§§constant(35)] - _loc1_[§§constant(21)]) * -0.25;
         _loc14_ = (eval("{invalid_utf8=227}{invalid_utf8=206}{invalid_utf8=5}*")[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(34)][§§constant(37)] - _loc1_[§§constant(14)]) * -0.25;
         _loc10_ = _loc15_ * _loc15_ + _loc14_ * _loc14_;
         if(_loc10_ > _loc1_[§§constant(38)] * _loc1_[§§constant(38)])
         {
            _loc11_ = eval(§§constant(39))[§§constant(40)](_loc10_);
            _loc15_ *= _loc1_[§§constant(38)] / _loc11_;
            _loc14_ *= _loc1_[§§constant(38)] / _loc11_;
         }
         _loc5_ = 0;
         _loc4_ = 0;
         _loc3_ = {(§§constant(52)):_loc15_,(§§constant(53)):_loc14_};
         _loc2_ = 0;
         while(_loc2_ < 15)
         {
            _loc6_[§§constant(46)](2,16777215,100 - 100 * _loc2_ / 15);
            _loc1_[§§constant(54)](_loc3_);
            _loc5_ += _loc3_[§§constant(52)];
            _loc4_ += _loc3_[§§constant(53)];
            _loc6_[§§constant(47)](_loc5_,_loc4_);
            _loc2_ = _loc2_ + 1;
         }
         if(!_loc1_[§§constant(43)])
         {
            _loc1_[§§constant(43)] = new eval("{invalid_utf8=227}{invalid_utf8=206}{invalid_utf8=5}*")[§§constant(3)][§§constant(7)][§§constant(8)](_loc1_[§§constant(41)],§§constant(55));
            _loc1_[§§constant(43)][§§constant(22)]();
         }
         _loc1_[§§constant(43)][§§constant(21)] = _loc8_;
         _loc1_[§§constant(43)][§§constant(14)] = _loc7_;
         _loc1_[§§constant(43)][§§constant(12)]();
         §§pop()[§§pop()] = §§pop();
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
            if(eval("{invalid_utf8=227}{invalid_utf8=206}{invalid_utf8=5}*")[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(57)] == this)
            {
               _loc20_ = eval("{invalid_utf8=227}{invalid_utf8=206}{invalid_utf8=5}*")[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(34)][§§constant(35)] - this[§§constant(58)];
               _loc21_ = eval("{invalid_utf8=227}{invalid_utf8=206}{invalid_utf8=5}*")[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(34)][§§constant(37)] - this[§§constant(59)] + 100;
               if(eval(§§constant(39))[§§constant(60)](_loc20_) < 5 && eval(§§constant(39))[§§constant(60)](eval("{invalid_utf8=227}{invalid_utf8=206}{invalid_utf8=5}*")[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(34)][§§constant(37)] - this[§§constant(59)]) < 5)
               {
                  this[§§constant(21)] = this[§§constant(58)];
                  this[§§constant(14)] = this[§§constant(59)];
                  return undefined;
               }
               if(_loc20_ * _loc20_ + _loc21_ * _loc21_ <= 16900)
               {
                  _loc26_ = this[§§constant(21)];
                  _loc24_ = this[§§constant(14)];
                  this[§§constant(33)] = (eval("{invalid_utf8=227}{invalid_utf8=206}{invalid_utf8=5}*")[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(34)][§§constant(35)] - this[§§constant(21)]) / 2;
                  this[§§constant(36)] = (eval("{invalid_utf8=227}{invalid_utf8=206}{invalid_utf8=5}*")[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(34)][§§constant(37)] - this[§§constant(14)]) / 2;
                  eval("{invalid_utf8=227}{invalid_utf8=206}{invalid_utf8=5}*")[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(57)] = null;
                  this[§§constant(11)]();
                  eval("{invalid_utf8=227}{invalid_utf8=206}{invalid_utf8=5}*")[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(57)] = this;
                  this[§§constant(33)] = this[§§constant(21)] - _loc26_;
                  this[§§constant(36)] = this[§§constant(14)] - _loc24_;
                  return undefined;
               }
               eval("{invalid_utf8=227}{invalid_utf8=206}{invalid_utf8=5}*")[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(57)] = null;
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
                        if(_loc9_ = eval("{invalid_utf8=227}{invalid_utf8=206}{invalid_utf8=5}*")[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(66)][§§constant(67)][_loc4_][_loc3_])
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
                  while(_loc8_ < eval("{invalid_utf8=227}{invalid_utf8=206}{invalid_utf8=5}*")[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(69)][§§constant(70)])
                  {
                     _loc2_ = eval("{invalid_utf8=227}{invalid_utf8=206}{invalid_utf8=5}*")[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(69)][_loc8_];
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
                     this[§§constant(75)](eval("{invalid_utf8=227}{invalid_utf8=206}{invalid_utf8=5}*")[§§constant(3)][§§constant(4)][§§constant(5)][§§constant(74)]);
                  }
                  else
                  {
                     this[§§constant(75)](eval("{invalid_utf8=227}{invalid_utf8=206}{invalid_utf8=5}*")[§§constant(3)][§§constant(4)][§§constant(5)][§§constant(76)]);
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
                        if(_loc9_ = eval("{invalid_utf8=227}{invalid_utf8=206}{invalid_utf8=5}*")[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(66)][§§constant(67)][_loc4_][_loc3_])
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
                  while(_loc8_ < eval("{invalid_utf8=227}{invalid_utf8=206}{invalid_utf8=5}*")[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(69)][§§constant(70)])
                  {
                     _loc2_ = eval("{invalid_utf8=227}{invalid_utf8=206}{invalid_utf8=5}*")[§§constant(3)][§§constant(4)][§§constant(15)][§§constant(69)][_loc8_];
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
                     this[§§constant(75)](eval("{invalid_utf8=227}{invalid_utf8=206}{invalid_utf8=5}*")[§§constant(3)][§§constant(4)][§§constant(5)][§§constant(77)]);
                  }
                  else
                  {
                     this[§§constant(75)](eval("{invalid_utf8=227}{invalid_utf8=206}{invalid_utf8=5}*")[§§constant(3)][§§constant(4)][§§constant(5)][§§constant(78)]);
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
         §§constant(83)(eval("{invalid_utf8=227}{invalid_utf8=206}{invalid_utf8=5}*")[§§constant(3)][§§constant(4)][§§constant(5)][§§constant(9)],null,1);
         break;
      }
      if(eval("\x01") == 680)
      {
         set("\x01",eval("\x01") - 96);
         break;
      }
      if(eval("\x01") == 584)
      {
         set("\x01",eval("\x01") + 50);
      }
      else if(eval("\x01") == 762)
      {
         set("\x01",eval("\x01") - 82);
         if(§§pop())
         {
            set("\x01",eval("\x01") - 96);
         }
      }
      else if(eval("\x01") == 505)
      {
         set("\x01",eval("\x01") - 264);
      }
      else if(eval("\x01") == 345)
      {
         set("\x01",eval("\x01") - 104);
      }
      else if(eval("\x01") == 27)
      {
         set("\x01",eval("\x01") - 26);
         var §§pop() = §§pop();
      }
      else if(eval("\x01") == 882)
      {
         set("\x01",eval("\x01") - 434);
         §§push(eval(§§pop()));
      }
      else if(eval("\x01") == 818)
      {
         set("\x01",eval("\x01") - 440);
         if(§§pop())
         {
            set("\x01",eval("\x01") + 567);
         }
      }
      else
      {
         if(eval("\x01") != 378)
         {
            if(eval("\x01") == 945)
            {
               set("\x01",eval("\x01") - 911);
               if(!_global.com)
               {
                  _global.com = new Object();
               }
               §§pop();
               if(!_global.com.senocular)
               {
                  _global.com.senocular = new Object();
               }
               §§pop();
               if(!_global.com.senocular.drawing)
               {
                  _global.com.senocular.drawing = new Object();
               }
               §§pop();
               if(!_global.com.senocular.drawing.DashedLine)
               {
                  _loc2_ = com.senocular.drawing.DashedLine = function(target, onLength, offLength)
                  {
                     this.target = target;
                     this.setDash(onLength,offLength);
                     this.isLine = true;
                     this.overflow = 0;
                     this.pen = {x:0,y:0};
                  }.prototype;
                  _loc2_.setDash = function(onLength, offLength)
                  {
                     this.onLength = onLength;
                     this.offLength = offLength;
                     this.dashLength = this.onLength + this.offLength;
                  };
                  _loc2_.getDash = function(Void)
                  {
                     return [this.onLength,this.offLength];
                  };
                  _loc2_.moveTo = function(x, y)
                  {
                     this.targetMoveTo(x,y);
                  };
                  _loc2_.lineTo = function(x, y)
                  {
                     var _loc15_ = x - this.pen.x;
                     var _loc13_ = y - this.pen.y;
                     var _loc14_ = Math.atan2(_loc13_,_loc15_);
                     var _loc11_ = Math.cos(_loc14_);
                     var _loc9_ = Math.sin(_loc14_);
                     var _loc3_ = this.lineLength(_loc15_,_loc13_);
                     if(this.overflow)
                     {
                        if(this.overflow > _loc3_)
                        {
                           if(this.isLine)
                           {
                              this.targetLineTo(x,y);
                           }
                           else
                           {
                              this.targetMoveTo(x,y);
                           }
                           this.overflow -= _loc3_;
                           return undefined;
                        }
                        if(this.isLine)
                        {
                           this.targetLineTo(this.pen.x + _loc11_ * this.overflow,this.pen.y + _loc9_ * this.overflow);
                        }
                        else
                        {
                           this.targetMoveTo(this.pen.x + _loc11_ * this.overflow,this.pen.y + _loc9_ * this.overflow);
                        }
                        _loc3_ -= this.overflow;
                        this.overflow = 0;
                        this.isLine = !this.isLine;
                        if(!_loc3_)
                        {
                           return undefined;
                        }
                     }
                     var _loc8_ = Math.floor(_loc3_ / this.dashLength);
                     var _loc7_;
                     var _loc6_;
                     var _loc5_;
                     var _loc4_;
                     var _loc2_;
                     if(_loc8_)
                     {
                        _loc7_ = _loc11_ * this.onLength;
                        _loc6_ = _loc9_ * this.onLength;
                        _loc5_ = _loc11_ * this.offLength;
                        _loc4_ = _loc9_ * this.offLength;
                        _loc2_ = 0;
                        while(_loc2_ < _loc8_)
                        {
                           if(this.isLine)
                           {
                              this.targetLineTo(this.pen.x + _loc7_,this.pen.y + _loc6_);
                              this.targetMoveTo(this.pen.x + _loc5_,this.pen.y + _loc4_);
                           }
                           else
                           {
                              this.targetMoveTo(this.pen.x + _loc5_,this.pen.y + _loc4_);
                              this.targetLineTo(this.pen.x + _loc7_,this.pen.y + _loc6_);
                           }
                           _loc2_ = _loc2_ + 1;
                        }
                        _loc3_ -= this.dashLength * _loc8_;
                     }
                     if(this.isLine)
                     {
                        if(_loc3_ > this.onLength)
                        {
                           this.targetLineTo(this.pen.x + _loc11_ * this.onLength,this.pen.y + _loc9_ * this.onLength);
                           this.targetMoveTo(x,y);
                           this.overflow = this.offLength - (_loc3_ - this.onLength);
                           this.isLine = false;
                        }
                        else
                        {
                           this.targetLineTo(x,y);
                           if(_loc3_ == this.onLength)
                           {
                              this.overflow = 0;
                              this.isLine = !this.isLine;
                           }
                           else
                           {
                              this.overflow = this.onLength - _loc3_;
                              this.targetMoveTo(x,y);
                           }
                        }
                     }
                     else if(_loc3_ > this.offLength)
                     {
                        this.targetMoveTo(this.pen.x + _loc11_ * this.offLength,this.pen.y + _loc9_ * this.offLength);
                        this.targetLineTo(x,y);
                        this.overflow = this.onLength - (_loc3_ - this.offLength);
                        this.isLine = true;
                     }
                     else
                     {
                        this.targetMoveTo(x,y);
                        if(_loc3_ == this.offLength)
                        {
                           this.overflow = 0;
                           this.isLine = !this.isLine;
                        }
                        else
                        {
                           this.overflow = this.offLength - _loc3_;
                        }
                     }
                  };
                  _loc2_.curveTo = function(cx, cy, x, y)
                  {
                     var _loc8_ = this.pen.x;
                     var _loc7_ = this.pen.y;
                     var _loc14_ = this.curveLength(_loc8_,_loc7_,cx,cy,x,y);
                     var _loc3_ = 0;
                     var _loc4_ = 0;
                     var _loc2_;
                     if(this.overflow)
                     {
                        if(this.overflow > _loc14_)
                        {
                           if(this.isLine)
                           {
                              this.targetCurveTo(cx,cy,x,y);
                           }
                           else
                           {
                              this.targetMoveTo(x,y);
                           }
                           this.overflow -= _loc14_;
                           return undefined;
                        }
                        _loc3_ = this.overflow / _loc14_;
                        _loc2_ = this.curveSliceUpTo(_loc8_,_loc7_,cx,cy,x,y,_loc3_);
                        if(this.isLine)
                        {
                           this.targetCurveTo(_loc2_[2],_loc2_[3],_loc2_[4],_loc2_[5]);
                        }
                        else
                        {
                           this.targetMoveTo(_loc2_[4],_loc2_[5]);
                        }
                        this.overflow = 0;
                        this.isLine = !this.isLine;
                        if(!_loc14_)
                        {
                           return undefined;
                        }
                     }
                     var _loc15_ = _loc14_ - _loc14_ * _loc3_;
                     var _loc16_ = Math.floor(_loc15_ / this.dashLength);
                     var _loc12_ = this.onLength / _loc14_;
                     var _loc13_ = this.offLength / _loc14_;
                     var _loc11_;
                     if(_loc16_)
                     {
                        _loc11_ = 0;
                        while(_loc11_ < _loc16_)
                        {
                           if(this.isLine)
                           {
                              _loc4_ = _loc3_ + _loc12_;
                              _loc2_ = this.curveSlice(_loc8_,_loc7_,cx,cy,x,y,_loc3_,_loc4_);
                              this.targetCurveTo(_loc2_[2],_loc2_[3],_loc2_[4],_loc2_[5]);
                              _loc3_ = _loc4_;
                              _loc4_ = _loc3_ + _loc13_;
                              _loc2_ = this.curveSlice(_loc8_,_loc7_,cx,cy,x,y,_loc3_,_loc4_);
                              this.targetMoveTo(_loc2_[4],_loc2_[5]);
                           }
                           else
                           {
                              _loc4_ = _loc3_ + _loc13_;
                              _loc2_ = this.curveSlice(_loc8_,_loc7_,cx,cy,x,y,_loc3_,_loc4_);
                              this.targetMoveTo(_loc2_[4],_loc2_[5]);
                              _loc3_ = _loc4_;
                              _loc4_ = _loc3_ + _loc12_;
                              _loc2_ = this.curveSlice(_loc8_,_loc7_,cx,cy,x,y,_loc3_,_loc4_);
                              this.targetCurveTo(_loc2_[2],_loc2_[3],_loc2_[4],_loc2_[5]);
                           }
                           _loc3_ = _loc4_;
                           _loc11_ = _loc11_ + 1;
                        }
                     }
                     _loc15_ = _loc14_ - _loc14_ * _loc3_;
                     if(this.isLine)
                     {
                        if(_loc15_ > this.onLength)
                        {
                           _loc4_ = _loc3_ + _loc12_;
                           _loc2_ = this.curveSlice(_loc8_,_loc7_,cx,cy,x,y,_loc3_,_loc4_);
                           this.targetCurveTo(_loc2_[2],_loc2_[3],_loc2_[4],_loc2_[5]);
                           this.targetMoveTo(x,y);
                           this.overflow = this.offLength - (_loc15_ - this.onLength);
                           this.isLine = false;
                        }
                        else
                        {
                           _loc2_ = this.curveSliceFrom(_loc8_,_loc7_,cx,cy,x,y,_loc3_);
                           this.targetCurveTo(_loc2_[2],_loc2_[3],_loc2_[4],_loc2_[5]);
                           if(_loc14_ == this.onLength)
                           {
                              this.overflow = 0;
                              this.isLine = !this.isLine;
                           }
                           else
                           {
                              this.overflow = this.onLength - _loc15_;
                              this.targetMoveTo(x,y);
                           }
                        }
                     }
                     else if(_loc15_ > this.offLength)
                     {
                        _loc4_ = _loc3_ + _loc13_;
                        _loc2_ = this.curveSlice(_loc8_,_loc7_,cx,cy,x,y,_loc3_,_loc4_);
                        this.targetMoveTo(_loc2_[4],_loc2_[5]);
                        _loc2_ = this.curveSliceFrom(_loc8_,_loc7_,cx,cy,x,y,_loc4_);
                        this.targetCurveTo(_loc2_[2],_loc2_[3],_loc2_[4],_loc2_[5]);
                        this.overflow = this.onLength - (_loc15_ - this.offLength);
                        this.isLine = true;
                     }
                     else
                     {
                        this.targetMoveTo(x,y);
                        if(_loc15_ == this.offLength)
                        {
                           this.overflow = 0;
                           this.isLine = !this.isLine;
                        }
                        else
                        {
                           this.overflow = this.offLength - _loc15_;
                        }
                     }
                  };
                  _loc2_.clear = function(Void)
                  {
                     this.target.clear();
                  };
                  _loc2_.lineStyle = function(thickness, rgb, alpha)
                  {
                     this.target.lineStyle(thickness,rgb,alpha);
                  };
                  _loc2_.beginFill = function(rgb, alpha)
                  {
                     this.target.beginFill(rgb,alpha);
                  };
                  _loc2_.beginGradientFill = function(fillType, colors, alphas, ratios, matrix)
                  {
                     this.target.beginGradientFill(fillType,colors,alphas,ratios,matrix);
                  };
                  _loc2_.endFill = function(Void)
                  {
                     this.target.endFill();
                  };
                  _loc2_.lineLength = function(sx, sy, ex, ey)
                  {
                     if(arguments.length == 2)
                     {
                        return Math.sqrt(sx * sx + sy * sy);
                     }
                     var _loc3_ = ex - sx;
                     var _loc2_ = ey - sy;
                     return Math.sqrt(_loc3_ * _loc3_ + _loc2_ * _loc2_);
                  };
                  _loc2_.curveLength = function(sx, sy, cx, cy, ex, ey, accuracy)
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
                     var _loc12_ = !accuracy ? this._curveaccuracy : accuracy;
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
                        _loc13_ += this.lineLength(_loc11_,_loc10_,_loc9_,_loc8_);
                        _loc11_ = _loc9_;
                        _loc10_ = _loc8_;
                        _loc3_ = _loc3_ + 1;
                     }
                     return _loc13_;
                  };
                  _loc2_.curveSlice = function(sx, sy, cx, cy, ex, ey, t1, t2)
                  {
                     if(t1 == 0)
                     {
                        return this.curveSliceUpTo(sx,sy,cx,cy,ex,ey,t2);
                     }
                     if(t2 == 1)
                     {
                        return this.curveSliceFrom(sx,sy,cx,cy,ex,ey,t1);
                     }
                     var _loc2_ = this.curveSliceUpTo(sx,sy,cx,cy,ex,ey,t2);
                     _loc2_.push(t1 / t2);
                     return this.curveSliceFrom.apply(this,_loc2_);
                  };
                  _loc2_.curveSliceUpTo = function(sx, sy, cx, cy, ex, ey, t)
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
                  _loc2_.curveSliceFrom = function(sx, sy, cx, cy, ex, ey, t)
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
                  _loc2_.targetMoveTo = function(x, y)
                  {
                     this.pen = {x:x,y:y};
                     this.target.moveTo(x,y);
                  };
                  _loc2_.targetLineTo = function(x, y)
                  {
                     if(x == this.pen.x && y == this.pen.y)
                     {
                        return undefined;
                     }
                     this.pen = {x:x,y:y};
                     this.target.lineTo(x,y);
                  };
                  _loc2_.targetCurveTo = function(cx, cy, x, y)
                  {
                     if(cx == x && cy == y && x == this.pen.x && y == this.pen.y)
                     {
                        return undefined;
                     }
                     this.pen = {x:x,y:y};
                     this.target.curveTo(cx,cy,x,y);
                  };
                  _loc2_._curveaccuracy = 6;
                  _loc2_.isLine = true;
                  _loc2_.overflow = 0;
                  _loc2_.offLength = 0;
                  _loc2_.onLength = 0;
                  _loc2_.dashLength = 0;
                  §§push(ASSetPropFlags(com.senocular.drawing.DashedLine.prototype,null,1));
               }
               §§pop();
               break;
            }
            if(eval("\x01") == 34)
            {
               set("\x01",eval("\x01") - 34);
            }
            break;
         }
         set("\x01",eval("\x01") + 567);
      }
   }
}
