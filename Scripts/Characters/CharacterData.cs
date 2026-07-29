using Godot;
using System.Collections.Generic;

namespace SurvivorGame.Characters
{
    public enum CharacterElement
    {
        Fire,
        Earth,
        Water,
        Wind,
        Death
    }

    /// <summary>
    /// Basisattribute eines Charakters. Alle abgeleiteten Werte berechnen sich aus diesen.
    /// </summary>
    public class CharacterStats
    {
        public int Intelligence { get; set; }  // Magieschaden
        public int Strength { get; set; }       // Angriffsschaden
        public int Agility { get; set; }        // Angriffsgeschwindigkeit / Crit Chance
        public int Constitution { get; set; }   // Max HP / HP-Regen / Widerstände
        public int Willpower { get; set; }      // Aktivfähigkeiten
        public int Endurance { get; set; }      // Bewegungsgeschwindigkeit

        // Abgeleitete Werte
        public float MaxHp        => 50f + Constitution * 15f;
        public float HpRegen      => Constitution * 0.5f;
        public float MoveSpeed    => 80f + Endurance * 10f;
        public float AttackDamage => Strength * 5f;
        public float MagicDamage  => Intelligence * 6f;
        public float AttackSpeed  => 1.0f + Agility * 0.05f;
        public float CritChance   => Agility * 0.02f;
    }

    /// <summary>
    /// Speichert Auswahl eines Spielers im Coop: welcher Charakter + welche Variante.
    /// </summary>
    public struct PlayerSelection
    {
        public string CharacterId;
        public int    VariantIndex; // 0–3

        public PlayerSelection(string id, int variant)
        {
            CharacterId  = id;
            VariantIndex = variant;
        }
    }

    public class CharacterData
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Role { get; }
        public string Description { get; }
        public CharacterElement Element { get; }
        public CharacterStats BaseStats { get; }
        public string PortraitPath { get; }
        public bool IsStartCharacter { get; }

        /// <summary>
        /// 4 Varianten-Namen (Index 0 = Hauptname). Stammen aus der Kulturtradition des Charakters.
        /// Werden im Coop verwendet wenn derselbe Charakter mehrfach gewählt wird.
        /// </summary>
        public string[] VariantNames { get; }

        /// <summary>
        /// Subtile Sprite-Einfärbung je Variante. Variant 0 = Standard (weiß = kein Tint).
        /// </summary>
        public static readonly Color[] VariantTints =
        {
            new Color(1.00f, 1.00f, 1.00f), // Variante 0: Standard
            new Color(1.00f, 0.88f, 0.75f), // Variante 1: Warmer Sonnenuntergang-Tint
            new Color(0.78f, 0.90f, 1.00f), // Variante 2: Kühler Mondlicht-Tint
            new Color(0.90f, 0.78f, 1.00f), // Variante 3: Zwielicht/Lila-Tint
        };

        public CharacterData(string id, string displayName, string role, string description,
            CharacterElement element, CharacterStats baseStats, string portraitPath,
            string[] variantNames, bool isStartCharacter = false)
        {
            Id               = id;
            DisplayName      = displayName;
            Role             = role;
            Description      = description;
            Element          = element;
            BaseStats        = baseStats;
            PortraitPath     = portraitPath;
            VariantNames     = variantNames ?? new[] { displayName, displayName, displayName, displayName };
            IsStartCharacter = isStartCharacter;
        }

