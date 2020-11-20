using Data;
using Network.Transport.Tcp;
using Serialization;
using Util;

namespace Network.Session
{
    /// <summary>
    /// Session class.
    /// </summary>
    public abstract class Session : ISocketListener
    {
        /// <summary>
        /// Socket.
        /// </summary>
        protected SocketPeer m_socket = null;

        /// <summary>
        /// Get the socket.
        /// </summary>
        public SocketPeer Socket => m_socket;

        /// <summary>
        /// Current Status of the Session.
        /// </summary>
        protected EnSessionStatus m_enStatus = EnSessionStatus.Closed;

        //----------------------------------------------------------------------------------------------------
        public void Initialize(string strIpAddress, int nMaximumTransferUnit = 8192)
        {
            // If socket is not null, it's necessary to disconnect old socket.
            if (m_socket != null)
            {
                // Disconnect the socket.
                Disconnect();
            }

            LoggerHelper.Log($"Session.Init() -> Connecting in the IP: {strIpAddress}");

            // Initialize the socket.
            m_socket = new SocketPeer(this)
            {
                MaximumTransferUnit = nMaximumTransferUnit,
                //DisconnectTimeout = 30000
            };

            // Connect the socket.
            m_socket.Connect(strIpAddress);

            // Change the status.
            m_enStatus = EnSessionStatus.Connecting;
        }

        //----------------------------------------------------------------------------------------------------
        public void Disconnect()
        {
            // If socket is not null, disconnect and set how null.
            if (m_socket != null)
            {
                // Disconnect the socket.
                m_socket.Disconnect();

                // Clear the socket.
                m_socket = null;
            }

            // Update the status to closed.
            m_enStatus = EnSessionStatus.Closed;

            // Call the function.
            OnDisconnectedByClient();
        }

        //----------------------------------------------------------------------------------------------------
        public virtual void Tick()
        {
            // Tick the socket.
            if (m_socket != null)
            {
                m_socket.Tick();
            }
        }

        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Check if session is connected.
        /// </summary>
        /// <returns>True if it's connected.</returns>
        public bool IsConnected() => EnSessionStatus.Connected == m_enStatus;

        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Check if session is connecting.
        /// </summary>
        /// <returns>True if it's connecting.</returns>
        public bool IsConnecting() => EnSessionStatus.Connecting == m_enStatus;

        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Check if session is closed.
        /// </summary>
        /// <returns>True if it's closed.</returns>
        public bool IsClosed() => EnSessionStatus.Closed == m_enStatus;

        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Check if socket of the session is initialized.
        /// </summary>
        /// <returns>True if it's initialied.</returns>
        public bool HasSocket() => m_socket != null;

        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Send the packet.
        /// </summary>
        /// <param name="packet">Packet data.</param>
        /// <param name="bIsCompress">Compress the packet.</param>
        /// <returns>Return true if packet is sended without any problem.</returns>
        public bool Send(IPacket packet, bool bIsCompress = false)
        {
#if UNITY_EDITOR && DEBUG
            LoggerHelper.Log($"Session.: Sending packet ID: <color=green>{packet.GetType().Name}.</color>");
#endif

            // Check if socket is not null and if session is connected.
            if (IsClosed() || m_socket == null)
            {
                LoggerHelper.LogError($"Socket is not connected. Current status: {m_enStatus}.");
                return false;
            }

            // Send the packet.
            return m_socket.Send(packet, bIsCompress);
        }

        //----------------------------------------------------------------------------------------------------
        public void OnAcceptConnection()
        {
            // 1. Invoked when the client's connection is successful.
            // 2. Creates a SecurityAssociation object for packet security here (one per Session)
            // 3. It is intended to synchronize security authentication by sending SecurityAssociation object information to the client.

            // Ignore if client is not connected or not received yet auth key from server.
            if (!m_socket.m_socket.AuthKeyRecved)
            {
                LoggerHelper.LogError("Auth key not received yet!");
                return;
            }

            // Send SA information before to update SPI.
            SendAcceptConnection(m_socket.m_socket.SPIndex, m_socket.m_socket.SecurityAssociation.GetAuthKey().Buffer, m_socket.m_socket.SecurityAssociation.GetCryptoKey().Buffer,
                m_socket.m_socket.SecurityAssociation.GetSequenceNum(), m_socket.m_socket.SecurityAssociation.LastSequenceNum, m_socket.m_socket.SecurityAssociation.ReplayWindowMask);
        }

        //----------------------------------------------------------------------------------------------------
        protected abstract void SendAcceptConnection(ushort usSPI, byte[] arrAuthKey, byte[] arrCryptoKey,
            uint uiSequenceNum, uint uiLastSequenceNum, uint uiReplayWindowMask);

        //----------------------------------------------------------------------------------------------------

        #region Implementation of ISocketListener

        //----------------------------------------------------------------------------------------------------
        public void NetworkLog(LogLevel enLevel, string strMsg)
        {
            if (enLevel == LogLevel.Error)
            {
                LoggerHelper.Log($"<color=red>[{enLevel}]: {strMsg}</color>");
            }
            else if (enLevel == LogLevel.Warning)
            {
                LoggerHelper.Log($"<color=yellow> [{enLevel}]: {strMsg} </color>");
            }
            else
            {
                LoggerHelper.Log($"[{enLevel}]: {strMsg}");
            }
        }

        //----------------------------------------------------------------------------------------------------
        public abstract void OnPacketReceived(IPacket packet);

        //----------------------------------------------------------------------------------------------------
        public void OnStatusChanged(EnStatusCode enStatusCode)
        {
            switch (enStatusCode)
            {
                case EnStatusCode.Connect:
                    {
                        OnAcceptConnection();
                    }
                    break;
                case EnStatusCode.Disconnect:
                case EnStatusCode.DisconnectByServer:
                case EnStatusCode.DisconnectByServerLogic:
                case EnStatusCode.DisconnectByServerUserLimit:
                case EnStatusCode.Exception:
                case EnStatusCode.ExceptionOnConnect:
                case EnStatusCode.TimeoutDisconnect:
                    {
                        if (IsConnected() || IsConnecting())
                        {
                            m_enStatus = EnSessionStatus.Closed;
                            m_socket.Disconnect();
                            m_socket = null;

                            LoggerHelper.Log(enStatusCode.ToString());
                            OnDisconnected(enStatusCode);
                        }
                    }
                    break;
                case EnStatusCode.ConnectionEstablished:
                    {
                        m_enStatus = EnSessionStatus.Connected;

                        LoggerHelper.Log("OnConnected");
                        OnConnected();
                    }
                    break;
                default:
                    {
                        Disconnect();
                        OnDisconnected(enStatusCode);
                    }
                    break;
            }
        }

        #endregion

        //----------------------------------------------------------------------------------------------------
        protected abstract void OnConnected();

        //----------------------------------------------------------------------------------------------------
        protected abstract void OnDisconnected(EnStatusCode enStatusCode);

        //----------------------------------------------------------------------------------------------------
        protected abstract void OnDisconnectedByClient();
    }
}
