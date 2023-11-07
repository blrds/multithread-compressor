using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Master.Infrastructure
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

    }
}
