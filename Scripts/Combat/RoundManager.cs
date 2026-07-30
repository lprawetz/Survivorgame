using Godot;

namespace SurvivorGame.Combat
{
    /// <summary>
    /// Überwacht den Rundenstatus: erkennt wenn alle Spieler tot sind (Game Over)
    /// und zeigt den GameOver-Screen. Als Node in GameWorld.tscn einfügen.
    ///
    /// Setup: GameOverScreen als Kind (CanvasLayer) oder über Export zuweisen.
    /// </summary>
    public partial class RoundManager : Node
    {
        [Signal] public delegate void AllPlayersDeadEventHandler();

        private bool  _gameOverTriggered;
        private float _checkCooldown;

        public override void _Ready() => AddToGroup("round_manager");

        public override void _Process(double delta)
        {
            if (_gameOverTriggered) return;

            // Nicht jeden Frame prüfen
            _checkCooldown -= (float)delta;
            if (_checkCooldown > 0f) return;
            _checkCooldown = 0.5f;

            if (AllPlayersDead())
            {
                _gameOverTriggered = true;
                EmitSignal(SignalName.AllPlayersDead);
            }
        }

        private bool AllPlayersDead()
        {
            var players = GetTree().GetNodesInGroup("player");
            if (players.Count == 0) return false;

            foreach (Node node in players)
                if (node is Player p && !p.IsDead)
                    return false; // Mindestens einer lebt noch

            return true;
        }
    }
}
