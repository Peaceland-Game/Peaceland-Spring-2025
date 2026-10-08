using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Peaceland.Notebook
{
    /// <summary>
    /// Scene adapter: assign NotebookEntryDefinition assets here, then call Collect()
    /// when the player finishes the interaction. This component does not decide minigame rules.
    /// </summary>
    public class NotebookCollectTrigger : MonoBehaviour, IPointerClickHandler
    {
        [Tooltip("Optional. Found automatically when the scene has a notebook.")]
        [SerializeField] private NotebookController notebookController;
        [Tooltip("Entries unlocked when Collect() runs. Reference assets from Assets/Notebook/Data.")]
        [SerializeField] private List<NotebookEntryDefinition> entries = new List<NotebookEntryDefinition>();
        [Tooltip("Hide this object once all its entries are collected.")]
        [SerializeField] private bool disableAfterCollect = true;
        [Tooltip("Collect when the player clicks/taps this object. Needs a Collider2D (world) or a UI Graphic, an EventSystem, and a Physics2DRaycaster on the camera for world objects. Turn off when a minigame or adapter should decide instead.")]
        [SerializeField] private bool collectOnPointerClick = true;
        [Tooltip("Legacy mouse click. Does nothing in this project (Input System only); kept so older scenes still load.")]
        [SerializeField] private bool collectOnMouseDown = true;
        [Tooltip("Runs once when at least one new entry was collected.")]
        [SerializeField] private UnityEvent onCollected;

        /// <summary>Turns clicking this object on or off as a way to collect. Adapters turn it off.</summary>
        public void SetClickToCollect(bool enabled)
        {
            collectOnPointerClick = enabled;
            collectOnMouseDown = enabled;
        }

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

        /// <summary>
        /// Unlocks every assigned entry that is not already collected, then saves.
        /// Call this from a UnityEvent, drag-complete adapter, or other gameplay script.
        /// </summary>
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
                Debug.Log("Notebook: '" + name + "' collected '" + toastTitle + "'.", this);
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
