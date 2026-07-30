using Godot;
using System.Collections.Generic;

namespace SurvivorGame.Equipment
{
    public enum EquipmentSlot
    {
        Head,
        Chest,
        Hands,
        Feet
    }

    /// <summary>Aktive Fähigkeit die ein vollständiges Set freischaltet.</summary>
    public enum SetAbility
    {
        Erdspalte,        // Kharadsche Wacht
        HeilendeFlut,     // Spiegelseen
        Windstoss,        // Offene Winde
        Totenruf,         // Asche-Stadt
        Feuersturm,       // Königliches Erbe
        GoldeneGelegenheit // Wanderhändler
    }

    public class EquipmentSetData
    {
        public string     Id           { get; }
        public string     DisplayName  { get; }
        public string     Culture      { get; }
        public SetAbility Ability      { get; }
        public float      Cooldown     { get; }
        public string     AbilityDesc  { get; }

        public EquipmentSetData(string id, string displayName, string culture,
            SetAbility ability, float cooldown, string abilityDesc)
        {
            Id          = id;
            DisplayName = displayName;
            Culture     = culture;
            Ability     = ability;
            Cooldown    = cooldown;
            AbilityDesc = abilityDesc;
        }
    }

    /// <summary>Alle Ausrüstungssets aus dem LoreScript (Abschnitt 5).</summary>
    public static class EquipmentSetDatabase
    {
        public static readonly Dictionary<string, EquipmentSetData> All = new()
        {
            { "Kharad", new EquipmentSetData(
                "Kharad", "Set der Kharadschen Wacht", "Berg/Erde",
                SetAbility.Erdspalte, 18f,
                "Erdspalte: Steinpfeiler schleudern alle Gegner im Umkreis zurück.") },

            { "Spiegelsee", new EquipmentSetData(
                "Spiegelsee", "Set der Spiegelseen", "Wasser",
                SetAbility.HeilendeFlut, 22f,
                "Heilende Flut: Heilt 30% max. HP und verlangsamt Gegner.") },

            { "Wind", new EquipmentSetData(
                "Wind", "Set der offenen Winde", "Wind",
                SetAbility.Windstoss, 10f,
                "Windstoß: Rast als Windwirbel vorwärts und schleudert Gegner weg.") },

            { "Asche", new EquipmentSetData(
                "Asche", "Set der Asche-Stadt", "Tod",
                SetAbility.Totenruf, 25f,
                "Totenruf: Beschwört 3 Schattengeister die Gegner angreifen.") },

            { "Koenigserbe", new EquipmentSetData(
                "Koenigserbe", "Königliches Erbe", "Feuer",
                SetAbility.Feuersturm, 20f,
                "Feuersturm: Expandierender Feuerring mit Brandschaden um den Träger.") },

            { "Haendler", new EquipmentSetData(
                "Haendler", "Set des Wanderhändlers", "Überregional",
                SetAbility.GoldeneGelegenheit, 45f,
                "Goldene Gelegenheit: 20s doppeltes Gold und ein Händler erscheint.") },
        };
    }
}
