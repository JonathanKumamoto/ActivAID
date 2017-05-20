using System.Text.RegularExpressions;


namespace ActivAID
{
    class HTMLMessager
    {
        private bool resolveLi = true;
        private string path;
        
        private void handleLiTag(ref string line)
        {
            Regex regexNestedTag = new Regex("^.*<li.*>.*<.*>.*<" + (char)(92) + "/li>$");
            if (regexNestedTag.IsMatch(line))
            {
                resolveLi = true;
            }

        }

        public void removeFromLine(ref string line)
        {
            Regex regexLiTag = new Regex("^.*<li.*>.*|^.*<" + (char)(92) + "/li>.*$");
            Regex regexUlTagBegin = new Regex("< *ul");
            Regex regexUlTagEnd = new Regex("ul *>");
            Regex regexHtmlChar = new Regex("&.*;");

            line = regexUlTagBegin.Replace(line, "<ol");
            line = regexUlTagEnd.Replace(line, "ol>");
            line = regexHtmlChar.Replace(line, "");
        }

        private string giveTempPath()
        {
            string bName = System.IO.Path.GetFileName(path);
            string NBPath = new Regex("[^\\\\]*.html").Replace(path, "");
            return NBPath + "TempForParser" + bName;
        }
    }
}
