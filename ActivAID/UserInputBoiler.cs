using System.Threading.Tasks;
using Syn.Bot.Siml;
using System.IO;
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Common;

namespace ActivAID
{

    public class UserInputBoiler
    {
        private RegexList fgexes;

        private void initializeFGEXES()
        {
            try
            {
                System.IO.StreamReader str = new System.IO.StreamReader(@"Patterns.exe.config");
                System.Xml.Serialization.XmlSerializer xSerializer = new System.Xml.Serialization.XmlSerializer(typeof(RegexList));
                fgexes = (RegexList)xSerializer.Deserialize(str);
                str.Close();
            }
            catch (Exception ex)
            {

                fgexes = new RegexList();
            }
        }

        private bool isAcceptableKeyWord(string potentialKeyWord)
        {
            return potentialKeyWord.Length > 1 && !(new Regex(@"[=\|\n\t\r;\-:'\/\,<\>%\!]|[0-9]").IsMatch(potentialKeyWord));
        }

        private string splitFileNamePattern(string fileName)
        {
            List<string> retArray = new List<string>();
            string aggregateString = "";
            int check = 0;
            foreach (char ch in fileName)
            {
                if (ch < 97)
                {
                    aggregateString += (ch + 32);
                    if (check == 0)
                    {
                        ++check;
                    }
                    else
                    {
                        if (isAcceptableKeyWord(aggregateString))
                        {
                            retArray.Add(aggregateString);
                        }
                        aggregateString = "";
                    }
                }
                else
                {
                    aggregateString += ch;
                }
            }
            return String.Join("|", retArray.ToArray());
        }

        private string handleTie(string check, List<string> tiedStrings)
        {
            int max = -99;
            string maxString = "";
            foreach (string fileName in tiedStrings)
            {
                int matches = Regex.Matches(check, splitFileNamePattern(fileName).Replace(" ", "")).Count;
                if (matches > max)
                {
                    max = matches;
                    maxString = fileName;
                }
            }
            return maxString;
        }

        private string getMaxRegexMatchesFile(string check)
        {
            int max = -99;
            List<string> maxStrings = new List<string>();
            foreach (var fgex in fgexes.filePatternsArray)
            {
                int matches = Regex.Matches(check, fgex.pattern.Replace(" ", "")).Count;


                if (matches == max)
                {
                    maxStrings.Add(fgex.name);
                }
                if (matches > max)
                {
                    max = matches;
                    maxStrings = new List<string>();
                    maxStrings.Add(fgex.name);
                }
            }
            if (max > 1)
            {
                return handleTie(check, maxStrings);
            }
            else
            {
                return "No Acceptable Match Found";
            }
        }
        public string boilDown(string sentence)
        {
            initializeFGEXES();
            string retString = getMaxRegexMatchesFile(sentence);
            if (retString == "No Acceptable Match Found")
            {
                throw new NoFileMatchException();
            }
            return retString;
        }
    }
}