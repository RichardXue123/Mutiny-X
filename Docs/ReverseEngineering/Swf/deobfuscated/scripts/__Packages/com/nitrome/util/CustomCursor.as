var §\x01§ = 722;
var §\x0f§ = 1;
class com.nitrome.util.CustomCursor extends MovieClip
{
   static var currentCursor = "";
   function CustomCursor()
   {
      super();
   }
   static function setCursor(cursorType, force)
   {
      if(com.nitrome.util.CustomCursor.currentCursor == cursorType && !force)
      {
         return undefined;
      }
      if(cursorType == "")
      {
         com.nitrome.util.CustomCursor.restoreCursor(force);
         return undefined;
      }
      if(com.nitrome.throwgame.Controller.root.cursor)
      {
         com.nitrome.throwgame.Controller.root.cursor._visible = true;
         com.nitrome.throwgame.Controller.root.cursor.gotoAndStop(cursorType);
         Mouse.hide();
      }
      com.nitrome.util.CustomCursor.currentCursor = cursorType;
   }
   static function restoreCursor(force)
   {
      if(com.nitrome.util.CustomCursor.currentCursor == "" && !force)
      {
         return undefined;
      }
      com.nitrome.throwgame.Controller.root.cursor._visible = false;
      Mouse.show();
      com.nitrome.util.CustomCursor.currentCursor = "";
   }
   function onLoad()
   {
      com.nitrome.util.CustomCursor.setCursor(com.nitrome.util.CustomCursor.currentCursor,true);
   }
   function onEnterFrame()
   {
      this._x = this._parent._xmouse;
      this._y = this._parent._ymouse;
   }
}
