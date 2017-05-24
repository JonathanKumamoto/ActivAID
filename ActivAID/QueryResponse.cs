using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivAID
{
    public class QueryResponse
    {
        public string responseHTML { get; set; }
        public string originalSentence { get; set; }
        public List<Tuple<string,string>> hrefs { get; set; }
        public List<string> elements { get; set; }
        public List<string> keywords { get; set; }

        public Dictionary<int, List<string>> blocks { get; set; }

        public QueryResponse(string responseHTML, string originalSentence, List<Tuple<string,string>> hrefs,
                             List<string> elements, List<string> keywords,
                             Dictionary<int, List<string>> blocks)
        {
            this.responseHTML = responseHTML;
            this.originalSentence = originalSentence;
            this.hrefs = hrefs;
            this.elements = elements;
            this.keywords = keywords;
            this.blocks = cleanUpBlocks(blocks);
        }

        private Dictionary<int, List<string>> cleanUpBlocks(Dictionary<int, List<string>> blocks)
        {
            var rDict = new Dictionary<int, List<string>>();
            foreach (var kvPair in blocks)
            {
                List<string> toInsert = new List<string>();
                foreach (string str in kvPair.Value)
                {
                    string toMessage = str;
                    new HTMLMessager().removeFromLine(ref toMessage);
                    toInsert.Add(toMessage);
                }
                rDict.Add(kvPair.Key, toInsert);
            }
            return rDict;
        }
    }
}
