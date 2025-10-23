using Godot;
using System.Collections.Generic;
using SurvivorGame.Core;

namespace SurvivorGame.UI.Menus
{
    public partial class CharacterSelect : Control
    {
        private const string SILHOUETTE_TEXTURE = "res://Assets/UI/Characters/character_silhouette.png";
        
        private GridContainer _characterGrid;
        private Panel _infoPanel;
        private Label _characterName;
        private Label _characterDescription;
        private Button _startButton;
        private Button _backButton;
        
        private Dictionary<string, CharacterData> _characters = new()
        {
            { "Esmeralda", new CharacterData(
                "Esmeralda",
                "Fire Mage",
                "A powerful fire mage with the ability to cast devastating spells.",
                "res://Assets/Characters/Esmeralda/portrait.png")
            },
            { "Earth", new CharacterData(
                "Terra",
                "Earth Warrior",
                "A stalwart defender who harnesses the power of stone and earth.",
                "res://Assets/Characters/Earth/portrait.png")
            },
            // Add other characters here
        };
        
        private string _selectedCharacter = "Esmeralda";
        
        public override void _Ready()
        {
            _characterGrid = GetNode<GridContainer>("CharacterGrid");
            _infoPanel = GetNode<Panel>("InfoPanel");
            _characterName = _infoPanel.GetNode<Label>("CharacterName");
            _characterDescription = _infoPanel.GetNode<Label>("CharacterDescription");
            _startButton = GetNode<Button>("StartButton");
            _backButton = GetNode<Button>("BackButton");
            
            _startButton.Pressed += OnStartPressed;
            _backButton.Pressed += OnBackPressed;
            
            SetupCharacterGrid();
            UpdateCharacterInfo("Esmeralda");
        }
        
        private void SetupCharacterGrid()
        {
            var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
            
            foreach (var character in _characters)
            {
                var button = new TextureButton();
                var isUnlocked = saveSystem.UnlockedCharacters.ContainsKey(character.Key) && 
                                saveSystem.UnlockedCharacters[character.Key];
                
                button.TextureNormal = GD.Load<Texture2D>(
                    isUnlocked ? character.Value.PortraitPath : SILHOUETTE_TEXTURE
                );
                
                button.Pressed += () => OnCharacterSelected(character.Key);
                _characterGrid.AddChild(button);
            }
        }
        
        private void OnCharacterSelected(string characterId)
        {
            _selectedCharacter = characterId;
            UpdateCharacterInfo(characterId);
        }
        
        private void UpdateCharacterInfo(string characterId)
        {
            if (_characters.TryGetValue(characterId, out var character))
            {
                _characterName.Text = character.DisplayName;
                _characterDescription.Text = character.Description;
            }
        }
        
        private void OnStartPressed()
        {
            // TODO: Pass selected character to game scene
            GetTree().ChangeSceneToFile("res://Scenes/World/GameWorld.tscn");
        }
        
        private void OnBackPressed()
        {
            GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
        }
    }
    
    public class CharacterData
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public string PortraitPath { get; }
        
        public CharacterData(string id, string displayName, string description, string portraitPath)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            PortraitPath = portraitPath;
        }
    }
}