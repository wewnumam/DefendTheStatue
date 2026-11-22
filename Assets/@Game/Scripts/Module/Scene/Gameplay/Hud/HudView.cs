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
    /// stamina, mana), the statue bar and the essence/arrow counters. Gauges are Sliders (0..1), so the
    /// bar is driven by the slider value instead of an Image fill amount.
    /// </summary>
    public class HudView : BaseView
    {
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private Slider _staminaSlider;
        [SerializeField] private Slider _manaSlider;
        [SerializeField] private Slider _statueSlider;
        [SerializeField] private TMP_Text _essenceText;
        [SerializeField] private TMP_Text _arrowText;
        [SerializeField] private TMP_Text _weaponText;

        /// <summary>
        /// Run clock and boss bar are built at runtime: the scene's HUD is hand-authored, and adding
        /// two more UI trees there is more fragile than four lines of anchors. ponytail: move them into
        /// the scene if a designer ever needs to theme them.
        /// </summary>
        private const float BossBarWidth = 540f;

        private TMP_Text _clockText;
        private GameObject _bossBarRoot;
        private RawImage _bossFill;
        private RectTransform _bossFillRect;
        private TMP_Text _bossPhaseText;
        private int _clockSeconds;
        private TMP_Text _skillText;
        private float _bossFillWidth = BossBarWidth - 4f;

        public void SetStats(PlayerStatsChangedMessage message)
        {
            SetBar(_healthSlider, message.Health, message.MaxHealth);
            SetBar(_staminaSlider, message.Stamina, message.MaxStamina);
            SetBar(_manaSlider, message.Mana, message.MaxMana);

            if (_essenceText != null)
            {
                _essenceText.text = $"Essence {message.Essence}";
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

        /// <summary>GDD §21: the statue bar reads red while a monster stands in the heal zone.</summary>
        public void SetStatueTainted(bool tainted)
        {
            if (_statueSlider == null || _statueSlider.fillRect == null)
            {
                return;
            }

            Image fill = _statueSlider.fillRect.GetComponent<Image>();
            if (fill != null)
            {
                fill.color = tainted ? new Color(0.9f, 0.25f, 0.25f, 1f) : Color.white;
            }
        }

        /// <summary>GDD §15: the eight skill boxes, one line each, built at runtime like the clock.</summary>
        public void SetSkillGrid(string[] lines)
        {
            EnsureSkillGrid();
            _skillText.text = lines == null ? string.Empty : string.Join("\n", lines);
        }

        private void EnsureSkillGrid()
        {
            if (_skillText != null)
            {
                return;
            }

            _skillText = CreateText(
                "SkillGrid", new Vector2(1f, 0f), new Vector2(-16f, 16f), new Vector2(320f, 220f), 18f);
            _skillText.alignment = TextAlignmentOptions.BottomRight;
            _skillText.rectTransform.pivot = new Vector2(1f, 0f);
        }

        /// <summary>Which weapon is in hand, mirrored from the skill module's resolved switch.</summary>
        public void SetWeapon(EnumManager.WeaponKind weapon)
        {
            if (_weaponText != null)
            {
                _weaponText.text = weapon == EnumManager.WeaponKind.Bow ? "Weapon Bow" : "Weapon Sword";
            }
        }

        /// <summary>Reference HUD: mm:ss, blinking from 9:30, solid red once the boss is due.</summary>
        public void SetRunClock(int totalSeconds)
        {
            EnsureRunUi();
            _clockSeconds = Mathf.Max(0, totalSeconds);
            _clockText.text = $"{_clockSeconds / 60:00}:{_clockSeconds % 60:00}";
        }

        public void ResetRunUi()
        {
            SetRunClock(0);
            SetBossVisible(false);
            _clockText.color = Color.white;
        }

        public void SetBossVisible(bool visible)
        {
            EnsureRunUi();
            _bossBarRoot.SetActive(visible);

            if (visible)
            {
                _bossFillRect.sizeDelta = new Vector2(Mathf.Max(0f, BossBarWidth - 4f), 0f);
                _bossFill.color = new Color(0.75f, 0.4f, 1f, 1f);
                SetBossPhase(1);
            }
        }

        public void SetBossHealth(float health, float maxHealth, int phase)
        {
            EnsureRunUi();
            _bossBarRoot.SetActive(true);

            float fraction = maxHealth > 0f ? Mathf.Clamp01(health / maxHealth) : 0f;
            _bossFillWidth = Mathf.Max(0f, BossBarWidth - 4f) * fraction;
            _bossFillRect.sizeDelta = new Vector2(_bossFillWidth, 0f);
            _bossFill.color = phase >= 3
                ? new Color(1f, 0.2f, 0.2f, 1f)
                : phase == 2 ? new Color(1f, 0.6f, 0.2f, 1f) : new Color(0.75f, 0.4f, 1f, 1f);

            SetBossPhase(phase);
        }

        public void SetBossPhase(int phase)
        {
            if (_bossPhaseText == null)
            {
                return;
            }

            _bossPhaseText.text = $"BOSS — PHASE {Mathf.Clamp(phase, 1, 3)}";
            _bossPhaseText.color = phase >= 3
                ? new Color(1f, 0.3f, 0.3f, 1f)
                : phase == 2 ? new Color(1f, 0.75f, 0.3f, 1f) : new Color(0.85f, 0.6f, 1f, 1f);
        }

        private void Update()
        {
            if (_clockText == null || _clockSeconds < 540)
            {
                return;
            }

            // Unscaled: the clock keeps blinking while the run is paused or slowed.
            bool on = Mathf.Repeat(Time.unscaledTime, 1f) < 0.5f;
            if (_clockSeconds >= 600)
            {
                _clockText.color = on ? new Color(1f, 0.15f, 0.15f, 1f) : new Color(1f, 0.65f, 0.65f, 1f);
            }
            else
            {
                _clockText.color = on ? new Color(1f, 0.3f, 0.3f, 1f) : Color.white;
            }
        }

        private void EnsureRunUi()
        {
            if (_clockText != null)
            {
                return;
            }

            _clockText = CreateText("RunClock", new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(260f, 52f), 38f);
            _clockText.text = "00:00";

            _bossBarRoot = new GameObject("BossBar", typeof(RectTransform));
            RectTransform barRect = _bossBarRoot.GetComponent<RectTransform>();
            barRect.SetParent(transform, false);
            barRect.anchorMin = barRect.anchorMax = new Vector2(0.5f, 1f);
            barRect.pivot = new Vector2(0.5f, 1f);
            barRect.anchoredPosition = new Vector2(0f, -84f);
            barRect.sizeDelta = new Vector2(BossBarWidth, 22f);

            RawImage background = _bossBarRoot.AddComponent<RawImage>();
            background.texture = Texture2D.whiteTexture;
            background.color = new Color(0f, 0f, 0f, 0.75f);
            background.raycastTarget = false;

            GameObject fillObject = new GameObject("Fill", typeof(RectTransform));
            _bossFillRect = fillObject.GetComponent<RectTransform>();
            _bossFillRect.SetParent(barRect, false);
            _bossFillRect.anchorMin = new Vector2(0f, 0f);
            _bossFillRect.anchorMax = new Vector2(0f, 1f);
            _bossFillRect.pivot = new Vector2(0f, 0.5f);
            _bossFillRect.offsetMin = new Vector2(2f, 2f);
            _bossFillRect.offsetMax = new Vector2(2f, -2f);
            _bossFillRect.sizeDelta = new Vector2(_bossFillWidth, 0f);

            _bossFill = fillObject.AddComponent<RawImage>();
            _bossFill.texture = Texture2D.whiteTexture;
            _bossFill.color = new Color(0.75f, 0.4f, 1f, 1f);
            _bossFill.raycastTarget = false;

            _bossPhaseText = CreateText("BossPhase", new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(320f, 26f), 20f);
            _bossPhaseText.rectTransform.SetParent(barRect, false);
            _bossPhaseText.text = "BOSS — PHASE 1";
            _bossBarRoot.SetActive(false);
        }

        private TMP_Text CreateText(string name, Vector2 anchor, Vector2 anchoredPosition, Vector2 size, float fontSize)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(transform, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            return text;
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
