using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Displays a popup modal window enlarging the clicked artifact card alongside detailed info.
    /// </summary>
    public class NotebookArtifactDetailModal : MonoBehaviour
    {
        [Header("Window Roots")]
        [Tooltip("Root GameObject of the modal containing backdrop and dialog.")]
        [SerializeField] private GameObject modalRoot;
        [Tooltip("CanvasGroup on the modal root for fade transitions.")]
        [SerializeField] private CanvasGroup canvasGroup;
        [Tooltip("Dialog box RectTransform for punch / scale animation.")]
        [SerializeField] private RectTransform dialogBox;
        [SerializeField] private float animationDuration = 0.15f;

        [Header("Close Buttons")]
        [Tooltip("Clicking backdrop / outside the dialog box dismisses the modal.")]
        [SerializeField] private UnityEngine.UI.Button backdropButton;
        [Tooltip("Close button (e.g. 'X') on the dialog box.")]
        [SerializeField] private UnityEngine.UI.Button closeButton;
        [Tooltip("Optional secondary close button at the bottom of the info panel.")]
        [SerializeField] private UnityEngine.UI.Button bottomCloseButton;

        [Header("Enlarged Artifact Card (Left Column)")]
        [SerializeField] private UnityEngine.UI.Image cardBackground;
        [SerializeField] private UnityEngine.UI.Image pedestalBackground;
        [SerializeField] private UnityEngine.UI.Image artifactIcon;
        [SerializeField] private TMP_Text cardBadgeText;

        [Header("Artifact Info (Right Column)")]
        [SerializeField] private TMP_Text categoryChapterText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text statusHintText;

        private ArtifactDefinition currentArtifact;
        private Coroutine transitionCoroutine;

        public bool IsShowing => modalRoot != null && modalRoot.activeSelf;
        public ArtifactDefinition CurrentArtifact => currentArtifact;

        private void Awake()
        {
            if (backdropButton != null)
            {
                backdropButton.onClick.RemoveAllListeners();
                backdropButton.onClick.AddListener(() => Hide());
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() => Hide());
            }

            if (bottomCloseButton != null)
            {
                bottomCloseButton.onClick.RemoveAllListeners();
                bottomCloseButton.onClick.AddListener(() => Hide());
            }
        }

        private void Start()
        {
            if (modalRoot != null && !IsShowing)
            {
                modalRoot.SetActive(false);
            }
        }

        public void Show(ArtifactDefinition artifact)
        {
            if (artifact == null) return;
            currentArtifact = artifact;

            PopulateArtifactData(artifact);

            if (modalRoot != null)
            {
                modalRoot.SetActive(true);
            }

            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }
            transitionCoroutine = StartCoroutine(AnimateOpenRoutine());
        }

        public void Hide(bool immediate = false)
        {
            if (!IsShowing) return;

            if (immediate)
            {
                if (transitionCoroutine != null)
                {
                    StopCoroutine(transitionCoroutine);
                }
                if (modalRoot != null)
                {
                    modalRoot.SetActive(false);
                }
                return;
            }

            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }
            transitionCoroutine = StartCoroutine(AnimateCloseRoutine());
        }

        private void PopulateArtifactData(ArtifactDefinition artifact)
        {
            // Category & Chapter Header
            if (categoryChapterText != null)
            {
                string cat = string.IsNullOrEmpty(artifact.Category) ? "MEMORIES" : artifact.Category.ToUpper();
                string chap = string.IsNullOrEmpty(artifact.Chapter) ? "General" : artifact.Chapter;
                categoryChapterText.text = $"{cat}  •  {chap.ToUpper()}";
            }

            // Title
            if (titleText != null)
            {
                titleText.text = artifact.Title;
            }

            // Description
            if (descriptionText != null)
            {
                descriptionText.text = artifact.Description;
            }

            // Status Note
            if (statusHintText != null)
            {
                statusHintText.text = "Archived in Journal  •  Press ESC or click outside to return";
            }

            // Card Badge
            if (cardBadgeText != null)
            {
                cardBadgeText.text = artifact.Chapter;
            }

            // Icon
            if (artifactIcon != null)
            {
                if (artifact.Icon != null)
                {
                    artifactIcon.sprite = artifact.Icon;
                    artifactIcon.enabled = true;
                    artifactIcon.preserveAspect = true;
                }
                else
                {
                    artifactIcon.enabled = false;
                }
            }
        }

        private IEnumerator AnimateOpenRoutine()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = true;
            }

            if (dialogBox != null)
            {
                dialogBox.localScale = Vector3.one * 0.92f;
            }

            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / animationDuration);
                float ease = 1f - Mathf.Pow(1f - t, 3f);

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = ease;
                }
                if (dialogBox != null)
                {
                    dialogBox.localScale = Vector3.LerpUnclamped(Vector3.one * 0.92f, Vector3.one, ease);
                }
                yield return null;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            if (dialogBox != null)
            {
                dialogBox.localScale = Vector3.one;
            }

            transitionCoroutine = null;
        }

        private IEnumerator AnimateCloseRoutine()
        {
            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
            }

            float duration = animationDuration * 0.8f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = t * t;

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f - ease;
                }
                if (dialogBox != null)
                {
                    dialogBox.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.94f, ease);
                }
                yield return null;
            }

            if (modalRoot != null)
            {
                modalRoot.SetActive(false);
            }
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
            }

            transitionCoroutine = null;
        }
    }
}
