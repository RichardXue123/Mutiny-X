var §\x01§ = 216;
var §\x0f§ = 1;
class com.nitrome.util.Clip
{
   var linkageName;
   var parent;
   var mc = null;
   var mcHolder = null;
   var useHolder = false;
   var x = 0;
   var y = 0;
   var rotation = 0;
   function Clip(parent, linkageName)
   {
      this.parent = parent;
      if(linkageName)
      {
         this.link(linkageName);
      }
   }
   function link(newLinkage)
   {
      if(newLinkage == this.linkageName)
      {
         return undefined;
      }
      if(this.mc)
      {
         this.hide();
      }
      this.linkageName = newLinkage;
   }
   function toString()
   {
      return "[Clip " + this.linkageName + "]";
   }
   function show()
   {
      var _loc2_;
      if(!this.mc)
      {
         _loc2_ = this.parent.getNextHighestDepth();
         if(this.useHolder)
         {
            this.mcHolder = this.parent.createEmptyMovieClip("empty-" + _loc2_.toString(),_loc2_);
            _loc2_ = this.mcHolder.getNextHighestDepth();
            this.mc = this.mcHolder.attachMovie(this.linkageName,this.linkageName + "-" + _loc2_.toString(),_loc2_);
         }
         else
         {
            this.mc = this.parent.attachMovie(this.linkageName,this.linkageName + "-" + _loc2_.toString(),_loc2_);
         }
         this.mc.cl = this;
         this.update();
      }
   }
   function hide()
   {
      if(this.mc)
      {
         this.mc.removeMovieClip();
         this.mc = null;
      }
      if(this.mcHolder)
      {
         this.mcHolder.removeMovieClip();
         this.mcHolder = null;
      }
   }
   function update()
   {
      if(this.mcHolder)
      {
         this.mcHolder._x = this.x << 0;
         this.mcHolder._y = this.y << 0;
         this.mc._rotation = this.rotation;
      }
      else
      {
         this.mc._x = this.x + 0.5 << 0;
         this.mc._y = this.y + 0.5 << 0;
         this.mc._rotation = this.rotation;
      }
   }
   function destroy()
   {
      this.hide();
      this.parent = null;
      this.linkageName = null;
   }
   static function createEmpty(parent, id)
   {
      if(!id)
      {
         id = "empty";
      }
      var _loc1_ = parent.getNextHighestDepth();
      id += "-" + _loc1_.toString();
      return parent.createEmptyMovieClip(id,_loc1_);
   }
}
