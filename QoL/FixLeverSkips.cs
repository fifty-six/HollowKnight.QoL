using JetBrains.Annotations;
using HutongGames.PlayMaker.Actions;
using QoL.Util;

namespace QoL
{
    [UsedImplicitly]
    public class FixLeverSkips : FauxMod
    {
        public override void Initialize()
        {
            On.PlayMakerFSM.OnEnable += FixLevers;
        }

        public override void Unload()
        {
            On.PlayMakerFSM.OnEnable -= FixLevers;
        }

        private static void FixLevers(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            if (self.FsmName == "Switch Control" && self.gameObject.name.Contains("Ruins Lever"))
            {
                self.GetState("Range").RemoveAllOfType<BoolTest>();
                self.GetState("Check If Nail").RemoveAllOfType<BoolTest>();
            }
            
            orig(self);
        }
    }
}
