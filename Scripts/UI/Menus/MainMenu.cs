using Godot;
using SurvivorGame.Core;

namespace SurvivorGame.UI.Menus
{
    public partial class MainMenu : Control
    {
        private Button _playButton;
        private Button _progressButton;
        private Button _optionsButton;
        private Button _quitButton;
        
        public override void _Ready()
        {
            _playButton = GetNode<Button>("VBoxContainer/PlayButton");
            _progressButton = GetNode<Button>("VBoxContainer/ProgressButton");
            _optionsButton = GetNode<Button>("VBoxContainer/OptionsButton");
            _quitButton = GetNode<Button>("VBoxContainer/QuitButton");
            
            _playButton.Pressed += OnPlayPressed;
            _progressButton.Pressed += OnProgressPressed;
            _optionsButton.Pressed += OnOptionsPressed;
            _quitButton.Pressed += OnQuitPressed;
        }

        private void OnPlayPressed()
        {
            GetTree().ChangeSceneToFile("res://Scenes/UI/CharacterSelect.tscn");
        }
        
        private void OnProgressPressed()
        {
            GetTree().ChangeSceneToFile("res://Scenes/UI/ProgressMenu.tscn");
        }
        
        private void OnOptionsPressed()
        {
            GetTree().ChangeSceneToFile("res://Scenes/UI/OptionsMenu.tscn");
        }
        
        private void OnQuitPressed()
        {
            Game.Instance.QuitGame();
        }
    }
}