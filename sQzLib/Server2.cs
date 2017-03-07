using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace sQzLib
{
    public delegate bool DgNetCodeHndl(NetCode c, byte[] dat, int offs, ref byte[] outMsg);

    public class Server2
    {
        TcpListener mServer = null;
        bool bRW;//will cause trouble in multithreading
        bool bListening;//raise flag to stop
        int mPort;
        DgNetCodeHndl dgHndl;

        public Server2(DgNetCodeHndl dg)
        {
            string filePath = "ServerPort2.txt";
            mPort = 23820;
            if (System.IO.File.Exists(filePath))
                mPort = Convert.ToInt32(System.IO.File.ReadAllText(filePath));
            bRW = bListening = false;
            dgHndl = dg;
        }

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
            }
            catch (SocketException e)
            {
                cbMsg += "\nEx: " + e.Message;
                Stop(ref cbMsg);
            }

            while (bListening)
            {
                bool p = false;
                try { p = mServer.Pending(); }
                catch (SocketException e)
                {
                    p = false; cbMsg += "\nEx: " + e.Message; Stop(ref cbMsg);
                }
                if (p)
                {
                    bRW = true;
                    TcpClient cli = null;
                    NetworkStream stream = null;
                    try { cli = mServer.AcceptTcpClient(); }
                    catch (SocketException e)
                    {
                        cbMsg += "\nEx: " + e.Message;
                        Stop(ref cbMsg);
                    }

                    try { stream = cli.GetStream(); }
                    catch (SocketException e)
                    {
                        cbMsg += "\nEx: " + e.Message;
                        //Stop(ref cbMsg);
                        bRW = false;
                    }

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
                            }
                            catch (System.IO.IOException e)
                            { //client crash
                                cbMsg += "\nEx: " + e.Message;
                                //Stop(ref cbMsg);
                                bRW = false;
                            }
                            if (bRW && 0 < nByte)
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
                        if (bRW && recvMsg != null && 4 <= recvMsg.Length)
                        {
                            byte[] msg = null;
                            NetCode c = (NetCode)BitConverter.ToInt32(recvMsg, 0);
                            bRW = dgHndl(c, recvMsg, 4, ref msg);
                            if (bRW && msg != null && 0 < msg.Length)
                                try
                                {
                                    stream.Write(msg, 0, msg.Length);
                                }
                                catch (SocketException e)
                                {
                                    cbMsg += "\nEx: " + e.Message;
                                    //Stop(ref cbMsg);
                                    bRW = false;
                                }
                        }
                    }
                    try { cli.Close(); }
                    catch (SocketException e) {
                        cbMsg += "\nEx: " + e.Message;
                    }
                    cbMsg += "\nA client stopped.";
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
