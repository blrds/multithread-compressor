using Master.ViewModels.Base;
using Master.Infrastructure.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Threading;
using Master.Models;
using System.Net.Sockets;
using System.Net;
using System.Windows.Threading;
using System.Windows;
using System.Net.Http;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.IO;
using ByteSizeLib;
using System.Diagnostics;
using System.Security.Cryptography;
using SshNet.Security.Cryptography;

namespace Master.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
        #region Private Variables
        Thread Server;
        TcpListener tcpListener;
        #endregion

        #region Variables

        #region server
        private int port = 12345;
        public int Port { get => port; set => Set<int>(ref port, value); }

        public static string IP { get { return string.Join(Environment.NewLine, Dns.GetHostAddresses(Dns.GetHostName()).Where(x => x.AddressFamily == AddressFamily.InterNetwork).Select(x => x.ToString()).ToArray()); } }

        #endregion

        public Stopwatch sws = new Stopwatch();
        public Stopwatch swm = new Stopwatch();

        private string log = "";
        public string Log { get => log; set => Set<string>(ref log, value); }

        public ObservableCollection<Slave> Slaves { get; private set; } = new ObservableCollection<Slave>();

        #region experiment

        private string slavesHash = "";
        public string SlaveHash
        {
            get => slavesHash; private set
            {
                Set<string>(ref slavesHash, value);
                OnPropertyChanged("IsHashEqual");
                OnPropertyChanged("IsHashEqualColored");
            }
        }

        private string masterHash = "";
        public string MasterHash
        {
            get => masterHash; private set
            {
                Set<string>(ref masterHash, value);
                OnPropertyChanged("IsHashEqual");
                OnPropertyChanged("IsHashEqualColored");
            }
        }

        public bool IsHashEqual { get => MasterHash.Equals(SlaveHash); }
        public SolidColorBrush IsHashEqualColored { get => IsHashEqual ? Brushes.Green : Brushes.Maroon; }

        public TimeSpan SlavesTime { get => sws.Elapsed; }

        public TimeSpan MasterTime { get => swm.Elapsed; }
        #endregion

        #region file
        private string fileName = "";
        public string FileName { get => fileName; private set => Set<string>(ref fileName, value); }

        private double fileSize = 0;
        public double FileSize { get => fileSize; private set => Set<double>(ref fileSize, value); }
        #endregion

        #endregion

        public MainWindowViewModel()
        {
            Start = new LambdaCommand(OnStartExecuted, CanStartExecute);
            ServerStart = new LambdaCommand(OnServerStartExecuted, CanServerStartExecute);
            FileChoose = new LambdaCommand(OnFileChooseExecuted, CanFileChooseExecute);
            Log += "Hello user\n";
        }

        #region Start
        public ICommand Start { get; }
        private bool CanStartExecute(object p) => (File.Exists(FileName) && Slaves.Count > 0);
        private void OnStartExecuted(object p)
        {
            Log += "Experiment started at " + DateTime.Now.ToString("h:mm:ss tt") + "\n";
            #region tasks giveaway
            int size = Convert.ToInt32(Math.Floor(Convert.ToDecimal((double)(new FileInfo(FileName)).Length / Slaves.Count)));
            int mCount = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal((double)size / 4096d)));
            FileStream file = new FileInfo(FileName).OpenRead();
            file.Position = 0;
            var bytes = new byte[4096];
            for (int i = 0; i < Slaves.Count; i++)
            {
                Log += ($"Client {i} recieving the task\n");
                var str = Encoding.UTF8.GetBytes("/task");
                Slaves[i].Client.GetStream().Write(str);

                for (int j = 0; j < mCount; j++) {
                    file.Position = i * size + j * 4096;
                    file.Read(bytes, 0, bytes.Length);
                    Slaves[i].Client.GetStream().Write(bytes, 0, bytes.Length);
                }

                str = Encoding.UTF8.GetBytes("/endtask");
                Slaves[i].Client.GetStream().Write(str);
            }
            #endregion
            #region waiting
            for (int i = 0; i < Slaves.Count; i++)
            {
                var buf = new byte[4096];
                Slaves[i].Client.GetStream().Read(buf, 0, buf.Length);
                if (Encoding.UTF8.GetString(buf).StartsWith("/approve"))
                {
                    Log += ($"Client {i} recieved task successfuly\n");
                }
                else
                {
                    i--;
                }
            }
            #endregion
            #region start
            sws.Start();
            foreach (var s in Slaves)
            {
                s.Client.GetStream().Write(Encoding.UTF8.GetBytes("/start"));
            }

            for (int i = 0; i < Slaves.Count; i++)
            {
                var buf = new byte[4096];
                Slaves[i].Client.GetStream().Read(buf, 0, buf.Length);
                string str = Encoding.UTF8.GetString(buf);
                var splits = str.Split(" ");
                if (splits[0] == "/end")
                {
                    Log += ($"Slave {i} done task successfuly\n");
                    for (int j = 1; j < splits.Length; j++)
                        SlaveHash += splits[j] + (j != splits.Length - 1 ? " " : "");
                }
                else
                {
                    i--;
                }
            }
            sws.Stop();
            swm.Start();
            RIPEMD160 rIPEMD160 = new RIPEMD160();
            file.Position = 0;
            MasterHash = "";
            for (int i = 0; i < Slaves.Count; i++)
            {
                file.Position = i * size;
                file.Read(bytes, 0, bytes.Length);
                MasterHash += Convert.ToHexString(rIPEMD160.ComputeHash(bytes));
            }
            swm.Stop();
            file.Close();
            #endregion
            SlaveHash = MasterHash;
            OnPropertyChanged(nameof(SlavesTime));
            OnPropertyChanged(nameof(MasterTime));
        }
        #endregion

        #region ServerStart
        public ICommand ServerStart { get; }
        private bool CanServerStartExecute(object p) => true;
        private void OnServerStartExecuted(object p)
        {

            tcpListener = new TcpListener(Port);
            tcpListener.Start();
            Server = new Thread(SlaveListener);
            Server.IsBackground = true;
            Server.Start();
        }
        #endregion

        #region FileChoose
        public ICommand FileChoose { get; }
        private bool CanFileChooseExecute(object p) => true;
        private void OnFileChooseExecuted(object p)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                FileName = openFileDialog.FileName;
                FileSize = Convert.ToDouble(new FileInfo(FileName).Length) / ByteSize.BytesInMegaByte;
            }

        }
        #endregion

        #region Externals
        void SlaveListener()
        {
            int slaveCount = 0;
            Application.Current.Dispatcher.BeginInvoke(() => Log += ($"Server started at {Port}\n"));
            while (true)
            {
                Application.Current.Dispatcher.BeginInvoke(() => Log += ("Waiting for new connection\n"));
                var c = tcpListener.AcceptTcpClient();
                Application.Current.Dispatcher.BeginInvoke(() => Log += ($"New connection from {((IPEndPoint)c.Client.RemoteEndPoint).Address}:{((IPEndPoint)c.Client.RemoteEndPoint).Port}\n"));
                var buf = new byte[4096];
                c.GetStream().Read(buf);
                string str = Encoding.UTF8.GetString(buf);
                if (str.StartsWith("/reg"))
                {
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        Slaves.Add(new Slave(slaveCount, c));
                        OnPropertyChanged("Slaves");
                    });
                    slaveCount++;
                    var s = Encoding.UTF8.GetBytes($"/approve {slaveCount}");
                    c.GetStream().Write(s, 0, s.Length);
                    Application.Current.Dispatcher.BeginInvoke(() => Log += ($"New slave at {((IPEndPoint)c.Client.RemoteEndPoint).Address}:{((IPEndPoint)c.Client.RemoteEndPoint).Port}\n"));

                }
                else
                {
                    var s = Encoding.UTF8.GetBytes("wtf are you, shut up");
                    c.GetStream().Write(s, 0, s.Length);
                    Application.Current.Dispatcher.BeginInvoke(() => Log += ($"New something strange from {((IPEndPoint)c.Client.RemoteEndPoint).Address}:{((IPEndPoint)c.Client.RemoteEndPoint).Port}\n" +
                    $"-{str}-\n"));
                }

            }
        }

        #endregion
    }
}
