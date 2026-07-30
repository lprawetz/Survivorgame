using Godot;
using System.Collections.Generic;
using SurvivorGame.Equipment;

namespace SurvivorGame.Combat
{
    /// <summary>
    /// Händler-NPC. Bietet zufällige Set-Teile an, die der Spieler mit Gold kauft.
    /// Als Area2D-basierte Szene: Spieler betritt Radius → Kaufmenü kann geöffnet werden.
    ///
    /// SZENEN-AUFBAU (Merchant.tscn):
    ///   Area2D  [Script]
    ///     Sprite2D / ColorRect  (Darstellung)
    ///     CollisionShape2D      (Interaktionsradius)
    ///
    /// Das eigentliche Kauf-UI wird über das Signal PlayerInRange gesteuert.
    /// </summary>
    public partial class Merchant : Area2D
    {
        [Signal] public delegate void PlayerEnteredEventHandler();
        [Signal] public delegate void PlayerExitedEventHandler();

        public string       CultureName { get; private set; }
        public List<Offer>  Inventory   { get; } = new();

        private readonly RandomNumberGenerator _rng = new();
        private static readonly string[] Cultures =
            { "Berg/Erde", "Wasser", "Wind", "Tod", "Feuer", "Überregional" };

        public struct Offer
        {
            public string        SetId;
            public EquipmentSlot Slot;
            public int           Price;
        }

        public override void _Ready()
        {
            _rng.Randomize();
            GenerateInventory();

            BodyEntered += _ => EmitSignal(SignalName.PlayerEntered);
            BodyExited  += _ => EmitSignal(SignalName.PlayerExited);
        }

        private void GenerateInventory()
        {
            CultureName = Cultures[_rng.RandiRange(0, Cultures.Length - 1)];

            // Passendes Set zur Kultur wählen
            string setId = MatchSetToCulture(CultureName);

            // 30% Chance: gar keine Set-Teile
            if (_rng.Randf() < 0.3f) return;

            // 1–3 zufällige Slots dieses Sets anbieten
            var slots = new List<EquipmentSlot>
            {
                EquipmentSlot.Head, EquipmentSlot.Chest,
                EquipmentSlot.Hands, EquipmentSlot.Feet
            };
            int count = _rng.RandiRange(1, 3);

            for (int i = 0; i < count && slots.Count > 0; i++)
            {
                int idx = _rng.RandiRange(0, slots.Count - 1);
                Inventory.Add(new Offer
                {
                    SetId = setId,
                    Slot  = slots[idx],
                    Price = _rng.RandiRange(30, 80)
                });
                slots.RemoveAt(idx);
            }
        }

        private static string MatchSetToCulture(string culture) => culture switch
        {
            "Berg/Erde"    => "Kharad",
            "Wasser"       => "Spiegelsee",
            "Wind"         => "Wind",
            "Tod"          => "Asche",
            "Feuer"        => "Koenigserbe",
            _              => "Haendler"
        };

        /// <summary>
        /// Versucht ein Angebot zu kaufen. Gold wird über das ExperienceSystem abgezogen,
        /// das Teil über den SetManager des Spielers angelegt.
        /// </summary>
        public bool TryBuy(int offerIndex, ExperienceSystem economy, SetManager setManager)
        {
            if (offerIndex < 0 || offerIndex >= Inventory.Count) return false;
            var offer = Inventory[offerIndex];

            if (!economy.SpendGold(offer.Price)) return false;

            setManager.EquipPiece(offer.SetId, offer.Slot);
            Inventory.RemoveAt(offerIndex);
            return true;
        }
    }
}
