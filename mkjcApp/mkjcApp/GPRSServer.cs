using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
namespace mkjcApp
{
    public class GPRSServer
    {
        public GPRSServer(string ip, int port)
        {
            this.IP = ip;
            this.Port = port;
        }

        public string IP { get; private set; }
        public int Port { get; private set; }
        private Socket server;
        private bool start = true;
        private byte[] return_buff = new byte[] { 0x68, 0x33, 0x00, 0x33, 0x00, 0x68, 0x08, 0x56, 0x08, 0x03, 0x01, 0x00, 0x02, 0x70, 0x00, 0x00, 0x01, 0x00, 0xDD, 0x16 };
        public bool Start()
        {
            try
            {
                server = new Socket(SocketType.Stream, ProtocolType.Tcp);

                IPEndPoint point = new IPEndPoint(IPAddress.Parse(this.IP), this.Port);
                server.Bind(point);
                server.ReceiveTimeout = 3000;
                server.SendTimeout = 3000;
                server.Listen(100);
                Task tsk = Task.Run(() =>
                {
                    while (start)
                    {
                        try
                        {
                            Socket client = server.Accept();
                            client.ReceiveTimeout = 10 * 1000;//10s接收超时
                            Task.Run(() =>
                            {
                                byte[] buff = new byte[200];
                                client.Receive(buff, SocketFlags.None);
                                client.Send(return_buff);
                            });
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        private bool Stop()
        {
            try
            {
                if (server != null)
                {
                    server.Close();
                    server = null;
                }
            }
            catch (Exception ex)
            {

            }
            return true;
        }
    }
}
