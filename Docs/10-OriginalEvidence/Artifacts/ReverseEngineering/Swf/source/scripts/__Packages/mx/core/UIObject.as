function §\x04\x05§()
{
   set("\x03",511 % 511 * true);
   return eval("\x03");
}
var §\x01§ = 263 + "\x04\x05"();
var _loc2_;
while(true)
{
   if(eval("\x01") == 263)
   {
      set("\x01",eval("\x01") + 279);
      §§push(true);
   }
   else if(eval("\x01") == 347)
   {
      set("\x01",eval("\x01") - 250);
   }
   else if(eval("\x01") == 679)
   {
      set("\x01",eval("\x01") - 267);
      if(§§pop())
      {
         set("\x01",eval("\x01") + 362);
      }
   }
   else if(eval("\x01") == 995)
   {
      set("\x01",eval("\x01") - 898);
   }
   else if(eval("\x01") == 734)
   {
      set("\x01",eval("\x01") - 355);
      §§push(eval(§§pop()));
   }
   else
   {
      if(eval("\x01") == 774)
      {
         set("\x01",eval("\x01") + 225);
         if(!_global.mx)
         {
            _global.mx = new Object();
         }
         §§pop();
         if(!_global.mx.core)
         {
            _global.mx.core = new Object();
         }
         §§pop();
         if(!_global.mx.core.UIObject)
         {
            mx.core.UIObject extends MovieClip;
            _loc2_ = mx.core.UIObject = function()
            {
               super();
               this.constructObject();
            }.prototype;
            _loc2_.__get__width = function()
            {
               return this._width;
            };
            _loc2_.__get__height = function()
            {
               return this._height;
            };
            _loc2_.__get__left = function()
            {
               return this._x;
            };
            _loc2_.__get__x = function()
            {
               return this._x;
            };
            _loc2_.__get__top = function()
            {
               return this._y;
            };
            _loc2_.__get__y = function()
            {
               return this._y;
            };
            _loc2_.__get__right = function()
            {
               return this._parent.width - (this._x + this.width);
            };
            _loc2_.__get__bottom = function()
            {
               return this._parent.height - (this._y + this.height);
            };
            _loc2_.getMinHeight = function(Void)
            {
               return this._minHeight;
            };
            _loc2_.setMinHeight = function(h)
            {
               this._minHeight = h;
            };
            _loc2_.__get__minHeight = function()
            {
               return this.getMinHeight();
            };
            _loc2_.__set__minHeight = function(h)
            {
               this.setMinHeight(h);
               return this.minHeight;
            };
            _loc2_.getMinWidth = function(Void)
            {
               return this._minWidth;
            };
            _loc2_.setMinWidth = function(w)
            {
               this._minWidth = w;
            };
            _loc2_.__get__minWidth = function()
            {
               return this.getMinWidth();
            };
            _loc2_.__set__minWidth = function(w)
            {
               this.setMinWidth(w);
               return this.minWidth;
            };
            _loc2_.setVisible = function(x, noEvent)
            {
               if(x != this._visible)
               {
                  this._visible = x;
                  if(noEvent != true)
                  {
                     this.dispatchEvent({type:(!x ? "hide" : "reveal")});
                  }
               }
            };
            _loc2_.__get__visible = function()
            {
               return this._visible;
            };
            _loc2_.__set__visible = function(x)
            {
               this.setVisible(x,false);
               return this.visible;
            };
            _loc2_.__get__scaleX = function()
            {
               return this._xscale;
            };
            _loc2_.__set__scaleX = function(x)
            {
               this._xscale = x;
               return this.scaleX;
            };
            _loc2_.__get__scaleY = function()
            {
               return this._yscale;
            };
            _loc2_.__set__scaleY = function(y)
            {
               this._yscale = y;
               return this.scaleY;
            };
            _loc2_.doLater = function(obj, fn)
            {
               if(this.methodTable == undefined)
               {
                  this.methodTable = new Array();
               }
               this.methodTable.push({obj:obj,fn:fn});
               this.onEnterFrame = this.doLaterDispatcher;
            };
            _loc2_.doLaterDispatcher = function(Void)
            {
               delete this.onEnterFrame;
               if(this.invalidateFlag)
               {
                  this.redraw();
               }
               var _loc3_ = this.methodTable;
               this.methodTable = new Array();
               var _loc2_;
               if(_loc3_.length > 0)
               {
                  while((_loc2_ = _loc3_.shift()) != undefined)
                  {
                     _loc2_.obj[_loc2_.fn]();
                  }
               }
            };
            _loc2_.cancelAllDoLaters = function(Void)
            {
               delete this.onEnterFrame;
               this.methodTable = new Array();
            };
            _loc2_.invalidate = function(Void)
            {
               this.invalidateFlag = true;
               this.onEnterFrame = this.doLaterDispatcher;
            };
            _loc2_.invalidateStyle = function(Void)
            {
               this.invalidate();
            };
            _loc2_.redraw = function(bAlways)
            {
               var _loc2_;
               if(this.invalidateFlag || bAlways)
               {
                  this.invalidateFlag = false;
                  for(_loc2_ in this.tfList)
                  {
                     this.tfList[_loc2_].draw();
                  }
                  this.draw();
                  this.dispatchEvent({type:"draw"});
               }
            };
            _loc2_.draw = function(Void)
            {
            };
            _loc2_.move = function(x, y, noEvent)
            {
               var _loc3_ = this._x;
               var _loc2_ = this._y;
               this._x = x;
               this._y = y;
               if(noEvent != true)
               {
                  this.dispatchEvent({type:"move",oldX:_loc3_,oldY:_loc2_});
               }
            };
            _loc2_.setSize = function(w, h, noEvent)
            {
               var _loc3_ = this.__width;
               var _loc2_ = this.__height;
               this.__width = w;
               this.__height = h;
               this.size();
               if(noEvent != true)
               {
                  this.dispatchEvent({type:"resize",oldWidth:_loc3_,oldHeight:_loc2_});
               }
            };
            _loc2_.size = function(Void)
            {
               this._width = this.__width;
               this._height = this.__height;
            };
            _loc2_.drawRect = function(x1, y1, x2, y2)
            {
               this.moveTo(x1,y1);
               this.lineTo(x2,y1);
               this.lineTo(x2,y2);
               this.lineTo(x1,y2);
               this.lineTo(x1,y1);
            };
            _loc2_.createLabel = function(name, depth, text)
            {
               this.createTextField(name,depth,0,0,0,0);
               var _loc2_ = this[name];
               _loc2_._color = mx.core.UIObject.textColorList;
               _loc2_._visible = false;
               _loc2_.__text = text;
               if(this.tfList == undefined)
               {
                  this.tfList = new Object();
               }
               this.tfList[name] = _loc2_;
               _loc2_.invalidateStyle();
               this.invalidate();
               _loc2_.styleName = this;
               return _loc2_;
            };
            _loc2_.createObject = function(linkageName, id, depth, initobj)
            {
               return this.attachMovie(linkageName,id,depth,initobj);
            };
            _loc2_.createClassObject = function(className, id, depth, initobj)
            {
               var _loc3_ = className.symbolName == undefined;
               if(_loc3_)
               {
                  Object.registerClass(className.symbolOwner.symbolName,className);
               }
               var _loc4_ = mx.core.UIObject(this.createObject(className.symbolOwner.symbolName,id,depth,initobj));
               if(_loc3_)
               {
                  Object.registerClass(className.symbolOwner.symbolName,className.symbolOwner);
               }
               return _loc4_;
            };
            _loc2_.createEmptyObject = function(id, depth)
            {
               return this.createClassObject(mx.core.UIObject,id,depth);
            };
            _loc2_.destroyObject = function(id)
            {
               var _loc2_ = this[id];
               var _loc4_;
               var _loc5_;
               var _loc3_;
               if(_loc2_.getDepth() < 0)
               {
                  _loc4_ = this.buildDepthTable();
                  _loc5_ = this.findNextAvailableDepth(0,_loc4_,"up");
                  _loc3_ = _loc5_;
                  _loc2_.swapDepths(_loc3_);
               }
               _loc2_.removeMovieClip();
               delete this[id];
            };
            _loc2_.getSkinIDName = function(tag)
            {
               return this.idNames[tag];
            };
            _loc2_.setSkin = function(tag, linkageName, initObj)
            {
               if(_global.skinRegistry[linkageName] == undefined)
               {
                  mx.skins.SkinElement.registerElement(linkageName,mx.skins.SkinElement);
               }
               return this.createObject(linkageName,this.getSkinIDName(tag),tag,initObj);
            };
            _loc2_.createSkin = function(tag)
            {
               var _loc2_ = this.getSkinIDName(tag);
               this.createEmptyObject(_loc2_,tag);
               return this[_loc2_];
            };
            _loc2_.createChildren = function(Void)
            {
            };
            _loc2_._createChildren = function(Void)
            {
               this.createChildren();
               this.childrenCreated = true;
            };
            _loc2_.constructObject = function(Void)
            {
               if(this._name == undefined)
               {
                  return undefined;
               }
               this.init();
               this._createChildren();
               this.createAccessibilityImplementation();
               this._endInit();
               if(this.validateNow)
               {
                  this.redraw(true);
               }
               else
               {
                  this.invalidate();
               }
            };
            _loc2_.initFromClipParameters = function(Void)
            {
               var _loc4_ = false;
               var _loc2_;
               for(_loc2_ in this.clipParameters)
               {
                  if(this.hasOwnProperty(_loc2_))
                  {
                     _loc4_ = true;
                     this["def_" + _loc2_] = this[_loc2_];
                     delete this[_loc2_];
                  }
               }
               var _loc3_;
               if(_loc4_)
               {
                  for(_loc2_ in this.clipParameters)
                  {
                     _loc3_ = this["def_" + _loc2_];
                     if(_loc3_ != undefined)
                     {
                        this[_loc2_] = _loc3_;
                     }
                  }
               }
            };
            _loc2_.init = function(Void)
            {
               this.__width = this._width;
               this.__height = this._height;
               if(this.initProperties == undefined)
               {
                  this.initFromClipParameters();
               }
               else
               {
                  this.initProperties();
               }
               if(_global.cascadingStyles == true)
               {
                  this.stylecache = new Object();
               }
            };
            _loc2_.getClassStyleDeclaration = function(Void)
            {
               var _loc4_ = this;
               var _loc3_ = this.className;
               while(_loc3_ != undefined)
               {
                  if(this.ignoreClassStyleDeclaration[_loc3_] == undefined)
                  {
                     if(_global.styles[_loc3_] != undefined)
                     {
                        return _global.styles[_loc3_];
                     }
                  }
                  _loc4_ = _loc4_.__proto__;
                  _loc3_ = _loc4_.className;
               }
            };
            _loc2_.setColor = function(color)
            {
            };
            _loc2_.__getTextFormat = function(tf, bAll)
            {
               var _loc8_ = this.stylecache.tf;
               var _loc3_;
               if(_loc8_ != undefined)
               {
                  for(_loc3_ in mx.styles.StyleManager.TextFormatStyleProps)
                  {
                     if(bAll || mx.styles.StyleManager.TextFormatStyleProps[_loc3_])
                     {
                        if(tf[_loc3_] == undefined)
                        {
                           tf[_loc3_] = _loc8_[_loc3_];
                        }
                     }
                  }
                  return false;
               }
               var _loc6_ = false;
               var _loc5_;
               for(_loc3_ in mx.styles.StyleManager.TextFormatStyleProps)
               {
                  if(bAll || mx.styles.StyleManager.TextFormatStyleProps[_loc3_])
                  {
                     if(tf[_loc3_] == undefined)
                     {
                        _loc5_ = this._tf[_loc3_];
                        if(_loc5_ != undefined)
                        {
                           tf[_loc3_] = _loc5_;
                        }
                        else if(_loc3_ == "font" && this.fontFamily != undefined)
                        {
                           tf[_loc3_] = this.fontFamily;
                        }
                        else if(_loc3_ == "size" && this.fontSize != undefined)
                        {
                           tf[_loc3_] = this.fontSize;
                        }
                        else if(_loc3_ == "color" && this.color != undefined)
                        {
                           tf[_loc3_] = this.color;
                        }
                        else if(_loc3_ == "leftMargin" && this.marginLeft != undefined)
                        {
                           tf[_loc3_] = this.marginLeft;
                        }
                        else if(_loc3_ == "rightMargin" && this.marginRight != undefined)
                        {
                           tf[_loc3_] = this.marginRight;
                        }
                        else if(_loc3_ == "italic" && this.fontStyle != undefined)
                        {
                           tf[_loc3_] = this.fontStyle == _loc3_;
                        }
                        else if(_loc3_ == "bold" && this.fontWeight != undefined)
                        {
                           tf[_loc3_] = this.fontWeight == _loc3_;
                        }
                        else if(_loc3_ == "align" && this.textAlign != undefined)
                        {
                           tf[_loc3_] = this.textAlign;
                        }
                        else if(_loc3_ == "indent" && this.textIndent != undefined)
                        {
                           tf[_loc3_] = this.textIndent;
                        }
                        else if(_loc3_ == "underline" && this.textDecoration != undefined)
                        {
                           tf[_loc3_] = this.textDecoration == _loc3_;
                        }
                        else if(_loc3_ == "embedFonts" && this.embedFonts != undefined)
                        {
                           tf[_loc3_] = this.embedFonts;
                        }
                        else
                        {
                           _loc6_ = true;
                        }
                     }
                  }
               }
               var _loc9_;
               if(_loc6_)
               {
                  _loc9_ = this.styleName;
                  if(_loc9_ != undefined)
                  {
                     if(typeof _loc9_ != "string")
                     {
                        _loc6_ = _loc9_.__getTextFormat(tf,true,this);
                     }
                     else if(_global.styles[_loc9_] != undefined)
                     {
                        _loc6_ = _global.styles[_loc9_].__getTextFormat(tf,true,this);
                     }
                  }
               }
               var _loc10_;
               if(_loc6_)
               {
                  _loc10_ = this.getClassStyleDeclaration();
                  if(_loc10_ != undefined)
                  {
                     _loc6_ = _loc10_.__getTextFormat(tf,true,this);
                  }
               }
               if(_loc6_)
               {
                  if(_global.cascadingStyles)
                  {
                     if(this._parent != undefined)
                     {
                        _loc6_ = this._parent.__getTextFormat(tf,false);
                     }
                  }
               }
               if(_loc6_)
               {
                  _loc6_ = _global.style.__getTextFormat(tf,true,this);
               }
               return _loc6_;
            };
            _loc2_._getTextFormat = function(Void)
            {
               var _loc2_ = this.stylecache.tf;
               if(_loc2_ != undefined)
               {
                  return _loc2_;
               }
               _loc2_ = new TextFormat();
               this.__getTextFormat(_loc2_,true);
               this.stylecache.tf = _loc2_;
               var _loc3_;
               if(this.enabled == false)
               {
                  _loc3_ = this.getStyle("disabledColor");
                  _loc2_.color = _loc3_;
               }
               return _loc2_;
            };
            _loc2_.getStyleName = function(Void)
            {
               var _loc2_ = this.styleName;
               if(_loc2_ != undefined)
               {
                  if(typeof _loc2_ != "string")
                  {
                     return _loc2_.getStyleName();
                  }
                  return _loc2_;
               }
               if(this._parent != undefined)
               {
                  return this._parent.getStyleName();
               }
               return undefined;
            };
            _loc2_.getStyle = function(styleProp)
            {
               var _loc3_;
               _global.getStyleCounter = _global.getStyleCounter + 1;
               if(this[styleProp] != undefined)
               {
                  return this[styleProp];
               }
               var _loc6_ = this.styleName;
               var _loc7_;
               if(_loc6_ != undefined)
               {
                  if(typeof _loc6_ != "string")
                  {
                     _loc3_ = _loc6_.getStyle(styleProp);
                  }
                  else
                  {
                     _loc7_ = _global.styles[_loc6_];
                     _loc3_ = _loc7_.getStyle(styleProp);
                  }
               }
               if(_loc3_ != undefined)
               {
                  return _loc3_;
               }
               _loc7_ = this.getClassStyleDeclaration();
               if(_loc7_ != undefined)
               {
                  _loc3_ = _loc7_[styleProp];
               }
               if(_loc3_ != undefined)
               {
                  return _loc3_;
               }
               var _loc5_;
               if(_global.cascadingStyles)
               {
                  if(mx.styles.StyleManager.isInheritingStyle(styleProp) || mx.styles.StyleManager.isColorStyle(styleProp))
                  {
                     _loc5_ = this.stylecache;
                     if(_loc5_ != undefined)
                     {
                        if(_loc5_[styleProp] != undefined)
                        {
                           return _loc5_[styleProp];
                        }
                     }
                     if(this._parent != undefined)
                     {
                        _loc3_ = this._parent.getStyle(styleProp);
                     }
                     else
                     {
                        _loc3_ = _global.style[styleProp];
                     }
                     if(_loc5_ != undefined)
                     {
                        _loc5_[styleProp] = _loc3_;
                     }
                     return _loc3_;
                  }
               }
               if(_loc3_ == undefined)
               {
                  _loc3_ = _global.style[styleProp];
               }
               return _loc3_;
            };
            mx.core.UIObject = function()
            {
               super();
               this.constructObject();
            }.mergeClipParameters = function(o, p)
            {
               for(var _loc3_ in p)
               {
                  o[_loc3_] = p[_loc3_];
               }
               return true;
            };
            mx.core.UIObject = function()
            {
               super();
               this.constructObject();
            }.symbolName = "UIObject";
            mx.core.UIObject = function()
            {
               super();
               this.constructObject();
            }.symbolOwner = mx.core.UIObject;
            mx.core.UIObject = function()
            {
               super();
               this.constructObject();
            }.version = "2.0.2.127";
            mx.core.UIObject = function()
            {
               super();
               this.constructObject();
            }.textColorList = {color:1,disabledColor:1};
            _loc2_.invalidateFlag = false;
            _loc2_.lineWidth = 1;
            _loc2_.lineColor = 0;
            _loc2_.tabEnabled = false;
            _loc2_.clipParameters = {visible:1,minHeight:1,minWidth:1,maxHeight:1,maxWidth:1,preferredHeight:1,preferredWidth:1};
            §§push(_loc2_.addProperty("bottom",_loc2_.__get__bottom,function()
            {
            }
            ));
            §§push(_loc2_.addProperty("height",_loc2_.__get__height,function()
            {
            }
            ));
            §§push(_loc2_.addProperty("left",_loc2_.__get__left,function()
            {
            }
            ));
            §§push(_loc2_.addProperty("minHeight",_loc2_.__get__minHeight,_loc2_.__set__minHeight));
            §§push(_loc2_.addProperty("minWidth",_loc2_.__get__minWidth,_loc2_.__set__minWidth));
            §§push(_loc2_.addProperty("right",_loc2_.__get__right,function()
            {
            }
            ));
            §§push(_loc2_.addProperty("scaleX",_loc2_.__get__scaleX,_loc2_.__set__scaleX));
            §§push(_loc2_.addProperty("scaleY",_loc2_.__get__scaleY,_loc2_.__set__scaleY));
            §§push(_loc2_.addProperty("top",_loc2_.__get__top,function()
            {
            }
            ));
            §§push(_loc2_.addProperty("visible",_loc2_.__get__visible,_loc2_.__set__visible));
            §§push(_loc2_.addProperty("width",_loc2_.__get__width,function()
            {
            }
            ));
            §§push(_loc2_.addProperty("x",_loc2_.__get__x,function()
            {
            }
            ));
            §§push(_loc2_.addProperty("y",_loc2_.__get__y,function()
            {
            }
            ));
            §§push(ASSetPropFlags(mx.core.UIObject.prototype,null,1));
         }
         §§pop();
         break;
      }
      if(eval("\x01") == 412)
      {
         set("\x01",eval("\x01") + 362);
      }
      else if(eval("\x01") == 955)
      {
         set("\x01",eval("\x01") - 221);
         §§push("\x0f");
      }
      else if(eval("\x01") == 379)
      {
         set("\x01",eval("\x01") + 300);
         §§push(!§§pop());
      }
      else
      {
         if(eval("\x01") == 999)
         {
            set("\x01",eval("\x01") - 999);
            break;
         }
         if(eval("\x01") == 933)
         {
            set("\x01",eval("\x01") + 22);
            var §§pop() = §§pop();
         }
         else if(eval("\x01") == 97)
         {
            set("\x01",eval("\x01") + 836);
            §§push("\x0f");
            §§push(1);
         }
         else
         {
            if(eval("\x01") == 414)
            {
               set("\x01",eval("\x01") - 67);
               prevFrame();
               break;
            }
            if(eval("\x01") != 542)
            {
               break;
            }
            set("\x01",eval("\x01") - 128);
            if(§§pop())
            {
               set("\x01",eval("\x01") - 67);
            }
         }
      }
   }
}
