using System;
using System.Collections.Generic;
using Data;
using MessagePack;
using Network.Security;
using Serialization;
using Util;

namespace Network.Transport.Tcp
{
    public class SocketBase
    {
        /// <summary>
        /// Delegate.
        /// </summary>
        internal delegate void SocketAction();

        // ==================================================================================================
        // CONSTANT's
        // ==================================================================================================

        /// <summary>
        /// Packet count. Used in pool.
        /// </summary>
        public const int PACKET_COUNT = 32;

        // ==================================================================================================
        // STATIC's
        // ==================================================================================================

        #region MessagePack

        /// <summary>
        /// MessagePack Options.
        /// </summary>
        private static readonly MessagePackSerializerOptions s_messagePackOptions =
            MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

        #endregion MessagePack

        /// <summary>
        /// Queue Message Buffer (Pool).
        /// </summary>
        protected internal static Queue<StreamBuffer> s_queueMessageBufferPool = new Queue<StreamBuffer>(PACKET_COUNT);

        // ==================================================================================================
        // VARIABLE's
        // ==================================================================================================

        /// <summary>
        /// Security Association.
        /// </summary>
        protected SecurityAssociation m_securityAssociation = null;

        /// <summary>
        /// SPIndex.
        /// </summary>
        protected ushort m_usSPIndex = 0;

        /// <summary>
        /// Variable that verifies that the security key has been received.
        /// </summary>
        protected bool m_bAuthKeyRecved = false;

        /// <summary>
        /// Socket Peer.
        /// </summary>
        internal SocketPeer m_socketPeer = null;

        /// <summary>
        /// Socket.
        /// </summary>
        internal SocketAbstract m_socket = null;

        /// <summary>
        /// Connection state.
        /// </summary>
        internal EnConnectionStateValue m_enConnectionState = EnConnectionStateValue.Disconnected;

        /// <summary>
        /// Byte Count Last Operation.
        /// </summary>
        internal int m_nByteCountLastOperation = 0;

        /// <summary>
        /// Byte Count Current Dispatch.
        /// </summary>
        internal int m_nByteCountCurrentDispatch = 0;
        
        /// <summary>
        /// Queue action.
        /// </summary>
        internal readonly Queue<SocketAction> m_queueAction = new Queue<SocketAction>();

        /// <summary>
        /// Queue incoming.
        /// </summary>
        private Queue<byte[]> m_queueIncoming = new Queue<byte[]>(PACKET_COUNT);

        /// <summary>
        /// List of outgoing stream.
        /// </summary>
        internal List<StreamBuffer> m_listOutgoingStream = null;

        /// <summary>
        /// Application is initialized.
        /// </summary>
        internal bool m_bApplicationIsInitialized = false;

        /// <summary>
        /// Outgoing commands in stream.
        /// </summary>
        internal int m_nOutgoingCommandsInStream = 0;

        // ==================================================================================================
        // PROPERTY'ies
        // ==================================================================================================

        /// <summary>
        /// Get the Listener.
        /// </summary>
        internal ISocketListener Listener => m_socketPeer.Listener;

        /// <summary>
        /// Get the SecurityAssociation.
        /// </summary>
        public SecurityAssociation SecurityAssociation => m_securityAssociation;

        /// <summary>
        /// Get the SPIndex.
        /// </summary>
        public ushort SPIndex => m_usSPIndex;

        /// <summary>
        /// Check if received the auth key.
        /// </summary>
        public bool AuthKeyRecved => m_bAuthKeyRecved;

        /// <summary>
        /// Server Address.
        /// </summary>
        public string ServerAddress { get; internal set; }

        /// <summary>
        /// Get the Log Level.
        /// </summary>
        internal LogLevel m_enLogLevel => m_socketPeer.m_enLogLevel;

        /// <summary>
        /// Get the MTU.
        /// </summary>
        internal int MTU => m_socketPeer.MaximumTransferUnit;

        // ==================================================================================================
        // FUNCTION's
        // ==================================================================================================

        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        public SocketBase()
        {
            // Init the Security Association.
            m_securityAssociation = new SecurityAssociation();
        }

        //----------------------------------------------------------------------------------------------------
        public static StreamBuffer MessageBufferPoolGet()
        {
            lock (s_queueMessageBufferPool)
            {
                return s_queueMessageBufferPool.Count > 0 ? s_queueMessageBufferPool.Dequeue() : new StreamBuffer();
            }
        }

