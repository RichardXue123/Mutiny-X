var §\x01§ = 674;
var §\x0f§ = 1;
class com.nitrome.highscore.HiscoreBoard extends MovieClip
{
   var next_arrow;
   var prev_arrow;
   var saved_min_rank;
   var start_interval;
   var max_digits = 8;
   var zero_fill = false;
   var max_rank = 100;
   function HiscoreBoard()
   {
      super();
      this.start_interval = setInterval(this,"loadHiscores",100,1);
   }
   function loadHiscores(min_rank)
   {
      clearInterval(this.start_interval);
      trace("loading hiscores from: " + min_rank);
      if(min_rank == 1)
      {
         this.prev_arrow.hideAway();
         trace("hiding prev arrow");
      }
      else
      {
         this.prev_arrow.display();
      }
      if(min_rank == 91)
      {
         this.next_arrow.hideAway();
         trace("hiding next arrow");
      }
      this.saved_min_rank = min_rank;
      GSScoreSubmit.unauthLeaderBoard(min_rank,11);
   }
   function displayHiscores(data_string)
   {
      trace("displayHiscores: " + data_string);
      var _loc3_;
      var _loc4_;
      var _loc5_;
      if(data_string != "0")
      {
         _loc3_ = 0;
         while(_loc3_ <= 9)
         {
            _loc4_ = _root.ng.getHighscoreLine(data_string,_loc3_ + 1);
            _loc5_ = String("score_line_" + (_loc3_ + 1));
            if(_loc4_ == null)
            {
               this[_loc5_].hideAway();
            }
            else
            {
               this[_loc5_].displayData(this.saved_min_rank + _loc3_,_loc4_.username,_loc4_.score);
            }
            _loc3_ = _loc3_ + 1;
         }
         if(_root.ng.displayNextButton(data_string) == true)
         {
            this.next_arrow.display();
         }
         else
         {
            this.next_arrow.hideAway();
         }
         if(_root.ng.displayPreviousButton(data_string) == true)
         {
            this.prev_arrow.display();
         }
         else
         {
            this.prev_arrow.hideAway();
         }
         this.gotoAndPlay("reveal");
      }
      else
      {
         trace("error with high scores");
      }
   }
   function getZeroFill()
   {
      return this.zero_fill;
   }
   function getMaxDigits()
   {
      return this.max_digits;
   }
   function shiftScoresPrev()
   {
      this.gotoAndStop("loading");
      var _loc2_ = this.saved_min_rank - 10;
      if(_loc2_ < 1)
      {
         _loc2_ = 1;
      }
      this.loadHiscores(_loc2_);
   }
   function shiftScoresNext()
   {
      this.gotoAndStop("loading");
      var _loc2_ = this.saved_min_rank + 10;
      this.loadHiscores(_loc2_);
   }
   function displayHiscoresMTV(leaderboard)
   {
      var _loc2_;
      var _loc3_;
      var _loc5_;
      var _loc4_;
      if(leaderboard.length > 0)
      {
         _loc2_ = 0;
         while(_loc2_ <= 10)
         {
            _loc3_ = String(leaderboard[_loc2_].user_name);
            _loc5_ = String(leaderboard[_loc2_].score);
            if(_loc3_ == "" || _loc3_ == undefined || _loc3_ == "undefined")
            {
               this[String("score_line_" + (_loc2_ + 1))].hideAway();
               this.next_arrow.hideAway();
            }
            else
            {
               _loc4_ = String("score_line_" + (_loc2_ + 1));
               this[_loc4_].displayData(this.saved_min_rank + _loc2_,_loc3_,_loc5_);
            }
            _loc2_ = _loc2_ + 1;
         }
         this.gotoAndPlay("reveal");
      }
      else
      {
         trace("no leaderboard data");
      }
   }
}
