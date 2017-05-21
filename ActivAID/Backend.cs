using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using BlockDataAndKeyWords = System.Collections.Generic.List<System.Tuple<string[], string[]>>;
using System.Diagnostics;

namespace ActivAID
{
    delegate void ActionRef(QueryResponse qr, ref string item);

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

       public static void loadIronPython()//tuple<scriptengine, scriptscope> loadironpython()
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
                    //points to python site-packages
                    string spackagespath = Environment.GetEnvironmentVariable("SPACKAGES");
                    //points to iron python lib folder
                    string libpath = Environment.GetEnvironmentVariable("LIB");
                    var searchpaths = engine.GetSearchPaths();
                    searchpaths.Add(spackagespath);
                    searchpaths.Add(libpath);
                    engine.SetSearchPaths(searchpaths);
                    return engine.ImportModule("phrase_generator");
                });


                //builtinscope = python.getbuiltinmodule(engine);
                //phrase_generator = engine.importmodule("phrase_generator");
            }
            //return new tuple<scriptengine, scriptscope>(phrase_generator, builtinscope);
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
                }

                return str.Take(8).ToArray();
            });


            UserInputBoiler sb = new UserInputBoiler();
            DataAccess dA = new DataAccessDB();
            return new QueryHandler(dA, sb, func, summarize);
        }

        private static string getFullText(List<string> elements)
        {
            string text = "";
            foreach (var element in elements)
            {
                string e = element;
                new HTMLMessager().removeFromLine(ref e);
                text += " " + e;
            }
            return text;
        }

        private static string checkForPhrase(string kw, string rString, List<string> phrases)
        {
            int count = 0;
            foreach(var phrase in phrases)
            {
                if (new Regex(kw).Match(phrase).Success && !new Regex(phrase).Match(rString).Success)
                {                    
                    phrases.RemoveAt(count);
                    return phrase;
                }
                ++count;
            }
            return kw;
        }

        private static string addKeywordsOrPhrases(List<string> keywords, List<string> phrases, string rString)
        {
            int count = 0;
            foreach (var kw in keywords)
            {
                if (count != 0)
                {
                    rString += "\n";
                }
                rString += "     " + checkForPhrase(kw, rString, phrases);
                ++count;
            }
            return new Regex("[\n\r]{2,}").Replace(rString, "");
        }

        private static string addHrefs(List<string> hrefs, string rString)
        {
            rString += "\nRelated documents:\n";
            int count = 0;
            foreach (string s in hrefs)
            {
                if (s != null)
                {
                    rString += s + (count == 0 ? " | " : "");
                    count++;
                }
            }
            return rString;
        }

        public static List<string> getPhrases(string text, List<string> keywords)
        {
            IronPython.Runtime.PythonGenerator gen = (IronPython.Runtime.PythonGenerator)phrase_generator.gen_phrases(text, keywords);
            List<string> phraseList = new List<string>();
            foreach (string str in gen.Cast<string>())
            {
                phraseList.Add(str);
            }
            return phraseList;
        }

        private static void getKeyWords(QueryResponse response, ref string rString)
        {
            string text = getFullText(response.elements);
            List<string> phrases = getPhrases(text, response.keywords);

            rString = addKeywordsOrPhrases(response.keywords, phrases, rString);
            if (response.hrefs.Count() > 1)
            {
                rString = addHrefs(response.hrefs, rString);
            }
        }

        private static void getSteps(QueryResponse response, ref string rString)
        {
            Func<string, string>stringOp = new Func<string, string>((x) =>
            {
                var temp = x;
                new HTMLMessager().removeFromLine(ref temp);
                return temp;
            });
            int prev = -99;
            string prevString ="";
            bool foundSteps = false;
            int count = 0;

            foreach (var kv in response.blocks)
            {
                if (kv.Key != prev)
                {
                    var messageStep = kv.Value.Select((x) => { return stringOp(x); }).ToList();
                    var removedTrailingWhiteSpace = messageStep.Select((x) => { return x.Trim(); }).ToArray();
                    foundSteps = true;
                    if (count != 0)
                    {
                        rString += "\n";
                       
                        rString += "\n" + String.Join(" ", removedTrailingWhiteSpace);
                    }
                    else
                    {
                        rString += "\n" + removedTrailingWhiteSpace.Last();
                    }
                    ++count;
                }
                else if (foundSteps)
                {
                    break; 
                }
            }
            
        }

        private List<List<string>> getBlockWithKeyWord(QueryResponse response, string keyword)
        {
            List<List<string>> retStrings = new List<List<string>>();
            foreach (var block in response.blocks)
            {
                foreach (string element in block.Value)
                {
                    if (Regex.Match(element, keyword).Success)
                    {
                        retStrings.Add(block.Value);
                        break;
                    }
                }
            }
            return retStrings;
        }

        private static void aggregateReturnString(List<QueryResponse> responses, ActionRef aggregateFunction, ref string rString)
        {
            foreach (var response in responses)
            {
                aggregateFunction(response, ref rString);
            }
        }

        public static string backendCommand(string paragraph)
        {
            phrase_generator = phrase_generator_task.Result;
            string rString = "";//getInitialStringFromMode(mode);
            var responses = getNewQueryHandler().handleQuery(new string[] { paragraph });
            aggregateReturnString(responses, getSteps, ref rString);
            if (rString.Trim() == "")
            {
                rString = "This article covers topics and keywords related to: \n";
                aggregateReturnString(responses, getKeyWords, ref rString);
            }
            else
            {
                rString = "Here are some steps that are relevant to your request: \n" + rString;
            }
            return rString;
            
        }
    }
}