using Microsoft.VisualBasic;
using Riptide;
using Riptide.Transports.Tcp;
using Riptide.Utils;
using System.Net;

namespace RiptideTesting
{
    internal static class Program
    {
        const int port = 1300;
        static Client client = new Client();
        static Server server;
        static Thread updateThread;

        static void Main(string[] args)
        {
            RiptideLogger.Initialize(Console.WriteLine, false);

            //OnClient();
            updateThread = new Thread(new ThreadStart(loopster));
            updateThread.Start();

            while (true)
            {
                string[] input = (Console.ReadLine() ?? "").Split(';', StringSplitOptions.TrimEntries);
                Message msg;
                switch (input[0])
                {
                    case "serv":
                        //OnServer();
                        Console.Clear();
                        server = new Server();
                        server.Start(port, 64);
                        client.Connect($"{/*Dns.GetHostAddresses(Dns.GetHostName())[0].MapToIPv4()*/"127.0.0.1"}:{port}");

                        
                        Thread beeper3th = new Thread(new ThreadStart(beeper3));
                        static void beeper3(){
                            Console.Beep();
                            Thread.Sleep(150);
                            Console.Beep();
                            Thread.Sleep(150);
                            Console.Beep();
                        }
                        beeper3th.Start();
                        break;

                    case "con":
                        client.Connect($"{input[1]}:{port}");

                        Thread beeper2th = new Thread(new ThreadStart(beeper2));
                        static void beeper2()
                        {
                            Console.Beep();
                            Thread.Sleep(150);
                            Console.Beep();
                        }
                        beeper2th.Start();
                        break;

                    case "msg":
                    case "post":
                    case "send":
                        msg = Message.Create(MessageSendMode.Reliable, (ushort)PacketTypes.post);
                        msg.AddString(input[1]);
                        client.Send(msg);
                        break;
                }
            }
        }

        #region serverHandlers

        [MessageHandler((ushort)PacketTypes.post)]
        private static void ServerHandlePost(ushort id, Message message)
        {
            string str = message.GetString();
            Console.WriteLine($"server got {str}");

            Message msg = Message.Create(MessageSendMode.Reliable, (ushort)PacketTypes.post);
            msg.AddString(str);
            server.SendToAll(msg);
        }

        #endregion

        #region clientHandlers

        [MessageHandler((ushort)PacketTypes.post)]
        private static void HandlePost(Message message)
        {
            string str = message.GetString();

            Console.WriteLine($"client got {string.Concat(str)}");
        }

        #endregion



        enum PacketTypes
        {
            post
        }

        static DateTime threadStartTime = DateTime.Now;
        static int tickspersecond = 60;

        static void loopster()
        {
            while (true)
            {
                threadStartTime = DateTime.Now;

                if(server != null) server.Update();
                client.Update();

                Thread.Sleep(/*(1000 / tickspersecond) - (DateTime.Now.Millisecond - threadStartTime.Millisecond)*/ 60);
            }
        }
    }
}
