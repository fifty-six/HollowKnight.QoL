using JetBrains.Annotations;
using ModCommon.Util;
using UnityEngine;

namespace QoL
{
    [UsedImplicitly]
    public class SpeedBroke : FauxMod
    {
        [SerializeToSetting]
        [UsedImplicitly]
        public static bool EnableMenuDrop = true;

        [SerializeToSetting]
        [UsedImplicitly]
        public static bool Storage = true;

        [SerializeToSetting]
        [UsedImplicitly]
        public static bool NoHardFalls;

        public override void Initialize()
        {
            On.HeroController.CanOpenInventory += MenuDrop;
            On.HeroController.CanQuickMap += CanQuickMap;
            On.TutorialEntryPauser.Start += AllowPause;
            On.HeroController.ShouldHardLand += CanHardLand;
            On.PlayMakerFSM.OnEnable += NoKPHardFall;
        }

        public override void Unload()
        {
            On.HeroController.CanOpenInventory -= MenuDrop;
            On.HeroController.CanQuickMap -= CanQuickMap;
            On.TutorialEntryPauser.Start -= AllowPause;
            On.HeroController.ShouldHardLand -= CanHardLand;
            On.PlayMakerFSM.OnEnable -= NoKPHardFall;
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

        private static bool MenuDrop(On.HeroController.orig_CanOpenInventory orig, HeroController self)
        {
            return EnableMenuDrop
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

        private static bool CanHardLand(On.HeroController.orig_ShouldHardLand orig, HeroController self, Collision2D collision)
        {
            return !NoHardFalls && orig(self, collision);
        }

        private static void NoKPHardFall(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            if (!NoHardFalls || self.name != "Initial Fall Impact" || self.FsmName != "Control")
            {
                orig(self);
                return;
            }

            self.ChangeTransition("Idle", "LAND", "Return Control");
        }
    }
}