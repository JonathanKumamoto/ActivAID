using System;
using HrefsandBlocks = System.Tuple<System.Tuple<string,string>[], System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<string>>>;

namespace ActivAID
{
    public interface DataAccess
    {
        HrefsandBlocks query(Query query);
    }
}
