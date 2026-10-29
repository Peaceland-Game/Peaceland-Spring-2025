using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Attach to any clickable object in the scene to make it a collectable artifact.
    /// Supports uniform pedestal base, standardized sizing, and mouse/pointer interactions.
    /// </summary>
    [SelectionBase]
    public class ClickableArtifact : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Artifact Data")]
        [Tooltip("The artifact definition asset associated with this object.")]
        [SerializeField] private ArtifactDefinition artifact;

        [Header("Visual Presentation")]
        [Tooltip("The pedestal / glow circular backing sprite (matches the reference artwork).")]
        [SerializeField] private Sprite pedestalSprite;
        [Tooltip("Fixed diameter of the circular base in world units for uniform sizing.")]
        [SerializeField] private float uniformBaseDiameter = 2.0f;
        [Tooltip("Max size of the artifact icon within the base.")]
        [SerializeField] private float maxItemSize = 1.6f;

        [Header("Interaction Settings")]
        [Tooltip("Prompt shown on hover (e.g. 'Click to inspect').")]
        [SerializeField] private string hoverPrompt = "Click to inspect";
        [Tooltip("If true, this GameObject is deactivated after being collected.")]
        [SerializeField] private bool disableOnCollect = true;
        [Tooltip("Optional hover highlight scale factor.")]
        [SerializeField] private float hoverScale = 1.12f;
        [Tooltip("Highlight color tint on hover.")]
        [SerializeField] private Color hoverColor = new Color(1f, 0.98f, 0.88f, 1f);

        [Header("Events")]
        public UnityEvent onCollected;

        private Transform visualRoot;
        private SpriteRenderer pedestalRenderer;
        private SpriteRenderer itemRenderer;
        private Vector3 initialScale;
        private Color initialItemColor = Color.white;
        private bool isHovered;
        private bool isCollected;

        public ArtifactDefinition Artifact => artifact;
        public string HoverPrompt => hoverPrompt;

        private void Awake()
        {
            SetupHierarchyAndSizing();
            initialScale = transform.localScale;
        }

        private void Start()
        {
            if (NotebookJournalManager.Instance != null && artifact != null)
            {
                if (NotebookJournalManager.Instance.IsArtifactCollected(artifact.Id))
                {
                    isCollected = true;
                    if (disableOnCollect)
                    {
                        gameObject.SetActive(false);
                    }
                }
            }
        }

        public void SetupHierarchyAndSizing()
        {
            Transform existingVisual = transform.Find("Visual");
            if (existingVisual == null)
            {
                GameObject visObj = new GameObject("Visual");
                visObj.transform.SetParent(transform, false);
                visualRoot = visObj.transform;
            }
            else
            {
                visualRoot = existingVisual;
            }

            // Pedestal Background
            Transform pedObj = visualRoot.Find("Pedestal");
            if (pedObj == null)
            {
                GameObject p = new GameObject("Pedestal", typeof(SpriteRenderer));
                p.transform.SetParent(visualRoot, false);
                pedObj = p.transform;
            }
            pedestalRenderer = pedObj.GetComponent<SpriteRenderer>();
            pedestalRenderer.sortingOrder = 5;

            if (pedestalSprite != null)
            {
                pedestalRenderer.sprite = pedestalSprite;
                float spriteW = pedestalSprite.rect.width / pedestalSprite.pixelsPerUnit;
                if (spriteW > 0.001f)
                {
                    float pedScale = uniformBaseDiameter / spriteW;
                    pedObj.localScale = new Vector3(pedScale, pedScale, 1f);
                }
            }

            // Item Foreground
            Transform itemObj = visualRoot.Find("Item");
            if (itemObj == null)
            {
                GameObject it = new GameObject("Item", typeof(SpriteRenderer));
                it.transform.SetParent(visualRoot, false);
                itemObj = it.transform;
            }
            itemRenderer = itemObj.GetComponent<SpriteRenderer>();
            itemRenderer.sortingOrder = 6;

            // Remove any SpriteRenderer on root to avoid duplicate rendering
            SpriteRenderer rootSr = GetComponent<SpriteRenderer>();
            if (rootSr != null)
            {
                if (artifact != null && artifact.Icon != null && itemRenderer.sprite == null)
                {
                    itemRenderer.sprite = artifact.Icon;
                }
                else if (rootSr.sprite != null && itemRenderer.sprite == null)
                {
                    itemRenderer.sprite = rootSr.sprite;
                }
                if (Application.isPlaying)
                {
                    Destroy(rootSr);
                }
                else
                {
                    DestroyImmediate(rootSr);
                }
            }

            if (artifact != null && artifact.Icon != null)
            {
                itemRenderer.sprite = artifact.Icon;
            }

            // Normalize item size so all artifacts are the exact same standardized size
            if (itemRenderer.sprite != null)
            {
                Sprite s = itemRenderer.sprite;
                float width = s.rect.width / s.pixelsPerUnit;
                float height = s.rect.height / s.pixelsPerUnit;
                float maxDim = Mathf.Max(width, height);
                if (maxDim > 0.001f)
                {
                    float targetScale = maxItemSize / maxDim;
                    itemObj.localScale = new Vector3(targetScale, targetScale, 1f);
                }
                initialItemColor = itemRenderer.color;
            }

            // Standardize collider size to the uniform circular pedestal
            CircleCollider2D col = GetComponent<CircleCollider2D>();
            if (col == null)
            {
                BoxCollider2D boxCol = GetComponent<BoxCollider2D>();
                if (boxCol != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(boxCol);
                    }
                    else
                    {
                        DestroyImmediate(boxCol);
                    }
                }
                col = gameObject.AddComponent<CircleCollider2D>();
            }
            col.radius = uniformBaseDiameter * 0.5f;
        }

        public void SetArtifact(ArtifactDefinition newArtifact)
        {
            artifact = newArtifact;
            SetupHierarchyAndSizing();
        }

        public void SetPedestalSprite(Sprite newPedestal)
        {
            pedestalSprite = newPedestal;
            SetupHierarchyAndSizing();
        }

        public void Collect()
        {
            if (isCollected && disableOnCollect)
            {
                return;
            }

            if (artifact == null)
            {
                Debug.LogWarning($"[ClickableArtifact] No artifact assigned on '{gameObject.name}'.", this);
                return;
            }

            isCollected = true;

            if (NotebookJournalManager.Instance != null)
            {
                NotebookJournalManager.Instance.CollectArtifact(artifact);
            }

            onCollected?.Invoke();

            if (disableOnCollect)
            {
                StartCoroutine(CollectAnimationRoutine());
            }
        }

        private IEnumerator CollectAnimationRoutine()
        {
            float elapsed = 0f;
            float duration = 0.28f;
            Vector3 startScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float bounce = 1f + Mathf.Sin(t * Mathf.PI) * 0.25f;
                transform.localScale = startScale * bounce;

                float alpha = 1f - t;
                if (pedestalRenderer != null)
                {
                    Color c = Color.white;
                    c.a = alpha;
                    pedestalRenderer.color = c;
                }
                if (itemRenderer != null)
                {
                    Color c = initialItemColor;
                    c.a = alpha;
                    itemRenderer.color = c;
                }
                yield return null;
            }

            gameObject.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Collect();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetHover(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetHover(false);
        }

        private void SetHover(bool hover)
        {
            isHovered = hover;
            transform.localScale = hover ? initialScale * hoverScale : initialScale;

            if (pedestalRenderer != null)
            {
                pedestalRenderer.color = hover ? hoverColor : Color.white;
            }
            if (itemRenderer != null)
            {
                itemRenderer.color = hover ? hoverColor : initialItemColor;
            }

            if (NotebookJournalManager.Instance != null && artifact != null)
            {
                NotebookJournalManager.Instance.SetWorldHoverHint(hover ? $"{artifact.Title} ({hoverPrompt})" : null);
            }
        }

        private void OnDisable()
        {
            if (isHovered && NotebookJournalManager.Instance != null)
            {
                NotebookJournalManager.Instance.SetWorldHoverHint(null);
            }
        }
    }
}
