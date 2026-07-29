using Godot;

namespace SurvivorGame.Combat
{
    /// <summary>
    /// Basisklasse für alle Gegner. Verfolgt automatisch den Spieler (Gruppe "player").
    /// Braucht eine HurtBox als Kind-Node, um Schaden zu empfangen.
    ///
    /// Ableitung: Erstelle eine neue Klasse, die EnemyBase erweitert, und überschreibe
    /// Die() oder _PhysicsProcess() für spezielles Verhalten.
    /// </summary>
    public partial class EnemyBase : CharacterBody2D
    {
        [Export] public float MaxHp            { get; set; } = 30f;
        [Export] public float MovementSpeed    { get; set; } = 50f;
        [Export] public float ContactDamage    { get; set; } = 5f;
        [Export] public int   ExperienceReward { get; set; } = 10;
        [Export] public int   GoldReward       { get; set; } = 2;

        [Signal] public delegate void EnemyDiedEventHandler(Vector2 position, int experience, int gold);

        protected float  CurrentHp;
        protected Node2D Target;
        private   HurtBox _hurtBox;

        public override void _Ready()
        {
            CurrentHp = MaxHp;
            AddToGroup("enemy");

            _hurtBox = GetNodeOrNull<HurtBox>("HurtBox");
            if (_hurtBox != null)
                _hurtBox.Hurt += OnHurt;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (Target == null)
            {
                Target = GetTree().GetFirstNodeInGroup("player") as Node2D;
                if (Target == null) return;
            }

            Vector2 direction = (Target.GlobalPosition - GlobalPosition).Normalized();
            Velocity = direction * MovementSpeed;
            MoveAndSlide();
        }

        private void OnHurt(float damage)
        {
            CurrentHp -= damage;
            if (CurrentHp <= 0f)
                Die();
        }

        protected virtual void Die()
        {
            EmitSignal(SignalName.EnemyDied, GlobalPosition, ExperienceReward, GoldReward);
            QueueFree();
        }

        /// <summary>
        /// Skaliert HP und Geschwindigkeit mit dem Schwierigkeitsgrad (aufgerufen vom Spawner).
        /// </summary>
        public void ApplyDifficultyScale(int difficultyLevel)
        {
            float scale = 1f + (difficultyLevel - 1) * 0.15f;
            MaxHp         *= scale;
            CurrentHp      = MaxHp;
            MovementSpeed *= 1f + (difficultyLevel - 1) * 0.05f;
            ExperienceReward = (int)(ExperienceReward * (1f + (difficultyLevel - 1) * 0.1f));
        }
    }
}
