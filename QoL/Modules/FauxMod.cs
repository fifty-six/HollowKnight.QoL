using JetBrains.Annotations;

namespace QoL.Modules 
{
    [UsedImplicitly]
    public abstract class FauxMod
    {
        public bool DefaultState { get; } = true;

        protected FauxMod() {}

        protected FauxMod(bool enabled)
        {
            DefaultState = enabled;
        }

        [PublicAPI]
        internal void Log(object obj)
        {
            Modding.Logger.Log($"[QoL : {GetType().Name}] - {obj}");
        }
        
        internal bool IsLoaded { get; set; }
        
        public virtual void Initialize() { }

        public virtual void Unload() { }
    }
}