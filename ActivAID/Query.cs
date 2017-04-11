using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivAID
{
    
    public struct Query
    {
        public List<Attrib> attributeList { get; set; }
        public Tuple<int, bool> quantityOfResponses { get; set; }
        public string resourceExpected { get; set; }

        public string originalSentence { get; set; }

        public Query(List<Attrib> attributeList, 
                     Tuple<int,bool> quantityOfResponses, 
                     string resourceExpected,
                     string originalSentence) : this()
        {
            this.attributeList = attributeList;
            this.quantityOfResponses = quantityOfResponses;
            this.resourceExpected = resourceExpected;
            this.originalSentence = originalSentence;
        }
    }
}
