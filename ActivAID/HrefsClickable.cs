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
        List<TextBlock> tbList;
        string link;
        public HrefsClickable(ref TextBlock tb, string outputToUI, string link)
        {
            this.outputToUI = outputToUI;
            this.link = link;
            tbList = new List<TextBlock>();
            tbList.Add(tb);
            tb.Text = outputToUI;
            tb.MouseUp += callback;
        }
        public override void callback(Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(link);
        }
    }
}
