using UnityEngine;

namespace ProjectTA.Utility
{
    /// <summary>
    /// World-space bar that rides above one entity and stays hidden until that entity loses health.
    /// The prefab wires the frame and the fill; the fill sprite is left-pivoted, so scaling its x
    /// shrinks the bar towards its left edge instead of its middle.
    /// </summary>
    public class HealthBarView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _frame;
        [SerializeField] private SpriteRenderer _fill;

        /// <summary>Height above the owner's origin, in the owner's own scale.</summary>
        [SerializeField] private float _heightFactor = 0.6f;

        /// <summary>Bar size relative to the art's imported width, so one prefab fits both kinds.</summary>
        [SerializeField] private float _worldScale = 1f;

        private Vector3 _fillScale;
        private bool _ready;

        private void Awake()
        {
            if (_fill != null)
            {
                _fillScale = _fill.transform.localScale;
            }

            Show(1f);
        }

        /// <summary>
        /// Parents the bar to the entity and keeps its world size fixed, so a monster authored at one
        /// unit and a tree scaled to 2.5 both carry the same bar.
        /// </summary>
        public void Init(Transform owner, float ownerScale)
        {
            if (owner == null || _frame == null || _fill == null)
            {
                Debug.LogError("[HealthBar] Frame or fill is not wired on the bar prefab.");
                return;
            }

            _fillScale = _fill.transform.localScale;

            float k = (ownerScale > 0.0001f ? 1f / ownerScale : 1f) * Mathf.Max(0.01f, _worldScale);
            transform.SetParent(owner, false);
            transform.localScale = Vector3.one * k;

            float width = _frame.sprite != null ? _frame.sprite.bounds.size.x : 0f;
            transform.position = owner.position + new Vector3(-0.5f * width * k, _heightFactor * ownerScale, 0f);

            _ready = true;
            Show(1f);
        }

        /// <summary>Ratios at or above 1 mean "untouched": the bar hides itself.</summary>
        public void Show(float normalized)
        {
            float value = Mathf.Clamp01(normalized);
            bool visible = _ready && value < 1f;

            if (_frame != null)
            {
                _frame.enabled = visible;
            }

            if (_fill == null)
            {
                return;
            }

            _fill.enabled = visible;
            _fill.transform.localScale = new Vector3(_fillScale.x * value, _fillScale.y, _fillScale.z);
        }
    }
}
