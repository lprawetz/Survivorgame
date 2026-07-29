using Godot;
using SurvivorGame.Characters;

namespace SurvivorGame
{
    /// <summary>
    /// Steuert die Spielfigur. Liest Input über PlayerInputManager (Coop-kompatibel).
    /// PlayerIndex 0 = P1 (WASD), 1 = P2 (Pfeile), 2/3 = Gamepad.
    /// Die Spielfigur muss zur Gruppe "player" gehören, damit Gegner und Kamera sie finden.
    /// </summary>
    public partial class Player : CharacterBody2D
    {
        [Export] public string CharacterId  { get; set; } = "Esmeralda";
        [Export] public int    PlayerIndex  { get; set; } = 0;
        [Export] public int    VariantIndex { get; set; } = 0; // 0–3, bestimmt Tint + Variantenname

        [Signal] public delegate void HpChangedEventHandler(float current, float max);
        [Signal] public delegate void PlayerDiedEventHandler();

        private Sprite2D      _sprite;
        private CharacterData _data;
        private float         _currentHp;
        private float         _maxHp;
        private float         _moveSpeed;
        private float         _hpRegen;
        private float         _regenAccumulator;

        // Leuchtkreis-Farbe aus CoopCamera (wird beim Spawn gesetzt)
        private MeshInstance2D _indicator;

        public CharacterData Data      => _data;
        public float         CurrentHp => _currentHp;
        public float         MaxHp     => _maxHp;

        public override void _Ready()
        {
            AddToGroup("player");
            _sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
            LoadCharacterData();
            CreatePlayerIndicator();
        }

        private void LoadCharacterData()
        {
            if (CharacterDatabase.All.TryGetValue(CharacterId, out _data))
            {
                _maxHp     = _data.BaseStats.MaxHp;
                _moveSpeed = _data.BaseStats.MoveSpeed;
                _hpRegen   = _data.BaseStats.HpRegen;

                // Varianten-Tint auf den Sprite anwenden
                if (_sprite != null && VariantIndex > 0)
                    _sprite.Modulate = CharacterData.VariantTints[
                        Mathf.Clamp(VariantIndex, 0, CharacterData.VariantTints.Length - 1)];

                // Varianten-Name als Node-Name setzen (sichtbar im Debugger)
                Name = _data.GetVariantName(VariantIndex);
            }
            else
            {
                _maxHp     = 100f;
                _moveSpeed = 90f;
                _hpRegen   = 1f;
            }

            _currentHp = _maxHp;
            EmitSignal(SignalName.HpChanged, _currentHp, _maxHp);
        }

        /// <summary>Erzeugt einen farbigen Leuchtkreis unter dem Charakter (Coop-Unterscheidung).</summary>
        private void CreatePlayerIndicator()
        {
            if (PlayerIndex < 0 || PlayerIndex >= CoopCamera.PlayerColors.Length) return;

            _indicator = new MeshInstance2D
            {
                Mesh     = new SphereMesh { Radius = 12f, Height = 4f },
                Position = new Vector2(0, 8),
                ZIndex   = -1,
                Modulate = CoopCamera.PlayerColors[PlayerIndex]
            };
            AddChild(_indicator);
        }

        public override void _PhysicsProcess(double delta)
        {
            // Input über den PlayerInputManager abrufen (Coop-kompatibel)
            var inputManager = PlayerInputManager.Instance;
            Vector2 direction = inputManager != null
                ? inputManager.GetMovement(PlayerIndex)
                : Input.GetVector("left", "right", "up", "down");

            if (_sprite != null)
            {
                if (direction.X > 0)      _sprite.FlipH = false;
                else if (direction.X < 0) _sprite.FlipH = true;
            }

            Velocity = direction * _moveSpeed;
            MoveAndSlide();

            // HP-Regeneration (1x pro Sekunde)
            _regenAccumulator += (float)delta;
            if (_regenAccumulator >= 1.0f)
            {
                _regenAccumulator -= 1.0f;
                Heal(_hpRegen);
            }
        }

