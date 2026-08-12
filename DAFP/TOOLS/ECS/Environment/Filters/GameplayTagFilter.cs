using System;
using BandoWare.GameplayTags;
using DAFP.TOOLS.ECS.Environment.TriggerSys;
using ModestTree;
using TNRD;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment.Filters
{
    [Serializable]
    public class GameplayTagFilter : IFilter<GameplayTagContainer>, IFilter<IHaveGameplayTag>, ITriggerFilter,
        IHaveGameplayTag, IFilter<GameObject>, IFilter<IEntity>
    {
        [SerializeField] private SerializableInterface<IHaveGameplayTag> Tags;

        [SerializeField] private GTagCompareMode Mode = GTagCompareMode.HasAny;

        public bool Evaluate(GameplayTagContainer go, IFilterContext ctx)
        {
            return Mode switch
            {
                GTagCompareMode.HasAny => go.HasAny(GameplayTag),
                GTagCompareMode.HasAll => go.HasAll(GameplayTag),
                GTagCompareMode.HasAllExact => go.HasAllExact(GameplayTag),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public bool Evaluate(IHaveGameplayTag go, IFilterContext ctx)
        {
            return Evaluate(go.GameplayTag, ctx);
        }

        public bool Evaluate(GameObject go, IFilterContext ctx)
        {
            return go.TryGetComponent<IHaveGameplayTag>(out var _tag) && Evaluate(_tag.GameplayTag, ctx);
        }

        public TriggerEntity.TriggerEvent Event { get; set; }
        public bool? LastStatus { get; set; }

        public GameplayTagContainer GameplayTag
        {
            get => Tags.Value?.GameplayTag ?? GameplayTagContainer.Empty;
            set => Tags.Value = value;
        }

        public bool Evaluate(IEntity go, IFilterContext ctx)
        {
            return Evaluate((IHaveGameplayTag)go, ctx);
        }

        internal enum GTagCompareMode
        {
            HasAny,
            HasAll,
            HasAllExact,
        }

        public GameplayTagFilter(GameplayTagContainer container)
        {
            Tags = new SerializableInterface<IHaveGameplayTag>(container);
        }

        public GameplayTagFilter()
        {
        }
    }
}