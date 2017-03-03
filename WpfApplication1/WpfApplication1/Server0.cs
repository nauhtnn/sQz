using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace WpfApplication1
{
    enum RequestCode
    {
        None = '@',
        DateStudentRetriving = 'A',
        DateStudentRetrieved,
        QuestAnsKeyRetrieving,
        QuestAnsKeyRetrieved,
        MarkSubmitting,
        MarkSubmitted,
        Dating = 'a',
        Dated,
        Authenticating,
        Authenticated,
        ExamRetrieving,
        ExamRetrieved,
        Submiting,
        Submitted
    }

    public delegate string DgResponseMsg(char code);

    class Server0
    {
        TcpListener mServer;
        DgResponseMsg dgResponse;

        public Server0(DgResponseMsg dg)
        {
            dgResponse = dg;
        }

        public void Start(ref bool bToUpdateMsg, ref string msg)
        {    
            try
            {
                bToUpdateMsg = false;
                msg = String.Empty;
                // Set the TcpListener on port 23820.
                Int32 port = 23820;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener mServer = new TcpListener(port);
                //mServer = new TcpListener(IPAddress.Any, port);
                mServer = new TcpListener(localAddr, port);

                // Start listening for mClient requests.
                mServer.Start();

                msg += "\n Server has started";
                bToUpdateMsg = true;
                //Console.Write("\n Server has started");

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user mServer.AcceptSocket() here.
                    TcpClient mClient = mServer.AcceptTcpClient();

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = mClient.GetStream();

                    int i;

                    // Loop to receive all the data sent by the mClient.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        Console.WriteLine("Received: {0}", data);
                        string recvMsg = System.Text.Encoding.UTF8.GetString(bytes);
                        byte[] byteMsg = null;
                        char code = recvMsg[0];
                        switch(code)
                        {
                            case (char)RequestCode.DateStudentRetriving:
                                msg = dgResponse(code);
                                break;
                            case (char)RequestCode.QuestAnsKeyRetrieving:
                                msg = dgResponse(code);
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
                msg += "\nSocketException: " + e.Message;
                bToUpdateMsg = true;
                Console.Write("\nSocketException: {0}", e.Message);
            }
            finally
            {
                // Stop listening for new clients.
                mServer.Stop();
            }


            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }

        public void Stop()
        {
            if (mServer != null)
                mServer.Stop();
        }

        private bool Authenticate(string id, string birthdate)
        {
            if (id.Equals("A10") && birthdate.Equals("01/01/1990"))
                return true;
            return false;
        }
    }
}
