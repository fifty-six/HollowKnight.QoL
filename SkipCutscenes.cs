using GlobalEnums;
using Modding;
using JetBrains.Annotations;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace QoL
{
    [UsedImplicitly]
    public class SkipCutscenes : Mod, ITogglableMod
    {
        public override void Initialize()
        {
            On.CreditsHelper.BeginCredits += EnableSkip;
            ModHooks.Instance.SceneChanged += SceneModHook;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneLoaded;
        }

        private void SceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            CinematicPlayer player = Object.FindObjectOfType<CinematicPlayer>();
            if (player == null) return;

            Log("allowed skip from Unity Hook");
            player.skipMode = SkipPromptMode.SKIP_INSTANT;
            player.startSkipLocked = false;
            player.UnlockSkip();
        }

        private static void EnableSkip(On.CreditsHelper.orig_BeginCredits orig, CreditsHelper self)
        {
            self.cutSceneHelper.skipMode = SkipPromptMode.SKIP_INSTANT;
            self.cutSceneHelper.UnlockSkip();
            orig(self);
        }

        private void SceneModHook(string targetScene)
        {
            CinematicPlayer player = Object.FindObjectOfType<CinematicPlayer>();
            if (player == null) return;

            Log("allowed skip from ModHook");
            player.skipMode = SkipPromptMode.SKIP_INSTANT;
            player.startSkipLocked = false;
            player.UnlockSkip();
        }

        public void Unload()
        {
            On.CreditsHelper.BeginCredits -= EnableSkip;
            ModHooks.Instance.SceneChanged -= SceneModHook;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneLoaded;
        }

    }
}
