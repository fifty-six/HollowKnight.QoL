using JetBrains.Annotations;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using QoL.Util;

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

        private static void FixGrubBottle(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            if (self.FsmName == "Bottle Control" && self.GetState("Shatter") is FsmState shatter)
            {
                shatter.RemoveAllOfType<BoolTest>();
            }
            
            orig(self);
        }
    }
}
