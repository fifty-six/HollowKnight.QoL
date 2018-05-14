using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalEnums;
using Modding;
using System.Reflection;
using On;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace QoL
{
    public class SkipCutscenes : Mod, ITogglableMod
    {
        public SkipCutscenes Instance;

        public override void Initialize()
        {
            Instance = this;
            On.CreditsHelper.BeginCredits += EnableSkip;
            ModHooks.Instance.SceneChanged += SceneModHook;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneLoaded;
        }

        private void SceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            CinematicPlayer player = GameObject.FindObjectOfType<CinematicPlayer>();
            if (player == null) return;

            Log("allowed skip from Unity Hook");
            player.skipMode = SkipPromptMode.SKIP_INSTANT;
            player.startSkipLocked = false;
            player.UnlockSkip();
        }

        private void EnableSkip(On.CreditsHelper.orig_BeginCredits orig, CreditsHelper self)
        {
            self.cutSceneHelper.skipMode = SkipPromptMode.SKIP_INSTANT;
            self.cutSceneHelper.UnlockSkip();
            orig(self);
        }

        private void SceneModHook(string targetScene)
        {
            CinematicPlayer player;
            player = GameObject.FindObjectOfType<CinematicPlayer>();
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
