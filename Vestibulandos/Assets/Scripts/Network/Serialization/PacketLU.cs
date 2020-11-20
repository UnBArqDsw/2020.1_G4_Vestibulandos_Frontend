using System.Collections.Generic;
using MessagePack;
using Serialization.Data;

namespace Serialization
{
    //------------------------------------------------------------------
    // Packet Enumeration
    //------------------------------------------------------------------
    public enum OPCODE_LU
    {
        LU_OPCODE_BEGIN = OPCODE.OPCODE_LU_BEGIN,

        LU_HEARTBEAT = LU_OPCODE_BEGIN,

        LU_ACCEPT_CONNECTION,
        LU_CONNECTION_ESTABLISHED,

        LU_ACCOUNT_RES,
        LU_LOGOUT_RES,

        LU_SERVER_LIST,

        LU_OPCODE_END = OPCODE.OPCODE_LU_END
    };

    //------------------------------------------------------------------
    [MessagePackObject]
    public struct LU_HEARTBEAT : IHeartBeat
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_LU.LU_HEARTBEAT;

        [Key(1)]
        public int Tick { get; set; }
    }

    //------------------------------------------------------------------
    [MessagePackObject]
    public struct LU_ACCEPT_CONNECTION : IAcceptionConnection
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_LU.LU_ACCEPT_CONNECTION;

        [Key(1)]
        public ushort SPI { get; set; }

        [Key(2)]
        public byte[] AuthKey { get; set; }

        [Key(3)]
        public byte[] CryptoKey { get; set; }

        [Key(4)]
        public uint SequenceNum { get; set; }

        [Key(5)]
        public uint LastSequenceNum { get; set; }

        [Key(6)]
        public uint ReplayWindowMask { get; set; }
    }

    //------------------------------------------------------------------
    [MessagePackObject]
    public struct LU_CONNECTION_ESTABLISHED : IConnectionEstablished
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_LU.LU_CONNECTION_ESTABLISHED;

        [Key(1)]
        public bool RetCode { get; set; }
    }

    //------------------------------------------------------------------
    [MessagePackObject]
    public struct LU_ACCOUNT_RES : IPacket
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_LU.LU_ACCOUNT_RES;

        [Key(1)]
        public ulong UniqueKey { get; set; }

        [Key(2)]
        public string Account { get; set; }

        [Key(3)]
        public byte[] SessionKey { get; set; }

        [Key(4)]
        public int LastLobbyID { get; set; }

        [Key(5)]
        public int RetCode { get; set; }
    }

    //------------------------------------------------------------------
    [MessagePackObject]
    public struct LU_LOGOUT_RES : IPacket
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_LU.LU_LOGOUT_RES;

        [Key(1)]
        public int RetCode { get; set; }
    }

    //------------------------------------------------------------------
    [MessagePackObject]
    public struct LU_SERVER_LIST : IPacket
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_LU.LU_SERVER_LIST;

        [Key(1)]
        public List<ServerData> ServerList { get; set; }
    }
}
