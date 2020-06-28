using ModCommon.Util;
using UnityEngine;

using ReflectionHelper = Modding.ReflectionHelper;

namespace QoL
{
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
        public static bool NoHardFalls;

        public override void Initialize()
        {
            On.HeroController.CanOpenInventory += CanOpenInventory;
            On.HeroController.CanQuickMap += CanQuickMap;
            On.TutorialEntryPauser.Start += AllowPause;
            On.HeroController.ShouldHardLand += CanHardLand;
            On.PlayMakerFSM.OnEnable += ModifyFSM;
            On.InputHandler.Update += EnableSuperslides;
        }

        public override void Unload()
        {
            On.HeroController.CanOpenInventory -= CanOpenInventory;
            On.HeroController.CanQuickMap -= CanQuickMap;
            On.TutorialEntryPauser.Start -= AllowPause;
            On.HeroController.ShouldHardLand -= CanHardLand;
            On.PlayMakerFSM.OnEnable -= ModifyFSM;
            On.InputHandler.Update -= EnableSuperslides;
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
                && (!self.cState.attacking || self.GetAttr<HeroController, float>("attack_time") >= self.ATTACK_RECOVERY_TIME)
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
            }

            orig(self);
        }
    }
}