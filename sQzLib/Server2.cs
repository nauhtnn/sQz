using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace sQzLib
{
    public delegate bool DgSrvrCodeHndl(NetCode c, byte[] dat, int offs, ref byte[] outMsg);

    public class Server2
    {
        TcpListener mServer = null;
        bool bRW;//will cause trouble in multithreading, so it's thread-depending
        bool bListening;//raise flag to stop
        int mPort;
        DgSrvrCodeHndl dgHndl;

        public Server2(DgSrvrCodeHndl dg)
        {
            string filePath = "ServerPort2.txt";
            mPort = 23820;
            if (System.IO.File.Exists(filePath))
                mPort = Convert.ToInt32(System.IO.File.ReadAllText(filePath));
            bRW = bListening = false;
            dgHndl = dg;
        }

        public int SrvrPort { set { mPort = value; } }

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
                System.Threading.Thread.Sleep(8);//do not overhead CPU
                bool p = false;
                try { p = mServer.Pending(); }
                catch (SocketException e)
                {
                    p = false;
                    cbMsg += "\nEx: " + e.Message;
                    Stop(ref cbMsg);
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
                        bRW = false;
                    }

                    byte[] buf = new byte[1024 * 1024];

                    while (bRW)
                    {
                        List<byte[]> vRecvMsg = new List<byte[]>();
                        byte[] recvMsg = null;
                        int nByte = 0, nnByte = 0, nExpByte = 0;

                        //Incoming message may be larger than the buffer size.
                        //Do not rely on stream.DataAvailable, because the response
                        //  may be split into multiple TCP packets, and a packet
                        //  has not yet been delivered at the moment checking DataAvailable
                        try
                        {
                            nnByte += nByte = stream.Read(buf, 0, buf.Length);
                        }
                        catch (System.IO.IOException e)
                        {
                            cbMsg += "\nEx: " + e.Message;
                            nnByte = nByte = 0;
                            bRW = false;
                        }
                        if (4 < nByte)
                        {
                            nExpByte = BitConverter.ToInt32(buf, 0);
                            nByte -= 4;
                            nnByte -= 4;
                            byte[] x = new byte[nByte];//use new buf
                            Buffer.BlockCopy(buf, 4, x, 0, nByte);
                            vRecvMsg.Add(x);
                        }
                        else
                            break;//todo
                        while (bRW && nnByte < nExpByte)
                        {
                            try
                            {
                                nnByte += nByte = stream.Read(buf, 0, buf.Length);
                            }
                            catch (System.IO.IOException e)
                            {
                                cbMsg += "\nEx: " + e.Message;
                                bRW = false;
                            }
                            if (bRW && 0 < nByte)
                            {
                                byte[] x = new byte[nByte];//use new buf
                                Buffer.BlockCopy(buf, 0, x, 0, nByte);
                                vRecvMsg.Add(x);
                            }
                        }
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
                        if (bRW && recvMsg != null && 3 < recvMsg.Length)
                        {
                            byte[] msg = null;
                            NetCode c = (NetCode)BitConverter.ToInt32(recvMsg, 0);
                            //if (c == NetCode.ToClose)
                            //{
                            //    bClntRW = bRW = false;
                            //}
                            //else
                            //bRW = dgHndl(c, recvMsg, 4, ref msg);
                            bool nextRW = dgHndl(c, recvMsg, 4, ref msg);
                            byte[] msg2 = null;
                            if (msg == null || msg.Length < 1)
                                bRW = false;//case 1/2 to send NetCode.ToClose
                            else
                            {
                                msg2 = new byte[4 + msg.Length];
                                Buffer.BlockCopy(BitConverter.GetBytes(msg.Length), 0, msg2, 0, 4);//todo
                                Buffer.BlockCopy(msg, 0, msg2, 4, msg.Length);
                            }
                            if (bRW)
                                try
                                {
                                    stream.Write(msg2, 0, msg2.Length);
                                }
                                catch (SocketException e)
                                {
                                    cbMsg += "\nEx: " + e.Message;
                                    bRW = false;
                                }
                            if (!nextRW)
                                break;
                        }
                        else
                            bRW = false;//case 1/2 to send NetCode.ToClose
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
            bRW = bListening = false;
        }
    }
}
