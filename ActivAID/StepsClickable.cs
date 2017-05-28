using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Shapes;

namespace ActivAID
{
    public class StepsClickable : Clickable
    {
        Stack<string> steps;
        TextBlock tb;
        string endText;
        QueryResponse qr;
        const string DEFAULTFILE = @"C:\Program Files\ActivATE\ActivATE 5.x\Logs\ACLLog_";
        

        public StepsClickable(ref TextBlock tb, string outputToUI, List<string> steps, QueryResponse qr)
        {
            this.outputToUI = outputToUI;
            this.steps = new Stack<string>(steps);
            this.steps.Pop();
            endText = steps.Count() == 0 ? "\n - END -" : "\n - CLICK FOR MORE INFO -";
            tb.Text = outputToUI + endText;
            tb.MouseLeftButtonUp += callback;
            tb.MouseRightButtonUp += activATECallback;
            this.tb = tb;
            this.qr = qr;
        }

        public bool hasActivAIDHook(string filePath)
        {
            System.IO.FileInfo f = new System.IO.FileInfo(filePath);
            string fileName = f.Name;
            return fileName == "EditUser.html" || fileName == "NewTestProgram.html";
        }

        public void activATECallback(Object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Console.WriteLine("\n\n"+qr.responseHTML+"\n\n"+hasActivAIDHook(qr.responseHTML));
            if (hasActivAIDHook(qr.responseHTML))
            {
                Console.WriteLine("\n\nHI\n\n");
                System.IO.FileInfo f = new System.IO.FileInfo(qr.responseHTML);
                string fileName = f.Name;
                var aInterface = new ActivateInterface();
                aInterface.Init();
                if (fileName == "NewTestProgram.html")
                {
                    aInterface.LaunchNewTP();
                }
                else if (fileName == "EditUser.html")
                {
                    aInterface.LaunchEditUsers();
                }
            }
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
