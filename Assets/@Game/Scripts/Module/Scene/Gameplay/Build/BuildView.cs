using Agate.MVC.Base;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ProjectTA.Module.Build
{
    /// <summary>
    /// Read-only view: reports where the cursor is and when the player asked to build or upgrade.
    /// The buy menu is built at runtime so no scene wiring is needed for it.
    /// ponytail: raw keyboard reads, because there is no authored build menu. Route E and F through
    /// InputController once the menu lands in the scene.
    /// </summary>
    public class BuildView : BaseView
    {
        [SerializeField] private Camera _camera;

        [Header("God statue")]
        [SerializeField] private Transform _godStatue;
        [SerializeField] private float _statuePickRadius = 150f;

        [Header("Buy menu")]
        [SerializeField] private bool _showBuyMenu = true;

        private static readonly BuildKind[] MenuKinds =
        {
            BuildKind.Tower, BuildKind.Wall, BuildKind.Guard, BuildKind.Trap
        };

        private UnityAction<Vector2> _onBuild;
        private UnityAction _onUpgrade;
        private UnityAction<BuildKind> _onBuildKind;

        private GameObject _menuRoot;
        private TMP_Text _menuHint;
        private readonly Button[] _menuButtons = new Button[MenuKinds.Length];

        /// <summary>
        /// E is one context key: over the god statue it opens the skill upgrade interface, anywhere
        /// else it builds. The radius is authored here because the art scale is not a game constant.
        /// </summary>
        public bool IsOverStatue(Vector2 worldPosition)
        {
            if (_godStatue == null)
            {
                return false;
            }

            return ((Vector2)_godStatue.position - worldPosition).sqrMagnitude
                <= _statuePickRadius * _statuePickRadius;
        }

        public Vector2 CursorPosition
        {
            get
            {
                Camera camera = _camera != null ? _camera : Camera.main;
                if (camera == null || Mouse.current == null)
                {
                    return Vector2.zero;
                }

                return camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            }
        }

        public void SetCallbacks(UnityAction<Vector2> onBuild, UnityAction onUpgrade)
        {
            _onBuild = onBuild;
            _onUpgrade = onUpgrade;
        }

        public void SetBuildKindCallback(UnityAction<BuildKind> onBuildKind)
        {
            _onBuildKind = onBuildKind;
        }

        /// <summary>Hidden until the run starts so the buy menu never floats over the pre-game screen.</summary>
        public void SetBuyMenuVisible(bool visible)
        {
            if (visible)
            {
                EnsureBuyMenu();
            }

            if (_menuRoot != null)
            {
                _menuRoot.SetActive(visible);
            }
        }

        /// <summary>Greys out whatever the player cannot afford. Called from the controller.</summary>
        public void RefreshBuyMenu(int essence, bool freeBuild, int statueLevel)
        {
            EnsureBuyMenu();
            if (_menuRoot == null)
            {
                return;
            }

            if (_menuHint != null)
            {
                _menuHint.text = $"Statue Lv {statueLevel} — {essence} essence";
            }

            for (int i = 0; i < _menuButtons.Length; i++)
            {
                if (_menuButtons[i] != null)
                {
                    _menuButtons[i].interactable = freeBuild || essence >= BuildRules.CostOf(MenuKinds[i]);
                }
            }
        }

        private void Update()
        {
            // Build and upgrade land instantly, so they must not fire while the world is frozen.
            if (Time.timeScale <= 0f || Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                _onBuild?.Invoke(CursorPosition);
            }

            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                _onUpgrade?.Invoke();
            }
        }

        /// <summary>
        /// GDD §12 buy list. Built at runtime because the HUD canvas is authored and adding a panel
        /// to the scene file by hand is the fragile option.
        /// </summary>
        private void EnsureBuyMenu()
        {
            if (_menuRoot != null || !_showBuyMenu)
            {
                return;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            }

            if (canvas == null)
            {
                return;
            }

            _menuRoot = new GameObject("BuildMenu", typeof(RectTransform));
            RectTransform root = _menuRoot.GetComponent<RectTransform>();
            root.SetParent(canvas.transform, false);
            root.anchorMin = root.anchorMax = new Vector2(0f, 0.5f);
            root.pivot = new Vector2(0f, 0.5f);
            root.anchoredPosition = new Vector2(16f, -40f);
            root.sizeDelta = new Vector2(180f, 32f);

            VerticalLayoutGroup layout = _menuRoot.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 4f;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            ContentSizeFitter fitter = _menuRoot.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _menuHint = CreateLabel(root, "Hint");
            _menuHint.fontSize = 14f;
            _menuHint.text = "Statue Lv 1 — 0 essence";
            _menuHint.gameObject.AddComponent<LayoutElement>().preferredHeight = 20f;

            for (int i = 0; i < MenuKinds.Length; i++)
            {
                _menuButtons[i] = CreateButton(root, MenuKinds[i]);
            }

            // Stays hidden until the run starts; SetBuyMenuVisible(true) turns it on.
            _menuRoot.SetActive(false);
        }

        private Button CreateButton(RectTransform parent, BuildKind kind)
        {
            GameObject row = new GameObject(kind + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform rect = row.GetComponent<RectTransform>();
            rect.SetParent(parent, false);

            Image background = row.GetComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.55f);

            Button button = row.GetComponent<Button>();
            button.targetGraphic = background;
            BuildKind captured = kind;
            button.onClick.AddListener(() => _onBuildKind?.Invoke(captured));

            LayoutElement element = row.AddComponent<LayoutElement>();
            element.preferredHeight = 30f;

            TMP_Text text = CreateLabel(rect, "Label");
            text.text = $"{kind} — {BuildRules.CostOf(kind)} essence";

            return button;
        }

        private static TMP_Text CreateLabel(RectTransform parent, string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(8f, 0f);
            rect.offsetMax = new Vector2(-8f, 0f);

            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = 15f;
            text.alignment = TextAlignmentOptions.Left;
            text.raycastTarget = false;
            return text;
        }
    }
}
