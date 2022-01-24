using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using QoL.Modules;

namespace QoL
{
    [PublicAPI]
    public static class SettingsOverride
    {
        private static readonly Dictionary<string, bool?> _ModuleOverrides;
        private static readonly Dictionary<string, bool> _OrigEnabledModules = new();

        private static readonly Dictionary<string, FieldInfo> _Fields;
        private static readonly Dictionary<string, bool?> _SettingOverrides;
        private static readonly Dictionary<string, bool> _OrigSettings = new();

        public static void OverrideModuleToggle(string name, bool enable)
        {
            if (name == null || !_ModuleOverrides.ContainsKey(name))
                throw new ArgumentException("Name does not correspond to any togglable QoL module.", nameof(name));

            if (QoL.GlobalSettings.EnabledModules.TryGetValue(name, out bool value) && !_OrigEnabledModules.ContainsKey(name))
                _OrigEnabledModules[name] = value;

            _ModuleOverrides[name] = enable;
            QoL.ToggleModule(name, enable);
        }

        public static void RemoveModuleOverride(string name)
        {
            bool wasOverwritten = _ModuleOverrides.TryGetValue(name, out bool? value);
            bool hadOrig = _OrigEnabledModules.TryGetValue(name, out bool orig);

            if (wasOverwritten && value != null)
                _ModuleOverrides[name] = null;

            if (!hadOrig)
                return;

            _OrigEnabledModules.Remove(name);

            if (value is bool val && val != orig)
                QoL.ToggleModule(name, orig);
        }

        public static void OverrideSettingToggle(string type, string field, bool enable)
        {
            string key = $"{type}:{field}";

            if (!_Fields.TryGetValue(key, out FieldInfo fi))
                throw new ArgumentException($"QoL setting {key} not found.", nameof(field));

            if (!_OrigSettings.ContainsKey(key))
                _OrigSettings[key] = (bool) fi.GetValue(null);

            _SettingOverrides[key] = enable;
            fi.SetValue(null, enable);
        }

        public static void RemoveSettingOverride(string type, string field)
        {
            string key = $"{type}:{field}";
            bool wasOverwritten = _SettingOverrides.TryGetValue(key, out bool? enabled);
            bool hadOrig = _OrigSettings.TryGetValue(key, out bool orig);

            if (wasOverwritten)
                _SettingOverrides[key] = null;

            if (!hadOrig)
                return;

            _OrigSettings.Remove(key);

            if (enabled is bool val && val != orig)
                _Fields[key].SetValue(null, orig);
        }

        public static bool TryGetModuleOverride(string name, out bool enabled)
        {
            bool result = _ModuleOverrides.TryGetValue(name, out bool? value);
            enabled = value.HasValue && value.Value;

            return result && value.HasValue;
        }

        public static bool TryGetSettingOverride(string name, out bool enabled)
        {
            bool result = _SettingOverrides.TryGetValue(name, out bool? value);
            enabled = value.HasValue && value.Value;

            return result && value.HasValue;
        }

        public static bool TryGetOrigSetting(string name, out bool value) => _OrigSettings.TryGetValue(name, out value);

        public static bool TryGetOrigModuleEnabled(string name, out bool value) => _OrigEnabledModules.TryGetValue(name, out value);

        static SettingsOverride()
        {
            Type[] types = typeof(SettingsOverride).Assembly.GetTypes();

            _ModuleOverrides = types.Where(t => t.IsSubclassOf(typeof(FauxMod)) && t.GetMethod(nameof(FauxMod.Unload))!.DeclaringType != typeof(FauxMod))
                                    .Select(t => t.Name)
                                    .ToDictionary<string, string, bool?>(s => s, _ => null);

            _Fields = types.SelectMany(t => t.GetFields())
                           .Where(f => f.FieldType == typeof(bool) && Attribute.IsDefined(f, typeof(SerializeToSetting)))
                           .ToDictionary(f => $"{f.DeclaringType!.Name}:{f.Name}");

            _SettingOverrides = _Fields.Keys.ToDictionary<string, string, bool?>(key => key, _ => null);
        }
    }
}
