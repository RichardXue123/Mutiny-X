var §\x01§ = 571;
var §\x0f§ = 1;
class com.nitrome.highscore.ArrowButton extends MovieClip
{
   function ArrowButton()
   {
      super();
   }
   function hideAway()
   {
      trace(this._name + ": hide");
      this._visible = false;
   }
   function display()
   {
      trace(this._name + ": display");
      this._visible = true;
   }
   function onRollOver()
   {
      this.gotoAndStop("over");
   }
   function onRollOut()
   {
      this.gotoAndStop("up");
   }
   function onPress()
   {
      if(this._name == "prev_arrow")
      {
         this._parent.shiftScoresPrev();
      }
      else if(this._name == "next_arrow")
      {
         this._parent.shiftScoresNext();
      }
   }
}
