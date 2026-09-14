var §\x01§ = 264;
var §\x0f§ = 1;
class com.nitrome.highscore.ScoreSubmitPanel extends MovieClip
{
   var name_text;
   var score_text;
   var submit_button;
   var MAX_LENGTH = 10;
   function ScoreSubmitPanel()
   {
      super();
   }
   function onLoad()
   {
      this.score_text.text = String("YOUR SCORE IS " + _root.score);
      Key.addListener(this);
   }
   function addLetter(l)
   {
      var _loc3_ = this.name_text.text;
      var _loc4_;
      if(_loc3_.length < this.MAX_LENGTH)
      {
         _loc4_ = _loc3_ + l;
         this.name_text.text = _loc4_;
         _root.name_entered = this.name_text.text.toUpperCase();
         this.submit_button.enable();
      }
   }
   function getNameText()
   {
      return String(this.name_text.text);
   }
   function clearName()
   {
      this.submit_button.disable();
      this.name_text.text = "";
      _root.name_entered = this.name_text.text;
   }
}