        //----------------------------------------------------------------------------------------------------
        public static void MessageBufferPoolPut(StreamBuffer buff)
        {
            lock (s_queueMessageBufferPool)
            {
                buff.Position = 0;
                buff.SetLength(0);

                s_queueMessageBufferPool.Enqueue(buff);
            }
        }

        //----------------------------------------------------------------------------------------------------
        internal virtual void Initialize()
        {
            m_nByteCountLastOperation = 0;
            m_nByteCountCurrentDispatch = 0;

            m_enConnectionState = EnConnectionStateValue.Disconnected;
            m_bApplicationIsInitialized = false;
        }

        //----------------------------------------------------------------------------------------------------
        internal bool Connect(string strServerAddress)
        {
            if (m_enConnectionState > EnConnectionStateValue.Disconnected)
            {
                Listener.NetworkLog(LogLevel.Warning, "Connect() can't be called if peer is not Disconnected. Not connecting.");
                return false;
            }

            if (m_enLogLevel >= LogLevel.All)
            {
                Listener.NetworkLog(LogLevel.All, "Connect()");
            }

            // Set the server address.
            ServerAddress = strServerAddress;

            Initialize();

            // Initialize the list.
            m_listOutgoingStream = new List<StreamBuffer>();

            // Intialize the socket.
            m_socket = new SocketAbstract(this);

            // Check if socket it's initialized with success.
            if (m_socket == null)
            {
                Listener.NetworkLog(LogLevel.Error, "Connect() failed, because SocketImplementation or socket was null. Set SocketPeer.SocketImplementation before Connect().");
                return false;
            }

            // Connect the socket.
            if (!m_socket.Connect())
            {
                // Failed to connect.
                m_enConnectionState = EnConnectionStateValue.Disconnected;
                return false;
            }

            // Connected with success.
            m_enConnectionState = EnConnectionStateValue.Connecting;
            return true;
        }

        //----------------------------------------------------------------------------------------------------
        public void OnConnect()
        {
            SendOutgoingCommands();
        }

        //----------------------------------------------------------------------------------------------------
        internal void Disconnect()
        {
            // Ignore if state is already disconnecting or disconnected.
            if (m_enConnectionState == EnConnectionStateValue.Disconnected ||
                m_enConnectionState == EnConnectionStateValue.Disconnecting)
            {
                return;
            }

            if (m_enLogLevel >= LogLevel.All)
            {
                Listener.NetworkLog(LogLevel.All, "Socket.Disconnect()");
            }

            StopConnection();
        }

        //----------------------------------------------------------------------------------------------------
        internal void StopConnection()
        {
            // Update the connection state.
            m_enConnectionState = EnConnectionStateValue.Disconnecting;

            // Disconnect the socket.
            m_socket?.Disconnect();

            // Clear the incoming packets.
            lock (m_queueIncoming)
            {
                m_queueIncoming.Clear();
            }

            // Update the connection state.
            m_enConnectionState = EnConnectionStateValue.Disconnected;
            EnqueueStatusCallback(EnStatusCode.Disconnect);
        }

        //----------------------------------------------------------------------------------------------------
        internal bool EnqueuePacket<T>(T packet, bool bCompress)
            where T : IPacket
        {
            if (m_enConnectionState != EnConnectionStateValue.Connected)
            {
                if (m_enLogLevel >= LogLevel.Error)
                {
                    Listener.NetworkLog(LogLevel.Error, "Cannot send message! Not connected. PeerState: " + m_enConnectionState);
                }

                Listener.OnStatusChanged(EnStatusCode.SendError);
                return false;
            }

            return EnqueuePacketAsPayload(SerializePacket(packet, bCompress));
        }

        //---------------------------------------------------------------------------------------------------
        internal bool EnqueuePacketAsPayload(StreamBuffer stream)
        {
            // Check if stream is invalid.
            if (stream == null)
            {
                LoggerHelper.LogError("Received a null stream.");
                return false;
            }

            lock (m_listOutgoingStream)
            {
                m_listOutgoingStream.Add(stream);
                m_nOutgoingCommandsInStream++;
            }

            m_nByteCountLastOperation = stream.Length;
            return true;
        }

