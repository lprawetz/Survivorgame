using Godot;

namespace SurvivorGame.Combat
{
    /// <summary>
    /// Biom-spezifische Gegner. Jede Klasse setzt in _Ready() eigene Basiswerte,
    /// bevor EnemyBase._Ready() läuft. Erstelle für jede Klasse eine eigene .tscn
    /// (CharacterBody2D + HurtBox + Sprite/ColorRect) mit dem passenden Script.
    ///
    /// Die Farbtönung dient als Platzhalter bis eigene Sprites vorliegen.
    /// </summary>

    // Frostmonster – Eiswüste / Eisgebirge: langsam, zäh
    public partial class FrostEnemy : EnemyBase
    {
        public override void _Ready()
        {
            MaxHp            = 45f;
            MovementSpeed    = 35f;
            ContactDamage    = 7f;
            ExperienceReward = 14;
            GoldReward       = 3;
            base._Ready();
            TintSprite(new Color(0.6f, 0.8f, 1.0f));
        }

        protected void TintSprite(Color color)
        {
            var sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
            if (sprite != null) sprite.Modulate = color;
        }
    }

    // Feuerwesen – Vulkan: schnell, aggressiv, wenig HP
    public partial class FireEnemy : EnemyBase
    {
        public override void _Ready()
        {
            MaxHp            = 22f;
            MovementSpeed    = 75f;
            ContactDamage    = 9f;
            ExperienceReward = 16;
            GoldReward       = 4;
            base._Ready();
            var sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
            if (sprite != null) sprite.Modulate = new Color(1.0f, 0.5f, 0.2f);
        }
    }

    // Wüstenkreatur – Wüste: ausgewogen, mehr Gold
    public partial class DesertEnemy : EnemyBase
    {
        public override void _Ready()
        {
            MaxHp            = 30f;
            MovementSpeed    = 55f;
            ContactDamage    = 6f;
            ExperienceReward = 12;
            GoldReward       = 6;
            base._Ready();
            var sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
            if (sprite != null) sprite.Modulate = new Color(0.9f, 0.8f, 0.4f);
        }
    }

    // Sumpfwesen – Sumpf: langsam, viel HP, wenig Schaden
    public partial class SwampEnemy : EnemyBase
    {
        public override void _Ready()
        {
            MaxHp            = 55f;
            MovementSpeed    = 40f;
            ContactDamage    = 5f;
            ExperienceReward = 13;
            GoldReward       = 3;
            base._Ready();
            var sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
            if (sprite != null) sprite.Modulate = new Color(0.4f, 0.6f, 0.3f);
        }
    }
}
