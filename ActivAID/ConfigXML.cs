using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Common
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
}
