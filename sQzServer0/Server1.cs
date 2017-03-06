using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using sQzLib;

namespace sQzServer0
{
    class Server1
    {
        TcpListener mServer;
        bool mStart;
        bool bRunning;
        bool mClosing;

        public Server1()
        {
            string filePath = "ServerPort1.txt";
            int port = 23821;
            if (System.IO.File.Exists(filePath))
                port = Convert.ToInt32(System.IO.File.ReadAllText(filePath));
            //IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            mServer = new TcpListener(IPAddress.Any, port);

            mStart = mClosing = false;
            bRunning = true;
        }

        public void Start(ref bool bToUpdateMsg, ref string cbMsg)
        {
            try
            {
                bToUpdateMsg = false;
                cbMsg = String.Empty;

                // Start listening for mClient requests.
                mServer.Start();

                mStart = true;
                mClosing = false;

                cbMsg += "\n Server has started";
                bToUpdateMsg = true;
                //Console.Write("\n Server has started");

                // Enter the listening loop.
                while (bRunning)
                {
                    //Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user mServer.AcceptSocket() here.
                    if (mServer.Pending())
                    {
                        TcpClient mClient = mServer.AcceptTcpClient();

                        bRunning = true;

                        // Get a stream object for reading and writing
                        NetworkStream stream = mClient.GetStream();

                        // Check to see if this NetworkStream is readable.
                        while (bRunning)
                        {
                            byte[] buf = new byte[1024];
                            List<byte[]> vRecvMsg = new List<byte[]>();
                            byte[] recvMsg = null;
                            int nByte = 0, nnByte = 0;

                            //Incoming message may be larger than the buffer size.
                            do
                            {
                                try
                                {
                                    nnByte += nByte = stream.Read(buf, 0, buf.Length);
                                }
                                catch (System.IO.IOException e)
                                {
                                    //stand firmly when client crash
                                    Console.WriteLine(e.Message);
                                    bRunning = false;
                                }
                                byte[] x = new byte[nByte];//use new buf
                                Buffer.BlockCopy(buf, 0, x, 0, nByte);
                                vRecvMsg.Add(x);
                            }
                            while (bRunning && stream.DataAvailable);
                            if (0 < vRecvMsg.Count)
                            {
                                recvMsg = new byte[nnByte];
                                int offs = 0;
                                for (int i = 0; i < vRecvMsg.Count; ++i)
                                {
                                    Buffer.BlockCopy(vRecvMsg[i], 0, recvMsg, offs, vRecvMsg[i].Length);
                                    offs += vRecvMsg[i].Length;
                                }
                            }
                            if (bRunning && recvMsg != null && 0 < recvMsg.Length)
                            {
                                byte[] msg = null;
                                NetSttCode c = (NetSttCode)BitConverter.ToInt32(recvMsg, 0);
                                switch (c)
                                {
                                    case NetSttCode.Dating:
                                        msg = Date.sbArr;
                                        break;
                                    case NetSttCode.Authenticating:
                                        msg = BitConverter.GetBytes((Int32)1);
                                        break;
                                    case NetSttCode.ExamRetrieving:
                                        msg = Question.sbArr;
                                        break;
                                    case NetSttCode.Submiting:
                                        msg = BitConverter.GetBytes((Int32)NetSttCode.Unknown);
                                        break;
                                    default:
                                        msg = BitConverter.GetBytes((Int32)NetSttCode.Unknown);
                                        break;
                                }
                                if (bRunning && msg != null && 0 < msg.Length)
                                    stream.Write(msg, 0, msg.Length);
                            }
                        }
                        // Shutdown and end connection
                        try
                        {
                            //stand firmly when client crash
                            mClient.Close();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                cbMsg += "\nSocketException: " + e.Message;
                bToUpdateMsg = true;
                Console.Write("\nSocketException: {0}", e.Message);
                mClosing = true;
            }
            finally
            {
                bRunning = false;
                mStart = false;
                if (mClosing)
                    mServer.Stop();
            }
        }

        public void Stop(ref bool bToUpdateMsg, ref string msg)
        {
            mClosing = true;
            if (mStart && !bRunning)
                mServer.Stop();//TODO: send msg to all client before stop
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
