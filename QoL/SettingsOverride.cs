using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using MonoMod.ModInterop;
using QoL.Modules;

namespace QoL
{
    [PublicAPI]
    [ModExportName("QoL")]
    public static class SettingsOverride
    {
        private static readonly HashSet<string> _Modules;

        private static readonly Dictionary<string, bool> _ModuleOverrides = new();
        private static readonly Dictionary<string, bool> _OrigEnabledModules = new();

        private static readonly Dictionary<string, FieldInfo> _Fields;
        private static readonly Dictionary<string, bool> _SettingOverrides = new();
        private static readonly Dictionary<string, bool> _OrigSettings = new();

        public static void RemoveOverride
        (
            string key,
            Dictionary<string, bool> overrides,
            Dictionary<string, bool> origSettings,
            Action<string, bool> toggle
        )
        {
            bool overridden = overrides.TryGetValue(key, out bool @override);
            bool hadOrig = origSettings.TryGetValue(key, out bool orig);

            if (overridden)
                overrides.Remove(key);

            // If we didn't have a state to return to, then we don't need to do anything
            if (!hadOrig)
                return;

            origSettings.Remove(key);

            // If overridden to a state different than the original, swap it back to the original
            if (@override != orig)
                toggle(key, @override);
        }

        public static void OverrideModuleToggle(string name, bool enable)
        {
            if (name == null || !_Modules.Contains(name))
                throw new ArgumentException("Name does not correspond to any togglable QoL module.", nameof(name));

            if (QoL.GlobalSettings.EnabledModules.TryGetValue(name, out bool value) && !_OrigEnabledModules.ContainsKey(name))
                _OrigEnabledModules[name] = value;

            _ModuleOverrides[name] = enable;
            QoL.ToggleModule(name, enable);
        }

        public static void OverrideSettingToggle(string type, string field, bool enable)
        {
            string key = $"{type}:{field}";

            if (!_Fields.TryGetValue(key, out FieldInfo fi))
                throw new ArgumentException($"QoL setting {key} not found.", nameof(field));

            // Backup the original value before overriding
            if (!_OrigSettings.ContainsKey(key))
                _OrigSettings[key] = (bool) fi.GetValue(null);

            _SettingOverrides[key] = enable;
            fi.SetValue(null, enable);
        }

        public static void RemoveModuleOverride(string name) => RemoveOverride(name, _ModuleOverrides, _OrigEnabledModules, QoL.ToggleModule);

        public static void RemoveSettingOverride(string type, string field) =>
            RemoveOverride($"{type}:{field}", _SettingOverrides, _OrigSettings, (k, v) => _Fields[k].SetValue(null, v));

        private static bool TryGetOverride(string name, out bool enabled, Dictionary<string, bool> overrides)
        {
            bool res = overrides.TryGetValue(name, out bool value);
            enabled = value;

            return res;
        }

        public static bool TryGetModuleOverride(string name, out bool enabled) => TryGetOverride(name, out enabled, _ModuleOverrides);
        public static bool TryGetSettingOverride(string name, out bool enabled) => TryGetOverride(name, out enabled, _SettingOverrides);

        public static bool TryGetOrigSetting(string name, out bool value) => _OrigSettings.TryGetValue(name, out value);
        public static bool TryGetOrigModuleEnabled(string name, out bool value) => _OrigEnabledModules.TryGetValue(name, out value);

        static SettingsOverride()
        {
            Type[] types = typeof(SettingsOverride).Assembly.GetTypes();

            _Modules = new HashSet<string>
            (
                types.Where(FauxMod.IsToggleableFauxMod)
                     .Select(t => t.Name)
            );

            _Fields = types.SelectMany(t => t.GetFields())
                           .Where(f => f.FieldType == typeof(bool) && SerializeToSetting.ShouldSerialize(f))
                           .ToDictionary(f => $"{f.DeclaringType!.Name}:{f.Name}");
        }
    }
}