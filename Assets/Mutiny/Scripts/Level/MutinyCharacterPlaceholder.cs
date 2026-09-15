using System.Collections.Generic;
using UnityEngine;

namespace Mutiny.Levels
{
    [DisallowMultipleComponent]
    public sealed class MutinyCharacterPlaceholder : MonoBehaviour
    {
        public string CharacterType;
        public int TeamIndex; // 1 = Red / Player, 2 = Blue / Enemy
        public int GridX;
        public int GridY;
        public List<string> PropertyKeys = new List<string>();
        public List<string> PropertyValues = new List<string>();

        public void SetProperties(Dictionary<string, string> properties)
        {
            PropertyKeys.Clear();
            PropertyValues.Clear();
            if (properties == null)
                return;

            foreach (var kvp in properties)
            {
                PropertyKeys.Add(kvp.Key);
                PropertyValues.Add(kvp.Value);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = TeamIndex == 1 ? Color.red : Color.blue;
            Gizmos.DrawWireSphere(transform.position, 0.4f);
        }
    }
}

