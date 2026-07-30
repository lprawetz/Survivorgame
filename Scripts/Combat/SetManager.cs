using Godot;
using System.Collections.Generic;
using SurvivorGame.Equipment;

namespace SurvivorGame.Combat
{
    /// <summary>
    /// Verwaltet getragene Ausrüstungsteile eines Spielers und schaltet bei vollständigem
    /// Set die aktive Fähigkeit frei. Als Kind-Node des Spielers einfügen.
    ///
    /// Die aktive Fähigkeit wird über die Aktiv-Taste des Spielers ausgelöst
    /// (PlayerInputManager.IsActiveAbilityJustPressed).
    /// </summary>
    public partial class SetManager : Node
    {
        // Getragene Teile je Set-ID: Menge der Slots (0–4)
        private readonly Dictionary<string, HashSet<EquipmentSlot>> _wornPieces = new();

        private EquipmentSetData _activeSet;   // Vollständig getragenes Set (falls vorhanden)
        private float            _cooldownLeft;
        private Player           _player;

        public bool  HasActiveSet   => _activeSet != null;
        public float CooldownLeft   => _cooldownLeft;
        public string ActiveSetName => _activeSet?.DisplayName ?? "–";

        [Signal] public delegate void SetAbilityReadyEventHandler();
        [Signal] public delegate void SetAbilityUsedEventHandler(string abilityName);

        public override void _Ready()
        {
            _player = GetParent<Player>();
        }

        public override void _Process(double delta)
        {
            if (_cooldownLeft > 0f)
            {
                _cooldownLeft -= (float)delta;
                if (_cooldownLeft <= 0f)
                    EmitSignal(SignalName.SetAbilityReady);
            }

            // Aktiv-Taste abfragen
            if (_activeSet != null && _cooldownLeft <= 0f && _player != null)
            {
                var input = PlayerInputManager.Instance;
                if (input != null && input.IsActiveAbilityJustPressed(_player.PlayerIndex))
                    UseAbility();
            }
        }

        /// <summary>Legt ein Ausrüstungsteil an und prüft auf Set-Vollständigkeit.</summary>
        public void EquipPiece(string setId, EquipmentSlot slot)
        {
            if (!_wornPieces.TryGetValue(setId, out var slots))
            {
                slots = new HashSet<EquipmentSlot>();
                _wornPieces[setId] = slots;
            }
            slots.Add(slot);
            RecalculateActiveSet();
        }

        public void UnequipPiece(string setId, EquipmentSlot slot)
        {
            if (_wornPieces.TryGetValue(setId, out var slots))
                slots.Remove(slot);
            RecalculateActiveSet();
        }

        private void RecalculateActiveSet()
        {
            _activeSet = null;
            foreach (var kv in _wornPieces)
            {
                if (kv.Value.Count >= 4 && EquipmentSetDatabase.All.TryGetValue(kv.Key, out var data))
                {
                    _activeSet    = data;
                    _cooldownLeft = 0f;
                    break;
                }
            }
        }

        private void UseAbility()
        {
            _cooldownLeft = _activeSet.Cooldown;
            EmitSignal(SignalName.SetAbilityUsed, _activeSet.DisplayName);

            switch (_activeSet.Ability)
            {
                case SetAbility.Erdspalte:          KnockbackNearbyEnemies(200f, 400f); break;
                case SetAbility.HeilendeFlut:       HealAndSlow(0.3f, 300f);            break;
                case SetAbility.Windstoss:          DashForward(400f);                  break;
                case SetAbility.Totenruf:           SummonSpirits(3);                   break;
                case SetAbility.Feuersturm:         FireStorm(350f);                    break;
                case SetAbility.GoldeneGelegenheit: GoldRush();                         break;
            }
        }

        // ─── Fähigkeits-Effekte (vereinfachte Erstimplementierung) ────────

        private void KnockbackNearbyEnemies(float radius, float force)
        {
            foreach (Node node in GetTree().GetNodesInGroup("enemy"))
            {
                if (node is not Node2D enemy) continue;
                Vector2 offset = enemy.GlobalPosition - _player.GlobalPosition;
                if (offset.Length() <= radius)
                    enemy.GlobalPosition += offset.Normalized() * (force * 0.1f);
            }
        }

        private void HealAndSlow(float healPct, float radius)
        {
            _player.Heal(_player.MaxHp * healPct);
            // Verlangsamung: hier vereinfacht als Rückstoß-freie Markierung (Erweiterung folgt)
        }

        private void DashForward(float distance)
        {
            var input = PlayerInputManager.Instance;
            Vector2 dir = input != null ? input.GetMovement(_player.PlayerIndex) : Vector2.Right;
            if (dir == Vector2.Zero) dir = Vector2.Right;
            _player.GlobalPosition += dir.Normalized() * distance;
        }

        private void SummonSpirits(int count)
        {
            // Platzhalter: echte Geist-Szene folgt. Sofortiger Flächenschaden als Ersatz.
            KnockbackNearbyEnemies(250f, 200f);
        }

        private void FireStorm(float radius)
        {
            // Platzhalter-Flächenschaden bis eine echte Feuersturm-Szene existiert.
            KnockbackNearbyEnemies(radius, 100f);
        }

        private void GoldRush()
        {
            var xp = GetTree().GetFirstNodeInGroup("experience_system") as ExperienceSystem;
            xp?.AddGold(50); // Sofortbonus als Erstimplementierung
        }
    }
}
