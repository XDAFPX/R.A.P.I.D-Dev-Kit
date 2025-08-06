namespace DAFP.TOOLS.ECS.BigData
{
    public class HpBoard : FloatBoard
    {
        private MaxHpBoard board;
        public override float MaxValue => board.Value;
        public override float MinValue => 0;

        protected override void OnTick()
        {
        }

        protected override void OnInitialize()
        {

            board = Host.GetEntComponent<MaxHpBoard>();
        }

        protected override void OnStart()
        {
            
        }
    }
}