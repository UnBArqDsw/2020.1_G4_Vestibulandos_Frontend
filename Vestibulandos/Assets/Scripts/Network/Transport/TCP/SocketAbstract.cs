using Data;
using Network.Security;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Threading;

namespace Network.Transport.Tcp
{
    public class SocketAbstract : IDisposable
    {
        // ==================================================================================================
        // VARIABLE's
        // ==================================================================================================

        /// <summary>
        /// Socket base.
        /// </summary>
        protected SocketBase m_socketBase = null;

        /// <summary>
        /// Pool Receive.
        /// </summary>
        public bool m_bPollReceive = false;

        /// <summary>
        /// Socket.
        /// </summary>
        private System.Net.Sockets.Socket m_socket = null;

        /// <summary>
        /// Lock.
        /// </summary>
        private readonly object m_lock = new object();

        // ==================================================================================================
        // PROPERTY'ies
        // ==================================================================================================

        /// <summary>
        /// Get the Listener.
        /// </summary>
        protected ISocketListener Listener => m_socketBase.Listener;

        /// <summary>
        /// Get the MTU.
        /// </summary>
        protected internal int MTU => m_socketBase.MTU;

        /// <summary>
        /// Get the current state.
        /// </summary>
        public EnSocketState State { get; protected set; }

        /// <summary>
        /// Check if current state is connected.
        /// </summary>
        public bool Connected => State == EnSocketState.Connected;

        /// <summary>
        /// Get the address connected.
        /// </summary>
        public string ConnectAddress => m_socketBase.ServerAddress;

        /// <summary>
        /// Server Address.
        /// </summary>
        public string ServerAddress { get; protected set; }

        /// <summary>
        /// Server IP Address.
        /// </summary>
        public static string ServerIpAddress { get; private set; }

        /// <summary>
        /// Server Port.
        /// </summary>
        public int ServerPort { get; protected set; }

        /// <summary>
        /// Ipv6.
        /// </summary>
        public bool AddressResolvedAsIpv6 { get; protected internal set; }

        /// <summary>
        /// Url protocol.
        /// </summary>
        public string UrlProtocol { get; protected set; }

        /// <summary>
        /// Url path.
        /// </summary>
        public string UrlPath { get; protected set; }

        // ==================================================================================================
        // FUNCTION's
        // ==================================================================================================

        //---------------------------------------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="socketBase"></param>
        internal SocketAbstract(SocketBase socketBase)
        {
            if (socketBase == null)
            {
                throw new Exception("Can't init without peer");
            }

            m_socketBase = socketBase;

            if (ReportDebugOfLevel(LogLevel.All))
            {
                Listener.NetworkLog(LogLevel.All, "SocketTcp: TCP, DotNet, Unity.");
            }

            m_bPollReceive = false;
        }

        //---------------------------------------------------------------------------------------------------
        public void Dispose()
        {
            State = EnSocketState.Disconnecting;

            if (m_socket != null)
            {
                try
                {
                    if (m_socket.Connected)
                    {
                        m_socket.Close();
                    }
                }
                catch (Exception ex)
                {
                    EnqueueDebugReturn(LogLevel.Info, "Exception in Dispose(): " + ex);
                }
            }

            m_socket = null;
            State = EnSocketState.Disconnected;
        }

        //---------------------------------------------------------------------------------------------------
        public virtual bool Connect()
        {
            if (State > EnSocketState.Disconnected)
            {
                if (m_socketBase.m_enLogLevel >= LogLevel.Error)
                {
                    m_socketBase.Listener.NetworkLog(LogLevel.Error, "Connect() failed: connection in State: " + State);
                }

                return false;
            }

            if (!TryParseAddress(m_socketBase.ServerAddress, out string strServerAddress, out ushort nServerPort, out string strUrlProtocol, out string strUrlPath))
            {
                if (m_socketBase.m_enLogLevel >= LogLevel.Error)
                {
                    m_socketBase.Listener.NetworkLog(LogLevel.Error, "Failed parsing address: " + m_socketBase.ServerAddress);
                }

                return false;
            }

            ServerIpAddress = "";

            ServerAddress = strServerAddress;
            ServerPort = nServerPort;
            UrlProtocol = strUrlProtocol;
            UrlPath = strUrlPath;

            if (m_socketBase.m_enLogLevel >= LogLevel.All)
            {
                Listener.NetworkLog(LogLevel.All, string.Concat("ISocket.Connect() ", ServerAddress, ":", ServerPort));
            }

            State = EnSocketState.Connecting;
            new Thread(DnsAndConnect)
            {
                IsBackground = true
            }.Start();

            return true;
        }

