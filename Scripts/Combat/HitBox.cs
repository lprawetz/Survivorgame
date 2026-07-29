using Godot;

namespace SurvivorGame.Combat
{
    /// <summary>
    /// Hitbox für Angriffe. Wird der Gruppe "attack" hinzugefügt, damit HurtBox sie erkennt.
    /// Lege im Godot-Editor eine CollisionShape2D als Kind-Node an.
    /// </summary>
    public partial class HitBox : Area2D
    {
        [Export] public float Damage { get; set; } = 10f;

        private CollisionShape2D _collision;

        public override void _Ready()
        {
            _collision = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
            AddToGroup("attack");
        }

        /// <summary>
        /// Deaktiviert die Kollision für eine kurze Zeit (z. B. nach einem Treffer),
        /// damit nicht jedes Frame Schaden angerechnet wird.
        /// </summary>
        public void TemporaryDisable(float duration = 0.3f)
        {
            if (_collision == null) return;
            _collision.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
            GetTree().CreateTimer(duration).Timeout += () =>
            {
                if (IsInstanceValid(_collision))
                    _collision.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
            };
        }
    }
}
