
using System.Net;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("Ы\n");
Console.Write("server ip:");
IPAddress ip = IPAddress.Parse(Console.ReadLine());
Console.Write("server port:");
int sport = Int32.Parse(Console.ReadLine());
TcpClient client = new TcpClient();
client.Connect(ip, sport);
client.GetStream().Write(Encoding.UTF8.GetBytes("\reg"));
client.GetStream
