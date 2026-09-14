var §\x01§ = 806;
var §\x0f§ = 1;
class DangleFont extends mx.core.UIComponent
{
   var base;
   var boundingBox_mc;
   var createEmptyMovieClip;
   var holder;
   var invalidate;
   var letters;
   var useHandCursor;
   static var symbolName = "DangleFont";
   static var symbolOwner = DangleFont;
   var className = "DangleFont";
   var __text = "enter text";
   var __tracking = 0;
   var __align = "left";
   var __line_spacing = 13;
   function DangleFont()
   {
      super();
   }
   function init()
   {
      super.init();
      this.letters = new Array();
      this.holder = new Array();
      this.useHandCursor = false;
      this.boundingBox_mc._visible = false;
      this.boundingBox_mc._width = 0;
      this.boundingBox_mc._height = 0;
   }
   function get text()
   {
      return this.__text;
   }
   function set text(newText)
   {
      this.__text = newText;
      this.invalidate();
   }
   function get tracking()
   {
      return this.__tracking;
   }
   function set tracking(newTracking)
   {
      this.__tracking = newTracking;
      this.invalidate();
   }
   function get line_spacing()
   {
      return this.__line_spacing;
   }
   function set line_spacing(newLine_spacing)
   {
      this.__line_spacing = newLine_spacing;
      this.invalidate();
   }
   function get align()
   {
      return this.__align;
   }
   function set align(newAlign)
   {
      this.__align = newAlign;
      this.invalidate();
   }
   function draw()
   {
      this.base.removeMovieClip();
      this.base = this.createEmptyMovieClip("base",0);
      this.holder = new Array();
      var _loc3_ = 0;
      this.holder.push(this.base.createEmptyMovieClip("holder" + _loc3_,_loc3_));
      this.letters = new Array();
      this.letters.push(new Array());
      var _loc4_ = 0;
      var _loc7_;
      while(_loc4_ < this.__text.length)
      {
         this.__text = this.__text.toLowerCase();
         _loc7_ = this.__text.charAt(_loc4_);
         switch(_loc7_)
         {
            case " ":
               this.letters[_loc3_].push(this.holder[_loc3_].attachMovie("text_space","t" + _loc4_,_loc4_));
               break;
            case ".":
               this.letters[_loc3_].push(this.holder[_loc3_].attachMovie("text_stop","t" + _loc4_,_loc4_));
               break;
            case ",":
               this.letters[_loc3_].push(this.holder[_loc3_].attachMovie("text_comma","t" + _loc4_,_loc4_));
               break;
            case "-":
               this.letters[_loc3_].push(this.holder[_loc3_].attachMovie("text_hyphen","t" + _loc4_,_loc4_));
               break;
            case "\'":
               this.letters[_loc3_].push(this.holder[_loc3_].attachMovie("text_apostrophe","t" + _loc4_,_loc4_));
               break;
            case "?":
               this.letters[_loc3_].push(this.holder[_loc3_].attachMovie("text_question","t" + _loc4_,_loc4_));
               break;
            case "!":
               this.letters[_loc3_].push(this.holder[_loc3_].attachMovie("text_exclamation","t" + _loc4_,_loc4_));
               break;
            case "\n":
            case "\r":
            case "|":
               _loc3_ = _loc3_ + 1;
               this.letters.push(new Array());
               this.holder.push(this.base.createEmptyMovieClip("holder" + _loc3_,_loc3_));
               this.holder[_loc3_]._y = this.line_spacing * _loc3_;
               break;
            default:
               this.letters[_loc3_].push(this.holder[_loc3_].attachMovie("text_" + _loc7_,"t" + _loc4_,_loc4_));
         }
         _loc4_ = _loc4_ + 1;
      }
      _loc4_ = 0;
      var _loc5_;
      var _loc6_;
      while(_loc4_ < this.letters.length)
      {
         _loc5_ = 1;
         while(_loc5_ < this.letters[_loc4_].length)
         {
            _loc6_ = this.__tracking;
            if(this.letters[_loc4_][_loc5_ - 1].kerning != undefined)
            {
               _loc6_ += this.letters[_loc4_][_loc5_ - 1].kerning._x;
            }
            else
            {
               _loc6_ += this.letters[_loc4_][_loc5_ - 1]._width;
            }
            this.letters[_loc4_][_loc5_]._x = this.letters[_loc4_][_loc5_ - 1]._x + _loc6_;
            _loc5_ = _loc5_ + 1;
         }
         switch(this.__align)
         {
            case "center":
               this.holder[_loc4_]._x -= Math.round(this.holder[_loc4_]._width * 0.5);
               break;
            case "right":
               this.holder[_loc4_]._x -= this.holder[_loc4_]._width;
         }
         _loc4_ = _loc4_ + 1;
      }
      this.base.cacheAsBitmap = true;
      super.draw();
   }
}
