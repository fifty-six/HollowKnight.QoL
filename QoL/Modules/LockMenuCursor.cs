using System;
using System.Reflection;
using JetBrains.Annotations;
using Modding;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using UnityEngine;

namespace QoL.Modules
{
    [UsedImplicitly]
    public class LockMenuCursor : FauxMod
    {

        public LockMenuCursor() : base(false) { }

        public override void Initialize()
        {
            IL.InputHandler.OnGUI += OnGUI;

            HookEndpointManager.Modify
            (
                MethodBase.GetMethodFromHandle(typeof(ModHooks).GetMethod("OnCursor", BindingFlags.Instance | BindingFlags.NonPublic).MethodHandle),
                (ILContext.Manipulator) OnCursor
            );
        }

        public override void Unload()
        {
            IL.InputHandler.OnGUI -= OnGUI;

            HookEndpointManager.Unmodify
            (
                MethodBase.GetMethodFromHandle(typeof(ModHooks).GetMethod("OnCursor", BindingFlags.Instance | BindingFlags.NonPublic).MethodHandle),
                (ILContext.Manipulator) OnCursor
            );
        }

        private static void OnCursor(ILContext il)
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

        private static void OnGUI(ILContext il)
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