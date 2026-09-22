function §\x04\x05§()
{
   set("\x03",519 % 511 * true);
   return eval("\x03");
}
var §\x01§ = 18 + "\x04\x05"();
var _loc7_;
var _loc6_;
var _loc5_;
var _loc8_;
var _loc2_;
var _loc12_;
var _loc13_;
var _loc4_;
var _loc3_;
var _loc9_;
while(true)
{
   if(eval("\x01") == 26)
   {
      set("\x01",eval("\x01") + 338);
      §§push(true);
   }
   else if(eval("\x01") == 184)
   {
      set("\x01",eval("\x01") + 295);
      if(§§pop())
      {
         set("\x01",eval("\x01") + 508);
      }
   }
   else if(eval("\x01") == 458)
   {
      set("\x01",eval("\x01") + 207);
      if(§§pop())
      {
         set("\x01",eval("\x01") - 625);
      }
   }
   else if(eval("\x01") == 232)
   {
      set("\x01",eval("\x01") + 226);
      §§push(true);
   }
   else if(eval("\x01") == 906)
   {
      set("\x01",eval("\x01") - 312);
      if(§§pop())
      {
         set("\x01",eval("\x01") + 80);
      }
   }
   else if(eval("\x01") == 592)
   {
      set("\x01",eval("\x01") - 409);
      §§push(eval(§§pop()));
   }
   else if(eval("\x01") == 603)
   {
      set("\x01",eval("\x01") + 181);
      §§push("\x0f");
      §§push(1);
   }
   else
   {
      if(eval("\x01") == 665)
      {
         set("\x01",eval("\x01") - 625);
         break;
      }
      if(eval("\x01") == 40)
      {
         set("\x01",eval("\x01") + 563);
      }
      else if(eval("\x01") == 183)
      {
         set("\x01",eval("\x01") + 1);
         §§push(!§§pop());
      }
      else if(eval("\x01") == 364)
      {
         set("\x01",eval("\x01") + 609);
         if(§§pop())
         {
            set("\x01",eval("\x01") - 821);
         }
      }
      else if(eval("\x01") == 111)
      {
         set("\x01",eval("\x01") + 294);
         if(§§pop())
         {
            set("\x01",eval("\x01") - 37);
         }
      }
      else if(eval("\x01") == 674)
      {
         set("\x01",eval("\x01") - 442);
      }
      else if(eval("\x01") == 534)
      {
         set("\x01",eval("\x01") - 423);
         §§push(true);
      }
      else
      {
         if(eval("\x01") == 973)
         {
            set("\x01",eval("\x01") - 821);
            break;
         }
         if(eval("\x01") == 737)
         {
            set("\x01",eval("\x01") - 203);
         }
         else if(eval("\x01") == 152)
         {
            set("\x01",eval("\x01") + 382);
         }
         else if(eval("\x01") == 565)
         {
            set("\x01",eval("\x01") + 38);
         }
         else
         {
            if(eval("\x01") == 405)
            {
               set("\x01",eval("\x01") - 37);
               §§pop() extends §§pop() < §§pop();
               §§push(§§pop() gt §§pop() >>> typeof (§§pop() >>> (§§pop() gt (§§pop() > §§pop()))));
               break;
            }
            if(eval("\x01") == 256)
            {
               set("\x01",eval("\x01") - 24);
            }
            else if(eval("\x01") == 368)
            {
               set("\x01",eval("\x01") + 324);
            }
            else if(eval("\x01") == 216)
            {
               set("\x01",eval("\x01") + 476);
            }
            else if(eval("\x01") == 622)
            {
               set("\x01",eval("\x01") - 30);
               §§push("\x0f");
            }
            else if(eval("\x01") == 784)
            {
               set("\x01",eval("\x01") - 162);
               var §§pop() = §§pop();
            }
            else if(eval("\x01") == 479)
            {
               set("\x01",eval("\x01") + 508);
            }
            else
            {
               if(eval("\x01") != 692)
               {
                  if(eval("\x01") == 594)
                  {
                     set("\x01",eval("\x01") + 80);
                     §§push(§§pop()());
                     loop1:
                     while(true)
                     {
                        if(§§pop() >= §§pop() - §§pop()[§§pop()])
                        {
                           if(this.velocityY > 0)
                           {
                              if(_loc2_.y >= this.y)
                              {
                                 if(_loc2_.y - _loc2_.topExtent <= this.y + this.velocityY + this.bottomExtent)
                                 {
                                    _loc7_ = _loc2_.y - _loc2_.topExtent - this.bottomExtent - 0.1;
                                    if(_loc6_ > _loc7_ || !_loc5_)
                                    {
                                       _loc6_ = _loc7_;
                                       _loc5_ = true;
                                    }
                                 }
                              }
                           }
                           else if(_loc2_.y <= this.y)
                           {
                              if(_loc2_.y + _loc2_.bottomExtent >= this.y + this.velocityY - this.topExtent)
                              {
                                 _loc7_ = _loc2_.y + _loc2_.bottomExtent + this.topExtent + 0.1;
                                 if(_loc6_ < _loc7_ || !_loc5_)
                                 {
                                    _loc6_ = _loc7_;
                                    _loc5_ = true;
                                 }
                              }
                           }
                        }
                        while(true)
                        {
                           _loc8_ = _loc8_ + 1;
                           if(_loc8_ >= com.nitrome.throwgame.Controller.boxes.length)
                           {
                              break loop1;
                           }
                           _loc2_ = com.nitrome.throwgame.Controller.boxes[_loc8_];
                           if(_loc2_ != this)
                           {
                              if(_loc2_.x - _loc2_.leftExtent <= this.x + this.rightExtent)
                              {
                                 break;
                              }
                           }
                        }
                        §§push(_loc2_.x + _loc2_.rightExtent);
                        §§push(this.x);
                        §§push(this);
                        §§push("leftExtent");
                     }
                     if(_loc5_)
                     {
                        this.velocityY *= - this.bounce;
                        this.y = _loc6_;
                        if(this.velocityY < 0)
                        {
                           this.velocityX = Math.max(Math.abs(this.velocityX) - this.friction,0) * (this.velocityX < 0 ? -1 : 1);
                           this.contact(com.nitrome.throwgame.Solid.FLOOR);
                        }
                        else
                        {
                           this.contact(com.nitrome.throwgame.Solid.CEILING);
                        }
                     }
                     else
                     {
                        this.y += this.velocityY;
                     }
                     _loc5_ = false;
                     if(this.velocityX != 0)
                     {
                        _loc12_ = this.y - this.topExtent >> 5;
                        _loc13_ = this.y + this.bottomExtent >> 5;
                        if(this.hitsTiles)
                        {
                           _loc4_ = _loc19_ + _loc10_;
                           while(_loc4_ != _loc17_ + _loc10_)
                           {
                              _loc3_ = _loc12_;
                              while(_loc3_ <= _loc13_)
                              {
                                 if(_loc9_ = com.nitrome.throwgame.Controller.tileSystem.tileGrid[_loc4_][_loc3_])
                                 {
                                    _loc5_ = true;
                                 }
                                 _loc3_ = _loc3_ + 1;
                              }
                              if(_loc5_)
                              {
                                 if(this.velocityX > 0)
                                 {
                                    _loc6_ = (_loc4_ << 5) - this.rightExtent - 0.1;
                                 }
                                 else
                                 {
                                    _loc6_ = (_loc4_ << 5) + this.leftExtent + 32.1;
                                 }
                                 break;
                              }
                              _loc4_ += _loc10_;
                           }
                        }
                        if(this.hitsBoxes)
                        {
                           _loc8_ = 0;
                           while(_loc8_ < com.nitrome.throwgame.Controller.boxes.length)
                           {
                              _loc2_ = com.nitrome.throwgame.Controller.boxes[_loc8_];
                              if(_loc2_ != this)
                              {
                                 if(_loc2_.y - _loc2_.topExtent <= this.y + this.bottomExtent)
                                 {
                                    if(_loc2_.y + _loc2_.bottomExtent >= this.y - this.topExtent)
                                    {
                                       if(this.velocityX > 0)
                                       {
                                          if(_loc2_.x >= this.x)
                                          {
                                             if(_loc2_.x - _loc2_.leftExtent <= this.x + this.velocityX + this.rightExtent)
                                             {
                                                _loc7_ = _loc2_.x - _loc2_.leftExtent - this.rightExtent - 0.1;
                                                if(_loc6_ > _loc7_ || !_loc5_)
                                                {
                                                   _loc6_ = _loc7_;
                                                   _loc5_ = true;
                                                }
                                             }
                                          }
                                       }
                                       else if(_loc2_.x <= this.x)
                                       {
                                          if(_loc2_.x + _loc2_.rightExtent >= this.x + this.velocityX - this.leftExtent)
                                          {
                                             _loc7_ = _loc2_.x + _loc2_.rightExtent + this.leftExtent + 0.1;
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
                           this.velocityX *= -0.4;
                           this.x = _loc6_;
                           if(this.velocityX < 0)
                           {
                              this.contact(com.nitrome.throwgame.Solid.WALL_RIGHT);
                           }
                           else
                           {
                              this.contact(com.nitrome.throwgame.Solid.WALL_LEFT);
                           }
                        }
                        else
                        {
                           this.x += this.velocityX;
                        }
                     }
                     addr17a3:
                     §§pop()[§§pop()] = §§pop();
                     _loc2_.contact = function(side)
                     {
                     };
                     _loc2_.leftExtent = 10;
                     _loc2_.rightExtent = 10;
                     _loc2_.topExtent = 10;
                     _loc2_.bottomExtent = 10;
                     _loc2_.velocityX = 0;
                     _loc2_.velocityY = 0;
                     _loc2_.weight = 1;
                     _loc2_.friction = 0.3;
                     _loc2_.bounce = 0.2;
                     _loc2_.draggable = true;
                     _loc2_.twangable = false;
                     _loc2_.twangMaxForce = 20;
                     _loc2_.twangPin = null;
                     _loc2_.isCharacter = false;
                     _loc2_.isWeapon = false;
                     _loc2_.hitsTiles = true;
                     _loc2_.hitsBoxes = false;
                     _loc2_.overWater = true;
                     _loc1_.FLOOR = 0;
                     _loc1_.WALL_RIGHT = 1;
                     _loc1_.CEILING = 2;
                     _loc1_.WALL_LEFT = 3;
                     §§push(ASSetPropFlags(com.nitrome.throwgame.Solid.prototype,null,1));
                  }
                  else
                  {
                     if(eval("\x01") != 987)
                     {
                        if(eval("\x01") == 740)
                        {
                           set("\x01",eval("\x01") - 740);
                        }
                        break;
                     }
                     set("\x01",eval("\x01") - 247);
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
                     if(!_global.com.nitrome.throwgame.Solid)
                     {
                        com.nitrome.throwgame.Solid extends com.nitrome.util.Clip;
                        _loc2_ = com.nitrome.throwgame.Solid = function(parent, linkageName)
                        {
                           super(parent,linkageName);
                           this.useHolder = true;
                        }.prototype;
                        _loc2_.advance = function()
                        {
                           this.advanceMotion();
                           this.update();
                        };
                        _loc2_.splashCheck = function()
                        {
                           var _loc4_ = this.y < com.nitrome.throwgame.Controller.water.y;
                           var _loc3_;
                           if(this.overWater != _loc4_)
                           {
                              _loc3_ = new com.nitrome.throwgame.Debris(com.nitrome.throwgame.Controller.waterLayer,"splash");
                              _loc3_.x = this.x;
                              _loc3_.y = com.nitrome.throwgame.Controller.water.y;
                              _loc3_.show();
                              _loc3_.mc.gotoAndPlay("splash" + com.nitrome.throwgame.Controller.skyColour.toString());
                              com.nitrome.throwgame.Controller.water.hide();
                              com.nitrome.throwgame.Controller.water.show();
                              com.nitrome.throwgame.Controller.water.mc.gotoAndStop("water" + com.nitrome.throwgame.Controller.skyColour.toString());
                              _root.sfx_manager.playSound("splash");
                           }
                           this.overWater = _loc4_;
                        };
                        _loc2_.release = function()
                        {
                        };
                        _loc2_.twang = function()
                        {
                           this.velocityX = (com.nitrome.throwgame.Controller.content._xmouse - this.x) * -0.25;
                           this.velocityY = (com.nitrome.throwgame.Controller.content._ymouse - this.y) * -0.25;
                           var _loc2_ = this.velocityX * this.velocityX + this.velocityY * this.velocityY;
                           var _loc3_;
                           if(_loc2_ > this.twangMaxForce * this.twangMaxForce)
                           {
                              _loc3_ = Math.sqrt(_loc2_);
                              this.velocityX *= this.twangMaxForce / _loc3_;
                              this.velocityY *= this.twangMaxForce / _loc3_;
                           }
                           this.mcHolder.clear();
                           this.twangPin.destroy();
                           this.twangPin = null;
                        };
                        _loc2_.drawTwangLine = function()
                        {
                           var _loc8_ = com.nitrome.throwgame.Controller.content._xmouse - this.x;
                           var _loc7_ = com.nitrome.throwgame.Controller.content._ymouse - this.y;
                           var _loc12_ = _loc8_ * _loc8_ + _loc7_ * _loc7_;
                           var _loc9_ = this.twangMaxForce * 4;
                           var _loc13_;
                           if(_loc12_ > _loc9_ * _loc9_)
                           {
                              _loc13_ = Math.sqrt(_loc12_);
                              _loc8_ *= _loc9_ / _loc13_;
                              _loc7_ *= _loc9_ / _loc13_;
                           }
                           this.mcHolder.clear();
                           this.mcHolder.lineStyle(2,16777215);
                           this.mcHolder.lineTo(_loc8_,_loc7_);
                           this.mcHolder.moveTo(0,0);
                           var _loc6_ = new com.senocular.drawing.DashedLine(this.mcHolder,8,8);
                           var _loc15_ = (com.nitrome.throwgame.Controller.content._xmouse - this.x) * -0.25;
                           var _loc14_ = (com.nitrome.throwgame.Controller.content._ymouse - this.y) * -0.25;
                           var _loc10_ = _loc15_ * _loc15_ + _loc14_ * _loc14_;
                           var _loc11_;
                           if(_loc10_ > this.twangMaxForce * this.twangMaxForce)
                           {
                              _loc11_ = Math.sqrt(_loc10_);
                              _loc15_ *= this.twangMaxForce / _loc11_;
                              _loc14_ *= this.twangMaxForce / _loc11_;
                           }
                           var _loc5_ = 0;
                           var _loc4_ = 0;
                           var _loc3_ = {vx:_loc15_,vy:_loc14_};
                           var _loc2_ = 0;
                           while(_loc2_ < 15)
                           {
                              _loc6_.lineStyle(2,16777215,100 - 100 * _loc2_ / 15);
                              this.twangPrediction(_loc3_);
                              _loc5_ += _loc3_.vx;
                              _loc4_ += _loc3_.vy;
                              _loc6_.lineTo(_loc5_,_loc4_);
                              _loc2_ = _loc2_ + 1;
                           }
                           if(!this.twangPin)
                           {
                              this.twangPin = new com.nitrome.util.Clip(this.mcHolder,"pin");
                              this.twangPin.show();
                           }
                           this.twangPin.x = _loc8_;
                           this.twangPin.y = _loc7_;
                           this.twangPin.update();
                        };
                        _loc2_.twangPrediction = function(obj)
                        {
                           obj.vy += this.weight;
                        };
                        §§goto(addr17a3);
                        §§push(_loc2_);
                        §§push("advanceMotion");
                        function()
                        {
                           var _loc20_;
                           var _loc21_;
                           var _loc26_;
                           var _loc24_;
                           if(com.nitrome.throwgame.Controller.dragging == this)
                           {
                              _loc20_ = com.nitrome.throwgame.Controller.content._xmouse - this.dragStartX;
                              _loc21_ = com.nitrome.throwgame.Controller.content._ymouse - this.dragStartY + 100;
                              if(Math.abs(_loc20_) < 5 && Math.abs(com.nitrome.throwgame.Controller.content._ymouse - this.dragStartY) < 5)
                              {
                                 this.x = this.dragStartX;
                                 this.y = this.dragStartY;
                                 return undefined;
                              }
                              if(_loc20_ * _loc20_ + _loc21_ * _loc21_ <= 16900)
                              {
                                 _loc26_ = this.x;
                                 _loc24_ = this.y;
                                 this.velocityX = (com.nitrome.throwgame.Controller.content._xmouse - this.x) / 2;
                                 this.velocityY = (com.nitrome.throwgame.Controller.content._ymouse - this.y) / 2;
                                 com.nitrome.throwgame.Controller.dragging = null;
                                 this.advanceMotion();
                                 com.nitrome.throwgame.Controller.dragging = this;
                                 this.velocityX = this.x - _loc26_;
                                 this.velocityY = this.y - _loc24_;
                                 return undefined;
                              }
                              com.nitrome.throwgame.Controller.dragging = null;
                              this.release();
                           }
                           this.velocityY += this.weight;
                           if(this.velocityX == 0 && this.velocityY == 0)
                           {
                              return undefined;
                           }
                           var _loc23_ = this.x;
                           var _loc22_ = this.y;
                           var _loc27_ = this.x + this.velocityX;
                           var _loc25_ = this.y + this.velocityY;
                           var _loc10_ = this.velocityX < 0 ? -1 : 1;
                           var _loc11_ = this.velocityY < 0 ? -1 : 1;
                           var _loc19_ = _loc23_ - this.leftExtent * _loc10_ >> 5;
                           var _loc17_ = _loc27_ + this.rightExtent * _loc10_ >> 5;
                           var _loc18_ = _loc22_ - this.topExtent * _loc11_ >> 5;
                           var _loc16_ = _loc25_ + this.bottomExtent * _loc11_ >> 5;
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
                           if(this.velocityY != 0)
                           {
                              _loc15_ = this.x - this.leftExtent >> 5;
                              _loc14_ = this.x + this.rightExtent >> 5;
                              if(this.hitsTiles)
                              {
                                 _loc3_ = _loc18_ + _loc11_;
                                 while(_loc3_ != _loc16_ + _loc11_)
                                 {
                                    _loc4_ = _loc15_;
                                    while(_loc4_ <= _loc14_)
                                    {
                                       if(_loc9_ = com.nitrome.throwgame.Controller.tileSystem.tileGrid[_loc4_][_loc3_])
                                       {
                                          _loc5_ = true;
                                       }
                                       _loc4_ = _loc4_ + 1;
                                    }
                                    if(_loc5_)
                                    {
                                       if(this.velocityY > 0)
                                       {
                                          _loc6_ = (_loc3_ << 5) - this.bottomExtent - 0.1;
                                       }
                                       else
                                       {
                                          _loc6_ = (_loc3_ << 5) + this.topExtent + 32.1;
                                       }
                                       break;
                                    }
                                    _loc3_ += _loc11_;
                                 }
                              }
                              if(this.hitsBoxes)
                              {
                                 _loc8_ = 0;
                                 while(_loc8_ < com.nitrome.throwgame.Controller.boxes.length)
                                 {
                                    _loc2_ = com.nitrome.throwgame.Controller.boxes[_loc8_];
                                    if(_loc2_ != this)
                                    {
                                       if(_loc2_.x - _loc2_.leftExtent <= this.x + this.rightExtent)
                                       {
                                          if(_loc2_.x + _loc2_.rightExtent >= this.x - this.leftExtent)
                                          {
                                             if(this.velocityY > 0)
                                             {
                                                if(_loc2_.y >= this.y)
                                                {
                                                   if(_loc2_.y - _loc2_.topExtent <= this.y + this.velocityY + this.bottomExtent)
                                                   {
                                                      _loc7_ = _loc2_.y - _loc2_.topExtent - this.bottomExtent - 0.1;
                                                      if(_loc6_ > _loc7_ || !_loc5_)
                                                      {
                                                         _loc6_ = _loc7_;
                                                         _loc5_ = true;
                                                      }
                                                   }
                                                }
                                             }
                                             else if(_loc2_.y <= this.y)
                                             {
                                                if(_loc2_.y + _loc2_.bottomExtent >= this.y + this.velocityY - this.topExtent)
                                                {
                                                   _loc7_ = _loc2_.y + _loc2_.bottomExtent + this.topExtent + 0.1;
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
                                 this.velocityY *= - this.bounce;
                                 this.y = _loc6_;
                                 if(this.velocityY < 0)
                                 {
                                    this.velocityX = Math.max(Math.abs(this.velocityX) - this.friction,0) * (this.velocityX < 0 ? -1 : 1);
                                    this.contact(com.nitrome.throwgame.Solid.FLOOR);
                                 }
                                 else
                                 {
                                    this.contact(com.nitrome.throwgame.Solid.CEILING);
                                 }
                              }
                              else
                              {
                                 this.y += this.velocityY;
                              }
                           }
                           _loc5_ = false;
                           var _loc12_;
                           var _loc13_;
                           if(this.velocityX != 0)
                           {
                              _loc12_ = this.y - this.topExtent >> 5;
                              _loc13_ = this.y + this.bottomExtent >> 5;
                              if(this.hitsTiles)
                              {
                                 _loc4_ = _loc19_ + _loc10_;
                                 while(_loc4_ != _loc17_ + _loc10_)
                                 {
                                    _loc3_ = _loc12_;
                                    while(_loc3_ <= _loc13_)
                                    {
                                       if(_loc9_ = com.nitrome.throwgame.Controller.tileSystem.tileGrid[_loc4_][_loc3_])
                                       {
                                          _loc5_ = true;
                                       }
                                       _loc3_ = _loc3_ + 1;
                                    }
                                    if(_loc5_)
                                    {
                                       if(this.velocityX > 0)
                                       {
                                          _loc6_ = (_loc4_ << 5) - this.rightExtent - 0.1;
                                       }
                                       else
                                       {
                                          _loc6_ = (_loc4_ << 5) + this.leftExtent + 32.1;
                                       }
                                       break;
                                    }
                                    _loc4_ += _loc10_;
                                 }
                              }
                              if(this.hitsBoxes)
                              {
                                 _loc8_ = 0;
                                 while(_loc8_ < com.nitrome.throwgame.Controller.boxes.length)
                                 {
                                    _loc2_ = com.nitrome.throwgame.Controller.boxes[_loc8_];
                                    if(_loc2_ != this)
                                    {
                                       if(_loc2_.y - _loc2_.topExtent <= this.y + this.bottomExtent)
                                       {
                                          if(_loc2_.y + _loc2_.bottomExtent >= this.y - this.topExtent)
                                          {
                                             if(this.velocityX > 0)
                                             {
                                                if(_loc2_.x >= this.x)
                                                {
                                                   if(_loc2_.x - _loc2_.leftExtent <= this.x + this.velocityX + this.rightExtent)
                                                   {
                                                      _loc7_ = _loc2_.x - _loc2_.leftExtent - this.rightExtent - 0.1;
                                                      if(_loc6_ > _loc7_ || !_loc5_)
                                                      {
                                                         _loc6_ = _loc7_;
                                                         _loc5_ = true;
                                                      }
                                                   }
                                                }
                                             }
                                             else if(_loc2_.x <= this.x)
                                             {
                                                if(_loc2_.x + _loc2_.rightExtent >= this.x + this.velocityX - this.leftExtent)
                                                {
                                                   _loc7_ = _loc2_.x + _loc2_.rightExtent + this.leftExtent + 0.1;
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
                                 this.velocityX *= -0.4;
                                 this.x = _loc6_;
                                 if(this.velocityX < 0)
                                 {
                                    this.contact(com.nitrome.throwgame.Solid.WALL_RIGHT);
                                 }
                                 else
                                 {
                                    this.contact(com.nitrome.throwgame.Solid.WALL_LEFT);
                                 }
                              }
                              else
                              {
                                 this.x += this.velocityX;
                              }
                           }
                        }
                     }
                  }
                  §§pop();
                  break;
               }
               set("\x01",eval("\x01") + 214);
               §§push(true);
            }
         }
      }
   }
}
