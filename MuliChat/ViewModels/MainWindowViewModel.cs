using MuliChat.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MuliChat.ViewModels
{
    public class MainWindowViewModel:BaseViewModel
    {
        public RelayCommand StartBtnCommand { get; set; }
        public RelayCommand ConnectBtnCommand { get; set; }
        public RelayCommand SendBtnCommand { get; set; }
        TcpClient client;
        public StreamReader STR;
        public StreamWriter STW;
        public string Receive;
        public string TextToSend;
        public MainWindow MainWindow { get; set; }


        private readonly BackgroundWorker backgroundWorker1 = new BackgroundWorker();
        private readonly BackgroundWorker backgroundWorker2 = new BackgroundWorker();
        public MainWindowViewModel(MainWindow  mainWindow)
        {
            MainWindow = mainWindow;
            backgroundWorker1.DoWork += BackgroundWorker1_DoWork;
            backgroundWorker2.DoWork += BackgroundWorker2_DoWork;
           mainWindow.ServerIpTextbox.Text = "192.168.1.103";

            StartBtnCommand = new RelayCommand((sender) => {
                App.Current.Dispatcher.Invoke(() =>
                {


                    TcpListener listener = new TcpListener(IPAddress.Parse(mainWindow.ServerIpTextbox.Text), int.Parse(mainWindow.ServerPortTextBox.Text));
                    listener.Start();

                    Task.Run(() =>
                    {

                        client = listener.AcceptTcpClient();
                        STR = new StreamReader(client.GetStream());
                        STW = new StreamWriter(client.GetStream());
                        STW.AutoFlush = true;
                        backgroundWorker1.RunWorkerAsync();
                        backgroundWorker2.WorkerSupportsCancellation = true;

                    });



                });
            });
            ConnectBtnCommand = new RelayCommand((sender) => {
                client = new TcpClient();
                IPEndPoint ipEnd = new IPEndPoint(IPAddress.Parse(mainWindow.ClientIpTextbox.Text), int.Parse(mainWindow.ClientPortTextbox.Text));
                client.Connect(ipEnd);

                //try
                //{

               mainWindow.ChatScreenTextbox.Text += "Connect To Server\n";
                STW = new StreamWriter(client.GetStream());
                STR = new StreamReader(client.GetStream());
                STW.AutoFlush = true;
                backgroundWorker1.RunWorkerAsync();
                backgroundWorker2.WorkerSupportsCancellation = true;
                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show(ex.Message);
                //}

            });
            SendBtnCommand = new RelayCommand((sender) => {
                if (mainWindow.MessageTextBox.Text != "")
                {
                    TextToSend =mainWindow.MessageTextBox.Text;
                    backgroundWorker2.RunWorkerAsync();
                }
               mainWindow.MessageTextBox.Text = "";

            });
        }
        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (client.Connected)
            {
                try
                {
                    Receive = STR.ReadLine();
                    App.Current.Dispatcher.Invoke(() =>
                    {
                       MainWindow.ChatScreenTextbox.Text += "You : " + Receive + "\n";
                        Receive = "";
                    });
                }
                catch (Exception)
                {

                }
            }
        }
        private void BackgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            if (client.Connected)
            {
                STW.WriteLine(TextToSend);
                App.Current.Dispatcher.Invoke(() =>
                {
                  MainWindow.ChatScreenTextbox.Text += "Me : " + TextToSend + "\n";

                });
            }
            else
            {
                MessageBox.Show("Sending Failed");
            }
            backgroundWorker2.CancelAsync();
        }
    }
}
