var §\x01§ = 112;
var §\x0f§ = 1;
class com.nitrome.throwgame.VoodooDoll extends com.nitrome.throwgame.Weapon
{
   var advanceMotion;
   var draggable;
   var drawTwangLine;
   var hitsBoxes;
   var mc;
   var rotation;
   var show;
   var throwVX;
   var throwVY;
   var twangable;
   var velocityX;
   var velocityY;
   var targetCharacter = null;
   var framesOnThis = 10;
   var framesOnTarget = 10;
   var thrownCharacter = false;
   function VoodooDoll()
   {
      super("voodooDoll");
      this.show();
      this.draggable = false;
      this.hitsBoxes = true;
   }
   function setTargetCharacter(character)
   {
      if(!this.fired)
      {
         this.targetCharacter = character;
         this.twangable = true;
         this.mc.useHandCursor = true;
         this.mc.onPress = null;
         com.nitrome.throwgame.Controller.tileSystem.panToCharacter = this.owner;
      }
   }
   function twang()
   {
      super.twang();
      this.throwVX = this.velocityX;
      this.throwVY = this.velocityY;
   }
   function advance()
   {
      if(this.fired && !this.simulation)
      {
         this.rotation += this.velocityX * 2;
      }
      this.advanceMotion();
      this.update();
      if(this.fired)
      {
         if(this.framesOnThis > 0)
         {
            this.framesOnThis--;
            if(this.framesOnThis <= 0)
            {
               com.nitrome.throwgame.Controller.tileSystem.panToCharacter = this.targetCharacter;
               this.track = false;
            }
         }
         else if(com.nitrome.throwgame.Controller.tileSystem.panToCharacter != this.targetCharacter)
         {
            if(this.framesOnTarget > 0)
            {
               this.framesOnTarget--;
            }
            else if(!this.thrownCharacter)
            {
               this.targetCharacter.velocityX = this.throwVX;
               this.targetCharacter.velocityY = this.throwVY;
               this.thrownCharacter = true;
            }
            else
            {
               this.mc._alpha -= 10;
               if(this.mc._alpha <= 0)
               {
                  this.finished = true;
               }
            }
         }
      }
      if(!this.fired && com.nitrome.throwgame.Controller.twanging == this)
      {
         this.drawTwangLine();
      }
   }
   function aiPerform(details)
   {
      this.fired = true;
      this.velocityX = this.throwVX = details.vx;
      this.velocityY = this.throwVY = details.vy;
      this.targetCharacter = details.target;
      _root.sfx_manager.playSound("voodoo");
   }
}
