using Agate.MVC.Base;
using ProjectTA.Utility;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ProjectTA.Module.DevConsole
{
    /// <summary>
    /// IMGUI dev panel. It is deliberately not scene-authored: this is a debug tool, not a shipped
    /// screen, and OnGUI keeps it out of the canvas/prefab graph. F1 toggles it.
    /// </summary>
    public class DevConsoleView : BaseView
    {
        private static readonly string[] FlagLabels =
        {
            "God mode",
            "Infinite stamina",
            "Infinite arrows",
            "Free build",
            "No cooldown",
            "No spawn",
            "Fast forward x2",
            "One-hit kill",
            "Infinite essence"
        };

        private static readonly string[] ActionLabels =
        {
            "+1000 essence",
            "+100 mana",
            "Heal statue",
            "Heal player",
            "Kill all monsters",
            "Spawn boss",
            "Kill boss",
            "Max statue level"
        };

        private readonly bool[] _flags = new bool[DevConsoleModel.FlagCount];
        private UnityAction<EnumManager.CheatType> _onToggle;
        private UnityAction<DevConsoleAction> _onAction;
        private UnityAction _onToggleOpen;

        private bool _available;
        private bool _isOpen;

        public void SetCallbacks(
            UnityAction onToggleOpen,
            UnityAction<EnumManager.CheatType> onToggle,
            UnityAction<DevConsoleAction> onAction)
        {
            _onToggleOpen = onToggleOpen;
            _onToggle = onToggle;
            _onAction = onAction;
        }

        public void SetAvailable(bool isAvailable)
        {
            _available = isAvailable;
            if (!isAvailable)
            {
                _isOpen = false;
            }
        }

        public void SetOpen(bool isOpen)
        {
            _isOpen = isOpen && _available;
        }

        public void SetFlag(EnumManager.CheatType cheat, bool isOn)
        {
            _flags[(int)cheat] = isOn;
        }

        private void Update()
        {
            if (_available && Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
            {
                _onToggleOpen?.Invoke();
            }
        }

        private void OnGUI()
        {
            if (!_available || !_isOpen)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(12f, 12f, 280f, 560f), GUI.skin.box);
            GUILayout.Label("DEV CONSOLE  (F1)");

            GUILayout.Space(4f);
            GUILayout.Label("Cheats");
            for (int i = 0; i < FlagLabels.Length; i++)
            {
                bool next = GUILayout.Toggle(_flags[i], FlagLabels[i]);
                if (next != _flags[i])
                {
                    _onToggle?.Invoke((EnumManager.CheatType)i);
                }
            }

            GUILayout.Space(8f);
            GUILayout.Label("Actions");
            for (int i = 0; i < ActionLabels.Length; i++)
            {
                if (GUILayout.Button(ActionLabels[i]))
                {
                    _onAction?.Invoke((DevConsoleAction)i);
                }
            }

            GUILayout.EndArea();
        }
    }
}
