function §\x04\x05§()
{
   set("\x03",509 % 511 * true);
   return eval("\x03");
}
var §\x01§ = -192 + "\x04\x05"();
var _loc2_;
while(true)
{
   if(eval("\x01") == 317)
   {
      set("\x01",eval("\x01") - 39);
      §§push(true);
   }
   else if(eval("\x01") == 588)
   {
      set("\x01",eval("\x01") - 5);
      §§push(!§§pop());
   }
   else if(eval("\x01") == 693)
   {
      set("\x01",eval("\x01") - 76);
   }
   else
   {
      if(eval("\x01") == 536)
      {
         set("\x01",eval("\x01") + 157);
         break;
      }
      if(eval("\x01") == 81)
      {
         set("\x01",eval("\x01") - 62);
         §§push(true);
      }
      else if(eval("\x01") == 19)
      {
         set("\x01",eval("\x01") + 517);
         if(§§pop())
         {
            set("\x01",eval("\x01") + 157);
         }
      }
      else if(eval("\x01") == 278)
      {
         set("\x01",eval("\x01") + 62);
         if(§§pop())
         {
            set("\x01",eval("\x01") + 250);
         }
      }
      else if(eval("\x01") == 45)
      {
         set("\x01",eval("\x01") + 36);
      }
      else if(eval("\x01") == 590)
      {
         set("\x01",eval("\x01") - 509);
      }
      else if(eval("\x01") == 743)
      {
         set("\x01",eval("\x01") - 126);
      }
      else if(eval("\x01") == 617)
      {
         set("\x01",eval("\x01") + 231);
         §§push("\x0f");
         §§push(1);
      }
      else
      {
         if(eval("\x01") == 340)
         {
            set("\x01",eval("\x01") + 250);
            §§push(§§pop() >>> §§pop());
            break;
         }
         if(eval("\x01") == 848)
         {
            set("\x01",eval("\x01") - 620);
            var §§pop() = §§pop();
         }
         else if(eval("\x01") == 228)
         {
            set("\x01",eval("\x01") - 75);
            §§push("\x0f");
         }
         else if(eval("\x01") == 153)
         {
            set("\x01",eval("\x01") + 435);
            §§push(eval(§§pop()));
         }
         else
         {
            if(eval("\x01") == 405)
            {
               set("\x01",eval("\x01") - 268);
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
               if(!_global.com.nitrome.throwgame.Tile)
               {
                  com.nitrome.throwgame.Tile extends com.nitrome.util.Clip;
                  _loc2_ = com.nitrome.throwgame.Tile = function(type, gridX, gridY)
                  {
                     super(com.nitrome.throwgame.Controller.tileLayer);
                     this.gridX = gridX;
                     this.gridY = gridY;
                     this.x = gridX * 32;
                     this.y = gridY * 32;
                     this.link(type);
                     this.hide();
                  }.prototype;
                  _loc2_.show = function()
                  {
                     super.show();
                     if(this.mc._totalframes > 1)
                     {
                        this.mc.gotoAndPlay(1 + com.nitrome.throwgame.Controller.tileSystem.animationCounter % this.mc._totalframes);
                     }
                  };
                  _loc2_.gridX = 0;
                  _loc2_.gridY = 0;
                  §§push(ASSetPropFlags(com.nitrome.throwgame.Tile.prototype,null,1));
               }
               §§pop();
               break;
            }
            if(eval("\x01") == 583)
            {
               set("\x01",eval("\x01") + 47);
               if(§§pop())
               {
                  set("\x01",eval("\x01") - 225);
               }
            }
            else
            {
               if(eval("\x01") != 630)
               {
                  if(eval("\x01") == 137)
                  {
                     set("\x01",eval("\x01") - 137);
                  }
                  break;
               }
               set("\x01",eval("\x01") - 225);
            }
         }
      }
   }
}
