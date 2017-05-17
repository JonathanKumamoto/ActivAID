using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using ActivAID;
using OpenTextSummarizer;
using System.Xml.Serialization;
using System.IO;

namespace Parser
{
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

    class ParserWrapper
    {
        Dictionary<string, ParsedCHM> parsedCHMs;
        ActivAIDDB db;
        RegexList fgexes;
        private void initializeFGEXES()
        {
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
        }

        public ParserWrapper(string filePath)
        {
            db = new ActivAIDDB();
            Action<string> action = (str) =>
            {
                db.insertIntoFiles(filePath);
                parsedCHMs = new Dictionary<string, ParsedCHM>();
                parsedCHMs[filePath] = new ParsedCHM(filePath);
                initializeFGEXES();
                persistData();
            };

            FileAttributes attr = File.GetAttributes(filePath);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                foreach (string fileName in System.IO.Directory.EnumerateFiles(filePath))
                {
                    action(fileName);
                }
            }
            else
            {
                action(filePath);
            }                        
        }

        public ParserWrapper(List<string> filePaths)
        {
            db = new ActivAIDDB();
            parsedCHMs = new Dictionary<string, ParsedCHM>();
            foreach (string file in filePaths)
            {
                db.insertIntoFiles(file);
                parsedCHMs[file] = new ParsedCHM(file);
            }
            initializeFGEXES();
            persistData();
            Console.WriteLine("Data successfully inserted into database");
        }

        private void insertBlocksIntoDB(string filePath, List<List<Element>> blocks)
        {
            foreach (List<Element> block in blocks)
            {
                int blockCount = 1; // changed to start from 1 not 0
                foreach (Element element in block)
                {
                    if (element.name == "img")
                    {
                        // TODO IMAGE HANDLING
                        ;// db.insertIntoImages();
                    }
                    else
                    {
                        
                        db.insertIntoElements(filePath, blockCount, element.data);
                    }
                    ++blockCount;
                }
            }
        }

      private void insertHREFSOIntoDB(string filePath, List<string> hrefs)
        {
            foreach (string href in hrefs)
            {
                db.insertIntoHyperlinks(filePath, href);
            }
        }

        private string[] splitFileName(string fileName)
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
                        retArray.Add(aggregateString);
                        aggregateString = "";
                    }
                }
                else
                {
                    aggregateString += ch;
                }
            }
            return retArray.ToArray();
        }

        private string[] summarize(string[] toSummarize)
        {
            Func<string, string> stringOp = new Func<string, string>((x) =>
            {
                var temp = x;
                new HTMLMessager().removeFromLine(ref temp);
                temp = new Regex("[=\\|\n\t\r;'/,<>%!]").Replace(temp, "");
                return temp;
            });
            // Need to change
            List<string> sumList = new List<string>();
            OpenTextSummarizer.SummarizerArguments args = new OpenTextSummarizer.SummarizerArguments();
            args.InputString = String.Join(" ", toSummarize.Select((x) => stringOp(x)).ToArray());
            OpenTextSummarizer.SummarizedDocument sd = OpenTextSummarizer.Summarizer.Summarize(args);
            return sd.Concepts.Take(5).ToArray();
        }


        private string generateRegexPatterns(string fileName, List<string> elementData)
        {
            string regexPattern = "";
            int count = 0;
            List<string> keyWords = new List<string>(summarize(elementData.ToArray()));
            keyWords.AddRange(splitFileName(Path.GetFileName(fileName)));
            foreach (string str in summarize(elementData.ToArray()))
            {
                if (count != 0)
                {
                    regexPattern += "|";
                }
                regexPattern += str;
                ++count;
            }
            return regexPattern;
        }


        private List<string> aggregateElementData(List<List<Element>> blocks)
        {
            Func<string, string> message = new Func<string, string>((x) =>
            {
                var temp = x;
                new HTMLMessager().removeFromLine(ref temp);
                return temp;
            });

            List<string> elementData = new List<string>();
            foreach (List<Element> block in blocks)
            {
                foreach (Element element in block)
                {
                    if (element.isText)
                    {
                        elementData.Add(message(element.data));
                    }
                }
            }
            return elementData;
        }
        //tup.Item2.Select((x) => stringOp(x)).ToArray()
        private void getRegexPerFile(string filePath, List<List<Element>> blocks)
        {
            List<string> elementData = aggregateElementData(blocks);
            string regex = generateRegexPatterns(filePath, elementData);
            FileRegex fgex = new FileRegex();
            fgex.name = filePath;
            fgex.pattern = regex;
            fgexes.Add(fgex);

        }

        private void insertRegexIntoConfig()
        {
            using (Stream fileStream = new FileStream("config_patterns.xml", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(RegexList));
                serializer.Serialize(fileStream, fgexes);
            }
        }


        public void persistData()
        {
            //ActiveAIDDB db = new ActiveAIDDB();
            //db.insertIntoFiles(title, "");

            foreach (KeyValuePair<string, ParsedCHM> pair in parsedCHMs)
            {
                getRegexPerFile(pair.Key, pair.Value.blocks);
                insertBlocksIntoDB(pair.Key, pair.Value.blocks);
                insertHREFSOIntoDB(pair.Value.title, pair.Value.hrefs);
            }
            insertRegexIntoConfig();
        }

        /*public void genModel()
        {
            List<List<string>> fileKeyWords = new List<List<string>>();
            List<string> responseFileNames = new List<string>();
            string response;
            foreach (var pair in parsedCHMs)
            {
                List<string> keywords = new List<string>(); 
                responseFileNames.Add(pair.Key);
                keywords.AddRange(KeyWordFinder.handleLineKeyWords(5,pair.Value.title));//keywords weighted by ordering in which they appear in list
                keywords.AddRange(KeyWordFinder.concatAllKeyWords(5, pair.Value.blocks, keywords));
                fileKeyWords.Add(keywords);
            }
            response = String.Join(";", responseFileNames.ToArray());
            //aggregateKeyWords(fileKeyWords)
            //Console.WriteLine(response);
            foreach (string keyword in keywords)
            {
                Console.WriteLine(keyword);
            }
        }*/
    }
}

