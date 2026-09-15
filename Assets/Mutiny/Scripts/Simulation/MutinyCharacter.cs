using System;
using System.Collections.Generic;
using Mutiny.Levels;
using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(MutinyPhysicsBody))]
    public sealed class MutinyCharacter : MonoBehaviour
    {
        [Header("Identity")]
        public string CharacterType;
        public int TeamIndex; // 1 = Red / Player, 2 = Blue / Enemy
        public int GridX;
        public int GridY;

        [Header("Health")]
        public float Health = 100f;
        public float MaxHealth = 100f;
        public bool IsAlive = true;
        public bool IsDrowned = false;

        [Header("Action State")]
        public bool CanThrow = true;
        public bool CanShoot = true;
        public bool IsSelected = false;
        [NonSerialized] public bool IsHovered = false;

        [Header("Inventory")]
        [NonSerialized] public Dictionary<string, int> WeaponInventory = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        [NonSerialized] public HashSet<string> InfiniteWeapons = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private MutinyPhysicsBody m_PhysicsBody;
        public MutinyPhysicsBody PhysicsBody
        {
            get
            {
                if (m_PhysicsBody == null)
                    m_PhysicsBody = GetComponent<MutinyPhysicsBody>();
                return m_PhysicsBody;
            }
            private set => m_PhysicsBody = value;
        }

        private SpriteRenderer m_SpriteRenderer;
        private Color m_OriginalColor = Color.white;
        private float m_HurtFlashTimer = 0f;

        public event Action OnHealthChanged;
        public event Action OnDeath;

        private void Awake()
        {
            m_SpriteRenderer = GetComponent<SpriteRenderer>();
            PhysicsBody = GetComponent<MutinyPhysicsBody>();
            if (PhysicsBody == null)
                PhysicsBody = gameObject.AddComponent<MutinyPhysicsBody>();
        }

        private void Start()
        {
            RestoreInventoryFromLevelXml();
            if (!HasAnyWeapon())
                AddWeapon("cannonball");

            // Reapply immutable Character Solid parameters so legacy baked scenes do
            // not depend on a serialized runtime struct.
            if (PhysicsBody != null)
            {
                PhysicsBody.State.Weight = 1f;
                PhysicsBody.State.Friction = 2f;
                PhysicsBody.State.LeftExtent = 6f;
                PhysicsBody.State.RightExtent = 6f;
                PhysicsBody.State.TopExtent = 8f;
                PhysicsBody.State.BottomExtent = 8f;
                PhysicsBody.State.HitsTiles = true;
            }

            if (m_SpriteRenderer != null)
            {
                m_OriginalColor = m_SpriteRenderer.color;
            }
        }

        private void RestoreInventoryFromLevelXml()
        {
            MutinyLevelController controller = FindAnyObjectByType<MutinyLevelController>();
            if (controller == null || controller.LevelXml == null)
                return;

            try
            {
                MutinyLevelData data = MutinyLevelXmlParser.Parse(controller.LevelXml.text, controller.LevelXml.name);
                for (int i = 0; i < data.Objects.Count; i++)
                {
                    MutinyLevelObject obj = data.Objects[i];
                    if (obj.X == GridX && obj.Y == GridY &&
                        string.Equals(obj.Type, CharacterType, StringComparison.Ordinal))
                    {
                        ParseWeapons(obj.Properties);
                        return;
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private void Update()
        {
            if (m_HurtFlashTimer > 0f)
            {
                m_HurtFlashTimer -= Time.deltaTime;
                if (m_HurtFlashTimer <= 0f && m_SpriteRenderer != null)
                {
                    m_SpriteRenderer.color = m_OriginalColor;
                }
            }

            if (PhysicsBody != null && m_SpriteRenderer != null && Mathf.Abs(PhysicsBody.State.VelocityX) > 0.2f)
            {
                m_SpriteRenderer.flipX = PhysicsBody.State.VelocityX < 0f;
            }
        }

        public void Initialize(string charType, int team, int gx, int gy, Dictionary<string, string> properties)
        {
            CharacterType = charType;
            TeamIndex = team;
            GridX = gx;
            GridY = gy;
            Health = MaxHealth = 100f;
            IsAlive = true;
            IsDrowned = false;
            CanThrow = true;
            CanShoot = true;
            IsSelected = false;

            ParseWeapons(properties);

            PhysicsBody = GetComponent<MutinyPhysicsBody>();
            if (PhysicsBody == null)
            {
                PhysicsBody = gameObject.AddComponent<MutinyPhysicsBody>();
            }

            PhysicsBody.OnEnterWater -= Drown;
            PhysicsBody.OnEnterWater += Drown;
            PhysicsBody.OnSimulationStep -= AdvanceOriginalVisualTick;
            PhysicsBody.OnSimulationStep += AdvanceOriginalVisualTick;

            Vector2 px = MutinyPhysics.UnityToPixel(transform.position);
            PhysicsBody.State = PhysicsBodyState.CreateDefault(px.x, px.y);
            PhysicsBody.State.Friction = 2.0f; // Character friction in Flash
            PhysicsBody.State.LeftExtent = 6f;
            PhysicsBody.State.RightExtent = 6f;
            PhysicsBody.State.TopExtent = 8f;
            PhysicsBody.State.BottomExtent = 8f;
        }

        public void ParseWeapons(Dictionary<string, string> properties)
        {
            WeaponInventory.Clear();
            InfiniteWeapons.Clear();
            if (properties == null)
                return;

            foreach (var kvp in properties)
            {
                string key = kvp.Key;
                if (key == "x" || key == "y" || key == "type" || key == "luck" || key == "maxChests")
                    continue;

                if (int.TryParse(kvp.Value, out int count))
                {
                    // In Nitrome Flash Mutiny, value 10 indicates infinite ammo
                    if (count == 10)
                    {
                        InfiniteWeapons.Add(key);
                        WeaponInventory[key] = int.MaxValue;
                    }
                    else if (count > 0)
                    {
                        WeaponInventory[key] = count;
                    }
                }
            }
        }

        public bool HasWeapon(string weaponType)
        {
            if (string.IsNullOrEmpty(weaponType))
                return false;
            if (InfiniteWeapons.Contains(weaponType))
                return true;
            return WeaponInventory.TryGetValue(weaponType, out int count) && count > 0;
        }

        public bool IsInfinite(string weaponType)
        {
            return InfiniteWeapons.Contains(weaponType);
        }

        public bool HasAnyWeapon()
        {
            if (InfiniteWeapons.Count > 0)
                return true;
            foreach (KeyValuePair<string, int> entry in WeaponInventory)
            {
                if (entry.Value > 0)
                    return true;
            }
            return false;
        }

        public int GetAmmunition(string weaponType)
        {
            if (InfiniteWeapons.Contains(weaponType))
                return -1; // -1 represents infinite ammo
            return WeaponInventory.TryGetValue(weaponType, out int count) ? count : 0;
        }

        public bool ConsumeWeapon(string weaponType)
        {
            if (InfiniteWeapons.Contains(weaponType))
                return true;

            if (WeaponInventory.TryGetValue(weaponType, out int count) && count > 0)
            {
                WeaponInventory[weaponType] = count - 1;
                return true;
            }
            return false;
        }

        public void AddWeapon(string weaponType, int count = 1)
        {
            if (string.IsNullOrEmpty(weaponType) || count <= 0 || InfiniteWeapons.Contains(weaponType))
                return;

            WeaponInventory.TryGetValue(weaponType, out int existing);
            WeaponInventory[weaponType] = existing + count;
        }

        public void TakeDamage(float damage)
        {
            if (!IsAlive)
                return;

            Health = Mathf.Max(0f, Health - damage);
            OnHealthChanged?.Invoke();
            GetComponent<MutinyCharacterAnimator>()?.PlayHit();

            Mutiny.Presentation.MutinyAudioManager.Instance?.PlaySFX("hitwall");

            if (m_SpriteRenderer != null)
            {
                m_SpriteRenderer.color = new Color(1f, 0.35f, 0.35f, 1f);
                m_HurtFlashTimer = 0.2f;
            }

            if (Health <= 0f)
            {
                Die();
            }
        }

        public void Drown()
        {
            if (IsDrowned)
                return;

            IsDrowned = true;
            Health = 0f;
            OnHealthChanged?.Invoke();
            Mutiny.Presentation.MutinyAudioManager.Instance?.PlaySFX("splash");
            Die();
        }

        private void Die()
        {
            if (!IsAlive)
                return;

            IsAlive = false;
            CanThrow = false;
            CanShoot = false;
            IsSelected = false;
            Mutiny.Presentation.MutinyAudioManager.Instance?.PlaySFX("die");
            OnDeath?.Invoke();

            if (m_SpriteRenderer != null)
            {
                m_SpriteRenderer.color = new Color(0.45f, 0.45f, 0.45f, 0.55f);
            }
        }

        public void ApplyImpulse(Vector2 impulse)
        {
            if (PhysicsBody != null)
            {
                // Convert Unity impulse vector to Flash pixel velocity (Y flipped)
                PhysicsBody.AddVelocity(impulse.x * MutinyPhysics.PixelsPerUnit, -impulse.y * MutinyPhysics.PixelsPerUnit);
            }
        }

        public void ResetTurnActions()
        {
            if (IsAlive)
            {
                CanThrow = true;
                CanShoot = true;
            }
        }

        private void AdvanceOriginalVisualTick()
        {
            if (PhysicsBody != null && Mathf.Abs(PhysicsBody.State.VelocityX) > 0f)
                transform.Rotate(0f, 0f, -PhysicsBody.State.VelocityX * 3f);
        }

        private void OnDestroy()
        {
            if (PhysicsBody != null)
            {
                PhysicsBody.OnEnterWater -= Drown;
                PhysicsBody.OnSimulationStep -= AdvanceOriginalVisualTick;
            }
        }
    }
}
