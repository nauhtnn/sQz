﻿using System;
using System.Net.Sockets;

namespace sQzLib
{
    public enum NetSttCode
    {
        PrepDateStudent = '@',
        DateStudentRetriving = 'A',
        DateStudentRetrieved,
        PrepQuestAnsKey,
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
        PrepExamRet,
        ExamRetrieving,
        ExamRetrieved,
        Submiting,
        Submitted,
        Unknown
    }

    public delegate void DgCallback(IAsyncResult ar);

    public class Client0
    {
        string mServerAddr;
        Int32 mPort;
        TcpClient mClient = null;
        static Client0 sClient = null;

        public Client0()
        {
            string filePath = "ServerAddr.txt";
            if (System.IO.File.Exists(filePath))
                mServerAddr = System.IO.File.ReadAllText(filePath);
            else
                mServerAddr = "127.0.0.1";
            filePath = "ServerPort.txt";
            if (System.IO.File.Exists(filePath))
                mPort = Convert.ToInt32(System.IO.File.ReadAllText(filePath));
            else
                mPort = 23821;
            mClient = new TcpClient(AddressFamily.InterNetwork);
        }

        public void SetSrvrAddr(string addr)
        {
            mServerAddr = addr;
        }

        public void SetSrvrPort(Int32 port)
        {
            mPort = port;
        }

        public static Client0 Instance()
        {
            if (sClient == null)
                sClient = new Client0();
            return sClient;
        }

        public void BeginConnect(DgCallback callback)
        {
            if (mClient == null)
                mClient = new TcpClient(AddressFamily.InterNetwork);
            if (!mClient.Connected)
                mClient.BeginConnect(mServerAddr, mPort, new AsyncCallback(callback), mClient);
        }

        public void BeginWrite(string data, DgCallback callback)
        {
            if (mClient == null || !mClient.Connected)
                return;

            NetworkStream stream = mClient.GetStream();
            stream.BeginWrite(System.Text.Encoding.UTF8.GetBytes(data), 0, System.Text.Encoding.UTF8.GetByteCount(data),
                new AsyncCallback(callback), stream);
        }

        public void BeginRead(byte[] buf, int sz, DgCallback callback)
        {
            if (mClient == null || !mClient.Connected)
                return;
            NetworkStream stream = mClient.GetStream();
            stream.BeginRead(buf, 0, sz, new AsyncCallback(callback), stream);
        }

        public void Close()
        {
            if (mClient == null)
                return;
            mClient.Close();
            mClient = null;
        }
    }
}