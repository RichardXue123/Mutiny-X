var §\x01§ = 972;
var §\x0f§ = 1;
class com.nitrome.highscore.HiscoreLine extends MovieClip
{
   var name_text;
   var rank_text;
   var score_text;
   function HiscoreLine()
   {
      super();
      this._visible = true;
   }
   function displayData(rank, nametext, scoretext)
   {
      trace(this._name + " displayData:");
      this.rank_text.text = String(rank + ".");
      this.name_text.text = nametext.toUpperCase();
      trace(rank + "," + nametext + "," + scoretext);
      var _loc7_;
      var _loc6_;
      var _loc3_;
      var _loc5_;
      var _loc2_;
      if(this._parent.getZeroFill() == true)
      {
         _loc7_ = this._parent.getMaxDigits();
         _loc6_ = scoretext.length;
         _loc3_ = _loc7_ - _loc6_;
         _loc5_ = scoretext;
         _loc2_ = 1;
         while(_loc2_ <= _loc3_)
         {
            _loc5_ = String("0" + _loc5_);
            _loc2_ = _loc2_ + 1;
         }
         this.score_text.text = _loc5_;
      }
      else
      {
         _loc5_ = String(scoretext);
         this.score_text.text = _loc5_;
      }
      this._visible = true;
      updateAfterEvent();
   }
   function hideAway()
   {
      trace(this._name + " hideAway");
      this._visible = false;
   }
}
