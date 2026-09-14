var §\x01§ = 327;
var §\x0f§ = 1;
class com.nitrome.game.IngameTextArea extends MovieClip
{
   var lines;
   var textField;
   var thisLineFrame = 0;
   function IngameTextArea()
   {
      super();
      this.lines = [];
   }
   function say(str)
   {
      this.lines.push(str);
      if(this.lines.length == 1)
      {
         this.textField.text = str;
      }
   }
   function onEnterFrame()
   {
      if(com.nitrome.throwgame.Controller.speechBubble)
      {
         return undefined;
      }
      if(this.lines.length > 0)
      {
         this.thisLineFrame++;
         if(this.thisLineFrame > 70)
         {
            this.thisLineFrame = 0;
            this.lines.splice(0,1);
            this.textField.text = this.lines[0];
         }
         if(this.thisLineFrame < 10)
         {
            this._y = 400 - 3 * this.thisLineFrame;
         }
         else if(this.thisLineFrame < 60)
         {
            this._y = 370;
         }
         else
         {
            this._y = 370 + 3 * (this.thisLineFrame - 60);
         }
      }
   }
}
