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
        bool end = false;

        public StepsClickable(ref TextBlock tb, string outputToUI, List<string> steps, QueryResponse qr)
        {
            this.outputToUI = outputToUI;
            this.steps = new Stack<string>(steps);
            this.steps.Pop();
            tb.Text = outputToUI;
            tb.MouseUp += this.callback;
            this.tb = tb;
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
                toUI = "";
                if (!end)
                {
                    toUI = "\n- END -";
                    end = true;
                }
            }
            tb.Text = tb.Text + (toUI.Trim() == "" ? "" : "\n" + toUI.Trim());
        }
    }
}
