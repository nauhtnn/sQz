using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace sQzLib
{
    public enum NetCode
    {
        Srvr1Auth = 0,
        Srvr1DatRetriving,
        QuestRetrieving,
        AnsKeyRetrieving,
        SrvrSubmitting,
        Dating,
        Authenticating,
        ExamRetrieving,
        Submiting,
        Unknown
    }

    public delegate bool DgClntBufHndl(byte[] buf);
    public delegate byte[] DgClntBufPrep();

    public class Client2
    {
        string mSrvrAddr;
        int mSrvrPort;
        TcpClient mTcpClnt = null;
        DgClntBufHndl dgBufHndl;
        DgClntBufPrep dgBufPrep;
        bool bRW;
        bool bCbMsg;

        public Client2(DgClntBufHndl hndl, DgClntBufPrep prep, bool allMsg)
        {
            string filePath = "ServerAddr.txt";
            if (System.IO.File.Exists(filePath))
                mSrvrAddr = System.IO.File.ReadAllText(filePath);
            else
                mSrvrAddr = "127.0.0.1";
            filePath = "ServerPort.txt";
            if (System.IO.File.Exists(filePath))
                mSrvrPort = Convert.ToInt32(System.IO.File.ReadAllText(filePath));
            else
                mSrvrPort = 23821;
            dgBufHndl = hndl;
            dgBufPrep = prep;
            bRW = false;
            bCbMsg = allMsg;
        }

        public string SrvrAddr { set { mSrvrAddr = value; } }

        public int SrvrPort { set { mSrvrPort = value; } }

        private bool ErrCode2Msg(int code, ref UICbMsg cbm)
        {
            if (code == 10061)
            {
                cbm += Txt.s._((int)TxI.CONN_NOK);
                return false;
            }
            return true;
        }

        public bool ConnectWR(ref UICbMsg cbMsg)
        {
            if (mTcpClnt != null)
            {
                cbMsg += "Client 2 ConnectWR: mTcpClnt != null";
                return false;
            }
            mTcpClnt = new TcpClient(AddressFamily.InterNetwork);
            bool bConn = true;//srvr side
            bRW = true;//clnt side
            try {
                mTcpClnt.Connect(mSrvrAddr, mSrvrPort);
            } catch (SocketException e) {
                if (ErrCode2Msg(e.ErrorCode, ref cbMsg))
                    cbMsg += "\nEx: " + e.Message;
                bConn = false;
            }
            NetworkStream stream = null;
            if(bConn)
                stream = mTcpClnt.GetStream();

            byte[] buf = new byte[1024 * 1024];

            while (bConn && bRW)
            {
                //write message to server
                byte[] msg = dgBufPrep();
                if (msg == null)
                    break;

                try {
                    stream.Write(BitConverter.GetBytes(msg.Length), 0, 4);
                    stream.Write(msg, 0, msg.Length);
                }
                catch (System.IO.IOException e) {
                    cbMsg += "\nEx: " + e.Message;
                    bConn = false;
                    break;
                }

                if (!bRW)
                    break;

                //read message from server
                int nLength, nByte;

                //Incoming message may be larger than the buffer size.
                //Do not rely on stream.DataAvailable, because the response
                //  may be split into multiple TCP packets, and a packet
                //  has not yet been delivered at the moment checking DataAvailable
                try
                {
                    nByte = stream.Read(buf, 0, buf.Length);
                } catch (System.IO.IOException e) {
                    cbMsg += "\nEx: " + e.Message;
                    bConn = false;
                    break;
                }

                if(nByte < 4)
                {
                    bConn = false;
                    break;
                }

                nLength = BitConverter.ToInt32(buf, 0);
                nByte -= 4;

                if (nLength < 1)
                    break;

                byte[] recvMsg = new byte[nLength];

                int offs = 0;
                if (0 < nByte)
                {
                    if (nLength < nByte)
                        nByte = nLength;
                    Buffer.BlockCopy(buf, 4, recvMsg, offs, nByte);
                    nLength -= nByte;
                    offs += nByte;
                }

                while (bConn && bRW && 0 < nLength)
                {
                    try
                    {
                        nByte = stream.Read(buf, 0, buf.Length);
                    }
                    catch (System.IO.IOException e)
                    {
                        cbMsg += "\nEx: " + e.Message;
                        bConn = false;
                    }
                    if (bConn)
                    {
                        if (nLength < nByte)
                            nByte = nLength;
                        Buffer.BlockCopy(buf, 0, recvMsg, offs, nByte);
                        nLength -= nByte;
                        offs += nByte;
                    }
                }

                if (bRW && recvMsg != null && 0 < recvMsg.Length)
                    bRW = dgBufHndl(recvMsg);
            }
            if (bCbMsg)
                cbMsg += Txt.s._((int)TxI.CONN_CLNT_CE);
            mTcpClnt.Close();
            mTcpClnt = null;
            return !bConn;
        }

        public void Close()
        {
            bRW = false;
        }
    }
}