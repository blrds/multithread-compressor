using Master.ViewModels.Base;
using Master.Infrastructure.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Threading;
using Master.Infrastructure;
using System.Net.Sockets;
using System.Net;

namespace Master.ViewModels
{
    internal class MainWindowViewModel:ViewModel
    {
        Thread Server;
        List<Slave> slaves = new List<Slave>();

        #region Variables


        private int port = 12345;
        public int Port { get { return port; } set { Set<int>(ref port, value); } }


        TcpListener tcpListener;
        #endregion

        public MainWindowViewModel()
        {
            Click = new LambdaCommand(OnClickExecuted, CanClickExecute);
            ServerStart = new LambdaCommand(OnServerStartExecuted, CanServerStartExecute);
        }

        #region Click
        public ICommand Click { get; }
        private bool CanClickExecute(object p) => true;
        private void OnClickExecuted(object p)
        {
            //Server.Abort();
        }
        #endregion

        #region ServerStart
        public ICommand ServerStart { get; }
        private bool CanServerStartExecute(object p) => true;
        private void OnServerStartExecuted(object p)
        {

            tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), Port);
            tcpListener.Start();
            Server = new Thread(SlaveListener);
            Server.Start();
        }
        #endregion

        #region Externals
        void SlaveListener()
        {
            while (true)
            {
                TcpClient c = new TcpClient();
                c = tcpListener.AcceptTcpClient();
                var buf=new byte[4096];
                c.GetStream().Read(buf);
                string str=Encoding.UTF8.GetString(buf);
                var lexems = str.Split(" ");
                switch (lexems[0])
                {
                    case "/reg":
                        {
                            slaves.Add(new Slave(slaves.Count, c));
                            var s = Encoding.UTF8.GetBytes($"\\approve {slaves.Count}");
                            c.GetStream().Write(s, 0, s.Length);
                            break;
                        }
                }
            }
        }
        #endregion
    }
}