        //---------------------------------------------------------------------------------------------------
        public bool Disconnect()
        {
            if (ReportDebugOfLevel(LogLevel.Info))
            {
                EnqueueDebugReturn(LogLevel.Info, "SocketTcp.Disconnect()");
            }

            State = EnSocketState.Disconnecting;

            lock (m_lock)
            {
                if (m_socket != null)
                {
                    try
                    {
                        m_socket.Close();
                    }
                    catch (Exception ex)
                    {
                        EnqueueDebugReturn(LogLevel.Info, "Exception in Disconnect(): " + ex);
                    }

                    m_socket = null;
                }
            }

            State = EnSocketState.Disconnected;
            return true;
        }

        //---------------------------------------------------------------------------------------------------
        public EnSocketError Send(byte[] arrData, int nLength)
        {
            if (m_socket == null || !m_socket.Connected)
                return EnSocketError.Skipped;

            try
            {
                m_socket.Send(arrData, 0, nLength, SocketFlags.None);
            }
            catch (Exception ex)
            {
                if (State != EnSocketState.Disconnecting && State > EnSocketState.Disconnected)
                {
                    if (ReportDebugOfLevel(LogLevel.Error))
                    {
                        string str = "";
                        if (m_socket != null)
                        {
                            str =
                                $" Local: {m_socket.LocalEndPoint} Remote: {m_socket.RemoteEndPoint} ({(m_socket.Connected ? "connected" : "not connected")}, " +
                                $"{(m_socket.IsBound ? "bound" : "not bound")})";
                        }

                        EnqueueDebugReturn(LogLevel.Error,
                            $"Cannot send to: {ServerAddress}. Uptime: {(AddressResolvedAsIpv6 ? " IPv6" : "")} ms. {str} {ex}");
                    }

                    HandleException(EnStatusCode.Exception);
                }

                return EnSocketError.Exception;
            }

            return EnSocketError.Success;
        }

        //---------------------------------------------------------------------------------------------------
        public EnSocketError Receive(out byte[] arrData)
        {
            arrData = null;
            return EnSocketError.NoData;
        }

        //---------------------------------------------------------------------------------------------------
        public void DnsAndConnect()
        {
            try
            {
                IPAddress ipAddress = GetIpAddress(ServerAddress);
                if (ipAddress == null)
                {
                    throw new ArgumentException("Invalid IPAddress. Address: " + ServerAddress);
                }

                m_socket = new System.Net.Sockets.Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                {
                    NoDelay = true,
                    //ReceiveTimeout = this.SocketBase.DisconnectTimeout,
                    //SendTimeout = this.SocketBase.DisconnectTimeout
                };

                m_socket.Connect(ipAddress, ServerPort);
                AddressResolvedAsIpv6 = IsIpv6SimpleCheck(ipAddress);
                State = EnSocketState.Connected;

                m_socketBase.OnConnect();
            }
            catch (SecurityException ex)
            {
                if (ReportDebugOfLevel(LogLevel.Error))
                {
                    Listener.NetworkLog(LogLevel.Error, "Connect() to '" + ServerAddress + "' failed: " + ex);
                }

                HandleException(EnStatusCode.SecurityExceptionOnConnect);
                return;
            }
            catch (Exception ex)
            {
                if (ReportDebugOfLevel(LogLevel.Error))
                {
                    Listener.NetworkLog(LogLevel.Error, "Connect() to '" + ServerAddress + "' failed: " + ex);
                }

                HandleException(EnStatusCode.ExceptionOnConnect);
                return;
            }

            new Thread(ReceiveLoop)
            {
                IsBackground = true
            }.Start();
        }

