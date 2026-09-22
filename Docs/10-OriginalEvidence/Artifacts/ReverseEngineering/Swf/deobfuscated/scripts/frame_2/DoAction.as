var §\x01§ = 13;
var §\x0f§ = 1;
function load(url)
{
   var _loc3_ = createEmptyMovieClip("loader_mc",getNextHighestDepth());
   var _loc2_ = new MovieClipLoader();
   _loc2_.addListener(this);
   _loc2_.loadClip(url,_loc3_);
}
function onLoadInit(mc)
{
   _root.gs_loaded = true;
   gs_gameServices = new GameServices(gameServicesCallback);
   gs_scoreSubmit = new GSScoreSubmit(gameServicesCallback);
   gs_sendToFriend = new GSSendToFriend(stfCallback);
   GameServices.initGameTracking(null,false);
   trace("Game Services module " + mtvnGSPath + " loaded, GS ver=" + GameServices.getVersionString());
   trace("BaseDir=" + GameServices.getBaseDir());
   trace(showGSStartupInfo());
   trace("gotoAndStop:" + g_startFrame);
   _root.gotoAndStop(g_startFrame);
}
function stfCallback(action)
{
   trace("STF callback hit with " + action);
   if(action == "nodata")
   {
      stop();
   }
   else if(action == "load")
   {
      gotoAndStop("ReceivedData");
   }
   else
   {
      gotoAndStop("Result");
   }
}
function gameServicesCallback(gsResponseInfo, whichCommand)
{
   trace("GS called back with " + gsResponseInfo + " from " + whichCommand);
   var _loc6_;
   var _loc8_;
   var _loc5_;
   var _loc3_;
   var _loc7_;
   if(gsResponseInfo.isError())
   {
      trace("Error=" + gsResponseInfo.message + "; " + gsResponseInfo.extended_info);
      startupVars.text = gsResponseInfo.message + "; " + gsResponseInfo.extended_info;
   }
   else
   {
      _loc6_ = gsResponseInfo.success;
      if(_loc6_)
      {
         switch(whichCommand)
         {
            case "SetUserScoreGetScores":
            case "SetUserScoreGetScoresUnauth":
            case "GetScoreRanks":
            case "GetScoreRanksUnauth":
            case "GetUserScoreRanks":
            case "GetUserRatingRanks":
            case "GetRatingRanks":
               trace("got leaderboard");
               _loc8_ = gsResponseInfo.results;
               _root.hiscoreboard.displayHiscoresMTV(_loc8_);
               break;
            case "SetUserScoreUnauth":
               trace("submitted the score");
               _root.tt.doTween("view_scores",5329233);
               break;
            case "SetUserScoreGetRank":
            case "SetUserScoreGetRankUnauth":
            case "GetSiteUserRank":
               _loc5_ = gsResponseInfo.outparams.user_rank;
               showData.text = "Your rank is " + _loc5_;
               break;
            case "GetRegistrationStatus":
            case "UserLogin":
               _loc3_ = gsResponseInfo.results[0];
               showData.text = "User " + _loc3_.user_name + " (id=" + _loc3_.user_id + ") created on " + _loc3_.date_created + "\nLogged in " + _loc3_.login_count + " times, last login on " + _loc3_.last_login;
               GameServices.loadAvatar(1,_root,"myAvatar",_root.getNextHighestDepth());
               break;
            case "GetUserGameGroupExperiencePoints":
            case "GetUserGameExperiencePoints":
            case "GetUserSiteExperiencePoints":
               _loc7_ = gsResponseInfo.results[0].experience_points;
               showData.text = "You currently have " + _loc7_ + " experience points.";
         }
         if(whichCommand.indexOf("SetUserScore") != -1)
         {
            resultCode = gsResponseInfo.message;
            if(resultCode == "UNCHANGED")
            {
               startupVars.text = "You already have a better score.";
            }
            else
            {
               startupVars.text = "Score submitted!";
            }
         }
      }
      else
      {
         showData.text = "Command " + whichCommand + " failed: " + gsResponseInfo.message;
      }
   }
}
function showGSStartupInfo()
{
   gs_siteId = GameServices.getSiteId();
   gs_sessionId = GameServices.getSessionId();
   gs_playbackId = gs_sendToFriend.getPlaybackId();
   return "Loaded from " + mtvnGSPath + "; version " + GameServices.getVersionString() + "\nSiteId=" + gs_siteId + "; SessionId=" + gs_sessionId + "; PlaybackId=" + gs_playbackId;
}
var g_isDev = _root.GSPath == null;
var mtvnGSPath = "gs1.swf";
var g_startFrame = "preloader";
if(g_isDev)
{
   _root.game_id = "6";
   _root.site_id = "10";
   _root.SubmitURL = "http://gs.mtv-q.mtvi.com/community.php";
}
else
{
   mtvnGSPath = _root.GSPath + mtvnGSPath;
}
var gs_gameServices = null;
var gs_scoreSubmit = null;
var gs_sendToFriend = null;
var gs_loaded = false;
if(!_root.gs_loaded)
{
   trace("Loading GS from " + mtvnGSPath);
   load(mtvnGSPath);
}
_root.ng = new NitromeGame();
_root.ng.init("mutiny","yoho",33);
var i = 16;
while(i <= 33)
{
   _root.ng.setLevelUnlocked(i);
   i++;
}
_root.selected_level = 1;
_root.game_type = 1;
_root.won_p1 = 0;
_root.won_p2 = 0;
_lockroot = true;
_root.gotoAndStop("preloader");
