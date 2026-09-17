using Mutiny.Diagnostics;
using UnityEngine;

namespace Mutiny.Simulation
{
    /// <summary>Flash Cannon: placeable body, pull-back pin, then delayed cannonball fire.</summary>
    [DisallowMultipleComponent]
    public sealed class MutinyCannon : MutinyWeapon
    {
        public const float PlacementOffsetY = -100f;
        public const float InitialEquipmentOffsetY = -10f;
        public const float PlacementRadius = 120f;
        public const float BodyClickRadius = 20f;
        public const float PinClickRadius = 8f;
        public const float PinRestX = -21f;
        public const float PinMinX = -40f;
        public const float PinFireThresholdX = -30f;
        public const float PinReturnPerTick = 15f;
        public const float FireStrength = 30f;

        private float m_TickAccumulator;
        private float m_Visibility = 2f;
        private float m_FireStrength;
        private int m_AiFireTicksRemaining;
        private Vector2 m_AiFireVelocity;
        private bool m_DraggingBody;
        private bool m_DraggingPin;
        private MutinyCannonball m_Cannonball;

        public float PinX { get; private set; } = PinRestX;
        public int RotationDegrees { get; private set; }
        public bool IsDraggingBody => m_DraggingBody;
        public bool IsDraggingPin => m_DraggingPin;
        public MutinyCannonball Cannonball => m_Cannonball;

        public static readonly Vector2 OriginalPivot = new Vector2(26f / 53f, 19f / 38f); // Symbol 850: origin (26, 19) of 53x38

        protected override void Awake()
        {
            WeaponType = "cannon";
            Extent = 10f;
            base.Awake();
            Texture2D texture = Resources.Load<Texture2D>("Art/Weapons/Cannon/1");
            if (texture != null && SpriteRenderer != null)
            {
                texture.filterMode = FilterMode.Point;
                SpriteRenderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
                    OriginalPivot, MutinyPhysics.PixelsPerUnit);
            }
            else
            {
                Sprite sprite = Resources.Load<Sprite>("Art/Weapons/Cannon/1");
                if (sprite != null && SpriteRenderer != null)
                    SpriteRenderer.sprite = sprite;
            }
        }

        public override void Initialize(MutinyCharacter owner)
        {
            base.Initialize(owner);
            PhysicsBody.State.Weight = 0f;
            PhysicsBody.State.HitsBoxes = true;
            PhysicsBody.IsActive = false;
            PlaceAtEquipmentPosition();
            PinX = PinRestX;
            RotationDegrees = 0;
            m_Visibility = 2f;
            m_AiFireTicksRemaining = 0;
            ApplyVisualRotation();
        }

        public void PlaceAtEquipmentPosition()
        {
            if (Owner == null)
                return;
            Vector2 ownerPosition = Owner.PhysicsBody != null
                ? new Vector2(Owner.PhysicsBody.State.X, Owner.PhysicsBody.State.Y)
                : MutinyPhysics.UnityToPixel(Owner.transform.position);
            SetPosition(ownerPosition.x, ownerPosition.y + InitialEquipmentOffsetY);
        }

        public bool TryBeginBodyDrag(Vector2 mousePixels)
        {
            if (IsFired || m_DraggingPin || Vector2.Distance(mousePixels, Position) >= BodyClickRadius)
                return false;
            m_DraggingBody = true;
            return true;
        }

        public bool TryBeginPinDrag(Vector2 mousePixels)
        {
            if (IsFired || m_DraggingBody || Vector2.Distance(mousePixels, PinWorldPosition()) >= PinClickRadius)
                return false;
            m_DraggingPin = true;
            return true;
        }

        public void DragBodyTo(Vector2 mousePixels)
        {
            if (!m_DraggingBody || Owner == null)
                return;
            Vector2 center = new Vector2(Owner.PhysicsBody.State.X, Owner.PhysicsBody.State.Y + PlacementOffsetY);
            Vector2 offset = mousePixels - center;
            if (offset.sqrMagnitude > PlacementRadius * PlacementRadius)
                offset = offset.normalized * PlacementRadius;
            SetPosition(center.x + offset.x, center.y + offset.y);
        }

        public void DragPinTo(Vector2 mousePixels)
        {
            if (!m_DraggingPin)
                return;
            Vector2 delta = mousePixels - Position;
            float degrees = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg + 180f;
            RotationDegrees = Mathf.RoundToInt(Mathf.Repeat(degrees, 360f));
            // Cannon.as first points the cannon directly away from the cursor, then
            // reads mc.pin._x in that rotated local space.  The local x coordinate
            // is consequently the negative pointer distance, rather than world X.
            PinX = Mathf.Clamp(-delta.magnitude, PinMinX, PinRestX);
            ApplyVisualRotation();
        }

