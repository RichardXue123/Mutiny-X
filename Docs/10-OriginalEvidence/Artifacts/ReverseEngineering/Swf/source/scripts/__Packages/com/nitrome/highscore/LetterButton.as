function §\x04\x05§()
{
   set("\x03",1647 % 511 * true);
   return eval("\x03");
}
var §\x01§ = 452 + "\x04\x05"();
var _loc2_;
while(true)
{
   if(eval("\x01") == 566)
   {
      set("\x01",eval("\x01") - 72);
      §§push(true);
   }
   else if(eval("\x01") == 300)
   {
      set("\x01",eval("\x01") + 532);
      §§push("\x0f");
   }
   else if(eval("\x01") == 161)
   {
      set("\x01",eval("\x01") + 95);
      §§push(true);
   }
   else if(eval("\x01") == 494)
   {
      set("\x01",eval("\x01") + 166);
      if(§§pop())
      {
         set("\x01",eval("\x01") - 153);
      }
   }
   else if(eval("\x01") == 808)
   {
      set("\x01",eval("\x01") + 106);
   }
   else if(eval("\x01") == 914)
   {
      set("\x01",eval("\x01") - 785);
      §§push(true);
   }
   else
   {
      if(eval("\x01") == 660)
      {
         set("\x01",eval("\x01") - 153);
         break;
      }
      if(eval("\x01") == 927)
      {
         set("\x01",eval("\x01") - 13);
      }
      else if(eval("\x01") == 507)
      {
         set("\x01",eval("\x01") - 346);
      }
      else if(eval("\x01") == 129)
      {
         set("\x01",eval("\x01") + 335);
         if(§§pop())
         {
            set("\x01",eval("\x01") + 421);
         }
      }
      else if(eval("\x01") == 16)
      {
         set("\x01",eval("\x01") + 145);
      }
      else if(eval("\x01") == 256)
      {
         set("\x01",eval("\x01") + 392);
         if(§§pop())
         {
            set("\x01",eval("\x01") + 160);
         }
      }
      else
      {
         if(eval("\x01") == 648)
         {
            set("\x01",eval("\x01") + 160);
            break;
         }
         if(eval("\x01") == 464)
         {
            set("\x01",eval("\x01") + 421);
            set("\x01",eval("\x01") - 263);
            break;
         }
         if(eval("\x01") == 885)
         {
            set("\x01",eval("\x01") - 770);
         }
         else if(eval("\x01") == 307)
         {
            set("\x01",eval("\x01") - 192);
         }
         else if(eval("\x01") == 115)
         {
            set("\x01",eval("\x01") + 428);
            §§push("\x0f");
            §§push(1);
         }
         else if(eval("\x01") == 543)
         {
            set("\x01",eval("\x01") - 243);
            var §§pop() = §§pop();
         }
         else if(eval("\x01") == 832)
         {
            set("\x01",eval("\x01") - 129);
            §§push(eval(§§pop()));
         }
         else if(eval("\x01") == 703)
         {
            set("\x01",eval("\x01") + 161);
            §§push(!§§pop());
         }
         else if(eval("\x01") == 864)
         {
            set("\x01",eval("\x01") - 219);
            if(§§pop())
            {
               set("\x01",eval("\x01") - 168);
            }
         }
         else
         {
            if(eval("\x01") != 645)
            {
               if(eval("\x01") == 477)
               {
                  set("\x01",eval("\x01") + 223);
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
                  if(!_global.com.nitrome.highscore)
                  {
                     _global.com.nitrome.highscore = new Object();
                  }
                  §§pop();
                  if(!_global.com.nitrome.highscore.LetterButton)
                  {
                     com.nitrome.highscore.LetterButton extends MovieClip;
                     _loc2_ = com.nitrome.highscore.LetterButton = function()
                     {
                        super();
                        this.letter_text = this._name;
                     }.prototype;
                     _loc2_.onLoad = function()
                     {
                        this.letter_holder.letter.text = this._name;
                     };
                     _loc2_.onRollOver = function()
                     {
                        this.gotoAndStop("over");
                        this.letter_holder.letter.text = this._name;
                     };
                     _loc2_.onRollOut = function()
                     {
                        this.gotoAndStop("up");
                        this.letter_holder.letter.text = this._name;
                     };
                     _loc2_.onPress = function()
                     {
                        this._parent.addLetter(this.letter_text);
                     };
                     §§push(ASSetPropFlags(com.nitrome.highscore.LetterButton.prototype,null,1));
                  }
                  §§pop();
                  break;
               }
               if(eval("\x01") == 700)
               {
                  set("\x01",eval("\x01") - 700);
               }
               break;
            }
            set("\x01",eval("\x01") - 168);
         }
      }
   }
}
