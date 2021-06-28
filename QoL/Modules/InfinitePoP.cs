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
            ModHooks.GetPlayerBoolHook += GetBool;
            ModHooks.SetPlayerBoolHook += SetBool;

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

        private static bool SetBool(string field, bool value)
        {
            if (field != nameof(PlayerData.newDataBindingSeal) || !value) 
                return value;

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

            // Make sure the set doesn't change the save.
            return PlayerData.instance.newDataBindingSeal;
        }

        private static bool GetBool(string originalset, bool orig)
        {
            return originalset == nameof(PlayerData.newDataBindingSeal) || orig;
        }

        public override void Unload()
        {
            ModHooks.GetPlayerBoolHook -= GetBool;
            ModHooks.SetPlayerBoolHook -= SetBool;
            
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;
        }
    }
}