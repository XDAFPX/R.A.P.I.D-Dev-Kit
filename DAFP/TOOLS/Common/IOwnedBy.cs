namespace DAFP.TOOLS.Common
{
    public interface IOwnedBy<TOwner> 
    {
        TOwner GetCurrentOwner();
        void ChangeOwner(TOwner newOwner);
    }
}