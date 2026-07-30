using Godot;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Game-Over-Bildschirm. Verbindet sich mit dem RoundManager (Gruppe "round_manager")
    /// und erscheint wenn alle Spieler tot sind.
    ///
    /// SZENEN-AUFBAU (GameOverScreen.tscn):
    ///   CanvasLayer  [Script]
    ///     Panel "Backdrop"
    ///       VBoxContainer
    ///         Label  "Title"
    ///         Button "RetryButton"
    ///         Button "MenuButton"
    /// </summary>
    public partial class GameOverScreen : CanvasLayer
    {
        private Panel _backdrop;

        public override void _Ready()
        {
            _backdrop = GetNode<Panel>("Backdrop");
            _backdrop.Visible = false;

            GetNode<Button>("Backdrop/VBoxContainer/RetryButton").Pressed += OnRetry;
            GetNode<Button>("Backdrop/VBoxContainer/MenuButton").Pressed  += OnMenu;

            CallDeferred(MethodName.ConnectRoundManager);
        }

        private void ConnectRoundManager()
        {
            var rm = GetTree().GetFirstNodeInGroup("round_manager") as Combat.RoundManager;
            if (rm != null)
                rm.AllPlayersDead += Show;
        }

        private void Show()
        {
            _backdrop.Visible = true;
            GetTree().Paused  = true;
        }

        private void OnRetry()
        {
            GetTree().Paused = false;
            GetTree().ReloadCurrentScene();
        }

        private void OnMenu()
        {
            GetTree().Paused = false;
            GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
        }
    }
}
