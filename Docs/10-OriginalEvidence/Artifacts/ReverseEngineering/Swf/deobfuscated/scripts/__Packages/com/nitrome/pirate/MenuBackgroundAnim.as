var §\x01§ = 603;
var §\x0f§ = 1;
class com.nitrome.pirate.MenuBackgroundAnim extends MovieClip
{
   var bg;
   var water;
   function MenuBackgroundAnim()
   {
      super();
   }
   function onEnterFrame()
   {
      this.water._x -= 8;
      if(this.water._x <= -64)
      {
         this.water._x = 0;
      }
      this.bg.cloudBase._x -= 2;
      if(this.bg.cloudBase._x <= -550)
      {
         this.bg.cloudBase._x = 0;
      }
      this.bg.frontClouds._x -= 4;
      if(this.bg.frontClouds._x <= -1000)
      {
         this.bg.frontClouds._x = 0;
      }
      this.bg.hills._x -= 2;
      if(this.bg.hills._x <= -840)
      {
         this.bg.hills._x = 0;
      }
      this.bg.backClouds._x -= 1;
      if(this.bg.backClouds._x <= -900)
      {
         this.bg.backClouds._x = 0;
      }
   }
}
