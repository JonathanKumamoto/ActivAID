using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivAID
{
    public class HrefsClickable : Clickable
    {
        public override void callback(Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(outputToUI);
        }
    }
}
