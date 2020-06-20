using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HutongGames.PlayMaker;
using UnityEngine.SceneManagement;
using HutongGames.PlayMaker.Actions;
using ModCommon.Util;

namespace QoL
{
    [UsedImplicitly]
    public class FixLeverSkips : FauxMod
    {
        public override void Initialize()
        {
            On.PlayMakerFSM.OnEnable += FixLevers;
        }

        private static void FixLevers(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            if (self.FsmName == "Switch Control" && self.gameObject.name.Contains("Ruins Lever"))
            {
                RemoveBoolTests(self.GetState("Range"));
                RemoveBoolTests(self.GetState("Check If Nail"));
            }
        }

        private static void RemoveBoolTests(FsmState state)
        {
            List<FsmStateAction> actions = state.Actions.ToList();
            state.Actions = actions.Where(a => !(a is BoolTest test)).ToArray();
        }

    }
}
