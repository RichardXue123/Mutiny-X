using UnityEngine;

namespace Mutiny.Presentation
{
    // Original DefineSprite 801 at 25 fps. Frame numbers and text come from the
    // SWF timeline and DangleFont construction actions, not Unity gameplay code.
    public static class MutinyEndingSequence
    {
        public const int FrameCount = 774;
        public const float FramesPerSecond = 25f;

        public readonly struct ShipSymbol
        {
            public readonly int Id;
            public readonly int FrameCount;

            public ShipSymbol(int id, int frameCount)
            {
                Id = id;
                FrameCount = frameCount;
            }
        }

        public readonly struct ShipLayer
        {
            public readonly int SymbolIndex;
            public readonly Vector2 Offset;

            public ShipLayer(int symbolIndex, float x, float y)
            {
                SymbolIndex = symbolIndex;
                Offset = new Vector2(x, y);
            }
        }

        // DefineSprite 780 has one frame, but its nested clips keep playing.
        // 707/724/741/758 are 16-frame ripples; 767/779 are 12-frame pirates.
        public static readonly ShipSymbol[] ShipSymbols =
        {
            new ShipSymbol(685, 1), new ShipSymbol(687, 1),
            new ShipSymbol(690, 1), new ShipSymbol(707, 16),
            new ShipSymbol(724, 16), new ShipSymbol(741, 16),
            new ShipSymbol(758, 16), new ShipSymbol(767, 12),
            new ShipSymbol(779, 12)
        };

        // Sprite 780's PlaceObject depth order and twip positions (divided
        // by 20), relative to its top-left at stage (161,107).
        public static readonly ShipLayer[] ShipLayers =
        {
            new ShipLayer(0, 137.7f, 133.5f),
            new ShipLayer(1, 0f, 0f),
            new ShipLayer(0, 37f, 38f),
            new ShipLayer(2, 97.25f, 0f),
            new ShipLayer(3, 36.75f, 255f),
            new ShipLayer(4, 100.75f, 255f),
            new ShipLayer(5, 3.75f, 255f),
            new ShipLayer(6, 132.75f, 255f),
            new ShipLayer(4, 68.75f, 255f),
            new ShipLayer(2, 3f, 128f),
            new ShipLayer(2, 67.25f, 160f),
            new ShipLayer(2, 185f, 128f),
            new ShipLayer(7, 88f, 106f),
            new ShipLayer(7, 11.25f, 40f),
            new ShipLayer(7, 11f, 136f),
            new ShipLayer(2, 152f, 128f),
            new ShipLayer(8, 171f, 100f),
            new ShipLayer(0, 174.5f, 134f),
            new ShipLayer(2, 82f, 128.5f),
            new ShipLayer(2, 98.25f, 159.5f),
            new ShipLayer(0, 10f, 104f),
            new ShipLayer(0, 50f, 134f),
            new ShipLayer(0, 48.25f, 121.5f),
            new ShipLayer(0, 36f, 134f),
            new ShipLayer(0, 62f, 135f),
            new ShipLayer(0, 145f, 38f),
            new ShipLayer(0, 74f, 6f),
            new ShipLayer(7, 165.75f, 39.5f)
        };

        // Child MovieClips continue after the parent reaches frame 774 stop().
        public static int ShipFrameAt(int tick, int symbolIndex)
        {
            if (symbolIndex < 0 || symbolIndex >= ShipSymbols.Length)
                return 1;
            return Mathf.Max(0, tick) % ShipSymbols[symbolIndex].FrameCount + 1;
        }

        public readonly struct Dialogue
        {
            public readonly int Start;
            public readonly int End;
            public readonly int Symbol;
            public readonly string Text;
            public readonly Rect Bubble;
            public readonly Vector2 TextOrigin;

            public Dialogue(int start, int end, int symbol, string text,
                Rect bubble, Vector2 textOrigin)
            {
                Start = start;
                End = end;
                Symbol = symbol;
                Text = text;
                Bubble = bubble;
                TextOrigin = textOrigin;
            }
        }

