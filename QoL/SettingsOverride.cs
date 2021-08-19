using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QoL.Modules;

namespace QoL
{
    public static class SettingsOverride
    {
        private static readonly Dictionary<string, bool?> _moduleOverrides;
        private static readonly Dictionary<string, bool> _origEnabledModules = new();

        private static readonly Dictionary<string, FieldInfo> _fields;
        private static readonly Dictionary<string, bool?> _settingOverrides;
        private static readonly Dictionary<string, bool> _origSettings = new();

        public static void OverrideModuleToggle(string name, bool enable)
        {
            if (name == null || !_moduleOverrides.ContainsKey(name))
                throw new ArgumentException("Name does not correspond to any togglable QoL module.", nameof(name));

            if (QoL._globalSettings.EnabledModules.TryGetValue(name, out bool value))
            {
                _origEnabledModules[name] = value;
            }

            _moduleOverrides[name] = enable;
            QoL.ToggleModule(name, enable);
        }

        public static void RemoveModuleOverride(string name)
        {
            bool wasOverwritten = _moduleOverrides.TryGetValue(name, out bool? value);
            bool hadOrig = _origEnabledModules.TryGetValue(name, out bool orig);

            if (wasOverwritten && value != null)
                _moduleOverrides[name] = null;

            if (!hadOrig)
                return;

            _origEnabledModules.Remove(name);

            if (value is bool val && val != orig)
                QoL.ToggleModule(name, orig);
        }

        public static void OverrideSettingToggle(string type, string field, bool enable)
        {
            string key = $"{type}:{field}";

            if (!_fields.TryGetValue(key, out FieldInfo fi))
                throw new ArgumentException($"QoL setting {key} not found.", nameof(field));

            _origSettings[key] = (bool) fi.GetValue(null);
            _settingOverrides[key] = enable;
            fi.SetValue(null, enable);
        }

        public static void RemoveSettingOverride(string type, string field)
        {
            string key = $"{type}:{field}";
            bool wasOverwritten = _settingOverrides.TryGetValue(key, out bool? enabled);
            bool hadOrig = _origSettings.TryGetValue(key, out bool orig);

            if (wasOverwritten)
                _settingOverrides[key] = null;

            if (!hadOrig)
                return;

            _origSettings.Remove(key);

            if (enabled is bool val && val != orig)
                _fields[key].SetValue(null, orig);
        }

        public static bool TryGetModuleOverride(string name, out bool enabled)
        {
            bool result = _moduleOverrides.TryGetValue(name, out bool? value);
            enabled = value.HasValue && value.Value;

            return result && value.HasValue;
        }

        public static bool TryGetSettingOverride(string name, out bool enabled)
        {
            bool result = _settingOverrides.TryGetValue(name, out bool? value);
            enabled = value.HasValue && value.Value;

            return result && value.HasValue;
        }

        public static bool TryGetOrigSetting(string name, out bool value) => _origSettings.TryGetValue(name, out value);

        public static bool TryGetOrigModuleEnabled(string name, out bool value) => _origEnabledModules.TryGetValue(name, out value);

        static SettingsOverride()
        {
            Type[] types = typeof(SettingsOverride).Assembly.GetTypes();

            _moduleOverrides = types.Where(t => t.IsSubclassOf(typeof(FauxMod)) && t.GetMethod(nameof(FauxMod.Unload)).DeclaringType != typeof(FauxMod))
                                    .Select(t => t.Name)
                                    .ToDictionary<string, string, bool?>(s => s, _ => null);

            _fields = types.SelectMany(t => t.GetFields())
                           .Where(f => f.FieldType == typeof(bool) && Attribute.IsDefined(f, typeof(SerializeToSetting)))
                           .ToDictionary(f => $"{f.DeclaringType.Name}:{f.Name}");

            _settingOverrides = _fields.Keys.ToDictionary<string, string, bool?>(key => key, _ => null);
        }
    }
}