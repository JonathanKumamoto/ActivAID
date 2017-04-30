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
using System.Diagnostics;

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
                    //points to python site-packages
                    string spackagesPath = Environment.GetEnvironmentVariable("SPACKAGES");
                    //points to python libs
                    string libsPath = Environment.GetEnvironmentVariable("LIBS");
                    //points to phrase generator script
                    string codePath = Environment.GetEnvironmentVariable("CODE");
                    var searchPaths = engine.GetSearchPaths();
                    //path to site-packages
                    //searchPaths.Add(codePath);
                    searchPaths.Add(codePath);
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

                return str.Take(5).ToArray();
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

        private static string getCommand(List<string> keywords, string text)
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
            var command = "phrase_generator.py \"" + text.Replace("\n", "");
            command = command.Replace("\r", "").Replace("\r\n", "").Replace("|", " ") + "\" ";
            command += pyListString;
            return command;
        }

        private static string callPython(string command)
        {
            Process pProcess = new Process();
            pProcess.StartInfo.FileName = "python.exe";
            pProcess.StartInfo.Arguments = command;
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
                rString += checkForPhrase(kw, rString, phrases);
                ++count;
            }
            return rString;
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

        public static string backendCommand(string paragraph)
        {
            phrase_generator = phrase_generator_task.Result;
            //will not handle ands and such since it just takes paragraph
            string rString = "This article covers topics and keywords related to: \n";
            var responses = getNewQueryHandler().handleQuery(new string[] { paragraph });
            foreach (var response in responses)
            {
                string text = getFullText(response.elements);
                //dynamic a = phrase_generator.text_to_phrases(text, response.keywords);
                //Console.WriteLine(IronPython.Modules.Builtin.len(a));
                string strOutput = callPython(getCommand(response.keywords, text));

                /*var dict = new IronPython.Runtime.PythonDictionary();
                var scriptDomain = new IronPython.Runtime.ScriptDomainManager();
                var pcontext = new IronPython.Runtime.PythonContext(scriptDomain);
                var module = new IronPython.Runtime.ModuleContext(dict, pcontext);
                dynamic retList = IronPython.Modules.Builtin.eval(new IronPython.Runtime.CodeContext(dict, module), strOutput);*/

                //dynamic retList = engine.Execute("eval(\""+ strOutput + "\")");
                List<string> phrases = getPhrases(strOutput);

                rString = addKeywordsOrPhrases(response.keywords, phrases, rString);
                if (response.hrefs.Count() > 1)
                {
                    rString = addHrefs(response.hrefs, rString);   
                }
            }
            return rString;
        }
    }
}