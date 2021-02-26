using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Modding;
using QoL.Modules;

namespace QoL
{
    [UsedImplicitly]
    public class QoL : Mod, ITogglableMod
    {
        public override string GetVersion() => Vasi.VersionUtil.GetVersion<QoL>();

        public override ModSettings GlobalSettings
        {
            get => _globalSettings;
            set => _globalSettings = (Settings) value;
        }
        
        private Settings _globalSettings = new();

        private readonly List<FauxMod> _fauxMods = new();

        // So that UnencryptedSaves' BeforeSavegameSave runs last, showing all Mod settings.
        public override int LoadPriority() => int.MaxValue;

        public override void Initialize()
        {
            foreach (Type t in Assembly.GetAssembly(typeof(QoL)).GetTypes().Where(x => x.IsSubclassOf(typeof(FauxMod))))
            {
                var fm = (FauxMod) Activator.CreateInstance(t);

                // If Disable isn't overridden then it can't be toggled.
                bool cantDisable = t.GetMethod(nameof(FauxMod.Unload))?.DeclaringType == typeof(FauxMod);

                if (!_globalSettings.EnabledModules.TryGetValue(t.Name, out bool enabled))
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

        public void Unload()
        {
            foreach (FauxMod fm in _fauxMods.Where(x => x.IsLoaded))
            {
                fm.Unload();
            }
        }
    }
}