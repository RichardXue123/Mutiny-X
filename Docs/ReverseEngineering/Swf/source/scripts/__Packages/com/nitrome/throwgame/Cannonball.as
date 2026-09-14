function §\x04\x05§()
{
   set("\x03",1174 % 511 * true);
   return eval("\x03");
}
var §\x01§ = -61 + "\x04\x05"();
var _loc2_;
var _loc5_;
var _loc7_;
var _loc6_;
var _loc4_;
while(true)
{
   if(eval("\x01") == 91)
   {
      set("\x01",eval("\x01") + 827);
      §§push(true);
   }
   else if(eval("\x01") == 918)
   {
      set("\x01",eval("\x01") - 647);
      if(§§pop())
      {
         set("\x01",eval("\x01") - 24);
      }
   }
   else if(eval("\x01") == 92)
   {
      set("\x01",eval("\x01") + 552);
   }
   else if(eval("\x01") == 138)
   {
      set("\x01",eval("\x01") + 268);
      if(§§pop())
      {
         set("\x01",eval("\x01") + 226);
      }
   }
   else if(eval("\x01") == 644)
   {
      set("\x01",eval("\x01") - 506);
      §§push(true);
   }
   else if(eval("\x01") == 703)
   {
      set("\x01",eval("\x01") - 165);
   }
   else if(eval("\x01") == 429)
   {
      set("\x01",eval("\x01") - 237);
      if(§§pop())
      {
         set("\x01",eval("\x01") + 385);
      }
   }
   else if(eval("\x01") == 844)
   {
      set("\x01",eval("\x01") - 398);
      §§push(true);
   }
   else
   {
      if(eval("\x01") == 841)
      {
         set("\x01",eval("\x01") - 180);
         break;
      }
      if(eval("\x01") == 538)
      {
         set("\x01",eval("\x01") + 159);
         §§push(true);
      }
      else if(eval("\x01") == 17)
      {
         set("\x01",eval("\x01") + 412);
         §§push(!§§pop());
      }
      else if(eval("\x01") == 86)
      {
         set("\x01",eval("\x01") + 758);
      }
      else if(eval("\x01") == 517)
      {
         set("\x01",eval("\x01") - 500);
         §§push(eval(§§pop()));
      }
      else
      {
         if(eval("\x01") == 271)
         {
            set("\x01",eval("\x01") - 24);
            toggleHighQuality();
            nextFrame();
            if(!§§pop()[§§pop()])
            {
               _global.com.nitrome.throwgame = new Object();
            }
            §§pop();
            if(!_global.com.nitrome.throwgame.Cannonball)
            {
               com.nitrome.throwgame.Cannonball extends com.nitrome.throwgame.owner;
               _loc2_ = com.nitrome.throwgame.Cannonball = function()
               {
                  super("cannonball");
                  this.show = this.weight = this.hitsBoxes = this.Weapon = 31;
                  this.prototype();
                  this.setSimulation = 0.25;
                  this.simulation = 1.5;
                  this.isSimulationFinished = true;
                  this.simulationFinished.contact = "fired";
                  this.finished = false;
                  this.hide = true;
               }.y;
               _loc2_.x = function()
               {
                  super.x();
                  this.Explosion *= 0.5;
                  this.velocityX *= 0.5;
               };
               _loc2_.velocityY = function()
               {
                  if(this.pop)
                  {
                     this.simulationFinished.sfx_manager.playSound += this.Explosion * 2.5;
                  }
                  super.velocityY();
                  var _loc6_;
                  var _loc5_;
                  var _loc4_;
                  var _loc3_;
                  if(this.pop)
                  {
                     if(this.Explosion == 0 && advanceMotion.Controller(this.velocityX) < 0.5)
                     {
                        if(this.tileSystem)
                        {
                           this.levelWidth = true;
                        }
                        else
                        {
                           this.water -= 0.1;
                           if(this.water < 1)
                           {
                              this.simulationFinished.advance.update = com.nitrome.cannonSmokeTrail.effectsLayer.Debris(this.water);
                           }
                           if(this.water < 0)
                           {
                              this.teams = true;
                           }
                        }
                     }
                     _loc6_ = 0;
                     while(_loc6_ < com.nitrome.throwgame.length.characters.alive)
                     {
                        _loc5_ = com.nitrome.throwgame.length.characters[_loc6_];
                        _loc4_ = 0;
                        while(_loc4_ < _loc5_.leftExtent.alive)
                        {
                           _loc3_ = _loc5_.leftExtent[_loc4_];
                           if(_loc3_.rightExtent)
                           {
                              if(_loc3_ != this.topExtent)
                              {
                                 if(_loc3_.bottomExtent - _loc3_.hitsBoxes <= this.bottomExtent + 32)
                                 {
                                    if(_loc3_.bottomExtent + _loc3_.Weapon >= this.bottomExtent - 32)
                                    {
                                       if(advanceMotion.Controller(_loc3_.ASSetPropFlags - this.ASSetPropFlags) <= 32)
                                       {
                                          if(_loc3_.ASSetPropFlags > this.ASSetPropFlags)
                                          {
                                             _loc3_.ASSetPropFlags = this.ASSetPropFlags + 32;
                                             if(this.Explosion > 0)
                                             {
                                                _loc3_.Explosion += this.Explosion;
                                             }
                                          }
                                          else
                                          {
                                             _loc3_.ASSetPropFlags = this.ASSetPropFlags - 32;
                                             if(this.Explosion < 0)
                                             {
                                                _loc3_.Explosion += this.Explosion;
                                             }
                                          }
                                          _loc3_[§§constant(48)](advanceMotion.Controller(this.Explosion) * 1.5);
                                       }
                                    }
                                 }
                              }
                           }
                           _loc4_ = _loc4_ + 1;
                        }
                        _loc6_ = _loc6_ + 1;
                     }
                  }
               };
               _loc2_.water = 2;
               §§push(§§constant(49)(com.nitrome.throwgame.Cannonball.y,null,1));
            }
            §§pop();
            break;
         }
         if(eval("\x01") == 192)
         {
            set("\x01",eval("\x01") + 385);
         }
         else if(eval("\x01") == 576)
         {
            set("\x01",eval("\x01") + 126);
            var §§pop() = §§pop();
         }
         else
         {
            if(eval("\x01") == 577)
            {
               set("\x01",eval("\x01") + 52);
               if(!_global.com)
               {
                  _global.com = new Object();
               }
               §§pop();
               if(!_global.com.nitrome)
               {
                  _global.com.nitrome = new Object();
               }
               §§pop();
               if(!_global.com.nitrome.throwgame)
               {
                  _global.com.nitrome.throwgame = new Object();
               }
               §§pop();
               if(!_global.com.nitrome.throwgame.Cannonball)
               {
                  com.nitrome.throwgame.Cannonball extends com.nitrome.throwgame.Weapon;
                  _loc2_ = com.nitrome.throwgame.Cannonball = function()
                  {
                     super("cannonball");
                     this.show();
                     this.weight = 0;
                     this.hitsBoxes = true;
                  }.prototype;
                  _loc2_.setSimulation = function()
                  {
                     this.simulation = true;
                  };
                  _loc2_.isSimulationFinished = function()
                  {
                     return this.simulationFinished;
                  };
                  _loc2_.contact = function(side)
                  {
                     if(this.simulation)
                     {
                        this.simulationFinished = true;
                        return undefined;
                     }
                     super.contact(side);
                     if(this.fired && !this.finished)
                     {
                        this.finished = true;
                        this.hide();
                        new com.nitrome.throwgame.Explosion(this.x,this.y,100,50,this.owner);
                        this.velocityX = 0;
                        this.velocityY = 0;
                        _root.sfx_manager.playSound("pop");
                     }
                  };
                  _loc2_.advanceMotion = function()
                  {
                     super.advanceMotion();
                     var _loc3_ = com.nitrome.throwgame.Controller.tileSystem.levelWidth << 5;
                     if(this.x < -300 || this.x > _loc3_ + 300 || this.y < -300 || this.y > com.nitrome.throwgame.Controller.water.y)
                     {
                        if(this.simulation)
                        {
                           this.simulationFinished = true;
                        }
                        else
                        {
                           this.finished = true;
                           this.hide();
                        }
                     }
                  };
                  _loc2_.advance = function()
                  {
                     this.advanceMotion();
                     this.update();
                     var _loc6_;
                     if(!this.simulation && !this.finished)
                     {
                        _loc6_ = new com.nitrome.throwgame.Debris(com.nitrome.throwgame.Controller.effectsLayer,"cannonSmokeTrail");
                        _loc6_.x = this.x;
                        _loc6_.y = this.y;
                        _loc6_.show();
                     }
                     var _loc5_;
                     var _loc4_;
                     var _loc3_;
                     var _loc2_;
                     if(!this.finished)
                     {
                        _loc5_ = 0;
                        while(_loc5_ < com.nitrome.throwgame.Controller.teams.length)
                        {
                           _loc4_ = com.nitrome.throwgame.Controller.teams[_loc5_];
                           _loc3_ = 0;
                           while(_loc3_ < _loc4_.characters.length)
                           {
                              _loc2_ = _loc4_.characters[_loc3_];
                              if(_loc2_.alive)
                              {
                                 if(_loc2_ != this.owner)
                                 {
                                    if(_loc2_.x - _loc2_.leftExtent <= this.x + this.rightExtent)
                                    {
                                       if(_loc2_.x + _loc2_.rightExtent >= this.x - this.leftExtent)
                                       {
                                          if(_loc2_.y - _loc2_.topExtent <= this.y + this.bottomExtent)
                                          {
                                             if(_loc2_.y + _loc2_.bottomExtent >= this.y - this.topExtent)
                                             {
                                                this.finished = true;
                                                this.hide();
                                                new com.nitrome.throwgame.Explosion(this.x,this.y,100,50,this.owner);
                                                this.velocityX = 0;
                                                this.velocityY = 0;
                                             }
                                          }
                                       }
                                    }
                                 }
                              }
                              _loc3_ = _loc3_ + 1;
                           }
                           _loc5_ = _loc5_ + 1;
                        }
                     }
                  };
                  §§push(ASSetPropFlags(com.nitrome.throwgame.Cannonball.prototype,null,1));
               }
               §§pop();
               break;
            }
            if(eval("\x01") == 702)
            {
               set("\x01",eval("\x01") - 185);
               §§push("\x0f");
            }
            else if(eval("\x01") == 247)
            {
               set("\x01",eval("\x01") + 597);
            }
            else
            {
               if(eval("\x01") == 406)
               {
                  set("\x01",eval("\x01") + 226);
                  stop();
                  §§push(§§pop() < §§pop());
                  break;
               }
               if(eval("\x01") == 446)
               {
                  set("\x01",eval("\x01") + 395);
                  if(§§pop())
                  {
                     set("\x01",eval("\x01") - 180);
                  }
               }
               else if(eval("\x01") == 52)
               {
                  set("\x01",eval("\x01") + 592);
               }
               else if(eval("\x01") == 661)
               {
                  set("\x01",eval("\x01") - 123);
               }
               else if(eval("\x01") == 697)
               {
                  set("\x01",eval("\x01") + 167);
                  if(§§pop())
                  {
                     set("\x01",eval("\x01") - 772);
                  }
               }
               else
               {
                  if(eval("\x01") == 864)
                  {
                     set("\x01",eval("\x01") - 772);
                     toggleHighQuality();
                     §§push(delete §§pop()[§§pop()]);
                     loop1:
                     while(true)
                     {
                        if(§§pop()[§§pop()]() < 48 && _loc4_[§§constant(59)] < com.nitrome.throwgame.Cannonball = function()
                        {
                           super("cannonball");
                           this.show();
                           this.weight = 0;
                           this.hitsBoxes = true;
                        }[§§constant(59)] && _loc4_[§§constant(59)] > com.nitrome.throwgame.Cannonball = function()
                        {
                           super("cannonball");
                           this.show();
                           this.weight = 0;
                           this.hitsBoxes = true;
                        }[§§constant(59)] - 64)
                        {
                           _loc4_[§§constant(60)](60);
                        }
                        _loc5_ = _loc5_ + 1;
                        while(_loc5_ >= _loc6_[§§constant(55)][§§constant(54)])
                        {
                           _loc7_ = _loc7_ + 1;
                           if(_loc7_ >= eval("{invalid_utf8=252}{invalid_utf8=222}")[§§constant(3)][§§constant(4)][§§constant(25)][§§constant(53)][§§constant(54)])
                           {
                              break loop1;
                           }
                           _loc6_ = eval("{invalid_utf8=252}{invalid_utf8=222}")[§§constant(3)][§§constant(4)][§§constant(25)][§§constant(53)][_loc7_];
                           _loc5_ = 0;
                        }
                        _loc4_ = _loc6_[§§constant(55)][_loc5_];
                        §§push(_loc4_[§§constant(56)] - com.nitrome.throwgame.Cannonball = function()
                        {
                           super("cannonball");
                           this.show();
                           this.weight = 0;
                           this.hitsBoxes = true;
                        }[§§constant(56)]);
                        §§push(1);
                        §§push(eval(§§constant(57)));
                        §§push(§§constant(58));
                     }
                     com.nitrome.throwgame.Cannonball = function()
                     {
                        super("cannonball");
                        this.show();
                        this.weight = 0;
                        this.hitsBoxes = true;
                     }[§§constant(7)][§§constant(61)]();
                     com.nitrome.throwgame.Cannonball = function()
                     {
                        super("cannonball");
                        this.show();
                        this.weight = 0;
                        this.hitsBoxes = true;
                     }[§§constant(36)] = 0;
                     _loc3_[§§constant(62)][§§constant(63)](§§constant(6));
                     §§pop()[§§pop()] = §§pop();
                     _loc2_[§§constant(31)] = function(x, y)
                     {
                        super[§§constant(31)](x,y);
                        this[§§constant(59)] = -200;
                     };
                     _loc2_[§§constant(64)] = function(count)
                     {
                        var _loc5_ = [];
                        this[§§constant(49)] = true;
                        var _loc2_ = 0;
                        while(_loc2_ < count)
                        {
                           this[§§constant(56)] = eval(§§constant(57))[§§constant(67)](eval(§§constant(57))[§§constant(65)]() * (eval("{invalid_utf8=252}{invalid_utf8=222}")[§§constant(3)][§§constant(4)][§§constant(25)][§§constant(26)][§§constant(66)] << 5));
                           this[§§constant(59)] = -200;
                           this[§§constant(68)] = 0;
                           this[§§constant(36)] = 40;
                           this[§§constant(52)] = false;
                           while(!this[§§constant(52)] && this[§§constant(59)] < eval("{invalid_utf8=252}{invalid_utf8=222}")[§§constant(3)][§§constant(4)][§§constant(25)][§§constant(69)][§§constant(59)])
                           {
                              this[§§constant(37)]();
                           }
                           if(this[§§constant(52)])
                           {
                              _loc5_[§§constant(72)]({(§§constant(56)):this[§§constant(56)],(§§constant(70)):this[§§constant(56)],(§§constant(71)):this[§§constant(59)]});
                           }
                           _loc2_ = _loc2_ + 1;
                        }
                        this[§§constant(49)] = false;
                        return _loc5_;
                     };
                     _loc2_[§§constant(73)] = function(details)
                     {
                        this[§§constant(31)](details[§§constant(56)],0);
                        this[§§constant(20)] = 20;
                        this[§§constant(32)]();
                     };
                     _loc2_[§§constant(35)] = false;
                     _loc2_[§§constant(39)] = 30;
                     _loc2_[§§constant(40)] = 10;
                     _loc2_[§§constant(20)] = 0;
                     §§constant(74)(eval("{invalid_utf8=252}{invalid_utf8=222}")[§§constant(3)][§§constant(4)][§§constant(5)][§§constant(18)],null,1);
                     break;
                  }
                  if(eval("\x01") == 632)
                  {
                     set("\x01",eval("\x01") - 204);
                  }
                  else if(eval("\x01") == 428)
                  {
                     set("\x01",eval("\x01") + 148);
                     §§push("\x0f");
                     §§push(1);
                  }
                  else
                  {
                     if(eval("\x01") != 14)
                     {
                        if(eval("\x01") == 629)
                        {
                           set("\x01",eval("\x01") - 629);
                        }
                        break;
                     }
                     set("\x01",eval("\x01") + 414);
                  }
               }
            }
         }
      }
   }
}
