﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using BlockDataAndKeyWords = System.Collections.Generic.List<System.Tuple<string[], string[]>>;
using System.Diagnostics;
using System.Windows.Controls;
using System.Runtime.Serialization;
using System.IO;

namespace ActivAID
{
    delegate void ActionRef(QueryResponse qr, ref string item);
    delegate void ActionRefList(QueryResponse qr, ref List<string> rList);

    public class BackEnd
    {
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

       public static void loadIronPython()
        {
            if (phrase_generator_task == null)
            {
                phrase_generator_task = Task<dynamic>.Factory.StartNew
                (() =>
                {
                    try
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
                    }
                    catch(Exception)
                    {
                        return null;
                    }
                });
            }
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

        private static void addKeywordsOrPhrases(List<string> keywords, List<string> phrases, string rString, ref List<string> rList)
        {
             foreach (var kw in keywords)
             {
                 rString = checkForPhrase(kw, rString, phrases);
                 rString = new Regex("[\n\r]{2,}").Replace(rString, "");
                 rList.Add(rString);
             }
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
            try
            {
                IronPython.Runtime.PythonGenerator gen = (IronPython.Runtime.PythonGenerator)phrase_generator.gen_phrases(text, keywords);
                List<string> phraseList = new List<string>();
                foreach (string str in gen.Cast<string>())
                {
                    phraseList.Add(str);
                }
                return phraseList;
            }
            catch (Exception)
            {
                return new List<string>();
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

        private static List<string> aggregateReturnList(List<QueryResponse> responses, ActionRefList aggregateFunction)
        {
            List<string> rList = new List<string>();
            foreach (var response in responses)
            {
                aggregateFunction(response, ref rList);
            }
            return rList;
        }

        private static void aggSteps(QueryResponse response, ref List<string> rList)
        {
            Func<string, string> stringOp = new Func<string, string>((x) =>
            {
                var temp = x;
                new HTMLMessager().removeFromLine(ref temp);
                return temp;
            });
            int prev = -99;
            bool foundSteps = false;
            int count = 0;
            

            foreach (var kv in response.blocks)
            {   
                if (kv.Key != prev )//&& outerCounter != 0)
                {
                    var messageStep = kv.Value.Select((x) => { return stringOp(x); }).ToList();
                    var removedTrailingWhiteSpace = messageStep.Select((x) => { return x.Trim(); }).ToArray();
                    foundSteps = true;
                    if (count != 0)
                    {
                        rList.Add(String.Join(" ", removedTrailingWhiteSpace));
                    }
                    else
                    {
                        rList.Add(removedTrailingWhiteSpace.Last());
                    }
                    ++count;
                }
                else if (foundSteps)
                {
                    break;
                }
                prev = kv.Key;
                //++outerCounter;
            }
        }

        private static void aggKeywords(QueryResponse response, ref List<string> rList)
        {
            string text = getFullText(response.elements);
            List<string> phrases = getPhrases(text, response.keywords);
            addKeywordsOrPhrases(response.keywords, phrases, String.Join(" ", rList), ref rList);
        }

        private static List<TextBlock> handleHrefCommand(string paragraph)
        {
            List<TextBlock> rList = new List<TextBlock>();
            string fileToGet = paragraph.Split(':')[1];
            if (fileToGet.Trim() == "")
            {
                throw new NoFileMatchException();
            }

            var hrefResponses = getNewQueryHandler().handleQuery(new string[] { fileToGet });
            var tb = new TextBlock();
            tb.Text = "Here are the links associated with your request: ";
            rList.Add(tb);
            List<Tuple<string, string>> aggHrefs = new List<Tuple<string, string>>();
            aggHrefs.Add(new Tuple<string, string>(hrefResponses.First().originalSentence, hrefResponses.First().responseHTML));
            foreach (var response in hrefResponses)
            {
                aggHrefs.AddRange(response.hrefs);
            }
            foreach (var hrefTup in aggHrefs)
            {
                if (!hrefTup.Item2.Contains("#"))
                {
                    tb = new TextBlock();
                    rList.Add(tb);
                    new HrefsClickable(ref tb, hrefTup.Item1, @"Media\HelpHTML\"+Path.GetFileName(hrefTup.Item2));
                }
            }
            return rList;
        }

        private static List<TextBlock> handleStepsCase(string paragraph, List<QueryResponse> responses)
        {
            List<TextBlock> rList = new List<TextBlock>();
            List<string> steps = aggregateReturnList(responses, aggSteps);
            string fString = "Here are some steps that are relevant to your request: \n" + steps.First();
            var tb = new TextBlock();
            if (steps.Count() <= 1)
            {
                throw new Exception();
            }
            new StepsClickable(ref tb, fString, steps, responses.First());
            rList.Add(tb);
            return rList;
        }

        private static List<TextBlock> handleKWCase(string paragraph, List<QueryResponse> responses)
        {
            string fString = "The following clickable documents cover topics and keywords related to: ";
            var tb = new TextBlock();
            List<TextBlock> rList = new List<TextBlock>();
            tb.Text = fString;
            rList.Add(tb);
            List<string> kwList = aggregateReturnList(responses, aggKeywords);
            foreach (var kw in kwList)
            {
                if (kw.Trim().Count() <= 2)
                {
                    continue;
                }
                tb = new TextBlock();
                new KeyWordClickable(ref tb, kw, responses.First());
                rList.Add(tb);
            }
            return rList;
        }

        private static List<TextBlock> handleDoCommand(string paragraph, QueryResponse responseToDo)
        {
            var rList = new List<TextBlock>();
            if (hasActivATEHook(responseToDo.responseHTML))
            {
                var tb = new TextBlock();
                tb.Text = "Taking Care Of It...";
                rList.Add(tb);
            }
            return rList;
        }

        private static bool hasActivATEHook(string filePath)
        {
            System.IO.FileInfo f = new System.IO.FileInfo(filePath);
            string fileName = f.Name;
            return fileName == "EditUser.html" || fileName == "NewTestProgram.html" || fileName == "CHANGE";
        }

        private static void callActivATE(QueryResponse qr)
        {
            if (hasActivATEHook(qr.responseHTML))
            {
                System.IO.FileInfo f = new System.IO.FileInfo(qr.responseHTML);
                string fileName = f.Name;
                var aInterface = new ActivateInterface();
                aInterface.Init();
                if (fileName == "NewTestProgram.html")
                {
                    aInterface.LaunchNewTP();
                }
                else if (fileName == "EditUser.html")
                {
                    aInterface.LaunchEditUsers();
                }
                else if (fileName == "CHANGE")
                {
                    aInterface.LaunchChangeUser();
                }
            }
        }

        public static List<TextBlock> backendCommand(string paragraph)
        {
            if (Regex.IsMatch(paragraph, "href:"))
            {
                return handleHrefCommand(paragraph);
            }

            if (Regex.IsMatch(paragraph.ToLower(), "do:"))
            {
                List<QueryResponse> responsesToDo;
                if (paragraph == "do: change user")
                {
                    responsesToDo = new List<QueryResponse>();
                    var qr = new QueryResponse("CHANGE",null,null,null,null, new Dictionary<int, List<string>>());
                    responsesToDo.Add(qr);
                }
                else
                {
                    string fileToGet = paragraph.Split(':')[1];
                    try
                    {
                        responsesToDo = getNewQueryHandler().handleQuery(new string[] { fileToGet });
                    }
                    catch (NoFileMatchException)
                    {
                        responsesToDo = new List<QueryResponse>();
                        responsesToDo.Add(new QueryResponse("Bad Command", null, null, null, null, new Dictionary<int, List<string>>()));
                    }
                }
                var rList = handleDoCommand(paragraph, responsesToDo.First());
                if (rList.Count() > 0)
                {
                    callActivATE(responsesToDo.First());
                }
                else
                { 
                    rList.Clear();
                    var tb = new TextBlock();
                    tb.Text = ":( :( :( YOU REALLY CONFUSED ME! You just asked me to do something I can not do :( :( :(";
                    rList.Add(tb);
                }
                return rList;
            }
        

            var responses = getNewQueryHandler().handleQuery(new string[] { paragraph });
            phrase_generator = phrase_generator_task.Result;
            try
            {
                return handleStepsCase(paragraph, responses);
            }
            catch (Exception)
            {
                return handleKWCase(paragraph, responses);
            }
        }
    }

    [Serializable]
    internal class NoCommandException : Exception
    {
        public NoCommandException()
        {
        }

        public NoCommandException(string message) : base(message)
        {
        }

        public NoCommandException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoCommandException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}