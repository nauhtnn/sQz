using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace WpfApplication1
{
    enum NetSttCode
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
        Submitted,
        Unknown
    }

    public delegate byte[] DgResponseMsg(char code);

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
                            byte[] buf = new byte[1024];
                            //StringBuilder recvMsg = new StringBuilder();
                            List<byte[]> vRecvMsg = new List<byte[]>();
                            byte[] recvMsg = null;
                            int nByte = 0, totByte = 0;

                            // Incoming message may be larger than the buffer size.
                            do
                            {
                                totByte += nByte = stream.Read(buf, 0, buf.Length);
                                //recvMsg.AppendFormat("{0}", Encoding.UTF8.GetString(buf, 0, nByte));
                                //if (nByte < buf.Length)
                                //{
                                    byte[] x = new byte[nByte];//use new buf
                                    Buffer.BlockCopy(buf, 0, x, 0, nByte);
                                    vRecvMsg.Add(x);
                                //}
                                //else
                                //    vRecvMsg.Add(buf);
                            }
                            while (!mClosing && stream.DataAvailable);
                            if(0 < vRecvMsg.Count)
                            {
                                recvMsg = new byte[totByte];
                                int offs = 0;
                                for (int i = 0; i < vRecvMsg.Count; ++i)
                                {
                                    Buffer.BlockCopy(vRecvMsg[i], 0, recvMsg, offs, vRecvMsg[i].Length);
                                    offs += vRecvMsg[i].Length;
                                }
                            }
                            if (!mClosing && recvMsg != null)
                            {
                                byte[] msg = null;
                                //char code = BitConverter.ToChar(recvMsg, 0);
                                Int32 c = BitConverter.ToInt32(recvMsg, 0);
                                char code = (char)c;
                                switch (code)
                                {
                                    case (char)NetSttCode.DateStudentRetriving:
                                        //msg = dgResponse(code);
                                        int sz = 0;
                                        if (Date.sbArr != null)
                                            sz += Date.sbArr.Length;
                                        if (Student.sbArr != null)
                                            sz += Student.sbArr.Length;
                                        msg = new byte[sz];
                                        sz = 0;
                                        if (Date.sbArr != null)
                                        {
                                            sz = Date.sbArr.Length;
                                            Buffer.BlockCopy(Date.sbArr, 0, msg, 0, sz);
                                        }
                                        if (Student.sbArr != null)
                                            Buffer.BlockCopy(Student.sbArr, 0, msg, sz, Student.sbArr.Length);
                                        break;
                                    case (char)NetSttCode.QuestAnsKeyRetrieving:
                                        //msg = dgResponse(code);
                                        msg = Question.sbArr;
                                        break;
                                    case (char)NetSttCode.MarkSubmitting:
                                        break;
                                    default:
                                        msg = BitConverter.GetBytes((char)NetSttCode.Unknown);
                                        break;
                                }
                                // Send back a response.
                                stream.Write(msg, 0, msg.Length);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Sorry.  You cannot read from this NetworkStream.");
                        }

                        // Shutdown and end connection
                        if(mClient.Connected)
                            mClient.Close();
                        //mClient = null;//bookmark
                        mRunning = false;
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
