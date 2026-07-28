using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Peaceland.Notebook
{
    public class NotebookBookmarkTabBar : MonoBehaviour
    {
        private static readonly NotebookSection[] SectionOrder =
        {
            NotebookSection.Directory,
            NotebookSection.Present,
            NotebookSection.Memory1,
            NotebookSection.Memory2,
            NotebookSection.HiddenStats,
        };

        [SerializeField] private NotebookController notebookController;
        [SerializeField] private RectTransform leftRail;
        [SerializeField] private RectTransform rightRail;
        [SerializeField] private List<NotebookBookmarkTabView> tabs = new List<NotebookBookmarkTabView>();

        private NotebookBookArtLayout artLayout;

        private void Awake()
        {
            CacheArtLayout();
            EnsureRails();
            RefreshTabsFromRails();
            WireTabs();
        }

        public void Configure(
            NotebookController controller,
            IEnumerable<NotebookBookmarkTabView> bookmarkTabs,
            RectTransform leftBookmarkRail = null,
            RectTransform rightBookmarkRail = null)
        {
            notebookController = controller;
            if (leftBookmarkRail != null)
            {
                leftRail = leftBookmarkRail;
            }

            if (rightBookmarkRail != null)
            {
                rightRail = rightBookmarkRail;
            }

            CacheArtLayout();
            EnsureRails();
            if (bookmarkTabs != null)
            {
                tabs = OrderTabsBySection(bookmarkTabs);
            }
            else
            {
                RefreshTabsFromRails();
            }

            RefreshAndWire(NotebookSection.Directory, true);
        }

        public void RefreshAndWire(NotebookSection activeSection, bool onMainDirectory)
        {
            RefreshTabsFromRails();
            EnsureRails();
            ApplyFeatureVisibility();
            WireTabs();
            SyncActiveTab(activeSection, onMainDirectory);
        }

        public void ApplyFeatureVisibility()
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                NotebookBookmarkTabView tab = tabs[i];
                if (tab == null)
                {
                    continue;
                }

                tab.gameObject.SetActive(NotebookFeatureFlags.IsSectionVisible(tab.Section));
            }
        }

        public static List<NotebookBookmarkTabView> CollectTabsFromOpenRoot(Transform openRoot)
        {
            if (openRoot == null)
            {
                return new List<NotebookBookmarkTabView>();
            }

            Dictionary<NotebookSection, NotebookBookmarkTabView> bySection = CollectTabsBySection(openRoot);
            return OrderTabsFromLookup(bySection);
        }

        public void SyncActiveTab(NotebookSection activeSection, bool onMainDirectory)
        {
            Dictionary<NotebookSection, NotebookBookmarkTabView> tabsBySection = BuildTabsBySection();
            int lastLeftSectionIndex = onMainDirectory
                ? 0
                : GetSectionOrderIndex(activeSection);

            int leftVisibleCount = 0;
            int rightVisibleCount = 0;
            int leftMaxSlotIndex = -1;
            int rightMaxSlotIndex = -1;

            for (int i = 0; i < SectionOrder.Length; i++)
            {
                NotebookSection section = SectionOrder[i];
                if (!NotebookFeatureFlags.IsSectionVisible(section))
                {
                    continue;
                }

                if (!tabsBySection.TryGetValue(section, out NotebookBookmarkTabView tab) || tab == null)
                {
                    continue;
                }

                ApplySectionLabel(tab, section);

                int sectionSlotIndex = GetSectionOrderIndex(section);
                bool onLeft = sectionSlotIndex <= lastLeftSectionIndex;
                if (onLeft)
                {
                    // Fixed vertical slot per section (physical bookmark) — only the rail side changes.
                    PlaceTab(tab, leftRail, NotebookBookmarkSide.Left, sectionSlotIndex);
                    leftVisibleCount++;
                    leftMaxSlotIndex = Mathf.Max(leftMaxSlotIndex, sectionSlotIndex);
                }
                else
                {
                    PlaceTab(tab, rightRail, NotebookBookmarkSide.Right, sectionSlotIndex);
                    rightVisibleCount++;
                    rightMaxSlotIndex = Mathf.Max(rightMaxSlotIndex, sectionSlotIndex);
                }

                bool isDirectoryTab = section == NotebookSection.Directory;
                bool active = isDirectoryTab
                    ? onMainDirectory
                    : !onMainDirectory && section == activeSection;
                tab.SetActiveVisual(active);
            }

            ResizeRailIfAllowed(leftRail, leftMaxSlotIndex);
            ResizeRailIfAllowed(rightRail, rightMaxSlotIndex);
            SetRailVisible(leftRail, leftVisibleCount > 0);
            SetRailVisible(rightRail, rightVisibleCount > 0);
            StripRailMask(leftRail);
            StripRailMask(rightRail);
        }

        private static Dictionary<NotebookSection, NotebookBookmarkTabView> CollectTabsBySection(Transform openRoot)
        {
            Dictionary<NotebookSection, NotebookBookmarkTabView> bySection =
                new Dictionary<NotebookSection, NotebookBookmarkTabView>();
            CollectUniqueTabs(openRoot.Find("Bookmark Tabs Left"), bySection);
            CollectUniqueTabs(openRoot.Find("Bookmark Tabs Right"), bySection);

            if (bySection.Count == 0)
            {
                NotebookBookmarkTabView[] views = openRoot.GetComponentsInChildren<NotebookBookmarkTabView>(true);
                for (int i = 0; i < views.Length; i++)
                {
                    NotebookBookmarkTabView view = views[i];
                    if (view == null || bySection.ContainsKey(view.Section))
                    {
                        continue;
                    }

                    bySection[view.Section] = view;
                }
            }

            return bySection;
        }

        private static void CollectUniqueTabs(
            Transform rail,
            Dictionary<NotebookSection, NotebookBookmarkTabView> bySection)
        {
            if (rail == null)
            {
                return;
            }

            NotebookBookmarkTabView[] views = rail.GetComponentsInChildren<NotebookBookmarkTabView>(true);
            for (int i = 0; i < views.Length; i++)
            {
                NotebookBookmarkTabView view = views[i];
                if (view == null || bySection.ContainsKey(view.Section))
                {
                    continue;
                }

                bySection[view.Section] = view;
            }
        }

        private void RefreshTabsFromRails()
        {
            Transform openRoot = ResolveOpenRoot();
            if (openRoot != null)
            {
                tabs = CollectTabsFromOpenRoot(openRoot);
                return;
            }

            Dictionary<NotebookSection, NotebookBookmarkTabView> bySection =
                new Dictionary<NotebookSection, NotebookBookmarkTabView>();
            CollectUniqueTabs(leftRail, bySection);
            CollectUniqueTabs(rightRail, bySection);
            tabs = OrderTabsFromLookup(bySection);
        }

        private Dictionary<NotebookSection, NotebookBookmarkTabView> BuildTabsBySection()
        {
            Dictionary<NotebookSection, NotebookBookmarkTabView> bySection =
                new Dictionary<NotebookSection, NotebookBookmarkTabView>();
            for (int i = 0; i < tabs.Count; i++)
            {
                NotebookBookmarkTabView tab = tabs[i];
                if (tab == null || bySection.ContainsKey(tab.Section))
                {
                    continue;
                }

                bySection[tab.Section] = tab;
            }

            return bySection;
        }

        private static List<NotebookBookmarkTabView> OrderTabsFromLookup(
            Dictionary<NotebookSection, NotebookBookmarkTabView> bySection)
        {
            List<NotebookBookmarkTabView> ordered = new List<NotebookBookmarkTabView>();
            for (int i = 0; i < SectionOrder.Length; i++)
            {
                if (!NotebookFeatureFlags.IsSectionVisible(SectionOrder[i]))
                {
                    continue;
                }

                if (bySection.TryGetValue(SectionOrder[i], out NotebookBookmarkTabView tab))
                {
                    ordered.Add(tab);
                }
            }

            return ordered;
        }

        private static List<NotebookBookmarkTabView> OrderTabsBySection(IEnumerable<NotebookBookmarkTabView> source)
        {
            Dictionary<NotebookSection, NotebookBookmarkTabView> bySection =
                new Dictionary<NotebookSection, NotebookBookmarkTabView>();
            foreach (NotebookBookmarkTabView tab in source)
            {
                if (tab == null || bySection.ContainsKey(tab.Section))
                {
                    continue;
                }

                bySection[tab.Section] = tab;
            }

            return OrderTabsFromLookup(bySection);
        }

        private void PlaceTab(NotebookBookmarkTabView tab, RectTransform rail, NotebookBookmarkSide side, int sectionSlotIndex)
        {
            if (tab == null || rail == null)
            {
                return;
            }

            RectTransform tabRect = tab.transform as RectTransform;
            tabRect.SetParent(rail, false);
            tab.SetSide(side);
            if (side == NotebookBookmarkSide.Left)
            {
                NotebookBookShellLayout.ApplyBookmarkTabLeft(tabRect, sectionSlotIndex);
            }
            else
            {
                NotebookBookShellLayout.ApplyBookmarkTabRight(tabRect, sectionSlotIndex);
            }
        }

        private void ResizeRailIfAllowed(RectTransform rail, int maxSectionSlotIndex)
        {
            ResizeRail(rail, maxSectionSlotIndex);
        }

        private void ResizeRail(RectTransform rail, int maxSectionSlotIndex)
        {
            if (rail == null)
            {
                return;
            }

            int slotCount = Mathf.Max(1, maxSectionSlotIndex + 1);
            float height = (slotCount * NotebookBookShellLayout.BookmarkHeight)
                + (Mathf.Max(0, slotCount - 1) * NotebookBookShellLayout.BookmarkSpacing);
            Vector2 sizeDelta = rail.sizeDelta;
            sizeDelta.y = height;
            rail.sizeDelta = sizeDelta;
        }

        private static void SetRailVisible(RectTransform rail, bool visible)
        {
            if (rail != null)
            {
                rail.gameObject.SetActive(visible);
            }
        }

        private static int GetSectionOrderIndex(NotebookSection section)
        {
            for (int i = 0; i < SectionOrder.Length; i++)
            {
                if (SectionOrder[i] == section)
                {
                    return i;
                }
            }

            return 0;
        }

        private void CacheArtLayout()
        {
            if (artLayout != null)
            {
                return;
            }

            artLayout = GetComponentInParent<NotebookBookArtLayout>();
            Transform openRoot = ResolveOpenRoot();
            if (artLayout == null && openRoot != null)
            {
                artLayout = openRoot.GetComponent<NotebookBookArtLayout>();
            }
        }

        private bool ShouldSkipAutoLayout()
        {
            CacheArtLayout();
            return artLayout != null && artLayout.LockLayout;
        }

        private void EnsureRails()
        {
            if (leftRail == null)
            {
                Transform found = transform.Find("Bookmark Tabs Left");
                if (found == null)
                {
                    found = transform.parent != null ? transform.parent.Find("Bookmark Tabs Left") : null;
                }

                leftRail = found as RectTransform;
            }

            if (rightRail == null)
            {
                Transform found = transform.Find("Bookmark Tabs Right");
                if (found == null)
                {
                    found = transform.parent != null ? transform.parent.Find("Bookmark Tabs Right") : null;
                }

                rightRail = found as RectTransform;
            }

            CacheArtLayout();
            if (ShouldSkipAutoLayout())
            {
                return;
            }

            float topInset = artLayout != null ? artLayout.BookmarkTopInset : NotebookBookShellLayout.BookmarkTopInset;
            float horizontalOffset = artLayout != null
                ? artLayout.BookmarkHorizontalOffset
                : NotebookBookShellLayout.BookmarkHorizontalOffset;

            if (leftRail != null)
            {
                NotebookBookShellLayout.ApplyBookmarkRailLeft(leftRail, topInset, horizontalOffset);
            }

            if (rightRail != null)
            {
                NotebookBookShellLayout.ApplyBookmarkRailRight(rightRail, topInset, horizontalOffset);
            }
        }

        private static void StripRailMask(RectTransform rail)
        {
            if (rail == null)
            {
                return;
            }

            RectMask2D mask = rail.GetComponent<RectMask2D>();
            if (mask == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Object.DestroyImmediate(mask);
                return;
            }
#endif
            Object.Destroy(mask);
        }

        private void ApplySectionLabel(NotebookBookmarkTabView tab, NotebookSection section)
        {
            string label = GetDefaultSectionLabel(section);
            if (section == NotebookSection.Directory && label.Length > 8)
            {
                label = "Dir";
            }

            tab.Configure(section, label);
        }

        private static string GetDefaultSectionLabel(NotebookSection section)
        {
            switch (section)
            {
                case NotebookSection.Directory:
                    return "Directory";
                case NotebookSection.Present:
                    return "Present";
                case NotebookSection.Memory1:
                    return "Mem 1";
                case NotebookSection.Memory2:
                    return "Mem 2";
                case NotebookSection.HiddenStats:
                    return "Stats";
                default:
                    return section.ToString();
            }
        }

        private void WireTabs()
        {
            NotebookController controller = ResolveController();
            if (controller == null)
            {
                return;
            }

            Dictionary<NotebookSection, NotebookBookmarkTabView> tabsBySection = BuildTabsBySection();
            for (int i = 0; i < SectionOrder.Length; i++)
            {
                NotebookSection section = SectionOrder[i];
                if (!NotebookFeatureFlags.IsSectionVisible(section))
                {
                    continue;
                }

                if (!tabsBySection.TryGetValue(section, out NotebookBookmarkTabView tab)
                    || tab == null
                    || tab.Button == null)
                {
                    continue;
                }

                tab.Button.onClick.RemoveAllListeners();
                if (section == NotebookSection.Directory)
                {
                    tab.Button.onClick.AddListener(controller.ShowDirectory);
                }
                else
                {
                    NotebookSection capturedSection = section;
                    tab.Button.onClick.AddListener(() => controller.ShowSectionDirectory(capturedSection));
                }
            }
        }

        private Transform ResolveOpenRoot()
        {
            if (leftRail != null)
            {
                return leftRail.parent;
            }

            if (rightRail != null)
            {
                return rightRail.parent;
            }

            return transform.parent;
        }

        private NotebookController ResolveController()
        {
            if (notebookController != null)
            {
                return notebookController;
            }

            notebookController = GetComponentInParent<NotebookController>();
            if (notebookController == null)
            {
                notebookController = FindFirstObjectByType<NotebookController>();
            }

            return notebookController;
        }
    }
}
