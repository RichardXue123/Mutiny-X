var §\x01§ = 629;
var §\x0f§ = 1;
class com.nitrome.throwgame.Cannonball extends com.nitrome.throwgame.Weapon
{
   var bottomExtent;
   var finished;
   var fired;
   var hide;
   var hitsBoxes;
   var leftExtent;
   var owner;
   var rightExtent;
   var show;
   var simulation;
   var simulationFinished;
   var topExtent;
   var update;
   var velocityX;
   var velocityY;
   var weight;
   var x;
   var y;
   function Cannonball()
   {
      super("cannonball");
      this.show();
      this.weight = 0;
      this.hitsBoxes = true;
   }
   function setSimulation()
   {
      this.simulation = true;
   }
   function isSimulationFinished()
   {
      return this.simulationFinished;
   }
   function contact(side)
   {
      if(this.simulation)
      {
         this.simulationFinished = true;
         return undefined;
      }
      super.contact(side);
      if(this.fired && !this.finished)
      {
         this.finished = true;
         this.hide();
         new com.nitrome.throwgame.Explosion(this.x,this.y,100,50,this.owner);
         this.velocityX = 0;
         this.velocityY = 0;
         _root.sfx_manager.playSound("pop");
      }
   }
   function advanceMotion()
   {
      super.advanceMotion();
      var _loc3_ = com.nitrome.throwgame.Controller.tileSystem.levelWidth << 5;
      if(this.x < -300 || this.x > _loc3_ + 300 || this.y < -300 || this.y > com.nitrome.throwgame.Controller.water.y)
      {
         if(this.simulation)
         {
            this.simulationFinished = true;
         }
         else
         {
            this.finished = true;
            this.hide();
         }
      }
   }
   function advance()
   {
      this.advanceMotion();
      this.update();
      var _loc6_;
      if(!this.simulation && !this.finished)
      {
         _loc6_ = new com.nitrome.throwgame.Debris(com.nitrome.throwgame.Controller.effectsLayer,"cannonSmokeTrail");
         _loc6_.x = this.x;
         _loc6_.y = this.y;
         _loc6_.show();
      }
      var _loc5_;
      var _loc4_;
      var _loc3_;
      var _loc2_;
      if(!this.finished)
      {
         _loc5_ = 0;
         while(_loc5_ < com.nitrome.throwgame.Controller.teams.length)
         {
            _loc4_ = com.nitrome.throwgame.Controller.teams[_loc5_];
            _loc3_ = 0;
            while(_loc3_ < _loc4_.characters.length)
            {
               _loc2_ = _loc4_.characters[_loc3_];
               if(_loc2_.alive)
               {
                  if(_loc2_ != this.owner)
                  {
                     if(_loc2_.x - _loc2_.leftExtent <= this.x + this.rightExtent)
                     {
                        if(_loc2_.x + _loc2_.rightExtent >= this.x - this.leftExtent)
                        {
                           if(_loc2_.y - _loc2_.topExtent <= this.y + this.bottomExtent)
                           {
                              if(_loc2_.y + _loc2_.bottomExtent >= this.y - this.topExtent)
                              {
                                 this.finished = true;
                                 this.hide();
                                 new com.nitrome.throwgame.Explosion(this.x,this.y,100,50,this.owner);
                                 this.velocityX = 0;
                                 this.velocityY = 0;
                              }
                           }
                        }
                     }
                  }
               }
               _loc3_ = _loc3_ + 1;
            }
            _loc5_ = _loc5_ + 1;
         }
      }
   }
}
