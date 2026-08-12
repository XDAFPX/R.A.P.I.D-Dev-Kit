using Archon.SwissArmyLib.Utils.Editor;
using DAFP.TOOLS.Common.Utill;
using TNRD;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.Injection
{
    internal sealed class EditorModsSys : IInitializable
    {
        [Inject(Id = "EditorMods")] private IMod[] mods;
        [Inject] private IModManager manager;
        public void Initialize()
        {
            foreach (var _mod in mods)
            {
                manager.RegisterMod(_mod);
            }
        }
    }
}