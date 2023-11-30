
using System.Net;
using System.Net.Sockets;
using System.Text;

Console.Write("server ip:");
IPAddress ip = IPAddress.Parse(Console.ReadLine());
Console.WriteLine(ip.ToString());
Console.Write("server port:");
int sport = Int32.Parse(Console.ReadLine());
Console.WriteLine(sport.ToString());
TcpClient client = new TcpClient();
Console.WriteLine("connection to server");
client.Connect(ip, sport);
Console.WriteLine(client.Connected);
client.GetStream().Write(Encoding.UTF8.GetBytes("/reg "));
while (true)
{
    byte[] buffer = new byte[1024];
    client.GetStream().Read(buffer, 0, buffer.Length);
    Console.WriteLine(UTF8Encoding.UTF8.GetString(buffer));
}
