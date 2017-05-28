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
using System.Windows.Media.Imaging;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ActivAID
{
    public partial class MainWindow
    {
        const string DEFAULTFILE = @"C:\Program Files\ActivATE\ActivATE 5.x\Logs\ACLLog_";
        public string outPut;
        public SimlBot Chatbot; //the SimlBot variable
        public Label usermsg;
        public Label botmsg;
        public String ColorBOT, ColorUser, FontColor;
        public static MainWindow AppWindow;
        QueryHandler queryHandler;
        public Settings settwindow;
        public int fontSize;
        public String GoldBOT;
        public Boolean mainBOTmsg;
        public Boolean RobotResponding;
        public Func<string, string> stringOp;
        public Func<string[], string[]> summarize;
        SpeechRecognitionEngine sRecognize; //-----intialize speech recogniztion

        public MainWindow()
        {


            BackEnd.loadIronPython();


            InitializeComponent();
            InputBox.TextChanged += OnTextChangedHandler;
            //Chatbot = new SimlBot();
            //Chatbot.PackageManager.LoadFromString(File.ReadAllText("Knowledge.simlpk"));
            ColorBOT = "#232c3a"; // Color of Bot message rectangle
            ColorUser = "#40464e"; // Color of User message rectangle
            FontColor = "#FFFFFF"; // Font color for text in Chat
            AppWindow = this;
            MouseDown += delegate { DragMove(); };
            settwindow = new Settings();
            fontSize = 16;
            GoldBOT = "botmsg";
            mainBOTmsg = true;
            RobotResponding = false;
            defineFunctionObjects();
            UserInputBoiler uib = new UserInputBoiler();
            DataAccess dA = new DataAccessDB();
            queryHandler = new QueryHandler(dA, uib, stringOp, summarize);
            MainWindow_Creator();
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
            if (!RobotResponding) //give an option for lee way as the developer to give time for the bot to respond.
            {
                RobotResponding = true;

                if (!string.IsNullOrWhiteSpace(InputBox.Text)) //Check if the entry isn't empty
                {
                    SendButton_action();
                    InputBox.Clear();

                    if (!mic.IsEnabled)
                    {
                        sRecognize.RecognizeAsyncStop();
                    }
                    mic.IsEnabled = true;
                    mic.Visibility = System.Windows.Visibility.Visible;



                }
            }
        }

        private void voiceControl_OnClick(object sender, RoutedEventArgs e)
        {
            sRecognize = new SpeechRecognitionEngine();
            //InputBox.Clear();
            mic.Visibility = System.Windows.Visibility.Collapsed;
            mic.IsEnabled = false;
            Choices sList = new Choices();
            sList.Add(new string[] {"Reset my drivers", "take me to the astronics website","voice", "recognition", "hello", "astronics", "bot", "can", "website", "how", "are", "you", "the", "help"});
            Grammar gr = new Grammar(new GrammarBuilder(sList));
            try
            {
                sRecognize.RequestRecognizerUpdate();
                sRecognize.LoadGrammar(gr);
                sRecognize.SpeechRecognized += SRecognize_SpeechRecognized;
                sRecognize.SetInputToDefaultAudioDevice();
                sRecognize.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch
            {
                return;
            }
        }

        private void SRecognize_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            InputBox.Text += " " + e.Result.Text.ToString();
        }

        private void MainWindow_Creator()
        {
            Label mainmsg = new Label(); //Default main BOT message creator
            TextBlock txtBlockbot = new TextBlock();
            txtBlockbot.TextWrapping = TextWrapping.Wrap;
            txtBlockbot.Text = "Hello, what are you looking for?";
            mainmsg.Name = "mainmsg";
            mainmsg.Content = txtBlockbot;
            mainmsg.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorBOT));
            mainmsg.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(FontColor));
            mainmsg.Width = 230;
            mainmsg.FontSize = 16;
            mainmsg.FontFamily = new FontFamily("Candara");
            mainmsg.Margin = new Thickness(50, -40, 0, 0);
            UserBubble_Creator(true);
            OutputBox.Items.Add(mainmsg); // Add textblock to ListBox
        }
        private void UserBubble_Creator(bool bot)
        {
            Ellipse user = new Ellipse(); //Image bubble creator
            ImageBrush userimage = new ImageBrush();
            userimage.ImageSource = (bot ? new BitmapImage(new Uri(@"Media\iconBot.png", UriKind.Relative)) : new BitmapImage(new Uri(@"Media\iconCustomer.png", UriKind.Relative)));
            user.Height = 38;
            user.Width = 38;
            user.Margin = (bot ? new Thickness(0, 0, 0, 0) : new Thickness(430, 10, 0, 0));
            user.Fill = userimage;
            user.HorizontalAlignment = HorizontalAlignment.Right;
            OutputBox.Items.Add(user);// Add to ListBox
        }
        private void SendButton_action()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            clear.Visibility = System.Windows.Visibility.Collapsed;
            mic.Visibility = System.Windows.Visibility.Visible;
            //User Message creator, txtblock inside a Label
            usermsg = new Label();
            TextBlock txtBlockuser = new TextBlock();
            TextBlock txtBlockbot = new TextBlock();
            txtBlockuser.TextWrapping = TextWrapping.Wrap;
            txtBlockbot.TextWrapping = TextWrapping.Wrap;
            
            txtBlockuser.Text = InputBox.Text;
            usermsg.Name = "usermsg";   //user input box
            usermsg.Target = OutputBox;
            usermsg.Content = txtBlockuser;
            usermsg.BorderThickness = new Thickness(1);
            usermsg.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorUser));
            usermsg.HorizontalAlignment = HorizontalAlignment.Center;
            usermsg.VerticalAlignment = VerticalAlignment.Top;
            usermsg.MaxWidth = 150;
            usermsg.FontFamily = new FontFamily("Candara");
            usermsg.FontSize = 16;
            usermsg.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(FontColor));
            usermsg.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            int ActualWidth = 420 - ((int)usermsg.DesiredSize.Width); //Algorithm to calcualte margin for every user message
            usermsg.Margin = new Thickness(ActualWidth, -30, 25, 10);
            Mouse.OverrideCursor = null;
            UserBubble_Creator(false);
            OutputBox.Items.Add(usermsg); //Add to ListBox
            //-----------------------------------------------------------------------------------------
         
            OutputBox.Items.MoveCurrentToLast();
            OutputBox.SelectedItem = OutputBox.Items.CurrentItem;
            OutputBox.ScrollIntoView(OutputBox.Items.CurrentItem);

            Console.WriteLine(InputBox.Text);
            string WILLBEDELETED = InputBox.Text;
            SendButton_actionBOT(InputBox, WILLBEDELETED);
            
        }

        private void setOutPut(string command, ref TextBlock outputToUI, ref TextBox tb)
        {
            if (!unixCommands(command))//checks for specific responses by the bot to perform functions
            {
                try
                {
                    if("make a new test program" == command.ToLower() || "set up a new test program" == command.ToLower())
                    {
                        command = "do: new test program template";
                    }

                    foreach (var textBlock in BackEnd.backendCommand(command.ToLower()))
                    {
                        textBlock.TextWrapping = TextWrapping.Wrap;
                        Label botmsg = new Label();
                        UserBubble_Creator(true);
                        botmsg.Name = "botmsg";   //bot's response box
                        botmsg.Target = OutputBox;
                        botmsg.Content = textBlock;
                        botmsg.BorderThickness = new Thickness(1);
                        botmsg.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorBOT));
                        botmsg.HorizontalAlignment = HorizontalAlignment.Center;
                        botmsg.VerticalAlignment = VerticalAlignment.Top;
                        botmsg.MaxWidth = 220;
                        botmsg.Width = textBlock.Width;
                        botmsg.Width = 210;
                        botmsg.Margin = new Thickness(50, -40, 0, 0);
                        botmsg.FontFamily = new FontFamily("Candara");
                        botmsg.FontSize = fontSize;
                        botmsg.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(FontColor));
                        OutputBox.Items.Add(botmsg);
                    }
                }
                catch (NoFileMatchException)
                {
                    outputToUI.Text = "I'm hearing ya... I just don't getcha. Can you make your request more specific?";
                    Label botmsg = new Label();
                    botmsg.Name = "botmsg";   //bot's response box
                    botmsg.Target = OutputBox;
                    botmsg.Content = outputToUI;
                    botmsg.BorderThickness = new Thickness(1);
                    botmsg.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorBOT));
                    botmsg.HorizontalAlignment = HorizontalAlignment.Center;
                    botmsg.VerticalAlignment = VerticalAlignment.Top;
                    botmsg.MaxWidth = 220;
                    botmsg.Width = outputToUI.Width;
                    botmsg.Width = 210;
                    botmsg.Margin = new Thickness(50, -40, 0, 0);
                    botmsg.FontFamily = new FontFamily("Candara");
                    botmsg.FontSize = 16;
                    botmsg.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(FontColor));
                    UserBubble_Creator(true);
                    OutputBox.Items.Add(botmsg);
                }
            }
        tb.Text = string.Empty;
    }

    private async void SendButton_actionBOT(TextBox tb, string WILLBEDELETED)
    {
        await Task.Delay(500);
        TextBlock txtBlockbot = new TextBlock();
        txtBlockbot.TextWrapping = TextWrapping.Wrap;
        var result = "this needs to be uncommented";// getMaxRegexMatchesFile(InputBox.Text);//Chatbot.Chat(InputBox.Text);
        //----------------------------------------------------------------------------------------
        /*
         * ADD: Timer to have robot wait and no immediately reply to the user.
         * 
         */
                //int milliseconds = 1000;
                //Thread.Sleep(milliseconds);
                //----------------------------------------------------------------------------------------

                setOutPut(WILLBEDELETED,ref txtBlockbot ,ref tb);

            //txtBlockbot.Text = result;//result.BotMessage;
            Label botmsg = new Label();
            botmsg.Name = "botmsg";   //bot's response box
            botmsg.Target = OutputBox;
            botmsg.Content = txtBlockbot;
            botmsg.BorderThickness = new Thickness(1);
            botmsg.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColorBOT));
            botmsg.HorizontalAlignment = HorizontalAlignment.Center;
            botmsg.VerticalAlignment = VerticalAlignment.Top;
            botmsg.MaxWidth = 220;
            botmsg.Width = txtBlockbot.Width;
            botmsg.Width = 210;
            botmsg.Margin = new Thickness(50, -40, 0, 0);
            botmsg.FontFamily = new FontFamily("Candara");
            botmsg.FontSize = 16;
            botmsg.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom(FontColor));
           // UserBubble_Creator(true);
           // OutputBox.Items.Add(botmsg); // Adding to Listbox
            OutputBox.Items.MoveCurrentToLast();
            OutputBox.SelectedItem = OutputBox.Items.CurrentItem;
            OutputBox.ScrollIntoView(OutputBox.Items.CurrentItem);
            RobotResponding = false;
        }

        /*
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
        */

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
           // if (botOutput == "Application closed." || botOutput == "Cleared." || botOutput == "Astronics homepage.")
            if(botOutput.ToLower() == "clear")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool unixCommands(string botOutput)
        {
            if (botOutput == "Application closed.") //closes application window
            {
                Application.Current.Shutdown();
                return true;
            }
            else if (botOutput.ToLower() == "clear") //clear all the text
            {
                OutputBox.Items.Clear();
                MainWindow_Creator();
                /* WARNING -> BUG: if user is in a nested command with the BOT (ex: "close application" -> )
                 * the bot will not be reset only the text chat. Please fix!
                 */
                return true;
            }
            else if (botOutput.ToLower() == "astronics homepage") //sends users to the astronics homepage
            {
                System.Diagnostics.Process.Start("http://astronicstestsystems.com/");
                return true;
            }
            else
            {
                return false;
            }
        }


        /*
         * Added event binding for the 'return' key which calls the "SendButton_OnClick" method
         */
        private void OnKeyUpHandler(object sender, KeyEventArgs e)
        {
            //Console.WriteLine("Here" + InputBox.Text);
            if (e.Key == Key.Return && !string.IsNullOrWhiteSpace(InputBox.Text))
            {
                SendButton_OnClick(sender, e);
                
            }
        }

        private void OnTextChangedHandler(object sender, TextChangedEventArgs e)
        {
            if (InputBox.Text.Length > 0)
            {
                mic.Visibility = System.Windows.Visibility.Collapsed;
                clear.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                clear.Visibility = System.Windows.Visibility.Collapsed;
                mic.Visibility = System.Windows.Visibility.Visible;
            }
        }
        private void ClearTextBox_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            InputBox.Clear();
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
            //Dialog box pop up with options to chagne color scheme or font size in user/bot conversation
            settwindow.Show();
            settwindow.Topmost = true;
        }
    }

    internal class Graphics
    {
    }
}
