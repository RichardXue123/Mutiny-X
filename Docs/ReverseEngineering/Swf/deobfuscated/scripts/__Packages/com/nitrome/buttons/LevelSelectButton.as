var §\x01§ = 888;
var §\x0f§ = 1;
class com.nitrome.buttons.LevelSelectButton extends MovieClip
{
   var id;
   var level_number;
   var onPress;
   var onRollOut;
   var onRollOver;
   var pirate_clip;
   var question_mark;
   function LevelSelectButton()
   {
      super();
   }
   function onLoad()
   {
      this.init();
   }
   function init()
   {
      this.id = Number(this._name.slice(3));
      var _loc3_;
      var _loc4_;
      if(_root.ng.getLevelUnlocked(this.id) == true)
      {
         _loc3_ = String(this.id);
         if(_loc3_.length == 1)
         {
            _loc3_ = "0" + _loc3_;
         }
         this.level_number.text = _loc3_;
         this.pirate_clip.gotoAndStop(this.id);
         this.question_mark._visible = false;
         this.useHandCursor = true;
         this.onRollOver = this.doRollOver;
         this.onRollOut = this.doRollOut;
         this.onPress = this.doPress;
      }
      else
      {
         this.level_number.text = "";
         this.pirate_clip.gotoAndStop(this.id);
         _loc4_ = new Color(this.pirate_clip);
         _loc4_.setRGB(0);
         this.question_mark._visible = true;
         this.useHandCursor = false;
      }
   }
   function doRollOver()
   {
      this.gotoAndStop("over");
   }
   function doRollOut()
   {
      this.gotoAndStop("up");
   }
   function doPress()
   {
      _root.selected_level = this.id;
      _root.score = 0;
      _root.tt.doTween("game");
   }
}
