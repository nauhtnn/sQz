using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace WpfApplication1
{
    class Server1
    {
        TcpListener server = null;

        public void Start()
        {
            try
            {
                // Set the TcpListener on port 23821.
                Int32 port = 23821;
                //IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(IPAddress.Any, port);

                // Start listening for mClient requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient mClient = server.AcceptTcpClient();

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = mClient.GetStream();

                    int i;

                    // Loop to receive all the data sent by the mClient.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        Console.WriteLine("Received: {0}", data);
                        string recvMsg = System.Text.Encoding.UTF8.GetString(bytes);
                        string msg = null;
                        byte[] byteMsg = null;
                        char code = recvMsg[0];
                        switch (code)
                        {
                            case (char)RequestCode.Dating:
                                msg = "2017/03/01";
                                break;
                            case (char)RequestCode.Authenticating:
                                int splitIdx = recvMsg.IndexOf('\n');
                                string id = recvMsg.Substring(1, splitIdx - 1);
                                string birthdate = recvMsg.Substring(splitIdx + 1);
                                if (Authenticate(id, birthdate))
                                    msg = "pass";
                                else
                                    msg = "again";
                                break;
                            case (char)RequestCode.ExamRetrieving:
                                msg = sQzCS.Utils.ReadFile("qz1.txt");
                                break;
                            case (char)RequestCode.MarkSubmitting:
                                break;
                            default:
                                msg = "unknown";
                                break;
                        }
                        int sz = Encoding.UTF8.GetByteCount(msg);
                        byteMsg = new byte[sz];
                        byteMsg = Encoding.UTF8.GetBytes(msg);

                        // Send back a response.
                        stream.Write(byteMsg, 0, byteMsg.Length);
                        Console.WriteLine("Sent back: {0}", code);
                    }

                    // Shutdown and end connection
                    mClient.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }


            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }

        public static void Stop()
        {
            if (server != null)
                server.Stop();
        }

        private bool Authenticate(string id, string birthdate)
        {
            if (id.Equals("A10") && birthdate.Equals("01/01/1990"))
                return true;
            return false;
        }
    }
}
