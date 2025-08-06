
using DAFP.TOOLS.BTs;
using UnityEngine;
using UnityEngine.Serialization;

namespace BDeshi.BTSM
{
    public class FsmRunner: MonoBehaviour
    {
        public IRunnableStateMachine Fsm;
        public bool ShouldLog = false;
        [FormerlySerializedAs("shouldTickAutomatically")] public bool ShouldTickAutomatically = true;
        public BlackBoard Data;
        /// <summary>
        /// Calls fsm.enter
        /// </summary>
        /// 

        public  void Initialize(IRunnableStateMachine fsm, bool callEnter = true)
        {
            this.Fsm = fsm;
            fsm.BlackBoard = Data;
            fsm.DebugContext = gameObject;
            SyncDebugOnInFSM();
            fsm.Enter(callEnter);
        }
        public void InitializeData(BlackBoard data)
        {
            this.Data = data;
        }
        private void OnValidate()
        {
            if(Fsm != null)
                SyncDebugOnInFSM();
        }

        /// <summary>
        /// Manually tick FSM.
        /// </summary>
        public void ManualTick()
        {
            Fsm.Tick();
        }

        /// <summary>
        /// Just ticks FSM.
        /// </summary>
        protected virtual void Update()
        {
            if(ShouldTickAutomatically)
                Fsm.Tick();
        }

        public void SyncDebugOnInFSM()
        {
            Fsm.ShouldLog = ShouldLog;
        }
        
    }
}