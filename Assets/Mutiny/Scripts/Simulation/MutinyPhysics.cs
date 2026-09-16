using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mutiny.Simulation
{
    [Serializable]
    public struct PhysicsBodyState
    {
        public float X; // Flash pixel coordinates
        public float Y;
        public float VelocityX;
        public float VelocityY;

        public float Weight; // default 1.0f
        public float Bounce; // default 0.2f
        public float Friction; // default 0.3f (2.0f for Character)

        public float LeftExtent; // default 6
        public float RightExtent; // default 6
        public float TopExtent; // default 8
        public float BottomExtent; // default 8

        public bool HitsTiles;

        public bool IsAtRest => Mathf.Abs(VelocityX) < 0.001f && Mathf.Abs(VelocityY) < 0.2f;

        public static PhysicsBodyState CreateDefault(float x, float y)
        {
            return new PhysicsBodyState
            {
                X = x,
                Y = y,
                VelocityX = 0f,
                VelocityY = 0f,
                Weight = 1.0f,
                Bounce = 0.2f,
                Friction = 0.3f,
                LeftExtent = 6f,
                RightExtent = 6f,
                TopExtent = 8f,
                BottomExtent = 8f,
                HitsTiles = true
            };
        }
    }

    public struct StepResult
    {
        public bool HitFloor;
        public bool HitCeiling;
        public bool HitLeftWall;
        public bool HitRightWall;
        public bool IsAtRest;
    }

    public static class MutinyPhysics
    {
        public const float PixelsPerUnit = 32f;
        public const float TimeStep = 0.04f; // 25 Hz = 1/25 s
        public const float DefaultTwangMaxForce = 20.0f;
        public const float Gravity = 1.0f; // 1 px/tick default Flash gravity
        public const float DefaultGravity = 1.0f;

        public static Vector3 PixelToUnity(float px, float py)
        {
            return new Vector3(px / PixelsPerUnit, -py / PixelsPerUnit, 0f);
        }

        public static Vector2 UnityToPixel(Vector3 unityPos)
        {
            return new Vector2(unityPos.x * PixelsPerUnit, -unityPos.y * PixelsPerUnit);
        }

        public static Vector2 CalculateTwangVelocity(Vector2 startPosPx, Vector2 dragPosPx, float maxForce = DefaultTwangMaxForce)
        {
            float vx = (dragPosPx.x - startPosPx.x) * -0.25f;
            float vy = (dragPosPx.y - startPosPx.y) * -0.25f;

            float sqrLen = vx * vx + vy * vy;
            if (sqrLen > maxForce * maxForce)
            {
                float len = Mathf.Sqrt(sqrLen);
                vx *= maxForce / len;
                vy *= maxForce / len;
            }

            return new Vector2(vx, vy);
        }

        public static StepResult Step(ref PhysicsBodyState body, string[,] terrainGrid, int gridWidth, int gridHeight)
        {
            var result = new StepResult();

            body.VelocityY += body.Weight;

            if (body.VelocityX == 0f && body.VelocityY == 0f)
            {
                result.IsAtRest = true;
                return result;
            }

            float startX = body.X;
            float startY = body.Y;
            float endX = body.X + body.VelocityX;
            float endY = body.Y + body.VelocityY;

            int stepX = body.VelocityX < 0f ? -1 : 1;
            int stepY = body.VelocityY < 0f ? -1 : 1;

            int minGridCol = (int)Mathf.Floor((startX - body.LeftExtent * stepX) / 32f);
            int maxGridCol = (int)Mathf.Floor((endX + body.RightExtent * stepX) / 32f);
            int minGridRow = (int)Mathf.Floor((startY - body.TopExtent * stepY) / 32f);
            int maxGridRow = (int)Mathf.Floor((endY + body.BottomExtent * stepY) / 32f);

            bool hitY = false;
            float targetY = body.Y;

            // 1. Vertical Movement & Collision
            if (body.VelocityY != 0f)
            {
                int leftCol = (int)Mathf.Floor((body.X - body.LeftExtent) / 32f);
                int rightCol = (int)Mathf.Floor((body.X + body.RightExtent) / 32f);

                if (body.HitsTiles && terrainGrid != null)
                {
                    int r = minGridRow + stepY;
                    while (r != maxGridRow + stepY)
                    {
                        for (int c = leftCol; c <= rightCol; c++)
                        {
                            if (IsSolidTile(terrainGrid, c, r, gridWidth, gridHeight))
                            {
                                hitY = true;
                                break;
                            }
                        }

                        if (hitY)
                        {
                            if (body.VelocityY > 0f)
                            {
                                targetY = (r * 32f) - body.BottomExtent - 0.1f;
                            }
                            else
                            {
                                targetY = (r * 32f) + body.TopExtent + 32.1f;
                            }
                            break;
                        }

                        r += stepY;
                    }
                }

                if (hitY)
                {
                    body.VelocityY *= -body.Bounce;
                    body.Y = targetY;

                    if (body.VelocityY < 0f) // Rebounded upwards off floor
                    {
                        // Ground friction
                        float absVx = Mathf.Abs(body.VelocityX);
                        float newAbsVx = Mathf.Max(0f, absVx - body.Friction);
                        body.VelocityX = newAbsVx * Mathf.Sign(body.VelocityX);

                        result.HitFloor = true;
                    }
                    else
                    {
                        result.HitCeiling = true;
                    }
                }
                else
                {
                    body.Y += body.VelocityY;
                }
            }

            // 2. Horizontal Movement & Collision
            bool hitX = false;
            float targetX = body.X;

            if (body.VelocityX != 0f)
            {
                int topRow = (int)Mathf.Floor((body.Y - body.TopExtent) / 32f);
                int bottomRow = (int)Mathf.Floor((body.Y + body.BottomExtent) / 32f);

                if (body.HitsTiles && terrainGrid != null)
                {
                    int c = minGridCol + stepX;
                    while (c != maxGridCol + stepX)
                    {
                        for (int r = topRow; r <= bottomRow; r++)
                        {
                            if (IsSolidTile(terrainGrid, c, r, gridWidth, gridHeight))
                            {
                                hitX = true;
                                break;
                            }
                        }

                        if (hitX)
                        {
                            if (body.VelocityX > 0f)
                            {
                                targetX = (c * 32f) - body.RightExtent - 0.1f;
                                result.HitRightWall = true;
                            }
                            else
                            {
                                targetX = (c * 32f) + body.LeftExtent + 32.1f;
                                result.HitLeftWall = true;
                            }
                            break;
                        }

                        c += stepX;
                    }
                }

                if (hitX)
                {
                    body.VelocityX *= -0.4f; // Fixed Flash wall bounce
                    body.X = targetX;
                }
                else
                {
                    body.X += body.VelocityX;
                }
            }

            result.IsAtRest = body.IsAtRest;
            return result;
        }

        private static bool IsSolidTile(string[,] grid, int c, int r, int w, int h)
        {
            if (c < 0 || c >= w || r < 0 || r >= h)
                return false;

            string tile = grid[r, c];
            if (string.IsNullOrEmpty(tile) || tile == "-")
                return false;

            // Water ripple tiles in row are water visuals, not solid ground
            if (tile.IndexOf("ripple", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            return true;
        }

        public static List<Vector2> SimulateTrajectory(PhysicsBodyState initialState, string[,] terrainGrid, int gridWidth, int gridHeight, int maxSteps = 60)
        {
            var points = new List<Vector2>(maxSteps + 1);
            PhysicsBodyState simBody = initialState;
            points.Add(new Vector2(simBody.X, simBody.Y));

            for (int i = 0; i < maxSteps; i++)
            {
                StepResult res = Step(ref simBody, terrainGrid, gridWidth, gridHeight);
                points.Add(new Vector2(simBody.X, simBody.Y));

                if (res.IsAtRest)
                    break;
            }

            return points;
        }
    }

    public static class MutinyRotationRules
    {
        public static float CharacterMotionDelta(float velocityX)
        {
            return -velocityX * 3f;
        }

        public static float CharacterWaterDelta(float velocityX, float velocityY)
        {
            return -(velocityX + velocityY) * 4f;
        }

        public static float SettleCharacterFloorAngle(float unityAngle)
        {
            float angle = Mathf.DeltaAngle(0f, unityAngle) * 0.5f;
            if (angle > -1f && angle < 1f)
                angle = 0f;
            return angle;
        }

        public static float WeaponRotationMultiplier(string weaponType)
        {
            switch (weaponType)
            {
                case "banana":
                case "dynamite":
                case "rumBottle":
                case "voodooDoll":
                    return 2f;
                case "boulder":
                    return 2.5f;
                default:
                    return 0f;
            }
        }

        public static float WeaponMotionDelta(string weaponType, float velocityX)
        {
            return -velocityX * WeaponRotationMultiplier(weaponType);
        }
    }
}
