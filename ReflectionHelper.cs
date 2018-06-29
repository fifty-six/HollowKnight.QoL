using System;
using System.Collections.Generic;
using System.Reflection;

namespace QoL
{
    public static class ReflectionHelper
    {
        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> Fields =
            new Dictionary<Type, Dictionary<string, FieldInfo>>();

        public static T GetAttr<T>(object obj, string name, bool instance = true)
        {
            if (obj == null || string.IsNullOrEmpty(name)) return default(T);

            Type t = obj.GetType();

            if (!Fields.ContainsKey(t))
            {
                Fields.Add(t, new Dictionary<string, FieldInfo>());
            }

            Dictionary<string, FieldInfo> typeFields = Fields[t];

            if (!typeFields.ContainsKey(name))
            {
                typeFields.Add(name,
                    t.GetField(name,
                        BindingFlags.NonPublic | BindingFlags.Public |
                        (instance ? BindingFlags.Instance : BindingFlags.Static)));
            }

            return (T) typeFields[name]?.GetValue(obj);
        }

        public static void SetAttr<T>(object obj, string name, T val, bool instance = true)
        {
            if (obj == null || string.IsNullOrEmpty(name)) return;

            Type t = obj.GetType();

            if (!Fields.ContainsKey(t))
            {
                Fields.Add(t, new Dictionary<string, FieldInfo>());
            }

            Dictionary<string, FieldInfo> typeFields = Fields[t];

            if (!typeFields.ContainsKey(name))
            {
                typeFields.Add(name,
                    t.GetField(name,
                        BindingFlags.NonPublic | BindingFlags.Public |
                        (instance ? BindingFlags.Instance : BindingFlags.Static)));
            }

            typeFields[name]?.SetValue(obj, val);
        }
    }

    public static class ReflectionExtensions
    {
        public static void SetAttr<T>(this object obj, string name, T val, bool instance = true) =>
            ReflectionHelper.SetAttr(obj, name, val, instance);

        public static T GetAttr<T>(this object obj, string name, bool instance = true) =>
            ReflectionHelper.GetAttr<T>(obj, name, instance);
    }
}