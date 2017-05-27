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
        TextBlock tb;
        string endText;

        public StepsClickable(ref TextBlock tb, string outputToUI, List<string> steps, QueryResponse qr)
        {
            this.outputToUI = outputToUI;
            this.steps = new Stack<string>(steps);
            this.steps.Pop();
            endText = steps.Count() == 0 ? "\n - END -" : "\n - CLICK FOR MORE INFO -";
            tb.Text = outputToUI + endText;
            tb.MouseUp += callback;
            tb.MouseLeftButtonUp += activATECallback;
            this.tb = tb;
        }

        public void activATECallback(Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ;
        }

        public override void callback(Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string toUI;
            string tbText;
            try
            {
                toUI = steps.Pop();
                tbText = Regex.Replace(tb.Text,"- CLICK FOR MORE INFO -", "");
                endText = steps.Count() == 0 ? "\n - END -" : "\n - CLICK FOR MORE INFO -";
                toUI = toUI + endText;
            }
            catch (InvalidOperationException)
            {
                tbText = tb.Text;
                toUI = "";
            }
            tb.Text = tbText.Trim() + (toUI.Trim() == "" ? "" : "\n" + toUI.Trim());
        }
    }
}
