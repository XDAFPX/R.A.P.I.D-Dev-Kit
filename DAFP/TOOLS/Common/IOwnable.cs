namespace DAFP.TOOLS.Common
{
    public interface IOwnable<TOwner> where TOwner : IOwner<TOwner>
    {
        TOwner GetCurrentOwner();
        void ChangeOwner(TOwner newOwner);
    }
}