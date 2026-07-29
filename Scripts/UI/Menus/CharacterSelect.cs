using Godot;
using System.Collections.Generic;
using SurvivorGame.Characters;
using SurvivorGame.Core;

namespace SurvivorGame.UI.Menus
{
    /// <summary>
    /// Coop-fähiges Charakterauswahlmenü (1–4 Spieler, sequenziell).
    ///
    /// Ablauf:
    ///   1. P1 wählt einen freigeschalteten Charakter → bestätigt mit Start-Button.
    ///   2. Falls weitere Spieler aktiv sind (PlayerInputManager), wählt P2, usw.
    ///   3. Sobald alle aktiven Spieler gewählt haben → Szene wechselt.
    ///
    /// Wird derselbe Charakter mehrfach gewählt, erhält jede weitere Instanz
    /// automatisch die nächste Variante (Tint + Variantenname).
    ///
    /// SZENEN-STRUKTUR (CharacterSelect.tscn):
    ///   Control
    ///     Label               (Name: "PlayerHint")       – "P1 wählt..."
    ///     GridContainer       (Name: "CharacterGrid")
    ///     Panel               (Name: "InfoPanel")
    ///       VBoxContainer
    ///         Label           (Name: "CharacterName")
    ///         Label           (Name: "CharacterRole")
    ///         Label           (Name: "CharacterElement")
    ///         Label           (Name: "CharacterDescription")
    ///         GridContainer   (Name: "StatsGrid")
    ///     HBoxContainer       (Name: "SelectionBar")     – zeigt P1–P4 Auswahlen
    ///     Button              (Name: "StartButton")
    ///     Button              (Name: "BackButton")
    /// </summary>
    public partial class CharacterSelect : Control
    {
        private const string SilhouetteTexture = "res://Assets/UI/character_silhouette.png";

        private Label         _playerHint;
        private GridContainer _characterGrid;
        private Panel         _infoPanel;
        private Label         _nameLabel;
        private Label         _roleLabel;
        private Label         _elementLabel;
        private Label         _descriptionLabel;
        private GridContainer _statsGrid;
        private HBoxContainer _selectionBar;
        private Button        _startButton;
        private Button        _backButton;

        // Auswahl je Spieler-Slot (null = noch nicht gewählt)
        private readonly PlayerSelection?[] _selections = new PlayerSelection?[PlayerInputManager.MaxPlayers];

        // Aktuell wählender Spieler
        private int  _currentPickingPlayer = 0;
        private int  _totalActivePlayers   = 1;

        // Wie oft ist ein bestimmter Charakter schon gewählt? → bestimmt den VariantIndex
        private readonly Dictionary<string, int> _pickCount = new();

        public override void _Ready()
        {
            _playerHint       = GetNode<Label>("PlayerHint");
            _characterGrid    = GetNode<GridContainer>("CharacterGrid");
            _infoPanel        = GetNode<Panel>("InfoPanel");
            _nameLabel        = _infoPanel.GetNode<Label>("VBoxContainer/CharacterName");
            _roleLabel        = _infoPanel.GetNode<Label>("VBoxContainer/CharacterRole");
            _elementLabel     = _infoPanel.GetNode<Label>("VBoxContainer/CharacterElement");
            _descriptionLabel = _infoPanel.GetNode<Label>("VBoxContainer/CharacterDescription");
            _statsGrid        = _infoPanel.GetNode<GridContainer>("VBoxContainer/StatsGrid");
            _selectionBar     = GetNode<HBoxContainer>("SelectionBar");
            _startButton      = GetNode<Button>("StartButton");
            _backButton       = GetNode<Button>("BackButton");

            _startButton.Pressed += OnConfirmPressed;
            _backButton.Pressed  += OnBackPressed;

            // Anzahl aktiver Spieler vom InputManager lesen
            if (PlayerInputManager.Instance != null)
                _totalActivePlayers = PlayerInputManager.Instance.ActivePlayerCount;

            BuildCharacterGrid();
            UpdateInfoPanel(GetFirstUnlocked());
            UpdatePlayerHint();
            UpdateSelectionBar();
        }

        // ─── Charaktergitter ─────────────────────────────────────────────

        private void BuildCharacterGrid()
        {
            foreach (Node child in _characterGrid.GetChildren())
                child.QueueFree();

            var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");

            foreach (var kvp in CharacterDatabase.All)
            {
                string id        = kvp.Key;
                var    data      = kvp.Value;
                bool   unlocked  = data.IsStartCharacter ||
                                   (saveSystem.UnlockedCharacters.TryGetValue(id, out bool v) && v);

                var btn = new TextureButton();
                if (ResourceLoader.Exists(data.PortraitPath))
                    btn.TextureNormal = GD.Load<Texture2D>(data.PortraitPath);

                if (!unlocked)
                {
                    if (ResourceLoader.Exists(SilhouetteTexture))
                        btn.TextureNormal = GD.Load<Texture2D>(SilhouetteTexture);
                    btn.Modulate = new Color(0.4f, 0.4f, 0.4f);
                    btn.Disabled  = true;
                }
                else
                {
                    string capturedId = id;
                    btn.Pressed += () => OnCharacterSelected(capturedId);
                }

                _characterGrid.AddChild(btn);
            }
        }

