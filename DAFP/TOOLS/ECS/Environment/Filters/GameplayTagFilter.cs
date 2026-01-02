using System;
using BandoWare.GameplayTags;
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

        public bool Evaluate(GameplayTagContainer go)
        {
            return Mode switch
            {
                GTagCompareMode.HasAny => go.HasAny(GameplayTag),
                GTagCompareMode.HasAll => go.HasAll(GameplayTag),
                GTagCompareMode.HasAllExact => go.HasAllExact(GameplayTag),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public bool Evaluate(IHaveGameplayTag go)
        {
            return Evaluate(go.GameplayTag);
        }

        public bool Evaluate(GameObject go)
        {
            return go.TryGetComponent<IHaveGameplayTag>(out var _tag) && Evaluate(_tag.GameplayTag);
        }

        public TriggerEntity.TriggerEvent Event { get; set; }
        public bool? LastStatus { get; set; }
        public GameplayTagContainer GameplayTag => Tags.Value?.GameplayTag ?? GameplayTagContainer.Empty;

        public bool Evaluate(IEntity go)
        {
            return Evaluate((IHaveGameplayTag)go);
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