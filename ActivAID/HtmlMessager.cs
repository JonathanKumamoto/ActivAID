using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
//this will act as a wrapper for the parser

namespace ActivAID
{
    class HTMLMessager
    {
        private bool resolveLi = true;
        private string path;
        
        private void handleLiTag(ref string line)//test these expressions
        {

            //case where li tag on one line
            //case where start tag caused entry
            //case where end tag caused entry
            Regex regexNestedTag = new Regex("^.*<li.*>.*<.*>.*<" + (char)(92) + "/li>$");//might have to handle whitespace 
                                                                                          //with tags
            if (regexNestedTag.IsMatch(line))
            {
                resolveLi = true;
                //handle taking nested tag out
            }

        }

        public void removeFromLine(ref string line)
        {
            Regex regexLiTag = new Regex("^.*<li.*>.*|^.*<" + (char)(92) + "/li>.*$"); //might have to look for mult lines
            Regex regexUlTagBegin = new Regex("< *ul");
            Regex regexUlTagEnd = new Regex("ul *>");
            //Regex regexLT = new Regex("&lt;"); need to figure out how to reinject
            //Regex regexGT = new Regex("&gt;");
            Regex regexHtmlChar = new Regex("&.*;");

            /////////////////we'll see if we need to even handle this/////////
            /*if (regexLiTag.IsMatch(line))
            {
                resolveLi = false;
                handleLiTag(ref line);
            }*/

            line = regexUlTagBegin.Replace(line, "<ol");
            line = regexUlTagEnd.Replace(line, "ol>");
            //line = regexLT.Replace(line, "<");
            //line = regexGT.Replace(line, ">");
            line = regexHtmlChar.Replace(line, "");
        }

        private string giveTempPath()
        {
            string bName = System.IO.Path.GetFileName(path);
            string NBPath = new Regex("[^\\\\]*.html").Replace(path, "");
            return NBPath + "TempForParser" + bName;
        }

        /*~HTMLMessager()
        {
            System.IO.File.Delete(giveTempPath());
        }*/
    }
}
