using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    public class NotebookEntryView : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Image entryImage;
        [SerializeField] private GameObject imageRoot;
        [SerializeField] private GameObject newBadgeRoot;
        [SerializeField] private GameObject highlightRoot;
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private NotebookEntryCellLayout cellLayout;

        private RectTransform recordChoiceRoot;

        public void Configure(
            TMP_Text targetTitleText,
            TMP_Text targetBodyText,
            Image targetEntryImage,
            GameObject targetImageRoot,
            GameObject targetNewBadgeRoot,
            GameObject targetHighlightRoot,
            LayoutElement targetLayoutElement)
        {
            titleText = targetTitleText;
            bodyText = targetBodyText;
            entryImage = targetEntryImage;
            imageRoot = targetImageRoot;
            newBadgeRoot = targetNewBadgeRoot;
            highlightRoot = targetHighlightRoot;
            layoutElement = targetLayoutElement;
            cellLayout = GetComponent<NotebookEntryCellLayout>();
        }

        public void ConfigureFromHierarchy()
        {
            Transform root = transform;
            TMP_Text targetTitleText = root.Find("Title")?.GetComponent<TMP_Text>();
            TMP_Text targetBodyText = root.Find("Content Row/Body")?.GetComponent<TMP_Text>()
                ?? root.Find("Body")?.GetComponent<TMP_Text>();
            Transform targetImageRoot = root.Find("Content Row/Image Root")
                ?? root.Find("Image Root");
            Image targetEntryImage = targetImageRoot?.Find("Image")?.GetComponent<Image>();

            Configure(
                targetTitleText,
                targetBodyText,
                targetEntryImage,
                targetImageRoot?.gameObject,
                root.Find("New Badge")?.gameObject,
                root.Find("Highlight")?.gameObject,
                GetComponent<LayoutElement>());
        }

        public void Bind(
            NotebookEntryDefinition definition,
            bool isNew,
            Action<string> onViewed = null,
            float layoutHeightOverride = 0f,
            bool recordChoiceCompleted = false,
            int selectedRecordChoiceIndex = -1,
            Action<string, int> onRecordChoiceSelected = null)
        {
            if (definition == null)
            {
                return;
            }

            gameObject.SetActive(true);

            if (titleText != null)
            {
                titleText.text = definition.Title;
            }

            if (bodyText != null)
            {
                bodyText.text = definition.BodyText;
            }

            bool hasImage = definition.Image != null;

            if (entryImage != null)
            {
                entryImage.sprite = definition.Image;
                entryImage.enabled = hasImage;
                entryImage.preserveAspect = true;
            }

            if (imageRoot != null)
            {
                imageRoot.SetActive(hasImage);
            }

            if (newBadgeRoot != null)
            {
                newBadgeRoot.SetActive(isNew);
            }

            if (highlightRoot != null)
            {
                highlightRoot.SetActive(isNew);
            }

            float choiceHeight = BindRecordChoice(
                definition,
                recordChoiceCompleted,
                selectedRecordChoiceIndex,
                onRecordChoiceSelected);

            if (layoutElement != null)
            {
                float preferredHeight = layoutHeightOverride > 0f
                    ? layoutHeightOverride
                    : definition.LayoutHeight + choiceHeight;
                float minimumHeight = cellLayout != null ? cellLayout.MinimumHeight : 1f;
                layoutElement.preferredHeight = Mathf.Max(minimumHeight, preferredHeight);
            }

            if (!definition.RequiresRecordChoice || recordChoiceCompleted)
            {
                onViewed?.Invoke(definition.EntryId);
            }
        }

        private float BindRecordChoice(
            NotebookEntryDefinition definition,
            bool completed,
            int selectedIndex,
            Action<string, int> onRecordChoiceSelected)
        {
            EnsureRecordChoiceRoot();
            ClearRecordChoiceRoot();

            if (!definition.RequiresRecordChoice || recordChoiceRoot == null)
            {
                if (recordChoiceRoot != null)
                {
                    recordChoiceRoot.gameObject.SetActive(false);
                }

                return 0f;
            }

            recordChoiceRoot.gameObject.SetActive(true);
            float promptHeight = cellLayout != null ? cellLayout.ChoicePromptHeight : 28f;
            float recordedHeight = cellLayout != null ? cellLayout.ChoiceRecordedHeight : 28f;
            float buttonHeight = cellLayout != null ? cellLayout.ChoiceButtonHeight : 38f;
            CreateRecordChoiceText("Record Prompt", definition.RecordChoicePrompt, 17f, FontStyles.Bold, promptHeight);

            IReadOnlyList<NotebookEntryDefinition.RecordChoice> choices = definition.RecordChoices;
            if (completed)
            {
                string selectedLabel = GetSelectedChoiceLabel(choices, selectedIndex);
                CreateRecordChoiceText("Recorded Choice", "Recorded: " + selectedLabel, 15f, FontStyles.Italic, recordedHeight);
                return cellLayout != null ? cellLayout.MeasureChoiceHeight(choices.Count, true) : 64f;
            }

            int count = Mathf.Min(3, choices.Count);
            for (int i = 0; i < count; i++)
            {
                NotebookEntryDefinition.RecordChoice choice = choices[i];
                if (choice == null)
                {
                    continue;
                }

                CreateRecordChoiceButton(definition.EntryId, i, choice.Label, onRecordChoiceSelected, buttonHeight);
            }

            return cellLayout != null ? cellLayout.MeasureChoiceHeight(count, false) : 50f + (count * 44f);
        }

        private void EnsureRecordChoiceRoot()
        {
            if (recordChoiceRoot != null)
            {
                return;
            }

            Transform found = transform.Find("Record Choice Root");
            if (found != null)
            {
                recordChoiceRoot = found as RectTransform;
                return;
            }

            GameObject root = new GameObject("Record Choice Root", typeof(RectTransform), typeof(VerticalLayoutGroup));
            root.transform.SetParent(transform, false);
            recordChoiceRoot = root.GetComponent<RectTransform>();

            VerticalLayoutGroup layout = root.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
        }

        private void ClearRecordChoiceRoot()
        {
            if (recordChoiceRoot == null)
            {
                return;
            }

            for (int i = recordChoiceRoot.childCount - 1; i >= 0; i--)
            {
                Transform child = recordChoiceRoot.GetChild(i);
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        private TMP_Text CreateRecordChoiceText(string name, string text, float fontSize, FontStyles fontStyle, float height)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(LayoutElement), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(recordChoiceRoot, false);

            LayoutElement layout = textObject.GetComponent<LayoutElement>();
            layout.preferredHeight = height;

            TMP_Text label = textObject.GetComponent<TMP_Text>();
            label.text = text;
            label.fontSize = fontSize;
            label.fontStyle = fontStyle;
            label.alignment = TextAlignmentOptions.Left;
            label.color = new Color(0.19f, 0.15f, 0.11f, 1f);
            return label;
        }

        private void CreateRecordChoiceButton(
            string entryId,
            int choiceIndex,
            string label,
            Action<string, int> onRecordChoiceSelected,
            float height)
        {
            GameObject buttonObject = new GameObject(
                "Record Choice " + (choiceIndex + 1),
                typeof(RectTransform),
                typeof(LayoutElement),
                typeof(Image),
                typeof(Button));
            buttonObject.transform.SetParent(recordChoiceRoot, false);

            LayoutElement layout = buttonObject.GetComponent<LayoutElement>();
            layout.preferredHeight = height;

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.92f, 0.82f, 0.64f, 1f);

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            int capturedChoiceIndex = choiceIndex;
            button.onClick.AddListener(() => onRecordChoiceSelected?.Invoke(entryId, capturedChoiceIndex));

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonObject.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(10f, 4f);
            labelRect.offsetMax = new Vector2(-10f, -4f);

            TMP_Text text = labelObject.GetComponent<TMP_Text>();
            text.text = label;
            text.fontSize = 15f;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(0.18f, 0.13f, 0.09f, 1f);
        }

        private static string GetSelectedChoiceLabel(
            IReadOnlyList<NotebookEntryDefinition.RecordChoice> choices,
            int selectedIndex)
        {
            if (choices != null
                && selectedIndex >= 0
                && selectedIndex < choices.Count
                && choices[selectedIndex] != null
                && !string.IsNullOrWhiteSpace(choices[selectedIndex].Label))
            {
                return choices[selectedIndex].Label;
            }

            return "choice saved";
        }
    }
}
