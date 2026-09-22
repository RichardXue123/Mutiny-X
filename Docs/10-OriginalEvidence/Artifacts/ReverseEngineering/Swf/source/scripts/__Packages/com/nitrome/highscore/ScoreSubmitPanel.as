function §\x04\x05§()
{
   set("\x03",289 % 511 * true);
   return eval("\x03");
}
var §\x01§ = 183 + "\x04\x05"();
var _loc4_;
var _loc2_;
while(true)
{
   if(eval("\x01") == 472)
   {
      set("\x01",eval("\x01") - 364);
      §§push(true);
   }
   else if(eval("\x01") == 94)
   {
      set("\x01",eval("\x01") + 644);
   }
   else if(eval("\x01") == 828)
   {
      set("\x01",eval("\x01") - 383);
   }
   else if(eval("\x01") == 522)
   {
      set("\x01",eval("\x01") - 77);
   }
   else if(eval("\x01") == 642)
   {
      set("\x01",eval("\x01") - 76);
      if(§§pop())
      {
         set("\x01",eval("\x01") - 44);
      }
   }
   else if(eval("\x01") == 397)
   {
      set("\x01",eval("\x01") + 427);
      §§push("\x0f");
   }
   else
   {
      if(eval("\x01") != 445)
      {
         if(eval("\x01") == 566)
         {
            set("\x01",eval("\x01") - 44);
            _loc4_ = §§pop();
            this[§§constant(16)][§§constant(10)] = _loc4_;
            _root[§§constant(19)] = this[§§constant(16)][§§constant(10)][§§constant(20)]();
            this[§§constant(21)][§§constant(22)]();
            addr0210:
            §§pop()[§§pop()] = §§pop();
            _loc2_[§§constant(23)] = function()
            {
               return String(this[§§constant(16)][§§constant(10)]);
            };
            _loc2_[§§constant(24)] = function()
            {
               this[§§constant(21)][§§constant(25)]();
               this[§§constant(16)][§§constant(10)] = §§constant(26);
               _root[§§constant(19)] = this[§§constant(16)][§§constant(10)];
            };
            _loc2_[§§constant(18)] = 10;
            §§push(§§constant(27)(eval("<{invalid_utf8=158}")["\x05{invalid_utf8=160}wf"][§§constant(4)][§§constant(5)][§§constant(7)],null,1));
         }
         else
         {
            if(eval("\x01") == 558)
            {
               set("\x01",eval("\x01") - 161);
               var §§pop() = §§pop();
               continue;
            }
            if(eval("\x01") == 824)
            {
               set("\x01",eval("\x01") - 12);
               §§push(eval(§§pop()));
               continue;
            }
            if(eval("\x01") == 701)
            {
               set("\x01",eval("\x01") - 607);
               §§push(§§pop() / §§pop());
               break;
            }
            if(eval("\x01") == 261)
            {
               set("\x01",eval("\x01") + 477);
               continue;
            }
            if(eval("\x01") == 812)
            {
               set("\x01",eval("\x01") - 153);
               §§push(!§§pop());
               continue;
            }
            if(eval("\x01") == 738)
            {
               set("\x01",eval("\x01") - 96);
               §§push(true);
               continue;
            }
            if(eval("\x01") == 659)
            {
               set("\x01",eval("\x01") + 287);
               if(§§pop())
               {
                  set("\x01",eval("\x01") - 589);
               }
               continue;
            }
            if(eval("\x01") == 108)
            {
               set("\x01",eval("\x01") + 593);
               if(§§pop())
               {
                  set("\x01",eval("\x01") - 607);
               }
               continue;
            }
            if(eval("\x01") == 946)
            {
               set("\x01",eval("\x01") - 589);
               continue;
            }
            if(eval("\x01") != 357)
            {
               if(eval("\x01") == 264)
               {
                  set("\x01",eval("\x01") - 264);
               }
               break;
            }
            set("\x01",eval("\x01") - 93);
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
            if(!_global.com.nitrome.highscore.ScoreSubmitPanel)
            {
               com.nitrome.highscore.ScoreSubmitPanel extends MovieClip;
               _loc2_ = com.nitrome.highscore.ScoreSubmitPanel = function()
               {
                  super();
               }.prototype;
               _loc2_.onLoad = function()
               {
                  this.score_text.text = String("YOUR SCORE IS " + _root.score);
                  Key.addListener(this);
               };
               §§goto(addr0210);
               §§push(_loc2_);
               §§push("addLetter");
               function(l)
               {
                  var _loc3_ = this.name_text.text;
                  var _loc4_;
                  if(_loc3_.length < this.MAX_LENGTH)
                  {
                     _loc4_ = _loc3_ + l;
                     this[§§constant(16)][§§constant(10)] = _loc4_;
                     _root[§§constant(19)] = this[§§constant(16)][§§constant(10)][§§constant(20)]();
                     this[§§constant(21)][§§constant(22)]();
                  }
               }
            }
         }
         §§pop();
         break;
      }
      set("\x01",eval("\x01") + 113);
      §§push("\x0f");
      §§push(1);
   }
}
