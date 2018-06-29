using System;
using System.Reflection;
using HutongGames.PlayMaker.Actions;
using Modding;
using FsmUtil;
using InControl;
using On.InControl.NativeProfile;
using UnityEngine;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace QoL
{
    public class MouseBindings : Mod
    {
        // F13, F14, F15
        // are 
        // Button4, Button5, Button6
        // cause tc was high when they wrote this shit
        public override void Initialize()
        {
            // On.InControl.PlayerAction.ctor += Cancer;
            On.InputHandler.AddKeyBinding += TcWhy;
            On.MappableKey.OnBindingAdded += Reeee;
        }

        private void Reeee(On.MappableKey.orig_OnBindingAdded orig, MappableKey self, PlayerAction action, BindingSource binding)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (binding.Name)
            {
                case "Button4":
                    action.AddBinding(new KeyBindingSource(Key.F13));
                    break;
                case "Button5":
                    action.AddBinding(new KeyBindingSource(Key.F14));
                    break;
                case "Button6":
                    action.AddBinding(new KeyBindingSource(Key.F15));
                    break;
            }

            orig(self, action, binding);
        }

        private void TcWhy(On.InputHandler.orig_AddKeyBinding orig, PlayerAction action, Key key)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (key)
            {
                case Key.F13:
                    action.AddBinding(new MouseBindingSource(Mouse.Button4));
                    break;
                case Key.F14:
                    action.AddBinding(new MouseBindingSource(Mouse.Button5));
                    break;
                case Key.F15:
                    action.AddBinding(new MouseBindingSource(Mouse.Button6));
                    break;
            }

            orig(action, key);
        }
    }
}