        //---------------------------------------------------------------------------------------------------
        public void ReceiveLoop()
        {
            StreamBuffer buffer = new StreamBuffer(MTU);
            byte[] arrPacketHeader = new byte[SecurityAssociation.PACKET_LENGTH_HEADER];

            while (State == EnSocketState.Connected)
            {
                buffer.SetLength(0);

                try
                {
                    int packetReadLeft = 0;
                    int packetLenData = 0;

                    while (packetReadLeft < SecurityAssociation.PACKET_LENGTH_HEADER)
                    {
                        try
                        {
                            packetLenData = m_socket.Receive(arrPacketHeader, packetReadLeft, SecurityAssociation.PACKET_LENGTH_HEADER - packetReadLeft, SocketFlags.None);
                        }
                        catch (SocketException ex)
                        {
                            if (State != EnSocketState.Disconnecting && State > EnSocketState.Disconnected &&
                                ex.SocketErrorCode == System.Net.Sockets.SocketError.WouldBlock)
                            {
                                if (ReportDebugOfLevel(LogLevel.All))
                                {
                                    EnqueueDebugReturn(LogLevel.All, "ReceiveLoop() got a WouldBlock exception. This is non-fatal. Going to continue.");
                                }

                                continue;
                            }

                            throw;
                        }

                        packetReadLeft += packetLenData;
                        if (packetLenData == 0)
                        {
                            throw new SocketException((int)System.Net.Sockets.SocketError.ConnectionReset);
                        }
                    }

                    // Get packet length.
                    int packetLen = arrPacketHeader[1] << 8 | arrPacketHeader[0];

                    buffer.SetCapacityMinimum(packetLen - SecurityAssociation.PACKET_LENGTH_HEADER);
                    //streamBuffer.Write(array, 0, i - 2);
                    packetReadLeft = 0;
                    packetLen -= SecurityAssociation.PACKET_LENGTH_HEADER;

                    while (packetReadLeft < packetLen)
                    {
                        try
                        {
                            packetLenData = m_socket.Receive(buffer.GetBuffer(), buffer.Position, packetLen - packetReadLeft, SocketFlags.None);
                        }
                        catch (SocketException ex)
                        {
                            if (State != EnSocketState.Disconnecting && State > EnSocketState.Disconnected &&
                                ex.SocketErrorCode == System.Net.Sockets.SocketError.WouldBlock)
                            {
                                if (ReportDebugOfLevel(LogLevel.All))
                                {
                                    EnqueueDebugReturn(LogLevel.All, "ReceiveLoop() got a WouldBlock exception. This is non-fatal. Going to continue.");
                                }

                                continue;
                            }
                            throw;
                        }

                        buffer.Position += packetLenData;
                        packetReadLeft += packetLenData;

                        if (packetLenData == 0)
                        {
                            throw new SocketException((int)System.Net.Sockets.SocketError.ConnectionReset);
                        }
                    }

                    HandleReceivedPacket(buffer.ToArray(), buffer.Length, false);
                }
                catch (SocketException ex)
                {
                    if (State != EnSocketState.Disconnecting && State > EnSocketState.Disconnected)
                    {
                        if (ReportDebugOfLevel(LogLevel.Error))
                        {
                            EnqueueDebugReturn(LogLevel.Error,
                                "Receiving failed. SocketException: " + ex.SocketErrorCode);
                        }

                        if (ex.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionReset ||
                            ex.SocketErrorCode == System.Net.Sockets.SocketError.ConnectionAborted)
                        {
                            HandleException(EnStatusCode.DisconnectByServer);
                        }
                        else
                        {
                            HandleException(EnStatusCode.ExceptionOnReceive);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (State != EnSocketState.Disconnecting && State > EnSocketState.Disconnected)
                    {
                        if (ReportDebugOfLevel(LogLevel.Error))
                        {
                            EnqueueDebugReturn(LogLevel.Error, string.Concat("Receive issue. State: ",
                                State, ". Server: '", ServerAddress, "' Exception: ", ex));
                        }

                        HandleException(EnStatusCode.ExceptionOnReceive);
                    }
                }
            }

            Disconnect();
        }

        //---------------------------------------------------------------------------------------------------
        public void HandleReceivedPacket(byte[] arrInBuffer, int nLength, bool bWillBeReused)
        {
            m_socketBase.ReceiveIncomingCommands(arrInBuffer, nLength);
        }

        //---------------------------------------------------------------------------------------------------
        public bool ReportDebugOfLevel(LogLevel enDebugLevel)
        {
            return m_socketBase.m_enLogLevel >= enDebugLevel;
        }

        //---------------------------------------------------------------------------------------------------
        public void EnqueueDebugReturn(LogLevel enDebugLevel, string strMessage)
        {
            m_socketBase.EnqueueDebugReturn(enDebugLevel, strMessage);
        }

        //---------------------------------------------------------------------------------------------------
        protected internal void HandleException(EnStatusCode enStatusCode)
        {
            State = EnSocketState.Disconnecting;

            m_socketBase.EnqueueStatusCallback(enStatusCode);
            m_socketBase.EnqueueActionForDispatch(() => m_socketBase.Disconnect());
        }

        //---------------------------------------------------------------------------------------------------
        protected internal bool TryParseAddress(string strUrl, out string strAddress, out ushort nPort, out string strUrlProtocol, out string strUrlPath)
        {
            strAddress = "";
            nPort = 0;

            strUrlProtocol = "";
            strUrlPath = "";

            string strTmpUrl = strUrl;

            if (string.IsNullOrEmpty(strTmpUrl))
            {
                return false;
            }

            int nSubUrl = strTmpUrl.IndexOf("://", StringComparison.Ordinal);
            if (nSubUrl >= 0)
            {
                strUrlProtocol = strTmpUrl.Substring(0, nSubUrl);
                strTmpUrl = strTmpUrl.Substring(nSubUrl + 3);
            }

            nSubUrl = strTmpUrl.IndexOf("/", StringComparison.Ordinal);
            if (nSubUrl >= 0)
            {
                strUrlPath = strTmpUrl.Substring(nSubUrl);
                strTmpUrl = strTmpUrl.Substring(0, nSubUrl);
            }

            nSubUrl = strTmpUrl.LastIndexOf(':');
            if (nSubUrl < 0)
                return false;

            if (strTmpUrl.IndexOf(':') != nSubUrl && (!strTmpUrl.Contains("[") || !strTmpUrl.Contains("]")))
                return false;

            strAddress = strTmpUrl.Substring(0, nSubUrl);
            return ushort.TryParse(strTmpUrl.Substring(nSubUrl + 1), out nPort);
        }

        //---------------------------------------------------------------------------------------------------
        protected internal bool IsIpv6SimpleCheck(IPAddress address)
        {
            return address != null && address.ToString().Contains(":");
        }

        //---------------------------------------------------------------------------------------------------
        protected internal static IPAddress GetIpAddress(string strAddress)
        {
            if (IPAddress.TryParse(strAddress, out IPAddress ipaddress))
                return ipaddress;

            IPHostEntry hostEntry = Dns.GetHostEntry(strAddress);
            IPAddress[] addressList = hostEntry.AddressList;

            foreach (IPAddress ip in addressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    ServerIpAddress = ip.ToString();
                    return ip;
                }

                if (ipaddress == null && ip.AddressFamily == AddressFamily.InterNetwork)
                    ipaddress = ip;
            }

            ServerIpAddress = ((ipaddress != null) ? ipaddress.ToString() : (strAddress + " not resolved"));
            return ipaddress;
        }
    }
}