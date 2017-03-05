using System.IO;
using System.Windows;
using Syn.Bot.Siml;
using System.Windows.Input;

namespace ActivAID
{
    public partial class MainWindow
    {
        public SimlBot Chatbot; //the SimlBot variable

        public MainWindow()
        {
            InitializeComponent();
            Chatbot = new SimlBot();
            Chatbot.PackageManager.LoadFromString(File.ReadAllText("Knowledge.simlpk"));
        }


        /*
         * Handles the "send" button click
         */ 
        private void SendButton_OnClick(object sender, RoutedEventArgs e)
        {
            var result = Chatbot.Chat(InputBox.Text);
            string outPut = result.BotMessage;
            OutputBox.Text = string.Format("User: {0}\nBot: {1}\n{2}", InputBox.Text, result.BotMessage, OutputBox.Text);
            InputBox.Text = string.Empty;
            unixCommands(outPut); //checks for specific responses by the bot to perform functions
        }

        /*
         * This method should handle the user-inputted commands to perform functions within the bot.
         * these events are based on responses given by the BOT, and it looks for specific sentences which are defined in the SIML package
         * 
         * Ex: user wants to exit application, types in "close application" and types "yes" to close application
         * 
         * ---------------------------------COMMAND LIST FOR DEVELOPERS (responses by the BOT)-----------------------------------------------
         * "Application closed." -> closes application window
         * "Cleared." -> clears all the text
         * "Astronics homepage." -> opens the default web browser to astronics homepage
         * 
         * --------------------------------------END----------------------------------------------------------------------------------------
         */
        private void unixCommands(string botOutput)
        {
            if(botOutput == "Application closed.") //closes application window
            {
                Application.Current.Shutdown();
            }
            else if (botOutput == "Cleared.") //clear all the text
            {
                OutputBox.Clear();
                /* WARNING -> BUG: if user is in a nested command with the BOT (ex: "close application" -> )
                 * the bot will not be reset only the text chat. Please fix!
                 */
            }
            else if(botOutput == "Astronics homepage.") //sends users to the astronics homepage
            {
                System.Diagnostics.Process.Start("https://www.astronics.com/");
            }
        }

        /*
         * Added event binding for the 'return' key which calls the "SendButton_OnClick" method
         */
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SendButton_OnClick(sender, e);
            }
        }

        private void OutputBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
