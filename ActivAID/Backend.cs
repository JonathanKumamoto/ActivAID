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
                    //points to python libs
                    string libspath = Environment.GetEnvironmentVariable("LIBS");
                    //points to phrase generator script
                    string codepath = Environment.GetEnvironmentVariable("CODE");
                    var searchpaths = engine.GetSearchPaths();
                    //path to site-packages
                    //searchpaths.add(codepath);
                    searchpaths.Add(codepath);
                    searchpaths.Add(spackagespath);
                    searchpaths.Add(libspath);
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

        private static string getCommand(string text, List<string> keywords)
        {
            string pyListString = "\"[";
            int check = 0;
            foreach (var kw in keywords)
            {
                if (check != 0)
                {
                    pyListString += ",";
                }
                pyListString += "'" + kw + "'";
                check += 1;
            }
            pyListString += "]\"";
            var command = "phrase_generator.py \"" + text.Replace("\n", "").Replace("\r", "");
            command = command.Replace("\r", "").Replace("\r\n", "").Replace("|", " ") + "\" ";
            command += pyListString;
            return command;
        }

        private static string callPython(string command)
        {
            Process pProcess = new Process();
            pProcess.StartInfo.FileName = "python.exe";
            pProcess.StartInfo.Arguments = command.Replace("\n"," ").Replace("\t", " ").Replace("\r", " ");

            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.Start();
            return pProcess.StandardOutput.ReadToEnd();
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
                rString += "\t" + checkForPhrase(kw, rString, phrases);
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

        public static List<string> getPhrases(string strOutput)
        {
            IronPython.Runtime.PythonGenerator gen = (IronPython.Runtime.PythonGenerator)phrase_generator.eval_string(strOutput);
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
            string strOutput = callPython(getCommand(text, response.keywords));
            List<string> phrases = getPhrases(strOutput);

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
                        //rString += prevString;
                        rString += "\n";
                       
                        rString += "\t" + String.Join(" ", removedTrailingWhiteSpace);
                    }
                    else
                    {
                        rString += "\t" + removedTrailingWhiteSpace.Last();
                    }
                    ++count;
                }
                else if (foundSteps)
                {
                    break; 
                }
                prev = kv.Key;
                var prevMessageStep = kv.Value.Select((x) => { return stringOp(x); }).ToList();
                var prevRemovedTrailingWhiteSpace = prevMessageStep.Select((x) => { return x.Trim(); }).ToArray();
                prevString = String.Join(" ", prevRemovedTrailingWhiteSpace);
            }
            
        }

        private static string getInitialStringFromMode(string mode)
        {
            if (mode == "keywords")
            { return "This article covers topics and keywords related to: \n"; }
            else if (mode == "steps")
            { return "Here are some steps that are relevant to your request: \n"; }
            else { return ""; }
        }

        public static string backendCommand(string paragraph, string mode)
        {
            phrase_generator = phrase_generator_task.Result;
            string rString = getInitialStringFromMode(mode);
            var responses = getNewQueryHandler().handleQuery(new string[] { paragraph });
            foreach (var response in responses)
            {
                if (mode == "keywords")
                {
                    getKeyWords(response, ref rString);
                }
                else if (mode == "steps")
                {
                    getSteps(response, ref rString);
                }
               
            }
            return rString;
            
        }
    }
}