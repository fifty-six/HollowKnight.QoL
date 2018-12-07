using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using Modding;
using JetBrains.Annotations;
using MonoMod.RuntimeDetour;
using UnityEngine;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;

namespace QoL
{
    [UsedImplicitly]
    public class SkipCutscenes : Mod, ITogglableMod
    {
        private const string GUARDIAN = "Dream_Guardian_";

        private static readonly string[] DREAMERS = {"Deepnest_Spider_Town", "Fungus3_archive_02", "Ruins2_Watcher_Room"};

        private const string UUMUU = "Fungus3_archive_02";
        private Hook _rando;

        public override int LoadPriority() => int.MaxValue;

        public override void Initialize()
        {
            On.CinematicSequence.Begin += CinematicBegin;
            On.FadeSequence.Begin += FadeBegin;
            On.AnimatorSequence.Begin += AnimatorBegin;
            On.InputHandler.SetSkipMode += OnSetSkip;
            On.GameManager.BeginSceneTransitionRoutine += Dreamers;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += FsmSkips;

            Type t = Type.GetType("RandomizerMod.RandomizerMod, RandomizerMod2.0");

            if (t == null) return;

            _rando = new Hook
            (
                t.GetMethod("SceneHasPreload", BindingFlags.NonPublic | BindingFlags.Static),
                typeof(SkipCutscenes).GetMethod(nameof(FixRandoMonomon))
            );
        }

        [UsedImplicitly]
        public static bool FixRandoMonomon(Func<string, bool> orig, string sceneName)
        {
            // this is really dumb
            return string.IsNullOrEmpty(sceneName) || !sceneName.StartsWith(UUMUU) && orig(sceneName);
        }

        public void Unload()
        {
            On.CinematicSequence.Begin -= CinematicBegin;
            On.FadeSequence.Begin -= FadeBegin;
            On.AnimatorSequence.Begin -= AnimatorBegin;
            On.InputHandler.SetSkipMode -= OnSetSkip;
            On.GameManager.BeginSceneTransitionRoutine -= Dreamers;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= FsmSkips;
            _rando?.Dispose();
        }

        private static void FsmSkips(Scene arg0, Scene arg1)
        {
            if (HeroController.instance == null) return;
            HeroController.instance.StartCoroutine(DreamerFsm(arg1));
            HeroController.instance.StartCoroutine(AbsRadSkip(arg1));
            HeroController.instance.StartCoroutine(HKPrimeSkip(arg1));
        }

        private static IEnumerator HKPrimeSkip(Scene arg1)
        {
            if (arg1.name != "GG_Hollow_Knight") yield break;

            yield return null;

            PlayMakerFSM control = GameObject.Find("HK Prime").LocateMyFSM("Control");

            control.ChangeTransition("Init", "FINISHED", "Intro Roar");
            control.GetAction<Wait>("Intro 2", 3).time = 0.01f;
            control.GetAction<Wait>("Intro 1", 0).time = 0.01f;
            control.GetAction<Wait>("Intro Roar", 7).time = 1f;
        }

        private static IEnumerator AbsRadSkip(Scene arg1)
        {
            if (arg1.name != "GG_Radiance") yield break;

            yield return null;

            PlayMakerFSM control = GameObject.Find("Boss Control").LocateMyFSM("Control");

            UObject.Destroy(GameObject.Find("Sun"));
            UObject.Destroy(GameObject.Find("feather_particles"));

            control.GetAction<Wait>("Setup", 6).time = 1.5f;
            control.RemoveAction("Setup", 5);
            control.RemoveAction("Setup", 4);
            control.ChangeTransition("Setup", "FINISHED", "Eye Flash");
            control.GetAction<Wait>("Title Up", 6).time = 1f;
        }

        private static IEnumerator DreamerFsm(Scene arg1)
        {
            if (!DREAMERS.Contains(arg1.name)) yield break;

            yield return null;

            GameObject.Find("Dream Enter").LocateMyFSM("Control").ChangeTransition("Idle", "DREAM HIT", "Change Scene");
        }

        private static IEnumerator Dreamers(On.GameManager.orig_BeginSceneTransitionRoutine orig, GameManager self, GameManager.SceneLoadInfo info)
        {
            info.EntryDelay = 0f;

            if (info.SceneName.Length <= 15 || info.SceneName.Substring(0, 15) != GUARDIAN)
            {
                yield return orig(self, info);
                yield break;
            }

            string @bool = info.SceneName.Substring(15);

            PlayerData.instance.SetBool($"{@bool.ToLower()}Defeated", true);
            PlayerData.instance.SetBool($"maskBroken{@bool}", true);
            PlayerData.instance.guardiansDefeated++;
            PlayerData.instance.crossroadsInfected = true;

            info.SceneName = GameManager.instance.sceneName;
            info.EntryGateName = "door_dreamReturn";

            GameCameras.instance.cameraFadeFSM.Fsm.Event("FADE INSTANT");

            yield return orig(self, info);

            GameCameras.instance.cameraFadeFSM.Fsm.SetState("FadeIn");

            yield return null;

            HeroController.instance.MaxHealth();

            while (GameManager.instance.gameState != GameState.PLAYING)
            {
                yield return null;
            }

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