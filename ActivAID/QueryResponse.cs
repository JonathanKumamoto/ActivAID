using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivAID
{
    class QueryResponse
    {
        public string originalSentence { get; set; }
        public List<string> hrefs { get; set; }
        public List<string> elements { get; set; }
        public List<string> keywords { get; set; }

        public QueryResponse(string originalSentence, List<string> hrefs,
                             List<string> element, List<string> keywords)
        {
            this.originalSentence = originalSentence;
            this.hrefs = hrefs;
            this.elements = element;
            this.keywords = keywords;
        }
    }
}
