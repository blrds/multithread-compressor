
using SshNet.Security.Cryptography;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

Console.Title = "Работник";

Console.Write("server ip:");
IPAddress ip = IPAddress.Parse(Console.ReadLine());
Console.Write("server port:");
int sport = Int32.Parse(Console.ReadLine());

TcpClient client = new TcpClient();
Console.Write("connection to server ");
client.Connect(ip, sport);
Console.WriteLine(client.Connected);
client.GetStream().Write(Encoding.UTF8.GetBytes("/reg"));
List<byte> data = new List<byte>();
while (true)
{
    byte[] buffer = new byte[4096];
    client.GetStream().Read(buffer, 0, buffer.Length);
    var str= Encoding.UTF8.GetString(buffer);
    if (str.StartsWith("/approve"))
    {
        var val = str.Split(" ");
        Console.Title = "Работник " + val[1];
    }

    if (str.StartsWith("/task"))
    {
        Console.WriteLine(str);
        while (true)
        {
            client.GetStream().Read(buffer, 0, buffer.Length);
            if (Encoding.UTF8.GetString(buffer).StartsWith("/endtask"))
            {
                Console.WriteLine("/endtask");
                break;
            }
            data.AddRange(buffer);
        }
        client.GetStream().Write(Encoding.UTF8.GetBytes("/approve"));
        continue;
    }
    if (str.StartsWith("/start"))
    {
        Console.WriteLine(str);
        Console.WriteLine(Convert.ToHexString(data.ToArray()));
        RIPEMD160 rIPEMD160 = new RIPEMD160();
        string hash = Convert.ToHexString(rIPEMD160.ComputeHash(data.ToArray()));
        data.Clear();
        //Console.WriteLine(hash);
        client.GetStream().Write(Encoding.UTF8.GetBytes("/end " + hash));
    }
}
