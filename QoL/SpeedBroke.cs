using System;
using System.Collections;
using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vasi;
using ReflectionHelper = Modding.ReflectionHelper;

namespace QoL
{
    [UsedImplicitly]
    public class SpeedBroke : FauxMod
    {
        [SerializeToSetting]
        public static bool MenuDrop = true;

        [SerializeToSetting]
        public static bool Storage = true;

        [SerializeToSetting]
        public static bool Superslides = true;

        [SerializeToSetting]
        public static bool Televator = true;
        
        [SerializeToSetting]
        public static bool ExplosionPogo = true;

        [SerializeToSetting]
        public static bool GrubsThroughWalls = true;
        
        [SerializeToSetting]
        public static bool LeverSkips = true;

        [SerializeToSetting]
        public static bool NoHardFalls;
        
        [SerializeToSetting]
        public static bool ShadeSoulLeverSkip;

        public override void Initialize()
        {
            On.HeroController.CanOpenInventory += CanOpenInventory;
            On.HeroController.CanQuickMap += CanQuickMap;
            On.TutorialEntryPauser.Start += AllowPause;
            On.HeroController.ShouldHardLand += CanHardLand;
            On.PlayMakerFSM.OnEnable += ModifyFSM;
            On.InputHandler.Update += EnableSuperslides;
            ModHooks.Instance.ObjectPoolSpawnHook += OnObjectPoolSpawn;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneChanged;
            ModHooks.Instance.HitInstanceHook += CheckLeverSkip;
        }

        public override void Unload()
        {
            On.HeroController.CanOpenInventory -= CanOpenInventory;
            On.HeroController.CanQuickMap -= CanQuickMap;
            On.TutorialEntryPauser.Start -= AllowPause;
            On.HeroController.ShouldHardLand -= CanHardLand;
            On.PlayMakerFSM.OnEnable -= ModifyFSM;
            On.InputHandler.Update -= EnableSuperslides;
            ModHooks.Instance.ObjectPoolSpawnHook -= OnObjectPoolSpawn;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneChanged;
            ModHooks.Instance.HitInstanceHook -= CheckLeverSkip;
        }

        private static void AllowPause(On.TutorialEntryPauser.orig_Start orig, TutorialEntryPauser self)
        {
            HeroController.instance.isEnteringFirstLevel = false;
        }

        private static bool CanQuickMap(On.HeroController.orig_CanQuickMap orig, HeroController self)
        {
            HeroControllerStates cs = self.cState;
            
            return Storage
                ? !GameManager.instance.isPaused
                && !cs.onConveyor
                && !cs.dashing
                && !cs.backDashing
                && (!cs.attacking || ReflectionHelper.GetAttr<HeroController, float>(self, "attack_time") >= self.ATTACK_RECOVERY_TIME)
                && !cs.recoiling
                && !cs.hazardDeath
                && !cs.hazardRespawning
                : orig(self);
        }

        private static bool CanOpenInventory(On.HeroController.orig_CanOpenInventory orig, HeroController self)
        {
            HeroControllerStates cs = self.cState;
            
            return MenuDrop
                ? !GameManager.instance.isPaused
                && !self.controlReqlinquished
                && !cs.recoiling
                && !cs.transitioning
                && !cs.hazardDeath
                && !cs.hazardRespawning
                && !self.playerData.disablePause
                && self.CanInput()
                || self.playerData.atBench
                : orig(self);
        }

        private static void EnableSuperslides(On.InputHandler.orig_Update orig, InputHandler self)
        {
            if (Superslides && GameManager.instance.TimeSlowed)
            {
                // Ensure the slide has the correct speed
                ReflectionHelper.SetAttr(HeroController.instance, "recoilSteps", 0);

                // Kill the thing that kills superslides
                ref int timeSlowedCount = ref Mirror.GetFieldRef<GameManager, int>(GameManager.instance, "timeSlowedCount");

                int origCount = timeSlowedCount;

                timeSlowedCount = 0;
                
                orig(self);

                // Restore to old value
                timeSlowedCount = origCount;
            }
            else
            {
                orig(self);
            }
        }

        private static bool CanHardLand(On.HeroController.orig_ShouldHardLand orig, HeroController self, Collision2D collision)
        {
            return !NoHardFalls && orig(self, collision);
        }

