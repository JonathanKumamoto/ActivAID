using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTextSummarizer;
using System.Text.RegularExpressions;
using BlockResponse = System.Collections.Generic.List<System.Tuple<string, string[]>>;
using QueryResponseTup = System.Tuple<string[], System.Collections.Generic.List<System.Tuple<string, string[]>>>;
using BlockDataAndKeyWords = System.Collections.Generic.List<System.Tuple<string[], string[]>>;

namespace ActivAID
{
    class QueryHandler
    {
        DataAccess dA;
        QueryGenerator queryGen;
        Func<string, string> stringOp;
        Func<string[], string[]> summarize;
        List<Query> queries;        

        public QueryHandler(
                            DataAccess dA,
                            UserInputBoiler uib,
                            Func<string, string> stringOp,
                            Func<string[], string[]> summarize
                            )
        {                                                                               
            this.dA = dA;
            queryGen = new QueryGenerator(uib);
            this.stringOp = stringOp;
            this.summarize = summarize;
            queries = new List<Query>();
        }

        /// <summary>
        /// takes in input from front end and generates queries
        /// </summary>
        /// <param name="sentences">input quries from front end</param>
        private void genQueries(string[] sentences)
        {
            foreach (string sentence in sentences)
            {
                queries.Add(queryGen.queryGen(sentence));
            }
        }

        /// <summary>
        /// retrieves responses to queries and aggregates those responses
        /// </summary>
        /// <param name="q">Query to be sent off to data tier</param>
        private QueryResponseTup aggregateQueryResults(Query q)
        {
            BlockResponse blockList = new BlockResponse();
            List<string> hrefList = new List<string>();
            var qResponse = dA.query(q);
            foreach (var kvpair in qResponse.Item2)
            {
                blockList.Add(new Tuple<string, string[]>(q.originalSentence, kvpair.Value.ToArray()));
            }
            foreach(string href in qResponse.Item1)
            {
                hrefList.Add(href);
            }
            //return blockList;
            return new QueryResponseTup(hrefList.ToArray(), blockList);
        }

        /// <summary>
        /// sends off query objects to the data tier
        /// </summary>
        private async Task<QueryResponseTup[]> sendOff()
        {
            List<Task<QueryResponseTup>> issuedQueries = new List<Task<QueryResponseTup>>();
            foreach (Query q in queries)
            {
                issuedQueries.Add
                (
                    Task<QueryResponseTup>.Factory.StartNew
                    (() => {return aggregateQueryResults(q);})
                );
            }
            return await Task.WhenAll(issuedQueries).ConfigureAwait(false);
        }

        private List<string> getInputStringList(List<QueryResponseTup> toSummarize)
        {
            List<string> inputList = new List<string>();
            foreach (QueryResponseTup qrl in toSummarize)
            {
                Func<string, string, string> fun = (x,y)=> { return x + " " + y; };
                string input = (from tup in qrl.Item2
                        select tup.Item2.Aggregate(fun)).Aggregate(fun);
                inputList.Add(input);
            }
            return inputList;
        }

        private string[] getMaxStrings(BlockDataAndKeyWords tupList)
        {
            int occurance = 0;
            int max = -99;
            string[] maxStrings = new string[] { "" };
            foreach (var sTup in tupList)
            {
                if (sTup.Item1 == null || sTup.Item2 == null)
                {
                    continue;
                }
                foreach (string kw in sTup.Item2)
                {
                    occurance = occurance + Regex.Matches(String.Join(" ", sTup.Item1), kw).Count;
                }
                maxStrings = occurance > max ? sTup.Item1 : maxStrings;//average hits per element
                max = occurance > max ? occurance : max;
            }
            return maxStrings;
        }

        /*private BlockDataAndKeyWords getTupList(List<Tuple<string, string[]>> block)
        {
            BlockDataAndKeyWords retList = new BlockDataAndKeyWords();
            foreach (var e in block)
            {
                string[] kwArray = KeyWordFinder.handleLineKeyWords(5, e.Item1).ToArray();
                retList.Add(new Tuple<string[], string[]>(e.Item2, kwArray));
            }
            return retList;
        }*/

        /*private void populateTupList(string paragraph, ref BlockDataAndKeyWords tupList)
        {
            var sentences = paragraph.Split('.');
            foreach (QueryResponseTup response in handleQuery(sentences))
            {
                var block = response.Item2.ToList();
                tupList.AddRange(getTupList(block));
                tupList.Add(new Tuple<string[], string[]>(null, null));
            }
        }*/
        /*public string backendCommand(string paragraph)
        {
            BlockDataAndKeyWords tupList = new BlockDataAndKeyWords();
            populateTupList(paragraph, ref tupList);

            string rString = "";
            foreach (string s in getMaxStrings(tupList))
            {
                rString = rString + s + "\n";
            }
            return rString;
        }*/

        /// <summary>
        /// acts as the junction for communcication between data access and front end
        /// </summary>
        /// <param name="sentences">input queries from the front end</param>
        public List<QueryResponseTup> handleQuery(string[] sentences)
        {
            genQueries(sentences);
            List<QueryResponseTup> handledQueries = new List<QueryResponseTup>();
            QueryResponseTup[] response = sendOff().Result;
            foreach(QueryResponseTup br in response)
            {
                var augBlocks = from tup in br.Item2
                                select new Tuple<string, string[]>(tup.Item1, summarize(tup.Item2.Select((x) => stringOp(x)).ToArray()));
                handledQueries.Add(new QueryResponseTup(br.Item1, augBlocks.ToList()));
            }
            return handledQueries;
        }
    }
}
