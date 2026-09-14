var §\x01§ = 186;
var §\x0f§ = 1;
class com.nitrome.buttons.LevelSelect2p extends MovieClip
{
   var content_clip;
   var next_button;
   var play_button;
   var prev_button;
   var level_id = 16;
   var min_level_id = 16;
   var max_level_id = 33;
   function LevelSelect2p()
   {
      super();
   }
   function onLoad()
   {
      this.init();
   }
   function init()
   {
      if(_root.selected_level >= 16)
      {
         this.level_id = _root.selected_level;
      }
      this.doDisplay();
      this.prev_button.onRollOver = function()
      {
         this.gotoAndStop("over");
      };
      this.prev_button.onRollOut = function()
      {
         this.gotoAndStop("up");
      };
      this.prev_button.onPress = function()
      {
         this._parent.pressPrev();
      };
      this.next_button.onRollOver = function()
      {
         this.gotoAndStop("over");
      };
      this.next_button.onRollOut = function()
      {
         this.gotoAndStop("up");
      };
      this.next_button.onPress = function()
      {
         this._parent.pressNext();
      };
      this.play_button.onRollOver = function()
      {
         this.gotoAndStop("over");
      };
      this.play_button.onRollOut = function()
      {
         this.gotoAndStop("up");
      };
      this.play_button.onPress = function()
      {
         this._parent.pressPlay();
      };
   }
   function doDisplay()
   {
      this.content_clip.gotoAndStop(this.level_id - 15);
      if(this.level_id <= this.min_level_id)
      {
         this.level_id = this.min_level_id;
         this.prev_button._visible = false;
      }
      else
      {
         this.prev_button._visible = true;
      }
      if(this.level_id >= this.max_level_id)
      {
         this.level_id = this.max_level_id;
         this.next_button._visible = false;
      }
      else
      {
         this.next_button._visible = true;
      }
   }
   function pressNext()
   {
      this.level_id++;
      this.doDisplay();
   }
   function pressPrev()
   {
      this.level_id--;
      this.doDisplay();
   }
   function pressPlay()
   {
      _root.selected_level = this.level_id;
      _root.tt.doTween("game");
   }
}
