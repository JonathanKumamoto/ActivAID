using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenTextSummarizer;

namespace ActivAID
{
    public abstract class KeyWordFinder
    {
        private static void insertIntoOccuranceDict(ref Dictionary<string, int> occuranceOfWord, string[] data, int weight)
        {
            foreach (string word in data)//really need to fix this
            {
                if (occuranceOfWord.ContainsKey(word))
                {
                    occuranceOfWord[word.Trim().ToLower()] = occuranceOfWord[word] + 1 * weight;//500 for title
                }
                else
                {
                    occuranceOfWord[word.Trim().ToLower()] = 1 * weight;
                }
            }
        }

        private static string getMaxWord(Dictionary<string, int> dict)
        {
            Tuple<string, int> tup = new Tuple<string, int>("", -9999);
            foreach (var pair in dict)
            {
                if (pair.Value > tup.Item2)
                {
                    tup = new Tuple<string, int>(pair.Key, pair.Value);
                }
            }
            return tup.Item1;
        }

        private static bool throwOutWord(string check, List<string> titleKeyWords)
        {
            string regex = "&.*?;|^\\s*$|^and$|^or$|^not$|^the$|^a$|^an$|^be$|^on$|^being$|^been$|^but$|^in$|^of$|^will$|";
            regex = string.Concat(new string[] { regex, "^under$|^from$|^with$|^without$|^for$|", "^to$|^too$|^so$|^have$|^has$|^had$" });
            Regex regexThrowOut = new Regex(regex);
            return regexThrowOut.IsMatch(check) || check == "" || titleKeyWords.Contains(check);
        }

        private static List<string> getTopKeyWords(int top, Dictionary<string, int> occuranceOfWord, List<string> titleKeyWords)
        {
            List<string> keywords = new List<string>();
            for (var i = 0; i < top && occuranceOfWord.Count() != 0;)
            {
                string maxWord = getMaxWord(occuranceOfWord);
                if (!throwOutWord(maxWord, titleKeyWords))
                {
                    keywords.Add(maxWord);
                    ++i;
                }
                occuranceOfWord.Remove(maxWord);
            }
            return keywords;
        }

        public static List<string> concatAllKeyWords(int top, List<List<Element>> blocks, List<string> titleKeyWords)
        {
            Dictionary<string, int> occuranceOfWord = new Dictionary<string, int>();
            foreach (List<Element> block in blocks)
            {
                foreach (Element element in block)
                {
                    if (element.name != "img")
                    {
                        insertIntoOccuranceDict(ref occuranceOfWord, element.data.Split(' '), 1);
                    }
                }
            }
            return getTopKeyWords(top, occuranceOfWord, titleKeyWords);
        }

        public static List<string> handleLineKeyWords(int top, string sentence)
        {
            /*Dictionary<string, int> occuranceOfWord = new Dictionary<string, int>();
            insertIntoOccuranceDict(ref occuranceOfWord, sentence.Split(' '), 1);
            return getTopKeyWords(top, occuranceOfWord, new List<string>());*/
            List<string> sumList = new List<string>();
            OpenTextSummarizer.SummarizerArguments args = new OpenTextSummarizer.SummarizerArguments();
            args.InputString = String.Join(" ", sentence);
            OpenTextSummarizer.SummarizedDocument sd = OpenTextSummarizer.Summarizer.Summarize(args);
            return sd.Concepts;    
        }

        private static List<string> aggregateKeyWords(int top, List<List<string>> fileKeyWords)
        {
            Dictionary<string, int> occuranceOfWord = new Dictionary<string, int>();
            foreach (var words in fileKeyWords)
            {
                int range = words.Count() - 5;
                insertIntoOccuranceDict(ref occuranceOfWord, words.Take(range).ToArray(), 500);
                insertIntoOccuranceDict(ref occuranceOfWord, words.Skip(range).ToArray(), 1);
            }
            return getTopKeyWords(top, occuranceOfWord, new List<string>());
        }
    }
}
