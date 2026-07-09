using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.Services;
using FluentResults;
using RapidLib.DAFP.TOOLS.Common;
using TripleA.Utils.Extensions;
using UGizmo;
using Zenject;

namespace DAFP.TOOLS.Injection
{
    public class UniversalModManager : IModManager, IResetable, INameable
    {
        [Inject]private IObjectCreatePreparer preparer;
        [Inject] private Adam adam;

        private List<IMod> installed = new();

        public Result RegisterMod(IMod mod)
        {
            if (installed.Contains(mod))
                return Result.Fail(new Error($"that mod ({mod.Name}) is already installed"));

            preparer.Create(Adam.CreationInfo.New<IMod>(), mod);
            installed.Add(mod);
            return Result.Ok();
        }

        public Result UnRegisterMod(IMod mod)
        {
            if (!installed.Contains(mod))
                return Result.Fail(new Error($"that mod ({mod.Name}) is already uninstalled"));
            
            installed.Remove(mod);
            adam.Destroy(mod).Forget();
            return Result.Ok();
        }

        public IEnumerable<IMod> Mods()
        {
            return installed;
        }

        public void ResetToDefault()
        {
            var _l = installed.Clone();
            foreach (var _mod in _l)
            {
                UnRegisterMod(_mod);
            }
        }

        public string Name { get; set; } = nameof(IModManager);
    }

    public interface IModManager
    {
        public Result RegisterMod(IMod mod);
        public Result UnRegisterMod(IMod mod);
        public IEnumerable<IMod> Mods();
    }

    public interface IMod :  IDisposable,IInitializable, INameable, IDescriptable, IAuthorContainer
    {
    }
}