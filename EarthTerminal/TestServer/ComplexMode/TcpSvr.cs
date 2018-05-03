using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

/////////////////////////////////////////////////////////////////////////////////////////
/*

���⣺��C#��ʹ���첽Socket���ʵ��TCP��������C/S��ͨѶ����(һ)----������ⲿ��

������.NET��TcpListener��TcpClient��ʱ��,�ҷǳ�����,�Ǿ�������Ҫ��ͨѶģʽ
����ʹ��֮�������ǵ�����̫������,������Ҫһ�����õ�������������.

�����ṩ��һЩ��,���Ժܺõ����Tcp��C/SͨѶģʽ.�ڱ��ĵĵڶ�����,�ҽ�Ϊ��ҽ�����ôʹ������

��Ҫͨ���¼�����ʵ�����Ĺ���:
���������¼�����:

��������
�¿ͻ�������
�ͻ��˹ر�
���յ�����

�ͻ���ʹ�õ��¼�����:

�����ӷ�����
���յ�����
���ӹر�

����Ϊ�����Ĵ�������仯,���ṩ�˱������ͱ��Ľ�������ʵ�ַ���.
ע��:�����û�о����ϸ�Ĳ���,�����Bug,�뷢�͸���,�һ�������������Ϊ�Ƕ��ҵĹ�����֧��.

 
/// <summary>
/// (C)2003-2005 C2217 Studio
/// ��������Ȩ��
///
/// �ļ�����: TcpCSFramework.cs
/// �ļ�ID:
/// �������: C#
/// �ļ�˵��: �ṩTCP��������C/S��ͨѶ���ܻ�����
/// (ʹ���첽Socket���ʵ��)
///
/// ��ǰ�汾: 1.1
/// �滻�汾: 1.0
///
/// ����: �����
/// EMail: dyj057@gmail.com
/// ��������: 2005-3-9
/// ����޸�����: 2005-3-17
///
/// ��ʷ�޸ļ�¼:
///
/// ʱ��: 2005-3-14
/// �޸�����:
/// 1.����Ibms.Net.TcpCSFramework�����ռ�����Session����.
/// 2.�޸�NetEventArgs��,����Ӧ����Ӷ���.
/// 3.����˻Ự�˳�����,���ʺ�ʵ�ʵ����.
/// ע��:
/// * ǿ���˳�������Ӧ�ó���ֱ�ӽ���,����ͨ���������������
/// ������߳����쳣�˳���,û��ִ���������˳�������������.
/// * �������˳�������Ӧ�ó���ִ���������˳��ķ����ؼ�����
/// ��Ҫ����Socket.Shutdown( SocketShutdown.Both )��ŵ���
/// Socket.Close()����,������ֱ�ӵĵ���Socket.Close()����,
/// ����������ý�����ǿ���˳�����.
///
/// ʱ��: 2005-3-16
/// �޸�����:
/// 1.����TcpCli,Coder,DatagramResover����,�ѳ����ʵ�ֲ��ַ���
/// 2.�ļ��汾�޸�Ϊ1.1,1.0�汾��Ȼ����,����Ϊ:
/// TcpCSFramework_v1.0.cs
/// 3.��TcpServer���޸��Զ����hashtableΪϵͳHashtable����
///
/// </summary>
*/
//////////////////////////////////////////////////////////////////////////////////////////


