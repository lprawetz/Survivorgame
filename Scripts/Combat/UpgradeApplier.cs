using Godot;

namespace SurvivorGame.Combat
{
    /// <summary>
    /// Verbindet die Skill-Auswahl (LevelUpSystem) mit den Spielern und deren AutoAttack.
    /// Als Node in GameWorld.tscn einfügen.
    ///
    /// Wendet gewählte Skills auf ALLE Spieler an (gemeinsame Progression, passend zum
    /// einzelnen geteilten ExperienceSystem).
    ///
    /// Verbindung: findet LevelUpSystem automatisch über Gruppe "level_up_system".
    /// </summary>
    public partial class UpgradeApplier : Node
    {
        private LevelUpSystem _levelUpSystem;

        public override void _Ready()
        {
            CallDeferred(MethodName.ConnectSignals);
        }

        private void ConnectSignals()
        {
            _levelUpSystem = GetTree().GetFirstNodeInGroup("level_up_system") as LevelUpSystem;
            if (_levelUpSystem != null)
                _levelUpSystem.SkillSelected += OnSkillSelected;
        }

        private void OnSkillSelected(int skillTypeIndex, float value)
        {
            var type = (SkillType)skillTypeIndex;

            foreach (Node node in GetTree().GetNodesInGroup("player"))
            {
                if (node is not Player player) continue;
                var attack = player.GetNodeOrNull<AutoAttack>("AutoAttack");
                ApplyToPlayer(player, attack, type, value);
            }
        }

        private static void ApplyToPlayer(Player player, AutoAttack attack, SkillType type, float value)
        {
            switch (type)
            {
                case SkillType.DamageUp:
                case SkillType.MagicDamageUp:
                    if (attack != null) attack.DamageMultiplier += value;
                    break;

                case SkillType.SpeedUp:
                    // value ist ein Prozentwert (0.10 = +10%); auf ~90 Basis-Speed umgerechnet
                    player.ApplyStatModifier(moveSpeedBonus: value * 90f);
                    break;

                case SkillType.HpUp:
                    player.ApplyStatModifier(maxHpBonus: value);
                    break;

                case SkillType.HpRegen:
                    player.ApplyStatModifier(hpRegenBonus: value);
                    break;

                case SkillType.CritChance:
                    if (attack != null) attack.CritChance += value;
                    break;

                case SkillType.AttackSpeed:
                    if (attack != null) attack.ApplyAttackSpeed(attack.AttackSpeedMultiplier + value);
                    break;

                case SkillType.AreaUp:
                    if (attack != null) attack.AreaMultiplier += value;
                    break;

                case SkillType.Multishot:
                    if (attack != null) attack.ExtraProjectiles += (int)value;
                    break;
            }
        }
    }
}
