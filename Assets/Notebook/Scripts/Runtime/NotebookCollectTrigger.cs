using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Peaceland.Notebook
{
    public class NotebookCollectTrigger : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private NotebookController notebookController;
        [SerializeField] private List<NotebookEntryDefinition> entries = new List<NotebookEntryDefinition>();
        [SerializeField] private bool disableAfterCollect = true;
        [SerializeField] private bool collectOnPointerClick = true;
        [SerializeField] private bool collectOnMouseDown = true;
        [SerializeField] private UnityEvent onCollected;

        private bool isRegisteredAsCollectable;

        private void OnEnable()
        {
            if (!HasUncollectedEntries())
            {
                if (disableAfterCollect)
                {
                    gameObject.SetActive(false);
                }

                return;
            }

            RegisterCollectableSource();
        }

        private void OnDisable()
        {
            UnregisterCollectableSource();
        }

        public void Collect()
        {
            UnregisterCollectableSource();

            NotebookController controller = ResolveController();
            NotebookCollectHintHost hintHost = ResolveHintHost();
            bool collectedAny = false;
            string toastTitle = null;

            for (int i = 0; i < entries.Count; i++)
            {
                NotebookEntryDefinition entry = entries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.EntryId))
                {
                    continue;
                }

                if (controller != null)
                {
                    if (!controller.HasCollected(entry.EntryId))
                    {
                        controller.CollectEntry(entry.EntryId);
                        collectedAny = true;
                        toastTitle = entry.Title;
                    }
                }
                else if (!NotebookSaveUtility.IsCollected(entry.EntryId))
                {
                    NotebookGlobalBridge.CollectEntry(entry.EntryId);
                    collectedAny = true;
                    toastTitle = entry.Title;
                }
            }

            if (collectedAny)
            {
                onCollected?.Invoke();

                if (controller == null && hintHost != null)
                {
                    hintHost.PlayCollectedToast("Notebook updated: " + (toastTitle ?? "entry"));
                }
            }

            if (disableAfterCollect && !HasUncollectedEntries())
            {
                gameObject.SetActive(false);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (collectOnPointerClick)
            {
                Collect();
            }
        }

        private void OnMouseDown()
        {
            if (collectOnMouseDown)
            {
                Collect();
            }
        }

        private NotebookController ResolveController()
        {
            if (notebookController != null)
            {
                return notebookController;
            }

            NotebookController parentController = GetComponentInParent<NotebookController>();
            if (parentController != null)
            {
                return parentController;
            }

            return NotebookSceneLookup.FindController();
        }

        private NotebookCollectHintHost ResolveHintHost()
        {
            return FindFirstObjectByType<NotebookCollectHintHost>(FindObjectsInactive.Include);
        }

        private bool HasUncollectedEntries()
        {
            NotebookController controller = ResolveController();
            for (int i = 0; i < entries.Count; i++)
            {
                NotebookEntryDefinition entry = entries[i];
                if (entry == null)
                {
                    continue;
                }

                if (controller != null)
                {
                    if (!controller.HasCollected(entry.EntryId))
                    {
                        return true;
                    }
                }
                else if (!NotebookSaveUtility.IsCollected(entry.EntryId))
                {
                    return true;
                }
            }

            return false;
        }

        private void RegisterCollectableSource()
        {
            if (isRegisteredAsCollectable)
            {
                return;
            }

            NotebookController controller = ResolveController();
            if (controller != null)
            {
                controller.RegisterCollectableSource();
            }
            else
            {
                NotebookCollectHintHost hintHost = ResolveHintHost();
                if (hintHost != null)
                {
                    hintHost.RegisterCollectableSource();
                }
            }

            isRegisteredAsCollectable = true;
        }

        private void UnregisterCollectableSource()
        {
            if (!isRegisteredAsCollectable)
            {
                return;
            }

            NotebookController controller = ResolveController();
            if (controller != null)
            {
                controller.UnregisterCollectableSource();
            }
            else
            {
                NotebookCollectHintHost hintHost = ResolveHintHost();
                if (hintHost != null)
                {
                    hintHost.UnregisterCollectableSource();
                }
            }

            isRegisteredAsCollectable = false;
        }
    }
}
