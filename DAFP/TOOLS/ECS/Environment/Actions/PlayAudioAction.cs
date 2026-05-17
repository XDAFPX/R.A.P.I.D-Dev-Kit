using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.Audio;
using DAFP.TOOLS.ECS.Environment.TriggerSys;
using RapidLib.DAFP.TOOLS.Common;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.Environment.Actions
{
    [System.Serializable]
    public class PlayAudioAction : ITriggerAction, IActionUpon<IEntity>, IActionUpon<GameObject>
    {
        [SerializeField] private string Sound;

        [Inject] private IAudioSystem system;

        // [SerializeField]private Transform SoundLocation;
        // [SerializeField]private bool TransformToVectorOnPlay;
        public void Act(TriggerContext target)
        {
            system.PlayOneShot(new GeneralAudioSettings(new PositionTarget(target.Target.transform.position)), Sound);
        }

        public void Act(IEntity target)
        {
            system.PlayOneShot(
                new GeneralAudioSettings(new PositionTarget(target.GetWorldRepresentation().transform.position)),
                Sound);
        }

        public void Act(GameObject target)
        {
            system.PlayOneShot(
                new GeneralAudioSettings(new PositionTarget(target.transform.position)),
                Sound);
        }

        public void Act()
        {
        }
    }
}