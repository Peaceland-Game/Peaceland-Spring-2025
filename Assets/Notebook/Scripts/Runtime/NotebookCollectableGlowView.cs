using UnityEngine;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Simple pulsing highlight for world collectibles (flowers, items, etc.).
    /// </summary>
    public class NotebookCollectableGlowView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private Color glowColor = new Color(1f, 0.92f, 0.45f, 0.85f);
        [SerializeField] private float pulseSpeed = 2.4f;
        [SerializeField] private float minAlpha = 0.25f;
        [SerializeField] private float maxAlpha = 0.95f;
        [SerializeField] private Vector3 glowScale = new Vector3(1.18f, 1.18f, 1f);

        private Transform glowTransform;
        private SpriteRenderer glowRenderer;

        private void Awake()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<SpriteRenderer>();
            }

            EnsureGlowChild();
        }

        private void Update()
        {
            if (glowRenderer == null)
            {
                return;
            }

            float wave = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            Color color = glowColor;
            color.a = Mathf.Lerp(minAlpha, maxAlpha, wave);
            glowRenderer.color = color;
        }

        private void EnsureGlowChild()
        {
            if (glowTransform != null)
            {
                return;
            }

            GameObject glowObject = new GameObject("Collect Glow", typeof(SpriteRenderer));
            glowObject.transform.SetParent(transform, false);
            glowObject.transform.localPosition = Vector3.zero;
            glowObject.transform.localScale = glowScale;
            glowRenderer = glowObject.GetComponent<SpriteRenderer>();
            glowRenderer.sprite = targetRenderer != null ? targetRenderer.sprite : null;
            glowRenderer.sortingLayerID = targetRenderer != null ? targetRenderer.sortingLayerID : 0;
            glowRenderer.sortingOrder = targetRenderer != null ? targetRenderer.sortingOrder - 1 : -1;
            glowTransform = glowObject.transform;
        }
    }
}
