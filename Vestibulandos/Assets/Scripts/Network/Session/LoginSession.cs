using System;
using Data;
using Serialization;
using Util;

namespace Network.Session
{
    public class LoginSession : Session
    {
        /// <summary>
        /// Singleton.
        /// </summary>
        public static LoginSession Instance => Singleton<LoginSession>.GetInstance();

        //----------------------------------------------------------------------------------------------------
        public event Delegate EventConnected = null;
        public event Delegate EventDisconnected = null;
        public event Delegate EventDisconnectedByClient = null;

        /// <summary>
        /// Heart beat tick.
        /// </summary>
        private int m_dwHBTick;

        /// <summary>
        /// Maximum time to wait for a heart beat.
        /// </summary>
        public readonly int m_dwHBGap = 15000;

        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        public LoginSession() { }

        //----------------------------------------------------------------------------------------------------
        public override void Tick()
        {
            // Heartbeat. Send in every 15 seconds.
            if (IsConnected() && Environment.TickCount - m_dwHBTick >= m_dwHBGap)
            {
                SendHeartBeat();
            }

            base.Tick();
        }

        //----------------------------------------------------------------------------------------------------
        public override void OnPacketReceived(IPacket packet)
        {
            // Check if packet is valid.
            if (packet == null)
            {
                LoggerHelper.LogError("Received packet null.");
                return;
            }

            // Validate the packet received.
            if (packet.OpCode < (int)OPCODE_LU.LU_OPCODE_BEGIN ||
                packet.OpCode > (int)OPCODE_LU.LU_OPCODE_END)
            {
                LoggerHelper.LogError($"Received current packet in incorrect session! Packet ID: <color=red>{packet.GetType().Name}</color>");
                return;
            }

            // Process the packet received.
            switch ((OPCODE_LU)packet.OpCode)
            {
                case OPCODE_LU.LU_HEARTBEAT:
                    {
                        // heart bit filtering - no queueing
                    }
                    break;
                case OPCODE_LU.LU_ACCOUNT_RES:
                    {
                        //EventManager.Instance.Fire(this, PacketLoginAccountResEventArgs.Create((LU_ACCOUNT_RES)packet));
                    }
                    break;
                case OPCODE_LU.LU_SERVER_LIST:
                    {
                        //EventManager.Instance.Fire(this, PacketServerInfoListEventArgs.Create((LU_SERVER_LIST)packet));
                    }
                    break;
                default:
                    {
                        LoggerHelper.LogError("[{0}] - Packet ID: {1} is not defined.", this.GetType().Name, packet.OpCode);
                    }
                    break;
            }

#if UNITY_EDITOR && DEBUG
            LoggerHelper.Log($"LoginSession.: Received packet ID: <color=green>{packet.GetType().Name}</color>");
#endif
        }

        //----------------------------------------------------------------------------------------------------
        #region Receive Protocol's

        //----------------------------------------------------------------------------------------------------

        #endregion

        //----------------------------------------------------------------------------------------------------
        #region Send Protocol's

        //----------------------------------------------------------------------------------------------------
        protected override void SendAcceptConnection(ushort spi, byte[] authKey, byte[] cryptoKey,
            uint sequenceNum, uint lastSequenceNum, uint replayWindowMask)
        {
            // Send SA information before to update SPI.
            Send(new UL_ACCEPT_CONNECTION
            {
                SPI = spi,
                AuthKey = authKey,
                CryptoKey = cryptoKey,
                SequenceNum = sequenceNum,
                LastSequenceNum = lastSequenceNum,
                ReplayWindowMask = replayWindowMask
            });
        }

        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// Heart beat protocol.
        /// </summary>
        protected void SendHeartBeat()
        {
            m_dwHBTick = Environment.TickCount;

            // Send the heart beat.
            Send(new UL_HEARTBEAT());
        }

        #endregion

        //----------------------------------------------------------------------------------------------------
        #region Event's

        //----------------------------------------------------------------------------------------------------
        protected override void OnConnected()
        {
            EventConnected?.Invoke();
        }

        //----------------------------------------------------------------------------------------------------
        protected override void OnDisconnected(EnStatusCode enStatusCode)
        {
            LoggerHelper.Log(enStatusCode);
            EventDisconnected?.Invoke();
        }

        //----------------------------------------------------------------------------------------------------
        protected override void OnDisconnectedByClient()
        {
            LoggerHelper.Log("OnDisconnectedByClient() called.");
            EventDisconnectedByClient?.Invoke();
        }

        #endregion
    }
}