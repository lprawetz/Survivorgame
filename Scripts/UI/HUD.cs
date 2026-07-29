using Godot;
using System.Collections.Generic;

namespace SurvivorGame.UI
{
    /// <summary>
    /// In-Game HUD für 1–4 Spieler.
    /// Zeigt je Spieler einen HP-Balken, einen gemeinsamen XP-Balken und Gold.
    ///
    /// Verbindet sich automatisch mit allen Nodes der Gruppe "player" sowie
    /// dem ersten ExperienceSystem der Szene.
    ///
    /// SZENEN-AUFBAU (HUD.tscn):
    ///   CanvasLayer
    ///     Control  [Script: HUD.cs]
    ///       VBoxContainer  "PlayerBars"   – obere linke Ecke
    ///       HBoxContainer  "BottomBar"    – unten
    /// </summary>
    public partial class HUD : Control
    {
        private VBoxContainer _playerBars;
        private ProgressBar   _xpBar;
        private Label         _levelLabel;
        private Label         _goldLabel;
        private Label         _timerLabel;

        // HP-Balken je Spieler-Index
        private readonly Dictionary<int, ProgressBar> _hpBars = new();

        private float _elapsedSeconds;

        public override void _Ready()
        {
            _playerBars = GetNode<VBoxContainer>("PlayerBars");
            BuildBottomBar();
            CallDeferred(MethodName.ConnectToGameNodes);
        }

        public override void _Process(double delta)
        {
            _elapsedSeconds += (float)delta;
            if (_timerLabel != null)
            {
                int min = (int)_elapsedSeconds / 60;
                int sec = (int)_elapsedSeconds % 60;
                _timerLabel.Text = $"{min:00}:{sec:00}";
            }
        }

        // ─── Aufbau ──────────────────────────────────────────────────────

        private void BuildBottomBar()
        {
            var bottom = new HBoxContainer
            {
                AnchorBottom = 1f,
                AnchorTop    = 1f,
                AnchorRight  = 1f,
                OffsetTop    = -50f,
                GrowVertical = Control.GrowDirection.Begin,
            };
            AddChild(bottom);

            // XP-Bereich
            var xpBox = new VBoxContainer();
            xpBox.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            bottom.AddChild(xpBox);

            _levelLabel = new Label { Text = "Lv. 1" };
            xpBox.AddChild(_levelLabel);

            _xpBar = new ProgressBar { Value = 0, MaxValue = 100, ShowPercentage = false };
            _xpBar.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            xpBox.AddChild(_xpBar);

            // Timer (Mitte)
            _timerLabel = new Label
            {
                Text                    = "00:00",
                HorizontalAlignment     = HorizontalAlignment.Center,
                SizeFlagsHorizontal     = SizeFlags.ExpandFill,
            };
            _timerLabel.AddThemeFontSizeOverride("font_size", 20);
            bottom.AddChild(_timerLabel);

            // Gold (rechts)
            _goldLabel = new Label
            {
                Text                = "Gold: 0",
                HorizontalAlignment = HorizontalAlignment.Right,
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
            };
            bottom.AddChild(_goldLabel);
        }

        /// <summary>Verbindet sich mit Spielern und ExperienceSystem in der aktuellen Szene.</summary>
        private void ConnectToGameNodes()
        {
            // Spieler-HP-Balken
            int index = 0;
            foreach (Node node in GetTree().GetNodesInGroup("player"))
            {
                if (node is Player player)
                {
                    AddPlayerBar(player, index);
                    player.HpChanged += (cur, max) => OnHpChanged(index, cur, max);
                    index++;
                }
            }

            // XP & Gold
            var xpSystem = GetTree().GetFirstNodeInGroup("experience_system") as Combat.ExperienceSystem;
            if (xpSystem != null)
            {
                xpSystem.ExperienceChanged += OnXpChanged;
                xpSystem.GoldChanged       += OnGoldChanged;
                xpSystem.LevelUp           += OnLevelUp;
            }
        }

        private void AddPlayerBar(Player player, int playerIndex)
        {
            Color playerColor = playerIndex < CoopCamera.PlayerColors.Length
                ? CoopCamera.PlayerColors[playerIndex]
                : Colors.White;

            var row = new HBoxContainer();
            _playerBars.AddChild(row);

            var nameLabel = new Label
            {
                Text    = $"P{playerIndex + 1}",
                Modulate = playerColor,
            };
            nameLabel.AddThemeFontSizeOverride("font_size", 13);
            row.AddChild(nameLabel);

            var bar = new ProgressBar
            {
                MinValue         = 0,
                MaxValue         = player.MaxHp,
                Value            = player.CurrentHp,
                ShowPercentage   = false,
                CustomMinimumSize = new Vector2(180, 16),
            };
            bar.AddThemeColorOverride("font_color", playerColor);
            row.AddChild(bar);

            var hpLabel = new Label { Text = $"{(int)player.CurrentHp}/{(int)player.MaxHp}" };
            hpLabel.AddThemeFontSizeOverride("font_size", 11);
            row.AddChild(hpLabel);

            _hpBars[playerIndex] = bar;
        }

        // ─── Signal-Handler ───────────────────────────────────────────────

        private void OnHpChanged(int playerIndex, float current, float max)
        {
            if (!_hpBars.TryGetValue(playerIndex, out var bar)) return;
            bar.MaxValue = max;
            bar.Value    = current;

            // HP-Label aktualisieren
            if (bar.GetParent() is HBoxContainer row && row.GetChildCount() > 2)
            {
                if (row.GetChild(2) is Label lbl)
                    lbl.Text = $"{(int)current}/{(int)max}";
            }
        }

        private void OnXpChanged(int current, int required)
        {
            _xpBar.MaxValue = required;
            _xpBar.Value    = current;
        }

        private void OnLevelUp(int newLevel)
        {
            _levelLabel.Text = $"Lv. {newLevel}";
        }

        private void OnGoldChanged(int gold)
        {
            _goldLabel.Text = $"Gold: {gold}";
        }
    }
}
