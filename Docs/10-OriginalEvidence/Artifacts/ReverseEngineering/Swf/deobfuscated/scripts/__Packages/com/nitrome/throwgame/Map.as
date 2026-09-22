var §\x01§ = 359;
var §\x0f§ = 1;
class com.nitrome.throwgame.Map extends com.nitrome.util.Clip
{
   var activeClip;
   var tileClip;
   var dotSize = 3;
   function Map()
   {
      super(com.nitrome.throwgame.Controller.root.mapHolder,"map");
      this.show();
      this.reset();
   }
   function show()
   {
      super.show();
      this.tileClip = this.mc.createEmptyMovieClip("tileClip",this.mc.getNextHighestDepth());
      this.activeClip = this.mc.createEmptyMovieClip("activeClip",this.mc.getNextHighestDepth());
   }
   function reset()
   {
      this.drawTiles();
      this.drawActive();
   }
   function drawTiles()
   {
      this.tileClip.clear();
      var _loc3_ = -2;
      var _loc2_;
      while(_loc3_ < com.nitrome.throwgame.Controller.tileSystem.levelWidth + 2)
      {
         _loc2_ = -2;
         while(_loc2_ < com.nitrome.throwgame.Controller.tileSystem.levelHeight + 2)
         {
            if(com.nitrome.throwgame.Controller.tileSystem.tileGrid[_loc3_][_loc2_])
            {
               this.dot(this.tileClip,_loc3_,_loc2_,0,0,50);
            }
            else
            {
               this.dot(this.tileClip,_loc3_,_loc2_,0,0,20);
            }
            _loc2_ = _loc2_ + 1;
         }
         _loc3_ = _loc3_ + 1;
      }
      var _loc5_ = com.nitrome.throwgame.Controller.tileSystem.levelWidth * this.dotSize;
      var _loc4_ = com.nitrome.throwgame.Controller.tileSystem.levelHeight * this.dotSize;
      this.mc.t._xscale = this.mc.b._xscale = (_loc5_ + 12) * 100;
      this.mc.l._yscale = this.mc.r._yscale = (_loc4_ + 12) * 100;
      this.mc.tr._x = this.mc.br._x = this.mc.r._x = com.nitrome.throwgame.Controller.tileSystem.levelWidth * this.dotSize + 11;
      this.mc.bl._y = this.mc.br._y = this.mc.b._y = com.nitrome.throwgame.Controller.tileSystem.levelHeight * this.dotSize + 11;
   }
   function drawActive()
   {
      this.activeClip.clear();
      var _loc8_ = 0;
      var _loc9_;
      var _loc4_;
      var _loc3_;
      while(_loc8_ < com.nitrome.throwgame.Controller.chests.length)
      {
         _loc9_ = com.nitrome.throwgame.Controller.chests[_loc8_];
         _loc4_ = (_loc9_.x * this.dotSize >> 5) / this.dotSize;
         _loc3_ = (_loc9_.y * this.dotSize >> 5) / this.dotSize;
         if(_loc3_ > -2)
         {
            this.dot(this.activeClip,_loc4_,_loc3_,-1,16776960,_loc9_.visibility * 50);
         }
         _loc8_ = _loc8_ + 1;
      }
      var _loc6_ = 0;
      var _loc7_;
      var _loc5_;
      var _loc2_;
      while(_loc6_ < com.nitrome.throwgame.Controller.teams.length)
      {
         _loc7_ = com.nitrome.throwgame.Controller.teams[_loc6_];
         _loc5_ = 0;
         while(_loc5_ < _loc7_.characters.length)
         {
            _loc2_ = _loc7_.characters[_loc5_];
            if(!(!_loc2_.alive && _loc2_.mapVisibility <= 0))
            {
               _loc4_ = (_loc2_.x * this.dotSize >> 5) / this.dotSize;
               if(_loc4_ > -2)
               {
                  if(_loc4_ < com.nitrome.throwgame.Controller.tileSystem.levelWidth + 2)
                  {
                     _loc3_ = (_loc2_.y * this.dotSize >> 5) / this.dotSize;
                     if(_loc3_ > -2)
                     {
                        if(_loc3_ < com.nitrome.throwgame.Controller.tileSystem.levelHeight + 2)
                        {
                           this.dot(this.activeClip,_loc4_,_loc3_,-2,_loc6_ != 0 ? 3366911 : 16726057,_loc2_.mapVisibility * 100);
                        }
                     }
                  }
               }
            }
            _loc5_ = _loc5_ + 1;
         }
         _loc6_ = _loc6_ + 1;
      }
   }
   function dot(target, x, y, offset, color, alpha)
   {
      target.beginFill(color,alpha);
      var _loc3_ = x * this.dotSize + offset;
      var _loc2_ = y * this.dotSize + offset;
      target.moveTo(_loc3_,_loc2_);
      target.lineTo(_loc3_ + this.dotSize,_loc2_);
      target.lineTo(_loc3_ + this.dotSize,_loc2_ + this.dotSize);
      target.lineTo(_loc3_,_loc2_ + this.dotSize);
      target.lineTo(_loc3_,_loc2_);
      target.endFill();
   }
}
