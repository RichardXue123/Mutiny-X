var §\x01§ = 168;
var §\x0f§ = 1;
class com.nitrome.throwgame.SpeechBubble extends com.nitrome.util.Clip
{
   var mc;
   var show;
   var target;
   var text;
   var update;
   var x;
   var y;
   var startTime = 0;
   var completeTime = 0;
   function SpeechBubble()
   {
      super(com.nitrome.throwgame.Controller.characterLayer,"speechBubble");
      this.startTime = 10;
   }
   function advance()
   {
      if(this.startTime > 0)
      {
         this.startTime--;
         if(this.startTime < 1)
         {
            this.show();
         }
         return undefined;
      }
      if(this.mc.textHolder.textField.text.length < this.text.length)
      {
         this.mc.textHolder.textField.text += this.text.substr(this.mc.textHolder.textField.text.length,3);
         this.completeTime = 0;
      }
      else
      {
         this.completeTime++;
      }
   }
   function setTarget(newTarget)
   {
      _root.sfx_manager.playSound(newTarget.team.getTeamType());
      this.target = newTarget;
      this.x = this.target.x;
      this.y = this.target.y - 90;
      this.update();
   }
}
