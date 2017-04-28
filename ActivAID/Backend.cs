using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using BlockDataAndKeyWords = System.Collections.Generic.List<System.Tuple<string[], string[]>>;


//using Trainer.

namespace ActivAID
{
    public class BackEnd
    {
        //private static ScriptScope builtinscope;
        private static Task<dynamic> phrase_generator_task;
        private static dynamic phrase_generator;

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

        public static void loadIronPython()//Tuple<ScriptEngine, ScriptScope> loadIronPython()
        {
            //if (builtinscope == null && phrase_generator == null)
            if (phrase_generator_task == null)
            {
                phrase_generator_task = Task<dynamic>.Factory.StartNew
                (() =>
                {
                    var options = new Dictionary<string, object>();
                    options["Frames"] = true;
                    options["FullFrames"] = true;
                    ScriptEngine engine = Python.CreateEngine(options);

                    string spackagesPath = Environment.GetEnvironmentVariable("SPACKAGES");
                    string libsPath = Environment.GetEnvironmentVariable("LIBS");
                    var searchPaths = engine.GetSearchPaths();
                    searchPaths.Add(@"C:\Users\Matthew\Desktop\191B\model_for_trigram_output");
                    searchPaths.Add(spackagesPath);
                    searchPaths.Add(libsPath);
                    engine.SetSearchPaths(searchPaths);
                    return engine.ImportModule("phrase_generator");
                });


                //builtinscope = Python.GetBuiltinModule(engine);
                //phrase_generator = engine.ImportModule("phrase_generator");
            }
            //return new Tuple<ScriptEngine, ScriptScope>(phrase_generator, builtinscope);
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
            //ScriptEngine engine = Python.CreateEngine();
            //dynamic phrase_generator = engine.ImportModule("phrase_generator");
            Func<string, string> func = new Func<string, string>((x) =>
            {
                var temp = x;
                new HTMLMessager().removeFromLine(ref temp);
                return temp;
            });
            /*Func<string[], string[]> summarize = new Func<string[], string[]>((toSummarize) =>
            {
                // Need to change
                //List<string> sumList = new List<string>();
                OpenTextSummarizer.SummarizerArguments args = new OpenTextSummarizer.SummarizerArguments();
                args.InputString = String.Join(" ", toSummarize);
                OpenTextSummarizer.SummarizedDocument sd = OpenTextSummarizer.Summarizer.Summarize(args);
                List<string> str = new List<string>(new string[] { "This article covers topics and keywords related to: " });
                foreach (var concept in sd.Concepts)
                {
                    str.Add(concept);
                };

                return str.Take(5).ToArray();
            });*/
            Func<string[], string[]> summarize = new Func<string[], string[]>((toSummarize) =>
            {
                // Need to change
                //List<string> sumList = new List<string>();
                OpenTextSummarizer.SummarizerArguments args = new OpenTextSummarizer.SummarizerArguments();
                args.InputString = String.Join(" ", toSummarize);
                OpenTextSummarizer.SummarizedDocument sd = OpenTextSummarizer.Summarizer.Summarize(args);
                List<string> str = new List<string>();
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

        //definiTE REFACTOR/////////////////////
        /* private static void populateTupList(string paragraph, QueryHandler qHandler, 
                                             ref BlockDataAndKeyWords tupList, ref List<string> hrefs,
                                             ref List<string> fileText)
         {
               var sentences = paragraph.Split('.');
               var htmlM = new HTMLMessager();
               //Console.WriteLine(sentences.Count() + " " + paragraph.Count());
               foreach (var response in qHandler.handleQuery(sentences))
               { 
                   var block = response.Item2.ToList();
                   List<string> strings = new List<string>();
                   foreach (var element in block)
                   {
                       strings.AddRange(element.Item2);
                   }
                   foreach (var tup in response.Item2)
                   {
                       Console.WriteLine("\n\n hi" + tup.Item1 + " \n\n");
                       //string refs = s;
                       //htmlM.removeFromLine(ref refs);
                       //fileText.Add(" " + refs);
                   }
                   fileText.Add(null);
                   tupList.AddRange(getTupList(block));
                   tupList.Add(new Tuple<string[], string[]>(null, null));

                   hrefs.AddRange(response.Item1);
                   hrefs.Add(null);                
               }
         }*/
        public static string backendCommand(string paragraph)
        {
            loadIronPython();
            //BlockDataAndKeyWords tupList = new BlockDataAndKeyWords();
            //List<string> hrefs = new List<string>();
            //List<string> imgs = new List<string>();
            //List<string> fileText = new List<string>();
            //populateTupList(paragraph, getNewQueryHandler(), ref tupList, ref hrefs, ref fileText);

            /*string rString = "";
            var maxStrings = getMaxStrings(tupList);
            foreach (string s in maxStrings)
            {
                rString = rString + s + "\n";                
            }

            //aggregates file contents and runs phrase generator
            string singleText = "";
            
            phrase_generator = phrase_generator_task.Result;
            foreach (var str in fileText)
            {
                if (str == null)
                {
                    //phrase_generator.text_to_phrases(singleText, maxStrings);
                    Console.WriteLine(singleText);
                    singleText = "";
                    continue;
                }
                singleText = singleText +" "+ str;
            }
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
        }*/

            phrase_generator = phrase_generator_task.Result;
            //will not handle ands and such since it just takes paragraph
            string rString = "This article covers topics and keywords related to: \n";
            foreach (var response in getNewQueryHandler().handleQuery(new string[] { paragraph }))
            {
                string text = "";
                foreach (var element in response.elements)
                {
                    text = text + " " + element;
                }
                //phrase_generator.text_to_phrases(str, response.keywords);

                foreach (var kw in response.keywords)
                {
                    rString += kw + "\n";
                }

                if (response.hrefs.Count() > 1)
                {
                    rString += "\nRelated documents:\n";
                    int count = 0;
                    foreach (string s in response.hrefs)
                    {
                        if (s != null)
                        {
                            rString = rString + s + (count == 0 ? " | " : "");
                            count++;
                        }
                    }
                }
            }
            return rString;
        }
    }
}