using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Services;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Components
{
    public class CreationInfoContainer : MonoBehaviour
    {
        [SerializeField] internal Adam.CreationInfo Info;

        public static CreationInfoContainer EnsureOn(GameObject obj, Adam.CreationInfo info)
        {
            var _comp = obj.AddOrGetComponent<CreationInfoContainer>();
            _comp.Info = info;
            return _comp;
        }

        public static void AbsenceFrom(GameObject obj )
        {
            if(obj.TryGetComponent(out CreationInfoContainer info ))
                Destroy(info);
        }
    }
}