function §\x04\x05§()
{
   set("\x03",1518 % 511 * true);
   return eval("\x03");
}
var §\x01§ = -198 + "\x04\x05"();
var _loc2_;
while(true)
{
   if(eval("\x01") == 298)
   {
      set("\x01",eval("\x01") + 37);
      §§push(true);
   }
   else if(eval("\x01") == 335)
   {
      set("\x01",eval("\x01") + 216);
      if(§§pop())
      {
         set("\x01",eval("\x01") + 357);
      }
   }
   else if(eval("\x01") == 132)
   {
      set("\x01",eval("\x01") + 740);
      if(§§pop())
      {
         set("\x01",eval("\x01") - 414);
      }
   }
   else
   {
      if(eval("\x01") == 662)
      {
         set("\x01",eval("\x01") - 248);
         §§push(§§pop() instanceof §§pop());
         break;
      }
      if(eval("\x01") == 224)
      {
         set("\x01",eval("\x01") + 562);
         §§push(§§pop() + §§pop());
         break;
      }
      if(eval("\x01") == 79)
      {
         set("\x01",eval("\x01") + 228);
         §§push(true);
      }
      else if(eval("\x01") == 617)
      {
         set("\x01",eval("\x01") - 393);
         if(§§pop())
         {
            set("\x01",eval("\x01") + 562);
         }
      }
      else
      {
         if(eval("\x01") == 551)
         {
            set("\x01",eval("\x01") + 357);
            break;
         }
         if(eval("\x01") == 154)
         {
            set("\x01",eval("\x01") + 13);
            §§push(eval(§§pop()));
         }
         else
         {
            if(eval("\x01") == 136)
            {
               set("\x01",eval("\x01") + 631);
               break;
            }
            if(eval("\x01") == 43)
            {
               set("\x01",eval("\x01") + 777);
               §§push(true);
            }
            else if(eval("\x01") == 908)
            {
               set("\x01",eval("\x01") - 865);
            }
            else if(eval("\x01") == 538)
            {
               set("\x01",eval("\x01") - 384);
               §§push("\x0f");
            }
            else if(eval("\x01") == 820)
            {
               set("\x01",eval("\x01") - 158);
               if(§§pop())
               {
                  set("\x01",eval("\x01") - 248);
               }
            }
            else if(eval("\x01") == 167)
            {
               set("\x01",eval("\x01") - 35);
               §§push(!§§pop());
            }
            else if(eval("\x01") == 786)
            {
               set("\x01",eval("\x01") - 754);
            }
            else if(eval("\x01") == 753)
            {
               set("\x01",eval("\x01") - 710);
            }
            else if(eval("\x01") == 236)
            {
               set("\x01",eval("\x01") + 381);
               §§push(true);
            }
            else if(eval("\x01") == 414)
            {
               set("\x01",eval("\x01") - 335);
            }
            else if(eval("\x01") == 14)
            {
               set("\x01",eval("\x01") + 65);
            }
            else if(eval("\x01") == 307)
            {
               set("\x01",eval("\x01") - 171);
               if(§§pop())
               {
                  set("\x01",eval("\x01") + 631);
               }
            }
            else if(eval("\x01") == 767)
            {
               set("\x01",eval("\x01") - 531);
            }
            else if(eval("\x01") == 886)
            {
               set("\x01",eval("\x01") - 650);
            }
            else if(eval("\x01") == 324)
            {
               set("\x01",eval("\x01") + 214);
               var §§pop() = §§pop();
            }
            else if(eval("\x01") == 552)
            {
               set("\x01",eval("\x01") - 520);
            }
            else if(eval("\x01") == 32)
            {
               set("\x01",eval("\x01") + 292);
               §§push("\x0f");
               §§push(1);
            }
            else
            {
               if(eval("\x01") != 872)
               {
                  if(eval("\x01") == 458)
                  {
                     set("\x01",eval("\x01") + 209);
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
                     if(!_global.com.nitrome.game)
                     {
                        _global.com.nitrome.game = new Object();
                     }
                     §§pop();
                     if(!_global.com.nitrome.game.Preloader)
                     {
                        com.nitrome.game.Preloader extends MovieClip;
                        _loc2_ = com.nitrome.game.Preloader = function()
                        {
                           super();
                           this.load_bar._xscale = 1;
                           this.start_t = getTimer();
                           this.onEnterFrame = function()
                           {
                              var _loc6_ = _root.getBytesLoaded();
                              var _loc5_ = _root.getBytesTotal();
                              var _loc4_ = Math.round(_loc6_ / _loc5_ * 100);
                              var _loc7_ = getTimer();
                              var _loc3_ = Math.round((_loc7_ - this.start_t) / 250);
                              if(_loc3_ > 100)
                              {
                                 _loc3_ = 100;
                              }
                              if(_loc4_ < _loc3_)
                              {
                                 trace("percent:" + _loc4_);
                                 this.load_text.text = String(_loc4_);
                                 this.load_bar._xscale = _loc4_;
                              }
                              else
                              {
                                 trace("tt:" + _loc3_);
                                 this.load_text.text = String(_loc3_);
                                 this.load_bar._xscale = _loc3_;
                              }
                              trace(_loc3_);
                              trace(_loc6_ == _loc5_);
                              trace(_loc3_ >= 100);
                              if(_loc6_ == _loc5_ && _loc3_ >= 100)
                              {
                                 _root.tt.doTween("mtv");
                                 delete this.onEnterFrame;
                              }
                           };
                        }.prototype;
                        §§push(ASSetPropFlags(com.nitrome.game.Preloader.prototype,null,1));
                     }
                     §§pop();
                     break;
                  }
                  if(eval("\x01") == 667)
                  {
                     set("\x01",eval("\x01") - 667);
                  }
                  break;
               }
               set("\x01",eval("\x01") - 414);
            }
         }
      }
   }
}
