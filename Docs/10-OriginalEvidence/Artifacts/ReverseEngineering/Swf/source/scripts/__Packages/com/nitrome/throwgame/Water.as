function §\x04\x05§()
{
   set("\x03",440 % 511 * true);
   return eval("\x03");
}
var §\x01§ = 269 + "\x04\x05"();
var _loc2_;
while(true)
{
   if(eval("\x01") == 709)
   {
      set("\x01",eval("\x01") + 232);
      §§push(true);
   }
   else if(eval("\x01") == 941)
   {
      set("\x01",eval("\x01") - 849);
      if(§§pop())
      {
         set("\x01",eval("\x01") + 529);
      }
   }
   else if(eval("\x01") == 661)
   {
      set("\x01",eval("\x01") - 283);
      if(§§pop())
      {
         set("\x01",eval("\x01") + 532);
      }
   }
   else if(eval("\x01") == 91)
   {
      set("\x01",eval("\x01") + 266);
      if(§§pop())
      {
         set("\x01",eval("\x01") + 227);
      }
   }
   else if(eval("\x01") == 90)
   {
      set("\x01",eval("\x01") + 1);
      §§push(!§§pop());
   }
   else if(eval("\x01") == 357)
   {
      set("\x01",eval("\x01") + 227);
   }
   else if(eval("\x01") == 259)
   {
      set("\x01",eval("\x01") - 169);
      §§push(eval(§§pop()));
   }
   else
   {
      if(eval("\x01") == 584)
      {
         set("\x01",eval("\x01") - 444);
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
         if(!_global.com.nitrome.throwgame.Water)
         {
            com.nitrome.throwgame.Water extends com.nitrome.util.Clip;
            _loc2_ = com.nitrome.throwgame.Water = function()
            {
               super(com.nitrome.throwgame.Controller.waterLayer,"water");
               this.show();
               this.mc.gotoAndStop("water" + com.nitrome.throwgame.Controller.skyColour.toString());
            }.prototype;
            _loc2_.advance = function()
            {
               this.x = -49 * Math.ceil(com.nitrome.throwgame.Controller.content._x / 49);
               this.update();
               com.nitrome.throwgame.Controller.root.background.waterBackground._y = this.y + com.nitrome.throwgame.Controller.content._y;
               var _loc2_ = this.y + com.nitrome.throwgame.Controller.content._y - 350;
               com.nitrome.throwgame.Controller.root.background.cloudBase._y = Math.floor(_loc2_ * 0.5);
               com.nitrome.throwgame.Controller.root.background.hills._y = Math.floor(_loc2_ * 0.3 + 45);
               com.nitrome.throwgame.Controller.root.background.frontClouds._y = Math.floor(_loc2_ * 0.4 - 30);
               com.nitrome.throwgame.Controller.root.background.backClouds._y = Math.floor(_loc2_ * 0.2 - 30);
               _loc2_ = com.nitrome.throwgame.Controller.content._x;
               com.nitrome.throwgame.Controller.root.background.cloudBase._x = Math.floor(com.nitrome.util.Global.negativeModulo(_loc2_ * 0.3,550));
               com.nitrome.throwgame.Controller.root.background.hills._x = Math.floor(com.nitrome.util.Global.negativeModulo(_loc2_ * 0.25,840));
               com.nitrome.throwgame.Controller.root.background.frontClouds._x = Math.floor(com.nitrome.util.Global.negativeModulo(_loc2_ * 0.3,1000));
               com.nitrome.throwgame.Controller.root.background.backClouds._x = Math.floor(com.nitrome.util.Global.negativeModulo(_loc2_ * 0.2,900)) - 450;
               this.mc._visible = com.nitrome.throwgame.Controller.tileSystem.cameraY > this.y - 420;
            };
            §§push(ASSetPropFlags(com.nitrome.throwgame.Water.prototype,null,1));
         }
         §§pop();
         break;
      }
      if(eval("\x01") == 92)
      {
         set("\x01",eval("\x01") + 529);
         §§push(§§pop() >>> (§§pop() > (§§pop() | (§§pop() | §§pop()))));
         break;
      }
      if(eval("\x01") == 803)
      {
         set("\x01",eval("\x01") - 142);
         §§push(true);
      }
      else if(eval("\x01") == 412)
      {
         set("\x01",eval("\x01") + 391);
      }
      else
      {
         if(eval("\x01") == 378)
         {
            set("\x01",eval("\x01") + 532);
            stop();
            break;
         }
         if(eval("\x01") == 744)
         {
            set("\x01",eval("\x01") - 485);
            §§push("\x0f");
         }
         else if(eval("\x01") == 910)
         {
            set("\x01",eval("\x01") - 228);
         }
         else if(eval("\x01") == 621)
         {
            set("\x01",eval("\x01") + 182);
         }
         else
         {
            if(eval("\x01") == 140)
            {
               set("\x01",eval("\x01") - 140);
               break;
            }
            if(eval("\x01") == 652)
            {
               set("\x01",eval("\x01") + 30);
            }
            else if(eval("\x01") == 682)
            {
               set("\x01",eval("\x01") - 417);
               §§push("\x0f");
               §§push(1);
            }
            else
            {
               if(eval("\x01") != 265)
               {
                  break;
               }
               set("\x01",eval("\x01") + 479);
               var §§pop() = §§pop();
            }
         }
      }
   }
}
