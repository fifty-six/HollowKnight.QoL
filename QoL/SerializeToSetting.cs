using System;
using System.Reflection;
using JetBrains.Annotations;

namespace QoL
{
    [MeansImplicitUse]
    public class SerializeToSetting : Attribute
    {
        public static bool ShouldSerialize(FieldInfo fi) => Attribute.IsDefined(fi, typeof(SerializeToSetting));
    }
}