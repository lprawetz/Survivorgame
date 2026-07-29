using Godot;
using System.Collections.Generic;

namespace SurvivorGame
{
    /// <summary>
    /// Dynamische Coop-Kamera für 1–4 Spieler.
    /// Zentriert sich auf den Mittelpunkt aller aktiven Spieler und
    /// zoomt automatisch heraus, wenn die Spieler sich voneinander entfernen.
    ///
    /// SETUP im Godot-Editor (GameWorld.tscn):
    ///   - Diesen Node als Camera2D in die GameWorld-Szene einfügen.
    ///   - PositionSmoothingEnabled = true, PositionSmoothingSpeed = 5
    ///   - Alle Spieler müssen zur Gruppe "player" gehören.
    /// </summary>
    public partial class CoopCamera : Camera2D
    {
        [Export] public float ZoomDefault      { get; set; } = 1.0f;
        [Export] public float ZoomMin          { get; set; } = 0.4f;
        [Export] public float ZoomSpeed        { get; set; } = 2.0f;   // Zoom-Glättung
        [Export] public float CameraLerpSpeed  { get; set; } = 5.0f;   // Positions-Glättung
        [Export] public float RubberbandRadius { get; set; } = 600f;   // Tiles bis Gummiband
        [Export] public float RubberbandForce  { get; set; } = 300f;   // Kraft des Rückzugs

        // Spielerfarben: P1=Rot, P2=Blau, P3=Grün, P4=Gelb
        public static readonly Color[] PlayerColors =
        {
            new Color(1.00f, 0.27f, 0.27f), // P1 Rot
            new Color(0.27f, 0.53f, 1.00f), // P2 Blau
            new Color(0.27f, 0.80f, 0.27f), // P3 Grün
            new Color(1.00f, 0.87f, 0.00f), // P4 Gelb
        };

        private List<Node2D> _players = new();

        public override void _Ready()
        {
            PositionSmoothingEnabled = true;
            PositionSmoothingSpeed   = CameraLerpSpeed;
        }

        public override void _Process(double delta)
        {
            RefreshPlayerList();
            if (_players.Count == 0) return;

            Vector2 center    = ComputeCenter();
            float   maxRadius = ComputeMaxRadius(center);

            // Kamera auf Mittelpunkt bewegen
            GlobalPosition = GlobalPosition.Lerp(center, (float)delta * CameraLerpSpeed);

            // Zoom anpassen: je größer der Abstand, desto weiter heraus
            float targetZoom = ComputeTargetZoom(maxRadius);
            Zoom = Zoom.Lerp(new Vector2(targetZoom, targetZoom), (float)delta * ZoomSpeed);
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_players.Count == 0) return;
            Vector2 center = ComputeCenter();
            ApplyRubberband(center, (float)delta);
        }

        // ─── Hilfsmethoden ───────────────────────────────────────────────

        private void RefreshPlayerList()
        {
            _players.Clear();
            foreach (Node node in GetTree().GetNodesInGroup("player"))
            {
                if (node is Node2D p && IsInstanceValid(p))
                    _players.Add(p);
            }
        }

        private Vector2 ComputeCenter()
        {
            Vector2 sum = Vector2.Zero;
            foreach (var p in _players)
                sum += p.GlobalPosition;
            return sum / _players.Count;
        }

        private float ComputeMaxRadius(Vector2 center)
        {
            float max = 0f;
            foreach (var p in _players)
                max = Mathf.Max(max, center.DistanceTo(p.GlobalPosition));
            return max;
        }

        private float ComputeTargetZoom(float maxRadius)
        {
            // Linearer Abfall: bei maxRadius=0 → ZoomDefault, bei großem Abstand → ZoomMin
            float t = Mathf.Clamp(maxRadius / (RubberbandRadius * 1.5f), 0f, 1f);
            return Mathf.Lerp(ZoomDefault, ZoomMin, t);
        }

        /// <summary>
        /// Schiebt Spieler, die zu weit weg sind, sanft zurück zur Gruppe.
        /// </summary>
        private void ApplyRubberband(Vector2 center, float delta)
        {
            foreach (var player in _players)
            {
                float dist = player.GlobalPosition.DistanceTo(center);
                if (dist <= RubberbandRadius) continue;

                // Gummiband-Kraft proportional zur Überschreitung
                float   excess    = dist - RubberbandRadius;
                Vector2 direction = (center - player.GlobalPosition).Normalized();
                Vector2 force     = direction * excess * RubberbandForce * delta;

                // Anwenden falls der Spieler ein CharacterBody2D ist
                if (player is Player p)
                    p.GlobalPosition += force;
            }
        }
    }
}
