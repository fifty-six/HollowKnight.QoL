using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Modding;
using UnityEngine;

namespace QoL
{
    [Serializable]
    public class Settings : ISerializationCallbackReceiver
    {
        private readonly Assembly _asm = Assembly.GetAssembly(typeof(Settings));

        internal readonly Dictionary<FieldInfo, Type> Fields = new();

        public Dictionary<string, bool> EnabledModules { get; set; } = new();

        public Dictionary<string, bool>  Booleans { get; set; } = new();
        public Dictionary<string, float> Floats   { get; set; } = new();
        public Dictionary<string, int>   Integers { get; set; } = new();
        
        public Settings()
        {
            foreach (Type t in _asm.GetTypes())
            {
                foreach (FieldInfo fi in t.GetFields().Where(x => x.GetCustomAttributes(typeof(SerializeToSetting), false).Length > 0))
                {
                    Fields.Add(fi, t);
                }
            }
        }

        public void OnBeforeSerialize()
        {
            foreach (var (fi, type) in Fields)
            {
                if (fi.FieldType == typeof(bool))
                {
                    Booleans[$"{type.Name}:{fi.Name}"] = (bool) fi.GetValue(null);
                }
                else if (fi.FieldType == typeof(float))
                {
                    Floats[$"{type.Name}:{fi.Name}"] = (float) fi.GetValue(null);
                }
                else if (fi.FieldType == typeof(int))
                {
                    Integers[$"{type.Name}:{fi.Name}"] = (int) fi.GetValue(null);
                }
            }
        }

        public void OnAfterDeserialize()
        {
            foreach (var (fi, type) in Fields)
            {
                if (fi.FieldType == typeof(bool))
                {
                    if (Booleans.TryGetValue($"{type.Name}:{fi.Name}", out bool val))
                        fi.SetValue(null, val);
                }
                else if (fi.FieldType == typeof(float))
                {
                    if (Floats.TryGetValue($"{type.Name}:{fi.Name}", out float val))
                        fi.SetValue(null, val);
                }
                else if (fi.FieldType == typeof(int))
                {
                    if (Integers.TryGetValue($"{type.Name}:{fi.Name}", out int val))
                        fi.SetValue(null, val);
                }
            }
        }
    }
}