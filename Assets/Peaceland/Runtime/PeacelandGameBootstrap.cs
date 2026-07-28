using UnityEngine;
using UnityEngine.SceneManagement;

namespace Peaceland
{
    /// <summary>
    /// DontDestroyOnLoad host for unified save, stats, and progress.
    /// </summary>
    public sealed class PeacelandGameBootstrap : MonoBehaviour
    {
        private static PeacelandGameBootstrap instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreateForPlayMode()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            EnsureExists();
        }

        public static void EnsureExists()
        {
            if (instance != null)
            {
                return;
            }

            PeacelandGameBootstrap existing = FindFirstObjectByType<PeacelandGameBootstrap>();
            if (existing != null)
            {
                instance = existing;
                return;
            }

            GameObject host = new GameObject("Peaceland Game Bootstrap");
            instance = host.AddComponent<PeacelandGameBootstrap>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            PeacelandSaveService saveService = GetComponent<PeacelandSaveService>();
            if (saveService == null)
            {
                saveService = gameObject.AddComponent<PeacelandSaveService>();
            }

            saveService.InitializeAsHost();

            if (GetComponent<PeacelandStatManager>() == null)
            {
                gameObject.AddComponent<PeacelandStatManager>();
            }

            if (GetComponent<PeacelandProgress>() == null)
            {
                gameObject.AddComponent<PeacelandProgress>();
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (instance == this)
            {
                instance = null;
            }
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (instance == null)
            {
                return;
            }

            PeacelandSceneCheckpointPolicy policy = FindFirstObjectByType<PeacelandSceneCheckpointPolicy>();
            if (policy != null && !policy.RecordAsGameplayCheckpoint)
            {
                return;
            }

            PeacelandProgress.Instance.SetCurrentSceneCheckpoint(scene.name);
        }
    }
}
