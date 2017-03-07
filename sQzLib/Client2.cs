using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace sQzLib
{
    public enum NetCode
    {
        PrepDateStudent = '@',
        DateStudentRetriving = 'A',
        DateStudentRetrieved,
        //PrepQuestAnsKey,
        QuestAnsKeyRetrieving,
        QuestAnsKeyRetrieved,
        PrepMark,
        MarkSubmitting,
        MarkSubmitted,
        PrepDate = 'a',
        Dating,
        Dated,
        PrepAuth,
        Authenticating,
        Authenticated,
        //PrepExamRet,
        ExamRetrieving,
        ExamRetrieved,
        Submiting,
        Submitted,
        ToClose,
        Unknown
    }

    public delegate bool DgNetBufHndl(byte[] buf, int offs);
    public delegate bool DgBufPrep(ref byte[] outBuf);

    public class Client2
    {
        string mSrvrAddr;
        int mSrvrPort;
        TcpClient mClient = null;
        DgNetBufHndl dgBufHndl;
        DgBufPrep dgBufPrep;
        bool bRW;

        public Client2(DgNetBufHndl hndl, DgBufPrep prep)
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
                return true;
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

            while (ok && bRW)
            {
                //write message to server
                byte[] msg = null;
                bRW = dgBufPrep(ref msg);

                if (!bRW)
                    break;

                try { stream.Write(msg, 0, msg.Length); }
                catch(System.IO.IOException e)
                {
                    cbMsg += e.Message;
                    ok = false;
                }

                if (!ok)
                    break;

                //read message from server
                byte[] buf = new byte[1024*1024];
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
                        cbMsg += "\nEx: " + e.Message;
                        ok = false;
                    }
                    if (ok && 0 < nByte)
                    {
                        byte[] x = new byte[nByte];//use new buf
                        Buffer.BlockCopy(buf, 0, x, 0, nByte);
                        vRecvMsg.Add(x);
                    }
                } while (ok && stream.DataAvailable);
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
                if (ok && recvMsg != null && 4 <= recvMsg.Length)
                    bRW = dgBufHndl(recvMsg, 0);
            }
            try { mClient.Close(); }
            catch (SocketException e)
            {
                cbMsg += "\nEx: " + e.Message;
            }
            cbMsg += "\nA client stopped.";
            mClient = null;
            return !ok;
        }
    }
}