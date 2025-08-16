using DAFP.GAME.Essential;

namespace DAFP.TOOLS.Common
{
    // ReSharper disable once InconsistentNaming
    public class GODManager : Manager<GODManager>
    {
        public override void Awake()
        {
            // If an instance already exists (and it’s not this one), destroy this duplicate
            if (Singleton != null && Singleton != this)
            {
                Destroy(gameObject);
                return;
            }

            // Otherwise let base set up the singleton
            base.Awake();

            // Persist across scene loads
            DontDestroyOnLoad(gameObject);
        }
    }
}