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

        //public void Start(ref bool bMsging, ref string cbMsg.txt)
        public void Start(ref UICbMsg cbMsg)
        {
            if (mServer != null)
                return;
            bListening = true;
            cbMsg += "\nServer started.";
            try
            {
                mServer = new TcpListener(IPAddress.Any, mPort);
                mServer.Start();
            } catch(SocketException e) {
                cbMsg += "\nEx: " + e.Message;
                Stop(ref cbMsg);
            }

            while (bListening)
            {
                bool p = false;
                try { p = mServer.Pending(); }
                catch(SocketException e) {
                    p = false; cbMsg += "\nEx: " + e.Message; Stop(ref cbMsg);
                }
                if (p)
                {
                    bRW = true;
                    TcpClient cli = null;
                    NetworkStream stream = null;
                    try
                    {
                        cli = mServer.AcceptTcpClient();
                        stream = cli.GetStream();
                    } catch(SocketException e) {
                        cbMsg += "\nEx: " + e.Message; Stop(ref cbMsg);
                    }

                    while (bRW)
                    {
                        byte[] buf = new byte[1024];
                        List<byte[]> vRecvMsg = new List<byte[]>();
                        byte[] recvMsg = null;
                        int nByte = 0, nnByte = 0;

                        //Incoming message may be larger than the buffer size.
                        do {
                            try {
                                nnByte += nByte = stream.Read(buf, 0, buf.Length);
                            } catch(System.IO.IOException e) { //client crash
                                cbMsg += "\nEx: " + e.Message;
                                Stop(ref cbMsg);
                            }
                            if (bRW)
                            {
                                byte[] x = new byte[nByte];//use new buf
                                Buffer.BlockCopy(buf, 0, x, 0, nByte);
                                vRecvMsg.Add(x);
                            }
                        } while (bRW && stream.DataAvailable);
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
                                case NetSttCode.ToClose:
                                    bRW = false;
                                    break;
                                default:
                                    msg = BitConverter.GetBytes((Int32)NetSttCode.Unknown);
                                    break;
                            }
                            if (bRW && msg != null && 0 < msg.Length)
                                try {
                                    stream.Write(msg, 0, msg.Length);
                                } catch(SocketException e) {
                                    cbMsg += "\nEx: " + e.Message;
                                    Stop(ref cbMsg);
                                }
                        }
                    }
                    bRW = false;
                    try { cli.Close(); }
                    catch (SocketException e) { cbMsg += "\nEx: " + e.Message; Stop(ref cbMsg); }
                }
            }
            Stop(ref cbMsg);
            try { mServer.Stop(); }
            catch (SocketException e) { cbMsg += "\nEx: " + e.Message; }
            mServer = null;
            cbMsg += "\nServer stopped.";
        }

        public void Stop(ref UICbMsg cbMsg)
        {
            if (bListening)
                cbMsg += "\nServer is stopping.";
            bListening = false;
            bRW = false;
        }
    }
}
