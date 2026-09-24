using System.Collections.Generic;
using Mutiny.Simulation;
using UnityEngine;

namespace Mutiny.Presentation
{
    // Flash root.text is an "ingame text area" clip (symbol 1896) containing a
    // centered DangleFont textField. Team.startTurn and TreasureChest.advance both
    // call its say method; its own onEnterFrame advances the queued lines and _y.
    [DisallowMultipleComponent]
    public sealed class MutinyIngameTextArea : MonoBehaviour
    {
        private const float TickSeconds = 1f / 25f;
        private const int RiseTicks = 10;
        private const int HoldTicks = 75;
        private const int FallTicks = 10;
        private const float HiddenY = 400f;
        private const float ShownY = 370f;

        private readonly Queue<string> m_Lines = new Queue<string>();
        private MutinyTurnManager m_TurnManager;
        private MutinySpeechController m_Speech;
        private float m_TickAccumulator;
        private string m_CurrentLine;
        private int m_ThisLineFrame;

        public string VisibleText => m_CurrentLine;
        public int PendingLineCount => m_Lines.Count;
        public float ClipY { get; private set; } = HiddenY;
        public bool IsVisible => m_CurrentLine != null &&
                                 (m_Speech == null || !m_Speech.HasPendingOrActiveSpeech);

        public void Initialize(MutinyTurnManager turnManager, MutinySpeechController speech)
        {
            if (m_TurnManager != null)
                m_TurnManager.OnTurnStarted -= HandleTurnStarted;

            m_TurnManager = turnManager;
            m_Speech = speech;
            m_Lines.Clear();
            m_CurrentLine = null;
            m_ThisLineFrame = 0;
            m_TickAccumulator = 0f;
            ClipY = HiddenY;

            if (m_TurnManager != null)
                m_TurnManager.OnTurnStarted += HandleTurnStarted;
        }

        private void OnDestroy()
        {
            if (m_TurnManager != null)
                m_TurnManager.OnTurnStarted -= HandleTurnStarted;
        }

        private void Update()
        {
            m_TickAccumulator += Time.unscaledDeltaTime;
            while (m_TickAccumulator >= TickSeconds)
            {
                m_TickAccumulator -= TickSeconds;
                AdvanceOriginalTick();
            }
        }

        public void Say(string line)
        {
            if (!string.IsNullOrEmpty(line))
                m_Lines.Enqueue(line);
        }

        public void SayCollected(string weaponType)
        {
            // WeaponSelectButton.hoverText["hover_" + type][0] supplies the
            // original display name. The HUD already keeps that same mapping.
            MutinyGameHUD.GetOriginalActionCopy(weaponType, out string name, out _);
            if (name == "weapons")
                name = weaponType;
            if (!string.IsNullOrEmpty(name))
                Say("collected " + name);
        }

        private void HandleTurnStarted(MutinyTeam team)
        {
            if (team == null)
                return;
            Say(team.IsAiControlled
                ? "Computer, take your turn"
                : $"Player {team.TeamNumber}, take your turn");
        }

        // The exported IngameTextArea class records thisLineFrame, a queue and _y,
        // but its protected AS2 body has no trustworthy timing constants. Animate
        // at the SWF's 25 Hz and keep those values in one place for frame comparison.
        public void AdvanceOriginalTick()
        {
            if (m_TurnManager == null || m_TurnManager.CurrentPhase == TurnPhase.NotStarted ||
                (m_Speech != null && m_Speech.HasPendingOrActiveSpeech))
                return;

            MutinyFrontendController frontend = FindAnyObjectByType<MutinyFrontendController>();
            if (frontend != null && frontend.CurrentPage != MutinyFrontendPage.Gameplay)
                return;

            if (m_CurrentLine == null)
            {
                if (m_Lines.Count == 0)
                    return;
                m_CurrentLine = m_Lines.Dequeue();
                m_ThisLineFrame = 0;
                ClipY = HiddenY;
            }

            m_ThisLineFrame++;
            if (m_ThisLineFrame <= RiseTicks)
                ClipY = Mathf.Lerp(HiddenY, ShownY, m_ThisLineFrame / (float)RiseTicks);
            else if (m_ThisLineFrame <= RiseTicks + HoldTicks)
                ClipY = ShownY;
            else
                ClipY = Mathf.Lerp(ShownY, HiddenY,
                    (m_ThisLineFrame - RiseTicks - HoldTicks) / (float)FallTicks);

            if (m_ThisLineFrame >= RiseTicks + HoldTicks + FallTicks)
            {
                m_CurrentLine = null;
                ClipY = HiddenY;
            }
        }
    }
}
