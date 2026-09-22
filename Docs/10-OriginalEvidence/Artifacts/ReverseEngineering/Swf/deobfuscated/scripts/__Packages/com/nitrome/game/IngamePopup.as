var §\x01§ = 568;
var §\x0f§ = 1;
class com.nitrome.game.IngamePopup extends MovieClip
{
   var finalScore;
   var levelScore;
   var totalScore;
   var show = false;
   function IngamePopup()
   {
      super();
      this._x = 275;
      this._visible = false;
      this._alpha = 0;
   }
   function onEnterFrame()
   {
      if(this.show && this._alpha < 100)
      {
         this._alpha += 25;
         this._visible = true;
      }
      else if(!this.show && this._alpha > 0)
      {
         this._alpha -= 25;
         if(this._alpha < 1)
         {
            this._visible = false;
         }
      }
      var _loc5_;
      var _loc6_;
      var _loc3_;
      var _loc4_;
      if(this.show)
      {
         if(this.levelScore)
         {
            _loc5_ = com.nitrome.throwgame.Controller.get1PLevelScore();
            _loc6_ = _root.score;
            if(Number(this.levelScore.text) < _loc5_)
            {
               _loc3_ = Number(this.levelScore.text);
               _loc3_ += 287;
               if(_loc3_ > _loc5_)
               {
                  _loc3_ = _loc5_;
               }
               this.levelScore.text = _loc3_.toString();
            }
            else if(Number(this.totalScore.text) < _loc6_)
            {
               _loc3_ = Number(this.totalScore.text);
               _loc3_ += 347;
               if(_loc3_ > _loc6_)
               {
                  _loc3_ = _loc6_;
               }
               this.totalScore.text = _loc3_.toString();
            }
         }
         else if(this.finalScore)
         {
            _loc4_ = _root.score;
            if(Number(this.finalScore.text) < _loc4_)
            {
               _loc3_ = Number(this.finalScore.text);
               _loc3_ += 347;
               if(_loc3_ > _loc4_)
               {
                  _loc3_ = _loc4_;
               }
               this.finalScore.text = _loc3_.toString();
            }
         }
      }
   }
   function reset()
   {
      if(this.levelScore)
      {
         this.levelScore.text = "0";
      }
      if(this.totalScore)
      {
         this.totalScore.text = "0";
      }
      if(this.finalScore)
      {
         this.finalScore.text = "0";
      }
   }
}
