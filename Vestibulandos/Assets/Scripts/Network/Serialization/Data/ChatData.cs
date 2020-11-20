using Common.Constants;
using MessagePack;

namespace Serialization.Data
{
    [MessagePackObject]
    public class ChatData
    {
        //---------------------------------------------------------------------------------------------------
        // Member variables.
        //---------------------------------------------------------------------------------------------------
        //private ulong m_ulCharacterIndex;
        //private string m_strCharacterName;

        //private EnGender m_enGender;

        //private int m_iAvatarIndex;

        //private int m_iLevel;
        //private int m_iVipLevel;

        //private int m_iChatType;
        //private string m_strMessage;

        [Key(0)]
        public ulong CharacterIndex { get; set; }

        [Key(1)]
        public string CharacterName { get; set; }

        [Key(2)]
        public int /*EnGender*/ Gender { get; set; }

        [Key(3)]
        public int AvatarIndex { get; set; }

        [Key(4)]
        public int Level { get; set; }

        [Key(5)]
        public int VipLevel { get; set; }

        [Key(6)]
        public int /*EnChatType*/ ChatType { get; set; }

        [Key(7)]
        public string Message { get; set; }

        //---------------------------------------------------------------------------------------------------
        // Constructors.
        //---------------------------------------------------------------------------------------------------
        //public ChatData(ulong ulCharacterIndex, string strCharacterName, EnGender enGender, int iAvatarIndex, int iLevel, int iVipLevel, int iChatType, string strMessage)
        //{
        //    CharacterIndex = ulCharacterIndex;
        //    CharacterName = strCharacterName;

        //    AvatarIndex = iAvatarIndex;

        //    Gender = enGender;

        //    Level = iLevel;
        //    VipLevel = iVipLevel;

        //    ChatType = iChatType;
        //    Message = strMessage;
        //}

        //---------------------------------------------------------------------------------------------------
        // Properties.
        //---------------------------------------------------------------------------------------------------
        //public ulong CharacterIndex => m_ulCharacterIndex;
        //public string CharacterName => m_strCharacterName;

        //public EnGender Gender => m_enGender;

        //public int AvatarIndex => m_iAvatarIndex

        //public int Level => m_iLevel;
        //public int VipLevel => m_iVipLevel;

        //public int ChatType => m_iChatType;
        //public string Message => m_strMessage;
    }
}
