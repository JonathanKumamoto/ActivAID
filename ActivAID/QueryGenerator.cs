using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivAID
{
    public class QueryGenerator
    {
        private UserInputBoiler uib;

        public QueryGenerator(UserInputBoiler uib)
        {
            this.uib = uib;
        }

        public Query queryGen(string sentence)
        {
            List<Attrib> attributeList = new List<Attrib>();
            attributeList.Add(new Attrib("filePath", uib.boilDown(sentence),false));
            return new Query(attributeList, new Tuple<int, bool>(0, false), "text",sentence);
        }
    }
}
