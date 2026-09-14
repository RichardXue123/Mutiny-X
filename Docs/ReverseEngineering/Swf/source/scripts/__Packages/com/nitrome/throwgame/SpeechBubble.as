function §\x04\x05§()
{
   set("\x03",587 % 511 * true);
   return eval("\x03");
}
var §\x01§ = -66 + "\x04\x05"();
var _loc2_;
while(true)
{
   if(eval("\x01") == 10)
   {
      set("\x01",eval("\x01") + 579);
      §§push(true);
   }
   else if(eval("\x01") == 695)
   {
      set("\x01",eval("\x01") - 596);
   }
   else
   {
      if(eval("\x01") != 589)
      {
         if(eval("\x01") == 801)
         {
            set("\x01",eval("\x01") - 106);
            §§pop()[§§pop()] = §§pop() + this[§§constant(18)][§§constant(20)](this[§§constant(15)][§§constant(16)][§§constant(17)][§§constant(18)][§§constant(19)],3);
            this[§§constant(21)] = 0;
            addr029d:
            §§pop()[§§pop()] = §§pop();
            _loc2_[§§constant(22)] = function(newTarget)
            {
               _root[§§constant(25)][§§constant(26)](newTarget[§§constant(23)][§§constant(24)]());
               this[§§constant(27)] = newTarget;
               this[§§constant(28)] = this[§§constant(27)][§§constant(28)];
               this[§§constant(29)] = this[§§constant(27)][§§constant(29)] - 90;
               this[§§constant(30)]();
            };
            _loc2_[§§constant(9)] = 0;
            _loc2_[§§constant(21)] = 0;
            §§push(§§constant(31)(eval(§§constant(1))[§§constant(3)][§§constant(4)][§§constant(5)][§§constant(12)],null,1));
         }
         else
         {
            if(eval("\x01") == 659)
            {
               set("\x01",eval("\x01") - 516);
               §§push(!§§pop());
               continue;
            }
            if(eval("\x01") == 2)
            {
               set("\x01",eval("\x01") + 597);
               var §§pop() = §§pop();
               continue;
            }
            if(eval("\x01") == 931)
            {
               set("\x01",eval("\x01") - 832);
               continue;
            }
            if(eval("\x01") != 290)
            {
               if(eval("\x01") == 776)
               {
                  set("\x01",eval("\x01") - 486);
               }
               else if(eval("\x01") == 99)
               {
                  set("\x01",eval("\x01") - 97);
                  §§push("\x0f");
                  §§push(1);
               }
               else if(eval("\x01") == 599)
               {
                  set("\x01",eval("\x01") - 452);
                  §§push("\x0f");
               }
               else if(eval("\x01") == 143)
               {
                  set("\x01",eval("\x01") + 633);
                  if(§§pop())
                  {
                     set("\x01",eval("\x01") - 486);
                  }
               }
               else
               {
                  if(eval("\x01") == 168)
                  {
                     set("\x01",eval("\x01") - 168);
                     break;
                  }
                  if(eval("\x01") != 147)
                  {
                     break;
                  }
                  set("\x01",eval("\x01") + 512);
                  §§push(eval(§§pop()));
               }
               continue;
            }
            set("\x01",eval("\x01") - 122);
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
            if(!_global.com.nitrome.throwgame.SpeechBubble)
            {
               com.nitrome.throwgame.SpeechBubble extends com.nitrome.util.Clip;
               _loc2_ = com.nitrome.throwgame.SpeechBubble = function()
               {
                  super(com.nitrome.throwgame.Controller.characterLayer,"speechBubble");
                  this.startTime = 10;
               }.prototype;
               §§goto(addr029d);
               §§push(_loc2_);
               §§push("advance");
               function()
               {
                  if(this.startTime > 0)
                  {
                     this.startTime = this.startTime - 1;
                     if(this.startTime < 1)
                     {
                        this.show();
                     }
                     return undefined;
                  }
                  if(this.mc.textHolder.textField.text.length < this.text.length)
                  {
                     this.mc.textHolder.textField.text += this[§§constant(18)][§§constant(20)](this[§§constant(15)][§§constant(16)][§§constant(17)][§§constant(18)][§§constant(19)],3);
                     this[§§constant(21)] = 0;
                  }
                  else
                  {
                     this.completeTime = this.completeTime + 1;
                  }
               }
            }
         }
         §§pop();
         break;
      }
      set("\x01",eval("\x01") + 212);
      if(§§pop())
      {
         set("\x01",eval("\x01") - 106);
      }
   }
}
