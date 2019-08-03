using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Modding;

namespace QoL
{
    public class QoL : Mod, ITogglableMod
    {
        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        
        public override ModSettings GlobalSettings
        {
            get => _globalSettings;
            set => _globalSettings = (Settings) value;
        }
        
        private Settings _globalSettings = new Settings();

        private readonly List<FauxMod> _fauxMods = new List<FauxMod>();
        
        // So that UnencryptedSaves' BeforeSavegameSave runs last, showing all Mod settings.
        public override int LoadPriority() => int.MaxValue;

        public override void Initialize()
        {
            foreach (Type t in Assembly.GetAssembly(typeof(QoL)).GetTypes().Where(x => x.IsSubclassOf(typeof(FauxMod))))
            {
                bool cantDisable = t.GetMethod(nameof(FauxMod.Unload))?.DeclaringType == typeof(FauxMod);
                
                if (cantDisable || !GlobalSettings.BoolValues.TryGetValue(t.Name, out bool enabled))
                {
                    if (!cantDisable)
                        GlobalSettings.BoolValues.Add(t.Name, true);
                    
                    enabled = true;
                }

                var fm = (FauxMod) Activator.CreateInstance(t);

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