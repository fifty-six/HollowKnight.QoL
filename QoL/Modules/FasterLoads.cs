using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace QoL.Modules
{
    [UsedImplicitly]
    public class FasterLoads : FauxMod
    {
        private static readonly float[] SKIP = {0.4f, .165f};
        
        private ILHook _hook;

        public override void Initialize()
        {
            Unload();
            
            Type type = typeof(HeroController).GetNestedType("<EnterScene>c__Iterator0", BindingFlags.NonPublic | BindingFlags.Instance);
            
            _hook = new ILHook
            (
                type.GetMethod("MoveNext"),
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
                if (!(c.Instrs[c.Index].Operand is float f)) continue;

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