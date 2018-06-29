using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using GlobalEnums;
using Modding;
using System.Reflection;

namespace QoL
{
    public class SpeedBroke : Mod, ITogglableMod
    {
        public override void Initialize()
        {
            On.HeroController.CanOpenInventory += MenuDrop;
            On.HeroController.CanQuickMap += CanQuickMap;
        }

        public void Unload()
        {
            On.HeroController.CanOpenInventory -= MenuDrop;
            On.HeroController.CanQuickMap -= CanQuickMap;
        }

        private bool CanQuickMap(On.HeroController.orig_CanQuickMap orig, HeroController self)
        {
            return !GameManager.instance.isPaused && !self.cState.onConveyor && !self.cState.dashing &&
                   !self.cState.backDashing && (!self.cState.attacking || self.GetAttr<float?>("attack_time") >= self.ATTACK_RECOVERY_TIME) &&
                   !self.cState.recoiling && !self.cState.hazardDeath &&
                   !self.cState.hazardRespawning;

        }

        private bool MenuDrop(On.HeroController.orig_CanOpenInventory orig, HeroController self)
        {
            return (!GameManager.instance.isPaused && !self.controlReqlinquished && !self.cState.recoiling && !self.cState.transitioning && !self.cState.hazardDeath && !self.cState.hazardRespawning && !self.playerData.disablePause && self.CanInput()) || self.playerData.atBench;
        }
    }
}
