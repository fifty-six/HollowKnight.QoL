using System.Reflection;
using Modding;
using InControl;
using JetBrains.Annotations;
using UObject = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace QoL
{
    [UsedImplicitly]
    public class MouseBindings : Mod, ITogglableMod
    {
        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        
        // F13, F14, F15
        // are 
        // Button4, Button5, Button6
        public override void Initialize()
        {
            On.InputHandler.AddKeyBinding += AddMouseBindings;
            On.MappableKey.OnBindingAdded += ConvertMouseBindingToKey;
        }

        public void Unload()
        {
            On.InputHandler.AddKeyBinding -= AddMouseBindings;
            On.MappableKey.OnBindingAdded -= ConvertMouseBindingToKey;
        }

        private static void ConvertMouseBindingToKey(On.MappableKey.orig_OnBindingAdded orig, MappableKey self, PlayerAction action, BindingSource binding)
        {
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

        private static void AddMouseBindings(On.InputHandler.orig_AddKeyBinding orig, PlayerAction action, Key key)
        {
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