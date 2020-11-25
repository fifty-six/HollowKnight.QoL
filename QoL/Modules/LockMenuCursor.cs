using System;
using System.Reflection;
using JetBrains.Annotations;
using Modding;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using UnityEngine;

namespace QoL.Modules
{
    [UsedImplicitly]
    public class LockMenuCursor : FauxMod
    {
        private ILHook _hook;

        public LockMenuCursor() : base(false) { }

        public override void Initialize()
        {
            IL.InputHandler.OnGUI += ReplaceLockState;

            _hook = new ILHook
            (
                typeof(ModHooks).GetMethod("OnCursor", BindingFlags.Instance | BindingFlags.NonPublic),
                ReplaceLockState
            );
        }

        public override void Unload()
        {
            IL.InputHandler.OnGUI -= ReplaceLockState;

            _hook?.Dispose();
        }

        private static void ReplaceLockState(ILContext il)
        {
            ILCursor cursor = new ILCursor(il).Goto(0);

            while
            (
                cursor.TryFindNext
                (
                    out ILCursor[] cursors,
                    instr => instr.MatchLdcI4(0),
                    instr => instr.MatchCall(typeof(Cursor), "set_lockState")
                )
            )
            {
                cursors[0].Remove();
                cursors[0].EmitDelegate<Func<int>>(() => (int) CursorLockMode.Confined);
            }
        }
    }
}