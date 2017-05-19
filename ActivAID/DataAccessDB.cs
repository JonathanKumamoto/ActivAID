using System.Collections.Generic;
using HrefsandBlocks = System.Tuple<string[], System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<string>>>;

namespace ActivAID
{
    public class DataAccessDB : DataAccess
    {
        private ActivAIDDB db;

        public DataAccessDB()
        {
            db = new ActivAIDDB();
        }

        private Dictionary<int, List<string>> getBlocks(Query query)
        { 
            return db.getAllElements(query.attributeList[0].value);
        }

        private string[] getHrefs(Query query)
        {
            return db.getHyperlinks(query.attributeList[0].value);
        }
        public HrefsandBlocks query(Query query)
        {
            return new HrefsandBlocks(getHrefs(query), getBlocks(query));
        }
    }
}
