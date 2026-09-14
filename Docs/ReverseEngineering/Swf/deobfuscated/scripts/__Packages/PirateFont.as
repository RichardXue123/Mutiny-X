var §\x01§ = 450;
var §\x0f§ = 1;
class PirateFont extends mx.core.UIComponent
{
   var base;
   var boundingBox_mc;
   var createEmptyMovieClip;
   var holder;
   var invalidate;
   var letters;
   var useHandCursor;
   static var symbolName = "PirateFont";
   static var symbolOwner = PirateFont;
   var className = "PirateFont";
   var __text = "enter text";
   var __tracking = 1;
   var __centered = false;
   var __line_spacing = 9;
   function PirateFont()
   {
      super();
      _global.useFocusRect = false;
   }
   function init()
   {
      super.init();
      this.letters = new Array();
      this.holder = new Array();
      this.useHandCursor = false;
      this.boundingBox_mc._visible = false;
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
   function get centered()
   {
      return this.__centered;
   }
   function set centered(newCentered)
   {
      this.__centered = newCentered;
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
      var _loc5_ = 0;
      var _loc7_;
      while(_loc5_ < this.__text.length)
      {
         this.__text = this.__text.toLowerCase();
         _loc7_ = this.__text.charAt(_loc5_);
         switch(_loc7_)
         {
            case " ":
               this.letters[_loc3_].push(this.holder[_loc3_].attachMovie("pir_space","t" + _loc5_,_loc5_));
               break;
            case ".":
               this.letters[_loc3_].push(this.holder[_loc3_].attachMovie("pir_stop","t" + _loc5_,_loc5_));
               break;
            case ",":
               this.letters[_loc3_].push(this.holder[_loc3_].attachMovie("pir_comma","t" + _loc5_,_loc5_));
               break;
            case "?":
               this.letters[_loc3_].push(this.holder[_loc3_].attachMovie("pir_question","t" + _loc5_,_loc5_));
               break;
            case "!":
               this.letters[_loc3_].push(this.holder[_loc3_].attachMovie("pir_exclamation","t" + _loc5_,_loc5_));
               break;
            case "-":
               this.letters[_loc3_].push(this.holder[_loc3_].attachMovie("pir_hyphen","t" + _loc5_,_loc5_));
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
               this.letters[_loc3_].push(this.holder[_loc3_].attachMovie("pir_" + _loc7_,"t" + _loc5_,_loc5_));
         }
         _loc5_ = _loc5_ + 1;
      }
      _loc5_ = 0;
      var _loc4_;
      var _loc6_;
      while(_loc5_ < this.letters.length)
      {
         _loc4_ = 1;
         while(_loc4_ < this.letters[_loc5_].length)
         {
            _loc6_ = this.__tracking;
            if(this.letters[_loc5_][_loc4_ - 1].kerning != undefined)
            {
               _loc6_ += this.letters[_loc5_][_loc4_ - 1].kerning._x;
            }
            else
            {
               _loc6_ += this.letters[_loc5_][_loc4_ - 1]._width;
            }
            this.letters[_loc5_][_loc4_]._x = this.letters[_loc5_][_loc4_ - 1]._x + _loc6_;
            _loc4_ = _loc4_ + 1;
         }
         if(this.__centered)
         {
            this.holder[_loc5_]._x -= Math.round(this.holder[_loc5_]._width * 0.5);
         }
         _loc5_ = _loc5_ + 1;
      }
      super.draw();
   }
}
