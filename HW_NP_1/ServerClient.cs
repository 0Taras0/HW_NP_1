using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class ServerClient
    {
        private Socket _server;
        public bool IsConnected => _server.Connected;

        public async Task ConnectAsync(string ipAddress, int port)
        {
            if (_server != null)
                Close();

            IPAddress ip = IPAddress.Parse(ipAddress);
            IPEndPoint serverEndPoint = new IPEndPoint(ip, port);

            _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await _server.ConnectAsync(serverEndPoint);
        }

        public async Task<string> SendAndReceiveAsync(string message)
        {
            if (!IsConnected)
                throw new Exception("Немає з'єднання з сервером.");

            byte[] buffer = Encoding.Unicode.GetBytes(message);
            await _server.SendAsync(buffer, SocketFlags.None);

            buffer = new byte[1024];
            string response = String.Empty;
            int bytesRead;

            do
            {
                bytesRead = await _server.ReceiveAsync(buffer, SocketFlags.None);
                response += Encoding.Unicode.GetString(buffer, 0, bytesRead);
            } while (_server.Available > 0);

            return response;
        }

        public void Close()
        {
            if (IsConnected)
            {
                _server.Shutdown(SocketShutdown.Both);
                _server.Close();
            }
        }
    }
}
