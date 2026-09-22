function §\x04\x05§()
{
   set("\x03",1659 % 511 * true);
   return eval("\x03");
}
var §\x01§ = 824 + "\x04\x05"();
var _loc2_;
while(true)
{
   if(eval("\x01") == 950)
   {
      set("\x01",eval("\x01") - 487);
      §§push(true);
   }
   else if(eval("\x01") == 146)
   {
      set("\x01",eval("\x01") + 797);
      §§push("\x0f");
      §§push(1);
   }
   else if(eval("\x01") == 435)
   {
      set("\x01",eval("\x01") + 428);
      if(§§pop())
      {
         set("\x01",eval("\x01") - 582);
      }
   }
   else if(eval("\x01") == 281)
   {
      set("\x01",eval("\x01") - 135);
   }
   else if(eval("\x01") == 10)
   {
      set("\x01",eval("\x01") + 776);
   }
   else if(eval("\x01") == 744)
   {
      set("\x01",eval("\x01") - 598);
   }
   else
   {
      if(eval("\x01") == 863)
      {
         set("\x01",eval("\x01") - 582);
         var _temp_1 = §§pop() > §§pop();
         stop();
         §§push(§§pop() == _temp_1);
         break;
      }
      if(eval("\x01") == 920)
      {
         set("\x01",eval("\x01") - 652);
         §§push("\x0f");
      }
      else
      {
         if(eval("\x01") == 611)
         {
            set("\x01",eval("\x01") - 601);
            §§push(§§pop() eq §§pop());
            break;
         }
         if(eval("\x01") == 343)
         {
            set("\x01",eval("\x01") + 443);
         }
         else if(eval("\x01") == 15)
         {
            set("\x01",eval("\x01") + 528);
            §§push(!§§pop());
         }
         else if(eval("\x01") == 543)
         {
            set("\x01",eval("\x01") - 331);
            if(§§pop())
            {
               set("\x01",eval("\x01") + 37);
            }
         }
         else if(eval("\x01") == 324)
         {
            set("\x01",eval("\x01") + 663);
            §§push(true);
         }
         else if(eval("\x01") == 943)
         {
            set("\x01",eval("\x01") - 23);
            var §§pop() = §§pop();
         }
         else if(eval("\x01") == 786)
         {
            set("\x01",eval("\x01") - 351);
            §§push(true);
         }
         else if(eval("\x01") == 268)
         {
            set("\x01",eval("\x01") - 253);
            §§push(eval(§§pop()));
         }
         else if(eval("\x01") == 135)
         {
            set("\x01",eval("\x01") + 189);
         }
         else if(eval("\x01") == 212)
         {
            set("\x01",eval("\x01") + 37);
         }
         else if(eval("\x01") == 859)
         {
            set("\x01",eval("\x01") - 535);
         }
         else if(eval("\x01") == 987)
         {
            set("\x01",eval("\x01") - 376);
            if(§§pop())
            {
               set("\x01",eval("\x01") - 601);
            }
         }
         else
         {
            if(eval("\x01") == 249)
            {
               set("\x01",eval("\x01") + 637);
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
               if(!_global.com.nitrome.buttons)
               {
                  _global.com.nitrome.buttons = new Object();
               }
               §§pop();
               if(!_global.com.nitrome.buttons.QuitGameButton)
               {
                  com.nitrome.buttons.QuitGameButton extends com.nitrome.buttons.SimpleButton;
                  _loc2_ = com.nitrome.buttons.QuitGameButton = function()
                  {
                     super();
                  }.prototype;
                  _loc2_.onRelease = function()
                  {
                     com.nitrome.game.TransitionTween(_root.tt).doTweenWithFunction(function()
                     {
                        com.nitrome.throwgame.Controller.endGame();
                        if(_root.game_type == 2)
                        {
                           _root.gotoAndStop("level_select_2p");
                        }
                        else
                        {
                           _root.gotoAndStop("level_select_1p");
                        }
                     }
                     );
                  };
                  §§push(ASSetPropFlags(com.nitrome.buttons.QuitGameButton.prototype,null,1));
               }
               §§pop();
               break;
            }
            if(eval("\x01") == 886)
            {
               set("\x01",eval("\x01") - 886);
               break;
            }
            if(eval("\x01") == 240)
            {
               set("\x01",eval("\x01") + 619);
               §§push(§§pop() >>> (§§pop() | §§pop()));
               break;
            }
            if(eval("\x01") != 463)
            {
               break;
            }
            set("\x01",eval("\x01") - 223);
            if(§§pop())
            {
               set("\x01",eval("\x01") + 619);
            }
         }
      }
   }
}
