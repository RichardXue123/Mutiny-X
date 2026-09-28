using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mutiny.Simulation
{
    // The synchronous diagnostic API and production sliced search use the same
    // one-tick predictor. No random draws or live object reads happen here.
    internal sealed class MutinyAIPrediction
    {
        internal enum Kind { Weapon, Character, Anchor, Seagull }
        public PhysicsBodyState Body;
        public int Steps { get; private set; }
        public bool Complete { get; private set; }
        public bool HitFloor { get; private set; }
        public Vector2 Impact => new Vector2(Body.X, Body.Y);
        private readonly Kind m_Kind;
        private readonly string m_Weapon;
        private readonly string[,] m_Terrain;
        private readonly int m_Width, m_Height, m_Limit;
        private readonly float m_Water;
        private readonly IReadOnlyList<PhysicsBoxObstacle> m_Boxes;
        private readonly IReadOnlyList<Vector2> m_BananaPositions;

        public MutinyAIPrediction(Kind kind, PhysicsBodyState body, string weapon,
            string[,] terrain, int width, int height, float water,
            IReadOnlyList<PhysicsBoxObstacle> boxes, IReadOnlyList<Vector2> bananaPositions = null)
        {
            m_Kind = kind;
            Body = body;
            m_Weapon = weapon;
            m_Terrain = terrain;
            m_Width = width;
            m_Height = height;
            m_Water = water;
            m_Boxes = boxes;
            m_BananaPositions = bananaPositions;
            m_Limit = kind == Kind.Character || Is("cannonball") ? 4096 :
                kind == Kind.Anchor ? 1024 : kind == Kind.Seagull ? 512 : 101;
        }

        // Returns true only if a physics tick was performed. A final false call
        // has no side effects, which also handles an initially resting character.
        public bool Advance()
        {
            if (Complete) return false;
            if (Steps >= m_Limit ||
                (m_Kind != Kind.Weapon && Body.Y >= m_Water) ||
                (m_Kind == Kind.Character && !(Body.VelocityX != 0f || Mathf.Abs(Body.VelocityY) > 0.2f)))
            {
                Complete = true;
                return false;
            }
            if (m_Kind == Kind.Anchor)
            {
                Body.VelocityX = 0f;
                Body.VelocityY = MutinyAnchor.DropSpeedPixelsPerTick;
            }
            if (Is("parachuteBomb")) MutinyParachuteBomb.ApplyOriginalAirMotion(ref Body);
            StepResult contact = MutinyPhysics.Step(ref Body, m_Terrain, m_Width, m_Height, m_Boxes);
            Steps++;
            bool touched = contact.HitFloor || contact.HitCeiling || contact.HitLeftWall || contact.HitRightWall;
            HitFloor |= contact.HitFloor;
            if (m_Kind == Kind.Anchor) Complete = contact.HitFloor;
            else if (m_Kind == Kind.Seagull) Complete = touched;
            else if (m_Kind == Kind.Weapon)
            {
                Complete = touched && (Is("cherryBomb") || Is("rumBottle") ||
                    Is("piecesOfEight") || Is("parachuteBomb") || Is("cannonball"));
                if ((Is("dynamite") || Is("mine")) &&
                    Body.VelocityX == 0f && Mathf.Abs(Body.VelocityY) < 0.2f) Complete = true;
                if (Is("banana"))
                {
                    float nearest = float.PositiveInfinity;
                    if (m_BananaPositions != null)
                        for (int i = 0; i < m_BananaPositions.Count; i++)
                            nearest = Mathf.Min(nearest, (Impact - m_BananaPositions[i]).sqrMagnitude);
                    Complete |= Body.VelocityX == 0f && Mathf.Abs(Body.VelocityY) < 0.5f ||
                        MutinyBanana.ShouldDetonateForAi(nearest, float.PositiveInfinity);
                }
                // Contact termination precedes the ceiling clamp in the source.
                if (!Complete && Is("parachuteBomb")) MutinyParachuteBomb.ApplyOriginalCeilingClamp(ref Body);
                if (Is("cannonball") && (Body.X < -300f || Body.X > m_Width * MutinyPhysics.PixelsPerUnit + 300f ||
                    Body.Y < -300f || (!float.IsInfinity(m_Water) && Body.Y > m_Water))) Complete = true;
            }
            return true;
        }

        private bool Is(string weapon) => string.Equals(m_Weapon, weapon, StringComparison.OrdinalIgnoreCase);
    }
}
