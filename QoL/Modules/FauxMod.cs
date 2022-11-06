using System;
using JetBrains.Annotations;
using Modding;

namespace QoL.Modules
{
    [UsedImplicitly]
    public abstract class FauxMod
    {
        public bool DefaultState { get; } = true;

        protected FauxMod() { }

        protected FauxMod(bool enabled) => DefaultState = enabled;

        [PublicAPI]
        internal void Log(object obj) => Logger.Log($"[QoL : {GetType().Name}] - {obj}");

        internal bool IsLoaded { get; set; }

        public virtual void Initialize() { }

        public virtual void Unload() { }

        public static bool IsToggleableFauxMod(Type t)
        {
            return t.IsSubclassOf(typeof(FauxMod)) && IsToggleableUnsafe(t);
        }

        public static bool IsToggleable(Type t)
        {
            if (!t.IsSubclassOf(typeof(FauxMod)))
                throw new ArgumentException($"Type {t} is not a FauxMod!", nameof(t));

            return IsToggleableUnsafe(t);
        }

        private static bool IsToggleableUnsafe(Type t) => t.GetMethod(nameof(Unload))!.DeclaringType != typeof(FauxMod);
    }
}