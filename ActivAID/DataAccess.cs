using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HrefsandBlocks = System.Tuple<string[], System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<string>>>;

namespace ActivAID
{
    public interface DataAccess
    {
        HrefsandBlocks query(Query query);
    }
}
