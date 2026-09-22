var §\x01§ = 700;
var §\x0f§ = 1;
class com.nitrome.highscore.LetterButton extends MovieClip
{
   var letter_holder;
   var letter_text;
   function LetterButton()
   {
      super();
      this.letter_text = this._name;
   }
   function onLoad()
   {
      this.letter_holder.letter.text = this._name;
   }
   function onRollOver()
   {
      this.gotoAndStop("over");
      this.letter_holder.letter.text = this._name;
   }
   function onRollOut()
   {
      this.gotoAndStop("up");
      this.letter_holder.letter.text = this._name;
   }
   function onPress()
   {
      this._parent.addLetter(this.letter_text);
   }
}
