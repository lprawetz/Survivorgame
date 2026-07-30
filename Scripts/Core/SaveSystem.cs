using Godot;
using System.Collections.Generic;

namespace SurvivorGame.Core
{
    public partial class SaveSystem : Node
    {
        private const string SAVE_PATH = "user://game_save.json";

        public Dictionary<string, bool> UnlockedEquipmentSets { get; private set; } = new();
        public Dictionary<string, bool> UnlockedCharacters    { get; private set; } = new();

        /// <summary>Extraleben je Charakter-ID. Wird durch Ritual-Questreihe aufgefüllt.</summary>
        public Dictionary<string, int> ExtraLives { get; private set; } = new();

        /// <summary>Fortschritt der Ritual-Questreihe (0–3 Schritte).</summary>
        public int RitualQuestProgress { get; private set; } = 0;

        /// <summary>Fester Weltseed dieses Spielstands. 0 = noch nicht generiert.</summary>
        public int WorldSeed { get; private set; } = 0;

        public override void _Ready() => LoadGame();

        // Liefert den Weltseed; erzeugt ihn beim ersten Aufruf einmalig.
        public int GetOrCreateWorldSeed()
        {
            if (WorldSeed == 0)
            {
                var rng = new RandomNumberGenerator();
                rng.Randomize();
                WorldSeed = (int)rng.Randi() | 1; // niemals 0
                SaveGame();
            }
            return WorldSeed;
        }

        // Setzt einen expliziten Weltseed (z. B. vom Spieler eingegeben).
        public void SetWorldSeed(int seed)
        {
            WorldSeed = seed == 0 ? 1 : seed; // 0 ist reserviert für "nicht gesetzt"
            SaveGame();
        }

        // ─── ExtraLeben-Logik ─────────────────────────────────────────────

        public int GetExtraLives(string characterId) =>
            ExtraLives.TryGetValue(characterId, out int v) ? v : 0;

        public void AddExtraLife(string characterId, int amount = 1)
        {
            ExtraLives.TryGetValue(characterId, out int current);
            ExtraLives[characterId] = current + amount;
            SaveGame();
        }

        /// <summary>
        /// Verbraucht ein Extraleben. Gibt true zurück wenn eines verfügbar war.
        /// </summary>
        public bool ConsumeExtraLife(string characterId)
        {
            if (!ExtraLives.TryGetValue(characterId, out int lives) || lives <= 0)
                return false;
            ExtraLives[characterId] = lives - 1;
            SaveGame();
            return true;
        }

        public void AdvanceRitualQuest()
        {
            RitualQuestProgress = Mathf.Min(RitualQuestProgress + 1, 3);
            // Ab Schritt 1 gibt es Extraleben für alle freigeschalteten Charaktere
            if (RitualQuestProgress >= 1)
                foreach (var kvp in UnlockedCharacters)
                    if (kvp.Value) AddExtraLife(kvp.Key, 1);
            SaveGame();
        }

        // ─── Speichern / Laden ────────────────────────────────────────────

        public void SaveGame()
        {
            var extraLivesRaw = new Godot.Collections.Dictionary();
            foreach (var kv in ExtraLives)
                extraLivesRaw[kv.Key] = kv.Value;

            var saveData = new Godot.Collections.Dictionary
            {
                ["unlockedSets"]       = SerializeBoolDict(UnlockedEquipmentSets),
                ["unlockedCharacters"] = SerializeBoolDict(UnlockedCharacters),
                ["extraLives"]         = extraLivesRaw,
                ["ritualProgress"]     = RitualQuestProgress,
                ["worldSeed"]          = WorldSeed,
            };

            using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Write);
            file.StoreLine(Json.Stringify(saveData));
        }

        public void LoadGame()
        {
            if (!FileAccess.FileExists(SAVE_PATH))
            {
                UnlockedCharacters = new Dictionary<string, bool> { { "Esmeralda", true } };
                SaveGame();
                return;
            }

            using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Read);
            var root = Json.ParseString(file.GetLine()).AsGodotDictionary();

            UnlockedEquipmentSets = DeserializeBoolDict(root["unlockedSets"].AsGodotDictionary());
            UnlockedCharacters    = DeserializeBoolDict(root["unlockedCharacters"].AsGodotDictionary());

            ExtraLives = new Dictionary<string, int>();
            if (root.ContainsKey("extraLives"))
                foreach (var kv in root["extraLives"].AsGodotDictionary())
                    ExtraLives[kv.Key.AsString()] = kv.Value.AsInt32();

            RitualQuestProgress = root.ContainsKey("ritualProgress")
                ? root["ritualProgress"].AsInt32() : 0;

            WorldSeed = root.ContainsKey("worldSeed")
                ? root["worldSeed"].AsInt32() : 0;
        }

        // ─── Hilfsmethoden ────────────────────────────────────────────────

        private static Godot.Collections.Dictionary SerializeBoolDict(Dictionary<string, bool> src)
        {
            var dict = new Godot.Collections.Dictionary();
            foreach (var kv in src) dict[kv.Key] = kv.Value;
            return dict;
        }

        private static Dictionary<string, bool> DeserializeBoolDict(Godot.Collections.Dictionary src)
        {
            var dict = new Dictionary<string, bool>();
            foreach (var kv in src) dict[kv.Key.AsString()] = kv.Value.AsBool();
            return dict;
        }
    }
}