namespace TestServer.ComplexMode
{
    /// <summary>
    /// �ṩTCP���ӷ���ķ�������
    ///
    /// �汾: 1.1
    /// �滻�汾: 1.0
    ///
    /// �ص�:
    /// 1.ʹ��hash�������������ӿͻ��˵�״̬���յ�����ʱ��ʵ�ֿ��ٲ���.ÿ��
    /// ��һ���µĿͻ������Ӿͻ����һ���µĻỰ(Session).��Session�����˿�
    /// ���˶���.
    /// 2.ʹ���첽��Socket�¼���Ϊ�������������ͨѶ����.
    /// 3.֧�ִ���ǵ����ݱ��ĸ�ʽ��ʶ��,����ɴ����ݱ��ĵĴ������Ӧ���ӵ���
    /// �绷��.�����涨����֧�ֵ�������ݱ���Ϊ640K(��һ�����ݰ��Ĵ�С���ܴ���
    /// 640K,���������������Զ�ɾ����������,��Ϊ�ǷǷ�����),��ֹ��Ϊ���ݱ���
    /// �����Ƶ����������Ƿ���������
    /// 4.ͨѶ��ʽĬ��ʹ��Encoding.Default��ʽ�����Ϳ��Ժ���ǰ32λ����Ŀͻ���
    /// ͨѶ.Ҳ����ʹ��U-16��U-8�ĵ�ͨѶ��ʽ����.�����ڸ�DatagramResolver���
    /// �̳��������ر���ͽ��뺯��,�Զ�����ܸ�ʽ����ͨѶ.��֮ȷ���ͻ��������
    /// ����ʹ����ͬ��ͨѶ��ʽ
    /// 5.ʹ��C# native code,��������Ч�ʵĿ��ǿ��Խ�C++����д�ɵ�32λdll������
    /// C#���Ĵ���, ��������ȱ������ֲ��,������Unsafe����(�����C++����Ҳ����)
    /// 6.�������Ʒ�����������½�ͻ�����Ŀ
    /// 7.��ʹ��TcpListener�ṩ���Ӿ�ϸ�Ŀ��ƺ͸���ǿ���첽���ݴ���Ĺ���,����Ϊ
    /// TcpListener�������
    /// 8.ʹ���첽ͨѶģʽ,��ȫ���õ���ͨѶ�������߳�����,���뿼��ͨѶ��ϸ��
    ///
    /// ע��:
    /// 1.���ֵĴ�����Rational XDE����,���������淶����
    ///
    /// ԭ��:
    ///
    ///
    /// ʹ���÷�:
    ///
    /// ����:
    ///
    /// </summary>
    public class TcpSvr
    {
        #region �����ֶ�

        /// <summary>
        /// Ĭ�ϵķ�����������ӿͻ��˶�����
        /// </summary>
        public const int DefaultMaxClient = 100;

        /// <summary>
        /// �������ݻ�������С64K
        /// </summary>
        public const int DefaultBufferSize = 64 * 1024;

        /// <summary>
        /// ������ݱ��Ĵ�С
        /// </summary>
        public const int MaxDatagramSize = 640 * 1024;

        /// <summary>
        /// ���Ľ�����
        /// </summary>
        private DatagramResolver _resolver;

        /// <summary>
        /// ͨѶ��ʽ���������
        /// </summary>
        private Coder _coder;

        /// <summary>
        /// ����������ʹ�õĶ˿�
        /// </summary>
        private ushort _port;

        /// <summary>
        /// ������������������ͻ���������
        /// </summary>
        private ushort _maxClient;

        /// <summary>
        /// ������������״̬
        /// </summary>
        private bool _isRun;

        /// <summary>
        /// �������ݻ�����
        /// </summary>
        private byte[] _recvDataBuffer;

        /// <summary>
        /// ������ʹ�õ��첽Socket��,
        /// </summary>
        private Socket _svrSock;

        /// <summary>
        /// �������пͻ��˻Ự�Ĺ�ϣ��
        /// </summary>
        private Hashtable _sessionTable;

        /// <summary>
        /// ��ǰ�����ӵĿͻ�����
        /// </summary>
        private ushort _clientCount;

        #endregion

        #region �¼�����

        /// <summary>
        /// �ͻ��˽��������¼�
        /// </summary>
        public event NetEvent ClientConn;

        /// <summary>
        /// �ͻ��˹ر��¼�
        /// </summary>
        public event NetEvent ClientClose;

        /// <summary>
        /// �������Ѿ����¼�
        /// </summary>
        public event NetEvent ServerFull;

