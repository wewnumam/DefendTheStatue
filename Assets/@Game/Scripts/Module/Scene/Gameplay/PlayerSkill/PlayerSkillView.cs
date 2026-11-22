using Agate.MVC.Base;
using ProjectTA.Utility;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ProjectTA.Module.PlayerSkill
{
    /// <summary>
    /// Drives the skill model's cooldown clocks and reads the skill keys. Guarded art lives elsewhere;
    /// this view owns no renderer at all.
    /// ponytail: raw number keys — the Input asset has no skill actions yet, so route these through
    /// InputController once the map is regenerated in the Editor.
    /// </summary>
    public class PlayerSkillView : BaseView
    {
        [Header("Upgrade interface")]
        [SerializeField] private Transform _statue;
        [SerializeField] private float _upgradeRadius = 120f;

        private UnityAction<float> _onTick;
        private UnityAction<EnumManager.SkillSlot> _onCastRequested;
        private UnityAction _onUpgradePanelToggleRequested;

        public void SetCallbacks(
            UnityAction<float> onTick,
            UnityAction<EnumManager.SkillSlot> onCastRequested,
            UnityAction onUpgradePanelToggleRequested)
        {
            _onTick = onTick;
            _onCastRequested = onCastRequested;
            _onUpgradePanelToggleRequested = onUpgradePanelToggleRequested;
        }

        public void SetTickCallback(UnityAction<float> onTick)
        {
            _onTick = onTick;
        }

        /// <summary>The upgrade interface is statue-local: the player has to stand inside the radius.</summary>
        public bool IsWithinUpgradeRadius(Vector3 worldPosition)
        {
            if (_statue == null)
            {
                return false;
            }

            return ((Vector2)_statue.position - (Vector2)worldPosition).sqrMagnitude
                <= _upgradeRadius * _upgradeRadius;
        }

        private void Update()
        {
            _onTick?.Invoke(Time.deltaTime);

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            // Tab is a second way into the skill upgrade interface; E over the god statue is the first.
            if (keyboard.tabKey.wasPressedThisFrame && Time.timeScale > 0f)
            {
                _onUpgradePanelToggleRequested?.Invoke();
            }

            // Casting must not fire while the world is frozen.
            if (Time.timeScale <= 0f)
            {
                return;
            }

            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                _onCastRequested?.Invoke(EnumManager.SkillSlot.Skill1);
            }

            if (keyboard.digit2Key.wasPressedThisFrame)
            {
                _onCastRequested?.Invoke(EnumManager.SkillSlot.Skill2);
            }

            if (keyboard.digit3Key.wasPressedThisFrame)
            {
                _onCastRequested?.Invoke(EnumManager.SkillSlot.Skill3);
            }

            if (keyboard.digit4Key.wasPressedThisFrame)
            {
                _onCastRequested?.Invoke(EnumManager.SkillSlot.Skill4);
            }
        }
    }
}