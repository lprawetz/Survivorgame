using Godot;

namespace SurvivorGame.Combat
{
	public enum HurtBoxType
	{
		Cooldown,  // Kurze Unverwundbarkeit nach Treffer
		HitOnce    // Deaktiviert die angreifende HitBox nach Treffer
	}

	/// <summary>
	/// HurtBox empfängt Schaden von HitBoxen aus der Gruppe "attack".
	/// Gibt das Signal Hurt(damage) aus, das die besitzende Einheit abhört.
	/// Lege im Godot-Editor eine CollisionShape2D als Kind-Node an.
	/// </summary>
	public partial class HurtBox : Area2D
	{
		[Export] public HurtBoxType Type                  { get; set; } = HurtBoxType.Cooldown;
		[Export] public float       InvincibilityDuration { get; set; } = 0.5f;

		[Signal] public delegate void HurtEventHandler(float damage);

		private CollisionShape2D _collision;
		private bool _isInvincible;

		public override void _Ready()
		{
			_collision = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
			AreaEntered += OnAreaEntered;
		}

		private void OnAreaEntered(Area2D area)
		{
			if (_isInvincible) return;
			if (!area.IsInGroup("attack")) return;

			// Damage über Variant abrufen (kompatibel mit C#-Export-Properties)
			Variant damageVariant = area.Get(HitBox.PropertyName.Damage);
			float damage = damageVariant.AsSingle();

			EmitSignal(SignalName.Hurt, damage);

			if (Type == HurtBoxType.Cooldown)
				StartInvincibility();
			else if (Type == HurtBoxType.HitOnce && area is HitBox hitBox)
				hitBox.TemporaryDisable();
		}

		private void StartInvincibility()
		{
			_isInvincible = true;
			GetTree().CreateTimer(InvincibilityDuration).Timeout += () =>
			{
				_isInvincible = false;
			};
		}
	}
}
