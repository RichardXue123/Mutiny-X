function §\x04\x05§()
{
   set("\x03",1554 % 511 * true);
   return eval("\x03");
}
var §\x01§ = 28 + "\x04\x05"();
var _loc2_;
while(true)
{
   if(eval("\x01") == 49)
   {
      set("\x01",eval("\x01") + 359);
      §§push(true);
   }
   else if(eval("\x01") == 76)
   {
      set("\x01",eval("\x01") + 413);
      §§push(!§§pop());
   }
   else if(eval("\x01") == 260)
   {
      set("\x01",eval("\x01") - 116);
   }
   else if(eval("\x01") == 506)
   {
      set("\x01",eval("\x01") - 313);
      if(§§pop())
      {
         set("\x01",eval("\x01") - 80);
      }
   }
   else if(eval("\x01") == 385)
   {
      set("\x01",eval("\x01") - 241);
   }
   else if(eval("\x01") == 144)
   {
      set("\x01",eval("\x01") + 362);
      §§push(true);
   }
   else if(eval("\x01") == 408)
   {
      set("\x01",eval("\x01") - 146);
      if(§§pop())
      {
         set("\x01",eval("\x01") + 123);
      }
   }
   else if(eval("\x01") == 123)
   {
      set("\x01",eval("\x01") + 347);
   }
   else
   {
      if(eval("\x01") == 193)
      {
         set("\x01",eval("\x01") - 80);
         loop1:
         while(true)
         {
            if(eval("\x01") == 616)
            {
               set("\x01",eval("\x01") - 303);
               §§push(true);
            }
            else if(eval("\x01") == 598)
            {
               set("\x01",eval("\x01") - 590);
            }
            else if(eval("\x01") == 66)
            {
               set("\x01",eval("\x01") + 550);
            }
            else if(eval("\x01") == 399)
            {
               set("\x01",eval("\x01") - 182);
            }
            else if(eval("\x01") == 704)
            {
               set("\x01",eval("\x01") - 677);
               if(§§pop())
               {
                  set("\x01",eval("\x01") + 943);
               }
            }
            else
            {
               if(eval("\x01") == 125)
               {
                  set("\x01",eval("\x01") + 191);
                  return;
               }
               if(eval("\x01") == 664)
               {
                  set("\x01",eval("\x01") - 496);
                  var §§pop() = §§pop();
               }
               else if(eval("\x01") == 231)
               {
                  set("\x01",eval("\x01") + 21);
                  if(§§pop())
                  {
                     set("\x01",eval("\x01") + 147);
                  }
               }
               else if(eval("\x01") == 659)
               {
                  set("\x01",eval("\x01") - 43);
               }
               else if(eval("\x01") == 217)
               {
                  set("\x01",eval("\x01") + 447);
                  §§push("\x0f");
                  §§push(1);
               }
               else if(eval("\x01") == 165)
               {
                  set("\x01",eval("\x01") + 832);
                  if(§§pop())
                  {
                     set("\x01",eval("\x01") - 45);
                  }
               }
               else if(eval("\x01") == 168)
               {
                  set("\x01",eval("\x01") - 86);
                  §§push("\x0f");
               }
               else if(eval("\x01") == 149)
               {
                  set("\x01",eval("\x01") + 68);
               }
               else if(eval("\x01") == 8)
               {
                  set("\x01",eval("\x01") + 223);
                  §§push(true);
               }
               else
               {
                  if(eval("\x01") == 970)
                  {
                     set("\x01",eval("\x01") - 27);
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
                     if(!_global.com.nitrome.throwgame.TidalWave)
                     {
                        com.nitrome.throwgame.TidalWave extends com.nitrome.teams.length;
                        _loc2_ = com.nitrome.throwgame.TidalWave = function(x, y, toRight)
                        {
                           super(com.nitrome.throwgame.placeableWeapon.hitsTiles,"tidalWave");
                           this.weight = x;
                           this.showCircle = y;
                           this.draggable = toRight;
                           this.hitPlayers();
                           var _loc9_ = 0;
                           var _loc7_;
                           var _loc4_;
                           var _loc3_;
                           var _loc6_;
                           var _loc5_;
                           var _loc8_;
                           while(_loc9_ < com.nitrome.throwgame.placeableWeapon.Weapon.prototype)
                           {
                              _loc7_ = com.nitrome.throwgame.placeableWeapon.Weapon[_loc9_];
                              _loc4_ = 0;
                              while(_loc4_ < _loc7_.advance.prototype)
                              {
                                 _loc3_ = _loc7_.advance[_loc4_];
                                 if(_loc3_.fired)
                                 {
                                    _loc6_ = _loc3_.weight - x;
                                    _loc5_ = _loc3_.showCircle + _loc3_.x - y;
                                    _loc8_ = _loc6_ * _loc6_ + _loc5_ * _loc5_;
                                    if(_loc8_ < 64)
                                    {
                                       _loc3_.Controller = (tileSystem.levelWidth() - 0.5) * 8;
                                       _loc3_.finished = - (tileSystem.levelWidth() * 2 + 6);
                                       _loc3_.hide(30);
                                    }
                                 }
                                 _loc4_ = _loc4_ + 1;
                              }
                              _loc9_ = _loc9_ + 1;
                           }
                        }.characters;
                        _loc2_.alive = function()
                        {
                           var _loc4_ = this.weight + 8 * (!this.draggable ? -1 : 1);
                           var _loc3_ = _loc4_ >> 5;
                           var _loc2_ = this.showCircle >> 5;
                           if(!com.nitrome.throwgame.placeableWeapon.y.water[_loc3_][_loc2_])
                           {
                              return undefined;
                           }
                           if(com.nitrome.throwgame.placeableWeapon.y.water[_loc3_][_loc2_ - 1])
                           {
                              return undefined;
                           }
                           new com.nitrome.throwgame.TidalWave(_loc4_,this.showCircle,this.draggable);
                           com.nitrome.throwgame.placeableWeapon.subtractHealth = 0;
                        };
                        §§push(mouseButtonDown(com.nitrome.throwgame.TidalWave.characters,null,1));
                     }
                     §§pop();
                     return;
                  }
                  if(eval("\x01") == 186)
                  {
                     set("\x01",eval("\x01") - 61);
                     if(§§pop())
                     {
                        set("\x01",eval("\x01") + 191);
                     }
                  }
                  else if(eval("\x01") == 624)
                  {
                     set("\x01",eval("\x01") - 459);
                     §§push(true);
                  }
                  else
                  {
                     if(eval("\x01") == 943)
                     {
                        set("\x01",eval("\x01") - 943);
                        return;
                     }
                     if(eval("\x01") == 902)
                     {
                        set("\x01",eval("\x01") - 716);
                        §§push(true);
                     }
                     else if(eval("\x01") == 780)
                     {
                        set("\x01",eval("\x01") + 122);
                     }
                     else
                     {
                        if(eval("\x01") == 997)
                        {
                           set("\x01",eval("\x01") - 45);
                           return;
                        }
                        if(eval("\x01") == 952)
                        {
                           set("\x01",eval("\x01") - 50);
                        }
                        else if(eval("\x01") == 313)
                        {
                           set("\x01",eval("\x01") + 373);
                           if(§§pop())
                           {
                              set("\x01",eval("\x01") - 686);
                           }
                        }
                        else if(eval("\x01") == 0)
                        {
                           set("\x01",eval("\x01") + 624);
                        }
                        else if(eval("\x01") == 81)
                        {
                           set("\x01",eval("\x01") + 543);
                        }
                        else
                        {
                           if(eval("\x01") == 686)
                           {
                              set("\x01",eval("\x01") - 686);
                              while(true)
                              {
                                 if(eval("\x01") == 582)
                                 {
                                    set("\x01",eval("\x01") + 317);
                                 }
                                 else if(eval("\x01") == 807)
                                 {
                                    set("\x01",eval("\x01") - 709);
                                    §§push(!§§pop());
                                 }
                                 else
                                 {
                                    if(eval("\x01") == 846)
                                    {
                                       set("\x01",eval("\x01") - 450);
                                       §§pop()[§§pop()]["{invalid_utf8=153}{invalid_utf8=250}{invalid_utf8=165}"] = new §5\fw§();
                                       §§pop();
                                       if(!eval("{invalid_utf8=249}{invalid_utf8=229}")["{invalid_utf8=241}{invalid_utf8=136}{invalid_utf8=205}{invalid_utf8=221}"]["{invalid_utf8=153}{invalid_utf8=250}{invalid_utf8=165}"][§§constant(4)])
                                       {
                                          eval("{invalid_utf8=249}{invalid_utf8=229}")["{invalid_utf8=241}{invalid_utf8=136}{invalid_utf8=205}{invalid_utf8=221}"]["{invalid_utf8=153}{invalid_utf8=250}{invalid_utf8=165}"][§§constant(4)] = new §5\fw§();
                                       }
                                       §§pop();
                                       if(!eval("{invalid_utf8=249}{invalid_utf8=229}")["{invalid_utf8=241}{invalid_utf8=136}{invalid_utf8=205}{invalid_utf8=221}"]["{invalid_utf8=153}{invalid_utf8=250}{invalid_utf8=165}"][§§constant(4)][§§constant(5)])
                                       {
                                          eval("{invalid_utf8=241}{invalid_utf8=136}{invalid_utf8=205}{invalid_utf8=221}")["{invalid_utf8=153}{invalid_utf8=250}{invalid_utf8=165}"][§§constant(4)][§§constant(5)] extends eval("{invalid_utf8=241}{invalid_utf8=136}{invalid_utf8=205}{invalid_utf8=221}")["{invalid_utf8=153}{invalid_utf8=250}{invalid_utf8=165}"][§§constant(4)][§§constant(20)];
                                          _loc2_ = eval("{invalid_utf8=241}{invalid_utf8=136}{invalid_utf8=205}{invalid_utf8=221}")["{invalid_utf8=153}{invalid_utf8=250}{invalid_utf8=165}"][§§constant(4)][§§constant(5)] = function()
                                          {
                                             super(§§constant(6));
                                             this[§§constant(7)] = this[§§constant(8)] = this[§§constant(9)] = this[§§constant(10)] = 31;
                                             this[§§constant(11)]();
                                             this[§§constant(12)] = 0.25;
                                             this[§§constant(13)] = 1.5;
                                             this[§§constant(14)] = true;
                                             this[§§constant(15)][§§constant(16)] = §§constant(17);
                                             this[§§constant(18)] = false;
                                             this[§§constant(19)] = true;
                                          }[§§constant(21)];
                                          _loc2_[§§constant(22)] = function()
                                          {
                                             super[§§constant(22)]();
                                             this[§§constant(23)] *= 0.5;
                                             this[§§constant(24)] *= 0.5;
                                          };
                                          _loc2_[§§constant(25)] = function()
                                          {
                                             if(this[§§constant(26)])
                                             {
                                                this[§§constant(15)][§§constant(27)][§§constant(28)] += this[§§constant(23)] * 2.5;
                                             }
                                             super[§§constant(25)]();
                                             var _loc6_;
                                             var _loc5_;
                                             var _loc4_;
                                             var _loc3_;
                                             if(this[§§constant(26)])
                                             {
                                                if(this[§§constant(23)] == 0 && eval(§§constant(29))[§§constant(30)](this[§§constant(24)]) < 0.5)
                                                {
                                                   if(this[§§constant(31)])
                                                   {
                                                      this[§§constant(32)] = true;
                                                   }
                                                   else
                                                   {
                                                      this[§§constant(33)] -= 0.1;
                                                      if(this[§§constant(33)] < 1)
                                                      {
                                                         this[§§constant(15)][§§constant(34)][§§constant(35)] = eval("{invalid_utf8=241}{invalid_utf8=136}{invalid_utf8=205}{invalid_utf8=221}")["{invalid_utf8=153}{invalid_utf8=250}{invalid_utf8=165}"][§§constant(36)][§§constant(37)][§§constant(38)](this[§§constant(33)]);
                                                      }
                                                      if(this[§§constant(33)] < 0)
                                                      {
                                                         this[§§constant(39)] = true;
                                                      }
                                                   }
                                                }
                                                _loc6_ = 0;
                                                while(_loc6_ < eval("{invalid_utf8=241}{invalid_utf8=136}{invalid_utf8=205}{invalid_utf8=221}")["{invalid_utf8=153}{invalid_utf8=250}{invalid_utf8=165}"][§§constant(4)][§§constant(40)][§§constant(41)][§§constant(42)])
                                                {
                                                   _loc5_ = eval("{invalid_utf8=241}{invalid_utf8=136}{invalid_utf8=205}{invalid_utf8=221}")["{invalid_utf8=153}{invalid_utf8=250}{invalid_utf8=165}"][§§constant(4)][§§constant(40)][§§constant(41)][_loc6_];
                                                   _loc4_ = 0;
                                                   while(_loc4_ < _loc5_[§§constant(43)][§§constant(42)])
                                                   {
                                                      _loc3_ = _loc5_[§§constant(43)][_loc4_];
                                                      if(_loc3_[§§constant(44)])
                                                      {
                                                         if(_loc3_ != this[§§constant(45)])
                                                         {
                                                            if(_loc3_[§§constant(46)] - _loc3_[§§constant(9)] <= this[§§constant(46)] + 32)
                                                            {
                                                               if(_loc3_[§§constant(46)] + _loc3_[§§constant(10)] >= this[§§constant(46)] - 32)
                                                               {
                                                                  if(eval(§§constant(29))[§§constant(30)](_loc3_[§§constant(47)] - this[§§constant(47)]) <= 32)
                                                                  {
                                                                     if(_loc3_[§§constant(47)] > this[§§constant(47)])
                                                                     {
                                                                        _loc3_[§§constant(47)] = this[§§constant(47)] + 32;
                                                                        if(this[§§constant(23)] > 0)
                                                                        {
                                                                           _loc3_[§§constant(23)] += this[§§constant(23)];
                                                                        }
                                                                     }
                                                                     else
                                                                     {
                                                                        _loc3_[§§constant(47)] = this[§§constant(47)] - 32;
                                                                        if(this[§§constant(23)] < 0)
                                                                        {
                                                                           _loc3_[§§constant(23)] += this[§§constant(23)];
                                                                        }
                                                                     }
                                                                     _loc3_[§§constant(48)](eval(§§constant(29))[§§constant(30)](this[§§constant(23)]) * 1.5);
                                                                  }
                                                               }
                                                            }
                                                         }
                                                      }
                                                      _loc4_ = _loc4_ + 1;
                                                   }
                                                   _loc6_ = _loc6_ + 1;
                                                }
                                             }
                                          };
                                          _loc2_[§§constant(33)] = 2;
                                          §§push(§§constant(49)(eval("{invalid_utf8=241}{invalid_utf8=136}{invalid_utf8=205}{invalid_utf8=221}")["{invalid_utf8=153}{invalid_utf8=250}{invalid_utf8=165}"][§§constant(4)][§§constant(5)][§§constant(21)],null,1));
                                       }
                                       §§pop();
                                       break;
                                    }
                                    if(eval("\x01") == 392)
                                    {
                                       set("\x01",eval("\x01") - 105);
                                    }
                                    else if(eval("\x01") == 396)
                                    {
                                       set("\x01",eval("\x01") + 503);
                                    }
                                    else if(eval("\x01") == 463)
                                    {
                                       set("\x01",eval("\x01") + 484);
                                       if(§§pop())
                                       {
                                          set("\x01",eval("\x01") - 458);
                                       }
                                    }
                                    else if(eval("\x01") == 432)
                                    {
                                       set("\x01",eval("\x01") - 320);
                                       var §§pop() = §§pop();
                                    }
                                    else if(eval("\x01") == 545)
                                    {
                                       set("\x01",eval("\x01") + 301);
                                       if(§§pop())
                                       {
                                          set("\x01",eval("\x01") - 450);
                                       }
                                    }
                                    else
                                    {
                                       if(eval("\x01") == 947)
                                       {
                                          set("\x01",eval("\x01") - 458);
                                          break;
                                       }
                                       if(eval("\x01") == 286)
                                       {
                                          set("\x01",eval("\x01") + 106);
                                          break;
                                       }
                                       if(eval("\x01") == 899)
                                       {
                                          set("\x01",eval("\x01") - 149);
                                          §§push(true);
                                       }
                                       else if(eval("\x01") == 655)
                                       {
                                          set("\x01",eval("\x01") - 489);
                                       }
                                       else if(eval("\x01") == 166)
                                       {
                                          set("\x01",eval("\x01") + 266);
                                          §§push("\x0f");
                                          §§push(1);
                                       }
                                       else if(eval("\x01") == 287)
                                       {
                                          set("\x01",eval("\x01") + 176);
                                          §§push(true);
                                       }
                                       else if(eval("\x01") == 112)
                                       {
                                          set("\x01",eval("\x01") + 328);
                                          §§push("\x0f");
                                       }
                                       else if(eval("\x01") == 440)
                                       {
                                          set("\x01",eval("\x01") + 367);
                                          §§push(eval(§§pop()));
                                       }
                                       else if(eval("\x01") == 98)
                                       {
                                          set("\x01",eval("\x01") + 6);
                                          if(§§pop())
                                          {
                                             set("\x01",eval("\x01") + 337);
                                          }
                                       }
                                       else if(eval("\x01") == 750)
                                       {
                                          set("\x01",eval("\x01") - 464);
                                          if(§§pop())
                                          {
                                             set("\x01",eval("\x01") + 106);
                                          }
                                       }
                                       else if(eval("\x01") == 104)
                                       {
                                          set("\x01",eval("\x01") + 337);
                                       }
                                       else
                                       {
                                          if(eval("\x01") == 441)
                                          {
                                             set("\x01",eval("\x01") + 355);
                                             if(!eval("{invalid_utf8=223}W\x0f")["O©"])
                                             {
                                                eval("{invalid_utf8=223}W\x0f")["O©"] = new §\§\§constant(2)§();
                                             }
                                             §§pop();
                                             if(!eval("{invalid_utf8=223}W\x0f")["O©"][§§constant(3)])
                                             {
                                                eval("{invalid_utf8=223}W\x0f")["O©"][§§constant(3)] = new §\§\§constant(2)§();
                                             }
                                             §§pop();
                                             if(!eval("{invalid_utf8=223}W\x0f")["O©"][§§constant(3)][§§constant(4)])
                                             {
                                                eval("{invalid_utf8=223}W\x0f")["O©"][§§constant(3)][§§constant(4)] = new §\§\§constant(2)§();
                                             }
                                             §§pop();
                                             §§push(eval("{invalid_utf8=223}W\x0f")["O©"][§§constant(3)][§§constant(4)]);
                                             §§push(§§constant(5));
                                             break loop1;
                                          }
                                          if(eval("\x01") != 301)
                                          {
                                             if(eval("\x01") == 796)
                                             {
                                                set("\x01",eval("\x01") - 796);
                                             }
                                             break;
                                          }
                                          set("\x01",eval("\x01") - 14);
                                       }
                                    }
                                 }
                                 while(true)
                                 {
                                    if(eval("\x01") == 559)
                                    {
                                       set("\x01",eval("\x01") - 14);
                                       §§push(true);
                                    }
                                    else
                                    {
                                       if(eval("\x01") != 489)
                                       {
                                          break;
                                       }
                                       set("\x01",eval("\x01") - 323);
                                    }
                                 }
                              }
                              return;
                           }
                           if(eval("\x01") != 493)
                           {
                              return;
                           }
                           set("\x01",eval("\x01") + 88);
                           if(§§pop())
                           {
                              set("\x01",eval("\x01") + 78);
                           }
                        }
                     }
                  }
               }
            }
            while(true)
            {
               if(eval("\x01") == 592)
               {
                  set("\x01",eval("\x01") - 99);
                  §§push(true);
                  continue;
               }
               if(eval("\x01") == 27)
               {
                  set("\x01",eval("\x01") + 943);
                  continue;
               }
               if(eval("\x01") == 10)
               {
                  set("\x01",eval("\x01") + 694);
                  §§push(!§§pop());
                  continue;
               }
               if(eval("\x01") == 82)
               {
                  set("\x01",eval("\x01") - 72);
                  §§push(eval(§§pop()));
                  continue;
               }
               if(eval("\x01") == 581)
               {
                  break;
               }
               if(eval("\x01") == 316)
               {
                  set("\x01",eval("\x01") - 308);
                  continue;
               }
               if(eval("\x01") == 252)
               {
                  set("\x01",eval("\x01") + 147);
                  prevFrame();
                  break loop1;
               }
               continue loop1;
            }
            set("\x01",eval("\x01") + 78);
            toggleHighQuality();
            §§push(§§pop() + §§pop());
            stop();
            stop();
            return;
         }
         if(!§§pop()[§§pop()])
         {
            eval("{invalid_utf8=187}h")["j{invalid_utf8=247}\r{invalid_utf8=132}"][§§constant(4)][§§constant(5)] extends eval("{invalid_utf8=187}h")["j{invalid_utf8=247}\r{invalid_utf8=132}"][§§constant(4)][§§constant(22)];
            _loc2_ = eval("{invalid_utf8=187}h")["j{invalid_utf8=247}\r{invalid_utf8=132}"][§§constant(4)][§§constant(5)] = function()
            {
               super(§§constant(6));
               this[§§constant(7)] = this[§§constant(8)] = this[§§constant(9)] = this[§§constant(10)] = 14;
               this[§§constant(11)] = 1.5;
               this[§§constant(12)]();
               this[§§constant(13)] = false;
               this[§§constant(14)] = true;
               this[§§constant(15)] = 180;
               this[§§constant(16)] = [0,15,30,38,45,49,53,55,57,59];
               this[§§constant(18)][§§constant(19)](§§constant(17));
               this[§§constant(20)] = false;
               this[§§constant(21)] = true;
            }[§§constant(23)];
            _loc2_[§§constant(24)] = function()
            {
               super[§§constant(24)]();
               if(this[§§constant(25)] && this[§§constant(16)][§§constant(26)] > 0 && 60 - this[§§constant(27)] >= this[§§constant(16)][0])
               {
                  _root[§§constant(29)][§§constant(30)](§§constant(28));
                  this[§§constant(16)][§§constant(31)](0,1);
               }
            };
            _loc2_[§§constant(32)] = function()
            {
               super[§§constant(32)]();
               if(this[§§constant(33)])
               {
                  this[§§constant(34)]();
               }
               if(this[§§constant(35)] == 0 && eval(§§constant(37))[§§constant(38)](this[§§constant(36)]) < 0.2)
               {
                  if(this[§§constant(39)])
                  {
                     this[§§constant(40)] = true;
                  }
                  else if(this[§§constant(33)] && !this[§§constant(41)])
                  {
                     this[§§constant(41)] = true;
                     eval("{invalid_utf8=187}h")["j{invalid_utf8=247}\r{invalid_utf8=132}"][§§constant(4)][§§constant(42)][§§constant(43)][§§constant(44)](this);
                     if(!this[§§constant(25)])
                     {
                        this[§§constant(18)][§§constant(46)](§§constant(45));
                     }
                  }
               }
               if(this[§§constant(25)] && !this[§§constant(47)])
               {
                  this[§§constant(27)]--;
                  if(this[§§constant(27)] <= 0)
                  {
                     this[§§constant(48)]();
                  }
                  eval("{invalid_utf8=187}h")["j{invalid_utf8=247}\r{invalid_utf8=132}"][§§constant(4)][§§constant(42)][§§constant(49)] = 0;
               }
            };
            _loc2_[§§constant(34)] = function()
            {
               if(!this[§§constant(33)])
               {
                  return undefined;
               }
               if(this[§§constant(25)])
               {
                  return undefined;
               }
               if(--this[§§constant(50)] > 0)
               {
                  return undefined;
               }
               var _loc8_ = 0;
               var _loc6_;
               var _loc3_;
               var _loc2_;
               var _loc5_;
               var _loc4_;
               var _loc7_;
               loop0:
               while(_loc8_ < eval("{invalid_utf8=187}h")["j{invalid_utf8=247}\r{invalid_utf8=132}"][§§constant(4)][§§constant(42)][§§constant(51)][§§constant(26)])
               {
                  _loc6_ = eval("{invalid_utf8=187}h")["j{invalid_utf8=247}\r{invalid_utf8=132}"][§§constant(4)][§§constant(42)][§§constant(51)][_loc8_];
                  _loc3_ = 0;
                  while(true)
                  {
                     if(_loc3_ < _loc6_[§§constant(52)][§§constant(26)])
                     {
                        _loc2_ = _loc6_[§§constant(52)][_loc3_];
                        if(_loc2_[§§constant(53)])
                        {
                           if(!(_loc2_[§§constant(35)] == 0 && eval(§§constant(37))[§§constant(38)](_loc2_[§§constant(36)]) <= 0.2 && eval("{invalid_utf8=187}h")["j{invalid_utf8=247}\r{invalid_utf8=132}"][§§constant(4)][§§constant(42)][§§constant(54)] != _loc2_))
                           {
                              _loc5_ = _loc2_[§§constant(55)] - this[§§constant(55)];
                              _loc4_ = _loc2_[§§constant(56)] + _loc2_[§§constant(10)] - this[§§constant(56)];
                              _loc7_ = _loc5_ * _loc5_ + _loc4_ * _loc4_;
                              if(_loc7_ < 3600)
                              {
                                 break;
                              }
                           }
                        }
                        continue;
                     }
                     continue loop0;
                     _loc8_ = _loc8_ + 1;
                     _loc3_ = _loc3_ + 1;
                  }
                  this[§§constant(25)] = true;
                  this[§§constant(18)][§§constant(46)](§§constant(57));
                  return undefined;
               }
            };
            _loc2_[§§constant(48)] = function()
            {
               if(this[§§constant(39)])
               {
                  this[§§constant(40)] = true;
                  return undefined;
               }
               if(this[§§constant(47)])
               {
                  return undefined;
               }
               new eval("{invalid_utf8=187}h")["j{invalid_utf8=247}\r{invalid_utf8=132}"][§§constant(4)][§§constant(59)](this[§§constant(55)],this[§§constant(56)],250,70,this[§§constant(58)]);
               _root[§§constant(29)][§§constant(30)](§§constant(60));
               var _loc3_ = eval("{invalid_utf8=187}h")["j{invalid_utf8=247}\r{invalid_utf8=132}"][§§constant(4)][§§constant(42)][§§constant(43)][§§constant(26)] - 1;
               while(_loc3_ >= 0)
               {
                  if(eval("{invalid_utf8=187}h")["j{invalid_utf8=247}\r{invalid_utf8=132}"][§§constant(4)][§§constant(42)][§§constant(43)][_loc3_] == this)
                  {
                     eval("{invalid_utf8=187}h")["j{invalid_utf8=247}\r{invalid_utf8=132}"][§§constant(4)][§§constant(42)][§§constant(43)][§§constant(31)](_loc3_,1);
                  }
                  _loc3_ = _loc3_ - 1;
               }
               this[§§constant(41)] = true;
               this[§§constant(47)] = true;
               this[§§constant(61)]();
            };
            _loc2_[§§constant(50)] = 10;
            _loc2_[§§constant(27)] = 60;
            _loc2_[§§constant(25)] = false;
            _loc2_[§§constant(47)] = false;
            §§push(§§constant(62)(eval("{invalid_utf8=187}h")["j{invalid_utf8=247}\r{invalid_utf8=132}"][§§constant(4)][§§constant(5)][§§constant(23)],null,1));
         }
         §§pop();
         break;
      }
      if(eval("\x01") == 141)
      {
         set("\x01",eval("\x01") + 712);
         if(!eval("{invalid_utf8=174}L\b{invalid_utf8=197}.")["D`\n\x01B"])
         {
            eval("{invalid_utf8=174}L\b{invalid_utf8=197}.")["D`\n\x01B"] = new §{invalid_utf8=146}{invalid_utf8=232}\x1c{invalid_utf8=195}f§();
         }
         §§pop();
         if(!eval("{invalid_utf8=174}L\b{invalid_utf8=197}.")["D`\n\x01B"][§§constant(3)])
         {
            eval("{invalid_utf8=174}L\b{invalid_utf8=197}.")["D`\n\x01B"][§§constant(3)] = new §{invalid_utf8=146}{invalid_utf8=232}\x1c{invalid_utf8=195}f§();
         }
         §§pop();
         if(!eval("{invalid_utf8=174}L\b{invalid_utf8=197}.")["D`\n\x01B"][§§constant(3)][§§constant(4)])
         {
            eval("{invalid_utf8=174}L\b{invalid_utf8=197}.")["D`\n\x01B"][§§constant(3)][§§constant(4)] = new §{invalid_utf8=146}{invalid_utf8=232}\x1c{invalid_utf8=195}f§();
         }
         §§pop();
         if(!eval("{invalid_utf8=174}L\b{invalid_utf8=197}.")["D`\n\x01B"][§§constant(3)][§§constant(4)][§§constant(5)])
         {
            eval("D`\n\x01B")[§§constant(3)][§§constant(4)][§§constant(5)] extends eval("D`\n\x01B")[§§constant(3)][§§constant(4)][§§constant(13)];
            _loc2_ = eval("D`\n\x01B")[§§constant(3)][§§constant(4)][§§constant(5)] = function()
            {
               super(§§constant(6));
               this[§§constant(7)] = true;
               this[§§constant(8)] = false;
               this[§§constant(9)] = 0;
               this[§§constant(10)] = false;
               this[§§constant(11)] = false;
               this[§§constant(12)] = [];
            }[§§constant(14)];
            _loc2_[§§constant(15)] = function()
            {
               var _loc6_;
               var _loc5_;
               var _loc4_;
               var _loc3_;
               if(this[§§constant(16)])
               {
                  super[§§constant(15)]();
                  if(this[§§constant(17)] > (eval("D`\n\x01B")[§§constant(3)][§§constant(4)][§§constant(18)][§§constant(19)][§§constant(20)] << 5) + 550)
                  {
                     this[§§constant(21)] = true;
                     this[§§constant(22)]();
                  }
                  _loc6_ = 0;
                  while(_loc6_ < eval("D`\n\x01B")[§§constant(3)][§§constant(4)][§§constant(18)][§§constant(23)][§§constant(24)])
                  {
                     _loc5_ = eval("D`\n\x01B")[§§constant(3)][§§constant(4)][§§constant(18)][§§constant(23)][_loc6_];
                     _loc4_ = 0;
                     while(_loc4_ < _loc5_[§§constant(25)][§§constant(24)])
                     {
                        _loc3_ = _loc5_[§§constant(25)][_loc4_];
                        if(_loc3_[§§constant(26)])
                        {
                           if(_loc3_[§§constant(27)] >= eval("D`\n\x01B")[§§constant(3)][§§constant(4)][§§constant(18)][§§constant(28)][§§constant(27)] - 300)
                           {
                              if(_loc3_[§§constant(17)] >= this[§§constant(17)] - 150)
                              {
                                 if(_loc3_[§§constant(17)] <= this[§§constant(17)] + 150)
                                 {
                                    _loc3_[§§constant(29)](5);
                                 }
                              }
                           }
                        }
                        _loc4_ = _loc4_ + 1;
                     }
                     _loc6_ = _loc6_ + 1;
                  }
               }
               else if(eval("D`\n\x01B")[§§constant(3)][§§constant(4)][§§constant(18)][§§constant(19)][§§constant(30)])
               {
                  this[§§constant(31)]();
               }
            };
            _loc2_[§§constant(32)] = function(details)
            {
               this[§§constant(31)]();
            };
            _loc2_[§§constant(31)] = function()
            {
               this[§§constant(16)] = true;
               this[§§constant(33)] = true;
               this[§§constant(17)] = -550;
               this[§§constant(27)] = eval("D`\n\x01B")[§§constant(3)][§§constant(4)][§§constant(18)][§§constant(28)][§§constant(27)];
               this[§§constant(34)] = 20;
               this[§§constant(35)]();
               this[§§constant(39)][§§constant(40)](§§constant(36) + eval("D`\n\x01B")[§§constant(3)][§§constant(4)][§§constant(18)][§§constant(37)][§§constant(38)]());
               this[§§constant(41)][§§constant(42)] = false;
               this[§§constant(41)][§§constant(43)] = false;
            };
            §§push(§§constant(44)(eval("D`\n\x01B")[§§constant(3)][§§constant(4)][§§constant(5)][§§constant(14)],null,1));
         }
         §§pop();
         break;
      }
      if(eval("\x01") == 262)
      {
         set("\x01",eval("\x01") + 123);
         §§push(§§pop() >> §§pop());
         break;
      }
      if(eval("\x01") == 113)
      {
         set("\x01",eval("\x01") + 480);
      }
      else
      {
         if(eval("\x01") == 724)
         {
            set("\x01",eval("\x01") - 601);
            §§pop() extends §§pop() << (§§pop() >>> (§§pop() > (§§pop() | §§pop() << §§pop())));
            var _temp_1 = §§pop();
            §§pop() >>> (§§pop() | §§pop() > (§§pop() gt §§pop() >>> §§pop())) extends _temp_1;
            §§push(§§pop() >>> §§pop());
            break;
         }
         if(eval("\x01") == 881)
         {
            set("\x01",eval("\x01") - 288);
         }
         else if(eval("\x01") == 593)
         {
            set("\x01",eval("\x01") + 265);
            §§push(true);
         }
         else if(eval("\x01") == 858)
         {
            set("\x01",eval("\x01") - 134);
            if(§§pop())
            {
               set("\x01",eval("\x01") - 601);
            }
         }
         else if(eval("\x01") == 414)
         {
            set("\x01",eval("\x01") + 56);
         }
         else if(eval("\x01") == 470)
         {
            set("\x01",eval("\x01") + 25);
            §§push("\x0f");
            §§push(1);
         }
         else if(eval("\x01") == 495)
         {
            set("\x01",eval("\x01") - 157);
            var §§pop() = §§pop();
         }
         else if(eval("\x01") == 338)
         {
            set("\x01",eval("\x01") + 36);
            §§push("\x0f");
         }
         else if(eval("\x01") == 374)
         {
            set("\x01",eval("\x01") - 298);
            §§push(eval(§§pop()));
         }
         else if(eval("\x01") == 489)
         {
            set("\x01",eval("\x01") + 288);
            if(§§pop())
            {
               set("\x01",eval("\x01") - 636);
            }
         }
         else
         {
            if(eval("\x01") != 777)
            {
               if(eval("\x01") == 853)
               {
                  set("\x01",eval("\x01") - 853);
               }
               break;
            }
            set("\x01",eval("\x01") - 636);
         }
      }
   }
}
