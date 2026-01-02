using BandoWare.GameplayTags;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment
{
    [CreateAssetMenu( menuName ="R.A.P.I.D/Tag/"+nameof(GameplayTag), fileName = nameof(GameplayTag))]
    public class GameplayTagAsset : ScriptableObject, IHaveGameplayTag
    {
        public GameplayTagContainer Tags;
        public GameplayTagContainer GameplayTag => Tags;
    }
}