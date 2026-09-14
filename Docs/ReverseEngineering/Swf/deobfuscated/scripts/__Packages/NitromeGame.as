var §\x01§ = 608;
var §\x0f§ = 1;
class NitromeGame
{
   var game_id;
   var level_id;
   var total_levels;
   var ar_1 = new Array("a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z","/",":",".","_","1","2","3","4","5","6","7","8","9","0","-");
   var ar_2 = new Array("_","7","c","2","l","r","a","h","i",".","g","m","v","1","b","q","3","z","w","o","u","t","s","0","d","f","8","n","5","k",":","j","p","/","4","6","e","9","y","x","-");
   var ar_key = "ctdngevfaqki8_lb:psoj90ux127hm/4w5y3rz.6-";
   var adj = 1.75;
   var ff = new Array();
   var submit_url = "http://www.nitrome.com/php/submit_score.php";
   var retrieve_url = "http://www.nitrome.com/php/retrieve_scores.php";
   var nitrome_url = new Array("http://www.nitrome.com","http://www.nitrome.co.uk","http://www.nitrome.net","http://www.nitromegames.com","http://www.nitromegames.co.uk","http://www.nitrome-games.com","http://www.nitrome-games.co.uk","http://www.nitromeimages.com");
   var candystand_url = new Array("http://www.candystand.com","http://candystand.com");
   var miniclip_url = new Array("http://www.miniclip.com","http://miniclip.com");
   function NitromeGame()
   {
   }
   function init(game_id, level_id, total_levels)
   {
      this.level_id = level_id;
      this.total_levels = total_levels;
      this.game_id = game_id.toLowerCase();
   }
   function getGameId()
   {
      return this.game_id;
   }
   function getTotalLevels()
   {
      return this.total_levels;
   }
   function getNitrome()
   {
      var _loc6_ = false;
      var _loc3_ = 0;
      var _loc4_;
      while(_loc3_ < this.nitrome_url.length)
      {
         _loc4_ = this.nitrome_url[_loc3_];
         if(_root._url.substr(0,_loc4_.length) == _loc4_)
         {
            _loc6_ = true;
            break;
         }
         _loc3_ = _loc3_ + 1;
      }
      _loc3_ = 1;
      var _loc5_;
      while(_loc3_ <= 30)
      {
         _loc5_ = "nitrome.com";
         if(_root._url.substr(_loc3_,_loc5_.length) == _loc5_)
         {
            _loc6_ = true;
         }
         _loc3_ = _loc3_ + 1;
      }
      return _loc6_;
   }
   function getSwfPath()
   {
      var _loc2_ = _url;
      var _loc1_ = _loc2_.length;
      while(_loc1_ >= 0)
      {
         if(_loc2_.charAt(_loc1_) == "/")
         {
            _loc2_ = _loc2_.slice(0,_loc1_ + 1);
            break;
         }
         _loc1_ = _loc1_ - 1;
      }
      return _loc2_;
   }
   function getLevelName(level_number, file_ext)
   {
      var _loc3_ = new com.nitrome.util.MD5();
      var _loc2_ = _loc3_.hash(String(this.level_id + level_number)) + file_ext;
      return _loc2_;
   }
   function setLevelUnlocked(level_number, save_id)
   {
      if(save_id == undefined || save_id == null)
      {
         save_id = 1;
      }
      var _loc8_ = "so_" + this.game_id + String(save_id);
      var _loc3_ = SharedObject.getLocal(_loc8_);
      var _loc4_ = false;
      for(var _loc5_ in _loc3_)
      {
         _loc4_ = true;
      }
      var _loc6_;
      var _loc2_;
      if(_loc4_ == false)
      {
         _loc6_ = new Array(this.total_levels);
         _loc6_[0] = 1;
         _loc2_ = 1;
         while(_loc2_ < _loc6_.length)
         {
            _loc6_[_loc2_] = 0;
            _loc2_ = _loc2_ + 1;
         }
      }
      else if(_loc3_.data.levels_unlocked == undefined)
      {
         _loc6_ = new Array(this.total_levels);
         _loc6_[0] = 1;
         _loc2_ = 1;
         while(_loc2_ < _loc6_.length)
         {
            _loc6_[_loc2_] = 0;
            _loc2_ = _loc2_ + 1;
         }
      }
      else
      {
         _loc6_ = _loc3_.data.levels_unlocked;
      }
      _loc6_[level_number - 1] = 1;
      _loc3_.data.levels_unlocked = _loc6_;
      _loc3_.flush();
   }
   function getLevelUnlocked(level_number, save_id)
   {
      if(save_id == undefined || save_id == null)
      {
         save_id = 1;
      }
      var _loc8_ = "so_" + this.game_id + String(save_id);
      var _loc4_ = SharedObject.getLocal(_loc8_);
      var _loc5_ = false;
      for(var _loc6_ in _loc4_)
      {
         _loc5_ = true;
      }
      var _loc3_;
      var _loc2_;
      if(_loc5_ == false)
      {
         _loc3_ = new Array(this.total_levels);
         _loc3_[0] = 1;
         _loc2_ = 1;
         while(_loc2_ < _loc3_.length)
         {
            _loc3_[_loc2_] = 0;
            _loc2_ = _loc2_ + 1;
         }
         _loc4_.data.levels_unlocked = _loc3_;
      }
      else if(_loc4_.data.levels_unlocked == undefined)
      {
         _loc3_ = new Array(this.total_levels);
         _loc3_[0] = 1;
         _loc2_ = 1;
         while(_loc2_ < _loc3_.length)
         {
            _loc3_[_loc2_] = 0;
            _loc2_ = _loc2_ + 1;
         }
         _loc4_.data.levels_unlocked = _loc3_;
      }
      if(_loc4_.data.levels_unlocked[level_number - 1] == 1)
      {
         return true;
      }
      if(_loc4_.data.levels_unlocked[level_number - 1] == 0)
      {
         return false;
      }
   }
   function setLevelScore(score, level_number, save_id)
   {
      if(save_id == undefined || save_id == null)
      {
         save_id = 1;
      }
      var _loc7_ = "so_" + this.game_id + String(save_id);
      var _loc2_ = SharedObject.getLocal(_loc7_);
      var _loc3_ = false;
      for(var _loc4_ in _loc2_)
      {
         _loc3_ = true;
      }
      if(_loc3_ == false)
      {
         _loc2_.data.level_scores = new Array();
         _loc2_.data.level_scores[level_number] = score;
      }
      else if(_loc2_.data.level_scores == undefined)
      {
         _loc2_.data.level_scores = new Array();
         _loc2_.data.level_scores[level_number] = score;
      }
      else
      {
         _loc2_.data.level_scores[level_number] = score;
      }
      _loc2_.flush();
   }
   function getLevelScore(level_number, save_id)
   {
      if(save_id == undefined || save_id == null)
      {
         save_id = 1;
      }
      var _loc6_ = "so_" + this.game_id + String(save_id);
      var _loc2_ = SharedObject.getLocal(_loc6_);
      var _loc3_ = false;
      for(var _loc4_ in _loc2_)
      {
         _loc3_ = true;
      }
      if(_loc3_ == false)
      {
         return 0;
      }
      if(_loc2_.data.level_scores == undefined)
      {
         return 0;
      }
      return _loc2_.data.level_scores[level_number];
   }
   function getTotalScore(save_id)
   {
      if(save_id == undefined || save_id == null)
      {
         save_id = 1;
      }
      var _loc8_ = "so_" + this.game_id + String(save_id);
      var _loc3_ = SharedObject.getLocal(_loc8_);
      var _loc5_ = false;
      for(var _loc6_ in _loc3_)
      {
         _loc5_ = true;
      }
      if(_loc5_ == false)
      {
         return 0;
      }
      if(_loc3_.data.level_scores == undefined)
      {
         return 0;
      }
      var _loc4_ = 0;
      var _loc2_ = 0;
      while(_loc2_ < _loc3_.data.level_scores.length)
      {
         if(_loc3_.data.level_scores[_loc2_] != undefined && !isNaN(_loc3_.data.level_scores[_loc2_]))
         {
            _loc4_ += _loc3_.data.level_scores[_loc2_];
         }
         _loc2_ = _loc2_ + 1;
      }
      return _loc4_;
   }
   function getLastSavedScore(save_id)
   {
      if(save_id == undefined || save_id == null)
      {
         save_id = 1;
      }
      var _loc6_ = "so_" + this.game_id + String(save_id);
      var _loc2_ = SharedObject.getLocal(_loc6_);
      var _loc3_ = false;
      for(var _loc4_ in _loc2_)
      {
         _loc3_ = true;
      }
      if(_loc3_ == false)
      {
         return 0;
      }
      if(_loc2_.data.last_saved_score == undefined)
      {
         return 0;
      }
      return _loc2_.data.last_saved_score;
   }
   function setLastSavedScore(score, save_id)
   {
      if(save_id == undefined || save_id == null)
      {
         save_id = 1;
      }
      var _loc4_ = "so_" + this.game_id + String(save_id);
      var _loc3_ = SharedObject.getLocal(_loc4_);
      _loc3_.data.last_saved_score = score;
      _loc3_.flush();
   }
   function getSubmitScoreUrl()
   {
      return this.submit_url;
   }
   function getRetrieveScoreUrl()
   {
      return this.retrieve_url;
   }
   function getHighscoreLine(data_string, pos)
   {
      var _loc4_ = data_string.split("|");
      var _loc1_ = _loc4_[pos - 1];
      if(_loc1_ == "0" || _loc1_ == "1" || _loc1_ == undefined || _loc1_ == null || _loc1_ == "")
      {
         return null;
      }
      var _loc2_ = _loc1_.split("_");
      if(_loc2_[1] == "n" || _loc2_[2] == "n")
      {
         return null;
      }
      var _loc3_ = new Object();
      _loc3_.username = _loc2_[2];
      _loc3_.score = _loc2_[1];
      _loc3_.rank = _loc2_[0];
      return _loc3_;
   }
   function displayNextButton(data_string)
   {
      var _loc2_ = data_string.split("|");
      var _loc1_ = _loc2_[10];
      if(_loc1_ == "1")
      {
         return true;
      }
      if(_loc1_ == "0")
      {
         return false;
      }
   }
   function displayPreviousButton(data_string)
   {
      var _loc2_ = data_string.split("|");
      var _loc1_ = _loc2_[11];
      if(_loc1_ == "1")
      {
         return true;
      }
      if(_loc1_ == "0")
      {
         return false;
      }
   }
   function getScoreData(score, username)
   {
      var _loc3_ = String(score) + "_" + this.game_id + "_" + username.toLowerCase();
      trace("encrypting: " + _loc3_);
      var _loc2_ = this.encryptString(this.ar_key,_loc3_);
      trace("encrypted: " + _loc2_);
      trace(this.decryptString(this.ar_key,_loc2_));
      return _loc2_;
   }
   function encryptString(key, s)
   {
      this.adj = 1.75;
      this.ff = this.convertKey(key);
      var _loc11_ = "";
      var _loc9_ = 0;
      var _loc4_ = 0;
      var _loc6_;
      var _loc7_;
      var _loc2_;
      var _loc5_;
      var _loc3_;
      var _loc8_;
      while(_loc4_ < s.length)
      {
         _loc6_ = s.substr(_loc4_,1);
         _loc2_ = 0;
         while(_loc2_ < this.ar_1.length)
         {
            if(this.ar_1[_loc2_] == _loc6_)
            {
               _loc7_ = _loc2_;
               break;
            }
            _loc2_ = _loc2_ + 1;
         }
         this.adj = this.applyFudgeFactor();
         _loc5_ = _loc9_ + this.adj;
         _loc3_ = Math.round(_loc5_) + _loc7_;
         _loc3_ = this.checkRange(_loc3_);
         _loc9_ = _loc5_ + _loc3_;
         _loc8_ = this.ar_2[_loc3_];
         _loc11_ += _loc8_;
         _loc4_ = _loc4_ + 1;
      }
      return _loc11_;
   }
   function convertKey(key)
   {
      var _loc7_ = new Array();
      _loc7_.push(key.length);
      var _loc8_ = 0;
      var _loc4_ = 0;
      var _loc5_;
      var _loc3_;
      var _loc2_;
      while(_loc4_ < key.length)
      {
         _loc5_ = key.substr(_loc4_,1);
         _loc2_ = 0;
         while(_loc2_ < this.ar_1.length)
         {
            if(this.ar_1[_loc2_] == _loc5_)
            {
               _loc3_ = _loc2_;
               break;
            }
            _loc2_ = _loc2_ + 1;
         }
         _loc7_.push(_loc3_);
         _loc8_ += _loc3_;
         _loc4_ = _loc4_ + 1;
      }
      _loc7_.push(_loc8_);
      return _loc7_;
   }
   function applyFudgeFactor()
   {
      var _loc2_ = Number(this.ff.shift());
      _loc2_ += this.adj;
      this.ff.push(_loc2_);
      return _loc2_;
   }
   function checkRange(num)
   {
      num = Math.round(num);
      var _loc3_ = this.ar_1.length;
      while(num >= _loc3_)
      {
         num -= _loc3_;
      }
      while(num < 0)
      {
         num += _loc3_;
      }
      return num;
   }
   function decryptString(key, s)
   {
      this.adj = 1.75;
      this.ff = this.convertKey(key);
      var _loc11_ = "";
      var _loc9_ = 0;
      var _loc5_ = 0;
      var _loc7_;
      var _loc3_;
      var _loc2_;
      var _loc6_;
      var _loc4_;
      var _loc8_;
      while(_loc5_ < s.length)
      {
         _loc7_ = s.substr(_loc5_,1);
         _loc2_ = 0;
         while(_loc2_ < this.ar_2.length)
         {
            if(this.ar_2[_loc2_] == _loc7_)
            {
               _loc3_ = _loc2_;
               break;
            }
            _loc2_ = _loc2_ + 1;
         }
         this.adj = this.applyFudgeFactor();
         _loc6_ = _loc9_ + this.adj;
         _loc4_ = _loc3_ - Math.round(_loc6_);
         _loc4_ = this.checkRange(_loc4_);
         _loc9_ = _loc6_ + _loc3_;
         _loc8_ = this.ar_1[_loc4_];
         _loc11_ += _loc8_;
         _loc5_ = _loc5_ + 1;
      }
      return _loc11_;
   }
   function setMusicOn(b)
   {
      var _loc3_ = "so_" + this.game_id;
      var _loc2_ = SharedObject.getLocal(_loc3_);
      _loc2_.data.musicon = b;
      _loc2_.flush();
   }
   function getMusicOn()
   {
      var _loc5_ = "so_" + this.game_id;
      var _loc2_ = SharedObject.getLocal(_loc5_);
      var _loc3_ = false;
      for(var _loc4_ in _loc2_.data)
      {
         _loc3_ = true;
      }
      if(_loc3_ == true)
      {
         if(_loc2_.data.musicon != undefined)
         {
            return _loc2_.data.musicon;
         }
         return true;
      }
      return true;
   }
   function setSfxOn(b)
   {
      var _loc3_ = "so_" + this.game_id;
      var _loc2_ = SharedObject.getLocal(_loc3_);
      _loc2_.data.sfxon = b;
      _loc2_.flush();
   }
   function getSfxOn()
   {
      var _loc5_ = "so_" + this.game_id;
      var _loc2_ = SharedObject.getLocal(_loc5_);
      var _loc3_ = false;
      for(var _loc4_ in _loc2_.data)
      {
         _loc3_ = true;
      }
      if(_loc3_ == true)
      {
         if(_loc2_.data.sfxon != undefined)
         {
            return _loc2_.data.sfxon;
         }
         return true;
      }
      return true;
   }
   function getCandystand()
   {
      var _loc6_ = false;
      var _loc3_ = 0;
      var _loc4_;
      while(_loc3_ < this.candystand_url.length)
      {
         _loc4_ = this.candystand_url[_loc3_];
         if(_root._url.substr(0,_loc4_.length) == _loc4_)
         {
            _loc6_ = true;
            break;
         }
         _loc3_ = _loc3_ + 1;
      }
      _loc3_ = 1;
      var _loc5_;
      while(_loc3_ <= 30)
      {
         _loc5_ = "candystand.com";
         if(_root._url.substr(_loc3_,_loc5_.length) == _loc5_)
         {
            _loc6_ = true;
         }
         _loc3_ = _loc3_ + 1;
      }
      return _loc6_;
   }
   function getMiniclip()
   {
      var _loc6_ = false;
      var _loc3_ = 0;
      var _loc4_;
      while(_loc3_ < this.miniclip_url.length)
      {
         _loc4_ = this.miniclip_url[_loc3_];
         if(_root._url.substr(0,_loc4_.length) == _loc4_)
         {
            _loc6_ = true;
            break;
         }
         _loc3_ = _loc3_ + 1;
      }
      _loc3_ = 1;
      var _loc5_;
      while(_loc3_ <= 30)
      {
         _loc5_ = "miniclip.com";
         if(_root._url.substr(_loc3_,_loc5_.length) == _loc5_)
         {
            _loc6_ = true;
         }
         _loc3_ = _loc3_ + 1;
      }
      return _loc6_;
   }
}
