using System;
using System.Collections.Generic;
using Mutiny.Diagnostics;
using Mutiny.Levels;
using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(MutinyPhysicsBody))]
    public sealed class MutinyCharacter : MonoBehaviour
    {
        // Character.contact plays hitwall only when the prior contact was more
        // than five original 25 Hz ticks ago.  There is no separate footstep or
        // rolling sound in the Flash character path.
        public const int OriginalContactSoundCooldownTicks = 5;

        [Header("Identity")]
        public string CharacterType;
        public int TeamIndex; // 1 = Red / Player, 2 = Blue / Enemy
        public int GridX;
        public int GridY;

        [Header("Health")]
        public float Health = 100f;
        public float MaxHealth = 100f;
        public float ShownHealth = 100f;
        [NonSerialized] public float Evilness = 0f;
        public bool IsAlive = true;
        public bool IsDrowned = false;

        [Header("Action State")]
        public bool CanThrow = true;
        public bool CanShoot = true;
        // Character.weaponLocked blocks replacement/cancellation during PiecesOfEight.
        [NonSerialized] public bool WeaponLocked = false;
        // Character.thrown in the original only records a deliberate self throw.
        // It must not be inferred from velocity: explosions and collisions also move
        // a character while its overlay remains visible.
        [NonSerialized] public bool IsSelfThrown = false;
        [Tooltip("Flash level XML <obj luck>; used by AI candidate sampling.")]
        public float Luck = 5f;
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
        private int m_ContactTimeTicks;
        private int m_ContactSoundCount;
        private bool m_LandDeathPresented;
        private MutinyDeadCharacterEffect m_DeadCharacterEffect;
        private readonly MutinyRotationState m_RotationState = new MutinyRotationState();
        private bool m_HasLoggedRotationVelocity;
        private float m_LastLoggedRotationVelocityX;
        private bool m_OverWater = true;

        public bool IsResolvingHealthDisplay =>
            !IsDrowned && !Mathf.Approximately(ShownHealth, Health);
        public bool HasLandDeathPresentation => m_LandDeathPresented;
        // Kept observable for the production-physics regression test and logs.
        public int ContactSoundCount => m_ContactSoundCount;
        public float LogicalRotationDegrees => m_RotationState.LogicalAngle;

        public event Action OnHealthChanged;
        public event Action OnDeath;

        private void Awake()
        {
            m_SpriteRenderer = GetComponent<SpriteRenderer>();
            PhysicsBody = GetComponent<MutinyPhysicsBody>();
            if (PhysicsBody == null)
                PhysicsBody = gameObject.AddComponent<MutinyPhysicsBody>();
            m_RotationState.Reset(transform.localEulerAngles.z);
            m_HasLoggedRotationVelocity = false;
            m_OverWater = true;
            BindPhysicsEvents();
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
                PhysicsBody.State.HitsBoxes = true;
            }

            if (m_SpriteRenderer != null)
            {
                m_OriginalColor = m_SpriteRenderer.color;
            }

            // New serialized fields are zero in some legacy baked scenes.
            if (Health > 0f && ShownHealth <= 0f)
                ShownHealth = Health;
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

        private void LateUpdate()
        {
            if (PhysicsBody == null)
                return;

            transform.localRotation = Quaternion.Euler(
                0f, 0f, m_RotationState.Sample(PhysicsBody.SimulationInterpolationAlpha));
        }

        public void Initialize(string charType, int team, int gx, int gy, Dictionary<string, string> properties)
        {
            CharacterType = charType;
            TeamIndex = team;
            GridX = gx;
            GridY = gy;
            Health = MaxHealth = 100f;
            ShownHealth = 100f;
            IsAlive = true;
            IsDrowned = false;
            Evilness = 0f;
            m_LandDeathPresented = false;
            m_DeadCharacterEffect = null;
            CanThrow = true;
            CanShoot = true;
            WeaponLocked = false;
            IsSelfThrown = false;
            IsSelected = false;
            m_ContactTimeTicks = 0;
            m_ContactSoundCount = 0;
            m_RotationState.Reset(transform.localEulerAngles.z);
            m_HasLoggedRotationVelocity = false;
            m_OverWater = true;

            ParseWeapons(properties);

            PhysicsBody = GetComponent<MutinyPhysicsBody>();
            if (PhysicsBody == null)
            {
                PhysicsBody = gameObject.AddComponent<MutinyPhysicsBody>();
            }

            BindPhysicsEvents();

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
                if (key == "luck")
                {
                    if (float.TryParse(kvp.Value, out float parsedLuck))
                        Luck = parsedLuck;
                    continue;
                }

                if (key == "x" || key == "y" || key == "type" || key == "maxChests")
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

        public static readonly string[] AllRegisteredWeapons = new string[]
        {
            "cherryBomb", "boulder", "dynamite", "piecesOfEight", "rumBottle",
            "banana", "parachuteBomb", "woodenCrate", "gunpowderBarrel", "seagull",
            "mine", "cannon", "cannonball", "anchor", "voodooDoll", "tidalWave"
        };

        public void UnlockAllWeapons(bool infinite = true)
        {
            for (int i = 0; i < AllRegisteredWeapons.Length; i++)
            {
                string w = AllRegisteredWeapons[i];
                if (infinite)
                {
                    InfiniteWeapons.Add(w);
                    WeaponInventory[w] = int.MaxValue;
                }
                else
                {
                    WeaponInventory[w] = 99;
                }
            }

            CanShoot = true;
        }

        public bool HasWeapon(string weaponType)
        {
            if (string.IsNullOrEmpty(weaponType))
                return false;
            if (InfiniteWeapons.Contains(weaponType))
                return true;
            if (weaponType.Equals("cannon", StringComparison.OrdinalIgnoreCase) && InfiniteWeapons.Contains("cannonball"))
                return true;
            if (weaponType.Equals("cannonball", StringComparison.OrdinalIgnoreCase) && InfiniteWeapons.Contains("cannon"))
                return true;
            if (WeaponInventory.TryGetValue(weaponType, out int count) && count > 0)
                return true;
            if (weaponType.Equals("cannon", StringComparison.OrdinalIgnoreCase) && WeaponInventory.TryGetValue("cannonball", out int c1) && c1 > 0)
                return true;
            if (weaponType.Equals("cannonball", StringComparison.OrdinalIgnoreCase) && WeaponInventory.TryGetValue("cannon", out int c2) && c2 > 0)
                return true;
            return false;
        }

        public bool IsInfinite(string weaponType)
        {
            if (string.IsNullOrEmpty(weaponType))
                return false;
            if (InfiniteWeapons.Contains(weaponType))
                return true;
            if (weaponType.Equals("cannon", StringComparison.OrdinalIgnoreCase) && InfiniteWeapons.Contains("cannonball"))
                return true;
            if (weaponType.Equals("cannonball", StringComparison.OrdinalIgnoreCase) && InfiniteWeapons.Contains("cannon"))
                return true;
            return false;
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
            if (IsInfinite(weaponType))
                return -1; // -1 represents infinite ammo
            if (WeaponInventory.TryGetValue(weaponType, out int count))
                return count;
            if (weaponType.Equals("cannon", StringComparison.OrdinalIgnoreCase) && WeaponInventory.TryGetValue("cannonball", out int c1))
                return c1;
            if (weaponType.Equals("cannonball", StringComparison.OrdinalIgnoreCase) && WeaponInventory.TryGetValue("cannon", out int c2))
                return c2;
            return 0;
        }

        public bool ConsumeWeapon(string weaponType)
        {
            if (IsInfinite(weaponType))
                return true;

            if (WeaponInventory.TryGetValue(weaponType, out int count) && count > 0)
            {
                WeaponInventory[weaponType] = count - 1;
                return true;
            }
            if (weaponType.Equals("cannon", StringComparison.OrdinalIgnoreCase) && WeaponInventory.TryGetValue("cannonball", out int c1) && c1 > 0)
            {
                WeaponInventory["cannonball"] = c1 - 1;
                return true;
            }
            if (weaponType.Equals("cannonball", StringComparison.OrdinalIgnoreCase) && WeaponInventory.TryGetValue("cannon", out int c2) && c2 > 0)
            {
                WeaponInventory["cannon"] = c2 - 1;
                return true;
            }
            return false;
        }

        public void AddWeapon(string weaponType, int count = 1)
        {
            if (string.IsNullOrEmpty(weaponType) || count <= 0 || IsInfinite(weaponType))
                return;

            WeaponInventory.TryGetValue(weaponType, out int existing);
            WeaponInventory[weaponType] = existing + count;
        }

        public void TakeDamage(float damage)
        {
            if (!IsAlive)
                return;

            float roundedDamage = Mathf.Round(damage);
            Health = Mathf.Max(0f, Health - roundedDamage);
            OnHealthChanged?.Invoke();
            GetComponent<MutinyCharacterAnimator>()?.PlayHit();

            if (m_SpriteRenderer != null)
            {
                m_SpriteRenderer.color = new Color(1f, 0.35f, 0.35f, 1f);
                m_HurtFlashTimer = 0.2f;
            }

            if (Health <= 0f)
            {
                MarkDead();
            }
        }

        public void Drown()
        {
            if (IsDrowned)
                return;

            IsDrowned = true;
            Health = 0f;
            ShownHealth = 1f;
            OnHealthChanged?.Invoke();
            PhysicsBodyState state = PhysicsBody != null ? PhysicsBody.State : default;
            float waterPixelY = PhysicsBody != null ? PhysicsBody.WaterPixelY : state.Y;
            MutinyDebugLog.Info("Water",
                $"character drowned name={name} team=T{TeamIndex} position=({state.X:F2},{state.Y:F2}) waterY={waterPixelY:F2}", this);
            MarkDead();
        }

        private void MarkDead()
        {
            if (!IsAlive)
                return;

            IsAlive = false;
            CanThrow = false;
            CanShoot = false;
            IsSelected = false;
            OnDeath?.Invoke();
        }

        private void AdvanceOriginalHealthTick()
        {
            if (IsDrowned || Mathf.Approximately(ShownHealth, Health) || PhysicsBody == null)
                return;

            PhysicsBodyState state = PhysicsBody.State;
            if (state.VelocityX != 0f || Mathf.Abs(state.VelocityY) >= 0.2f)
                return;

            ShownHealth = Mathf.MoveTowards(ShownHealth, Health, 1f);
            OnHealthChanged?.Invoke();

            if (Health <= 0f && ShownHealth < 1f)
                PresentLandDeath();
        }

        private void PresentLandDeath()
        {
            if (m_LandDeathPresented || IsDrowned)
                return;

            m_LandDeathPresented = true;
            if (m_SpriteRenderer != null)
            {
                m_SpriteRenderer.color = m_OriginalColor;
                m_SpriteRenderer.enabled = false;
            }

            m_DeadCharacterEffect = MutinyDeadCharacterEffect.Spawn(this);
            Mutiny.Presentation.MutinyAudioManager.Instance?.PlaySFX("die");
            MutinyDebugLog.Info("Death",
                $"land death presented name={name} shownHealth={ShownHealth:F0} frameCount={(m_DeadCharacterEffect == null ? 0 : m_DeadCharacterEffect.FrameCount)}", this);
        }

        public void ApplyImpulse(Vector2 impulse)
        {
            if (PhysicsBody != null)
            {
                // Convert Unity impulse vector to Flash pixel velocity (Y flipped)
                PhysicsBody.AddVelocity(impulse.x * MutinyPhysics.PixelsPerUnit, -impulse.y * MutinyPhysics.PixelsPerUnit);
            }
        }

        public void MarkSelfThrown(string source)
        {
            if (IsSelfThrown)
                return;

            IsSelfThrown = true;
            MutinyDebugLog.Info("Character",
                $"self throw started name={name} source={source}", this);
        }

        public void ClearSelfThrown(string reason)
        {
            if (!IsSelfThrown)
                return;

            IsSelfThrown = false;
            MutinyDebugLog.Info("Character",
                $"self throw cleared name={name} reason={reason}", this);
        }

        public void ResetTurnActions()
        {
            if (IsAlive)
            {
                CanThrow = true;
                CanShoot = true;
                WeaponLocked = false;
                ClearSelfThrown("start turn");
            }
        }

        private void AdvanceOriginalRotationTick()
        {
            if (PhysicsBody == null)
                return;

            float delta = MutinyRotationRules.CharacterMotionDelta(PhysicsBody.State.VelocityX);
            m_RotationState.AddDelta(PhysicsBody.SimulationTickCount, delta);
            if (!m_HasLoggedRotationVelocity ||
                !Mathf.Approximately(m_LastLoggedRotationVelocityX, PhysicsBody.State.VelocityX))
            {
                MutinyDebugLog.Info("Rotation",
                    $"character angular step tick={PhysicsBody.SimulationTickCount} name={name} vx={PhysicsBody.State.VelocityX:F2} delta={delta:F2} target={m_RotationState.LogicalAngle:F2}", this);
                m_LastLoggedRotationVelocityX = PhysicsBody.State.VelocityX;
                m_HasLoggedRotationVelocity = true;
            }
        }

        private void AdvanceOriginalWaterRotationTick()
        {
            if (PhysicsBody == null)
                return;

            float delta = MutinyRotationRules.CharacterWaterDelta(
                PhysicsBody.State.VelocityX, PhysicsBody.State.VelocityY);
            m_RotationState.AddDelta(PhysicsBody.SimulationTickCount, delta);
        }

        private void HandleFloorContact()
        {
            float before = m_RotationState.LogicalAngle;
            float after = MutinyRotationRules.SettleCharacterFloorAngle(before);
            m_RotationState.SetAngle(PhysicsBody.SimulationTickCount, after);

            if (!Mathf.Approximately(before, after))
            {
                MutinyDebugLog.Info("Rotation",
                    $"character floor damping tick={PhysicsBody.SimulationTickCount} name={name} angle={before:F2}->{after:F2} vx={PhysicsBody.State.VelocityX:F2}", this);
            }

            if (!Mathf.Approximately(before, 0f) && Mathf.Approximately(after, 0f))
            {
                MutinyDebugLog.Info("Animation",
                    $"character rotation settled name={name} from={before:F2} to=0", this);
            }
        }

        internal void ResetOriginalRotation(float angle)
        {
            m_RotationState.Reset(angle);
            transform.localRotation = Quaternion.Euler(0f, 0f, m_RotationState.LogicalAngle);
        }

        internal float SampleOriginalRotation(float alpha)
        {
            return m_RotationState.Sample(alpha);
        }

        private void HandleOriginalPhysicalContact()
        {
            // Character.advance calls contact during advanceMotion, then increments
            // contactTime at the end of the same tick.  A contact immediately after
            // five quiet ticks sees contactTime == 6 and is therefore audible.
            if (m_ContactTimeTicks > OriginalContactSoundCooldownTicks)
            {
                Mutiny.Presentation.MutinyAudioManager.Instance?.PlaySFX("hitwall");
                m_ContactSoundCount++;
                MutinyDebugLog.Info("CharacterAudio",
                    $"hitwall contact name={name} ticksSinceContact={m_ContactTimeTicks}", this);
            }

            m_ContactTimeTicks = 0;
        }

        private void AdvanceOriginalContactTimerTick()
        {
            m_ContactTimeTicks++;
        }

        private void BindPhysicsEvents()
        {
            if (PhysicsBody == null)
                return;

            // C# event subscriptions are not serialized into baked Unity scenes.
            // Rebind in Awake on every Play Mode start as well as after Initialize.
            PhysicsBody.OnEnterWater -= Drown;
            PhysicsBody.OnEnterWater += Drown;
            PhysicsBody.OnFloorLanded -= HandleFloorContact;
            PhysicsBody.OnFloorLanded += HandleFloorContact;
            PhysicsBody.OnFloorLanded -= HandleOriginalPhysicalContact;
            PhysicsBody.OnFloorLanded += HandleOriginalPhysicalContact;
            PhysicsBody.OnCeilingHit -= HandleOriginalPhysicalContact;
            PhysicsBody.OnCeilingHit += HandleOriginalPhysicalContact;
            PhysicsBody.OnWallHit -= HandleOriginalPhysicalContact;
            PhysicsBody.OnWallHit += HandleOriginalPhysicalContact;
            PhysicsBody.OnAfterMotionStep -= AdvanceOriginalRotationTick;
            PhysicsBody.OnAfterMotionStep += AdvanceOriginalRotationTick;
            PhysicsBody.OnAfterMotionStep -= AdvanceOriginalSplashCheck;
            PhysicsBody.OnAfterMotionStep += AdvanceOriginalSplashCheck;
            PhysicsBody.OnWaterMotionAdjusted -= AdvanceOriginalWaterRotationTick;
            PhysicsBody.OnWaterMotionAdjusted += AdvanceOriginalWaterRotationTick;
            PhysicsBody.OnSimulationStep -= AdvanceOriginalHealthTick;
            PhysicsBody.OnSimulationStep += AdvanceOriginalHealthTick;
            PhysicsBody.OnSimulationStep -= AdvanceOriginalContactTimerTick;
            PhysicsBody.OnSimulationStep += AdvanceOriginalContactTimerTick;
        }

        private void AdvanceOriginalSplashCheck()
        {
            PhysicsBodyState state = PhysicsBody.State;
            MutinyWaterSurface.CheckSplashCrossing(state.X, state.Y,
                PhysicsBody.WaterPixelY, ref m_OverWater);
        }

        private void OnDestroy()
        {
            if (PhysicsBody != null)
            {
                PhysicsBody.OnEnterWater -= Drown;
                PhysicsBody.OnFloorLanded -= HandleFloorContact;
                PhysicsBody.OnFloorLanded -= HandleOriginalPhysicalContact;
                PhysicsBody.OnCeilingHit -= HandleOriginalPhysicalContact;
                PhysicsBody.OnWallHit -= HandleOriginalPhysicalContact;
                PhysicsBody.OnAfterMotionStep -= AdvanceOriginalRotationTick;
                PhysicsBody.OnAfterMotionStep -= AdvanceOriginalSplashCheck;
                PhysicsBody.OnWaterMotionAdjusted -= AdvanceOriginalWaterRotationTick;
                PhysicsBody.OnSimulationStep -= AdvanceOriginalHealthTick;
                PhysicsBody.OnSimulationStep -= AdvanceOriginalContactTimerTick;
            }
        }
    }
}
