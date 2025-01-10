using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ServerLogic
    {
        public static async Task Main()
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            string hostName = Dns.GetHostName();
            Console.WriteLine($"Мій хост: {hostName}");

            IPHostEntry localHost = await Dns.GetHostEntryAsync(hostName);
            Console.WriteLine("Оберіть IP адресу, на якій буде працювати сервер:");

            for (int i = 0; i < localHost.AddressList.Length; i++)
                Console.WriteLine($"{i + 1}. {localHost.AddressList[i]}");

            int numberIP = 0;
            while (true)
            {
                Console.Write("->_ ");
                if (Int32.TryParse(Console.ReadLine(), out numberIP) && numberIP > 0 && numberIP <= localHost.AddressList.Length)
                    break;
                Console.WriteLine("Некоректний вибір. Спробуйте ще раз.");
            }

            int serverPort = 0;
            while (true)
            {
                Console.Write("Введіть порт для сервера (від 1 до 65535): ");
                if (Int32.TryParse(Console.ReadLine(), out serverPort) && serverPort > 0 && serverPort <= IPEndPoint.MaxPort)
                    break;
                Console.WriteLine("Некоректний порт. Спробуйте ще раз.");
            }

            IPAddress serverIp = localHost.AddressList[numberIP - 1];
            Console.Title = $"{serverIp}:{serverPort}";

            IPEndPoint iPEndPoint = new IPEndPoint(serverIp, serverPort);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                server.Bind(iPEndPoint);
                server.Listen(10);

                Console.WriteLine($"Сервер запущено на {serverIp}:{serverPort} та очікує запити...\n");

                while (true)
                {
                    Socket client = await server.AcceptAsync();
                    Task clientTask = HandleClientAsync(client);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Проблема: {e.Message} \n");
            }
            finally
            {
                server.Close();
            }
        }

        private static async Task HandleClientAsync(Socket client)
        {
            Console.WriteLine($"До нас стукає: {client.RemoteEndPoint}\n");

            try
            {
                byte[] buffer = new byte[1024];

                while (true)
                {
                    int bytesRead = await client.ReceiveAsync(buffer, SocketFlags.None);
                    if (bytesRead == 0) 
                        break;

                    string receivedMessage = Encoding.Unicode.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Повідомлення від клієнта {client.RemoteEndPoint}: '{receivedMessage}'. Час сервера: {DateTime.Now}\n");

                    string responseMessage = $"Дякую за ваше повідомлення: '{receivedMessage}'. Час сервера: {DateTime.Now}";
                    byte[] responseBuffer = Encoding.Unicode.GetBytes(responseMessage);

                    await client.SendAsync(responseBuffer, SocketFlags.None);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при обробці клієнта: {ex.Message}\n");
            }
            finally
            {
                Console.WriteLine($"Клієнт {client.RemoteEndPoint} відключився.\n");
                client.Close();
            }
        }
    }
}
