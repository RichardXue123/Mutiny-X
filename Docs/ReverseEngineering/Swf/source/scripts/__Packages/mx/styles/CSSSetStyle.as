function §\x04\x05§()
{
   set("\x03",1308 % 511 * true);
   return eval("\x03");
}
var §\x01§ = -240 + "\x04\x05"();
var _loc2_;
while(true)
{
   if(eval("\x01") == 46)
   {
      set("\x01",eval("\x01") + 576);
      §§push(true);
   }
   else if(eval("\x01") == 441)
   {
      set("\x01",eval("\x01") + 176);
      §§push(!§§pop());
   }
   else if(eval("\x01") == 622)
   {
      set("\x01",eval("\x01") - 46);
      if(§§pop())
      {
         set("\x01",eval("\x01") - 468);
      }
   }
   else if(eval("\x01") == 108)
   {
      set("\x01",eval("\x01") + 55);
   }
   else
   {
      if(eval("\x01") == 576)
      {
         break;
      }
      if(eval("\x01") == 889)
      {
         set("\x01",eval("\x01") - 726);
      }
      else if(eval("\x01") == 163)
      {
         set("\x01",eval("\x01") + 261);
         §§push("\x0f");
         §§push(1);
      }
      else if(eval("\x01") == 687)
      {
         set("\x01",eval("\x01") - 153);
         §§push("\x0f");
      }
      else if(eval("\x01") == 347)
      {
         set("\x01",eval("\x01") + 411);
      }
      else if(eval("\x01") == 424)
      {
         set("\x01",eval("\x01") + 263);
         var §§pop() = §§pop();
      }
      else if(eval("\x01") == 534)
      {
         set("\x01",eval("\x01") - 93);
         §§push(eval(§§pop()));
      }
      else if(eval("\x01") == 617)
      {
         set("\x01",eval("\x01") - 270);
         if(§§pop())
         {
            set("\x01",eval("\x01") + 411);
         }
      }
      else
      {
         if(eval("\x01") == 758)
         {
            set("\x01",eval("\x01") - 305);
            if(!_global.mx)
            {
               _global.mx = new Object();
            }
            §§pop();
            if(!_global.mx.styles)
            {
               _global.mx.styles = new Object();
            }
            §§pop();
            if(!_global.mx.styles.CSSSetStyle)
            {
               _loc2_ = mx.styles.CSSSetStyle = function()
               {
               }.prototype;
               _loc2_._setStyle = function(styleProp, newValue)
               {
                  this[styleProp] = newValue;
                  if(mx.styles.StyleManager.TextStyleMap[styleProp] != undefined)
                  {
                     if(styleProp == "color")
                     {
                        if(isNaN(newValue))
                        {
                           newValue = mx.styles.StyleManager.getColorName(newValue);
                           this[styleProp] = newValue;
                           if(newValue == undefined)
                           {
                              return undefined;
                           }
                        }
                     }
                     _level0.changeTextStyleInChildren(styleProp);
                     return undefined;
                  }
                  var _loc7_;
                  var _loc6_;
                  var _loc8_;
                  var _loc4_;
                  var _loc5_;
                  var _loc9_;
                  var _loc10_;
                  if(mx.styles.StyleManager.isColorStyle(styleProp))
                  {
                     if(isNaN(newValue))
                     {
                        newValue = mx.styles.StyleManager.getColorName(newValue);
                        this[styleProp] = newValue;
                        if(newValue == undefined)
                        {
                           return undefined;
                        }
                     }
                     if(styleProp == "themeColor")
                     {
                        _loc7_ = mx.styles.StyleManager.colorNames.haloBlue;
                        _loc6_ = mx.styles.StyleManager.colorNames.haloGreen;
                        _loc8_ = mx.styles.StyleManager.colorNames.haloOrange;
                        _loc4_ = {};
                        _loc4_[_loc7_] = 12188666;
                        _loc4_[_loc6_] = 13500353;
                        _loc4_[_loc8_] = 16766319;
                        _loc5_ = {};
                        _loc5_[_loc7_] = 13958653;
                        _loc5_[_loc6_] = 14942166;
                        _loc5_[_loc8_] = 16772787;
                        _loc9_ = _loc4_[newValue];
                        _loc10_ = _loc5_[newValue];
                        if(_loc9_ == undefined)
                        {
                           _loc9_ = newValue;
                        }
                        if(_loc10_ == undefined)
                        {
                           _loc10_ = newValue;
                        }
                        this.setStyle("selectionColor",_loc9_);
                        this.setStyle("rollOverColor",_loc10_);
                     }
                     _level0.changeColorStyleInChildren(this.styleName,styleProp,newValue);
                  }
                  else
                  {
                     if(styleProp == "backgroundColor" && isNaN(newValue))
                     {
                        newValue = mx.styles.StyleManager.getColorName(newValue);
                        this[styleProp] = newValue;
                        if(newValue == undefined)
                        {
                           return undefined;
                        }
                     }
                     _level0.notifyStyleChangeInChildren(this.styleName,styleProp,newValue);
                  }
               };
               _loc2_.changeTextStyleInChildren = function(styleProp)
               {
                  var _loc4_ = getTimer();
                  var _loc5_;
                  var _loc2_;
                  for(_loc5_ in this)
                  {
                     _loc2_ = this[_loc5_];
                     if(_loc2_._parent == this)
                     {
                        if(_loc2_.searchKey != _loc4_)
                        {
                           if(_loc2_.stylecache != undefined)
                           {
                              delete _loc2_.stylecache.tf;
                              delete _loc2_.stylecache[styleProp];
                           }
                           _loc2_.invalidateStyle(styleProp);
                           _loc2_.changeTextStyleInChildren(styleProp);
                           _loc2_.searchKey = _loc4_;
                        }
                     }
                  }
               };
               _loc2_.changeColorStyleInChildren = function(sheetName, colorStyle, newValue)
               {
                  var _loc6_ = getTimer();
                  var _loc7_;
                  var _loc2_;
                  var _loc4_;
                  for(_loc7_ in this)
                  {
                     _loc2_ = this[_loc7_];
                     if(_loc2_._parent == this)
                     {
                        if(_loc2_.searchKey != _loc6_)
                        {
                           if(_loc2_.getStyleName() == sheetName || sheetName == undefined || sheetName == "_global")
                           {
                              if(_loc2_.stylecache != undefined)
                              {
                                 delete _loc2_.stylecache[colorStyle];
                              }
                              if(typeof _loc2_._color == "string")
                              {
                                 if(_loc2_._color == colorStyle)
                                 {
                                    _loc4_ = _loc2_.getStyle(colorStyle);
                                    if(colorStyle == "color")
                                    {
                                       if(this.stylecache.tf.color != undefined)
                                       {
                                          this.stylecache.tf.color = _loc4_;
                                       }
                                    }
                                    _loc2_.setColor(_loc4_);
                                 }
                              }
                              else if(_loc2_._color[colorStyle] != undefined)
                              {
                                 if(typeof _loc2_ != "movieclip")
                                 {
                                    _loc2_._parent.invalidateStyle();
                                 }
                                 else
                                 {
                                    _loc2_.invalidateStyle(colorStyle);
                                 }
                              }
                           }
                           _loc2_.changeColorStyleInChildren(sheetName,colorStyle,newValue);
                           _loc2_.searchKey = _loc6_;
                        }
                     }
                  }
               };
               _loc2_.notifyStyleChangeInChildren = function(sheetName, styleProp, newValue)
               {
                  var _loc5_ = getTimer();
                  var _loc6_;
                  var _loc2_;
                  for(_loc6_ in this)
                  {
                     _loc2_ = this[_loc6_];
                     if(_loc2_._parent == this)
                     {
                        if(_loc2_.searchKey != _loc5_)
                        {
                           if(_loc2_.styleName == sheetName || _loc2_.styleName != undefined && typeof _loc2_.styleName == "movieclip" || sheetName == undefined)
                           {
                              if(_loc2_.stylecache != undefined)
                              {
                                 delete _loc2_.stylecache[styleProp];
                                 delete _loc2_.stylecache.tf;
                              }
                              delete _loc2_.enabledColor;
                              _loc2_.invalidateStyle(styleProp);
                           }
                           _loc2_.notifyStyleChangeInChildren(sheetName,styleProp,newValue);
                           _loc2_.searchKey = _loc5_;
                        }
                     }
                  }
               };
               _loc2_.setStyle = function(styleProp, newValue)
               {
                  if(this.stylecache != undefined)
                  {
                     delete this.stylecache[styleProp];
                     delete this.stylecache.tf;
                  }
                  this[styleProp] = newValue;
                  var _loc10_;
                  var _loc9_;
                  var _loc11_;
                  var _loc6_;
                  var _loc7_;
                  var _loc12_;
                  var _loc13_;
                  if(mx.styles.StyleManager.isColorStyle(styleProp))
                  {
                     if(isNaN(newValue))
                     {
                        newValue = mx.styles.StyleManager.getColorName(newValue);
                        this[styleProp] = newValue;
                        if(newValue == undefined)
                        {
                           return undefined;
                        }
                     }
                     if(styleProp == "themeColor")
                     {
                        _loc10_ = mx.styles.StyleManager.colorNames.haloBlue;
                        _loc9_ = mx.styles.StyleManager.colorNames.haloGreen;
                        _loc11_ = mx.styles.StyleManager.colorNames.haloOrange;
                        _loc6_ = {};
                        _loc6_[_loc10_] = 12188666;
                        _loc6_[_loc9_] = 13500353;
                        _loc6_[_loc11_] = 16766319;
                        _loc7_ = {};
                        _loc7_[_loc10_] = 13958653;
                        _loc7_[_loc9_] = 14942166;
                        _loc7_[_loc11_] = 16772787;
                        _loc12_ = _loc6_[newValue];
                        _loc13_ = _loc7_[newValue];
                        if(_loc12_ == undefined)
                        {
                           _loc12_ = newValue;
                        }
                        if(_loc13_ == undefined)
                        {
                           _loc13_ = newValue;
                        }
                        this.setStyle("selectionColor",_loc12_);
                        this.setStyle("rollOverColor",_loc13_);
                     }
                     if(typeof this._color == "string")
                     {
                        if(this._color == styleProp)
                        {
                           if(styleProp == "color")
                           {
                              if(this.stylecache.tf.color != undefined)
                              {
                                 this.stylecache.tf.color = newValue;
                              }
                           }
                           this.setColor(newValue);
                        }
                     }
                     else if(this._color[styleProp] != undefined)
                     {
                        this.invalidateStyle(styleProp);
                     }
                     this.changeColorStyleInChildren(undefined,styleProp,newValue);
                  }
                  else
                  {
                     if(styleProp == "backgroundColor" && isNaN(newValue))
                     {
                        newValue = mx.styles.StyleManager.getColorName(newValue);
                        this[styleProp] = newValue;
                        if(newValue == undefined)
                        {
                           return undefined;
                        }
                     }
                     this.invalidateStyle(styleProp);
                  }
                  var _loc8_;
                  var _loc5_;
                  if(mx.styles.StyleManager.isInheritingStyle(styleProp) || styleProp == "styleName")
                  {
                     _loc5_ = newValue;
                     if(styleProp == "styleName")
                     {
                        _loc8_ = typeof newValue != "string" ? _loc5_ : _global.styles[newValue];
                        _loc5_ = _loc8_.themeColor;
                        if(_loc5_ != undefined)
                        {
                           _loc8_.rollOverColor = _loc8_.selectionColor = _loc5_;
                        }
                     }
                     this.notifyStyleChangeInChildren(undefined,styleProp,newValue);
                  }
               };
               mx.styles.CSSSetStyle = function()
               {
               }.enableRunTimeCSS = function()
               {
               };
               mx.styles.CSSSetStyle = function()
               {
               }.classConstruct = function()
               {
                  var _loc2_ = MovieClip.prototype;
                  var _loc3_ = mx.styles.CSSSetStyle.prototype;
                  mx.styles.CSSStyleDeclaration.prototype.setStyle = _loc3_._setStyle;
                  _loc2_.changeTextStyleInChildren = _loc3_.changeTextStyleInChildren;
                  _loc2_.changeColorStyleInChildren = _loc3_.changeColorStyleInChildren;
                  _loc2_.notifyStyleChangeInChildren = _loc3_.notifyStyleChangeInChildren;
                  _loc2_.setStyle = _loc3_.setStyle;
                  _global.ASSetPropFlags(_loc2_,"changeTextStyleInChildren",1);
                  _global.ASSetPropFlags(_loc2_,"changeColorStyleInChildren",1);
                  _global.ASSetPropFlags(_loc2_,"notifyStyleChangeInChildren",1);
                  _global.ASSetPropFlags(_loc2_,"setStyle",1);
                  var _loc4_ = TextField.prototype;
                  _loc4_.setStyle = _loc2_.setStyle;
                  _loc4_.changeTextStyleInChildren = _loc3_.changeTextStyleInChildren;
                  return true;
               };
               mx.styles.CSSSetStyle = function()
               {
               }.classConstructed = mx.styles.CSSSetStyle.classConstruct();
               mx.styles.CSSSetStyle = function()
               {
               }.CSSStyleDeclarationDependency = mx.styles.CSSStyleDeclaration;
               §§push(ASSetPropFlags(mx.styles.CSSSetStyle.prototype,null,1));
            }
            §§pop();
         }
         else if(eval("\x01") == 453)
         {
            set("\x01",eval("\x01") - 453);
         }
         return;
      }
   }
}
set("\x01",eval("\x01") - 468);
return true;
