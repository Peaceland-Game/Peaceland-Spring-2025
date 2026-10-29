using UnityEngine;
using UnityEngine.SceneManagement;

namespace Peaceland
{
    /// <summary>
    /// DontDestroyOnLoad host for unified save, stats, and progress.
    /// Runs before scene Awake so Notebook/Save consumers never hit a missing instance.
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    public sealed class PeacelandGameBootstrap : MonoBehaviour
    {
        private static PeacelandGameBootstrap instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
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
                existing.EnsureHostReady();
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
            EnsureHostReady();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void EnsureHostReady()
        {
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
        }

        private void OnDestroy()
        {
            if (instance != this)
            {
                return;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (instance == null)
            {
                return;
            }

            PeacelandSceneCheckpointPolicy policy = FindCheckpointPolicy(scene);
            if (policy == null)
            {
                Debug.LogWarning(
                    "[SaveLoad] Scene '" + scene.name
                    + "' has no PeacelandSceneCheckpointPolicy. Checkpoint was not changed.");
                return;
            }

            if (!policy.RecordAsGameplayCheckpoint)
            {
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(scene.name))
            {
                Debug.LogError(
                    "[SaveLoad] Scene '" + scene.name
                    + "' is marked as a checkpoint but is not enabled in Build Settings.");
                return;
            }

            PeacelandProgress.Instance.SetCurrentSceneCheckpoint(
                scene.name,
                policy.CheckpointKey,
                policy.CheckpointDisplayName,
                policy.ReplaceMatchingCheckpoint);
        }

        private static PeacelandSceneCheckpointPolicy FindCheckpointPolicy(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                PeacelandSceneCheckpointPolicy policy =
                    root.GetComponentInChildren<PeacelandSceneCheckpointPolicy>(true);
                if (policy != null)
                {
                    return policy;
                }
            }

            return null;
        }
    }
}