        /// <summary>
        /// ���������յ������¼�
        /// </summary>
        public event NetEvent RecvData;

        #endregion

        #region ���캯��

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="port">�������˼����Ķ˿ں�</param>
        /// <param name="maxClient">�����������ɿͻ��˵��������</param>
        /// <param name="encodingMothord">ͨѶ�ı��뷽ʽ</param>
        public TcpSvr(ushort port, ushort maxClient, Coder coder)
        {
            _port = port;
            _maxClient = maxClient;
            _coder = coder;
        }


        /// <summary>
        /// ���캯��(Ĭ��ʹ��Default���뷽ʽ)
        /// </summary>
        /// <param name="port">�������˼����Ķ˿ں�</param>
        /// <param name="maxClient">�����������ɿͻ��˵��������</param>
        public TcpSvr(ushort port, ushort maxClient)
        {
            _port = port;
            _maxClient = maxClient;
            _coder = new Coder(Coder.EncodingMothord.Default);
        }


        // <summary>
        /// ���캯��(Ĭ��ʹ��Default���뷽ʽ��DefaultMaxClient(100)���ͻ��˵�����)
        /// </summary>
        /// <param name="port">�������˼����Ķ˿ں�</param>
        public TcpSvr(ushort port)
            : this(port, DefaultMaxClient)
        {
        }

        #endregion

        #region ����

        /// <summary>
        /// ��������Socket����
        /// </summary>
        public Socket ServerSocket
        {
            get
            {
                return _svrSock;
            }
        }

        /// <summary>
        /// ���ݱ��ķ�����
        /// </summary>
        public DatagramResolver Resovlver
        {
            get
            {
                return _resolver;
            }
            set
            {
                _resolver = value;
            }
        }

        /// <summary>
        /// �ͻ��˻Ự����,�������еĿͻ���,������Ը���������ݽ����޸�
        /// </summary>
        public Hashtable SessionTable
        {
            get
            {
                return _sessionTable;
            }
        }

        /// <summary>
        /// �������������ɿͻ��˵��������
        /// </summary>
        public int Capacity
        {
            get
            {
                return _maxClient;
            }
        }

        /// <summary>
        /// ��ǰ�Ŀͻ���������
        /// </summary>
        public int SessionCount
        {
            get
            {
                return _clientCount;
            }
        }

        /// <summary>
        /// ����������״̬
        /// </summary>
        public bool IsRun
        {
            get
            {
                return _isRun;
            }

        }

        #endregion

        #region ���з���

        /// <summary>
        /// ��������������,��ʼ�����ͻ�������
        /// </summary>
        public virtual void Start()
        {
            if (_isRun)
            {
                throw (new ApplicationException("TcpSvr�Ѿ�������."));
            }

            _sessionTable = new Hashtable(53);

            _recvDataBuffer = new byte[DefaultBufferSize];

            //��ʼ��socket
            _svrSock = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            //�󶨶˿�
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, _port);
            _svrSock.Bind(iep);

            //��ʼ����
            _svrSock.Listen(5);

            //�����첽�������ܿͻ�������
            _svrSock.BeginAccept(new AsyncCallback(AcceptConn), _svrSock);

            _isRun = true;

        }

        /// <summary>
        /// ֹͣ����������,������ͻ��˵����ӽ��ر�
        /// </summary>
        public virtual void Stop()
        {
            if (!_isRun)
            {
                throw (new ApplicationException("TcpSvr�Ѿ�ֹͣ"));
            }

            //���������䣬һ��Ҫ�ڹر����пͻ�����ǰ����
            //������EndConn����ִ���
            _isRun = false;

            //�ر���������,����ͻ��˻���Ϊ��ǿ�ƹر�����
            if (_svrSock.Connected)
            {
                _svrSock.Shutdown(SocketShutdown.Both);
            }

            CloseAllClient();

            //������Դ
            _svrSock.Close();

            _sessionTable = null;

        }


