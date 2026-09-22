function §\x04\x05§()
{
   set("\x03",447 % 511 * true);
   return eval("\x03");
}
var §\x01§ = -113 + "\x04\x05"();
var _loc4_;
var _loc6_;
var _loc5_;
var _loc3_;
var _loc2_;
while(true)
{
   if(eval("\x01") == 334)
   {
      set("\x01",eval("\x01") + 469);
      §§push(true);
   }
   else
   {
      if(eval("\x01") == 599)
      {
         set("\x01",eval("\x01") - 547);
         toggleHighQuality();
         var _temp_1 = mbsubstring(§§pop(),§§pop(),§§pop());
         prevFrame();
         §§pop()[§§pop()] = mbsubstring(§§pop(),§§pop(),§§pop());
         eval(§§constant(45))[§§constant(22)]();
         _loc1_[§§constant(8)][§§constant(54)](eval(§§constant(45)));
         _loc1_[§§constant(34)]();
         _loc1_[§§constant(22)]();
         _loc1_[§§constant(55)][§§constant(56)](§§constant(45));
         _loc3_[§§constant(61)][§§constant(62)](§§constant(57) + (eval(§§constant(58))[§§constant(60)](eval(§§constant(58))[§§constant(59)]() * 3) + 1));
         eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(14)][§§constant(31)][§§constant(32)] = false;
         _loc4_ = 0;
         while(_loc4_ < _loc1_[§§constant(8)][§§constant(41)])
         {
            _loc1_[§§constant(8)][_loc4_][§§constant(35)]();
            _loc4_ = _loc4_ + 1;
         }
         if(_loc1_[§§constant(18)] > (eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(14)][§§constant(31)][§§constant(44)] << 5) + 275 && _loc1_[§§constant(8)][§§constant(41)] < 1)
         {
            _loc1_[§§constant(43)] = true;
         }
         _loc1_[§§constant(63)] = _loc1_[§§constant(19)] + 100;
         §§pop()[§§pop()] = §§pop();
         _loc2_[§§constant(49)] = function(shot)
         {
            var _loc2_ = this[§§constant(8)][§§constant(41)] - 1;
            while(_loc2_ >= 0)
            {
               if(this[§§constant(8)][_loc2_] == shot)
               {
                  this[§§constant(8)][§§constant(42)](_loc2_,1);
               }
               _loc2_ = _loc2_ - 1;
            }
         };
         _loc2_[§§constant(51)] = function()
         {
            super[§§constant(51)]();
            this[§§constant(13)][§§constant(51)]();
         };
         _loc2_[§§constant(64)] = function()
         {
            var _loc15_ = this[§§constant(28)][§§constant(38)][§§constant(65)];
            var _loc12_ = eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(14)][§§constant(66)][2 - this[§§constant(28)][§§constant(38)][§§constant(67)]][§§constant(65)];
            var _loc8_;
            var _loc9_;
            var _loc6_;
            var _loc3_;
            var _loc5_;
            var _loc4_;
            var _loc7_;
            var _loc10_;
            var _loc21_ = eval(§§constant(68));
            _loc3_ = 0;
            while(_loc3_ < _loc12_[§§constant(41)])
            {
               _loc6_ = _loc12_[_loc3_];
               if(_loc6_[§§constant(69)])
               {
                  if(_loc6_[§§constant(19)] < _loc21_)
                  {
                     _loc21_ = _loc6_[§§constant(19)];
                  }
               }
               _loc3_ = _loc3_ + 1;
            }
            _loc21_ -= 100;
            _loc21_ -= eval(§§constant(58))[§§constant(60)](eval(§§constant(58))[§§constant(59)]() * 100);
            var _loc14_ = [];
            var _loc13_ = 0;
            while(_loc13_ < 10)
            {
               _loc14_[§§constant(54)](eval(§§constant(58))[§§constant(60)](eval(§§constant(58))[§§constant(59)]() * (eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(14)][§§constant(31)][§§constant(44)] << 5)));
               _loc13_ = _loc13_ + 1;
            }
            _loc14_ = _loc14_[§§constant(72)](eval(§§constant(70))[§§constant(71)]);
            var _loc17_ = [];
            var _loc18_ = 0;
            _loc13_ = 0;
            var _loc16_;
            var _loc2_;
            var _loc11_;
            while(_loc13_ < _loc14_[§§constant(41)])
            {
               _loc16_ = _loc14_[_loc13_];
               _loc2_ = new eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(23)]();
               _loc2_[§§constant(18)] = _loc16_ - 10;
               _loc2_[§§constant(19)] = _loc21_;
               _loc2_[§§constant(33)] = 10;
               _loc2_[§§constant(10)] = 1;
               _loc2_[§§constant(47)] = function(side)
               {
                  this[§§constant(73)] = true;
               };
               _loc2_[§§constant(53)] = true;
               while(!_loc2_[§§constant(73)] && _loc2_[§§constant(19)] < eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(14)][§§constant(52)][§§constant(19)])
               {
                  _loc2_[§§constant(74)]();
               }
               _loc11_ = 0;
               _loc3_ = 0;
               while(_loc3_ < _loc12_[§§constant(41)])
               {
                  _loc6_ = _loc12_[_loc3_];
                  if(_loc6_[§§constant(69)])
                  {
                     _loc5_ = _loc6_[§§constant(18)] - _loc2_[§§constant(18)];
                     _loc4_ = _loc6_[§§constant(19)] - _loc2_[§§constant(19)];
                     _loc7_ = _loc5_ * _loc5_ + _loc4_ * _loc4_;
                     if(_loc7_ < 1600)
                     {
                        _loc10_ = eval(§§constant(58))[§§constant(75)](_loc7_);
                        _loc11_ += 1 - _loc10_ / 40;
                     }
                  }
                  _loc3_ = _loc3_ + 1;
               }
               _loc9_ = 0;
               while(_loc9_ < _loc15_[§§constant(41)])
               {
                  _loc8_ = _loc15_[_loc9_];
                  if(_loc8_[§§constant(69)])
                  {
                     _loc5_ = _loc8_[§§constant(18)] - _loc2_[§§constant(18)];
                     _loc4_ = _loc8_[§§constant(19)] - _loc2_[§§constant(19)];
                     _loc7_ = _loc5_ * _loc5_ + _loc4_ * _loc4_;
                     if(_loc7_ < 1600)
                     {
                        _loc10_ = eval(§§constant(58))[§§constant(75)](_loc7_);
                        _loc11_ -= 1.5 - _loc10_ / 40;
                     }
                  }
                  _loc9_ = _loc9_ + 1;
               }
               if(_loc11_ > 0)
               {
                  _loc17_[§§constant(54)](_loc14_[_loc13_]);
                  _loc18_ += _loc11_;
               }
               _loc2_[§§constant(51)]();
               _loc13_ = _loc13_ + 1;
            }
            return {(§§constant(76)):_loc18_,(§§constant(8)):_loc17_,(§§constant(77)):_loc21_};
         };
         _loc2_[§§constant(78)] = function(details)
         {
            this[§§constant(25)](-300,details[§§constant(77)]);
            this[§§constant(40)] = details[§§constant(8)];
         };
         §§constant(79)(eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(5)][§§constant(24)],null,1);
         break;
      }
      if(eval("\x01") == 293)
      {
         set("\x01",eval("\x01") + 642);
         var §§pop() = §§pop();
      }
      else if(eval("\x01") == 494)
      {
         set("\x01",eval("\x01") - 168);
         §§push(true);
      }
      else if(eval("\x01") == 323)
      {
         set("\x01",eval("\x01") + 566);
      }
      else if(eval("\x01") == 803)
      {
         set("\x01",eval("\x01") - 48);
         if(§§pop())
         {
            set("\x01",eval("\x01") - 22);
         }
      }
      else if(eval("\x01") == 488)
      {
         set("\x01",eval("\x01") - 195);
         §§push("\x0f");
         §§push(1);
      }
      else if(eval("\x01") == 911)
      {
         set("\x01",eval("\x01") - 417);
      }
      else if(eval("\x01") == 859)
      {
         set("\x01",eval("\x01") + 30);
      }
      else if(eval("\x01") == 340)
      {
         set("\x01",eval("\x01") - 198);
         if(§§pop())
         {
            set("\x01",eval("\x01") + 731);
         }
      }
      else if(eval("\x01") == 302)
      {
         set("\x01",eval("\x01") + 186);
      }
      else
      {
         if(eval("\x01") == 755)
         {
            set("\x01",eval("\x01") - 22);
            toggleHighQuality();
            §§pop();
            loop2:
            while(true)
            {
               §§pop()[§§pop()] = §§pop() + §§pop()[§§pop()];
               while(true)
               {
                  _loc3_[§§constant(48)](eval(§§constant(29))[§§constant(30)](_loc1_[§§constant(23)]) * 1.5);
                  while(true)
                  {
                     _loc4_ = _loc4_ + 1;
                     while(_loc4_ >= _loc5_[§§constant(43)][§§constant(42)])
                     {
                        _loc6_ = _loc6_ + 1;
                        if(_loc6_ >= eval("{invalid_utf8=139}I{invalid_utf8=217}\x12{invalid_utf8=240}")["\x13{invalid_utf8=232}d"]["8{invalid_utf8=183}\x13"][§§constant(40)][§§constant(41)][§§constant(42)])
                        {
                           break loop2;
                        }
                        _loc5_ = eval("{invalid_utf8=139}I{invalid_utf8=217}\x12{invalid_utf8=240}")["\x13{invalid_utf8=232}d"]["8{invalid_utf8=183}\x13"][§§constant(40)][§§constant(41)][_loc6_];
                        _loc4_ = 0;
                     }
                     _loc3_ = _loc5_[§§constant(43)][_loc4_];
                     if(_loc3_[§§constant(44)])
                     {
                        if(_loc3_ != _loc1_[§§constant(45)])
                        {
                           if(_loc3_[§§constant(46)] - _loc3_[§§constant(9)] <= _loc1_[§§constant(46)] + 32)
                           {
                              if(_loc3_[§§constant(46)] + _loc3_[§§constant(10)] >= _loc1_[§§constant(46)] - 32)
                              {
                                 if(eval(§§constant(29))[§§constant(30)](_loc3_[§§constant(47)] - _loc1_[§§constant(47)]) <= 32)
                                 {
                                    break;
                                 }
                              }
                           }
                        }
                     }
                  }
                  if(_loc3_[§§constant(47)] > _loc1_[§§constant(47)])
                  {
                     _loc3_[§§constant(47)] = _loc1_[§§constant(47)] + 32;
                     if(_loc1_[§§constant(23)] > 0)
                     {
                        _loc3_[§§constant(23)] += _loc1_[§§constant(23)];
                     }
                  }
                  else
                  {
                     _loc3_[§§constant(47)] = _loc1_[§§constant(47)] - 32;
                     if(_loc1_[§§constant(23)] < 0)
                     {
                        break;
                     }
                  }
               }
               §§push(_loc3_);
               §§push(§§constant(23));
               §§push(_loc3_[§§constant(23)]);
               §§push(_loc1_);
               §§push(§§constant(23));
            }
            §§pop()[§§pop()] = §§pop();
            _loc2_[§§constant(33)] = 2;
            §§constant(49)(eval("{invalid_utf8=139}I{invalid_utf8=217}\x12{invalid_utf8=240}")["\x13{invalid_utf8=232}d"]["8{invalid_utf8=183}\x13"][§§constant(5)][§§constant(21)],null,1);
            break;
         }
         if(eval("\x01") == 652)
         {
            set("\x01",eval("\x01") - 312);
            §§push(!§§pop());
         }
         else if(eval("\x01") == 52)
         {
            set("\x01",eval("\x01") + 436);
         }
         else
         {
            if(eval("\x01") == 873)
            {
               set("\x01",eval("\x01") - 97);
               if(!eval("{invalid_utf8=234}z")["{invalid_utf8=139}I{invalid_utf8=217}\x12{invalid_utf8=240}"])
               {
                  eval("{invalid_utf8=234}z")["{invalid_utf8=139}I{invalid_utf8=217}\x12{invalid_utf8=240}"] = new §S2\x0e§();
               }
               §§pop();
               if(!eval("{invalid_utf8=234}z")["{invalid_utf8=139}I{invalid_utf8=217}\x12{invalid_utf8=240}"]["\x13{invalid_utf8=232}d"])
               {
                  eval("{invalid_utf8=234}z")["{invalid_utf8=139}I{invalid_utf8=217}\x12{invalid_utf8=240}"]["\x13{invalid_utf8=232}d"] = new §S2\x0e§();
               }
               §§pop();
               if(!eval("{invalid_utf8=234}z")["{invalid_utf8=139}I{invalid_utf8=217}\x12{invalid_utf8=240}"]["\x13{invalid_utf8=232}d"]["8{invalid_utf8=183}\x13"])
               {
                  eval("{invalid_utf8=234}z")["{invalid_utf8=139}I{invalid_utf8=217}\x12{invalid_utf8=240}"]["\x13{invalid_utf8=232}d"]["8{invalid_utf8=183}\x13"] = new §S2\x0e§();
               }
               §§pop();
               if(!eval("{invalid_utf8=234}z")["{invalid_utf8=139}I{invalid_utf8=217}\x12{invalid_utf8=240}"]["\x13{invalid_utf8=232}d"]["8{invalid_utf8=183}\x13"][§§constant(5)])
               {
                  eval("{invalid_utf8=139}I{invalid_utf8=217}\x12{invalid_utf8=240}")["\x13{invalid_utf8=232}d"]["8{invalid_utf8=183}\x13"][§§constant(5)] extends eval("{invalid_utf8=139}I{invalid_utf8=217}\x12{invalid_utf8=240}")["\x13{invalid_utf8=232}d"]["8{invalid_utf8=183}\x13"][§§constant(19)];
                  _loc2_ = eval("{invalid_utf8=139}I{invalid_utf8=217}\x12{invalid_utf8=240}")["\x13{invalid_utf8=232}d"]["8{invalid_utf8=183}\x13"][§§constant(5)] = function()
                  {
                     super(§§constant(6));
                     this[§§constant(7)] = this[§§constant(8)] = this[§§constant(9)] = this[§§constant(10)] = 11;
                     this[§§constant(11)]();
                     this[§§constant(12)] = 1.7;
                     this[§§constant(13)] = true;
                     this[§§constant(15)][§§constant(16)](§§constant(14));
                     this[§§constant(17)] = false;
                     this[§§constant(18)] = true;
                  }[§§constant(20)];
                  _loc2_[§§constant(21)] = function()
                  {
                     if(this[§§constant(22)])
                     {
                        this[§§constant(23)] += this[§§constant(24)] * 2;
                     }
                     super[§§constant(21)]();
                     if(this[§§constant(24)] == 0 && eval(§§constant(26))[§§constant(27)](this[§§constant(25)]) < 0.2)
                     {
                        if(this[§§constant(28)])
                        {
                           this[§§constant(29)] = true;
                        }
                        else if(this[§§constant(22)] && !this[§§constant(30)])
                        {
                           this[§§constant(30)] = true;
                           this[§§constant(31)]();
                           new eval("{invalid_utf8=139}I{invalid_utf8=217}\x12{invalid_utf8=240}")["\x13{invalid_utf8=232}d"]["8{invalid_utf8=183}\x13"][§§constant(35)](this[§§constant(34)],this[§§constant(33)],250,70,this[§§constant(32)]);
                           _root[§§constant(37)][§§constant(38)](§§constant(36));
                        }
                     }
                     if(!this[§§constant(28)] && this[§§constant(33)] > eval("{invalid_utf8=139}I{invalid_utf8=217}\x12{invalid_utf8=240}")["\x13{invalid_utf8=232}d"]["8{invalid_utf8=183}\x13"][§§constant(39)][§§constant(40)][§§constant(33)])
                     {
                        this[§§constant(15)][§§constant(42)](§§constant(41));
                     }
                  };
                  _loc2_[§§constant(43)] = function()
                  {
                     super[§§constant(43)]();
                     var _loc3_;
                     if(!this[§§constant(28)] && !this[§§constant(30)])
                     {
                        _loc3_ = new eval("{invalid_utf8=139}I{invalid_utf8=217}\x12{invalid_utf8=240}")["\x13{invalid_utf8=232}d"]["8{invalid_utf8=183}\x13"][§§constant(46)](eval("{invalid_utf8=139}I{invalid_utf8=217}\x12{invalid_utf8=240}")["\x13{invalid_utf8=232}d"]["8{invalid_utf8=183}\x13"][§§constant(39)][§§constant(45)],§§constant(44));
                        _loc3_[§§constant(34)] = this[§§constant(34)];
                        _loc3_[§§constant(33)] = this[§§constant(33)];
                        _loc3_[§§constant(11)]();
                     }
                  };
                  §§push(§§constant(47)(eval("{invalid_utf8=139}I{invalid_utf8=217}\x12{invalid_utf8=240}")["\x13{invalid_utf8=232}d"]["8{invalid_utf8=183}\x13"][§§constant(5)][§§constant(20)],null,1));
               }
               §§pop();
               break;
            }
            if(eval("\x01") == 889)
            {
               set("\x01",eval("\x01") - 443);
               §§push(true);
            }
            else if(eval("\x01") == 733)
            {
               set("\x01",eval("\x01") - 239);
            }
            else
            {
               if(eval("\x01") == 623)
               {
                  set("\x01",eval("\x01") + 236);
                  break;
               }
               if(eval("\x01") == 326)
               {
                  set("\x01",eval("\x01") + 297);
                  if(§§pop())
                  {
                     set("\x01",eval("\x01") + 236);
                  }
               }
               else if(eval("\x01") == 935)
               {
                  set("\x01",eval("\x01") - 115);
                  §§push("\x0f");
               }
               else if(eval("\x01") == 820)
               {
                  set("\x01",eval("\x01") - 168);
                  §§push(eval(§§pop()));
               }
               else if(eval("\x01") == 446)
               {
                  set("\x01",eval("\x01") + 153);
                  if(§§pop())
                  {
                     set("\x01",eval("\x01") - 547);
                  }
               }
               else
               {
                  if(eval("\x01") != 142)
                  {
                     if(eval("\x01") == 776)
                     {
                        set("\x01",eval("\x01") - 776);
                     }
                     break;
                  }
                  set("\x01",eval("\x01") + 731);
               }
            }
         }
      }
   }
}
