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
        private ActivAIDDB db;

        public DataAccessDB()
        {
            db = new ActivAIDDB();
        }

        private Dictionary<int, List<string>> getBlocks(Query query) // might change to filenames instead
        {
            string current_directory = System.IO.Directory.GetCurrentDirectory() + (char)92;
            string helplocation = current_directory + "help" + (char)92 + "Help HTML" + (char)92;
            string filepath = helplocation + query.attributeList[0].value;
            return db.getAllElements(filepath);
        }

        private string[] getHrefs(Query query) // might change to filenames instead
        {
            string current_directory = System.IO.Directory.GetCurrentDirectory() + (char)92;
            string helplocation = current_directory + "help" + (char)92 + "Help HTML" + (char)92;
            string filepath = helplocation + query.attributeList[0].value;
            return db.getHyperlinks(filepath);
        }
        public HrefsandBlocks query(Query query)
        {
            return new HrefsandBlocks(getHrefs(query), getBlocks(query));
        }
    }
}
