using Agate.MVC.Base;
using ProjectTA.Message;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectTA.Module.Hud
{
    /// <summary>
    /// Battle HUD. Every element is authored in the scene and wired here: three radial gauges (health,
    /// stamina, mana), the statue bar and the gold/arrow counters.
    /// </summary>
    public class HudView : BaseView
    {
        [SerializeField] private Image _healthFill;
        [SerializeField] private Image _staminaFill;
        [SerializeField] private Image _manaFill;
        [SerializeField] private Image _statueFill;
        [SerializeField] private TMP_Text _goldText;
        [SerializeField] private TMP_Text _arrowText;

        public void SetStats(PlayerStatsChangedMessage message)
        {
            SetFill(_healthFill, message.Health, message.MaxHealth);
            SetFill(_staminaFill, message.Stamina, message.MaxStamina);
            SetFill(_manaFill, message.Mana, message.MaxMana);

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
            SetFill(_statueFill, health, maxHealth);
        }

        private static void SetFill(Image image, float value, float max)
        {
            if (image == null)
            {
                return;
            }

            image.fillAmount = max > 0f ? Mathf.Clamp01(value / max) : 0f;
        }
    }
}
