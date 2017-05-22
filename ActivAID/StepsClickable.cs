using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace ActivAID
{
    public class StepsClickable : Clickable
    {
        Stack<string> steps;
        List<TextBlock> tbList;

        public StepsClickable(ref TextBlock tb, string outputToUI, List<string> steps, QueryResponse qr)
        {
            this.outputToUI = outputToUI;
            this.steps = new Stack<string>(steps);
            this.steps.Pop();
            tbList = new List<TextBlock>();
            tbList.Add(tb);
            tb.Text = outputToUI;
            tb.MouseUp += this.callback;
        }

        public override void callback(Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string toUI;
            try
            {
                toUI = steps.Pop();
            }
            catch (InvalidOperationException)
            {
                toUI = "\n- END -";
            }
            tbList.First().Text = tbList.First().Text + '\n' + toUI.Trim();
        }
    }
}
