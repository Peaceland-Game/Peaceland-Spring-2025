using UnityEngine;

namespace Peaceland
{
    public sealed class PeacelandProgress : MonoBehaviour
    {
        private static PeacelandProgress instance;

        public static PeacelandProgress Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<PeacelandProgress>();
                }

                if (instance == null)
                {
                    PeacelandGameBootstrap.EnsureExists();
                    instance = FindFirstObjectByType<PeacelandProgress>();
                }

                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public bool HasFlag(string flagId)
        {
            return PeacelandSaveService.Instance.HasProgressFlag(flagId);
        }

        public void SetFlag(string flagId, bool value = true)
        {
            PeacelandSaveService.Instance.SetProgressFlag(flagId, value);
        }

        public void ClearFlag(string flagId)
        {
            PeacelandSaveService.Instance.SetProgressFlag(flagId, false);
        }

        public string GetCurrentSceneCheckpoint()
        {
            return PeacelandSaveService.Instance.GetLastSceneName();
        }

        public void SetCurrentSceneCheckpoint(string sceneName)
        {
            PeacelandSaveService.Instance.SetLastSceneName(sceneName);
        }
    }
}
