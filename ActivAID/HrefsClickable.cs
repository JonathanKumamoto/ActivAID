using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ActivAID
{
    public class HrefsClickable : Clickable
    {
        TextBlock tb;
        string link;
        public HrefsClickable(ref TextBlock tb, string outputToUI, string link)
        {
            this.outputToUI = outputToUI;
            this.link = link;
            tb.Text = outputToUI;
            tb.MouseUp += callback;
            this.tb = tb;
        }
        public override void callback(Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(link);
            }
            catch
            {; }
        }
    }
}