        // ─── Auswahl-Logik ────────────────────────────────────────────────

        private void OnCharacterSelected(string id)
        {
            UpdateInfoPanel(id);
            // Info anzeigen; erst bei Confirm-Button bestätigen
            _startButton.Disabled = false;
            _startButton.SetMeta("pending_id", id);
        }

        private void OnConfirmPressed()
        {
            if (!_startButton.HasMeta("pending_id")) return;
            string id = _startButton.GetMeta("pending_id").AsString();

            // Variant-Index bestimmen (wie oft wurde dieser Char schon gewählt?)
            _pickCount.TryGetValue(id, out int count);
            _selections[_currentPickingPlayer] = new PlayerSelection(id, count % CharacterData.VariantTints.Length);
            _pickCount[id] = count + 1;

            _startButton.Disabled = true;
            _startButton.RemoveMeta("pending_id");

            _currentPickingPlayer++;
            UpdateSelectionBar();

            // Alle aktiven Spieler haben gewählt → Spiel starten
            if (_currentPickingPlayer >= _totalActivePlayers)
            {
                StartGame();
                return;
            }

            UpdatePlayerHint();
        }

        private void OnBackPressed()
        {
            if (_currentPickingPlayer > 0)
            {
                // Letzte Auswahl rückgängig machen
                _currentPickingPlayer--;
                string undoneId = _selections[_currentPickingPlayer]!.Value.CharacterId;
                _selections[_currentPickingPlayer] = null;
                if (_pickCount.ContainsKey(undoneId))
                    _pickCount[undoneId] = Mathf.Max(0, _pickCount[undoneId] - 1);

                UpdatePlayerHint();
                UpdateSelectionBar();
            }
            else
            {
                GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
            }
        }

        // ─── Game starten ─────────────────────────────────────────────────

        private void StartGame()
        {
            // Auswahlen an GameState übergeben
            var gs = GetNode<SurvivorGame.Core.GameState>("/root/GameState");
            if (gs != null)
            {
                var list = new System.Collections.Generic.List<PlayerSelection>();
                for (int i = 0; i < _totalActivePlayers; i++)
                    if (_selections[i].HasValue)
                        list.Add(_selections[i]!.Value);
                gs.SetSelections(list);
            }

            GetTree().ChangeSceneToFile("res://Scenes/World/GameWorld.tscn");
        }

        // ─── UI-Updates ───────────────────────────────────────────────────

        private void UpdatePlayerHint()
        {
            Color c = CoopCamera.PlayerColors[_currentPickingPlayer];
            _playerHint.AddThemeColorOverride("font_color", c);
            _playerHint.Text = $"Spieler {_currentPickingPlayer + 1} wählt einen Charakter ...";
        }

        private void UpdateSelectionBar()
        {
            foreach (Node child in _selectionBar.GetChildren())
                child.QueueFree();

            for (int i = 0; i < _totalActivePlayers; i++)
            {
                var panel = new PanelContainer();
                var label = new Label();

                if (_selections[i].HasValue)
                {
                    var sel  = _selections[i]!.Value;
                    var data = CharacterDatabase.All[sel.CharacterId];
                    label.Text    = $"P{i + 1}\n{data.GetVariantName(sel.VariantIndex)}";
                    label.Modulate = CharacterData.VariantTints[sel.VariantIndex];
                }
                else
                {
                    label.Text    = $"P{i + 1}\n–";
                    label.Modulate = new Color(0.5f, 0.5f, 0.5f);
                }

                label.HorizontalAlignment = HorizontalAlignment.Center;
                panel.AddChild(label);
                panel.AddThemeColorOverride("font_color", CoopCamera.PlayerColors[i]);
                _selectionBar.AddChild(panel);
            }
        }

        private void UpdateInfoPanel(string id)
        {
            if (id == null || !CharacterDatabase.All.TryGetValue(id, out var data)) return;

            _nameLabel.Text        = data.DisplayName;
            _roleLabel.Text        = $"Rolle: {data.Role}";
            _elementLabel.Text     = $"Element: {data.Element}";
            _descriptionLabel.Text = data.Description;
            BuildStatsGrid(data.BaseStats);
        }

        private void BuildStatsGrid(CharacterStats stats)
        {
            foreach (Node child in _statsGrid.GetChildren())
                child.QueueFree();

            AddStatRow("Intelligenz",   stats.Intelligence);
            AddStatRow("Stärke",        stats.Strength);
            AddStatRow("Beweglichkeit", stats.Agility);
            AddStatRow("Konstitution",  stats.Constitution);
            AddStatRow("Willenskraft",  stats.Willpower);
            AddStatRow("Ausdauer",      stats.Endurance);
        }

        private void AddStatRow(string name, int value)
        {
            _statsGrid.AddChild(new Label { Text = name });
            _statsGrid.AddChild(new Label { Text = value.ToString() });
        }

