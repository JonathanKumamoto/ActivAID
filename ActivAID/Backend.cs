using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using BlockDataAndKeyWords = System.Collections.Generic.List<System.Tuple<string[], string[]>>;

//using Trainer.

namespace ActivAID
{
    public class BackEnd
    {
        private static string[] getMaxStrings(BlockDataAndKeyWords tupList)
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

        private static BlockDataAndKeyWords getTupList(List<Tuple<string, string[]>> block)
        {
            BlockDataAndKeyWords retList = new BlockDataAndKeyWords();
            foreach (var e in block)
            {
                string[] kwArray = KeyWordFinder.handleLineKeyWords(5, e.Item1).ToArray();
                retList.Add(new Tuple<string[], string[]>(e.Item2, kwArray));
            }
            return retList;
        }

        private static QueryHandler getNewQueryHandler()
        {
            Func<string, string> func = new Func<string, string>((x) =>
            {
                var temp = x;
                new HTMLMessager().removeFromLine(ref temp);
                return temp;
            });
            Func<string[], string[]> summarize = new Func<string[], string[]>((toSummarize) =>
            {
                // Need to change
                List<string> sumList = new List<string>();
                OpenTextSummarizer.SummarizerArguments args = new OpenTextSummarizer.SummarizerArguments();
                args.InputString = String.Join(" ", toSummarize);
                OpenTextSummarizer.SummarizedDocument sd = OpenTextSummarizer.Summarizer.Summarize(args);
                List<string> str = new List<string>(new string[] { "This article covers topics and keywords related to: " });
                foreach (var concept in sd.Concepts)
                {
                    str.Add(concept);
                };
                return str.Take(5).ToArray();
            });


            UserInputBoiler sb = new UserInputBoiler();
            DataAccess dA = new DataAccessDB();
            return new QueryHandler(dA, sb, func, summarize);
        }

        private static void populateTupList(string paragraph, QueryHandler qHandler, ref BlockDataAndKeyWords tupList, ref List<string> hrefs)
        {
            var sentences = paragraph.Split('.');
            Console.WriteLine(sentences.Count() + " " + paragraph.Count());
            foreach (var response in qHandler.handleQuery(sentences))
            {
                var block = response.Item2.ToList();
                tupList.AddRange(getTupList(block));
                tupList.Add(new Tuple<string[], string[]>(null, null));

                hrefs.AddRange(response.Item1);
                hrefs.Add(null);
            }
        }
        public static string backendCommand(string paragraph)
        {
            BlockDataAndKeyWords tupList = new BlockDataAndKeyWords();
            List<string> hrefs = new List<string>();
            populateTupList(paragraph, getNewQueryHandler(), ref tupList, ref hrefs);

            string rString = "";
            foreach (string s in getMaxStrings(tupList))
            {
                rString = rString + s + "\n";
            }
            Console.WriteLine(hrefs.Count());
            if (hrefs.Count() > 1)
            {
                rString = rString + "Related documents:\n";
                int count = 0;
                foreach (string s in hrefs)
                {
                    if (s != null)
                    {
                        rString = rString + s + (count == 0 ? " | " : "");
                        count++;
                    }
                }
            }
            return rString;
        }

    }
}
