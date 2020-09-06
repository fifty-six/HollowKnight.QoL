using InControl;
using JetBrains.Annotations;
using Modding;
using Vasi;

namespace QoL.Modules
{
    [UsedImplicitly]
    public class FixDashmaster : FauxMod
    {
        public override void Initialize()
        {
            Unload();
            
            On.HeroController.HeroDash += KillDiagonals;
        }

        private static void KillDiagonals(On.HeroController.orig_HeroDash orig, HeroController self)
        {
            InputHandler input = ReflectionHelper.GetAttr<HeroController, InputHandler>(HeroController.instance, "inputHandler");

            if (input.inputActions.left.IsPressed || input.inputActions.right.IsPressed)
            {
                ref bool down_enabled = ref Mirror.GetFieldRef<OneAxisInputControl, bool>(input.inputActions.down, "Enabled");

                bool orig_enabled = down_enabled;

                down_enabled = false;

                orig(self);

                down_enabled = orig_enabled;
            }
            else
            {
                orig(self);
            }
        }

        public override void Unload()
        {
            On.HeroController.HeroDash -= KillDiagonals;
        }
    }
}