using MessagePack;

namespace Serialization
{
    //---------------------------------------------------------------------------------------------------
    // User (Client) -> Login Server
    //---------------------------------------------------------------------------------------------------

    #region User (Client) -> Login Server

    [MessagePack.Union((int)OPCODE_UL.UL_HEARTBEAT, typeof(UL_HEARTBEAT))]
    [MessagePack.Union((int)OPCODE_UL.UL_ACCEPT_CONNECTION, typeof(UL_ACCEPT_CONNECTION))]
    [MessagePack.Union((int)OPCODE_UL.UL_ACCOUNT_REQ, typeof(UL_ACCOUNT_REQ))]
    [MessagePack.Union((int)OPCODE_UL.UL_LOGOUT_REQ, typeof(UL_LOGOUT_REQ))]

    #endregion User (Client) -> Login Server

    //---------------------------------------------------------------------------------------------------
    // User (Client) -> Game Server
    //---------------------------------------------------------------------------------------------------

    #region User (Client) -> Game Server

    [MessagePack.Union((int)OPCODE_UG.UG_HEARTBEAT, typeof(UG_HEARTBEAT))]
    [MessagePack.Union((int)OPCODE_UG.UG_ACCEPT_CONNECTION, typeof(UG_ACCEPT_CONNECTION))]
    [MessagePack.Union((int)OPCODE_UG.UG_GAME_ENTER_REQ, typeof(UG_GAME_ENTER_REQ))]
    [MessagePack.Union((int)OPCODE_UG.UG_AVATAR_CREATE_REQ, typeof(UG_AVATAR_CREATE_REQ))]
    [MessagePack.Union((int)OPCODE_UG.UG_CHAT_REQ, typeof(UG_CHAT_REQ))]
    [MessagePack.Union((int)OPCODE_UG.UG_INVENTORY_MOVE_ITEM_REQ, typeof(UG_INVENTORY_MOVE_ITEM_REQ))]

    #endregion User (Client) -> Game Server

    //---------------------------------------------------------------------------------------------------
    // Login Server -> User (Client)
    //---------------------------------------------------------------------------------------------------

    #region Login Server -> User (Client)

    [MessagePack.Union((int)OPCODE_LU.LU_HEARTBEAT, typeof(LU_HEARTBEAT))]
    [MessagePack.Union((int)OPCODE_LU.LU_ACCEPT_CONNECTION, typeof(LU_ACCEPT_CONNECTION))]
    [MessagePack.Union((int)OPCODE_LU.LU_CONNECTION_ESTABLISHED, typeof(LU_CONNECTION_ESTABLISHED))]
    [MessagePack.Union((int)OPCODE_LU.LU_ACCOUNT_RES, typeof(LU_ACCOUNT_RES))]
    [MessagePack.Union((int)OPCODE_LU.LU_LOGOUT_RES, typeof(LU_LOGOUT_RES))]
    [MessagePack.Union((int)OPCODE_LU.LU_SERVER_LIST, typeof(LU_SERVER_LIST))]

    #endregion Login Server -> User (Client)

    //---------------------------------------------------------------------------------------------------
    // Game Server -> User (Client)
    //---------------------------------------------------------------------------------------------------

    #region Game Server -> User (Client)

    [MessagePack.Union((int)OPCODE_GU.GU_HEARTBEAT, typeof(GU_HEARTBEAT))]
    [MessagePack.Union((int)OPCODE_GU.GU_ACCEPT_CONNECTION, typeof(GU_ACCEPT_CONNECTION))]
    [MessagePack.Union((int)OPCODE_GU.GU_CONNECTION_ESTABLISHED, typeof(GU_CONNECTION_ESTABLISHED))]
    [MessagePack.Union((int)OPCODE_GU.GU_GAME_ENTER_RES, typeof(GU_GAME_ENTER_RES))]
    [MessagePack.Union((int)OPCODE_GU.GU_AVATAR_CREATE_RES, typeof(GU_AVATAR_CREATE_RES))]
    [MessagePack.Union((int)OPCODE_GU.GU_GAME_AVATAR_CREATE_RES, typeof(GU_GAME_AVATAR_CREATE_RES))]
    [MessagePack.Union((int)OPCODE_GU.GU_AVATAR_LOAD_RES, typeof(GU_AVATAR_LOAD_RES))]
    [MessagePack.Union((int)OPCODE_GU.GU_CHANNEL_LIST_ACK, typeof(GU_CHANNEL_LIST_ACK))]
    [MessagePack.Union((int)OPCODE_GU.GU_CHAT_RES, typeof(GU_CHAT_RES))]

    #endregion Game Server -> User (Client)

    public interface IPacket
    {
        [Key(0)]
        int OpCode { get; }
    }
}
