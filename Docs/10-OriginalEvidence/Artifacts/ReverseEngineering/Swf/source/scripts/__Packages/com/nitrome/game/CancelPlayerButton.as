function §\x04\x05§()
{
   set("\x03",2379 % 511 * true);
   return eval("\x03");
}
var §\x01§ = 275 + "\x04\x05"();
var _loc2_;
while(true)
{
   if(eval("\x01") == 610)
   {
      set("\x01",eval("\x01") - 523);
      §§push(true);
   }
   else
   {
      if(eval("\x01") == 700)
      {
         set("\x01",eval("\x01") - 102);
         break;
      }
      if(eval("\x01") == 285)
      {
         set("\x01",eval("\x01") + 661);
         §§push(true);
      }
      else if(eval("\x01") == 680)
      {
         set("\x01",eval("\x01") + 145);
         §§push(true);
      }
      else if(eval("\x01") == 452)
      {
         set("\x01",eval("\x01") + 46);
         var §§pop() = §§pop();
      }
      else if(eval("\x01") == 87)
      {
         set("\x01",eval("\x01") + 513);
         if(§§pop())
         {
            set("\x01",eval("\x01") + 127);
         }
      }
      else if(eval("\x01") == 710)
      {
         set("\x01",eval("\x01") - 425);
      }
      else if(eval("\x01") == 488)
      {
         set("\x01",eval("\x01") - 203);
      }
      else if(eval("\x01") == 462)
      {
         set("\x01",eval("\x01") - 10);
         §§push("\x0f");
         §§push(1);
      }
      else if(eval("\x01") == 394)
      {
         set("\x01",eval("\x01") + 43);
         if(§§pop())
         {
            set("\x01",eval("\x01") + 474);
         }
      }
      else if(eval("\x01") == 386)
      {
         set("\x01",eval("\x01") + 8);
         §§push(!§§pop());
      }
      else
      {
         if(eval("\x01") == 600)
         {
            set("\x01",eval("\x01") + 127);
            if(!§§pop()[§§pop()][§§constant(4)][§§constant(5)])
            {
               eval("r]Vt")[§§constant(3)][§§constant(4)][§§constant(5)] extends eval("r]Vt")[§§constant(3)][§§constant(4)][§§constant(6)];
               _loc2_ = eval("r]Vt")[§§constant(3)][§§constant(4)][§§constant(5)] = function()
               {
                  super();
               }[§§constant(7)];
               _loc2_[§§constant(8)] = function()
               {
                  _root[§§constant(10)][§§constant(11)](§§constant(9));
               };
               §§push(§§constant(12)(eval("r]Vt")[§§constant(3)][§§constant(4)][§§constant(5)][§§constant(7)],null,1));
            }
            §§pop();
            break;
         }
         if(eval("\x01") == 598)
         {
            set("\x01",eval("\x01") - 136);
         }
         else if(eval("\x01") == 946)
         {
            set("\x01",eval("\x01") - 246);
            if(§§pop())
            {
               set("\x01",eval("\x01") - 102);
            }
         }
         else if(eval("\x01") == 24)
         {
            set("\x01",eval("\x01") + 362);
            §§push(eval(§§pop()));
         }
         else
         {
            if(eval("\x01") == 148)
            {
               set("\x01",eval("\x01") + 340);
               break;
            }
            if(eval("\x01") == 727)
            {
               set("\x01",eval("\x01") - 47);
            }
            else if(eval("\x01") == 919)
            {
               set("\x01",eval("\x01") - 239);
            }
            else if(eval("\x01") == 392)
            {
               set("\x01",eval("\x01") + 70);
            }
            else if(eval("\x01") == 498)
            {
               set("\x01",eval("\x01") - 474);
               §§push("\x0f");
            }
            else if(eval("\x01") == 825)
            {
               set("\x01",eval("\x01") - 677);
               if(§§pop())
               {
                  set("\x01",eval("\x01") + 340);
               }
            }
            else
            {
               if(eval("\x01") != 437)
               {
                  if(eval("\x01") == 911)
                  {
                     set("\x01",eval("\x01") - 822);
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
                     if(!_global.com.nitrome.game.CancelPlayerButton)
                     {
                        com.nitrome.game.CancelPlayerButton extends com.nitrome.game.SimpleButton;
                        _loc2_ = com.nitrome.game.CancelPlayerButton = function()
                        {
                           super();
                        }.prototype;
                        _loc2_.onPress = function()
                        {
                           if(com.nitrome.throwgame.Controller.currentTeam.selectedCharacter)
                           {
                              com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.unequip();
                              if(com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.canThrow)
                              {
                                 com.nitrome.throwgame.Controller.currentTeam.selectedCharacter = null;
                              }
                           }
                        };
                        _loc2_.onEnterFrame = function()
                        {
                           this._visible = com.nitrome.throwgame.Controller.currentTeam.selectedCharacter.canThrow;
                        };
                        _loc2_.onRollOver = function()
                        {
                           com.nitrome.game.WeaponSelectPanel(this._parent).title.text = "close";
                           com.nitrome.game.WeaponSelectPanel(this._parent).description.text = "click here to cancel and select|another player.";
                        };
                        _loc2_.onRollOut = function()
                        {
                           com.nitrome.game.WeaponSelectPanel(this._parent).title.text = "weapons";
                           com.nitrome.game.WeaponSelectPanel(this._parent).description.text = "Click one of the options above|to select it.";
                        };
                        §§push(ASSetPropFlags(com.nitrome.game.CancelPlayerButton.prototype,null,1));
                     }
                     §§pop();
                     break;
                  }
                  if(eval("\x01") == 89)
                  {
                     set("\x01",eval("\x01") - 89);
                  }
                  break;
               }
               set("\x01",eval("\x01") + 474);
            }
         }
      }
   }
}
