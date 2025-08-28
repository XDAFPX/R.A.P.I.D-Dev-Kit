using DAFP.TOOLS.ECS.GlobalState;
using Zenject;

namespace DAFP.TOOLS.ECS.Services
{
    public interface IService
    {
        [Inject] IGlobalGameStateHandler GameState { get; set; }
    }
}