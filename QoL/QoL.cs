using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Modding;
using QoL.Modules;
using Vasi;

namespace QoL
{
    [UsedImplicitly]
    public class QoL : Mod, ITogglableMod, IGlobalSettings<Settings>, ICustomMenuMod
    {
        public override string GetVersion() => VersionUtil.GetVersion<QoL>();

        internal static Settings GlobalSettings { get; private set; } = new();

        private static readonly List<FauxMod> _FauxMods = new();

        // So that UnencryptedSaves' BeforeSavegameSave runs last, showing all Mod settings.
        public override int LoadPriority() => int.MaxValue;

        bool ICustomMenuMod.ToggleButtonInsideMenu => true;

        public MenuScreen GetMenuScreen(MenuScreen returnScreen, ModToggleDelegates? dels)
        {
            return ModMenu.GetMenuScreen(returnScreen, dels!.Value);
        }

        public void OnLoadGlobal(Settings? s) => GlobalSettings = s ?? GlobalSettings;

        public Settings OnSaveGlobal() => GlobalSettings;

        public override void Initialize()
        {
            foreach (Type t in Assembly.GetAssembly(typeof(QoL)).GetTypes().Where(x => x.IsSubclassOf(typeof(FauxMod))))
            {
                var fm = (FauxMod) Activator.CreateInstance(t);

                // If Disable isn't overridden then it can't be toggled.
                bool cantDisable = t.GetMethod(nameof(FauxMod.Unload))?.DeclaringType == typeof(FauxMod);

                if
                (
                    !SettingsOverride.TryGetModuleOverride(t.Name, out bool enabled)
                    && !GlobalSettings.EnabledModules.TryGetValue(t.Name, out enabled)
                )
                {
                    enabled = fm.DefaultState;

                    GlobalSettings.EnabledModules.Add(t.Name, enabled);
                }

                if (cantDisable)
                    enabled = true;

                if (enabled)
                {
                    fm.Initialize();
                    fm.IsLoaded = true;
                }

                _FauxMods.Add(fm);
            }
        }

        internal static void ToggleModule(string name, bool enable)
        {
            FauxMod? fm = _FauxMods.FirstOrDefault(f => f.GetType().Name == name);

            if (fm == null || fm.IsLoaded == enable)
                return;

            if (enable)
            {
                fm.Initialize();
                fm.IsLoaded = true;
                GlobalSettings.EnabledModules[name] = true;
            }
            else
            {
                fm.Unload();
                fm.IsLoaded = false;
                GlobalSettings.EnabledModules[name] = false;
            }
        }


        public void Unload()
        {
            foreach (FauxMod fm in _FauxMods.Where(x => x.IsLoaded))
            {
                fm.Unload();
            }
        }
    }
}