        //----------------------------------------------------------------------------------------------------
        internal StreamBuffer SerializePacket<T>(T packet, bool bCompress) where T : IPacket
        {
            // Get the stream from pool.
            StreamBuffer streamBuffer = MessageBufferPoolGet();
            streamBuffer.SetLength(0);

            // Buffer.
            byte[] arrBuffer = null;

            try
            {
                // Serialize the packet.
                arrBuffer = (bCompress) ?
                    MessagePackSerializer.Serialize<IPacket>(packet, s_messagePackOptions) : // compress the packet with lz4.
                    MessagePackSerializer.Serialize<IPacket>(packet);   // normal packet without compress.
            }
            catch (MessagePackSerializationException e)
            {
                // Exception.
                LoggerHelper.LogError($"Failed to serialized the packet. Packet {packet.OpCode} | Exception: {e.Message}");

                // Failed to serialize.
                return null;
            }

            // Convert packet to Secure Buffer.
            ByteStream kbuff_ = new ByteStream(arrBuffer);
            SecureBuffer kSecBuff = new SecureBuffer(m_usSPIndex, m_securityAssociation);
            kSecBuff.Create(kbuff_);

            ByteStream bsbuff = new ByteStream((ulong)(kSecBuff.GetSize + SecurityAssociation.PACKET_SPI_HEADER + SecurityAssociation.PACKET_COMPRESS_HEADER));
            // Set the packet length total.
            bsbuff.Append(BitConverter.GetBytes((ushort)(kSecBuff.GetSize + SecurityAssociation.PACKET_SPI_HEADER + SecurityAssociation.PACKET_COMPRESS_HEADER)),
                0, SecurityAssociation.PACKET_SPI_HEADER);

            // Set the packet is compressed.
            bsbuff.Append(BitConverter.GetBytes(bCompress), SecurityAssociation.PACKET_SPI_HEADER, SecurityAssociation.PACKET_COMPRESS_HEADER);

            // Set the packet data.
            bsbuff.Append(kSecBuff.GetData, SecurityAssociation.PACKET_SPI_HEADER + SecurityAssociation.PACKET_COMPRESS_HEADER, kSecBuff.GetSize);

            streamBuffer.Write(bsbuff.Buffer, 0, bsbuff.Length);

            return streamBuffer;
        }

        //----------------------------------------------------------------------------------------------------
        internal bool SendOutgoingCommands()
        {
            //Check if connection is not disconnected.
            if (m_enConnectionState == EnConnectionStateValue.Disconnected)
            {
                return false;
            }

            // Check if socket is still connected.
            if (!m_socket.Connected)
            {
                return false;
            }

            lock (m_listOutgoingStream)
            {
                for (int nIndex = 0; nIndex < m_listOutgoingStream.Count; nIndex++)
                {
                    StreamBuffer streamBuffer = m_listOutgoingStream[nIndex];

                    try
                    {
                        m_socket.Send(streamBuffer.GetBuffer(), streamBuffer.Length);
                    }
                    catch (Exception ex)
                    {
                        if (m_enLogLevel >= LogLevel.Error)
                        {
                            Listener.NetworkLog(LogLevel.Error, ex.ToString());
                        }
                    }

                    MessageBufferPoolPut(streamBuffer);
                }

                m_listOutgoingStream.Clear();
                m_nOutgoingCommandsInStream = 0;

                return false;
            }
        }

        //----------------------------------------------------------------------------------------------------
        internal void ReceiveIncomingCommands(byte[] arrInBuffer, int nDataLength)
        {
            if (arrInBuffer == null)
            {
                if (m_enLogLevel >= LogLevel.Error)
                {
                    EnqueueDebugReturn(LogLevel.Error, "checkAndQueueIncomingCommands() inBuff: null");
                }
            }
            else
            {
                byte[] arrBuffer = new byte[nDataLength];
                Buffer.BlockCopy(arrInBuffer, 0, arrBuffer, 0, nDataLength);

                lock (m_queueIncoming)
                {
                    m_queueIncoming.Enqueue(arrBuffer);
                }
            }
        }

        //----------------------------------------------------------------------------------------------------
        internal bool DispatchIncomingCommands()
        {
            for (; ; )
            {
                SocketAction action = null;

                lock (m_queueAction)
                {
                    // Check if has some action for be processed.
                    if (m_queueAction.Count <= 0)
                        break;

                    // Dequeue the action.
                    action = m_queueAction.Dequeue();
                }

                // Execute the action.
                action();
            }

            // Temporary buffer.
            byte[] arrBuffer = null;

            lock (m_queueIncoming)
            {
                // Check if has some incoming packet for be processed.
                if (m_queueIncoming.Count <= 0)
                    return false;

                // Dequeue the packet.
                arrBuffer = m_queueIncoming.Dequeue();
            }

            // Update the statics.
            m_nByteCountCurrentDispatch = arrBuffer.Length + SecurityAssociation.PACKET_LENGTH_HEADER;

            // Deserialize the packet.
            return DeserializePacket(arrBuffer);
        }

