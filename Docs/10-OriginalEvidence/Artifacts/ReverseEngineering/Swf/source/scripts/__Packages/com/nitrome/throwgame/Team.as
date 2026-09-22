function §\x04\x05§()
{
   set("\x03",1548 % 511 * true);
   return eval("\x03");
}
var §\x01§ = 287 + "\x04\x05"();
var _loc2_;
while(true)
{
   if(eval("\x01") == 302)
   {
      set("\x01",eval("\x01") + 186);
      §§push(true);
   }
   else if(eval("\x01") == 341)
   {
      set("\x01",eval("\x01") - 248);
   }
   else if(eval("\x01") == 617)
   {
      set("\x01",eval("\x01") - 459);
      §§push(!§§pop());
   }
   else if(eval("\x01") == 708)
   {
      set("\x01",eval("\x01") - 302);
   }
   else if(eval("\x01") == 763)
   {
      set("\x01",eval("\x01") - 357);
   }
   else if(eval("\x01") == 122)
   {
      set("\x01",eval("\x01") + 203);
   }
   else if(eval("\x01") == 839)
   {
      set("\x01",eval("\x01") - 222);
      §§push(eval(§§pop()));
   }
   else if(eval("\x01") == 718)
   {
      set("\x01",eval("\x01") - 183);
      if(§§pop())
      {
         set("\x01",eval("\x01") + 294);
      }
   }
   else
   {
      if(eval("\x01") == 93)
      {
         set("\x01",eval("\x01") + 425);
         if(!pY["{invalid_utf8=209}y{invalid_utf8=156}N\x01"])
         {
            pY["{invalid_utf8=209}y{invalid_utf8=156}N\x01"] = new §{invalid_utf8=132}{invalid_utf8=222}{invalid_utf8=247}§();
         }
         §§pop();
         if(!pY["{invalid_utf8=209}y{invalid_utf8=156}N\x01"]["z{t{invalid_utf8=181}"])
         {
            pY["{invalid_utf8=209}y{invalid_utf8=156}N\x01"]["z{t{invalid_utf8=181}"] = new §{invalid_utf8=132}{invalid_utf8=222}{invalid_utf8=247}§();
         }
         §§pop();
         if(!pY["{invalid_utf8=209}y{invalid_utf8=156}N\x01"]["z{t{invalid_utf8=181}"][§§constant(4)])
         {
            pY["{invalid_utf8=209}y{invalid_utf8=156}N\x01"]["z{t{invalid_utf8=181}"][§§constant(4)] = new §{invalid_utf8=132}{invalid_utf8=222}{invalid_utf8=247}§();
         }
         §§pop();
         if(!pY["{invalid_utf8=209}y{invalid_utf8=156}N\x01"]["z{t{invalid_utf8=181}"][§§constant(4)][§§constant(5)])
         {
            _loc2_ = eval("{invalid_utf8=209}y{invalid_utf8=156}N\x01")["z{t{invalid_utf8=181}"][§§constant(4)][§§constant(5)] = function(teamNumber)
            {
               this[§§constant(6)] = teamNumber;
               this[§§constant(7)] = [];
               if(_root[§§constant(8)] == 1)
               {
                  this[§§constant(9)] = teamNumber == 2;
               }
               else
               {
                  this[§§constant(9)] = false;
               }
            }[§§constant(10)];
            _loc2_[§§constant(11)] = function()
            {
               var _loc2_;
               var _loc4_;
               var _loc3_;
               var _loc5_;
               var _loc6_;
               if(this[§§constant(9)])
               {
                  if(!this[§§constant(12)] && eval("{invalid_utf8=209}y{invalid_utf8=156}N\x01")["z{t{invalid_utf8=181}"][§§constant(4)][§§constant(13)][§§constant(14)] == this)
                  {
                     _loc2_ = 0;
                     while(_loc2_ < this[§§constant(7)][§§constant(15)])
                     {
                        if(!this[§§constant(7)][_loc2_][§§constant(12)])
                        {
                           _loc4_ = getTimer();
                           do
                           {
                              this[§§constant(7)][_loc2_][§§constant(16)]();
                           }
                           while(!this[§§constant(7)][_loc2_][§§constant(12)] && getTimer() < _loc4_ + 30);
                           break;
                        }
                        _loc2_ = _loc2_ + 1;
                     }
                     this[§§constant(12)] = this[§§constant(7)][this[§§constant(7)][§§constant(15)] - 1][§§constant(12)];
                     if(this[§§constant(12)])
                     {
                        _loc3_ = [];
                        _loc2_ = 0;
                        while(_loc2_ < this[§§constant(7)][§§constant(15)])
                        {
                           _loc3_ = _loc3_[§§constant(18)](this[§§constant(7)][_loc2_][§§constant(17)]);
                           _loc2_ = _loc2_ + 1;
                        }
                        _loc5_ = null;
                        _loc6_ = - eval(§§constant(19));
                        _loc2_ = 0;
                        while(_loc2_ < _loc3_[§§constant(15)])
                        {
                           if(_loc3_[_loc2_][§§constant(20)] > _loc6_)
                           {
                              _loc6_ = _loc3_[_loc2_][§§constant(20)];
                              _loc5_ = _loc3_[_loc2_];
                           }
                           _loc2_ = _loc2_ + 1;
                        }
                        if(_loc6_ > 0 || !this[§§constant(21)])
                        {
                           this[§§constant(23)](_loc5_[§§constant(22)]);
                           this[§§constant(24)] = _loc5_;
                           trace(§§constant(25) + _loc5_[§§constant(20)]);
                           trace(§§constant(26) + (_loc5_[§§constant(27)] !== -1 ? _loc5_[§§constant(22)][§§constant(28)][_loc5_[§§constant(27)]] : §§constant(29)));
                           eval("{invalid_utf8=209}y{invalid_utf8=156}N\x01")["z{t{invalid_utf8=181}"][§§constant(4)][§§constant(13)][§§constant(30)][§§constant(31)] = _loc5_[§§constant(22)];
                        }
                        else
                        {
                           eval("{invalid_utf8=209}y{invalid_utf8=156}N\x01")["z{t{invalid_utf8=181}"][§§constant(4)][§§constant(13)][§§constant(32)]();
                        }
                     }
                  }
                  else if(this[§§constant(24)] && eval("{invalid_utf8=209}y{invalid_utf8=156}N\x01")["z{t{invalid_utf8=181}"][§§constant(4)][§§constant(13)][§§constant(30)][§§constant(31)] == null)
                  {
                     if(this[§§constant(24)][§§constant(27)] >= 0)
                     {
                        this[§§constant(24)][§§constant(22)][§§constant(33)](this[§§constant(24)][§§constant(27)]);
                        this[§§constant(24)][§§constant(22)][§§constant(34)][§§constant(35)](this[§§constant(24)]);
                        this[§§constant(24)][§§constant(22)][§§constant(36)] = false;
                        this[§§constant(24)][§§constant(22)][§§constant(37)] = false;
                     }
                     else
                     {
                        this[§§constant(24)][§§constant(22)][§§constant(38)]();
                        this[§§constant(24)][§§constant(22)][§§constant(39)] = this[§§constant(24)][§§constant(40)];
                        this[§§constant(24)][§§constant(22)][§§constant(41)] = this[§§constant(24)][§§constant(42)];
                        this[§§constant(24)][§§constant(22)][§§constant(43)] = true;
                        this[§§constant(24)][§§constant(22)][§§constant(36)] = false;
                     }
                     this[§§constant(24)] = null;
                  }
               }
               _loc2_ = 0;
               while(_loc2_ < this[§§constant(7)][§§constant(15)])
               {
                  this[§§constant(7)][_loc2_][§§constant(11)]();
                  _loc2_ = _loc2_ + 1;
               }
               if(eval("{invalid_utf8=209}y{invalid_utf8=156}N\x01")["z{t{invalid_utf8=181}"][§§constant(4)][§§constant(13)][§§constant(14)] == this && !this[§§constant(44)])
               {
                  eval("{invalid_utf8=209}y{invalid_utf8=156}N\x01")["z{t{invalid_utf8=181}"][§§constant(4)][§§constant(13)][§§constant(45)] = 0;
               }
               var _loc7_ = 0;
               var _loc8_ = 0;
               _loc2_ = 0;
               while(_loc2_ < this[§§constant(7)][§§constant(15)])
               {
                  _loc8_ += this[§§constant(7)][_loc2_][§§constant(46)];
                  _loc7_ += this[§§constant(7)][_loc2_][§§constant(47)];
                  _loc2_ = _loc2_ + 1;
               }
               var _loc9_ = eval("{invalid_utf8=209}y{invalid_utf8=156}N\x01")["z{t{invalid_utf8=181}"][§§constant(4)][§§constant(13)][§§constant(48)][§§constant(49) + this[§§constant(6)]];
               var _loc10_ = 1 + eval(§§constant(50))[§§constant(51)](96 * _loc8_ / _loc7_);
               _loc9_[§§constant(56)](eval("{invalid_utf8=209}y{invalid_utf8=156}N\x01")["z{t{invalid_utf8=181}"][§§constant(53)][§§constant(54)][§§constant(55)](_loc9_[§§constant(52)],_loc10_,1));
            };
            _loc2_[§§constant(57)] = function()
            {
               var _loc2_ = 0;
               while(_loc2_ < this[§§constant(7)][§§constant(15)])
               {
                  if(this[§§constant(7)][_loc2_][§§constant(58)])
                  {
                     return true;
                  }
                  _loc2_ = _loc2_ + 1;
               }
               return false;
            };
            _loc2_[§§constant(59)] = function()
            {
               var _loc3_ = 0;
               var _loc2_ = 0;
               while(_loc2_ < this[§§constant(7)][§§constant(15)])
               {
                  if(this[§§constant(7)][_loc2_][§§constant(58)])
                  {
                     _loc3_ = _loc3_ + 1;
                  }
                  _loc2_ = _loc2_ + 1;
               }
               return _loc3_;
            };
            _loc2_[§§constant(60)] = function()
            {
               var _loc2_ = 0;
               while(_loc2_ < this[§§constant(7)][§§constant(15)])
               {
                  if(!(this[§§constant(7)][_loc2_][§§constant(62)][§§constant(63)](§§constant(61)) == -1 && this[§§constant(7)][_loc2_][§§constant(62)] != §§constant(64)))
                  {
                     if(this[§§constant(7)][_loc2_][§§constant(58)])
                     {
                        return this[§§constant(7)][_loc2_];
                     }
                  }
                  _loc2_ = _loc2_ + 1;
               }
               _loc2_ = 0;
               while(_loc2_ < this[§§constant(7)][§§constant(15)])
               {
                  if(this[§§constant(7)][_loc2_][§§constant(58)])
                  {
                     return this[§§constant(7)][_loc2_];
                  }
                  _loc2_ = _loc2_ + 1;
               }
               return null;
            };
            _loc2_[§§constant(65)] = function()
            {
               var _loc2_ = 0;
               while(_loc2_ < this[§§constant(7)][§§constant(15)])
               {
                  if(this[§§constant(7)][_loc2_][§§constant(62)][§§constant(63)](§§constant(61)) != -1)
                  {
                     return this[§§constant(7)][_loc2_];
                  }
                  if(this[§§constant(7)][_loc2_][§§constant(62)] == §§constant(64))
                  {
                     return this[§§constant(7)][_loc2_];
                  }
                  _loc2_ = _loc2_ + 1;
               }
               return null;
            };
            _loc2_[§§constant(23)] = function(character, again)
            {
               if(!character[§§constant(58)])
               {
                  return undefined;
               }
               this[§§constant(44)] = character;
               this[§§constant(44)][§§constant(43)] = false;
               this[§§constant(44)][§§constant(66)] = false;
               this[§§constant(44)][§§constant(67)] = false;
               if(!again)
               {
                  _root[§§constant(69)][§§constant(70)](this[§§constant(68)]());
               }
            };
            _loc2_[§§constant(71)] = function()
            {
               this[§§constant(44)] = null;
            };
            _loc2_[§§constant(72)] = function()
            {
               this[§§constant(73)]++;
               this[§§constant(44)] = null;
               var _loc2_ = 0;
               while(_loc2_ < this[§§constant(7)][§§constant(15)])
               {
                  this[§§constant(7)][_loc2_][§§constant(74)][§§constant(75)][§§constant(76)] = _loc2_[§§constant(77)]();
                  _loc2_ = _loc2_ + 1;
               }
               if(this[§§constant(9)])
               {
                  this[§§constant(12)] = false;
                  _loc2_ = 0;
                  while(_loc2_ < this[§§constant(7)][§§constant(15)])
                  {
                     this[§§constant(7)][_loc2_][§§constant(78)]();
                     _loc2_ = _loc2_ + 1;
                  }
                  this[§§constant(21)] = false;
               }
               var _loc7_ = null;
               var _loc6_ = eval(§§constant(19));
               _loc2_ = 0;
               var _loc4_;
               var _loc3_;
               var _loc5_;
               while(_loc2_ < this[§§constant(7)][§§constant(15)])
               {
                  this[§§constant(7)][_loc2_][§§constant(74)][§§constant(75)][§§constant(79)] = false;
                  if(this[§§constant(7)][_loc2_][§§constant(28)][§§constant(15)] < 1)
                  {
                     this[§§constant(7)][_loc2_][§§constant(28)][§§constant(81)](§§constant(80));
                  }
                  this[§§constant(7)][_loc2_][§§constant(37)] = true;
                  this[§§constant(7)][_loc2_][§§constant(36)] = true;
                  this[§§constant(7)][_loc2_][§§constant(67)] = false;
                  this[§§constant(7)][_loc2_][§§constant(82)] = false;
                  this[§§constant(7)][_loc2_][§§constant(83)] = 0;
                  if(this[§§constant(7)][_loc2_][§§constant(58)])
                  {
                     _loc4_ = eval("{invalid_utf8=209}y{invalid_utf8=156}N\x01")["z{t{invalid_utf8=181}"][§§constant(4)][§§constant(13)][§§constant(30)][§§constant(84)] + 275 - this[§§constant(7)][_loc2_][§§constant(85)];
                     _loc3_ = eval("{invalid_utf8=209}y{invalid_utf8=156}N\x01")["z{t{invalid_utf8=181}"][§§constant(4)][§§constant(13)][§§constant(30)][§§constant(86)] + 275 - this[§§constant(7)][_loc2_][§§constant(87)];
                     _loc5_ = _loc4_ * _loc4_ + _loc3_ * _loc3_;
                     if(_loc5_ < _loc6_)
                     {
                        _loc6_ = _loc5_;
                        _loc7_ = this[§§constant(7)][_loc2_];
                     }
                  }
                  _loc2_ = _loc2_ + 1;
               }
               var _loc8_;
               if(!this[§§constant(44)])
               {
                  eval("{invalid_utf8=209}y{invalid_utf8=156}N\x01")["z{t{invalid_utf8=181}"][§§constant(4)][§§constant(13)][§§constant(30)][§§constant(31)] = _loc7_;
                  _loc8_ = this[§§constant(65)]();
                  if(this[§§constant(73)] == 1 && _loc8_)
                  {
                     eval("{invalid_utf8=209}y{invalid_utf8=156}N\x01")["z{t{invalid_utf8=181}"][§§constant(4)][§§constant(13)][§§constant(30)][§§constant(31)] = _loc8_;
                  }
               }
               eval("{invalid_utf8=209}y{invalid_utf8=156}N\x01")["z{t{invalid_utf8=181}"][§§constant(4)][§§constant(13)][§§constant(48)][§§constant(90)][§§constant(56)](this[§§constant(6)] != 2 ? §§constant(88) : §§constant(89));
               if(this[§§constant(9)])
               {
                  eval("{invalid_utf8=209}y{invalid_utf8=156}N\x01")["z{t{invalid_utf8=181}"][§§constant(4)][§§constant(13)][§§constant(76)][§§constant(92)](§§constant(91));
               }
               else
               {
                  eval("{invalid_utf8=209}y{invalid_utf8=156}N\x01")["z{t{invalid_utf8=181}"][§§constant(4)][§§constant(13)][§§constant(76)][§§constant(92)](§§constant(93) + this[§§constant(6)] + §§constant(94));
               }
            };
            _loc2_[§§constant(95)] = function()
            {
               this[§§constant(44)][§§constant(43)] = false;
               this[§§constant(44)][§§constant(66)] = false;
               this[§§constant(44)][§§constant(96)]();
               this[§§constant(44)] = null;
            };
            _loc2_[§§constant(97)] = function()
            {
               if(!this[§§constant(44)][§§constant(58)])
               {
                  return true;
               }
               return !(this[§§constant(44)][§§constant(36)] || this[§§constant(44)][§§constant(37)]);
            };
            _loc2_[§§constant(98)] = function()
            {
               this[§§constant(23)](this[§§constant(44)],true);
               var _loc2_;
               if(this[§§constant(9)])
               {
                  this[§§constant(12)] = false;
                  _loc2_ = 0;
                  while(_loc2_ < this[§§constant(7)][§§constant(15)])
                  {
                     this[§§constant(7)][_loc2_][§§constant(78)]();
                     this[§§constant(7)][_loc2_][§§constant(37)] = this[§§constant(7)][_loc2_] == this[§§constant(44)];
                     this[§§constant(7)][_loc2_][§§constant(36)] = false;
                     _loc2_ = _loc2_ + 1;
                  }
                  this[§§constant(21)] = true;
               }
            };
            _loc2_[§§constant(68)] = function()
            {
               var _loc2_ = this[§§constant(7)][0][§§constant(62)];
               _loc2_ = _loc2_[§§constant(100)](§§constant(61))[§§constant(101)](§§constant(99));
               _loc2_ = _loc2_[§§constant(100)](§§constant(102))[§§constant(101)](§§constant(99));
               return _loc2_;
            };
            _loc2_[§§constant(103)] = function()
            {
               var _loc3_ = 0;
               var _loc2_ = 0;
               while(_loc2_ < this[§§constant(7)][§§constant(15)])
               {
                  if(this[§§constant(7)][_loc2_][§§constant(58)])
                  {
                     _loc3_ += this[§§constant(7)][_loc2_][§§constant(46)];
                  }
                  _loc2_ = _loc2_ + 1;
               }
               return _loc3_;
            };
            _loc2_[§§constant(104)] = function()
            {
               return this[§§constant(103)]() / this[§§constant(7)][§§constant(15)];
            };
            _loc2_[§§constant(105)] = function()
            {
               var _loc2_ = 0;
               while(_loc2_ < this[§§constant(7)][§§constant(15)])
               {
                  this[§§constant(7)][_loc2_][§§constant(38)]();
                  this[§§constant(7)][_loc2_][§§constant(105)]();
                  _loc2_ = _loc2_ + 1;
               }
            };
            _loc2_[§§constant(44)] = null;
            _loc2_[§§constant(9)] = false;
            _loc2_[§§constant(12)] = false;
            _loc2_[§§constant(21)] = false;
            _loc2_[§§constant(73)] = 0;
            eval("{invalid_utf8=209}y{invalid_utf8=156}N\x01")["z{t{invalid_utf8=181}"][§§constant(4)][§§constant(5)] = function(teamNumber)
            {
               this[§§constant(6)] = teamNumber;
               this[§§constant(7)] = [];
               if(_root[§§constant(8)] == 1)
               {
                  this[§§constant(9)] = teamNumber == 2;
               }
               else
               {
                  this[§§constant(9)] = false;
               }
            }[§§constant(106)] = {(§§constant(107)):[§§constant(111),§§constant(110),§§constant(109),§§constant(108)],(§§constant(112)):[§§constant(116),§§constant(115),§§constant(114),§§constant(113)],(§§constant(117)):[§§constant(121),§§constant(120),§§constant(119),§§constant(118)],(§§constant(122)):[§§constant(126),§§constant(125),§§constant(124),§§constant(123)],(§§constant(127)):[§§constant(131),§§constant(130),§§constant(129),§§constant(128)],(§§constant(132)):[§§constant(136),§§constant(135),§§constant(134),§§constant(133)],(§§constant(137)):[§§constant(141),§§constant(140),§§constant(139),§§constant(138)],(§§constant(142)):[§§constant(146),§§constant(145),§§constant(144),§§constant(143)],(§§constant(147)):[§§constant(151),§§constant(150),§§constant(149),§§constant(148)],(§§constant(152)):[§§constant(156),§§constant(155),§§constant(154),§§constant(153)],(§§constant(157)):[§§constant(161),§§constant(160),§§constant(159),§§constant(158)],(§§constant(162)):[§§constant(166),§§constant(165),§§constant(164),§§constant(163)],(§§constant(167)):[§§constant(171),§§constant(170),§§constant(169),§§constant(168)],(§§constant(172)):[§§constant(176),§§constant(175),§§constant(174),§§constant(173)],(§§constant(177)):[§§constant(181),§§constant(180),§§constant(179),§§constant(178)]};
            §§push(§§constant(182)(eval("{invalid_utf8=209}y{invalid_utf8=156}N\x01")["z{t{invalid_utf8=181}"][§§constant(4)][§§constant(5)][§§constant(10)],null,1));
         }
         §§pop();
         break;
      }
      if(eval("\x01") == 893)
      {
         set("\x01",eval("\x01") - 54);
         §§push("\x0f");
      }
      else if(eval("\x01") == 158)
      {
         set("\x01",eval("\x01") + 183);
         if(§§pop())
         {
            set("\x01",eval("\x01") - 248);
         }
      }
      else if(eval("\x01") == 406)
      {
         set("\x01",eval("\x01") + 312);
         §§push(true);
      }
      else if(eval("\x01") == 505)
      {
         set("\x01",eval("\x01") + 388);
         var §§pop() = §§pop();
      }
      else if(eval("\x01") == 325)
      {
         set("\x01",eval("\x01") + 180);
         §§push("\x0f");
         §§push(1);
      }
      else if(eval("\x01") == 480)
      {
         set("\x01",eval("\x01") - 12);
         §§push(true);
      }
      else
      {
         if(eval("\x01") == 535)
         {
            set("\x01",eval("\x01") + 294);
            §§push(§§pop() + §§pop());
            break;
         }
         if(eval("\x01") == 518)
         {
            set("\x01",eval("\x01") - 518);
            break;
         }
         if(eval("\x01") == 249)
         {
            set("\x01",eval("\x01") - 127);
            break;
         }
         if(eval("\x01") == 488)
         {
            set("\x01",eval("\x01") + 374);
            if(§§pop())
            {
               set("\x01",eval("\x01") - 99);
            }
         }
         else
         {
            if(eval("\x01") == 862)
            {
               set("\x01",eval("\x01") - 99);
               stop();
               break;
            }
            if(eval("\x01") == 977)
            {
               set("\x01",eval("\x01") - 497);
            }
            else if(eval("\x01") == 290)
            {
               set("\x01",eval("\x01") + 35);
            }
            else if(eval("\x01") == 468)
            {
               set("\x01",eval("\x01") - 219);
               if(§§pop())
               {
                  set("\x01",eval("\x01") - 127);
               }
            }
            else if(eval("\x01") == 301)
            {
               set("\x01",eval("\x01") + 179);
            }
            else if(eval("\x01") == 915)
            {
               set("\x01",eval("\x01") - 886);
               §§push(true);
            }
            else
            {
               if(eval("\x01") == 62)
               {
                  set("\x01",eval("\x01") + 239);
                  break;
               }
               if(eval("\x01") == 829)
               {
                  set("\x01",eval("\x01") + 86);
               }
               else if(eval("\x01") == 29)
               {
                  set("\x01",eval("\x01") + 33);
                  if(§§pop())
                  {
                     set("\x01",eval("\x01") + 239);
                  }
               }
               else
               {
                  if(eval("\x01") != 187)
                  {
                     break;
                  }
                  set("\x01",eval("\x01") + 728);
               }
            }
         }
      }
   }
}
