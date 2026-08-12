using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys;
using DAFP.TOOLS.ECS.ViewModel;
using Optional;
using RapidLib.DAFP.TOOLS.Common;
using SKUnityToolkit.SerializableDictionary;
using UnityEngine;
using UnityGetComponentCache;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class AnimatorView : MonoBehaviour, IViewModel
    {
        [SerializeField]private SerializableDictionary<string, string> Map;
        [GetComponent] private Animator animator;
        public List<IEntity> Owners { get; } = new List<IEntity>();

        public string Name
        {
            get => name;
            set => name = value;
        }

        public bool Enabled => animator.enabled;

        public void Enable()
        {
            animator.enabled = true;
        }

        public void Disable()
        {
            animator.enabled = false;
        }

        public IViewModel InitOwner(IEntity owner)
        {
            ((IPetOf<IEntity, IViewModel>)this).ChangeOwner(owner);
            GetComponentCacheInitializer.InitializeCaches(this);

            return this;
        }

        public HurtGroup<IEntity> GetHurtGroup(IEntity owner)
        {
            return null;
        }

        public Option<UniTask> Resolve(IAnimAction action)
        {
            var _name = action.GetType().Name;
            if (Map.TryGetValue(_name, out var _value))
            {
                return animator.Play(_value, CancellationToken.None).Some();
            }

            return Option.None<UniTask>();
        }
    }
}