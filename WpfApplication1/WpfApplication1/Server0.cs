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
        PrepDateStudent = '@',
        DateStudentRetriving = 'A',
        DateStudentRetrieved,
        PrepQuestAnsKey,
        QuestAnsKeyRetrieving,
        QuestAnsKeyRetrieved,
        PrepMark,
        MarkSubmitting,
        MarkSubmitted,
        PrepDate = 'a',
        Dating,
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
        bool mStart;
        bool mRunning;
        bool mClosing;

        public Server0(DgResponseMsg dg)
        {
            string filePath = "ServerPort0.txt";
            int port = 23820;
            if (System.IO.File.Exists(filePath))
                port = Convert.ToInt32(System.IO.File.ReadAllText(filePath));
            //IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            mServer = new TcpListener(IPAddress.Any, port);

            dgResponse = dg;
            mStart = mRunning = mClosing = false;
        }

        public void Start(ref bool bToUpdateMsg, ref string msg)
        {    
            try
            {
                bToUpdateMsg = false;
                msg = String.Empty;

                // Start listening for mClient requests.
                mServer.Start();

                mStart = true;
                mClosing = false;

                msg += "\n Server has started";
                bToUpdateMsg = true;
                //Console.Write("\n Server has started");

                // Buffer for reading data
                Byte[] bytes = new Byte[256];

                // Enter the listening loop.
                while (!mClosing)
                {
                    //Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user mServer.AcceptSocket() here.
                    if (mServer.Pending())
                    {
                        TcpClient mClient = mServer.AcceptTcpClient();

                        mRunning = true;

                        // Get a stream object for reading and writing
                        NetworkStream stream = mClient.GetStream();

                        // Check to see if this NetworkStream is readable.
                        if (stream.CanRead)
                        {
                            byte[] myReadBuffer = new byte[1024];
                            StringBuilder recvMsg = new StringBuilder();
                            int nByte = 0;

                            // Incoming message may be larger than the buffer size.
                            do
                            {
                                nByte = stream.Read(myReadBuffer, 0, myReadBuffer.Length);

                                recvMsg.AppendFormat("{0}", Encoding.UTF8.GetString(myReadBuffer, 0, nByte));

                            }
                            while (!mClosing && stream.DataAvailable);
                            if (!mClosing && 0 < recvMsg.Length)
                            {
                                byte[] byteMsg = null;
                                char code = recvMsg[0];
                                switch (code)
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
                            }
                        }
                        else
                        {
                            Console.WriteLine("Sorry.  You cannot read from this NetworkStream.");
                        }

                        // Shutdown and end connection
                        if(mClient.Connected)
                            mClient.Close();
                        mRunning = false;
                    }
                }
            }
            catch (SocketException e)
            {
                msg += "\nSocketException: " + e.Message;
                bToUpdateMsg = true;
                Console.Write("\nSocketException: {0}", e.Message);
                mClosing = true;
            }
            finally
            {
                mRunning = false;
                mStart = false;
                if (mClosing)
                    mServer.Stop();
            }
        }

        public void Stop(ref bool bToUpdateMsg, ref string msg)
        {
            mClosing = true;
            if (mStart && !mRunning)
                mServer.Stop();
            bToUpdateMsg = true;
            msg = "Server is closing.\n";
        }

        //private bool Authenticate(string id, string birthdate)
        //{
        //    if (id.Equals("A10") && birthdate.Equals("01/01/1990"))
        //        return true;
        //    return false;
        //}
    }
}
