var §\x01§ = 34;
var §\x0f§ = 1;
class com.senocular.drawing.DashedLine
{
   var pen;
   var target;
   var _curveaccuracy = 6;
   var isLine = true;
   var overflow = 0;
   var offLength = 0;
   var onLength = 0;
   var dashLength = 0;
   function DashedLine(target, onLength, offLength)
   {
      this.target = target;
      this.setDash(onLength,offLength);
      this.isLine = true;
      this.overflow = 0;
      this.pen = {x:0,y:0};
   }
   function setDash(onLength, offLength)
   {
      this.onLength = onLength;
      this.offLength = offLength;
      this.dashLength = this.onLength + this.offLength;
   }
   function getDash(Void)
   {
      return [this.onLength,this.offLength];
   }
   function moveTo(x, y)
   {
      this.targetMoveTo(x,y);
   }
   function lineTo(x, y)
   {
      var _loc15_ = x - this.pen.x;
      var _loc13_ = y - this.pen.y;
      var _loc14_ = Math.atan2(_loc13_,_loc15_);
      var _loc11_ = Math.cos(_loc14_);
      var _loc9_ = Math.sin(_loc14_);
      var _loc3_ = this.lineLength(_loc15_,_loc13_);
      if(this.overflow)
      {
         if(this.overflow > _loc3_)
         {
            if(this.isLine)
            {
               this.targetLineTo(x,y);
            }
            else
            {
               this.targetMoveTo(x,y);
            }
            this.overflow -= _loc3_;
            return undefined;
         }
         if(this.isLine)
         {
            this.targetLineTo(this.pen.x + _loc11_ * this.overflow,this.pen.y + _loc9_ * this.overflow);
         }
         else
         {
            this.targetMoveTo(this.pen.x + _loc11_ * this.overflow,this.pen.y + _loc9_ * this.overflow);
         }
         _loc3_ -= this.overflow;
         this.overflow = 0;
         this.isLine = !this.isLine;
         if(!_loc3_)
         {
            return undefined;
         }
      }
      var _loc8_ = Math.floor(_loc3_ / this.dashLength);
      var _loc7_;
      var _loc6_;
      var _loc5_;
      var _loc4_;
      var _loc2_;
      if(_loc8_)
      {
         _loc7_ = _loc11_ * this.onLength;
         _loc6_ = _loc9_ * this.onLength;
         _loc5_ = _loc11_ * this.offLength;
         _loc4_ = _loc9_ * this.offLength;
         _loc2_ = 0;
         while(_loc2_ < _loc8_)
         {
            if(this.isLine)
            {
               this.targetLineTo(this.pen.x + _loc7_,this.pen.y + _loc6_);
               this.targetMoveTo(this.pen.x + _loc5_,this.pen.y + _loc4_);
            }
            else
            {
               this.targetMoveTo(this.pen.x + _loc5_,this.pen.y + _loc4_);
               this.targetLineTo(this.pen.x + _loc7_,this.pen.y + _loc6_);
            }
            _loc2_ = _loc2_ + 1;
         }
         _loc3_ -= this.dashLength * _loc8_;
      }
      if(this.isLine)
      {
         if(_loc3_ > this.onLength)
         {
            this.targetLineTo(this.pen.x + _loc11_ * this.onLength,this.pen.y + _loc9_ * this.onLength);
            this.targetMoveTo(x,y);
            this.overflow = this.offLength - (_loc3_ - this.onLength);
            this.isLine = false;
         }
         else
         {
            this.targetLineTo(x,y);
            if(_loc3_ == this.onLength)
            {
               this.overflow = 0;
               this.isLine = !this.isLine;
            }
            else
            {
               this.overflow = this.onLength - _loc3_;
               this.targetMoveTo(x,y);
            }
         }
      }
      else if(_loc3_ > this.offLength)
      {
         this.targetMoveTo(this.pen.x + _loc11_ * this.offLength,this.pen.y + _loc9_ * this.offLength);
         this.targetLineTo(x,y);
         this.overflow = this.onLength - (_loc3_ - this.offLength);
         this.isLine = true;
      }
      else
      {
         this.targetMoveTo(x,y);
         if(_loc3_ == this.offLength)
         {
            this.overflow = 0;
            this.isLine = !this.isLine;
         }
         else
         {
            this.overflow = this.offLength - _loc3_;
         }
      }
   }
   function curveTo(cx, cy, x, y)
   {
      var _loc8_ = this.pen.x;
      var _loc7_ = this.pen.y;
      var _loc14_ = this.curveLength(_loc8_,_loc7_,cx,cy,x,y);
      var _loc3_ = 0;
      var _loc4_ = 0;
      var _loc2_;
      if(this.overflow)
      {
         if(this.overflow > _loc14_)
         {
            if(this.isLine)
            {
               this.targetCurveTo(cx,cy,x,y);
            }
            else
            {
               this.targetMoveTo(x,y);
            }
            this.overflow -= _loc14_;
            return undefined;
         }
         _loc3_ = this.overflow / _loc14_;
         _loc2_ = this.curveSliceUpTo(_loc8_,_loc7_,cx,cy,x,y,_loc3_);
         if(this.isLine)
         {
            this.targetCurveTo(_loc2_[2],_loc2_[3],_loc2_[4],_loc2_[5]);
         }
         else
         {
            this.targetMoveTo(_loc2_[4],_loc2_[5]);
         }
         this.overflow = 0;
         this.isLine = !this.isLine;
         if(!_loc14_)
         {
            return undefined;
         }
      }
      var _loc15_ = _loc14_ - _loc14_ * _loc3_;
      var _loc16_ = Math.floor(_loc15_ / this.dashLength);
      var _loc12_ = this.onLength / _loc14_;
      var _loc13_ = this.offLength / _loc14_;
      var _loc11_;
      if(_loc16_)
      {
         _loc11_ = 0;
         while(_loc11_ < _loc16_)
         {
            if(this.isLine)
            {
               _loc4_ = _loc3_ + _loc12_;
               _loc2_ = this.curveSlice(_loc8_,_loc7_,cx,cy,x,y,_loc3_,_loc4_);
               this.targetCurveTo(_loc2_[2],_loc2_[3],_loc2_[4],_loc2_[5]);
               _loc3_ = _loc4_;
               _loc4_ = _loc3_ + _loc13_;
               _loc2_ = this.curveSlice(_loc8_,_loc7_,cx,cy,x,y,_loc3_,_loc4_);
               this.targetMoveTo(_loc2_[4],_loc2_[5]);
            }
            else
            {
               _loc4_ = _loc3_ + _loc13_;
               _loc2_ = this.curveSlice(_loc8_,_loc7_,cx,cy,x,y,_loc3_,_loc4_);
               this.targetMoveTo(_loc2_[4],_loc2_[5]);
               _loc3_ = _loc4_;
               _loc4_ = _loc3_ + _loc12_;
               _loc2_ = this.curveSlice(_loc8_,_loc7_,cx,cy,x,y,_loc3_,_loc4_);
               this.targetCurveTo(_loc2_[2],_loc2_[3],_loc2_[4],_loc2_[5]);
            }
            _loc3_ = _loc4_;
            _loc11_ = _loc11_ + 1;
         }
      }
      _loc15_ = _loc14_ - _loc14_ * _loc3_;
      if(this.isLine)
      {
         if(_loc15_ > this.onLength)
         {
            _loc4_ = _loc3_ + _loc12_;
            _loc2_ = this.curveSlice(_loc8_,_loc7_,cx,cy,x,y,_loc3_,_loc4_);
            this.targetCurveTo(_loc2_[2],_loc2_[3],_loc2_[4],_loc2_[5]);
            this.targetMoveTo(x,y);
            this.overflow = this.offLength - (_loc15_ - this.onLength);
            this.isLine = false;
         }
         else
         {
            _loc2_ = this.curveSliceFrom(_loc8_,_loc7_,cx,cy,x,y,_loc3_);
            this.targetCurveTo(_loc2_[2],_loc2_[3],_loc2_[4],_loc2_[5]);
            if(_loc14_ == this.onLength)
            {
               this.overflow = 0;
               this.isLine = !this.isLine;
            }
            else
            {
               this.overflow = this.onLength - _loc15_;
               this.targetMoveTo(x,y);
            }
         }
      }
      else if(_loc15_ > this.offLength)
      {
         _loc4_ = _loc3_ + _loc13_;
         _loc2_ = this.curveSlice(_loc8_,_loc7_,cx,cy,x,y,_loc3_,_loc4_);
         this.targetMoveTo(_loc2_[4],_loc2_[5]);
         _loc2_ = this.curveSliceFrom(_loc8_,_loc7_,cx,cy,x,y,_loc4_);
         this.targetCurveTo(_loc2_[2],_loc2_[3],_loc2_[4],_loc2_[5]);
         this.overflow = this.onLength - (_loc15_ - this.offLength);
         this.isLine = true;
      }
      else
      {
         this.targetMoveTo(x,y);
         if(_loc15_ == this.offLength)
         {
            this.overflow = 0;
            this.isLine = !this.isLine;
         }
         else
         {
            this.overflow = this.offLength - _loc15_;
         }
      }
   }
   function clear(Void)
   {
      this.target.clear();
   }
   function lineStyle(thickness, rgb, alpha)
   {
      this.target.lineStyle(thickness,rgb,alpha);
   }
   function beginFill(rgb, alpha)
   {
      this.target.beginFill(rgb,alpha);
   }
   function beginGradientFill(fillType, colors, alphas, ratios, matrix)
   {
      this.target.beginGradientFill(fillType,colors,alphas,ratios,matrix);
   }
   function endFill(Void)
   {
      this.target.endFill();
   }
   function lineLength(sx, sy, ex, ey)
   {
      if(arguments.length == 2)
      {
         return Math.sqrt(sx * sx + sy * sy);
      }
      var _loc3_ = ex - sx;
      var _loc2_ = ey - sy;
      return Math.sqrt(_loc3_ * _loc3_ + _loc2_ * _loc2_);
   }
   function curveLength(sx, sy, cx, cy, ex, ey, accuracy)
   {
      var _loc13_ = 0;
      var _loc11_ = sx;
      var _loc10_ = sy;
      var _loc9_;
      var _loc8_;
      var _loc2_;
      var _loc4_;
      var _loc7_;
      var _loc6_;
      var _loc5_;
      var _loc12_ = !accuracy ? this._curveaccuracy : accuracy;
      var _loc3_ = 1;
      while(_loc3_ <= _loc12_)
      {
         _loc2_ = _loc3_ / _loc12_;
         _loc4_ = 1 - _loc2_;
         _loc7_ = _loc4_ * _loc4_;
         _loc6_ = 2 * _loc2_ * _loc4_;
         _loc5_ = _loc2_ * _loc2_;
         _loc9_ = _loc7_ * sx + _loc6_ * cx + _loc5_ * ex;
         _loc8_ = _loc7_ * sy + _loc6_ * cy + _loc5_ * ey;
         _loc13_ += this.lineLength(_loc11_,_loc10_,_loc9_,_loc8_);
         _loc11_ = _loc9_;
         _loc10_ = _loc8_;
         _loc3_ = _loc3_ + 1;
      }
      return _loc13_;
   }
   function curveSlice(sx, sy, cx, cy, ex, ey, t1, t2)
   {
      if(t1 == 0)
      {
         return this.curveSliceUpTo(sx,sy,cx,cy,ex,ey,t2);
      }
      if(t2 == 1)
      {
         return this.curveSliceFrom(sx,sy,cx,cy,ex,ey,t1);
      }
      var _loc2_ = this.curveSliceUpTo(sx,sy,cx,cy,ex,ey,t2);
      _loc2_.push(t1 / t2);
      return this.curveSliceFrom.apply(this,_loc2_);
   }
   function curveSliceUpTo(sx, sy, cx, cy, ex, ey, t)
   {
      if(t == undefined)
      {
         t = 1;
      }
      var _loc5_;
      var _loc4_;
      if(t != 1)
      {
         _loc5_ = cx + (ex - cx) * t;
         _loc4_ = cy + (ey - cy) * t;
         cx = sx + (cx - sx) * t;
         cy = sy + (cy - sy) * t;
         ex = cx + (_loc5_ - cx) * t;
         ey = cy + (_loc4_ - cy) * t;
      }
      return [sx,sy,cx,cy,ex,ey];
   }
   function curveSliceFrom(sx, sy, cx, cy, ex, ey, t)
   {
      if(t == undefined)
      {
         t = 1;
      }
      var _loc5_;
      var _loc4_;
      if(t != 1)
      {
         _loc5_ = sx + (cx - sx) * t;
         _loc4_ = sy + (cy - sy) * t;
         cx += (ex - cx) * t;
         cy += (ey - cy) * t;
         sx = _loc5_ + (cx - _loc5_) * t;
         sy = _loc4_ + (cy - _loc4_) * t;
      }
      return [sx,sy,cx,cy,ex,ey];
   }
   function targetMoveTo(x, y)
   {
      this.pen = {x:x,y:y};
      this.target.moveTo(x,y);
   }
   function targetLineTo(x, y)
   {
      if(x == this.pen.x && y == this.pen.y)
      {
         return undefined;
      }
      this.pen = {x:x,y:y};
      this.target.lineTo(x,y);
   }
   function targetCurveTo(cx, cy, x, y)
   {
      if(cx == x && cy == y && x == this.pen.x && y == this.pen.y)
      {
         return undefined;
      }
      this.pen = {x:x,y:y};
      this.target.curveTo(cx,cy,x,y);
   }
}
