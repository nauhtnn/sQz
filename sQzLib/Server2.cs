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
        bool bClntRW;
        bool bListening;//raise flag to stop
        int mPort;
        DgSrvrCodeHndl dgHndl;

        public Server2(DgSrvrCodeHndl dg)
        {
            string filePath = "ServerPort2.txt";
            mPort = 23820;
            if (System.IO.File.Exists(filePath))
                mPort = Convert.ToInt32(System.IO.File.ReadAllText(filePath));
            bClntRW = bRW = bListening = false;
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
                    bClntRW = bRW = true;
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
                        bClntRW = bRW = false;
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
                                bClntRW = bRW = false;
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
                            if (c == NetCode.ToClose)
                            {
                                bClntRW = bRW = false;
                            }
                            else
                                bRW = dgHndl(c, recvMsg, 4, ref msg);
                            if (msg == null || msg.Length < 4)
                                bRW = false;//case 1/2 to send NetCode.ToClose
                            if (bRW)
                                try
                                {
                                    stream.Write(msg, 0, msg.Length);
                                }
                                catch (SocketException e)
                                {
                                    cbMsg += "\nEx: " + e.Message;
                                    bClntRW = bRW = false;
                                }
                        }
                        else
                            bRW = false;//case 1/2 to send NetCode.ToClose
                    }
                    bool toClose = true;
                    if(bClntRW)
                        try { stream.Write(BitConverter.GetBytes((int)NetCode.ToClose), 0, sizeof(int)); }
                        catch (System.IO.IOException e)
                        {
                            cbMsg += e.Message;
                            toClose = false;
                        }
                    if(toClose)
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
            bClntRW = bRW = bListening = false;
        }
    }
}
