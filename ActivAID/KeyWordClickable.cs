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
        List<TextBlock> tbList;

        public KeyWordClickable(ref TextBlock tb, string outputToUI, QueryResponse qr)
        {
            this.outputToUI = outputToUI;
            relatedBlocks = getRelatedBlocks(outputToUI, qr);
            tbList = new List<TextBlock>();
            tbList.Add(tb);
            tb.Text = outputToUI;
            tb.MouseUp += callback;
        }

        public override void callback(Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string toUI;
            try
            {
                toUI = relatedBlocks.Pop();
            }
            catch(InvalidOperationException)
            {
                toUI = "\n-END-";
            }
            tbList.First().Text = tbList.First().Text + '\n' + toUI.Trim();
        }

        private Stack<string> getRelatedBlocks(string keyword, QueryResponse response)
        {
            List<string> retStrings = new List<string>();
            foreach (var block in response.blocks)
            {
                foreach (string element in block.Value)
                {
                    if (Regex.Match(element, keyword).Success)
                    {
                        retStrings.Add(String.Join(" ", block.Value.ToArray()));
                        break;
                    }
                }
            }
            return new Stack<string>(retStrings.ToArray());
        }
    }
}
