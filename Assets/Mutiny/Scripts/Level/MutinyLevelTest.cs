using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mutiny.Levels
{
    public sealed class MutinyLevelTest : MonoBehaviour
    {
        [SerializeField]
        private TextAsset levelXml;

        private void Start()
        {
            if (levelXml == null)
            {
                Debug.LogError("Please assign a level XML.", this);
                return;
            }

            MutinyLevelData level;
            try
            {
                level = MutinyLevelXmlParser.Parse(levelXml.text, levelXml.name);
            }
            catch (FormatException exception)
            {
                Debug.LogError($"Level validation failed: {exception.Message}", this);
                return;
            }

            Debug.Log(
                $"Level loaded: {level.Width} x {level.Height}, " +
                $"players={level.Players}, objects={level.Objects.Count}", this);

            var tileTypes = new HashSet<string>(StringComparer.Ordinal);
            for (int y = 0; y < level.Height; y++)
            {
                for (int x = 0; x < level.Width; x++)
                {
                    string tile = level.Terrain[y, x];
                    if (tile != null)
                        tileTypes.Add(tile);
                }
            }

            Debug.Log($"Terrain tile types: {tileTypes.Count}", this);

            foreach (MutinyLevelObject obj in level.Objects)
            {
                Debug.Log(obj.ToString(), this);
                foreach (KeyValuePair<string, string> property in obj.Properties)
                    Debug.Log($"    {property.Key} = {property.Value}", this);
            }
        }
    }
}
