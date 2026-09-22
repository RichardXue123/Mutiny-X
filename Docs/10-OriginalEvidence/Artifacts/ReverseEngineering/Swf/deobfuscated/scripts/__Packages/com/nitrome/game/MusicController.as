var §\x01§ = 506;
var §\x0f§ = 1;
class com.nitrome.game.MusicController extends MovieClip
{
   var game_sound;
   var menu_sound;
   var music_type;
   var start;
   var music_on = true;
   var sfx_on = true;
   function MusicController()
   {
      super();
      this.menu_sound = new Sound(_root.menu_music_mc);
      this.game_sound = new Sound(_root.game_music_mc);
      this.menu_sound.attachSound("menu_music");
      this.game_sound.attachSound("game_music");
      this.music_on = _root.ng.getMusicOn();
      this.sfx_on = _root.ng.getSfxOn();
   }
   function startMenuMusic(from_toggle)
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
   }
   function startGameMusic(from_toggle)
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
   }
   function getMusicOn()
   {
      return this.music_on;
   }
   function toggleMusic()
   {
      if(this.music_on == true)
      {
         this.turnOffMusic();
      }
      else if(this.music_on == false)
      {
         this.turnOnMusic();
      }
   }
   function turnOnMusic()
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
   }
   function turnOffMusic()
   {
      this.music_on = false;
      this.menu_sound.stop();
      this.game_sound.stop();
      _root.ng.setMusicOn(false);
   }
   function getSfxOn()
   {
      return this.sfx_on;
   }
   function toggleSfx()
   {
      if(this.sfx_on == true)
      {
         this.turnOffSfx();
      }
      else if(this.sfx_on == false)
      {
         this.turnOnSfx();
      }
   }
   function turnOnSfx()
   {
      this.sfx_on = true;
      _root.ng.setSfxOn(true);
   }
   function turnOffSfx()
   {
      this.sfx_on = false;
      _root.ng.setSfxOn(false);
   }
}
