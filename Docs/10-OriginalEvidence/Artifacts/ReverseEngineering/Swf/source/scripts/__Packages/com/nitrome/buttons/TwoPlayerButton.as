function §\x04\x05§()
{
   set("\x03",1687 % 511 * true);
   return eval("\x03");
}
var §\x01§ = 445 + "\x04\x05"();
var _loc2_;
loop0:
while(true)
{
   if(eval("\x01") != 599)
   {
      while(true)
      {
         if(eval("\x01") == 870)
         {
            set("\x01",eval("\x01") + 92);
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
            if(_global.com.nitrome.buttons)
            {
               break;
            }
         }
         else
         {
            if(eval("\x01") == 102)
            {
               set("\x01",eval("\x01") + 768);
               continue loop0;
            }
            if(eval("\x01") == 896)
            {
               set("\x01",eval("\x01") - 142);
               §§push(true);
               continue loop0;
            }
            if(eval("\x01") == 856)
            {
               set("\x01",eval("\x01") - 136);
               §§push(eval(§§pop()));
               continue loop0;
            }
            if(eval("\x01") == 360)
            {
               set("\x01",eval("\x01") + 511);
               continue loop0;
            }
            if(eval("\x01") == 689)
            {
               set("\x01",eval("\x01") - 484);
               set(§§pop(),eval(§§pop()) - 407);
               return;
            }
            if(eval("\x01") == 695)
            {
               set("\x01",eval("\x01") + 99);
               continue loop0;
            }
            if(eval("\x01") == 524)
            {
               set("\x01",eval("\x01") + 332);
               §§push("\x0f");
               continue loop0;
            }
            if(eval("\x01") == 794)
            {
               set("\x01",eval("\x01") + 132);
               §§push("\x0f");
               §§push(1);
               continue loop0;
            }
            if(eval("\x01") != 750)
            {
               if(eval("\x01") == 754)
               {
                  set("\x01",eval("\x01") - 4);
                  if(§§pop())
                  {
                     set("\x01",eval("\x01") - 55);
                  }
               }
               else if(eval("\x01") == 289)
               {
                  set("\x01",eval("\x01") + 400);
                  if(§§pop())
                  {
                     set("\x01",eval("\x01") - 484);
                  }
               }
               else
               {
                  if(eval("\x01") == 432)
                  {
                     set("\x01",eval("\x01") - 72);
                     return;
                  }
                  if(eval("\x01") == 514)
                  {
                     set("\x01",eval("\x01") + 280);
                  }
                  else
                  {
                     if(eval("\x01") == 962)
                     {
                        set("\x01",eval("\x01") - 962);
                        return;
                     }
                     if(eval("\x01") == 720)
                     {
                        set("\x01",eval("\x01") + 25);
                        §§push(!§§pop());
                     }
                     else if(eval("\x01") == 745)
                     {
                        set("\x01",eval("\x01") - 643);
                        if(§§pop())
                        {
                           set("\x01",eval("\x01") + 768);
                        }
                     }
                     else if(eval("\x01") == 232)
                     {
                        set("\x01",eval("\x01") + 200);
                        if(§§pop())
                        {
                           set("\x01",eval("\x01") - 72);
                        }
                     }
                     else if(eval("\x01") == 926)
                     {
                        set("\x01",eval("\x01") - 402);
                        var §§pop() = §§pop();
                     }
                     else if(eval("\x01") == 464)
                     {
                        set("\x01",eval("\x01") + 432);
                     }
                     else if(eval("\x01") == 205)
                     {
                        set("\x01",eval("\x01") + 691);
                     }
                     else if(eval("\x01") == 871)
                     {
                        set("\x01",eval("\x01") - 582);
                        §§push(true);
                     }
                     else
                     {
                        if(eval("\x01") != 666)
                        {
                           return;
                        }
                        set("\x01",eval("\x01") + 205);
                     }
                  }
               }
               continue loop0;
            }
            set("\x01",eval("\x01") - 55);
         }
         _global.com.nitrome.buttons = new Object();
         break;
      }
      §§pop();
      if(!_global.com.nitrome.buttons.TwoPlayerButton)
      {
         com.nitrome.buttons.TwoPlayerButton extends com.nitrome.buttons.SimpleButton;
         _loc2_ = com.nitrome.buttons.TwoPlayerButton = function()
         {
            super();
         }.prototype;
         _loc2_.onPress = function()
         {
            _root.game_type = 2;
            _root.won_p1 = 0;
            _root.won_p2 = 0;
            _root.tt.doTween("level_select_2p");
         };
         §§push(ASSetPropFlags(com.nitrome.buttons.TwoPlayerButton.prototype,null,1));
      }
      §§pop();
      break;
   }
   set("\x01",eval("\x01") - 367);
   §§push(true);
}
