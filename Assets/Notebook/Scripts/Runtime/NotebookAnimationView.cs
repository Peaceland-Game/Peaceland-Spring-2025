using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    public class NotebookAnimationView : MonoBehaviour
    {
        [SerializeField] private Image animationImage;
        [SerializeField] private List<Sprite> openFrames = new List<Sprite>();
        [SerializeField] private float frameDuration = 0.08f;
        [SerializeField] private float openStartScale = 0.38f;
        [SerializeField] private float openEndScale = 1f;
        [SerializeField] private AnimationCurve openScaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public bool HasFrames => animationImage != null && openFrames.Count > 0;

        public void Configure(Image targetAnimationImage, IEnumerable<Sprite> frames = null)
        {
            animationImage = targetAnimationImage;
            if (frames != null)
            {
                openFrames = frames.Where(frame => frame != null).ToList();
            }

            if (animationImage != null)
            {
                animationImage.preserveAspect = true;
            }
        }

        public void SetFrames(IEnumerable<Sprite> frames)
        {
            openFrames = frames != null
                ? frames.Where(frame => frame != null).ToList()
                : new List<Sprite>();
        }

        public void PrepareForOpen()
        {
            if (animationImage == null)
            {
                return;
            }

            animationImage.preserveAspect = true;
            animationImage.rectTransform.localScale = Vector3.one * openStartScale;
            transform.SetAsLastSibling();
        }

        public IEnumerator PlayOpen()
        {
            if (!HasFrames)
            {
                yield break;
            }

            PrepareForOpen();
            SetOverlayVisible(true);

            float totalDuration = Mathf.Max(openFrames.Count * frameDuration, frameDuration);
            float elapsed = 0f;
            int frameIndex = 0;
            animationImage.sprite = openFrames[0];

            while (elapsed < totalDuration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / totalDuration);
                float scale = Mathf.Lerp(openStartScale, openEndScale, openScaleCurve.Evaluate(normalizedTime));
                animationImage.rectTransform.localScale = Vector3.one * scale;

                int targetFrame = Mathf.Min(
                    openFrames.Count - 1,
                    Mathf.FloorToInt(elapsed / frameDuration));
                if (targetFrame != frameIndex)
                {
                    frameIndex = targetFrame;
                    animationImage.sprite = openFrames[frameIndex];
                }

                yield return null;
            }

            animationImage.sprite = openFrames[openFrames.Count - 1];
            animationImage.rectTransform.localScale = Vector3.one * openEndScale;
        }

        public void SetClosedFrame()
        {
            if (HasFrames)
            {
                animationImage.sprite = openFrames[0];
            }

            ResetPresentation();
        }

        public void SetOpenFrame()
        {
            if (HasFrames)
            {
                animationImage.sprite = openFrames[openFrames.Count - 1];
                animationImage.rectTransform.localScale = Vector3.one * openEndScale;
            }

            SetOverlayVisible(false);
        }

        public void ResetPresentation()
        {
            if (animationImage != null)
            {
                animationImage.rectTransform.localScale = Vector3.one * openStartScale;
            }

            SetOverlayVisible(false);
        }

        public void SetOverlayVisible(bool visible)
        {
            if (animationImage == null)
            {
                return;
            }

            if (visible)
            {
                transform.SetAsLastSibling();
            }

            animationImage.raycastTarget = visible;
            animationImage.enabled = visible;
            gameObject.SetActive(visible);
        }
    }
}
