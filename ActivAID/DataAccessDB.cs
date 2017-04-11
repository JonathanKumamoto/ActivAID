using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HrefsandBlocks = System.Tuple<string[], System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<string>>>;

namespace ActivAID
{
    public class DataAccessDB : DataAccess
    {
        //private ActivAIDDB db;

        public DataAccessDB()
        {
            ;//db = new ActivAIDDB();
        }

        private Dictionary<int, List<string>> getBlocks(Query query) //TODO
        {
            //access db
        }

        private string[] getHrefs(Query query) //TODO
        {
            //access db
        }
        public HrefsandBlocks query(Query query)
        {
            return new HrefsandBlocks(getHrefs(), getBlocks());
        }
    }
}
