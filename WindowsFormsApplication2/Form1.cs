using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ActivAID
{

    public partial class Form1 : Form
    {
        int strikes = 0,        //easter egg purpose, lets the computer make smart remarks depending on the circumstance.
            strikesNaughty = 0; //easter egg purpose, lets the computer make smart remarks depending on the circumstance.
        string[] NaughtyWords = { "ass", "damn"}; //keyword list (move it elsewhere into a database)

        public Form1()
        {
            InitializeComponent();
        }

        /******************************************************
          *Button "Clear Search"
          * Last Edit By: Jonathan K.
          *-----------------------------------------------------
          * Meant to clear the chat, may need to do more eventually
          * 
          ******************************************************/
        private void ClearList(object sender, EventArgs e)
        {
            ChatBox.Items.Clear();
            strikes = 0;
            strikesNaughty = 0;
        }

        private void ActivAID_Click(object sender, EventArgs e)
        {
           //Blank
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Blank
        }
        
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Blank
        }

        /******************************************************
          *Button "GO"
          * Last Edit By: Jonathan K.
          *-----------------------------------------------------
          * I'm letting the button function do all the work.
          * This is bad and needs to be moved elsewhere soon...
          * 
          ******************************************************/
        private void button2_Click(object sender, EventArgs e)
        {
            ChatBox.Items.Add("Me: " + textBox1.Text);
            string userLine = textBox1.Text;
   
            if (string.IsNullOrWhiteSpace(userLine))
            {
                switch (strikes)
                {
                    case 2:
                        ChatBox.Items.Add("BOT: Third times the charm eh?");
                        break;
                    case 3:
                        ChatBox.Items.Add("BOT: Are you even trying?");
                        break;
                    default:
                        ChatBox.Items.Add("BOT: I can't work with nothing, please enter something.");
                        break;
                }
                strikes += 1;
            }
            else
            {
                //***********************************************************************IF/ELSE Response chain.
                if (NaughtyWords.Contains(textBox1.Text))
                {
                    strikesNaughty += 1;
                    switch (strikesNaughty)
                    {
                        case 2:
                            ChatBox.Items.Add("BOT: I'm warning you. STOP!");
                            break;
                        case 3:
                            ChatBox.Items.Add("BOT: Your language will force my hand.");
                            break;
                        case 4:
                            Application.Exit(); //kill itself
                            break;
                        default:
                            ChatBox.Items.Add("BOT: Whoa there... I'm gonna have to stop you there!"); ;
                            break;
                    }
                }
                else if (textBox1.Text == "Order 66")
                {
                    ChatBox.Items.Add("BOT: It will be done my Lord.");
                }
                else if (textBox1.Text == "quit" || textBox1.Text =="q")
                {
                    Application.Exit(); //kill itself
                }
                else if (textBox1.Text == "google" || textBox1.Text == "Google")
                {
                    System.Diagnostics.Process.Start("http://google.com");
                }
                else
                {
                    ChatBox.Items.Add("BOT: I will look into it...");
                }
                //END********************************************************************END OF: IF/ELSE Response chain.

            }

            //Clear the input box once we're done with it
            textBox1.Clear(); 
        }


        //Makes it so user can press "Enter" key without having to hit "GO" if they don't want too
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button2_Click(this, new EventArgs());
            }
        }

        //automatic summarization goes here
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //blank
        }

    }
}
