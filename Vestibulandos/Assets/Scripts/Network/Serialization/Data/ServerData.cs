using MessagePack;

namespace Serialization.Data
{
    [MessagePackObject]
    public class ServerData
    {
        [Key(0)]
        public int ServerId { get; set; }

        [Key(1)]
        public string Name { get; set; }

        [Key(2)]
        public string ServerIp { get; set; }

        [Key(3)]
        public int ServerPort { get; set; }

        [Key(4)]
        public int CurrentUserCount { get; set; }

        [Key(5)]
        public int MaxUserCount { get; set; }

        [Key(6)]
        public int Status { get; set; }

        [Key(7)]
        public bool IsNew { get; set; }

        [Key(8)]
        public bool IsMaintenance { get; set; }
    }
}
