using JetBrains.Annotations;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QoL.Modules
{
    [UsedImplicitly]
    public class InfinitePoP : FauxMod
    {
        public override void Initialize()
        {
            ModHooks.Instance.GetPlayerBoolHook += GetBool;
            ModHooks.Instance.SetPlayerBoolHook += SetBool;

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private static void OnSceneChanged(Scene arg0, Scene arg1)
        {
            if (arg1.name != "White_Palace_06") return;

            GameObject blocker = GameObject.Find("Path of Pain Blocker");

            if (blocker != null)
            {
                Object.Destroy(blocker);
            }
        }

        private static void SetBool(string originalset, bool value)
        {
            PlayerData.instance.SetBoolInternal(originalset, value);

            if (originalset != nameof(PlayerData.newDataBindingSeal) || !value) return;

            SceneData sd = GameManager.instance.sceneData;

            sd.SaveMyState(new PersistentBoolData
            {
                sceneName = "White_Palace_17",
                id = "WP Lever",
                activated = false
            });

            sd.SaveMyState(new PersistentBoolData
            {
                sceneName = "White_Palace_17",
                id = "Collapser Small",
                activated = false
            });

            sd.SaveMyState(new PersistentBoolData
            {
                sceneName = "White_Palace_06",
                id = "Breakable Wall Ruin Lift",
                activated = false
            });
        }

        private static bool GetBool(string originalset)
        {
            return originalset == nameof(PlayerData.newDataBindingSeal) || PlayerData.instance.GetBoolInternal(originalset);
        }

        public override void Unload()
        {
            ModHooks.Instance.GetPlayerBoolHook -= GetBool;
            ModHooks.Instance.SetPlayerBoolHook -= SetBool;
            
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;
        }
    }
}