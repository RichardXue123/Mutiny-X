using UnityEngine;

namespace Mutiny.Simulation
{
    internal static class MutinyCombatMath
    {
        internal static bool ExplosionHit(Vector2 target, Vector2 origin, float radius, float maxDamage,
            out float ratio, out Vector2 impulse)
        {
            float dx = target.x - origin.x, dy = target.y - origin.y;
            float distSq = dx * dx + dy * dy;
            ratio = 0f; impulse = Vector2.zero;
            if (distSq > radius * radius) return false;
            float dist = Mathf.Sqrt(distSq), nx, ny;
            if (dist < 0.0001f) { nx = 0f; ny = -1f; dist = 1f; }
            else { nx = dx / dist; ny = dy / dist; }
            ratio = 1f - dist / radius;
            float force = 0.06f * ratio * maxDamage;
            impulse = new Vector2(nx * 5f * force, ny * 5f * force - force * 6f);
            return true;
        }
    }
}
