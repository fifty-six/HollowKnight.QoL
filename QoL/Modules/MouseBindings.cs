using InControl;
using JetBrains.Annotations;

namespace QoL.Modules
{
    [UsedImplicitly]
    public class MouseBindings : FauxMod
    {
        // F13, F14, and F15
        // are 
        // Button4, Button5, Button6
        public override void Initialize()
        {
            On.InputHandler.AddKeyBinding += AddMouseBindings;
            On.MappableKey.OnBindingAdded += ConvertMouseBindingToKey;
        }

        public override void Unload()
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