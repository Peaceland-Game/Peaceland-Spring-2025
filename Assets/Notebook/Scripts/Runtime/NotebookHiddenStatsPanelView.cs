using Peaceland;
using Peaceland.Notebook.EditableScenePack;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    /// <summary>
    /// In-notebook playtest surface for hidden narrative stats.
    /// </summary>
    public sealed class NotebookHiddenStatsPanelView : MonoBehaviour
    {
        private static readonly PeacelandStatId[] StatOrder =
        {
            PeacelandStatId.KindnessCruelty,
            PeacelandStatId.SelfishAltruistic,
            PeacelandStatId.InsightNaivety,
            PeacelandStatId.NationalismRebellion,
        };

        [SerializeField] private RectTransform rowsRoot;
        [SerializeField] private TMP_Text headerText;
        [SerializeField] private TMP_Text footerText;

        private readonly TMP_Text[] valueLabels = new TMP_Text[StatOrder.Length];
        private bool isBuilt;

        public void Show()
        {
            EnsureBuilt();
            gameObject.SetActive(true);
            RefreshAll();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void EnsureBuilt()
        {
            if (isBuilt)
            {
                return;
            }

            isBuilt = true;
            BuildUiIfMissing();
        }

        private void BuildUiIfMissing()
        {
            RectTransform panelRect = transform as RectTransform;
            if (panelRect == null)
            {
                panelRect = gameObject.AddComponent<RectTransform>();
            }

            if (!NotebookUILayoutGuard.ShouldSkipLayoutApply(panelRect))
            {
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.offsetMin = new Vector2(24f, 24f);
                panelRect.offsetMax = new Vector2(-24f, -24f);
            }

            VerticalLayoutGroup panelLayout = gameObject.GetComponent<VerticalLayoutGroup>();
            if (panelLayout == null)
            {
                panelLayout = gameObject.AddComponent<VerticalLayoutGroup>();
            }

            panelLayout.padding = new RectOffset(24, 24, 18, 18);
            panelLayout.spacing = 14f;
            panelLayout.childAlignment = TextAnchor.UpperLeft;
            panelLayout.childControlWidth = true;
            panelLayout.childControlHeight = true;
            panelLayout.childForceExpandWidth = true;
            panelLayout.childForceExpandHeight = false;

            if (headerText == null)
            {
                headerText = CreateText("Header", panelRect, 24, FontStyles.Bold);
                headerText.text = "Hidden Stats";
                headerText.alignment = TextAlignmentOptions.TopLeft;
                AddLayoutElement(headerText.gameObject, 0f, 36f, 0f);
            }

            if (rowsRoot == null)
            {
                GameObject rowsObject = new GameObject("Stat Rows", typeof(RectTransform));
                rowsObject.transform.SetParent(panelRect, false);
                rowsRoot = rowsObject.GetComponent<RectTransform>();

                VerticalLayoutGroup rowsLayout = rowsObject.AddComponent<VerticalLayoutGroup>();
                rowsLayout.spacing = 10f;
                rowsLayout.childAlignment = TextAnchor.UpperLeft;
                rowsLayout.childControlWidth = true;
                rowsLayout.childControlHeight = true;
                rowsLayout.childForceExpandWidth = true;
                rowsLayout.childForceExpandHeight = false;

                AddLayoutElement(rowsObject, 0f, 0f, 1f);
            }

            for (int i = 0; i < StatOrder.Length; i++)
            {
                if (valueLabels[i] == null)
                {
                    CreateStatRow(StatOrder[i], i);
                }
            }

            if (footerText == null)
            {
                footerText = CreateText("Footer", panelRect, 14, FontStyles.Italic);
                footerText.text = "Debug controls update the same saved hidden stats used by notebook record choices.";
                footerText.alignment = TextAlignmentOptions.BottomLeft;
                AddLayoutElement(footerText.gameObject, 0f, 42f, 0f);
            }
        }

        private void CreateStatRow(PeacelandStatId statId, int index)
        {
            GameObject rowObject = new GameObject(statId + " Row", typeof(RectTransform), typeof(Image));
            rowObject.transform.SetParent(rowsRoot, false);

            Image rowBackground = rowObject.GetComponent<Image>();
            rowBackground.color = new Color(0.18f, 0.14f, 0.1f, 0.42f);

            HorizontalLayoutGroup rowLayout = rowObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.padding = new RectOffset(14, 14, 8, 8);
            rowLayout.spacing = 10f;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = true;

            AddLayoutElement(rowObject, 0f, 64f, 0f);

            TMP_Text title = CreateText("Label", rowObject.transform, 16, FontStyles.Normal);
            title.text = NotebookStatDisplayNames.GetShortLabel(statId) + "\n<size=12>" + NotebookStatDisplayNames.GetTitle(statId) + "</size>";
            title.alignment = TextAlignmentOptions.MidlineLeft;
            AddLayoutElement(title.gameObject, 180f, 0f, 1f);

            Button minusButton = CreateButton(rowObject.transform, "-", new Color(0.45f, 0.22f, 0.22f, 1f));
            TMP_Text valueLabel = CreateText("Value", rowObject.transform, 28, FontStyles.Bold);
            valueLabel.alignment = TextAlignmentOptions.Center;
            AddLayoutElement(valueLabel.gameObject, 56f, 0f, 0f);

            Button plusButton = CreateButton(rowObject.transform, "+", new Color(0.22f, 0.42f, 0.28f, 1f));

            valueLabels[index] = valueLabel;

            PeacelandStatId captured = statId;
            minusButton.onClick.AddListener(() => AdjustStat(captured, -1));
            plusButton.onClick.AddListener(() => AdjustStat(captured, 1));
        }

        private void AdjustStat(PeacelandStatId statId, int delta)
        {
            PeacelandStatManager.Instance.AddDelta(statId, delta);
            RefreshAll();
        }

        private void RefreshAll()
        {
            for (int i = 0; i < StatOrder.Length; i++)
            {
                if (valueLabels[i] == null)
                {
                    continue;
                }

                valueLabels[i].text = PeacelandStatManager.Instance.Get(StatOrder[i]).ToString("+0;-0;0");
            }
        }

        private static TMP_Text CreateText(string name, Transform parent, float fontSize, FontStyles fontStyle)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);

            TMP_Text text = textObject.GetComponent<TMP_Text>();
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.color = new Color(0.22f, 0.16f, 0.09f, 1f);
            return text;
        }

        private static Button CreateButton(Transform parent, string label, Color color)
        {
            GameObject buttonObject = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            Image image = buttonObject.GetComponent<Image>();
            image.color = color;

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.18f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            TMP_Text text = CreateText("Text", buttonObject.transform, 22, FontStyles.Bold);
            text.text = label;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;

            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            AddLayoutElement(buttonObject, 44f, 44f, 0f);
            return button;
        }

        private static void AddLayoutElement(GameObject target, float preferredWidth, float preferredHeight, float flexibleWidth)
        {
            LayoutElement layoutElement = target.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = target.AddComponent<LayoutElement>();
            }

            if (preferredWidth > 0f)
            {
                layoutElement.minWidth = preferredWidth;
                layoutElement.preferredWidth = preferredWidth;
            }

            if (preferredHeight > 0f)
            {
                layoutElement.minHeight = preferredHeight;
                layoutElement.preferredHeight = preferredHeight;
            }

            layoutElement.flexibleWidth = flexibleWidth;
        }
    }
}