        private static void ModifyFSM(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            switch (self.FsmName)
            {
                case "Control" when self.name == "Initial Fall Impact" && NoHardFalls:
                    self.ChangeTransition("Idle", "LAND", "Return Control");
                    break;

                case "Call Lever" when self.name.StartsWith("Lift Call Lever") && Televator:
                    // Don't change big elevators.
                    if (self.GetState("Check Already Called") == null) break;
                    
                    self.ChangeTransition("Left", "FINISHED", "Send Msg");
                    self.ChangeTransition("Right", "FINISHED", "Send Msg");
                    break;

                case "Bottle Control" when self.GetState("Shatter") is FsmState shatter && GrubsThroughWalls:
                {
                    shatter.RemoveAllOfType<BoolTest>();
                    break;
                }

                case "Switch Control" when self.name.Contains("Ruins Lever") && LeverSkips:
                {
                    self.GetState("Range").RemoveAllOfType<BoolTest>();
                    self.GetState("Check If Nail").RemoveAllOfType<BoolTest>();
                    break;
                }

                case "Dream Nail" when self.name == "Knight" && Storage:
                {
                    self.GetState("Cancelable").GetAction<ListenForDreamNail>().activeBool = true;
                    self.GetState("Cancelable Dash").GetAction<ListenForDreamNail>().activeBool = true;
                    self.GetState("Queuing").GetAction<ListenForDreamNail>().activeBool = true;
                    self.GetState("Queuing").RemoveAllOfType<BoolTest>();
                    break;
                }
            }

            orig(self);
        }
        
        private static GameObject OnObjectPoolSpawn(GameObject go)
        {
            if (!ExplosionPogo)
                return go;
            
            if (!go.name.StartsWith("Gas Explosion Recycle M"))
                return go;

            go.layer = (int) PhysLayers.ENEMIES;
            
            var bouncer = go.GetComponent<NonBouncer>();

            if (bouncer) 
                bouncer.active = false;

            return go;
        }
        
        private static HitInstance CheckLeverSkip(Fsm owner, HitInstance hit) {
            if (!ShadeSoulLeverSkip) return hit;

            GameManager gm = GameManager.instance;
            GameObject slash = hit.Source;
            
            // is right scene
            if (gm.sceneName != "Ruins1_31") return hit;
            // is dash slash
            if (slash.name != "Dash Slash") return hit;
            // is left direction
            if (Math.Abs(hit.Direction - 180f) > 0.1) return hit;
            // is right x pos window
            if (slash.transform.GetPositionX() < 44.6 || slash.transform.GetPositionX() > 45.0) return hit;
            // is right y pos window
            if (slash.transform.GetPositionY() < 56.4 || slash.transform.GetPositionY() > 57.0) return hit;
            
            
            PersistentBoolData lever = gm.sceneData.persistentBoolItems.Find(data => data.sceneName == "Ruins1_31" && data.id == "Ruins Lever");

            // first time entering this scene
            if (lever == null) {
                lever = new PersistentBoolData {
                    sceneName = "Ruins1_31",
                    id = "Ruins Lever",
                    activated = false
                };
                
                gm.sceneData.SaveMyState(lever);
            }
            
            // gate is already opened
            if (lever.activated) return hit;
            
            // open gate
            lever.activated = true;
            GameObject.Find("Ruins Gate").LocateMyFSM("Toll Gate").SendEvent("OPEN");
            return hit;
        }
        
        private static void SceneChanged(Scene from, Scene to) {
            if (ShadeSoulLeverSkip && to.name == "Ruins1_31") {
                HeroController.instance.StartCoroutine(ExtendWall());
            }
        }
        
        // extends a wall in Ruins1_31 to enable climbing it with claw only (like on 1221)
        private static IEnumerator ExtendWall() {
            yield return null;
            
            GameObject chunk = GameObject.Find("Chunk 1 1");
            Vector2[] newPoints = {
                new Vector2(0, 12),
                new Vector2(0, 11),
                new Vector2(12, 11),
                new Vector2(12, 12),
                new Vector2(13, 12),
                new Vector2(13, 16),
                new Vector2(21.5f, 16),
                new Vector2(21.5f, 19),
                new Vector2(23, 19),
                new Vector2(23, 23),
                new Vector2(0, 23),
                new Vector2(0, 12)
            };
                    
            foreach (EdgeCollider2D edgeCollider2D in chunk.GetComponents<EdgeCollider2D>()) {
                if (!(Math.Abs(edgeCollider2D.points[0].y - 12) < 0.1)) continue;
                edgeCollider2D.points = newPoints;
                break;
            }
        }
    }
}