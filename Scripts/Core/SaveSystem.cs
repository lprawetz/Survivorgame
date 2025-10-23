using Godot;
using System.Collections.Generic;

namespace SurvivorGame.Core
{
    public partial class SaveSystem : Node
    {
        private const string SAVE_PATH = "user://game_save.json";
        
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