var §\x01§ = 598;
var §\x0f§ = 1;
class com.nitrome.game.TransitionTween extends MovieClip
{
   var next_frame;
   var next_frame_function;
   function TransitionTween()
   {
      super();
   }
   function doTween(next_frame)
   {
      this.next_frame = next_frame;
      this.next_frame_function = null;
      this.gotoAndPlay(2);
   }
   function doTweenWithFunction(next)
   {
      this.next_frame = null;
      this.next_frame_function = next;
      this.gotoAndPlay(2);
   }
   function performTask()
   {
      if(this.next_frame)
      {
         this._parent.gotoAndStop(this.next_frame);
      }
      if(this.next_frame_function)
      {
         this.next_frame_function();
         this.next_frame_function = null;
      }
   }
}
