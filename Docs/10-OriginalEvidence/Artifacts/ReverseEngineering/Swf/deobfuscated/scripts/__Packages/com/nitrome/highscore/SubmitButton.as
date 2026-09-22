var §\x01§ = 637;
var §\x0f§ = 1;
class com.nitrome.highscore.SubmitButton extends MovieClip
{
   var pressed = false;
   var disabled = true;
   function SubmitButton()
   {
      super();
      this.pressed = false;
      this.gotoAndStop("disabled");
   }
   function onRelease()
   {
      if(this.disabled == true)
      {
         this.gotoAndStop("disabled");
      }
      else if(this.pressed == false)
      {
         this._parent.loading_clip.gotoAndPlay(2);
         this.pressed = true;
      }
   }
   function submitScore()
   {
      _root.name_entered = this._parent.getNameText();
      if(_root.name_entered != "")
      {
         trace("submit score: " + _root.name_entered + ":" + _root.score);
         GSScoreSubmit.unauthSubmitScore(_root.score,_root.score,"",_root.name_entered);
      }
      else
      {
         _root.gotoAndStop("view_scores");
      }
   }
   function enable()
   {
      this.disabled = false;
      this.gotoAndStop("up");
   }
   function disable()
   {
      this.disabled = true;
      this.gotoAndStop("disabled");
   }
   function onRollOver()
   {
      if(this.disabled == true)
      {
         this.gotoAndStop("disabled");
         this.useHandCursor = false;
      }
      else
      {
         this.gotoAndStop("over");
         this.useHandCursor = true;
      }
   }
   function onRollOut()
   {
      if(this.disabled == true)
      {
         this.gotoAndStop("disabled");
      }
      else
      {
         this.gotoAndStop("up");
      }
   }
}
