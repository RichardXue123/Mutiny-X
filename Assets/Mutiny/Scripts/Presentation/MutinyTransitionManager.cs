using System;
using System.Collections;
using UnityEngine;

namespace Mutiny.Presentation
{
    public enum TransitionPhase
    {
        Idle,
        FadeIn,
        Peak,
        FadeOut
    }

    [DisallowMultipleComponent]
    public sealed class MutinyTransitionManager : MonoBehaviour
    {
        public static MutinyTransitionManager Instance { get; private set; }

        public const float OriginalCanvasWidth = 550f;
        public const float OriginalCanvasHeight = 400f;

        // Flash 25 fps: 8 frames fade-in, 8 frames fade-out = 8 / 25f = 0.32s each
        public const float DefaultFadeDuration = 8f / 25f; // 0.32s
        public const float DefaultLoadingHoldDuration = 0.25f; // Authentic pause while displaying "loading"

        public TransitionPhase Phase { get; private set; } = TransitionPhase.Idle;
        public bool IsTransitioning => Phase != TransitionPhase.Idle;
        public bool IsLoadingVisible => Phase == TransitionPhase.Peak && m_ShowLoading;
        public float CurrentAlpha { get; private set; } = 0f;

        private Action m_OnPeakAction;
        private bool m_ShowLoading;
        private float m_Timer;
        private float m_FadeDuration = DefaultFadeDuration;
        private float m_PeakHoldDuration = 0f;
        private Coroutine m_TransitionRoutine;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            if (Instance != null)
                return;

            GameObject host = new GameObject("MutinyTransitionManager");
            DontDestroyOnLoad(host);
            host.AddComponent<MutinyTransitionManager>();
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public static bool IsTransitionActive => Instance != null && Instance.IsTransitioning;

        public static void RequestTransition(Action onPeak, bool showLoading = false, float fadeDuration = DefaultFadeDuration)
        {
            if (Instance != null)
            {
                Instance.TransitionTo(onPeak, showLoading, fadeDuration);
            }
            else
            {
                onPeak?.Invoke();
            }
        }

        public void ResetTransitionState()
        {
            if (m_TransitionRoutine != null)
            {
                StopCoroutine(m_TransitionRoutine);
                m_TransitionRoutine = null;
            }
            Phase = TransitionPhase.Idle;
            CurrentAlpha = 0f;
            m_Timer = 0f;
            m_ShowLoading = false;
            m_OnPeakAction = null;
        }

        public void TransitionTo(Action onPeak, bool showLoading = false, float fadeDuration = DefaultFadeDuration)
        {
            if (Phase != TransitionPhase.Idle)
                return;

            if (m_TransitionRoutine != null)
            {
                StopCoroutine(m_TransitionRoutine);
                m_TransitionRoutine = null;
            }

            m_FadeDuration = Mathf.Max(0.01f, fadeDuration);
            m_OnPeakAction = onPeak;
            m_ShowLoading = showLoading;
            m_PeakHoldDuration = showLoading ? DefaultLoadingHoldDuration : 0.04f;
            m_Timer = 0f;
            CurrentAlpha = 0f;
            Phase = TransitionPhase.FadeIn;

            if (Application.isPlaying)
            {
                m_TransitionRoutine = StartCoroutine(TransitionCoroutine());
            }
        }

        private IEnumerator TransitionCoroutine()
        {
            m_Timer = 0f;

            while (m_Timer < m_FadeDuration)
            {
                m_Timer += Time.unscaledDeltaTime;
                CurrentAlpha = Mathf.Clamp01(m_Timer / m_FadeDuration);
                yield return null;
            }

            CurrentAlpha = 1f;
            Phase = TransitionPhase.Peak;

            try
            {
                m_OnPeakAction?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
            }
            m_OnPeakAction = null;

            if (m_PeakHoldDuration > 0f)
            {
                yield return new WaitForSecondsRealtime(m_PeakHoldDuration);
            }
            else
            {
                yield return null;
            }

            Phase = TransitionPhase.FadeOut;
            m_Timer = 0f;

            while (m_Timer < m_FadeDuration)
            {
                m_Timer += Time.unscaledDeltaTime;
                CurrentAlpha = Mathf.Clamp01(1f - (m_Timer / m_FadeDuration));
                yield return null;
            }

            CurrentAlpha = 0f;
            Phase = TransitionPhase.Idle;
            m_ShowLoading = false;
            m_TransitionRoutine = null;
        }

        public void StepForVerification(float deltaSeconds)
        {
            if (m_TransitionRoutine != null)
            {
                StopCoroutine(m_TransitionRoutine);
                m_TransitionRoutine = null;
            }

            if (Phase == TransitionPhase.Idle)
                return;

            if (Phase == TransitionPhase.FadeIn)
            {
                m_Timer += deltaSeconds;
                CurrentAlpha = Mathf.Clamp01(m_Timer / m_FadeDuration);
                if (m_Timer >= m_FadeDuration)
                {
                    CurrentAlpha = 1f;
                    Phase = TransitionPhase.Peak;
                    m_Timer = 0f;
                    try
                    {
                        m_OnPeakAction?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex, this);
                    }
                    m_OnPeakAction = null;
                }
            }
            else if (Phase == TransitionPhase.Peak)
            {
                m_Timer += deltaSeconds;
                if (m_Timer >= m_PeakHoldDuration)
                {
                    Phase = TransitionPhase.FadeOut;
                    m_Timer = 0f;
                }
            }
            else if (Phase == TransitionPhase.FadeOut)
            {
                m_Timer += deltaSeconds;
                CurrentAlpha = Mathf.Clamp01(1f - (m_Timer / m_FadeDuration));
                if (m_Timer >= m_FadeDuration)
                {
                    CurrentAlpha = 0f;
                    Phase = TransitionPhase.Idle;
                    m_ShowLoading = false;
                }
            }
        }

        private void OnGUI()
        {
            if (Phase == TransitionPhase.Idle || CurrentAlpha <= 0f)
                return;

            GUI.depth = -15000;

            Matrix4x4 oldMatrix = GUI.matrix;
            Color oldColor = GUI.color;

            float scale = Mathf.Min(Screen.width / OriginalCanvasWidth, Screen.height / OriginalCanvasHeight);
            if (scale <= 0f)
                return;

            float left = (Screen.width - OriginalCanvasWidth * scale) * 0.5f;
            float top = (Screen.height - OriginalCanvasHeight * scale) * 0.5f;

            GUI.matrix = Matrix4x4.TRS(new Vector3(left, top, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));

            GUI.color = new Color(1f, 1f, 1f, CurrentAlpha);
            Rect canvasCover = new Rect(0f, 0f, OriginalCanvasWidth, OriginalCanvasHeight);
            GUI.DrawTexture(canvasCover, Texture2D.whiteTexture, ScaleMode.StretchToFill);

            if (IsLoadingVisible && CurrentAlpha >= 0.99f)
            {
                GUI.color = Color.white;
                MutinyLocalizedText.Pirate(
                    new Rect(0f, 178f, OriginalCanvasWidth, 24f),
                    "event.loading", "loading", false, centered: true, tracking: -3);
            }

            GUI.color = oldColor;
            GUI.matrix = oldMatrix;
        }
    }
}
