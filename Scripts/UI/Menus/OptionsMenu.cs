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
            var cfg = new ConfigFile();
            if (cfg.Load("user://settings.cfg") == Error.Ok)
            {
                _masterVolumeSlider.Value = (double)cfg.GetValue("audio", "master", 1.0);
                _musicVolumeSlider.Value  = (double)cfg.GetValue("audio", "music",  1.0);
                _sfxVolumeSlider.Value    = (double)cfg.GetValue("audio", "sfx",    1.0);
            }
            // Werte sofort auf AudioServer anwenden
            ApplyVolume("Master", _masterVolumeSlider.Value);
            ApplyVolume("Music",  _musicVolumeSlider.Value);
            ApplyVolume("SFX",    _sfxVolumeSlider.Value);
        }

        private void SaveSettings()
        {
            var cfg = new ConfigFile();
            cfg.SetValue("audio", "master", _masterVolumeSlider.Value);
            cfg.SetValue("audio", "music",  _musicVolumeSlider.Value);
            cfg.SetValue("audio", "sfx",    _sfxVolumeSlider.Value);
            cfg.Save("user://settings.cfg");
        }
        
        private void OnMasterVolumeChanged(double value)
        {
            ApplyVolume("Master", value);
            SaveSettings();
        }

        private void OnMusicVolumeChanged(double value)
        {
            ApplyVolume("Music", value);
            SaveSettings();
        }

        private void OnSFXVolumeChanged(double value)
        {
            ApplyVolume("SFX", value);
            SaveSettings();
        }

        private static void ApplyVolume(string busName, double linearValue)
        {
            int idx = AudioServer.GetBusIndex(busName);
            if (idx < 0) return;
            // Lautstärke 0.0–1.0 → Dezibel; Stille bei 0 = -80 dB
            float db = linearValue <= 0.0001 ? -80f : Mathf.LinearToDb((float)linearValue);
            AudioServer.SetBusVolumeDb(idx, db);
        }
        
        private void OnBackPressed()
        {
            SaveSettings();
            GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
        }
    }
}