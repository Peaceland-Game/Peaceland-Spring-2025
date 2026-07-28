using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Peaceland
{
    /// <summary>
    /// Title screen flow: Start → save slot panel → load gameplay scene.
    /// </summary>
    public sealed class PeacelandGameStartController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button closeSavePanelButton;
        [SerializeField] private PeacelandSaveSlotPanel saveSlotPanel;

        [Header("Gameplay")]
        [SerializeField] private string firstGameplaySceneName = "NoteBookTesting";
        [SerializeField] private bool resumeLastSceneWhenAvailable = true;

        private void Awake()
        {
            PeacelandGameBootstrap.EnsureExists();
            PeacelandSaveService.Instance.ClearActiveSlotSelection();

            if (saveSlotPanel != null)
            {
                saveSlotPanel.Initialize(this);
            }

            if (startGameButton != null)
            {
                startGameButton.onClick.AddListener(OnStartGamePressed);
            }

            if (closeSavePanelButton != null)
            {
                closeSavePanelButton.onClick.AddListener(OnCloseSavePanelPressed);
            }
        }

        private void OnStartGamePressed()
        {
            if (saveSlotPanel != null)
            {
                saveSlotPanel.Show();
            }
        }

        private void OnCloseSavePanelPressed()
        {
            if (saveSlotPanel != null)
            {
                saveSlotPanel.Hide();
            }
        }

        public void StartNewGameInSlot(int slotIndex)
        {
            PeacelandSaveService saveService = PeacelandSaveService.Instance;
            saveService.ActivateSlotAndStartNewGame(slotIndex);
            PlayerPrefs.DeleteKey("peaceland.notebook.state");
            PlayerPrefs.Save();
            LoadGameplayScene(string.Empty);
        }

        public void ContinueFromSlot(int slotIndex)
        {
            PeacelandSaveService saveService = PeacelandSaveService.Instance;
            if (!saveService.ActivateSlotAndContinue(slotIndex))
            {
                Debug.LogWarning("Could not continue slot " + slotIndex + "; starting fresh in that slot.");
                StartNewGameInSlot(slotIndex);
                return;
            }

            LoadGameplayScene(saveService.GetLastSceneName());
        }

        private void LoadGameplayScene(string lastSceneName)
        {
            string targetScene = ResolveTargetScene(lastSceneName);
            if (string.IsNullOrWhiteSpace(targetScene))
            {
                Debug.LogError("PeacelandGameStartController has no valid gameplay scene configured.");
                return;
            }

            SceneManager.LoadScene(targetScene);
        }

        private string ResolveTargetScene(string lastSceneName)
        {
            if (resumeLastSceneWhenAvailable && !string.IsNullOrWhiteSpace(lastSceneName))
            {
                if (Application.CanStreamedLevelBeLoaded(lastSceneName))
                {
                    return lastSceneName;
                }

                Debug.LogWarning("Last scene not in build settings: " + lastSceneName);
            }

            return firstGameplaySceneName;
        }
    }
}
