using System.Threading.Tasks;

namespace Valerie.Handlers.ModuleHandler.Interactive
{
    public interface IInteractive<T>
    {
        Task<bool> JudgeAsync(IContext Context, T TypeParameter);
    }
}