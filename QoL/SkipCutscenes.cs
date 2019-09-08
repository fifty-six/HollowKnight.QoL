using System;
using System.Collections;
using System.Linq;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;

namespace QoL
{
    [UsedImplicitly]
    public class SkipCutscenes : FauxMod
    {
        private const string GUARDIAN = "Dream_Guardian_";

        private static readonly string[] DREAMERS = {"Deepnest_Spider_Town", "Fungus3_archive_02", "Ruins2_Watcher_Room"};

        private static readonly string[] ALL_DREAMER_BOOLS =
        {
            "corniferAtHome",
            "iseldaConvo1",
            "dungDefenderSleeping",
            "corn_crossroadsLeft",
            "corn_greenpathLeft",
            "corn_fogCanyonLeft",
            "corn_fungalWastesLeft",
            "corn_cityLeft",
            "corn_waterwaysLeft",
            "corn_minesLeft",
            "corn_cliffsLeft",
            "corn_deepnestLeft",
            "corn_outskirtsLeft",
            "corn_royalGardensLeft",
            "corn_abyssLeft",
            "metIselda"
        };

        public override void Initialize()
        {
            On.CinematicSequence.Begin += CinematicBegin;
            On.FadeSequence.Begin += FadeBegin;
            On.AnimatorSequence.Begin += AnimatorBegin;
            On.InputHandler.SetSkipMode += OnSetSkip;
            On.GameManager.BeginSceneTransitionRoutine += Dreamers;
            On.HutongGames.PlayMaker.Actions.EaseColor.OnEnter += EaseColorSucks;
            On.GameManager.FadeSceneInWithDelay += NoFade;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += FsmSkips;
        }

        private static IEnumerator NoFade(On.GameManager.orig_FadeSceneInWithDelay orig, GameManager self, float delay)
        {
            yield return orig(self, 0);
        }

        private static void EaseColorSucks(On.HutongGames.PlayMaker.Actions.EaseColor.orig_OnEnter orig, EaseColor self)
        {
            if (self.Owner.name == "Blanker White" && Math.Abs(self.time.Value - 0.3) < .05)
            {
                self.time.Value = 0.066f;
            }

            orig(self);
        }

        public override void Unload()
        {
            On.CinematicSequence.Begin -= CinematicBegin;
            On.FadeSequence.Begin -= FadeBegin;
            On.AnimatorSequence.Begin -= AnimatorBegin;
            On.InputHandler.SetSkipMode -= OnSetSkip;
            On.GameManager.BeginSceneTransitionRoutine -= Dreamers;
            On.HutongGames.PlayMaker.Actions.EaseColor.OnEnter -= EaseColorSucks;
            On.GameManager.FadeSceneInWithDelay -= NoFade;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= FsmSkips;
        }

        private static void FsmSkips(Scene arg0, Scene arg1)
        {
            if (HeroController.instance == null) return;

            HeroController.instance.StartCoroutine(DreamerFsm(arg1));
            HeroController.instance.StartCoroutine(AbsRadSkip(arg1));
            HeroController.instance.StartCoroutine(HKPrimeSkip(arg1));
            HeroController.instance.StartCoroutine(StatueWait(arg1));
        }

        private static IEnumerator StatueWait(Scene arg1)
        {
            if (arg1.name != "GG_Workshop") yield break;

            foreach (PlayMakerFSM fsm in UObject.FindObjectsOfType<PlayMakerFSM>().Where(x => x.FsmName == "GG Boss UI"))
            {
                fsm.ChangeTransition("On Left", "FINISHED", "Dream Box Down");
                fsm.ChangeTransition("On Right", "FINISHED", "Dream Box Down");

                fsm.InsertAction("Dream Box Down", fsm.GetAction<SetPlayerDataString>("Impact", 2), 0);
            }
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

            PlayerData pd = PlayerData.instance;
            
            pd.SetBool($"{@bool.ToLower()}Defeated", true);
            pd.SetBool($"maskBroken{@bool}", true);
            pd.guardiansDefeated++;
            pd.crossroadsInfected = true;

            if (pd.guardiansDefeated == 3)
            {
                pd.mrMushroomState = 1;
                pd.brettaState++;
                
                foreach(string pdBool in ALL_DREAMER_BOOLS)
                    pd.SetBool(pdBool, true);
            }

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

            PlayMakerFSM.BroadcastEvent("BOX DOWN");
            PlayMakerFSM.BroadcastEvent("BOX DOWN DREAM");

            pd.SetBenchRespawn(UObject.FindObjectOfType<RespawnMarker>(), GameManager.instance.sceneName, 2);
            GameManager.instance.SaveGame();

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