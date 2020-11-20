using MessagePack;

namespace Serialization
{
    //------------------------------------------------------------------
    // Packet Enumeration
    //------------------------------------------------------------------
    public enum OPCODE_UL
    {
        UL_OPCODE_BEGIN = OPCODE.OPCODE_UL_BEGIN,

        UL_HEARTBEAT = UL_OPCODE_BEGIN,

        UL_ACCEPT_CONNECTION,

        UL_ACCOUNT_REQ,
        UL_LOGOUT_REQ,

        UL_OPCODE_END = OPCODE.OPCODE_UL_END
    };

    //------------------------------------------------------------------
    [MessagePackObject]
    public struct UL_HEARTBEAT : IHeartBeat
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_UL.UL_HEARTBEAT;

        [Key(1)]
        public int Tick { get; set; }
    }

    //------------------------------------------------------------------
    [MessagePackObject]
    public struct UL_ACCEPT_CONNECTION : IAcceptionConnection
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_UL.UL_ACCEPT_CONNECTION;

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
    public struct UL_ACCOUNT_REQ : IPacket
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_UL.UL_ACCOUNT_REQ;

        [Key(1)]
        public string Login { get; set; }

        [Key(2)]
        public string Password { get; set; }

        [Key(3)]
        public string IP { get; set; }
    }

    //------------------------------------------------------------------
    [MessagePackObject]
    public struct UL_LOGOUT_REQ : IPacket
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_UL.UL_LOGOUT_REQ;

        [Key(1)]
        public byte Dummy { get; set; }
    }
}
