var §\x01§ = 807;
var §\x0f§ = 1;
class com.nitrome.throwgame.GunpowderBarrel extends com.nitrome.throwgame.BoxWeapon
{
   var createMore;
   var x;
   var y;
   function GunpowderBarrel()
   {
      super("gunpowderBarrel");
      this.createMore = 1;
   }
   function explode()
   {
      super.explode();
      new com.nitrome.throwgame.Explosion(this.x,this.y,150,30,null);
   }
}
