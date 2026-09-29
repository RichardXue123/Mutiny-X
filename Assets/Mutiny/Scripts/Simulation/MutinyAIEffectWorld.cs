using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mutiny.Simulation
{
    // One private, value-only battle per candidate. Advance is ONE 25 Hz tick;
    // the controller's existing budget pump decides how many ticks fit a frame.
    internal sealed class MutinyAIEffectWorld
    {
        internal const int DefaultMaxTicks = 2048;
        private readonly MutinyAIEffectInput m_Input;
        private readonly MutinyAIEffectCommand m_Command;
        private readonly MutinyAIEffectCharacter[] m_Characters;
        private readonly List<MutinyAIEffectBox> m_Boxes;
        private readonly List<MutinyAIEffectMine> m_Mines;
        private readonly List<PhysicsBoxObstacle> m_Obstacles = new List<PhysicsBoxObstacle>();
        private readonly List<Projectile> m_Projectiles = new List<Projectile>();
        private readonly List<Explosion> m_Explosions = new List<Explosion>();
        private readonly List<Flame> m_Flames = new List<Flame>();
        private readonly MutinyAIEffectRandom m_Random;
        private readonly int m_MaxTicks, m_Seed;
        private readonly Vector2[] m_CoinVelocities = new Vector2[MutinyPiecesOfEight.TotalCoins];
        private readonly List<Vector2> m_BoxPositions = new List<Vector2>();
        private bool m_ActionDone, m_Finished;
        private int m_Tick, m_QuietTicks, m_CoinIndex, m_CoinWait, m_AnchorWait, m_AnchorHold;
        private int m_BoxWait, m_BoxAfter, m_BoxIndex, m_PossibilityIndex;
        private float m_BoxOffset;
        private Vector2 m_BoxNext = new Vector2(float.NaN, float.NaN);
        private PhysicsBodyState m_Anchor, m_Cannon;
        private float m_WaveX = -550f, m_BirdX = -300f;
        private int m_ShotIndex, m_CannonWait;
        private bool m_Wave, m_Bird, m_AnchorFalling, m_Voodoo, m_BoxSequence;
        internal MutinyAIEffectEvaluation Outcome { get; } = new MutinyAIEffectEvaluation();
        internal MutinyAIActionPlan Plan => new MutinyAIActionPlan(m_Command.FanDirection,
            m_Command.Weapon == "piecesofeight" ? m_CoinVelocities : null,
            m_BoxPositions.Count > 0 ? m_BoxPositions.ToArray() : null, m_Seed);
        internal MutinyAIEffectCharacter CharacterAt(int index) => m_Characters[index];

        private struct Projectile { public PhysicsBodyState Body; public string Kind; public float Visibility; }
        private struct Explosion { public Vector2 Position; public float Size, Damage; public int Due; }
        private struct Flame { public Vector2 Position; public bool Right; public int Due; }

        internal MutinyAIEffectWorld(MutinyAIEffectInput input, MutinyAIEffectCommand command, int seed,
            int maxTicks = DefaultMaxTicks)
        {
            m_Input = input; m_Command = command; m_MaxTicks = Mathf.Max(1, maxTicks); m_Seed = seed;
            m_Characters = (MutinyAIEffectCharacter[])input.Characters.Clone();
            m_Boxes = new List<MutinyAIEffectBox>(input.Boxes);
            m_Mines = new List<MutinyAIEffectMine>(input.Mines);
            m_Random = new MutinyAIEffectRandom(seed);
            Outcome.Velocity = command.Velocity; Outcome.FanDirection = command.FanDirection;
            Outcome.SimulationSeed = seed; Outcome.TargetActor = command.TargetActor; Outcome.Target = command.Target;
            Outcome.FlightY = command.FlightY; Outcome.ShotXs = command.ShotXs != null ? (float[])command.ShotXs.Clone() : null;
            foreach (var character in m_Characters)
                if (character.Alive)
                {
                    if (character.Team == input.OwnTeam) Outcome.AllyHpBefore += character.Health;
                    else Outcome.EnemyHpBefore += character.Health;
                }
            StartAction();
        }

        private void StartAction()
        {
            if (m_Command.Type == AIMoveType.Pass) { m_ActionDone = true; return; }
            if (m_Command.Actor < 0 || m_Command.Actor >= m_Characters.Length || !m_Characters[m_Command.Actor].Alive)
            { Finish(false, "invalid-actor"); return; }
            var actor = m_Characters[m_Command.Actor];
            if (m_Command.Type == AIMoveType.SelfThrow)
            {
                actor.Body.VelocityX = m_Command.Velocity.x; actor.Body.VelocityY = m_Command.Velocity.y;
                m_Characters[m_Command.Actor] = actor; m_ActionDone = true; return;
            }
            string weapon = m_Command.Weapon;
            switch (weapon)
            {
                case "tidalwave": m_Wave = true; break;
                case "seagull": m_Bird = true; break;
                case "anchor":
                    m_Anchor = ProjectileBody(weapon, actor.Body); m_Anchor.X = m_Command.Target.x;
                    m_Anchor.Y = MutinyAnchor.DropStartYPixels; m_AnchorWait = 20; m_AnchorFalling = true; break;
                case "voodoodoll": m_Voodoo = true; break;
                case "woodencrate": case "gunpowderbarrel":
                    m_BoxSequence = true; SelectNextBox(); break;
                case "cannon":
                    m_Cannon = ProjectileBody("cannonball", actor.Body);
                    m_Cannon.Y = actor.Body.Y + MutinyCannon.InitialEquipmentOffsetY;
                    m_Cannon.VelocityX = m_Command.Target.x - m_Cannon.X;
                    m_Cannon.VelocityY = m_Command.Target.y - m_Cannon.Y;
                    BuildObstacles(); MutinyPhysics.Step(ref m_Cannon, m_Input.Terrain, m_Input.Width, m_Input.Height, m_Obstacles);
                    m_CannonWait = 25; break;
                case "mine":
                    m_Mines.Add(new MutinyAIEffectMine { Body = LaunchedBody(weapon, actor.Body, m_Command.Velocity),
                        Ignore = MutinyMine.IgnoreTicks, Countdown = MutinyMine.CountdownTicks });
                    m_ActionDone = true; break;
                case "piecesofeight": FireCoin(m_Command.Velocity); break;
                case "cherrybomb": case "dynamite": case "banana": case "boulder": case "rumbottle": case "parachutebomb": case "cannonball":
                    m_Projectiles.Add(new Projectile { Body = LaunchedBody(weapon, actor.Body, m_Command.Velocity), Kind = weapon, Visibility = 2f }); break;
                default: Finish(false, "unsupported-weapon"); break;
            }
        }

        internal bool Advance()
        {
            if (m_Finished) return false;
            m_Tick++;
            BuildObstacles();
            for (int i = 0; i < m_Characters.Length; i++)
            {
                var character = m_Characters[i];
                // Corpses still advance in production. They earn no HP score,
                // but Banana proximity includes every character, including dead.
                MutinyPhysics.Step(ref character.Body, m_Input.Terrain, m_Input.Width, m_Input.Height, m_Obstacles);
                if (character.Body.Y > m_Input.WaterY)
                {
                    character.Health = 0f; character.Alive = false;
                    character.Body.VelocityX *= 0.8f; character.Body.VelocityY *= 0.8f;
                    if (character.Body.VelocityY > 1.5f) character.Body.VelocityY = Mathf.Max(1.5f, character.Body.VelocityY - 4f);
                }
                m_Characters[i] = character;
            }
            AdvanceBoxes();
            BuildObstacles();
            AdvanceDedicatedActions();
            if (m_Finished) return false;
            for (int i = m_Projectiles.Count - 1; i >= 0; i--) AdvanceProjectile(i);
            AdvanceMines();
            AdvanceExplosions();
            AdvanceFlames();

            bool quiet = !Busy();
            m_QuietTicks = quiet ? m_QuietTicks + 1 : 0;
            if (m_QuietTicks >= 20) Finish(true, "settled");
            else if (m_Tick >= m_MaxTicks) Finish(false, "horizon-exceeded");
            return !m_Finished;
        }

        private void BuildObstacles(int exceptBox = -1)
        {
            m_Obstacles.Clear();
            for (int i = 0; i < m_Boxes.Count; i++)
                if (i != exceptBox && !m_Boxes[i].Removed) m_Obstacles.Add(PhysicsBoxObstacle.FromSnapshot(m_Boxes[i].Body));
        }

        private void AdvanceBoxes()
        {
            for (int i = 0; i < m_Boxes.Count; i++)
            {
                var box = m_Boxes[i]; if (box.Removed) continue;
                BuildObstacles(i);
                MutinyPhysics.Step(ref box.Body, m_Input.Terrain, m_Input.Width, m_Input.Height, m_Obstacles);
                m_Boxes[i] = box;
            }
        }

        private void AdvanceDedicatedActions()
        {
            if (m_Wave)
            {
                m_WaveX += MutinyTidalWave.OriginalSpeed;
                for (int i = 0; i < m_Characters.Length; i++)
                {
                    var c = m_Characters[i];
                    if (c.Alive && c.Body.Y >= m_Input.WaterY - MutinyTidalWave.OriginalHitHeightAboveWater &&
                        c.Body.X >= m_WaveX - MutinyTidalWave.OriginalHitHalfWidth &&
                        c.Body.X <= m_WaveX + MutinyTidalWave.OriginalHitHalfWidth) Damage(i, MutinyTidalWave.OriginalDamagePerTick);
                }
                if (m_WaveX > m_Input.Width * 32f + MutinyTidalWave.OriginalExitPadding) { m_Wave = false; m_ActionDone = true; }
            }
            if (m_Bird)
            {
                m_BirdX += MutinySeagull.OriginalFlightSpeed;
                if (m_Command.ShotXs != null && m_ShotIndex < m_Command.ShotXs.Length &&
                    m_BirdX >= m_Command.ShotXs[m_ShotIndex])
                {
                    m_ShotIndex++;
                    if (m_BirdX < m_Input.Width * 32f)
                    {
                        var body = ProjectileBody("seagullfire", m_Characters[m_Command.Actor].Body);
                        body.X = m_BirdX + MutinySeagull.OriginalShotXOffset; body.Y = m_Command.FlightY;
                        body.VelocityX = MutinySeagull.OriginalFlightSpeed;
                        m_Projectiles.Add(new Projectile { Body = body, Kind = "seagullfire" });
                    }
                }
                if (m_BirdX > m_Input.Width * 32f + MutinySeagull.OriginalExitPadding && m_Projectiles.Count == 0)
                { m_Bird = false; m_ActionDone = true; }
            }
            if (m_CannonWait > 0 && --m_CannonWait == 0)
            {
                m_Cannon.VelocityX = m_Command.Velocity.x; m_Cannon.VelocityY = m_Command.Velocity.y;
                m_Projectiles.Add(new Projectile { Body = m_Cannon, Kind = "cannonball" });
            }
            if (m_Voodoo)
            {
                if (m_Tick == 21 && m_Command.TargetActor >= 0 && m_Command.TargetActor < m_Characters.Length)
                {
                    var c = m_Characters[m_Command.TargetActor];
                    c.Body.VelocityX = m_Command.Velocity.x; c.Body.VelocityY = m_Command.Velocity.y;
                    m_Characters[m_Command.TargetActor] = c;
                }
                if (m_Tick >= 32) { m_Voodoo = false; m_ActionDone = true; }
            }
            if (m_AnchorFalling)
            {
                if (m_AnchorWait > 0) m_AnchorWait--;
                else
                {
                    m_Anchor.VelocityX = 0f; m_Anchor.VelocityY = MutinyAnchor.DropSpeedPixelsPerTick;
                    StepResult step = MutinyPhysics.Step(ref m_Anchor, m_Input.Terrain, m_Input.Width, m_Input.Height, m_Obstacles);
                    if (step.HitFloor)
                    {
                        for (int i = 0; i < m_Characters.Length; i++)
                        {
                            var c = m_Characters[i];
                            if (Mathf.Abs(c.Body.X - m_Anchor.X) < 48f && c.Body.Y < m_Anchor.Y && c.Body.Y > m_Anchor.Y - 64f)
                                Damage(i, MutinyAnchor.CrushDamage);
                        }
                        m_AnchorFalling = false; m_AnchorHold = 40;
                    }
                }
            }
            else if (m_AnchorHold > 0 && --m_AnchorHold == 0) m_ActionDone = true;
            if (m_BoxSequence) AdvancePlacement();
            if (m_Command.Weapon == "piecesofeight" && m_Projectiles.Count == 0 && m_CoinIndex < MutinyPiecesOfEight.TotalCoins)
            {
                if (!m_Characters[m_Command.Actor].Alive) m_ActionDone = true;
                else if (m_CoinWait > 0 && --m_CoinWait == 0)
                    FireCoin(AimedCoinVelocity());
            }
        }

        private void AdvanceProjectile(int index)
        {
            var projectile = m_Projectiles[index]; var body = projectile.Body; string kind = projectile.Kind;
            float startX = body.X;
            if (kind == "parachutebomb")
            {
                MutinyParachuteBomb.ApplyOriginalAirMotion(ref body);
                body.VelocityX += m_Command.FanDirection * MutinyParachuteBomb.FanImpulsePerTick;
            }
            StepResult step = MutinyPhysics.Step(ref body, m_Input.Terrain, m_Input.Width, m_Input.Height, m_Obstacles);
            bool contact = step.HitFloor || step.HitCeiling || step.HitLeftWall || step.HitRightWall;
            bool end = false, explode = false; float size = 0, damage = 0;
            switch (kind)
            {
                case "cherrybomb": end = explode = contact; size = 80f; damage = 40f; break;
                case "dynamite": end = explode = body.VelocityX == 0f && Mathf.Abs(body.VelocityY) < 0.2f; size = 250f; damage = 70f; break;
                case "banana":
                    end = explode = body.VelocityX == 0f && Mathf.Abs(body.VelocityY) < 0.5f;
                    foreach (var c in m_Characters)
                        if ((new Vector2(c.Body.X, c.Body.Y) - new Vector2(body.X, body.Y)).sqrMagnitude < MutinyBanana.AiImmediateDetonationDistanceSquared)
                            end = explode = true;
                    size = 160f; damage = 80f; break;
                case "rumbottle":
                    end = explode = contact; size = 80f; damage = 25f;
                    if (contact && (step.HitFloor || step.HitCeiling)) body.X = startX;
                    if (step.HitFloor) StartFlames(new Vector2(body.X, body.Y));
                    break;
                case "parachutebomb":
                    end = explode = contact; size = MutinyParachuteBomb.ExplosionSize; damage = MutinyParachuteBomb.ExplosionDamage;
                    MutinyParachuteBomb.ApplyOriginalCeilingClamp(ref body); break;
                case "piecesofeight": end = contact || body.Y >= m_Input.WaterY; explode = contact; size = 50f; damage = 25f; break;
                case "seagullfire": end = step.HitFloor || step.HitLeftWall || step.HitRightWall || body.Y > m_Input.WaterY;
                    explode = step.HitFloor || step.HitLeftWall || step.HitRightWall; size = 50f; damage = 50f; break;
                case "cannonball":
                    end = explode = contact; size = 100f; damage = 50f;
                    if (!contact)
                    {
                        if (body.X < -300f || body.Y < -300f || body.X > m_Input.Width * 32f + 300f || body.Y > m_Input.WaterY) end = true;
                        else for (int i = 0; i < m_Characters.Length; i++)
                            if (i != m_Command.Actor && m_Characters[i].Alive && Overlaps(body, m_Characters[i].Body)) end = explode = true;
                    }
                    break;
                case "boulder":
                    if (body.VelocityX == 0f && Mathf.Abs(body.VelocityY) < MutinyBoulder.SettleVelocityYThreshold)
                    {
                        projectile.Visibility -= MutinyBoulder.VisibilityDecrementPerTick;
                        end = projectile.Visibility < 0f;
                    }
                    if (!end) ApplyBoulderContacts(body);
                    break;
            }
            if (explode) QueueExplosion(new Vector2(body.X, body.Y), size, damage);
            // Source water crossings alone do not detonate ordinary weapons.
            if (kind != "cannonball" && kind != "piecesofeight" && kind != "seagullfire" &&
                ((body.VelocityY > 0f && body.Y > m_Input.Height * 32f) ||
                 (body.VelocityX == 0f && Mathf.Abs(body.VelocityY) < 0.2f))) end = true;
            if (end)
            {
                m_Projectiles.RemoveAt(index);
                if (kind == "piecesofeight")
                {
                    m_CoinIndex++; Outcome.CoinsFired = m_CoinIndex;
                    m_CoinWait = MutinyPiecesOfEight.AiReaimDelayTicks;
                    if (m_CoinIndex >= MutinyPiecesOfEight.TotalCoins || !m_Characters[m_Command.Actor].Alive) m_ActionDone = true;
                }
                else if (kind != "seagullfire") m_ActionDone = true;
            }
            else { projectile.Body = body; m_Projectiles[index] = projectile; }
        }

        private void AdvanceMines()
        {
            for (int i = 0; i < m_Mines.Count; i++)
            {
                var mine = m_Mines[i]; if (mine.Removed) continue;
                MutinyPhysics.Step(ref mine.Body, m_Input.Terrain, m_Input.Width, m_Input.Height, m_Obstacles);
                if (!mine.Active && --mine.Ignore <= 0)
                    foreach (var c in m_Characters)
                    {
                        if (!c.Alive || (c.Body.VelocityX == 0f && Mathf.Abs(c.Body.VelocityY) <= 0.2f)) continue;
                        float dx = c.Body.X - mine.Body.X, dy = c.Body.Y + c.Body.BottomExtent - mine.Body.Y;
                        if (dx * dx + dy * dy < MutinyMine.TriggerRadiusPixels * MutinyMine.TriggerRadiusPixels) { mine.Active = true; break; }
                    }
                if (mine.Body.VelocityX == 0f && Mathf.Abs(mine.Body.VelocityY) < 0.2f) mine.Stored = true;
                if (mine.Active && --mine.Countdown <= 0)
                { mine.Removed = true; QueueExplosion(new Vector2(mine.Body.X, mine.Body.Y), 250f, 70f); }
                m_Mines[i] = mine;
            }
        }

        private void QueueExplosion(Vector2 position, float size, float damage) =>
            m_Explosions.Add(new Explosion { Position = position, Size = size, Damage = damage, Due = m_Tick + 2 });

        private void AdvanceExplosions()
        {
            for (int i = m_Explosions.Count - 1; i >= 0; i--)
            {
                var explosion = m_Explosions[i]; if (explosion.Due > m_Tick) continue;
                m_Explosions.RemoveAt(i); Outcome.Explosions++;
                float radius = explosion.Size * 0.5f + 20f;
                for (int c = 0; c < m_Characters.Length; c++)
                {
                    var character = m_Characters[c]; if (!character.Alive) continue;
                    if (!MutinyCombatMath.ExplosionHit(new Vector2(character.Body.X, character.Body.Y), explosion.Position,
                        radius, explosion.Damage, out float ratio, out Vector2 impulse)) continue;
                    character.Body.VelocityX += impulse.x; character.Body.VelocityY += impulse.y;
                    m_Characters[c] = character; Damage(c, explosion.Damage * ratio);
                }
                // All character hits first, then synchronous obstacle removal.
                for (int b = 0; b < m_Boxes.Count; b++)
                {
                    var box = m_Boxes[b]; if (box.Removed) continue;
                    float dx = explosion.Position.x - Mathf.Clamp(explosion.Position.x, box.Body.X - box.Body.LeftExtent, box.Body.X + box.Body.RightExtent);
                    float dy = explosion.Position.y - Mathf.Clamp(explosion.Position.y, box.Body.Y - box.Body.TopExtent, box.Body.Y + box.Body.BottomExtent);
                    if (dx * dx + dy * dy > radius * radius) continue;
                    box.Removed = true; m_Boxes[b] = box;
                    if (box.Barrel) QueueExplosion(new Vector2(box.Body.X, box.Body.Y), 150f, 30f);
                }
            }
        }

        private void StartFlames(Vector2 impact)
        {
            if (m_Input.Terrain == null) return;
            int col = Mathf.FloorToInt(impact.x / 32f), row = Mathf.FloorToInt(impact.y / 32f) + 1;
            while (row > 0 && Solid(col, row - 1)) row--;
            var origin = new Vector2(col * 32f, row * 32f);
            SpawnFlame(origin, true); SpawnFlame(origin, false);
        }

        private bool Solid(int col, int row) => MutinySweepingFlame.IsSolidTile(m_Input.Terrain, m_Input.Width, m_Input.Height, col, row);
        private void SpawnFlame(Vector2 position, bool right)
        {
            Outcome.FlameSegments++;
            for (int i = 0; i < m_Characters.Length; i++)
            {
                var c = m_Characters[i]; if (!c.Alive) continue;
                float dx = c.Body.X - position.x, dy = c.Body.Y + c.Body.BottomExtent - position.y;
                if (dx * dx + dy * dy >= MutinySweepingFlame.CharacterHitDistanceSquared) continue;
                c.Body.VelocityX = m_Random.Value() * 8f - 4f; c.Body.VelocityY = -(m_Random.Value() * 2f + 6f);
                m_Characters[i] = c; Damage(i, MutinySweepingFlame.Damage);
            }
            m_Flames.Add(new Flame { Position = position, Right = right, Due = m_Tick + MutinySweepingFlame.PropagationFrame - 1 });
        }

        private void AdvanceFlames()
        {
            // Capture count: new nodes must wait three ticks, not run recursively
            // in their birth tick. Rendering's eleven-frame fade has no more hits.
            for (int i = m_Flames.Count - 1; i >= 0; i--)
            {
                var flame = m_Flames[i]; if (flame.Due > m_Tick) continue;
                m_Flames.RemoveAt(i);
                var next = flame.Position + new Vector2(flame.Right ? 8f : -8f, 0f);
                int col = Mathf.FloorToInt(next.x / 32f), row = Mathf.FloorToInt(next.y / 32f);
                if (Solid(col, row) && !Solid(col, row - 1)) SpawnFlame(next, flame.Right);
            }
        }

        private void ApplyBoulderContacts(PhysicsBodyState body)
        {
            for (int i = 0; i < m_Characters.Length; i++)
            {
                var c = m_Characters[i];
                if (!c.Alive || i == m_Command.Actor || c.Body.Y - c.Body.TopExtent > body.Y + 32f ||
                    c.Body.Y + c.Body.BottomExtent < body.Y - 32f || Mathf.Abs(c.Body.X - body.X) > 32f) continue;
                bool right = c.Body.X > body.X; c.Body.X = body.X + (right ? 32f : -32f);
                if ((right && body.VelocityX > 0f) || (!right && body.VelocityX < 0f)) c.Body.VelocityX += body.VelocityX;
                m_Characters[i] = c; Damage(i, Mathf.Abs(body.VelocityX) * MutinyBoulder.DamagePerVelocityX);
            }
        }

        private void FireCoin(Vector2 velocity)
        {
            var owner = m_Characters[m_Command.Actor].Body;
            var body = LaunchedBody("piecesofeight", owner, velocity);
            if (m_CoinIndex > 0) body.Y = owner.Y + 5f;
            m_CoinVelocities[m_CoinIndex] = velocity;
            m_Projectiles.Add(new Projectile { Body = body, Kind = "piecesofeight" });
        }

        private Vector2 AimedCoinVelocity()
        {
            var owner = m_Characters[m_Command.Actor].Body;
            int nearest = -1; float distance = float.PositiveInfinity;
            for (int i = 0; i < m_Characters.Length; i++)
            {
                var c = m_Characters[i]; if (!c.Alive || c.Team == m_Input.OwnTeam) continue;
                float d = Mathf.Abs(c.Body.X - owner.X);
                if (d < distance) { distance = d; nearest = i; }
            }
            if (nearest < 0) return m_Command.Velocity;
            var target = m_Characters[nearest].Body;
            return AimedVelocity(new Vector2(owner.X, owner.Y + 5f), new Vector2(target.X, target.Y), 20f);
        }

        internal static Vector2 AimedVelocity(Vector2 start, Vector2 target, float maxForce, float timeScale = 1f, float weight = 1f)
        {
            if (weight == 0f) return (target - start).normalized * maxForce;
            float ticks = Mathf.Clamp(Mathf.Abs(target.x - start.x) / 9f, 8f, 30f) * timeScale;
            var velocity = new Vector2((target.x - start.x) / ticks,
                (target.y - start.y - weight * ticks * (ticks + 1f) * 0.5f) / ticks);
            return Vector2.ClampMagnitude(velocity, maxForce);
        }

        private void SelectNextBox()
        {
            if (m_BoxIndex >= (m_Command.Weapon == "woodencrate" ? 3 : 2))
            { m_BoxSequence = false; m_ActionDone = true; return; }
            var possibilities = m_Command.BoxPossibilities;
            if (possibilities == null || possibilities.Length < 3) { Finish(false, "invalid-box-plan"); return; }
            BuildObstacles();
            var living = new List<PhysicsBodyState>(); foreach (var c in m_Characters) if (c.Alive) living.Add(c.Body);
            bool CanPlace(Vector2 p) => MutinyBoxPlacementRules.CanPlace(p, m_Input.Terrain, m_Input.Width, m_Input.Height,
                m_Obstacles, m_Input.Chests, living);
            if (!float.IsNaN(m_BoxNext.x) && CanPlace(m_BoxNext + new Vector2(0f, -48f)))
            { m_BoxNext.y -= 48f; m_BoxOffset -= 48f; m_BoxWait = 40; return; }
            int count = Mathf.Min(3, possibilities.Length);
            for (int attempts = 0; attempts < count * 12; attempts++)
            {
                m_PossibilityIndex++;
                if (m_PossibilityIndex >= count) { m_PossibilityIndex = 0; m_BoxOffset -= 32f; }
                var candidate = possibilities[m_PossibilityIndex] + new Vector2(0f, m_BoxOffset);
                if (CanPlace(candidate)) { m_BoxNext = candidate; m_BoxWait = 40; return; }
            }
            Finish(false, "incomplete-box-plan");
        }

        private void AdvancePlacement()
        {
            if (m_BoxWait > 0)
            {
                if (--m_BoxWait != 0) return;
                var body = ProjectileBody("woodencrate", m_Characters[m_Command.Actor].Body);
                body.X = m_BoxNext.x; body.Y = m_BoxNext.y;
                m_Boxes.Add(new MutinyAIEffectBox { Body = body, Barrel = m_Command.Weapon == "gunpowderbarrel" });
                m_BoxPositions.Add(m_BoxNext); Outcome.BoxesPlaced = ++m_BoxIndex;
                m_BoxAfter = 10;
                if (m_BoxIndex >= (m_Command.Weapon == "woodencrate" ? 3 : 2)) { m_BoxSequence = false; m_ActionDone = true; }
            }
            else if (m_BoxAfter > 0 && --m_BoxAfter == 0) SelectNextBox();
        }

        private bool Busy()
        {
            if (!m_ActionDone || m_Projectiles.Count > 0 || m_Explosions.Count > 0 || m_Flames.Count > 0) return true;
            foreach (var c in m_Characters) if (c.Alive && !c.Body.IsAtRest) return true;
            foreach (var box in m_Boxes) if (!box.Removed && !box.Body.IsAtRest) return true;
            foreach (var mine in m_Mines) if (!mine.Removed && (!mine.Stored || mine.Active)) return true;
            return false;
        }

        private void Damage(int index, float damage)
        {
            var c = m_Characters[index]; if (!c.Alive) return;
            c.Health = Mathf.Max(0f, c.Health - Mathf.Round(damage));
            if (c.Health <= 0f) c.Alive = false;
            m_Characters[index] = c;
        }

        private void Finish(bool settled, string status)
        {
            m_Finished = true; Outcome.Settled = settled; Outcome.Status = status; Outcome.Ticks = m_Tick;
            for (int i = 0; i < m_Characters.Length; i++)
            {
                var c = m_Characters[i]; var initial = m_Input.Characters[i];
                if (c.Team == m_Input.OwnTeam)
                { if (c.Alive) Outcome.AllyHpAfter += c.Health; if (initial.Alive && !c.Alive) Outcome.AlliesLost++; }
                else { if (c.Alive) Outcome.EnemyHpAfter += c.Health; if (initial.Alive && !c.Alive) Outcome.EnemiesLost++; }
            }
            if (m_Command.Actor >= 0 && m_Command.Actor < m_Characters.Length)
                Outcome.FinalActorPosition = new Vector2(m_Characters[m_Command.Actor].Body.X, m_Characters[m_Command.Actor].Body.Y);
            if (m_Command.Weapon == "piecesofeight") Outcome.CoinVelocities = (Vector2[])m_CoinVelocities.Clone();
            if (m_BoxPositions.Count > 0) Outcome.BoxPositions = m_BoxPositions.ToArray();
        }

        private static bool Overlaps(PhysicsBodyState a, PhysicsBodyState b) =>
            b.X - b.LeftExtent <= a.X + a.RightExtent && b.X + b.RightExtent >= a.X - a.LeftExtent &&
            b.Y - b.TopExtent <= a.Y + a.BottomExtent && b.Y + b.BottomExtent >= a.Y - a.TopExtent;

        private static PhysicsBodyState LaunchedBody(string kind, PhysicsBodyState owner, Vector2 velocity)
        {
            var body = ProjectileBody(kind, owner); body.VelocityX = velocity.x; body.VelocityY = velocity.y; return body;
        }

        internal static PhysicsBodyState ProjectileBody(string kind, PhysicsBodyState owner)
        {
            var body = PhysicsBodyState.CreateDefault(owner.X, owner.Y + (kind == "boulder" ? -30f : -10f));
            body.HitsBoxes = true; float extent = 10f;
            switch (kind)
            {
                case "cherrybomb": extent = 9f; break;
                case "dynamite": extent = 11f; body.Friction = 1.7f; break;
                case "banana": extent = 7f; body.Bounce = 0.8f; body.Friction = 0.5f; break;
                case "boulder": extent = 31f; body.Weight = 1.5f; body.Friction = 0.25f; break;
                case "parachutebomb": extent = 11f; break;
                case "piecesofeight": extent = 7f; break;
                case "rumbottle": extent = 14f; break;
                case "mine": extent = 14f; body.Friction = 1.5f; break;
                case "cannonball": body.Weight = 0f; break;
                case "anchor": extent = 48f; body.Weight = 0f; break;
                case "woodencrate": extent = 16f; break;
            }
            body.LeftExtent = body.RightExtent = body.TopExtent = body.BottomExtent = extent;
            if (kind == "anchor") { body.TopExtent = 96f; body.BottomExtent = 0f; }
            if (kind == "woodencrate") { body.RightExtent = body.BottomExtent = 15f; body.Y = owner.Y; }
            return body;
        }
    }
}
