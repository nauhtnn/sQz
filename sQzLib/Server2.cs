using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace sQzLib
{
    public delegate bool DgSrvrBufHndl(byte[] msg, out byte[] outMsg);

    public class Server2
    {
        TcpListener mTcpListr = null;
        //bool bRW;//todo all clnt
        bool bRW1;//will cause trouble in multithreading, so it's thread-depending
        bool bListning;//raise flag to stop
        int mPort;
        DgSrvrBufHndl dgHndl;

        public Server2(DgSrvrBufHndl dg)
        {
            string filePath = "ServerPort2.txt";
            mPort = 23820;
            if (System.IO.File.Exists(filePath))
                mPort = Convert.ToInt32(System.IO.File.ReadAllText(filePath));
            bRW1 = bListning = false;
            dgHndl = dg;
        }

        public int SrvrPort { set { mPort = value; } }

        public void Start(ref UICbMsg cbMsg)
        {
            if (mTcpListr != null)
                return;
            bListning = true;
            cbMsg += Txt.s._[(int)TxI.CONN_SRVR_ST];
            mTcpListr = new TcpListener(IPAddress.Any, mPort);
            try {
                mTcpListr.Start();
            }
            catch (SocketException e) {
                cbMsg += "\nEx: " + e.Message;
                Stop(ref cbMsg);
            }

            while (bListning)
            {
                System.Threading.Thread.Sleep(8);//do not overhead CPU
                bool p = false;
                try { p = mTcpListr.Pending(); }
                catch (InvalidOperationException e) {
                    p = false;
                    cbMsg += "\nEx: " + e.Message;
                    Stop(ref cbMsg);
                }
                if (p)
                {
                    bRW1 = true;
                    TcpClient cli = null;
                    NetworkStream stream = null;
                    try { cli = mTcpListr.AcceptTcpClient(); }
                    catch (SocketException e)
                    {
                        cbMsg += "\nEx: " + e.Message;
                        Stop(ref cbMsg);
                    }

                    if (!bListning)
                        break;

                    try { stream = cli.GetStream(); }
                    catch (InvalidOperationException e)
                    {
                        cbMsg += "\nEx: " + e.Message;
                        bRW1 = false;
                    }

                    byte[] buf = new byte[1024 * 1024];

                    List<byte[]> vRecvMsg = new List<byte[]>();
                    int nExpByte, nByte, nnByte;
                    while (bRW1)
                    {
                        vRecvMsg.Clear();
                        nnByte = 0;
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
                            bRW1 = false;
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
                            break;
                        
                        while (bRW1 && nnByte < nExpByte)
                        {
                            try
                            {
                                nnByte += nByte = stream.Read(buf, 0, buf.Length);
                            }
                            catch (System.IO.IOException e)
                            {
                                cbMsg += "\nEx: " + e.Message;
                                bRW1 = false;
                            }
                            if (bRW1 && 0 < nByte)
                            {
                                byte[] x = new byte[nByte];//use new buf
                                Buffer.BlockCopy(buf, 0, x, 0, nByte);
                                vRecvMsg.Add(x);
                            }
                        }

                        byte[] recvMsg = null;
                        if (bRW1 && 0 < vRecvMsg.Count)
                        {
                            recvMsg = new byte[nnByte];
                            int offs = 0;
                            foreach(byte[] i in vRecvMsg)
                            {
                                Buffer.BlockCopy(i, 0, recvMsg, offs, i.Length);
                                offs += i.Length;
                            }
                        }
                        if (bRW1 && recvMsg != null && 3 < recvMsg.Length)
                        {
                            byte[] msg;
                            bRW1 = dgHndl(recvMsg, out msg);
                            if (msg != null || 0 < msg.Length)
                            {
                                try
                                {
                                    stream.Write(BitConverter.GetBytes(msg.Length), 0, 4);
                                    stream.Write(msg, 0, msg.Length);
                                }
                                catch (System.IO.IOException e)
                                {
                                    cbMsg += "\nEx: " + e.Message;
                                    bRW1 = false;
                                }
                            }
                            else
                                bRW1 = false;
                        }
                        else
                            bRW1 = false;
                    }
                    cli.Close();
                    cbMsg += Txt.s._[(int)TxI.CONN_CLNT_CE];
                }
            }
            bListning = false;
            try { mTcpListr.Stop(); }
            catch (SocketException e) { cbMsg += "\nEx: " + e.Message; }
            mTcpListr = null;
            cbMsg += Txt.s._[(int)TxI.CONN_SRVR_CE];
        }

        public void Stop(ref UICbMsg cbMsg)
        {
            if (bListning)
                cbMsg += Txt.s._[(int)TxI.CONN_SRVR_CG];
            bRW1 = bListning = false;
        }
    }
}
