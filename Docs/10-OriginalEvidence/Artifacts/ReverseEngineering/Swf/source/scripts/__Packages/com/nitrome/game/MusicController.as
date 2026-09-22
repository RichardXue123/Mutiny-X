function §\x04\x05§()
{
   set("\x03",59 % 511 * true);
   return eval("\x03");
}
var §\x01§ = 655 + "\x04\x05"();
var _loc2_;
while(true)
{
   if(eval("\x01") == 714)
   {
      set("\x01",eval("\x01") - 96);
      §§push(true);
   }
   else if(eval("\x01") == 21)
   {
      set("\x01",eval("\x01") + 888);
      §§push("\x0f");
   }
   else if(eval("\x01") == 176)
   {
      set("\x01",eval("\x01") + 716);
      §§push("\x0f");
      §§push(1);
   }
   else if(eval("\x01") == 118)
   {
      set("\x01",eval("\x01") + 58);
   }
   else if(eval("\x01") == 137)
   {
      set("\x01",eval("\x01") + 39);
   }
   else
   {
      if(eval("\x01") == 8)
      {
         set("\x01",eval("\x01") + 129);
         break;
      }
      if(eval("\x01") == 763)
      {
         set("\x01",eval("\x01") - 278);
         if(§§pop())
         {
            set("\x01",eval("\x01") - 364);
         }
      }
      else if(eval("\x01") == 618)
      {
         set("\x01",eval("\x01") - 610);
         if(§§pop())
         {
            set("\x01",eval("\x01") + 129);
         }
      }
      else if(eval("\x01") == 892)
      {
         set("\x01",eval("\x01") - 871);
         var §§pop() = §§pop();
      }
      else if(eval("\x01") == 157)
      {
         set("\x01",eval("\x01") + 606);
         §§push(!§§pop());
      }
      else if(eval("\x01") == 909)
      {
         set("\x01",eval("\x01") - 752);
         §§push(eval(§§pop()));
      }
      else
      {
         if(eval("\x01") == 121)
         {
            set("\x01",eval("\x01") + 385);
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
            if(!_global.com.nitrome.game.MusicController)
            {
               com.nitrome.game.MusicController extends MovieClip;
               _loc2_ = com.nitrome.game.MusicController = function()
               {
                  super();
                  this.menu_sound = new Sound(_root.menu_music_mc);
                  this.game_sound = new Sound(_root.game_music_mc);
                  this.menu_sound.attachSound("menu_music");
                  this.game_sound.attachSound("game_music");
                  this.music_on = _root.ng.getMusicOn();
                  this.sfx_on = _root.ng.getSfxOn();
               }.prototype;
               _loc2_.startMenuMusic = function(from_toggle)
               {
                  if(this.music_type != "menu" || from_toggle == true)
                  {
                     if(this.music_on == true)
                     {
                        this.menu_sound.onSoundComplete = function()
                        {
                           this.start();
                        };
                        this.game_sound.onSoundComplete = function()
                        {
                        };
                        this.game_sound.stop();
                        this.menu_sound.start();
                     }
                     this.music_type = "menu";
                  }
               };
               _loc2_.startGameMusic = function(from_toggle)
               {
                  if(this.music_type != "game" || from_toggle == true)
                  {
                     if(this.music_on == true)
                     {
                        this.game_sound.onSoundComplete = function()
                        {
                           this.start();
                        };
                        this.menu_sound.onSoundComplete = function()
                        {
                        };
                        this.menu_sound.stop();
                        this.game_sound.start();
                     }
                     this.music_type = "game";
                  }
               };
               _loc2_.getMusicOn = function()
               {
                  return this.music_on;
               };
               _loc2_.toggleMusic = function()
               {
                  if(this.music_on == true)
                  {
                     this.turnOffMusic();
                  }
                  else if(this.music_on == false)
                  {
                     this.turnOnMusic();
                  }
               };
               _loc2_.turnOnMusic = function()
               {
                  this.music_on = true;
                  if(this.music_type == "menu")
                  {
                     this.startMenuMusic(true);
                  }
                  else if(this.music_type == "game")
                  {
                     this.startGameMusic(true);
                  }
                  _root.ng.setMusicOn(true);
               };
               _loc2_.turnOffMusic = function()
               {
                  this.music_on = false;
                  this.menu_sound.stop();
                  this.game_sound.stop();
                  _root.ng.setMusicOn(false);
               };
               _loc2_.getSfxOn = function()
               {
                  return this.sfx_on;
               };
               _loc2_.toggleSfx = function()
               {
                  if(this.sfx_on == true)
                  {
                     this.turnOffSfx();
                  }
                  else if(this.sfx_on == false)
                  {
                     this.turnOnSfx();
                  }
               };
               _loc2_.turnOnSfx = function()
               {
                  this.sfx_on = true;
                  _root.ng.setSfxOn(true);
               };
               _loc2_.turnOffSfx = function()
               {
                  this.sfx_on = false;
                  _root.ng.setSfxOn(false);
               };
               _loc2_.music_on = true;
               _loc2_.sfx_on = true;
               §§push(ASSetPropFlags(com.nitrome.game.MusicController.prototype,null,1));
            }
            §§pop();
            break;
         }
         if(eval("\x01") != 485)
         {
            if(eval("\x01") == 506)
            {
               set("\x01",eval("\x01") - 506);
            }
            break;
         }
         set("\x01",eval("\x01") - 364);
      }
   }
}
