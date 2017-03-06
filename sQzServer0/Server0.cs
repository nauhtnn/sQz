using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using sQzLib;

namespace WpfApplication1
{
    class Server0
    {
        TcpListener mServer = null;
        bool bRW;//will cause trouble in multithreading
        bool bListening;//raise flag to stop
        int mPort;

        public Server0()
        {
            string filePath = "ServerPort0.txt";
            mPort = 23820;
            if (System.IO.File.Exists(filePath))
                mPort = Convert.ToInt32(System.IO.File.ReadAllText(filePath));
            bRW = bListening = false;
        }

        public void Start(ref bool bMsging, ref string cbMsg)
        {
            if (mServer != null)
                return;
            bListening = true;
            try
            {
                mServer = new TcpListener(IPAddress.Any, mPort);
                mServer.Start();
            } catch(SocketException e) {
                cbMsg += e.Message;
                bMsging = true;
                bListening = false;
            }

            cbMsg += "\n Server has started";
            bMsging = true;

            while (bListening)
            {
                if (mServer.Pending())
                {
                    bRW = true;
                    TcpClient mClient = mServer.AcceptTcpClient();
                    NetworkStream stream = mClient.GetStream();
                    while (bRW)
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
                            } catch(System.IO.IOException e)
                            {
                                //stand firmly when client crash
                                cbMsg += e.Message;
                                bMsging = true;
                                bRW = false;
                            }
                            if (bRW)
                            {
                                byte[] x = new byte[nByte];//use new buf
                                Buffer.BlockCopy(buf, 0, x, 0, nByte);
                                vRecvMsg.Add(x);
                            }
                        }
                        while (bRW && stream.DataAvailable);
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
                        if (bRW && recvMsg != null && 0 < recvMsg.Length)
                        {
                            byte[] msg = null;
                            NetSttCode c = (NetSttCode)BitConverter.ToInt32(recvMsg, 0);
                            switch (c)
                            {
                                case NetSttCode.DateStudentRetriving:
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
                                case NetSttCode.QuestAnsKeyRetrieving:
                                    msg = Question.sbArr;
                                    break;
                                case NetSttCode.MarkSubmitting:
                                    msg = BitConverter.GetBytes((Int32)NetSttCode.Unknown);
                                    break;
                                case NetSttCode.ToShutdown:
                                    bRW = false;
                                    break;
                                default:
                                    msg = BitConverter.GetBytes((Int32)NetSttCode.Unknown);
                                    break;
                            }
                            if (bRW && msg != null && 0 < msg.Length)
                                stream.Write(msg, 0, msg.Length);
                        }
                    }
                    bRW = false;
                    try { mClient.Close(); }
                    catch (SocketException e) { Console.WriteLine(e.Message); }
                }
            }
            cbMsg += "Server stopped";
            bMsging = true;
            bListening = false;
            try { mServer.Stop(); }
            catch (SocketException e) { Console.WriteLine(e.Message); }
            mServer = null;
        }

        public void Stop(ref bool bMsging, ref string msg)
        {
            bListening = false;
            bRW = false;
            bMsging = true;
            msg = "Server is stopping";
        }

        //private bool Authenticate(string id, string birthdate)
        //{
        //    if (id.Equals("A10") && birthdate.Equals("01/01/1990"))
        //        return true;
        //    return false;
        //}
    }
}