        public bool ReleasePointer(MutinyTurnManager turnManager)
        {
            if (m_DraggingBody)
            {
                m_DraggingBody = false;
                return false;
            }

            if (!m_DraggingPin)
                return false;

            m_DraggingPin = false;
            if (PinX >= PinFireThresholdX)
            {
                m_FireStrength = 0f;
                return false;
            }

            m_FireStrength = FireStrength;
            if (Owner != null)
            {
                Owner.CanShoot = false;
                Owner.CanThrow = false;
            }
            turnManager?.NotifyActionStarted();
            MutinyDebugLog.Info("Cannon", $"pin released x={PinX:F1}; loading {FireStrength:F0}", this);
            return true;
        }

        /// <summary>
        /// Cannon.aiPerform: move to the pre-simulated placement, aim, then leave
        /// exactly 25 original ticks before firing the pre-simulated cannonball.
        /// </summary>
        public void BeginAiFire(Vector2 placement, int rotationDegrees, Vector2 velocity)
        {
            if (IsFired || IsFinished)
                return;

            SetPosition(placement.x, placement.y);
            RotationDegrees = Mathf.RoundToInt(Mathf.Repeat(rotationDegrees, 360f));
            ApplyVisualRotation();
            m_AiFireVelocity = velocity;
            m_AiFireTicksRemaining = 25;
            MutinyDebugLog.Info("Cannon",
                $"AI armed placement=({placement.x:F1},{placement.y:F1}) angle={RotationDegrees} delayTicks={m_AiFireTicksRemaining} velocity={velocity}", this);
        }

        protected override void Update()
        {
            if (IsFinished)
                return;
            m_TickAccumulator += Time.deltaTime;
            while (m_TickAccumulator >= MutinyPhysics.TimeStep)
            {
                m_TickAccumulator -= MutinyPhysics.TimeStep;
                AdvanceOriginalTick();
            }
        }

        public void AdvanceOriginalTickForVerification() => AdvanceOriginalTick();

        private void AdvanceOriginalTick()
        {
            if (m_AiFireTicksRemaining > 0)
            {
                m_AiFireTicksRemaining--;
                if (m_AiFireTicksRemaining < 1)
                    FireCannon(m_AiFireVelocity);
                return;
            }

            if (!IsFired)
            {
                if (!m_DraggingPin)
                {
                    PinX = Mathf.Min(PinRestX, PinX + PinReturnPerTick);
                    if (Mathf.Approximately(PinX, PinRestX) && m_FireStrength > 4f)
                    {
                        float radians = RotationDegrees * Mathf.Deg2Rad;
                        FireCannon(new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * m_FireStrength);
                    }
                }
                return;
            }

            m_Visibility = Mathf.Max(0f, m_Visibility - 0.1f);
            if (SpriteRenderer != null && m_Visibility < 1f)
                SpriteRenderer.color = new Color(1f, 1f, 1f, m_Visibility);
            if (m_Cannonball != null)
            {
                SetPosition(m_Cannonball.PhysicsBody.State.X, m_Cannonball.PhysicsBody.State.Y);
                if (m_Cannonball.IsFinished && Mathf.Approximately(m_Visibility, 0f))
                {
                    SpriteRenderer.enabled = false;
                    Finish();
                    Destroy(m_Cannonball.gameObject, 0.2f);
                    Destroy(gameObject, 0.2f);
                }
            }
        }

        private void FireCannon(Vector2 velocity)
        {
            if (IsFired)
                return;
            IsFired = true;
            m_FireStrength = 0f;
            GameObject ballObject = new GameObject("Cannonball");
            m_Cannonball = ballObject.AddComponent<MutinyCannonball>();
            m_Cannonball.Initialize(Owner);
            m_Cannonball.SetLaunchPosition(Position);
            m_Cannonball.Fire(velocity);
            Mutiny.Presentation.MutinyAudioManager.Instance?.PlaySFX("cannon explosion");
            MutinyDebugLog.Info("Cannon", $"fired angle={RotationDegrees} velocity={velocity}", this);
        }

        private Vector2 Position => new Vector2(PhysicsBody.State.X, PhysicsBody.State.Y);

        private Vector2 PinWorldPosition() => Position + DirectionForRotation() * PinX;

        private Vector2 DirectionForRotation()
        {
            float radians = RotationDegrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        private void ApplyVisualRotation()
        {
            // Flash uses positive clockwise angles in its Y-down stage.  Unity's
            // displayed Y axis is up, so the equivalent sprite rotation is negated.
            transform.rotation = Quaternion.Euler(0f, 0f, -RotationDegrees);
        }

        private void SetPosition(float x, float y)
        {
            PhysicsBody.State.X = x;
            PhysicsBody.State.Y = y;
            transform.position = MutinyPhysics.PixelToUnity(x, y);
        }
    }
}
