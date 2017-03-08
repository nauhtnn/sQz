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
        Resubmit,
        ToClose,
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
        bool bSrvrRW;

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
            bSrvrRW = bRW = false;
        }

        public string SrvrAddr { set { mSrvrAddr = value; } }

        public int SrvrPort { set { mSrvrPort = value; } }

        public bool ConnectWR(ref UICbMsg cbMsg)
        {
            if (mClient != null)
                return false;
            bool ok = true;
            mClient = new TcpClient(AddressFamily.InterNetwork);
            bSrvrRW = bRW = true;
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

                if (!bRW || msg == null || msg.Length < sizeof(int))
                {
                    bRW = false;
                    break;
                }

                try { stream.Write(msg, 0, msg.Length); }
                catch(System.IO.IOException e)
                {
                    cbMsg += e.Message;
                    ok = false;
                }

                if (!ok || !bRW)
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

                if (ok && recvMsg != null && sizeof(int) <= recvMsg.Length)
                {
                    NetCode c = (NetCode)BitConverter.ToInt32(recvMsg, 0);
                    if (c == NetCode.ToClose)
                    {
                        bSrvrRW = bRW = false;
                    }
                    else
                        bRW = dgBufHndl(recvMsg, 0);
                }
            }
            if (ok && bSrvrRW)
                try { stream.Write(BitConverter.GetBytes((int)NetCode.ToClose), 0, sizeof(int)); }
                catch (System.IO.IOException e)
                {
                    cbMsg += e.Message;
                    ok = false;
                }
            bRW = false;
            try { mClient.Close(); }
            catch (SocketException e)
            {
                cbMsg += "\nEx: " + e.Message;
                ok = false;
            }
            cbMsg += "\nA client stopped.";
            mClient = null;
            return ok;
        }
    }
}