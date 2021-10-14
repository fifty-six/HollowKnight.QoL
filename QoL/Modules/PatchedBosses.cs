using System;
using System.Linq;
using HutongGames.PlayMaker;
using MonoMod.Cil;
using UnityEngine;
using Vasi;

namespace QoL.Modules
{
    public class PatchedBosses : FauxMod
    {
        [SerializeToSetting]
        public static bool SporeShroomHollowKnight = true;

        [SerializeToSetting]
        public static bool NoBackrolls = false;

        [SerializeToSetting]
        public static bool SleepyCrystalGuardian = false;


        public override void Initialize()
        {
            IL.ExtraDamageable.RecieveExtraDamage += AllowRecieveExtraDamage;
            On.DamageEffectTicker.OnTriggerExit2D += KeepDamagingInactives;

            On.PlayMakerFSM.OnEnable += BossFsmChanges;
        }

        public override void Unload()
        {
            IL.ExtraDamageable.RecieveExtraDamage -= AllowRecieveExtraDamage;
            On.DamageEffectTicker.OnTriggerExit2D -= KeepDamagingInactives;

            On.PlayMakerFSM.OnEnable -= BossFsmChanges;
        }

        private void BossFsmChanges(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            if (self.FsmName == "Black Knight" && NoBackrolls)
            {
                self.GetState("In Range Choice").RemoveTransition("In Range Double");
            }
            if (self.FsmName == "Beam Miner" && self.gameObject.name.StartsWith("Mega Zombie Beam Miner") && SleepyCrystalGuardian)
            {
                if (!self.TryGetState("Sleep", out FsmState? sleep))
                {
                    return;
                }
                sleep.Transitions = sleep.Transitions.Where(x => x.FsmEvent.Name != "EXTRA DAMAGED" && x.FsmEvent.Name != "TOOK DAMAGE").ToArray();
            }
        }

        private void AllowRecieveExtraDamage(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext
            (
                MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<ExtraDamageable>("damagedThisFrame")
            ))
            {
                cursor.EmitDelegate<Func<bool, bool>>(x => x & !SporeShroomHollowKnight);
            }
        }

        private void KeepDamagingInactives(On.DamageEffectTicker.orig_OnTriggerExit2D orig, DamageEffectTicker self, Collider2D otherCollider)
        {
            if (!otherCollider.gameObject.activeSelf && SporeShroomHollowKnight)
                return;
            orig(self, otherCollider);
        }
    }
}
