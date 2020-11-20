using System.Collections.Generic;
using Serialization.Data;
using MessagePack;

namespace Serialization
{
    //------------------------------------------------------------------
    // Packet Enumeration
    //------------------------------------------------------------------
    public enum OPCODE_GU
    {
        GU_OPCODE_BEGIN = OPCODE.OPCODE_GU_BEGIN,

        GU_HEARTBEAT = GU_OPCODE_BEGIN,
        
        GU_ACCEPT_CONNECTION,
        GU_CONNECTION_ESTABLISHED,

        GU_GAME_ENTER_RES,
        GU_GAME_AVATAR_CREATE_RES,

        GU_AVATAR_CREATE_RES,
        GU_AVATAR_LOAD_RES,

        GU_CHANNEL_LIST_ACK,

        // Chat.
        GU_CHAT_RES,

        // Inventory.
        GU_INVENTORY_MOVE_ITEM_RES,
        GU_INVENTORY_MOVE_ITEM_ACK,

        GU_OPCODE_END = OPCODE.OPCODE_GU_END
    };

    //------------------------------------------------------------------
    [MessagePackObject]
    public struct GU_HEARTBEAT : IHeartBeat
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_GU.GU_HEARTBEAT;

        [Key(1)]
        public int Tick { get; set; }
    }

    //------------------------------------------------------------------
    [MessagePackObject]
    public struct GU_ACCEPT_CONNECTION : IAcceptionConnection
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_GU.GU_ACCEPT_CONNECTION;

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
    public struct GU_CONNECTION_ESTABLISHED : IConnectionEstablished
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_GU.GU_CONNECTION_ESTABLISHED;

        [Key(1)]
        public bool RetCode { get; set; }
    }

    //------------------------------------------------------------------
    [MessagePackObject]
    public struct GU_GAME_ENTER_RES : IPacket
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_GU.GU_GAME_ENTER_RES;

        [Key(1)]
        public int RetCode { get; set; }
    }

    //------------------------------------------------------------------
    [MessagePackObject]
    public struct GU_GAME_AVATAR_CREATE_RES : IPacket
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_GU.GU_GAME_AVATAR_CREATE_RES;

        [Key(1)]
        public int RetCode { get; set; }
    }

    //------------------------------------------------------------------
    [MessagePackObject]
    public struct GU_AVATAR_CREATE_RES : IPacket
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_GU.GU_AVATAR_CREATE_RES;

        [Key(1)]
        public int RetCode { get; set; }
    }

    //------------------------------------------------------------------
    [MessagePackObject]
    public struct GU_AVATAR_LOAD_RES : IPacket
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_GU.GU_AVATAR_LOAD_RES;

        [Key(1)]
        public int AccountAuth { get; set; }

        [Key(2)]
        public int RetCode { get; set; }
    }

    //------------------------------------------------------------------
    [MessagePackObject]
    public struct GU_CHANNEL_LIST_ACK : IPacket
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_GU.GU_CHANNEL_LIST_ACK;
    }

    //------------------------------------------------------------------
    [MessagePackObject]
    public struct GU_CHAT_RES : IPacket
    {
        [Key(0)]
        public int OpCode => (int)OPCODE_GU.GU_CHAT_RES;

        [Key(1)]
        public ChatData Data { get; set; }

        [Key(2)]
        public int RetCode { get; set; }
    }
}
