function §\x04\x05§()
{
   set("\x03",11 % 511 * true);
   return eval("\x03");
}
var §\x01§ = 540 + "\x04\x05"();
var _loc2_;
while(true)
{
   if(eval("\x01") == 551)
   {
      set("\x01",eval("\x01") + 63);
      §§push(true);
   }
   else if(eval("\x01") == 614)
   {
      set("\x01",eval("\x01") - 527);
      if(§§pop())
      {
         set("\x01",eval("\x01") + 719);
      }
   }
   else
   {
      if(eval("\x01") == 87)
      {
         set("\x01",eval("\x01") + 719);
         break;
      }
      if(eval("\x01") == 806)
      {
         set("\x01",eval("\x01") - 304);
      }
      else if(eval("\x01") == 135)
      {
         set("\x01",eval("\x01") - 46);
         §§push("\x0f");
      }
      else if(eval("\x01") == 809)
      {
         set("\x01",eval("\x01") - 27);
         §§push(!§§pop());
      }
      else if(eval("\x01") == 259)
      {
         set("\x01",eval("\x01") + 243);
      }
      else if(eval("\x01") == 782)
      {
         set("\x01",eval("\x01") - 20);
         if(§§pop())
         {
            set("\x01",eval("\x01") + 142);
         }
      }
      else if(eval("\x01") == 315)
      {
         set("\x01",eval("\x01") - 180);
         var §§pop() = §§pop();
      }
      else if(eval("\x01") == 502)
      {
         set("\x01",eval("\x01") - 187);
         §§push("\x0f");
         §§push(1);
      }
      else if(eval("\x01") == 762)
      {
         set("\x01",eval("\x01") + 142);
      }
      else
      {
         if(eval("\x01") == 904)
         {
            set("\x01",eval("\x01") - 630);
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
            if(!_global.mx.core.ext)
            {
               _global.mx.core.ext = new Object();
            }
            §§pop();
            if(!_global.mx.core.ext.UIComponentExtensions)
            {
               _loc2_ = mx.core.ext.UIComponentExtensions = function()
               {
               }.prototype;
               mx.core.ext.UIComponentExtensions = function()
               {
               }.Extensions = function()
               {
                  if(mx.core.ext.UIComponentExtensions.bExtended == true)
                  {
                     return true;
                  }
                  mx.core.ext.UIComponentExtensions.bExtended = true;
                  TextField.prototype.setFocus = function()
                  {
                     Selection.setFocus(this);
                  };
                  TextField.prototype.onSetFocus = function(oldFocus)
                  {
                     if(this.tabEnabled != false)
                     {
                        if(this.getFocusManager().bDrawFocus)
                        {
                           this.drawFocus(true);
                        }
                     }
                  };
                  TextField.prototype.onKillFocus = function(oldFocus)
                  {
                     if(this.tabEnabled != false)
                     {
                        this.drawFocus(false);
                     }
                  };
                  TextField.prototype.drawFocus = mx.core.UIComponent.prototype.drawFocus;
                  TextField.prototype.getFocusManager = mx.core.UIComponent.prototype.getFocusManager;
                  mx.managers.OverlappedWindows.enableOverlappedWindows();
                  mx.styles.CSSSetStyle.enableRunTimeCSS();
                  mx.managers.FocusManager.enableFocusManagement();
               };
               mx.core.ext.UIComponentExtensions = function()
               {
               }.bExtended = false;
               mx.core.ext.UIComponentExtensions = function()
               {
               }.UIComponentExtended = mx.core.ext.UIComponentExtensions.Extensions();
               mx.core.ext.UIComponentExtensions = function()
               {
               }.UIComponentDependency = mx.core.UIComponent;
               mx.core.ext.UIComponentExtensions = function()
               {
               }.FocusManagerDependency = mx.managers.FocusManager;
               mx.core.ext.UIComponentExtensions = function()
               {
               }.OverlappedWindowsDependency = mx.managers.OverlappedWindows;
               §§push(ASSetPropFlags(mx.core.ext.UIComponentExtensions.prototype,null,1));
            }
            §§pop();
            break;
         }
         if(eval("\x01") != 89)
         {
            if(eval("\x01") == 274)
            {
               set("\x01",eval("\x01") - 274);
            }
            break;
         }
         set("\x01",eval("\x01") + 720);
         §§push(eval(§§pop()));
      }
   }
}
