using Godot;
using System.Collections.Generic;
using SurvivorGame.Characters;

namespace SurvivorGame.Core
{
    /// <summary>
    /// Autoload-Singleton: Überträgt Daten zwischen Szenen (CharacterSelect → GameWorld).
    /// Hält die Spielerauswahlen (Charakter + Variante) für die aktuelle Runde.
    /// </summary>
    public partial class GameState : Node
    {
        public static GameState Instance { get; private set; }

        /// <summary>Auswahlen aller aktiven Spieler für die laufende Runde.</summary>
        public List<PlayerSelection> PlayerSelections { get; private set; } = new()
        {
            new PlayerSelection("Esmeralda", 0) // Fallback: Solo P1
        };

        public int ActivePlayerCount => PlayerSelections.Count;

        public override void _Ready()
        {
            if (Instance == null) Instance = this;
            else { QueueFree(); return; }
        }

        /// <summary>
        /// Wird von CharacterSelect aufgerufen um die Auswahl zu speichern.
        /// </summary>
        public void SetSelections(List<PlayerSelection> selections)
        {
            if (selections == null || selections.Count == 0) return;
            PlayerSelections = new List<PlayerSelection>(selections);
        }

        /// <summary>
        /// Kompatibilitätsmethode: liest das Godot-Array-Format aus CharacterSelect.
        /// Format: Array von Dictionaries mit "CharacterId" und "VariantIndex".
        /// </summary>
        public void SetSelectionsFromArray(Godot.Collections.Array arr)
        {
            PlayerSelections.Clear();
            foreach (var item in arr)
            {
                var dict = item.AsGodotDictionary();
                PlayerSelections.Add(new PlayerSelection(
                    dict["CharacterId"].AsString(),
                    dict["VariantIndex"].AsInt32()
                ));
            }
        }
    }
}
