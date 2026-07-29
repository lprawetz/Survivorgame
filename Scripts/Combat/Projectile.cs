using Godot;

namespace SurvivorGame.Combat
{
    /// <summary>
    /// Projektil für automatische Angriffe. Bewegt sich in einer Richtung, trägt Schaden
    /// und löscht sich bei Treffer oder nach Erreichen der Maximalreichweite.
    ///
    /// Wird der Gruppe "attack" hinzugefügt – HurtBox erkennt es dadurch automatisch.
    /// Das Damage-Property wird von HurtBox per Reflection ausgelesen.
    ///
    /// SZENEN-AUFBAU (Projectile.tscn):
    ///   Area2D  [Script: Projectile.cs]
    ///     CollisionShape2D  (CircleShape2D, Radius 6)
    ///
    /// Die visuelle Darstellung wird prozedural erstellt (Polygon2D).
    /// Ersetze sie sobald Sprite-Assets vorliegen durch ein Sprite2D.
    /// </summary>
    public partial class Projectile : Area2D
    {
        [Export] public float Speed      { get; set; } = 320f;
        [Export] public float Damage     { get; set; } = 12f;
        [Export] public float MaxRange   { get; set; } = 500f;
        [Export] public Color TintColor  { get; set; } = new Color(1f, 0.5f, 0.1f); // Orange-Feuer

        private Vector2 _direction;
        private float   _distanceTraveled;

        public override void _Ready()
        {
            AddToGroup("attack");
            AreaEntered += OnAreaEntered;
            CreateVisual();
        }

        /// <summary>Muss direkt nach Instantiierung aufgerufen werden.</summary>
        public void Initialize(Vector2 direction, float damage, Color tint = default)
        {
            _direction = direction.Normalized();
            Damage     = damage;
            if (tint != default) TintColor = tint;
        }

        public override void _PhysicsProcess(double delta)
        {
            var motion = _direction * Speed * (float)delta;
            Position          += motion;
            _distanceTraveled += motion.Length();

            if (_distanceTraveled >= MaxRange)
                QueueFree();
        }

        private void OnAreaEntered(Area2D area)
        {
            // HurtBox der Gegner empfängt den Schaden automatisch über ihre eigene Logik.
            // Das Projektil löscht sich nach dem ersten Treffer.
            if (area.IsInGroup("enemy") || area.GetParent()?.IsInGroup("enemy") == true)
                QueueFree();
        }

        /// <summary>Erstellt einen einfachen Leuchtpunkt als Platzhalter-Visual.</summary>
        private void CreateVisual()
        {
            // Kleiner farbiger Diamant aus 4 Punkten
            var poly = new Polygon2D
            {
                Polygon = new[]
                {
                    new Vector2( 0, -7),
                    new Vector2( 4,  0),
                    new Vector2( 0,  7),
                    new Vector2(-4,  0),
                },
                Color = TintColor
            };
            AddChild(poly);

            // Kleiner weißer Kern für Glanz-Effekt
            var core = new Polygon2D
            {
                Polygon = new[]
                {
                    new Vector2( 0, -3),
                    new Vector2( 2,  0),
                    new Vector2( 0,  3),
                    new Vector2(-2,  0),
                },
                Color = new Color(1f, 1f, 1f, 0.8f)
            };
            AddChild(core);
        }
    }
}
