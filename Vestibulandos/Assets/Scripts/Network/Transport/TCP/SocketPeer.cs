using System;
using Data;
using Serialization;
using Util;

namespace Network.Transport.Tcp
{
    public class SocketPeer
    {
        // ==================================================================================================
        // CONSTANT's
        // ==================================================================================================

        private const int MINIMUM_MTU_VALUE = 576;
        private const int MTU_DEFAULT = 1200;

        // ==================================================================================================
        // STATIC's
        // ==================================================================================================

        /// <summary>
        /// Outgoing Stream Buffer Size
        /// </summary>
        public static int s_nOutgoingStreamBufferSize = MTU_DEFAULT;

        // ==================================================================================================
        // VARIABLE's
        // ==================================================================================================

        private readonly object m_objSendOutgoingLockObject = new object();
        private readonly object m_objDispatchLockObject = new object();
        private readonly object m_objEnqueueLock = new object();

        /// <summary>
        /// Socket.
        /// </summary>
        internal SocketBase m_socket = null;

        /// <summary>
        /// Socket Listener.
        /// </summary>
        protected ISocketListener m_socketListener = null;

        /// <summary>
        /// Maximum Transfer Unit.
        /// </summary>
        private int m_nMTU = MTU_DEFAULT;

        /// <summary>
        /// Disconnect Timeout.
        /// </summary>
        private int m_nDisconnectTimeout = 10000;

        /// <summary>
        /// Log Level.
        /// </summary>
        public LogLevel m_enLogLevel =
#if UNITY_EDITOR || DEBUG
            LogLevel.All;
#else
            LogLevel.All;
#endif

        // ==================================================================================================
        // PROPERTY'ies
        // ==================================================================================================

        /// <summary>
        /// Get the Peer State.
        /// </summary>
        public EnPeerStateValue PeerState
        {
            get
            {
                if (m_socket.m_enConnectionState == EnConnectionStateValue.Connected && !m_socket.m_bApplicationIsInitialized)
                {
                    return EnPeerStateValue.InitializingApplication;
                }

                return (EnPeerStateValue)m_socket.m_enConnectionState;
            }
        }

        /// <summary>
        /// Get or set the Maximum Transfer Unit.
        /// </summary>
        public int MaximumTransferUnit
        {
            get => m_nMTU;
            set
            {
                if (PeerState > EnPeerStateValue.Disconnected)
                {
                    throw new Exception("MaximumTransferUnit is only settable while disconnected. State: " + PeerState);
                }

                m_nMTU = (value < MINIMUM_MTU_VALUE) ? MINIMUM_MTU_VALUE : value;
            }
        }

        /// <summary>
        /// Get the Listener.
        /// </summary>
        public ISocketListener Listener => m_socketListener;

        // ==================================================================================================
        // FUNCTION's
        // ==================================================================================================

        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        public SocketPeer()
        {
            InitializeSocket();
        }

        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="listener"></param>
        public SocketPeer(ISocketListener listener)
            : this()
        {
            m_socketListener = listener;
        }

        //----------------------------------------------------------------------------------------------------
        public virtual bool Connect(string strIP)
        {
            lock (m_objDispatchLockObject)
            {
                lock (m_objSendOutgoingLockObject)
                {
                    InitializeSocket();

                    // Connect the socket.
                    return m_socket != null &&
                        m_socket.Connect(strIP);
                }
            }
        }

        //----------------------------------------------------------------------------------------------------
        private void InitializeSocket()
        {
            // Initiliaze the socket.
            m_socket = new SocketBase();

            // Check if socket is initialize with success.
            if (m_socket == null)
            {
                throw new Exception("Failed to initialize the socket.");
            }

            // Set the socket.
            m_socket.m_socketPeer = this;
        }

        //---------------------------------------------------------------------------------------------------
        public virtual void Tick()
        {
            // Process all packets received.
            while (DispatchIncomingCommands()) { }

            // Process packets will be sended.
            while (SendOutgoingCommands()) { }
        }

        //---------------------------------------------------------------------------------------------------
        public virtual bool DispatchIncomingCommands()
        {
            lock (m_objDispatchLockObject)
            {
                m_socket.m_nByteCountCurrentDispatch = 0;

                // Dispatch the received packets.
                return m_socket.DispatchIncomingCommands();
            }
        }

        //---------------------------------------------------------------------------------------------------
        public virtual bool SendOutgoingCommands()
        {
            lock (m_objSendOutgoingLockObject)
            {
                return m_socket.SendOutgoingCommands();
            }
        }

        //---------------------------------------------------------------------------------------------------
        /// <summary>
        /// Disconnect the socket.
        /// </summary>
        public virtual void Disconnect()
        {
            lock (m_objDispatchLockObject)
            {
                lock (m_objSendOutgoingLockObject)
                {
                    // Disconnect the socket.
                    m_socket.Disconnect();
                }
            }
        }

        //---------------------------------------------------------------------------------------------------
        /// <summary>
        /// Send the packet.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="packet"></param>
        /// <param name="bCompress"></param>
        /// <returns></returns>
        public virtual bool Send<T>(T packet, bool bCompress)
            where T : IPacket
        {
            lock (m_objEnqueueLock)
            {
                // Check if socket is not null.
                if (m_socket == null)
                {
                    LoggerHelper.LogError("Socket is null.");
                    return false;
                }

                // Enqueue the packet.
                return m_socket.EnqueuePacket(packet, bCompress);
            }
        }
    }
}
