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
    public class QoL : Mod, ITogglableMod, IGlobalSettings<Settings>, IMenuMod
    {
        public override string GetVersion() => VersionUtil.GetVersion<QoL>();

        internal static Settings _globalSettings = new();

        private static readonly List<FauxMod> _fauxMods = new();


        // So that UnencryptedSaves' BeforeSavegameSave runs last, showing all Mod settings.
        public override int LoadPriority() => int.MaxValue;

        bool IMenuMod.ToggleButtonInsideMenu => true;

        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? button) 
        {
            List<IMenuMod.MenuEntry> li = new() { button ?? throw new NullReferenceException(nameof(button)) };

            string[] bools = { "false", "true" };
 
             foreach ((FieldInfo fi, Type t) in _globalSettings.Fields)
             {
                 if (fi.FieldType != typeof(bool) || SettingsOverride.TryGetSettingOverride($"{t.Name}:{fi.Name}", out _))
                     continue;
 
                 li.Add
                 (
                     new IMenuMod.MenuEntry
                     (
                         Regex.Replace(fi.Name, "([A-Z])", " $1").TrimEnd(),
                         bools,
                         $"Comes from {t.Name}",
                         i => fi.SetValue(null, Convert.ToBoolean(i)),
                         () => Convert.ToInt32(fi.GetValue(null))
                     )
                 );
             }
 
             return li;   
        }

        public void OnLoadGlobal(Settings? s) => _globalSettings = s ?? _globalSettings;

        public Settings OnSaveGlobal() => _globalSettings;
        
        public override void Initialize()
        {
            foreach (Type t in Assembly.GetAssembly(typeof(QoL)).GetTypes().Where(x => x.IsSubclassOf(typeof(FauxMod))))
            {
                var fm = (FauxMod) Activator.CreateInstance(t);

                // If Disable isn't overridden then it can't be toggled.
                bool cantDisable = t.GetMethod(nameof(FauxMod.Unload))?.DeclaringType == typeof(FauxMod);

                if (!SettingsOverride.TryGetModuleOverride(t.Name, out bool enabled)
                    && !_globalSettings.EnabledModules.TryGetValue(t.Name, out enabled))
                {
                    enabled = fm.DefaultState;

                    _globalSettings.EnabledModules.Add(t.Name, enabled);
                }

                if (cantDisable)
                    enabled = true;

                if (enabled)
                {
                    fm.Initialize();
                    fm.IsLoaded = true;
                }

                _fauxMods.Add(fm);
            }
        }

        internal static void ToggleModule(string name, bool enable)
        {
            var fm = _fauxMods.FirstOrDefault(f => f.GetType().Name == name);
            if (fm != null && fm.IsLoaded != enable)
            {
                if (enable)
                {
                    fm.Initialize();
                    fm.IsLoaded = true;
                    _globalSettings.EnabledModules[name] = true;
                }
                else
                {
                    fm.Unload();
                    fm.IsLoaded = false;
                    _globalSettings.EnabledModules[name] = false;
                }
            }
        }


        public void Unload()
        {
            foreach (FauxMod fm in _fauxMods.Where(x => x.IsLoaded))
            {
                fm.Unload();
            }
        }
    }
}