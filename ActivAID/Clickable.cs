using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ActivAID
{
    public abstract class Clickable
    {
        //UICOMPONENT
        //ClickHandler
        protected string outputToUI;
        public abstract void callback(Object sender, System.Windows.Input.MouseButtonEventArgs e);
        //register
    }
}
