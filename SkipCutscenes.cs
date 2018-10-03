using System.Collections;
using System.Linq;
using GlobalEnums;
using Modding;
using JetBrains.Annotations;
using ModCommon;
using ModCommon.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = Modding.Logger;
using UObject = UnityEngine.Object;

namespace QoL
{
    [UsedImplicitly]
    public class SkipCutscenes : Mod, ITogglableMod
    {
        private const string GUARDIAN = "Dream_Guardian_";
        
        private static readonly string[] DREAMERS = { "Deepnest_Spider_Town", "Fungus3_archive_02", "Ruins2_Watcher_Room"};

        public override void Initialize()
        {
            On.CinematicSequence.Begin += CinematicBegin;
            On.FadeSequence.Begin += FadeBegin;
            On.AnimatorSequence.Begin += AnimatorBegin;
            On.InputHandler.SetSkipMode += OnSetSkip;
            On.GameManager.BeginSceneTransitionRoutine += Dreamers;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += DreamerFsm;
        }

        public void Unload()
        {
            On.CinematicSequence.Begin -= CinematicBegin;
            On.FadeSequence.Begin -= FadeBegin;
            On.AnimatorSequence.Begin -= AnimatorBegin;
            On.InputHandler.SetSkipMode -= OnSetSkip;
            On.GameManager.BeginSceneTransitionRoutine -= Dreamers;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= DreamerFsm;
        }

        private static void DreamerFsm(Scene arg0, Scene arg1)
        {
            HeroController.instance.StartCoroutine(DreamerFsm(arg1));
        }

        private static IEnumerator DreamerFsm(Scene arg1)
        {
            if (!DREAMERS.Contains(arg1.name)) yield break;
            
            yield return null;

            GameObject.Find("Dream Enter").LocateMyFSM("Control").ChangeTransition("Idle", "DREAM HIT", "Change Scene");
        }

        private static IEnumerator Dreamers(On.GameManager.orig_BeginSceneTransitionRoutine orig, GameManager self, GameManager.SceneLoadInfo info)
        {
            if (info.SceneName.Length <= 15 || info.SceneName.Substring(0, 15) != GUARDIAN)
            {
                yield return orig(self, info);
                yield break;
            }


            string @bool = info.SceneName.Substring(15);

            PlayerData.instance.SetBool($"{@bool.ToLower()}Defeated", true);
            PlayerData.instance.SetBool($"maskBroken{@bool}", true);
            PlayerData.instance.guardiansDefeated++;
            
            info.SceneName = GameManager.instance.sceneName;
            info.EntryGateName = "door_dreamReturn";
            
            GameCameras.instance.cameraFadeFSM.Fsm.Event("FADE INSTANT");
            
            yield return orig(self, info);
            
            GameCameras.instance.cameraFadeFSM.Fsm.SetState("FadeIn");

            yield return null;

            while (GameManager.instance.gameState != GameState.PLAYING)
                yield return null;

            yield return null;

            HeroController.instance.AcceptInput();
        }

        private static void OnSetSkip(On.InputHandler.orig_SetSkipMode orig, InputHandler self, SkipPromptMode newmode)
        {
            orig(self, SkipPromptMode.SKIP_INSTANT);
        }

        private static void AnimatorBegin(On.AnimatorSequence.orig_Begin orig, AnimatorSequence self) => self.Skip();

        private static void FadeBegin(On.FadeSequence.orig_Begin orig, FadeSequence self) => self.Skip();

        private static void CinematicBegin(On.CinematicSequence.orig_Begin orig, CinematicSequence self) => self.Skip();
    }
}