using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace QoL
{
    [UsedImplicitly]
    public class HitGrubsThroughWalls : FauxMod
    {
        public override void Initialize()
        {
            On.PlayMakerFSM.OnEnable += FixGrubBottle;
        }

        public override void Unload()
        {
            On.PlayMakerFSM.OnEnable -= FixGrubBottle;
        }

        private void FixGrubBottle(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            if (self.FsmName == "Bottle Control" && self.GetState("Shatter") is FsmState shatter)
            {
                RemoveBoolTests(shatter);
            }
        }

        private static void RemoveBoolTests(FsmState state)
        {
            List<FsmStateAction> actions = state.Actions.ToList();
            state.Actions = actions.Where(a => !(a is BoolTest test)).ToArray();
        }
    }
}
