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

        public override void _Ready() => LoadGame();

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
        
        public Dictionary<string, bool> UnlockedEquipmentSets { get; private set; } = new();
        public Dictionary<string, bool> UnlockedCharacters { get; private set; } = new();
        
        public override void _Ready()
        {
            LoadGame();
        }
        
        public void SaveGame()
        {
            var saveData = new Dictionary<string, object>
            {
                { "unlockedSets", UnlockedEquipmentSets },
                { "unlockedCharacters", UnlockedCharacters }
            };
            
            using var saveFile = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Write);
            var jsonString = Json.Stringify(saveData);
            saveFile.StoreLine(jsonString);
        }
        
        public void LoadGame()
        {
            if (!FileAccess.FileExists(SAVE_PATH))
            {
                // Initialize with default values
                UnlockedEquipmentSets = new Dictionary<string, bool>();
                UnlockedCharacters = new Dictionary<string, bool>
                {
                    { "Esmeralda", true } // Starting character is always unlocked
                };
                SaveGame();
                return;
            }

            using var saveFile = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Read);
            var jsonString = saveFile.GetLine();
            var json = Json.ParseString(jsonString).AsGodotDictionary();
            
            // Load equipment sets
            var sets = json["unlockedSets"].AsGodotDictionary();
            UnlockedEquipmentSets = new Dictionary<string, bool>();
            foreach (var key in sets.Keys)
            {
                UnlockedEquipmentSets[key.AsString()] = sets[key].AsBool();
            }
            
            // Load characters
            var chars = json["unlockedCharacters"].AsGodotDictionary();
            UnlockedCharacters = new Dictionary<string, bool>();
            foreach (var key in chars.Keys)
            {
                UnlockedCharacters[key.AsString()] = chars[key].AsBool();
            }
        }
    }
}