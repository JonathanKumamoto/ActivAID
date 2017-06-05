using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Controls;

namespace ActivAID
{
    public class KeyWordClickable : Clickable
    {
        Stack<string> relatedBlocks;
        TextBlock tb;
        string endText;

        public KeyWordClickable(ref TextBlock tb, string outputToUI, QueryResponse qr)
        {
            this.outputToUI = outputToUI;
            relatedBlocks = getRelatedBlocks(outputToUI, qr);
            endText = relatedBlocks.Count() == 0 ? "\n - That is all I know! -" : "\n - Click for more info -\n";
            tb.Text = outputToUI;
            tb.MouseUp += callback;
            this.tb = tb;
        }

        public override void callback(Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string toUI;
            string tbText;
            try
            {
                toUI = relatedBlocks.Pop();
                tbText = Regex.Replace(tb.Text, "- Click for more info -\n", "");
                endText = relatedBlocks.Count() == 0 ? "\n - That is all I know! -" : "\n - Click for more info -\n";
                toUI = toUI + endText;
            }
            catch (InvalidOperationException)
            {
                tbText = tb.Text;
                toUI = "";
            }
            tb.Text = tbText + (toUI.Trim() == "" ? "" : '\n' + toUI.Trim());
        }

        private Stack<string> getRelatedBlocks(string keyword, QueryResponse response)
        {
            List<string> retStrings = new List<string>();
            List<string> temp = new List<string>();
            int prev = -99;
            int count = 0;
            bool containsKeyWord = false;
            foreach (var block in response.blocks)
            {
                if (count == 0)
                {
                    prev = block.Key;
                }
                ++count;
                foreach (string element in block.Value)
                {
                    if (Regex.Match(element.ToLower(), keyword).Success)
                    {
                        containsKeyWord = true;
                    }
                    temp.Add(element);

                    if(prev == block.Key)
                    {
                        if (containsKeyWord)
                        {
                            retStrings.Add(String.Join(" ", temp.Select((x) => { return Regex.Replace(x, @"[\r\n\t]| {2,}", ""); }).ToArray()));
                        }
                        temp = new List<string>();
                        containsKeyWord = false;
                    }
                }
                prev = block.Key;
            }
            return new Stack<string>(retStrings.ToArray());
        }
    }
}
