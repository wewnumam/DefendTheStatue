using Agate.MVC.Base;
using ProjectTA.Message;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ProjectTA.Utility;

namespace ProjectTA.Module.Hud
{
    /// <summary>
    /// Battle HUD. Every element is authored in the scene and wired here: three gauges (health,
    /// stamina, mana), the statue bar and the gold/arrow counters. Gauges are Sliders (0..1), so the
    /// bar is driven by the slider value instead of an Image fill amount.
    /// </summary>
    public class HudView : BaseView
    {
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private Slider _staminaSlider;
        [SerializeField] private Slider _manaSlider;
        [SerializeField] private Slider _statueSlider;
        [SerializeField] private TMP_Text _goldText;
        [SerializeField] private TMP_Text _arrowText;
        [SerializeField] private TMP_Text _weaponText;

        public void SetStats(PlayerStatsChangedMessage message)
        {
            SetBar(_healthSlider, message.Health, message.MaxHealth);
            SetBar(_staminaSlider, message.Stamina, message.MaxStamina);
            SetBar(_manaSlider, message.Mana, message.MaxMana);

            if (_goldText != null)
            {
                _goldText.text = $"Gold {message.Gold}";
            }

            if (_arrowText != null)
            {
                _arrowText.text = $"Arrow {message.Arrows}";
            }
        }

        public void SetStatueHealth(float health, float maxHealth)
        {
            SetBar(_statueSlider, health, maxHealth);
        }

        /// <summary>Which weapon is in hand, mirrored from the skill module's resolved switch.</summary>
        public void SetWeapon(EnumManager.WeaponKind weapon)
        {
            if (_weaponText != null)
            {
                _weaponText.text = weapon == EnumManager.WeaponKind.Bow ? "Weapon Bow" : "Weapon Sword";
            }
        }

        private static void SetBar(Slider slider, float value, float max)
        {
            if (slider == null)
            {
                return;
            }

            slider.value = max > 0f ? Mathf.Clamp01(value / max) : 0f;
        }
    }
}
