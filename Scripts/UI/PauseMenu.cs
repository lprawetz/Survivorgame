using Godot;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Pause-Menü. Öffnet/schließt mit ESC und pausiert das Spiel.
    /// Als CanvasLayer in GameWorld.tscn einfügen.
    ///
    /// SZENEN-AUFBAU (PauseMenu.tscn):
    ///   CanvasLayer  [Script]
    ///     Panel "Backdrop"
    ///       VBoxContainer
    ///         Label  "Title"
    ///         Button "ResumeButton"
    ///         Button "MenuButton"
    /// </summary>
    public partial class PauseMenu : CanvasLayer
    {
        private Panel _backdrop;

        public override void _Ready()
        {
            _backdrop = GetNode<Panel>("Backdrop");
            _backdrop.Visible = false;

            GetNode<Button>("Backdrop/VBoxContainer/ResumeButton").Pressed += Resume;
            GetNode<Button>("Backdrop/VBoxContainer/MenuButton").Pressed   += OnMenu;

            ProcessMode = Node.ProcessModeEnum.Always; // reagiert auch bei Pause
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel"))
            {
                if (_backdrop.Visible) Resume();
                else                   Pause();
                GetViewport().SetInputAsHandled();
            }
        }

        private void Pause()
        {
            _backdrop.Visible = true;
            GetTree().Paused  = true;
        }

        private void Resume()
        {
            _backdrop.Visible = false;
            GetTree().Paused  = false;
        }

        private void OnMenu()
        {
            GetTree().Paused = false;
            GetTree().ChangeSceneToFile("res://Scenes/UI/MainMenu.tscn");
        }
    }
}