        public void TakeDamage(float damage)
        {
            _currentHp = Mathf.Max(0f, _currentHp - damage);
            EmitSignal(SignalName.HpChanged, _currentHp, _maxHp);
            if (_currentHp <= 0f)
                EmitSignal(SignalName.PlayerDied);
        }

        public void Heal(float amount)
        {
            _currentHp = Mathf.Min(_maxHp, _currentHp + amount);
            EmitSignal(SignalName.HpChanged, _currentHp, _maxHp);
        }

        public void ApplyStatModifier(float moveSpeedBonus = 0f, float maxHpBonus = 0f, float hpRegenBonus = 0f)
        {
            _moveSpeed += moveSpeedBonus;
            _maxHp     += maxHpBonus;
            _currentHp  = Mathf.Min(_currentHp + maxHpBonus, _maxHp);
            _hpRegen   += hpRegenBonus;
            EmitSignal(SignalName.HpChanged, _currentHp, _maxHp);
        }
    }
}


        [Signal] public delegate void HpChangedEventHandler(float current, float max);
        [Signal] public delegate void PlayerDiedEventHandler();

        private Sprite2D _sprite;
        private CharacterData _data;
        private float _currentHp;
        private float _maxHp;
        private float _moveSpeed;
        private float _hpRegen;
        private float _regenAccumulator;

        public CharacterData Data => _data;
        public float CurrentHp   => _currentHp;
        public float MaxHp       => _maxHp;

        public override void _Ready()
        {
            AddToGroup("player");
            _sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
            LoadCharacterData();
        }

        private void LoadCharacterData()
        {
            if (CharacterDatabase.All.TryGetValue(CharacterId, out _data))
            {
                _maxHp    = _data.BaseStats.MaxHp;
                _moveSpeed = _data.BaseStats.MoveSpeed;
                _hpRegen  = _data.BaseStats.HpRegen;
            }
            else
            {
                _maxHp    = 100f;
                _moveSpeed = 90f;
                _hpRegen  = 1f;
            }

            _currentHp = _maxHp;
            EmitSignal(SignalName.HpChanged, _currentHp, _maxHp);
        }

        public override void _PhysicsProcess(double delta)
        {
            // Bewegung mit den Input-Actions: left/right/up/down
            Vector2 direction = Input.GetVector("left", "right", "up", "down");

            if (_sprite != null)
            {
                if (direction.X > 0)       _sprite.FlipH = false;
                else if (direction.X < 0)  _sprite.FlipH = true;
            }

            Velocity = direction * _moveSpeed;
            MoveAndSlide();

            // HP-Regeneration
            _regenAccumulator += (float)delta;
            if (_regenAccumulator >= 1.0f)
            {
                _regenAccumulator -= 1.0f;
                Heal(_hpRegen);
            }
        }

        public void TakeDamage(float damage)
        {
            _currentHp = Mathf.Max(0f, _currentHp - damage);
            EmitSignal(SignalName.HpChanged, _currentHp, _maxHp);

            if (_currentHp <= 0f)
                EmitSignal(SignalName.PlayerDied);
        }

        public void Heal(float amount)
        {
            _currentHp = Mathf.Min(_maxHp, _currentHp + amount);
            EmitSignal(SignalName.HpChanged, _currentHp, _maxHp);
        }

        // Wird vom LevelUpSystem aufgerufen
        public void ApplyStatModifier(float moveSpeedBonus = 0f, float maxHpBonus = 0f, float hpRegenBonus = 0f)
        {
            _moveSpeed += moveSpeedBonus;
            _maxHp     += maxHpBonus;
            _currentHp  = Mathf.Min(_currentHp + maxHpBonus, _maxHp);
            _hpRegen   += hpRegenBonus;
            EmitSignal(SignalName.HpChanged, _currentHp, _maxHp);
        }
    }
}

