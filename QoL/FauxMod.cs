namespace QoL 
{
    public abstract class FauxMod
    {
        public bool DefaultState { get; } = true;

        protected FauxMod() {}

        protected FauxMod(bool enabled)
        {
            DefaultState = enabled;
        }
        
        internal bool IsLoaded { get; set; }
        
        public virtual void Initialize() { }

        public virtual void Unload() { }
    }
}