        /// <summary>Gibt den Namen für eine bestimmte Variante zurück.</summary>
        public string GetVariantName(int variantIndex)
            => VariantNames[Mathf.Clamp(variantIndex, 0, VariantNames.Length - 1)];
    }

    /// <summary>
    /// Enthält alle spielbaren Charaktere mit ihren Basis-Daten aus dem Lore-Dokument.
    /// </summary>
    public static class CharacterDatabase
    {
        public static readonly Dictionary<string, CharacterData> All = new()
        {
            {
                "Esmeralda", new CharacterData(
                    id: "Esmeralda",
                    displayName: "Prinzessin Esmeralda",
                    role: "Magierin",
                    description: "Die letzte Erbin eines alten Königshauses, das einst den Obelisken bewachte. " +
                                 "Ihr Feuer ist kein bloßes Element – es ist die Erinnerung an ein verbranntes " +
                                 "Königreich und der Anfang von etwas Neuem.",
                    element: CharacterElement.Fire,
                    baseStats: new CharacterStats
                    {
                        Intelligence = 8,
                        Strength     = 4,
                        Agility      = 6,
                        Constitution = 5,
                        Willpower    = 7,
                        Endurance    = 5
                    },
                    portraitPath: "res://Assets/Characters/Fire/Esmaralda.png",
                    variantNames: new[] { "Esmeralda", "Sera", "Vael", "Kaelra" },
                    isStartCharacter: true)
            },
            {
                "Rusk", new CharacterData(
                    id: "Rusk",
                    displayName: "Rusk",
                    role: "Tank / Supporter",
                    description: "Ein uralter Steinwächter aus den Tiefen der Erde. " +
                                 "Träge, geduldig, enorm stabil – der letzte, der noch weiß, " +
                                 "wie die Erde einmal eine Seele hatte.",
                    element: CharacterElement.Earth,
                    baseStats: new CharacterStats
                    {
                        Intelligence = 3,
                        Strength     = 7,
                        Agility      = 2,
                        Constitution = 10,
                        Willpower    = 5,
                        Endurance    = 2
                    },
                    portraitPath: "res://Assets/Characters/Earth/Rusk.png",
                    variantNames: new[] { "Rusk", "Korrath", "Borvann", "Tharok" })
            },
            {
                "Thalira", new CharacterData(
                    id: "Thalira",
                    displayName: "Thalira",
                    role: "Heilerin / Magierin",
                    description: "Eine Wasserwächterin aus den Spiegelseen – weder Mensch noch Geist, " +
                                 "sondern eine lebendige Erinnerung. Sie erinnert sich an alle, die im " +
                                 "Wasser gestorben sind, und hält dennoch ihre Hand über die Überlebenden.",
                    element: CharacterElement.Water,
                    baseStats: new CharacterStats
                    {
                        Intelligence = 7,
                        Strength     = 2,
                        Agility      = 4,
                        Constitution = 7,
                        Willpower    = 8,
                        Endurance    = 4
                    },
                    portraitPath: "res://Assets/Characters/Water/Thalira.png",
                    variantNames: new[] { "Thalira", "Nyris", "Selva", "Osea" })
            },
            {
                "Vehyr", new CharacterData(
                    id: "Vehyr",
                    displayName: "Vehyr",
                    role: "Ranger / Assassine / Duelist",
                    description: "Ein Windläufer – weder ganz Mensch noch ganz Elementar. " +
                                 "Extrem agil und unberechenbar. " +
                                 "Seine Freiheit ist seine größte Stärke und seine größte Schwäche.",
                    element: CharacterElement.Wind,
                    baseStats: new CharacterStats
                    {
                        Intelligence = 5,
                        Strength     = 5,
                        Agility      = 10,
                        Constitution = 4,
                        Willpower    = 4,
                        Endurance    = 8
                    },
                    portraitPath: "res://Assets/Characters/Wind/Vehyr.png",
                    variantNames: new[] { "Vehyr", "Caelan", "Aeron", "Ilyr" })
            },
            {
                "Morvane", new CharacterData(
                    id: "Morvane",
                    displayName: "Morvane",
                    role: "Totensprecher / Magier",
                    description: "Eine Präsenz zwischen Leben, Erinnerung und Verfall. " +
                                 "Er ist nicht der Feind des Lebens – sondern jener, der es versteht. " +
                                 "Seine Macht ist eine Art letzter Respekt vor allem, was vergeht.",
                    element: CharacterElement.Death,
                    baseStats: new CharacterStats
                    {
                        Intelligence = 9,
                        Strength     = 3,
                        Agility      = 4,
                        Constitution = 6,
                        Willpower    = 9,
                        Endurance    = 3
                    },
                    portraitPath: "res://Assets/Characters/Death/Morvane.png",
                    variantNames: new[] { "Morvane", "Varkh", "Nyxar", "Drenhal" })
            }
        };
    }
}
