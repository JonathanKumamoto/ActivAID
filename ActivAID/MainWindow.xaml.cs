using System.IO;
using System.Windows;
using Syn.Bot.Siml;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using System;
using System.Windows.Controls.Primitives;
using System.Collections.Generic;

namespace ActivAID
{
    public partial class MainWindow
    {
        public SimlBot Chatbot; //the SimlBot variable
        public Label usermsg;
        public Label botmsg;
        public String ColorBOT, ColorUser, FontColor;
        public static MainWindow AppWindow;
        QueryHandler queryHandler;
        public Func<string, string> stringOp;
        public Func<string[], string[]> summarize;

        public MainWindow()
        {
            InitializeComponent();
            Chatbot = new SimlBot();
            Chatbot.PackageManager.LoadFromString(File.ReadAllText("Knowledge.simlpk"));
            ColorBOT = "#FF4A4B53";
            ColorUser = "#FF5383AD";
            FontColor = "#FFFFFF";
            AppWindow = this;
            MouseDown += delegate { DragMove(); };
            defineFunctionObjects();
//<<<<<<< HEAD
            UserInputBoiler sb = new UserInputBoiler();
//=======
            UserInputBoiler uib = new UserInputBoiler();
//>>>>>>> origin/mf_dev
            DataAccess dA = new DataAccessDB();
            queryHandler  = new QueryHandler(dA, uib, stringOp, summarize);
        }

        private void defineFunctionObjects()
        {
            stringOp = new Func<string, string>((x) =>
            {
                var temp = x;
                new HTMLMessager().removeFromLine(ref temp);
                return temp;
            });
            summarize = new Func<string[], string[]>((toSummarize) =>
            {
                List<string> sumList = new List<string>();
                OpenTextSummarizer.SummarizerArguments args = new OpenTextSummarizer.SummarizerArguments();
                args.InputString = String.Join(" ", toSummarize);
                OpenTextSummarizer.SummarizedDocument sd = OpenTextSummarizer.Summarizer.Summarize(args);
                return sd.Sentences.ToArray();
            });
        }
/*        public void setColorScheme(String ColorScheme)
        {
            OutputBox.Items.Clear();
            if (ColorScheme == "DLG")
            {
                grid1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF284167"));
                close.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF5383AD"));
                ColorBOT = "#FF4A4B53";
                ColorUser = "#FF5383AD";
                FontColor = "#FFFFFF";
            }
            else if(ColorScheme=="GBW"){
                grid1.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4A4B53"));
                close.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF000000"));
                close.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFF"));
                firstBOT.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF284167"));
                ColorBOT = "#FF284167";
                ColorUser = "#FF000000";
            }
        }*/

        /*
         * Handles the "send" button click
         */ 
        private void SendButton_OnClick(object sender, RoutedEventArgs e)
        {
            usermsg = new Label();
            botmsg = new Label();
            TextBlock txtBlockuser = new TextBlock();
            TextBlock txtBlockbot = new TextBlock();
            txtBlockuser.TextWrapping = TextWrapping.Wrap;
            txtBlockbot.TextWrapping = TextWrapping.Wrap;
            var result = Chatbot.Chat(InputBox.Text);
            string outPut = result.BotMessage;
            txtBlockuser.Text = " You: "+InputBox.Text;
            usermsg.Name = "usermsg";   //user input box
            usermsg.Target = OutputBox;
            usermsg.Content = txtBlockuser;
            usermsg.BorderThickness = new Thickness(1);
            usermsg.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorUser));
            usermsg.HorizontalAlignment = HorizontalAlignment.Center;
            usermsg.VerticalAlignment = VerticalAlignment.Top;
            usermsg.Width = 185;
            usermsg.Margin = new Thickness(0, 10, 0, 0);
            usermsg.RenderTransformOrigin = new Point(0.5,0.5);
            usermsg.RenderTransform = new SkewTransform(10,0);
            usermsg.FontFamily = new FontFamily("Candara");
            usermsg.FontSize = 12;
            usermsg.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(FontColor));

            OutputBox.Items.Add(usermsg);
            txtBlockbot.Text = "  BOT: "+result.BotMessage;
            botmsg.Name = "botmsg";   //bot's response box
            botmsg.Target = OutputBox;
            botmsg.Content = txtBlockbot;
            botmsg.BorderThickness = new Thickness(1);
            botmsg.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorBOT));
            botmsg.HorizontalAlignment = HorizontalAlignment.Center;
            botmsg.VerticalAlignment = VerticalAlignment.Top;
            botmsg.Width = 185; 
            botmsg.Margin = new Thickness(22, 0, 0, 0);
            botmsg.RenderTransformOrigin = new Point(0.5, 0.5);
            botmsg.RenderTransform = new SkewTransform(-10, 0);
            botmsg.FontFamily = new FontFamily("Candara");
            botmsg.FontSize = 12;
            botmsg.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(FontColor)); 
            OutputBox.Items.Add(botmsg);
            OutputBox.SelectedIndex = OutputBox.Items.Count - 1;
            OutputBox.SelectedIndex = -1;
            //InputBox.Text = string.Empty;
            if (isUnixCommand(outPut))
            {
                unixCommands(outPut);
            }
            //checks for specific responses by the bot to perform functions
            else
            {
                txtBlockbot.Text = " BOT: \n" + queryHandler.backendCommand(outPut);
            }
            InputBox.Text = string.Empty;
        }
        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
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

        private bool isUnixCommand(string botOutput)
        {
            if (botOutput == "Application closed." || botOutput == "Cleared." || botOutput == "Astronics homepage.")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void unixCommands(string botOutput)
        {
            if (botOutput == "Application closed.") //closes application window
            {
                Application.Current.Shutdown();
            }
            else if (botOutput == "Cleared.") //clear all the text
            {
                OutputBox.Items.Clear();
                /* WARNING -> BUG: if user is in a nested command with the BOT (ex: "close application" -> )
                 * the bot will not be reset only the text chat. Please fix!
                 */
            }
            else if (botOutput == "Astronics homepage.") //sends users to the astronics homepage
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
        public void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Text = string.Empty;
            tb.GotFocus -= TextBox_GotFocus; //Take off the "Search..." placeholder when user clicks on textbox
        }
        private void BringSelectionIntoView(object sender, SelectionChangedEventArgs e)
        { //Scroll down to the bottom after every query
            Selector selector = sender as Selector;
            if (selector is ListBox)
            {
                (selector as ListBox).ScrollIntoView(selector.SelectedItem);
            }
        }

        private void SettingsButton_OnClick(object sender, MouseButtonEventArgs e)
        {
            //To be developed: Dialog box pop up with options to chagne color scheme or font size in user/bot conversation
            
        }
    }

    internal class Graphics
    {
    }
}
