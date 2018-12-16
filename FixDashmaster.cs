using System.Reflection;
using InControl;
using JetBrains.Annotations;
using Modding;

namespace QoL
{
    [UsedImplicitly]
    public class FixDashmaster : Mod, ITogglableMod
    {
        private static readonly FieldInfo HERO_INPUT_HANDLER = typeof(HeroController).GetField("inputHandler", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo ONE_AXIS_ENABLED   = typeof(OneAxisInputControl).GetField("Enabled", BindingFlags.NonPublic | BindingFlags.Instance);
        
        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public override void Initialize() => Hook();

        private static void KillDiagonals(On.HeroController.orig_HeroDash orig, HeroController self)
        {
            var input = (InputHandler) HERO_INPUT_HANDLER.GetValue(self);

            if (input.inputActions.left.IsPressed || input.inputActions.right.IsPressed)
            {
                bool downEnabled = (bool) ONE_AXIS_ENABLED.GetValue(input.inputActions.down);

                ONE_AXIS_ENABLED.SetValue(input.inputActions.down, false);

                orig(self);

                ONE_AXIS_ENABLED.SetValue(input.inputActions.down, downEnabled);
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

        void ITogglableMod.Unload() => UnHook();
    }
}