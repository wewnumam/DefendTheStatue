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
        private UnityAction<float> _onTick;
        private UnityAction<EnumManager.SkillSlot> _onCastRequested;

        public void SetCallbacks(UnityAction<float> onTick, UnityAction<EnumManager.SkillSlot> onCastRequested)
        {
            _onTick = onTick;
            _onCastRequested = onCastRequested;
        }

        public void SetTickCallback(UnityAction<float> onTick)
        {
            _onTick = onTick;
        }

        private void Update()
        {
            _onTick?.Invoke(Time.deltaTime);

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
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
        }
    }
}