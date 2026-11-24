using Agate.MVC.Base;
using ProjectTA.Message;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
        /// <summary>Optional scene object the round gauges hang under — its rect is the group's origin.</summary>
        [SerializeField] private RectTransform _gaugeAnchor;

        /// <summary>
        /// Four-state software pointer. The art is 16x16, so each hotspot is given in texture pixels
        /// from the bottom-left of its sprite; edit the hotspot table if the art changes.
        /// </summary>
        public enum PointerState
        {
            Default = 0,
            Hand = 1,
            Sword = 2,
            Bow = 3
        }

        private static readonly string[] PointerPaths =
        {
            "Sprites/Pointer/pointer_default",
            "Sprites/Pointer/pointer_hand",
            "Sprites/Pointer/pointer_sword",
            "Sprites/Pointer/pointer_bow",
        };

        /// <summary>Over the tip of the arrow, the fingertip, the blade tip, the bow's grip.</summary>
        private static readonly Vector2[] PointerHotspots =
        {
            new Vector2(0f, 0f),
            new Vector2(2f, 1f),
            new Vector2(1f, 0f),
            new Vector2(8f, 8f),
        };

        private readonly Texture2D[] _pointerTextures = new Texture2D[PointerPaths.Length];
        private readonly List<RaycastResult> _pointerHits = new List<RaycastResult>();
        private PointerEventData _pointerData;
        private PointerState? _pointerApplied;
        private EnumManager.WeaponKind _weapon = EnumManager.WeaponKind.Sword;

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

        /// <summary>
        /// GDD §15: eight boxes bottom-right. Built at runtime from the Resources art so the HUD needs no
        /// scene wiring; the scene text grid stays as the fallback when an icon is missing.
        /// </summary>
        private const int SkillBoxCount = 8;
        private const int SkillColumns = 4;
        private const float SkillBoxSize = 56f;
        private const float SkillBoxGap = 6f;
        private const string SkillIconRoot = "Sprites/UI Skill Icon/UI ICON/";
        private const string LockIconPath = "Sprites/UI Skill Icon/UI Skill Lock atau off/Lock (Gembok)";
        private const string BoxFillPath = "Sprites/UI Skill Icon/UI Skill Unlocked atau On/fill";
        private const string BoxFramePath = "Sprites/UI Skill Icon/UI Skill Unlocked atau On/frame";

        /// <summary>Sword 1-4 then bow 1-4 — the same order the skill model publishes.</summary>
        private static readonly string[] SkillIconPaths =
        {
            "Sword/Skill 1 UI ICON Sword",
            "Sword/Skill 2 UI ICON Sword",
            "Sword/Skill 3 UI ICON Sword",
            "Sword/Skill 4 UI ICON Sword",
            "Bow/Skill 1 UI ICON BOW",
            "Bow/Skill 2 UI ICON Bow",
            "Bow/Skill 3 UI ICON Bow",
            "Bow/Skill 4 UI ICON Bow",
        };

        private readonly Sprite[] _skillIcons = new Sprite[SkillBoxCount];
        private readonly Image[] _skillBoxFrames = new Image[SkillBoxCount];
        private readonly Image[] _skillBoxFills = new Image[SkillBoxCount];
        private readonly Image[] _skillBoxIcons = new Image[SkillBoxCount];
        private readonly Image[] _skillBoxLocks = new Image[SkillBoxCount];
        private readonly TMP_Text[] _skillBoxTimers = new TMP_Text[SkillBoxCount];
        private Sprite _lockIcon;
        private Sprite _boxFillArt;
        private Sprite _boxFrameArt;
        private bool _skillBoxesBuilt;
        private bool _skillIconsLoaded;

        /// <summary>
        /// GDD §15 circular gauges. The scene's three sliders are horizontal bars, so the round art is
        /// drawn as its own radial overlay bottom-left and the sliders stay as the fallback.
        /// </summary>
        private const string GaugeRoot = "Sprites/UI (Health, Mana, Stamina)/";
        private const float GaugeSize = 88f;
        private const float GaugeGap = 10f;

        private static readonly string[] GaugeFramePaths =
        {
            "Health Bar/Frame (Health)",
            "Stamina Bar/Frame (Stamina)",
            "Mana Bar/Frame (Mana)",
        };

        private static readonly string[] GaugeFillPaths =
        {
            "Health Bar/Fill (Health)",
            "Stamina Bar/Fill (Stamina)",
            "Mana Bar/Fill (Mana)",
        };

        private static readonly string[] GaugeLabels = { "HP", "ST", "MP" };

        private readonly Image[] _gaugeFills = new Image[3];
        private bool _gaugesBuilt;

        private static readonly Color BoxIdle = new Color(0f, 0f, 0f, 0.55f);
        private static readonly Color BoxActive = new Color(0.2f, 0.65f, 0.3f, 0.8f);
        private static readonly Color BoxOnCooldown = new Color(0.25f, 0.12f, 0.12f, 0.7f);
        private static readonly Color IconDimmed = new Color(1f, 1f, 1f, 0.35f);
        private static readonly Color IconOffWeapon = new Color(1f, 1f, 1f, 0.55f);
        private static readonly Color IconLocked = new Color(0.35f, 0.35f, 0.35f, 0.7f);

        public void SetStats(PlayerStatsChangedMessage message)
        {
            SetBar(_healthSlider, message.Health, message.MaxHealth);
            SetBar(_staminaSlider, message.Stamina, message.MaxStamina);
            SetBar(_manaSlider, message.Mana, message.MaxMana);
            SetGauges(message.Health, message.MaxHealth, message.Stamina, message.MaxStamina,
                message.Mana, message.MaxMana);

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

        /// <summary>GDD §15: three circular gauges bottom-left, driven by the same stats message.</summary>
        private void SetGauges(float health, float maxHealth, float stamina, float maxStamina, float mana, float maxMana)
        {
            EnsureGauges();
            SetGauge(_gaugeFills[0], health, maxHealth);
            SetGauge(_gaugeFills[1], stamina, maxStamina);
            SetGauge(_gaugeFills[2], mana, maxMana);
        }

        private static void SetGauge(Image fill, float value, float max)
        {
            if (fill != null)
            {
                fill.fillAmount = max > 0f ? Mathf.Clamp01(value / max) : 0f;
            }
        }

        private void EnsureGauges()
        {
            if (_gaugesBuilt)
            {
                return;
            }

            _gaugesBuilt = true;

            bool anchored = _gaugeAnchor != null;
            Transform parent = anchored ? _gaugeAnchor : transform;

            for (int i = 0; i < 3; i++)
            {
                GameObject gauge = new GameObject("Gauge" + GaugeLabels[i], typeof(RectTransform));
                RectTransform rect = gauge.GetComponent<RectTransform>();
                rect.SetParent(parent, false);
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 0f);
                rect.pivot = new Vector2(0f, 0f);
                rect.sizeDelta = new Vector2(GaugeSize, GaugeSize);
                // Without an anchor the scene's three 300x80 sliders already sit at the bottom-left
                // corner, so the round gauges start to their right instead of landing on top of them.
                rect.anchoredPosition = anchored
                    ? new Vector2(i * (GaugeSize + GaugeGap), 0f)
                    : new Vector2(316f + i * (GaugeSize + GaugeGap), 16f);

                _gaugeFills[i] = CreateRadialFill(rect, GaugeFillPaths[i]);
                CreateOverlay(rect, "Frame", GaugeFramePaths[i]);

                TMP_Text label = CreateText(
                    "Label", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(GaugeSize, 20f), 15f);
                label.rectTransform.SetParent(rect, false);
                label.text = GaugeLabels[i];
            }
        }

        private Image CreateRadialFill(RectTransform parent, string path)
        {
            GameObject go = new GameObject("Fill", typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = go.AddComponent<Image>();
            image.sprite = LoadSprite(GaugeRoot + path);
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Radial360;
            image.fillOrigin = (int)Image.Origin360.Top;
            image.fillClockwise = true;
            image.fillAmount = 1f;
            image.raycastTarget = false;
            return image;
        }

        private Image CreateOverlay(RectTransform parent, string name, string path)
        {
            return CreateOverlay(parent, name, LoadSprite(GaugeRoot + path));
        }

        private Image CreateOverlay(RectTransform parent, string name, Sprite sprite)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
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

        /// <summary>
        /// GDD §15 fallback: when the icon art cannot be loaded the eight skills are printed as text.
        /// With icons present the boxes carry the same information, so the text grid stays hidden.
        /// </summary>
        public void SetSkillGrid(string[] lines)
        {
            LoadSkillIcons();
            if (_skillIcons[0] != null)
            {
                return;
            }

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

        /// <summary>GDD §15: eight icon boxes with lock state, cooldown countdown and the active highlight.</summary>
        public void SetSkillState(SkillStateMessage message)
        {
            EnsureSkillBoxes();
            if (_skillBoxFrames[0] == null || message.Boxes == null)
            {
                return;
            }

            for (int i = 0; i < SkillBoxCount && i < message.Boxes.Length; i++)
            {
                SkillBoxState box = message.Boxes[i];
                bool onWeapon = box.Weapon == message.Weapon;
                bool cooling = box.CooldownRemaining > 0.05f;

                _skillBoxIcons[i].sprite = _skillIcons[i];
                _skillBoxIcons[i].color = !box.Unlocked ? IconLocked
                    : cooling ? IconDimmed
                    : onWeapon ? Color.white : IconOffWeapon;

                _skillBoxLocks[i].enabled = !box.Unlocked;

                Color state = box.Active ? BoxActive
                    : cooling ? BoxOnCooldown
                    : BoxIdle;

                _skillBoxFrames[i].color = state;
                if (_skillBoxFills[i] != null)
                {
                    _skillBoxFills[i].color = state;
                }

                _skillBoxTimers[i].text = !box.Unlocked
                    ? "Lv" + box.RequiredLevel
                    : box.Active ? Mathf.CeilToInt(Mathf.Max(0f, box.ActiveRemaining)).ToString()
                    : cooling ? Mathf.CeilToInt(box.CooldownRemaining).ToString()
                    : string.Empty;
            }
        }

        private void EnsureSkillBoxes()
        {
            if (_skillBoxesBuilt)
            {
                return;
            }

            _skillBoxesBuilt = true;
            LoadSkillIcons();

            GameObject root = new GameObject("SkillBoxes", typeof(RectTransform));
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.SetParent(transform, false);
            rootRect.anchorMin = rootRect.anchorMax = new Vector2(1f, 0f);
            rootRect.pivot = new Vector2(1f, 0f);
            rootRect.anchoredPosition = new Vector2(-16f, 16f);
            rootRect.sizeDelta = new Vector2(
                SkillColumns * SkillBoxSize + (SkillColumns - 1) * SkillBoxGap,
                2f * SkillBoxSize + SkillBoxGap);

            for (int i = 0; i < SkillBoxCount; i++)
            {
                int column = i % SkillColumns;
                int row = i / SkillColumns;

                GameObject box = new GameObject("Skill" + (i + 1), typeof(RectTransform));
                RectTransform rect = box.GetComponent<RectTransform>();
                rect.SetParent(rootRect, false);
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.sizeDelta = new Vector2(SkillBoxSize, SkillBoxSize);
                rect.anchoredPosition = new Vector2(
                    column * (SkillBoxSize + SkillBoxGap),
                    -row * (SkillBoxSize + SkillBoxGap));

                _skillBoxFrames[i] = box.AddComponent<Image>();
                _skillBoxFrames[i].raycastTarget = false;
                _skillBoxFrames[i].color = BoxIdle;

                // Authored chrome: fill under the icon, frame above it. A null sprite leaves the flat
                // tint in place, so a missing art file degrades instead of breaking the box.
                if (_boxFillArt != null)
                {
                    _skillBoxFills[i] = CreateOverlay(rect, "Fill", _boxFillArt);
                }

                _skillBoxIcons[i] = CreateIcon(rect, "Icon", _skillIcons[i]);
                _skillBoxLocks[i] = CreateIcon(rect, "Lock", _lockIcon);
                _skillBoxLocks[i].enabled = false;

                if (_boxFrameArt != null)
                {
                    CreateOverlay(rect, "Frame", _boxFrameArt);
                }

                _skillBoxTimers[i] = CreateText(
                    "Timer", new Vector2(0.5f, 0f), new Vector2(0f, 2f), new Vector2(SkillBoxSize, 20f), 16f);
                _skillBoxTimers[i].rectTransform.SetParent(rect, false);
                _skillBoxTimers[i].rectTransform.pivot = new Vector2(0.5f, 0f);
                _skillBoxTimers[i].alignment = TextAlignmentOptions.Bottom;
                _skillBoxTimers[i].text = string.Empty;
            }
        }

        private void LoadSkillIcons()
        {
            if (_skillIconsLoaded)
            {
                return;
            }

            _skillIconsLoaded = true;
            _lockIcon = LoadSprite(LockIconPath);
            _boxFillArt = LoadSprite(BoxFillPath);
            _boxFrameArt = LoadSprite(BoxFramePath);

            for (int i = 0; i < SkillBoxCount; i++)
            {
                _skillIcons[i] = LoadSprite(SkillIconRoot + SkillIconPaths[i]);
            }
        }

        /// <summary>
        /// The art sits under Resources as multi-sprite sheets, so the first sub-sprite is the icon.
        /// A missing icon leaves the box empty rather than throwing during a run.
        /// </summary>
        private static Sprite LoadSprite(string path)
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>(path);
            return sprites != null && sprites.Length > 0 ? sprites[0] : null;
        }

        private Image CreateIcon(RectTransform parent, string name, Sprite sprite)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(4f, 4f);
            rect.offsetMax = new Vector2(-4f, -4f);

            Image image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        /// <summary>Which weapon is in hand, mirrored from the skill module's resolved switch.</summary>
        public void SetWeapon(EnumManager.WeaponKind weapon)
        {
            _weapon = weapon;

            if (_weaponText != null)
            {
                _weaponText.text = weapon == EnumManager.WeaponKind.Bow ? "Weapon Bow" : "Weapon Sword";
            }
        }

        /// <summary>
        /// A button wins over the weapon, and a frozen run (pause, game over) drops back to the arrow
        /// so the hand/sword cue never outlives the run it describes.
        /// </summary>
        public static PointerState ResolvePointerState(bool overButton, EnumManager.WeaponKind weapon, bool playing)
        {
            if (overButton)
            {
                return PointerState.Hand;
            }

            if (!playing)
            {
                return PointerState.Default;
            }

            return weapon == EnumManager.WeaponKind.Bow ? PointerState.Bow : PointerState.Sword;
        }

        private void Awake()
        {
            for (int i = 0; i < PointerPaths.Length; i++)
            {
                _pointerTextures[i] = Resources.Load<Texture2D>(PointerPaths[i]);
                if (_pointerTextures[i] == null)
                {
                    Debug.LogWarning($"[HudView] Pointer texture missing: Resources/{PointerPaths[i]}");
                }
            }
        }

        /// <summary>
        /// The hand is decided by a UI raycast against any interactable Selectable, so every authored
        /// button gets it without per-button wiring.
        /// </summary>
        private void UpdatePointer()
        {
            PointerState next = ResolvePointerState(IsOverButton(), _weapon, Time.timeScale > 0f);
            if (next == _pointerApplied)
            {
                return;
            }

            _pointerApplied = next;
            Cursor.SetCursor(_pointerTextures[(int)next], PointerHotspots[(int)next], CursorMode.Auto);
        }

        private bool IsOverButton()
        {
            if (EventSystem.current == null || Mouse.current == null)
            {
                return false;
            }

            if (_pointerData == null)
            {
                _pointerData = new PointerEventData(EventSystem.current);
            }

            _pointerData.position = Mouse.current.position.ReadValue();
            _pointerHits.Clear();
            EventSystem.current.RaycastAll(_pointerData, _pointerHits);

            foreach (RaycastResult hit in _pointerHits)
            {
                Selectable selectable = hit.gameObject.GetComponentInParent<Selectable>();
                if (selectable != null && selectable.IsInteractable())
                {
                    return true;
                }
            }

            return false;
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
            UpdatePointer();

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
