using MessagePack;

namespace Serialization
{
    /*
        ------------------------------------------------
        NOTICE :
        ------------------------------------------------
        
        1) Naming Op-code
        
            To distinguish type of a packet from other types, we use some alphabet letters in op-codes.
    			U - User (Client)
    			C - Center Server
    			L - Login Server
    			G - Game Server
    			B - Battle Server
                X - UNDEFINED SERVER
    
            For example, if we design a packet that is sent from a Game Server to a Center Server, its op-code must be named as "GC".
    
        2) Ranges that Op-code values belong to.

            SYSTEM series packets' Op code should be in [0, 99].
    
            - User (Client) <-> Login Server
    		UL series packets' Op code should be in [100, 999].
    		LU series packets' Op code should be in [1000, 1999].
    
            - User (Client) <-> Game Server
    		UG series packets' Op code should be in [2000, 2999].
    		GU series packets' Op code should be in [3000, 3999].

             - User (Client) <-> Battle Server
    		UB series packets' Op code should be in [4000, 4999].
    		BU series packets' Op code should be in [5000, 5999].
    
            - Game Server <-> Central Server
    		CG series packets' Op code should be in [6000, 6999].
    		GC series packets' Op code should be in [7000, 7999].
    
            - Battle Server <-> Central Server
    		BC series packets' Op code should be in [8000, 8999].
    		CB series packets' Op code should be in [9000, 9999].
    */

    //------------------------------------------------------------------
    public enum OPCODE
    {
        // User (Client) <-> Login Server
        OPCODE_UL_BEGIN = 100,
        OPCODE_UL_END = 999,

        OPCODE_LU_BEGIN = 1000,
        OPCODE_LU_END = 1999,

        // User (Client) <-> Game Server
        OPCODE_UG_BEGIN = 2000,
        OPCODE_UG_END = 2999,

        OPCODE_GU_BEGIN = 3000,
        OPCODE_GU_END = 3999,

        // User (Client) <-> Battle Server
        OPCODE_UB_BEGIN = 4000,
        OPCODE_UB_END = 4999,

        OPCODE_BU_BEGIN = 5000,
        OPCODE_BU_END = 5999,
    }

    //------------------------------------------------------------------
    public interface IHeartBeat : IPacket
    {
        [IgnoreMember]
        int Tick { get; set; }
    }

    //------------------------------------------------------------------
    public interface IAcceptionConnection : IPacket
    {
        [IgnoreMember]
        ushort SPI { get; set; }

        [IgnoreMember]
        byte[] AuthKey { get; set; }

        [IgnoreMember]
        byte[] CryptoKey { get; set; }

        [IgnoreMember]
        uint SequenceNum { get; set; }

        [IgnoreMember]
        uint LastSequenceNum { get; set; }

        [IgnoreMember]
        uint ReplayWindowMask { get; set; }
    }

    //------------------------------------------------------------------
    public interface IConnectionEstablished : IPacket
    {
        [IgnoreMember]
        bool RetCode { get; set; }
    }
}
