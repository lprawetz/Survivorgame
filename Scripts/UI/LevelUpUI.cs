using Godot;
using System.Collections.Generic;
using SurvivorGame.Combat;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Level-Up-UI: Pausiert das Spiel und zeigt 3 Skill-Optionen zur Auswahl.
    /// Verbindet sich automatisch mit dem LevelUpSystem der Szene.
    ///
    /// SZENEN-AUFBAU (LevelUpUI.tscn):
    ///   CanvasLayer
    ///     Panel  "Backdrop"  (Vollbild, halbtransparent)
    ///       VBoxContainer
    ///         Label  "Title"
    ///         HBoxContainer  "OptionsRow"
    ///           [3 Buttons werden dynamisch erstellt]
    ///
    /// Verbindung in GameWorld.tscn:
    ///   ExperienceSystem.LevelUp → LevelUpSystem.OnPlayerLevelUp
    ///   LevelUpSystem.SkillOptionsReady → LevelUpUI.OnSkillOptionsReady (automatisch)
    /// </summary>
    public partial class LevelUpUI : CanvasLayer
    {
        private Panel         _backdrop;
        private Label         _titleLabel;
        private HBoxContainer _optionsRow;
        private LevelUpSystem _levelUpSystem;

        public override void _Ready()
        {
            _backdrop   = GetNode<Panel>("Backdrop");
            _titleLabel = GetNode<Label>("Backdrop/VBoxContainer/Title");
            _optionsRow = GetNode<HBoxContainer>("Backdrop/VBoxContainer/OptionsRow");

            // Standardmäßig versteckt
            _backdrop.Visible = false;

            // Automatisch mit LevelUpSystem verbinden
            CallDeferred(MethodName.ConnectToLevelUpSystem);
        }

        private void ConnectToLevelUpSystem()
        {
            _levelUpSystem = GetTree().GetFirstNodeInGroup("level_up_system") as LevelUpSystem;
            if (_levelUpSystem != null)
                _levelUpSystem.SkillOptionsReady += OnSkillOptionsReady;
        }

        private void OnSkillOptionsReady()
        {
            if (_levelUpSystem == null) return;

            // Buttons leeren und neu aufbauen
            foreach (Node child in _optionsRow.GetChildren())
                child.QueueFree();

            var options = _levelUpSystem.CurrentOptions;
            for (int i = 0; i < options.Count; i++)
            {
                int capturedIndex = i;
                var opt = options[i];

                var card = new PanelContainer();
                card.CustomMinimumSize = new Vector2(200, 130);

                var vbox = new VBoxContainer();
                card.AddChild(vbox);

                var nameLabel = new Label
                {
                    Text                = opt.DisplayName,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    AutowrapMode        = TextServer.AutowrapMode.WordSmart,
                };
                nameLabel.AddThemeFontSizeOverride("font_size", 16);
                vbox.AddChild(nameLabel);

                var descLabel = new Label
                {
                    Text                = opt.Description,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    AutowrapMode        = TextServer.AutowrapMode.WordSmart,
                };
                descLabel.AddThemeFontSizeOverride("font_size", 12);
                vbox.AddChild(descLabel);

                var btn = new Button { Text = "Wählen" };
                btn.Pressed += () => OnOptionChosen(capturedIndex);
                vbox.AddChild(btn);

                _optionsRow.AddChild(card);
            }

            _titleLabel.Text  = "Level Up!";
            _backdrop.Visible = true;
            GetTree().Paused  = true;
        }

        private void OnOptionChosen(int index)
        {
            _levelUpSystem?.SelectSkill(index);
            _backdrop.Visible = false;
            GetTree().Paused  = false;
        }
    }
}
