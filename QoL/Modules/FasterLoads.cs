using System.Linq;
using JetBrains.Annotations;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using UnityEngine;

namespace QoL.Modules
{
    [UsedImplicitly]
    public class FasterLoads : FauxMod
    {
        private static readonly float[] SKIP = { 0.4f, .165f };

        private ILHook? _hook;

        public override void Initialize()
        {
            Unload();

            _hook = new ILHook
            (
                typeof(HeroController).GetMethod(nameof(HeroController.EnterScene)).GetStateMachineTarget(),
                EnterScene
            );
        }

        private static void EnterScene(ILContext il)
        {
            ILCursor c = new ILCursor(il).Goto(0);

            while (c.TryGotoNext
            (
                i => i.OpCode == OpCodes.Ldc_R4,
                i => i.OpCode == OpCodes.Newobj && i.MatchNewobj<WaitForSeconds>()
            ))
            {
                if (c.Instrs[c.Index].Operand is not float f) continue;

                if (!SKIP.Contains(f)) continue;

                c.Remove();
                c.Remove();

                // convert to yield return null
                c.Emit(OpCodes.Ldnull);
            }
        }

        public override void Unload()
        {
            _hook?.Dispose();
        }
    }
}