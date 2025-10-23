using Godot;
using System.Collections.Generic;
using SurvivorGame.Core;

namespace SurvivorGame.UI.Menus
{
    public partial class ProgressMenu : Control
    {
        private GridContainer _equipmentGrid;
        private Button _backButton;
        private Panel _infoPanel;
        private Label _setName;
        private Label _setDescription;
        
        private Dictionary<string, EquipmentSetData> _equipmentSets = new()
        {
            { "FireSet", new EquipmentSetData(
                "Flames of the Phoenix",
                "A legendary set that enhances fire magic abilities.",
                new string[] { "Helm", "Chest", "Gloves", "Boots" },
                "res://Assets/Equipment/Sets/fire_set.png")
            },
            // Add other equipment sets here
        };
        
        public override void _Ready()
        {
            _equipmentGrid = GetNode<GridContainer>("EquipmentGrid");
            _backButton = GetNode<Button>("BackButton");
            _infoPanel = GetNode<Panel>("InfoPanel");
            _setName = _infoPanel.GetNode<Label>("SetName");
            _setDescription = _infoPanel.GetNode<Label>("SetDescription");
            
            _backButton.Pressed += OnBackPressed;
            
            SetupEquipmentGrid();
        }
        
        private void SetupEquipmentGrid()
        {
            var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
            
            foreach (var set in _equipmentSets)
            {
                var isUnlocked = saveSystem.UnlockedEquipmentSets.ContainsKey(set.Key) && 
                                saveSystem.UnlockedEquipmentSets[set.Key];
                
                var panel = new PanelContainer();
                var texture = new TextureRect();
                texture.Texture = GD.Load<Texture2D>(set.Value.IconPath);
                
                if (!isUnlocked)
                {
                    // Add darkening effect for locked sets
                    texture.Modulate = new Color(0.5f, 0.5f, 0.5f);
                }
                
                panel.GuiInput += (InputEvent @event) =>
                {
                    if (@event is InputEventMouseButton mouseButton && 
                        mouseButton.ButtonIndex == MouseButton.Left && 
                        mouseButton.Pressed)
                    {
                        OnSetSelected(set.Key);
                    }
                };
                
                panel.AddChild(texture);
                _equipmentGrid.AddChild(panel);
            }
        }
        
        private void OnSetSelected(string setId)
        {
            if (_equipmentSets.TryGetValue(setId, out var set))
            {
                _setName.Text = set.DisplayName;
                _setDescription.Text = set.Description;
                
                var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
                var isUnlocked = saveSystem.UnlockedEquipmentSets.ContainsKey(setId) && 
                                saveSystem.UnlockedEquipmentSets[setId];
                
                if (!isUnlocked)
                {
                    _setDescription.Text += "\n[Locked]";
                }
            }
        }
        
        private void OnBackPressed()
        {
            GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
        }
    }
    
    public class EquipmentSetData
    {
        public string DisplayName { get; }
        public string Description { get; }
        public string[] Pieces { get; }
        public string IconPath { get; }
        
        public EquipmentSetData(string displayName, string description, string[] pieces, string iconPath)
        {
            DisplayName = displayName;
            Description = description;
            Pieces = pieces;
            IconPath = iconPath;
        }
    }
}