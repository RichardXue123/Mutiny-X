using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mutiny.Simulation
{
    [DisallowMultipleComponent]
    public sealed class MutinyTeam : MonoBehaviour
    {
        public int TeamNumber; // 1 = Player / Red, 2 = Enemy / Blue
        public bool IsAiControlled;
        public List<MutinyCharacter> Characters = new List<MutinyCharacter>();
        public MutinyCharacter SelectedCharacter;

        public event Action<MutinyCharacter> OnCharacterSelected;

        public int AliveCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < Characters.Count; i++)
                {
                    if (Characters[i] != null && Characters[i].IsAlive)
                        count++;
                }
                return count;
            }
        }

        public bool IsDefeated => AliveCount == 0;

        public void RegisterCharacter(MutinyCharacter character)
        {
            if (character != null && !Characters.Contains(character))
            {
                Characters.Add(character);
            }
        }

        public void SelectCharacter(MutinyCharacter character)
        {
            if (SelectedCharacter != null)
            {
                SelectedCharacter.IsSelected = false;
            }

            if (character != null && character.IsAlive && Characters.Contains(character))
            {
                SelectedCharacter = character;
                SelectedCharacter.IsSelected = true;
                Mutiny.Presentation.MutinyAudioManager.Instance?.PlayCharacterVoice(character.CharacterType);
                OnCharacterSelected?.Invoke(SelectedCharacter);
            }
            else
            {
                SelectedCharacter = null;
            }
        }

        public MutinyCharacter SelectNextAliveCharacter()
        {
            if (Characters.Count == 0 || IsDefeated)
            {
                SelectCharacter(null);
                return null;
            }

            int startIndex = SelectedCharacter != null ? Characters.IndexOf(SelectedCharacter) : -1;
            for (int i = 1; i <= Characters.Count; i++)
            {
                int nextIndex = (startIndex + i) % Characters.Count;
                if (Characters[nextIndex] != null && Characters[nextIndex].IsAlive)
                {
                    SelectCharacter(Characters[nextIndex]);
                    return SelectedCharacter;
                }
            }

            SelectCharacter(null);
            return null;
        }

        public int TotalTurnsTaken = 0;

        public bool IsTurnComplete()
        {
            if (SelectedCharacter == null || !SelectedCharacter.IsAlive)
                return true;

            return !(SelectedCharacter.CanThrow || SelectedCharacter.CanShoot);
        }

        public MutinyCharacter GetCaptain()
        {
            for (int i = 0; i < Characters.Count; i++)
            {
                var ch = Characters[i];
                if (ch != null && ch.IsAlive && !string.IsNullOrEmpty(ch.CharacterType))
                {
                    if (ch.CharacterType.IndexOf("Captain", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        ch.CharacterType.IndexOf("Chief", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return ch;
                    }
                }
            }
            return null;
        }

        public void StartTurn()
        {
            TotalTurnsTaken++;

            for (int i = 0; i < Characters.Count; i++)
            {
                if (Characters[i] != null)
                {
                    // Team.startTurn gives an otherwise unarmed character one basic
                    // cannonball for that turn.
                    if (Characters[i].IsAlive && !Characters[i].HasAnyWeapon())
                        Characters[i].AddWeapon("cannonball");
                    Characters[i].ResetTurnActions();
                }
            }

            // Team.startTurn in the Flash game always returns to character selection.
            // The captain/nearest character is a camera target, not an implicit choice.
            SelectCharacter(null);
        }

        public void FinishTurn()
        {
            if (SelectedCharacter != null)
            {
                SelectedCharacter.IsSelected = false;
                SelectedCharacter = null;
            }
        }
    }
}
