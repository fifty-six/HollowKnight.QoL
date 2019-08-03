using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Modding;
using UnityEngine;

namespace QoL
{
    public class Settings : ModSettings, ISerializationCallbackReceiver
    {
        private readonly Assembly _asm = Assembly.GetAssembly(typeof(Settings));

        private readonly Dictionary<FieldInfo, Type> _fields = new Dictionary<FieldInfo, Type>();

        public Settings()
        {
            foreach (Type t in _asm.GetTypes())
            {
                foreach (FieldInfo fi in t.GetFields().Where(x => x.GetCustomAttributes(typeof(SerializeToSetting), false).Length > 0))
                {
                    _fields.Add(fi, t);
                }
            }
        }

        public void OnBeforeSerialize()
        {
            foreach (KeyValuePair<FieldInfo, Type> pair in _fields)
            {
                FieldInfo fi = pair.Key;

                if (fi.FieldType == typeof(bool))
                {
                    BoolValues[$"{pair.Value.Name}:{fi.Name}"] = (bool) fi.GetValue(null);
                }
                else if (fi.FieldType == typeof(float))
                {
                    FloatValues[$"{pair.Value.Name}:{fi.Name}"] = (float) fi.GetValue(null);
                }
                else if (fi.FieldType == typeof(int))
                {
                    IntValues[$"{pair.Value.Name}:{fi.Name}"] = (int) fi.GetValue(null);
                }
            }
        }

        public void OnAfterDeserialize()
        {
            foreach (KeyValuePair<FieldInfo, Type> pair in _fields)
            {
                FieldInfo fi = pair.Key;

                if (fi.FieldType == typeof(bool))
                {
                    if (BoolValues.TryGetValue($"{pair.Value.Name}:{fi.Name}", out bool val))
                        fi.SetValue(null, val);
                }
                else if (fi.FieldType == typeof(float))
                {
                    if (FloatValues.TryGetValue($"{pair.Value.Name}:{fi.Name}", out float val))
                        fi.SetValue(null, val);
                }
                else if (fi.FieldType == typeof(int))
                {
                    if (IntValues.TryGetValue($"{pair.Value.Name}:{fi.Name}", out int val))
                        fi.SetValue(null, val);
                }
            }
        }
    }
}