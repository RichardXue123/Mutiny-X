var §\x01§ = 853;
var §\x0f§ = 1;
class com.nitrome.throwgame.TidalWave extends com.nitrome.throwgame.Weapon
{
   var draggable;
   var hide;
   var hitPlayers;
   var hitsTiles;
   var mc;
   var show;
   var velocityX;
   var weight;
   var x;
   var y;
   function TidalWave()
   {
      super("tidalWave");
      this.placeableWeapon = true;
      this.hitsTiles = false;
      this.weight = 0;
      this.showCircle = false;
      this.draggable = false;
      this.hitPlayers = [];
   }
   function advance()
   {
      var _loc6_;
      var _loc5_;
      var _loc4_;
      var _loc3_;
      if(this.fired)
      {
         super.advance();
         if(this.x > (com.nitrome.throwgame.Controller.tileSystem.levelWidth << 5) + 550)
         {
            this.finished = true;
            this.hide();
         }
         _loc6_ = 0;
         while(_loc6_ < com.nitrome.throwgame.Controller.teams.length)
         {
            _loc5_ = com.nitrome.throwgame.Controller.teams[_loc6_];
            _loc4_ = 0;
            while(_loc4_ < _loc5_.characters.length)
            {
               _loc3_ = _loc5_.characters[_loc4_];
               if(_loc3_.alive)
               {
                  if(_loc3_.y >= com.nitrome.throwgame.Controller.water.y - 300)
                  {
                     if(_loc3_.x >= this.x - 150)
                     {
                        if(_loc3_.x <= this.x + 150)
                        {
                           _loc3_.subtractHealth(5);
                        }
                     }
                  }
               }
               _loc4_ = _loc4_ + 1;
            }
            _loc6_ = _loc6_ + 1;
         }
      }
      else if(com.nitrome.throwgame.Controller.tileSystem.mouseButtonDown)
      {
         this.startWave();
      }
   }
   function aiPerform(details)
   {
      this.startWave();
   }
   function startWave()
   {
      this.fired = true;
      this.track = true;
      this.x = -550;
      this.y = com.nitrome.throwgame.Controller.water.y;
      this.velocityX = 20;
      this.show();
      this.mc.gotoAndPlay("anim" + com.nitrome.throwgame.Controller.skyColour.toString());
      this.owner.canThrow = false;
      this.owner.canShoot = false;
   }
}