        /// <summary>
        /// �ر����еĿͻ��˻Ự,�����еĿͻ������ӻ�Ͽ�
        /// </summary>
        public virtual void CloseAllClient()
        {
            foreach (Session client in _sessionTable.Values)
            {
                client.Close();
            }

            _sessionTable.Clear();
        }


        /// <summary>
        /// �ر�һ����ͻ���֮��ĻỰ
        /// </summary>
        /// <param name="closeClient">��Ҫ�رյĿͻ��˻Ự����</param>
        public virtual void CloseSession(Session closeClient)
        {
            Debug.Assert(closeClient != null);

            if (closeClient != null)
            {

                closeClient.Datagram = null;

                _sessionTable.Remove(closeClient.ID);

                _clientCount--;

                //�ͻ���ǿ�ƹر�����
                if (ClientClose != null)
                {
                    ClientClose(this, new NetEventArgs(closeClient));
                }

                closeClient.Close();
            }
        }


        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="recvDataClient">�������ݵĿͻ��˻Ự</param>
        /// <param name="datagram">���ݱ���</param>
        public virtual void Send(Session recvDataClient, string datagram)
        {
            //������ݱ���
            byte[] data = _coder.GetEncodingBytes(datagram);

            recvDataClient.ClientSocket.BeginSend(data, 0, data.Length, SocketFlags.None,
                new AsyncCallback(SendDataEnd), recvDataClient.ClientSocket);

        }

        #endregion

        #region �ܱ�������
        /// <summary>
        /// �ر�һ���ͻ���Socket,������Ҫ�ر�Session
        /// </summary>
        /// <param name="client">Ŀ��Socket����</param>
        /// <param name="exitType">�ͻ����˳�������</param>
        protected virtual void CloseClient(Socket client, Session.ExitType exitType)
        {
            Debug.Assert(client != null);

            //���Ҹÿͻ����Ƿ����,���������,�׳��쳣
            Session closeClient = FindSession(client);

            closeClient.TypeOfExit = exitType;

            if (closeClient != null)
            {
                CloseSession(closeClient);
            }
            else
            {
                throw (new ApplicationException("��Ҫ�رյ�Socket���󲻴���"));
            }
        }


        /// <summary>
        /// �ͻ������Ӵ�����
        /// </summary>
        /// <param name="iar">���������������ӵ�Socket����</param>
        protected virtual void AcceptConn(IAsyncResult iar)
        {
            //���������ֹͣ�˷���,�Ͳ����ٽ����µĿͻ���
            if (!_isRun)
            {
                return;
            }

            //����һ���ͻ��˵���������
            Socket oldserver = (Socket)iar.AsyncState;

            Socket client = oldserver.EndAccept(iar);

            //����Ƿ�ﵽ��������Ŀͻ�����Ŀ
            if (_clientCount == _maxClient)
            {
                //����������,����֪ͨ
                if (ServerFull != null)
                {
                    ServerFull(this, new NetEventArgs(new Session(client)));
                }

            }
            else
            {

                Session newSession = new Session(client);

                _sessionTable.Add(newSession.ID, newSession);

                //�ͻ������ü���+1
                _clientCount++;

                //��ʼ�������Ըÿͻ��˵�����
                client.BeginReceive(_recvDataBuffer, 0, _recvDataBuffer.Length, SocketFlags.None,
                    new AsyncCallback(ReceiveData), client);

                //�µĿͻ�������,����֪ͨ
                if (ClientConn != null)
                {
                    ClientConn(this, new NetEventArgs(newSession));
                }
            }

            //�������ܿͻ���
            _svrSock.BeginAccept(new AsyncCallback(AcceptConn), _svrSock);
        }


        /// <summary>
        /// ͨ��Socket�������Session����
        /// </summary>
        /// <param name="client"></param>
        /// <returns>�ҵ���Session����,���Ϊnull,˵���������ڸûػ�</returns>
        private Session FindSession(Socket client)
        {
            SessionId id = new SessionId((int)client.Handle);

            return (Session)_sessionTable[id];
        }


