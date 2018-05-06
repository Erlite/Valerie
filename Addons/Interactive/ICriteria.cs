using System.Threading.Tasks;

namespace Valerie.Addons.Interactive
{
    public interface ICriteria<T>
    {
        Task<bool> JudgeAsync(IContext Context, T ObjectType);
    }
}