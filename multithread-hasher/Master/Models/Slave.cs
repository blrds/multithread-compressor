using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Master.Models
{
    internal class Slave
    {
        public Slave(int id, TcpClient client)
        {
            Id = id;
            Client = client;
        }

        public int Id { get; private set; }

        public TcpClient Client { get; private set; }

        public int Port { get => ((IPEndPoint)Client.Client.RemoteEndPoint).Port; }
        public IPAddress IP { get => ((IPEndPoint)Client.Client.RemoteEndPoint).Address; }

    }
}
