namespace QoL 
{
    public abstract class FauxMod
    {
        internal bool IsLoaded { get; set; }
        
        public virtual void Initialize() { }

        public virtual void Unload() { }
    }
}