var §\x01§ = 287;
var §\x0f§ = 1;
class com.nitrome.throwgame.Boulder extends com.nitrome.throwgame.Weapon
{
   var bottomExtent;
   var draggable;
   var friction;
   var hitsBoxes;
   var leftExtent;
   var mc;
   var rightExtent;
   var show;
   var topExtent;
   var twangable;
   var velocityX;
   var velocityY;
   var weight;
   var x;
   var y;
   var visibility = 2;
   function Boulder()
   {
      super("boulder");
      this.leftExtent = this.rightExtent = this.topExtent = this.bottomExtent = 31;
      this.show();
      this.friction = 0.25;
      this.weight = 1.5;
      this.hitsBoxes = true;
      this.mc.blendMode = "layer";
      this.draggable = false;
      this.twangable = true;
   }
   function release()
   {
      super.release();
      this.velocityX *= 0.5;
      this.velocityY *= 0.5;
   }
   function advanceMotion()
   {
      if(this.fired)
      {
         this.mc.rotating._rotation += this.velocityX * 2.5;
      }
      super.advanceMotion();
      var _loc6_;
      var _loc5_;
      var _loc4_;
      var _loc3_;
      if(this.fired)
      {
         if(this.velocityX == 0 && Math.abs(this.velocityY) < 0.5)
         {
            if(this.simulation)
            {
               this.simulationFinished = true;
            }
            else
            {
               this.visibility -= 0.1;
               if(this.visibility < 1)
               {
                  this.mc.transform.colorTransform = com.nitrome.util.Global.whiteOut(this.visibility);
               }
               if(this.visibility < 0)
               {
                  this.finished = true;
               }
            }
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
                  if(_loc3_ != this.owner)
                  {
                     if(_loc3_.y - _loc3_.topExtent <= this.y + 32)
                     {
                        if(_loc3_.y + _loc3_.bottomExtent >= this.y - 32)
                        {
                           if(Math.abs(_loc3_.x - this.x) <= 32)
                           {
                              if(_loc3_.x > this.x)
                              {
                                 _loc3_.x = this.x + 32;
                                 if(this.velocityX > 0)
                                 {
                                    _loc3_.velocityX += this.velocityX;
                                 }
                              }
                              else
                              {
                                 _loc3_.x = this.x - 32;
                                 if(this.velocityX < 0)
                                 {
                                    _loc3_.velocityX += this.velocityX;
                                 }
                              }
                              _loc3_.subtractHealth(Math.abs(this.velocityX) * 1.5);
                           }
                        }
                     }
                  }
               }
               _loc4_ = _loc4_ + 1;
            }
            _loc6_ = _loc6_ + 1;
         }
      }
   }
}
