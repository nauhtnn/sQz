using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace WpfApplication1
{
    class ServerInstance
    {
        static TcpListener server = null;

        public static void Start()
        {    
            try
            {
                // Set the TcpListener on port 23821.
                Int32 port = 23821;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

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
                    Console.WriteLine("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = mClient.GetStream();

                    int i;

                    string queStr = sQzCS.Utils.ReadFile("qz1.txt");

                    // Loop to receive all the data sent by the mClient.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);

                        // Process the data sent by the mClient.
                        data = data.ToUpper();

                        //byte[] msg = System.Text.Encoding.ASCII.GetBytes("hi, there");// data);
                        byte[] msg = System.Text.Encoding.UTF8.GetBytes(queStr.ToArray());

                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent: {0}", queStr.Substring((int)(queStr.Length*0.9f)));
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
    }
}
