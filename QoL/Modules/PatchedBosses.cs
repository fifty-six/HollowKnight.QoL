using System;
using System.Collections;
using System.Linq;
using HutongGames.PlayMaker;
using MonoMod.Cil;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vasi;

namespace QoL.Modules
{
    public class PatchedBosses : FauxMod
    {
        [SerializeToSetting]
        public static bool SporeShroomHollowKnight = true;

        [SerializeToSetting]
        public static bool NoBackrolls;

        [SerializeToSetting]
        public static bool SleepyCrystalGuardian;


        public override void Initialize()
        {
            IL.ExtraDamageable.RecieveExtraDamage += AllowRecieveExtraDamage;
            On.DamageEffectTicker.OnTriggerExit2D += KeepDamagingInactives;

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += BossFsmChanges;
        }

        public override void Unload()
        {
            IL.ExtraDamageable.RecieveExtraDamage -= AllowRecieveExtraDamage;
            On.DamageEffectTicker.OnTriggerExit2D -= KeepDamagingInactives;

            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= BossFsmChanges;
        }


        private void BossFsmChanges(Scene scene, LoadSceneMode lsm)
        {
            switch (scene.name)
            {
                case "Ruins2_03_boss" when NoBackrolls:
                case "GG_Watcher_Knights" when NoBackrolls:
                    GameManager.instance.StartCoroutine(FixWatchers(scene));
                    break;
                case "Mines_18_boss" when SleepyCrystalGuardian:
                    GameManager.instance.StartCoroutine(FixCrystalGuardian());
                    break;
            }
        }
        private IEnumerator FixWatchers(Scene scene)
        {
            yield return null;

            GameObject battleControl = scene.GetRootGameObjects().First(obj => obj.name == "Battle Control");

            foreach (PlayMakerFSM pfsm in battleControl.GetComponentsInChildren<PlayMakerFSM>())
            {
                if (pfsm.FsmName == "Black Knight")
                    pfsm.GetState("In Range Choice").RemoveTransition("In Range Double");
            }
        }
        private IEnumerator FixCrystalGuardian()
        {
            yield return null;

            GameObject miner = GameObject.Find("Mega Zombie Beam Miner (1)");

            if (miner.LocateMyFSM("Beam Miner").TryGetState("Sleep", out FsmState? sleep))
                sleep.Transitions = sleep.Transitions.Where(x => x.FsmEvent.Name != "EXTRA DAMAGED" && x.FsmEvent.Name != "TOOK DAMAGE").ToArray();
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
                cursor.EmitDelegate<Func<bool, bool>>(x => x && !SporeShroomHollowKnight);
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
