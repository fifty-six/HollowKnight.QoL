using System.Reflection;
using InControl;
using JetBrains.Annotations;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace QoL.Modules
{
    [UsedImplicitly]
    public class FixGodhomeMenus : FauxMod
    {
        private ILHook _hook;

        public override void Initialize()
        {
            Unload();
            MethodInfo methodInfo = typeof(HollowKnightInputModule).GetMethod("SendButtonEventToSelectedObject", BindingFlags.NonPublic | BindingFlags.Instance);
            _hook = new ILHook(methodInfo, SendButtonEventToSelectedObject);
        }

        public override void Unload()
        {
            _hook?.Dispose();
        }

        private static void SendButtonEventToSelectedObject(ILContext il)
        {
            ILCursor c = new ILCursor(il).Goto(0);
            ILLabel checkFlag = c.DefineLabel();

            bool success = c.TryGotoNext
            (
                MoveType.After,
                i => i.MatchLdnull(),
                i => i.MatchCall<BindingSource>("op_Inequality"),
                i => i.MatchBrfalse(out _)
            );

            if (success)
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit<HollowKnightInputModule>(OpCodes.Call, "get_AttackAction");
                c.Emit<OneAxisInputControl>(OpCodes.Callvirt, "get_WasPressed");
                c.Emit(OpCodes.Brfalse, checkFlag);
                c.GotoNext
                (
                    i => i.MatchLdloc(2),
                    i => i.MatchBrfalse(out _)
                );
                c.MarkLabel(checkFlag);
            }
        }
    }
}