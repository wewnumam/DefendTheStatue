using Agate.MVC.Base;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ProjectTA.Module.Build
{
    /// <summary>
    /// Read-only view: reports that the pawn pressed the interact key and floats the reference game's
    /// golden "E" prompt over whatever the pawn can build. No build menu — every rule and price lives
    /// in the controller.
    /// ponytail: raw keyboard reads, because there is no authored build UI. Route E and F through
    /// InputController if they ever need rebinding.
    /// </summary>
    public class BuildView : BaseView
    {
        [Header("God statue")]
        [SerializeField] private Transform _godStatue;
        [SerializeField] private float _statuePickRadius = 150f;

        [Header("Interact prompt")]
        /// <summary>World units above the target. The reference game uses 40 px; 1 unit is ~45 px here.</summary>
        [SerializeField] private float _promptOffsetY = 1f;
        [SerializeField] private Color _promptColor = new Color(1f, 0.843f, 0f);

        private UnityAction _onInteract;
        private UnityAction _onUpgrade;
        private UnityAction<float> _onTick;

        private GameObject _promptRoot;
        private TMP_Text _promptLabel;
        private Image _promptLabelBack;

        /// <summary>
        /// E is one context key: next to the god statue it opens the upgrade panel, next to an empty
        /// build slot it spends essence. The statue radius is authored here because the art scale is
        /// not a game constant.
        /// </summary>
        public bool HasStatue => _godStatue != null;
        public Vector2 StatuePosition => _godStatue != null ? (Vector2)_godStatue.position : Vector2.zero;
        public float StatuePickRadius => _statuePickRadius;

        public void SetCallbacks(UnityAction onInteract, UnityAction onUpgrade)
        {
            _onInteract = onInteract;
            _onUpgrade = onUpgrade;
        }

        /// <summary>Per-frame hook: the controller decides whether anything is in reach.</summary>
        public void SetTickCallback(UnityAction<float> onTick)
        {
            _onTick = onTick;
        }

        /// <summary>Gold "E" over the target, with the price under it. An empty label hides the tag.</summary>
        public void ShowPrompt(Vector2 position, string label)
        {
            EnsurePrompt();
            if (_promptRoot == null)
            {
                return;
            }

            _promptRoot.SetActive(true);
            float bob = Mathf.Sin(Time.unscaledTime * 3.5f) * 0.06f;
            _promptRoot.transform.localPosition = new Vector3(position.x, position.y + _promptOffsetY + bob, 0f);

            bool hasLabel = !string.IsNullOrEmpty(label);
            if (_promptLabel != null)
            {
                _promptLabel.gameObject.SetActive(hasLabel);
                _promptLabel.text = hasLabel ? label : string.Empty;
            }

            if (_promptLabelBack != null)
            {
                _promptLabelBack.gameObject.SetActive(hasLabel);
            }
        }

        public void HidePrompt()
        {
            if (_promptRoot != null && _promptRoot.activeSelf)
            {
                _promptRoot.SetActive(false);
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
                _onInteract?.Invoke();
            }

            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                _onUpgrade?.Invoke();
            }

            _onTick?.Invoke(Time.deltaTime);
        }

        /// <summary>
        /// World-space canvas built at runtime: the HUD canvas is authored, and adding a floating
        /// world prompt to the scene file by hand is the fragile option.
        /// </summary>
        private void EnsurePrompt()
        {
            if (_promptRoot != null)
            {
                return;
            }

            _promptRoot = new GameObject("InteractPrompt", typeof(RectTransform), typeof(Canvas));
            RectTransform root = _promptRoot.GetComponent<RectTransform>();
            root.SetParent(transform, false);
            root.sizeDelta = new Vector2(80f, 80f);
            root.localScale = Vector3.one * 0.01f;

            Canvas canvas = _promptRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;

            CreateBox(root, "Border", new Vector2(84f, 84f), _promptColor);
            CreateBox(root, "Fill", new Vector2(76f, 76f), new Color(0f, 0f, 0f, 0.85f));

            TMP_Text key = CreateText(root, "Key", "E", 56f, _promptColor);
            key.rectTransform.sizeDelta = new Vector2(80f, 80f);

            _promptLabelBack = CreateBox(root, "LabelBack", new Vector2(200f, 34f), new Color(0f, 0f, 0f, 0.85f));
            _promptLabelBack.rectTransform.anchoredPosition = new Vector2(0f, -58f);

            _promptLabel = CreateText(root, "Label", string.Empty, 28f, _promptColor);
            _promptLabel.rectTransform.sizeDelta = new Vector2(200f, 34f);
            _promptLabel.rectTransform.anchoredPosition = new Vector2(0f, -58f);

            _promptRoot.SetActive(false);
        }

        private static Image CreateBox(RectTransform parent, string name, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;

            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static TMP_Text CreateText(RectTransform parent, string name, string text, float fontSize, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(80f, 80f);

            TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
            label.font = TMP_Settings.defaultFontAsset;
            label.fontSize = fontSize;
            label.color = color;
            label.text = text;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            return label;
        }
    }
}
