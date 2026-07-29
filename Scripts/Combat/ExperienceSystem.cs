using Godot;

namespace SurvivorGame.Combat
{
    /// <summary>
    /// Verwaltet Erfahrungspunkte, Level und Gold des Spielers.
    /// Verbinde EnemySpawner.DropCollected -> ExperienceSystem.OnDropCollected.
    /// Verbinde LevelUp -> LevelUpSystem.OnPlayerLevelUp.
    /// </summary>
    public partial class ExperienceSystem : Node
    {
        [Signal] public delegate void LevelUpEventHandler(int newLevel);
        [Signal] public delegate void ExperienceChangedEventHandler(int current, int required);
        [Signal] public delegate void GoldChangedEventHandler(int gold);

        public override void _Ready() => AddToGroup("experience_system");
        private int _currentXp  = 0;
        private int _gold        = 0;

        public int Level => _level;
        public int Gold  => _gold;
        public int CurrentXp => _currentXp;
        public int XpRequired => ComputeXpRequired(_level);

        // XP-Kurve: steigt quadratisch mit dem Level
        private static int ComputeXpRequired(int level) => 50 + level * 30 + level * level * 5;

        /// <summary>Wird mit EnemySpawner.DropCollected verbunden.</summary>
        public void OnDropCollected(int experience, int gold)
        {
            AddExperience(experience);
            AddGold(gold);
        }

        public void AddExperience(int amount)
        {
            _currentXp += amount;

            // Mehrfaches Level-Up in einem Frame möglich (z. B. großer XP-Drop)
            while (_currentXp >= ComputeXpRequired(_level))
            {
                _currentXp -= ComputeXpRequired(_level);
                _level++;
                EmitSignal(SignalName.LevelUp, _level);
            }

            EmitSignal(SignalName.ExperienceChanged, _currentXp, ComputeXpRequired(_level));
        }

        public void AddGold(int amount)
        {
            _gold += amount;
            EmitSignal(SignalName.GoldChanged, _gold);
        }

        /// <returns>true wenn Gold abgezogen werden konnte, false bei zu wenig Gold.</returns>
        public bool SpendGold(int amount)
        {
            if (_gold < amount) return false;
            _gold -= amount;
            EmitSignal(SignalName.GoldChanged, _gold);
            return true;
        }
    }
}
