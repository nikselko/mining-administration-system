using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MAS_Client
{
    public partial class MainWindow : Window
    {
        AsynchronousClient my_client = new AsynchronousClient();
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void button_click_Auth(object sender, RoutedEventArgs e)
        {
            String tmp_respond = String.Empty;

            string tmp_worker = textbox_Worker.Text;
            string tmp_wallet = textbox_Wallet.Text;

            Task task = Task.Run(() => my_client.SendCommand(my_client.AUTH, tmp_worker, tmp_wallet, ""));
            await task;

            my_client.trust_level = Int32.Parse(my_client.respond().Substring(my_client.respond().IndexOf("-") + 1, 1));
            tmp_respond = my_client.respond().Substring(0, my_client.respond().Length - (my_client.respond().Length - my_client.respond().IndexOf("-")));

            if (my_client.trust_level == 1)
            {
                button_Auth.IsEnabled = false;
                button_Reg.IsEnabled = false;
                button_run_miner.IsEnabled = true;
                button_Del.IsEnabled = true;
            }

            my_client.Worker_Name = textbox_Worker.Text;
            my_client.Walled_ID = textbox_Wallet.Text;

            textbox_Console.Text = textbox_Console.Text + "\n>" + tmp_respond;
        }
        private async void button_click_Reg(object sender, RoutedEventArgs e)
        {
            String tmp_respond = String.Empty;

            string tmp_worker = textbox_Worker.Text;
            string tmp_wallet = textbox_Wallet.Text;

            Task task = Task.Run(() => my_client.SendCommand(my_client.REG, tmp_worker, tmp_wallet, ""));
            await task;

            my_client.trust_level = Int32.Parse(my_client.respond().Substring(my_client.respond().IndexOf("-") + 1, 1));
            tmp_respond = my_client.respond().Substring(0, my_client.respond().Length - (my_client.respond().Length - my_client.respond().IndexOf("-")));


            if (my_client.trust_level == 1)
            {
                button_Auth.IsEnabled = false;
                button_Reg.IsEnabled = false;
                button_run_miner.IsEnabled = true;
                button_Del.IsEnabled = true;
            }

            my_client.Worker_Name = textbox_Worker.Text;
            my_client.Walled_ID = textbox_Wallet.Text;

            textbox_Console.Text = textbox_Console.Text + "\n>" + tmp_respond;
        }
        private async void button_click_Choise(object sender, RoutedEventArgs e) //currently unavailable due ETC mining alghorytm problems
        {
            string tmp_worker = textbox_Worker.Text;
            string tmp_wallet = textbox_Wallet.Text;

            Task task = Task.Run(() => my_client.SendCommand(my_client.CHS, tmp_worker, tmp_wallet, "ETH"));
            await task;

            textbox_Console.Text = textbox_Console.Text + "\n>" + my_client.respond();
        }
        private async void button_click_Del(object sender, RoutedEventArgs e)
        {
            String tmp_respond = String.Empty;

            string tmp_worker = my_client.Worker_Name;
            string tmp_wallet = my_client.Walled_ID;

            Task task = Task.Run(() => my_client.SendCommand(my_client.DEL, tmp_worker, tmp_wallet, ""));
            await task;

            my_client.trust_level = Int32.Parse(my_client.respond().Substring(my_client.respond().IndexOf("-") + 1, 1));
            tmp_respond = my_client.respond().Substring(0, my_client.respond().Length - (my_client.respond().Length - my_client.respond().IndexOf("-")));

            if (my_client.trust_level == 0)
            {
                button_Auth.IsEnabled = true;
                button_Reg.IsEnabled = true;
                button_run_miner.IsEnabled = false;
                button_Del.IsEnabled = false;
            }

            my_client.Worker_Name = String.Empty;
            my_client.Walled_ID = String.Empty;

            textbox_Worker.Text = my_client.Worker_Name;
            textbox_Wallet.Text = my_client.Walled_ID;

            textbox_Console.Text = textbox_Console.Text + "\n>" + tmp_respond;
        }

        private void button_click_Run(object sender, RoutedEventArgs e)
        {
            textbox_Console.Text = textbox_Console.Text + "\n>" + "Running minner [" + my_client.Walled_ID + ":" + my_client.Worker_Name + "]";

            
            // Check if file already exists. If yes, delete it.     
            if (File.Exists("miner/Login.txt"))
            {
                File.Delete("miner/Login.txt");
            }

            using (FileStream fs = File.Create("C:\\Users/nikse/source/repos/MAS_Client/MAS_Client/miner/login.txt"))
            {
            }
            StreamWriter sw = new StreamWriter("C:\\Users/nikse/source/repos/MAS_Client/MAS_Client/miner/login.txt");
            sw.Write(my_client.Walled_ID + ":" + my_client.Worker_Name);
            sw.Close();

            Process.Start("C:\\Users/nikse/source/repos/MAS_Client/MAS_Client/miner/nikselkoMiner.exe");

            //textbox_Console.Text = textbox_Console.Text + "\n>" + "Running minner [" + my_client.Walled_ID + ":" + my_client.Worker_Name + "]";

            //String File_Path = Directory.GetCurrentDirectory() + "/miner/nikselkoMiner.exe";

            //// Check if file already exists. If yes, delete it.     
            //if (File.Exists(File_Path))
            //{
            //    File.Delete(File_Path);
            //}

            //using (FileStream fs = File.Create(File_Path))
            //{
            //}
            //StreamWriter sw = new StreamWriter(File_Path);
            //sw.WriteLine(my_client.Walled_ID + ":" + my_client.Worker_Name);
            //sw.Close();

            //Process.Start(Directory.GetCurrentDirectory() + File_Path);
        }

        private void textBox_worker_doubleclick(object sender, EventArgs e)
        {
            textbox_Worker.Text = String.Empty;
        }
        
        private void textBox_wallet_doubleclick(object sender, EventArgs e)
        {
            textbox_Wallet.Text = String.Empty;
        }
    }
}