        //----------------------------------------------------------------------------------------------------
        internal virtual bool DeserializePacket(byte[] arrBuffer)
        {
            ReadOnlySpan<byte> spanPacket = new ReadOnlySpan<byte>(arrBuffer);

            // Get if packet is compressed.
            bool bIsCompress = spanPacket.Slice(0, SecurityAssociation.PACKET_COMPRESS_HEADER)[0] != 0;

            // Only the data except the length is buffered.
            ReadOnlySpan<byte> spanPacketBuffer =
                spanPacket.Slice(SecurityAssociation.PACKET_COMPRESS_HEADER);

            // If a DOS attack comes in, the server will be dangerous because it will print the error log as much as the attack amount.
            SecureBuffer kSecBuf = new SecureBuffer(m_usSPIndex, spanPacketBuffer.ToArray(), (ulong)spanPacketBuffer.ToArray().Length, m_securityAssociation);
            if (!kSecBuf.IsAuthentic())
            {
                LoggerHelper.LogError("Packet authentication failed.");
                return false;
            }

            // After finishing the authentication and decryption process, only pure payload is obtained.
            ByteStream payload = null;

            // PS: Since you've already called IsAuthentic(), the second argument is false!
            if (kSecBuf.GetPayload(ref payload) && payload != null)
            {
                // Packet.
                IPacket packet = null;

                try
                {
                    // Deserialize the packet.
                    packet = (bIsCompress) ?
                        MessagePackSerializer.Deserialize<IPacket>(payload.Buffer, s_messagePackOptions) : // Decompress the packet.
                        MessagePackSerializer.Deserialize<IPacket>(payload.Buffer);
                }
                catch (MessagePackSerializationException e)
                {
                    // Exception.
                    LoggerHelper.LogError(e.Message);
                }

                switch (packet.OpCode)
                {
                    case (int)OPCODE_LU.LU_ACCEPT_CONNECTION:
                    case (int)OPCODE_GU.GU_ACCEPT_CONNECTION:
                        {
                            // 1. First packet received from the server when the client's server connection is successful.
                            // 2. Receives a KSecurityAssociation object for packet security from the server and stores it in SADB.
                            // 3. SPIndex arriving from server is not used.
                            // 4. Client generates a non-overlapping SPIndex at random and uses it (KSecurityAssociation should use the same as received from the server).

                            IAcceptionConnection pkt = (IAcceptionConnection)packet;

                            // Update SPI.
                            m_usSPIndex = pkt.SPI;

                            // Update Auth key.
                            m_securityAssociation.SetAuthKey(pkt.AuthKey);

                            // Update Crypto key.
                            m_securityAssociation.SetCryptoKey(pkt.CryptoKey);

                            // Define about client received spi, crypto and auth keys.
                            m_bAuthKeyRecved = true;

                            ///////////////////////////////////////////
                            if (m_enConnectionState == EnConnectionStateValue.Connecting)
                                m_enConnectionState = EnConnectionStateValue.Connected;

                            m_bApplicationIsInitialized = true;
                            Listener.OnStatusChanged(EnStatusCode.Connect);
                        }
                        break;
                    case (int)OPCODE_LU.LU_CONNECTION_ESTABLISHED:
                    case (int)OPCODE_GU.GU_CONNECTION_ESTABLISHED:
                        {
                            Listener.OnStatusChanged(EnStatusCode.ConnectionEstablished);
                        }
                        break;
                    default:
                        Listener.OnPacketReceived(packet);
                        break;
                }

                return true;
            }

            LoggerHelper.LogError("Packet invalid, without payload.");
            return false;
        }

        //----------------------------------------------------------------------------------------------------
        internal void EnqueueActionForDispatch(SocketAction action)
        {
            lock (m_queueAction)
            {
                m_queueAction.Enqueue(action);
            }
        }

        //----------------------------------------------------------------------------------------------------
        internal void EnqueueDebugReturn(LogLevel level, string debugReturn)
        {
            lock (m_queueAction)
            {
                m_queueAction.Enqueue(() => Listener.NetworkLog(level, debugReturn));
            }
        }

        //----------------------------------------------------------------------------------------------------
        internal void EnqueueStatusCallback(EnStatusCode statusValue)
        {
            lock (m_queueAction)
            {
                m_queueAction.Enqueue(() => Listener.OnStatusChanged(statusValue));
            }
        }
    }
}