        private static string GetFirstUnlocked()
        {
            foreach (var kvp in CharacterDatabase.All)
                if (kvp.Value.IsStartCharacter) return kvp.Key;
            return "Esmeralda";
        }
    }
}

    /// Rechts wird ein Infopanel mit Name, Rolle, Element und Attributen angezeigt.
    ///
    /// SZENEN-STRUKTUR (CharacterSelect.tscn):
    ///   Control
    ///     GridContainer       (Name: "CharacterGrid")
    ///     Panel               (Name: "InfoPanel")
    ///       VBoxContainer
    ///         Label           (Name: "CharacterName")
    ///         Label           (Name: "CharacterRole")
    ///         Label           (Name: "CharacterElement")
    ///         Label           (Name: "CharacterDescription")
    ///         GridContainer   (Name: "StatsGrid")
    ///     Button              (Name: "StartButton")
    ///     Button              (Name: "BackButton")
    /// </summary>
    public partial class CharacterSelect : Control
    {
        private const string SilhouetteTexture = "res://Assets/UI/character_silhouette.png";

        private GridContainer _characterGrid;
        private Panel         _infoPanel;
        private Label         _nameLabel;
        private Label         _roleLabel;
        private Label         _elementLabel;
        private Label         _descriptionLabel;
        private GridContainer _statsGrid;
        private Button        _startButton;
        private Button        _backButton;

        private string _selectedCharacterId = "Esmeralda";

        public override void _Ready()
        {
            _characterGrid    = GetNode<GridContainer>("CharacterGrid");
            _infoPanel        = GetNode<Panel>("InfoPanel");
            _nameLabel        = _infoPanel.GetNode<Label>("VBoxContainer/CharacterName");
            _roleLabel        = _infoPanel.GetNode<Label>("VBoxContainer/CharacterRole");
            _elementLabel     = _infoPanel.GetNode<Label>("VBoxContainer/CharacterElement");
            _descriptionLabel = _infoPanel.GetNode<Label>("VBoxContainer/CharacterDescription");
            _statsGrid        = _infoPanel.GetNode<GridContainer>("VBoxContainer/StatsGrid");
            _startButton      = GetNode<Button>("StartButton");
            _backButton       = GetNode<Button>("BackButton");

            _startButton.Pressed += OnStartPressed;
            _backButton.Pressed  += OnBackPressed;

            BuildCharacterGrid();
            UpdateInfoPanel("Esmeralda");
        }

        private void BuildCharacterGrid()
        {
            var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");

            foreach (var kvp in CharacterDatabase.All)
            {
                string id   = kvp.Key;
                var    data = kvp.Value;

                bool isUnlocked = data.IsStartCharacter ||
                                  (saveSystem.UnlockedCharacters.TryGetValue(id, out bool val) && val);

                var button = new TextureButton();
                string texturePath = isUnlocked ? data.PortraitPath : SilhouetteTexture;

                // Portrait laden; bei fehlendem Asset stumm scheitern
                if (ResourceLoader.Exists(texturePath))
                    button.TextureNormal = GD.Load<Texture2D>(texturePath);

                if (!isUnlocked)
                    button.Modulate = new Color(0.4f, 0.4f, 0.4f);

                // Capture für Lambda
                string capturedId = id;
                button.Pressed += () => OnCharacterSelected(capturedId, isUnlocked);
                _characterGrid.AddChild(button);
            }
        }

        private void OnCharacterSelected(string id, bool isUnlocked)
        {
            UpdateInfoPanel(id);
            _selectedCharacterId = id;
            _startButton.Disabled = !isUnlocked;
        }

        private void UpdateInfoPanel(string id)
        {
            if (!CharacterDatabase.All.TryGetValue(id, out var data)) return;

            _nameLabel.Text        = data.DisplayName;
            _roleLabel.Text        = $"Rolle: {data.Role}";
            _elementLabel.Text     = $"Element: {data.Element}";
            _descriptionLabel.Text = data.Description;

            BuildStatsGrid(data.BaseStats);
        }

        private void BuildStatsGrid(CharacterStats stats)
        {
            foreach (Node child in _statsGrid.GetChildren())
                child.QueueFree();

            AddStatRow("Intelligenz",      stats.Intelligence);
            AddStatRow("Stärke",           stats.Strength);
            AddStatRow("Beweglichkeit",    stats.Agility);
            AddStatRow("Konstitution",     stats.Constitution);
            AddStatRow("Willenskraft",     stats.Willpower);
            AddStatRow("Ausdauer",         stats.Endurance);
        }

        private void AddStatRow(string statName, int value)
        {
            _statsGrid.AddChild(new Label { Text = statName });
            _statsGrid.AddChild(new Label { Text = value.ToString() });
        }

        private void OnStartPressed()
        {
            // Gewählten Charakter im autoload speichern, damit die Spielwelt ihn laden kann
            if (Engine.HasSingleton("GameState"))
            {
                var gameState = Engine.GetSingleton("GameState");
                gameState.Set("SelectedCharacterId", _selectedCharacterId);
            }

            GetTree().ChangeSceneToFile("res://Scenes/World/GameWorld.tscn");
        }

        private void OnBackPressed()
        {
            GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
        }
    }
}
