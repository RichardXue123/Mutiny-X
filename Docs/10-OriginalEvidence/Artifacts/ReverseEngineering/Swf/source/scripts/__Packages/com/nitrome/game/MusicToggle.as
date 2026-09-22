function §\x04\x05§()
{
   set("\x03",1998 % 511 * true);
   return eval("\x03");
}
var §\x01§ = -185 + "\x04\x05"();
var _loc2_;
while(true)
{
   if(eval("\x01") == 280)
   {
      set("\x01",eval("\x01") + 242);
      §§push(true);
   }
   else if(eval("\x01") == 76)
   {
      set("\x01",eval("\x01") + 248);
      §§push("\x0f");
      §§push(1);
   }
   else if(eval("\x01") == 226)
   {
      set("\x01",eval("\x01") - 150);
   }
   else if(eval("\x01") == 296)
   {
      set("\x01",eval("\x01") - 220);
   }
   else if(eval("\x01") == 54)
   {
      set("\x01",eval("\x01") + 205);
      §§push(eval(§§pop()));
   }
   else if(eval("\x01") == 607)
   {
      set("\x01",eval("\x01") + 86);
      §§push(true);
   }
   else if(eval("\x01") == 252)
   {
      set("\x01",eval("\x01") + 355);
   }
   else if(eval("\x01") == 680)
   {
      set("\x01",eval("\x01") + 207);
      if(§§pop())
      {
         set("\x01",eval("\x01") - 501);
      }
   }
   else
   {
      if(eval("\x01") == 525)
      {
         set("\x01",eval("\x01") - 299);
         §§pop() extends §§pop();
         §§pop() extends §§pop() ^ §§pop();
         §§pop() extends §§pop();
         break;
      }
      if(eval("\x01") == 887)
      {
         set("\x01",eval("\x01") - 501);
      }
      else if(eval("\x01") == 164)
      {
         set("\x01",eval("\x01") + 443);
      }
      else
      {
         if(eval("\x01") == 12)
         {
            set("\x01",eval("\x01") + 152);
            break;
         }
         if(eval("\x01") == 693)
         {
            set("\x01",eval("\x01") - 168);
            if(§§pop())
            {
               set("\x01",eval("\x01") - 299);
            }
         }
         else if(eval("\x01") == 324)
         {
            set("\x01",eval("\x01") + 289);
            var §§pop() = §§pop();
         }
         else if(eval("\x01") == 613)
         {
            set("\x01",eval("\x01") - 559);
            §§push("\x0f");
         }
         else if(eval("\x01") == 522)
         {
            set("\x01",eval("\x01") - 510);
            if(§§pop())
            {
               set("\x01",eval("\x01") + 152);
            }
         }
         else
         {
            if(eval("\x01") != 259)
            {
               if(eval("\x01") == 386)
               {
                  set("\x01",eval("\x01") - 145);
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
                  if(!_global.com.nitrome.game.MusicToggle)
                  {
                     com.nitrome.game.MusicToggle extends MovieClip;
                     _loc2_ = com.nitrome.game.MusicToggle = function()
                     {
                        super();
                        if(_root.mc.getMusicOn() == false)
                        {
                           this.gotoAndStop("_off_up");
                        }
                        else
                        {
                           this.gotoAndStop("_on_up");
                        }
                     }.prototype;
                     _loc2_.onRollOver = function()
                     {
                        this.updateGraphic(true);
                        _root.sfx_manager.playSound("rollover");
                     };
                     _loc2_.onRollOut = function()
                     {
                        this.updateGraphic(false);
                     };
                     _loc2_.onPress = function()
                     {
                        _root.mc.toggleMusic();
                        this.updateGraphic(true);
                     };
                     _loc2_.updateGraphic = function(mouse_is_over)
                     {
                        if(mouse_is_over == true)
                        {
                           if(_root.mc.getMusicOn() == true)
                           {
                              this.gotoAndStop("_on_over");
                           }
                           else if(_root.mc.getMusicOn() == false)
                           {
                              this.gotoAndStop("_off_over");
                           }
                        }
                        else if(mouse_is_over == false)
                        {
                           if(_root.mc.getMusicOn() == true)
                           {
                              this.gotoAndStop("_on_up");
                           }
                           else if(_root.mc.getMusicOn() == false)
                           {
                              this.gotoAndStop("_off_up");
                           }
                        }
                     };
                     §§push(ASSetPropFlags(com.nitrome.game.MusicToggle.prototype,null,1));
                  }
                  §§pop();
                  break;
               }
               if(eval("\x01") == 241)
               {
                  set("\x01",eval("\x01") - 241);
               }
               break;
            }
            set("\x01",eval("\x01") + 421);
            §§push(!§§pop());
         }
      }
   }
}
