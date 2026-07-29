using Godot;
using System.Collections.Generic;

namespace SurvivorGame
{
    /// <summary>
    /// Autoload-Singleton: Verwaltet die Eingabegeräte für 1–4 Spieler.
    ///
    /// Spieler-Slots:
    ///   P1 – Keyboard WASD    + Leertaste (Aktivfähigkeit)
    ///   P2 – Keyboard Pfeile  + Enter
    ///   P3 – Gamepad 0        + Südtaste (Button 0)
    ///   P4 – Gamepad 1        + Südtaste (Button 0)
    ///
    /// Gamepad-Unterstützung ist plug-and-play: Wird ein Gamepad erkannt,
    /// wird es automatisch dem nächsten freien Slot zugeordnet.
    ///
    /// SETUP in project.godot:
    ///   Füge diesen Node als Autoload ein:
    ///   Name: "PlayerInputManager"
    ///   Path: "res://Scripts/PlayerInputManager.cs"
    ///
    /// Nutzung im Player-Script:
    ///   var dir = PlayerInputManager.Instance.GetMovement(PlayerIndex);
    ///   bool active = PlayerInputManager.Instance.IsActiveAbilityPressed(PlayerIndex);
    /// </summary>
    public partial class PlayerInputManager : Node
    {
        public static PlayerInputManager Instance { get; private set; }

        // Maximal 4 Spieler
        public const int MaxPlayers = 4;

        // Zugeordnete Gamepad-IDs je Spieler-Slot (P3 = Index 2, P4 = Index 3)
        // -1 = kein Gamepad zugeordnet
        private readonly int[] _gamepadSlots = { -1, -1, -1, -1 };

        // Welche Slots sind aktiv (d. h. ein Spieler hat sich eingeloggt)
        private readonly bool[] _activeSlots = { false, false, false, false };

        // Anzahl aktiver Spieler
        public int ActivePlayerCount { get; private set; } = 1;

        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                // P1 ist immer aktiv (Keyboard)
                _activeSlots[0] = true;
            }
            else
            {
                QueueFree();
                return;
            }

            Input.JoyConnectionChanged += OnJoyConnectionChanged;

            // Bereits verbundene Gamepads erkennen
            foreach (int deviceId in Input.GetConnectedJoypads())
                AssignGamepad(deviceId);
        }

        public override void _Process(double delta)
        {
            // P2 aktivieren wenn Pfeiltasten gedrückt (zweiter Tastatur-Spieler)
            if (!_activeSlots[1] && IsKeyboardP2Moving())
                ActivateSlot(1);
        }

        // ─── Öffentliche API ─────────────────────────────────────────────

        /// <summary>Gibt die Bewegungsrichtung für einen Spieler zurück (normalisiert).</summary>
        public Vector2 GetMovement(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= MaxPlayers) return Vector2.Zero;
            if (!_activeSlots[playerIndex])                    return Vector2.Zero;

            return playerIndex switch
            {
                0 => GetKeyboardMovement("left_p1", "right_p1", "up_p1", "down_p1"),
                1 => GetKeyboardMovement("left_p2", "right_p2", "up_p2", "down_p2"),
                _ => GetGamepadMovement(playerIndex)
            };
        }

        /// <summary>Gibt zurück ob die Aktivfähigkeitstaste für diesen Spieler gedrückt wurde.</summary>
        public bool IsActiveAbilityJustPressed(int playerIndex)
        {
            if (!_activeSlots[playerIndex]) return false;

            return playerIndex switch
            {
                0 => Input.IsActionJustPressed("active_p1"),
                1 => Input.IsActionJustPressed("active_p2"),
                _ => IsGamepadButtonJustPressed(playerIndex, JoyButton.A)
            };
        }

        public bool IsSlotActive(int playerIndex) =>
            playerIndex >= 0 && playerIndex < MaxPlayers && _activeSlots[playerIndex];

        // ─── Interne Logik ───────────────────────────────────────────────

        private Vector2 GetKeyboardMovement(string left, string right, string up, string down)
        {
            var dir = new Vector2(
                Input.GetActionStrength(right) - Input.GetActionStrength(left),
                Input.GetActionStrength(down)  - Input.GetActionStrength(up)
            );
            return dir.LengthSquared() > 0.01f ? dir.Normalized() : Vector2.Zero;
        }

        private Vector2 GetGamepadMovement(int playerIndex)
        {
            int deviceId = _gamepadSlots[playerIndex];
            if (deviceId < 0) return Vector2.Zero;

            var dir = new Vector2(
                Input.GetJoyAxis(deviceId, JoyAxis.LeftX),
                Input.GetJoyAxis(deviceId, JoyAxis.LeftY)
            );
            // Deadzone
            return dir.LengthSquared() > 0.04f ? dir.Normalized() : Vector2.Zero;
        }

        private bool IsGamepadButtonJustPressed(int playerIndex, JoyButton button)
        {
            int deviceId = _gamepadSlots[playerIndex];
            return deviceId >= 0 && Input.IsJoyButtonPressed(deviceId, button);
        }

        private bool IsKeyboardP2Moving()
        {
            return Input.IsActionPressed("left_p2")  ||
                   Input.IsActionPressed("right_p2") ||
                   Input.IsActionPressed("up_p2")    ||
                   Input.IsActionPressed("down_p2");
        }

        private void ActivateSlot(int index)
        {
            _activeSlots[index] = true;
            ActivePlayerCount   = CountActiveSlots();
            GD.Print($"[PlayerInputManager] Spieler {index + 1} aktiviert. Aktive Spieler: {ActivePlayerCount}");
        }

        private void AssignGamepad(int deviceId)
        {
            // Gamepad den Slots P3 (Index 2) und P4 (Index 3) zuordnen
            for (int i = 2; i < MaxPlayers; i++)
            {
                if (_gamepadSlots[i] == deviceId) return; // bereits zugeordnet
                if (_gamepadSlots[i] == -1)
                {
                    _gamepadSlots[i] = deviceId;
                    ActivateSlot(i);
                    GD.Print($"[PlayerInputManager] Gamepad {deviceId} → Slot P{i + 1}");
                    return;
                }
            }
        }

        private void RemoveGamepad(int deviceId)
        {
            for (int i = 2; i < MaxPlayers; i++)
            {
                if (_gamepadSlots[i] == deviceId)
                {
                    _gamepadSlots[i] = -1;
                    _activeSlots[i]  = false;
                    ActivePlayerCount = CountActiveSlots();
                    GD.Print($"[PlayerInputManager] Gamepad {deviceId} (Slot P{i + 1}) getrennt.");
                    return;
                }
            }
        }

        private void OnJoyConnectionChanged(long device, bool connected)
        {
            if (connected) AssignGamepad((int)device);
            else           RemoveGamepad((int)device);
        }

        private int CountActiveSlots()
        {
            int count = 0;
            foreach (bool active in _activeSlots)
                if (active) count++;
            return count;
        }
    }
}
