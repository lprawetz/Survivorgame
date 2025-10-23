using Godot;

namespace SurvivorGame.UI.Menus
{
    public partial class OptionsMenu : Control
    {
        private HSlider _masterVolumeSlider;
        private HSlider _musicVolumeSlider;
        private HSlider _sfxVolumeSlider;
        private Button _backButton;
        
        public override void _Ready()
        {
            _masterVolumeSlider = GetNode<HSlider>("VBoxContainer/MasterVolume");
            _musicVolumeSlider = GetNode<HSlider>("VBoxContainer/MusicVolume");
            _sfxVolumeSlider = GetNode<HSlider>("VBoxContainer/SFXVolume");
            _backButton = GetNode<Button>("BackButton");
            
            _masterVolumeSlider.ValueChanged += OnMasterVolumeChanged;
            _musicVolumeSlider.ValueChanged += OnMusicVolumeChanged;
            _sfxVolumeSlider.ValueChanged += OnSFXVolumeChanged;
            _backButton.Pressed += OnBackPressed;
            
            LoadSettings();
        }
        
        private void LoadSettings()
        {
            // Load settings from config file
            if (FileAccess.FileExists("user://settings.cfg"))
            {
                using var file = FileAccess.Open("user://settings.cfg", FileAccess.ModeFlags.Read);
                _masterVolumeSlider.Value = float.Parse(file.GetLine());
                _musicVolumeSlider.Value = float.Parse(file.GetLine());
                _sfxVolumeSlider.Value = float.Parse(file.GetLine());
            }
        }
        
        private void SaveSettings()
        {
            using var file = FileAccess.Open("user://settings.cfg", FileAccess.ModeFlags.Write);
            file.StoreLine(_masterVolumeSlider.Value.ToString());
            file.StoreLine(_musicVolumeSlider.Value.ToString());
            file.StoreLine(_sfxVolumeSlider.Value.ToString());
        }
        
        private void OnMasterVolumeChanged(double value)
        {
            // TODO: Implement master volume control
            SaveSettings();
        }
        
        private void OnMusicVolumeChanged(double value)
        {
            // TODO: Implement music volume control
            SaveSettings();
        }
        
        private void OnSFXVolumeChanged(double value)
        {
            // TODO: Implement SFX volume control
            SaveSettings();
        }
        
        private void OnBackPressed()
        {
            SaveSettings();
            GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
        }
    }
}