        // Coordinates are in the original 550x400 stage. Sprite 802 places
        // sprite 801 at (210,140), then each dialogue child supplies its own
        // matrix. The bubble bounds include their exported negative origins.
        public static readonly Dialogue[] Dialogues =
        {
            new Dialogue(40, 300, 787,
                "Arrgh, Ahoy mateys! That|be too easy. Them bunch of|lilly livred land lubbers|never stood a chance.|we be both filthy and rich |now. Nothing be standing in |our way. Yo ho ho...",
                new Rect(225f, 72f, 244f, 132f), new Vector2(252f, 89f)),
            new Dialogue(310, 410, 789,
                "Aye... that be true. Unless|we be bumping into the|kraken that is?",
                new Rect(138f, 78f, 244f, 132f), new Vector2(162f, 93f)),
            new Dialogue(425, 525, 792,
                "kraken... what kraken?",
                new Rect(224f, 72f, 244f, 132f), new Vector2(243f, 90f)),
            new Dialogue(535, 635, 794,
                "Yearrrgh... I think he be on |about the sequel captin!",
                new Rect(216f, 13f, 244f, 132f), new Vector2(243f, 30f)),
            new Dialogue(645, 746, 796,
                "...Sequel ...what sequel?",
                new Rect(224f, 73f, 244f, 132f), new Vector2(251f, 90f))
        };

        // DefineSprite 801 moves the final panel on frames 754..770 and
        // leaves it in place when its frame 774 action stops the timeline.
        private static readonly int[] ScorePanelYTwips =
        {
            -4980, -4278, -3621, -3009, -2443, -1921, -1446, -1015,
            -630, -290, 4, 254, 458, 616, 729, 797, 820
        };

        public static int FrameAtElapsed(float elapsedSeconds)
        {
            return Mathf.Clamp(1 + Mathf.FloorToInt(Mathf.Max(0f, elapsedSeconds) * FramesPerSecond),
                1, FrameCount);
        }

        public static int DialogueIndexAtFrame(int frame)
        {
            for (int i = 0; i < Dialogues.Length; i++)
            {
                if (frame >= Dialogues[i].Start && frame <= Dialogues[i].End)
                    return i;
            }
            return -1;
        }

        public static float LineRevealAtFrame(int frame, int dialogueIndex, int lineIndex)
        {
            if (dialogueIndex < 0 || dialogueIndex >= Dialogues.Length || lineIndex < 0)
                return 0f;
            int localFrame = frame - Dialogues[dialogueIndex].Start + 1;
            return Mathf.Clamp01((localFrame - lineIndex * 10f) / 10f);
        }

        public static string LocalizedDialogue(int dialogueIndex) =>
            MutinyLocalization.Text("speech.ending." + dialogueIndex, Dialogues[dialogueIndex].Text);

        public static float DialogueRevealAtFrame(int frame, int dialogueIndex)
        {
            int lineCount = Dialogues[dialogueIndex].Text.Split('|').Length;
            return Mathf.Clamp01((frame - Dialogues[dialogueIndex].Start + 1f) / (lineCount * 10f));
        }

        public static bool IsScoreVisible(int frame) => frame >= 754;
        // The button is a child of sprite 800 from its first placed frame.
        // It moves with the panel rather than waiting for the landing frame.
        public static bool CanReturnToTitle(int frame) => frame >= 754;

        public static float ScorePanelTop(int frame)
        {
            if (!IsScoreVisible(frame))
                return -1000f;
            int index = Mathf.Clamp(frame - 754, 0, ScorePanelYTwips.Length - 1);
            // Sprite 800 shape 797 starts at local x=-180,y=-90.
            return 140f + ScorePanelYTwips[index] / 20f - 90f;
        }

        public static Rect BackButtonRect(int frame)
        {
            // Sprite 800 places button 799 at (0,55). Its 280x24 shape
            // starts at x=-140, and panel shape 797 starts at y=-90.
            return new Rect(141f, ScorePanelTop(frame) + 145f, 280f, 24f);
        }
    }
}
