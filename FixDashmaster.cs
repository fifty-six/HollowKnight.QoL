using System.Reflection;
using InControl;
using JetBrains.Annotations;
using Modding;

namespace QoL
{
    [UsedImplicitly]
    public class FixDashmaster : Mod, ITogglableMod
    {
        private static FieldInfo heroInputHandler = typeof(HeroController).GetField("inputHandler", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo oneAxisEnabled = typeof(OneAxisInputControl).GetField("Enabled", BindingFlags.NonPublic | BindingFlags.Instance);

        public override void Initialize()
        {
            Hook();
        }

        private void KillDiagonals(On.HeroController.orig_HeroDash orig, HeroController self)
        {
            InputHandler input = (InputHandler)heroInputHandler.GetValue(self);
            if (input.inputActions.left.IsPressed || input.inputActions.right.IsPressed)
            {
                bool downEnabled = (bool)oneAxisEnabled.GetValue(input.inputActions.down);

                oneAxisEnabled.SetValue(input.inputActions.down, false);

                orig(self);

                oneAxisEnabled.SetValue(input.inputActions.down, downEnabled);
            }
            else
            {
                orig(self);
            }
        }

        private void Hook()
        {
            UnHook();
            On.HeroController.HeroDash += KillDiagonals;
        }

        private void UnHook()
        {
            On.HeroController.HeroDash -= KillDiagonals;
        }

        void ITogglableMod.Unload() => UnHook();
    }
}
