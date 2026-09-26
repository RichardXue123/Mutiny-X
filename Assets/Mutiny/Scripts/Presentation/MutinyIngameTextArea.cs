using System;
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
        private const int HoldTicks = 40;
        private const int FallTicks = 10;
        private const float HiddenY = 400f;
        private const float ShownY = 370f;

        private readonly Queue<Func<string>> m_Lines = new Queue<Func<string>>();
        private MutinyTurnManager m_TurnManager;
        private MutinySpeechController m_Speech;
        private float m_TickAccumulator;
        private string m_CurrentLine;
        private Func<string> m_CurrentResolver;
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
            m_CurrentResolver = null;
            m_ThisLineFrame = 0;
            m_TickAccumulator = 0f;
            ClipY = HiddenY;

            if (m_TurnManager != null)
                m_TurnManager.OnTurnStarted += HandleTurnStarted;
            MutinyLocalization.Changed -= HandleLanguageChanged;
            MutinyLocalization.Changed += HandleLanguageChanged;
        }

        private void OnDestroy()
        {
            if (m_TurnManager != null)
                m_TurnManager.OnTurnStarted -= HandleTurnStarted;
            MutinyLocalization.Changed -= HandleLanguageChanged;
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
                m_Lines.Enqueue(() => line);
        }

        public void SayCollected(string weaponType)
        {
            // WeaponSelectButton.hoverText["hover_" + type][0] supplies the
            // original display name. The HUD already keeps that same mapping.
            if (string.IsNullOrEmpty(weaponType))
                return;
            m_Lines.Enqueue(() =>
            {
                MutinyGameHUD.GetOriginalActionCopy(weaponType, out string originalName, out _);
                string name = weaponType;
                if (originalName != "weapons")
                    MutinyGameHUD.GetLocalizedActionCopy(weaponType, out name, out _);
                return MutinyLocalization.Text("event.collected_weapon", "collected {0}", name);
            });
        }

        private void HandleTurnStarted(MutinyTeam team)
        {
            if (team == null)
                return;
            bool computer = team.IsAiControlled;
            int number = team.TeamNumber;
            m_Lines.Enqueue(() => computer
                ? MutinyLocalization.Text("event.turn_computer", "Computer, take your turn")
                : MutinyLocalization.Text("event.turn_player", "Player {0}, take your turn", number));
        }

        private void HandleLanguageChanged()
        {
            if (m_CurrentResolver != null)
                m_CurrentLine = m_CurrentResolver();
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
                m_CurrentResolver = m_Lines.Dequeue();
                m_CurrentLine = m_CurrentResolver();
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
                m_CurrentResolver = null;
                ClipY = HiddenY;
            }
        }
    }
}
