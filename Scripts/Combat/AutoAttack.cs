using Godot;
using SurvivorGame.Characters;

namespace SurvivorGame.Combat
{
    /// <summary>
    /// Automatisches Angriffssystem. Als Kind-Node des Spielers einfügen.
    /// Sucht periodisch den nächsten Gegner und spawnt ein Projektil in seine Richtung.
    ///
    /// SETUP im Editor:
    ///   - ProjectileScene: res://Scenes/World/Projectile.tscn zuweisen
    ///   - Node ist Kind von Player (CharacterBody2D)
    ///
    /// Wird von Player über ApplyStatModifier mit neuen Werten versorgt.
    /// Das Element des Spielers bestimmt die Projektilfarbe.
    /// </summary>
    public partial class AutoAttack : Node
    {
        [Export] public PackedScene ProjectileScene  { get; set; }
        [Export] public float       BaseInterval     { get; set; } = 1.2f; // Sekunden zwischen Angriffen
        [Export] public float       BaseDamage       { get; set; } = 12f;
        [Export] public float       AttackRange      { get; set; } = 420f;

        // Multiplikatoren – werden vom UpgradeApplier verändert
        public float DamageMultiplier      { get; set; } = 1.0f;
        public float AttackSpeedMultiplier { get; set; } = 1.0f;
        public float CritChance            { get; set; } = 0.0f;
        public float AreaMultiplier        { get; set; } = 1.0f;
        public int   ExtraProjectiles      { get; set; } = 0;

        private const float CritDamageFactor = 2.0f;
        private readonly RandomNumberGenerator _rng = new();

        private Timer   _timer;
        private Node2D  _owner2D;
        private Color   _projectileColor = new Color(1f, 0.5f, 0.1f);

        public override void _Ready()
        {
            _owner2D = GetParent<Node2D>();
            _rng.Randomize();

            // Projektilfarbe aus Charakter-Element ableiten
            if (_owner2D is Player player && player.Data != null)
                _projectileColor = ElementToColor(player.Data.Element);

            _timer = new Timer { WaitTime = BaseInterval, Autostart = true };
            _timer.Timeout += TryAttack;
            AddChild(_timer);
        }

        private void TryAttack()
        {
            if (ProjectileScene == null) return;

            var target = FindNearestEnemy();
            if (target == null) return;

            Vector2 baseDir = (target.GlobalPosition - _owner2D.GlobalPosition).Normalized();

            // Hauptprojektil + zusätzliche Projektile leicht fächerförmig
            int total = 1 + ExtraProjectiles;
            for (int i = 0; i < total; i++)
            {
                float angleOffset = total > 1 ? Mathf.DegToRad((i - (total - 1) / 2f) * 12f) : 0f;
                SpawnProjectile(baseDir.Rotated(angleOffset));
            }
        }

        private Node2D FindNearestEnemy()
        {
            Node2D nearest  = null;
            float  minDist  = AttackRange;

            foreach (Node node in GetTree().GetNodesInGroup("enemy"))
            {
                if (node is not Node2D enemy || !IsInstanceValid(enemy)) continue;
                float dist = _owner2D.GlobalPosition.DistanceTo(enemy.GlobalPosition);
                if (dist < minDist) { minDist = dist; nearest = enemy; }
            }
            return nearest;
        }

        private void SpawnProjectile(Vector2 direction)
        {
            var proj = ProjectileScene.Instantiate<Projectile>();
            proj.GlobalPosition = _owner2D.GlobalPosition;

            float damage = BaseDamage * DamageMultiplier;
            if (_rng.Randf() < CritChance) damage *= CritDamageFactor;

            proj.Initialize(direction, damage, _projectileColor);
            proj.Scale = new Vector2(AreaMultiplier, AreaMultiplier);
            GetTree().CurrentScene.AddChild(proj);
        }

        /// <summary>Aktualisiert Angriffsintervall nach Attributänderung.</summary>
        public void ApplyAttackSpeed(float multiplier)
        {
            AttackSpeedMultiplier    = multiplier;
            _timer.WaitTime          = BaseInterval / Mathf.Max(0.1f, AttackSpeedMultiplier);
        }

        public void ApplyDamageBonus(float multiplier) => DamageMultiplier = multiplier;

        private static Color ElementToColor(CharacterElement element) => element switch
        {
            CharacterElement.Fire  => new Color(1.0f, 0.40f, 0.10f),
            CharacterElement.Earth => new Color(0.6f, 0.45f, 0.20f),
            CharacterElement.Water => new Color(0.2f, 0.70f, 1.00f),
            CharacterElement.Wind  => new Color(0.8f, 0.95f, 1.00f),
            CharacterElement.Death => new Color(0.6f, 0.20f, 0.90f),
            _                      => Colors.White
        };
    }
}
