using InControl;
using JetBrains.Annotations;
using Modding;

namespace QoL
{
    [UsedImplicitly]
    public class FixDashmaster : FauxMod
    {
        public override void Initialize() => Hook();

        private static void KillDiagonals(On.HeroController.orig_HeroDash orig, HeroController self)
        {
            InputHandler input = ReflectionHelper.GetAttr<HeroController, InputHandler>(HeroController.instance, "inputHandler");

            if (input.inputActions.left.IsPressed || input.inputActions.right.IsPressed)
            {
                bool downEnabled = ReflectionHelper.GetAttr<OneAxisInputControl, bool>(input.inputActions.down, "Enabled");

                ReflectionHelper.SetAttr(input.inputActions.down, "Enabled", false);

                orig(self);

                ReflectionHelper.SetAttr(input.inputActions.down, "Enabled", downEnabled);
            }
            else
            {
                orig(self);
            }
        }

        private static void Hook()
        {
            UnHook();
            On.HeroController.HeroDash += KillDiagonals;
        }

        private static void UnHook()
        {
            On.HeroController.HeroDash -= KillDiagonals;
        }

        public override void Unload() => UnHook();
    }
}