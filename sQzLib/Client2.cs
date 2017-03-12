using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace sQzLib
{
    public enum NetCode
    {
        //PrepDateStudent = 0,
        DateStudentRetriving = 0,
        //DateStudentRetrieved,
        //PrepQuestAnsKey,
        QuestAnsKeyRetrieving,
        //QuestAnsKeyRetrieved,
        PrepMark,
        MarkSubmitting,
        MarkSubmitted,
        //PrepDate = 'a',
        Dating,
        //Dated,
        //PrepAuth,
        Authenticating,
        //Authenticated,
        //PrepExamRet,
        ExamRetrieving,
        //ExamRetrieved,
        Submiting,
        Submitted,
        Resubmit,
        //ToClose,
        Unknown
    }

    public delegate bool DgCliBufHndl(byte[] buf, int offs);
    public delegate bool DgCliBufPrep(ref byte[] outBuf);

    public class Client2
    {
        string mSrvrAddr;
        int mSrvrPort;
        TcpClient mClient = null;
        DgCliBufHndl dgBufHndl;
        DgCliBufPrep dgBufPrep;
        bool bRW;

        public Client2(DgCliBufHndl hndl, DgCliBufPrep prep)
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
        }

        public string SrvrAddr { set { mSrvrAddr = value; } }

        public int SrvrPort { set { mSrvrPort = value; } }

        public bool ConnectWR(ref UICbMsg cbMsg)
        {
            if (mClient != null)
                return false;
            bool ok = true;
            mClient = new TcpClient(AddressFamily.InterNetwork);
            bRW = true;
            try {
                mClient.Connect(mSrvrAddr, mSrvrPort);
            } catch (SocketException e) {
                cbMsg += e.Message;
                ok = false;
            }
            NetworkStream stream = null;
            if(ok)
                try { stream = mClient.GetStream(); }
                catch (SocketException e) {
                    cbMsg += "\nEx: " + e.Message;
                    ok = false;
                }

            byte[] buf = new byte[1024 * 1024];

            while (ok && bRW)
            {
                //write message to server
                byte[] msg = null;
                byte[] msg2 = null;//todo
                bRW = dgBufPrep(ref msg);

                if (!bRW || msg == null || msg.Length < sizeof(int))
                {
                    bRW = false;
                    break;
                }

                msg2 = new byte[4 + msg.Length];
                Buffer.BlockCopy(BitConverter.GetBytes(msg.Length), 0, msg2, 0, 4);//to optmz
                Buffer.BlockCopy(msg, 0, msg2, 4, msg.Length);

                try { stream.Write(msg2, 0, msg2.Length); }
                catch(System.IO.IOException e)
                {
                    cbMsg += e.Message;
                    ok = false;
                }

                if (!ok || !bRW)
                    break;

                //read message from server
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
                } catch (System.IO.IOException e) {
                    cbMsg += "\nEx: " + e.Message;
                    nnByte = nByte = 0;
                    ok = false;
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
                while (ok && nnByte < nExpByte)
                {
                    try
                    {
                        nnByte += nByte = stream.Read(buf, 0, buf.Length);
                    }
                    catch (System.IO.IOException e)
                    {
                        cbMsg += "\nEx: " + e.Message;
                        ok = false;
                    }
                    if (ok && 0 < nByte)
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

                if (ok && recvMsg != null && 0 < recvMsg.Length)
                {
                    //NetCode c = (NetCode)BitConverter.ToInt32(recvMsg, 0);
                    //if (c == NetCode.ToClose)
                    //    bRW = false;
                    //else
                        bRW = dgBufHndl(recvMsg, 0);
                }
            }
            bRW = false;
            mClient.Close();
            cbMsg += "\nA client stopped.";
            mClient = null;
            return ok;
        }

        public void Close()
        {
            //todo
            //if (mClient != null && mClient.Connected)
            //    mClient.Close();
        }
    }
}