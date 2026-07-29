using Godot;
using System.Collections.Generic;

namespace SurvivorGame.Combat
{
    public enum SkillType
    {
        DamageUp,
        MagicDamageUp,
        SpeedUp,
        HpUp,
        HpRegen,
        CritChance,
        AttackSpeed,
        AreaUp,
        Multishot
    }

    public class SkillOption
    {
        public SkillType Type        { get; }
        public string    DisplayName { get; }
        public string    Description { get; }
        public float     Value       { get; }

        public SkillOption(SkillType type, string displayName, string description, float value)
        {
            Type        = type;
            DisplayName = displayName;
            Description = description;
            Value       = value;
        }
    }

    /// <summary>
    /// Verwaltet die Skill-Auswahl beim Level-Up (3 zufällige Optionen aus dem Pool).
    /// Verbinde ExperienceSystem.LevelUp -> LevelUpSystem.OnPlayerLevelUp.
    /// Verbinde SkillSelected -> Player/Weapon-Knoten für die tatsächliche Anwendung.
    ///
    /// Das LevelUpUI sollte CurrentOptions auslesen und drei Buttons anzeigen.
    /// Wenn der Spieler einen Button drückt, ruft es SelectSkill(index) auf.
    /// </summary>
    public partial class LevelUpSystem : Node
    {
        [Signal] public delegate void SkillOptionsReadyEventHandler();
        [Signal] public delegate void SkillSelectedEventHandler(int skillTypeIndex, float value);

        private static readonly List<SkillOption> AllSkills = new()
        {
            new SkillOption(SkillType.DamageUp,      "Schaden +",                "+15% physischer Schaden",          0.15f),
            new SkillOption(SkillType.MagicDamageUp,  "Magieschaden +",           "+15% Magieschaden",               0.15f),
            new SkillOption(SkillType.SpeedUp,        "Bewegungsgeschwindigkeit +","+10% Bewegungsgeschwindigkeit",   0.10f),
            new SkillOption(SkillType.HpUp,           "Leben +",                  "+20 maximale Lebenspunkte",       20f),
            new SkillOption(SkillType.HpRegen,        "Regeneration +",           "+1 LP/s Lebensregeneration",       1f),
            new SkillOption(SkillType.CritChance,     "Kritischer Treffer +",     "+5% Crit-Chance",                  0.05f),
            new SkillOption(SkillType.AttackSpeed,    "Angriffsgeschwindigkeit +", "+10% Angriffsgeschwindigkeit",    0.10f),
            new SkillOption(SkillType.AreaUp,         "Wirkungsbereich +",        "+15% Angriffsfläche",              0.15f),
            new SkillOption(SkillType.Multishot,      "Mehrfachschuss",           "Angriffe treffen 1 weiteres Ziel", 1f),
        };

        public List<SkillOption> CurrentOptions { get; private set; } = new();
        private readonly RandomNumberGenerator _rng = new();

        public override void _Ready()
        {
            _rng.Randomize();
            AddToGroup("level_up_system");
        }

        /// <summary>Wird mit ExperienceSystem.LevelUp verbunden.</summary>
        public void OnPlayerLevelUp(int newLevel)
        {
            GenerateOptions(3);
            // Hier könnte das Spiel pausiert werden: GetTree().Paused = true;
        }

        public List<SkillOption> GenerateOptions(int count = 3)
        {
            var available = new List<SkillOption>(AllSkills);
            CurrentOptions.Clear();

            for (int i = 0; i < count && available.Count > 0; i++)
            {
                int index = _rng.RandiRange(0, available.Count - 1);
                CurrentOptions.Add(available[index]);
                available.RemoveAt(index);
            }

            EmitSignal(SignalName.SkillOptionsReady);
            return CurrentOptions;
        }

        /// <summary>Vom UI aufgerufen, wenn der Spieler eine der drei Optionen wählt.</summary>
        public void SelectSkill(int optionIndex)
        {
            if (optionIndex < 0 || optionIndex >= CurrentOptions.Count) return;
            var skill = CurrentOptions[optionIndex];
            EmitSignal(SignalName.SkillSelected, (int)skill.Type, skill.Value);
            CurrentOptions.Clear();
            // GetTree().Paused = false; // Spiel wieder fortsetzen
        }
    }
}
