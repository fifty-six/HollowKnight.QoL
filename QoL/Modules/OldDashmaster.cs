using InControl;
using JetBrains.Annotations;
using MonoMod.Cil;

namespace QoL.Modules
{
    [UsedImplicitly]
    public class OldDashmaster : FauxMod
    {
        public OldDashmaster() : base(false) { }

        public override void Initialize()
        {
            Unload();

            IL.HeroController.HeroDash += HeroDash;
        }

        private void HeroDash(ILContext il)
        {
            var cursor = new ILCursor(il);

            while (cursor.TryGotoNext
            (
                i => i.MatchLdloc(0),
                i => i.MatchLdfld<HeroActions>("left") || i.MatchLdfld<HeroActions>("right"),
                i => i.MatchCallvirt<OneAxisInputControl>("get_IsPressed"),
                i => i.MatchBrtrue(out _)
            ))
            {
                cursor.RemoveRange(4);
            }
        }

        public override void Unload()
        {
            IL.HeroController.HeroDash -= HeroDash;
        }
    }
}