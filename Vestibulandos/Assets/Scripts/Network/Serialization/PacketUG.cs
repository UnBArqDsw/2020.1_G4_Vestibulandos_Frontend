using Common.Constants;
using MessagePack;

namespace Serialization
{
    //------------------------------------------------------------------
    // Packet Enumeration
    //------------------------------------------------------------------
    public enum OPCODE_UG
    {
        UG_OPCODE_BEGIN = OPCODE.OPCODE_UG_BEGIN,

        UG_HEARTBEAT = UG_OPCODE_BEGIN,

        UG_ACCEPT_CONNECTION,

        UG_GAME_ENTER_REQ,

        UG_AVATAR_CREATE_REQ,

        // Chat
        UG_CHAT_REQ,

        // Inventory
        UG_INVENTORY_MOVE_ITEM_REQ,

        UG_OPCODE_END = OPCODE.OPCODE_UG_END
    };

    //------------------------------------------------------------------
    [MessagePackObject]
    public struct UG_HEARTBEAT : IHeartBeat
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_UG.UG_HEARTBEAT;

        [Key(1)]
        public int Tick { get; set; }
    }

    //------------------------------------------------------------------
    [MessagePackObject]
    public struct UG_ACCEPT_CONNECTION : IAcceptionConnection
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_UG.UG_ACCEPT_CONNECTION;

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
    public struct UG_GAME_ENTER_REQ : IPacket
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_UG.UG_GAME_ENTER_REQ;

        [Key(1)]
        public ulong UserID { get; set; }

        [Key(2)]
        public byte[] SessionKey { get; set; }
    }

    //------------------------------------------------------------------
    [MessagePackObject]
    public struct UG_AVATAR_CREATE_REQ : IPacket
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_UG.UG_AVATAR_CREATE_REQ;

        [Key(1)]
        public string Name { get; set; }

        [Key(2)]
        public byte Gender { get; set; }

        [Key(3)]
        public uint Hair { get; set; }

        [Key(4)]
        public uint Face { get; set; }

        [Key(5)]
        public uint Cloth { get; set; }

        [Key(6)]
        public int HeadIcon { get; set; }
    }

    //------------------------------------------------------------------
    [MessagePackObject]
    public struct UG_CHAT_REQ : IPacket
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_UG.UG_CHAT_REQ;

        [Key(1)]
        public int /*EnChatType*/ ChatType { get; set; }

        [Key(2)]
        public string Message { get; set; }
    }

    //------------------------------------------------------------------
    [MessagePackObject]
    public struct UG_INVENTORY_MOVE_ITEM_REQ : IPacket
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_UG.UG_INVENTORY_MOVE_ITEM_REQ;

        [Key(1)]
        public short CurrentSlot { get; set; }

        [Key(2)]
        public short NewSlot { get; set; }
    }
}