        /// <summary>
        /// ����������ɴ��������첽�����Ծ���������������У�
        /// �յ����ݺ󣬻��Զ�����Ϊ�ַ�������
        /// </summary>
        /// <param name="iar">Ŀ��ͻ���Socket</param>
        protected virtual void ReceiveData(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;

            try
            {
                //������ο�ʼ���첽�Ľ���,���Ե��ͻ����˳���ʱ��
                //������ִ��EndReceive

                int recv = client.EndReceive(iar);

                if (recv == 0)
                {
                    //�����Ĺر�
                    CloseClient(client, Session.ExitType.NormalExit);
                    return;
                }

                string receivedData = _coder.GetEncodingString(_recvDataBuffer, recv);

                //�����յ����ݵ��¼�
                if (RecvData != null)
                {
                    Session sendDataSession = FindSession(client);

                    Debug.Assert(sendDataSession != null);

                    //��������˱��ĵ�β���,��Ҫ�����ĵĶ������
                    if (_resolver != null)
                    {
                        if (sendDataSession.Datagram != null &&
                            sendDataSession.Datagram.Length != 0)
                        {
                            //�������һ��ͨѶʣ��ı���Ƭ��
                            receivedData = sendDataSession.Datagram + receivedData;
                        }

                        string[] recvDatagrams = _resolver.Resolve(ref receivedData);


                        foreach (string newDatagram in recvDatagrams)
                        {
                            //���,Ϊ�˱���Datagram�Ķ�����
                            ICloneable copySession = (ICloneable)sendDataSession;

                            Session clientSession = (Session)copySession.Clone();

                            clientSession.Datagram = newDatagram;
                            //����һ��������Ϣ
                            RecvData(this, new NetEventArgs(clientSession));
                        }

                        //ʣ��Ĵ���Ƭ��,�´ν��յ�ʱ��ʹ��
                        sendDataSession.Datagram = receivedData;

                        if (sendDataSession.Datagram.Length > MaxDatagramSize)
                        {
                            sendDataSession.Datagram = null;
                        }

                    }
                    //û�ж��屨�ĵ�β���,ֱ�ӽ�����Ϣ������ʹ��
                    else
                    {
                        ICloneable copySession = (ICloneable)sendDataSession;

                        Session clientSession = (Session)copySession.Clone();

                        clientSession.Datagram = receivedData;

                        RecvData(this, new NetEventArgs(clientSession));
                    }

                }//end of if(RecvData!=null)

                //���������������ͻ��˵�����
                client.BeginReceive(_recvDataBuffer, 0, _recvDataBuffer.Length, SocketFlags.None,
                    new AsyncCallback(ReceiveData), client);

            }
            catch (SocketException ex)
            {
                //�ͻ����˳�
                if (10054 == ex.ErrorCode)
                {
                    //�ͻ���ǿ�ƹر�
                    CloseClient(client, Session.ExitType.ExceptionExit);
                }

            }
            catch (ObjectDisposedException ex)
            {
                //�����ʵ�ֲ�������
                //������CloseSession()ʱ,��������ݽ���,�������ݽ���
                //�����л����int recv = client.EndReceive(iar);
                //�ͷ�����CloseSession()�Ѿ����õĶ���
                //����������ʵ�ַ���Ҳ�����˴��ŵ�.
                if (ex != null)
                {
                    ex = null;
                    //DoNothing;
                }
            }

        }


        /// <summary>
        /// ����������ɴ�����
        /// </summary>
        /// <param name="iar">Ŀ��ͻ���Socket</param>
        protected virtual void SendDataEnd(IAsyncResult iar)
        {
            Socket client = (Socket)iar.AsyncState;

            int sent = client.EndSend(iar);
        }

        #endregion

    }
}