using System.Threading.Tasks;
using System.Collections.Generic;

namespace Valerie.Addons.Interactive
{
    public class JudgeCriteria<T> : ICriteria<T>
    {
        List<ICriteria<T>> Criterias { get; set; } = new List<ICriteria<T>>();

        public JudgeCriteria<T> AddCriteria(ICriteria<T> Criteria)
        {
            Criterias.Add(Criteria);
            return this;
        }

        public async Task<bool> JudgeAsync(IContext Context, T TypeParameter)
        {
            foreach (var interactive in Criterias)
            {
                var Result = await interactive.JudgeAsync(Context, TypeParameter);
                if (!Result) return false;
            }
            return true;
        }
    }
}