using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using Modding;
using UnityEngine;
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

        public override void Initialize()
        {
            On.HeroController.CanOpenInventory += CanOpenInventory;
            On.HeroController.CanQuickMap += CanQuickMap;
            On.TutorialEntryPauser.Start += AllowPause;
            On.HeroController.ShouldHardLand += CanHardLand;
            On.PlayMakerFSM.OnEnable += ModifyFSM;
            On.InputHandler.Update += EnableSuperslides;
            ModHooks.Instance.ObjectPoolSpawnHook += OnObjectPoolSpawn;
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
        }

        private static void AllowPause(On.TutorialEntryPauser.orig_Start orig, TutorialEntryPauser self)
        {
            HeroController.instance.isEnteringFirstLevel = false;
        }

        private static bool CanQuickMap(On.HeroController.orig_CanQuickMap orig, HeroController self)
        {
            return Storage
                ? !GameManager.instance.isPaused
                && !self.cState.onConveyor
                && !self.cState.dashing
                && !self.cState.backDashing
                && (!self.cState.attacking || ReflectionHelper.GetAttr<HeroController, float>(self, "attack_time") >= self.ATTACK_RECOVERY_TIME)
                && !self.cState.recoiling
                && !self.cState.hazardDeath
                && !self.cState.hazardRespawning
                : orig(self);
        }

        private static bool CanOpenInventory(On.HeroController.orig_CanOpenInventory orig, HeroController self)
        {
            return MenuDrop
                ? !GameManager.instance.isPaused
                && !self.controlReqlinquished
                && !self.cState.recoiling
                && !self.cState.transitioning
                && !self.cState.hazardDeath
                && !self.cState.hazardRespawning
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
                int timeSlowedCount = ReflectionHelper.GetAttr<GameManager, int>(GameManager.instance, "timeSlowedCount");
                ReflectionHelper.SetAttr(GameManager.instance, "timeSlowedCount", 0);

                orig(self);

                // Restore to old value
                ReflectionHelper.SetAttr(GameManager.instance, "timeSlowedCount", timeSlowedCount);
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
    }
}