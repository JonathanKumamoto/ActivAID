using System.Threading.Tasks;
using Syn.Bot.Siml;
using System.IO;
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ActivAID
{
    //put in common folder
    [XmlRoot(ElementName = "file")]
    public class FileRegex
    {
        [XmlElement(ElementName = "fileName")]
        public string name { get; set; }

        [XmlElement(ElementName = "pattern")]
        public string pattern { get; set; }

    }

    [XmlRoot(ElementName = "patterns")]
    public class RegexList
    {
        public RegexList()
        {
            filePatternsArray = new List<FileRegex>();
        }
        public void Add(FileRegex fgex)
        {
            filePatternsArray.Add(fgex);
        }
        [XmlArrayItem()]
        public List<FileRegex> filePatternsArray { set; get; }
    }

    public class UserInputBoiler
    {
       /* public string boilDown(string sentence)
        {
            RegexList fgexes = new RegexList();
            try
            {
                System.IO.StreamReader str = new System.IO.StreamReader(@"config_patterns.xml");
                System.Xml.Serialization.XmlSerializer xSerializer = new System.Xml.Serialization.XmlSerializer(typeof(RegexList));
                fgexes = (RegexList)xSerializer.Deserialize(str);
                str.Close();
            }
            catch (Exception ex)
            {
                fgexes = new RegexList();
            }
            FileRegex maxFgex = new FileRegex();
            int max = -99;
            foreach(var fgex in fgexes.filePatternsArray)
            {
                Regex pattern = new Regex(fgex.pattern);
                int numMatches = pattern.Matches(sentence).Count;
                if (numMatches > max)
                {
                    max = numMatches;
                    maxFgex = fgex;
                }
            }
            return max <= 0 ? null : maxFgex.name;
        }*/
        public string boilDown(string sentence)
        {
            int max = -1;
            string maxFile = "";
            string path = "..\\..\\SynBotDir";
            foreach (string fileName in System.IO.Directory.EnumerateFiles(path)) //maybe don't hard code
            {
                SimlBot Chatbot = new SimlBot();
                Chatbot.PackageManager.LoadFromString(File.ReadAllText(fileName));
                var result = Chatbot.Chat(sentence);
                string outString = result.BotMessage;
                if (!outString.Contains(";"))
                {
                    continue;
                }                
                string[] output = result.BotMessage.Split(';');
                var lastString = output[output.Length-2];
                var response = lastString.Split(':');
                int test = Convert.ToInt32(response[0]);
                maxFile = max < test ? response[1] : maxFile;
                max = max < test ? test : max;
            }
            return maxFile;
        }
    }
}