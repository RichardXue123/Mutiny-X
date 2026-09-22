function §\x04\x05§()
{
   set("\x03",612 % 511 * true);
   return eval("\x03");
}
var §\x01§ = 54 + "\x04\x05"();
var _loc2_;
var _loc3_;
while(true)
{
   if(eval("\x01") == 155)
   {
      set("\x01",eval("\x01") + 172);
      §§push(true);
   }
   else if(eval("\x01") == 984)
   {
      set("\x01",eval("\x01") - 182);
      §§push("\x0f");
      §§push(1);
   }
   else if(eval("\x01") == 802)
   {
      set("\x01",eval("\x01") - 698);
      var §§pop() = §§pop();
   }
   else if(eval("\x01") == 495)
   {
      set("\x01",eval("\x01") + 489);
   }
   else if(eval("\x01") == 327)
   {
      set("\x01",eval("\x01") - 104);
      if(§§pop())
      {
         set("\x01",eval("\x01") - 147);
      }
   }
   else
   {
      if(eval("\x01") == 223)
      {
         set("\x01",eval("\x01") - 147);
         loop1:
         while(true)
         {
            _loc1_[§§constant(26)](§§pop() + _loc7_,§§pop());
            while(true)
            {
               _loc2_ = _loc2_ + 1;
               if(_loc2_ >= _loc8_)
               {
                  break loop1;
               }
               if(!_loc1_[§§constant(8)])
               {
                  break;
               }
               _loc1_[§§constant(26)](_loc1_[§§constant(10)][§§constant(11)] + _loc7_,_loc1_[§§constant(10)][§§constant(12)] + _loc6_);
               _loc1_[§§constant(19)](_loc1_[§§constant(10)][§§constant(11)] + _loc5_,_loc1_[§§constant(10)][§§constant(12)] + _loc4_);
            }
            _loc1_[§§constant(19)](_loc1_[§§constant(10)][§§constant(11)] + _loc5_,_loc1_[§§constant(10)][§§constant(12)] + _loc4_);
            §§push(_loc1_[§§constant(10)][§§constant(12)] + _loc6_);
            §§push(_loc1_[§§constant(10)][§§constant(11)]);
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
               _loc1_.Solid = _loc1_.y - _loc3_;
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
         §§constant(44)(eval("${invalid_utf8=228}\x03")[§§constant(3)][§§constant(4)][§§constant(5)][§§constant(13)],null,1);
         break;
      }
      if(eval("\x01") == 76)
      {
         set("\x01",eval("\x01") + 908);
      }
      else if(eval("\x01") == 104)
      {
         set("\x01",eval("\x01") + 450);
         §§push("\x0f");
      }
      else if(eval("\x01") == 471)
      {
         set("\x01",eval("\x01") + 125);
         if(§§pop())
         {
            set("\x01",eval("\x01") - 343);
         }
      }
      else if(eval("\x01") == 554)
      {
         set("\x01",eval("\x01") - 398);
         §§push(eval(§§pop()));
      }
      else if(eval("\x01") == 156)
      {
         set("\x01",eval("\x01") + 315);
         §§push(!§§pop());
      }
      else
      {
         if(eval("\x01") != 596)
         {
            if(eval("\x01") == 253)
            {
               set("\x01",eval("\x01") + 216);
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
               if(!_global.com.nitrome.throwgame)
               {
                  _global.com.nitrome.throwgame = new Object();
               }
               §§pop();
               if(!_global.com.nitrome.throwgame.Weapon)
               {
                  com.nitrome.throwgame.Weapon extends com.nitrome.throwgame.Solid;
                  _loc2_ = com.nitrome.throwgame.Weapon = function(linkageName)
                  {
                     super(com.nitrome.throwgame.Controller.characterLayer,linkageName);
                     this.isWeapon = true;
                  }.prototype;
                  _loc2_.update = function()
                  {
                     super.update();
                     this.trackX = this.x;
                     this.trackY = this.y;
                  };
                  _loc2_.release = function()
                  {
                     var _loc3_ = Math.sqrt(this.velocityX * this.velocityX + this.velocityY * this.velocityY);
                     var _loc2_ = 20;
                     if(_loc3_ > _loc2_)
                     {
                        this.velocityX = _loc2_ * this.velocityX / _loc3_;
                        this.velocityY = _loc2_ * this.velocityY / _loc3_;
                     }
                     this.fire(this.velocityX,this.velocityY);
                     this.finished = false;
                     this.owner.canShoot = false;
                     this.owner.canThrow = false;
                  };
                  _loc2_.twang = function()
                  {
                     super.twang();
                     this.fired = true;
                     this.track = true;
                     this.owner.canShoot = false;
                     this.owner.canThrow = false;
                  };
                  _loc2_.advance = function()
                  {
                     this.advanceMotion();
                     this.update();
                     if(this.fired && !this.finished)
                     {
                        if(this.velocityY > 0 && this.y > com.nitrome.throwgame.Controller.tileSystem.levelHeight << 5)
                        {
                           this.finished = true;
                        }
                        else if(this.velocityX == 0 && Math.abs(this.velocityY) < 0.2)
                        {
                           this.finished = true;
                        }
                        if(!this.simulation)
                        {
                           this.splashCheck();
                        }
                     }
                     if(!this.fired && com.nitrome.throwgame.Controller.twanging == this)
                     {
                        this.drawTwangLine();
                     }
                  };
                  _loc2_.randomThrows = function(count)
                  {
                     var _loc10_ = [];
                     var _loc12_ = this.x;
                     var _loc11_ = this.y;
                     this.simulation = true;
                     var _loc3_ = 0;
                     var _loc4_;
                     var _loc5_;
                     var _loc7_;
                     var _loc6_;
                     var _loc2_;
                     while(_loc3_ < count)
                     {
                        _loc4_ = 180 + int(Math.random() * 180);
                        _loc5_ = 5 + Math.random() * (this.twangMaxForce - 5);
                        _loc7_ = com.nitrome.util.Trig.cosTable[_loc4_] * _loc5_;
                        _loc6_ = com.nitrome.util.Trig.sinTable[_loc4_] * _loc5_;
                        _loc2_ = 0;
                        this.x = _loc12_;
                        this.y = _loc11_;
                        this.velocityX = _loc7_;
                        this.velocityY = _loc6_;
                        this.simulationFinished = false;
                        while(!this.simulationFinished)
                        {
                           this.advanceMotion();
                           if((_loc2_ = _loc2_ + 1) > 100)
                           {
                              break;
                           }
                        }
                        _loc10_.push({vx:_loc7_,vy:_loc6_,ex:this.x,ey:this.y});
                        _loc3_ = _loc3_ + 1;
                     }
                     this.x = _loc12_;
                     this.y = _loc11_;
                     this.velocityX = 0;
                     this.velocityY = 0;
                     this.simulation = false;
                     return _loc10_;
                  };
                  _loc2_.aiPerform = function(details)
                  {
                     this.fire(details.vx,details.vy);
                  };
                  _loc2_.fire = function(vx, vy)
                  {
                     if(!this.fired)
                     {
                        this.velocityX = vx;
                        this.velocityY = vy;
                        this.fired = true;
                        this.track = true;
                     }
                  };
                  _loc2_.place = function(x, y)
                  {
                     if(!this.fired)
                     {
                        this.x = x;
                        this.y = y;
                        this.fired = true;
                        this.track = true;
                     }
                  };
                  _loc2_.owner = null;
                  _loc2_.fired = false;
                  _loc2_.finished = false;
                  _loc2_.simulation = false;
                  _loc2_.simulationFinished = false;
                  _loc2_.placeableWeapon = false;
                  _loc2_.limitedToTurn = true;
                  _loc2_.trackX = 0;
                  _loc2_.trackY = 0;
                  _loc2_.track = false;
                  _loc2_.dragRange = 130;
                  _loc2_.dragOffset = -100;
                  _loc2_.showCircle = true;
                  §§push(ASSetPropFlags(com.nitrome.throwgame.Weapon.prototype,null,1));
               }
               §§pop();
               break;
            }
            if(eval("\x01") == 469)
            {
               set("\x01",eval("\x01") - 469);
            }
            break;
         }
         set("\x01",eval("\x01") - 343);
      }
   }
}
